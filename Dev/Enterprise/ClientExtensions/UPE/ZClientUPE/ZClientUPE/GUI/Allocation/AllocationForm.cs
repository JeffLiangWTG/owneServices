using System;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class AllocationForm : ZChildForm
	{
		public AllocationForm()
		{
		}

		public AllocationForm(Allocation allocation)
			: base(allocation)
		{
		}

		public Allocation Allocation
		{
			get { return (Allocation)BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return "Classifier Allocation"; }
		}

		void Process()
		{
			Allocation.RunPreSaveValidation();

			if (Allocation.HasErrors)
			{
				Globals.Message.ShowError("Please fix the errors before proceeding.");
			}
			else
			{
				try
				{
					Cursor.Current = Cursors.WaitCursor;
					Allocation.ReAllocate();
					Allocation.ReLoadClassifierAllocation();
				}
				finally
				{
					Cursor.Current = Cursors.Default;
				}
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void RefreshButton_Click(object sender, EventArgs e)
		{
			Allocation.RunPreSaveValidation();

			if (Allocation.HasErrors)
			{
				Globals.Message.ShowError("Please fix the errors before proceeding.");
			}
			else
			{
				Allocation.ReLoadClassifierAllocation();
			}
		}

		void ReAllocateButton_Click(object sender, EventArgs e)
		{
			Process();
		}
	}
}
