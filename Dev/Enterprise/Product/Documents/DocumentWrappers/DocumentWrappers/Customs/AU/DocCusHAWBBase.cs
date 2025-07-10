using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCusHAWBBase : DocumentWrapper
	{
		protected DocCusHAWBBase(CusHAWBBase cusHAWB, BusinessObjectFactory factoryToWrap)
			: base(cusHAWB, factoryToWrap)
		{
		}

		public static DocCusHAWBBase New(CusHAWBBase cusHAWB, BusinessObjectFactory factoryToWrap)
		{
			return (cusHAWB == null) ? null : new DocCusHAWBBase(cusHAWB, factoryToWrap);
		}

		protected CusHAWBBase CusHAWB
		{
			get { return (CusHAWBBase)WrappedObject; }
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		public ZBool IsMasterHouse
		{
			get { return CusHAWB.CS_IsMasterHouse; }
		}

		public ZString MasterHouseBill
		{
			get { return CusHAWB.CS_MasterHouseBill; }
		}

		public ZShort PiecesManifested
		{
			get { return CusHAWB.CS_PiecesManifested; }
		}

		public ZShort PiecesLanded
		{
			get { return CusHAWB.CS_PiecesLanded; }
		}

		public ZString CustomsStatusCMR
		{
			get { return CusHAWB.CMRCargoStatus.Description; }
		}

		public ZBool IsPrealertHeldByUser
		{
			get { return CusHAWB.CS_IsPrealertHeldByUser; }
		}

		public ZString MessageReference
		{
			get { return CusHAWB.CS_MessageReference; }
		}

		public ZDecimal ChargableWeight
		{
			get { return CusHAWB.CS_ChargableWeight; }
		}

		public DocCusMAWBBase MAWB
		{
			get { return CusHAWB.MAWB != null ? DocCusMAWBBase.New(CusHAWB.MAWB, Factory) : null; }
		}

		public ZString CommercialStatus
		{
			get { return CusHAWB.CS_CommercialStatus; }
		}

		public ZString ConsigneeCity
		{
			get { return CusHAWB.CS_ConsigneeCity; }
		}

		public ZString ConsigneeContactName
		{
			get { return CusHAWB.CS_ConsigneeContactName; }
		}

		public ZString ConsigneeName
		{
			get { return CusHAWB.CS_ConsigneeName; }
		}

		public ZString ConsigneePhone
		{
			get { return CusHAWB.CS_ConsigneePhone_Formatted; }
		}

		public ZString ConsigneePostcode
		{
			get { return CusHAWB.CS_ConsigneePostcode; }
		}

		public ZString ConsigneeState
		{
			get { return CusHAWB.CS_ConsigneeState; }
		}

		public ZString ConsigneeStreet
		{
			get { return CusHAWB.CS_ConsigneeStreet; }
		}

		public ZString ConsigneeStreet2
		{
			get { return CusHAWB.CS_ConsigneeStreet2; }
		}

		public ZString ConsignorCity
		{
			get { return CusHAWB.CS_ConsignorCity; }
		}

		public ZString ConsignorContactName
		{
			get { return CusHAWB.CS_ConsignorContactName; }
		}

		public ZString ConsignorName
		{
			get { return CusHAWB.CS_ConsignorName; }
		}

		public ZString ConsignorPhone
		{
			get { return CusHAWB.CS_ConsignorPhone_Formatted; }
		}

		public ZString ConsignorPostcode
		{
			get { return CusHAWB.CS_ConsignorPostcode; }
		}

		public ZString ConsignorState
		{
			get { return CusHAWB.CS_ConsignorState; }
		}

		public ZString ConsignorStreet
		{
			get { return CusHAWB.CS_ConsignorStreet; }
		}

		public ZString ConsignorStreet2
		{
			get { return CusHAWB.CS_ConsignorStreet2; }
		}

		//		public Unknown Cus type: CS
		//		{
		//			 get { return CusHAWB.CS_CS_MasterHouseBill.IsValid ? Unknown.New(CusHAWB.Factory, CusHAWB.CS_CS_MasterHouseBill) : null; } 
		//		}

		public ZString CustomsMainStatus
		{
			get { return CusHAWB.CS_CustomsMainStatus; }
		}

		public ZString CustomsStatus
		{
			get { return CusHAWB.CS_CustomsStatus; }
		}

		public ZString DataImage
		{
			get { return CusHAWB.CS_DataImage; }
		}

		public ZString FolioReference
		{
			get { return CusHAWB.CS_FolioReference; }
		}

		public ZString FreightPrepaidCollect
		{
			get { return CusHAWB.CS_FreightPrepaidCollect; }
		}

		public virtual ZString FreightPrepaidCollectDescription
		{
			get
			{
				CodeDescriptionPairList legacyCodeList = new CodeDescriptionPairList(OLookUpEditType.PaymentType);
				ZString result = legacyCodeList.GetDescriptionFromCode(CusHAWB.CS_FreightPrepaidCollect);
				if (result.IsEmpty)
				{
					result = new CMRMethodsOfPayment().GetDescriptionFromCode(CusHAWB.CS_FreightPrepaidCollect);
				}
				return result.Trim();
			}
		}

		public ZString GoodsDescription
		{
			get { return CusHAWB.CS_GoodsDescription; }
		}

		public ZDecimal GoodsValue
		{
			get { return CusHAWB.CS_GoodsValue; }
		}

		public ZString HAWB
		{
			get { return CusHAWB.CS_HAWB; }
		}

		public ZBool IsPersonalEffects
		{
			get { return CusHAWB.CS_IsPersonalEffects; }
		}

		public ZBool IsPrealerted
		{
			get { return CusHAWB.CS_IsPrealerted; }
		}

		public ZBool IsResponsePending
		{
			get { return CusHAWB.CS_IsResponsePending; }
		}

		public ZBool IsSelfAssessedClearance
		{
			get { return CusHAWB.CS_IsSelfAssessedClearance; }
		}

		public ZBool IsSurplus
		{
			get { return CusHAWB.CS_IsSurplus; }
		}

		public ZBool IsUserInterventionRequired
		{
			get { return CusHAWB.CS_IsUserInterventionRequired; }
		}

		public DocShipment Shipment
		{
			get { return CusHAWB.CS_JS.IsValid ? DocShipment.New(CusHAWB.Factory, CusHAWB.CS_JS) : null; }
		}

		public DocOrganisation Consignee
		{
			get
			{
				return CusHAWB.ConsigneeAddress != null ? DocOrganisation.New(CusHAWB.ConsigneeAddress, CusHAWB.Factory) : null;
			}
		}

		public DocOrganisation Consignor
		{
			get
			{
				return CusHAWB.ConsignorAddress != null ? DocOrganisation.New(CusHAWB.ConsignorAddress, CusHAWB.Factory) : null;
			}
		}

		public ZString OtherSystemConsigneeCode
		{
			get { return CusHAWB.CS_OtherSystemConsigneeCode; }
		}

		public ZString OtherSystemConsignorCode
		{
			get { return CusHAWB.CS_OtherSystemConsignorCode; }
		}

		public DocUNLOCO NKDestination
		{
			get { return CusHAWB.CS_RL_NKDestination.IsValid ? DocUNLOCO.New(CusHAWB.Factory, CusHAWB.CS_RL_NKDestination) : null; }
		}

		public DocUNLOCO NKLoadPort
		{
			get { return CusHAWB.CS_RL_NKLoadPort.IsValid ? DocUNLOCO.New(CusHAWB.Factory, CusHAWB.CS_RL_NKLoadPort) : null; }
		}

		public DocUNLOCO NKOrigin
		{
			get { return CusHAWB.CS_RL_NKOrigin.IsValid ? DocUNLOCO.New(CusHAWB.Factory, CusHAWB.CS_RL_NKOrigin) : null; }
		}

		public DocCountry NKConsigneeCountry
		{
			get { return CusHAWB.CS_RN_NKConsigneeCountry.IsValid ? DocCountry.New(CusHAWB.Factory, CusHAWB.CS_RN_NKConsigneeCountry) : null; }
		}

		public DocCountry NKConsignorCountry
		{
			get { return CusHAWB.CS_RN_NKConsignorCountry.IsValid ? DocCountry.New(CusHAWB.Factory, CusHAWB.CS_RN_NKConsignorCountry) : null; }
		}

		public DocServiceLevel ServiceLevel
		{
			get
			{
				var serviceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, CusHAWB.CS_RS_NK_ServiceLevel);
				return (serviceLevel == null) ? null : DocServiceLevel.New(serviceLevel, Factory);
			}
		}

		public DocCurrency GoodsCurrency
		{
			get { return CusHAWB.CS_RX_NKGoodsCurrency.IsEmpty ? null : DocCurrency.New(CusHAWB.GoodsCurrency, CusHAWB.Factory); }
		}

		public ZString ShipmentType
		{
			get { return CusHAWB.CS_ShipmentType; }
		}

		public ZString WarehouseLocation
		{
			get { return CusHAWB.CS_WarehouseLocation; }
		}

		public ZDecimal Weight
		{
			get { return CusHAWB.CS_Weight; }
		}

		public ZString WeightUQ
		{
			get { return CusHAWB.CS_WeightUQ; }
		}

		public ZDecimal WeightInKG
		{
			get { return Core.Constants.Weight.Convert(Weight, WeightUQ, Core.Constants.Weight.Kilograms); }
		}
	}
}
