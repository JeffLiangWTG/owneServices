using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Common.Shared;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public partial class JobDeclaration : AutoBRJobDeclaration,
		Integration.Customs.BR.IJobDeclaration,
		IBackDoorSavingSupportableBizObj,
		IApportionInvoiceHolder,
		Integration.Customs.ICusCodeDataTypeSupporter,
		IDocAddresses,
		IJobDocAddressOverrideSupporter,
		ILandedCostHeader,
		IInvoicesProviderValueChangedAnnouncerProvider,
		IAdditionalReferenceNumberTypeProvider
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoBRJobDeclaration.Schema
		{
			public const string AdminstrativeStatus = nameof(JobDeclaration.AdminstrativeStatus);
			public const string AdminstrativeStatusDescription = nameof(JobDeclaration.AdminstrativeStatusDescription);
			public const string CargoStatus = nameof(JobDeclaration.CargoStatus);
			public const string CargoStatusDescription = nameof(JobDeclaration.CargoStatusDescription);
			public const string ClearanceDateAsString = nameof(JobDeclaration.ClearanceDateAsString);
			public const string ClearanceOfficeIsCustomsEnclosure = nameof(JobDeclaration.ClearanceOfficeIsCustomsEnclosure);
			public const string ClearanceOfficeIsHomeDispatch = nameof(JobDeclaration.ClearanceOfficeIsHomeDispatch);
			public const string BoardingOfficeCode = nameof(JobDeclaration.BoardingOfficeCode);
			public const string BoardingOfficeIsCustomsEnclosure = nameof(JobDeclaration.BoardingOfficeIsCustomsEnclosure);
			public const string BoardingEnclosureCode = nameof(JobDeclaration.BoardingEnclosureCode);
			public const string EntranceOfficeCode = nameof(JobDeclaration.EntranceOfficeCode);
			public const string EntrySubmitDateAsString = nameof(JobDeclaration.EntrySubmitDateAsString);
			public const string EntryIssueDateAsString = nameof(JobDeclaration.EntryIssueDateAsString);
			public const string WarehouseAreasConcatenated = nameof(JobDeclaration.WarehouseAreasConcatenated);
			public const string OperationType = nameof(JobDeclaration.OperationType);
			public const string DeclarantType = nameof(JobDeclaration.DeclarantType);
			public const string EntryNumbersConcatenated = nameof(JobDeclaration.EntryNumbersConcatenated);
			public const string EntryStatusesConcatenated = nameof(JobDeclaration.EntryStatusesConcatenated);
			public const string EntryStatusDescriptionsConcatenated = nameof(JobDeclaration.EntryStatusDescriptionsConcatenated);
			public const string RiskChannel = nameof(JobDeclaration.RiskChannel);
			public const string RiskChannelDescription = nameof(JobDeclaration.RiskChannelDescription);
			public const string VesselCountry = nameof(JobDeclaration.VesselCountry);
			public const string BRTransportMode = nameof(JobDeclaration.BRTransportMode);
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ClearanceOfficeIsCustomsEnclosure = true;
			BoardingOfficeIsCustomsEnclosure = IsExport;
			JE_PaymentMethod = ZString.Empty;
		}

		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore()
		{
			return new EntryInstructionProvider(this);
		}

		public BROrgImpAddInfo ImporterAddInfo
		{
			get { return Importer != null ? BROrgImpAddInfo.Get(Importer) : null; }
		}

		#region protected override

		protected override Customs.Business.MergeManager GetMergeManager()
		{
			return new MergeManager(this);
		}

		protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

		protected override bool IsCustomsLineAmendmentATotalReplacement => false;

		protected override bool ShouldKeepDeletedLinesOnAmendmentCore => IsImportOnly;

		public ZBool IsImportOnly => base.IsImport;

		public ZBool IsImportLicense => JE_MessageType == BRJobMessageTypeList.Codes.ImportLicense;

		public ZBool IsImportSiscomex => JE_MessageType == BRJobMessageTypeList.Codes.ImportSiscomex;

		public override ZBool IsImport => IsImportOnly || IsImportSiscomex || IsImportLicense;

		public ZBool IsImportExcludingLicense => !IsImportLicense && IsImport;

		public ZBool IsLPCO => JE_MessageType == BRJobMessageTypeList.Codes.LPCO;

		public ZBool IsAFRMMApplicable => IsImportExcludingLicense && IsTransportByWater;

		public ZBool IsBillNumberOnEntryInstructionApplicable => IsImportOnly && (IsAir || IsRoad || IsRail || IsTransportByWater) && JE_DispatchModality.IsEmpty;

		protected override ZString LocalCurrencyCodeCore
		{
			get { return Core.Constants.CurrencyCodes.Brazil; }
		}

		protected override bool HasSplitEntriesCore
		{
			get
			{
				if (!GetType().FullName.Contains("BR"))
				{
					ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
				}
				return base.HasSplitEntriesCore;
			}
		}

		protected override ZString GetTransportModeGeneric()
		{
			switch (JE_TransportMode)
			{
				case TransportTypeList.Codes.Sea:
					return TransportTypeGenericList.Codes.Sea;

				case TransportTypeList.Codes.Air:
					return TransportTypeGenericList.Codes.Air;

				case TransportTypeList.Codes.Mail:
					return TransportTypeGenericList.Codes.PostMail;

				case TransportTypeList.Codes.Road:
					return TransportTypeGenericList.Codes.Road;

				case TransportTypeList.Codes.Rail:
					return TransportTypeGenericList.Codes.Rail;

				case TransportTypeList.Codes.Own:
					return TransportTypeGenericList.Codes.OwnPropulsion;

				case TransportTypeList.Codes.Lake:
				case TransportTypeList.Codes.River:
				case TransportTypeList.Codes.Fixed:
				case TransportTypeList.Codes.Other:
					return TransportTypeGenericList.Codes.Other;
			}
			return ZString.Empty;
		}

		public override bool ContainerModeVisible => (IsPost || IsTransportByWater) && !IsNonTransportDeclarationType;

		public override bool IsNonTransportDeclarationType => IsImportLicense;

		protected override bool IsPackingInformationRelevantCore => !IsImportLicense && base.IsPackingInformationRelevantCore;

		bool ILandedCostHeader.IsLCSupported => IsImportExcludingLicense;

		public override ZString JE_TransportMode
		{
			get { return base.JE_TransportMode; }
			set
			{
				var oldValue = JE_TransportMode;
				base.JE_TransportMode = value;
				if (!IsCopying && JE_TransportMode != oldValue)
				{
					if (!ContainerModeVisible)
					{
						JE_ContainerMode = ZString.Empty;
					}
					UpdateAFRMMOnEntryInstructionAndInvoiceLines();
				}
			}
		}

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsTransportModeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|BRTransportMode", Caption = "Transport Mode")]
		public ZString BRTransportMode
		{
			get { return BRTransportModeList.MapCW1CodeToBRTransportMode(JE_TransportMode, JE_TransportMeans); }
			set
			{
				var oldValue = BRTransportMode;
				if (!IsCopying && value != oldValue)
				{
					JE_TransportMode = BRTransportModeList.MapBRTransportModeToCW1Code(value);
					JE_TransportMeans = BRTransportModeList.GetTransportMeansForBRTransportMode(value);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateBRTransportMode();
				}

				BRTransportModeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BRTransportModeInfo => GetZPropertyInfo(Schema.BRTransportMode);

		protected override bool SupportsJobComInvoiceLineTaxCore => true;

		void UpdateAFRMMOnEntryInstructionAndInvoiceLines()
		{
			if (!IsAFRMMApplicable)
			{
				CustomsEntryInstructions.Cast<CusEntryInstruction>().Where(x => !x.CEI_AFRMMMethodOfCalculation.IsEmpty || x.IsAFRMMRateOverridden).ForEach(x => { x.CEI_AFRMMMethodOfCalculation = ZString.Empty; x.IsAFRMMRateOverridden = false; });
				InvoiceLines.Cast<JobComInvoiceLine>().Where(x => !x.FMMBenefit.IsEmpty).ForEach(x => x.FMMBenefit = ZString.Empty);
			}
		}

		#endregion

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		[UniversalCopyCollectionEntity(CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.Constants.CEI_JE)]
		public new CusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (CusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;

		public override ZString JE_MessageType
		{
			get => base.JE_MessageType;
			set
			{
				var oldValue = JE_MessageType;
				base.JE_MessageType = value;
				if (!IsCopying && JE_MessageType != oldValue)
				{
					if (!IsImportExcludingLicense)
					{
						ProcessRelatedNumbers.RemoveAndDeleteAll();
						DispatchInstructionNumbers.RemoveAndDeleteAll();
					}

					if (!IsExport)
					{
						ResetClearanceLocalInvolvedParty();
						ResetBoardingLocalAddress();
					}

					JE_OH_Consignee = ZGuid.Empty;
					UpdateJI_ProcedureOnInvoiceLines();
					ResetTaxDetailsDataOnInvoiceLines();
					RefreshIncotermAndChargeFactory();
					ClearCustomsOfficeFields();
					UpdateAFRMMOnEntryInstructionAndInvoiceLines();
				}
			}
		}

		public ZString FixedJobMessageType { get; set; }

		protected override bool JE_MessageType_ReadOnlyCore => !FixedJobMessageType.IsEmpty;

		[MaxLength(nameof(JE_VesselNameMaxLength))]
		public override ZString JE_VesselName { get => base.JE_VesselName; set => base.JE_VesselName = value; }

		int JE_VesselNameMaxLength => (IsImportSiscomex && IsRoad) ? 15 : Schema.JE_VesselNameMaxLength;

		#region JE_MessageSubType

		public override ZString JE_MessageSubType
		{
			get
			{
				return base.JE_MessageSubType;
			}
			set
			{
				var oldValue = JE_MessageSubType;
				base.JE_MessageSubType = value;
				if (!IsCopying && JE_MessageSubType != oldValue)
				{
					if (IsImportSiscomex)
					{
						UpdateJI_ProcedureOnInvoiceLines();
						UpdateJE_DispatchModality();
					}
				}
			}
		}

		#endregion

		void UpdateJI_ProcedureOnInvoiceLines()
		{
			var isImportSiscomex = IsImportSiscomex;
			InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.UpdateJI_Procedure(isImportSiscomex));
		}

		void UpdateJE_DispatchModality()
		{
			if (!JE_DispatchModality.IsEmpty && !RequiresDispatchModality)
			{
				JE_DispatchModality = ZString.Empty;
			}
		}

		[ReadOnlyMember(nameof(JE_OH_ConsigneeReadOnly))]
		public override ZGuid JE_OH_Consignee { get => base.JE_OH_Consignee; set => base.JE_OH_Consignee = value; }

		internal ZBool JE_OH_ConsigneeReadOnly => OperationType != TypeOfOperationImportList.Codes.AccountAndOrder && DeclarantType != DeclarantTypeList.Codes.DoorToDoor;

		public ResourceStringData ConsigneeCaption => IsImportExcludingLicense && DeclarantType != DeclarantTypeList.Codes.DoorToDoor ? Res.GetData("3458D446-635F-4F73-A5E6-CD20AF21274B", "Acquirer", "The Acquirer of the goods in Imports by Account and Order.") : Res.GetData("FE7B9A29-CA74-42C1-972D-48D5DEB919A6", "Consignee", "The Consignee in Door-to-Door Conveyor operations.");

		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|JE_GoodsOrigin", Caption = "Cargo Provenance", FullDescription = "The Country of Provenance of the Cargo.")]
		public override ZString JE_GoodsOrigin { get => base.JE_GoodsOrigin; set => base.JE_GoodsOrigin = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|JE_TotalNoOfPieces", Caption = "Units")]
		public override ZInt JE_TotalNoOfPieces { get => base.JE_TotalNoOfPieces; set => base.JE_TotalNoOfPieces = value; }

		void ClearCustomsOfficeFields()
		{
			JE_SubLocationOfGoods = ZString.Empty;
			JE_LocationOfGoods = ZString.Empty;
			JE_CustomsOffice = ZString.Empty;
			EntranceOfficeCode = ZString.Empty;
		}

		void ResetTaxDetailsDataOnInvoiceLines()
		{
			var taxGroups = new List<string> { Constants.RateCodes.ImportDuty, Constants.RateCodes.IPI, Constants.RateCodes.PIS, Constants.RateCodes.Cofins };
			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				if (IsImportSiscomex)
				{
					invoiceLine.Taxes.Cast<JobComInvoiceLineTax>().Where(w => taxGroups.Contains(w.JLT_Type)).ToList().ForEach(x => x.SetTaxesForImportSiscomex());
					invoiceLine.SpecialCaseTaxes.Rebuild();
				}
				invoiceLine.DuimpTaxRegimes.Rebuild();
				invoiceLine.TaxRegimeAttributes.Rebuild();
				invoiceLine.Attributes.Rebuild();
				invoiceLine.NVECusCodeDataCollection.RebuildFromCharacteristics();
			}
		}

		#region CloneStratergy

		protected override Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{
			return new JobDeclarationDeepCloneStrategy(this, cloneType);
		}

		#endregion

		#region Override Properties

		string IApportionInvoiceHolder.CountryContext => CountryCode + this.GetIncoTermChargeFactoryCacheKey();

		System.Collections.IComparer IApportionInvoiceHolder.ChargeComparer => new ChargesComparer();

		#endregion

		#region ApplicationCode

		#region JE_DeclarantType

		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|JE_DeclarantType", Caption = "Operation Type", FullDescription = "The Type that identifies the characteristics of the operation.")]
		public override ZString JE_DeclarantType
		{
			get => base.JE_DeclarantType;
			set
			{
				var oldValue = JE_DeclarantType;
				base.JE_DeclarantType = value;
				if (!IsCopying && oldValue != JE_DeclarantType)
				{
					if (IsExport)
					{
						DefaultDeclarantAddressIfNeeded(value);
					}
					if (JE_OH_ConsigneeReadOnly)
					{
						JE_OH_Consignee = ZGuid.Empty;
					}
				}
			}
		}

		void DefaultDeclarantAddressIfNeeded(ZString value)
		{
			switch (value)
			{
				case TypeOfOperationExportList.Codes._1001:
				case TypeOfOperationExportList.Codes._1002:
					JE_OA_DeclarantAddress = ZGuid.Empty;
					JE_OA_DeclarantAddress_ZAddress.OrgPK = ZGuid.Empty;
					break;
				case TypeOfOperationExportList.Codes._1003:
					if (JE_OA_DeclarantAddress.IsEmpty)
					{
						var orgProxyMainAddress = Branch?.OrgProxy?.MainAddress.PK ?? ZGuid.Empty;
						if (!orgProxyMainAddress.IsEmpty)
						{
							JE_OA_DeclarantAddress = orgProxyMainAddress;
						}
					}
					break;

				default:
					break;
			}
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.OperationTypeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|OperationType", Caption = "Operation Type", FullDescription = "The Type that identifies the characteristics of the operation.")]
		public ZString OperationType
		{
			get => JE_DeclarantType.SubstringSafe(0, 1).TrimEnd();
			set
			{
				if (value != OperationType)
				{
					UpdatedJE_DeclarantType(value, DeclarantType);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateOperationType();
				}

				OperationTypeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo OperationTypeInfo => GetZPropertyInfo(Schema.OperationType);

		public ZBool IsOperationTypeApplicable => IsImportOnly || (IsImportSiscomex && IsOperationTypeApplicableForDeclarationType(DeclarantType));

		ZBool IsOperationTypeApplicableForDeclarationType(ZString declarantType) => declarantType != DeclarantTypeList.Codes.DoorToDoor && declarantType != DeclarantTypeList.Codes.DiplomaticMission;

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.BRDeclarantTypeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|DeclarantType", Caption = "Declarant Type", FullDescription = "The Declarant Type.")]
		public ZString DeclarantType
		{
			get => JE_DeclarantType.SubstringSafe(1, 1).TrimEnd();
			set
			{
				if (value != DeclarantType)
				{
					var operationType = IsOperationTypeApplicableForDeclarationType(value) ? OperationType : ZString.Empty;
					UpdatedJE_DeclarantType(operationType, value);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateDeclarantType();
				}

				DeclarantTypeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo DeclarantTypeInfo => GetZPropertyInfo(Schema.DeclarantType);

		void UpdatedJE_DeclarantType(ZString operationType, ZString declarantType)
		{
			JE_DeclarantType = operationType.Left(1).PadRight(1) + declarantType;
		}

		#endregion

		#region JE_OA_DeclarantAddress

		[ReadOnlyMember(nameof(JE_OA_DeclarantAddressReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|JE_OA_DeclarantAddress", Caption = "Declarant")]
		public override ZGuid JE_OA_DeclarantAddress { get => base.JE_OA_DeclarantAddress; set => base.JE_OA_DeclarantAddress = value; }

		public bool JE_OA_DeclarantAddressReadOnly => IsExport && JE_DeclarantType == TypeOfOperationExportList.Codes._1001;

		#endregion

		#endregion

		#region JobDocAddresses

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => new DeclarationJobDocAddressValidation(addressToValidate, this);

		protected override DocAddressType[] SupportedAddressTypesCore
		{
			get
			{
				var adddressTypes = base.SupportedAddressTypesCore.ToList();
				if (IsExport && !ClearanceOfficeIsCustomsEnclosure)
				{
					adddressTypes.Add(DocAddressType.ClearanceLocalInvolvedParty);
				}
				if (IsExport && !BoardingOfficeIsCustomsEnclosure)
				{
					adddressTypes.Add(DocAddressType.BoardingLocalDocumentaryAddress);
				}
				return adddressTypes.ToArray();
			}
		}

		[ChildEditable(true)]
		public override JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new BRJobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}
				return fDocAddresses;
			}
		}

		BRJobDocAddressDependentCollection fDocAddresses;

		protected override JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.ClearanceLocalInvolvedParty:
					return ClearanceLocalInvolvedPartyRequirement;
				case DocAddressType.BoardingLocalDocumentaryAddress:
					return BoardingLocalAddressRequirement;
				default:
					return base.GetDocAddressRequirement(addressType);
			}
		}

		#region Clearance Involved Party Address

		public JobDocAddress ClearanceLocalInvolvedParty
		{
			get
			{
				if (fClearanceLocalInvolvedParty == null || fClearanceLocalInvolvedParty.IsDeleted)
				{
					fClearanceLocalInvolvedParty = DocAddresses.FindOrCreateWithRequirement(ClearanceLocalInvolvedPartyRequirement);
				}
				return fClearanceLocalInvolvedParty;
			}
		}
		JobDocAddress fClearanceLocalInvolvedParty;

		JobDocAddressRequirement ClearanceLocalInvolvedPartyRequirement
		{
			get => fClearanceLocalInvolvedPartyRequirement ?? (fClearanceLocalInvolvedPartyRequirement = new ClearanceLocalInvolvedPartyRequirement());
		}
		JobDocAddressRequirement fClearanceLocalInvolvedPartyRequirement;

		#endregion

		#region Boarding Local Address

		public JobDocAddress BoardingLocalAddress
		{
			get
			{
				if (fBoardingLocalAddress == null || fBoardingLocalAddress.IsDeleted)
				{
					fBoardingLocalAddress = DocAddresses.FindOrCreateWithRequirement(BoardingLocalAddressRequirement);
				}
				return fBoardingLocalAddress;
			}
		}
		JobDocAddress fBoardingLocalAddress;

		JobDocAddressRequirement BoardingLocalAddressRequirement
		{
			get => fBoardingLocalAddressRequirement ?? (fBoardingLocalAddressRequirement = new JobDocAddressRequirementWithLightValidation(DocAddressType.BoardingLocalDocumentaryAddress));
		}
		JobDocAddressRequirement fBoardingLocalAddressRequirement;

		#endregion

		#region IJobDocAddressOverrideSupporter Members

		Type IJobDocAddressOverrideSupporter.ZDocAddressControlType
		{
			get { return ObjectFactory.GetType<Integration.Customs.BR.IClearanceDocAddressControl>(); }
		}

		JobDocAddressCollectionForPlugin IJobDocAddressOverrideSupporter.GetJobDocAddressCollectionForPlugin(BusinessObjectFactory factory)
		{
			return null;
		}

		#endregion

		#endregion

		IDictionary<ZString, Type> Integration.Customs.ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ Common.BR.CusCodeDataTypeList.Codes.CustomsOffice, typeof(CustomsOffice) },
				{ Common.BR.CusCodeDataTypeList.Codes.CustomsEnclosure, typeof(CustomsEnclosure) },
				{ Common.BR.CusCodeDataTypeList.Codes.WarehouseArea, typeof(WarehouseArea) }
			};
		}

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		public override ZBool AreMultipleEntryInstructionsAllowed => false;

		public override ZString DeclarationNumber => ActiveEntryHeaders.FormalEntries.Select(x => x.MovementReferenceNumber).CombineValuesAsString(CommonMessageStatusList.Descriptions.MultipleMessageStatus);

		public ZBool IsLake => JE_TransportMode == TransportTypeList.Codes.Lake;

		public ZBool IsRiver => JE_TransportMode == TransportTypeList.Codes.River;

		public bool IsTransportByWater => IsSea || IsRiver || IsLake;

		public bool IsCargoArrivalDocumentApplicable => IsImportSiscomex && (IsTransportByWater || IsAir || IsRoad || IsRail);

		public bool IsMultimodalAvailable => IsCargoArrivalDocumentApplicable && MessageSubTypeList.MultimodalIsAvailable(JE_MessageSubType);

		public bool IsMantraApplicable => IsImportSiscomex && IsAir;

		public bool RequiresTransportDetails => MessageSubTypeList.RequiresTransportDetails(JE_MessageSubType);

		public bool RequiresDepartureDetails => MessageSubTypeList.RequiresDepartureDetails(JE_MessageSubType);

		public bool RequiresShippingLine => RequiresTransportDetails && (IsTransportByWater || IsAir || IsRoad || IsRail);

		public bool ShouldCalculateAfrmm => IsAFRMMApplicable && CustomsEntryInstructions.Count > 0 && (CustomsEntryInstructions[0].IsAFRMMRateOverridden || !CustomsEntryInstructions[0].CEI_AFRMMMethodOfCalculation.IsEmpty);

		public bool IsCargoProvenanceAvailable => IsImportLicense || IsImportSiscomex || (IsImportOnly && JE_DispatchModality.IsEmpty && (IsAir || IsRoad || IsRail));

		[ResourceStringData("FC1D762C-0AD5-461F-A3CE-94A3AD33129E", Caption = "Voyage", IsApplicableMember = nameof(IsTransportByWater))]
		public override ZString JE_VoyageFlightNo { get => base.JE_VoyageFlightNo; set => base.JE_VoyageFlightNo = value; }

		public ResourceStringData OceanBillCaption
		{
			get
			{
				if (IsRail)
				{
					return Res.GetData("62014f6a-ac16-4c69-9dd0-578370952c4f", "Rail Bill", "Rail Bill of the consignment.");
				}
				else if (IsRoad)
				{
					return Res.GetData("cc07f909-bd0a-4a99-885b-11c96e518ebb", "Road Bill", "Road Bill of the consignment.");
				}
				else
				{
					return Res.GetData("bc0ec930-dab2-47dd-bc72-07def6a7811d", "Ocean Bill", "Ocean Bill of the consignment.");
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|CountryVessel", FullDescription = "The Vessel's Flag, which will be submitted to Customs.")]
		public ZString VesselCountry => Vessel?.RV_RN_NKCountryOfReg ?? ZString.Empty;

		public ZPropertyInfo VesselCountryInfo => GetZPropertyInfo(Schema.VesselCountry);

		[MaxLength(18)]
		public override ZString JE_UCR { get => base.JE_UCR; set => base.JE_UCR = value; }

		public ResourceStringData UCRCaption
		{
			get
			{
				if (IsAir)
				{
					return Res.GetData("373175d4-f0c8-490d-a6c5-66bdc9a481e2", "DSIC", "The Cargo Information Subsidiary Document.");
				}
				else if (IsRail)
				{
					return Res.GetData("27a6ae7d-ebb7-4300-a425-6e383c18504e", "TIF/DTA");
				}
				else if (IsMail)
				{
					return Res.GetData("b7eff61b-ba12-4c9e-a55e-b159e66ee08b", "Barcode", "The Barcode.");
				}
				else if (IsTransportByWater)
				{
					return Res.GetData("CE44335F-AF42-4C0B-8AF1-58267787C2FC", "e-Bill", "Electronic Bill number generated by Merchant system.");
				}
				else if (IsRoad)
				{
					return Res.GetData("1E926469-7478-4249-8676-B4CE520423A3", "e-Bill", "Road Transport Bill Number.");
				}
				else
				{
					return Res.GetData("693d5ea4-6734-4d54-b9e1-2e84ae09ea30", "e-Bill", "The Electronic Bill Number, also known as Merchant e-Bill Number.");
				}
			}
		}

		public ZString AdminstrativeStatus => CombineStatusOnEntries(x => x.CH_AdministrativeStatus);

		public ZString AdminstrativeStatusDescription => GetDescriptionByCombinedStatus(AdminstrativeStatus, Factory.GetCachedValue<BRAdministrativeStatusList>());

		public ZString CargoStatus => CombineStatusOnEntries(x => x.CH_CargoStatus);

		public ZString CargoStatusDescription => GetDescriptionByCombinedStatus(CargoStatus, Factory.GetCachedValue<BRCargoStatusList>());

		public ZString ClearanceDateAsString => CombineDatesOnEntries(x => x.CH_EntryReleaseDate);

		public ZString EntrySubmitDateAsString => CombineDatesOnEntries(x => x.CH_EntrySubmittedDate);

		public ZString EntryIssueDateAsString => CombineDatesOnEntries(x => x.MovementReferenceNumberIssueDate);

		public ZString RiskChannel => CombineStatusOnEntries(x => x.CH_RiskChannel);

		public ZString RiskChannelDescription => GetDescriptionByCombinedStatus(RiskChannel, Factory.GetCachedValue<RiskChannelList>());

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.BillTypeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|BillType", Caption = "Bill Type")]
		public ZString BillType
		{
			get
			{
				var billType = "";

				if (IsMail)
				{
					billType = BillTypeList.Codes.Barcode;
				}
				else if (IsRoad)
				{
					billType = BillTypeList.Codes.CRT;
				}
				else
				{
					if (!JE_UCR.IsEmpty && IsAir)
					{
						billType = BillTypeList.Codes.DSIC;
					}
					else if (!JE_UCR.IsEmpty && IsRail)
					{
						billType = BillTypeList.Codes.TIFDTA;
					}
					else if (!JE_HouseBill.IsEmpty)
					{
						if (IsTransportByWater)
						{
							billType = BillTypeList.Codes.HBL;
						}
						else if (IsAir)
						{
							billType = BillTypeList.Codes.HAWB;
						}
						else if (IsRail)
						{
							billType = BillTypeList.Codes.HRWB;
						}
					}
					else if (!JE_MasterBill.IsEmpty)
					{
						if (IsAir)
						{
							billType = BillTypeList.Codes.AWB;
						}
						else if (IsRail)
						{
							billType = BillTypeList.Codes.RWB;
						}
					}
				}

				return billType;
			}
		}

		string CombineStatusOnEntries(Func<CusEntryHeader, ZString> getStatusOnEntry)
		{
			return ActiveEntryHeaders.FormalEntries.Select(getStatusOnEntry).CombineValuesAsString(CommonMessageStatusList.Codes.MultipleMessageStatus);
		}

		string CombineDatesOnEntries(Func<CusEntryHeader, ZDateTime> getDatesOnEntry)
		{
			return ActiveEntryHeaders.FormalEntries.Select(getDatesOnEntry).CombineValuesAsString(CommonMessageStatusList.Descriptions.MultipleMessageStatus);
		}

		string GetDescriptionByCombinedStatus(string status, CodeDescriptionPairList list)
		{
			return status == CommonMessageStatusList.Codes.MultipleMessageStatus ? CommonMessageStatusList.Descriptions.MultipleMessageStatus : list.GetDescriptionFromCode(status);
		}

		public GlbExternalPassword_CCT BrokerCertificate => BRGlbStaffWrapper.Get(CusAgent)?.GetCCTPassword();

		protected override bool RequiresOrderNumbersOnDocsCore()
		{
			return !IsImportLicense && base.RequiresOrderNumbersOnDocsCore();
		}

		protected override bool RequiresOrderTrackLinkCore()
		{
			return !IsImportLicense && base.RequiresOrderTrackLinkCore();
		}

		public bool AllOverseasFreightChargesHaveTheSameCurrency => Factory.GetValue(ref fOverseasFreightCollectOnAllInvoicesHaveTheSameCurrency,
				() => AllChargesHaveTheSameCurrency(ImportCustomsChargeTypeList.OverseasFreightChargeTypes));
		CachedProperty<bool> fOverseasFreightCollectOnAllInvoicesHaveTheSameCurrency;

		public bool AllOverseasInsuranceChargesHaveTheSameCurrency => Factory.GetValue(ref fInternacionalInsuranceOnAllInvoicesHaveTheSameCurrency,
				() => AllChargesHaveTheSameCurrency(CustomsChargeTypeList.Codes.OverseasInsurance));
		CachedProperty<bool> fInternacionalInsuranceOnAllInvoicesHaveTheSameCurrency;

		bool AllChargesHaveTheSameCurrency(params ZString[] chargeTypes)
		{
			return JobComInvoiceGroupHeaders.SelectMany(x => x.Charges.Cast<CommonNonApportionedCharge>())
						.Union(Invoices.SelectMany(x => x.Charges.Cast<CommonNonApportionedCharge>()))
						.Union(InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.Charges.Cast<CommonNonApportionedCharge>()))
						.Where(c => chargeTypes.Contains(c.J7_ChargeType) && !c.J7_RX_NKCurrency.IsEmpty).Select(c => c.J7_RX_NKCurrency).AllSame();
		}

		#endregion

		#region Process Related Numbers

		[ChildEditable(true)]
		public ProcessRelatedNumberCollection ProcessRelatedNumbers
		{
			get
			{
				if (fProcessRelatedNumbers == null)
				{
					fProcessRelatedNumbers = new ProcessRelatedNumberCollection(this);
					fProcessRelatedNumbers.Load();
					RegisterEditableChildObject(fProcessRelatedNumbers);
				}

				return fProcessRelatedNumbers;
			}
		}

		ProcessRelatedNumberCollection fProcessRelatedNumbers;

		#region Dispatch Instruction Numbers

		[ChildEditable(true)]
		public DispatchInstructionNumberCollection DispatchInstructionNumbers
		{
			get
			{
				if (fDispatchInstructionNumbers == null)
				{
					fDispatchInstructionNumbers = new DispatchInstructionNumberCollection(this);
					fDispatchInstructionNumbers.Load();
					RegisterEditableChildObject(fDispatchInstructionNumbers);
				}
				return fDispatchInstructionNumbers;
			}
		}

		DispatchInstructionNumberCollection fDispatchInstructionNumbers;

		#endregion

		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|JE_GS_NKCusAgent", Caption = "Broker", FullDescription = "If the Submit Type is BLT, the application will use the certificate of this staff to submit the Declaration")]
		public override ZString JE_GS_NKCusAgent
		{
			get => base.JE_GS_NKCusAgent;
			set => base.JE_GS_NKCusAgent = value;
		}

		#endregion

		#region Implementation

		protected override ZString GetMessageTypeForDocumentFilter()
		{
			var messageType = JE_MessageType;

			switch (messageType)
			{
				case BRJobMessageTypeList.Codes.LPCO:
				case BRJobMessageTypeList.Codes.ImportLicense:
				case BRJobMessageTypeList.Codes.ImportSiscomex:
					return messageType;
				default:
					return base.GetMessageTypeForDocumentFilter();
			}
		}

		public override void OnSaving()
		{
			if (!IsExport)
			{
				BoardingOffice?.Delete();
				BoardingEnclosure?.Delete();
				CustomsEntryInstructions.Cast<CusEntryInstruction>().ForEach(x => x.DeleteUCRNumber());
			}
			if (!IsImport)
			{
				EntranceOffice?.Delete();
			}
			if (!IsImportExcludingLicense)
			{
				WarehouseAreas.RemoveAndDeleteAll();
			}

			if (!IsBillNumberOnEntryInstructionApplicable)
			{
				CustomsEntryInstructions.Cast<CusEntryInstruction>().ForEach(x => x.DeleteBillNumber());
			}

			base.OnSaving();
		}

		public override void MakeNonPersistent()
		{
			base.MakeNonPersistent();
			CustomsOffices.Cast<CustomsOffice>().ForEach(x => x.MakeNonPersistent());
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobDeclarationFetchStrategy(this);
		}

		protected override void OnInvoicesCollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			base.OnInvoicesCollectionCountChange(sender, e);
			InvoiceLines.RefreshBinding();
		}

		public override bool EnableAttachCommercialInvoice => !IsImportLicense;

		public override bool EnableCopyCommercialInvoice => !IsImportLicense;

		#endregion

		#region IDocumentSupport Members

		protected override DocumentSupporter CreateNewDocumentSupporter()
		{
			return new JobDeclarationDocumentSupporter(this);
		}

		#endregion

		#region IBackDoorSavingSupportableBizObj Members

		AmendmentWithdrawalReason IBackDoorSavingSupportableBizObj.GetAmendmentWithdrawalReason()
		{
			return new AmendmentWithdrawalReason();
		}

		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			if (IsImportLicense)
			{
				return new DeclarationMultiMessageManager(new ImportLicenseMessageSendingObjectParent(this));
			}
			else if (IsExport)
			{
				return new DeclarationMultiMessageManager(new ExportJobDeclarationMessageSendingObjectParent(this));
			}
			else if (IsImportOnly)
			{
				return new DeclarationMultiMessageManager(new DuimpMessageSendingObjectParent(this));
			}
			return null;
		}

		ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return !MergeManager.RequiresMerge || DoMerge() ? ContinueWithDetection.Yes : ContinueWithDetection.No;
		}

		bool IBackDoorSavingSupportableBizObj.SupportBackDoorForSavingWhenAmendmentDetected => IsImportLicense || IsExport || IsImportOnly;

		bool IMessageManageableBizObj.IsInAStatusAmendmentSendable => (IsImportLicense || IsExport || IsImportOnly) && ActiveEntryHeaders.FormalEntries.Any(header => header.CanSendRectification || header.IsWaitingForResponse);

		#endregion

		#region AddInfo Properties

		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SpecialTransportModesList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|BR_SpecialTransport", ShortCaption = "Special Trans", Caption = "Special Mode of Transport")]
		public override ZString JE_SpecialTransport { get => base.JE_SpecialTransport; set => base.JE_SpecialTransport = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DispatchModalityList))]
		public override ZString JE_DispatchModality
		{
			get { return base.JE_DispatchModality; }
			set
			{
				var oldValue = JE_DispatchModality;
				base.JE_DispatchModality = value;
				if (!IsCopying && oldValue != JE_DispatchModality)
				{
					if (!IsBillNumberOnEntryInstructionApplicable)
					{
						CustomsEntryInstructions.Cast<CusEntryInstruction>().ForEach(f => f.BillNumber = ZString.Empty);
					}
					MarkAsNeedingValidation();
				}
			}
		}

		public bool RequiresDispatchModality => IsImportOnly || (IsImportSiscomex && MessageSubTypeList.RequiresDispatchModality(JE_MessageSubType));

		public ResourceStringData DispatchModalityCaption => IsImportOnly ? Res.GetData("28955322-6513-4C01-9FCB-3C8EFF7BEE15", "Dispatch Modality", "The Special Dispatch Modality.") : Res.GetData("2FE2255B-FDF9-4BC1-A391-7834DAA0D2BC", "Dispatch Modality", "Modality Adopted for Customs Clearance.");

		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|BR_IsMultimodal", Caption = "Multimodal")]
		public override ZBool JE_IsMultimodal { get => base.JE_IsMultimodal; set => base.JE_IsMultimodal = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CargoArrivalDocumentList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|BR_CargoArrivalDocumentType", Caption = "Cargo Arrival Document Type", ShortCaption = "Cargo Arrival Doc.", FullDescription = "The Type of the document that proves the arrival of the Cargo.")]
		public override ZString JE_CargoArrivalDocumentType { get => base.JE_CargoArrivalDocumentType; set => base.JE_CargoArrivalDocumentType = value; }

		[MaxLength(15)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|BR_CargoArrivalDocumentNumber", Caption = "Cargo Arrival Document Identification", ShortCaption = "Identification", FullDescription = "The document number that identifies the Cargo Arrival.")]
		public override ZString JE_CargoArrivalDocumentNumber { get => base.JE_CargoArrivalDocumentNumber; set => base.JE_CargoArrivalDocumentNumber = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CargoArrivalDocumentUtilizationList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|BR_CargoArrivalDocumentUtilization", Caption = "Indicative of Utilization", ShortCaption = "Utilization", FullDescription = "The indicative of use of the Bill of Lading at dispatch.")]
		public override ZString JE_CargoArrivalDocumentUtilization { get => base.JE_CargoArrivalDocumentUtilization; set => base.JE_CargoArrivalDocumentUtilization = value; }

		#endregion

		#region JE_MessageTypeChanged

		protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
		{
			base.JE_MessageTypeChanged(oldValue, newValue);

			if (!ContainerModeVisible)
			{
				JE_ContainerMode = ZString.Empty;
			}
			if (!IsImportSiscomex)
			{
				JE_UCR = ZString.Empty;
			}
		}

		#endregion

		#region CusCodeDataCollection

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public CustomsOfficeCollection CustomsOffices
		{
			get
			{
				if (customsOffices == null)
				{
					customsOffices = new CustomsOfficeCollection(this);
					customsOffices.Load();
					RegisterEditableChildObject(customsOffices);
				}
				return customsOffices;
			}
		}

		CustomsOfficeCollection customsOffices;

		[ChildEditable(true)]
		public CustomsEnclosureCollection CustomsEnclosures
		{
			get
			{
				if (customsEnclosures == null)
				{
					customsEnclosures = new CustomsEnclosureCollection(this);
					customsEnclosures.Load();
					RegisterEditableChildObject(customsEnclosures);
				}
				return customsEnclosures;
			}
		}

		CustomsEnclosureCollection customsEnclosures;

		#region Warehouse Area ID

		[ChildEditable(true)]
		public WarehouseAreaCollection WarehouseAreas
		{
			get
			{
				if (warehouseAreas == null)
				{
					warehouseAreas = new WarehouseAreaCollection(this);
					warehouseAreas.Load();
					RegisterEditableChildObject(warehouseAreas);
				}
				return warehouseAreas;
			}
		}

		WarehouseAreaCollection warehouseAreas;

		#endregion

		#endregion

		#region GenPivotCollection

		[ChildEditable(true)]
		public DeclarationRelatedImportLicenseEntryCollection AttachedImportLicenseEntries
		{
			get
			{
				if (fAttachedImportLicenseEntries == null)
				{
					fAttachedImportLicenseEntries = new DeclarationRelatedImportLicenseEntryCollection(this);
					fAttachedImportLicenseEntries.Load();
					RegisterEditableChildObject(fAttachedImportLicenseEntries);
				}
				return fAttachedImportLicenseEntries;
			}
		}
		DeclarationRelatedImportLicenseEntryCollection fAttachedImportLicenseEntries;

		#endregion

		#region PossibleImportLicenseDeclarationForAttachment_List

		public JobDeclarationCollection PossibleImportLicenseDeclarationForAttachment_List => AttachJobDeclarationModuleCollection.GetListForImportLicenseAttaching(this);

		#endregion

		#region Clearance Office

		[MaxLength(7)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOfficeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|JE_CustomsOffice", Caption = "Customs Office", FullDescription = "Clearance Customs Office")]
		public override ZString JE_CustomsOffice
		{
			get => base.JE_CustomsOffice;
			set
			{
				var oldValue = JE_CustomsOffice;
				base.JE_CustomsOffice = value;
				if (!IsCopying && JE_CustomsOffice != oldValue)
				{
					JE_SubLocationOfGoods = ZString.Empty;
				}
			}
		}

		[MaxLength(7)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsEnclosureList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|JE_LocationOfGoods", Caption = "Customs Enclosure", FullDescription = "Clearance Customs Enclosure")]
		public override ZString JE_LocationOfGoods
		{
			get => base.JE_LocationOfGoods;
			set
			{
				var oldValue = JE_LocationOfGoods;
				base.JE_LocationOfGoods = value;
				if (!IsCopying && JE_LocationOfGoods != oldValue)
				{
					JE_SubLocationOfGoods = ZString.Empty;
				}
			}
		}

		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|JE_SubLocationOfGoods", Caption = "Sector", FullDescription = "Sector Customs Enclosure")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SubLocationOfGoodsList))]
		public override ZString JE_SubLocationOfGoods { get => base.JE_SubLocationOfGoods; set => base.JE_SubLocationOfGoods = value; }

		public bool JE_LocationOfGoods_ReadOnly => IsExport && !ClearanceOfficeIsCustomsEnclosure;

		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|ClearanceOfficeIsCustomsEnclosure", Caption = "Is Customs Enclosure?", FullDescription = "Clearance Customs Office Is Customs Enclosure")]
		public ZBool ClearanceOfficeIsCustomsEnclosure
		{
			get => YesNoList.IsYes(JE_LocationQualifier.Left(1));
			set
			{
				base.JE_LocationQualifier = value.ConvertToYesNoList() + JE_LocationQualifier.SubstringSafe(1);
				ClearanceOfficeIsCustomsEnclosureInfo.RefreshBinding();

				if (!ClearanceOfficeIsCustomsEnclosure)
				{
					JE_LocationOfGoods = ZString.Empty;
				}
				else
				{
					ResetClearanceLocalInvolvedParty();
				}
			}
		}

		void ResetClearanceLocalInvolvedParty()
		{
			ClearanceLocalInvolvedParty.E2_AddressOverride = false;
			ClearanceLocalInvolvedParty.OrganisationPK = ZGuid.Empty;
			ClearanceOfficeIsHomeDispatch = false;
			ClearanceLocalInvolvedParty.E2_Latitude = ZDecimal.Zero;
			ClearanceLocalInvolvedParty.E2_Longitude = ZDecimal.Zero;
		}

		public ZPropertyInfo ClearanceOfficeIsCustomsEnclosureInfo => GetZPropertyInfo(Schema.ClearanceOfficeIsCustomsEnclosure);

		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|ClearanceOfficeIsHomeDispatch", Caption = "Is Home Dispatch?", FullDescription = "Clearance Customs Office Is Home Dispatch")]
		public ZBool ClearanceOfficeIsHomeDispatch
		{
			get => YesNoList.IsYes(JE_LocationQualifier.SubstringSafe(1, 1));
			set
			{
				base.JE_LocationQualifier = JE_LocationQualifier.Left(1) + value.ConvertToYesNoList();
				ClearanceOfficeIsHomeDispatchInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ClearanceOfficeIsHomeDispatchInfo => GetZPropertyInfo(Schema.ClearanceOfficeIsHomeDispatch);

		#endregion

		#region Boarding Office

		CustomsOffice BoardingOffice => CustomsOffices.GetFirstElementHaving(Constants.CustomsOfficeCodes.BoardingOffice);
		CustomsOffice CreateNewBoardingOffice() => CustomsOffices.AddNew(Constants.CustomsOfficeCodes.BoardingOffice);

		[MaxLength(7)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOfficeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|BoardingOfficeCode", Caption = "Customs Office", FullDescription = "Boarding Customs Office")]
		public ZString BoardingOfficeCode
		{
			get => BoardingOffice?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(BoardingOfficeCodeInfo, value);

				var office = BoardingOffice;
				if (!value.IsEmpty && office == null)
				{
					office = CreateNewBoardingOffice();
				}
				if (office != null)
				{
					office.CY_Data = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateBoardingOfficeCode();
				}

				BoardingOfficeCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BoardingOfficeCodeInfo => GetZPropertyInfo(Schema.BoardingOfficeCode);

		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|BoardingOfficeIsCustomsEnclosure", Caption = "Is Customs Enclosure?", FullDescription = "Boarding Customs Office Is Customs Enclosure")]
		public ZBool BoardingOfficeIsCustomsEnclosure
		{
			get => BoardingOffice?.CY_IsOverridden ?? ZBool.False;
			set
			{
				var office = BoardingOffice;
				if (value && office == null)
				{
					office = CreateNewBoardingOffice();
				}
				if (office != null)
				{
					office.CY_IsOverridden = value;
				}

				BoardingOfficeIsCustomsEnclosureInfo.RefreshBinding();

				if (!BoardingOfficeIsCustomsEnclosure)
				{
					BoardingEnclosureCode = ZString.Empty;
				}
				else
				{
					ResetBoardingLocalAddress();
				}
			}
		}

		void ResetBoardingLocalAddress()
		{
			BoardingLocalAddress.E2_AddressOverride = false;
			BoardingLocalAddress.OrganisationPK = ZGuid.Empty;
		}

		public ZPropertyInfo BoardingOfficeIsCustomsEnclosureInfo => GetZPropertyInfo(Schema.BoardingOfficeIsCustomsEnclosure);

		CustomsEnclosure BoardingEnclosure => CustomsEnclosures.GetFirstElementHaving(Constants.CustomsOfficeCodes.BoardingOffice);

		CustomsEnclosure CreateNewBoardingEnclosure() => CustomsEnclosures.AddNew(Constants.CustomsOfficeCodes.BoardingOffice);

		[MaxLength(7)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsEnclosureList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|BoardingEnclosureCode", Caption = "Customs Enclosure", FullDescription = "Boarding Customs Enclosure")]
		public ZString BoardingEnclosureCode
		{
			get => BoardingEnclosure?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(BoardingEnclosureCodeInfo, value);

				var enclosure = BoardingEnclosure;
				if (!value.IsEmpty && enclosure == null)
				{
					enclosure = CreateNewBoardingEnclosure();
				}
				if (enclosure != null)
				{
					enclosure.CY_Data = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateBoardingEnclosureCode();
				}

				BoardingEnclosureCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BoardingEnclosureCodeInfo => GetZPropertyInfo(Schema.BoardingEnclosureCode);

		public bool BoardingEnclosureCode_ReadOnly => !BoardingOfficeIsCustomsEnclosure;

		#endregion

		#region Office of Entrance

		CustomsOffice EntranceOffice => CustomsOffices.GetFirstElementHaving(Constants.CustomsOfficeCodes.EntranceOffice);

		CustomsOffice CreateNewEntranceOffice() => CustomsOffices.AddNew(Constants.CustomsOfficeCodes.EntranceOffice);

		[MaxLength(7)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOfficeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|EntranceOfficeCode", Caption = "Customs Office", FullDescription = "Entrance Customs Office")]
		public ZString EntranceOfficeCode
		{
			get => EntranceOffice?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(EntranceOfficeCodeInfo, value);

				var office = EntranceOffice;
				if (!value.IsEmpty && office == null)
				{
					office = CreateNewEntranceOffice();
				}
				if (office != null)
				{
					office.CY_Data = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateEntranceOfficeCode();
				}

				EntranceOfficeCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EntranceOfficeCodeInfo => GetZPropertyInfo(Schema.EntranceOfficeCode);

		#endregion

		#region PaymentBankAccount

		public override ZString JE_PaymentMethod
		{
			get => base.JE_PaymentMethod;
			set
			{
				base.JE_PaymentMethod = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidatePaymentBankAccount();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.BankAccounts))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|PaymentBankAccountPK", Caption = "Bank Account")]
		public ZGuid PaymentBankAccountPK
		{
			get
			{
				return JE_PaymentMethod == PaymentPartyCodeDescriptionList.Codes.Broker
					? Registry.BRCustomsDataRegistry.Instance.TaxFeeCustomsPaymentBankAccount.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty)
					: Guid.Empty;
			}
		}

		public ZPropertyInfo PaymentBankAccountPKInfo => GetZPropertyInfo(nameof(PaymentBankAccountPK));

		public AccBankAccount PaymentBankAccount => Factory.Load<AccBankAccount>(PaymentBankAccountPK);

		#endregion

		#region JE_OH_Importer

		protected override void JE_OH_ImporterChanged(ZGuid oldValue, ZGuid newValue)
		{
			base.JE_OH_ImporterChanged(oldValue, newValue);
			if (IsImport)
			{
				var orgImpAddInfo = ImporterAddInfo;
				if (orgImpAddInfo != null)
				{
					JE_PaymentMethod = orgImpAddInfo.ZO_AccountNumber.IsEmpty ? PaymentPartyCodeDescriptionList.Codes.Broker : PaymentPartyCodeDescriptionList.Codes.Importer;
				}
			}
		}

		#endregion

		#region Warehouse Areas IDs Concatenated

		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|WarehouseAreasConcatenated", Caption = "Area ID", FullDescription = "The Warehouse Area Identifier.")]
		public ZString WarehouseAreasConcatenated => string.Join(",", WarehouseAreas.Cast<CusCodeData>().Select(x => x.CY_Code));

		public ZPropertyInfo WarehouseAreasConcatenatedInfo => GetZPropertyInfo(Schema.WarehouseAreasConcatenated);

		#endregion

		#region EntryNumbersConcatenated

		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|EntryNumbersConcatenated", Caption = "License Number", FullDescription = "License(Entry) Number")]
		public ZString EntryNumbersConcatenated
		{
			get => string.Join(";", ActiveEntryHeaders.FormalEntries.Select(x => x.EntryNumber).Where(x => !x.IsEmpty));
		}

		#endregion

		#region EntryStatusesConcatenated

		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|EntryStatusesConcatenated", Caption = "License Status", FullDescription = "License(Entry) Status")]
		public ZString EntryStatusesConcatenated
		{
			get => string.Join(";", ActiveEntryHeaders.FormalEntries.Select(x => x.CH_EntryStatus).Where(x => !x.IsEmpty));
		}

		#endregion

		#region EntryStatusDescriptionsConcatenated

		[ResourceStringData("Enterprise.Customs.BR.Business.JobDeclaration|EntryStatusDescriptionsConcatenated", Caption = "License Status Description", FullDescription = "License(Entry) Status Description")]
		public ZString EntryStatusDescriptionsConcatenated
		{
			get => string.Join(";", ActiveEntryHeaders.FormalEntries.Select(x => x.EntryHeaderStatusDescription).Where(x => !x.IsEmpty));
		}

		#endregion

		#region AttachedOrdersVisible

		public override bool AttachedOrdersVisible => !IsLPCO && base.AttachedOrdersVisible;

		#endregion

		public NFeExportObject NFeExportObject => fNFeExportObject ?? (fNFeExportObject = new NFeExportObject(this));
		NFeExportObject fNFeExportObject;

		protected override ExchangeRateType RateTypeCore => IsExport ? ExchangeRateType.CustomsSecondary : base.RateTypeCore;

		protected override IReadOnlyList<string> MultipleKeysToUseCore => new string[] { JE_MessageType };

		#region FormalEntryHeaders

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public FormalCusEntryHeaderCollection FormalEntryHeaders => formalEntryHeaders ?? (formalEntryHeaders = new FormalCusEntryHeaderCollection(this));
		FormalCusEntryHeaderCollection formalEntryHeaders;

		#endregion

		IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer()
		{
			return new DeclarationValueChangedAnnouncer(this);
		}

		#region IAdditionalReferenceNumberType

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			CodeDescriptionPairList result = null;
			if (category == CusEntryNumber.Categories.AdditionalReferenceNumber)
			{
				result = Factory.GetCachedValue($"BRAdditionalReferenceNumberTypes_{IsMantraApplicable}", () =>
				{
					var tempList = CusEntryNumLookups.GetAdditionalReferenceNumberTypes(countryCode);
					if (!IsMantraApplicable)
					{
						tempList.RemoveCode(BrazilAdditionalReferenceNumberTypes.Codes.MBL);
					}
					return tempList;
				});
			}
			else
			{
				result = GetAdditionalReferenceNumberTypeListCore(category, countryCode);
			}
			return result;
		}

		#endregion
	}
}
