using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class Message819HeaderProviderHelperTest : TestCaseWithFactory
	{
		public void TestDestinationOfficeReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No offices entered", string.Empty, helper.DestinationOfficeReferenceNumber);
				var deliveryCustomsOffice = emcsDeclaration.CustomsOffices.AddNew();
				deliveryCustomsOffice.CY_Code = OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival;
				deliveryCustomsOffice.CY_Data = "DE00876";

				var dispatchCustomsOffice = emcsDeclaration.CustomsOffices.AddNew();
				dispatchCustomsOffice.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDispatch;
				dispatchCustomsOffice.CY_Data = "DE00934";
				AssertEquals("Delivery office code", "DE00876", helper.DestinationOfficeReferenceNumber);
			});
		}

		public void TestRejectedFlag()
		{
			AssertEquals("Not rejected", false, helper.RejectedFlag);

			alertOrReject.RejectedFlag = ZBool.True;
			AssertEquals("Rejected", true, helper.RejectedFlag);
		}

		public void TestDateOfAlertOrRejection()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Null by default", null, helper.DateOfAlertOrRejection);

				alertOrReject.DateOfAlertOrRejection = new ZDate(2020, 01, 02);
				AssertEquals("Returns Correct value", new DateTime(2020, 01, 02, 0, 0, 0), helper.DateOfAlertOrRejection);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			alertOrReject = new AlertOrRejectSendingAction(emcsDeclaration);
			helper = new Message819HeaderProviderHelper(emcsDeclaration, alertOrReject);
		}
		EMCSJobDeclaration emcsDeclaration;
		Message819HeaderProviderHelper helper;
		AlertOrRejectSendingAction alertOrReject;
	}
}
