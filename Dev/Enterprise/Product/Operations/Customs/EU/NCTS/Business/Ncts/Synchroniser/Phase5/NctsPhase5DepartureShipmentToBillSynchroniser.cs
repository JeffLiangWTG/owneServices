#if NETFRAMEWORK
using CargoWise.Common;
#endif
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPhase5DepartureShipmentToBillSynchroniser : BusinessObjectSynchroniser
	{
		public NctsPhase5DepartureShipmentToBillSynchroniser(NctsBill destination, ForwardingShipment source)
			: base(destination, source)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			Synchronisers.Add(new FieldSynchroniser(Destination.B0_ReferenceIDInfo, Source.JS_HouseBillInfo));

			if (!Destination.IsInPhase5TransitionPeriod)
			{
				AddTransportDocumentToSync();
			}

			AddShipmentToLineItemSynch();
		}

		void AddTransportDocumentToSync()
		{
			var transportDocuments = Destination.AdditionalDocuments.Cast<NctsBillAdditionalDocument>().Where(x => x.CSI_Type.EqualsIgnoringCase(CusSupportingInfoTypeList.Codes.AdditionalInfo) && x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.TransportDocument) && x.CSI_Code.EqualsIgnoringCase(NctsConstants.AdditionalInfoCodes.HouseBillOfLading)).ToArray();
			transportDocuments.Skip(1).DeleteAll();
			var lineSynch = GetNewNctsPhase5DepartureShipmentToTransportDocumentSynchroniser(transportDocuments.FirstOrDefault() ?? Destination.AdditionalDocuments.AddNew());
			Synchronisers.Add(lineSynch);
		}

		protected virtual ISynchroniser GetNewNctsPhase5DepartureShipmentToTransportDocumentSynchroniser(NctsBillAdditionalDocument transportDocument) => new NctsPhase5DepartureShipmentToTransportDocumentSynchroniser(transportDocument, Source);

		protected virtual void AddShipmentToLineItemSynch()
		{
			var goodsItems = Destination.GoodsItems.Cast<NctsDepartureCargoDesc>().ToArray();

			var distinctPackages = Source.OuterPackLines.Cast<ForwardingPackLine>().Where(x => !x.JL_HarmonisedCode.IsEmpty && !x.JL_RN_NKOrigin.IsEmpty && !x.JL_Description.IsEmpty).DistinctBy(x => new { x.JL_HarmonisedCode, x.JL_RN_NKOrigin, x.JL_Description }).ToList();
			var distinctPackagesWithEmptyValues = Source.OuterPackLines.Cast<ForwardingPackLine>().Where(x => x.JL_HarmonisedCode.IsEmpty || x.JL_RN_NKOrigin.IsEmpty || x.JL_Description.IsEmpty).ToList();
			distinctPackages.AddRange(distinctPackagesWithEmptyValues);
			if (distinctPackages.Any())
			{
				goodsItems.Skip(distinctPackages.Count).DeleteAll();
				var count = 0;
				foreach (var distinctPackage in distinctPackages)
				{
					var lineItem = goodsItems.Length > 0 && count < goodsItems.Length ? goodsItems[count] : Destination.GoodsItems.AddNew();
					var lineSynch = GetNewNctsDepartureGoodsItemSynchroniser(lineItem, distinctPackage);
					Synchronisers.Add(lineSynch);
					count++;
				}
			}
			else
			{
				var lineItem = goodsItems.FirstOrDefault();
				goodsItems.Skip(1).DeleteAll();

				var lineSynch = GetNewNctsDepartureGoodsItemSynchroniser(lineItem ?? Destination.GoodsItems.AddNew(), null);
				Synchronisers.Add(lineSynch);
			}
		}

		protected virtual ISynchroniser GetNewNctsDepartureGoodsItemSynchroniser(NctsDepartureCargoDesc goodsItem, ForwardingPackLine distinctSourcePackage) => new NctsPhase5DepartureShipmentToGoodsItemSynchroniser(goodsItem, Source, distinctSourcePackage, true, true, true, false);

		internal new NctsBill Destination => (NctsBill)base.Destination;

		internal new ForwardingShipment Source => (ForwardingShipment)base.Source;
	}
}
