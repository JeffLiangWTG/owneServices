using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class ExportEntryInstructionLayoutTemplate : ZUserControl
	{
		public ExportEntryInstructionLayoutTemplate()
		{
			InitializeComponent();
			VanningLocationsGrid.MaximumRows = VanningAddressCollection.MaxRowCount;
		}
	}
}
