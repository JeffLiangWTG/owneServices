using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	class NctsPhase5DepartureConsolShipmentToBillSynchroniser : NctsPhase5DepartureShipmentToBillSynchroniser
	{
		readonly ForwardingConsol forwardingConsol;
		readonly bool hasCommonConsignor;
		readonly bool hasCommonConsignee;
		readonly NctsHeader nctsHeader;

		public NctsPhase5DepartureConsolShipmentToBillSynchroniser(NctsBill destination, ForwardingShipment source, ForwardingConsol sourceConsol)
			: base(destination, source)
		{
			forwardingConsol = sourceConsol;

			hasCommonConsignor = forwardingConsol.Shipments.Cast<ForwardingShipment>().AllSame(x => x.Consignor?.PK ?? ZGuid.Empty);
			hasCommonConsignee = forwardingConsol.Shipments.Cast<ForwardingShipment>().AllSame(x => x.Consignee?.PK ?? ZGuid.Empty);
			nctsHeader = destination.Header;
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.B0_ReferenceIDInfo, Source.JS_UniqueConsignRefInfo));
			AddConsignorDocumentaryAddressSynchroniser();
			AddConsigneeDocumentaryAddressSynchroniser();
		}

		protected virtual void AddConsignorDocumentaryAddressSynchroniser()
		{
			if (!nctsHeader.DefaultConsignorConsigneeRegistryManager.IsRegistryEnabled())
			{
				Synchronisers.Add(new JobDocAddressSynchroniser(GetDestinationConsignor(), Source.ConsignorDocumentaryAddress));
			}
			else if (nctsHeader.DefaultConsignorConsigneeRegistryManager.IsConsignor() && Source.JobDirection == Directions.Export)
			{
				Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(GetDestinationConsignor(), (ZPropertyInfoGuid)Source.JS_OA_ExportReceivingDepotInfo));
			}
		}

		protected virtual void AddConsigneeDocumentaryAddressSynchroniser()
		{
			if (!nctsHeader.DefaultConsignorConsigneeRegistryManager.IsRegistryEnabled())
			{
				Synchronisers.Add(new JobDocAddressSynchroniser(GetDestinationConsignee(), Source.ConsigneeDocumentaryAddress));
			}
			else if (nctsHeader.DefaultConsignorConsigneeRegistryManager.IsConsignee() && Source.JobDirection == Directions.Import)
			{
				Synchronisers.Add(new JobDocAddressSynchroniser(GetDestinationConsignee(), Source.ConsigneeDeliveryAddress));
			}
		}

		JobDocAddress GetDestinationConsignor() => hasCommonConsignor ? nctsHeader.Consignor : Destination.Consignor;

		JobDocAddress GetDestinationConsignee() => hasCommonConsignee ? nctsHeader.Consignee : Destination.Consignee;

		protected override ISynchroniser GetNewNctsDepartureGoodsItemSynchroniser(NctsDepartureCargoDesc goodsItem, ForwardingPackLine distinctSourcePackage)
		{
			var hasCommonCountryOfDispatch = forwardingConsol.Shipments.Cast<ForwardingShipment>().AllSame(x => x.JS_RL_NKOrigin.Left(2));
			var hasCommonCountryOfDestination = forwardingConsol.Shipments.Cast<ForwardingShipment>().AllSame(x => x.JS_RL_NKDestination.Left(2));
			var hasCommonCTStatus = forwardingConsol.Shipments.Cast<ForwardingShipment>().AllSame(x => x.JS_CommunityTransitStatus);

			return new NctsPhase5DepartureShipmentToGoodsItemSynchroniser(goodsItem, Source, distinctSourcePackage, hasCommonCountryOfDispatch, hasCommonCountryOfDestination, hasCommonCTStatus, true);
		}
	}
}
