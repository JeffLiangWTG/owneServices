using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageMessageSendingObjectLookups : ZLookups
	{
		public TemporaryStorageMessageSendingObjectLookups(TemporaryStorageMessageSendingObject parent) : base(parent)
		{
		}

		public new TemporaryStorageMessageSendingObject Parent => (TemporaryStorageMessageSendingObject)base.Parent;

		public CodeDescriptionPairList MessageTypes => Parent.Header.MessagingProvider?.GetMessageTypes(Parent.Header) ?? new CodeDescriptionPairList();
	}
}
