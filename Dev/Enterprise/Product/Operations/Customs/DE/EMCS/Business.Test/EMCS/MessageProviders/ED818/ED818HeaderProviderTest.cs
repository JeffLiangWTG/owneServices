using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED818HeaderProvider))]
	class ED818HeaderProviderTest : HeaderProviderAbstractTest<ED818HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ED818HeaderProvider(emcsDeclaration, null));
		}

		public void TestSequenceNumber()
		{
			var cusEntryNumber = CusEntryNumber.New(emcsDeclaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNumber.CE_EntryLineReference = "1";

			AssertEquals(1, HeaderProvider.SequenceNumber);
		}

		public void TestSequenceNumber_Empty()
		{
			AssertEquals("No CusEntryNum", 0, HeaderProvider.SequenceNumber);
		}

		public void TestDestinationOfficeReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No offices entered", string.Empty, HeaderProvider.DestinationOfficeReferenceNumber);
				var deliveryCustomsOffice = emcsDeclaration.CustomsOffices.AddNew();
				deliveryCustomsOffice.CY_Code = OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival;
				deliveryCustomsOffice.CY_Data = "DE00876";

				var dispatchCustomsOffice = emcsDeclaration.CustomsOffices.AddNew();
				dispatchCustomsOffice.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDispatch;
				dispatchCustomsOffice.CY_Data = "DE00934";
				AssertEquals("Delivery office code", "DE00876", HeaderProvider.DestinationOfficeReferenceNumber);
			});
		}

		public void TestDateOfArrivalOfExciseProducts()
		{
			AssertEquals(null, HeaderProvider.DateOfArrivalOfExciseProducts);
			reportOfReceipt.ArrivalDate = new ZDate(2019, 12, 30);
			AssertEquals("Returns Correct value", new DateTime(2019, 12, 30, 0, 0, 0), HeaderProvider.DateOfArrivalOfExciseProducts);
		}

		public void TestGlobalConclusionOfReceipt()
		{
			AssertEquals(string.Empty, HeaderProvider.GlobalConclusionOfReceipt);
			reportOfReceipt.ReceiptResult = EMCSReceiptResultList.Codes.ReceiptAcceptedAlthoughUnsatisfactory;
			AssertEquals(EMCSReceiptResultList.Codes.ReceiptAcceptedAlthoughUnsatisfactory, HeaderProvider.GlobalConclusionOfReceipt);
		}

		public void TestIsReceiptPartiallyRefused()
		{
			CombineAssertions(() =>
			{
				reportOfReceipt.ReceiptResult = EMCSReceiptResultList.Codes.ReceiptAcceptedAlthoughUnsatisfactory;
				AssertEquals("Not partially refused", false, HeaderProvider.IsReceiptPartiallyRefused);
				reportOfReceipt.ReceiptResult = EMCSReceiptResultList.Codes.ReceiptPartiallyRefused;
				AssertEquals("Is partially refused", true, HeaderProvider.IsReceiptPartiallyRefused);
			});
		}

		public void TestComplementaryInformation()
		{
			reportOfReceipt.ComplementaryInformation = "OTHER DESCRIPTION";
			AssertEquals("OTHER DESCRIPTION", HeaderProvider.ComplementaryInformation.Text);
		}

		public void TestConsigneeTraderNull()
		{
			AssertNull("No Consignee Trader entered", HeaderProvider.ConsigneeTrader);
		}

		public void TestConsigneeTrader()
		{
			emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
			emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = GetPartyConsigneeOrg("TRD821", "43762894").PK;
			var consigneeTrader = HeaderProvider.ConsigneeTrader;
			CombineAssertions(() =>
			{
				AssertEquals("Trader ID", "TRD821", consigneeTrader.TraderId);
				AssertEquals("EORI Number", "GR43762894", consigneeTrader.EoriNumber);
			});
		}

		public void TestDeliveryPlaceTraderNull()
		{
			AssertNull("No Destination Warehouse entered", HeaderProvider.DeliveryPlaceTrader);
		}

		public void TestDeliveryPlaceTrader()
		{
			emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;
			emcsDeclaration.DestinationWarehouseDocumentaryAddress.OrganisationPK = GetPartyTraderIdOrg("TID355").PK;
			AssertEquals("Trader ID", "TID355", HeaderProvider.DeliveryPlaceTrader.TraderId);
		}

		public void TestLines_Null()
		{
			AssertEquals("Collections doesn't return null but is empty", false, HeaderProvider.Lines.Any());
		}

		public void TestLines()
		{
			var invoiceHeader = emcsDeclaration.InvoiceHeader;
			invoiceHeader.InvoiceLines.AddNew();
			for (var i = 1; i < 6; i++)
			{
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.Outturn.ReportOfReceiptReasons.AddNew();
			}
			AssertEquals("5 records with ReportOfReceiptReasons, contents is tested in the provider", 5, HeaderProvider.Lines.Count);
		}

		public void TestLinesEmptyIfReceiptResultIsReceiptAcceptedAndSatisfactory() => AssertLinesEmpty(EMCSReceiptResultList.Codes.ReceiptAcceptedAndSatisfactory);

		public void TestLinesEmptyIfReceiptResultIsExitAcceptedAndSatisfactory() => AssertLinesEmpty(EMCSReceiptResultList.Codes.ExitAcceptedAndSatisfactory);

		protected override void SetUp()
		{
			base.SetUp();
			reportOfReceipt = new ReportOfReceiptSendingAction(emcsDeclaration);
		}
		ReportOfReceiptSendingAction reportOfReceipt;

		protected override ED818HeaderProvider GetHeaderProvider() => new ED818HeaderProvider(emcsDeclaration, reportOfReceipt);

		protected override ED818HeaderProvider GetProvider()
		{
			reportOfReceipt.ComplementaryInformation = "OTHER DESCRIPTION";
			emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
			emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = GetPartyConsigneeOrg("TRD821", "43762894").PK;
			emcsDeclaration.DestinationWarehouseDocumentaryAddress.OrganisationPK = GetPartyTraderIdOrg("TID355").PK;
			return new ED818HeaderProvider(emcsDeclaration, reportOfReceipt);
		}

		protected override IEnumerable<Expression<Func<ED818HeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ComplementaryInformation;
			yield return x => x.ConsigneeTrader;
			yield return x => x.DeliveryPlaceTrader;
		}

		protected new IED818Header HeaderProvider => base.HeaderProvider;

		void AssertLinesEmpty(string receiptResult)
		{
			reportOfReceipt.ReceiptResult = receiptResult;
			AssertEquals("lines empty", false, HeaderProvider.Lines.Any());
		}
	}
}
