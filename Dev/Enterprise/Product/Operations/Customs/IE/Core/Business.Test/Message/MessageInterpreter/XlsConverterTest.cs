using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using FlexCel.XlsAdapter;

namespace Enterprise.Customs.IE.Business.Testing;

class XlsConverterTest : TestCaseWithFactory
{
	public void TestToXlsxStream()
	{
		var message = Factory.New<CustomsAndExciseReportInboundMessage>();
		message.EM_MessageType = CustomsAndExciseReportTypeList.Codes.PSR;
		var incomingJson = CustomsAndExciseReportInterchangeProcessorTestHelper.CreatePSRText();
		message.EM_MessageText = incomingJson;
		var psrProvider = message.GetDataProvider<PSRProvider>(typeof(PSRMessage));
		using var xlsxStream = XlsConverter.ToXlsxStream(psrProvider, "PSR - Period (Monthly) details", out var format);

		var xlsFile = new XlsFile(xlsxStream, false);
		xlsFile.ActiveSheet = 1;

		CombineAssertions("Output xls stream.", () =>
		{
			AssertEquals("Sheet name", "PSR - Period (Monthly) details", xlsFile.SheetName);

			var row = 1;

			AssertCellValue(xlsFile, row, 1, "EORI");
			AssertCellValue(xlsFile, row, 2, "Period");
			AssertCellValue(xlsFile, row, 3, "Tax Total");

			row = 2;
			AssertCellValue(xlsFile, row, 1, "IE1234567A");
			AssertCellValue(xlsFile, row, 2, "01-Aug-22 00:00:00");
			AssertCellValue(xlsFile, row, 3, "400.0");

			row = 4;
			AssertCellValue(xlsFile, row, 1, "Tax Breakdowns");

			row = 6;
			AssertCellValue(xlsFile, row, 1, "Tax Type");
			AssertCellValue(xlsFile, row, 2, "Payable Amount");
			row = 7;
			AssertCellValue(xlsFile, row, 1, "A00");
			AssertCellValue(xlsFile, row, 2, "150.0");
			row = 8;
			AssertCellValue(xlsFile, row, 1, "B00");
			AssertCellValue(xlsFile, row, 2, "250.0");

			row = 10;
			AssertCellValue(xlsFile, row, 1, "Daily Breakdowns");
			row = 12;
			AssertCellValue(xlsFile, row, 1, "Date");
			AssertCellValue(xlsFile, row, 2, "Tax Total");
			row = 13;
			AssertCellValue(xlsFile, row, 1, "10-Aug-22 00:00:00");
			AssertCellValue(xlsFile, row, 2, "3200.0");
			row = 14;
			AssertCellValue(xlsFile, row, 1, "11-Aug-22 00:00:00");
			AssertCellValue(xlsFile, row, 2, "200.0");
		});
	}

	void AssertCellValue(XlsFile file, int row, int column, object expectedValue)
	{
		AssertEquals($"Value on cell [{row} {column}].", expectedValue, file.GetCellValue(row, column));
	}
}
