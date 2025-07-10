using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	class TemporaryStorageBillTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			AssertGetTypeForBinding(Core.Constants.CountryCodes.Latvia, typeof(TemporaryStorageBill).FullName);

			AssertGetTypeForBinding(Core.Constants.CountryCodes.France, "Enterprise.Customs.FR.Business.CusTempStorage.TemporaryStorageBill");

			AssertGetTypeForBinding(Core.Constants.CountryCodes.Ireland, "Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageBill");

			AssertGetTypeForBinding(Core.Constants.CountryCodes.Spain, "Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageBill");

			AssertGetTypeForBinding(Core.Constants.CountryCodes.Italy, "Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageBill");

			void AssertGetTypeForBinding(string countryCode, string expected)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var typeDecider = new TemporaryStorageBillTypeDecider();
					AssertEquals(expected, typeDecider.GetTypeForBinding().FullName);
				}
			}
		}

		public void TestGetTypeForNew()
		{
			AssertGetTypeForNew(Core.Constants.CountryCodes.Latvia, typeof(TemporaryStorageBill).FullName);

			AssertGetTypeForNew(Core.Constants.CountryCodes.France, "Enterprise.Customs.FR.Business.CusTempStorage.TemporaryStorageBill");

			AssertGetTypeForNew(Core.Constants.CountryCodes.Ireland, "Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageBill");

			AssertGetTypeForNew(Core.Constants.CountryCodes.Spain, "Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageBill");

			AssertGetTypeForNew(Core.Constants.CountryCodes.Italy, "Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageBill");

			void AssertGetTypeForNew(string countryCode, string expected)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var typeDecider = new TemporaryStorageBillTypeDecider();
					AssertEquals(expected, typeDecider.GetTypeForNew().FullName);
				}
			}
		}

		public void TestGetTypeForLoad()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();

			AssertGetTypeForLoad(Core.Constants.CountryCodes.Latvia, typeof(TemporaryStorageBill).FullName);

			AssertGetTypeForLoad(Core.Constants.CountryCodes.France, "Enterprise.Customs.FR.Business.CusTempStorage.TemporaryStorageBill");

			AssertGetTypeForLoad(Core.Constants.CountryCodes.Ireland, "Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageBill");

			AssertGetTypeForLoad(Core.Constants.CountryCodes.Spain, "Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageBill");

			AssertGetTypeForLoad(Core.Constants.CountryCodes.Italy, "Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageBill");

			void AssertGetTypeForLoad(string countryCode, string expected)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var typeDecider = new TemporaryStorageBillTypeDecider();
					var row = ((INeedRow)bill).Row;
					var typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
					AssertEquals(expected, typeForLoad.FullName);
				}
			}
		}
	}
}
