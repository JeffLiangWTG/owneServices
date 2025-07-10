using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class ArrivalTransportInfosGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>), userControl.BindingSource.DataSourceType);
		}

		public void TestGrid()
		{
			var grid = userControl.TransportInfoGrid;
			CombineAssertions(() =>
			{
				AssertType<ZGrid>("Type", grid);
				AssertEquals("BindingMember", ".", grid.GetBindingMember());
			});
		}

		public void TestColumnStyles()
		{
			var grid = userControl.TransportInfoGrid;
			CombineAssertions(() =>
			{
				AssertColumnStyle<ZCalcEditColumnStyleInfo>("TPM_SequenceNumber");
				AssertColumnStyle<ZDropEditColumnStyleInfo>("TPM_TransportState");
				AssertColumnStyle<ZDropEditColumnStyleInfo>("TPM_TypeOfIdentification");
				AssertColumnStyle<ZTextBoxColumnStyleInfo>("TPM_IdentificationNumber");
				AssertColumnStyle<ZCodeFindBoxColumnStyleInfo>("TPM_RN_NKTransportNationality");
			});

			void AssertColumnStyle<T>(string name) where T : ZGridColumnInfo => AssertType<T>(name, grid.GetColumnStyle(name));
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ArrivalTransportInfosGridUserControl();
		}
		ArrivalTransportInfosGridUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
