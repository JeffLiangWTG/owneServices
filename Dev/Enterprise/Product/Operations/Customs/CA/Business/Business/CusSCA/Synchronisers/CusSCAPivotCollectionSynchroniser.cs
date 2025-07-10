namespace Enterprise.Customs.CA.Business
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Business.MessageBuilders;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Forwarding.Business;

	public class CusSCAPivotCollectionSynchroniser : Customs.Business.GenericCollectionSynchroniser<CusSCAPivotSynchroniser>
	{
		readonly ForwardingShipment sourceShipment;

		public CusSCAPivotCollectionSynchroniser(CusSCAHouse destination, ForwardingShipment source)
			: base(source, destination, source.MergeOuterPackLinesIntoInnerPackLines(), destination.PackLines)
		{
			sourceShipment = source;
		}

		protected override IEnumerable<BusinessObject> GetSourceBusinessObjects()
		{
			return sourceShipment.MergeOuterPackLinesIntoInnerPackLines();
		}

		protected override bool CompareBizosEqual(BusinessObject source, BusinessObject destination)
		{
			var packLine = (PackLine)source;
			var pivot = (CusSCAPivot)destination;
			var oceanBill = pivot.OceanBill;
			var consol = oceanBill != null ? oceanBill.Consol : null;
			ZInt quantity = -1;
			var unit = ZString.Empty;

			if (packLine != null)
			{
				(quantity, unit) = packLine.GetEffectivePackLineQuantity();
			}

			if (quantity < 0)
			{
				return false;
			}
			else
			{
				var linkedOuterPackLine = sourceShipment.GetLinkedOuterPackLineOrItself(packLine);
				var sourceContainerNum = packLine.ContainerNumberForConsol(consol);
				if (sourceContainerNum.IsEmpty)
				{
					sourceContainerNum = CusSCAHouse.NonContaineriseID;
				}
				return sourceContainerNum == pivot.CV_AssociatedContainer &&
					quantity == pivot.CV_PackageCount &&
					D96AMessageUtilities.ConvertPackUnitToACROSSUnit(unit) == pivot.CV_PackageType &&
					packLine.JL_ActualWeight == pivot.CV_Weight &&
					packLine.JL_ActualWeightUQ == pivot.CV_WeightUQ &&
					packLine.JL_ActualVolume == pivot.CV_Volume &&
					packLine.JL_ActualVolumeUQ == pivot.CV_VolumeUQ &&
					linkedOuterPackLine.JL_HarmonisedCode == pivot.CV_HarmonisedTariffNums &&
					CusSCAPivotSynchroniser.GetDescriptionFromShipmentPackLine(packLine, linkedOuterPackLine) == pivot.CV_GoodsDescription &&
					CusSCAPivotSynchroniser.GetMarksFromShipmentPackLine(linkedOuterPackLine) == pivot.CV_MarksAndNumbers &&
					linkedOuterPackLine.UNDGs.SequenceEqual(pivot.UNDGs, new UNDGDataItemComparer());
			}
		}

		protected override CusSCAPivotSynchroniser CreateNewSynchroniser(BusinessObject destination, BusinessObject source)
		{
			var packLine = (PackLine)source;
			return new CusSCAPivotSynchroniser((CusSCAPivot)destination, packLine, sourceShipment.GetLinkedOuterPackLineOrItself(packLine));
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
	}
}
