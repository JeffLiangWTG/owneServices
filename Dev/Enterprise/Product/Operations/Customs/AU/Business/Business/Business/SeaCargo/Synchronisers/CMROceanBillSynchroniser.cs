using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for CMROceanBillSynchroniser.
	/// </summary>
	public class CMROceanBillSynchroniser : OceanBillSynchroniser
	{
		public CMROceanBillSynchroniser(CusSCAOceanBill destination, CommonConsol source, CMRSeaCargoSynchroniser seaCargoSynchroniser)
			: base(destination, source)
		{
			this.OceanBill = destination;
			this.Consol = source;
			this.seaCargoSynchroniser = seaCargoSynchroniser;
		}

		public readonly CusSCAOceanBill OceanBill;
		public readonly CommonConsol Consol;
		readonly CMRSeaCargoSynchroniser seaCargoSynchroniser;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(OceanBill.CB_OH_ShippingLineInfo, () => Consol.ShippingLinePK, () => new[] { Consol.JK_OA_ShippingLineAddressInfo }, true));
			HookCollectionSynchronisers();
		}

		void HookCollectionSynchronisers()
		{
			Source.Shipments.CountChanged -= new CollectionCountChangedEventHandler(Shipments_CountChanged);
			Source.Shipments.CountChanged += new CollectionCountChangedEventHandler(Shipments_CountChanged);
		}

		void UnHookCollectionSynchronisers()
		{
			Source.Shipments.CountChanged -= new CollectionCountChangedEventHandler(Shipments_CountChanged);
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			UnHookCollectionSynchronisers();
		}

		void Shipments_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				var addedShipment = e.BizObject as CommonShipment;
				if (addedShipment != null)
				{
					seaCargoSynchroniser.GetHouseBill(addedShipment);
				}
			}
		}

		protected override ContainerSynchroniser GetNewContainerSynchroniser(CusSCAContainer sCAContainer, CommonContainer jobContainer, ForwardingConsol parent)
		{
			return new CMRContainerSynchroniser(sCAContainer, jobContainer, parent);
		}
	}
}
