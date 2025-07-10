using System;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	using System.ComponentModel;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Business.MessageProcessors;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Customs.Common.Shared;
	using Enterprise.Edifact.D00A.Elements;
	using Enterprise.Edifact.D00A.Messages.CUSRES;

	public class G7ExportStatusCalculator : EDIFACTMessageStatusCalculator
	{
		public override ZString MessageTypeDescription
		{
			get { return MessageTypeList.Descriptions.G7Export; }
		}

		public override ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject)
		{
			foreach (EDIMessage inMessage in linkedObject.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.CAEXP, Array.Empty<ZString>(), EDIMessage.Direction.Receive, true, ListSortDirection.Descending))
			{
				var cusres = (CUSRESMessage)inMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
				if (cusres != null && cusres.GIS.Count > 0)
				{
					if (cusres.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == ProcessingIndicatorDescriptionCodeList.MessageContentAccepted)// 1
					{
						return inMessage.EM_MessageSubType == MessageSubTypeCodes.Codes.Cancellation ? EntryStatusList.Codes.Cancelled : EntryStatusList.Codes.Clear;
					}
					if (cusres.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == ProcessingIndicatorDescriptionCodeList.ErrorMessage)// 14
					{
						var syntaxError = false;
						foreach (SegmentGroup4 g4 in cusres.Group4)
						{
							for (var i = 0; i < g4.ERP.Count; i++)
							{
								if (g4.ERP[i].ErrorPointDetails.MessageSubItemIdentifier == "28" ||
										g4.ERP[i].ErrorPointDetails.MessageSubItemIdentifier == "29")
								{
									syntaxError = true;
									break;
								}
							}
						}
						if (!syntaxError && cusres.Group4.Count > 0)
						{
							return EntryStatusList.Codes.Error;
						}
					}
				}
			}
			return linkedObject.JobStatus;
		}
	}
}
