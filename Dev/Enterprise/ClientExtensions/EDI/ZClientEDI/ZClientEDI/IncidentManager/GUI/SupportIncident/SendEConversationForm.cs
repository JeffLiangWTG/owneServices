using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Tools;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class SendEConversationForm : ZChildForm
	{
		public enum PerformAction
		{
			Send = 1,
			AwaitingResponse = 2,
			Discard = 3,
			Cancel = 4
		}

		public SendEConversationForm(ZString eConversationMessage, bool shouldShowAwaitingResponse) : this(shouldShowAwaitingResponse)
		{
			this.MessageTextBox.Text = eConversationMessage;
		}

		public SendEConversationForm(ZBlob eConversationMessage, bool shouldShowAwaitingResponse) : this(shouldShowAwaitingResponse)
		{
			#if !WINZOR
			this.MessageTextBox.RtfZBlob = eConversationMessage;
			#else
			this.MessageTextBox.HtmlZBlob = eConversationMessage;
			#endif
		}

		SendEConversationForm(bool shouldShowAwaitingResponse = false) : base()
		{
			Action = PerformAction.Discard;
			this.WarningImageBox.Paint += WarningImageBox_Paint;
			ActiveControl = SendButton;

			SpellChecker.InitialiseSpellcheck(MessageTextBox, "SendEConversationForm_MessageTextBox");
			if (!shouldShowAwaitingResponse)
			{
				CancelButton.Location = ControlDpiScalingHelper.NewScaledPoint(291, 224, true);
				DiscardButton.Location = AwaitingResponseButton.Location;
				Label2.Text = "Please click \"Send\" to send pending message. If you choose \"Discard\", the message will be lost.";
				AwaitingResponseButton.Visible = false;
			}
		}

		public string MessageText
		{
			get { return MessageTextBox.Text; }
		}

		public PerformAction Action { get; protected set; }

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void WarningImageBox_Paint(object sender, PaintEventArgs e)
		{
			var rect = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledRectangle(ControlDpiScalingHelper.NewScaledPoint(3, 3), SystemIcons.Warning.Size);
			e.Graphics.DrawIconUnstretched(SystemIcons.Warning, rect);
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			Action = PerformAction.Send;
			Close();
		}

		void AwaitingResponseButton_Click(object sender, EventArgs e)
		{
			Action = PerformAction.AwaitingResponse;
			Close();
		}

		void DiscardButton_Click(object sender, EventArgs e)
		{
			Action = PerformAction.Discard;
			Close();
		}

		void DiscardButton_MouseEnter(object sender, EventArgs e)
		{
			DiscardButton.BackColor = Color.Firebrick;
			DiscardButton.ForeColor = Color.White;
		}

		void DiscardButton_MouseLeave(object sender, EventArgs e)
		{
			DiscardButton.UseVisualStyleBackColor = true;
			DiscardButton.ForeColor = SendButton.ForeColor;
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Action = PerformAction.Cancel;
			Close();
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			if (Action == PerformAction.Discard)
			{
				if (DialogResult.Yes != Globals.Message.Show("The pending eConversation message will be discarded, are you sure?", "Discard Pending eConversation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No))
				{
					e.Cancel = true;
				}
			}
		}
	}
}
