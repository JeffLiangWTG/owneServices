using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	class AUContainerUserControl : BaseCustomsCusContainersWithTrackingUserControl
	{
		public AUContainerUserControl()
		{
			AddColumnsToGrid();
		}

		public new JobDeclaration JobDeclaration
		{
			get { return base.JobDeclaration as JobDeclaration; }
		}

		#region override

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.SealStartNumber).IsVisible = JobDeclaration != null && JobDeclaration.IsQuarantine;
			CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.SealEndNumber).IsVisible = JobDeclaration != null && JobDeclaration.IsQuarantine;
		}

		#endregion

		#region AddColumnsToGrid

		void AddColumnsToGrid()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo11.Caption = "Seal Start Number";
			zTextBoxColumnStyleInfo11.ColumnName = CusContainer.Schema.SealStartNumber;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo11, 120, true);
			zTextBoxColumnStyleInfo11.IsVisible = true;
			this.CusContainersBoundGrid.ColumnStyles.Insert(2, zTextBoxColumnStyleInfo11);

			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo12.Caption = "Seal End Number";
			zTextBoxColumnStyleInfo12.ColumnName = CusContainer.Schema.SealEndNumber;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo12, 120, true);
			zTextBoxColumnStyleInfo12.IsVisible = true;
			this.CusContainersBoundGrid.ColumnStyles.Insert(3, zTextBoxColumnStyleInfo12);
		}

		#endregion
	}
}
