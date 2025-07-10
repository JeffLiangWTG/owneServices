using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(EFTPaymentInformation))]
	sealed class EFTPaymentInformationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestScheduledPaymentDate() => CombineAssertions(() =>
		{
			var testInfo = new EFTPaymentInformation(EntryHeader);
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(testInfo.ScheduledPaymentDateInfo);
			AssertEquals("Caption", "Scheduled Payment Date", resourceStringData.Caption);
			AssertEquals("FullDescription", "Setting a date here will schedule the payment message to be sent at this date and time.", resourceStringData.FullDescription);

			EntryHeader.ScheduledPaymentDate = new ZDateTime(2023, 10, 21, 13, 08, 56);
			AssertEquals("Getter", new ZDateTime(2023, 10, 21, 13, 08, 56), testInfo.ScheduledPaymentDate);
			AssertEquals("PropertyInfo", new ZDateTime(2023, 10, 21, 13, 08, 56), testInfo.ScheduledPaymentDateInfo.Value);

			testInfo.ScheduledPaymentDate = new ZDateTime(2023, 05, 16, 08, 21, 42);
			AssertEquals("Setter", new ZDateTime(2023, 05, 16, 08, 21, 42), EntryHeader.ScheduledPaymentDate);
		});

		[TestDate(2023, 11, 25, 18, 21, 36)]
		public void TestValidateScheduledPaymentDate() => CombineAssertions(() =>
		{
			const string message = "Scheduled date is earlier than the current date so the payment message will be sent immediately.";
			var testInfo = new EFTPaymentInformation(EntryHeader);
			testInfo.ScheduledPaymentDate = ZDateTime.Empty;
			AssertNoWarning("ScheduledPaymentDate empty", testInfo.ScheduledPaymentDateInfo, message);
			testInfo.ScheduledPaymentDate = new ZDateTime(2023, 11, 25, 21, 00, 00);
			AssertNoWarning("ScheduledPaymentDate later than Now", testInfo.ScheduledPaymentDateInfo, message);
			testInfo.ScheduledPaymentDate = new ZDateTime(2023, 11, 25, 10, 25, 49);
			AssertHasWarning("Validation triggered by setting ScheduledPaymentDate", testInfo.ScheduledPaymentDateInfo, message);

			testInfo.ClearAllNotifications();
			testInfo.RunPreSaveValidation();
			AssertHasWarning("Validation triggered by RunPreSaveValidation", testInfo.ScheduledPaymentDateInfo, message);
		});

		public void TestAQISServiceFeeForSACEntry()
		{
			AssertEquals("PreCondition:SAC entry", false, TestDec.IsSAC);

			EFTPaymentInformation testInfo = new EFTPaymentInformation(EntryHeader);
			testInfo.AQISServicePaymentAmountPayableNow = 100m;
			AssertEquals("Message Error expected", false, testInfo.AQISServicePaymentAmountPayableNowInfo.GetMessageErrors().Contains(EFTPaymentInformation.AQISForSACMessage));

			TestDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			TestDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			testInfo.AQISServicePaymentAmountPayableNow = 100m;
			AssertEquals("Message Error expected", true, testInfo.AQISServicePaymentAmountPayableNowInfo.GetMessageErrors().Contains(EFTPaymentInformation.AQISForSACMessage));

			TestDec.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			testInfo.AQISServicePaymentAmountPayableNow = 100m;
			AssertEquals("Message Error expected", false, testInfo.AQISServicePaymentAmountPayableNowInfo.GetMessageErrors().Contains(EFTPaymentInformation.AQISForSACMessage));
		}

		public void TestHasAmountsToPay()
		{
			var testInfo = new EFTPaymentInformation(EntryHeader);
			AssertEquals("Has nothing to pay", false, testInfo.HasAmountsToPay);

			testInfo.AQISServicePaymentAmountPayableNow = 100m;
			AssertEquals(100m, EntryHeader.AddInfo.ZA_AQISPayNow_Hidden);
			AssertEquals("Has amounts to pay with AQIS Charge", true, testInfo.HasAmountsToPay);

			testInfo.AQISServicePaymentAmountPayableNow = 0m;
			testInfo.CustomsChargeAmountPayableNow = 100m;
			AssertEquals(100m, EntryHeader.AddInfo.ZA_CustomsPayNow_Hidden);
			AssertEquals("Has amounts to pay with Customs Charge", true, testInfo.HasAmountsToPay);

			testInfo.CustomsChargeAmountPayableNow = 0m;
			AssertEquals("Has amounts to pay", false, testInfo.HasAmountsToPay);
		}

		public void TestEntryHeaderReferenceNumbers()
		{
			EntryHeader.CH_BGMReference = "S00001000/1";
			EntryHeader.EntryNumber = "AAABBB111";
			var testInfo = new EFTPaymentInformation(EntryHeader);
			AssertEquals("BGM Reference number", "S00001000/1", testInfo.BGMReferenceNumber);
			AssertEquals("Entry number", "AAABBB111", testInfo.EntryNumber);
		}

		public void TestDefaultTotalPayableFromLastClearanceSession()
		{
			var entryHeaderMoq = Factory.NewMoq<CusEntryHeader>();
			entryHeaderMoq.Setup(m => m.TotalPayableDueAdvisedInLastClearanceMessage).Returns(new ZDecimal(123.50m));

			var entryHeader = entryHeaderMoq.Object;
			entryHeader.CustomsChargeAmountPayableNow = 100m;
			entryHeader.AQISServicePaymentAmountPayableNow = 50m;

			var testInfo = new EFTPaymentInformation(entryHeader, initialiseFromLastClearance: true);
			AssertEquals("Customs Charges payable from LastClearanceMessage", 123.50m, testInfo.CustomsChargeAmountPayableNow);

			entryHeader.CustomsChargeAmountPayableNow = 100m;
			entryHeader.AQISServicePaymentAmountPayableNow = 50m;

			testInfo = new EFTPaymentInformation(entryHeader, initialiseFromLastClearance: false);
			AssertEquals("Customs Charges payable from stored amount", 100m, testInfo.CustomsChargeAmountPayableNow);
			AssertEquals("AQIS Charges payable from stored amount", 50m, testInfo.AQISServicePaymentAmountPayableNow);
		}

		public void TestValidateCustomsChargeAmountDifference()
		{
			var entryHeader = Factory.NewMoq<CusEntryHeader>();
			entryHeader.Setup(m => m.TotalPayableDueAdvisedInLastClearanceMessage).Returns(new ZDecimal(123.50m));

			var testInfo = new EFTPaymentInformation(entryHeader.Object) { CustomsChargeAmountPayableNow = 50m };

			AssertHasMessageErrorContaining(testInfo.CustomsChargeAmountPayableNowInfo, "Customs advised total amount due is ");
		}

		public void TestValidateCustomsChargeAmountWhenCustomsStatusIsHELD()
		{
			TestDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			EntryHeader.CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;

			var paymentInfo = new EFTPaymentInformation(EntryHeader);
			paymentInfo.CustomsChargeAmountPayableNow = 100;
			AssertNoMessageErrorContaining(paymentInfo.CustomsChargeAmountPayableNowInfo, EFTPaymentInformation.EntryIsHeldAndAmountIsGreatThanZero);

			EntryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;
			paymentInfo = new EFTPaymentInformation(EntryHeader);
			paymentInfo.CustomsChargeAmountPayableNow = 100;
			AssertHasMessageErrorContaining(paymentInfo.CustomsChargeAmountPayableNowInfo, EFTPaymentInformation.EntryIsHeldAndAmountIsGreatThanZero);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EFTPaymentInformation(EntryHeader);
		}

		JobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = JobDeclaration.New(Factory);
				}
				return fTestDec;
			}
		}
		JobDeclaration fTestDec;

		CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = TestDec.CustomsEntryHeaders.AddNew();
				}
				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;
	}
}
