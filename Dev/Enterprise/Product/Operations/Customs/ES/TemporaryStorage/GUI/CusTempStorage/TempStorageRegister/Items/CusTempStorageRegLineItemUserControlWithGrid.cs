using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public partial class CusTempStorageRegLineItemUserControlWithGrid : ZUserControl
	{
		public CusTempStorageRegLineItemUserControlWithGrid()
		{
			InitializeComponent();
			ItemsGrid.AfterBind += ItemsGrid_AfterBind;
			ItemDetailsLayoutControl.AllowOutsideOfParent();
		}

		void ItemsGrid_AfterBind(object sender, EventArgs e)
		{
			SetItemsGridLayout();
		}

		protected void SetItemsGridLayout()
		{
			ItemDetailsLayoutControl.SetLayout(CreateNewCusTempStorageRegLineItemDetailsLayout());
		}

		protected virtual IPanelLayoutProvider CreateNewCusTempStorageRegLineItemDetailsLayout() => new CusTempStorageRegLineItemDetailsLayout();
	}
}
