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
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
namespace Enterprise.Customs.NL.Business.Declaration;

public partial class JobDeclaration : AutoNLJobDeclaration
	, Integration.Customs.NL.IJobDeclaration
	, IInvoicesProvider
{
	public JobDeclaration(BusinessObjectFactory factory, DataRow row)
	: base(factory, row)
	{
	}

	protected override string GetDeclarantTypeByDefault() => ZString.Empty;

	public IReadOnlyList<ZString> DistinctCountriesOnInvoiceLines => InvoiceLines.Select(l => string.IsNullOrEmpty(l.JI_RN_NKCountryOfExport) ? (Origin?.Country.Code ?? ZString.Empty) : l.JI_RN_NKCountryOfExport).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();

	[ChildEditable(false)]
	public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

	[ChildEditable]
	public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

	[ChildEditable(true)]
	public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

	public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

	public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => new DeclarationJobDocAddressValidation(addressToValidate, this);

	public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

	protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

	protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

	protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

	protected override Customs.Business.JobDeclarationLookups GetNewLookups()
	{
		JobDeclarationLookups result;
		if (IsImport)
		{
			result = new ImportJobDeclarationLookups(this);
		}
		else
		{
			result = new JobDeclarationLookups(this);
		}
		return result;
	}

	protected override Customs.Business.JobDeclarationValidation GetNewValidation()
	{
		JobDeclarationValidation result;
		if (IsImport)
		{
			result = new ImportJobDeclarationValidation(this);
		}
		else if (IsExport)
		{
			result = new ExportJobDeclarationValidation(this);
		}
		else
		{
			result = new JobDeclarationValidation(this);
		}
		return result;
	}

	protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

	protected override bool IsCustomsLineAmendmentATotalReplacement => false;

	protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Netherlands;

	protected override bool HasSplitEntriesCore
	{
		get
		{
			if (!GetType().FullName.Contains("NL"))
			{
				ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
			}
			return base.HasSplitEntriesCore;
		}
	}

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOfficesList))]
	public override ZString JE_CustomsOffice { get => base.JE_CustomsOffice; set => base.JE_CustomsOffice = value; }

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LocationQualifierList))]
	[MaxLength(1)]
	public override ZString JE_LocationQualifier { get => base.JE_LocationQualifier; set => base.JE_LocationQualifier = value; }

	#region PaymentPartyEORINumber
	[ReadOnly(true)]
	[ResourceStringData("NL.JobDeclaration.PaymentPartyEoriNumber", Caption = "Payment Party EORI Number", MediumCaption = "EORI Number")]
	public ZString PaymentPartyEORINumber
	{
		get
		{
			OrgHeader organisation = null;

			switch (JE_DeclarantType)
			{
				case Enterprise.Customs.EU.Business.RepresentationTypeList.Codes._1Self:
					organisation = Declarant.Header;
					break;
				case Enterprise.Customs.EU.Business.RepresentationTypeList.Codes._2Direct:
					if (JE_PaymentMethod == Enterprise.Customs.EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority)
					{
						organisation = ControllingAgent;
					}
					else if (JE_PaymentMethod == Enterprise.Customs.EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14)
					{
						organisation = Declarant.Header;
					}
					break;
				case Enterprise.Customs.EU.Business.RepresentationTypeList.Codes._3Indirect:
					if (JE_PaymentMethod == Enterprise.Customs.EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority)
					{
						organisation = Declarant.Header;
					}
					else if (JE_PaymentMethod == Enterprise.Customs.EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14)
					{
						organisation = DefermentPartyDocAddress.Organisation;
					}
					break;
			}

			if (organisation == null)
			{
				if (JE_PaymentMethod == Enterprise.Customs.EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14 && Importer != null)
				{
					organisation = Importer;
				}
				else if (JE_PaymentMethod != Enterprise.Customs.EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14 && !JE_PaymentMethod.IsEmpty && ControllingAgent != null)
				{
					organisation = ControllingAgent;
				}
			}

			if (organisation == null || JE_PaymentMethod.IsEmpty)
			{
				return ZString.Empty;
			}

			return organisation.GetIdentificationNumber();
		}
	}

	public ZPropertyInfo PaymentPartyEORINumberInfo
	{
		get { return GetZPropertyInfo(nameof(PaymentPartyEORINumber)); }
	}

	#endregion

	#region VATPartyTaxNumber
	[ReadOnly(true)]
	[ResourceStringData("NL.JobDeclaration.VatPartyTaxNumber", Caption = "VAT Party Tax Number", MediumCaption = "VAT Number")]
	public ZString VATPartyTaxNumber
	{
		get
		{
			ZString taxNumber;
			OrgHeader organisation = DefermentPartyDocAddress.Organisation;
			if (organisation == null)
			{
				return ZString.Empty;
			}

			var lfrAuthorisation = CusAuthorisationHeader.Loader.GetAuthorisation(Factory, Core.Constants.CountryCodes.Netherlands, NLCusAuthorisationHeaderTypeList.Codes.LFR, ZDate.Today, organisation.PK);
			var lfrCusCode = organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode, Core.Constants.CountryCodes.Netherlands);
			if (lfrAuthorisation == null || (lfrAuthorisation != null && lfrCusCode.IsEmpty))
			{
				taxNumber = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + organisation.CustomsCodes.GetCustomsRegNo(ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			}
			else
			{
				taxNumber = CountryCodes.Netherlands +  organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode, Core.Constants.CountryCodes.Netherlands);
			}
			return taxNumber;
		}
	}

	public ZPropertyInfo VATPartyTaxNumberInfo
	{
		get { return GetZPropertyInfo(nameof(VATPartyTaxNumber)); }
	}
	#endregion

	protected override void DefermentPartyDocAddressChanged(object sender, EventArgs e)
	{
		base.DefermentPartyDocAddressChanged(sender, e);
		MarkAsNeedingValidation();
	}

	public override ZString JE_TransportModeInland
	{
		get => base.JE_TransportModeInland;
		set
		{
			var oldValue = JE_TransportModeInland;
			base.JE_TransportModeInland = value;
			if (oldValue != JE_TransportModeInland)
			{
				InlandTransports.MarkAsNeedingValidation();
				InlandTransports.SetMaxCountValidation();
				BorderTransports.MarkAsNeedingValidation();
			}
		}
	}

	public new InlandTransportCollection InlandTransports => (InlandTransportCollection)base.InlandTransports;

	protected override EU.Business.Declaration.InlandTransportCollection GetNewInlandTransportCollection() => new InlandTransportCollection(this);

	protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
	{
		var result = base.GetCusCodeDataTypesCore();
		result[EU.Business.CusCodeDataTypeList.Codes.TransportInland] = typeof(InlandTransport);
		result[CusCodeDataTypeList.Codes.TransportAtBorder] = typeof(BorderTransport);
		return result;
	}

	protected override EU.Business.JobDeclarationCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new JobDeclarationCustomsOfficeRequirementHelper(this);

	#region public FiscalReferenceCollection FiscalReferences
	[ChildEditable(true)]
	public FiscalReferenceCollection FiscalReferences
	{
		get { return fFiscalReferences ?? (fFiscalReferences = GetFiscalReferences()); }
	}
	FiscalReferenceCollection fFiscalReferences;

	FiscalReferenceCollection GetFiscalReferences()
	{
		var result = CreateNewFiscalReferenceCollection();
		result.Load();
		RegisterEditableChildObject(result);
		return result;
	}

	protected virtual FiscalReferenceCollection CreateNewFiscalReferenceCollection()
	{
		return new FiscalReferenceCollection(this);
	}
	#endregion

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.FiscalReference] = typeof(FiscalReference);
		result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		return result;
	}

	[ChildEditable(true)]
	public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

	protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

	protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this);

	[ChildEditable(true)]
	[UniversalCopyCollectionEntity(CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.Constants.CEI_JE)]
	public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (ICusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;

	[ChildEditable]
	public BorderTransportCollection BorderTransports
	{
		get
		{
			if (borderTransports == null)
			{
				borderTransports = new BorderTransportCollection(this);
				borderTransports.Load();
				RegisterEditableChildObject(borderTransports);
			}

			return borderTransports;
		}
	}
	BorderTransportCollection borderTransports;

	IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer()
	{
		return new DeclarationValueChangedAnnouncer(this);
	}

	[ResourceStringData("A3A4FD1B-5CD8-4699-A20D-121517B4F7F9", Caption = "Transport", FullDescription = "[UCC 7/4] Transport")]
	public override ZString JE_TransportMode
	{
		get => base.JE_TransportMode;
		set
		{
			base.JE_TransportMode = value;
			BorderTransports.MarkAsNeedingValidationIncludingChildren();
		}
	}

	public override ZGuid JE_GB
	{
		get => base.JE_GB;
		set
		{
			var oldValue = JE_GB;
			base.JE_GB = value;
			if (!IsCopying && oldValue != JE_GB && !oldValue.IsEmpty)
			{
				BorderTransports.MarkAsNeedingValidation();
			}
		}
	}

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

	#region CustomsAccount
	[ResourceStringData("NL.JobDeclaration.CustomsAccount", Caption = "Customs Account")]
	public ZString CustomsAccount
	{
		get
		{
			return GetCustomsAccount();
		}
	}

	public ZPropertyInfo CustomsAccountInfo
	{
		get { return GetZPropertyInfo(nameof(CustomsAccount)); }
	}

	public ZString GetCustomsAccount()
	{
		var orgHeaderPK = ZGuid.Empty;
		switch (JE_DeclarantType)
		{
			case EU.Business.RepresentationTypeList.Codes._2Direct:
				orgHeaderPK = JE_OH_ControllingAgent;
				break;
			case EU.Business.RepresentationTypeList.Codes._1Self:
			case EU.Business.RepresentationTypeList.Codes._3Indirect:
				orgHeaderPK = DeclarantAddress?.OA_OH ?? ZGuid.Empty;
				break;
		}
		return orgHeaderPK.IsValid ? this.GetSenderInfoCustomsAccount(orgHeaderPK) : ZString.Empty;
	}
	#endregion

	#region BindingSource for ContainerUserControl

	protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection()
	{
		return new BaseCusContainerCollection<CusContainer>(this, base.Factory);
	}

	#endregion
	protected override void DecorateDocAddressRequirement(JobDocAddressRequirement requirement, DocAddressType addressType)
	{
		base.DecorateDocAddressRequirement(requirement, addressType);

		switch (addressType)
		{
			case DocAddressType.SupplierDocumentaryAddress:
				requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address
					+=
					new JobDocAddressRequirement.ValidationDelegate(Validation.ValidateSupplierDocumentaryAddress);
				break;

			case DocAddressType.ImporterDocumentaryAddress:
				requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address
					+=
					new JobDocAddressRequirement.ValidationDelegate(Validation.ValidateImporterDocumentaryAddress);
				break;
		}
	}

	protected override void ImporterDocumentaryAddressChanged(object sender, EventArgs e)
	{
		base.ImporterDocumentaryAddressChanged(sender, e);

		if (ImporterDocumentaryAddress?.Organisation != null)
		{
			var lfrAuthorisation = CusAuthorisationHeader.Loader.GetAuthorisation(Factory, Core.Constants.CountryCodes.Netherlands, NLCusAuthorisationHeaderTypeList.Codes.LFR, ZDate.Today, ImporterDocumentaryAddress.OrganisationPK);
			if (lfrAuthorisation == null)
			{
				var vatNumbers = ImporterDocumentaryAddress.Organisation.GetAllVatNumbers();
				var foreignVat = vatNumbers.Where(x => !x.OK_RN_NKCodeCountry.Equals(Core.Constants.CountryCodes.Netherlands));

				if (!vatNumbers.IsNullOrEmpty() && foreignVat.IsNullOrEmpty())
				{
					DefermentPartyDocAddress.E2_OA_Address = ImporterDocumentaryAddress.E2_OA_Address;
				}
			}
			else
			{
				if (!lfrAuthorisation.PermitHolder.CustomsCodes.GetCustomsRegNo(OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode, Core.Constants.CountryCodes.Netherlands).IsEmpty)
				{
					DefermentPartyDocAddress.E2_OA_Address = ImporterDocumentaryAddress.E2_OA_Address;
				}
			}
		}
	}

	[ResourceStringData("055B2377-99E3-4E20-922E-6992DB6153FB", Caption = "Start")]
	public override ZDateTime ZG_PresentationStartDate { get => base.ZG_PresentationStartDate; set => base.ZG_PresentationStartDate = value; }

	[ResourceStringData("A9B433BB-144D-49A3-BFF4-CA9A1A85E7C6", Caption = "End")]
	public override ZDateTime ZG_PresentationEndDate { get => base.ZG_PresentationEndDate; set => base.ZG_PresentationEndDate = value; }

	#region IDocumentSupport Members
	protected override DocumentSupporter CreateNewDocumentSupporter()
	{
		return new JobDeclarationDocumentSupporter(this);
	}
	#endregion

	[ResourceStringData("NL.JobDeclaration.JE_OH_ControllingCustomer", Caption = "Controlling Customer")]
	public override ZGuid JE_OH_ControllingCustomer { get => base.JE_OH_ControllingCustomer; set => base.JE_OH_ControllingCustomer = value; }

	[ResourceStringData("4DA2E5F3-4A5C-42D1-ACC0-F25E657FAC00",  Caption = "[UCC 2/4] DUCR")]
	public override ZString JE_UCR { get => base.JE_UCR; set => base.JE_UCR = value; }

	[ResourceStringData("123E2623-971D-4C54-A2D3-D3764462CE46", Caption = "[UCC 4/1] Incoterm")]
	public override ZString JE_ShipmentIncoTerm { get => base.JE_ShipmentIncoTerm; set => base.JE_ShipmentIncoTerm = value; }

	[ResourceStringData("C1296A0A-E995-4BB5-BCD1-89F1D2092C78", Caption = "[UCC 5/14] Dispatch")]
	public override ZString JE_RL_NKOrigin { get => base.JE_RL_NKOrigin; set => base.JE_RL_NKOrigin = value; }

	[ResourceStringData("467C1BE7-06D4-4B0B-8EE9-6A707DFA798E", Caption = "Destination", FullDescription = "[UCC 5/8] Destination")]
	public override ZString JE_RL_NKFinalDestination { get => base.JE_RL_NKFinalDestination; set => base.JE_RL_NKFinalDestination = value; }

	[ResourceStringData("EB0281BA-0950-4A59-A136-DF683D1C5237", Caption = "Circumstance", FullDescription = "[UCC 1/7] Circumstance")]
	public override ZString ZG_SpecificCircumstanceIndicator { get => base.ZG_SpecificCircumstanceIndicator; set => base.ZG_SpecificCircumstanceIndicator = value; }

	[ResourceStringData("7B60C16D-B22C-48A0-B128-46EE42FF5318", Caption = "Vessel", FullDescription = "[UCC 7/7] Vessel")]
	public override ZString JE_VesselName { get => base.JE_VesselName; set => base.JE_VesselName = value; }

	public bool AreTransportChargedAndSpecificCircumstanceVisibleForDeclarationType()
	{
		ZString[] declarationTypesWhereNotVisible = new ZString[] { NLConstants.EntryStyles.UnionGoods, NLConstants.EntryStyles.SpecialFiscalTerritory };

		if (IsImport || (IsExport && CustomsEntryInstructions.Any(x => declarationTypesWhereNotVisible.Contains(x.CEI_Style))))
		{
			return false;
		}

		return true;
	}

	protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
	{
		base.JE_MessageTypeChanged(oldValue, newValue);

		if (oldValue != newValue && IsExport && SupplierDocumentaryAddress.Organisation != null)
		{
			JE_DeclarantType = NLUniversalHelper.GetDeclarantType(SupplierDocumentaryAddress.Organisation);
		}

		if (oldValue == MessageTypeList.Codes.Export)
		{
			foreach (var header in Invoices.Cast<JobComInvoiceHeader>())
			{
				header.JZ_UCR = ZString.Empty;
				header.ZG_TransportChargesMethodOfPayment = ZString.Empty;
			}

			foreach (var invoiceLine in FilteredInvoiceLines.Cast<JobComInvoiceLine>())
			{
				invoiceLine.JI_OA_ConsigneeAddress = ZGuid.Empty;
				invoiceLine.JI_OA_ExporterAddress = ZGuid.Empty;
			}
			ItineraryCountries.RemoveAndDeleteAll();
			ZG_CTStatusID = ZString.Empty;
		}
	}

	[ResourceStringData("6AD1178E-E9AC-4C16-9DC0-B287FAEC8A52", Caption = "Nationality", FullDescription = "[UCC 7/8] Nationality")]
	[ReadOnlyMember(nameof(IsTransportNationalityDisabled))]
	public override ZString JE_RN_NKTransportNationality { get => base.JE_RN_NKTransportNationality; set => base.JE_RN_NKTransportNationality = value; }

	ZBool IsTransportNationalityDisabled => new ZString[] { TransportModes.Rail, TransportModes.Mail, TransportModes.FixedTransportInstallations }.Contains(JE_TransportMode);

	[ReadOnlyMember(nameof(IsTransportIDInlandDisabled))]
	public override ZString JE_TransportIDInland { get => base.JE_TransportIDInland; set => base.JE_TransportIDInland = value; }

	[ReadOnlyMember(nameof(JE_DeclarantTypeReadOnly))]
	[ResourceStringData("D4B085D0-5C28-40EC-ACC9-F267FFD7345D", Caption = "Rep. Type", FullDescription = "[UCC 3/21] Rep. Type")]
	public override ZString JE_DeclarantType
	{
		get => base.JE_DeclarantType;
		set
		{
			var oldValue = base.JE_DeclarantType;
			base.JE_DeclarantType = value;
		}
	}

	protected virtual bool JE_DeclarantTypeReadOnly => IsExport;

	public ZBool IsTransportIDInlandDisabled => IfDisableForUC9008 || IfDisableForUC9009;

	[ReadOnlyMember(nameof(IsTransportMeansDisabled))]
	public override ZString JE_TransportMeans { get => base.JE_TransportMeans; set => base.JE_TransportMeans = value; }

	public ZBool IsTransportMeansDisabled => IfDisableForUC9008 || IfDisableForUC9009;

	ZBool IfDisableForUC9008 => FilteredInvoiceLines.Count > 0 && new string[] { NLConstants.ProcedureCodes._10, NLConstants.ProcedureCodes._11, NLConstants.ProcedureCodes._23, NLConstants.ProcedureCodes._31 }.Any(prefix => FilteredInvoiceLines.Any(line => line.JI_FormattedProcedure.StartsWith(prefix))) && new ZString[] { TransportModes.Rail, TransportModes.Mail, TransportModes.FixedTransportInstallations }.Contains(JE_TransportMode) && IsExport;

	public ZBool IfDisableForUC9009 => FilteredInvoiceLines.Count > 0 && new string[] { NLConstants.ProcedureCodes._76, NLConstants.ProcedureCodes._77 }.Any(prefix => FilteredInvoiceLines.Any(line => line.JI_FormattedProcedure.StartsWith(prefix))) && new ZString[] { TransportModes.Mail, TransportModes.FixedTransportInstallations }.Contains(JE_TransportMode) && IsExport;

	public JobComInvoiceLine FirstFilteredInvoiceLine => FilteredInvoiceLines.FirstOrDefault() as JobComInvoiceLine;

	public bool HasProcedureIntoWarehouse => InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.CusProcedure != null && x.CusProcedure.IsIntoWarehouse());

	public bool HasProcedureOutOfWarehouse => InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.CusProcedure != null && x.CusProcedure.IsOutOfWarehouse());

	HashSet<string> procedureListC9008 => new HashSet<string> { "10", "11", "23", "31" };
	public bool HasInvoiceLineWithC9008Procedure => FilteredInvoiceLines.Any(x => procedureListC9008.Contains(x.JI_Procedure.Substring(0, 2)));

	public ZString GetSenderInfoCustomsAccount(ZGuid orgHeaderPK) => Factory.GetCachedValue("NL.JobDeclarationSenderInfoHelper.CustomsAccount" + RegistryCompanyPK + RegistryBranchPK + orgHeaderPK, () =>
	{
		var senderInfos = NLCustomsRegistry.Instance.SenderIDs.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
		var senderInfo = senderInfos.OfType<SenderInfo>().FirstOrDefault(x => x.OrganizationPK == orgHeaderPK);
		return senderInfo != null ? senderInfo.SenderID : ZString.Empty;
	});

	public override void OnSaving()
	{
		base.OnSaving();
		if(IsExport)
		{
			JE_DeclarantType = JE_OA_Representative != JE_OA_DeclarantAddress ? RepresentationTypeList.Codes._2Direct : RepresentationTypeList.Codes._3Indirect;
		}
	}

	protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobDeclarationFetchStrategy(this);

	public void SetDeclarantTypeFromSupplier()
	{
		if (IsExport)
		{
			JE_DeclarantType = NLUniversalHelper.GetDeclarantType(SupplierDocumentaryAddress.Organisation);
		}
	}

	public override ZGuid JE_OH_Supplier
	{
		get
		{
			return base.JE_OH_Supplier;
		}
		set
		{
			var oldValue = base.JE_OH_Supplier;
			base.JE_OH_Supplier = value;
			SupplierDocumentaryAddress.OrganisationPK = value;
			SetDeclarantTypeFromSupplier();
		}
	}

	#region Defaulting Disabled

	protected override void SetRepresentationTypeIfMatchingEORICodes()
	{
	}

	protected override void SetDefaultValuesForDeclarantAndRepresentativeWithOrgProxy(OrgHeader header)
	{
	}

	#endregion

	protected override IValueSetStrategy GetValueSetStrategy() => DeclarationValueSetStrategy;

	internal JobDeclarationValueSetStrategy DeclarationValueSetStrategy => declarationValueSetStrategy ??= new JobDeclarationValueSetStrategy(this);
	JobDeclarationValueSetStrategy declarationValueSetStrategy;

	protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser() => new JobDeclarationSynchroniser(this);

	[ReadOnlyMember(nameof(IsRepresentativeReadOnly))]
	public override ZGuid JE_OA_Representative { get => base.JE_OA_Representative; set => base.JE_OA_Representative = value; }

	protected bool IsRepresentativeReadOnly => IsExport && (JE_DeclarantType == EU.Business.RepresentationTypeList.Codes._3Indirect || JE_DeclarantType == EU.Business.RepresentationTypeList.Codes._1Self);

	protected override void SetupSupplierDocumentaryAddress(JobDocAddress supplierDocumentaryAddress)
	{
		base.SetupSupplierDocumentaryAddress(supplierDocumentaryAddress);
		supplierDocumentaryAddress.MarkParentAsNeedingValidation = true;
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		if (IsExport)
		{
			ZG_CTStatusID = NLConstants.CTStatusCodes.C;
			ZG_TypeOfSecurity = NLConstants.SecurityCodes._0;
		}
	}

	protected override ZBool ContainerControlCheckboxVisibleCore => true;

	protected override ZBool ContainerUnloadedCheckboxVisibleCore => true;

	public EntrySelectionCollection EntrySelections
	{
		get
		{
			if (entrySelections == null)
			{
				entrySelections = new EntrySelectionCollection(this);
			}

			return entrySelections;
		}
	}
	EntrySelectionCollection entrySelections;
}
