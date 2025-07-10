using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class ValuationIndicatorCodeListHelperTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetFromLineIfSetElseFromHeader()
		{
			NUnit.Framework.Assert.That(ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(true, ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader), NUnit.Framework.Is.EqualTo("1"), "headerIndicator true, lineIndicator SameAsInvoiceHeader");
			NUnit.Framework.Assert.That(ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(false, ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader), NUnit.Framework.Is.EqualTo("0"), "headerIndicator false, lineIndicator SameAsInvoiceHeader");
			NUnit.Framework.Assert.That(ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(true, ValuationIndicatorCodeList.Codes.Yes), NUnit.Framework.Is.EqualTo("1"), "headerIndicator true, lineIndicator Yes");
			NUnit.Framework.Assert.That(ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(false, ValuationIndicatorCodeList.Codes.Yes), NUnit.Framework.Is.EqualTo("1"), "headerIndicator false, lineIndicator Yes");
			NUnit.Framework.Assert.That(ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(true, ValuationIndicatorCodeList.Codes.No), NUnit.Framework.Is.EqualTo("0"), "headerIndicator true, lineIndicator No");
			NUnit.Framework.Assert.That(ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(false, ValuationIndicatorCodeList.Codes.No), NUnit.Framework.Is.EqualTo("0"), "headerIndicator false, lineIndicator No");
		}
	}
}
