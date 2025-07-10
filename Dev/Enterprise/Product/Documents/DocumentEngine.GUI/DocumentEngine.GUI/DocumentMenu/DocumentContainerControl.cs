using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	[SuppressBindingMemberBashingTest]
	[SuppressFormsLocalizedTestAttribute]
	partial class DocumentContainerControl : ZUserControl
	{
		public DocumentContainerControl()
		{
			InitializeComponent();
			this.CaptionRenderingEnabled = true;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			// don't call base - no binding necessary
		}
	}
}
