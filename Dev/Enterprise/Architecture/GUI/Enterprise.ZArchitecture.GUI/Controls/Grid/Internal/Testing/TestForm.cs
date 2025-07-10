using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture
{
#if DEBUG

	[SuppressFormDesignerAnalysis]
	public partial class TestForm : KForm
	{
		public TestForm()
		{
			InitializeComponent();
		}

		public ZGrid Grid
		{
			get { return zGrid1; }
		}
	}
#endif
}
