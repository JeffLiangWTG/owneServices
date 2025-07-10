using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class InvoiceLineAlcoholicStrengthUserControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(EMCSJobComInvoiceLine), control.BindingSource.DataSourceType);
		}

		public void TestAlcoholicUnitLabel()
		{
			AssertType<ZLabel>(control.AlcoholicUnitLabel);
		}
		public void TestAlcoholicStrengthCalcEdit()
		{
			AssertType<ZCalcEdit>(control.AlcoholicStrengthCalcEdit);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new InvoiceLineAlcoholicStrengthUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		InvoiceLineAlcoholicStrengthUserControl control;
	}
}
