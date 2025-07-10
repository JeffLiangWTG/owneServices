using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class IncidentEmailTemplatePairControl : ZUserControl
	{
		public IncidentEmailTemplatePairControl()
		{
			InitializeComponent();
			SetupHelpImage();
		}

		void SetupHelpImage()
		{
			HelpPictureBox.Image = Icons.GetImage(IconTypes.HelpButtonRest);
			HelpPictureBox.MouseEnter += delegate
			{ HelpPictureBox.Image = Icons.GetImage(IconTypes.HelpButtonActive); };
			HelpPictureBox.MouseLeave += delegate
			{ HelpPictureBox.Image = Icons.GetImage(IconTypes.HelpButtonRest); };
		}

		#region Events

		void DocumentFieldsGrid_DoubleClick(object sender, EventArgs e)
		{
			OnDocumentFieldsGridDoubleClick();
		}

		protected void OnDocumentFieldsGridDoubleClick()
		{
			if (textBoxToInsertFields != null && !textBoxToInsertFields.ReadOnly)
			{
				DocumentFieldDefinition selectedDocumentField = DocumentFieldsGrid.ListManager != null ? (DocumentFieldDefinition)DocumentFieldsGrid.ListManager.GetCurrent() : null;
				if (selectedDocumentField != null)
				{
					string tagFieldToInsert = Core.Constants.DocumentEngine.EmailParsing.StartTag + selectedDocumentField.FieldName + Core.Constants.DocumentEngine.EmailParsing.EndTag;
					propertyToInsertDocumentFieldTo.Value = (ZString)((ZString)propertyToInsertDocumentFieldTo.Value).Insert(cursorPosBeforeLosingFocus, tagFieldToInsert);
					cursorPosBeforeLosingFocus += tagFieldToInsert.Length;
				}
			}
		}

		void EmailTemplateTextBox_Leave(object sender, EventArgs e)
		{
			OnTextBoxLeave((ZTextBox)sender);
		}

		protected void OnTextBoxLeave(ZTextBox textBox)
		{
			if (CurrentDataItem != null)
			{
				if (textBox == legacyAndERequestV1EmailBodyTextBox)
				{
					propertyToInsertDocumentFieldTo = CurrentDataItem.LegacyAndERequestV1EmailTemplate.EmailBodyInfo;
				}
				else if (textBox == legacyAndERequestV1EmailSubjectTextBox)
				{
					propertyToInsertDocumentFieldTo = CurrentDataItem.LegacyAndERequestV1EmailTemplate.EmailSubjectInfo;
				}
				else if (textBox == eRequestV2EmailBodyTextBox)
				{
					propertyToInsertDocumentFieldTo = CurrentDataItem.ERequestV2EmailTemplate.EmailBodyInfo;
				}
				else if (textBox == eRequestV2EmailSubjectTextBox)
				{
					propertyToInsertDocumentFieldTo = CurrentDataItem.ERequestV2EmailTemplate.EmailSubjectInfo;
				}
			}

			textBoxToInsertFields = textBox;
			cursorPosBeforeLosingFocus = textBoxToInsertFields.SelectionStart;
		}

		string helpText;
		string HelpText
		{
			get
			{
				if (helpText == null)
				{
					helpText = Res.GetString("21128d70-6758-4de7-85f2-d8e78468a334",
@"You can place special fields in your email subject or body that will be merged with System information before the email is sent.
Special fields should start with {0} and end with {1} e.g. {0}{2}{1}

All available special fields are shown on the grid below. Double click on the row to insert field onto the current text position",
					Core.Constants.DocumentEngine.EmailParsing.StartTag, Core.Constants.DocumentEngine.EmailParsing.EndTag, "ContactName");
				}

				return helpText;
			}
		}

		void HelpPictureBox_Click(object sender, EventArgs e)
		{
			Globals.Message.ShowInformation(HelpText);
		}

		protected int cursorPosBeforeLosingFocus;
		ZTextBox textBoxToInsertFields;
		ZPropertyInfo propertyToInsertDocumentFieldTo;

		#endregion

		#region BusinessEntity

		public new IncidentEmailTemplatePair CurrentDataItem
		{
			get { return (IncidentEmailTemplatePair)base.CurrentDataItem; }
		}

		#endregion
	}
}
