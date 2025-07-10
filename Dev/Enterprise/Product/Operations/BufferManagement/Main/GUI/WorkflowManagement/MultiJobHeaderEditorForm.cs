using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class MultiJobHeaderEditorForm : ZEditForm
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for handling disposables in factory methods")]
		public static MultiJobHeaderEditorForm ShowFormIfAllowed(IEnumerable<BusinessObject> scheduledEntities, BusinessObjectFactory factory)
		{
			if (Env.Security.WorkflowHeadersMultiJobScheduling.IsAllowed)
			{
				var form = new MultiJobHeaderEditorForm(scheduledEntities, factory);

				try
				{
					form.Show();
				}
				catch
				{
					try
					{
						form.Dispose();
					}
					catch { }
					throw;
				}

				return form;
			}
			else
			{
				Env.Security.WorkflowHeadersMultiJobScheduling.ShowError();

				return null;
			}
		}

		MultiJobHeaderEditorForm(IEnumerable<BusinessObject> scheduledEntities, BusinessObjectFactory factory)
			: base(new MultiJobHeaderEditorViewModel(scheduledEntities, factory))
		{
			InitializeComponent();
		}

		#region ZForm Overrides

		protected override bool AllowNew
		{
			get { return false; }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion

		protected internal void ShowErrorsDialog()
		{
			base.ShowErrorsDialog();
		}

		public ZGrid SchedulesGrid => JobHeaderEditorControl.SchedulesGrid;
	}
}
