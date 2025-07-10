using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	sealed class UCC6TemporaryStorageBillControlHelperTest : TestCaseWithFactory
	{
		public void TestGetUCC6TemporaryStorageAdditionalInfosUserControlWithGrid()
		{
			AssertGetUCC6TemporaryStorageAdditionalInfosUserControlWithGrid(CountryCodes.Ireland, "Enterprise.Customs.IE.GUI.TSDAdditionalInfosUserControlWithGrid");

			AssertGetUCC6TemporaryStorageAdditionalInfosUserControlWithGrid(CountryCodes.Germany, "Enterprise.Customs.EU.TemporaryStorage.GUI.UCC6TemporaryStorageAdditionalInfosUserControlWithGrid");

			void AssertGetUCC6TemporaryStorageAdditionalInfosUserControlWithGrid(string countryCode, string controlTypeFullName)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				using (var userControl = UCC6TemporaryStorageBillControlHelper.GetUCC6TemporaryStorageAdditionalInfosUserControlWithGrid())
				{
					AssertEquals(countryCode, controlTypeFullName, userControl.GetType().FullName);
				}
			}
		}
	}
}
