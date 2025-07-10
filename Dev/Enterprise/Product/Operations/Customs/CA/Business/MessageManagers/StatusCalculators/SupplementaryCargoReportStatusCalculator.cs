using System;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Business.MessageProcessors;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Customs.Common.Shared;
	using Enterprise.Edifact.D00A.Elements;
	using Enterprise.Edifact.D00A.Messages.CUSRES;

	public class SupplementaryCargoReportStatusCalculator : EDIFACTMessageStatusCalculator
	{
		public override ZString MessageTypeDescription
		{
			get { return MessageTypeList.Descriptions.SupplementaryCargoReport; }
		}

		#region Booleans

		public override bool IsClear(ZString currentJobStatus)
		{
			return base.IsClear(currentJobStatus) || currentJobStatus == SupplementaryCargoReportJobStatusList.Codes.RACleared;
		}

		public bool IsInError(ZString currentJobStatus)
		{
			return currentJobStatus == SupplementaryCargoReportJobStatusList.Codes.Error;
		}

		public bool IsRAOutstanding(ZString currentJobStatus)
		{
			var raOutstandingList =
				new List<string>
					{
						SupplementaryCargoReportJobStatusList.Codes.DoNotLoad,
						SupplementaryCargoReportJobStatusList.Codes.DoNotUnload,
						SupplementaryCargoReportJobStatusList.Codes.Hold,
						SupplementaryCargoReportJobStatusList.Codes.Unknown
					};

			return raOutstandingList.Contains(currentJobStatus);
		}

		#endregion

		#region CalculatedJobStatus

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject)
		{
			ZString latestStatus = ZString.Empty;
			bool isValidated = false;
			foreach (EDIMessage inMessage in linkedObject.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.CAACI, Array.Empty<ZString>(), EDIMessage.Direction.Receive, true, ListSortDirection.Descending))
			{
				var cusres = (CUSRESMessage)inMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
				if (cusres != null && cusres.GIS.Count > 0)
				{
					if (cusres.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == ProcessingIndicatorDescriptionCodeList.MessageContentAccepted)// 1
					{
						if (inMessage.EM_MessageSubType == MessageSubTypeCodes.Codes.Cancellation)
						{
							if (latestStatus.IsEmpty || latestStatus == SupplementaryCargoReportJobStatusList.Codes.NotMatched)
							{
								return isValidated ? SupplementaryCargoReportJobStatusList.Codes.Validated : SupplementaryCargoReportJobStatusList.Codes.Cancelled;
							}
							return latestStatus;
						}
						if (!latestStatus.IsEmpty)
						{
							return latestStatus;
						}

						isValidated = true;
					}
					if (cusres.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == ProcessingIndicatorDescriptionCodeList.TransactionAccepted)// 32
					{
						if (latestStatus.IsEmpty)
						{
							latestStatus = SupplementaryCargoReportJobStatusList.Codes.Clear;
						}
					}
					if (cusres.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == ProcessingIndicatorDescriptionCodeList.TransactionRejected)// 33
					{
						if (latestStatus.IsEmpty)
						{
							latestStatus = SupplementaryCargoReportJobStatusList.Codes.NotMatched;
						}
					}
					if (cusres.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == ProcessingIndicatorDescriptionCodeList.ProhibitedRestrictedGoods)// 25
					{
						if (latestStatus.IsEmpty)
						{
							ZString raCode = "";
							if (cusres.Group4.Count > 0 && cusres.Group4[0].ERP.Count > 0)
							{
								raCode = cusres.Group4[0].ERP[0].ErrorPointDetails.MessageSubItemIdentifier;
							}
							latestStatus = GetImpedementJobStatus(raCode);
						}
					}
					if (cusres.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == ProcessingIndicatorDescriptionCodeList.ErrorMessage)// 14
					{
						if (latestStatus.IsEmpty && !isValidated)
						{
							bool syntaxError = false;
							foreach (SegmentGroup4 g4 in cusres.Group4)
							{
								int count = g4.ERP.Count;
								for (int i = 0; i < count; i++)
								{
									if (g4.ERP[i].ErrorPointDetails.MessageSubItemIdentifier == "28" ||
										g4.ERP[i].ErrorPointDetails.MessageSubItemIdentifier == "29")
									{
										syntaxError = true;
										break;
									}
								}
							}
							if (!syntaxError)
							{
								latestStatus = SupplementaryCargoReportJobStatusList.Codes.Error;
							}
						}
					}
				}
			}
			if (latestStatus.IsEmpty)
			{
				latestStatus = isValidated ? SupplementaryCargoReportJobStatusList.Codes.Validated : linkedObject.JobStatus.ToString();
			}
			return latestStatus;
		}

		protected ZString GetImpedementJobStatus(ZString impedementReasonCode)
		{
			switch (impedementReasonCode)
			{
				case "5":
					return SupplementaryCargoReportJobStatusList.Codes.DoNotLoad;
				case "6":
					return SupplementaryCargoReportJobStatusList.Codes.Hold;
				case "7":
					return SupplementaryCargoReportJobStatusList.Codes.DoNotUnload;
				case "1":
					return SupplementaryCargoReportJobStatusList.Codes.RACleared;
				default:
					return SupplementaryCargoReportJobStatusList.Codes.Unknown;
			}
		}

		#endregion
	}
}
