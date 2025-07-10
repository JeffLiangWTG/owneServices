using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Forms.Test
{
	partial class DebugInfoFormWithGrid : ZForm, IDevToolMessageBuilderMappingPathConfigurator
	{
		public DebugInfoFormWithGrid()
		{
			InitializeComponent();
		}

		public DebugInfoFormWithGrid(object dataSource) : base(dataSource)
		{
			InitializeComponent();
			OtherTextBox.DataBindings.Add("Text", dataSource, "Z0_Description");
		}

		internal ZGrid ItemsGrid;
		internal ZTextBox OtherTextBox;

		bool IDevToolMessageBuilderMappingPathConfigurator.CanDisplayMessageBuilderMappingPath => true;
	}
}
