using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Models;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	[SuppressFormsLocalizedTest]
	partial class UserDropDownConfirmationDialog : ZChildForm
	{
		// for the designer only
		UserDropDownConfirmationDialog()
		{
			InitializeComponent();
		}

		public UserDropDownConfirmationDialog(string message, string caption, ICodeDescriptionPairList optionsList)
			: base(new UserDropDownConfirmationModel(optionsList))
		{
			InitializeComponent();

			model = (UserDropDownConfirmationModel)DataSource;

			FormHeading = caption;
			messageLabel.Text = message;
		}

		readonly UserDropDownConfirmationModel model;

		public override string FormHeading { get; }

		public ICodeDescription Option
		{
			get
			{
				if (model == null)
				{
					return null;
				}

				var optionCode = model.Option;

				foreach (var option in model.OptionsList.OfType<ICodeDescription>())
				{
					if (string.Compare(option.Code, optionCode, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return option;
					}
				}

				return null;
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			okButton.Click += OnSendButtonClick;
			cancelButton.Click += OnCancelButtonClick;
		}

		void OnSendButtonClick(object sender, EventArgs e)
		{
			model.RunPreSaveValidation();

			if (model.HasErrors)
			{
				using (var errorMessageBox = new ZErrorMessageBox(model))
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