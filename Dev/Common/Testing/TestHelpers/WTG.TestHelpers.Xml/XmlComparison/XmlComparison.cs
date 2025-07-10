using NUnit.Framework;
using XMLTools;

namespace WTG.TestHelpers.Xml
{
	public static class XmlComparison
	{
		public static void CompareAndAssertXml(string expectedXmlResult, string actualXmlResult)
		{
			CompareAndAssertXmlCore(expectedXmlResult, actualXmlResult, string.Empty);
		}

		public static void CompareAndAssertXml(string expectedXmlResult, string actualXmlResult, string errorMessage)
		{
			CompareAndAssertXmlCore(expectedXmlResult, actualXmlResult, errorMessage);
		}

		static void CompareAndAssertXmlCore(string expectedXmlResult, string actualXmlResult, string errorMessage)
		{
			var xmlComparer = new XmlComparer();
			var res = xmlComparer.CompareXml(expectedXmlResult, actualXmlResult, getDiff: true);
			var message = string.IsNullOrEmpty(errorMessage) ? res.diff : $"{errorMessage}:{System.Environment.NewLine}{res.diff}";
			Assertion.Assert(message, res.result);
		}
	}
}
