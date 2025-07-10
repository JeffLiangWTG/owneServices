using System.Linq;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(NewCashbookExchangeDiffBankAccountFilterBusinessObject))]
	public class NewCashbookExchangeDiffBankAccountFilterBusinessObjectTest : AccBankAccountFilterBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new NewCashbookExchangeDiffBankAccountFilterBusinessObject();
		}

		public void TestLayoutContext()
		{
			AssertEquals("CashbookExchangeDiffBankAccountFilterBusinessObject", ((IFilterStripBusinessObjectInternals)filterBizO).LayoutContext);
		}

		public void TestModuleFilters()
		{
			var moduleCodes = filterBizO.ModuleFilters.Select(x => x.Code);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"Bank Code",
				"Account Number",
				"Description",
				"Bank Name",
				"Account Type",
				"Branch",
				"Currency",
				"Company"
			}, moduleCodes);
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBizO = new NewCashbookExchangeDiffBankAccountFilterBusinessObject();
		}
		NewCashbookExchangeDiffBankAccountFilterBusinessObject filterBizO;
	}
}
