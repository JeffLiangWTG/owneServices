namespace Enterprise.Customs.IE.GUI
{
	public partial class EntryInstructionGridUserControl : EU.GUI.EntryInstructionGridUserControl
	{
		public EntryInstructionGridUserControl()
		{
			InitializeComponent();
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			this.EntryInstructionsGrid.ColumnLayoutContext = (JobDeclaration?.IsExport ?? false) ? nameof(Customs.GUI.DeclarationType.Export) : nameof(Customs.GUI.DeclarationType.Import);
		}
	}
}
