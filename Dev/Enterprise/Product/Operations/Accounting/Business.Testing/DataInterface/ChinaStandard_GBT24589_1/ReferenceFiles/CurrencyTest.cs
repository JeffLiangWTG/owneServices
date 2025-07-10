using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	[TestedType(typeof(Currency))]
	public class CurrencyTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassProperties()
		{
			AssertEquals("T105", Currency.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Currency();
		}
	}

	[TestedType(typeof(CurrencyCollection))]
	public class CurrencyCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CurrencyCollection>
	{
		public void TestDefaultElements()
		{
			var chs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified);
			using (var mockData = chs.UseMockData())
			{
				var key = CustomizableDataResourceStrings.GetCustomizableDataKey("RX_Desc", "TESTENG");
				mockData.Put(key, new ResourceStringData(key, "TESTCN"));
				var refCurrency = Factory.NewWithValidTestData<RefCurrency>();
				refCurrency.RX_Desc = "TESTENG";
				var helper = new RefCurrencyTestHelper(Factory);
				helper.CreateRefLanguageText("RX_Desc", refCurrency.PK, "ZH-CN", "RX", "TESTCN");
				Factory.Save();
				CurrencyCollection collection = new CurrencyCollection(Factory);
				Currency testCurrency = collection.Cast<Currency>().FirstOrDefault(vARIABLE => vARIABLE.CurrencyName == "TESTCN");
				AssertEquals("The Currency should have CurrencyCode equal to TestCurrency.RX_Code", refCurrency.RX_Code, testCurrency.CurrencyCode);
				AssertEquals("The Currency should have CurrencyName equal to TESTCN", "TESTCN", testCurrency.CurrencyName);
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new Currency();
		}

		protected override CurrencyCollection GetCollectionToTest()
		{
			return new CurrencyCollection(Factory);
		}
	}
}
