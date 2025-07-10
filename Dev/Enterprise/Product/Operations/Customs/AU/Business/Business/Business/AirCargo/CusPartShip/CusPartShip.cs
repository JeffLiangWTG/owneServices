using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusPartShip : Customs.Business.CusPartShip, Customs.Business.IStatusNeedsRecalculationProvider, ICMRMessageRespondee, IUnderbondMovementRequestHeaderProvider, ICusUnderbondDependentCollectionParent, ICusHAWBBase, IOutturnableLine, IEDIMessageCollectionProvider, IDetailsTabPageHeadingProvider, Integration.Customs.AU.ICusPartShip
	{
		public CusPartShip(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			Calculator = new CusPartShipStatusCalculator(this);
		}

		public static CusPartShip LoadFromSendersReference(BusinessObjectFactory factory, ZString sendersReference)
		{
			sendersReference = sendersReference.TrimStart('P');
			ZString[] splitSendersReference = sendersReference.Split('/');
			if (splitSendersReference.Length == 2)
			{
				ZString hAWBReference = splitSendersReference[0];
				CTOCusHAWB hAWB = CTOCusHAWB.LoadFromSendersReference(factory, hAWBReference);
				ZString partShipReference = splitSendersReference[1];

				if (hAWB != null)
				{
					foreach (CusPartShip partShip in hAWB.PartShips)
					{
						if (partShip.CG_MessageReference == partShipReference)
						{
							return partShip;
						}
					}
				}
			}
			return null;
		}

		public static CusPartShip Load(BusinessObjectFactory factory, ICTOCusHAWBInformationProvider info)
		{
			if (info.ArrivalDate.IsEmpty || !info.ArrivalDate.IsValid)
			{
				return null;
			}

			ZQuery hAWBFilter = new ZQuery();
			hAWBFilter.AddToFilter(CusHAWBSchema.CS_HAWB, info.MAWB);

			foreach (CusHAWBBase hAWB in factory.Load(typeof(CusHAWBBase), hAWBFilter))
			{
				if (hAWB is CTOCusHAWB)
				{
					foreach (CusPartShip partShip in hAWB.PartShips)
					{
						if (partShip.CG_FlightNo == info.FlightNumber && partShip.CG_ArrivalDate == info.ArrivalDate)
						{
							return partShip;
						}
					}
				}
			}
			return null;
		}

		public readonly CusPartShipStatusCalculator Calculator;

		public override void OnSaving()
		{
			base.OnSaving();
			SetMessageReference();
		}

		public void SetMessageReference()
		{
			PopulateNumberPropertyIfRequired(CG_MessageReferenceInfo, GetNewMessageReference);
		}

		ZString GetNewMessageReference(BusinessObjectFactory factory)
		{
			var reference = Env.NumberFountains.AUAirCargoPartShipment.GetNextFormatted(factory);
			isMessageReferenceAssignedFromNumberFountain = true;
			return reference;
		}
		bool isMessageReferenceAssignedFromNumberFountain;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded)
			{
				CG_CustomsStatus = IsInDatabase ? (ZString)CG_CustomsStatusInfo.OriginalValue : ZString.Empty;

				if (isMessageReferenceAssignedFromNumberFountain)
				{
					CG_MessageReference = ZString.Empty;
				}
			}

			isMessageReferenceAssignedFromNumberFountain = false;
		}

		protected CusHAWBBase fHouseBill;
		public CusHAWBBase HouseBill
		{
			get
			{
				if (fHouseBill == null)
				{
					fHouseBill = Factory.Load<CusHAWBBase>(CG_CS);
				}
				return fHouseBill;
			}
		}

		protected CusMAWBBase fMAWB;
		public CusMAWBBase MAWB
		{
			get
			{
				if (fMAWB == null)
				{
					fMAWB = Factory.Load<CusMAWBBase>(CG_CM_LinkToPartMaster);
				}
				return fMAWB;
			}
		}

		public RefUNLOCOCollection PortList
		{
			get
			{
				return new RefUNLOCOCollection(Factory);
			}
		}

		public ZString CG_Description
		{
			get { return CG_FlightNo + " : " + CG_ArrivalDate.ToShortDateString(); }
		}

		EDIMessageCollection fMessages;
		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}

		[ReadOnly(true)]
		public override ZString CG_CustomsStatus
		{
			get { return base.CG_CustomsStatus; }
		}

		#region IStatusNeedsRecalculationProvider Members

		public bool StatusNeedsRecalculation
		{
			get { return Messages.HasChanges; }
		}

		#endregion

		#region ICMRMessageRespondee Members

		public ZString Details
		{
			get { return CG_Description; }
		}

		public virtual ZString ShortDescription
		{
			get { return CG_Description; }
		}

		#endregion

		#region IUnderbondMovementRequestHeaderProvider Members

		IUnderbondMovementRequestHeader IUnderbondMovementRequestHeaderProvider.GetHeader(CusUnderbond underbond)
		{
			if (HouseBill is CTOCusHAWB)
			{
				return new CTOCusPartShipUnderbondMovementRequestHeader(underbond, HouseBill as CTOCusHAWB, this);
			}
			else if (HouseBill is CusHAWB)
			{
				return new HouseCusPartShipUnderbondMovementRequestHeader(underbond, HouseBill as CusHAWB, this);
			}
			else if (MAWB is CusMAWB)
			{
				return new MasterCusPartShipUnderbondMovementRequestHeader(underbond, MAWB as CusMAWB, this);
			}
			return null;
		}

		public bool IsBureau
		{
			get
			{
				bool result = false;
				if (HouseBill != null && HouseBill.MAWB != null)
				{
					result = HouseBill.MAWB.CM_IsBureau;
				}
				else if (MAWB is CusMAWB)
				{
					result = MAWB.CM_IsBureau;
				}
				return result;
			}
		}

		#endregion

		#region ICusUnderbondDependentCollectionParent Members

		public IOutturnableLine[] OutturnableLines
		{
			get
			{
				return System.Array.Empty<IOutturnableLine>();
			}
		}

		Customs.Business.CusUnderbondCollection ICusUnderbondDependentCollectionParent.Underbonds => Underbonds;
		[ChildEditable(true)]
		public CusUnderbondCollection Underbonds
		{
			get
			{
				if (fUnderbonds == null)
				{
					fUnderbonds = new CusUnderbondCollection(this);
					fUnderbonds.Load();
					RegisterEditableChildObject(fUnderbonds);
				}
				return fUnderbonds;
			}
		}
		CusUnderbondCollection fUnderbonds;

		public bool CanSendWithoutDelay
		{
			get { return true; }
		}

		public ZString UnderbondHumanReadableName
		{
			get
			{
				ICusUnderbondDependentCollectionParent parent = MAWB as ICusUnderbondDependentCollectionParent
					?? HouseBill;

				return parent.UnderbondHumanReadableName + " - Part Shipment: " + CG_FlightNo + ", " + CG_ArrivalDate.ToShortDateString();
			}
		}

		ZString IOutturnableLine.CargoStatus
		{
			get { return ZString.Empty; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return 0; }
		}

		bool ICusUnderbondDependentCollectionParent.UsesTranshipmentPortOnUnderbond
		{
			get { return false; }
		}

		ZString ICusUnderbondDependentCollectionParent.DefaultTranshipmentPort
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ICusHAWBBase Members

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_MessageReferenceMaxLength)]
		public ZString CS_MessageReference
		{
			get { return CG_MessageReference; }
		}

		public ZPropertyInfo CS_MessageReferenceInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_MessageReference); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_HAWBMaxLength)]
		public ZString CS_HAWB
		{
			get { return HouseBill.CS_HAWB; }
		}

		public ZPropertyInfo CS_HAWBInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_HAWB); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_RL_NKLoadPortMaxLength)]
		public ZString CS_RL_NKLoadPort
		{
			get { return CG_RL_NKLoadPort; }
		}

		public ZPropertyInfo CS_RL_NKLoadPortInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_RL_NKLoadPort); }
		}
		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_RL_NKOriginMaxLength)]
		public ZString CS_RL_NKOrigin
		{
			get { return HouseBill.CS_RL_NKOrigin; }
		}

		public ZPropertyInfo CS_RL_NKOriginInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_RL_NKOrigin); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_RL_NKDestinationMaxLength)]
		public ZString CS_RL_NKDestination
		{
			get { return HouseBill.CS_RL_NKDestination; }
		}

		public ZPropertyInfo CS_RL_NKDestinationInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_RL_NKDestination); }
		}

		[BusinessObjectTestExclude]
		public ZDecimal CS_Weight
		{
			get { return HouseBill.CS_Weight; }
		}

		public ZPropertyInfo CS_WeightInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_Weight); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_WeightUQMaxLength)]
		public ZString CS_WeightUQ
		{
			get { return HouseBill.CS_WeightUQ; }
		}

		public ZPropertyInfo CS_WeightUQInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_WeightUQ); }
		}
		[BusinessObjectTestExclude]
		public ZDecimal CS_GoodsValue
		{
			get { return HouseBill.CS_GoodsValue; }
		}

		public ZPropertyInfo CS_GoodsValueInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_GoodsValue); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_RX_NKGoodsCurrencyMaxLength)]
		public ZString CS_RX_NKGoodsCurrency
		{
			get { return HouseBill.CS_RX_NKGoodsCurrency; }
		}

		public ZPropertyInfo CS_RX_NKGoodsCurrencyInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_RX_NKGoodsCurrency); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_GoodsDescriptionMaxLength)]
		public ZString CS_GoodsDescription
		{
			get { return HouseBill.CS_GoodsDescription; }
		}

		public ZPropertyInfo CS_GoodsDescriptionInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_GoodsDescription); }
		}
		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_CustomsStatusMaxLength)]
		public ZString CS_CustomsStatus
		{
			get { return CG_CustomsStatus; }
		}

		public ZPropertyInfo CS_CustomsStatusInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_CustomsStatus); }
		}

		[BusinessObjectTestExclude]
		public ZBool CS_IsMasterHouse
		{
			get { return HouseBill.CS_IsMasterHouse; }
		}

		public ZPropertyInfo CS_IsMasterHouseInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_IsMasterHouse); }
		}

		[BusinessObjectTestExclude]
		public ZShort CS_PiecesManifested
		{
			get { return CG_PiecesManifested; }
		}

		public ZPropertyInfo CS_PiecesManifestedInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_PiecesManifested); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_FreightPrepaidCollectMaxLength)]
		public ZString CS_FreightPrepaidCollect
		{
			get { return HouseBill.CS_FreightPrepaidCollect; }
		}

		public ZPropertyInfo CS_FreightPrepaidCollectInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_FreightPrepaidCollect); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_WarehouseLocationMaxLength)]
		public ZString CS_WarehouseLocation
		{
			get { return HouseBill.CS_WarehouseLocation; }
		}

		public ZPropertyInfo CS_WarehouseLocationInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_WarehouseLocation); }
		}

		[MaxLength(50)]
		public virtual ZString WarehouseLocationCaption
		{
			get { return "Warehouse Location:"; }
		}

		public ZPropertyInfo WarehouseLocationCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(WarehouseLocationCaption)); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_FolioReferenceMaxLength)]
		public ZString CS_FolioReference
		{
			get { return HouseBill.CS_FolioReference; }
		}

		public ZPropertyInfo CS_FolioReferenceInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_FolioReference); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_ShipmentTypeMaxLength)]
		public ZString CS_ShipmentType
		{
			get { return HouseBill.CS_ShipmentType; }
		}

		public ZPropertyInfo CS_ShipmentTypeInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ShipmentType); }
		}

		CusHAWBLookups ICusHAWBBase.Lookups
		{
			get { return HouseBill.Lookups; }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_ConsigneeCityMaxLength)]
		public ZString CS_ConsigneeCity
		{
			get { return HouseBill.CS_ConsigneeCity; }
		}

		public ZPropertyInfo CS_ConsigneeCityInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ConsigneeCity); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_ConsigneeNameMaxLength)]
		public ZString CS_ConsigneeName
		{
			get { return HouseBill.CS_ConsigneeName; }
		}

		public ZPropertyInfo CS_ConsigneeNameInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ConsigneeName); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_ConsigneePhoneMaxLength)]
		public ZString CS_ConsigneePhone
		{
			get { return HouseBill.CS_ConsigneePhone; }
		}

		public ZPropertyInfo CS_ConsigneePhoneInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ConsigneePhone); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_ConsigneePostcodeMaxLength)]
		public ZString CS_ConsigneePostcode
		{
			get { return HouseBill.CS_ConsigneePostcode; }
		}

		public ZPropertyInfo CS_ConsigneePostcodeInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ConsigneePostcode); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_ConsigneeStateMaxLength)]
		public ZString CS_ConsigneeState
		{
			get { return HouseBill.CS_ConsigneeState; }
		}

		public ZPropertyInfo CS_ConsigneeStateInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ConsigneeState); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_ConsigneeStreetMaxLength)]
		public ZString CS_ConsigneeStreet
		{
			get { return HouseBill.CS_ConsigneeStreet; }
		}

		public ZPropertyInfo CS_ConsigneeStreetInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ConsigneeStreet); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_ConsigneeStreet2MaxLength)]
		public ZString CS_ConsigneeStreet2
		{
			get { return HouseBill.CS_ConsigneeStreet2; }
		}

		public ZPropertyInfo CS_ConsigneeStreet2Info
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ConsigneeStreet2); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_ConsignorNameMaxLength)]
		public ZString CS_ConsignorName
		{
			get { return HouseBill.CS_ConsignorName; }
		}

		public ZPropertyInfo CS_ConsignorNameInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ConsignorName); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_ConsignorPostcodeMaxLength)]
		public ZString CS_ConsignorPostcode
		{
			get { return HouseBill.CS_ConsignorPostcode; }
		}

		public ZPropertyInfo CS_ConsignorPostcodeInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ConsignorPostcode); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_ConsignorStateMaxLength)]
		public ZString CS_ConsignorState
		{
			get { return HouseBill.CS_ConsignorState; }
		}

		public ZPropertyInfo CS_ConsignorStateInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ConsignorState); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_ConsignorCityMaxLength)]
		public ZString CS_ConsignorCity
		{
			get { return HouseBill.CS_ConsignorCity; }
		}

		public ZPropertyInfo CS_ConsignorCityInfo
		{
			get
			{
				return GetZPropertyInfo(CusHAWBBase.Schema.CS_ConsignorCity);
			}
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_ConsignorStreetMaxLength)]
		public ZString CS_ConsignorStreet
		{
			get { return HouseBill.CS_ConsignorStreet; }
		}

		public ZPropertyInfo CS_ConsignorStreetInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ConsignorStreet); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_ConsignorStreet2MaxLength)]
		public ZString CS_ConsignorStreet2
		{
			get { return HouseBill.CS_ConsignorStreet2; }
		}

		public ZPropertyInfo CS_ConsignorStreet2Info
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ConsignorStreet2); }
		}

		[BusinessObjectTestExclude]
		public ZGuid CS_OH_Consignee
		{
			get { return HouseBill.CS_OH_Consignee; }
		}

		public ZPropertyInfo CS_OH_ConsigneeInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_OH_Consignee); }
		}

		[BusinessObjectTestExclude]
		public ZGuid CS_OH_Consignor
		{
			get { return HouseBill.CS_OH_Consignor; }
		}

		public ZPropertyInfo CS_OH_ConsignorInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_OH_Consignor); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_RN_NKConsigneeCountryMaxLength)]
		public ZString CS_RN_NKConsigneeCountry
		{
			get { return HouseBill.CS_RN_NKConsigneeCountry; }
		}

		public ZPropertyInfo CS_RN_NKConsigneeCountryInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_RN_NKConsigneeCountry); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_RN_NKConsignorCountryMaxLength)]
		public ZString CS_RN_NKConsignorCountry
		{
			get { return HouseBill.CS_RN_NKConsignorCountry; }
		}

		public ZPropertyInfo CS_RN_NKConsignorCountryInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_RN_NKConsignorCountry); }
		}

		[BusinessObjectTestExclude]
		public ZDecimal CS_ChargableWeight
		{
			get { return HouseBill.CS_ChargableWeight; }
		}

		public ZPropertyInfo CS_ChargableWeightInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ChargableWeight); }
		}

		[MaxLength(50)]
		public virtual ZString ChargableWeightCaption
		{
			get
			{
				return Res.GetString("A40D990A-DEF8-4E57-BD20-BC199522A17F", "Chargeable Weight:");
			}
		}

		public ZPropertyInfo ChargableWeightCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(ChargableWeightCaption)); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWBBase.Schema.CS_RS_NK_ServiceLevelMaxLength)]
		public ZString CS_RS_NK_ServiceLevel
		{
			get { return HouseBill.CS_RS_NK_ServiceLevel; }
		}

		public ZPropertyInfo CS_RS_NK_ServiceLevelInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_RS_NK_ServiceLevel); }
		}

		[BusinessObjectTestExclude]
		public ZBool CS_IsSelfAssessedClearance
		{
			get { return HouseBill.CS_IsSelfAssessedClearance; }
		}

		public ZPropertyInfo CS_IsSelfAssessedClearanceInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_IsSelfAssessedClearance); }
		}

		[BusinessObjectTestExclude]
		public ZBool CS_IsPersonalEffects
		{
			get { return HouseBill.CS_IsPersonalEffects; }
		}

		public ZPropertyInfo CS_IsPersonalEffectsInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_IsPersonalEffects); }
		}

		public CusPartShipCollection PartShips
		{
			get { return HouseBill.PartShips; }
		}

		public bool CanDeleteFromICusHAWBCollection
		{
			get { return false; }
		}

		public ZString CS_PaymentTypeCaption
		{
			get { return HouseBill.CS_PaymentTypeCaption; }
		}

		public ZPropertyInfo CS_PaymentTypeCaptionInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_PaymentTypeCaption); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWB.Schema.CS_FreightPrepaidCollectMaxLength)]
		public ZString CS_FreightPrepaidCollectForBinding
		{
			get { return HouseBill.CS_FreightPrepaidCollectForBinding; }
		}

		public ZPropertyInfo CS_FreightPrepaidCollectForBindingInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_FreightPrepaidCollectForBinding); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWB.Schema.CS_ShipmentTypeMaxLength)]
		public ZString CS_ShipmentTypeForBinding
		{
			get { return HouseBill.CS_ShipmentTypeForBinding; }
		}

		public ZPropertyInfo CS_ShipmentTypeForBindingInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_ShipmentTypeForBinding); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(CusHAWB.Schema.CS_TranshipmentEntryNumMaxLength)]
		public ZString CS_TranshipmentEntryNum
		{
			get { return HouseBill.CS_TranshipmentEntryNum; }
		}

		public ZPropertyInfo CS_TranshipmentEntryNumInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_TranshipmentEntryNum); }
		}

		public MessageCusStatus CMRMessageStatus
		{
			get { return HouseBill.CMRMessageStatus; }
		}

		public CargoCusStatus CMRCargoStatus
		{
			get { return HouseBill.CMRCargoStatus; }
		}

		#endregion

		#region ISACLiabilityQuestionProvider

		public ZDecimal GoodsValueInLocalCurrency
		{
			get { return HouseBill.GoodsValueInLocalCurrency; }
		}

		public ZString GoodsDescription
		{
			get { return CS_GoodsDescription; }
		}

		public ZPropertyInfo SACFlagInfo
		{
			get { return CS_IsSelfAssessedClearanceInfo; }
		}

		#endregion

		string IDetailsTabPageHeadingProvider.Heading
		{
			get { return UnderbondHumanReadableName; }
		}
	}
}
