using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Common.AU.CMR.Testing
{
	class CMRConsolidatedCargoAndUnderbondStatusesTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetStatusesIsCached()
		{
			var expectedList = new CMRConsolidatedCargoStatuses() + new CMRUnderbondStatuses();
			var list1 = CMRConsolidatedCargoAndUnderbondStatuses.GetStatuses(Factory);
			NUnit.Framework.Assert.That(list1.Count, Is.EqualTo(expectedList.Count));
			foreach (ICodeDescription pair in expectedList)
			{
				NUnit.Framework.Assert.That(list1.GetDescriptionFromCode(pair.Code), Is.EqualTo(pair.Description), pair.Code);
			}
			NUnit.Framework.Assert.That(ReferenceEquals(list1, CMRConsolidatedCargoAndUnderbondStatuses.GetStatuses(Factory)), "CMRConsolidatedCargoAndUnderbondStatuses should be cached");
		}
	}
}
