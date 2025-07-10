using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusFRNctsHeader))]
	class CusFRNctsHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCFN_NatureOfSealsDefaultValue()
		{
			AssertEquals("2", frNctsHeader.CFN_NatureOfSeals);
		}

		public void TestLogCFN_DetailedDepartureStatusCode()
		{
			var statusChangeEvents = frNctsHeader.Header.Logs.Find(a => a.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(0, statusChangeEvents.Length);

			frNctsHeader.CFN_DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.Anticipated;
			statusChangeEvents = frNctsHeader.Header.Logs.Find(a => a.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(1, statusChangeEvents.Length);
			AssertEquals("ANT", statusChangeEvents[0].SL_Reference);
		}

		public void TestLogCFN_DetailedArrivalStatusCode()
		{
			var statusChangeEvents = frNctsHeader.Header.Logs.Find(a => a.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(0, statusChangeEvents.Length);

			frNctsHeader.CFN_DetailedArrivalStatusCode = NctsDetailedStatusList.Codes.ArrivalNotificationRejected;
			statusChangeEvents = frNctsHeader.Header.Logs.Find(a => a.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(1, statusChangeEvents.Length);
			AssertEquals("ANR", statusChangeEvents[0].SL_Reference);
		}

		public void TestCFN_IsTC11DeliveredByCustoms_ShouldBeFalse_WhenCFN_IsQueryAvailableOnPaperIsTurnedOff()
		{
			frNctsHeader.CFN_IsQueryAvailableOnPaper = true;
			frNctsHeader.CFN_IsTC11DeliveredByCustoms = true;

			frNctsHeader.CFN_IsQueryAvailableOnPaper = false;
			AssertEquals(false, frNctsHeader.CFN_IsTC11DeliveredByCustoms);
		}

		public void TestCFN_IsTC11DeliveredByCustoms_ReadOnly()
		{
			frNctsHeader.CFN_IsQueryAvailableOnPaper = true;
			AssertEquals(false, frNctsHeader.CFN_IsTC11DeliveredByCustomsInfo.ReadOnly);

			frNctsHeader.CFN_IsQueryAvailableOnPaper = false;
			AssertEquals(true, frNctsHeader.CFN_IsTC11DeliveredByCustomsInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return frNctsHeader;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return header.FRNctsHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			frNctsHeader = header.FRNctsHeader;
		}

		CusFRNctsHeader frNctsHeader;
	}
}
