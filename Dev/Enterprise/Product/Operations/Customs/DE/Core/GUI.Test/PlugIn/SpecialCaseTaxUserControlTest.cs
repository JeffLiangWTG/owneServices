using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.PlugIn.Testing
{
	public class SpecialCaseTaxUserControlTest : TestCaseWithFactory
	{
		public void TestTaxMethodOfPaymentDropEdit()
		{
			using (var userControl = new SpecialCaseTaxUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				userControl.SetDataBinding(declaration, ZString.Empty);

				var rateCalcEdit = userControl.FindSingle<ZCalcEdit>("TaxJLT_RateCalcEdit");
				AssertEquals(5, rateCalcEdit.DecimalPlaces);
			}
		}
	}
}
