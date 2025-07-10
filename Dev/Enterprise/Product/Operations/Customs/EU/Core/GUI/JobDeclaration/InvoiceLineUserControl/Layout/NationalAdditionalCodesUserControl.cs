using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class NationalAdditionalCodesUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
	{
		public NationalAdditionalCodesUserControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		void NationalAdditionalCodesEditButton_OnClick(object sender, System.EventArgs e)
		{
			var invoiceLine = (JobComInvoiceLine)CurrentDataItem;
			if (invoiceLine != null)
			{
				NationalAdditionalCodesForm.ShowDialog(invoiceLine);
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		public Control Host => this;

		public IControlExtensionCollection Extensions { get; }

		public string ResourceStringBindingMember => nameof(JobComInvoiceLine.JI_NationalAdditionalCodes);

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (Extensions != null)
				{
					Extensions.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
