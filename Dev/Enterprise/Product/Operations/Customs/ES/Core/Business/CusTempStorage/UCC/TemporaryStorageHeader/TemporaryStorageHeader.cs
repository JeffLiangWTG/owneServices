using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.ManifestBase;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using Constants = CargoWise.EventReference.Constants;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class TemporaryStorageHeader : EU.Business.CusTempStorage.TemporaryStorageHeader, Integration.Customs.ES.ITemporaryStorageHeader, IESMessageInfoProvider, IESResponseBOMessageStatus, IDocumentSupportable
{
	public TemporaryStorageHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	const string G5DCode = "G5D";

	#region GenAddOn
	public static class GenAddOnColumnConstants
	{
		public const string IsSimplifiedColumnName = "G5_IsSimplfied";
		public const string TrainingEntryColumnName = "TrainingEntry";
		public const string MovementOfContainersOnlyColumnName = "G5_MovementOfContainersOnly";
		public const string StoVersionColumnName = "StoVersion";
		public const string UnionGoodsColumnName = "UnionGoods";
	}
	#endregion

	protected override ZBool IsSentCore => base.IsSentCore || CustomsStatus == EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;

	protected override bool CusAuthorizationUsageProperty_ReadOnly => IsSent || AuthorizationType.IsEmpty;

	protected override bool IsMrnReadOnly => !((IsMessageTypeG5V1Reception || IsMessageTypeTSM) && CustomsStatus.IsEmpty);

	[ReadOnlyMember(nameof(AMA_MessageType_Readonly))]
	public override ZString AMA_MessageType
	{
		get => base.AMA_MessageType;
		set
		{
			base.AMA_MessageType = value;
			if (!IsMessageTypeTSM && UnionGoods)
			{
				UnionGoods = false;
			}
		}
	}

	bool AMA_MessageType_Readonly => IsSent || (!MRN.IsEmpty && IsMessageTypeG5V1Reception);

	[MaxLength(2)]
	public ZString StoVersion
	{
		get => this.GetSystemDefinedValue<ZString>(GenAddOnColumnConstants.StoVersionColumnName);
		set
		{
			var oldValue = StoVersion;
			if (value != oldValue)
			{
				CheckMaximumLength(StoVersionInfo, value);
				this.SetSystemDefinedValue(GenAddOnColumnConstants.StoVersionColumnName, value);
				StoVersionInfo.RefreshBinding(oldValue);
			}
		}
	}

	[ReadOnly(true)]
	[MaxLength(CusEntryNumber.Schema.CE_EntryStatusMaxLength)]
	[ResourceStringData("989A5381-DBE1-4766-9583-A66A17CE277F", Caption = "Circuit")]
	public ZString EntryStatus
	{
		get
		{
			var code = MRNEntryStatus?.CE_EntryStatus ?? ZString.Empty;
			var description = (ZString)CircuitCodeList.GetDescriptionFromCode(code);
			return !description.IsEmpty ? description : code;
		}
		set
		{
			if (EntryStatus != value)
			{
				if (MRNEntryStatus == null)
				{
					mrnEntryStatus = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, AMA_RN_NKCountry);
				}
				MRNEntryStatus.CE_EntryStatus = value;
				RegisterEditableChildObject(mrnEntryStatus);
				EntryStatusInfo.RefreshBinding();
			}
		}
	}
	CodeDescriptionPairList CircuitCodeList => Factory.GetCachedValue<CircuitCodeList>();

	public ZPropertyInfo EntryStatusInfo => GetZPropertyInfo(nameof(EntryStatus));

	CusEntryNumber MRNEntryStatus
	{
		get
		{
			if (mrnEntryStatus == null || mrnEntryStatus.IsDeleted)
			{
				mrnEntryStatus = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, AMA_RN_NKCountry);
			}
			return mrnEntryStatus;
		}
	}
	CusEntryNumber mrnEntryStatus;

	[ResourceStringData("DDD7A9D4-ABF0-40C6-98CF-509243F6D218", Caption = "Entry Date")]
	public ZDateTime EntryDate
	{
		get => LAMEEntryDate?.CE_IssueDate ?? ZDateTime.Empty;
		set
		{
			if (EntryDate != value)
			{
				if (LAMEEntryDate == null)
				{
					lameEntryDate = LoadOrCreateEntryNumCUS(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, AMA_RN_NKCountry, LAMEEntryNumber?.CE_EntryNum ?? ZString.Empty, value);
				}
				LAMEEntryDate.CE_IssueDate = value;
				RegisterEditableChildObject(LAMEEntryDate);
				EntryDateInfo.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo EntryDateInfo => GetZPropertyInfo(nameof(EntryDate));

	CusEntryNumber LAMEEntryDate
	{
		get
		{
			if (lameEntryDate == null || lameEntryDate.IsDeleted)
			{
				lameEntryDate = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, AMA_RN_NKCountry);
			}
			return lameEntryDate;
		}
	}
	CusEntryNumber lameEntryDate;

	[ResourceStringData("0C54A658-9335-4BE8-95B9-92E360995948", Caption = "Entry Number")]
	[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
	[ReadOnly(true)]
	public ZString EntryNumber
	{
		get => LAMEEntryNumber?.CE_EntryNum ?? ZString.Empty;
		set
		{
			if (EntryNumber != value)
			{
				if (LAMEEntryNumber == null)
				{
					lameEntryNumber = LoadOrCreateEntryNumCUS(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, AMA_RN_NKCountry, value, LAMEEntryDate?.CE_IssueDate ?? ZDateTime.Empty);
				}
				LAMEEntryNumber.CE_EntryNum = value;
				RegisterEditableChildObject(LAMEEntryNumber);
				EntryNumberInfo.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo EntryNumberInfo => GetZPropertyInfo(nameof(EntryNumber));

	CusEntryNumber LAMEEntryNumber
	{
		get
		{
			if (lameEntryNumber == null || lameEntryNumber.IsDeleted)
			{
				lameEntryNumber = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, AMA_RN_NKCountry);
			}
			return lameEntryNumber;
		}
	}
	CusEntryNumber lameEntryNumber;

	CusEntryNumber LoadOrCreateEntryNumCUS(BusinessObject header, ZString type, ZString contry, ZString number, ZDateTime date)
	{
		var entryNum = CusEntryNumber.LoadOrCreate(header, type, contry);
		entryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		entryNum.CE_EntryNum = number;
		entryNum.CE_IssueDate = date;

		return entryNum;
	}

	[ReadOnlyMember(nameof(IsNotReadOnlyMemberWhenMessageTypeTSMAndCustomsStatusEmpty))]
	[ResourceStringData("29F6DB3D-653D-4194-B8A8-3083FBE92071", Caption = "Acceptance Date")]
	public ZDateTime AcceptanceDate
	{
		get => MRNEntryIssueDate?.CE_IssueDate ?? ZDateTime.Empty;
		set
		{
			if (AcceptanceDate != value)
			{
				if (MRNEntryIssueDate == null)
				{
					mrnEntryissueDate = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, AMA_RN_NKCountry);
				}
				MRNEntryIssueDate.CE_IssueDate = value;
				RegisterEditableChildObject(MRNEntryIssueDate);
				AcceptanceDateInfo.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo AcceptanceDateInfo => GetZPropertyInfo(nameof(AcceptanceDate));

	CusEntryNumber MRNEntryIssueDate
	{
		get
		{
			if (mrnEntryissueDate == null || mrnEntryissueDate.IsDeleted)
			{
				mrnEntryissueDate = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, AMA_RN_NKCountry);
			}
			return mrnEntryissueDate;
		}
	}
	CusEntryNumber mrnEntryissueDate;

	[ReadOnly(true)]
	[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
	[ResourceStringData("3AAE0B37-B182-40D5-BA76-652D57DED73B", Caption = "Clearance Number")]
	public ZString ClearanceNumber
	{
		get => CLREntryNumber?.CE_EntryNum ?? ZString.Empty;
		set
		{
			if (ClearanceNumber != value)
			{
				if (CLREntryNumber == null)
				{
					clrEntryNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.Spain.ClearanceCSV, AMA_RN_NKCountry);
				}
				CLREntryNumber.CE_EntryNum = value;
				RegisterEditableChildObject(clrEntryNumber);
				ClearanceNumberInfo.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo ClearanceNumberInfo => GetZPropertyInfo(nameof(ClearanceNumber));

	[ReadOnly(true)]
	public ZDateTime ClearanceDate
	{
		get => CLREntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
		set
		{
			if (ClearanceDate != value)
			{
				if (CLREntryNumber == null)
				{
					clrEntryNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.Spain.ClearanceCSV, AMA_RN_NKCountry);
				}
				CLREntryNumber.CE_IssueDate = value;
				RegisterEditableChildObject(clrEntryNumber);
				ClearanceDateInfo.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo ClearanceDateInfo => GetZPropertyInfo(nameof(ClearanceDate));

	CusEntryNumber CLREntryNumber
	{
		get
		{
			if (clrEntryNumber == null || clrEntryNumber.IsDeleted)
			{
				clrEntryNumber = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.Spain.ClearanceCSV, AMA_RN_NKCountry);
			}
			return clrEntryNumber;
		}
	}
	CusEntryNumber clrEntryNumber;

	[ReadOnlyMember(nameof(IsNotReadOnlyMemberWhenMessageTypeTSMAndCustomsStatusEmptyAndIsNotUnionGoods))]
	[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
	[ResourceStringData("CD293F2E-2455-4C35-9C83-05867AD5DCE7", Caption = "DSDT MRN")]
	public ZString DsdtMrnNumber
	{
		get => DsdtMrn?.CE_EntryNum ?? ZString.Empty;
		set
		{
			if (DsdtMrnNumber != value)
			{
				if (DsdtMrn == null)
				{
					dsdtMrn = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.Spain.SummaryEntryNumber, AMA_RN_NKCountry);
				}
				DsdtMrn.CE_EntryNum = value;
				RegisterEditableChildObject(dsdtMrn);
				DsdtMrnNumberInfo.RefreshBinding();
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateDsdtMrnNumber();
			}
		}
	}

	public ZPropertyInfo DsdtMrnNumberInfo => GetZPropertyInfo(nameof(DsdtMrnNumber));

	CusEntryNumber DsdtMrn
	{
		get
		{
			if (dsdtMrn == null || dsdtMrn.IsDeleted)
			{
				dsdtMrn = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.Spain.SummaryEntryNumber, AMA_RN_NKCountry);
			}
			return dsdtMrn;
		}
	}
	CusEntryNumber dsdtMrn;

	[ReadOnly(true)]
	[ResourceStringData("1A4E6427-9F7C-4B16-8F14-C14F002B7729", Caption = "DSDT (SD Format)")]
	public ZString DsdtMrnNumberSdFormat
	{
		get => DocumentHelper.GetDsdtMRNNumberFormat(DsdtMrnNumber);
	}

	public ZString DsdtSummaryDeclarationUrl
	{
		get
		{
			return DsdtMrnNumber switch
			{
				var x when x.Length == 11 => GetUrlFromDstd(),
				var x when x.Length > 11 => GetUrlFromMrn(),
				_ => ZString.Empty
			};

			ZString GetUrlFromDstd()
			{
				var recinto = DsdtMrnNumber.Substring(0, 4);
				var anio = DsdtMrnNumber.Substring(4, 1);
				var numero = DsdtMrnNumber.Substring(5);

				return ESCustomsDataRegistry.Instance.SummaryDeclarationStatusQueryURL.Value.Replace(CustomsWebsiteUrlCodes.RecintoInRegistryUrl, recinto).Replace(CustomsWebsiteUrlCodes.AnioInRegistryUrl, anio).Replace(CustomsWebsiteUrlCodes.NumeroInRegistryUrl, numero).Replace(CustomsWebsiteUrlCodes.MRNinRegistryUrl, ZString.Empty);
			}

			ZString GetUrlFromMrn() =>
			ESCustomsDataRegistry.Instance.SummaryDeclarationStatusQueryURL.Value.Replace(CustomsWebsiteUrlCodes.RecintoInRegistryUrl, ZString.Empty).Replace(CustomsWebsiteUrlCodes.AnioInRegistryUrl, ZString.Empty).Replace(CustomsWebsiteUrlCodes.NumeroInRegistryUrl, ZString.Empty).Replace(CustomsWebsiteUrlCodes.MRNinRegistryUrl, DsdtMrnNumber);
		}
	}

	public ZPropertyInfo StoVersionInfo => GetZPropertyInfo(nameof(StoVersion));

	[ReadOnlyMember(nameof(IsSent))]
	public ZBool TrainingEntry
	{
		get => this.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.TrainingEntryColumnName);
		set
		{
			var oldValue = TrainingEntry;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.TrainingEntryColumnName, value);
				TrainingEntryInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo TrainingEntryInfo => GetZPropertyInfo(nameof(TrainingEntry));

	void SetDefaultTrainingEntry()
	{
		TrainingEntry = ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem();
	}

	[ResourceStringData("81C1561B-4E86-4FB2-AA83-85F5E9922BDB", Caption = "Is Simplified?", MediumCaption = "Simplified?", ShortCaption = "Simp.?", FullDescription = "If ticked a G5 Simplified will be sent otherwise a G5 General will be sent")]
	public ZBool IsSimplified
	{
		get => this.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.IsSimplifiedColumnName);
		set
		{
			var oldValue = IsSimplified;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.IsSimplifiedColumnName, value);
				IsSimplifiedInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo IsSimplifiedInfo => GetZPropertyInfo(nameof(IsSimplified));

	[ReadOnlyMember(nameof(AMA_Calc_HasHouseConsignment))]
	[ResourceStringData("20BBF913-461E-40AB-95E7-D6B8A776FBD4", Caption = "Movement of Containers Only?", MediumCaption = "Mov. Of Containers?", ShortCaption = "Mov. Cont.?", FullDescription = "If ticked, declaration will include only a Master Consignment with the containers to move")]
	public ZBool MovementOfContainersOnly
	{
		get => this.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.MovementOfContainersOnlyColumnName);
		set
		{
			var oldValue = MovementOfContainersOnly;
			if(oldValue != value)
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.MovementOfContainersOnlyColumnName, value);
				if (value)
				{
					Bills.ForEach(bill => bill.ABL_BolType = TemporaryStorageBill.ChildMocCode);
				}
				else
				{
					Bills.ForEach(bill => bill.ABL_BolType = TemporaryStorageBill.ChildBolCode);
				}
				MovementOfContainersOnlyInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo MovementOfContainersOnlyInfo => GetZPropertyInfo(nameof(MovementOfContainersOnly));

	[ResourceStringData("15559314-4ADF-41D4-B0FB-4FA6CC542F4E", Caption = "Has House Consignment?", MediumCaption = "Has House Consig.?", ShortCaption = "Has HC?", FullDescription = "If ticked, declaration will include House Consignments and if not, only Master Consignment")]
	public override ZBool AMA_Calc_HasHouseConsignment
	{
		get => base.AMA_Calc_HasHouseConsignment;
		set
		{
			if(value)
			{
				MovementOfContainersOnly = false;
			}
			base.AMA_Calc_HasHouseConsignment = value;
		}
	}

	[ReadOnlyMember(nameof(IsSent))]
	[MaxLength(10)]
	[ResourceStringData("65DCC3AA-AA9D-4385-9841-58D4D99E6CDC", Caption = "Destination Customs Office", MediumCaption = "Destination Customs Office", ShortCaption = "Dest. Customs Office", FullDescription = "Customs Office of Destination")]
	[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.DestinationCustomsOfficeCodeList))]
	public ZString DestinationCustomsOffice
	{
		get => DestinationCustomsOfficeCode.CY_Data;
		set
		{
			var oldValue = DestinationCustomsOffice;
			CheckMaximumLength(DestinationCustomsOfficeInfo, value);
			DestinationCustomsOfficeCode.CY_Data = value;
			RegisterEditableChildObject(DestinationCustomsOfficeCode);
			DestinationCustomsOfficeInfo.RefreshBinding(oldValue);
			if (!IsValidationSuspended)
			{
				Validation.ValidateDestinationCustomsOffice();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.CusTempStorageRegPremisesList))]
	[MaxLength(CusGoodsLocationAddress.Schema.E2_GovRegNumMaxLength)]
	[ResourceStringData("940E5851-F7EC-43ED-945F-9DFAE8CD07A4", Caption = "Goods Location", MediumCaption = "Goods Location", ShortCaption = "Location")]
	public ZString ManualDestinationCustomsOffice
	{
		get
		{
			return DestinationGoodsLocation.Address.E2_GovRegNum;
		}
		set
		{
			if (DestinationGoodsLocation.Address.E2_GovRegNum != value)
			{
				var oldValue = DestinationGoodsLocation.Address.E2_GovRegNum;
				CheckMaximumLength(ManualDestinationCustomsOfficeInfo, value);
				DestinationGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				DestinationGoodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
				DestinationGoodsLocation.Address.E2_GovRegNum = value;
				ManualDestinationCustomsOfficeInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo ManualDestinationCustomsOfficeInfo => GetZPropertyInfo(nameof(ManualDestinationCustomsOffice));

	public override bool IsPropertyReadOnlyAfterSent(string propertyName)
	{
		switch (propertyName)
		{
			case nameof(AMA_OA_Declarant):
			case nameof(AMA_OA_Representative):
			case nameof(AMA_TransportMeans):
			case nameof(AMA_TransportMode):
			case nameof(TransportType):
			case nameof(ArrivalTransportMeansCode):
			case nameof(AuthorizationType):
				return true;
		}

		return false;
	}

	public ZPropertyInfo DestinationCustomsOfficeInfo => GetZPropertyInfo(nameof(DestinationCustomsOffice));

	public TemporaryStorageOfficeCode DestinationCustomsOfficeCode => Factory.GetValue(ref destinationCustomsOffice,
		() => EU.Business.TemporaryStorageOfficeCode.LoadOrCreate<EU.Business.TemporaryStorageOfficeCode>(this, G5DCode));
	CachedProperty<TemporaryStorageOfficeCode> destinationCustomsOffice;

	public CusGoodsLocation DestinationGoodsLocation
	{
		get
		{
			if (destinationGoodsLocation == null)
			{
				destinationGoodsLocation = Customs.Business.CusGoodsLocation.LoadOrCreate<CusGoodsLocation>(this, G5DCode);
				RegisterEditableChildObject(destinationGoodsLocation);
				ToggleValueChangedHandlers(destinationGoodsLocation);
			}
			return destinationGoodsLocation;
		}
	}
	CusGoodsLocation destinationGoodsLocation;

	void ToggleValueChangedHandlers(CusGoodsLocation goodsLocation)
	{
		foreach (ZPropertyInfo item in GetPropertyInfoListToMarkAsNeedingValidation(goodsLocation))
		{
			item.ValueChanged += MarkAsNeedingValidation;
		}
	}

	[ResourceStringData("DDC99779-AE9D-4699-A04C-6E0C790D9567", Caption = "Destination Goods Location", MediumCaption = "Dest. Goods Location", ShortCaption = "Dest. Location", FullDescription = "Goods Location of Destination")]
	public ZString DestinationGoodsLocationDescription
	{
		get
		{
			if (destinationGoodsLocation == null)
			{
				destinationGoodsLocation = Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(this, G5DCode);
				if (destinationGoodsLocation != null)
				{
					RegisterEditableChildObject(destinationGoodsLocation);
				}
			}
			var displayText = destinationGoodsLocation?.DisplayText ?? ZString.Empty;
			if (displayText != destinationGoodsLocationDescriptionSave)
			{
				if (!displayText.IsEmpty)
				{
					PopulateGuaranteeWhenLocationIsSelectedWithTemporaryStorage(destinationGoodsLocation.Address.AuthorisationNumber);
				}
				destinationGoodsLocationDescriptionSave = displayText;
			}
			return displayText;
		}
	}

	ZString destinationGoodsLocationDescriptionSave;

	public void PopulateGuaranteeWhenLocationIsSelectedWithTemporaryStorage(ZString goodsLocation)
	{
		if (!goodsLocation.IsEmpty && Guarantee.PW_BondNumber.IsEmpty
			&& AMA_MessageType != G5MessageTypeCodeList.Codes.LameManualEntry
			&& destinationGoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber
			&& destinationGoodsLocation.CGL_Type == CusGoodsLocationTypeList.Codes.AuthorizedPlace
			&& !destinationGoodsLocation.Address.AuthorisationNumber.IsEmpty
			&& EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(CountryCode))
		{
			var guaranteeNumber = CusGuaranteeHeaderHelper.PopulateGuaranteeFromLocation(Factory, goodsLocation);
			if (!guaranteeNumber.IsEmpty)
			{
				Guarantee.PW_BondNumber = guaranteeNumber;
			}
		}
	}

	public ZPropertyInfo DestinationGoodsLocationDescriptionInfo => GetZPropertyInfo(nameof(DestinationGoodsLocationDescription));

	[ReadOnlyMember(nameof(IsSent))]
	[ResourceStringData("6382814B-88B5-45D0-BB3E-C83D25BC9B16", Caption = "Departure Customs Office", MediumCaption = "Departure Customs Office", ShortCaption = "Dept. Customs Office", FullDescription = "Customs Office of Departure")]
	public override ZString AMA_CustomsOffice { get => base.AMA_CustomsOffice; set => base.AMA_CustomsOffice = value; }

	[ResourceStringData("A18C9E0A-BFCA-4997-B386-E73F24296B91", Caption = "Departure Goods Location", MediumCaption = "Departure Goods Location", ShortCaption = "Dept. Goods Location", FullDescription = "Departure Location of Goods")]
	public override ZString GoodsLocationDescription { get => base.GoodsLocationDescription; }

	[ResourceStringData("4F6EDE51-2309-4953-BE0B-74CC730D395D", Caption = "Certificate", MediumCaption = "Certif.", ShortCaption = "Cert.", FullDescription = "The certificate selected will be used to sign and communicate with Customs to declare all entries in this Job")]
	[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.CertificateNames))]
	public override ZString AMA_CustomsProfile { get => base.AMA_CustomsProfile; set => base.AMA_CustomsProfile = value; }

	public override ZString AMA_GS_NKCustomsAgent
	{
		get => base.AMA_GS_NKCustomsAgent;
		set
		{
			var oldValue = base.AMA_GS_NKCustomsAgent;
			base.AMA_GS_NKCustomsAgent = value;

			if (!IsCopying && oldValue != value)
			{
				SetDefaultJE_CustomsProfile();
			}
		}
	}

	void SetDefaultJE_CustomsProfile()
	{
		var certificateNamesList = Lookups.CertificateNames;
		var customsProfile = AMA_CustomsProfile;
		if (customsProfile.IsEmpty || !certificateNamesList.GetAllCodesZString().Contains(customsProfile))
		{
			if (certificateNamesList.Count == 1)
			{
				AMA_CustomsProfile = certificateNamesList[0].Code;
			}
			else
			{
				AMA_CustomsProfile = ZString.Empty;
			}
		}
	}

	[ReadOnlyMember(nameof(IsNotReadOnlyMemberWhenMessageTypeTSMAndCustomsStatusEmpty))]
	[ResourceStringData("91CC895F-2EF5-43EF-8A88-B233CF789D56", Caption = "Union Goods")]
	public ZBool UnionGoods
	{
		get => this.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.UnionGoodsColumnName);
		set
		{
			var oldValue = UnionGoods;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.UnionGoodsColumnName, value);
				UnionGoodsInfo.RefreshBinding(oldValue);
				SetEmptyValuesAndReadOnlyForUnionGoods();
			}
		}
	}

	public ZPropertyInfo UnionGoodsInfo => GetZPropertyInfo(nameof(UnionGoods));

	void SetEmptyValuesAndReadOnlyForUnionGoods()
	{
		if (IsMessageTypeTSMAndIsUnionGoods)
		{
			Guarantee.PW_BondNumber = ZString.Empty;
			Guarantee.PW_Override = false;
			Guarantee.PW_BondAmount = 0;
			DsdtMrnNumber = ZString.Empty;
			Bills.ForEach(bill => bill.PackedItems.Cast<TemporaryStoragePackedItem>().ForEach(item => item.DutiesAndTaxes.RemoveAndDeleteAll()));
		}
	}

	public string GetUrlToLaunch() => !IsDeleted ? new UrlDecider(this).GetUrl() : ZString.Empty;

	IEnumerable<TemporaryStoragePack> GetPacksInLineItemsNotMissing() => Bills.SelectMany(bill => bill.Packs
											.Where(pack => pack.PackedItems.Cast<AsycudaPackPackedItemPivot>()
											.Any(pivot => pivot.PackedItem.Cast<TemporaryStoragePackedItem>().Any(line => !line.IsMissing))));

	public (ZBool, CusTempStorageRegHeader) CreateTemporaryStorageData()
	{
		var result = GetPacksInLineItemsNotMissing().Any();

		var regHeader = (CusTempStorageRegHeader)null;

		if (result)
		{
			const string LAMReferencePlaceholder = "LAMREFPLACEHOLDER";

			regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_AppCode = CusTempStorageRegHeader.ESAppCode;
			regHeader.SRH_Reference = IsMessageTypeLAM
											? EntryNumber.IsEmpty ? LAMReferencePlaceholder : EntryNumber
											: IsMessageTypeTSM
												? IsMessageTypeTSMAndIsUnionGoods ? MRN : DsdtMrnNumber
												: DsdtMrnNumberSdFormat;
			var issueDate = IsMessageTypeLAM ? EntryDate : AcceptanceDate;
			regHeader.SRH_ArrivalDate = issueDate.Date;
			regHeader.SRH_PresentationDate = issueDate;
			regHeader.SRH_PreviousReferenceType = IsMessageTypeLAM
													? PreviousReferenceTypeCodeList.Codes.LameEntries
													: IsMessageTypeTSM
														? PreviousReferenceTypeCodeList.Codes.ManualEntries
														: PreviousReferenceTypeCodeList.Codes.G5Reception;

			regHeader.SRH_PreviousReference = IsMessageTypeLAM || IsMessageTypeTSMAndIsUnionGoods ? ZString.Empty : MRN;
			regHeader.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
			if (IsMessageTypeTSMAndIsUnionGoods || (IsMessageTypeLAM && !EntryNumber.IsEmpty))
			{
				regHeader.SRH_InternalReference = regHeader.SRH_Reference;
			}

			var destinationGoodsLocation = DestinationGoodsLocation?.Address.AuthorisationNumber ?? ZString.Empty;
			TemporaryStorageHelper.SetPremisesAndGuaranteeIntoRegHeader(Factory, regHeader, destinationGoodsLocation, Guarantee, IsMessageTypeLAM);

			var bill = Bills.FirstOrDefault();
			var traderId = OrgHeaderExtension.GetIDCode((IsMessageTypeLAM ? bill?.Shipper : bill?.Consignee)?.Header);
			var branchHomePort = Branch.HomePort;

			issueDate = !ClearanceDate.IsEmpty && IsMessageTypeG5V1Reception ? ClearanceDate : issueDate;
			var issueDateTimeOffset = issueDate.ToDateTimeOffset(branchHomePort);

			var lineItemsNotMissingAndHasPacks = Bills.SelectMany(bill => bill.PackedItems.Cast<TemporaryStoragePackedItem>().Where(line => !line.IsMissing && line.PackagesPivot.Count > 0));
			var goodsItemsWithLiabilities = GetLiabilitiesForGoodsItemsFromGuarantee(lineItemsNotMissingAndHasPacks);
			var allPackagePivots = lineItemsNotMissingAndHasPacks.SelectMany(i => i.PackagesPivot.Cast<AsycudaPackPackedItemPivot>());

			var regLineSeq = (ZInt)1;
			var regLineAndPackTupleList = new List<Tuple<ZGuid, ZGuid>>();

			foreach (var (item, liability) in goodsItemsWithLiabilities)
			{
				var regLineItem = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>();
				regLineItem.SRI_Tariff = item.API_Tariff;
				regLineItem.SRI_CusC4Number = item.API_ChemicalSubstanceCode;
				regLineItem.SRI_GoodsDescription = item.API_GoodsDescription.Left(EU.TemporaryStorage.Business.CusTempStorageRegLineItem.Schema.SRI_GoodsDescriptionMaxLength);

				var packagesPivot = item.PackagesPivot.Cast<AsycudaPackPackedItemPivot>();

				var packTotalQtyInItem = (ZInt)packagesPivot.Sum(p => PackageHelper.PackTypeIsBulk(p.Pack.APA_PackUQ, Factory)
																? 1 : p.Pack.APA_PackQty);
				var itemNumber = item.API_LineNo;

				var itemGrossWeight = item.API_GrossWeight;

				var transactionPKsForItem = new List<ZGuid>();
				var transactionsTotalBondAmount = ZDecimal.Zero;
				var transactionsAndPivotsTotalGrossWeight = ZDecimal.Zero;

				foreach (var packPivot in packagesPivot)
				{
					var package = packPivot.Pack;

					var packType = package.APA_PackUQ;
					var packQty = package.APA_PackQty;
					var packMarks = package.APA_MarksAndNumbers;
					var packPK = package.PK;
					var pivotAndTransactionGrossWeightCalculated = (ZDecimal)(itemGrossWeight / (packTotalQtyInItem.IsEmpty ? 1 : packTotalQtyInItem) * (packQty.IsEmpty ? 1 : packQty));
					var internalReferenceType = IsMessageTypeLAM
													? CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry
													: IsMessageTypeTSM
														? CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry
														: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
					var shouldCheckExistingPackageInOtherRegLine = allPackagePivots.Count(p => p.APP_APA_Pack == packPivot.APP_APA_Pack) > 1;

					(itemNumber, regLineSeq, var transactionPK, var transactionAndPivotGrossWeight, var transactionBondAmount, var regLinePK) =
							TemporaryStorageHelper.CreateRegisterDataForPackage(Factory,
																				packType,
																				packQty,
																				packMarks,
																				pivotAndTransactionGrossWeightCalculated,
																				itemGrossWeight,
																				liability,
																				regHeader.PK,
																				regHeader.SRH_ArrivalDate,
																				traderId,
																				regLineItem.PK,
																				itemNumber,
																				regLineSeq,
																				AMA_JobReference,
																				issueDateTimeOffset,
																				issueDateTimeOffset,
																				internalReferenceType,
																				packPK,
																				regLineAndPackTupleList,
																				IsMessageTypeTSMAndIsUnionGoods,
																				shouldCheckExistingPackageInOtherRegLine);

					transactionPKsForItem.Add(transactionPK);
					transactionsTotalBondAmount += transactionBondAmount;
					transactionsAndPivotsTotalGrossWeight += transactionAndPivotGrossWeight;
					regLineAndPackTupleList.Add(new Tuple<ZGuid, ZGuid>(regLinePK, packPK));
				}

				var transactionsForItem = TemporaryStorageHelper.GetLineTransactionsFromListOfPks(Factory, transactionPKsForItem);
				if (!IsMessageTypeTSMAndIsUnionGoods)
				{
					TemporaryStorageHelper.CorrectBondAmountInLineTransactions(transactionsForItem, liability, transactionsTotalBondAmount);
				}
				TemporaryStorageHelper.CorrectGrossWeightInLineTransactions(transactionsForItem, itemGrossWeight, transactionsAndPivotsTotalGrossWeight);
				TemporaryStorageHelper.CorrectGrossWeightInItemPivots(Factory, regLineItem.PK, itemGrossWeight, transactionsAndPivotsTotalGrossWeight);

				regLineItem.SRI_GoodsItemNumber = itemNumber;
			}

			CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;
		}

		return (result, regHeader);
	}

	List<(TemporaryStoragePackedItem item, ZDecimal liability)> GetLiabilitiesForGoodsItemsFromGuarantee(IEnumerable<TemporaryStoragePackedItem> items)
	{
		const int decimalsForCalculatedLiabilityAmount = 2;
		var result = new List<(TemporaryStoragePackedItem item, ZDecimal liability)>();

		var guarantee = Guarantee;
		if (!IsMessageTypeLAM && guarantee != null && guarantee.PW_Override)
		{
			var goodsItemsTotalGrossWeight = ZDecimal.Zero;

			items.ForEach(g => goodsItemsTotalGrossWeight += g.API_GrossWeight);

			var guaranteeBondAmount = guarantee.PW_BondAmount;
			var totalLiabilityInItems = ZDecimal.Zero;

			items.ForEach(g =>
			{
				var itemLiability = ((ZDecimal)(guaranteeBondAmount / goodsItemsTotalGrossWeight * g.API_GrossWeight)).Truncate(decimalsForCalculatedLiabilityAmount);
				result.Add((g, itemLiability));
				totalLiabilityInItems += itemLiability;
			});

			return CorrectLiabilityAmountForGoodsItems(result, guaranteeBondAmount, totalLiabilityInItems);
		}
		else
		{
			items.ForEach(g => result.Add((g, IsMessageTypeLAM ? ZDecimal.Zero : g.LiabilityAmount)));
			return result;
		}
	}

	List<(TemporaryStoragePackedItem item, ZDecimal liability)> CorrectLiabilityAmountForGoodsItems(List<(TemporaryStoragePackedItem item, ZDecimal liability)> itemsWithLiability, ZDecimal guaranteeBondAmount, ZDecimal itemsTotalLiability)
	{
		var diff_amount = itemsTotalLiability - guaranteeBondAmount;
		var correctionAmount = (decimal)0.01;
		if (diff_amount != 0)
		{
			var result = new List<(TemporaryStoragePackedItem item, ZDecimal liability)>();

			itemsWithLiability.ForEach(x =>
			{
				var liability = x.liability;

				(diff_amount, liability) = TemporaryStorageHelper.CorrectDecimalValue(diff_amount, correctionAmount, liability);

				result.Add((x.item, liability));
			});

			return result;
		}
		else
		{
			return itemsWithLiability;
		}
	}

	protected override AsycudaManifestHeaderLookups GetNewLookups() => new TemporaryStorageHeaderLookups(this);
	public new TemporaryStorageHeaderLookups Lookups => (TemporaryStorageHeaderLookups)base.Lookups;

	protected override AsycudaManifestHeaderValidation GetNewValidation() => new TemporaryStorageHeaderValidation(this);

	public new TemporaryStorageHeaderValidation Validation => (TemporaryStorageHeaderValidation)base.Validation;

	[ChildEditable]
	public new ITemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader> Bills
		=> (ITemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>)base.Bills;

	protected override ITemporaryStorageBillCollection<EU.Business.CusTempStorage.TemporaryStorageBill, EU.Business.CusTempStorage.TemporaryStorageHeader> CreateNewTemporaryStorageBillCollection()
		=> new TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>(this);

	protected override Type GetBillTypeCore() => typeof(TemporaryStorageBill);

	protected override Type GoodsLocationTypeCore => typeof(CusGoodsLocation);

	protected override ZString GetCusGoodsLocationProviderKeyCore() => "ES" + GoodsLocationProviderApplications.Codes.JobDeclaration;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		StoVersion = StoVersionV1;
		SetDefaultTrainingEntry();
	}

	protected override ZString GenerateLocalReferenceNumber() => LRNGeneratorHelper.GenerateLocalReferenceNumber(this, this.GetEORIForLRNGeneration());

	public void AddNewGuaranteeTransactionForG5V1Reception()
	{
		if (GuaranteeHasBondAmountAndIsValid)
		{
			AddGuaranteeTransactionCommon(G5MessageTypeCodeList.Codes.G5v1Reception, LRN, ClearanceDate, -(Guarantee.PW_BondAmount));
		}
	}

	public void AddNewGuaranteeTransactionForG5V1Expedition()
	{
		var cusGuarantee = Guarantee.CusGuarantee;
		if (GuaranteeHasBondAmountAndIsValid && CusGuaranteeHasOBLAndIsValid(cusGuarantee, ZDate.Today))
		{
			AddGuaranteeTransaction(G5MessageTypeCodeList.Codes.G5v1Expedition, LRN);
		}
	}

	public void AddNewGuaranteeTransactionForTSM()
	{
		if (GuaranteeHasBondAmountAndIsValid)
		{
			AddGuaranteeTransaction(G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry, AMA_JobReference);
		}
	}

	public void AddNewGuaranteeTransactionForG5V1ExpeditionCancel(ZDateTime cancelationDate)
	{
		var cusGuarantee = Guarantee.CusGuarantee;
		if (CusGuaranteeHasOBLAndIsValid(cusGuarantee, ZDate.Today)
			&& IsDestinationGoodsLocationManageInPremisesAndDeclarantSameAsConsignee)
		{
			var conTransactionsAmount = GetTransactionsAmount(cusGuarantee);
			var commentSuffix = " " + Res.GetString("1E4BC307-D889-4CA8-BBE1-AD7AF33A68B4", "(Canceled)");

			if (!conTransactionsAmount.IsEmpty)
			{
				AddGuaranteeTransactionCommon(G5MessageTypeCodeList.Codes.G5v1Expedition, LRN, cancelationDate, conTransactionsAmount, commentSuffix);
			}
		}
	}

	public void AddNewGuaranteeTransactionForG5V1ExpeditionAmendment()
	{
		var cusGuarantee = Guarantee.CusGuarantee;
		if (CusGuaranteeHasOBLAndIsValid(cusGuarantee, ZDate.Today)
			&& IsDestinationGoodsLocationManageInPremisesAndDeclarantSameAsConsignee)
		{
			var commentSuffix = " " + Res.GetString("3E02F1BD-FC69-4F5B-A74E-27E14403A5D1", "(Customs Adj)");
			var conTransactionsAmount = GetTransactionsAmount(cusGuarantee);
			var bondAmount = Guarantee?.PW_BondAmount ?? ZDecimal.Zero;

			if (conTransactionsAmount != bondAmount)
			{
				if (conTransactionsAmount.IsEmpty)
				{
					AddGuaranteeTransaction(G5MessageTypeCodeList.Codes.G5v1Expedition, LRN);
				}
				else
				{
					AddGuaranteeTransactionCommon(G5MessageTypeCodeList.Codes.G5v1Expedition, LRN, AcceptanceDate, -(bondAmount - conTransactionsAmount), commentSuffix);
				}
			}
		}
	}

	ZDecimal GetTransactionsAmount(EU.Business.CusGuaranteeHeader guaranteeHeader) => Math.Abs(guaranteeHeader.GetTransactions()?.Cast<SharedCusPermitLineTransaction>().Where(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed && (x.CPL_Reference == DsdtMrnNumberSdFormat)).Sum(x => x.CPL_TranValue) ?? ZDecimal.Zero);

	bool GuaranteeHasBondAmountAndIsValid => Guarantee != null && !Guarantee.PW_BondAmount.IsEmpty;

	bool CusGuaranteeHasOBLAndIsValid(EU.Business.CusGuaranteeHeader guaranteeHeader, ZDate day) => guaranteeHeader != null
																										&& guaranteeHeader.HasOpeningBalanceTransaction
																										&& guaranteeHeader.CPH_StartDate < day
																										&& (guaranteeHeader.CPH_EndDate.IsEmpty || guaranteeHeader.CPH_EndDate > day);

	bool IsDestinationGoodsLocationManageInPremisesAndDeclarantSameAsConsignee => EU.Business.TemporaryStorageHelper.IsLocationManagedInPremises(Factory, DestinationGoodsLocation?.Address.AuthorisationNumber ?? ZString.Empty, CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse)
																					&& IsDeclarantSameAsConsignee;

	bool IsDeclarantSameAsConsignee
	{
		get
		{
			var declarantCode = Declarant?.Header?.OH_Code ?? ZString.Empty;
			var bill = Bills.FirstOrDefault(b => b.ABL_BolType == TemporaryStorageBill.ChildBolCode);
			var consigneeCode = bill?.Consignee?.Header?.OH_Code ?? ZString.Empty;
			if (!declarantCode.IsEmpty && !consigneeCode.IsEmpty && declarantCode == consigneeCode)
			{
				return true;
			}
			else
			{
				var declarantId = Declarant?.Header?.GetIDCode() ?? ZString.Empty;
				var consigneeId = bill?.ABL_ConsigneeRegNo ?? ZString.Empty;
				if (!declarantId.IsEmpty && !consigneeId.IsEmpty && declarantId == consigneeId)
				{
					return true;
				}
			}
			return false;
		}
	}

	void AddGuaranteeTransaction(ZString typeG5, ZString commentReferenceCode) => AddGuaranteeTransactionCommon(typeG5, commentReferenceCode, AcceptanceDate, -(Guarantee.PW_BondAmount));

	void AddGuaranteeTransactionCommon(ZString typeG5, ZString commentReferenceCode, ZDateTime transactionDate, ZDecimal tranValue, string commentSuffix = "")
	{
		var guarantee = Guarantee;
		var date = transactionDate.IsEmpty ? ZDateTime.Now : transactionDate;
		var comment = string.Format((NoResString)"{0} {1}. MRN: {2}{3}", typeG5, commentReferenceCode, MRN, commentSuffix);
		guarantee.CusGuarantee?.AddTransaction(IsMessageTypeTSM ? DsdtMrnNumber : DsdtMrnNumberSdFormat, comment, ZString.Empty, ZString.Empty,
														tranValue, ZDecimal.Zero, status: PermitTransactionStatusList.Codes.Confirmed, transactionDate: date,
														checkBursting: false);
	}

	public TemporaryStorageHeader CreateG5V1Reception()
	{
		var receptionHeader = (TemporaryStorageHeader)this.Clone();
		receptionHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		receptionHeader.MRN = MRN;
		receptionHeader.AMA_MessageStatus = ZString.Empty;
		receptionHeader.AMA_JobReference = ZString.Empty;
		receptionHeader.AMA_GS_NKCustomsAgent = AMA_GS_NKCustomsAgent;
		receptionHeader.AMA_CustomsProfile = AMA_CustomsProfile;
		receptionHeader.TrainingEntry = TrainingEntry;

		foreach (var container in Containers)
		{
			var newContainer = (TemporaryStorageContainer)container.Clone();

			var seals = container.AdditionalSeals.ToArray();
			foreach (var seal in seals)
			{
				var newSeal = (EU.Business.Declaration.CusSeal)seal.Clone();
				newSeal.BK_ParentID = newContainer.PK;
			}

			receptionHeader.Containers.Add(newContainer);
		}

		var receptionBill = receptionHeader.Bills[0];
		var bill = Bills[0];

		var packagePKConversion = new List<(ZGuid oldPK, ZGuid newPK)> { };

		receptionBill.Packs.RemoveAndDeleteAll();
		bill.Packs.Cast<TemporaryStoragePack>().ForEach(delegate (TemporaryStoragePack pack)
		{
			var newPack = (TemporaryStoragePack)GetTemporaryStorageHeaderCloneStrategy(pack).Clone();
			receptionBill.Packs.Add(newPack);
			newPack.APA_MarksAndNumbers = pack.APA_MarksAndNumbers;
			packagePKConversion.Add((pack.PK, newPack.PK));
		});

		receptionBill.PackedItems.RemoveAndDeleteAll();
		bill.PackedItems.ForEach(delegate (TemporaryStoragePackedItem packedItem)
		{
			var newItem = (TemporaryStoragePackedItem)GetTemporaryStorageHeaderCloneStrategy(packedItem).Clone();
			newItem.PresentationDate = packedItem.PresentationDate;

			foreach (AsycudaPackPackedItemPivot pivot in packedItem.PackagesPivot)
			{
				var packPKElement = packagePKConversion.FirstOrDefault(x => x.oldPK == pivot.APP_APA_Pack);
				if (!packPKElement.newPK.IsEmpty)
				{
					var newPivot = newItem.PackagesPivot.AddNew();
					newPivot.APP_APA_Pack = packPKElement.newPK;
				}
			}

			receptionBill.PackedItems.Add(newItem);
		});
		return receptionHeader;
	}

	public void SetClearanceNumber(ZString csvCodeFromUser)
	{
		if (!csvCodeFromUser.Equals(ClearanceNumber))
		{
			var oldValue = ClearanceNumber;
			ClearanceNumber = csvCodeFromUser;
			SetLogWhenSettingCSVCode((NoResString)"CSV Clearance Code", oldValue, ClearanceNumberInfo.Value.ToString());
		}
	}

	public void SetClearanceDate(ZDateTime clearanceDateFromUser)
	{
		if (!clearanceDateFromUser.Equals(ClearanceDate))
		{
			var oldValue = ClearanceDate;
			ClearanceDate = clearanceDateFromUser;

			SetLogWhenSettingCSVCode((NoResString)"Clearance Date", oldValue.ToCustomsFormatDateStringyyyyMMddTHHmmss(), ClearanceDate.ToCustomsFormatDateStringyyyyMMddTHHmmss());
		}
	}

	void SetLogWhenSettingCSVCode(string logReasonCode, string oldValue, string newValue)
	{
		var logTypeCode = "CSV";
		var logReason = (NoResString)"Manually Added " + logReasonCode;
		var parameters = new KeyValuePair<string, string>[]
		{
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Old, oldValue),
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.New, newValue.IsNullOrEmpty() ? (NoResString)"Empty" : newValue),
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, logTypeCode),
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Reason, logReason)
		};
		Logs.AddNew(ZArchitecture.Business.AutoEvents.ChangeOfIdentifier, parameters);
	}

	const string StoVersionV1 = "V1";

	ZString IESMessageBusinessObject.EntryReference => AMA_JobReference;

	GlbStaff IESMessageInfoProvider.Broker => CustomsAgent;
	ZString IESMessageInfoProvider.MRN => MRN;
	ZString IESMessageInfoProvider.DocumentJobReference => MRN;

	public new TemporaryStorageHeaderGuarantee Guarantee => (TemporaryStorageHeaderGuarantee)base.Guarantee;

	protected override Type TemporaryStorageHeaderGuaranteeType => typeof(TemporaryStorageHeaderGuarantee);

	protected override string GuaranteeBondTypeCore => EUGuaranteeTypeList.Codes.TST;

	ZString IESResponseBOMessageStatus.MessageStatus { set => AMA_MessageStatus = value; }

	ZGuid IESResponseBusinessObject.BranchPK => Branch.PK;

	EDIMessageCollection IESResponseBusinessObject.MessageCollection => Messages;

	public bool IsMessageTypeG5V1Reception => AMA_MessageType == G5MessageTypeCodeList.Codes.G5v1Reception;

	public bool IsMessageTypeG5V1Expedition => AMA_MessageType == G5MessageTypeCodeList.Codes.G5v1Expedition;

	public bool IsMessageTypeLAM => AMA_MessageType == G5MessageTypeCodeList.Codes.LameManualEntry;

	public bool IsMessageTypeTSM => AMA_MessageType == G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;

	public bool IsMessageType_G5E_G5R => AMA_MessageType == "G5E" || AMA_MessageType == "G5R";

	public bool IsMessageTypeTSMAndIsUnionGoods => IsMessageTypeTSM && UnionGoods;

	public bool IsMessageTypeG5_LAM_TSM => AMA_MessageType.In(typesG5_LAM_TSM);

	readonly ZString[] typesG5_LAM_TSM = new ZString[]
	{
		G5MessageTypeCodeList.Codes.G5v1Reception,
		G5MessageTypeCodeList.Codes.G5v1Expedition,
		G5MessageTypeCodeList.Codes.LameManualEntry,
		G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry
	};

	public bool IsMessageTypeManual => getListOfMessageTypeManual.Contains(AMA_MessageType);

	readonly ZString[] getListOfMessageTypeManual = new ZString[]
	{
		G5MessageTypeCodeList.Codes.LameManualEntry,
		G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry
	};

	public bool IsNotReadOnlyMemberWhenMessageTypeTSMAndCustomsStatusEmpty => AMA_MessageType != G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry || !CustomsStatus.IsEmpty;

	bool IsNotReadOnlyMemberWhenMessageTypeTSMAndCustomsStatusEmptyAndIsNotUnionGoods => IsNotReadOnlyMemberWhenMessageTypeTSMAndCustomsStatusEmpty || IsMessageTypeTSMAndIsUnionGoods;

	protected override bool IsMessageTypeMatchingToTSRegisterManagementSelectInventoryCore => (AMA_MessageType == G5MessageTypeCodeList.Codes.G5v1Expedition || AMA_MessageType == "G5E");

	#region Cloning and copying

	protected override EU.Business.CusTempStorage.TemporaryStorageHeaderCloneStrategy GetTemporaryStorageHeaderCloneStrategy(BusinessObject bizObjToClone) => new TemporaryStorageHeaderCloneStrategy(bizObjToClone);

	protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
	{
		var newTemporaryStorageHeader = (TemporaryStorageHeader)base.CloneInternal(args);
		CloneExtraFields();
		CloneGuarantee();
		CloneGoodsLocation(GoodsLocation);
		CloneGoodsLocation(DestinationGoodsLocation);

		return newTemporaryStorageHeader;

		void CloneExtraFields()
		{
			newTemporaryStorageHeader.TransportType = TransportType;
			newTemporaryStorageHeader.ArrivalTransportMeansCode = ArrivalTransportMeansCode;
			newTemporaryStorageHeader.DestinationCustomsOffice = DestinationCustomsOffice;
			newTemporaryStorageHeader.AuthorizationOwner = AuthorizationOwner;
			newTemporaryStorageHeader.AuthorizationType = AuthorizationType;
			newTemporaryStorageHeader.AuthorizationNumber = AuthorizationNumber;
		}

		void CloneGuarantee()
		{
			var newGuarantee = (TemporaryStorageHeaderGuarantee)GetTemporaryStorageHeaderCloneStrategy(Guarantee).Clone();
			newGuarantee.Parent = newTemporaryStorageHeader;
		}

		void CloneGoodsLocation(EU.Business.CusGoodsLocation goodsLocation)
		{
			if (goodsLocation != null)
			{
				var newGoodsLocation = (CusGoodsLocation)goodsLocation.Clone();
				var newAddress = (CusGoodsLocationAddress)goodsLocation.Address.Clone();
				newGoodsLocation.CGL_ParentID = newTemporaryStorageHeader.PK;
				newAddress.E2_ParentID = newGoodsLocation.PK;
				newAddress.E2_ParentTableCode = CusGoodsLocationSchema.Constants.Prefix;
				newAddress.E2_AddressType = DocAddressTypes.Codes.Location;
			}
		}
	}

	#endregion

	#region Confirm/Reserve Temporary Storage Goods

	protected override ZString TemporaryStorageTransactionInternalReferenceNumberCore => LRN;

	protected override ZString TemporaryStorageTransactionInternalReferenceTypeCore => CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;

	protected override (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) GetEntryLineDataDeclaredToReserveTSGoodsCore()
	{
		var resultList = new List<DeclarationDataToReserveTSGoods>();

		var docAndItemList = new List<(TemporaryStoragePreviousDocument doc, TemporaryStoragePackedItem item)>();

		var bill = Bills.FirstOrDefault();

		if (bill != null)
		{
			var goodsItemsWithPrevDoc = bill.PackedItems;
			var billDoc = bill.PreviousDocuments.FirstOrDefault();

			foreach (var item in goodsItemsWithPrevDoc)
			{
				var doc = item.PreviousDocuments.FirstOrDefault() ?? billDoc;
				if (doc != null)
				{
					docAndItemList.Add((doc, item));
				}
			}

			var distinctDocAndItemListByDoc = docAndItemList.GroupBy(x => new { refNumber = DocumentHelper.GetDsdtMRNNumberFormat(x.doc.CSI_ReferenceNumber), x.doc.CSI_LineNo });

			foreach (var distinctDocAndItem in distinctDocAndItemListByDoc)
			{
				var distinctDocAndItemArray = distinctDocAndItem.ToArray();
				var docToReturn = distinctDocAndItemArray.First().doc;
				var grossWeight = ZDecimal.Zero;
				var grossWeightForVINs = ZDecimal.Zero;
				var packagesAll = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>();

				foreach (var (doc, item) in distinctDocAndItemArray)
				{
					var itemGrossWeight = item.GrossWeightInKG;
					grossWeight += itemGrossWeight;
					grossWeightForVINs += item.TemporaryStorageLinkPackages.Cast<TemporaryStorageLinkPackage>().Any(x => x.IsLinked && x.Package.APA_PackUQ == PackTypeFrame) ? itemGrossWeight : 0;
					packagesAll.AddRange(GetPackages(item));
				}

				var packagesGroupBy = packagesAll.GroupBy(p => new { p.type, p.marksOrVin });

				var packages = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>();
				foreach (var pack in packagesGroupBy)
				{
					packages.Add((pack.Key.type, pack.ToArray().Sum(p => p.qty), pack.Key.marksOrVin, pack.First().isBulk));
				}

				resultList.Add(new DeclarationDataToReserveTSGoods()
				{
					Document = docToReturn,
					TotalGrossWeight = grossWeight,
					TotalGrossWeightForVINs = grossWeightForVINs,
					Packages = packages
				});
			}
		}

		return (resultList, ZString.Empty);

		IEnumerable<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)> GetPackages(TemporaryStoragePackedItem item)
		{
			var packages = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>();

			var packagingDetails = item.TemporaryStorageLinkPackages.Cast<TemporaryStorageLinkPackage>().Where(x => x.IsLinked).GroupBy(x => new { x.Package.APA_PackUQ, x.Package.APA_MarksAndNumbers });
			foreach (var pack in packagingDetails)
			{
				var packType = pack.Key.APA_PackUQ;
				var packqty = ZInt.Zero;
				var piecesqty = ZInt.Zero;
				foreach (var p in pack.ToArray())
				{
					packqty += p.PackQty;
				}
				packages.Add((packType, packqty, pack.Key.APA_MarksAndNumbers, PackageHelper.PackTypeIsBulk(packType, Factory)));
			}

			return packages;
		}
	}

	protected override ZBool ShouldConfirmTemporaryStorageGoodsConsumption => IsMessageTypeG5V1Expedition;

	protected override IReadOnlyList<ZString> CustomsStatusToCancelTemporaryStoragePendingTransactions => [EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Cancelled];

	protected override IReadOnlyList<ZString> CustomsStatusToConfirmTemporaryStoragePendingTransactions => [EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance];

	protected override IReadOnlyList<ZString> CustomsStatusToNotCreateTemporaryStorageTransactions => [ZString.Empty];

	protected override ZString TemporaryStorageTransactionCommentPrefixCore => TSTransactionCommentPrefixForG5;

	protected override ZString TemporaryStorageWriteOffTransactionCommentReferenceNumber => " / " + TemporaryStorageTransactionInternalReferenceType;

	protected override ZString PreviousDocumentCodeForDataToReserveTemporaryStorageGoods => PreviousDocumentHelper.PreviousDocumentCode337;

	protected override ZDateTime GetIssueDateForTemporaryStorageGoodsConsumptionConfirmation() => !ClearanceDate.IsEmpty && IsMessageTypeG5V1Expedition ? ClearanceDate : base.GetIssueDateForTemporaryStorageGoodsConsumptionConfirmation();

	protected override ZBool ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation => true;

	protected override ZString GetDocumentNumberFormat(ZString dsdtMRN) => DocumentHelper.GetDsdtMRNNumberFormat(dsdtMRN);

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string TSTransactionCommentPrefixForG5 = "G5 JOB:";
	const string PackTypeFrame = "FR";

	#endregion

	#region IDocumentSupportable

	DocumentSupporter IDocumentSupportable.DocumentSupporter => documentSupporter ??= new TemporaryStorageHeaderDocumentSupporter(this);
	DocumentSupporter documentSupporter;

	string IDocumentSupportable.TableName => TableName;

	#endregion
}
