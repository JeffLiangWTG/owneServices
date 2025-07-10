using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCommunityProtectionRiskCollection))]
	sealed class CMRCommunityProtectionRiskCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CMRCommunityProtectionRiskCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New(typeof(CMRCommunityProtectionRisk));
	}
}
