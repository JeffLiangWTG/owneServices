using System;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class SetEntryStatusForm : ZChildForm
	{
		public SetEntryStatusForm(SetEntryStatusDetail helper) : base(helper)
		{
			InitializeComponent();
			Icon = Icons.GetIcon(IconTypes.Events);
		}

		public override string FormHeading => base.FormCaption;

		SetEntryStatusDetail setEntryStatusDetail => base.BusinessEntity as SetEntryStatusDetail;

		void OkButton_Click(object sender, EventArgs e)
		{
			setEntryStatusDetail.RunPreSaveValidation();

			if (setEntryStatusDetail.HasErrors)
			{
				ShowErrorsDialog();
				return;
			}

			setEntryStatusDetail.SetEntryStatus();
			Close();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
