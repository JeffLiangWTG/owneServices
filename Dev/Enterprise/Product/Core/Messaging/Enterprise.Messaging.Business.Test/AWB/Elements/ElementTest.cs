using NUnit.Framework;

namespace Enterprise.Messaging.Business.AWB.Testing
{
	sealed class ElementTest : TestCase
	{
		public void TestIsMandatory()
		{
			Element element = new Element(StatusType.Mandatory);
			AssertEquals(StatusType.Mandatory, element.Status);
		}

		public void TestIsOptional()
		{
			Element element = new Element(StatusType.Optional);
			AssertEquals(StatusType.Optional, element.Status);
		}

		public void TestIsConditional()
		{
			Element element = new Element(StatusType.Conditional);
			AssertEquals(StatusType.Conditional, element.Status);
		}
	}
}
