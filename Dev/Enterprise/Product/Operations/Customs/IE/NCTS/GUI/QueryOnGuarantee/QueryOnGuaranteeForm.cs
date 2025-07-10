using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NctsHeader = Enterprise.Customs.IE.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IE.NCTS.GUI
{
	public partial class QueryOnGuaranteeForm : MessageSendingObjectForm
	{
		public QueryOnGuaranteeForm(QueryOnGuaranteeSendingActionParent parent) : base(parent)
		{
			this.parent = parent;
			this.header = parent.Header;
		}
		readonly NctsHeader header;
		readonly QueryOnGuaranteeSendingActionParent parent;

		public override string FormHeading => Res.GetString("38FCCB13-4365-41D9-91BC-7F790F89F711", "Query on Guarantee");

		public static void ShowForm(NctsHeader header)
		{
			QueryOnGuaranteeSendingActionParent parent = new QueryOnGuaranteeSendingActionParent(header);
			using (var messageSendingForm = new QueryOnGuaranteeForm(parent))
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
			parent.SendingObjectsCollection.Cast<QueryOnGuaranteeSendingAction>().Where(action => action.ShouldSend).ForEach(
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
			return Res.GetString("B542D1D3-BFE0-449E-B601-DB505E68EABD", "{0} message(s) queued for sending.", count);
		}

		protected override bool CheckIsOKToSend()
		{
			UpdateSendingActionShouldSend();
			return base.CheckIsOKToSend();
		}

		void UpdateSendingActionShouldSend()
		{
			foreach (var sendingAction in parent?.SendingObjectsCollection?.Cast<QueryOnGuaranteeSendingAction>())
			{
				sendingAction.ShouldSend = sendingAction.AllGuarantees.Cast<QueryOnGuaranteeSendingObject>().Any(p => p.ShouldSend);
			}
		}

		void ClearAllButton_Click(object sender, System.EventArgs e)
		{
			foreach (var item in parent.SendingObjectsCollection)
			{
				if (item is QueryOnGuaranteeSendingAction sendingAction)
				{
					foreach (var guarantee in sendingAction.AllGuarantees)
					{
						if (guarantee is QueryOnGuaranteeSendingObject sendingObject)
						{
							sendingObject.ShouldSend = false;
						}
					}
				}
			}
			GuaranteesGrid.Refresh();
		}

		void SelectAllButton_Click(object sender, System.EventArgs e)
		{
			foreach (var item in parent.SendingObjectsCollection)
			{
				if (item is QueryOnGuaranteeSendingAction sendingAction)
				{
					foreach (var guarantee in sendingAction.AllGuarantees)
					{
						if (guarantee is QueryOnGuaranteeSendingObject sendingObject)
						{
							sendingObject.ShouldSend = true;
						}
					}
				}
			}
			GuaranteesGrid.Refresh();
		}
	}
}
