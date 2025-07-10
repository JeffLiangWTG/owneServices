
namespace Enterprise.Customs.GB.GUI
{
	partial class GBRelatedDeclarationsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			// 
			// RelatedDeclarationsGrid
			// 
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("fb4f226c-ee73-4b8c-860f-4be070e4c85f", "MUCR");
			zTextBoxColumnStyleInfo1.ColumnName = "JE_MasterUCR";
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("05478b5a-5c93-4b5e-8996-ce43d3c2c309", "Split");
			zTextBoxColumnStyleInfo2.ColumnName = "ZG_HouseSplitReference";
			this.RelatedDeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RelatedDeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
		}

		#endregion
	}
}
