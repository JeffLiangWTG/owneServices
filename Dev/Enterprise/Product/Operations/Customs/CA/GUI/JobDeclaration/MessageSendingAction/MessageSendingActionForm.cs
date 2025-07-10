using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class MessageSendingActionForm : ZChildForm
	{
		public MessageSendingActionForm()
		{
		}

		public MessageSendingActionForm(CAMessageSendingActionCollection actions)
			: base(actions)
		{
			this.actions = actions;
			actions.IsCancelled = true;
			AddOrRemoveColumns();
		}

		public override string FormCaption
		{
			get { return Res.GetString("77f540d7-5d84-4d0f-9bd5-e174f6f35e9a", "Send Messages"); }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		#region Implementation
		internal void AddColumnsForAmendmentDetection()
		{
			EntriesGrid.AddToAvailableColumns(CAMessageSendingAction.Schema.CA_SaveWithoutSending,
				CAMessageSendingAction.Schema.CA_SaveWithoutSendingReasonText);
		}

		internal readonly CAMessageSendingActionCollection actions;

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
			MissingResourceStringChecker.ExcludeFromTest(MessageContentsTextBox);
		}

		void AddOrRemoveColumns()
		{
			//AmendmentController will add these columns back before showing the form for automatic amendment:AddColumnsForAmendmentDetection
			EntriesGrid.RemoveFromAvailableColumns(CAMessageSendingAction.Schema.CA_SaveWithoutSending,
				CAMessageSendingAction.Schema.CA_SaveWithoutSendingReasonText);
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			actions.IsCancelled = true;
			if (!actions.HasAtLeastOneToSendMessageFor)
			{
				Globals.Message.ShowInformation(YouHaveNotSelectedAnythingToSendMessagesFor);
			}
			else
			{
				actions.RunPreSaveValidation();
				if (actions.HasErrors())
				{
					using (var msgBox = new ZErrorMessageBox(actions, Res.GetString("92423a1b-8379-4e48-a35c-da185f3e2c50", "message"), Res.GetString("906257ca-8afa-4485-899b-42b3b8318ca9", "send"), Res.GetString("2bddf1bc-52a4-4662-9c45-aadb6220b391", "sent")))
					{
						ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
					}
				}
				else if (!actions.HasNotifications() || Globals.Message.Show(ThereIsANotification, Res.GetString("0a0a76eb-ac3f-4f72-826d-5fee6c0f3c6a", "Send messages"), MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK)
				{
					actions.IsCancelled = false;
					Close();
				}
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			actions.IsCancelled = true;
			Close();
		}

		internal static string YouHaveNotSelectedAnythingToSendMessagesFor
		{
			get { return Res.GetString("26cac2a1-aa45-458b-b4d1-42dd20f643f5", "There is nothing to send a message for"); }
		}
		internal static string ThereIsANotification
		{
			get { return Res.GetString("971bacdb-d03c-4de2-a262-a8bfb081c71c", "There is a notification. Are you sure you wish to continue?"); }
		}

		void EntriesGrid_AfterBind(object sender, EventArgs e)
		{
			EntriesGrid.ListManager.PositionChanged += new EventHandler(ListManager_PositionChanged);
			ListManager_PositionChanged(null, null);
		}

		void ListManager_PositionChanged(object sender, EventArgs e)
		{
			CurrencyManager listManager = EntriesGrid.ListManager;
			if (listManager != null)
			{
				CAMessageSendingAction current = (CAMessageSendingAction)listManager.GetCurrent();
				if (current != null)
				{
					bool isDiff = (currentAction != current);
					if (isDiff)
					{
						currentAction = current;
					}
				}
			}
		}
		CAMessageSendingAction currentAction;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		#endregion
	}
}
