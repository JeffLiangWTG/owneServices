using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class BulkAddDiscountForm : ZChildForm
	{
		public BulkAddDiscountForm(BulkAddDiscountBizO bizO)
			: base(bizO)
		{
			InitializeComponent();
		}

		BulkAddDiscountBizO BulkAddDiscountBizO
		{
			get { return (BulkAddDiscountBizO)BusinessEntity; }
		}

		void ValidateAndSaveButton_Click(object sender, EventArgs e)
		{
			if (BulkAddDiscountBizO.NewDiscountCollection.Count == 0)
			{
				Globals.Message.ShowError("Please enter at least one new discount.");
				return;
			}

			try
			{
				bool result = BulkAddDiscountBizO.ValidateAndSave();
				if (!result)
				{
					ShowErrorsDialog();
				}
				else
				{
					if (Globals.Message.Show("New discounts have been successfully saved.", "Complete", MessageBoxButtons.OK, DialogResult.OK) == DialogResult.OK)
					{
						Close();
					}
				}
			}
			catch (ZSaveException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}

