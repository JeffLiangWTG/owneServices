using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class HAWBToShipmentBridge : IUpdateFromShipment
	{
		public HAWBToShipmentBridge(CusHAWB hAWB)
		{
			this.hAWB = Argument.NotNull(hAWB, "HAWB cannot be null");
			fIsAir = true;
		}
		readonly CusHAWB hAWB;

		#region Should We Synchronise

		public bool ShouldWeSynchronise
		{
			get
			{
				bool result = fIsAir;
				if (hAWB.IsDeleted)
				{
					result = false;
				}

				ZString cMRMessageStatus = hAWB.CMRMessageStatus.Code;
				if (cMRMessageStatus == CMRBaseStatuses.Codes.OriginalAccepted ||
								cMRMessageStatus == CMRBaseStatuses.Codes.AmendmentAccepted ||
								cMRMessageStatus == CMRBaseStatuses.Codes.AwaitingResponseToOriginal ||
								cMRMessageStatus == CMRBaseStatuses.Codes.AwaitingResponseToAmendment)
				{
					result = false;
				}
				return result;
			}
		}

		#endregion

		#region IUpdateFromShipment Members

		ZDecimal IUpdateFromShipment.ActualWeight
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					hAWB.CS_Weight = value;
				}
			}
		}

		ZGuid IUpdateFromShipment.CoLoadMasterShipmentPK
		{
			set
			{
				if (ShouldWeSynchronise && !value.IsEmpty)
				{
					var masterHouse = CusHAWB.Load(hAWB.Factory, new[] { value }).FirstOrDefault();
					hAWB.CS_CS_MasterHouseBill = masterHouse == null ? ZGuid.Empty : masterHouse.PK;
				}
			}
		}

		ZString IUpdateFromShipment.CoLoadMasterShipmentHouseBillNumber
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					hAWB.CS_MasterHouseBill = value;
				}
			}
		}

		void IUpdateFromShipment.CopyConsigneeDetails(CommonShipment shipment)
		{
			if (ShouldWeSynchronise)
			{
				hAWB.CopyConsigneeDetails();
			}
		}

		void IUpdateFromShipment.CopyConsignorDetails(CommonShipment shipment)
		{
			if (ShouldWeSynchronise)
			{
				hAWB.CopyConsignorDetails();
			}
		}

		ZString IUpdateFromShipment.Destination
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					hAWB.CS_RL_NKDestination = value;
				}
			}
		}

		ZString IUpdateFromShipment.GoodsCurrency
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					hAWB.CS_RX_NKGoodsCurrency = value;
				}
			}
		}

		ZString IUpdateFromShipment.GoodsDescription
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					hAWB.CS_GoodsDescription = value;
				}
			}
		}

		ZDecimal IUpdateFromShipment.GoodsValue
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					hAWB.CS_GoodsValue = value;
				}
			}
		}

		ZString IUpdateFromShipment.HouseBillNumber
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					hAWB.CS_HAWB = value;
				}
			}
		}

		bool fIsAir;
		bool IUpdateFromShipment.IsAir
		{
			get { return fIsAir; }
			set { fIsAir = value; }
		}

		ZBool IUpdateFromShipment.IsCoload
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					hAWB.CS_IsMasterHouse = value;
				}
			}
		}

		ZString IUpdateFromShipment.Origin
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					hAWB.CS_RL_NKOrigin = value;
				}
			}
		}

		ZInt IUpdateFromShipment.OuterPacks
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					hAWB.CS_PiecesManifested = (short)value;
				}
			}
		}

		ZString IUpdateFromShipment.PaymentTerm
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					hAWB.CS_FreightPrepaidCollect = new CMRUtilities().ConvertOldAirCargoPaymentType(value);
				}
			}
		}

		ZString IUpdateFromShipment.UniqueConsignRef
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					hAWB.CS_MessageReference = value;
				}
			}
		}

		ZString IUpdateFromShipment.UnitOfWeight
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					hAWB.CS_WeightUQ = value;
				}
			}
		}

		#endregion
	}
}
