using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Customs.CA.Business
{
	public class NoticesMessage : AutoNoticesMessage, IDocumentSupportable
	{
		public NoticesMessage(EDIMessage message, ZString statusDescription)
			: base(message.Factory)
		{
			this.Message = message;
			this.statusDescription = statusDescription;
		}
		public readonly EDIMessage Message;
		readonly ZString statusDescription;

		public override ZString ReferenceNumber
		{
			get { return Message?.ReferenceNumber ?? ZString.Empty; }
		}

		public override ZString EM_MessageSubType
		{
			get { return Message?.EM_MessageSubType ?? ZString.Empty; }
		}

		public override ZDateTime EM_MessageDateTime
		{
			get { return Message?.EM_MessageDateTime ?? ZDateTime.Empty; }
		}

		public override ZDateTime RNSProcessingDate
		{
			get { return Message?.RNSProcessingDate ?? ZDateTime.Empty; }
		}

		public override ZString StatusDescription
		{
			get { return statusDescription.IsEmpty ? (Message?.StatusDescription ?? ZString.Empty) : statusDescription; }
		}

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get
			{
				if (Message is UniversalEventMessage)
				{
					return UniversalEventMessageDocumentSupporter;
				}
				return null;
			}
		}

		UniversalEventMessageDocumentSupporter UniversalEventMessageDocumentSupporter
		{
			get { return new UniversalEventMessageDocumentSupporter((UniversalEventMessage)Message); }
		}
	}
}
