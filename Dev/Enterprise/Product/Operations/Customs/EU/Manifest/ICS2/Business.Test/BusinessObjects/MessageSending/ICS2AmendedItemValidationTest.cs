using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ICS2AmendedItemValidation))]
	sealed class ICS2AmendedItemValidationTest : TestCaseWithFactory
	{
		public void TestValidateIsSelected_ReplyInfo()
		{
			var message = "This Referral request does not have any reply information entered.";

			var manifestHeader = Factory.New<AsycudaManifestHeader>();

			ICS2AmendedItem CreateAmendedItem(string messageType)
			{
				var requestHeader = manifestHeader.RequestHeaders.AddNew();
				var amendedItemsHeader = new ICS2AmendedItemsHeader(messageType, manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
				var result = new ICS2AmendedItem(amendedItemsHeader, requestHeader);
				result.IsSelected = true;

				return result;
			}

			var item = CreateAmendedItem(MessageTypes.Codes.R02);
			item.Validation.ValidateIsSelected();
			AssertHasWarning(item.IsSelectedInfo, message);

			item.RequestHeader.RequestResponses.AddNew();

			item.Validation.ValidateIsSelected();
			AssertNoWarning(item.IsSelectedInfo, message);

			item = CreateAmendedItem(MessageTypes.Codes.R03);
			item.Validation.ValidateIsSelected();
			AssertHasWarning(item.IsSelectedInfo, message);

			item.RequestHeader.RequestResponses.AddNew();

			item.Validation.ValidateIsSelected();
			AssertNoWarning(item.IsSelectedInfo, message);

			item = CreateAmendedItem(MessageTypes.Codes.A22);
			item.Validation.ValidateIsSelected();
			AssertNoWarning("Only apply this validation for R02 or R03 messages.", item.IsSelectedInfo, message);
		}

		public void TestValidateIsSelected_Status()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);

			var requestHeader = manifestHeader.RequestHeaders.AddNew();
			var item = new ICS2AmendedItem(amendedItemsHeader, requestHeader);

			item.IsSelected = true;

			requestHeader.EUS_Status = "AWA";
			var message = "This Referral request IS waiting for a response.";

			item.Validation.ValidateIsSelected();
			AssertHasMessageError(item.IsSelectedInfo, message);

			requestHeader.EUS_Status = "SNT";
			message = "This Referral request has already been submitted.";

			item.Validation.ValidateIsSelected();
			AssertHasMessageError(item.IsSelectedInfo, message);

			requestHeader.EUS_Status = "PND";

			item.Validation.ValidateIsSelected();
			AssertNoMessageError(item.IsSelectedInfo, message);
		}

		public void TestAutoValidationType()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
			var item = new ICS2AmendedItem(amendedItemsHeader, manifestHeader.RequestHeaders.AddNew());

			AssertEquals(typeof(ICS2AmendedItemValidation), item.Validation.AutoValidationType);
		}
	}
}
