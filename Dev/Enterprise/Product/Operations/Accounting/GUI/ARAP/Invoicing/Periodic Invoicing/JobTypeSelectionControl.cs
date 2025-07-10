using System;
using System.ComponentModel;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class JobTypeSelectionControl : ZUserControl
	{
		public JobTypeSelectionControl()
		{
			InitializeComponent();
		}

		[Browsable(false)]
		AllJobTypesForm JobTypesForm
		{
			get
			{
				if (jobTypesForm == null)
				{
					jobTypesForm = new AllJobTypesForm(JobTypesPicker);
					jobTypesForm.JobTypeSelectionChanged += jobTypesForm_JobTypeSelectionChanged;
					JobTypesForm.JobTypeSelectionCanceled += JobTypesForm_JobTypeSelectionCanceled;
				}
				return jobTypesForm;
			}
		}
		AllJobTypesForm jobTypesForm;

		[Browsable(false)]
		JobTypePicker JobTypesPicker
		{
			get
			{
				return BindingSource.Current as JobTypePicker;
			}
		}

		void jobTypesForm_JobTypeSelectionChanged(object sender, EventArgs e)
		{
			JobTypesPicker.SetSelectedJobTypeList();
		}

		void JobTypesForm_JobTypeSelectionCanceled(object sender, EventArgs e)
		{
			JobTypesPicker.UndoSelectedJobTypes();
		}

		void btnSelect_Click(object sender, EventArgs e)
		{
			JobTypesPicker.SetSelectedJobTypeList();
			JobTypesForm.ShowDialog();
		}
	}
}
