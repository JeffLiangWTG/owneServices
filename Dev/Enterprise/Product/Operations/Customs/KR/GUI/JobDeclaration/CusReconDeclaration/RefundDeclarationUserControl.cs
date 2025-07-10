using System;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class RefundDeclarationUserControl : ZUserControl
	{
		public RefundDeclarationUserControl()
		{
			InitializeComponent();
			RefundDeclarationDetailsPanel.UpdateLayout(new RefundDeclarationDetailsLayout());
			CustomsDetailsPanel.UpdateLayout(new CustomsDetailLayout());
			PayerPanel.UpdateLayout(new PayerLayout());
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (CusReconDeclaration != null)
			{
				CusReconDeclaration.CRD_RefundCauseCodeInfo.ValueChanged -= HasItem_ValueChanged;
				CusReconDeclaration.CRD_RefundCauseCodeInfo.ValueChanged += HasItem_ValueChanged;
				UpdateItemsVisiblity();
			}
		}
		void HasItem_ValueChanged(object sender, EventArgs e)
		{
			UpdateItemsVisiblity();
		}
		void UpdateItemsVisiblity()
		{
			CustomsDetailsPanel.UpdateLayout(new CustomsDetailLayout());
		}
		CusReconDeclaration CusReconDeclaration
		{
			get
			{
				if (BindingSource?.DataSource is CusReconDeclaration declaration)
				{
					return declaration;
				}
				return null;
			}
		}
	}
}
