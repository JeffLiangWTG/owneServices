using System;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class EditClearanceInfoForm : ZChildForm
	{
		public EditClearanceInfoForm(ClearanceInfo editClearanceInfo)
			: base(editClearanceInfo)
		{
			this.editClearanceInfo = editClearanceInfo;
			InitializeEditClearanceInfoLayout();
			InitializeClearanceDateFormLayout();
			InitializeArrivalLimitDateFormLayout();
		}
		readonly ClearanceInfo editClearanceInfo;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void InitializeClearanceDateFormLayout()
		{
			ClearanceDate.DateTimeValue = editClearanceInfo.ClearanceDate;
			ValidateClearanceDate();
			ClearanceDate.DateTextBox.TextChanged += new EventHandler(ClearanceDateTextChanged);
		}

		void InitializeArrivalLimitDateFormLayout()
		{
			ArrivalLimitDate.DateTimeValue = editClearanceInfo.ArrivalLimitDate;
			ValidateArrivalLimitDate();
			ArrivalLimitDate.DateTextBox.TextChanged += new EventHandler(ArrivalLimitDateTextChanged);
		}

		void ClearanceDateTextChanged(object sender, EventArgs e)
		{
			ValidateClearanceDate();
		}

		void ArrivalLimitDateTextChanged(object sender, EventArgs e)
		{
			ValidateArrivalLimitDate();
		}

		void InitializeEditClearanceInfoLayout()
		{
			ClearanceNumber.MaxLength = 16;
			ClearanceNumber.CharacterCasing = (System.Windows.Forms.CharacterCasing)ZCharacterCasing.Upper;
		}

		void ValidateClearanceDate()
		{
			if (!ClearanceDate.DateTimeValue.IsValid)
			{
				OKButton.Enabled = false;
			}
			else if (!OKButton.Enabled)
			{
				OKButton.Enabled = true;
			}
		}

		void ValidateArrivalLimitDate()
		{
			if (!ArrivalLimitDate.DateTimeValue.IsValid && ArrivalLimitDate.DateTimeValue > ClearanceDate.DateTimeValue)
			{
				OKButton.Enabled = false;
			}
			else if (!OKButton.Enabled)
			{
				OKButton.Enabled = true;
			}
		}

		public override string FormVerb => string.Empty;

		public override string FormCaption => Res.GetString("AC88AF0E-86A2-4981-85C8-98B41AA4840E", "Edit Clearance Info");
	}
}
