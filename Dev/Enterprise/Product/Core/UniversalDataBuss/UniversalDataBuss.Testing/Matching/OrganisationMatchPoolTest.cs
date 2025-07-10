using Enterprise.UniversalDataBuss.Management.Matching;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.UniversalDataBuss.Matching.Testing
{
	class OrganisationMatchPoolTest : TestCaseWithUniversalObjectFactory
	{
		public void TestInitialMatchWorksAndSecondMatchIsCached()
		{
			var orgHeaderMatch = OrgCreator.CreateBusinessObject("FAN", "BUDDHA", "5498");
			var pool = new OrganizationAddressMatchPool();

			var organisationData = OrgCreator.CreateDataObject("FAN", "BUDDHA", "5498");
			var firstMatch = pool.GetMatch(organisationData, Factory.BOFactory);
			CombineAssertions(() =>
			{
				AssertEquals("firstMatch.Success", true, firstMatch.Success);
				AssertEquals("firstMatch.MatchFound", orgHeaderMatch, firstMatch.MatchFound);
				AssertEquals("firstMatch.MatchingLog", $"Incoming Organization [FAN BUDDHA ENTERPRISES] matched to {Enterprise.Core.Constants.ProductName} Organization Code [FANBUDHTU].", string.Join("\r\n", firstMatch.GetMatchingLogsForTesting()));
			});

			var organisationWithSameData = OrgCreator.CreateDataObject("FAN", "BUDDHA", "5498");
			var secondMatch = pool.GetMatch(organisationWithSameData, Factory.BOFactory);
			CombineAssertions(() =>
			{
				AssertEquals("secondMatch.Success", true, secondMatch.Success);
				AssertEquals("secondMatch.MatchFound", orgHeaderMatch, secondMatch.MatchFound);
				AssertEquals("secondMatch.MatchingLog", $"Incoming Organization [FAN BUDDHA ENTERPRISES] matched to {Enterprise.Core.Constants.ProductName} Organization Code [FANBUDHTU].", string.Join("\r\n", secondMatch.GetMatchingLogsForTesting()));
				Assert("object.ReferenceEquals(firstMatch, secondMatch) - Cache should pick up previous match", object.ReferenceEquals(firstMatch, secondMatch));
			});
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			this.OrgCreator = new OrganisationTestHelper(Factory);
		}

		OrganisationTestHelper OrgCreator;

		#endregion
	}
}
