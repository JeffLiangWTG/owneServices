namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class DestinationLocationOfGoodsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MoreButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LocationOfGoodsDescription = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// MoreButton
			// 
			this.MoreButton.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("79013660-FDAC-4BAA-B5C8-3987CB8D423B", "More..");
			this.MoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 0, true);
			this.MoreButton.Name = "MoreButton";
			this.MoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.MoreButton.TabIndex = 1;
			this.MoreButton.ToolTipCaption = null;
			this.MoreButton.Click += new System.EventHandler(this.MoreButton_Click);
			// 
			// LocationOfGoodsDescription
			// 
			this.BindingSource.SetBindingMember(this.LocationOfGoodsDescription, "DestinationGoodsLocationDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).DestinationGoodsLocationDescription)));
			this.LocationOfGoodsDescription.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocationOfGoodsDescription, false);
			this.LocationOfGoodsDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LocationOfGoodsDescription.Name = "LocationOfGoodsDescription";
			this.LocationOfGoodsDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.LocationOfGoodsDescription.TabIndex = 0;
			// 
			// LocationOfGoodsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MoreButton);
			this.Controls.Add(this.LocationOfGoodsDescription);
			this.Name = "DestinationLocationOfGoodsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal ZArchitecture.GUI.ZButton MoreButton;
		protected internal ZArchitecture.ZTextBox LocationOfGoodsDescription;
	}
}
