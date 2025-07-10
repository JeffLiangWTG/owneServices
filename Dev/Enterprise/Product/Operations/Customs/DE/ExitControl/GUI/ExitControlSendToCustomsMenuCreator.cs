using Enterprise.Customs.DE.ExitControl.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.ExitControl.GUI
{
	public class ExitControlSendToCustomsMenuCreator : EU.ExitControl.GUI.ExitControlSendToCustomsMenuCreator
	{
		public ExitControlSendToCustomsMenuCreator(CusExitHeader header)
			: base(header)
		{
		}

		protected override void OnMessageSendingFormOk(EU.ExitControl.Business.ExitControlMessageSendingObjectParent sendingParent)
		{
			var messagesCreated = (sendingParent as ExitControlMessageSendingObjectParent).SendAndSaveMessages();
			if (messagesCreated > 0)
			{
				Globals.Message.ShowInformation(Res.GetString("d8f4a657-8cf4-4003-8980-af03e71f2c07", "{0} message(s) sent.", messagesCreated));
			}
		}

		protected new CusExitHeader header => (CusExitHeader)base.header;

		protected override EU.ExitControl.Business.ExitControlMessageSendingObjectParent GetMessageSendingParent() => new ExitControlMessageSendingObjectParent(header);

		protected override bool HasValidSystemSettings()
		{
			var result = base.HasValidSystemSettings();
			if (result && !ExitDeclarationMessageBuilderLoader.Instance.HasMessageBuildersForCurrentAESVersion)
			{
				Globals.Message.ShowError(Res.GetString("0DADEB6D-73EF-4573-88A4-E2964E17C153", "Current AES Version is not supported for sending Exit Control messages, please check Registry: Customs -> Country or Region Specific -> Germany -> Customs Message Version."));
				result = false;
			}
			return result;
		}
	}
}
