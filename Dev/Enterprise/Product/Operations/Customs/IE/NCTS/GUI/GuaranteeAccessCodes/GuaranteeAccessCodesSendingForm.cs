using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.NCTS.GUI
{
	public partial class GuaranteeAccessCodesSendingForm : EU.NCTS.GUI.GuaranteeAccessCodesSendingForm
	{
		public GuaranteeAccessCodesSendingForm()
		{
			InitializeComponent();
		}

		public GuaranteeAccessCodesSendingForm(GuaranteeAccessCodesSendingActionParent parent) : base(parent)
		{
			cusGuarantee = parent.CusGuaranteeHeader;
		}
		readonly CusGuaranteeHeader cusGuarantee;

		public new GuaranteeAccessCodesSendingActionParent BusinessEntity => (GuaranteeAccessCodesSendingActionParent)base.BusinessEntity;

		public static void ShowForm(CusGuaranteeHeader cusGuaranteeHeader)
		{
			using (var messageSendingForm = new GuaranteeAccessCodesSendingForm(new GuaranteeAccessCodesSendingActionParent(cusGuaranteeHeader)))
			{
				ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm);
			}
		}

		protected override void SendButton_ClickCore()
		{
			base.SendButton_ClickCore();
			SendToCustoms();
		}

		void SendToCustoms()
		{
			var messageSent = 0;

			foreach (var sendingAction in BusinessEntity.SendingObjectsCollection)
			{
				var sender = sendingAction.CreateSender();
				if (sender.Send() != null)
				{
					messageSent++;
				}
			}

			if (messageSent > 0)
			{
				TrySaveAndShowMessage(cusGuarantee.Factory, messageSent);
			}
		}

		void TrySaveAndShowMessage(BusinessObjectFactory factory, int messagesCreated)
		{
			try
			{
				factory.Save();
				Globals.Message.ShowInformation(GetMessageSentText(messagesCreated));
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		string GetMessageSentText(int count)
		{
			return Res.GetString("03E8824A-323E-49CC-AEF4-F6ECB59F1C7B", "Message has been queued/sent through Customs Guarantee Module.", count);
		}
	}
}
