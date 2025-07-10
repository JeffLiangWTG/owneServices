using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class HouseConsignmentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			SetUpData();
			var columnNames = houseConsignmentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
			IGridColumnLayoutProvider layoutProvider = new HouseConsignmentDetailsGridColumnLayout();
			var expectedColumnNames = layoutProvider.Layout.Columns.Select(x => x.ColumnName);
			AssertContainsExactElementsInExactOrder(expectedColumnNames, columnNames);
		}

		public void TestColumns_InPhase5TransitionPeriod()
		{
			var gridColumnLayoutMock = new Mock<IGridColumnLayout>();
			gridColumnLayoutMock.Setup(m => m.Columns).Returns([new ZDropEditColumnStyleInfo(CusInBondBillSchema.Constants.B0_RN_NKCountryOfExport, 115)]);

			var gridColumnLayoutProviderMock = new Mock<IGridColumnLayoutProvider>();
			gridColumnLayoutProviderMock.Setup(m => m.Layout).Returns(gridColumnLayoutMock.Object);

			var nctsPhase5LayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new NctsPhase5LayoutProviderForTest(gridColumnLayoutProviderMock.Object)) }
			};

			using (ObjectFactory.Substitute("NctsPhase5LayoutProviders", nctsPhase5LayoutProviders))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				SetUpData();
				var columnNames = houseConsignmentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
				var layoutProvider = gridColumnLayoutProviderMock.Object;
				var expectedColumnNames = layoutProvider.Layout.Columns.Select(x => x.ColumnName);
				AssertContainsExactElementsInExactOrder(expectedColumnNames, columnNames);
			}
		}

		void SetUpData()
		{
			var header = Factory.New<NctsHeader>();
			userControl = new HouseConsignmentsGridUserControl();
			userControl.SetDataBinding(header, "");
			houseConsignmentsGrid = userControl.HouseConsignmentsGrid;
		}

		HouseConsignmentsGridUserControl userControl;
		ZGrid houseConsignmentsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		sealed class NctsPhase5LayoutProviderForTest(IGridColumnLayoutProvider gridColumnLayoutProvider) : NctsPhase5LayoutProvider, INctsPhase5TransitionPeriodLayoutProvider
		{
			IGridColumnLayoutProvider INctsPhase5TransitionPeriodLayoutProvider.GetHouseConsignmentDetailsGridColumnLayout() => gridColumnLayoutProvider;
		}
	}
}
