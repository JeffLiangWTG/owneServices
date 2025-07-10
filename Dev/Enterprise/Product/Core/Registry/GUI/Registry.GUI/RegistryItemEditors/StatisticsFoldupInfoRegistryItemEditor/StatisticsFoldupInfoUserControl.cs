using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class StatisticsFoldupInfoUserControl : RegistryZUserControl
	{
		public StatisticsFoldupInfoUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			StatisticsInfoFoldupGrid.ReadOnly = readOnly;
		}

		void StatisticsInfoFoldupGrid_Navigate(object sender, NavigateEventArgs ne)
		{
		}
	}
}
