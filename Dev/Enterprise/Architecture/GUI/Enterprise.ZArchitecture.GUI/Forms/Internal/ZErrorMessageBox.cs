using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	public class ZErrorMessageBox : ZMessageBox
	{
		public ZErrorMessageBox(IBusiness businessEntity, bool includeIgnoreOption = false) : this(
			businessEntity, businessEntity.HumanReadableName,
			ResString.GetMultilingualString("f164e541-f7c7-4c18-bce3-91f7f01b4ef4", "save"),
			ResString.GetMultilingualString("2b369961-3de1-447e-93dc-c16d23271343", "saved"),
			includeIgnoreOption)
		{
		}

		public ZErrorMessageBox(IBusiness businessEntity, string entityName, string presentVerb, string pastVerb, bool includeIgnoreOption = false) : this(businessEntity,
			includeIgnoreOption
				? ResString.GetMultilingualString("AEA25184-7246-4C97-A885-157348DCF479", "There are errors on this {0}. Do you want to ignore them and proceed with {1}?", entityName, presentVerb)
				: ResString.GetMultilingualString("24f402d3-f548-4174-b48e-b4b4d260d281", "There are errors that need to be corrected before this {0} can be {1}.", entityName, pastVerb),
			ResString.GetMultilingualString("017f1c10-4108-43f2-a81f-ba8bee3ee0f3", "Unable to {1} this {0}...", entityName, presentVerb),
			includeIgnoreOption)
		{
		}

		public ZErrorMessageBox(IBusiness businessEntity, string message, string caption, bool includeIgnoreOption = false)
			: base(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1)
		{
			Intitialise(businessEntity, includeIgnoreOption);
		}

		public ZErrorMessageBox(IBusiness businessEntity, MultilingualString message, string caption, bool includeIgnoreOption = false)
			: base(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1)
		{
			Intitialise(businessEntity, includeIgnoreOption);
		}

		void Intitialise(IBusiness businessEntity, bool includeIgnoreOption)
		{
			if (businessEntity == null)
			{
				ErrorReporter.ReportOnce("ZErrorMessageBox", "Object was null");
			}
			BusinessEntity = businessEntity;
			InitialFormHeight = Height;
			if (includeIgnoreOption)
			{
				// There is no MessageBoxButtons option with only Abourt and Ignore buttons, so initialize them manually
				InitialiseTwoButtons(DialogResult.Abort, DialogResult.Ignore, MessageBoxDefaultButton.Button1);
				CancelButton = Button1;
			}
			SetupErrorDetails(includeIgnoreOption);
		}

		IBusiness BusinessEntity;

		#region Error Details

		ZGroupBox DetailsGroupBox;
		ZTextBox DetailsTextBox;
		ZButton DetailsButton;
		ZPanel DetailsPanel;

		const int MaxDetailsGroupBoxHeight = 196;
		const int DetailsPadding = 26;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Unicode character")]
		static string ShowDetailsText
		{
			get { return Res.GetString("462a565a-68dc-4b6b-b71d-1473ee227831", "Show &Details") + "\u25BC"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Unicode character")]
		static string HideDetailsText
		{
			get { return Res.GetString("c6b7aec8-1a52-4d86-a0a2-f2870753e2f7", "Hide &Details") + "\u25B2"; }
		}

		int InitialFormHeight;
		string fDetailsTextWithColumnNames;
		bool ShowDetails;

		void SetupErrorDetails(bool includeIgnoreOption)
		{
			var padding = ControlDpiScalingHelper.ScaleToCurrentDpiX(5);

			ControlDpiScalingHelper.SetWidth(this, Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(10), false);

			DetailsPanel = new ZPanel();
			DetailsPanel.BorderStyle = BorderStyle.None;
			DetailsPanel.BackColor = Color.White;
			DetailsPanel.Size = ControlDpiScalingHelper.NewScaledSize(ClientRectangle.Width - (padding * 2) - 60, Button1.Height, false);
			DetailsPanel.Location = ControlDpiScalingHelper.NewScaledPoint(padding, Button1.Top, false);
			Controls.Add(DetailsPanel);

			DetailsButton = new ZButton();
			DetailsButton.BackColor = Color.OldLace;
			DetailsButton.Font = new Font("Arial", 8.50F);
			DetailsButton.Text = ShowDetailsText;
			DetailsButton.AutoSize = true;
			DetailsButton.Dock = DockStyle.Left;
			ControlDpiScalingHelper.SetTop(ref DetailsButton, Button1.Top, false);
			DetailsButton.Click += new EventHandler(DetailsButton_Click);
			DetailsPanel.Controls.Add(DetailsButton);

			DetailsGroupBox = new ZGroupBox();
			DetailsGroupBox.Text = Res.GetString("ca10054e-c1f8-4002-b76c-41193f3ef03a", "Error Details");
			DetailsGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(padding, DetailsPanel.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(10), false);
			DetailsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(ClientRectangle.Width - (padding * 2), ClientRectangle.Bottom - ControlDpiScalingHelper.ScaleToCurrentDpiY(25), false);
			DetailsGroupBox.BackColor = Color.White;

			DetailsTextBox = new ZTextBox();
			DetailsTextBox.Multiline = true;
			DetailsTextBox.CharacterCasing = CharacterCasing.Normal;
			DetailsTextBox.WordWrap = true;
			DetailsTextBox.BorderStyle = BorderStyle.None;
			DetailsTextBox.ReadOnly = true;
			DetailsTextBox.Parent = DetailsGroupBox;
			DetailsTextBox.Dock = DockStyle.Fill;
			DetailsTextBox.BackColor = Color.White;
			DetailsGroupBox.Controls.Add(DetailsTextBox);

			var leftOfButton1 = (ClientRectangle.Width - Button1.Width - DetailsButton.Width - padding) / 2;
			if (includeIgnoreOption)
			{
				leftOfButton1 -= (Button2.Width + padding) / 2;
			}
			ControlDpiScalingHelper.SetLeft(ref Button1, leftOfButton1, false);
			Button1.Anchor = AnchorStyles.Left | AnchorStyles.Top;

			var leftOfNextControl = leftOfButton1 + Button1.Width + padding;

			if (includeIgnoreOption)
			{
				ControlDpiScalingHelper.SetLeft(ref Button2, leftOfNextControl, false);
				Button2.Anchor = AnchorStyles.Left | AnchorStyles.Top;
				leftOfNextControl += Button2.Width + padding;
			}

			ControlDpiScalingHelper.SetLeft(ref DetailsPanel, leftOfNextControl, false);
			ControlDpiScalingHelper.SetWidth(ref DetailsPanel, DetailsButton.Width + (padding * 2), false);
			ControlDpiScalingHelper.SetWidth(ref TextBox, TextBox.Width - (padding * 2), false);
		}

		void DetailsButton_Click(object sender, EventArgs e)
		{
			ShowDetails = !ShowDetails;

			if (ShowDetails)
			{
				DetailsButton.Text = HideDetailsText;
				UpdateDetailsText();
			}
			else
			{
				DetailsButton.Text = ShowDetailsText;
				Controls.Remove(DetailsGroupBox);
				ControlDpiScalingHelper.SetHeight(this, InitialFormHeight, false);
			}
		}

		protected void UpdateDetailsText()
		{
			Controls.Add(DetailsGroupBox);
			DetailsTextBox.Text = DetailsTextWithColumnNames;
			DetailsTextBox.ScrollBars = DetailsScrollBar;
			ControlDpiScalingHelper.SetHeight(ref DetailsGroupBox, DetailsHeight, false);
			ControlDpiScalingHelper.SetHeight(this, InitialFormHeight + DetailsGroupBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
		}

		protected ScrollBars DetailsScrollBar
		{
			get { return (RequestedDetailsHeight > ControlDpiScalingHelper.ScaleToCurrentDpiY(MaxDetailsGroupBoxHeight)) ? ScrollBars.Vertical : ScrollBars.None; }
		}

		protected internal string DetailsTextWithColumnNames
		{
			get
			{
				if (fDetailsTextWithColumnNames == null)
				{
					try
					{
						var fatalNotifications = new HumanReadableNotificationCollector(BusinessEntity, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetFatalNotifications();
						const char bullet = (char)8226;
						fDetailsTextWithColumnNames = "  " + bullet + " " + String.Join(System.Environment.NewLine + "  " + bullet + " ", fatalNotifications.GetUniqueMessageList());
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce("Exception occurred while getting validation error messages - your ZPropertyInfo may access the DataRow.", ex);
						fDetailsTextWithColumnNames = Res.GetString("054f3702-9262-4e28-8840-7e0d332350b1", "An error occurred while generating the error details.");
					}
				}
				return fDetailsTextWithColumnNames;
			}
		}

		protected int DetailsHeight
		{
			get { return Math.Min(RequestedDetailsHeight, ControlDpiScalingHelper.ScaleToCurrentDpiY(MaxDetailsGroupBoxHeight)); }
		}

		protected int RequestedDetailsHeight
		{
			get
			{
				int result;

				using (var graphics = DetailsTextBox.CreateGraphics())
				{
					var currentText = DetailsTextWithColumnNames;
					result = ControlDpiScalingHelper.ScaleToCurrentDpiY(DetailsPadding) + (int)graphics.MeasureString(currentText, DetailsTextBox.Font, DetailsTextBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(2)).Height;
				}

				return result;
			}
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (!ShowDetails)
			{
				DetailsGroupBox?.Dispose();
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
