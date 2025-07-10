using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class ContractNumberCollectionForm : ZChildForm
	{
		public ContractNumberCollectionForm(JobComInvoiceHeaderContractCollection contractNumbers)
			: base(contractNumbers)
		{
			this.ContractNumbers = contractNumbers;
		}

		public readonly JobComInvoiceHeaderContractCollection ContractNumbers;

		public static void ShowDialog(JobComInvoiceHeaderContractCollection contractNumbers)
		{
			ZFormModaliser.ShowDialogAndDispose(new ContractNumberCollectionForm(contractNumbers));
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb => "";

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			fOldItems = ContractNumbers.ContractNumbersAsString;
		}

		ZString fOldItems;

		protected override void OnClosing(CancelEventArgs e)
		{
			CloseButton.Focus();

			if (DialogResult == DialogResult.Cancel)
			{
				ContractNumbers.ContractNumbersAsString = fOldItems;
			}
			else
			{
				this.BusinessEntity.RunPreSaveValidation();
				foreach (var item in ContractNumbers)
				{
					if (item.NotificationsIncludingChildren.GetErrors().Any())
					{
						Globals.Message.ShowError(ResString.GetMultilingualString("5BA5B9A3-1050-4525-8C67-604378305950", "The form has errors. Please fix them before continuing."));
						e.Cancel = true;
						break;
					}
				}
			}

			base.OnClosing(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void OnOKButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
