using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Testing
{
	class TestForm : ZForm
	{
		public TestForm(DummyWithActiveCollection bizo)
			: base(bizo)
		{
			InitializeComponent();
		}

		new void InitializeComponent()
		{
			SuspendLayout();

			grid1 = new ZGrid
			{
				Location = ControlDpiScalingHelper.NewScaledPoint(4, 4),
				Size = ControlDpiScalingHelper.NewScaledSize(200, 100),
				BindTo = "ActiveCollection"
			};
			grid1.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 100));

			grid2 = new ZGrid
			{
				Location = ControlDpiScalingHelper.NewScaledPoint(4, 108),
				Size = ControlDpiScalingHelper.NewScaledSize(200, 100),
				BindTo = "ActiveCollection.ActiveCollection"
			};
			grid2.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 100));

			Controls.Add(grid1);
			Controls.Add(grid2);

			ResumeLayout(true);
		}

		public ZGrid grid1;
		public ZGrid grid2;
	}
}
