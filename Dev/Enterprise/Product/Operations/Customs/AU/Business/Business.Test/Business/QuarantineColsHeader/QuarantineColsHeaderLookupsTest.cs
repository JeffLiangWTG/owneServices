using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineColsHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLateLodgementReasonsList() => CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("COLLR", "COLS - Late Lodgement Reason", "AU");
			_ = helper.CreateNewOrGetExistingCusCodeList("AU", "COLLR", "7", "Other", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList("AU", "COLLR", "3", "Awaiting shipping details", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList("AU", "COLLR", "4", "COLS error preventing timely lodgement", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var colsHeader = Factory.New<QuarantineColsHeader>();
			var list = colsHeader.Lookups.LateLodgementReasonsList;
			AssertContainsExactElementsInExactOrder("List is sorted", "Awaiting shipping details, COLS error preventing timely lodgement, Other", list.CodesAsString);
			AssertSame("Cached", list, colsHeader.Lookups.LateLodgementReasonsList);
		});

		public void TestCOLSLodgementStatusList()
		{
			var header = Factory.New<QuarantineColsHeader>();
			var list = header.Lookups.COLSLodgementStatusList;
			AssertType<COLSLodgementStatusList>("List Type", list);
			AssertSame("list should be cached", list, header.Lookups.COLSLodgementStatusList);
		}

		public void TestDeliveryClassification()
		{
			var header = Factory.New<QuarantineColsHeader>();
			var list = header.Lookups.DeliveryClassification;
			AssertType<COLSDeliveryClassificationList>("List Type", list);

			AssertSame("Accessing the list twice should get the exact same object as the list is cached", list, header.Lookups.DeliveryClassification);
		}

		public void TestAllOrganisations()
		{
			var header = Factory.New<QuarantineColsHeader>();
			AssertEquals("The type of OrgHeaderCollection", typeof(OrgHeaderCollection), header.Lookups.AllOrganisations.GetType());
		}
	}
}
