using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EntryInstructionGuaranteesUserControl : ZUserControl
	{
		public EntryInstructionGuaranteesUserControl()
		{
			InitializeComponent();
			UpdateGridColumnLayout();
		}

		protected virtual IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new EntryInstructionGuaranteeGridColumnsLayout();

		void UpdateGridColumnLayout()
		{
			GuaranteesGrid.ApplyGridColumnLayout(GetGridColumnLayoutProvider());
		}
	}
}
