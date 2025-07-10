using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondSEAOUTManager : CusUnderbondOutturnManager
	{
		public CusUnderbondSEAOUTManager(CusOutturnHeader header) : base(null, ZString.Empty)
		{
			outturnHeader = header;
		}
		readonly CusOutturnHeader outturnHeader;

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo)
		{
			var result = new List<CMRMessageBuilder>();
			var outturnHdr = bizo as CusOutturnHeader;

			if (outturnHdr != null)
			{
				outturnHdr.ResetRejectedOutturnMessageStatus();
				if (SendingAmendment && outturnHdr.HasChanges)
				{
					outturnHdr.Factory.Save();
				}
			}

			ISeaOutturnReportHeaderInformation headerInfo = outturnHdr != null ? outturnHdr : new SeaOutturnStandAloneReportHeader(outturnHdr);
			ISeaOutturnReportLineInformation[] lines = (ISeaOutturnReportLineInformation[])headerInfo.Lines;
			ISeaOutturnReportLineInformation[] linesForMessage = lines;
			var maximumMessageLines = AUCustomsDataRegistry.Instance.MaximumSeaOutturnLinesPerMessage.Value;

			if (SendingAmendment)
			{
				maximumMessageLines = maximumMessageLines / 2;
				ISeaOutturnReportLineInformation[] previouslySentLines = (ISeaOutturnReportLineInformation[])GetLineBuilders(headerInfo.MessageLines);
				lines = linesForMessage = (ISeaOutturnReportLineInformation[])GetAmendedLines(lines, previouslySentLines);
			}

			var splitMessage = linesForMessage.Length > maximumMessageLines;
			if (splitMessage && (CanSendOriginal || SendingAmendment))
			{
				var splitLinesForMessage = new ISeaOutturnReportLineInformation[maximumMessageLines];
				for (int i = 0; i < maximumMessageLines; i++)
				{
					splitLinesForMessage.SetValue(linesForMessage[i], i);
				}

				var splitMessageBuilder = new SEAOUTMessageBuilder(headerInfo, splitLinesForMessage, true, false);
				splitMessageBuilder.SetSplitMessageIdentifier = true;
				result.Add(splitMessageBuilder);
			}
			else
			{
				result.Add(new SEAOUTMessageBuilder(headerInfo, lines, false, false));
			}

			if (splitMessage && (CanSendOriginal || SendingAmendment))
			{
				var linesExist = true;
				var additionalMessages = 0;
				while (linesExist)
				{
					additionalMessages++;
					var nextLineToReportIndex = additionalMessages * maximumMessageLines;
					var linesRemainingToReport = linesForMessage.Length - nextLineToReportIndex;

					if (linesRemainingToReport > 0)
					{
						var linesThisAdditionalMessage = linesRemainingToReport < maximumMessageLines ? linesRemainingToReport : maximumMessageLines;
						var linesForSubsequentSplitMessages = new ISeaOutturnReportLineInformation[linesThisAdditionalMessage];
						var currentLineInformationIndex = 0;
						for (int i = nextLineToReportIndex; i < (nextLineToReportIndex + linesThisAdditionalMessage); i++)
						{
							linesForSubsequentSplitMessages.SetValue(linesForMessage[i], currentLineInformationIndex);
							currentLineInformationIndex++;
						}

						if (lines.Length > 0)
						{
							var splitMessageBuilder = new SEAOUTMessageBuilder(headerInfo, linesForSubsequentSplitMessages, true, true);
							splitMessageBuilder.SetStatusToPending = true;
							splitMessageBuilder.SetSplitMessageIdentifier = true;
							result.Add(splitMessageBuilder);
						}

						if (linesForSubsequentSplitMessages.Length < maximumMessageLines)
						{
							linesExist = false;
						}
					}
					else
					{
						linesExist = false;
					}
				}
			}

			return result.ToArray();
		}

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => new CusOutturnHeaderSEAOUTAmendmentGenerator(bizo as CusOutturnHeader);

		public override bool CanSendOriginal => base.CanSendOriginal && LinesToSend > 0;

		public override bool ShouldSendOriginalOnSave => CanSendOriginal;

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { outturnHeader.Calculator };

		internal override string GetStatus() => outturnHeader.OutturnStatus.Code;

		public override BusinessObject BusinessObject => outturnHeader;

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			SendingAmendment = true;
			var result = new List<EDIMessage>();
			CMRMessageBuilder[] builders = GetBuilder(bizo);
			foreach (CMRMessageBuilder builder in builders)
			{
				builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
				builder.Messages = GetMessages(bizo);
				result.Add(builder.PopulateMessagesReturningResult());
			}

			return result.ToArray();
		}

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => (bizo as CusOutturnHeader).Messages;

		public override string MessageFriendlyName => "Outturn Report for: " + outturnHeader.C6_SendersMessageReference;

		public override bool CanSendWithdrawal
		{
			get
			{
				if (outturnHeader.HasSplitMessageFailedLog || outturnHeader.HasSplitMessageOriginalRejectedLog)
				{
					return true;
				}
				else
				{
					return base.CanSendWithdrawal;
				}
			}
		}

		protected override Customs.Business.MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			Customs.Business.MessageSendingNotificationCollection result = base.GetCommonNotificationsForSending();
			if (outturnHeader.Outturns.Count < 1)
			{
				result.AddError("There must be at least one outturn.");
			}

			return result;
		}

		protected override void ResetToOriginalCore()
		{
			base.ResetToOriginalCore();
			foreach (DepotCusOutturn outturn in outturnHeader.Outturns)
			{
				var underbond = outturn.Factory.Load<CusUnderbond>(outturn.C5_C4_Underbond);
				if (underbond != null)
				{
					underbond.C4_Outurned = ZDate.Empty;    // Outurn has been reset to original - reset Outurned dates
				}
			}
		}

		int LinesToSend => GetHeaderInformation(outturnHeader).Lines.Count();

		DepotCusOutturnHeaderOutturnReportHeaderInformation GetHeaderInformation(CusOutturnHeader header) => new DepotCusOutturnHeaderOutturnReportHeaderInformation(header);

		/// <summary>
		/// Determine if a line needs to be inserterd, amended or deleted & add to 'AmendedLines' report line collection
		/// </summary>
		/// <param name="Lines"></param>
		/// <param name="OldLines"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public IEnumerable<ISeaOutturnReportLineInformation> GetAmendedLines(ISeaOutturnReportLineInformation[] lines, ISeaOutturnReportLineInformation[] oldLines)
		{
			var result = new ArrayList();
			Array.Sort(lines, LineComparer);
			Array.Sort(oldLines, LineComparer);

			var linesHashtable = new Hashtable();
			foreach (ISeaOutturnReportLineInformation line in lines)
			{
				var uniqueIdentifier = line.ContainerNumber + line.OceanBillOfLading + line.HouseBillOfLading;
				if (!linesHashtable.Contains(uniqueIdentifier))
				{
					linesHashtable.Add(uniqueIdentifier, line);
				}
			}

			var oldLinesHashtable = new Hashtable();
			foreach (ISeaOutturnReportLineInformation reportLine in oldLines)
			{
				string lineIdentifier = reportLine.ContainerNumber + reportLine.OceanBillOfLading + reportLine.HouseBillOfLading;
				if (!oldLinesHashtable.ContainsKey(lineIdentifier))
				{
					oldLinesHashtable.Add(lineIdentifier, reportLine);
				}

				if (!linesHashtable.ContainsKey(lineIdentifier))
				{
					result.Add(reportLine);
				}
				else if (linesHashtable.ContainsKey(lineIdentifier))
				{
					ISeaOutturnReportLineInformation newLine = (ISeaOutturnReportLineInformation)linesHashtable[lineIdentifier];
					if (reportLine.OutturnStatus == CMRMessage.CMRMessageStatusDescription.AMENDMENTDETECTED ||
						reportLine.OutturnStatus == "" ||
						newLine.LastMessageDate.IsEmpty)
					{
						result.Add(newLine);
					}
				}
			}

			foreach (ISeaOutturnReportLineInformation line in lines)
			{
				var uniqueIdentifier = line.ContainerNumber + line.OceanBillOfLading + line.HouseBillOfLading;
				if (!oldLinesHashtable.ContainsKey(uniqueIdentifier))
				{
					result.Add(line);
				}
			}

			return (IEnumerable<ISeaOutturnReportLineInformation>)result.ToArray(typeof(ISeaOutturnReportLineInformation));
		}

		UniqueIdentifierMessageLineComparer LineComparer
		{
			get
			{
				if (fLineComparer == null)
				{
					fLineComparer = new UniqueIdentifierMessageLineComparer();
				}

				return fLineComparer;
			}
		}
		UniqueIdentifierMessageLineComparer fLineComparer;

		IEnumerable<ISeaOutturnReportLineInformation> GetLineBuilders(IEnumerable<ISeaOutturnReportLineInformation> reportLines)
		{
			var result = new ArrayList();
			foreach (ISeaOutturnReportLineInformation reportLine in reportLines)
			{
				result.Add(reportLine);
			}

			return (IEnumerable<ISeaOutturnReportLineInformation>)result.ToArray(typeof(ISeaOutturnReportLineInformation));
		}
	}
}
