using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Module.Testing
{
	public class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestMessageStatusList()
		{
			var lookups = new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
			AssertEquals("ACC, AWA, FAL, NOT, REJ", lookups.MessageStatusList().CodesAsString);
		}

		public void TestBRAdministrativeStatusList()
		{
			var lookups = new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
			AssertEquals("1, 2, 3, 4, 5", lookups.AdministrativeStatusList.CodesAsString);
		}

		public void TestEntryStatusList()
		{
			ReferenceTestDataHelper.CreateEntryStatusForLicenseAndExportList(Factory);

			var lookups = new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
			var actualList = lookups.EntryStatusList();
			AssertContainsExactElementsInAnyOrder(new string[] { "I31", "E10", "E11", "SUB", "ACK" }, actualList.GetAllCodes());
			var expectedList = lookups.EntryStatusList();
			AssertSame(expectedList, actualList);
		}

		public void TestMessageTypeList()
		{
			using (BRCustomsDataRegistry.Instance.EnableLPCO.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (BRCustomsDataRegistry.Instance.EnableImportLicense.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var lookups = new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
				var list = lookups.MessageTypeList;

				AssertCollectionNotContains("MessageTypeList not contain LIC", BRJobMessageTypeList.Codes.ImportLicense, list.GetAllCodes());
				AssertCollectionNotContains("MessageTypeList not contain LPC", BRJobMessageTypeList.Codes.LPCO, list.GetAllCodes());
			}
		}

		public void TestRiskChannelList()
		{
			var lookups = new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
			AssertEquals("1, 2, 3, 4, 5", lookups.RiskChannelList.CodesAsString);
		}
	}
}
