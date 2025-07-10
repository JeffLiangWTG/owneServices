using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE007TransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<IE007TransitOperationProvider>
	{
		protected override IE007TransitOperationProvider GetProvider() => new IE007TransitOperationProvider(nctsHeader);

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new IE007TransitOperationProvider(null));
		}

		public void TestArrivalNotificationDateAndTime()
		{
			AssertType<DateTime>(Provider.ArrivalNotificationDateAndTime);
			AssertEquals("Notification Date Time", ZDateTime.BrettsBirthday.ToDateTime(), Provider.ArrivalNotificationDateAndTime);
		}

		public void TestSimplifiedProcedure()
		{
			AssertNoExceptionThrown("Empty CusAuthorizationUsages will not cause error", () => { var procedure = GetProvider().SimplifiedProcedure; });

			var auth = nctsHeader.CusAuthorizationUsages.AddNew();
			auth.AGC_Code = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			auth.AGC_Number = "ABCD1234";

			var provider = GetProvider();
			AssertEquals("AGC_Code = ACE", true, provider.SimplifiedProcedure);

			auth.AGC_Code = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure;
			provider = GetProvider();
			AssertEquals("AGC_Code = ACT", true, provider.SimplifiedProcedure);

			auth.AGC_Code = string.Empty;
			provider = GetProvider();
			AssertEquals("AGC_Code is empty", false, provider.SimplifiedProcedure);
		}

		public void TestIncidentFlag()
		{
			nctsHeader.BH_ExportFlag = "Y";
			var provider = GetProvider();
			AssertEquals("Incident Flag (true)", true, provider.IncidentFlag);

			nctsHeader.BH_ExportFlag = "N";
			provider = GetProvider();
			AssertEquals("Incident Flag (false)", false, provider.IncidentFlag);
		}

		public void TestMRN()
		{
			nctsHeader.ArrivalMrnFromUser = "IE1234567";
			var provider = GetProvider();
			AssertEquals("MRN", "IE1234567", provider.MRN);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.CusAuthorizationUsages.RemoveAndDeleteAll();
			var header = nctsHeader.ArrivalMovementHeader;
			header.BM_ArrivalDate = ZDateTime.BrettsBirthday;
		}
		NctsHeader nctsHeader;
	}
}
