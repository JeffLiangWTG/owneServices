using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Module.Testing
{
	class LicenseFilterLookupsTest : TestCaseWithFactory
	{
		public void TestEntryStatusList()
		{
			ReferenceTestDataHelper.CreateEntryStatusForLicenseAndExportList(Factory);

			var lookups = new LicenseFilterLookups(new LicenseFilterStripBusinessObject());
			var actualList = lookups.EntryStatusList();
			AssertContainsExactElementsInAnyOrder(new string[] { "L01", "L02", "L03", "L04", "L05", "L06", "L07", "L08", "L09", "L10", "L15", "L16", "L17", "L18" }, actualList.GetAllCodes());
			var expectedList = lookups.EntryStatusList();
			AssertSame(expectedList, actualList);
		}

		public void TestMessageTypeList()
		{
			using (BRCustomsDataRegistry.Instance.EnableImportLicense.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var lookups = new LicenseFilterLookups(new LicenseFilterStripBusinessObject());
				var list = lookups.MessageTypeList;

				AssertContainsExactElementsInExactOrder("MessageTypeList has LIC code", new[] { BRJobMessageTypeList.Codes.ImportLicense }, list.GetAllCodes());
			}
		}

		public void TestMessageStatusList()
		{
			var lookups = new LicenseFilterLookups(new LicenseFilterStripBusinessObject());
			AssertEquals("ACC, AWA, FAL, NOT, REJ", lookups.MessageStatusList().CodesAsString);
		}

		public void TestExchangeHedgeTypeList()
		{
			var lookups = new LicenseFilterLookups(new LicenseFilterStripBusinessObject());
			AssertEquals("1, 2, 3, 4", lookups.ExchangeHedgeTypeList.CodesAsString);
		}

		public void TestManufacturerIndicatorList()
		{
			var lookups = new LicenseFilterLookups(new LicenseFilterStripBusinessObject());
			AssertEquals("1, 2, 3", lookups.ManufacturerIndicatorList.CodesAsString);
		}

		public void TestDrawbackModalityList()
		{
			var lookups = new LicenseFilterLookups(new LicenseFilterStripBusinessObject());
			AssertEquals("1, 2, 3, 4, 5", lookups.DrawbackModalityList.CodesAsString);
		}

		public void TestManufacturerList()
		{
			var lookups = new LicenseFilterLookups(new LicenseFilterStripBusinessObject());
			AssertType<ConsignorCollection>("Manufacturer List", lookups.ManufacturerList);
		}
	}
}
