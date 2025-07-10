using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	internal class LinkedObjectInfo
	{
		internal LinkedObjectInfo(IEDIFACTMessageAttachee obj, string objReference)
		{
			LinkedObject = obj;
			LinkedObjectReference = objReference;
			MatchedSentRNSMessages = new List<EDIMessage>();
		}

		internal LinkedObjectInfo(IEDIFACTMessageAttachee obj, string objReference, IEnumerable<EDIMessage> matchedSentRNSMessages)
			: this(obj, objReference)
		{
			MatchedSentRNSMessages.AddRange(matchedSentRNSMessages);
		}

		internal LinkedObjectInfo(IEDIFACTMessageAttachee obj, string objReference, EDIMessage sentMessage)
			: this(obj, objReference, new[] { sentMessage })
		{
		}

		#region Branch

		internal GlbBranch NotificationBranch
		{
			get
			{
				if (branch == null)
				{
					var lastSendMessage = MatchedSentRNSMessages.FirstOrDefault();
					if (lastSendMessage == null && LinkedObject != null)
					{
						lastSendMessage = LinkedObject.Messages.GetMatchingMessages(EDIInterchange.ApplicationCodes.CAIMP, new[] { (ZString)MessageTypeList.Codes.RNSRequest, (ZString)MessageTypeList.Codes.EDIRelease }, EDIInterchange.Direction.Transmit).Cast<EDIMessage>().LastOrDefault();
					}

					branch = lastSendMessage?.Branch;

					if (branch == null)
					{
						if (LinkedObject is JobDeclaration declaration)
						{
							branch = declaration.Branch;
						}
						else if (LinkedObject is CusEntryHeader entryHeader)
						{
							branch = entryHeader.Branch;
						}
						else
						{
							branch = GlbBranch.CurrentBranch;
						}
					}
				}
				return branch;
			}
		}

		GlbBranch branch;

		#endregion

		internal IEDIFACTMessageAttachee LinkedObject { get; private set; }
		internal string LinkedObjectReference { get; private set; }
		internal List<EDIMessage> MatchedSentRNSMessages { get; private set; }
	}
}
