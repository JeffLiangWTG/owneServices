using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class EDIMessageUserControl : ZUserControl
	{
		public EDIMessageUserControl()
		{
			InitializeComponent();
			MessagesGrid.AfterBind += new EventHandler(MessagesGrid_AfterBind);
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				if (MessagesGrid.ListManager != null)
				{
					MessagesGrid.ListManager.CurrentChanged -= new EventHandler(ListManager_CurrentChanged);
				}
			}
		}

		#endregion

		#region Binding

		public virtual void SetBindPrepend(ZString bindPrepend)
		{
			this.BindPrepend = bindPrepend;
		}

		[System.ComponentModel.Browsable(true)]
		public string BindPrepend
		{
			get { return bindPrepend; }
			set
			{
				if (bindPrepend + messagesGridBindTo != MessagesGrid.BindTo)
				{
					messagesGridBindTo = MessagesGrid.BindTo;
				}

				if (bindPrepend + messageTextTextBoxBindTo != MessageTextTextBox.BindTo)
				{
					messageTextTextBoxBindTo = MessageTextTextBox.BindTo;
				}

				bindPrepend = value;

				MessagesGrid.BindTo = bindPrepend + messagesGridBindTo;
				MessageTextTextBox.BindTo = bindPrepend + messageTextTextBoxBindTo;
			}
		}

		/// <summary>
		/// Use this method with care/last resort. Under some circumstances, the default Messages collection from IEDIMessageCollectionProvider is not what you want to bind,
		/// you can replace the it with another message collection here with minimum disruption.
		/// </summary>
		/// <param name="anotherCollectionName"></param>
		public void ReplaceBindingMessagesCollectionWithAnotherCollection(ZString anotherCollectionName)
		{
			MessagesGrid.BindTo = MessagesGrid.BindTo.Replace("Messages", anotherCollectionName);
			MessageTextTextBox.BindTo = MessageTextTextBox.BindTo.Replace("Messages", anotherCollectionName);
		}

		ZString bindPrepend;
		ZString messagesGridBindTo;
		ZString messageTextTextBoxBindTo;

		void MessagesGrid_AfterBind(object sender, EventArgs e)
		{
			if (MessagesGrid.ListManager != null)
			{
				MessagesGrid.ListManager.CurrentChanged += new EventHandler(ListManager_CurrentChanged);
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			ResetMessageHistoryHeading();
		}

		public bool ShowChangingBlueMessageHeading { get; set; }

		void ResetMessageHistoryHeading()
		{
			if (ShowChangingBlueMessageHeading)
			{
				EDIMessageCollection messageCollection = MessagesGrid.ListManager.List as EDIMessageCollection;
				string newMessageHistoryHeading = Res.GetString("1b3d9981-0c90-4276-beb5-ef75cc33eb86", "Messages");

				if (messageCollection != null)
				{
					if (messageCollection.Master != null && messageCollection.Master is IEDIMessageCollectionProvider)
					{
						newMessageHistoryHeading = ((IDetailsTabPageHeadingProvider)messageCollection.Master).Heading + " " + Res.GetString("1b3d9981-0c90-4276-beb5-ef75cc33eb86", "Messages");
					}
				}

				if (HistoryGroupBox.Text != newMessageHistoryHeading)
				{
					HistoryGroupBox.Text = newMessageHistoryHeading;
				}
			}
		}

		#endregion
	}
}
