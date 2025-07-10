using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;

namespace Enterprise.Customs.JP.Business
{
	sealed class MSXMessageProvider : IRegisterSupportingDocument
	{
		public MSXMessageProvider(MSXMessageSendingObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}

		readonly MSXMessageSendingObject sendingObject;

		public IEnumerable<IFile> Files
		{
			get
			{
				foreach (MSXMessageSendingObjectAttachment file in sendingObject.Attachments)
				{
					yield return TryGetFileProvider(file);
				}
			}
		}

		public string DeclarationNumber => sendingObject.Header.EntryNumber;

		public string DeclarationType => sendingObject.Header.CH_PhaseStatus;

		public string RegistrationType => sendingObject.RegistrationType ? "T" : string.Empty;

		public string CommunicationColumn => sendingObject.Communication;

		FileProvider TryGetFileProvider(MSXMessageSendingObjectAttachment file) => file != null ? new FileProvider(file) : null;
	}
}
