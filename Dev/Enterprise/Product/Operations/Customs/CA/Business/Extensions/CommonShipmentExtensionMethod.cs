using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.CA.Business
{
	public static class CommonShipmentExtensionMethod
	{
		public static PackLineCollection MergeOuterPackLinesIntoInnerPackLines(this CommonShipment shipment)
		{
			var result = new PackLineCollection(shipment, shipment.Factory);
			var linkedOuterPckLines = new HashSet<ZGuid>();
			foreach (PackLine innerPackLine in shipment.InnerPackLines)
			{
				if (!innerPackLine.JL_JL_OuterPackLine.IsEmpty && innerPackLine.JL_PackageCount > 0)
				{
					result.Add(innerPackLine);
					linkedOuterPckLines.Add(innerPackLine.JL_JL_OuterPackLine);
				}
			}
			foreach (PackLine outerPackLine in shipment.OuterPackLines)
			{
				if (!linkedOuterPckLines.Contains(outerPackLine.PK))
				{
					result.Add(outerPackLine);
				}
			}
			return result;
		}

		public static PackLine GetLinkedOuterPackLineOrItself(this CommonShipment shipment, PackLine packLine)
		{
			return packLine.IsInnerPackType ? shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(f => f.PK == packLine.JL_JL_OuterPackLine) : packLine;
		}
	}
}
