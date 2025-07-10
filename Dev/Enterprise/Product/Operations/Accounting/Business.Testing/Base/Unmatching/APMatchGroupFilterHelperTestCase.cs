using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Unmatching.Testing
{
	[TestedType(typeof(APMatchGroupFilterHelper))]
	public class APMatchGroupFilterHelperTestCase : MatchGroupFilterHelperTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new APMatchGroupFilterHelper(Factory);
		}

		public void TestFilterStringGroupBy()
		{
			SetupDataForFilteringTest();

			DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 3 AP UnmatchingRow objects : M00001334, M00001523 and M00009332", 3, dynBizOs.Count);

			TransactionMatchLink link = new TransactionMatchLinkGroup(Factory).AddNew();
			link.AP_AH = Matchlink5.AP_AH;
			link.AP_MatchDate = ZDateTime.Now.AddDays(1);
			Matchlink5.Delete();

			Factory.Save();
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 4 AP UnmatchingRow objects due to the GROUP BY MatchDate", 4, dynBizOs.Count);
		}

		#region TestFilterDoesNotExcludePayment

		public void TestFilterDoesNotExcludePayment()
		{
			SetupDataForDoesNotExcludePaymentTests();

			DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 2 MatchGroup rows: M00002243 and M00002551", 2, dynBizOs.Count);
		}

		#endregion
	}
}
