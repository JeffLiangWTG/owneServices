using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobManagement
{
	public partial class BulkJobCloseForm : ZChildForm
	{
		public BulkJobCloseForm() : base()
		{
		}
		public BulkJobCloseForm(BulkJobCloseProcessor bulkJobCloseProcessor)
			: base(bulkJobCloseProcessor)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			zNote4.Visible = AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.Value;

			this.BindingSource.SetBindingMember(this.zDateRangeControl1, "JobOpenDateFilter");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((BulkJobCloseProcessor)(null)).JobOpenDateFilter);

			this.BindingSource.SetBindingMember(this.zDateRangeControl2, "JobLastEditDateFilter");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((BulkJobCloseProcessor)(null)).JobLastEditDateFilter);
		}

		void btnFind_Click(object sender, EventArgs e)
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;

				var processor = BusinessEntity as BulkJobCloseProcessor;
				if (processor != null)
				{
					string msg = processor.Find();
					if (!string.IsNullOrWhiteSpace(msg))
					{
						Globals.Message.ShowError(msg);
					}
				}

				this.Cursor = Cursors.Default;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				this.Cursor = Cursors.Default;
				Globals.Message.ShowError(ex.Message);
			}
		}

		void btnCloseJob_Click(object sender, EventArgs e)
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;

				var processor = BusinessEntity as BulkJobCloseProcessor;
				if (processor != null && processor.JobPKs.Count > 0)
				{
					int closed = processor.CloseJobInBulk();
					if (closed > 0)
					{
						Globals.Message.Show(Res.GetString("4ab5581e-4e59-4385-acb4-4cc08ceb7188", "{0} out of {1} Job(s) have been closed.", closed, processor.JobPKs.Count));
						processor.Clear();
					}
					else
					{
						Globals.Message.Show(Res.GetString("3500399c-31a5-43eb-a916-b96d248ffe7f", "No Job has been closed."));
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("2fe9c29c-59b7-445a-91ed-312d52af53c3", "There is no Job to close. Please click 'Find' to select jobs that you want to close"));
				}

				this.Cursor = Cursors.Default;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message);
				this.Cursor = Cursors.Default;
			}
		}

		public override string FormCaption
		{
			get { return Res.GetString("b5459345-3d27-4938-8646-746c342dffb8", "Close Job in Bulk"); }
		}

		void btnClear_Click(object sender, EventArgs e)
		{
			try
			{
				var processor = BusinessEntity as BulkJobCloseProcessor;
				if (processor != null)
				{
					processor.Clear();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		void btnClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			var processor = BusinessEntity as BulkJobCloseProcessor;
			if (processor != null && processor.JobPKs.Count > 0)
			{
				base.ZForm_Closing(sender, e);
			}
		}

		protected override string FormClosingQuestion
		{
			get { return Res.GetString("66b3330e-a48d-4165-bdeb-e1048ba1294c", "Would you like to exit without Closing Job?"); }
		}
	}
}
