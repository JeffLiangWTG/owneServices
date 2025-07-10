using System.Globalization;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Lookups = Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro.Testing
{
	public class DTRTest : TestCaseWithFactory
	{
		[TestDate(2019, 01, 01)]
		public void TestDTR()
		{
			var report = CreateComplianceReportWithTransactions();
			var expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlForDTRWithOneTransaction);
			var additionalData = ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(report, ComplianceReportDataCollectionMode.Esterometro);
			var actualXml = new DTR(fileNumber: 1, EsterometroXmlWriter.MaximumTransactionsPerFileDefinedByXSD).BuildXml(report, null, additionalData).ToString();
			this.AssertXMLEqualsByDiff(expectedXml, actualXml);
		}

		const string expectedXmlForDTRWithOneTransaction = @"<DTR>
  <CessionarioCommittenteDTR>
    <IdentificativiFiscali>
      <IdFiscaleIVA>
        <IdPaese>IT</IdPaese>
        <IdCodice></IdCodice>
      </IdFiscaleIVA>
    </IdentificativiFiscali>
    <AltriDatiIdentificativi>
      <Denominazione>EDI CUSTOMS BROKERS</Denominazione>
      <Sede>
        <Indirizzo>10 HUTCHESON STREET ALBION  QLD</Indirizzo>
        <CAP>04010</CAP>
        <Comune />
        <Nazione>AU</Nazione>
      </Sede>
    </AltriDatiIdentificativi>
  </CessionarioCommittenteDTR>
  <CedentePrestatoreDTR>
    <IdentificativiFiscali>
      <IdFiscaleIVA>
        <IdPaese></IdPaese>
        <IdCodice></IdCodice>
      </IdFiscaleIVA>
    </IdentificativiFiscali>
    <AltriDatiIdentificativi>
      <Denominazione />
      <Sede>
        <Indirizzo />
        <CAP></CAP>
        <Comune />
        <Nazione></Nazione>
      </Sede>
    </AltriDatiIdentificativi>
    <DatiFatturaBodyDTR>
      <DatiGenerali>
        <TipoDocumento></TipoDocumento>
        <Data>2019-01-01</Data>
        <Numero>I0000</Numero>
        <DataRegistrazione>2019-01-01</DataRegistrazione>
      </DatiGenerali>
      <DatiRiepilogo>
        <ImponibileImporto>-100.00</ImponibileImporto>
        <DatiIVA>
          <Imposta>0.00</Imposta>
          <Aliquota>0.00</Aliquota>
        </DatiIVA>
        <Natura>N2</Natura>
      </DatiRiepilogo>
      <DatiRiepilogo>
        <ImponibileImporto>-80.00</ImponibileImporto>
        <DatiIVA>
          <Imposta>0.00</Imposta>
          <Aliquota>0.00</Aliquota>
        </DatiIVA>
        <Natura>N2</Natura>
      </DatiRiepilogo>
    </DatiFatturaBodyDTR>
  </CedentePrestatoreDTR>
</DTR>";

		#region Pagination Tests
		[TestDate(2019, 01, 01)]
		public void TestDTRPaginatesFiles_FirstPage()
		{
			var report = CreateComplianceReportWithTransactions(2);
			var expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlForDTRWithOneTransaction);
			var additionalData = ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(report, ComplianceReportDataCollectionMode.Esterometro);
			var actualXml = new DTR(fileNumber: 1, maxTransactionsPerFile: 1).BuildXml(report, null, additionalData).ToString();
			this.AssertXMLEqualsByDiff("First file, one transaction per file, two transactions in report should give XML with first invoice.", expectedXml, actualXml);
		}

		[TestDate(2019, 01, 01)]
		public void TestDTRPaginatesFiles_SecondPage()
		{
			var report = CreateComplianceReportWithTransactions(2);
			var expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlForDTRWithOneTransaction.Replace("<Numero>I0000</Numero>", "<Numero>I0001</Numero>"));
			var additionalData = ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(report, ComplianceReportDataCollectionMode.Esterometro);
			var actualXml = new DTR(fileNumber: 2, maxTransactionsPerFile: 1).BuildXml(report, null, additionalData).ToString();
			this.AssertXMLEqualsByDiff("Second file, one transaction per file, two transactions in report should give XML with second invoice.", expectedXml, actualXml);
		}

		[TestDate(2019, 01, 01)]
		public void TestDTRPaginatesFiles_LargePages()
		{
			var report = CreateComplianceReportWithTransactions(2);
			var expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlForDTRWithTwoTransactions);
			var additionalData = ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(report, ComplianceReportDataCollectionMode.Esterometro);
			var actualXml = new DTR(fileNumber: 1, maxTransactionsPerFile: EsterometroXmlWriter.MaximumTransactionsPerFileDefinedByXSD).BuildXml(report, null, additionalData).ToString();
			this.AssertXMLEqualsByDiff("First file, max transactions per file, two transactions in report should give XML with both invoices.", expectedXml, actualXml);
		}

		const string expectedXmlForDTRWithTwoTransactions = @"<DTR>
  <CessionarioCommittenteDTR>
    <IdentificativiFiscali>
      <IdFiscaleIVA>
        <IdPaese>IT</IdPaese>
        <IdCodice></IdCodice>
      </IdFiscaleIVA>
    </IdentificativiFiscali>
    <AltriDatiIdentificativi>
      <Denominazione>EDI CUSTOMS BROKERS</Denominazione>
      <Sede>
        <Indirizzo>10 HUTCHESON STREET ALBION  QLD</Indirizzo>
        <CAP>04010</CAP>
        <Comune />
        <Nazione>AU</Nazione>
      </Sede>
    </AltriDatiIdentificativi>
  </CessionarioCommittenteDTR>
  <CedentePrestatoreDTR>
    <IdentificativiFiscali>
      <IdFiscaleIVA>
        <IdPaese></IdPaese>
        <IdCodice></IdCodice>
      </IdFiscaleIVA>
    </IdentificativiFiscali>
    <AltriDatiIdentificativi>
      <Denominazione />
      <Sede>
        <Indirizzo />
        <CAP></CAP>
        <Comune />
        <Nazione></Nazione>
      </Sede>
    </AltriDatiIdentificativi>
    <DatiFatturaBodyDTR>
      <DatiGenerali>
        <TipoDocumento></TipoDocumento>
        <Data>2019-01-01</Data>
        <Numero>I0000</Numero>
        <DataRegistrazione>2019-01-01</DataRegistrazione>
      </DatiGenerali>
      <DatiRiepilogo>
        <ImponibileImporto>-100.00</ImponibileImporto>
        <DatiIVA>
          <Imposta>0.00</Imposta>
          <Aliquota>0.00</Aliquota>
        </DatiIVA>
        <Natura>N2</Natura>
      </DatiRiepilogo>
      <DatiRiepilogo>
        <ImponibileImporto>-80.00</ImponibileImporto>
        <DatiIVA>
          <Imposta>0.00</Imposta>
          <Aliquota>0.00</Aliquota>
        </DatiIVA>
        <Natura>N2</Natura>
      </DatiRiepilogo>
    </DatiFatturaBodyDTR>
  </CedentePrestatoreDTR>
  <CedentePrestatoreDTR>
    <IdentificativiFiscali>
      <IdFiscaleIVA>
        <IdPaese></IdPaese>
        <IdCodice></IdCodice>
      </IdFiscaleIVA>
    </IdentificativiFiscali>
    <AltriDatiIdentificativi>
      <Denominazione />
      <Sede>
        <Indirizzo />
        <CAP></CAP>
        <Comune />
        <Nazione></Nazione>
      </Sede>
    </AltriDatiIdentificativi>
    <DatiFatturaBodyDTR>
      <DatiGenerali>
        <TipoDocumento></TipoDocumento>
        <Data>2019-01-01</Data>
        <Numero>I0001</Numero>
        <DataRegistrazione>2019-01-01</DataRegistrazione>
      </DatiGenerali>
      <DatiRiepilogo>
        <ImponibileImporto>-100.00</ImponibileImporto>
        <DatiIVA>
          <Imposta>0.00</Imposta>
          <Aliquota>0.00</Aliquota>
        </DatiIVA>
        <Natura>N2</Natura>
      </DatiRiepilogo>
      <DatiRiepilogo>
        <ImponibileImporto>-80.00</ImponibileImporto>
        <DatiIVA>
          <Imposta>0.00</Imposta>
          <Aliquota>0.00</Aliquota>
        </DatiIVA>
        <Natura>N2</Natura>
      </DatiRiepilogo>
    </DatiFatturaBodyDTR>
  </CedentePrestatoreDTR>
