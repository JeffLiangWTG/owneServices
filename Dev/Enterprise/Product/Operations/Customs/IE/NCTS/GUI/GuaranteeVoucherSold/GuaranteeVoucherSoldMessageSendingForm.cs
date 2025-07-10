using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.NCTS.GUI
{
	public partial class GuaranteeVoucherSoldMessageSendingForm : MessageSendingObjectForm
	{
		public GuaranteeVoucherSoldMessageSendingForm(GuaranteeVoucherSoldSendingActionParent parent) : base(parent)
		{
			this.parent = parent;
			this.header = parent.Header;
		}
		readonly CusGuaranteeHeader header;
		readonly GuaranteeVoucherSoldSendingActionParent parent;

		public override string FormHeading => Res.GetString("ED65EA6E-5B56-49FB-BB02-99FE6049F9DC", "Guarantee Voucher Sold");

		public static void ShowForm(CusGuaranteeHeader header)
		{
			var parent = new GuaranteeVoucherSoldSendingActionParent(header);
			using (var messageSendingForm = new GuaranteeVoucherSoldMessageSendingForm(parent))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
				{
					messageSendingForm.SendToCustoms();
				}
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void SendToCustoms()
		{
			var messageSent = 0;
			parent.SendingObjectsCollection.Cast<GuaranteeVoucherSoldSendingAction>().Where(action => action.ShouldSend).ForEach(
				action =>
				{
					var sender = action.CreateSender();
					if (sender.Send() != null)
					{
						messageSent++;
					}
				});
			if (messageSent > 0)
			{
				TrySaveAndShowMessage(header.Factory, messageSent);
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
			return Res.GetString("B542D1D3-BFE0-449E-B601-DB505E68EABD", "{0} message(s) queued for sending.", count); // change wording ??
		}

		protected override bool CheckIsOKToSend()
		{
			UpdateSendingActionShouldSend();
			return base.CheckIsOKToSend();
		}

		void UpdateSendingActionShouldSend()
		{
			foreach (var sendingAction in parent?.SendingObjectsCollection?.Cast<GuaranteeVoucherSoldSendingAction>())
			{
				sendingAction.ShouldSend = true;
			}
		}
	}
}
