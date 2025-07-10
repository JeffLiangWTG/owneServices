using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageMessageSendingObjectLookups : EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectLookups
	{
		public TemporaryStorageMessageSendingObjectLookups(TemporaryStorageMessageSendingObject parent) : base(parent)
		{
		}

		public CodeDescriptionPairList DeclarationTypes
		{
			get
			{
				var provider = Parent.Header.MessagingProvider;
				return provider is TemporaryStorageMessagingProvider ieProvider ? ieProvider.GetDeclarationTypes() : new CodeDescriptionPairList();
			}
		}
	}
}
