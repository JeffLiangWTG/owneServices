namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class RelatedDeclarationsUserControl : Customs.GUI.BaseRelatedDeclarationsUserControl
	{
		public RelatedDeclarationsUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
			RelatedDeclarationsGrid.SetAvailability(false, [Customs.Business.AutoJobDeclaration.Schema.JE_MessageSubType, Business.JobDeclaration.Schema.JE_DeclarationType]);
		}

		protected override void ChangeGridColumnsVisibility()
		{
			if (JobDeclaration != null)
			{
				RelatedDeclarationsGrid.SetAvailability(!JobDeclaration.AreMultipleEntryInstructionsAllowed, Business.JobDeclaration.Schema.JE_DeclarationType);
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			RelatedDeclarationsGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo()
			{
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105),
				CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("87790976-7752-4194-82DE-1CDA3452D2D3", "Declaration Type"),
				ColumnName = Business.JobDeclaration.Schema.JE_DeclarationType,
				IsReadOnly = true
			});
		}
	}
}
