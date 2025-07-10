using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	internal static class ConcurrencyTestHelper
	{
		internal static void AssertStrictConcurrencyForAccTransactionHeader<T>(ZString propertyNameToTest, IZType originalValue, IZType value1, IZType value2) where T : AccTransactionHeader
		{
			var testFactory = new BusinessObjectFactory();
			var transactionHeader = testFactory.NewWithValidTestData(typeof(T));
			SetPropertyValue(transactionHeader, originalValue);
			testFactory.Save();

			var userFactory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var transactionHeaderInUserFactory1 = userFactory1.Load(typeof(T), transactionHeader.PK);

			var userFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var transactionHeaderInUserFactory2 = userFactory2.Load(typeof(T), transactionHeader.PK);

			SetPropertyValue(transactionHeaderInUserFactory1, value1);
			userFactory1.Save();

			SetPropertyValue(transactionHeaderInUserFactory2, value2);
			var exception = Assertion.AssertExceptionThrown<ZSaveConcurrencyException>(() => userFactory2.Save());
			var errorMessage = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: AccTransactionHeader
PK: {transactionHeader.PK}";
			Assertion.AssertContains(errorMessage, exception.Message);

			void SetPropertyValue(BusinessObject header, IZType valueToSet)
			{
				var propertyInfo = header.FindPropertyInfo(propertyNameToTest);
				propertyInfo.Value = valueToSet;
			}
		}
	}
}
