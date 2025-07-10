using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Module
{
	[TestedType(typeof(BillingPricesFilterBusinessObject))]
	public class BillingPricesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLanguageFilter()
		{
			var item1 = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			var item2 = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			var item3 = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			item1.L7_Language = Core.SharedConstants.Languages.French;
			item2.L7_Language = Core.SharedConstants.Languages.ChineseSimplified;
			item3.L7_Language = "";
			var filter = BillingPricesFilterBO["Language"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "A";
			var items = Factory.Load<ClientLicencePriceItem>(filter.Query);
			AssertEquals(0, items.Length);
			filter.Property = Core.SharedConstants.Languages.French;
			items = Factory.Load<ClientLicencePriceItem>(filter.Query);
			AssertEquals(1, items.Length);
			AssertEquals(item1.PK, items[0].PK);
			filter.Property = Core.SharedConstants.Languages.ChineseSimplified;
			items = Factory.Load<ClientLicencePriceItem>(filter.Query);
			AssertEquals(1, items.Length);
			AssertEquals(item2.PK, items[0].PK);
			filter.Property = "";
			items = Factory.Load<ClientLicencePriceItem>(filter.Query);
			AssertEquals(4, items.Length);
		}

		public void TestDisbursementFilter()
		{
			var item1 = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			var item2 = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			var item3 = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			item1.L7_DisbursementDirection = "IMP";
			item2.L7_DisbursementDirection = "";
			item3.L7_DisbursementDirection = "EXP";

			item1.L7_RN_NKDisbursementCountry = "";
			item2.L7_RN_NKDisbursementCountry = "US";
			item3.L7_RN_NKDisbursementCountry = "GB";

			var filter = BillingPricesFilterBO["Disbursement Country"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "US";
			var items = Factory.Load<ClientLicencePriceItem>(filter.Query);
			AssertEquals(item2.PK, items.Single().PK);

			filter.IsActive = false;
			filter = BillingPricesFilterBO["Disbursement Direction"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "IMP";
			items = Factory.Load<ClientLicencePriceItem>(filter.Query);
			AssertEquals(item1.PK, items.Single().PK);
		}

		#region BusinessObjectTestCase
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BillingPricesFilterBusinessObject();
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			CreateContactsForTest();
			BillingPricesFilterBO = new BillingPricesFilterBusinessObject();
		}

		void CreateContactsForTest()
		{
			Factory.NewWithValidTestData<ClientLicencePriceItem>();
			Factory.Save();
		}

		BillingPricesFilterBusinessObject BillingPricesFilterBO;
		#endregion
	}
}
