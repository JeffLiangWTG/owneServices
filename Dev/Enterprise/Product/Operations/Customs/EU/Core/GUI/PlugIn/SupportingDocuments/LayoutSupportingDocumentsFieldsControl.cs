using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class LayoutSupportingDocumentsFieldsControl : ZUserControl, ISupportingDocumentsFieldsControl
	{
		public LayoutSupportingDocumentsFieldsControl()
		{
			InitializeComponent();
		}

		public LayoutSupportingDocumentsFieldsControl(JobDeclaration declaration) : this()
		{
			JobDeclaration = declaration;
		}

		protected JobDeclaration JobDeclaration { get; }

		protected bool IsUCC6AndIsExport => JobDeclaration?.IsUCC6AndIsExport ?? true;

		protected virtual void SetCaption()
		{
			SupportingDocumentsGroupBox.CaptionResourceString = IsUCC6AndIsExport ? Res.GetData("B5280C3C-429F-49E8-B040-D7F95E64638C", "[2/3] Supporting Documents") : Res.GetData("6B588A89-2A5F-4638-9DF6-90A7ED25BD50", "[44] Supporting Documents");
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null && oldDataSource != dataSource)
			{
				oldDataSource = dataSource;
				SetCaption();
				dynamicDetailsPanel.UpdateLayout(GetLayout());
			}
		}
		object oldDataSource;

		protected virtual IPanelLayoutProvider GetLayout() => IsUCC6AndIsExport ? new UCC6AndExportSupportingDocumentFieldsLayout() : new NonUCC6SupportingDocumentFieldsLayout();
		ZCodeFindBox ISupportingDocumentsFieldsControl.CSI_RX_NKCurrencyCodeFindBox => null;
	}
}
