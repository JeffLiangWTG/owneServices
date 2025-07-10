using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public partial class NonPersistentSplitLineOrchestrator
	{
		public ZString PerformLoadFromLastFrd()
		{
			return new NonPersistentSplitLineFromFRDMessageHelper(Awb, SplitsAndFlightData).ParseFrdAndUpdateNpboSplitsCollection();
		}

		static ZShort[] GetDistributionOfNprToSeeIfFrcIsReallyNeeded(ICcsukCusAwb awb)
		{
			var nprs = new List<ZShort>();
			foreach (var s in (from SplitConsignment sc in awb.Splits.ToArray() orderby sc.SplitReference select sc))
			{
				nprs.Add(s.NumberOfPiecesReceived);
			}
			return nprs.ToArray();
		}

		static ContinueWithSave WipeExistingReceiptsAndUpdateWithNewDetailsAndSendFRCs(ICcsukCusAwb awb, NonPersistentSplitLineCollection npSplits, ISendMessagesToCustomsExtraMembersAndDetermineRequiredMessagesAndSendThem guiNotifier, ZShort[] oldDistributionOfNprs)
		{
			// Purge all old receipts
			foreach (var ot in awb.OutTurns)
			{
				ot.Delete();
			}
			// Make new receipts, one for each NPBO
			foreach (NonPersistentSplitLine npSplit in npSplits)
			{
				var newOt = awb.CreateNewOutTurn();
				newOt.SplitReferenceToWhichThisPertains = npSplit.SplitNumber;
				newOt.WarehouseLocationID = npSplit.WarehouseLocationID;
				newOt.C5_PackagesOutturned = npSplit.NumberOfPiecesReceived;
				newOt.C5_MarksAndNumbers = npSplit.HandlingDetail.Left(newOt.C5_MarksAndNumbersInfo.MaxLength);
			}
			// Send any FRC messages (parent and children) if we have jigged existing splits
			var oldParentNpr = awb.NumberOfPiecesReceived;
			awb.CalculateNprFromReceiptsIfNecessary();
			var newDistributionOfNpr = GetDistributionOfNprToSeeIfFrcIsReallyNeeded(awb);
			var result = ContinueWithSave.Yes;
			if (ShouldInvokeAmendmentDetectionForFrcMessages(awb, oldDistributionOfNprs, newDistributionOfNpr, oldParentNpr))  // only need to check for FRCs if NPR has changed - the changes in NPX will be handled by the FCS. CCSUK will automatically REDUCE NPR when you use FCS it reduce NPX, but do they automatically bump up NPR on the other split whose NPX is increating (assumiong no status1)?  If they do, then remove this, so that we always send FRCs. 
			{
				IMessageManager manager = awb.GetMessageManagerForAmendmentDetection();
				result = guiNotifier.DetermineRequiredMessagesAndSendThem(manager);
			}
			return result;
		}

		static bool ShouldInvokeAmendmentDetectionForFrcMessages(ICcsukCusAwb awb, ZShort[] oldDistributionOfNprs, ZShort[] newDistributionOfNpr, ZShort oldParentNpr)
		{
			if (awb.NumberOfPiecesReceived > 0 && awb.NumberOfPiecesReceived != awb.NumberOfPiecesExpected && oldDistributionOfNprs.Length == 0)
			{   // If creating new splits from no splits (oldDistributionOfNprs.Length == 0) and the parent Awb has at least one piece, force amendment detection. 
				return true;
			}
			var shortestLength = System.Math.Min(oldDistributionOfNprs.Length, newDistributionOfNpr.Length);
			for (int i = 0; i < shortestLength; i++)
			{
				var x = oldDistributionOfNprs[i];
				var y = newDistributionOfNpr[i];
				if (x != y)
				{
					return true; // do invoke amendment detection for FRC (which may determine some or no FRCs are needed)
				}
			}
			if (oldParentNpr != awb.NumberOfPiecesReceived)
			{
				return true; // If previously no splits existed and no pieces were received, and now we are splitting and simultaneously checking in ALL pieces, still want to send FRC. 
			}

			return false;  // don't bother checking for amendment - NPR is not changing in a relevant way. If we do check, then we may incorrectly generate unnecessary FRCs based on NPX changing. The FRCs are unnecessary if NPR is not changing - and FCS will handle that anyway. 
		}

		static void GetAggregateWarehouseIdAndMarksNumbers(SplitConsignment split, NonPersistentSplitLine npSplit)
		{
			// Squishes existing receipts into a few rows as possible - merges Marks/Numbers and keeps a single SSL if they were all in the same SSL (otherwise wipes)
			var outturns = ((ICcsukCusAwb)split).OutTurns;
			if ((from ot in outturns group ot by ot.WarehouseLocationID into grp select grp).Count() == 1)
			{
				npSplit.WarehouseLocationID = outturns.First().WarehouseLocationID;
			}
			ZString handlingConsolidated = outturns.Aggregate(new ZStringBuilder(), (ag, ot) => ag.AppendIfNotEmpty(ot.C5_MarksAndNumbers)).ToStringWithDelimiterBetweenAppends(", ");
			npSplit.HandlingDetail = handlingConsolidated.Left(NonPersistentSplitLine.Schema.HandlingDetailMaxLength);
		}

		public ZString PerformRemoveAllSplitsAndManagePiecesForShed(ISendsMessagesToCustoms initiator)
		{
			var reasonCannotDeleteAll = PerformRemoveAllSplitstNoMessage();
			if (reasonCannotDeleteAll.IsEmpty)
			{
				var aggregatedReceipts = from ot in Awb.OutTurns
										 group ot by ot.WarehouseLocationID into grp
										 select new
										 {
											 ShedStorageLocation = grp.Key,
											 TotalPieces = grp.Sum(tp => tp.C5_PackagesOutturned),
											 MarksAndNumbers = grp.Aggregate(new ZStringBuilder(), (ag, ot) => ag.AppendIfNotEmpty(ot.C5_MarksAndNumbers)).ToStringWithDelimiterBetweenAppends(", ")
										 };
				var outturnsToKeep = new List<CusOutTurn>();
				foreach (var aggregate in aggregatedReceipts)
				{
					var realReceipt = (from CusOutTurn otToKeep in Awb.OutTurns where otToKeep.WarehouseLocationID == aggregate.ShedStorageLocation select otToKeep).FirstOrDefault() ?? Awb.CreateNewOutTurn();
					realReceipt.SplitReferenceToWhichThisPertains = "";
					realReceipt.WarehouseLocationID = aggregate.ShedStorageLocation;
					realReceipt.C5_MarksAndNumbers = ((ZString)aggregate.MarksAndNumbers).Left(CusOutturn.Schema.C5_MarksAndNumbersMaxLength);
					realReceipt.C5_PackagesOutturned = aggregate.TotalPieces;
					outturnsToKeep.Add(realReceipt);
				}
				foreach (var outturnToKill in Awb.OutTurns.ToArray())
				{
					if (!outturnsToKeep.Contains(outturnToKill))
					{
						outturnToKill.Delete();
					}
				}

				// Delete local splits based on this:
				new ConsignmentSplitter(Awb).Split(SplitsAndFlightData.SplitLines);
				// Send message to advise community
				SendFcsToSplitOnNetwork(initiator);
				// Query to confirm
				FsaJobUpdater.SendFsaWithUpdateForAwbSilently(Awb);
				return ZString.Empty;  //success
			}
			else
			{
				return reasonCannotDeleteAll;
			}
		}
	}
}
