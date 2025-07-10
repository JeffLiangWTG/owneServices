using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeCurrency))]
	public class ElectronicProcessingChargeCurrencyTest : RegistryBusinessObjectTemplateTestCase<ElectronicProcessingChargeCurrency>
	{
		public void TestProperties()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var date = new ZDateTime(2019, 1, 1);
			var chargeCurrency = new ElectronicProcessingChargeCurrency();
			chargeCurrency.CurrencyPK = testObjectCreator.CNY.PK;
			chargeCurrency.ValidFromDate = date;

			AssertEquals(testObjectCreator.CNY.PK, chargeCurrency.CurrencyPK);
			AssertEquals("CurrencyPK", chargeCurrency.CurrencyPKInfo.Name);
			AssertEquals(date, chargeCurrency.ValidFromDate);
			AssertEquals("ValidFromDate", chargeCurrency.ValidFromDateInfo.Name);
		}

		public void TestValidateCurrencyPK()
		{
			BizObj.RunPreSaveValidation();
			AssertEquals("Precondition", ZGuid.Empty, BizObj.CurrencyPK);
			AssertHasError(BizObj.CurrencyPKInfo, "Please enter a Currency.");

			BizObj.CurrencyPK = ZGuid.Invalid;
			Assert("Precondition", !BizObj.Currencies.Any(x => x.PK == BizObj.CurrencyPK));
			AssertHasError(BizObj.CurrencyPKInfo, "Enter a valid Currency.");

			var testObjectCreator = new TestObjectCreator(Factory);
			BizObj.CurrencyPK = testObjectCreator.CNY.PK;
			AssertNoErrors(BizObj.CurrencyPKInfo);
		}

		public void TestValidateValidFromDate()
		{
			var sameDateForValidFromDate = ZDateTime.Today;
			var duplicatedItem = BizObj.ParentCollection.AddNew();
			duplicatedItem.ValidFromDate = sameDateForValidFromDate;

			BizObj.ValidFromDate = sameDateForValidFromDate;

			AssertHasError(BizObj.ValidFromDateInfo, "This ValidFromDate has been duplicated and must be unique.");

			BizObj.ParentCollection.RemoveAndDelete(duplicatedItem);
			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.ValidFromDateInfo);
		}

		protected override ElectronicProcessingChargeCurrency GetBusinessObjectToClone()
		{
			return (ElectronicProcessingChargeCurrency)GetNewBusinessObject();
		}

		protected override ElectronicProcessingChargeCurrency GetBusinessObjectToSerialise()
		{
			return (ElectronicProcessingChargeCurrency)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var collection = new ElectronicProcessingChargeCurrencyCollection();
			return collection.AddNew();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;
	}
}
