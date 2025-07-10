using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Module.Testing
{
	class LPCODeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestEntryStatusList()
		{
			ReferenceTestDataHelper.CreateEntryStatusForLicenseAndExportList(Factory);

			var lookups = new LPCODeclarationFilterLookups(new LPCODeclarationFilterStripBusinessObject());
			var actualList = lookups.EntryStatusList();
			AssertContainsExactElementsInAnyOrder(new string[] { "ANR", "CAN", "DEF", "EMA", "EME", "EMI", "EMV", "IND", "PAA", "REQ", "EXP" }, actualList.GetAllCodes());
			var expectedList = lookups.EntryStatusList();
			AssertSame(expectedList, actualList);
		}

		public void TestMessageTypeList()
		{
			using (BRCustomsDataRegistry.Instance.EnableLPCO.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var lookups = new LPCODeclarationFilterLookups(new LPCODeclarationFilterStripBusinessObject());
				var list = lookups.MessageTypeList;

				AssertContainsExactElementsInExactOrder("MessageTypeList has LPC code", new[] { BRJobMessageTypeList.Codes.LPCO }, list.GetAllCodes());
			}
		}

		public void TestMessageStatusList()
		{
			var lookups = new LPCODeclarationFilterLookups(new LPCODeclarationFilterStripBusinessObject());
			AssertEquals("ACC, AWA, FAL, NOT, REJ", lookups.MessageStatusList().CodesAsString);
		}
	}
}
