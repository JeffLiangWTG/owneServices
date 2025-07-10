using System;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class DV1UserControl : ZUserControl
	{
		public DV1UserControl()
		{
			InitializeComponent();
			DV1GridUserControl.UserControlType = GetGridUserControl();
			DynamicDV1DetailsPanel.UpdateLayout(GetDv1DetailsLayout());
		}

		protected virtual Type GetGridUserControl() => typeof(DV1GridUserControl);

		public virtual IPanelLayoutProvider GetDv1DetailsLayout() => new DV1DetailsLayout();

		public BaseJobDeclaration JobDeclaration => (BaseJobDeclaration)CurrentDataItem;
	}
}
