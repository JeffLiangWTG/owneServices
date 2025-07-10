using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.IN;
using Enterprise.Customs.IN.Business.Helpers;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IN.Business;

public partial class JobDeclaration : AutoINJobDeclaration, Integration.Customs.ICusSupportingInfoTypeSupporter
{
	public JobDeclaration(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : BaseJobDeclaration.Schema
	{
		public const string ExporterClass = nameof(JobDeclaration.ExporterClass);
		public const string IECCode = nameof(JobDeclaration.IECCode);
		public const string BranchSerialNumber = nameof(JobDeclaration.BranchSerialNumber);
		public const string AuthorizedDealerCode = nameof(JobDeclaration.AuthorizedDealerCode);
		public const int JE_RotationNumberMaxLength = 7;
		public const int JE_SealByMaxLength = 1;
	}

	public override ZBool AreMultipleEntryInstructionsAllowed => false;

	protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

	protected override bool IsCustomsLineAmendmentATotalReplacement => false;

	protected override bool SupportContainerEntryInstructionPivot => true;

	protected override bool SupportsChcPivotBetweenInvoiceLineAndPackingCore => true;

	protected override string GetIApportionInvoiceHolderCountryContextCore() => CountryCode + this.GetIncoTermChargeFactoryCacheKey();

	[MaxLength(2)]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.OriginStateList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_RW_NKOriginState", ShortCaption = "State", MediumCaption = "State", Caption = "Origin State")]
	public override ZString JE_RW_NKOriginState { get => base.JE_RW_NKOriginState; set => base.JE_RW_NKOriginState = value; }

	[ReadOnly(true)]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|ExporterClassDescription", ShortCaption = "Exp. Class", MediumCaption = "Exp. Class", Caption = "Exporter Class")]
	public ZString ExporterClassDescription
	{
		get
		{
			var code = ExporterClass;
			var description = Factory.GetCachedValue<ExporterClassList>().GetDescriptionFromCode(code);
			return code.IsEmpty ? ZString.Empty : $"{code} - {description}";
		}
	}

	public ZPropertyInfo ExporterClassDescriptionInfo => GetZPropertyInfo(nameof(ExporterClassDescription));

	public ZString ExporterClass
	{
		get
		{
			switch (Supplier?.OH_Category)
			{
				case OrgConstants.Category.Business:
				case OrgConstants.Category.NaturalPersonIndividual:
				case OrgConstants.Category.NonGovernmentOrganisation:
					return ExporterClassList.Codes.P;
				case OrgConstants.Category.Government:
					return ExporterClassList.Codes.G;
				default:
					return string.Empty;
			}
		}
	}

	public ZPropertyInfo ExporterClassInfo => GetZPropertyInfo(Schema.ExporterClass);

	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|AuthorizedDealerCode", ShortCaption = "AD Code", MediumCaption = "AD Code", Caption = "Authorized Dealer Code")]
	public ZString AuthorizedDealerCode
	{
		get
		{
			var address = IsExport ? SupplierDocumentaryAddress.Address : null;
			return address.GetCustomsRegNo(IndiaOrgCusCodeInfo.OrgCusCodes.ADC, Core.Constants.CountryCodes.India);
		}
	}

	public ZPropertyInfo AuthorizedDealerCodeInfo => GetZPropertyInfo(Schema.AuthorizedDealerCode);

	public override ZGuid JE_OH_Supplier
	{
		get => base.JE_OH_Supplier;
		set
		{
			var oldValue = JE_OH_Supplier;
			base.JE_OH_Supplier = value;
			if (!IsCopying && oldValue != JE_OH_Supplier)
			{
				ExporterClassInfo.RefreshBinding();
			}
		}
	}

	[ChildEditable(true)]
	public NonStandardExchangeRateCollection NonStandardExchangeRates
	{
		get
		{
			if (nonStandardExchangeRates == null)
			{
				nonStandardExchangeRates = new NonStandardExchangeRateCollection(this);
				nonStandardExchangeRates.Load();
				nonStandardExchangeRates.SyncWithInvoiceCurrencies();
				RegisterEditableChildObject(nonStandardExchangeRates);
			}
			return nonStandardExchangeRates;
		}
	}
	NonStandardExchangeRateCollection nonStandardExchangeRates;

	public override ZBool IsContainerised => base.IsContainerised || JE_ContainerMode == INContainerModeList.Codes.ContainerisedAndPackaged;

	[MaxLength(6)]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_CustomsOffice", ShortCaption = "Cus. House.", MediumCaption = "Cus. House.", Caption = "Customs House")]
	public override ZString JE_CustomsOffice
	{
		get => base.JE_CustomsOffice;
		set
		{
			var oldValue = JE_CustomsOffice;
			base.JE_CustomsOffice = value;
			var newValue = JE_CustomsOffice;
			if (!IsCopying && oldValue != newValue)
			{
				if (IsExport && JE_CustomsLoadPort.IsEmpty && UniversalReferenceDataHelper.GetCustomsOffice(Factory, newValue, DateOfValuation) is ZZRefCusCodeListCombined customsOffice && (customsOffice.ZZD_IsSea || customsOffice.ZZD_IsAir))
				{
					JE_CustomsLoadPort = newValue;
				}
			}
		}
	}

	[MaxLength(6)]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOfficeList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_CustomsLoadPort", ShortCaption = "CUS. Loc.", MediumCaption = "CUS. Location", Caption = "Customs Location")]
	public override ZString JE_CustomsLoadPort { get => base.JE_CustomsLoadPort; set => base.JE_CustomsLoadPort = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|BranchSerialNumber", ShortCaption = "Br. Sr.", MediumCaption = "Branch Sr. No.", Caption = "Branch Serial Number")]
	public ZString BranchSerialNumber
	{
		get
		{
			var address = IsExport ? SupplierDocumentaryAddress.Address : IsImport ? ImporterDocumentaryAddress.Address : null;
			return address.GetCustomsRegNo(IndiaOrgCusCodeInfo.OrgCusCodes.BSN, Core.Constants.CountryCodes.India);
		}
	}

	public ZPropertyInfo BranchSerialNumberInfo => GetZPropertyInfo(nameof(BranchSerialNumber));

	protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.India;

	public override ZString JE_MessageType
	{
		get => base.JE_MessageType;
		set
		{
			var oldValue = JE_MessageType;
			base.JE_MessageType = value;
			if (!IsCopying && oldValue != JE_MessageType)
			{
				RefreshIncotermAndChargeFactory();
				IECCodeInfo.RefreshBinding();
				SetExportOrientedUnitsDocAddress(SupplierDocumentaryAddress);
			}
		}
	}

	public override ZString JE_TransportMode
	{
		get => base.JE_TransportMode;
		set
		{
			var oldValue = JE_TransportMode;
			base.JE_TransportMode = value;
			if (!IsCopying && oldValue != JE_TransportMode)
			{
				SetExportOrientedUnitsDocAddress(SupplierDocumentaryAddress);
			}
		}
	}

	public override ZString JE_ContainerMode
	{
		get => base.JE_ContainerMode;
		set
		{
			var oldValue = JE_ContainerMode;
			base.JE_ContainerMode = value;
			if (!IsCopying && oldValue != JE_ContainerMode)
			{
				SetExportOrientedUnitsDocAddress(SupplierDocumentaryAddress);
			}
		}
	}

	public override void OnSaving()
	{
		base.OnSaving();
		CleanupHelper.CleanupIfNotApplicable(IsExport && !IsAir, JE_SealByInfo);
		CleanupHelper.CleanupIfNotApplicable(IsExport && IsSea, JE_RotationDateInfo, JE_RotationNumberInfo);
		CleanupHelper.CleanupIfNotApplicable(IsExport && IsSeaAndContainerised,
			CustomsEntryInstructions.SelectMany(instruction => new[]
			{
				instruction.CEI_TotalContainerInfo,
				instruction.CEI_LoosePackagesInfo
			}).ToList().Append(JE_StuffingAtInfo).ToArray());
		CleanupHelper.CleanupIfNotApplicable(IsExport && IsSeaAndContainerised && IsFactoryStuffed, JE_SampleAccompaniedInfo);
	}

	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|IECCode", Caption = "IEC Code")]
	public ZString IECCode
	{
		get
		{
			var organization = IsExport ? Supplier : IsImport ? Importer : null;
			return organization.GetCustomsRegNo(IndiaOrgCusCodeInfo.OrgCusCodes.IEC, Core.Constants.CountryCodes.India);
		}
	}

	public ZPropertyInfo IECCodeInfo => GetZPropertyInfo(Schema.IECCode);

	public ZString DischargeCountryCode => JE_RL_NKPortOfArrival.Left(2);

	public ZBool IsFactoryStuffed => JE_StuffingAt == StuffingAtList.Codes.FAC;

	public ZBool IsSeaAndContainerised => IsSea && IsContainerised;

	protected override bool HasSplitEntriesCore
	{
		get
		{
			if (!GetType().FullName.Contains("IN"))
			{
				ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
			}
			return base.HasSplitEntriesCore;
		}
	}

	[MaxLength(1)]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.EPZCodeList))]
	[ResourceStringData("e77e0efd-041f-4a66-9062-71795ff9bd6b", ShortCaption = "EPZ", MediumCaption = "EPZ Code", Caption = "EPZ Code")]
	public override ZString JE_EPZCode { get => base.JE_EPZCode; set => base.JE_EPZCode = value; }

	[ReadOnly(true)]
	public override ZString JE_MergeBy { get => base.JE_MergeBy; set => base.JE_MergeBy = value; }

	protected override string DefaultMergeBy => OrgConstants.MergeInvoiceLines.NotMerge;

	protected override Customs.Business.MergeManager GetMergeManager() => new MergeManager(this);

	[MaxLength(1)]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ExporterTypeList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_ExporterType", ShortCaption = "Exp Ty.", MediumCaption = "Exporter Ty.", Caption = "Type Of Exporter")]
	public override ZString JE_ExporterType { get => base.JE_ExporterType; set => base.JE_ExporterType = value; }

	#region ICusSupportingInfoTypeSupporter

	IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
	{
		{ CusSupportingInfoTypeList.Codes.NonStandardCurrency, typeof(NonStandardExchangeRate) }
	};

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
	{
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}

	#endregion

	protected override void JE_OH_SupplierChanged(ZGuid oldValue, ZGuid newValue)
	{
		base.JE_OH_SupplierChanged(oldValue, newValue);
		if (IsExport && Supplier is OrgHeader supplier)
		{
			var typeOfExporter = INOrgImpAddInfo.Get(supplier).ZO_TypeOfExporter;
			if (!typeOfExporter.IsEmpty)
			{
				JE_ExporterType = typeOfExporter;
			}

			foreach (JobComInvoiceHeader invoice in Invoices)
			{
				invoice.DefaultAuthorizedEconomicOperatorFromSupplier();
			}
		}
	}

	protected override void FlushImporterDocumentaryAddressIfBlank(ZGuid importer) { }

	protected override void FlushSupplierDocumentaryAddressIfBlank(ZGuid supplier) { }

	protected override void ImporterDocumentaryAddressChanged(object sender, EventArgs e)
	{
		base.ImporterDocumentaryAddressChanged(sender, e);
		var importer = ImporterDocumentaryAddress?.OrganisationPK ?? ZGuid.Empty;
		if (JE_OH_Importer != importer)
		{
			JE_OH_Importer = importer;
		}
	}

	protected override void SetupImporterDocumentaryAddress(JobDocAddress importerDocumentaryAddress)
	{
		base.SetupImporterDocumentaryAddress(importerDocumentaryAddress);
		importerDocumentaryAddress.MarkParentAsNeedingValidation = true;
	}

	protected override void SupplierDocumentaryAddressChanged(object sender, EventArgs e)
	{
		base.SupplierDocumentaryAddressChanged(sender, e);
		var supplierDocumentaryAddress = SupplierDocumentaryAddress;
		var supplier = supplierDocumentaryAddress?.OrganisationPK ?? ZGuid.Empty;
		SetExportOrientedUnitsDocAddress(supplierDocumentaryAddress);
		if (JE_OH_Supplier != supplier)
		{
			JE_OH_Supplier = supplier;
		}
	}

	void SetExportOrientedUnitsDocAddress(JobDocAddress supplierDocumentaryAddress)
	{
		if (IsExport && IsFactoryStuffed && IsSeaAndContainerised)
		{
			ExportOrientedUnitsDocAddress.E2_OA_Address = supplierDocumentaryAddress?.E2_OA_Address ?? ZGuid.Empty;
			ExportOrientedUnitsDocAddress.OrganisationPK = supplierDocumentaryAddress?.OrganisationPK ?? ZGuid.Empty;
		}
	}

	protected override void SetupSupplierDocumentaryAddress(JobDocAddress supplierDocumentaryAddress)
	{
		base.SetupSupplierDocumentaryAddress(supplierDocumentaryAddress);
		supplierDocumentaryAddress.MarkParentAsNeedingValidation = true;
	}

	#region ExportOrientedUnitsDocAddress

	public JobDocAddress ExportOrientedUnitsDocAddress
	{
		get
		{
			if (exportOrientedUnitsDocAddress == null || exportOrientedUnitsDocAddress.IsDeleted)
			{
				exportOrientedUnitsDocAddress = DocAddresses.FindOrCreateWithRequirement(ExportOrientedUnitsDocAddressRequirement);
			}
			return exportOrientedUnitsDocAddress;
		}
	}

	JobDocAddress exportOrientedUnitsDocAddress;

	JobDocAddressRequirement ExportOrientedUnitsDocAddressRequirement
	{
		get
		{
			if (exportOrientedUnitsDocAddressRequirement == null)
			{
				exportOrientedUnitsDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.CustomsExportOrientedUnitsAddress, ContactType.NoContactType);
				exportOrientedUnitsDocAddressRequirement.CanOverride = false;
				DocAddressManager.AddRequirement(exportOrientedUnitsDocAddressRequirement);
			}
			return exportOrientedUnitsDocAddressRequirement;
		}
	}
	JobDocAddressRequirement exportOrientedUnitsDocAddressRequirement;

	public ZString ImporterExporterCodeOfExportOrientedUnit => ExportOrientedUnitsDocAddress.Organisation.GetCustomsRegNo(IndiaOrgCusCodeInfo.OrgCusCodes.IEC, Core.Constants.CountryCodes.India);

	public ZString BranchSrNumberOfImporterExporter => ExportOrientedUnitsDocAddress.Address.GetCustomsRegNo(IndiaOrgCusCodeInfo.OrgCusCodes.BSN, Core.Constants.CountryCodes.India);

	#endregion

	#region TranshipperDocAddress

	public JobDocAddress TranshipperDocAddress
	{
		get
		{
			if (transhipperDocAddress == null || transhipperDocAddress.IsDeleted)
			{
				transhipperDocAddress = DocAddresses.FindOrCreateWithRequirement(TranshipperDocAddressRequirement);
			}
			return transhipperDocAddress;
		}
	}

	JobDocAddress transhipperDocAddress;

	JobDocAddressRequirement TranshipperDocAddressRequirement
	{
		get
		{
			if (transhipperDocAddressRequirement == null)
			{
				transhipperDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Transhipper);
				transhipperDocAddressRequirement.CanOverride = false;
				DocAddressManager.AddRequirement(transhipperDocAddressRequirement);
			}
			return transhipperDocAddressRequirement;
		}
	}

	JobDocAddressRequirement transhipperDocAddressRequirement;

	public ZString TranshipperCode => TranshipperDocAddress?.Organisation?.CustomsCodes.GetCustomsRegNo(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, Core.Constants.CountryCodes.India) ?? ZString.Empty;

	#endregion

	protected override OrgHeaderCollection GetOrgHeaderListCore(DocAddressType addressType)
	{
		return addressType switch
		{
			DocAddressType.CustomsExportOrientedUnitsAddress => Lookups.ExportOrientedUnitsCollection,
			DocAddressType.Transhipper => Lookups.TranshipperCollection,
			_ => base.GetOrgHeaderListCore(addressType)
		};
	}

	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_ExaminationDate", ShortCaption = "Exam. Dt.", MediumCaption = "Exam. Date", Caption = "Examination Date")]
	public override ZDateTime JE_ExaminationDate { get => base.JE_ExaminationDate; set => base.JE_ExaminationDate = value; }

	[MaxLength(AutoINJobDeclaration.Schema.JE_ExaminingOfficerDesignationMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_ExaminingOfficerDesignation", ShortCaption = "Designation", MediumCaption = "Officer Designation", Caption = "Examining Officer Designation")]
	public override ZString JE_ExaminingOfficerDesignation { get => base.JE_ExaminingOfficerDesignation; set => base.JE_ExaminingOfficerDesignation = value; }

	[MaxLength(AutoINJobDeclaration.Schema.JE_ExaminingOfficerNameMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_ExaminingOfficerName", ShortCaption = "Exam. Off. Name", MediumCaption = "Exam. Officer Name", Caption = "Examining Officer Name")]
	public override ZString JE_ExaminingOfficerName { get => base.JE_ExaminingOfficerName; set => base.JE_ExaminingOfficerName = value; }

	[MaxLength(Schema.JE_SealByMaxLength)]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SealByCodeList))]
	[ResourceStringData("F4DF329F-760B-4921-87F0-9EAE31EA8196", ShortCaption = "Seal By", MediumCaption = "Seal By", Caption = "Seal By")]
	public override ZString JE_SealBy { get => base.JE_SealBy; set => base.JE_SealBy = value; }

	[MaxLength(AutoINJobDeclaration.Schema.JE_RotationNumberMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_RotationNumber", ShortCaption = "Rotation No.", MediumCaption = "Rotation No.", Caption = "Rotation Number")]
	public override ZString JE_RotationNumber { get => base.JE_RotationNumber; set => base.JE_RotationNumber = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_RotationDate", ShortCaption = "Date", MediumCaption = "Rotation Dt.", Caption = "Rotation Date")]
	public override ZDateTime JE_RotationDate { get => base.JE_RotationDate; set => base.JE_RotationDate = value; }

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.StuffingAtList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_StuffingAt", ShortCaption = "Stuffing", MediumCaption = "Stuffing At", Caption = "Stuffing At")]
	public override ZString JE_StuffingAt
	{
		get => base.JE_StuffingAt;
		set
		{
			var oldValue = JE_StuffingAt;
			base.JE_StuffingAt = value;
			if (!IsCopying && oldValue != JE_StuffingAt)
			{
				SetExportOrientedUnitsDocAddress(SupplierDocumentaryAddress);
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SampleAccompaniedList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_SampleAccompanied", Caption = "Sample Accompanied", ShortCaption = "Sample Accompanied", MediumCaption = "Sample Accompanied")]
	public override ZString JE_SampleAccompanied { get => base.JE_SampleAccompanied; set => base.JE_SampleAccompanied = value; }

	[MaxLength(AutoINJobDeclaration.Schema.JE_SupervisingOfficerDesignationMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_SupervisingOfficerDesignation", ShortCaption = "Sup. Designation", MediumCaption = "Sup. Officer Designation", Caption = "Supervising Officer Designation")]
	public override ZString JE_SupervisingOfficerDesignation { get => base.JE_SupervisingOfficerDesignation; set => base.JE_SupervisingOfficerDesignation = value; }

	[MaxLength(AutoINJobDeclaration.Schema.JE_SupervisingOfficerNameMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_SupervisingOfficerName", ShortCaption = "Sup. Off. Name", MediumCaption = "Sup. Officer Name", Caption = "Supervising Officer Name")]
	public override ZString JE_SupervisingOfficerName { get => base.JE_SupervisingOfficerName; set => base.JE_SupervisingOfficerName = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|JE_Commissionerate", Caption = "Commissionerate", MediumCaption = "Comm.", ShortCaption = "Comm.")]
	public override ZString JE_Commissionerate { get => base.JE_Commissionerate; set => base.JE_Commissionerate = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|JE_Division", Caption = "Division", MediumCaption = "Division", ShortCaption = "Div.")]
	public override ZString JE_Division { get => base.JE_Division; set => base.JE_Division = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|JE_Range", Caption = "Range", MediumCaption = "Range", ShortCaption = "Range")]
	public override ZString JE_Range { get => base.JE_Range; set => base.JE_Range = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|JE_SealNo", Caption = "Seal No", MediumCaption = "Seal No", ShortCaption = "Seal No")]
	public override ZString JE_SealNo { get => base.JE_SealNo; set => base.JE_SealNo = value; }

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.VerifiedList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_Verified", Caption = "Verified?", ShortCaption = "Verified?", MediumCaption = "Verified?")]
	public override ZString JE_Verified { get => base.JE_Verified; set => base.JE_Verified = value; }

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SampleForwardedList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobDeclaration|JE_SampleForwarded", Caption = "Sample forwarded", ShortCaption = "Sample fwd.", MediumCaption = "Sample fwd.")]
	public override ZString JE_SampleForwarded { get => base.JE_SampleForwarded; set => base.JE_SampleForwarded = value; }

	public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
	{
		return IsExport ? new ExportDeclarationJobDocAddressValidation(addressToValidate) : base.PiggyBackedDocAddressValidation(addressToValidate);
	}
}
