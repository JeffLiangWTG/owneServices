using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Common.AU.CMR.Testing
{
	class CMRStatusesTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestStatusListHasBeenJoined()
		{
			CodeDescriptionPairList testList = new CMRStatuses();
			NUnit.Framework.Assert.That(testList.ContainsCode("WTO"));
			NUnit.Framework.Assert.That(testList.ContainsCode("CLR"));
		}
	}
}
