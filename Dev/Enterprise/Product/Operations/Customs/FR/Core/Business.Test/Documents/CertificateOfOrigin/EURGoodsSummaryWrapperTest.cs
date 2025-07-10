using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	class EURGoodsSummaryWrapperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when parameter is null", () => new EURGoodsSummaryWrapper(null));
		}
	}
}
