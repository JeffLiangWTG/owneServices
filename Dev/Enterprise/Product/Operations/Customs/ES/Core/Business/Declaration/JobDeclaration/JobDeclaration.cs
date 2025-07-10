using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.Declaration;

[UniversalCopyAddInfo(JobDeclarationSchema.Constants.Prefix, EU.Business.AddInfo.Schema.Prefix)]
public partial class JobDeclaration : AutoESJobDeclaration
	, Integration.Customs.ES.IJobDeclaration, ICommonInvoiceDataProvider, IApportionInvoiceHolder, IAddInfoChildOverrideTypeSupporter
{
	public JobDeclaration(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	#region Schema

	public new partial class Schema : EU.Business.Declaration.JobDeclaration.Schema
	{
		public const string DeclEmailAddr = "DeclEmailAddr";
	}

	#endregion

	protected override IDeclarationLevelPackageCollection<BasePackage> GetPackagesCollection() => new BaseDeclarationLevelPackageCollection<Package>(this);

	protected override bool ZG_AgreedPlaceCodeValidationSupportCore
		=> ((IsUCC6AndIsExport && !JE_ShipmentIncoTerm.IsEmpty && JE_ShipmentIncoTerm != Core.Constants.IncoTerms.Other) || !IsUCC6AndIsExport) && HasAnyDiffT2CAndT2lAndEXSEntry;

	#region Override Properties

	public override ZString JE_ShipmentIncoTermPlace
	{
		get => base.JE_ShipmentIncoTermPlace;
		set
		{
			var oldValue = JE_ShipmentIncoTermPlace;
			base.JE_ShipmentIncoTermPlace = value;
			if (!IsCopying && oldValue != JE_ShipmentIncoTermPlace)
			{
				Invoices.MarkAsNeedingValidation();
			}
		}
	}

	#region JE_CustomsOffice

	public override ZString JE_CustomsOffice
	{
		get => base.JE_CustomsOffice;
		set
		{
			var oldPartialWriteoff_ReadOnly = ZG_PartialWriteoff_ReadOnly;
			var oldValue = JE_CustomsOffice;
			base.JE_CustomsOffice = value;
			if (!IsCopying && oldValue != JE_CustomsOffice)
			{
				var newPartialWriteoff_ReadOnly = ZG_PartialWriteoff_ReadOnly;
				if (newPartialWriteoff_ReadOnly && newPartialWriteoff_ReadOnly != oldPartialWriteoff_ReadOnly)
				{
					ZG_PartialWriteoff = ZBool.False;
				}
			}
		}
	}

	#endregion

	#region JE_DeclarantType

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.RepresentationTypeList))]
	public override ZString JE_DeclarantType
	{
		get => base.JE_DeclarantType;
		set => base.JE_DeclarantType = value;
	}

	#endregion

	string IApportionInvoiceHolder.CountryContext
	{
		get { return CountryCode + this.GetIncoTermChargeFactoryCacheKey(); }
	}

	public override ZString JE_MessageType
	{
		get => base.JE_MessageType;
		set
		{
			var hasChanged = value != JE_MessageType;
			if (hasChanged)
			{
				base.JE_MessageType = value;
				NeedToGetNewIncoTermAndChargeFactory = true;
				foreach (var invoice in Invoices)
				{
					invoice.NeedToGetNewIncoTermAndChargeFactory = true;
				}
				Invoices.MarkAsNeedingValidation();
				Packages.MarkAsNeedingValidation();
				Equipments.MarkAsNeedingValidationIncludingChildren();
			}
		}
	}

	public override ZString JE_GoodsOrigin
	{
		get => base.JE_GoodsOrigin;
		set
		{
			var oldValue = JE_GoodsOrigin;
			base.JE_GoodsOrigin = value;
			if (!IsCopying && oldValue != JE_GoodsOrigin)
			{
				InvoiceLines.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString JE_ShipmentIncoTerm
	{
		get => base.JE_ShipmentIncoTerm;
		set
		{
			var oldvalue = JE_ShipmentIncoTerm;
			if (!IsCopying && oldvalue != value)
			{
				base.JE_ShipmentIncoTerm = value;
				Invoices.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString RepresentationTypeNo => JE_DeclarantType.ToString();

	[ResourceStringData("B2D895F5-BD62-4EBC-8ACB-FF7ECA391327", Caption = "Region of Destination", MediumCaption = "Reg. Destination", ShortCaption = "Reg. Dest.", FullDescription = "State or Region of Destination")]
	public override ZString EUD_RegionOrTerritoryOfDestination { get => base.EUD_RegionOrTerritoryOfDestination; set => base.EUD_RegionOrTerritoryOfDestination = value; }

	public bool IsGoodsDestinationESOrXCOrXLOrEmpty => IsDestinationESOrXCOrXLOrEmpty(JE_GoodsDestination);

	internal bool IsDestinationESOrXCOrXLOrEmpty(string destination)
		=>	destination == Core.Constants.CountryCodes.Spain ||
			destination == Core.Constants.NonStandardCountryCodes.Codes.XC ||
			destination == Core.Constants.NonStandardCountryCodes.Codes.XL ||
			destination == ZString.Empty;

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DestinationStateIslandCodeList))]
	public override ZString JE_DestinationState
	{
		get => base.JE_DestinationState;
		set => base.JE_DestinationState = value;
	}

	public override ZString ZG_DestinationState
	{
		get { return base.ZG_DestinationState; }
		set
		{
			if (value != ZG_DestinationState)
			{
				base.ZG_DestinationState = value;
				if (!IsCopying && IsImport)
				{
					InvoiceLines.MarkAsNeedingValidation();
					SetDefaultMethodOfPaymentForInvoiceLines();
				}
			}
		}
	}

	[ResourceStringData("57A4AC5A-961B-4D9A-8285-2024F73D2AAE", Caption = "Broker", MediumCaption = "Broker", ShortCaption = "Broker", FullDescription = "The broker selected will be the responsible of declarations to Customs in this Job")]
	public override ZString JE_GS_NKCusAgent
	{
		get => base.JE_GS_NKCusAgent;
		set
		{
			var oldValue = base.JE_GS_NKCusAgent;
			base.JE_GS_NKCusAgent = value;

			if (!IsCopying && oldValue != value)
			{
				SetDefaultJE_CustomsProfile();
			}
		}
	}

	void SetDefaultJE_CustomsProfile()
	{
		var certificateNamesList = Lookups.CertificateNames;
		var customsProfile = JE_CustomsProfile;
		if (customsProfile.IsEmpty || !certificateNamesList.GetAllCodesZString().Contains(customsProfile))
		{
			if (certificateNamesList.Count == 1)
			{
				JE_CustomsProfile = certificateNamesList[0].Code;
			}
			else
			{
				JE_CustomsProfile = ZString.Empty;
			}
		}
	}

	public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => new DeclarationJobDocAddressValidation(addressToValidate, this);

	public override ZString DeclarationNumber => GetDeclarationNumber();

	ZString GetDeclarationNumber()
	{
		var nonEmptyMrns = CustomsEntryHeaders.Select(x => x.MovementReferenceNumber).Where(eh => !eh.IsEmpty).Take(2).ToArray();
		switch (nonEmptyMrns.Length)
		{
			case 2:
				return Res.GetString("EFDD1A47-FE08-446E-B761-4039D5B9103F", "Multiple");

			case 1:
				return nonEmptyMrns[0];

			default:
				return ZString.Empty;
		}
	}

	public ZDateTime LatestEntryAcceptanceDate
	{
		get
		{
			var resultDate = ZDateTime.Empty;
			if (CustomsEntryHeaders.Count == 1)
			{
				var entryHeader = CustomsEntryHeaders.First();
				resultDate = entryHeader.MovementReferenceNumberIssueDate;
			}
			else if (CustomsEntryHeaders.Count > 1)
			{
				foreach (CusEntryHeader entryHeader in CustomsEntryHeaders)
				{
					if (entryHeader.MovementReferenceNumberIssueDate > resultDate || resultDate.IsEmpty)
					{
						resultDate = entryHeader.MovementReferenceNumberIssueDate;
					}
				}
			}

			return resultDate;
		}
	}

	[ResourceStringData("{92164C41-46AF-474B-90DF-FAFD75C0B26C}", Caption = "[21.2] Nationality", MediumCaption = "[21.2] Nationality", ShortCaption = "[21.2] Nationality")]
	public override ZString JE_RN_NKTransportNationality { get => base.JE_RN_NKTransportNationality; set => base.JE_RN_NKTransportNationality = value; }

	[ResourceStringData("{CF9C9AF2-1C03-41E6-915A-2F4185F7D8B5}", Caption = "[18.2] Nationality", MediumCaption = "[18.2] Nationality", ShortCaption = "[18.2] Nationality")]
	public override ZString ZG_Box18TransportNationality { get => base.ZG_Box18TransportNationality; set => base.ZG_Box18TransportNationality = value; }

	[ResourceStringData("{311AAF67-F69E-4512-9BA5-5DE67E39FB3B}", Caption = "[18] Transport ID", MediumCaption = "[18] Transport ID", ShortCaption = "[18] Transport ID")]
	public override ZString ZG_Box18TransportID { get => base.ZG_Box18TransportID; set => base.ZG_Box18TransportID = value; }

	[ResourceStringData("{7F7E99AC-E279-4086-B9FF-56D2C6859C82}", Caption = "[26] Trans. Mode", MediumCaption = "[26] Trans. Mode", ShortCaption = "[26] Trans. Mode")]
	public override ZString JE_TransportModeInland { get => base.JE_TransportModeInland; set => base.JE_TransportModeInland = value; }

	[ResourceStringData("{FD0A7DFC-C5C1-477A-9CC2-9CFD0244B6F1}", Caption = "IATA", MediumCaption = "IATA", ShortCaption = "IATA")]
	public override ZString JE_IATALoadPort { get => base.JE_IATALoadPort; set => base.JE_IATALoadPort = value; }

	public override ZString CustomsClearanceStatus => JE_EntryStatus;

	#endregion

	#region JE_CustomsProfile

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CertificateNames))]
	[ResourceStringData("DA76F12C-B925-44F0-9B23-E8DC27DC7875", Caption = "Certificate", MediumCaption = "Certif.", ShortCaption = "Cert.", FullDescription = "The certificate selected will be used to sign and communicate with Customs to declare all entries in this Job")]
	public override ZString JE_CustomsProfile { get => base.JE_CustomsProfile; set => base.JE_CustomsProfile = value; }

	#endregion

	public ESOrgImpAddInfo ImporterAddInfo
	{
		get { return Importer != null ? ESOrgImpAddInfo.Get(Importer) : null; }
	}

	protected override void JE_OH_ImporterChanged(ZGuid oldValue, ZGuid newValue)
	{
		base.JE_OH_ImporterChanged(oldValue, newValue);
		if (IsImport)
		{
			SetDefaultMethodOfPaymentForInvoiceLines();
		}
	}

	void SetDefaultMethodOfPaymentForInvoiceLines()
	{
		var importerAddInfo = ImporterAddInfo;
		if (!ZG_DestinationState.IsEmpty && importerAddInfo != null)
		{
			SetDefaultMOPIfNotEmpty(importerAddInfo);
			SetDefaultMOPCanIfNotEmpty(importerAddInfo);
		}
	}

	void SetDefaultMOPIfNotEmpty(ESOrgImpAddInfo importerAddInfo)
	{
		var mop = importerAddInfo.ZO_MethodOfPayment;
		if (!mop.IsEmpty)
		{
			InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.ZG_MethodOfPayment = mop);
		}
	}

	void SetDefaultMOPCanIfNotEmpty(ESOrgImpAddInfo importerAddInfo)
	{
		if (DestinationStateIsCanaryIsland)
		{
			var mopCan = importerAddInfo.ZO_MethodOfPaymentCan;
			if (!mopCan.IsEmpty)
			{
				InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.ZG_MethodOfPayment2 = mopCan);
			}
		}
	}

	#region SupportingDocuments

	public override int MaxSupportingDocuments => 99;
	public override ZString SupportingDocumentsValidationMessage => Res.GetString("875FF23A-3C8D-41C4-B533-DAEB04C93D5C", "Customs will not accept a declaration with more than 99 documents per line");

	#endregion

	public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

	protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

	public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

	protected override Customs.Business.JobDeclarationValidation GetNewValidation()
	{
		JobDeclarationValidation result;
		if (IsExport)
		{
			result = new ExportJobDeclarationValidation(this);
		}
		else if (IsImport)
		{
			result = new ImportJobDeclarationValidation(this);
		}
		else
		{
			result = new JobDeclarationValidation(this);
		}
		return result;
	}

	public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

	protected override Customs.Business.JobDeclarationLookups GetNewLookups() => new JobDeclarationLookups(this);

	protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

	[ChildEditable]
	public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

	[ChildEditable(true)]
	public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

	public new IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines => (IInvoiceLineViewCollection<JobComInvoiceLine>)base.FilteredInvoiceLines;

	protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

	protected override Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
	{
		return new JobDeclarationDeepCloneStrategy(this, cloneType);
	}

	protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>(this);

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		return result;
	}

	[ChildEditable(false)]
	public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

	protected override EU.Business.Declaration.JobComInvoiceHeaderValidation GetJobComInvoiceHeaderValidationCore(EU.Business.Declaration.JobComInvoiceHeader jobComInvoiceHeader)
	{
		JobComInvoiceHeaderValidation result;
		if (IsExport)
		{
			result = new ExportJobComInvoiceHeaderValidation(jobComInvoiceHeader);
		}
		else if (IsImport)
		{
			result = new ImportJobComInvoiceHeaderValidation(jobComInvoiceHeader);
		}
		else
		{
			result = new JobComInvoiceHeaderValidation(jobComInvoiceHeader);
		}
		return result;
	}

	protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

	public override EU.Business.Declaration.EntryCreationStrategy CreateEntryCreationStrategy() => new EntryCreationStrategy(this);

	protected override Customs.Business.MergeManager GetMergeManager() => new MergeManager(this);

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

	[ChildEditable(true)]
	public new ICusEquipmentCollection<CusEquipment> Equipments => (ICusEquipmentCollection<CusEquipment>)base.Equipments;

	protected override ICusEquipmentCollection<Customs.Business.CusEquipment> GetNewCusEquipmentCollection() => new CusEquipmentCollection<CusEquipment>(this);

	protected override DocumentSupporter CreateNewDocumentSupporter() => new JobDeclarationDocumentSupporter(this);

	protected override EU.Business.Declaration.CusEntryHeaderDocumentSupporter GetCusEntryHeaderDocumentSupporterCore(EU.Business.Declaration.CusEntryHeader entryHeader)
		=> new CusEntryHeaderDocumentSupporter((CusEntryHeader)entryHeader);

	Type IAddInfoChildOverrideTypeSupporter.AddInfoChildType => typeof(JobEUDeclaration);

	public new JobEUDeclaration AddInfoChild => (JobEUDeclaration)base.AddInfoChild;

	public new JobEUDeclarationLookups AddInfoChildLookups => AddInfoChild.Lookups;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		SetDefaultCommonValues();
	}

	void SetDefaultCommonValues()
	{
		SetDefaultEmail();
		SetDefaultZG_IsTrainingDeclaration();
	}

	void SetDefaultEmail()
	{
		ZG_OtherEmailAddr = EmailHelper.GetCurrentUserMainEmail();
	}

	void SetDefaultZG_IsTrainingDeclaration()
	{
		ZG_IsTrainingDeclaration = !Env.Instance.IsProductionSystem;
	}

	#region DeclEmailAddr

	public ZString DeclEmailAddr
	{
		get
		{
			if (!declEmailAddr.HasValue)
			{
				declEmailAddr = EmailHelper.GetDeclEmailAddrFromRegistry();
			}
			return declEmailAddr.Value;
		}
	}
	ZString? declEmailAddr;

	public ZPropertyInfo DeclEmailAddrInfo => GetZPropertyInfo(Schema.DeclEmailAddr);

	#endregion

	protected override string GetDeclarantTypeForMatchingEORICodes() => ESRepresentationTypeList.Codes._1Auto;

	protected override string GetDeclarantTypeByDefault() => ZString.Empty;

	protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
	{
		base.JE_MessageTypeChanged(oldValue, newValue);
		if (newValue == EU.Business.MessageTypeList.Codes.Export)
		{
			InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.JI_ZZF_NKTaxType = ZString.Empty);
		}
	}

	public new EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;

	protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this);

	public new ESGuaranteeCollection Guarantees => (ESGuaranteeCollection)base.Guarantees;

	protected override EU.Business.Declaration.GuaranteeForDeclarationCollection GetGuaranteesCore()
		=> new ESGuaranteeCollection(this);

	public bool DestinationStateIsCanaryIsland
	{
		get
		{
			bool IsCanaryIslandsDestination() => CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).ContainsCode(ZG_DestinationState);

			destinationStateIsCanaryIslandCached = new(Factory, () => IsImport && ((IsUCC6 && IsCustomOfficeCanaryIsland) || (!IsUCC6 && IsCanaryIslandsDestination())));
			return destinationStateIsCanaryIslandCached.Value;
		}
	}
	CachedProperty<bool> destinationStateIsCanaryIslandCached;

	public override bool IsDeclarantAddressRequired => false;

	protected override ZString Box30LocationOfGoodsForDocumentsAndMessagingCore => JE_LocationOfGoods + SubLocation;

	protected override ZString SupplierTraderIdCore => OrgHeaderExtension.GetIDCode(Supplier);

	protected override ZString ImporterTraderIdCore => OrgHeaderExtension.GetIDCode(Importer);

	#region CustomsOffices

	[ChildEditable(true)]
	public new OfficeCodeCollection CustomsOffices => (OfficeCodeCollection)base.CustomsOffices;

	protected override EuOfficeCodeCollection GetCustomsOffices() => new OfficeCodeCollection(this);

	protected override EU.Business.JobDeclarationCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new JobDeclarationCustomsOfficeRequirementHelper(this);

	public ZString GetExportCustomsOffice()
	{
		var office = GetCustomsOfficeFromList(EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport);
		return office.IsEmpty ? JE_CustomsOffice : office;
	}

	protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
	{
		var result = base.GetCusCodeDataTypesCore();
		result[CusCodeDataTypeList.Codes.OfficeCode] = typeof(OfficeCode);
		return result;
	}

	public ZString GetExitCustomsOffice()
	{
		var exitOffice = GetCustomsOfficeFromList(EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit);
		var exportOffice = GetCustomsOfficeFromList(EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport);
		return exitOffice.IsEmpty ? exportOffice.IsEmpty ? JE_CustomsOffice : exportOffice : exitOffice;
	}

	public ZString GetPresentationCustomsOffice()
	{
		var office = GetCustomsOfficeFromList(EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation);
		return office.IsEmpty ? JE_CustomsOffice : office;
	}

	public ZString GetCustomsOfficeFromList(string officeCode) => CustomsOffices.Cast<OfficeCode>().FirstOrDefault(x => x.CY_Code == officeCode)?.CY_Data ?? ZString.Empty;

	#endregion

		#region Implementation

	#region protected override

	protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;
	protected override bool IsCustomsLineAmendmentATotalReplacement => false;

	protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Spain;

	protected override bool HasSplitEntriesCore
	{
		get
		{
			if (!GetType().FullName.Contains("ES"))
			{
				ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
			}
			return base.HasSplitEntriesCore;
		}
	}

	#endregion

	#endregion

	public ZBool IsTransportModeInList(IEnumerable<ZString> modeList)
	{
		foreach (var mode in modeList)
		{
			if (JE_TransportMode == mode)
			{
				return true;
			}
		}
		return false;
	}

	public ZBool ShouldAddOrCopyHouseBillToTransportDocuments() => !JE_HouseBill.IsEmpty && IsTransportModeInList(ESConstants.TransportModeTypes.TransportModesForTransportDocuments);

	public void AddHouseBillTransportDocument()
	{
		CusSupportingInfo newTransportDocument = null;
		if (IsExport || AreAllEntriesT2LorT2C())
		{
			newTransportDocument = AdditionalInfos.AddNew();
			newTransportDocument.CSI_SubType = AdditionalDocList.Codes.TransportDocuments;
		}
		else if (IsImport && AreAllEntriesNoT2LNorT2C())
		{
			newTransportDocument = SupportingDocuments.AddNew();
			var bill = Bills.Cast<Bill>().FirstOrDefault(x => x.CU_BillType == BillTypeList.Codes.HouseBill);
			newTransportDocument.CSI_DateOfIssue = bill?.CU_IssueDate ?? ZDateTime.Empty;
		}

		if (newTransportDocument != null)
		{
			newTransportDocument.CSI_Code = DocumentHelper.GetDocumentByTransportType(JE_TransportMode);
			newTransportDocument.CSI_ReferenceNumber = JE_HouseBill;
		}
	}

	public ZBool AreAllEntriesT2LorT2C() => CustomsEntryInstructions.All<CusEntryInstruction>(x
													=> x.CEI_SubStyle == EntrySubStyleList.Codes.T2L
													|| x.CEI_SubStyle == EntrySubStyleList.Codes.T2C);

	public ZBool AreAllEntriesNoT2LNorT2C() => CustomsEntryInstructions.All<CusEntryInstruction>(x
													=> x.CEI_SubStyle != EntrySubStyleList.Codes.T2L
													&& x.CEI_SubStyle != EntrySubStyleList.Codes.T2C);

	public bool HasInvoiceWithEmptyIncoTermPlace => Invoices.Cast<JobComInvoiceHeader>().Any(x => x.JZ_IncoTermPlace.IsEmpty);

	protected override IValueSetStrategy GetValueSetStrategy() => new JobDeclarationValueSetStrategy(this);

	protected override ZString TradersOwnReferenceFullForBox7Core => !JE_OwnerRef.IsEmpty ? JE_OwnerRef : ZString.Empty;

	public bool HasAnyDiffT2CEntry => CustomsEntryInstructions.Any<CusEntryInstruction>(x => !x.IsT2C);

	public bool HasAnyEXSEntry => CustomsEntryInstructions.Any<CusEntryInstruction>(x => x.IsEXS);

	public bool HasAnyDiffT2CAndT2lEntry => CustomsEntryInstructions.Any<CusEntryInstruction>(x => !x.IsT2C && !x.IsT2L);

	public bool HasAnyDiffH2Entry => !CustomsEntryInstructions.Any<CusEntryInstruction>() || CustomsEntryInstructions.Any<CusEntryInstruction>(x => !x.IsH2);

	public bool HasAnyH2Entry => CustomsEntryInstructions.Any<CusEntryInstruction>(x => x.IsH2);

	public bool HasAnyDiffT2CAndT2lAndEXSEntry => !CustomsEntryInstructions.Any<CusEntryInstruction>() || CustomsEntryInstructions.Any<CusEntryInstruction>(x => !x.IsT2C && !x.IsT2L && !x.IsEXS);

	public bool HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry => CustomsEntryInstructions.Any<CusEntryInstruction>(x => !x.IsT2C && !x.IsT2L && !x.IsEXS && !x.IsSubStyleBOrC);

	public bool IsCustomOfficeCanaryIsland
	{
		get
		{
			var customOffice = JE_CustomsOffice;
			return customOffice.StartsWith(customOfficeCanaryIslandWith35) || customOffice.StartsWith(customOfficeCanaryIslandWith38) || customOffice.Equals(customOfficeCanaryIslandTest);
		}
	}

	const string customOfficeCanaryIslandWith35 = "ES0035";
	const string customOfficeCanaryIslandWith38 = "ES0038";
	const string customOfficeCanaryIslandTest = "ES009998";

	#region Generate ExitControl from Entries

	public int GenerateExitControlFromEntries(IEnumerable<ZString> selectedMRNs, CusExitControlHeader exitHeader)
	{
		var movementsAddedOrUpdated = 0;

		var exitDetails = exitHeader.CusExitDetails;

		foreach (var mrn in selectedMRNs)
		{
			var detail = exitDetails.Cast<CusExitDetail>().FirstOrDefault(x => x.CED_MovementReferenceNumber == mrn);
			if (detail == null)
			{
				detail = exitHeader.CusExitDetails.AddNew();
				detail.CED_MovementReferenceNumber = mrn;
			}
			detail.CED_CustomsOffice = GetExitCustomsOffice();
			detail.CED_ArrivalNotificationDate = exitHeader.CEH_ArrivalNotificationDate;
			detail.CED_ArrivalNotificationPlace = exitHeader.CEH_ArrivalNotificationPlace;

			movementsAddedOrUpdated++;
		}

		return movementsAddedOrUpdated;
	}

	public IEnumerable<CusEntryHeader> GetEntriesWithMRNInAcceptedExitDetails(IEnumerable<CusEntryHeader> acceptedEntries, CusExitDetailCollection exitDetails)
		=> acceptedEntries.Where(entry => exitDetails.Cast<CusExitDetail>().Any(detail => detail.CED_MovementReferenceNumber == entry.MovementReferenceNumber && detail.IsSentOrAccepted));

	public IEnumerable<CusEntryHeader> GetEntriesWithMRNInNotAcceptedExitDetails(IEnumerable<CusEntryHeader> acceptedEntries, CusExitDetailCollection exitDetails)
		=> acceptedEntries.Where(entry => exitDetails.Cast<CusExitDetail>().Any(detail => detail.CED_MovementReferenceNumber == entry.MovementReferenceNumber && !detail.IsSentOrAccepted));

	public IEnumerable<ZString> GetMRNFromEntries(IEnumerable<CusEntryHeader> acceptedEntries)
		=> acceptedEntries.Select(entry => entry.MovementReferenceNumber);

	public IEnumerable<Tuple<ZString, ZString>> GetMRNAndReferenceFromEntries(IEnumerable<CusEntryHeader> acceptedEntries)
	{
		var entryHeaderList = new List<Tuple<ZString, ZString>>();
		acceptedEntries.ForEach(entry => entryHeaderList.Add(new Tuple<ZString, ZString>(entry.MovementReferenceNumber, entry.CH_BGMReference)));
		return entryHeaderList;
	}

	public CusExitControlHeader GetOrCreateExitControlHeader()
	{
		var query = new ZQuery(CusExitControlHeaderSchema.CEH_ParentID, PK);
		var headerToReturn = Factory.LoadTop1<CusExitControlHeader>(query);
		if (headerToReturn == null)
		{
			var exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_Parent = this;
			exitHeader.CEH_ReferenceNumber = JE_DeclarationReference;
			headerToReturn = exitHeader;
		}

		FillExitHeaderData(headerToReturn);

		return headerToReturn;
	}

	void FillExitHeaderData(CusExitControlHeader exitHeader)
	{
		if (exitHeader.CEH_ArrivalNotificationDate.IsEmpty)
		{
			exitHeader.CEH_ArrivalNotificationDate = ZDateTime.Today;
		}

		exitHeader.CEH_OA_Agent = JE_OA_DeclarantAddress;

		if (Supplier != null)
		{
			exitHeader.CEH_OA_Carrier = Supplier.MainAddress.PK;
		}

		if (exitHeader.CEH_GS_NKCustomsAgent.IsEmpty && !GlbStaff.CurrentUser.GS_IsSystemAccount)
		{
			exitHeader.CEH_GS_NKCustomsAgent = GlbStaff.CurrentUser.GS_Code;
		}
	}

	#endregion

	public CusAuthorisationHeader LoadAEOCusGuaranteeHeaderFromReference(ZGuid holder_OH_PK)
	{
		CusAuthorisationHeader authorisation = null;
		if (!holder_OH_PK.IsEmpty)
		{
			var subQuery = new ZDBOnlyQuery(typeof(CusAuthorisationHeader));
			subQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_Type, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
			subQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_Type, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplificationsSecurityAndSafety);
			subQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_Type, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorSecurityAndSafety);

			var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Authorisation);
			query.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, holder_OH_PK);
			query.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDate.Today);

			var endDateQuery = new ZQuery(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDate.Today);
			endDateQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, null);
			query.AddToFilter(endDateQuery);

			query.AddToFilter(subQuery, JoinCondition.And);
			authorisation = Factory.LoadTop1<CusAuthorisationHeader>(query);
		}

		return authorisation;
	}

	protected override ZString DefaultDataGroupingCore => Core.Constants.CountryCodes.Spain;

	protected override void CleanUpNewDeclarationAfterCloneCore(BaseJobDeclaration result, CloneType cloneType)
	{
		base.CleanUpNewDeclarationAfterCloneCore(result, cloneType);
		result.JE_CustomsProfile = ZString.Empty;
	}

	public bool HasAnyCPCStartWithList(List<ZString> codes)
	{
		var hasAnyCPC = false;
		foreach (var code in codes)
		{
			hasAnyCPC = InvoiceLines.Any(x => ((JobComInvoiceLine)x).JI_FormattedProcedure.StartsWith(code));
			if (hasAnyCPC)
			{
				break;
			}
		}
		return hasAnyCPC;
	}

	public bool HasAnyCPCNotStartWithList(List<ZString> codes)
	{
		var cpcPrefixList = InvoiceLines.Select(x => ((JobComInvoiceLine)x).JI_FormattedProcedure.Left(2)).Distinct();
		return cpcPrefixList.Except(codes).Any();
	}

	protected override EU.Business.Declaration.EntryFeePaymentPartyUnderstander GetEntryFeePaymentPartyUnderstanderCore(EU.Business.Declaration.CusEntryHeader header) => new EntryFeePaymentPartyUnderstander(this);

	protected override ZBool IsSecurityAllowedCore() => base.IsSecurityAllowedCore() && !IsEntryStyleExportToSpecialTerritory;

	protected override bool EUD_AgreedPlaceCodeValidationSupportCore => false;

	protected override ZBool SupportEntryDeclarationMessageCore => true;

	protected override ZBool SupportValidateCustomsMessagingCore => true;

	protected override bool IsIntegrationWithAccountingSupported => true;

	protected override IProcessor GetEntryDeclarationMessageProcessorCore() => new AutoSendCustomsMessageProcessor(this);

	/// <summary>
	/// Temporary log for CS01787933 - NOAMARADT - AR/AP Details cannot be inserted/updated
	/// </summary>
	/// <param name="ex"></param>
	public void ReportSaveExceptionForSupplierOrImporter(Exception ex)
	{
		if (ex is not ZSaveException zex)
		{
			return;
		}

		if (!zex.Message.Contains("OrgHeader"))
		{
			return;
		}

		var dec = this;
		var supplierPk = dec.JE_OH_Supplier;
		var supplier = dec.Supplier;
		var importerPk = dec.JE_OH_Importer;
		var importer = dec.Importer;

		var newFactory = new BusinessObjectFactory();
		var supplierFromNewFactory = newFactory.Load<OrgHeader>(supplierPk);
		var importerFromNewFactory = newFactory.Load<OrgHeader>(importerPk);

		var errBuilder = new StringBuilder();

		AddOrgHeaderDetails(supplier, errBuilder, "Declaration.Supplier:");
		AddOrgHeaderDetails(supplierFromNewFactory, errBuilder, "JE_OH_Supplier from a new factory:");
		AddOrgHeaderDetails(importer, errBuilder, "Declaration.Importer:");
		AddOrgHeaderDetails(importerFromNewFactory, errBuilder, "JE_OH_Importer from a new factory:");

		var changedObjects = Factory.GetChanges().GetChangedObjects();
		foreach (var changedObject in changedObjects)
		{
			if (changedObject.SessionInstance is OrgHeader changedOrgHeader)
			{
				AddOrgHeaderDetails(changedOrgHeader, errBuilder, (NoResString)"Changed OrgHeader in the same factory:");
			}
		}

		var reg = OrganisationsDataRegistry.Instance;
		errBuilder.AppendLine((NoResString)"Current registry settings:")
			.Append('\t').Append(nameof(reg.OrgMatchThreshold)).Append(": ").AppendLine(reg.OrgMatchThreshold.Value)
			.Append('\t').Append(nameof(reg.OrgPatternMatchLimit)).Append(": ").AppendLine(reg.OrgPatternMatchLimit.Value.ToString())

			.Append('\t').Append(nameof(reg.UnmatchedOrganisationConfiguration)).AppendLine(":")
			.Append('\t').Append('\t').Append(nameof(reg.UnmatchedOrganisationConfiguration.IncludeMissingDefaults)).Append(": ").AppendLine(reg.UnmatchedOrganisationConfiguration.IncludeMissingDefaults.ToString())
			.Append('\t').Append('\t').Append(nameof(reg.UnmatchedOrganisationConfiguration.MaxLength)).Append(": ").AppendLine(reg.UnmatchedOrganisationConfiguration.MaxLength.ToString())
			.Append('\t').Append('\t').Append(nameof(reg.UnmatchedOrganisationConfiguration.RemoveNonDefaults)).Append(": ").AppendLine(reg.UnmatchedOrganisationConfiguration.RemoveNonDefaults.ToString())

			.Append('\t').Append(nameof(reg.OrgMatchUseDeduplication)).Append(": ").AppendLine(reg.OrgMatchUseDeduplication.Value.ToString())
			.Append('\t').Append(nameof(reg.UseUnmatchedOrganisationForMatching)).Append(": ").AppendLine(reg.UseUnmatchedOrganisationForMatching.Value.Code)
			.Append('\t').Append(nameof(reg.OrgAddressMinimumConfidence)).Append(": ").AppendLine(reg.OrgAddressMinimumConfidence.Value.ToString())
			.Append('\t').Append(nameof(reg.UXMLOrganisationMinimumConfidence)).Append(": ").AppendLine(reg.UXMLOrganisationMinimumConfidence.Value.ToString());

		ErrorReporter.ReportDeveloperExceptionOnce(errBuilder.ToString(), ex);

		void AddOrgHeaderDetails(OrgHeader orgHeader, StringBuilder sb, string msg)
		{
			sb.AppendLine(msg);

			if (orgHeader == null)
			{
				sb.Append('\t').AppendLine((NoResString)"OrgHeader is null");
				return;
			}

			using (((IBusinessObjectInternals)orgHeader).SuppressReportRowDeletedError())
			{
				sb.Append('\t').Append("IsInDatabase: ").AppendLine(orgHeader.IsInDatabase.ToYesNoString())
					.Append('\t').Append("IsDeleted: ").AppendLine(orgHeader.IsDeleted.ToYesNoString())
					.Append('\t').AppendLine((NoResString)"All Field Values:");

				var allProps = GetProperties(typeof(OrgHeader));
				foreach (var schemaColumn in OrgHeaderSchema.All)
				{
					if (allProps[schemaColumn.Name] == null)
					{
						continue;
					}

					sb.Append('\t').Append('\t').Append(schemaColumn.Name).Append(": ").AppendLine(orgHeader[schemaColumn.Name].ToString());
				}

				if (orgHeader.IsDeleted)
				{
					sb.Append('\t').AppendLine(orgHeader.DeletedStack);
				}
			}
		}
	}
}
