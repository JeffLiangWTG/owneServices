using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class Message818HeaderProviderHelperTest : TestCaseWithFactory
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

		public void TestDateOfArrivalOfExciseProducts()
		{
			AssertEquals(null, helper.DateOfArrivalOfExciseProducts);
			reportOfReceipt.ArrivalDate = new ZDate(2019, 12, 30);
			AssertEquals("Returns Correct value", new DateTime(2019, 12, 30, 0, 0, 0), helper.DateOfArrivalOfExciseProducts);
		}

		public void TestGlobalConclusionOfReceipt()
		{
			AssertEquals(string.Empty, helper.GlobalConclusionOfReceipt);
			reportOfReceipt.ReceiptResult = EMCSReceiptResultList.Codes.ReceiptAcceptedAlthoughUnsatisfactory;
			AssertEquals(EMCSReceiptResultList.Codes.ReceiptAcceptedAlthoughUnsatisfactory, helper.GlobalConclusionOfReceipt);
		}

		public void TestIsReceiptPartiallyRefused()
		{
			CombineAssertions(() =>
			{
				reportOfReceipt.ReceiptResult = EMCSReceiptResultList.Codes.ReceiptAcceptedAlthoughUnsatisfactory;
				AssertEquals("Not partially refused", false, helper.IsReceiptPartiallyRefused);
				reportOfReceipt.ReceiptResult = EMCSReceiptResultList.Codes.ReceiptPartiallyRefused;
				AssertEquals("Is partially refused", true, helper.IsReceiptPartiallyRefused);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			reportOfReceipt = new ReportOfReceiptSendingAction(emcsDeclaration);
			helper = new Message818HeaderProviderHelper(emcsDeclaration, reportOfReceipt);
		}
		EMCSJobDeclaration emcsDeclaration;
		Message818HeaderProviderHelper helper;
		ReportOfReceiptSendingAction reportOfReceipt;
	}
}
