using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CusRefRateCodeViewComparerTest : TestCaseWithFactory
	{
		public void TestComparer()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Brazil;

			var ipiRateType = helper.CreateNewOrGetExistingRateType(dataGrouping, Constants.RateTypes.IPI, Constants.RateTypes.IPI);
			ipiRateType.ZZR_CustomsValueFormula = "CV + DTY";
			var ipiRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "0000", ipiRateType.PK);

			var cofinsRateType = helper.CreateNewOrGetExistingRateType(dataGrouping, Constants.RateTypes.Cofins, Constants.RateTypes.Cofins);
			cofinsRateType.ZZR_CustomsValueFormula = "CV";
			var cofinsRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "5629", cofinsRateType.PK);

			var pisRateType = helper.CreateNewOrGetExistingRateType(dataGrouping, Constants.RateTypes.PIS, Constants.RateTypes.PIS);
			pisRateType.ZZR_CustomsValueFormula = "CV";
			var pisRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "5602", pisRateType.PK);

			var dutyRateType = helper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, Universal.Constants.RateTypes.Duty);
			var dutyRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "0086", dutyRateType.PK);
			Factory.Save();

			AssertEquals(-1, new CusRefRateCodeViewComparer().Compare(dutyRateCode, pisRateCode));
			AssertEquals(1, new CusRefRateCodeViewComparer().Compare(pisRateCode, dutyRateCode));
			AssertEquals(-1, new CusRefRateCodeViewComparer().Compare(dutyRateCode, cofinsRateCode));
			AssertEquals(1, new CusRefRateCodeViewComparer().Compare(cofinsRateCode, dutyRateCode));
			AssertEquals(-1, new CusRefRateCodeViewComparer().Compare(dutyRateCode, ipiRateCode));
			AssertEquals(1, new CusRefRateCodeViewComparer().Compare(ipiRateCode, dutyRateCode));

			var rateCodes = CusRefRateCodeView.Loader.Load(new BusinessObjectFactory(), dataGrouping).OrderBy(x => x, new CusRefRateCodeViewComparer()).ToArray();
			AssertContainsExactElementsInExactOrder(new ZString[] { Constants.RateCodes.ImportDuty, "0000", Constants.RateCodes.PIS, Constants.RateCodes.Cofins },
				rateCodes.Select(x => x.ZY1_RateCode));
		}
	}
}
