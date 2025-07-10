using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocumentWrapperForTesting))]
	sealed class DocAuthorityToDealLineConditionDetailsTestCase : DocumentWrapperTest
	{
		public void TestLocations()
		{
			CUSRESMessage message = new CUSRESMessage();
			SegmentGroup13 group13 = message.Group6.InstantiateAChildAndAddItToChildrenCollection().Group13.InstantiateAChildAndAddItToChildrenCollection();
			ERCSegment eRC = group13.ERC.InstantiateAChildAndAddItToChildrenCollection();
			eRC.ApplicationErrorDetail.ApplicationErrorIdentification = "12";
			ERCSegment eRC2 = group13.ERC.InstantiateAChildAndAddItToChildrenCollection();
			eRC2.ApplicationErrorDetail.ApplicationErrorIdentification = "17";

			AuthorityToDealLineConditionDetails line = new AuthorityToDealLineConditionDetails(group13);
			DocAuthorityToDealLineConditionDetails lineWrapper = DocAuthorityToDealLineConditionDetails.New(line, Factory);
			Assert("Locations", lineWrapper.Locations.Contains("12"));
			Assert("Locations", lineWrapper.Locations.Contains("17"));
		}
	}
}
