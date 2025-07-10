using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.EU.Testing
{
	class CustomsWareEntryStatusListTestCase : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetCodeFromCustomsWareStatusText()
		{
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("accepted"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Accepted).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Acknowledged"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Acknowledged).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("AmendmentRequested"), Is.EqualTo(CustomsWareEntryStatusList.Codes.AmendmentRequested).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Cancelled"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Cancelled).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Cleared"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Cleared).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Control"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Control).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Corrected"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Corrected).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Created"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Created).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Error"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Error).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Exported"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Exported).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Fallback"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Fallback).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Info"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Info).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Invalid"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Invalid).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Pending"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Pending).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Prelodged"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Prelodged).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Queued"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Queued).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Refused"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Refused).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Rejected"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Rejected).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Released"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Released).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Requested"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Requested).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Saved"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Saved).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Submitted"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Submitted).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Updated"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Updated).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Valid"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Valid).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("Sent"), Is.EqualTo(CustomsWareEntryStatusList.Codes.Sent).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText("xinvalidx"), Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}
	}
}
