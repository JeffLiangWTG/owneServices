using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondAIROUTManager : CusUnderbondOutturnManager
	{
		public CusUnderbondAIROUTManager(CusUnderbond underbond, ZString ownerCompanyABN) : base(underbond, ownerCompanyABN)
		{
		}

		public CusUnderbondAIROUTManager(CusUnderbond underbond, bool shouldDelaySending = false) : this(underbond, ZString.Empty)
		{
			this.shouldDelaySending = shouldDelaySending;
		}
		readonly bool shouldDelaySending;

		#region Concrete Stuff

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo)
		{
			var result = new List<CMRMessageBuilder>();
			var underbond = bizo as CusUnderbond;

			if (SendingAmendment && (underbond.HasSplitMessageFailedLog || underbond.HasSplitMessageOriginalRejectedLog))
			{
				// do not generate an amendment message - underbond is in conflict & must be withdrawn first
			}
			else
			{
				if (underbond != null)
				{
					underbond.ResetRejectedOutturnMessageStatus();
				}

				IAirOutturnReportHeaderInformationProvider parent = underbond.LinkedObject as IAirOutturnReportHeaderInformationProvider;
				IAirOutturnReportHeaderInformation headerInfo = parent != null ? parent.GetHeader(underbond) : new AirOutturnStandAloneReportHeader(underbond);
				IAirOutturnReportLineInformation[] lines = headerInfo.Lines;
				IAirOutturnReportLineInformation[] linesForMessage = headerInfo.Lines;
				var maximumMessageLines = AUCustomsDataRegistry.Instance.MaximumAirOutturnLinesPerMessage.Value;

				if (SendingAmendment)
				{
					lines = linesForMessage = GetAmendedLines(GetLineBuilders(headerInfo.Lines), GetLineBuilders(headerInfo.DatabaseLines));
				}

				var splitMessage = linesForMessage.Length > maximumMessageLines;
				if (splitMessage && (CanSendOriginal || SendingAmendment))
				{
					lines = new IAirOutturnReportLineInformation[maximumMessageLines];
					for (int i = 0; i < maximumMessageLines; i++)
					{
						lines.SetValue(linesForMessage[i], i);
					}

					var splitMessageBuilder = new AIROUTMessageBuilder(headerInfo, lines, true, false, shouldDelaySending);
					splitMessageBuilder.SetSplitMessageIdentifier = true;
					result.Add(splitMessageBuilder);
				}
				else
				{
					result.Add(new AIROUTMessageBuilder(headerInfo, lines, false, false, shouldDelaySending));
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
							lines = new IAirOutturnReportLineInformation[linesThisAdditionalMessage];
							var currentLineInformationIndex = 0;
							for (int i = nextLineToReportIndex; i < (nextLineToReportIndex + linesThisAdditionalMessage); i++)
							{
								lines.SetValue(linesForMessage[i], currentLineInformationIndex);
								currentLineInformationIndex++;
							}

							if (lines.Length > 0)
							{
								var splitMessageBuilder = new AIROUTMessageBuilder(headerInfo, lines, true, true, shouldDelaySending);
								splitMessageBuilder.SetStatusToPending = true;
								splitMessageBuilder.SetSplitMessageIdentifier = true;
								result.Add(splitMessageBuilder);
							}

							if (lines.Length < maximumMessageLines)
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
			}

			return result.ToArray();
		}

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => new CusUnderbondAIROUTAmendmentGenerator(bizo as CusUnderbond);

		#endregion

		#region Overrides

		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			if (HasSplitMessageFailedLog(bizo))
			{
				// do not generate a message - underbond has previously been split into multiple messages but is in conflict with customs due to errors & must be withdrawn first
				return Array.Empty<EDIMessage>();
			}
			else
			{
				return base.GenerateOriginalMessagesCore(bizo);
			}
		}

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			SendingAmendment = true;
			var result = new List<EDIMessage>();
			if (HasSplitMessageFailedLog(bizo))
			{
				// do not generate an amendment message - underbond is in conflict & must be withdrawn first
			}
			else
			{
				CMRMessageBuilder[] builders = GetBuilder(bizo);
				foreach (CMRMessageBuilder builder in builders)
				{
					builder.MessageSubType = Customs.Common.MessageBuilders.MessageSubTypes.Change;
					builder.Messages = GetMessages(bizo);
					result.Add(builder.PopulateMessagesReturningResult());
				}
			}

			return result.ToArray();
		}

		public override bool RequiresAmendment()
		{
			return !Underbond.HasSplitMessageFailedLog && base.RequiresAmendment();
		}

		public override bool CanSendOriginal
		{
			get
			{
				return !Underbond.HasSplitMessageFailedLog && base.CanSendOriginal;
			}
		}

		public override bool CanSendWithdrawal
		{
			get
			{
				if (Underbond.HasSplitMessageFailedLog || Underbond.HasSplitMessageOriginalRejectedLog)
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
			if (Underbond.Outturns.Count < 1)
			{
				result.AddError("There must be at least one outturn.");
			}

			return result;
		}

		#endregion

		#region Implementation

		/// <summary>
		/// Determine if a line needs to be inserterd, amended or deleted & add to 'AmendedLines' report line collection
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="oldLines"></param>
		/// <returns></returns>
		public IAirOutturnReportLineInformation[] GetAmendedLines(IAirOutturnReportLineInformation[] lines, IAirOutturnReportLineInformation[] oldLines)
		{
			var result = new ArrayList();
			Array.Sort(lines, LineComparer);
			Array.Sort(oldLines, LineComparer);

			Hashtable linesHashtable = new Hashtable();
			foreach (IAirOutturnReportLineInformation line in lines)
			{
				var uniqueIdentifier = line.MasterAirWaybillNumber + line.HouseAirWaybillNumber;
				if (!linesHashtable.Contains(uniqueIdentifier))
				{
					linesHashtable.Add(uniqueIdentifier, line);
				}
			}

			Hashtable oldLinesHashtable = new Hashtable();
			foreach (IAirOutturnReportLineInformation line in oldLines)
			{
				string lineIdentifier = line.MasterAirWaybillNumber + line.HouseAirWaybillNumber;
				if (!oldLinesHashtable.ContainsKey(lineIdentifier))
				{
					oldLinesHashtable.Add(lineIdentifier, line);
				}

				if (!linesHashtable.ContainsKey(lineIdentifier))
				{
					result.Add(line);
				}
				else if (linesHashtable.ContainsKey(lineIdentifier))
				{
					IAirOutturnReportLineInformation newLine = (IAirOutturnReportLineInformation)linesHashtable[lineIdentifier];
					if (line.DamageIndicator != newLine.DamageIndicator ||
						line.GoodsDescription != newLine.GoodsDescription ||
						line.NumberOfPackages != newLine.NumberOfPackages ||
						line.OutturnResultType != newLine.OutturnResultType ||
						line.PillageIndicator != newLine.PillageIndicator ||
						newLine.LastMessageDate.IsEmpty)
					{
						result.Add(newLine);
					}
				}
			}

			foreach (IAirOutturnReportLineInformation line in lines)
			{
				var uniqueIdentifier = line.MasterAirWaybillNumber + line.HouseAirWaybillNumber;
				if (!oldLinesHashtable.ContainsKey(uniqueIdentifier))
				{
					result.Add(line);
				}
			}

			return (IAirOutturnReportLineInformation[])result.ToArray(typeof(IAirOutturnReportLineInformation));
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

		IAirOutturnReportLineInformation[] GetLineBuilders(IAirOutturnReportLineInformation[] reportLines)
		{
			var result = new ArrayList();

			foreach (IAirOutturnReportLineInformation reportLine in reportLines)
			{
				result.Add(reportLine);
			}
			return (IAirOutturnReportLineInformation[])result.ToArray(typeof(IAirOutturnReportLineInformation));
		}

		#endregion
	}
}
