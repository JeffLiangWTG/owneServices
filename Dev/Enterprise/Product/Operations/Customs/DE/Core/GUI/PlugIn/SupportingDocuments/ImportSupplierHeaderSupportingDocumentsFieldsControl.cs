using System.Windows.Forms;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	public partial class ImportSupplierHeaderSupportingDocumentsFieldsControl : BaseCustomsEntryUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public ImportSupplierHeaderSupportingDocumentsFieldsControl()
		{
			InitializeComponent();
			CSI_ReferenceNumberCodeFindBox.AllowOverlap(CSI_ReferenceNumberTextBox);
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control is ZTextBox && previousControl is ZCodeFindBox);
		}
	}
}
