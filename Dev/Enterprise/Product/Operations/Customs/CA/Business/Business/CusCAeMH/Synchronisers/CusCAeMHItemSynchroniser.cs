using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHItemSynchroniser : BusinessObjectSynchroniser
	{
		readonly PackLine linkedOuterPackLine;

		public CusCAeMHItemSynchroniser(CusCAeMHItem destination, PackLine source)
			: base(destination, source)
		{
			linkedOuterPackLine = source;
		}

		public CusCAeMHItemSynchroniser(CusCAeMHItem destination, PackLine source, PackLine linkedOuterPackLine) : base(destination, source)
		{
			this.linkedOuterPackLine = Argument.NotNull(linkedOuterPackLine, nameof(linkedOuterPackLine));
		}

		protected override void HookSynchronisers()
		{
			HookSynchronisersForPackLine();
		}

		void HookSynchronisersForPackLine()
		{
			if (Source is PackLine source && !source.IsDeleted && Destination is CusCAeMHItem destination && !destination.IsDeleted)
			{
				var shipment = source.Shipment;
				Synchronisers.Add(new FieldSynchroniser(destination.BX_QuantityInfo, GetCount, () => new[] { source.JL_PackageCountInfo, shipment.JS_TotalPackageCountInfo }));
				Synchronisers.Add(new FieldSynchroniser(destination.BX_QuantityUQInfo, GetUnit, () => new[] { source.JL_F3_NKPackTypeInfo, shipment.JS_F3_NKTotalCountPackTypeInfo }));
				Synchronisers.Add(new FieldSynchroniser(destination.BX_HSCodeInfo, linkedOuterPackLine.JL_HarmonisedCodeInfo));
				Synchronisers.Add(new FieldSynchroniser(destination.BX_DescriptionInfo, () => CusSCAPivotSynchroniser.GetDescriptionFromShipmentPackLine(source, linkedOuterPackLine),
					() => new[] { source.JL_DescriptionInfo, source.JL_DetailedDescriptionInfo, linkedOuterPackLine.JL_DescriptionInfo, shipment.JS_GoodsDescriptionInfo, shipment.DetailedGoodsDescriptionNoteTextInfo }));
				Synchronisers.Add(new FieldSynchroniser(destination.BX_MarksInfo, () => CusSCAPivotSynchroniser.GetMarksFromShipmentPackLine(linkedOuterPackLine),
					() => new[] { linkedOuterPackLine.JL_MarksAndNumbersInfo, shipment.JS_MarksAndNumbersInfo }));
				Synchronisers.Add(new UNDGCollectionSynchroniser(linkedOuterPackLine, destination));
			}
		}

		IZType GetCount()
		{
			if (Source is PackLine packLine)
			{
				var data = packLine.GetEffectivePackLineQuantity();
				return (ZDecimal)(data.Qty);
			}
			return ZDecimal.Zero;
		}

		IZType GetUnit()
		{
			var result = ZString.Empty;
			if (Source is PackLine packLine)
			{
				var data = packLine.GetEffectivePackLineQuantity();
				result = data.UQ;
			}
			return D96AMessageUtilities.ConvertPackUnitToACROSSUnit(result);
		}
	}
}
