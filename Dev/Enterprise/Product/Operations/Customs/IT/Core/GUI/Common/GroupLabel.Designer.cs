namespace Enterprise.Customs.IT.GUI
{
	sealed partial class GroupLabel
	{
		void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// GroupLabel
			// 
			this.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.IsFontBold = true;
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.ResumeLayout(false);
		}
	}
}
