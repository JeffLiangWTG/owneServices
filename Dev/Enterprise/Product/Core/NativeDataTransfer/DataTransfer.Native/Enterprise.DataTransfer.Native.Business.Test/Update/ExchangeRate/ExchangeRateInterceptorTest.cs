using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.Integration;

namespace Enterprise.DataTransfer.Native.Business.Update.ExchangeRate;

sealed class ExchangeRateInterceptorTest : TestCaseWithFactory
{
	public void TestProcessNativeXml_INSERT()
	{
		importHandler.Import(new MemoryStream(Encoding.Default.GetBytes(SingleExchangeRatesXML)));
		AssertContains("RefExchangeRate - 1 inserts, 0 updates, 0 deletes", GetLogs());
	}

	public void TestProcessNativeXml_INSERT_DuplicateExchangeRates()
	{
		importHandler.Import(new MemoryStream(Encoding.Default.GetBytes(DuplicateExchangeRatesXML)));
		AssertContains("Overlapped start date is entered.", GetLogs());
	}

	public void TestProcessNativeXml_INSERT_SameExchangeRatesTwice() => CombineAssertions(() =>
	{
		importHandler.Import(new MemoryStream(Encoding.Default.GetBytes(SingleExchangeRatesXML)));
		AssertContains("First import without error.", "RefExchangeRate - 1 inserts, 0 updates, 0 deletes", GetLogs());
		importHandler.Import(new MemoryStream(Encoding.Default.GetBytes(SingleExchangeRatesXML)));
		AssertContains("Second import with error.", "Overlapped start date is entered.", GetLogs());
	});

	const string SingleExchangeRatesXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<CurrencyExchangeRate version=""20250422"">
	<RefExchangeRate Action=""INSERT"">
		<ExRateType>BUY</ExRateType>
		<StartDate>2025-05-01</StartDate>
		<ExpiryDate>2025-05-01</ExpiryDate>
		<SellRate>18.017458</SellRate>
		<RefCurrency><Code>USD</Code></RefCurrency>
		<GlbCompany><Code>DZA</Code></GlbCompany>
	</RefExchangeRate>
</CurrencyExchangeRate>";

	const string DuplicateExchangeRatesXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<CurrencyExchangeRate version=""20250422"">
	<RefExchangeRate Action=""INSERT"">
		<ExRateType>BUY</ExRateType>
		<StartDate>2025-05-01</StartDate>
		<ExpiryDate>2025-05-01</ExpiryDate>
		<SellRate>18.017458</SellRate>
		<RefCurrency><Code>USD</Code></RefCurrency>
		<GlbCompany><Code>DZA</Code></GlbCompany>
	</RefExchangeRate>
	<RefExchangeRate Action=""INSERT"">
		<ExRateType>BUY</ExRateType>
		<StartDate>2025-05-01</StartDate>
		<ExpiryDate>2025-05-01</ExpiryDate>
		<SellRate>18.2</SellRate>
		<RefCurrency><Code>USD</Code></RefCurrency>
		<GlbCompany><Code>DZA</Code></GlbCompany>
	</RefExchangeRate>
</CurrencyExchangeRate>";

	string GetLogs() => string.Join("\r\n", dummyLogger.Buffer.Logs().Select(log => log.Message).ToArray());

	void ErrorOccur(XElement source, Exception ex) => dummyLogger.Error("Test error: " + ex.Message);

	AncillaryImportServices sessionServices;
	ImportHandler importHandler;
	MemoryLogger dummyLogger;

	protected override void SetUp()
	{
		base.SetUp();

		sessionServices = new AncillaryImportServices();
		dummyLogger = (MemoryLogger)sessionServices.Logger;
		importHandler = new ImportHandler(sessionServices)
		{
			ErrorOccur = ErrorOccur
		};
		TestUtil.PrepareGlbCompanyTableData("DZA");
	}
}
