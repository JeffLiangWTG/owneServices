using System;
using System.Drawing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class RequestDateForm : ZChildForm
	{
		public RequestDateForm(ResourceString formTitle, ZString message, ZDateTime defaultDate, ResourceStringData dateCaption)
		{
			this.formTitle = formTitle;
			InitializeRequestDateFormLayout(message, defaultDate, dateCaption);
		}
		readonly ResourceString formTitle;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void InitializeRequestDateFormLayout(ZString message, ZDateTime defaultDate, ResourceStringData dateCaption)
		{
			MessageLabel.Text = message;
			DateEdit.DateTimeValue = defaultDate;
			DateEdit.CaptionResourceString = dateCaption ?? ResourceStringData.Empty;
			ValidateDateEdit();
			DateEdit.DateTextBox.TextChanged += new EventHandler(DateEditTextChanged);
		}

		void DateEditTextChanged(object sender, EventArgs e)
		{
			ValidateDateEdit();
		}

		void ValidateDateEdit()
		{
			if (!DateEdit.DateTimeValue.IsValid)
			{
				OKButton.Enabled = false;
				DateEdit.DateTextBox.ForeColor = Color.Red;
			}
			else if (!OKButton.Enabled)
			{
				OKButton.Enabled = true;
				DateEdit.DateTextBox.ForeColor = base.ForeColor;
			}
		}
		public override string FormVerb => string.Empty;
		public override string FormCaption => formTitle;
	}
}
