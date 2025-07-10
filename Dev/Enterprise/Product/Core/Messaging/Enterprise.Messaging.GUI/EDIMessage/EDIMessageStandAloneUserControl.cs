using System;
using System.IO;
using System.Windows.Forms;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Messaging.GUI.MessageSaveActionHandler;

namespace Enterprise.Messaging.GUI
{
	public partial class EDIMessageStandAloneUserControl : ZUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public EDIMessageStandAloneUserControl()
		{
			InitializeComponent();
		}

		public EDIMessage ediMessage { get; set; }
		public MessageSaveActionHandler saveActionHandler => new MessageSaveActionHandler(ediMessage);

		void EDIMessageStandAloneUserControl_Load(object sender, EventArgs e)
		{
			UpdateSaveToDiskAction();
#if DEBUG
			SaveAndOpenButton.Visible = true;
#endif
		}

		#region Implementation

		public void UpdateSaveToDiskAction()
		{
			if (ediMessage != null && ediMessage.IsMessageTextTooLong)
			{
				string sizeText = LargeMessageHelper.SizeInKb(LargeMessageHelper.DetailTextSizeLimit).ToString() + "KB";
				zLabelTruncateNotification.Text = Res.GetString("5EB5E539-1651-49EC-A326-F8AE05281F3D", "Content is greater than {0}, and only the first {0} will be displayed.", sizeText);

				zLabelTruncateNotification.Visible = true;
			}
			else
			{
				zLabelTruncateNotification.Visible = false;
			}
		}

		public void UpdateLinkedEDIMessageControls()
		{
			if (ediMessage?.LinkedEDIMessage == null)
			{
				linkedEDIMessageNumberTextBox.Visible = false;
				showLinkedEDIMessageButton.Visible = false;
				messageDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, messageDetailsGroupBoxHeight, true);
				return;
			}

			if (ediMessage.LinkedEDIMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
			{
				linkedEDIMessageNumberTextBox.CaptionResourceString = Res.GetData("EDIMessageStandAloneUserControl|D0190C67-DAA8-4E28-AD15-E041F8AD816D", "Response Message No.");
			}
			else
			{
				linkedEDIMessageNumberTextBox.CaptionResourceString = Res.GetData("EDIMessageStandAloneUserControl|1E82F1E6-6B8F-442C-A096-CB17B2A4702E", "Request Message No.");
			}

			linkedEDIMessageNumberTextBox.Visible = true;
			showLinkedEDIMessageButton.Visible = true;
			messageDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, messageDetailsGroupBoxHeight + 45, true);
		}

		public void zButtonSaveRawMessage_Click(object sender, EventArgs e)
		{
			saveActionHandler.SaveMessageAction(FileNameType.Raw);
		}

		public void zButtonSaveFormatedMessage_Click(object sender, EventArgs e)
		{
			saveActionHandler.SaveMessageAction(FileNameType.Formatted);
		}

		public void SetShowInterchangeButtonEnabled()
		{
			showInterchangeButton.Enabled = !ediMessage.EM_InterchangeNumber.IsEmpty;
		}

		public void ModifyEditMessageControls(bool isVisible)
		{
			zButtonSaveRawMessage.Visible = isVisible;
		}

		#endregion

		public bool AllowTabBackward(Control control, Control previousControl)
		{
			return (control == zButtonSaveFormatedMessage || previousControl == zButtonSaveFormatedMessage);
		}

		void SaveAndOpenButton_Click(object sender, EventArgs e)
		{
#if DEBUG
			var desktopFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
			var fileName = Path.Combine(desktopFolder, saveActionHandler.GetContentFileName(FileNameType.Formatted));

			using (var fileStream = File.Create(fileName))
			{
				saveActionHandler.SaveMessageContentToFile(fileStream, FileNameType.Formatted, fileName);
			}

			FileOpener.Open(fileName);
#endif
		}

		void ShowInterchangeButton_Click(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Messaging.EDIInterchange);
			var form = controller.ShowViewForm(ediMessage.Interchange);
			if (form != null)
			{
				form.Show();
			}
		}

		void ShowLinkedEDIMessageButton_Click(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Messaging.EDIMessage);
			var form = controller.ShowViewForm(ediMessage.LinkedEDIMessage);
			if (form != null)
			{
				form.Show();
			}
		}
	}
}
