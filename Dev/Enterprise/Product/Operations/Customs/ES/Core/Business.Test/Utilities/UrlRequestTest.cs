using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class UrlRequestTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Empty url", () => new UrlRequest(ZString.Empty));
		}
	}
}
