using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.Wow
{
	public partial class WowAUCustomsDeclarationForm : ZAUCustomsDeclarationForm
	{
		public WowAUCustomsDeclarationForm(WoolworthsJobDeclaration jobDeclaration) : base(jobDeclaration)
		{
			InitializeComponent();
			jobDeclaration.OrderInvoiceMismatching += new CancelEventHandler(OnOrderInvoiceMismatching);
			jobDeclaration.UpdatingCusContainerNumber += new CancelEventHandler(OnUpdatingCusContainerNumber);
			jobDeclaration.DeletingCusContainer += new CancelEventHandler(OnDeletingCusContainer);
			jobDeclaration.RunOrderInvoiceMismatchValidation();
		}

		#region Implementation

		protected void OnOrderInvoiceMismatching(object sender, CancelEventArgs e)
		{
			DialogResult result = Globals.Message.Show("You cannot modify this field as it will break the link between the order line delivery and the invoice line.",
				"",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);

			// don't allow Woolworths to do this ever because it is bad
			//			if (Result == DialogResult.No)
			//			{
			e.Cancel = true;
			//			}
		}

		protected void OnUpdatingCusContainerNumber(object sender, CancelEventArgs e)
		{
			DialogResult result = Globals.Message.Show("This may result in order container records changing their container numbers. This will take affect after the form is saved. Proceed?",
				"",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Error);
			if (result == DialogResult.No)
			{
				e.Cancel = true;
			}
		}

		protected void OnDeletingCusContainer(object sender, CancelEventArgs e)
		{
			DialogResult result = Globals.Message.Show("This may result in order containers being deleted. If you subsequently re-add this container record, a container will be " +
				"created in the order system and a declaration will be generated on the next pass (as it is considered a new container). " +
				"This will take affect after the form is saved. Proceed?",
				"",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Error);
			if (result == DialogResult.No)
			{
				e.Cancel = true;
			}
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var woolworthsJobDeclaration = (WoolworthsJobDeclaration)BusinessEntity;
				if (woolworthsJobDeclaration != null)
				{
					woolworthsJobDeclaration.OrderInvoiceMismatching -= new CancelEventHandler(OnOrderInvoiceMismatching);
					woolworthsJobDeclaration.UpdatingCusContainerNumber -= new CancelEventHandler(OnUpdatingCusContainerNumber);
					woolworthsJobDeclaration.DeletingCusContainer -= new CancelEventHandler(OnDeletingCusContainer);
				}
			}
			base.Dispose(disposing);
		}
	}
}
