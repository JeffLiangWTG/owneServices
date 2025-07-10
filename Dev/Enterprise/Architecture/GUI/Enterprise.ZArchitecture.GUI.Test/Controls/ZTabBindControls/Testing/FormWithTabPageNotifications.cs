using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[SuppressFormDesignerAnalysis]
	sealed partial class FormWithTabPageNotifications : ZForm
	{
		public FormWithTabPageNotifications(BusinessObject businessObject)
			: base(businessObject)
		{
		}

		private System.ComponentModel.IContainer components;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private ZTabControl TabControl;

		public ZTabPage TabPage1;

		public ZTabPage TabPage2;

		internal ZGrid Grid;

		private ZLabel zLabel1;

		public ZCalcEdit txtNumber;

		public TestBindingTabPage TabPage3;

		public UserControlForTestTabPage InnerControl;

		public TestBindingTabPage TabPage4;

		private ZTabControl TabControlWithDynamicCreationControl;

		private ZTabPage TabPageWithDynamicCreationControl;

		private ZDynamicControlCreationUserControl DynamicControlCreationUserControl;
	}
}
