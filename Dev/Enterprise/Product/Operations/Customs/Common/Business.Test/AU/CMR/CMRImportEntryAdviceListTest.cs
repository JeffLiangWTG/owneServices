using NUnit.Framework;

namespace Enterprise.Customs.Common.AU.CMR.Testing
{
	class CMRImportEntryAdviceListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetCMRImportEntryAdviceList()
		{
			var list = new CMRImportEntryAdviceList();
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode("AAA"), Is.Null, "AAA is not in the list");
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(CMRImportEntryAdvice.Withdrawn.Code), Is.EqualTo(CMRImportEntryAdvice.Withdrawn.Description), "CustomsEntryStatus.ClearPayment");
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(CMRImportEntryAdvice.DeclarationWorkComplete.Code), Is.EqualTo(CMRImportEntryAdvice.DeclarationWorkComplete.Description));
		}

		[ExpectNoExceptions]
		public void TestIsEntryStatusClear()
		{
			NUnit.Framework.Assert.That(CMRImportEntryAdviceList.IsEntryStatusClearOrFinalisedOrATDReceived(CMRImportEntryAdvice.Clear.Code), Is.EqualTo(true), "IsEntryClear");
			NUnit.Framework.Assert.That(CMRImportEntryAdviceList.IsEntryStatusClearOrFinalisedOrATDReceived(CMRImportEntryAdvice.Held.Code), Is.EqualTo(false), "IsEntryClear");
			NUnit.Framework.Assert.That(CMRImportEntryAdviceList.IsEntryStatusClearOrFinalisedOrATDReceived(CMRImportEntryAdvice.Finalised.Code), Is.EqualTo(true), "IsEntryClear");
			NUnit.Framework.Assert.That(CMRImportEntryAdviceList.IsEntryStatusClearOrFinalisedOrATDReceived(CMRImportEntryAdvice.Rejected.Code), Is.EqualTo(false), "IsEntryClear");
			NUnit.Framework.Assert.That(CMRImportEntryAdviceList.IsEntryStatusClearOrFinalisedOrATDReceived(CMRImportEntryAdvice.ATDReceived.Code), Is.EqualTo(true), "IsEntryClear");
			NUnit.Framework.Assert.That(CMRImportEntryAdviceList.IsEntryStatusClearOrFinalisedOrATDReceived(CMRImportEntryAdvice.Processing.Code), Is.EqualTo(false), "IsEntryClear");
		}
	}
}
