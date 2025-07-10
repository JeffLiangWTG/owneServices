using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Models;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed partial class MessageSettingsForm : ZChildForm
	{
		MessageSettingsForm()
		{
			InitializeComponent();
		}

		public MessageSettingsForm(BusinessObjectFactory factory, ZString workflowType)
			: base(new MessageSettingsExport(factory, workflowType))
		{
			Argument.NotNull(factory, nameof(factory));

			InitializeComponent();

			messageSettings = (MessageSettingsExport)DataSource;
		}

		readonly MessageSettingsExport messageSettings;

		public string RecipientType => messageSettings.RecipientType;
		public string RecipientTypeDescription => messageSettings.RecipientTypeList.GetDescriptionFromCode(RecipientType);

		public string PurposeCode => messageSettings.PurposeCode;
		public string PurposeCodeDescription => messageSettings.PurposeCodeList.GetDescriptionFromCode(PurposeCode);

		public override string FormHeading => Res.GetString("7b687594-c816-45f4-9c97-e15d8220e8d5", "Select Message Settings");

		void OnSendButtonClick(object sender, EventArgs e)
		{
			messageSettings.RunPreSaveValidation();

			if (messageSettings.HasErrors)
			{
				using (var errorMessageBox = new ZErrorMessageBox(messageSettings))
				{
					ZFormModaliser.ShowDialogAndDispose(errorMessageBox, this);
				}
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void OnCancelButtonClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
