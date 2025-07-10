using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class InvoiceLineDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
		}

		public void TestCommercialReferenceBox()
		{
			AssertType<ZTextBox>(control.CommercialReferenceTextBox);
		}

		public void TestT2LItemNumberCalcEdit()
		{
			AssertType<ZCalcEdit>(control.T2LItemNumberCalcEdit);
		}

		public void TestCountryOfDestinationCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.CountryOfDestinationCodeFindBox);
		}

		public void TestRegionOfDestinationCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.RegionOfDestinationCodeFindBox);
		}

		public void TestMethodOfPaymentDropEdit()
		{
			AssertType<ZDropEdit>(control.MethodOfPaymentDropEdit);
		}

		public void TestMethodOfPayment2DropEdit()
		{
			AssertType<ZDropEdit>(control.MethodOfPayment2DropEdit);
		}

		public void TestVATIGICTypeDropEdit()
		{
			AssertType<ZDropEdit>(control.VATIGICTypeDropEdit);
		}

		public void TestAIEMTypeDropEdit()
		{
			AssertType<ZDropEdit>(control.AIEMTypeDropEdit);
		}

		public void TestExciseExemptionDropEdit()
		{
			AssertType<ZDropEdit>(control.ExciseExemptionDropEdit);
		}

		public void TestGlobalWarmingPotentialCalcEdit()
		{
			AssertType<ZCalcEdit>(control.GlobalWarmingPotentialCalcEdit);
		}

		public void TestExciseCodeDropEdit()
		{
			AssertType<ZDropEdit>(control.ExciseCodeDropEdit);
		}

		public void TestPVPCalcFindBox()
		{
			AssertType<ZCalcFindBox>(control.PVPCalcFindBox);
		}

		public void TestREAProductCodeDropEdit()
		{
			AssertType<ZDropEdit>(control.REAProductCodeDropEdit);
		}

		public void TestREADirectConsumptionCheckBox()
		{
			AssertType<ZCheckBox>(control.READirectConsumptionCheckBox);
		}

		public void TestHasNonRecycledPlasticsCheckBox()
		{
			AssertType<ZCheckBox>(control.HasNonRecycledPlasticsCheckBox);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new InvoiceLineDetailsUserControl();
		}
		InvoiceLineDetailsUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
