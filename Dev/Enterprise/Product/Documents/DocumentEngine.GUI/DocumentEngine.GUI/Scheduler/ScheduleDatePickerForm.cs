using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	public partial class ScheduleDatePickerForm : ZChildForm
	{
		public ScheduleDatePickerForm(ReportScheduleTask businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		public new ReportScheduleTask BusinessEntity
		{
			get { return (ReportScheduleTask)base.BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return FormVerbs.Edit; }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			HandleOKButton();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			HandleCloseButton();
		}

		void HandleOKButton()
		{
			if (FireSaveButton() == ContinueWithSave.Yes)
			{
				Close();
			}
			else
			{
				DialogResult = DialogResult.None;
			}
		}

		void HandleCloseButton()
		{
			BusinessEntity.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Empty;
			Close();
		}
	}
}
