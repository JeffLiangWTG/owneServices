using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(TemporaryLandingInfoValidation))]
	sealed class TemporaryLandingInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var bill = Bill;
			var temporaryLanding = TemporaryLanding;
			var targetInfo = temporaryLanding.CSI_CodeInfo;
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "POX", "POS");

			temporaryLanding.CSI_ItemNumber = 1;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			temporaryLanding.CSI_ItemNumber = ZInt.Zero;
			temporaryLanding.CSI_DateOfIssue = new ZDate(2024, 7, 26);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			temporaryLanding.CSI_DateOfIssue = ZDate.Empty;
			temporaryLanding.CSI_DateOfExpiry = new ZDate(2024, 7, 26);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			temporaryLanding.CSI_DateOfExpiry = ZDate.Empty;
			bill.ABL_GoodsLocation = "Test";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			bill.ABL_GoodsLocation = ZString.Empty;
			temporaryLanding.CSI_ReferenceNumber = BondedTransportCodeList.Codes.Truck;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			temporaryLanding.CSI_ReferenceNumber = ZString.Empty;
			bill.OtherLawsandRegulations.AddNew().CFR_Reference = "T";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			temporaryLanding.CSI_ItemNumber = 1;
			temporaryLanding.CSI_Code = ZString.Empty;
			AssertNoErrors("Do not verify when not NVC01", targetInfo);
		}

		public void TestCheckCSI_DateOfExpiry()
		{
			var message = "Temporary Landing End Date must be later then Temporary Landing Start Date.";
			CombineAssertions(() =>
			{
				var targetInfo = TemporaryLanding.CSI_DateOfExpiryInfo;
				Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
				TemporaryLanding.CSI_DateOfIssue = new ZDateTime(2024, 7, 26);
				TemporaryLanding.CSI_DateOfExpiry = new ZDateTime(2024, 7, 25);
				AssertHasMessageError("End date is later then start date", targetInfo, message);

				TemporaryLanding.CSI_DateOfExpiry = new ZDateTime(2024, 7, 26);
				AssertNoMessageError("Same date", targetInfo, message);

				TemporaryLanding.CSI_DateOfExpiry = new ZDateTime(2024, 7, 27);
				AssertNoMessageError("End date is effective", targetInfo, message);

				TemporaryLanding.CSI_DateOfExpiry = ZDateTime.Empty;
				AssertNoMessageError("End date is invalid", targetInfo, message);

				TemporaryLanding.CSI_DateOfIssue = ZDateTime.Empty;
				TemporaryLanding.Validation.ValidateCSI_DateOfExpiry();
				AssertNoMessageError("Start date and end date are invalid", targetInfo, message);

				Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
				TemporaryLanding.CSI_DateOfIssue = new ZDateTime(2024, 7, 26);
				TemporaryLanding.CSI_DateOfExpiry = new ZDateTime(2024, 7, 25);
				AssertNoMessageErrors("Do not verify when not NVC01", targetInfo);
			});
		}

		public void TestCheckCSI_ItemNumber()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var bill = Bill;
			var temporaryLanding = TemporaryLanding;
			var targetInfo = temporaryLanding.CSI_ItemNumberInfo;
			temporaryLanding.CSI_Code = TemporaryLandingReasonList.Codes.REV;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			temporaryLanding.CSI_Code = ZString.Empty;
			temporaryLanding.CSI_DateOfIssue = new ZDate(2024, 7, 26);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			temporaryLanding.CSI_DateOfIssue = ZDate.Empty;
			temporaryLanding.CSI_DateOfExpiry = new ZDate(2024, 7, 26);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			temporaryLanding.CSI_DateOfExpiry = ZDate.Empty;
			bill.ABL_GoodsLocation = "Test";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			bill.ABL_GoodsLocation = ZString.Empty;
			temporaryLanding.CSI_ReferenceNumber = BondedTransportCodeList.Codes.Truck;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			temporaryLanding.CSI_ReferenceNumber = ZString.Empty;
			bill.OtherLawsandRegulations.AddNew().CFR_Reference = "T";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			temporaryLanding.CSI_DateOfIssue = new ZDate(2024, 7, 26);
			temporaryLanding.CSI_ItemNumber = ZInt.Zero;
			AssertNoMessageErrors("Do not verify when not NVC01", targetInfo);
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var bill = Bill;
			var temporaryLanding = TemporaryLanding;
			var targetInfo = temporaryLanding.CSI_ReferenceNumberInfo;
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "1", "6");
			temporaryLanding.CSI_DateOfIssue = new ZDate(2024, 7, 26);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			temporaryLanding.CSI_DateOfIssue = ZDate.Empty;
			temporaryLanding.CSI_DateOfExpiry = new ZDate(2024, 7, 26);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			temporaryLanding.CSI_DateOfExpiry = ZDate.Empty;
			bill.ABL_GoodsLocation = "Test";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			bill.ABL_GoodsLocation = ZString.Empty;
			bill.OtherLawsandRegulations.AddNew().CFR_Reference = "T";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			temporaryLanding.CSI_DateOfIssue = new ZDate(2024, 7, 26);
			temporaryLanding.CSI_ReferenceNumber = ZString.Empty;
			AssertNoMessageErrors("Do not verify when not NVC01", targetInfo);
		}

		public void TestCheckCSI_DateOfIssue()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var bill = Bill;
			bill.TemporaryLandingNumber = "123456";
			var temporaryLanding = TemporaryLanding;
			var targetInfo = temporaryLanding.CSI_DateOfIssueInfo;
			temporaryLanding.CSI_DateOfExpiry = new ZDate(2024, 7, 26);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			temporaryLanding.CSI_DateOfExpiry = ZDate.Empty;
			bill.ABL_GoodsLocation = "Test";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			bill.ABL_GoodsLocation = ZString.Empty;
			temporaryLanding.CSI_ReferenceNumber = BondedTransportCodeList.Codes.Truck;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			temporaryLanding.CSI_ReferenceNumber = ZString.Empty;
			bill.OtherLawsandRegulations.AddNew().CFR_Reference = "T";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			var expectedMessage = "Start date for Temporary landing must be today or a future date.";

			AssertEntityValidation(temporaryLanding)
				.WhenProperty(x => x.CSI_DateOfIssue, Is.EqualTo(ZDateTime.Today.AddDays(-1)))
				.ShouldCheckThat(x => x.CSI_DateOfIssueInfo, Has.MessageErrorContaining(expectedMessage));

			AssertEntityValidation(Bill)
				.WhenProperty(x => x.ABL_BillStatus, Is.EqualTo(JPCustomsStatusList.Codes.REG))
				.WhenValidating(temporaryLanding.Validation.ValidateCSI_DateOfIssue)
				.ShouldCheckThat(x => x.TemporaryLandingStartDateInfo, Has.NoMessageErrorContaining(expectedMessage));

			AssertEntityValidation(Bill)
				.WhenProperty(x => x.TemporaryLandingStatus, Is.EqualTo(TemporaryLandingStatusCodeList.Codes.CAN))
				.WhenValidating(temporaryLanding.Validation.ValidateCSI_DateOfIssue)
				.ShouldCheckThat(x => x.TemporaryLandingStartDateInfo, Has.NoMessageErrorContaining(expectedMessage));

			var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.NVC01 };
			using (Header.SetCurrentMessageSendingContext(messageSendingContext))
			{
				AssertEntityValidation(Bill)
					.WhenProperty(x => x.TemporaryLandingStatus, Is.EqualTo(TemporaryLandingStatusCodeList.Codes.CAN))
					.WhenValidating(temporaryLanding.Validation.ValidateCSI_DateOfIssue)
					.ShouldCheckThat(x => x.TemporaryLandingStartDateInfo, Has.MessageErrorContaining(expectedMessage));
			}

			messageSendingContext.ProcedureCode = JPProcedureCodeList.Codes.EDA;
			using (Header.SetCurrentMessageSendingContext(messageSendingContext))
			{
				AssertEntityValidation(Bill)
					.WhenProperty(x => x.TemporaryLandingStatus, Is.EqualTo(TemporaryLandingStatusCodeList.Codes.CAN))
					.WhenValidating(temporaryLanding.Validation.ValidateCSI_DateOfIssue)
					.ShouldCheckThat(x => x.TemporaryLandingStartDateInfo, Has.NoMessageErrorContaining(expectedMessage));
			}

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			temporaryLanding.CSI_DateOfExpiry = new ZDate(2024, 7, 26);
			temporaryLanding.CSI_DateOfIssue = ZDate.Empty;
			AssertNoMessageErrors("Do not verify when not NVC01", targetInfo);
		}

		TemporaryLandingInfo TemporaryLanding => temporaryLanding ??= Bill.TemporaryLandingInfoCollection.AddNew();
		TemporaryLandingInfo temporaryLanding;

		AsycudaBill Bill => bill ??= Header.Bills.AddNew();
		AsycudaBill bill;

		AsycudaManifestHeader Header => header ??= Factory.New<AsycudaManifestHeader>();
		AsycudaManifestHeader header;
	}
}
