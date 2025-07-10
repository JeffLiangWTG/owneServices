using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.CA.Business
{
	public partial class JobDeclaration : IEDIFACTMessageAttachee, IB3MessageProcessorLinkedObject
	{
		#region IEDIFACTMessageAttachee Members

		ZString IEDIFACTMessageAttachee.MessageStatus { get => JE_MessageStatus; set => JE_MessageStatus = value; }
		ZString IEDIFACTMessageAttachee.JobStatus { get => JE_EntryStatus; set => JE_EntryStatus = value; }

		ZString IEDIFACTMessageAttachee.JobIdentification => JE_DeclarationReference;

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject => this;

		void IEDIFACTMessageAttachee.AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			Messages.Add(message);
		}

		#endregion

		#region IB3MessageProcessorLinkedObject Members

		ZDateTime IB3MessageProcessorLinkedObject.EntryReleaseDate
		{
			get => CA_B2AcceptedDate;
			set => CA_B2AcceptedDate = value;
		}

		void IB3MessageProcessorLinkedObject.CancelScheduledB3Message()
		{
		}

		void IB3MessageProcessorLinkedObject.CancelB3LateSendingWarningEvent()
		{
		}

		void IB3MessageProcessorLinkedObject.AddDocumentsToGeneratorQueue()
		{
		}

		#endregion
	}
}
