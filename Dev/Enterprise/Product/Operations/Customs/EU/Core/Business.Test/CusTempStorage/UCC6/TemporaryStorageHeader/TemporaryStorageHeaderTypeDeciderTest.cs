using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class TemporaryStorageHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			var typeDecider = new TemporaryStorageHeaderTypeDecider();
			AssertEquals(typeof(TemporaryStorageHeader), typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertGetTypeForNew(Core.Constants.CountryCodes.Belgium, "Enterprise.Customs.BE.Business.CusTempStorage.TemporaryStorageHeader");

			AssertGetTypeForNew(Core.Constants.CountryCodes.France, "Enterprise.Customs.FR.Business.CusTempStorage.TemporaryStorageHeader");

			AssertGetTypeForNew(Core.Constants.CountryCodes.Ireland, "Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageHeader");

			AssertGetTypeForNew(Core.Constants.CountryCodes.Italy, "Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader");

			AssertGetTypeForNew(Core.Constants.CountryCodes.Spain, "Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader");

			void AssertGetTypeForNew(string countryCode, string expected)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var typeDecider = new TemporaryStorageHeaderTypeDecider();
					AssertEquals(expected, typeDecider.GetTypeForNew().FullName);
				}
			}
		}

		public void TestGetTypeForLoad()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.AMA_JobReference = "456";
			temporaryStorageHeader.AMA_ApplicationCode = "NVC";

			AssertGetTypeForLoad(Core.Constants.CountryCodes.Belgium, "Enterprise.Customs.BE.Business.CusTempStorage.TemporaryStorageHeader");

			AssertGetTypeForLoad(Core.Constants.CountryCodes.France, "Enterprise.Customs.FR.Business.CusTempStorage.TemporaryStorageHeader");

			AssertGetTypeForLoad(Core.Constants.CountryCodes.Ireland, "Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageHeader");

			AssertGetTypeForLoad(Core.Constants.CountryCodes.Italy, "Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader");

			AssertGetTypeForLoad(Core.Constants.CountryCodes.Spain, "Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader");

			void AssertGetTypeForLoad(string countryCode, string expected)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var typeDecider = new TemporaryStorageHeaderTypeDecider();
					var row = ((INeedRow)temporaryStorageHeader).Row;
					var typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
					AssertEquals(expected, typeForLoad.FullName);
				}
			}
		}
	}
}
