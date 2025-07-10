using System;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class TradeChainPartnerSendingMessageForm : ZChildForm
	{
		public TradeChainPartnerSendingMessageForm(TradeChainPartnerMessageManager messageManager) : base(messageManager)
		{
			this.messageManager = messageManager;
		}

		readonly TradeChainPartnerMessageManager messageManager;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			TCPGrid.GetDeleteMenuVisibleMethod += () => false;
		}

		public override string FormHeading
		{
			get { return Res.GetString("0A579434-8F95-4895-A2B7-2B171DB65792", "Update Trade Chain Partner"); }
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			messageManager.SendMessage();
			Close();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			messageManager.TCPCollection.RemoveAndDeleteAll();
			Close();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			messageManager.TCPCollection.RemoveAndDeleteAll();
		}
	}
}