</DTR>";

		[TestDate(2019, 01, 01)]
		public void TestDTRPaginatesFiles_InvalidPage()
		{
			var report = CreateComplianceReportWithTransactions(2);
			var expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlForDTRWithZeroInvoices);
			var additionalData = ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(report, ComplianceReportDataCollectionMode.Esterometro);
			var actualXml = new DTR(fileNumber: 3, maxTransactionsPerFile: 1).BuildXml(report, null, additionalData).ToString();
			this.AssertXMLEqualsByDiff("Third file, one transactions per file, two transactions in report should give XML with no invoices.", expectedXml, actualXml);
		}

		const string expectedXmlForDTRWithZeroInvoices = @"<DTR>
  <CessionarioCommittenteDTR>
    <IdentificativiFiscali>
      <IdFiscaleIVA>
        <IdPaese>IT</IdPaese>
        <IdCodice></IdCodice>
      </IdFiscaleIVA>
    </IdentificativiFiscali>
    <AltriDatiIdentificativi>
      <Denominazione>EDI CUSTOMS BROKERS</Denominazione>
      <Sede>
        <Indirizzo>10 HUTCHESON STREET ALBION  QLD</Indirizzo>
        <CAP>04010</CAP>
        <Comune />
        <Nazione>AU</Nazione>
      </Sede>
    </AltriDatiIdentificativi>
  </CessionarioCommittenteDTR>
</DTR>";
		#endregion

		#region Test Helpers
		AccComplianceReport CreateComplianceReportWithTransactions(int numberOfInvoices = 1)
		{
			var report = Creator.CreateComplianceReportWithTransactions(AccComplianceReport.ReportTypes.Esterometro, periodicity: Lookups.ReportPeriodicityCodes.AccountingPeriod, numberOfTransactions: numberOfInvoices);

			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
			AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
			AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);

			return report;
		}

		#endregion

		#region Implementation

		TestObjectCreator Creator
		{
			get { return creator ?? (creator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator creator;

		#endregion
	}
}
