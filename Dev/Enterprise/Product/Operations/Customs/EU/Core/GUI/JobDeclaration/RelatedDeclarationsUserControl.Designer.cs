using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class RelatedDeclarationsUserControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZCalcEditColumnStyleInfo box6 = new ZArchitecture.ZCalcEditColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.RelatedDeclarationsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// RelatedDeclarationsGrid
			// 
			box6.Caption = null;
			box6.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("11111111-5c93-4b5e-8996-ce43d3c2c309", "#Pieces");
			box6.ColumnName = "JE_TotalNoOfPacks";
			this.RelatedDeclarationsGrid.ColumnStyles.Add(box6);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobDeclaration);
			// 
			// RelatedDeclarationsUserControl
			// 
			this.Name = "RelatedDeclarationsUserControl";
			((System.ComponentModel.ISupportInitialize)(this.RelatedDeclarationsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
