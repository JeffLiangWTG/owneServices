using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(CampaignCollectionProvider))]
	sealed class CampaignCollectionProviderTest : CampaignCollectionProviderBaseTest
	{
		public void TestCreateCollection_FilteredByCampaignTypes()
		{
			IGlbCompanyCampaign voteCampaign = CreateCampaign(CampaignTypeList.Codes.Voting);
			IGlbCompanyCampaign surveyCampaign = CreateCampaign(CampaignTypeList.Codes.Survey);

			IBusinessObjectCollection collection = Provider.Collection;
			((BusinessObjectCollection)collection).Load();
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(voteCampaign));
			Assert(collection.Contains(surveyCampaign));
		}
	}
}
