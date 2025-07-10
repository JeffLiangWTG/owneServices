using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	class ResponseDetailTest : TestCase
	{
		public void TestEmpty()
		{
			CombineAssertions(() =>
			{
				AssertNull("XmlObjectType", ResponseDetail.Empty.XmlObjectType);
				AssertNull("ProcessorType", ResponseDetail.Empty.ProcessorType);
			});
		}
	}
}
