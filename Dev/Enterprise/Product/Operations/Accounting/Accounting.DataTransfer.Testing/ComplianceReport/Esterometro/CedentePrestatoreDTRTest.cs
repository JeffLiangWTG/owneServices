using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro.Testing
{
	public class CedentePrestatoreDTRTest : EsterometroTestCaseWithFactory
	{
		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_CreditorHasPrimaryBusinessRegistrationNumber()
		{
			var creditor = CreateCreditorOrganisation(true, true, true);
			var invoice = CreateAPInvoice(creditor);
			Factory.Save();

			var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD01</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APINV1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>300.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
      <Natura>N2</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>700.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>70.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>TG</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
			AssertXML(expectedXML, invoice);
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_CreditorDoesNotHavePrimaryBusinessRegistrationNumberButHasGBR()
		{
			var creditor = CreateCreditorOrganisation(false, true, true);
			var invoice = CreateAPInvoice(creditor);
			Factory.Save();

			var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>45689789</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD01</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APINV1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>300.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
      <Natura>N2</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>700.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>70.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>TG</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
			AssertXML(expectedXML, invoice);
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_CreditorDoesNotHavePrimaryBusinessRegistrationNumberAndGBRButHasGCR()
		{
			var creditor = CreateCreditorOrganisation(false, false, true);
			var invoice = CreateAPInvoice(creditor);
			Factory.Save();

			var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>11112222</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD01</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APINV1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>300.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
      <Natura>N2</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>700.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>70.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>TG</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
			AssertXML(expectedXML, invoice);
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_CreditorDoesNotHavePrimaryBusinessRegistrationNumberAndGBRAndGCR()
		{
			var creditor = CreateCreditorOrganisation(false, false, false);
			var invoice = CreateAPInvoice(creditor);
			Factory.Save();

			var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese></IdPaese>
      <IdCodice></IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD01</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APINV1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>300.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
      <Natura>N2</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>700.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>70.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>TG</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
			AssertXML(expectedXML, invoice);
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_CreditorFullNameIsLongerThan80Characters()
		{
			var creditor = CreateCreditorOrganisation(true, true, true);
			creditor.OH_FullName = "This is my really really really really really really really long organization name";
			var invoice = CreateAPInvoice(creditor);
			Factory.Save();

			var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>This is my really really really really really really really long organization na</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD01</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APINV1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>300.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
      <Natura>N2</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>700.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>70.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>TG</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
			AssertXML(expectedXML, invoice);
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_CreditorMainAddressPostCodeIsLongerThan5Characters()
		{
			var creditor = CreateCreditorOrganisation(true, true, true);
			creditor.MainAddress.OA_PostCode = "200563";
			var invoice = CreateAPInvoice(creditor);
			Factory.Save();

			var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20056</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD01</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APINV1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>300.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
      <Natura>N2</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>700.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>70.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>TG</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
			AssertXML(expectedXML, invoice);
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_CreditorMainAddressPostCodeIsShorterThan5Characters()
		{
			var creditor = CreateCreditorOrganisation(true, true, true);
			creditor.MainAddress.OA_PostCode = "256";
			var invoice = CreateAPInvoice(creditor);
			Factory.Save();

			var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>00256</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD01</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APINV1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>300.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
      <Natura>N2</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>700.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>70.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>TG</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
			AssertXML(expectedXML, invoice);
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_CreditorNameAddressAndCityHaveSpecialCharacters()
		{
			var creditor = CreateCreditorOrganisation(true, true, true);
			creditor.OH_FullName = "Italy SRL a €ocio unico";
			creditor.MainAddress.OA_Address1 = "Via Ca€€ane€e";
			creditor.MainAddress.OA_City = "Bre€cia";
			var invoice = CreateAPInvoice(creditor);
			Factory.Save();

			var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a  ocio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Ca  ane e 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Bre cia</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD01</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APINV1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>300.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
      <Natura>N2</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>700.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>70.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>TG</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
			AssertXML(expectedXML, invoice);
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_WhenCreditorIsNotInItaly()
		{
			var creditor = CreateCreditorOrganisation(true, true, true, false);
			var invoice = CreateAPInvoice(creditor);
			Factory.Save();

			var datiRiepilogoFormat = @"    <DatiRiepilogo>
      <ImponibileImporto>{0}</ImponibileImporto>
      <DatiIVA>
        <Imposta>{1}</Imposta>
        <Aliquota>{2}</Aliquota>
      </DatiIVA>
    </DatiRiepilogo>";

			var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>AU</IdPaese>
      <IdCodice>5682356</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>John Brown Pty Ltd</Denominazione>
    <Sede>
      <Indirizzo>14 Doody Street</Indirizzo>
      <CAP>02015</CAP>
      <Comune>Alexandria</Comune>
      <Nazione>AU</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD01</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APINV1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
{0}
{1}
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				ObjectCreator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.Esterometro, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TaxReporting);
				var report = CreateEsterometroReport(invoice);
				var reportLines = report.ReportLines.Cast<AccComplianceReportLine>().OrderBy(x => x.ACL_ReportSequence).ToList();
				var riepologo1 = String.Format(CultureInfo.InvariantCulture, datiRiepilogoFormat, Math.Abs(reportLines[0].TotalExTaxAmount.Round(2)), Math.Abs(reportLines[0].TotalTaxAmount.Round(2)), reportLines[0].AL_TaxRate.Round(2));
				var riepologo2 = String.Format(CultureInfo.InvariantCulture, datiRiepilogoFormat, Math.Abs(reportLines[1].TotalExTaxAmount.Round(2)), Math.Abs(reportLines[1].TotalTaxAmount.Round(2)), reportLines[1].AL_TaxRate.Round(2));

				expectedXML = String.Format(expectedXML, riepologo1, riepologo2);
				this.AssertXMLEqualsByDiff(expectedXML, new CedentePrestatoreDTR(reportLines).BuildXml(report, null, ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(report, ComplianceReportDataCollectionMode.Esterometro)).ToString());
			}
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_TipoDocumento_ComplianceSubTypes_INV()
		{
			AssertCedentePrestatoreDTRXml(ItalyComplianceInfo.ComplianceSubTypeCodes.APS);

			void AssertCedentePrestatoreDTRXml(string complianceSubType)
			{
				var creditor = CreateCreditorOrganisation(true, true, true);
				var invoice = CreateAPInvoice(creditor, complianceSubType: complianceSubType);
				Factory.Save();

				var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD01</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APINV1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>300.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
      <Natura>N2</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>700.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>70.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>TG</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
				AssertXML(expectedXML, invoice);
				ObjectCreator.ClearComplianceReportQueue();
			}
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_TipoDocumento_CRD()
		{
			SetUpStaticPKTaxRates();
			AssertCedentePrestatoreDTRXml(ItalyComplianceInfo.ComplianceSubTypeCodes.APS, "1");
			AssertCedentePrestatoreDTRXml(ItalyComplianceInfo.ComplianceSubTypeCodes.INT, "3");

			void AssertCedentePrestatoreDTRXml(string complianceSubType, string refNumber)
			{
				var creditor = CreateCreditorOrganisation(true, true, true);
				var shipment = ObjectCreator.CreateShipment("S1000" + refNumber);
				var job = ObjectCreator.CreateJob(shipment);
				var apCreditNote = ObjectCreator.CreateAPCreditNote("APCRD1", creditor, ObjectCreator.EUR, 1m, "ap credit note");
				var line1 = ObjectCreator.CreateAPCreditNoteLine(apCreditNote, job, ObjectCreator.CC3, ObjectCreator.EUR, 1m, "ap credite note line 1", 100m);
				ObjectCreator.CreateCharge(line1);
				var line2 = ObjectCreator.CreateAPCreditNoteLine(apCreditNote, job, ObjectCreator.CC4, ObjectCreator.EUR, 1m, "ap credite note line 2", 200m);
				ObjectCreator.CreateCharge(line2);
				var line3 = ObjectCreator.CreateAPCreditNoteLine(apCreditNote, job, ObjectCreator.CC1, ObjectCreator.EUR, 1m, "ap credite note line 3", 300m);
				ObjectCreator.CreateCharge(line3);
				var line4 = ObjectCreator.CreateAPCreditNoteLine(apCreditNote, job, ObjectCreator.CC2, ObjectCreator.EUR, 1m, "ap credite note line 4", 400m);
				ObjectCreator.CreateCharge(line4);
				apCreditNote.AH_ComplianceSubType = complianceSubType;
				Factory.Save();

				var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD04</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APCRD1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>300.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>700.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
				AssertXML(expectedXML, apCreditNote);
				ObjectCreator.ClearComplianceReportQueue();
			}
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_TipoDocumento_PositiveADJ()
		{
			AssertCedentePrestatoreDTRXml(ItalyComplianceInfo.ComplianceSubTypeCodes.APS);

			void AssertCedentePrestatoreDTRXml(string complianceSubType)
			{
				var creditor = CreateCreditorOrganisation(true, true, true);
				var positiveAdjNote = CreateAPAdjustmentNoteCore(creditor, isPositive: true, complianceSubType);
				Factory.Save();

				var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD01</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APADJ1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>300.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
      <Natura>N2</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>700.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>70.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>TG</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
				AssertXML(expectedXML, positiveAdjNote);
				ObjectCreator.ClearComplianceReportQueue();
			}
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_TipoDocumento_NegativeADJ()
		{
			AssertCedentePrestatoreDTRXml(ItalyComplianceInfo.ComplianceSubTypeCodes.APS);
			AssertCedentePrestatoreDTRXml(ItalyComplianceInfo.ComplianceSubTypeCodes.INT);

			void AssertCedentePrestatoreDTRXml(string complianceSubType)
			{
				var creditor = CreateCreditorOrganisation(true, true, true);
				var negativeAdjNote = CreateAPAdjustmentNoteCore(creditor, isPositive: false, complianceSubType);
				Factory.Save();

				var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD04</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APADJ1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>300.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
      <Natura>N2</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>700.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>70.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>TG</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
				AssertXML(expectedXML, negativeAdjNote);
				ObjectCreator.ClearComplianceReportQueue();
			}
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_TipoDocumento_GoodsAndServicesAmounts_INV()
		{
			AssertCedentePrestatoreDTRXml(ItalyComplianceInfo.ComplianceSubTypeCodes.INT, ServiceAmountAndGoodsAmountOption.ServiceAmountLessThanGoodsAmount, "TD10", "-600.00", "-500.00", "-50.00");
			AssertCedentePrestatoreDTRXml(ItalyComplianceInfo.ComplianceSubTypeCodes.INT, ServiceAmountAndGoodsAmountOption.ServiceAmountEqualToGoodsAmount, "TD10", "-300.00", "-700.00", "-70.00");
			AssertCedentePrestatoreDTRXml(ItalyComplianceInfo.ComplianceSubTypeCodes.INT, ServiceAmountAndGoodsAmountOption.ServiceAmountGreaterThanGoodsAmount, "TD11", "-300.00", "-800.00", "-80.00");

			void AssertCedentePrestatoreDTRXml(string complianceSubType, ServiceAmountAndGoodsAmountOption serviceAmountAndGoodsAmountOption, string expectedTipoDocumento, string expectedTaxFreeAmount, string expectedTaxedAmount, string expectedTaxAmount)
			{
				var creditor = CreateCreditorOrganisation(true, true, true);
				var invoice = CreateAPInvoice(creditor, serviceAmountAndGoodsAmountOption, complianceSubType);
				Factory.Save();

				var expectedXML = $@"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>{expectedTipoDocumento}</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APINV1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>{expectedTaxFreeAmount}</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
      <Natura>N2</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>{expectedTaxedAmount}</ImponibileImporto>
      <DatiIVA>
        <Imposta>{expectedTaxAmount}</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>TG</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
				AssertXML(expectedXML, invoice);
				ObjectCreator.ClearComplianceReportQueue();
			}
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_TipoDocumento_GoodsAndServicesAmounts_ADJ()
		{
			AssertCedentePrestatoreDTRXml(ItalyComplianceInfo.ComplianceSubTypeCodes.INT, ServiceAmountAndGoodsAmountOption.ServiceAmountLessThanGoodsAmount, "TD10", "-300.00", "800.00", "80.00");
			AssertCedentePrestatoreDTRXml(ItalyComplianceInfo.ComplianceSubTypeCodes.INT, ServiceAmountAndGoodsAmountOption.ServiceAmountEqualToGoodsAmount, "TD10", "-300.00", "700.00", "70.00");
			AssertCedentePrestatoreDTRXml(ItalyComplianceInfo.ComplianceSubTypeCodes.INT, ServiceAmountAndGoodsAmountOption.ServiceAmountGreaterThanGoodsAmount, "TD11", "-300.00", "600.00", "60.00");

			void AssertCedentePrestatoreDTRXml(string complianceSubType, ServiceAmountAndGoodsAmountOption serviceAmountAndGoodsAmountOption, string expectedTipoDocumento, string expectedTaxFreeAmount, string expectedTaxedAmount, string expectedTaxAmount)
			{
				var creditor = CreateCreditorOrganisation(true, true, true);
				var invoice = CreateAPAdjustmentNoteForGoodServiceAmountTests(creditor, serviceAmountAndGoodsAmountOption, complianceSubType);
				Factory.Save();

				var expectedXML = $@"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>{expectedTipoDocumento}</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APADJ1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>{expectedTaxFreeAmount}</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
      <Natura>N2</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>{expectedTaxedAmount}</ImponibileImporto>
      <DatiIVA>
        <Imposta>{expectedTaxAmount}</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>TG</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
				AssertXML(expectedXML, invoice);
				ObjectCreator.ClearComplianceReportQueue();
			}
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_TransactionNumberLongerThan20Characters()
		{
			var creditor = CreateCreditorOrganisation(true, true, true);
			var invoice = CreateAPInvoice(creditor, transactionNumberLongerThan20Characters: true);
			Factory.Save();

			var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD01</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APABCDEFGHIJKLMNOPQR</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>300.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>0.00</Imposta>
        <Aliquota>0.00</Aliquota>
      </DatiIVA>
      <Natura>N2</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>700.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>70.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>TG</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";
			AssertXML(expectedXML, invoice);
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_Imposta_UsesCorrectAmountWhenReverseCharge()
		{
			var firstGroupPK = ZGuid.NewZGuid();
			var secondGroupPK = ZGuid.NewZGuid();
			var sql = $"insert into dbo.AccInvMsg (A9_PK, A9_TaxGroupCode, A9_SystemCreateTimeUtc, A9_SystemCreateUser, A9_SystemLastEditTimeUtc, A9_SystemLastEditUser) values ('{firstGroupPK.ToString()}', '111', GetUtcDate(), '~BP', GetUtcDate(), '~BP'), ('{secondGroupPK.ToString()}', '222', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);

			var orderedTaxRate1 = Factory.NewWithPrimaryKey<AccTaxRate>(new Guid("ae43f819-371b-4f7f-8281-f2edb661a896"));
			orderedTaxRate1.AT_Code = "GSTREVO";
			orderedTaxRate1.AT_Type = "RVS";
			orderedTaxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			orderedTaxRate1.SetRate_ForTestOnly(10, 1);
			orderedTaxRate1.SetExtraRate_ForTestOnly(0, 1);
			var orderedTaxRate2 = Factory.NewWithPrimaryKey<AccTaxRate>(new Guid("c58eb824-c606-43b1-9a10-b842a1af38d7"));
			orderedTaxRate2.AT_Code = "GSTRATO";
			orderedTaxRate2.AT_Type = AccTaxRate.Types.Rated;
			orderedTaxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			orderedTaxRate2.SetRate_ForTestOnly(20, 1);
			orderedTaxRate2.SetExtraRate_ForTestOnly(0, 1);
			Factory.Save();

			var creditor = CreateCreditorOrganisation(true, true, true);
			var shipment = ObjectCreator.CreateShipment("S10002");
			var job = ObjectCreator.CreateJob(shipment);
			var invoice = ObjectCreator.CreateInvoice(typeof(APInvoice), "APINV1", ObjectCreator.EUR, 1m, creditor);

			var line1 = ObjectCreator.CreateInvoiceLine(invoice, job, ObjectCreator.CC1, 100m, ObjectCreator.EUR, 1m);
			line1.AL_AT = orderedTaxRate1.PK;
			line1.AL_A9_VATClass = firstGroupPK;
			ObjectCreator.CreateCharge(line1);
			var line2 = ObjectCreator.CreateInvoiceLine(invoice, job, ObjectCreator.CC1, 200m, ObjectCreator.EUR, 1m);
			line2.AL_AT = orderedTaxRate2.PK;
			line2.AL_A9_VATClass = secondGroupPK;
			ObjectCreator.CreateCharge(line2);
			Factory.Save();

			var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento></TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APINV1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>-200.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>-40.00</Imposta>
        <Aliquota>20.00</Aliquota>
      </DatiIVA>
      <Natura>22</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>-100.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>-10.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>11</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				ObjectCreator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.Esterometro, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TaxReporting);
				var report = CreateEsterometroReport(invoice);
				Factory.Save();
				var collector = ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(report, ComplianceReportDataCollectionMode.Esterometro);
				var reportLines = report.ReportLines.Cast<AccComplianceReportLine>().OrderBy(x => x.ACL_ReportSequence).ToList();
				var reportLine1 = reportLines.First(x => x.TotalExTaxAmount == -100);
				var reportLine2 = reportLines.First(x => x.TotalExTaxAmount == -200);

				AssertNotEquals("Values cannot be same for checking XML", reportLine1.TotalTaxAmount, reportLine1.TaxReverseChargeAmount);
				AssertNotEquals("Values cannot be same for checking XML", reportLine2.TotalTaxAmount, reportLine2.TaxReverseChargeAmount);
				this.AssertXMLEqualsByDiff("Line with RVS rate should use TaxReverseChargeAmount for Imposta", expectedXML, new CedentePrestatoreDTR(reportLines).BuildXml(report, null, collector).ToString());
			}
		}

		[TestDate(2020, 01, 15)]
		public void TestCedentePrestatoreDTR_NaturaWithNewSchema()
		{
			var firstGroupPK = ZGuid.NewZGuid();
			var secondGroupPK = ZGuid.NewZGuid();
			var sql = $"insert into dbo.AccInvMsg (A9_PK, A9_TaxGroupCode, A9_SystemCreateTimeUtc, A9_SystemCreateUser, A9_SystemLastEditTimeUtc, A9_SystemLastEditUser) values ('{firstGroupPK}', 'N31', GetUtcDate(), '~BP', GetUtcDate(), '~BP'), ('{secondGroupPK}', 'N22', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);
			var orderedTaxRate1 = Factory.NewWithPrimaryKey<AccTaxRate>(new Guid("ae43f819-371b-4f7f-8281-f2edb661a896"));
			orderedTaxRate1.AT_Code = "GSTREVO";
			orderedTaxRate1.AT_Type = "RVS";
			orderedTaxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			orderedTaxRate1.SetRate_ForTestOnly(10, 1);
			orderedTaxRate1.SetExtraRate_ForTestOnly(0, 1);
			var orderedTaxRate2 = Factory.NewWithPrimaryKey<AccTaxRate>(new Guid("c58eb824-c606-43b1-9a10-b842a1af38d7"));
			orderedTaxRate2.AT_Code = "GSTRATO";
			orderedTaxRate2.AT_Type = AccTaxRate.Types.Rated;
			orderedTaxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			orderedTaxRate2.SetRate_ForTestOnly(20, 1);
			orderedTaxRate2.SetExtraRate_ForTestOnly(0, 1);
			Factory.Save();

			var creditor = CreateCreditorOrganisation(true, true, true);
			var shipment = ObjectCreator.CreateShipment("S10022");
			var job = ObjectCreator.CreateJob(shipment);
			var invoice = ObjectCreator.CreateInvoice(typeof(APInvoice), "APINV11", ObjectCreator.EUR, 1m, creditor);

			var line1 = ObjectCreator.CreateInvoiceLine(invoice, job, ObjectCreator.CC1, 100m, ObjectCreator.EUR, 1m);
			line1.AL_AT = orderedTaxRate1.PK;
			line1.AL_A9_VATClass = firstGroupPK;
			ObjectCreator.CreateCharge(line1);
			var line2 = ObjectCreator.CreateInvoiceLine(invoice, job, ObjectCreator.CC1, 200m, ObjectCreator.EUR, 1m);
			line2.AL_AT = orderedTaxRate2.PK;
			line2.AL_A9_VATClass = secondGroupPK;
			ObjectCreator.CreateCharge(line2);
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2020, 02, 01)))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AssertCedentePrestatoreDTRXml("N2", "N3");
			}

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2020, 01, 01)))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AssertCedentePrestatoreDTRXml("N2.2", "N3.1");
			}

			void AssertCedentePrestatoreDTRXml(string expectedNatura1, string expectedNatura2)
			{
				var expectedXML = string.Format(@"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento></TipoDocumento>
      <Data>2020-01-15</Data>
      <Numero>APINV11</Numero>
      <DataRegistrazione>2020-01-15</DataRegistrazione>
    </DatiGenerali>
    <DatiRiepilogo>
      <ImponibileImporto>-200.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>-40.00</Imposta>
        <Aliquota>20.00</Aliquota>
      </DatiIVA>
      <Natura>{0}</Natura>
    </DatiRiepilogo>
    <DatiRiepilogo>
      <ImponibileImporto>-100.00</ImponibileImporto>
      <DatiIVA>
        <Imposta>-10.00</Imposta>
        <Aliquota>10.00</Aliquota>
      </DatiIVA>
      <Natura>{1}</Natura>
    </DatiRiepilogo>
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>"
, expectedNatura1
, expectedNatura2);

				AssertXML(expectedXML, invoice);
				ObjectCreator.ClearComplianceReportQueue();
			}
		}

		[TestDate(2019, 01, 01)]
		public void TestCedentePrestatoreDTR_CreditorDoesNotHaveTaxRegistrationNumber()
		{
			var creditor = CreateCreditorOrganisation(false, false, false, false);
			var invoice = CreateAPInvoice(creditor);
			Factory.Save();

			var datiRiepilogoFormat = @"    <DatiRiepilogo>
      <ImponibileImporto>{0}</ImponibileImporto>
      <DatiIVA>
        <Imposta>{1}</Imposta>
        <Aliquota>{2}</Aliquota>
      </DatiIVA>
    </DatiRiepilogo>";

			var expectedXML = @"<CedentePrestatoreDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>AU</IdPaese>
      <IdCodice>John Brown Pty Ltd</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>John Brown Pty Ltd</Denominazione>
    <Sede>
      <Indirizzo>14 Doody Street</Indirizzo>
      <CAP>02015</CAP>
      <Comune>Alexandria</Comune>
      <Nazione>AU</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
  <DatiFatturaBodyDTR>
    <DatiGenerali>
      <TipoDocumento>TD01</TipoDocumento>
      <Data>2019-01-01</Data>
      <Numero>APINV1</Numero>
      <DataRegistrazione>2019-01-01</DataRegistrazione>
    </DatiGenerali>
{0}
{1}
  </DatiFatturaBodyDTR>
</CedentePrestatoreDTR>";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				ObjectCreator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.Esterometro, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TaxReporting);
				var report = CreateEsterometroReport(invoice);
				var reportLines = report.ReportLines.Cast<AccComplianceReportLine>().OrderBy(x => x.ACL_ReportSequence).ToList();
				var riepologo1 = String.Format(CultureInfo.InvariantCulture, datiRiepilogoFormat, Math.Abs(reportLines[0].TotalExTaxAmount.Round(2)), Math.Abs(reportLines[0].TotalTaxAmount.Round(2)), reportLines[0].AL_TaxRate.Round(2));
				var riepologo2 = String.Format(CultureInfo.InvariantCulture, datiRiepilogoFormat, Math.Abs(reportLines[1].TotalExTaxAmount.Round(2)), Math.Abs(reportLines[1].TotalTaxAmount.Round(2)), reportLines[1].AL_TaxRate.Round(2));

				expectedXML = String.Format(expectedXML, riepologo1, riepologo2);
				this.AssertXMLEqualsByDiff(expectedXML, new CedentePrestatoreDTR(reportLines).BuildXml(report, null, ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(report, ComplianceReportDataCollectionMode.Esterometro)).ToString());
			}
		}

		#region Helpers

		void AssertXML(string expectedXML, InvoicingBase transactionForTest)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var report = CreateEsterometroReport(transactionForTest);
				var reportLines = report.ReportLines.Cast<AccComplianceReportLine>().OrderBy(x => x.ACL_ReportSequence).ToList();
				this.AssertXMLEqualsByDiff(expectedXML, new CedentePrestatoreDTR(reportLines).BuildXml(report, null, ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(report, ComplianceReportDataCollectionMode.Esterometro)).ToString());
			}
		}

		AccComplianceReport CreateEsterometroReport(InvoicingBase transaction)
		{
			ObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);

			var report = ObjectCreator.CreateComplianceReport(AccComplianceReport.ReportTypes.Esterometro);
			Factory.Save();

			ObjectCreator.CreateComplianceReportQueueEntry(report, transaction.Lines.Cast<AccTransactionLines>().ToArray());
			report.GenerateFromQueue();
			report.ClearReportLines_ForTestOnly();
			Factory.Save();

			AssertEquals("ReportLines.Count", 2, report.ReportLines.Count);
			AssertEquals(transaction.PK, report.ReportLines[0].AH_PK);
			AssertEquals(transaction.PK, report.ReportLines[1].AH_PK);
			AssertNotEquals("Should be grouped by tax id", report.ReportLines[0].AT_Code, report.ReportLines[1].AT_Code);
			return report;
		}

		InvoicingBase CreateAPAdjustmentNoteCore(OrgHeader creditor, bool isPositive, ZString subType)
		{
			var taxGroupPK = ZGuid.NewZGuid();
			var sql = $"INSERT INTO {AccInvMsgSchema.Constants.SqlSchemaName}.{AccInvMsgSchema.Constants.TableName} ({AccInvMsg.Schema.PK}, {AccInvMsg.Schema.A9_TaxGroupCode}, {AccInvMsg.Schema.A9_SystemCreateTimeUtc}, {AccInvMsg.Schema.A9_SystemCreateUser}, {AccInvMsg.Schema.A9_SystemLastEditTimeUtc}, {AccInvMsg.Schema.A9_SystemLastEditUser}) VALUES ('{taxGroupPK.ToString()}', 'TGP', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);

			var multiplier = isPositive ? 1m : -1m;
			var apAdjNote = ObjectCreator.CreateAdjustmentNote<APAdjustmentNote>("APADJ1", multiplier * 400m, 0m, ZDateTime.Today, creditor.PK);
			ObjectCreator.CreateAdjusmentNoteLine(apAdjNote, ObjectCreator.OverheadChargeCode.PK, multiplier * -100m, 0m);
			ObjectCreator.CreateAdjusmentNoteLine(apAdjNote, ObjectCreator.NonAccrualChargeCode.PK, multiplier * -200m, 0m);
			var lineWithTax1 = ObjectCreator.CreateAdjusmentNoteLine(apAdjNote, ObjectCreator.OverheadChargeCode.PK, multiplier * 300m, 0m);
			lineWithTax1.AL_AT = ObjectCreator.GST1.PK;
			lineWithTax1.AL_A9_VATClass = taxGroupPK;
			var lineWithTax2 = ObjectCreator.CreateAdjusmentNoteLine(apAdjNote, ObjectCreator.NonAccrualChargeCode.PK, multiplier * 400m, 0m);
			lineWithTax2.AL_AT = ObjectCreator.GST1.PK;
			lineWithTax2.AL_A9_VATClass = taxGroupPK;
			apAdjNote.AH_ComplianceSubType = subType;
			Factory.Save();
			return apAdjNote;
		}

		InvoicingBase CreateAPAdjustmentNoteForGoodServiceAmountTests(OrgHeader creditor, ServiceAmountAndGoodsAmountOption serviceAmountAndGoodsAmountOption, ZString subType)
		{
			var amountTable = new[]
			{
				// Line  1      2      3     4
				//      NOTAX  NOTAX   TAX   TAX
				//       SRV    GDS    GDS   SRV
				new[] { -100m, -200m, 300m, 300m },		// ServiceAmountAndGoodsAmountOption.ServiceAmountGreaterThanGoodsAmount
				new[] { -100m, -200m, 300m, 500m },		// ServiceAmountAndGoodsAmountOption.ServiceAmountLessThanGoodsAmount
				new[] { -100m, -200m, 300m, 400m },		// ServiceAmountAndGoodsAmountOption.ServiceAmountEqualToGoodsAmount
			};
			var amountIdx = (int)serviceAmountAndGoodsAmountOption;

			var taxGroupPK = ZGuid.NewZGuid();
			var sql = $"INSERT INTO {AccInvMsgSchema.Constants.SqlSchemaName}.{AccInvMsgSchema.Constants.TableName} ({AccInvMsg.Schema.PK}, {AccInvMsg.Schema.A9_TaxGroupCode}, {AccInvMsg.Schema.A9_SystemCreateTimeUtc}, {AccInvMsg.Schema.A9_SystemCreateUser}, {AccInvMsg.Schema.A9_SystemLastEditTimeUtc}, {AccInvMsg.Schema.A9_SystemLastEditUser}) VALUES ('{taxGroupPK.ToString()}', 'TGP', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);

			ObjectCreator.OverheadChargeCode.AC_GoodsServiceType = GoodServiceTypes.Codes.SRV;
			ObjectCreator.NonAccrualChargeCode.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;

			var shipment = ObjectCreator.CreateShipment($"S{shipmentNumber}");
			var job = ObjectCreator.CreateJob(shipment);
			var apAdjNote = ObjectCreator.CreateAdjustmentNote<APAdjustmentNote>("APADJ1", amountTable[amountIdx].Sum(), 0m, ZDateTime.Today, creditor.PK);
			ObjectCreator.CreateAdjusmentNoteLine(apAdjNote, ObjectCreator.OverheadChargeCode.PK, amountTable[amountIdx][0], 0m);
			ObjectCreator.CreateAdjusmentNoteLine(apAdjNote, ObjectCreator.NonAccrualChargeCode.PK, amountTable[amountIdx][1], 0m);
			var lineWithTax1 = ObjectCreator.CreateAdjusmentNoteLine(apAdjNote, ObjectCreator.OverheadChargeCode.PK, amountTable[amountIdx][2], 0m);
			lineWithTax1.AL_AT = ObjectCreator.GST1.PK;
			lineWithTax1.AL_A9_VATClass = taxGroupPK;
			var lineWithTax2 = ObjectCreator.CreateAdjusmentNoteLine(apAdjNote, ObjectCreator.NonAccrualChargeCode.PK, amountTable[amountIdx][3], 0m);
			lineWithTax2.AL_AT = ObjectCreator.GST1.PK;
			lineWithTax2.AL_A9_VATClass = taxGroupPK;
			apAdjNote.AH_ComplianceSubType = subType;

			Factory.Save();

			shipmentNumber = shipmentNumber + 1;
			return apAdjNote;
		}

		InvoicingBase CreateAPInvoice(OrgHeader creditor, ServiceAmountAndGoodsAmountOption serviceAmountAndGoodsAmountOption = ServiceAmountAndGoodsAmountOption.ServiceAmountEqualToGoodsAmount, string complianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.APS, bool transactionNumberLongerThan20Characters = false)
		{
			var transactionNum = transactionNumberLongerThan20Characters ? "APABCDEFGHIJKLMNOPQRSTUVWXYZ1" : "APINV1";
			var isCreditorInItaly = creditor.MainAddress.OA_RN_NKCountryCode == Core.Constants.CountryCodes.Italy;
			var amountTable = new[]
			{
				// Line  1     2     3     4
				//      NOTAX NOTAX  TAX   TAX
				//       SRV   GDS   GDS   SRV
				new[] { 100m, 200m, 300m, 500m },		// ServiceAmountAndGoodsAmountOption.ServiceAmountGreaterThanGoodsAmount
				new[] { 100m, 500m, 200m, 300m },		// ServiceAmountAndGoodsAmountOption.ServiceAmountLessThanGoodsAmount
				new[] { 100m, 200m, 300m, 400m },		// ServiceAmountAndGoodsAmountOption.ServiceAmountEqualToGoodsAmount
			};
			var amountIdx = (int)serviceAmountAndGoodsAmountOption;

			var taxGroupPK = ZGuid.NewZGuid();
			var sql = $"INSERT INTO {AccInvMsgSchema.Constants.SqlSchemaName}.{AccInvMsgSchema.Constants.TableName} ({AccInvMsg.Schema.PK}, {AccInvMsg.Schema.A9_TaxGroupCode}, {AccInvMsg.Schema.A9_SystemCreateTimeUtc}, {AccInvMsg.Schema.A9_SystemCreateUser}, {AccInvMsg.Schema.A9_SystemLastEditTimeUtc}, {AccInvMsg.Schema.A9_SystemLastEditUser}) VALUES ('{taxGroupPK.ToString()}', 'TGP', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);

			var shipment = ObjectCreator.CreateShipment($"S{shipmentNumber}");
			var job = ObjectCreator.CreateJob(shipment);
			var apInvoice = ObjectCreator.CreateInvoice(typeof(APInvoice), transactionNum, ObjectCreator.EUR, 1m, creditor);
			apInvoice.AH_ComplianceSubType = complianceSubType;
			var line1 = ObjectCreator.CreateInvoiceLine(apInvoice, job, isCreditorInItaly ? ObjectCreator.CC2 : ObjectCreator.CC4, amountTable[amountIdx][0] * -1m, ObjectCreator.EUR, 1m);
			ObjectCreator.CreateCharge(line1);
			var line2 = ObjectCreator.CreateInvoiceLine(apInvoice, job, isCreditorInItaly ? ObjectCreator.CC1 : ObjectCreator.CC3, amountTable[amountIdx][1] * -1m, ObjectCreator.EUR, 1m);
			ObjectCreator.CreateCharge(line2);
			var line3 = ObjectCreator.CreateInvoiceLine(apInvoice, job, ObjectCreator.CC1, amountTable[amountIdx][2] * -1m, ObjectCreator.EUR, 1m);
			line3.AL_AT = ObjectCreator.GST1.PK;
			line3.AL_A9_VATClass = isCreditorInItaly ? taxGroupPK : ZGuid.Empty;
			ObjectCreator.CreateCharge(line3);
			var line4 = ObjectCreator.CreateInvoiceLine(apInvoice, job, ObjectCreator.CC2, amountTable[amountIdx][3] * -1m, ObjectCreator.EUR, 1m);
			line4.AL_AT = ObjectCreator.GST1.PK;
			line4.AL_A9_VATClass = isCreditorInItaly ? taxGroupPK : ZGuid.Empty;
			ObjectCreator.CreateCharge(line4);

			ObjectCreator.CC2.AC_GoodsServiceType = ObjectCreator.CC4.AC_GoodsServiceType = GoodServiceTypes.Codes.SRV;
			ObjectCreator.CC1.AC_GoodsServiceType = ObjectCreator.CC3.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;

			Factory.Save();

			shipmentNumber = shipmentNumber + 1;
			return apInvoice;
		}
		int shipmentNumber = 10002;

		enum ServiceAmountAndGoodsAmountOption
		{
			ServiceAmountGreaterThanGoodsAmount = 0,
			ServiceAmountLessThanGoodsAmount = 1,
			ServiceAmountEqualToGoodsAmount = 2
		}

		OrgHeader CreateCreditorOrganisation(bool hasPrimaryBizRegNumber, bool hasGBR, bool hasGCR, bool isCreditorInItaly = true)
		{
			var creditor = ObjectCreator.CreateOrgHeader("FRATEL", false, false);
			creditor.OH_FullName = isCreditorInItaly ? "Italy SRL a socio unico" : "John Brown Pty Ltd";
			var creditorNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.Equal, isCreditorInItaly ? Core.Constants.CountryCodes.Italy : Core.Constants.CountryCodes.Australia));
			creditor.OH_RL_NKClosestPort = creditorNLOCO.RL_Code;
			if (hasPrimaryBizRegNumber)
			{
				creditor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "98645152", Core.Constants.CountryCodes.Italy);
				creditor.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "5682356", Core.Constants.CountryCodes.Australia);
			}
			if (hasGBR)
			{
				creditor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GovBusinessCode, "45689789", Core.Constants.CountryCodes.Italy);
			}
			if (hasGCR)
			{
				creditor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CorporationCode, "11112222", Core.Constants.CountryCodes.Italy);
			}

			creditor.MainAddress.OA_Address1 = isCreditorInItaly ? "Via Cassanese" : "14";
			creditor.MainAddress.OA_Address2 = isCreditorInItaly ? "224 Palazzo Caravaggio" : "Doody Street";
			creditor.MainAddress.OA_City = isCreditorInItaly ? "Milan" : "Alexandria";
			creditor.MainAddress.OA_State = isCreditorInItaly ? "MI" : "NSW";
			creditor.MainAddress.OA_PostCode = isCreditorInItaly ? "20090" : "2015";
			creditor.MainAddress.OA_RN_NKCountryCode = isCreditorInItaly ? Core.Constants.CountryCodes.Italy : Core.Constants.CountryCodes.Australia;
			return creditor;
		}

		void SetUpStaticPKTaxRates()
		{
			var orderedTaxRate1 = Factory.NewWithPrimaryKey<AccTaxRate>(new Guid("ba91dcea-d79f-4869-8d51-fc52cad4d175"));
			orderedTaxRate1.AT_Code = "GSTO";
			orderedTaxRate1.AT_Type = AccTaxRate.Types.Rated;
			orderedTaxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			orderedTaxRate1.SetRate_ForTestOnly(10, 1);
			orderedTaxRate1.SetExtraRate_ForTestOnly(0, 1);
			var orderedTaxRate2 = Factory.NewWithPrimaryKey<AccTaxRate>(new Guid("533d7327-3ad6-4ec8-9acb-979d5d633106"));
			orderedTaxRate2.AT_Code = "GSTFreeO";
			orderedTaxRate2.AT_Type = AccTaxRate.Types.Rated;
			orderedTaxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			orderedTaxRate2.SetRate_ForTestOnly(0, 1);
			orderedTaxRate2.SetExtraRate_ForTestOnly(0, 1);
			Factory.Save();

			ObjectCreator.CC1.AC_AT_GSTRate = ObjectCreator.CC2.AC_AT_GSTRate = orderedTaxRate1.PK;
			ObjectCreator.CC3.AC_AT_GSTRate = ObjectCreator.CC4.AC_AT_GSTRate = orderedTaxRate2.PK;
		}

		#endregion
	}
}
