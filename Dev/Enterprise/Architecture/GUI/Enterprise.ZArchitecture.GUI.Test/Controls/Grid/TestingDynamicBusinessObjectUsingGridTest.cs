using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestingDynamicBusinessObjectUsingGridTest : TestCaseWithDummy
	{
		public void TestBindDynamicBizO()
		{
			SaveTestData();

			var list = new DynamicBusinessObjectCollection(Factory);
			list.Load("select * from dbo.DUMMYBIZO where Z0_Number = @Num", new ZSqlParameterCollection(ZSqlParameter.New("@Num", 12, DummyBizoSchema.Z0_Number)));

			using (var form = new DynamicBizOTestForm(list))
			{
				form.Show();
				AssertEquals("Should have 2 columns", 2, form.Grid.TableStyles[0].GridColumnStyles.Count);
				AssertEquals("Should have 2 rows", 2, form.Grid.VisibleRowCount);
			}
		}

		void SaveTestData()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "DESC";
			dummy1.Z0_Number = 12;

			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Number = 12;

			Factory.Save();
		}

		public class DynamicBizOTestForm : ZChildForm
		{
			public DynamicBizOTestForm(IBusiness bizO) : base(bizO) { }

			public ZGrid Grid;

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				var oGridTextBoxColumnInfo1 = new ZTextBoxColumnStyleInfo();
				var oGridCalcEditColumnInfo1 = new ZCalcEditColumnStyleInfo();

				Grid = new ZGrid();
				Grid.BindTo = ".";
				Grid.Dock = DockStyle.Fill;
				oGridTextBoxColumnInfo1.ColumnName = "Z0_Description";
				oGridCalcEditColumnInfo1.ColumnName = "Z0_Number";
				Grid.ColumnStyles.Add(oGridTextBoxColumnInfo1);
				Grid.ColumnStyles.Add(oGridCalcEditColumnInfo1);

				Controls.Add(Grid);
			}
		}
	}
}
