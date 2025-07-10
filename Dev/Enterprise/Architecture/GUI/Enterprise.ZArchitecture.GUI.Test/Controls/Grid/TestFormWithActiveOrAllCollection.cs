using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Testing
{
	class TestFormWithActiveOrAllCollection : ZForm
	{
		public TestFormWithActiveOrAllCollection(ActiveOrAllBusinessObjectCollection bizo)
			: base(bizo)
		{
			InitializeComponent();
		}

		protected override void InitializeComponent()
		{
			SuspendLayout();

			grid1 = new ZGrid
			{
				Location = ControlDpiScalingHelper.NewScaledPoint(4, 4),
				Size = ControlDpiScalingHelper.NewScaledSize(200, 100),
				BindTo = "."
			};
			grid1.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 100));
			grid1.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Bool, 100));

			Controls.Add(grid1);
		}

		public ZGrid grid1;
	}
}
