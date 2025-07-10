using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(RequestHeaderValidation))]
	sealed class RequestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEUS_RequestType()
		{
			var requestHeader = Factory.New<RequestHeader>();
			requestHeader.EUS_Type = "XXX";
			Assert(requestHeader.EUS_TypeInfo.HasMessageError("The code you have selected is not in the list."));
		}

		public void TestCheckEUS_HouseBillNumber()
		{
			var housebillNumber = "TestHousebillNumber";

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = housebillNumber;

			var requestHeader = manifestHeader.RequestHeaders.AddNew();
			requestHeader.EUS_HouseBillNumber = housebillNumber;

			Assert(!requestHeader.EUS_HouseBillNumberInfo.HasMessageErrors());

			requestHeader.EUS_HouseBillNumber = "XXX";
			Assert(requestHeader.EUS_HouseBillNumberInfo.HasMessageError("The code you have selected is not in the list."));
		}

		public void TestCheckEUS_IncludeScreeningDetails()
		{
			var housebillNumber = "TestHousebillNumber";
			var message = "No HRCM Screening results exist for the House Bill or the Master Header. The Screening Results will not be included in the Reply";

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.BillScreenings.RemoveAndDeleteAll();

			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = housebillNumber;

			var requestHeader = manifestHeader.RequestHeaders.AddNew();
			requestHeader.EUS_HouseBillNumber = housebillNumber;
			requestHeader.EUS_IncludeScreeningDetails = true;
			AssertHasMessageError(requestHeader.EUS_IncludeScreeningDetailsInfo, message);

			manifestHeader.BillScreenings.AddNew();
			requestHeader.Validation.ValidateEUS_IncludeScreeningDetails();
			AssertNoMessageErrors(requestHeader.EUS_IncludeScreeningDetailsInfo);

			manifestHeader.BillScreenings.RemoveAndDeleteAll();
			bill.BillScreenings.AddNew();
			requestHeader.Validation.ValidateEUS_IncludeScreeningDetails();
			AssertNoMessageErrors(requestHeader.EUS_IncludeScreeningDetailsInfo);
		}

		public void TestCheckEUS_IncludeScreeningDetails_AfterChangeHousebillNumber()
		{
			var housebillNumber1 = "TestHousebillNumber1";
			var housebillNumber2 = "TestHousebillNumber2";

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var billHasHRCM = manifestHeader.Bills.AddNew();
			billHasHRCM.ABL_BillNumber = housebillNumber1;
			billHasHRCM.BillScreenings.AddNew();

			var billHasNoHRCM = manifestHeader.Bills.AddNew();
			billHasNoHRCM.ABL_BillNumber = housebillNumber2;

			var requestHeader = manifestHeader.RequestHeaders.AddNew();
			requestHeader.EUS_HouseBillNumber = housebillNumber1;

			requestHeader.EUS_IncludeScreeningDetails = true;
			AssertNoMessageErrors(requestHeader.EUS_IncludeScreeningDetailsInfo);

			requestHeader.EUS_HouseBillNumber = housebillNumber2;
			AssertHasMessageError(requestHeader.EUS_IncludeScreeningDetailsInfo, "No HRCM Screening results exist for the House Bill or the Master Header. The Screening Results will not be included in the Reply");
		}
	}
}
