using System;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Business.MessageProcessors;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.Common.CA;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Edifact.D99B.Elements;
	using Enterprise.Edifact.D99B.Messages.CUSRES;
	using MessageTypeList = MessageTypeList;

	public class B3ImportStatusCalculator : EDIFACTMessageStatusCalculator
	{
		public override ZString MessageTypeDescription
		{
			get { return MessageTypeList.Descriptions.B3CUSDEC; }
		}

		#region CalculatedJobStatus

		public override ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject)
		{
			return CalculatedJobStatus(linkedObject, null);
		}

		public ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject, List<int> group4Indexes)
		{
			var resultStatus = ZString.Empty;
			var group4Index = group4Indexes != null && group4Indexes.Count > 0 ? group4Indexes[0] : 0;
			var currentStatus = linkedObject.JobStatus;
			foreach (EDIMessage inMessage in linkedObject.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.CAIMP, Array.Empty<ZString>(), EDIMessage.Direction.Receive, true, ListSortDirection.Descending))
			{
				var cusresMessage = (CUSRESMessage)inMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());

				if (cusresMessage != null)
				{
					if (cusresMessage.GIS.Count > 0 && cusresMessage.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == ProcessingIndicatorDescriptionCodeList.ErrorMessage)
					{
						if (resultStatus.IsEmpty)
						{
							resultStatus = EDIReleaseImportEntryStatusList.Codes.SyntaxError;
						}
					}
					else
					{
						if (cusresMessage.Group4.Count > group4Index)
						{
							var group4 = cusresMessage.Group4[group4Index];
							if (currentStatus != B3EntryStatusList.Codes.Accepted && B3ResponseMessageProcessor.IsEntryError(cusresMessage.Group4, group4Indexes))
							{
								resultStatus = B3EntryStatusList.Codes.Error;
								continue;
							}
							if (B3ResponseMessageProcessor.IsEntryAccepted(group4))
							{
								resultStatus = B3EntryStatusList.Codes.Accepted;
								break;
							}
							if (B3ResponseMessageProcessor.IsEntryConfirmed(group4))
							{
								resultStatus = B3EntryStatusList.Codes.Confirmed;
								break;
							}
						}
						if (currentStatus != B3EntryStatusList.Codes.Accepted && cusresMessage.Group4.Cast<SegmentGroup4>().Any(group4 => group4.ERP.Count > 0 && B3EntryComponentTypes.ContainsCode(group4.ERP[0].ErrorPointDetails.MessageItemNumber)))
						{
							resultStatus = B3EntryStatusList.Codes.Error;
						}
					}
				}
			}
			return resultStatus.IsEmpty ? currentStatus : resultStatus;
		}

		public B3EntryComponentTypes B3EntryComponentTypes
		{
			get { return b3EntryComponentTypes ?? (b3EntryComponentTypes = new B3EntryComponentTypes()); }
		}
		B3EntryComponentTypes b3EntryComponentTypes;

		#endregion
	}
}
