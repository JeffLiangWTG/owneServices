using WTG.StaticAnalysis.Annotation;

namespace NUnit.Framework
{
	[CodeAlive("Framework to be used by other projects. Will be used soon.")]
	public static class XmlAssertions
	{
		public static IXmlAssertion AssertIsXml(string xml)
		{
			Assertion.Assert(true);
			return new XmlElementAssertion(xml);
		}

		public static IXmlAssertion AssertIsXml(string errorMessage, string xml)
		{
			Assertion.Assert(true);
			return new XmlElementAssertion(xml, message: errorMessage);
		}
	}
}
