using CargoWise.Windows.UI;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class EMCSInvoiceLineArrivalInformationUserControl : ZUserControl
	{
		public EMCSInvoiceLineArrivalInformationUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource is EMCSJobDeclaration declaration)
			{
				ReportOfReceiptGroupBox.SetReadOnly(declaration.IsConsignor);
			}
		}
	}
}
