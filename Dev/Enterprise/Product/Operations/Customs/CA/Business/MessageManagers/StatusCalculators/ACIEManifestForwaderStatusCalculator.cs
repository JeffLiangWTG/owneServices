using System;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.CA.Business.MessageProcessors;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Customs.Common.Shared;

	public class ACIEManifestForwaderStatusCalculator : EDIFACTMessageStatusCalculator
	{
		public ACIEManifestForwaderStatusCalculator(ZString messageTypeDescription) : base()
		{
			this.messageTypeDescription = messageTypeDescription;
		}

		public ACIEManifestForwaderStatusCalculator(EManifestResponseWrapper wrapper, ZString messageTypeDecsription)
			: base()
		{
			this.wrapper = wrapper;
			this.messageTypeDescription = messageTypeDecsription;
		}
		readonly EManifestResponseWrapper wrapper;
		readonly ZString messageTypeDescription;

		public override ZString MessageTypeDescription
		{
			get { return messageTypeDescription; }
		}

		#region Booleans

		public bool IsInError(ZString currentJobStatus)
		{
			return currentJobStatus == EManifestForwarderJobStatusList.Codes.Error;
		}

		public bool IsAccepted(ZString currentJobStatus)
		{
			return currentJobStatus == EManifestForwarderJobStatusList.Codes.Clear
				|| currentJobStatus == EManifestForwarderJobStatusList.Codes.NotMatched
				|| currentJobStatus == EManifestForwarderJobStatusList.Codes.Validated;
		}

		#endregion

		#region Override

		#region CalculatedJobStatus

		public override ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject)
		{
			return CalculateJobStatus(linkedObject.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.CAACI, Array.Empty<ZString>(), EDIMessage.Direction.Receive, true, ListSortDirection.Descending).OfType<EDIMessage>(), linkedObject);
		}

		public ZString CalculateJobStatus(IEnumerable<EDIMessage> matchingMessages, IEDIFACTMessageAttachee linkedObject)
		{
			var latestStatus = ZString.Empty;
			bool isValidated = false;
			foreach (EDIMessage message in matchingMessages)
			{
				var wrapperVar = new EManifestResponseWrapper(message);
				if (wrapperVar.HasGOVCBRMessage)
				{
					if (wrapperVar.IsAccepted)// 1 or 66
					{
						if (message.EM_MessageSubType == MessageSubTypeCodes.Codes.Cancellation)
						{
							if (latestStatus.IsEmpty || latestStatus == EManifestForwarderJobStatusList.Codes.NotMatched)
							{
								return isValidated ? EManifestForwarderJobStatusList.Codes.Validated : EManifestForwarderJobStatusList.Codes.Cancelled;
							}
							return latestStatus;
						}
						if (!latestStatus.IsEmpty)
						{
							return latestStatus;
						}

						isValidated = true;
					}
					if (wrapperVar.IsMatchedNotice)// matched
					{
						if (latestStatus.IsEmpty)
						{
							latestStatus = EManifestForwarderJobStatusList.Codes.Clear;
						}
					}
					if (wrapperVar.IsNOTMatchedNotice)// NOT matched
					{
						if (latestStatus.IsEmpty)
						{
							latestStatus = EManifestForwarderJobStatusList.Codes.NotMatched;
						}
					}
					if (wrapperVar.IsRejected)// 14 or 2
					{
						if (latestStatus.IsEmpty && !isValidated && !wrapperVar.IsSyntaxError && !wrapperVar.IsBatchOrDataError)
						{
							latestStatus = EManifestForwarderJobStatusList.Codes.Error;
						}
					}
				}
				else if (message.EM_MessageType != MessageTypeList.Codes.SyntaxError)
				{
					ErrorReporter.ReportOnce("ReceivedMessageNotAACIForwarderMessage",
						string.Format(CultureInfo.CurrentCulture, "A received eManifest Forwarder message is not a valid ACIForwarderMessage, when it was expected to be. Job {0}, message no {1}, interchange {2}, message type {3}, message text {4}",
						linkedObject.JobIdentification, message.EM_MessageNum, message.Interchange == null ? ZString.Empty : message.Interchange.EI_InterchangeNum, message.EM_MessageType, message.EM_MessageText));
				}
			}

			if (latestStatus.IsEmpty)
			{
				var customsStatus = linkedObject.JobStatus;
				if (customsStatus == EManifestForwarderJobStatusList.Codes.Clear || !isValidated)
				{
					latestStatus = customsStatus;
				}
				else
				{
					latestStatus = EManifestForwarderJobStatusList.Codes.Validated;
				}
			}

			return latestStatus;
		}

		#endregion

		protected override ZString GetMessageSubTypeCore(ZString currentMessageStatus)
		{
			if (wrapper != null && wrapper.LinkedObject is CusEntryHeader)
			{
				return wrapper.WrappedMessage.EM_MessageType;
			}
			return base.GetMessageSubTypeCore(currentMessageStatus);
		}

		#endregion

	}
}
