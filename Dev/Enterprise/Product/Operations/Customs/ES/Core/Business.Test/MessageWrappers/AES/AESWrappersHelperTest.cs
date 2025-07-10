using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ES.Business.Testing;

public class AESWrappersHelperTest : TestCaseWithFactory
{
	public void TestGetTotalAmount()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		var entryHeader = declaration.CustomsEntryHeaders[0];

		CombineAssertions(() =>
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";

			invoiceLine.JI_LinePrice = 1.12m;
			AssertEquals("Expected filled GetTotalAmount with 1 invoice line", 1.12m, AESWrappersHelper.GetTotalAmount(invoiceHeader, entryHeader));

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2.32m;

			mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);
			entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("Expected filled GetTotalAmount with 2 invoice lines 1.12+2.32=3.44", 3.44m, AESWrappersHelper.GetTotalAmount(invoiceHeader, entryHeader));

			invoiceHeader.JZ_RX_NKInvoice_Currency = "000";
			AssertEquals("Expected 0 GetTotalAmount when currency is 000", ZDecimal.Zero, AESWrappersHelper.GetTotalAmount(invoiceHeader, entryHeader));
		});
	}

	public void TestGetCurrency()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();

		invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
		AssertEquals("Expected filled GetCurrency", "EUR", AESWrappersHelper.GetCurrency(invoiceHeader));
	}

	public void TestActiveBorderTransportMeans()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "MQ", "FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();

		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertNull("Expected null GetActiveBorderTransportMeans when no data declared (even if TransportMode is declared)", AESWrappersHelper.GetActiveBorderTransportMeans(declaration));

			declaration.ZG_BorderTransportMeans = "00";
			AssertNotNull("Expected filled GetActiveBorderTransportMeans when TransportMode is not empty", AESWrappersHelper.GetActiveBorderTransportMeans(declaration));

			declaration.JE_TransportMode = ZString.Empty;
			AssertNull("Expected null GetActiveBorderTransportMeans when TransportMode is empty", AESWrappersHelper.GetActiveBorderTransportMeans(declaration));
		});

		CombineAssertions("ActiveBorderTransportMeans should be using default territory if present.", () =>
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.ZG_BorderTransportMeans = "00";
			declaration.JE_RN_NKTransportNationality = "RS";
			AssertEquals("Expected filled Country with Default Territory (XS)", "XS", AESWrappersHelper.GetActiveBorderTransportMeans(declaration).TransportNationality);

			declaration.JE_RN_NKTransportNationality = "ES";
			AssertEquals("Expected filled Country with given code since there is no Default Territory", "ES", AESWrappersHelper.GetActiveBorderTransportMeans(declaration).TransportNationality);

			declaration.JE_RN_NKTransportNationality = "MQ";
			AssertEquals("Expected filled Country with Default Territory (FR)", "FR", AESWrappersHelper.GetActiveBorderTransportMeans(declaration).TransportNationality);
		});
	}

	public void TestLineNumber()
	{
		CusSupportingInfo document = Factory.New<CusSupportingInfo>();
		CombineAssertions(() =>
		{
			document.CSI_LineNo = 2;
			document.CSI_ItemNumber = 5;
			AssertEquals("Expected filled LineNumber", "5", AESWrappersHelper.GetLineNumberForSupportingDocument(document));

			document.CSI_ItemNumber = 0;
			AssertEquals("Expected empty LineNumber when 0", ZString.Empty, AESWrappersHelper.GetLineNumberForSupportingDocument(document));
		});
	}
}
