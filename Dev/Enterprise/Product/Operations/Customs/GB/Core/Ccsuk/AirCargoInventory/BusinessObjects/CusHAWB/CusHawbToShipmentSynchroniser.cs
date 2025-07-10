using System;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusHawbToShipmentSynchroniser : IUpdateFromShipment
	{
		public CusHawbToShipmentSynchroniser(CusHAWB hAWB)
		{
			if (hAWB == null)
			{
				throw new ArgumentNullException(nameof(hAWB), "HAWB cannot be null");
			}

			this.hAWB = hAWB;
			fIsAir = true;
		}
		readonly CusHAWB hAWB;

		#region IUpdateFromShipment Members

		ZDecimal IUpdateFromShipment.ActualWeight
		{
			set
			{
				if (!hAWB.ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Mass))
				{
					hAWB.CS_Weight = value;
				}
			}
		}

		#region IUpdateFromShipment UNUSED Members

		ZGuid IUpdateFromShipment.CoLoadMasterShipmentPK
		{
			set { }  // do nothing with this field, it is not needed for CCSUK
		}

		ZString IUpdateFromShipment.CoLoadMasterShipmentHouseBillNumber
		{
			set { }  // do nothing with this field, it is not needed for CCSUK
		}

		void IUpdateFromShipment.CopyConsigneeDetails(CommonShipment shipment)
		{
			// do nothing, it is not needed for CCSUK
		}

		void IUpdateFromShipment.CopyConsignorDetails(CommonShipment shipment)
		{
			// do nothing, it is not needed for CCSUK
		}

		ZBool IUpdateFromShipment.IsCoload
		{
			set
			{
				// Do not set CS_IsMasterHouse
			}
		}

		ZString IUpdateFromShipment.PaymentTerm
		{
			set { }  // do nothing with this field, it is not needed for CCSUK
		}

		ZString IUpdateFromShipment.UniqueConsignRef
		{
			set { }  // do nothing with this field, it is not needed for CCSUK
		}

		#endregion

		ZString IUpdateFromShipment.Destination
		{
			set
			{
				if (!hAWB.ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.AirportOfDestination))
				{
					hAWB.CS_RL_NKDestination = value;
				}
			}
		}

		ZString IUpdateFromShipment.GoodsCurrency
		{
			set { hAWB.CS_RX_NKGoodsCurrency = value; }
		}

		ZString IUpdateFromShipment.GoodsDescription
		{
			set
			{
				if (!hAWB.ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Description))
				{
					hAWB.CS_GoodsDescription = value.Left(hAWB.CS_GoodsDescriptionInfo.MaxLength);
				}
			}
		}

		ZDecimal IUpdateFromShipment.GoodsValue
		{
			set { hAWB.CS_GoodsValue = value; }
		}

		ZString IUpdateFromShipment.HouseBillNumber
		{
			set
			{
				if (!hAWB.IsNonStandardHawbNumberSoDoNotSynch && !hAWB.ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.AwbNumber))
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

		ZString IUpdateFromShipment.Origin
		{
			set
			{
				if (!hAWB.ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.AirportOfOrigin))
				{
					hAWB.CS_RL_NKOrigin = value;
				}
			}
		}

		ZInt IUpdateFromShipment.OuterPacks
		{
			set
			{
				if (!hAWB.ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Npx))
				{
					hAWB.CS_PiecesManifested = (short)value;
				}
			}
		}

		ZString IUpdateFromShipment.UnitOfWeight
		{
			set
			{
				if (!hAWB.ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Mass))
				{
					hAWB.CS_WeightUQ = value;
				}
			}
		}

		#endregion

		internal void SynchroniseFromShipment(ForwardingShipment shipment)
		{
			var updater = (IUpdateFromShipment)this;
			updater.HouseBillNumber = shipment.JS_HouseBill;
			updater.GoodsDescription = shipment.JS_GoodsDescription;
			updater.ActualWeight = shipment.JS_ActualWeight;
			updater.UnitOfWeight = shipment.JS_UnitOfWeight;
			updater.OuterPacks = shipment.JS_OuterPacks;
			updater.GoodsValue = shipment.JS_GoodsValue;
			updater.GoodsCurrency = shipment.JS_RX_NKGoodsValueCurr;
			shipment.Logs.AddNew(Events.StatusUpdated, "Synchronised to CCSUK HAWB " + hAWB.ReferenceNumber + " " + hAWB.CS_WarehouseLocation, ZDateTimeOffset.Now);
		}
	}
}

