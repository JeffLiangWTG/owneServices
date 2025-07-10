using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHItemCollectionSynchroniser : GenericCollectionSynchroniser<CusCAeMHItemSynchroniser>
	{
		readonly ForwardingShipment sourceShipment;

		public CusCAeMHItemCollectionSynchroniser(ForwardingShipment shipment, CusCAeMHHouse houseBill)
			: base(shipment, houseBill, shipment.MergeOuterPackLinesIntoInnerPackLines(), houseBill.Items)
		{
			sourceShipment = shipment;
		}

		#region Implementation

		protected override IEnumerable<BusinessObject> GetSourceBusinessObjects()
		{
			return sourceShipment.MergeOuterPackLinesIntoInnerPackLines();
		}

		protected override bool CompareBizosEqual(BusinessObject source, BusinessObject destination)
		{
			if (destination is CusCAeMHItem item && source is PackLine packLine)
			{
				var quantityAndUQ = packLine.GetEffectivePackLineQuantity();
				var linkedOuterPackLine = sourceShipment.GetLinkedOuterPackLineOrItself(packLine);
				return item.BX_Quantity == quantityAndUQ.Qty &&
					item.BX_QuantityUQ == D96AMessageUtilities.ConvertPackUnitToACROSSUnit(quantityAndUQ.UQ) &&
					item.BX_Description == CusSCAPivotSynchroniser.GetDescriptionFromShipmentPackLine(packLine, linkedOuterPackLine) &&
					item.BX_Marks == CusSCAPivotSynchroniser.GetMarksFromShipmentPackLine(linkedOuterPackLine) &&
					item.BX_HSCode == linkedOuterPackLine.JL_HarmonisedCode &&
					item.UNDGs.SequenceEqual(linkedOuterPackLine.UNDGs, new UNDGDataItemComparer());
			}
			return false;
		}

		protected override CusCAeMHItemSynchroniser CreateNewSynchroniser(BusinessObject destination, BusinessObject source)
		{
			var packLine = (PackLine)source;
			return new CusCAeMHItemSynchroniser((CusCAeMHItem)destination, packLine, sourceShipment.GetLinkedOuterPackLineOrItself(packLine));
		}

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			if (Source is ForwardingShipment sourceShipment)
			{
				if (Convert2BusinessObjectCollection(sourceShipment.OuterPackLines, out var outerBOtCollection))
				{
					yield return outerBOtCollection;
				}
				if (Convert2BusinessObjectCollection(sourceShipment.InnerPackLines, out var innerBOtCollection))
				{
					yield return innerBOtCollection;
				}
			}
		}

		bool Convert2BusinessObjectCollection(IBusinessObjectCollection collection, out BusinessObjectCollection businessObjectCollection)
		{
			businessObjectCollection = collection as BusinessObjectCollection;
			return businessObjectCollection != null;
		}

		#endregion
	}
}
