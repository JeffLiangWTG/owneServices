using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class HeaderWrapper : IHeader
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public HeaderWrapper(DeltaGJobDeclarationMessageSendingObject sendingObject)
		{
			this.itemSendingObject = Argument.NotNull(sendingObject, "DeltaGJobDeclarationMessageSendingObject cannot be null");
			this.messageActionCode = Argument.NotNullOrEmpty(itemSendingObject.MessageType, "MessageType cannot be null or empty");
		}

		public ZString ActionCode => GetActionCode(this.messageActionCode);

		public IMotivation Motivation => new MotivationWrapper(itemSendingObject);

		public IReferences References => new ReferencesWrapper(itemSendingObject.Header);

		ZString GetActionCode(ZString codeMessageType)
		{
			return new ZString(EntryActionCodeList.GetMessageCodeNumber(codeMessageType, itemSendingObject.Header?.Declaration?.IsDeltaC ?? ZBool.True).ToString());
		}

		readonly DeltaGJobDeclarationMessageSendingObject itemSendingObject;
		readonly ZString messageActionCode;
	}
}
