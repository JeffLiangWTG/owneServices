using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public sealed class AttachedDocumentProvider
	{
		public AttachedDocumentProvider(IAttachedDocument typeOfDutyProvider)
		{
			this.typeOfDutyProvider = typeOfDutyProvider;
		}
		readonly IAttachedDocument typeOfDutyProvider;

		public ZString DocumentDate => typeOfDutyProvider.DocumentDate;
		public ZString DocumentIdentifier => typeOfDutyProvider.DocumentIdentifier;
		public ZString DocumentType => typeOfDutyProvider.DocumentType;
	}
}
