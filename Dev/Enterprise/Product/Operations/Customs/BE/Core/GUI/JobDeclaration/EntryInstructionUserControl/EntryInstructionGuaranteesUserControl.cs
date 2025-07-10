using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI
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
