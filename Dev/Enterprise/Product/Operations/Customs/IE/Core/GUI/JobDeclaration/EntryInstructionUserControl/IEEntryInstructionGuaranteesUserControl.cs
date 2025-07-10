using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class IEEntryInstructionGuaranteesUserControl : EntryInstructionGuaranteesUserControl
	{
		public IEEntryInstructionGuaranteesUserControl()
		{
			InitializeComponent();
		}

		protected override IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new IEEntryInstructionGuaranteeGridColumnsLayout();
	}
}
