using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common.GUI.Testing
{
	[CargoWise.Data.Testing.UseSnapshotProtection]
	public abstract class TariffColumnStyleTest : TestCaseWithFactory
	{
		public abstract TariffColumnStyleInfo GetNewTariffColumnStyleInfo();
		#region TestColumnStylePopulatesTheCurrentBOIntoTheFindBox
		public void TestColumnStylePopulatesTheCurrentBOIntoTheFindBox()
		{
			DummyEnterpriseBusinessObject dummyBO = DummyEnterpriseBusinessObject.New(Factory);
			using (TestFormTakingDummyBO form = new TestFormTakingDummyBO(dummyBO))
			{
				form.Show();
				DummyChildBusinessObject dummyChild1 = dummyBO.Collection.AddNew();
				DummyChildBusinessObject dummyChild2 = dummyBO.Collection.AddNew();
				TariffColumnStyle columnStyle = (TariffColumnStyle)form.Grid.Columns[0].ColumnStyle;
				form.Grid.CurrentCell = new DataGridCell(1, 0);
				form.Grid.CurrentCell = new DataGridCell(0, 0);
				AssertEquals("ColumnStyle.FindBox.ActiveBusinessObject == DummyChild1", dummyChild1, columnStyle.FindBox.ActiveBusinessObject);
				form.Grid.CurrentCell = new DataGridCell(1, 0);
				AssertEquals("ColumnStyle.FindBox.ActiveBusinessObject == DummyChild2", dummyChild2, columnStyle.FindBox.ActiveBusinessObject);
				var columnStyleInfo = (TariffColumnStyleInfo)form.Grid.GetColumnStyle(DummyBizoSchema.Z0_VarCharMax.Name);
				var tariffModuleFilter = columnStyleInfo.GetModuleFilter(DummyBizoSchema.Z0_VarCharMax, "I am a tariff");
				AssertType<ModuleNumberFilter>("We use number filter instead of guid filter for tariff.", tariffModuleFilter);
				AssertEquals("The description should be as specified.", "I am a tariff", tariffModuleFilter.MultilingualDescription);
				AssertEquals("The filter column should be set correctly.", DummyBizoSchema.Z0_VarCharMax, tariffModuleFilter.FilterColumn);
				AssertExceptionThrown<ArgumentException>("We should throw an exception if the tariff column style is bound to some other type schema.", () => columnStyleInfo.GetModuleFilter(DummyBizoSchema.GenericIntSchemaColumn, "I am a tariff, no you are not"));
			}
		}

		#endregion
		#region Implementation
		#region CurrentInstance
		static TariffColumnStyleTest currentInstance;
		protected override void SetUp()
		{
			base.SetUp();
			currentInstance = this;
		}

		protected override void TearDown()
		{
			currentInstance = null;
			base.TearDown();
		}

		#endregion
		#region class TestFormTakingDummyBO
		class TestFormTakingDummyBO : ZArchitecture.GUI.Testing.ZTestForm
		{
			public TestFormTakingDummyBO(DummyBusinessObject dummy) : base(dummy)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Grid.ColumnStyles.Clear();
				TariffColumnStyleInfo columnStyleInfo = TariffColumnStyleTest.currentInstance.GetNewTariffColumnStyleInfo();
				columnStyleInfo.ColumnName = DummyBizoSchema.Z0_VarCharMax.Name;
				columnStyleInfo.BindToList = "Lookups.DummyList";
				Grid.ColumnStyles.Add(columnStyleInfo);
			}
		}
		#endregion
		#endregion
	}
}
