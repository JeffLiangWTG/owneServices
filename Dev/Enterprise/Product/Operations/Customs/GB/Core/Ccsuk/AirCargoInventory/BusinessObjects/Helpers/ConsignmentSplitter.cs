using System.Collections;
using System.Linq;
using Enterprise.Core;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class ConsignmentSplitter
	{
		public ConsignmentSplitter(ICcsukCusAwb awb)
		{
			this.awb = awb;
		}

		// Recycle splits where possible - otherwise we cannot compare old vs new for amendment detection (for FRC) when managing splits
		internal void Split(IEnumerable splitLines)
		{
			foreach (var splitLine in splitLines)
			{
				var split = splitLine as ISplitLine;
				if (split != null)
				{
					var cusPartShip = awb.Splits[split.LineOrSplitNumber];
					if (split.NumberOfPiecesExpected > 0)
					{
						if (cusPartShip == null)
						{
							cusPartShip = awb.Splits.AddNew();
							cusPartShip.SplitReference = split.LineOrSplitNumber;
							cusPartShip.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
						}
						cusPartShip.NumberOfPiecesExpected = split.NumberOfPiecesExpected;
						if (awb.NumberOfPiecesExpected > 0 && awb.NumberOfPiecesExpected == awb.NumberOfPiecesReceived)
						{
							cusPartShip.NumberOfPiecesReceived = split.NumberOfPiecesExpected;
						}
						cusPartShip.Weight = split.Weight;
						cusPartShip.WeightCode = Constants.Weight.Kilograms;
						cusPartShip.HandlingInformation = split.DescriptionOfGoods;
					}
					else
					{
						// NPX = 0 means get rid of this split
						if (cusPartShip != null)
						{
							awb.Splits.RemoveAndDelete(cusPartShip);
						}
					}
				}
			}

			// Now kill CusPartShips that do not have a corresponding NPBO (should be none)
			foreach (SplitConsignment sc in awb.Splits.ToArray())
			{
				var hasCorrespondingNpbo = (from ISplitLine npbo in splitLines where npbo.LineOrSplitNumber == sc.SplitReference select npbo).Any();
				if (!hasCorrespondingNpbo)
				{
					awb.Splits.RemoveAndDelete(sc);
				}
			}

			if (awb.Splits.Count == 1)
			{
				awb.Splits.RemoveAndDeleteAll();  // One split with nonzero NoP means delete the split and make whole again
			}
		}

		readonly ICcsukCusAwb awb;
	}
}
