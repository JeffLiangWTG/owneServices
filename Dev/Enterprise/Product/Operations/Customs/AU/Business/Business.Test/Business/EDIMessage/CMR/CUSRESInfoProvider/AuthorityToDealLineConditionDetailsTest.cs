using CargoWise.EntityFramework.Testing;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class AuthorityToDealLineConditionDetailsTest : TestCaseWithFactory
	{
		public void TestID()
		{
			CUSRESMessage message = new CUSRESMessage();
			SegmentGroup13 group13 = message.Group6.InstantiateAChildAndAddItToChildrenCollection().Group13.InstantiateAChildAndAddItToChildrenCollection();
			ERPSegment eRP = group13.ERP.InstantiateAChildAndAddItToChildrenCollection();
			eRP.ErrorPointDetails.MessageSubItemNumber = "12";
			AuthorityToDealLineConditionDetails aTD = new AuthorityToDealLineConditionDetails(group13);
			AssertEquals("ID", "12", aTD.ID);
		}

		public void TestLocations()
		{
			CUSRESMessage message = new CUSRESMessage();
			SegmentGroup13 group13 = message.Group6.InstantiateAChildAndAddItToChildrenCollection().Group13.InstantiateAChildAndAddItToChildrenCollection();
			ERCSegment eRC = group13.ERC.InstantiateAChildAndAddItToChildrenCollection();
			eRC.ApplicationErrorDetail.ApplicationErrorIdentification = "12";
			AuthorityToDealLineConditionDetails aTD = new AuthorityToDealLineConditionDetails(group13);
			AssertEquals("Locations", 1, aTD.Locations.Length);
			AssertEquals("Location ID", "12", aTD.Locations[0]);
		}

		public void TestNoLocations()
		{
			CUSRESMessage message = new CUSRESMessage();
			SegmentGroup13 group13 = message.Group6.InstantiateAChildAndAddItToChildrenCollection().Group13.InstantiateAChildAndAddItToChildrenCollection();
			AuthorityToDealLineConditionDetails aTD = new AuthorityToDealLineConditionDetails(group13);
			AssertEquals("Locations", 0, aTD.Locations.Length);
		}

		public void TestMultipleLocations()
		{
			CUSRESMessage message = new CUSRESMessage();
			SegmentGroup13 group13 = message.Group6.InstantiateAChildAndAddItToChildrenCollection().Group13.InstantiateAChildAndAddItToChildrenCollection();
			ERCSegment eRC1 = group13.ERC.InstantiateAChildAndAddItToChildrenCollection();
			eRC1.ApplicationErrorDetail.ApplicationErrorIdentification = "12";
			ERCSegment eRC2 = group13.ERC.InstantiateAChildAndAddItToChildrenCollection();
			eRC2.ApplicationErrorDetail.ApplicationErrorIdentification = "15";
			AuthorityToDealLineConditionDetails aTD = new AuthorityToDealLineConditionDetails(group13);
			AssertEquals("Locations", 2, aTD.Locations.Length);
		}

		public void TestDescription()
		{
			CUSRESMessage message = new CUSRESMessage();
			SegmentGroup13 group13 = message.Group6.InstantiateAChildAndAddItToChildrenCollection().Group13.InstantiateAChildAndAddItToChildrenCollection();
			FTXSegment fTX = group13.FTX.InstantiateAChildAndAddItToChildrenCollection();
			fTX.TextLiteral.FreeTextValue1 = "Description";
			AuthorityToDealLineConditionDetails aTD = new AuthorityToDealLineConditionDetails(group13);
			AssertEquals("Description", "Description", aTD.Description);
		}
	}
}
