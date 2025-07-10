using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.CDS.CDSResponse.Testing
{
	public class ResponseFunctionTests : TestCaseWithFactory
	{
		public void TestShouldSendEntryDocs()
		{
			SetUpZZRefData(Factory);

			AssertShouldSendEntryDocs(new ResponseFunction.Empty(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.DeclarationAccepted(), true);
			AssertShouldSendEntryDocs(new ResponseFunction.MessageRegistered(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.MessageRejected(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.DeclarationIncomplete(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.DeclarationSubjectToPhysicalControl(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.DeclarationSubjectToPhysicalControl2(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.DeclarationUpdatedByCustoms(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.GoodsMayBeReleased(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.DeclarationCleared(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.DeclarationCancelled(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.AdditionalMessageProcessed(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.DutiesTaxesCalculatedAndDue(), true);
			AssertShouldSendEntryDocs(new ResponseFunction.InsufficientDefermentBalance(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.PaymentDue(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.GoodsExitedCustomsUnion(), true);
			AssertShouldSendEntryDocs(new ResponseFunction.ExceptionalIrregularityNeedsToBeHandled(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.ExitOfGoodsFromEUNotConfirmed(), false);
			AssertShouldSendEntryDocs(new ResponseFunction.DefraControl(), false);
		}

		void AssertShouldSendEntryDocs(ResponseFunction responseFunction, ZBool expectedShould)
		{
			var yupShould = responseFunction.ShouldSendEntryDocs(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService);
			AssertEquals(expectedShould, yupShould);
		}

		public void TestShouldUpdateCustomsStatus()
		{
			SetUpZZRefData(Factory);

			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationAccepted(), Constants.ThreeCharFunctionCodes.MessageRegistered, true, Constants.ThreeCharFunctionCodes.DeclarationAccepted);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationAccepted(), ZString.Empty, true, Constants.ThreeCharFunctionCodes.DeclarationAccepted);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationAccepted(), "XXX", false, Constants.ThreeCharFunctionCodes.DeclarationAccepted);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationAccepted(), Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue, true, Constants.ThreeCharFunctionCodes.DeclarationAccepted);

			AssertShouldUpdateCustomsStatus(new ResponseFunction.MessageRegistered(), ZString.Empty, true, Constants.ThreeCharFunctionCodes.MessageRegistered);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.MessageRegistered(), Constants.ThreeCharFunctionCodes.MessageRegistered, true, Constants.ThreeCharFunctionCodes.MessageRegistered);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.MessageRegistered(), "XXX", false, Constants.ThreeCharFunctionCodes.MessageRegistered);

			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationSubjectToPhysicalControl(), ZString.Empty, true, Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationSubjectToPhysicalControl(), Constants.ThreeCharFunctionCodes.DeclarationAccepted, true, Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationSubjectToPhysicalControl(), Constants.ThreeCharFunctionCodes.MessageRegistered, true, Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationSubjectToPhysicalControl(), "XXX", false, Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationSubjectToPhysicalControl(), Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue, true, Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl);

			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationSubjectToPhysicalControl2(), ZString.Empty, true, Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl2);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationSubjectToPhysicalControl2(), Constants.ThreeCharFunctionCodes.DeclarationAccepted, true, Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl2);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationSubjectToPhysicalControl2(), Constants.ThreeCharFunctionCodes.MessageRegistered, true, Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl2);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationSubjectToPhysicalControl2(), "XXX", false, Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl2);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationSubjectToPhysicalControl2(), Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue, true, Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl2);

			AssertShouldUpdateCustomsStatus(new ResponseFunction.DutiesTaxesCalculatedAndDue(), ZString.Empty, true, Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DutiesTaxesCalculatedAndDue(), Constants.ThreeCharFunctionCodes.DeclarationAccepted, false, Constants.ThreeCharFunctionCodes.DeclarationAccepted);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DutiesTaxesCalculatedAndDue(), Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl, false, Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DutiesTaxesCalculatedAndDue(), Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl2, false, Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl2);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DutiesTaxesCalculatedAndDue(), Constants.ThreeCharFunctionCodes.MessageRegistered, true, Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DutiesTaxesCalculatedAndDue(), "XXX", false, Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DutiesTaxesCalculatedAndDue(), Constants.ThreeCharFunctionCodes.DeclarationCleared, false, Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DutiesTaxesCalculatedAndDue(), Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue, false, Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue);

			AssertShouldUpdateCustomsStatus(new ResponseFunction.GoodsMayBeReleased(), "XXX", true, EntryStatusList.Codes.Clear);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationCleared(), "XXX", true, EntryStatusList.Codes.Clear);

			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationCancelled(), "XXX", true, EntryStatusList.Codes.Cancelled);

			AssertShouldUpdateCustomsStatus(new ResponseFunction.MessageRejected(), ZString.Empty, false, ZString.Empty);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.MessageRejected(), EntryStatusList.Codes.Cancelled, false, ZString.Empty);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.MessageRejected(), "XXX", true, EntryStatusList.Codes.Cancelled);

			AssertShouldUpdateCustomsStatus(new ResponseFunction.Empty(), "XXX", false, ZString.Empty);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DeclarationIncomplete(), "XXX", false, ZString.Empty);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.AdditionalMessageProcessed(), "XXX", false, ZString.Empty);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.InsufficientDefermentBalance(), "XXX", false, ZString.Empty);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.PaymentDue(), "XXX", false, ZString.Empty);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.GoodsExitedCustomsUnion(), "XXX", false, ZString.Empty);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.ExceptionalIrregularityNeedsToBeHandled(), "XXX", false, ZString.Empty);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.ExitOfGoodsFromEUNotConfirmed(), "XXX", false, ZString.Empty);
			AssertShouldUpdateCustomsStatus(new ResponseFunction.DefraControl(), "XXX", false, ZString.Empty);
		}

		void AssertShouldUpdateCustomsStatus(ResponseFunction responseFunction, ZString orignalStatus, ZBool expectedShould, ZString expectedNewStatus)
		{
			var (should, newStatus) = responseFunction.ShouldUpdateCustomsStatus(Factory, orignalStatus, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService);
			AssertEquals(expectedShould, should);
			AssertEquals(expectedNewStatus, newStatus);
		}

		public void TestShouldUpdateEntryNumber()
		{
			SetUpZZRefData(Factory);

			AssertShouldUpdateEntryNumber(new ResponseFunction.Empty(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.DeclarationAccepted(), true);
			AssertShouldUpdateEntryNumber(new ResponseFunction.MessageRegistered(), true);
			AssertShouldUpdateEntryNumber(new ResponseFunction.MessageRejected(), true);
			AssertShouldUpdateEntryNumber(new ResponseFunction.DeclarationIncomplete(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.DeclarationSubjectToPhysicalControl(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.DeclarationSubjectToPhysicalControl2(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.DeclarationUpdatedByCustoms(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.GoodsMayBeReleased(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.DeclarationCleared(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.DeclarationCancelled(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.AdditionalMessageProcessed(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.DutiesTaxesCalculatedAndDue(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.InsufficientDefermentBalance(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.PaymentDue(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.GoodsExitedCustomsUnion(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.ExceptionalIrregularityNeedsToBeHandled(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.ExitOfGoodsFromEUNotConfirmed(), false);
			AssertShouldUpdateEntryNumber(new ResponseFunction.DefraControl(), false);
		}

		void AssertShouldUpdateEntryNumber(ResponseFunction responseFunction, ZBool expectedShould)
		{
			var should = responseFunction.ShouldUpdateEntryNumber(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService);
			AssertEquals(expectedShould, should);
		}

		public void TestResponseFunction()
		{
			AssertResponseFunction<ResponseFunction.Empty>(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertResponseFunction<ResponseFunction.DeclarationAccepted>("01", "ACC", "Declaration has been legally accepted", "E2");
			AssertResponseFunction<ResponseFunction.MessageRegistered>("02", "RCV", "Message has been registered", "H2/P2");
			AssertResponseFunction<ResponseFunction.MessageRejected>("03", "REJ", "Pre-lodged declaration canceled OK", "27 (or N3/S3)", EDIMessageStatusList.Codes.Cancelled);
			AssertResponseFunction<ResponseFunction.MessageRejected>("03", "REJ", "Message has been rejected", "27 (or N3/S3)", ZString.Empty);
			AssertResponseFunction<ResponseFunction.DeclarationIncomplete>("04", "INC", "Declaration is incomplete", ZString.Empty);
			AssertResponseFunction<ResponseFunction.DeclarationSubjectToPhysicalControl>("05", "CTL", "Declaration is subject to physical control", "E1/X1");
			AssertResponseFunction<ResponseFunction.DeclarationSubjectToPhysicalControl2>("06", "DOC", "Declaration is subject to physical control", ZString.Empty);
			AssertResponseFunction<ResponseFunction.DeclarationUpdatedByCustoms>("07", "RES", "Declaration has been updated by Customs", ZString.Empty);
			AssertResponseFunction<ResponseFunction.GoodsMayBeReleased>("08", "ROG", "Goods may now be released", "N5");
			AssertResponseFunction<ResponseFunction.DeclarationCleared>("09", "CLE", "Declaration is now cleared", ZString.Empty);
			AssertResponseFunction<ResponseFunction.DeclarationCancelled>("10", "INV", "Declaration has been canceled", "N4/S4/S8");
			AssertResponseFunction<ResponseFunction.AdditionalMessageProcessed>("11", "REQ", "Additional message has been processed", ZString.Empty);
			AssertResponseFunction<ResponseFunction.DutiesTaxesCalculatedAndDue>("13", "TAX", "Duties and taxes have been calculated and are due", "E2");
			AssertResponseFunction<ResponseFunction.InsufficientDefermentBalance>("14", "CPI", "Insufficient deferment balance", "E9");
			AssertResponseFunction<ResponseFunction.PaymentDue>("15", "CPR", "Payment is due (reminder)", ZString.Empty);
			AssertResponseFunction<ResponseFunction.GoodsExitedCustomsUnion>("16", "EOG", "Goods have exited the Customs Union", ZString.Empty);
			AssertResponseFunction<ResponseFunction.ExceptionalIrregularityNeedsToBeHandled>("17", "EXT", "Exceptional irregularity needs to be handled", ZString.Empty);
			AssertResponseFunction<ResponseFunction.ExitOfGoodsFromEUNotConfirmed>("18", "GER", "Exit of goods from Customs Union is not yet confirmed", "S0");
			AssertResponseFunction<ResponseFunction.DefraControl>("50", "ALV", "DEFRA control applied", ZString.Empty);
		}

		public void TestGetNumericFunctionCode()
		{
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.DeclarationAccepted, Constants.NumbericFunctionCodes.DeclarationAccepted);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.MessageRegistered, Constants.NumbericFunctionCodes.MessageRegistered);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.MessageRejected, Constants.NumbericFunctionCodes.MessageRejected);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.DeclarationIncomplete, Constants.NumbericFunctionCodes.DeclarationIncomplete);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl, Constants.NumbericFunctionCodes.DeclarationSubjectToPhysicalControl);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl2, Constants.NumbericFunctionCodes.DeclarationSubjectToPhysicalControl2);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.DeclarationUpdatedByCustoms, Constants.NumbericFunctionCodes.DeclarationUpdatedByCustoms);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.GoodsMayBeReleased, Constants.NumbericFunctionCodes.GoodsMayBeReleased);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.DeclarationCleared, Constants.NumbericFunctionCodes.DeclarationCleared);
			AssertNumericFunctionCode(EntryStatusList.Codes.Clear, Constants.NumbericFunctionCodes.DeclarationCleared);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.DeclarationCancelled, Constants.NumbericFunctionCodes.DeclarationCancelled);
			AssertNumericFunctionCode(EntryStatusList.Codes.Cancelled, Constants.NumbericFunctionCodes.DeclarationCancelled);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.AdditionalMessageProcessed, Constants.NumbericFunctionCodes.AdditionalMessageProcessed);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue, Constants.NumbericFunctionCodes.DutiesTaxesCalculatedAndDue);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.InsufficientDefermentBalance, Constants.NumbericFunctionCodes.InsufficientDefermentBalance);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.PaymentDue, Constants.NumbericFunctionCodes.PaymentDue);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.GoodsExitedCustomsUnion, Constants.NumbericFunctionCodes.GoodsExitedCustomsUnion);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.ExceptionalIrregularityNeedsToBeHandled, Constants.NumbericFunctionCodes.ExceptionalIrregularityNeedsToBeHandled);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.ExitOfGoodsFromEUNotConfirmed, Constants.NumbericFunctionCodes.ExitOfGoodsFromEUNotConfirmed);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.DefraControl, Constants.NumbericFunctionCodes.DefraControl);
			AssertNumericFunctionCode(Constants.ThreeCharFunctionCodes.IncomingQueryNotification, Constants.NumbericFunctionCodes.IncomingQueryNotification);
			AssertNumericFunctionCode("XXX", ZString.Empty);
		}

		static void AssertNumericFunctionCode(ZString threeCharFunctionCode, ZString expectedumericFunctionCode)
		{
			AssertEquals(threeCharFunctionCode, expectedumericFunctionCode, ResponseFunction.GetNumericFunctionCode(threeCharFunctionCode));
		}

		void AssertResponseFunction<T>(ZString numericFunctionCode, ZString threeCharFunctionCode, ZString description, ZString oldCHIEFReportCode, string entryStatus = "") where T : ResponseFunction
		{
			var responseFunction = ResponseFunction.New(numericFunctionCode);
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_EntryStatus = entryStatus;
			AssertType<T>(responseFunction);
			AssertEquals("Numeric Function Code", numericFunctionCode, responseFunction.NumericFunctionCode);
			AssertEquals("Three-Char Function Code", threeCharFunctionCode, responseFunction.ThreeCharFunctionCode);
			AssertEquals("Description", description, responseFunction.GetDescription(entryHeader));
			AssertEquals("Old CHIEF Report Code", oldCHIEFReportCode, responseFunction.OldCHIEFReportCode);
		}

		public void TestGetDescription()
		{
			AssertResponseDescription(ZString.Empty, ZString.Empty);
			AssertResponseDescription("ACC", "Declaration has been legally accepted");
			AssertResponseDescription("RCV", "Message has been registered");
			AssertResponseDescription("REJ", "Message has been rejected");
			AssertResponseDescription("INC", "Declaration is incomplete");
			AssertResponseDescription("CTL", "Declaration is subject to physical control");
			AssertResponseDescription("DOC", "Declaration is subject to physical control");
			AssertResponseDescription("RES", "Declaration has been updated by Customs");
			AssertResponseDescription("ROG", "Goods may now be released");
			AssertResponseDescription("CLE", "Declaration is now cleared");
			AssertResponseDescription("INV", "Declaration has been canceled");
			AssertResponseDescription("REQ", "Additional message has been processed");
			AssertResponseDescription("TAX", "Duties and taxes have been calculated and are due");
			AssertResponseDescription("CPI", "Insufficient deferment balance");
			AssertResponseDescription("CPR", "Payment is due (reminder)");
			AssertResponseDescription("EOG", "Goods have exited the Customs Union");
			AssertResponseDescription("EXT", "Exceptional irregularity needs to be handled");
			AssertResponseDescription("GER", "Exit of goods from Customs Union is not yet confirmed");
			AssertResponseDescription("ALV", "DEFRA control applied");
		}

		static void AssertResponseDescription(ZString threeCharFunctionCode, ZString description)
		{
			var descriptionReturned = ResponseFunction.GetDescription(threeCharFunctionCode);
			AssertEquals("Description", description, descriptionReturned);
		}

		public static void SetUpZZRefData(BusinessObjectFactory factory)
		{
			var testHelper = new CDSResponseStatusTestDataHelper(factory);
			testHelper.CreateCDSCustomsStatuses();

			testHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ErrorCode, "Error Code");
			testHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ErrorCode, "CDS10001", "Obligation error: Obligation rule is not met", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			factory.Save();
		}
	}
}
