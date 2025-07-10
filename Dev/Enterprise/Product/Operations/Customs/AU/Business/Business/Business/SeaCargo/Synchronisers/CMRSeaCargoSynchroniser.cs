using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// I SYNCHRONISE COLOAD MASTERS RELATED SEA CARGO JOBS
	/// </summary>
	public class CMRSeaCargoSynchroniser : SeaCargoSynchroniser
	{
		public CMRSeaCargoSynchroniser(CommonConsol consol, bool synchroniseConsol = true)
			: base(consol, null, synchroniseConsol)
		{
		}

		public CMRSeaCargoSynchroniser(CommonConsol consol, CusSCAOceanBill oceanBill)
			: base(consol, oceanBill)
		{
		}

		#region Implementation

		protected override Type OceanBillType => typeof(CusSCAOceanBill);

		protected override HouseBillSynchroniser GetNewHouseBillSynchroniser(CommonShipment shipment, CusSCAHouse house)
		{
			return new CMRHouseBillSynchroniser(house, shipment);
		}

		protected override bool ShouldStartHouseSynchronisers(CusSCAHouse house)
		{
			return IsEnabled && house is CusSCAHouse cmrHouse && cmrHouse.CanDelete;
		}

		protected override void DefaultOceanBillDetailsAndChildren()
		{
			if (CanSynchroniseOceanBillDetails())
			{
				base.DefaultOceanBillDetailsAndChildren();
			}

			foreach (CommonShipment shipment in Consol.Shipments)
			{
				var house = GetHouseBill(shipment);
				if (house != null && CanSynchroniseHouseBillDetails(house))
				{
					var synchroniser = GetHouseBillSynchroniser(house);
					if (synchroniser == null)
					{
						synchroniser = GetNewHouseBillSynchroniser(shipment, house);
						BusinessObjectSynchronisers.Add(synchroniser);
						synchroniser.SetEnabled(true, synchroniser.DetectEnabled);
					}

					synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				}
			}
		}

		HouseBillSynchroniser GetHouseBillSynchroniser(CusSCAHouse house)
		{
			return BusinessObjectSynchronisers.OfType<HouseBillSynchroniser>().FirstOrDefault(h => h.Destination == house);
		}

		bool CanSynchroniseOceanBillDetails()
		{
			var result = IsEnabled;
			if (result)
			{
				foreach (CusSCAHouse houseBill in OceanBill.HouseBills)
				{
					if (!CanSynchroniseHouseBillDetails(houseBill))
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		bool CanSynchroniseHouseBillDetails(CusSCAHouse houseBill)
		{
			return IsEnabled && CMRStatusHelper.CanDelete(houseBill.CA_MessageStatus);
		}

		protected override OceanBillSynchroniser GetNewOceanBillSynchroniser(CusSCAOceanBill oceanBill, CommonConsol consol)
		{
			return new CMROceanBillSynchroniser(oceanBill, consol, this);
		}

		protected override void DefaultHouseDetails(CusSCAHouse houseBill, CommonShipment shipment)
		{
			base.DefaultHouseDetails(houseBill, shipment);
			if (shipment.JS_PaymentTerm == "CCX")
			{
				houseBill.CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.Collect;
			}
			else
			{
				houseBill.CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.PrepaidOnly;
			}
		}

		protected override ZString ConvertContainerMode(ZString containerMode)
		{
			ZString result = Enterprise.Core.Constants.ContainerModes.LCL;
			switch (containerMode)
			{
				case Enterprise.Core.Constants.ContainerModes.FCL:
					result = Enterprise.Core.Constants.ContainerModes.FCL;
					break;
				case Enterprise.Core.Constants.ContainerModes.FCLMixedShipper:
				case Enterprise.Core.Constants.ContainerModes.BuyersConsol:
					result = Enterprise.Core.Constants.ContainerModes.FCLMixedShipper;
					break;
			}
			return result;
		}

		#endregion
	}
}
