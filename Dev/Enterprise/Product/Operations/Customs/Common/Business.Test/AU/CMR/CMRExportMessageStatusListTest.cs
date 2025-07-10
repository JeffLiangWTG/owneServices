using NUnit.Framework;

namespace Enterprise.Customs.Common.AU.CMR.Testing
{
	class CMRExportOtherMessageStatusListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestIsAwaitingResponse()
		{
			NUnit.Framework.Assert.That(CMRExportOtherMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingWARRELOriginal.Code));
			NUnit.Framework.Assert.That(CMRExportOtherMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingWARRELWithdrawal.Code));
			NUnit.Framework.Assert.That(CMRExportOtherMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingWARRELReplacement.Code));
			NUnit.Framework.Assert.That(CMRExportOtherMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingWARRETOriginal.Code));
			NUnit.Framework.Assert.That(CMRExportOtherMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingWARRETReplacement.Code));
			NUnit.Framework.Assert.That(CMRExportOtherMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingDEPRECOriginal.Code));
			NUnit.Framework.Assert.That(CMRExportOtherMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingDEPRECWithdrawal.Code));
			NUnit.Framework.Assert.That(CMRExportOtherMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingDEPRECReplacement.Code));
			NUnit.Framework.Assert.That(CMRExportOtherMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingDEPRELOriginal.Code));
			NUnit.Framework.Assert.That(CMRExportOtherMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingDEPRELWithdrawal.Code));
			NUnit.Framework.Assert.That(CMRExportOtherMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingDEPRELReplacement.Code));
			NUnit.Framework.Assert.That(!CMRExportOtherMessageStatusList.IsAwaitingResponse("blah"));
		}
		[ExpectNoExceptions]
		public void TestGetExportOtherMessageStatusFromCode()
		{
			var list = new CMRExportOtherMessageStatusList();
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode("AAA"), Is.Null, "AAA is not in the list");
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(CustomsEntryStatus.AwaitingWARRELOriginal.Code), Is.EqualTo(CustomsEntryStatus.AwaitingWARRELOriginal.Description), "CustomsEntryStatus.AwaitingWARRELOriginal");
		}
	}
}
