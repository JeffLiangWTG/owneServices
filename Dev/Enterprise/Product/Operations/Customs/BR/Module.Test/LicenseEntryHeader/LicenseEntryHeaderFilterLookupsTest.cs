using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Module.Testing
{
	class LicenseEntryHeaderFilterLookupsTest : TestCaseWithFactory
	{
		public void TestEntryStatusList()
		{
			ReferenceTestDataHelper.CreateEntryStatusForLicenseAndExportList(Factory);

			var lookups = new LicenseEntryHeaderFilterLookups(new LicenseEntryHeaderFilterBusinessObject());
			var actualList = lookups.EntryStatusList(Core.Constants.CountryCodes.Brazil);
			AssertContainsExactElementsInAnyOrder(new string[] { "L01", "L02", "L03", "L04", "L05", "L06", "L07", "L08", "L09", "L10", "L15", "L16", "L17", "L18" }, actualList.GetAllCodes());
			var expectedList = lookups.EntryStatusList(Core.Constants.CountryCodes.Brazil);
			AssertSame(expectedList, actualList);
		}

		public void TestMessageTypeList()
		{
			using (BRCustomsDataRegistry.Instance.EnableImportLicense.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var lookups = new LicenseEntryHeaderFilterLookups(new LicenseEntryHeaderFilterBusinessObject());
				var list = lookups.MessageTypeList;

				AssertContainsExactElementsInExactOrder("MessageTypeList has LIC code", new[] { BRJobMessageTypeList.Codes.ImportLicense }, list.GetAllCodes());
			}
		}

		public void TestMessageStatusList()
		{
			var lookups = new LicenseEntryHeaderFilterLookups(new LicenseEntryHeaderFilterBusinessObject());
			AssertEquals("ACC, AWA, FAL, NOT, REJ", lookups.MessageStatusList().CodesAsString);
		}
	}
}
