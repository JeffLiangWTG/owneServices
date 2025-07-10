using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobComInvoiceHeader : AutoJobComInvoiceHeader, Integration.Customs.IT.IJobComInvoiceHeader
{
	public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new JobComInvoiceLineViewCollection JobComInvoiceLines => (JobComInvoiceLineViewCollection)base.JobComInvoiceLines;

	public new JobComInvoiceLineViewCollection InvoiceLines => JobComInvoiceLines;

	public new AddInfoJobComInvoiceHeader AddInfo => (AddInfoJobComInvoiceHeader)base.AddInfo;

	public new AddInfoJobComInvoiceHeaderValidation AddInfoValidation => (AddInfoJobComInvoiceHeaderValidation)base.AddInfoValidation;

	protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
	{
		var dec = JobDeclaration;
		return dec != null ? new JobComInvoiceLineViewCollection(this, dec.InvoiceLines) : null;
	}

	public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;
	public new JobComInvoiceHeaderLookups Lookups => (JobComInvoiceHeaderLookups)base.Lookups;
	protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups() => new JobComInvoiceHeaderLookups(this);

	protected override ZString LocalCurrencyCodeCore => JobDeclaration.LocalCurrencyConstantCode;

	protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
	{
		var collection = new InvoiceLineDependentCollection(this);
		collection.Load();
		return new JobComInvoiceLineViewCollection(this, collection);
	}

	public new JobComInvoiceHeaderDocAddressDependentCollection DocAddresses => (JobComInvoiceHeaderDocAddressDependentCollection)base.DocAddresses;

	protected override JobDocAddressDependentCollection GetDocAddressesCore()
	{
		var docAddresses = new JobComInvoiceHeaderDocAddressDependentCollection(this);
		docAddresses.Load();
		RegisterEditableChildObject(docAddresses);
		return docAddresses;
	}

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;
	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	public new IEnumerable<CusEntryInstruction> CusEntryInstructions => base.CusEntryInstructions.Cast<CusEntryInstruction>();

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;
	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	public new InvoiceHeaderAdditionalInfoCollection AdditionalInfos => (InvoiceHeaderAdditionalInfoCollection)base.AdditionalInfos;
	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new InvoiceHeaderAdditionalInfoCollection(this);

	protected override EU.Business.Declaration.AddInfoJobComInvoiceHeader GetNewAddInfo() => new AddInfoJobComInvoiceHeader(JZ_AddInfoInfo);

	protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation() => new JobComInvoiceHeaderValidation(this);

	protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobComInvoiceHeaderFetchStrategy(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(InvoiceHeaderAdditionalInfo);
		result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		return result;
	}

	public override void OnLoaded()
	{
		base.OnLoaded();
		DefaultSupplierOrgPkForBinding();
	}

	protected override ZDateTime EffectiveValuationDateCore
	{
		get
		{
			var originalEntryInstruction = CusEntryInstructions.Distinct().OrderBy(x => x.CEI_SubStyle).ThenBy(x => x.CEI_Description).FirstOrDefault(x => x.CEI_DateForDuty.IsValid);
			return originalEntryInstruction?.CEI_DateForDuty ?? base.EffectiveValuationDateCore;
		}
	}

	public override ZGuid JZ_JE
	{
		get => base.JZ_JE;
		set
		{
			var oldValue = JZ_JE;
			base.JZ_JE = value;
			if (!IsCopying && oldValue != JZ_JE)
			{
				ResetAeoCertificateManager();
			}
		}
	}

	#region JZ_OH_Supplier

	public override ZGuid JZ_OH_Supplier
	{
		get => base.JZ_OH_Supplier;
		set
		{
			if (!SetterSuspender.IsSetterSuspended(JobComInvoiceHeader.Schema.JZ_OH_Supplier))
			{
				var oldValue = JZ_OH_Supplier;
				base.JZ_OH_Supplier = value;
				DefaultSupplierOrgPkForBinding();
				if (!IsCopying && oldValue != JZ_OH_Supplier)
				{
					DefaultSupplierDocumentaryAddress();
					UpdateDefaultSupportingDocumentCountryToSupplierCountry();
				}
			}
		}
	}

	protected override bool AllowDefaultSupplier => false;

	void DefaultSupplierOrgPkForBinding() => SupplierOrgPK = JZ_OH_Supplier;

	protected override bool IsValidToDefaultIncoTermFromSupplier => JZ_IncoTerm.IsEmpty;

	void DefaultSupplierDocumentaryAddress()
	{
		if (IsAttachedToPersistentDeclaration && !SupplierDocumentaryAddress.E2_AddressOverride)
		{
			SupplierDocumentaryAddress.OrganisationPK = JZ_OH_Supplier;
		}
	}

	#endregion

	#region SupplierDocumentaryAddress

	[ResourceStringData("0FBA80BF-E65E-4E2C-9F00-A01BF4C49E19", Caption = "[2] Supplier")]
	public JobDocAddress SupplierDocumentaryAddress
	{
		get
		{
			if (fSupplierDocumentaryAddress == null || fSupplierDocumentaryAddress.IsDeleted)
			{
				fSupplierDocumentaryAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.SupplierDocumentaryAddress);
				fSupplierDocumentaryAddress.DocAddressChanged += SupplierDocumentaryAddress_DocAddressChanged;
				fSupplierDocumentaryAddress.AdditionalValidation = PiggyBackedDocAddressValidation(fSupplierDocumentaryAddress);
			}
			return fSupplierDocumentaryAddress;
		}
	}
	JobDocAddress fSupplierDocumentaryAddress;

	void SupplierDocumentaryAddress_DocAddressChanged(object sender, EventArgs e)
	{
		MarkAsNeedingValidation();

		var supplierDocumentaryAddress = SupplierDocumentaryAddress;
		if (JZ_OH_Supplier != supplierDocumentaryAddress.OrganisationPK)
		{
			JZ_OH_Supplier = supplierDocumentaryAddress.OrganisationPK;
		}
	}

	#endregion

	#region PiggyBackedDocAddressValidation

	public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
	{
		Argument.NotNull(addressToValidate, nameof(addressToValidate));
		switch (addressToValidate.E2_AddressType)
		{
			case DocAddressTypes.Codes.SupplierDocumentaryAddress:
				return GetSupplierDocumentaryAddressValidation(addressToValidate);
		}
		return base.PiggyBackedDocAddressValidation(addressToValidate);
	}

	ZValidation GetSupplierDocumentaryAddressValidation(JobDocAddress addressToValidate)
	{
		var declaration = JobDeclaration;
		var isExport = declaration?.IsExport ?? false;

		if (isExport && IsBuyersConsol)
		{
			return new TraderJobDocAddressValidation(addressToValidate, JZ_OA_SupplierAddressInfo.HumanReadableName, declaration);
		}
		return null;
	}

	#endregion

	public JobDocAddressManager JobDocAddressManager
	{
		get { return jobDocAddressManager ?? (jobDocAddressManager = new JobDocAddressManager()); }
	}
	JobDocAddressManager jobDocAddressManager;

	public ZBool IsBuyersConsol => !EntryInstructionsHaveDifferentParticipantTypes && (CusEntryInstructions.FirstOrDefault()?.IsBuyersConsol ?? ZBool.False);
	public ZBool EntryInstructionsHaveDifferentParticipantTypes => CusEntryInstructions.Select(x => x.ZG_ParticipantType).Distinct().Count() > 1;

	public AeoCertificateManager AeoCertificateManager => aeoCertificateManager ?? (aeoCertificateManager = new AeoCertificateManager(JobDeclaration.AeoCertificateSupporter, this));
	AeoCertificateManager aeoCertificateManager;

	void ResetAeoCertificateManager() => aeoCertificateManager = null;

	public void UpdateDefaultSupportingDocumentCountryToSupplierCountry()
	{
		if (Supplier_Effective != null)
		{
			SupportingDocuments.Cast<SupportingDocument>().Where(s => s.CSI_Code == DefaultInvoiceDocument).ForEach(x => x.CSI_RN_NKCountryCode = Supplier_Effective.MainAddress.OA_RN_NKCountryCode);
		}
	}

	[ResourceStringData("BE0B5588-DECE-49B9-A134-49DA30EC1ECA", Caption = "[20.1] INCO term")]
	public override ZString JZ_IncoTerm
	{
		get => base.JZ_IncoTerm;
		set
		{
			var oldValue = JZ_IncoTerm;
			base.JZ_IncoTerm = value;
			if (!IsCopying && oldValue != JZ_IncoTerm)
			{
				ClearAdditionalTermsIfNeeded();
			}
		}
	}

	void ClearAdditionalTermsIfNeeded()
	{
		if (!AdditionalTermsSupport)
		{
			JZ_AdditionalTerms = ZString.Empty;
		}
	}

	public bool AdditionalTermsSupport => JZ_IncoTerm == Core.Constants.IncoTerms.Other && (JobDeclaration?.IsUCC6AndIsExport ?? false);

	protected override ZBool AgreedPlaceCodeSupportAndVisibleCore => (JobDeclaration?.IsUCC6 ?? false) || base.AgreedPlaceCodeSupportAndVisibleCore;
}
