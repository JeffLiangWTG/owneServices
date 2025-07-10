using NUnit.Framework;

namespace Enterprise.Customs.Common.AU.CMR.Testing
{
	class CMRImportMessageStatusListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestIsAwaitingResponse()
		{
			NUnit.Framework.Assert.That(CMRImportMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingSAC.Code));
			NUnit.Framework.Assert.That(CMRImportMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingPreLodge.Code));
			NUnit.Framework.Assert.That(CMRImportMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingFormalLodge.Code));
			NUnit.Framework.Assert.That(CMRImportMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingPayment.Code));
			NUnit.Framework.Assert.That(CMRImportMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingAmendment.Code));
			NUnit.Framework.Assert.That(CMRImportMessageStatusList.IsAwaitingResponse(CustomsEntryStatus.AwaitingWithdrawal.Code));
			NUnit.Framework.Assert.That(!CMRImportMessageStatusList.IsAwaitingResponse("blah"));
		}

		[ExpectNoExceptions]
		public void TestIsFailedResponse()
		{
			NUnit.Framework.Assert.That(CMRImportMessageStatusList.IsFailedResponse(CustomsEntryStatus.FailSAC.Code));
			NUnit.Framework.Assert.That(CMRImportMessageStatusList.IsFailedResponse(CustomsEntryStatus.FailPreLodge.Code));
			NUnit.Framework.Assert.That(CMRImportMessageStatusList.IsFailedResponse(CustomsEntryStatus.FailFormalLodge.Code));
			NUnit.Framework.Assert.That(CMRImportMessageStatusList.IsFailedResponse(CustomsEntryStatus.FailPayment.Code));
			NUnit.Framework.Assert.That(CMRImportMessageStatusList.IsFailedResponse(CustomsEntryStatus.FailAmendment.Code));
			NUnit.Framework.Assert.That(CMRImportMessageStatusList.IsFailedResponse(CustomsEntryStatus.FailWithdrawal.Code));
			NUnit.Framework.Assert.That(!CMRImportMessageStatusList.IsFailedResponse("blah"));
		}

		[ExpectNoExceptions]
		public void TestGetCMRImportMessageStatusList()
		{
			var list = new CMRImportMessageStatusList();
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode("AAA"), Is.Null, "AAA is not in the list");
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(CustomsEntryStatus.ClearPayment.Code), Is.EqualTo(CustomsEntryStatus.ClearPayment.Description), "CustomsEntryStatus.ClearPayment");
		}
	}
}
