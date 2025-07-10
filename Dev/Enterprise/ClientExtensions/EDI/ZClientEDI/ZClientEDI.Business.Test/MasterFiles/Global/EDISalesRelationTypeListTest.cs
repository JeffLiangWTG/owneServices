using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class EDISalesRelationTypeListTest : TestCaseWithFactory
	{
		public void TestGetCodes()
		{
			var codesFromNew = EDISalesRelationTypeList.New().GetAllCodes();
			var codesFromGetCodes = EDISalesRelationTypeList.New().GetCodes().ToArray();
			var expectedCodes = new[]
			{
				EDIRelatableActivityTypeList.Codes.Incident,
				RelatableActivityTypeList.Codes.CampaignManagement,
				RelatableActivityTypeList.Codes.InquiryManager,
				RelatableActivityTypeList.Codes.OpportunityManager,
				RelatableActivityTypeList.Codes.Quotations,
				RelatableActivityTypeList.Codes.OneOffQuotes,
				RelatableActivityTypeList.Codes.Communication,
				RelatableActivityTypeList.Codes.Projects
			};

			AssertContainsExactElementsInAnyOrder(expectedCodes, codesFromNew);
			AssertContainsExactElementsInAnyOrder(expectedCodes, codesFromGetCodes);
		}
	}
}
