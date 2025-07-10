using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Common.AU.CMR.Testing
{
	class CMRAllStatusesTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestListLength()
		{
			CodeDescriptionPairList testList = new CMRAllStatuses();
			NUnit.Framework.Assert.That(testList.Count, Is.EqualTo(29));
		}
	}
}
