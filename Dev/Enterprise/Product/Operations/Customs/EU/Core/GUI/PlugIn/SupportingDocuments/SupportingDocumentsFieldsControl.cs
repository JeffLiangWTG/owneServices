using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
#if DEBUG
using Enterprise.ZArchitecture.GUI.Testing;
#endif

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class SupportingDocumentsFieldsControl : BaseCustomsEntryUserControl, ISupportingDocumentsFieldsControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public SupportingDocumentsFieldsControl()
		{
			InitializeComponent();
			SupDocReferenceTextBox.AllowOverlap(SupDocReferenceCodeFindBox);
#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(this.CSI_StatusDropEdit);
#endif
		}

		ZCodeFindBox ISupportingDocumentsFieldsControl.CSI_RX_NKCurrencyCodeFindBox => CSI_RX_NKCurrencyCodeFindBox;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			AddVisibleBindings();
		}

		void AddVisibleBindings()
		{
			if (DataSource != null)
			{
				var bindTo = ((SupportingDocumentsUserControl)Parent.Parent).SupportingDocumentsGrid.BindTo;
				SupDocReferenceCodeFindBox.DataBindings.Add(new KBinding(nameof(IsVisibleForBinding), DataSource, bindTo + "." + nameof(SupportingDocument.ShowCodeFindBoxForReferenceNumber)));
				SupDocReferenceTextBox.DataBindings.Add(new KBinding(nameof(IsVisibleForBinding), DataSource, bindTo + "." + nameof(SupportingDocument.ShowTextBoxForReferenceNumber)));
			}
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control is ZCodeFindBox && previousControl is ZTextBox;
		}
	}
}
