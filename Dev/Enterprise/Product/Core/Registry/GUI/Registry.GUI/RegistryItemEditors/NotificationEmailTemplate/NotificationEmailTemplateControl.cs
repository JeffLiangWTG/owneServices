using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Registry.GUI
{
	public partial class NotificationEmailTemplateControl : ZUserControl
	{
		public NotificationEmailTemplateControl()
		{
			InitializeComponent();
			SetupHelpImage();
		}

		public IEmailGenerator EmailGenerator => emailGenerator ?? (emailGenerator = GetEmailGenerator());
		internal IEmailGenerator emailGenerator;

		IEmailGenerator GetEmailGenerator()
		{
			try
			{
				var emailGeneratorAttributes = CurrentDataItem?.DocSourceType.GetCustomAttributes(typeof(EmailGeneratorAttribute), false) as EmailGeneratorAttribute[];

				return emailGeneratorAttributes?.Length > 0
					? ObjectFactory.Get(emailGeneratorAttributes[0].Type) as IEmailGenerator
					: null;
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce("Unable to get email generator", ex);

				return null;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			this.EmailBodyTextBox.Visible = !(CurrentDataItem?.ShouldHideEmailBody ?? false);
			this.PreviewButton.Visible = EmailGenerator != null;
		}

		void SetupHelpImage()
		{
			using (var graphics = HelpPictureBox.CreateGraphics())
			{
				var captionSize = graphics.MeasureString(this.zGroupBox1.CaptionResourceString.Caption, this.zGroupBox1.Font);
				HelpPictureBox.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.ScaleToCurrentDpiX(32) + (int)captionSize.Width, 0, false);
			}

			HelpPictureBox.Image = Icons.GetImage(IconTypes.HelpButtonRest);
			HelpPictureBox.MouseEnter += delegate
			{ HelpPictureBox.Image = Icons.GetImage(IconTypes.HelpButtonActive); };
			HelpPictureBox.MouseLeave += delegate
			{ HelpPictureBox.Image = Icons.GetImage(IconTypes.HelpButtonRest); };
		}

		#region Events

		internal void DocumentFieldsGrid_DoubleClick(object sender, EventArgs e)
		{
			if (textBoxToInsertFields != null && !textBoxToInsertFields.ReadOnly)
			{
				DocumentFieldDefinition selectedDocumentField = DocumentFieldsGrid.ListManager != null ? (DocumentFieldDefinition)DocumentFieldsGrid.ListManager.GetCurrent() : null;
				if (selectedDocumentField != null)
				{
					string tagFieldToInsert = Core.Constants.DocumentEngine.EmailParsing.StartTag + selectedDocumentField.FieldName + Core.Constants.DocumentEngine.EmailParsing.EndTag;
					var propertyToInsertDocumentFieldToOriginalValue = (ZString)propertyToInsertDocumentFieldTo.Value;
					if (cursorPosBeforeLosingFocus >= 0 && cursorPosBeforeLosingFocus <= propertyToInsertDocumentFieldToOriginalValue.Length)
					{
						propertyToInsertDocumentFieldTo.Value = (ZString)propertyToInsertDocumentFieldToOriginalValue.Insert(cursorPosBeforeLosingFocus, tagFieldToInsert);
					}
					cursorPosBeforeLosingFocus += tagFieldToInsert.Length;
				}
			}
		}

		internal void EmailTemplateTextBox_Leave(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				if (sender == EmailBodyTextBox)
				{
					propertyToInsertDocumentFieldTo = CurrentDataItem.EmailBodyInfo;
				}
				else if (sender == EmailSubjectTextBox)
				{
					propertyToInsertDocumentFieldTo = CurrentDataItem.EmailSubjectInfo;
				}
			}

			textBoxToInsertFields = (ZTextBox)sender;
			cursorPosBeforeLosingFocus = textBoxToInsertFields.SelectionStart;
		}

		void HelpPictureBox_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(helpText))
			{
				helpText = Res.GetString("1766aa2d-7508-46f0-bbde-7ea660352392",
@"You can place special fields in your email subject or body that will be merged with {0} information before the email is sent.
Special fields should start with {1} and end with {2} e.g. {1}{3}{2}

All available special fields are shown on the grid below. Double click on the row to insert field onto the current text position",
					Core.Constants.ProductName, Core.Constants.DocumentEngine.EmailParsing.StartTag, Core.Constants.DocumentEngine.EmailParsing.EndTag, "ContactName");
			}

			Globals.Message.ShowInformation(helpText);
		}

		internal int cursorPosBeforeLosingFocus;
		ZTextBox textBoxToInsertFields;
		ZPropertyInfo propertyToInsertDocumentFieldTo;

		#endregion

		#region BusinessEntity

		public new NotificationEmailTemplate CurrentDataItem
		{
			get { return (NotificationEmailTemplate)base.CurrentDataItem; }
		}

		#endregion

		public void SetAlternativeText(string groupBoxText, string subjectLabelText, string bodyLabelText, string helpText)
		{
			this.zGroupBox1.Text = groupBoxText;
			this.EmailSubjectTextBox.Extensions.Get<ILabelCaptionRenderer>().Caption = subjectLabelText;
			this.EmailBodyTextBox.Extensions.Get<ILabelCaptionRenderer>().Caption = bodyLabelText;
			this.helpText = helpText;
		}

		string helpText;

		void previewButton_Click(object sender, EventArgs e)
		{
			var email = emailGenerator.BuildEmail("john.smith@example.com", CurrentDataItem, emailGenerator.CreatePreviewSample());
			var tempfile = email.SaveAsEml();

			_ = FileOpener.Open(tempfile.Filename);
		}
	}
}
