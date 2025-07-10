using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Accounting.Business.EInvoicing.Testing.FatturaElettronicaXmlValueFormatterTest;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using TaxGroupCodeType = Enterprise.UniversalDataBuss.DataObjects.Universal.TaxGroupCodeType;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	public class DatiBeniServiziTest : TestCaseWithFactory
	{
		public void TestDatiBeniServiziForSignsOnAmountsAR()
		{
			var multiplier = -1;
			var datiBeniServizi = new DatiBeniServizi();

			for (var i = 0; i < 4; i++)
			{
				multiplier *= -1;
				var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());
				transactionInfo.Ledger = LedgerTypes.AccountsReceivable;
				transactionInfo.TransactionType = i < 2 ? TransactionType.CRD : TransactionType.ADJ;

				var taxRate1 = new TaxID();
				taxRate1.TaxCode = "GS1";
				taxRate1.TaxRate = 30m;

				var taxMessage1 = new TaxMessageID();
				taxMessage1.TaxMessageCode = "TM1";
				taxMessage1.Description = "TM1 Desc";

				var line1 = CreatePostingJournal();
				line1.Sequence = 1;
				line1.ChargeCode.Code = "CC1";
				line1.ChargeCode.Description = "Test CC1";
				line1.LocalAmount = multiplier * 120m;
				line1.LocalTotalAmount = multiplier * 156m;
				line1.OSAmount = multiplier * 120m;
				line1.OSGSTVATAmount = multiplier * 36m;
				line1.LocalGSTVATAmount = multiplier * 18m;
				line1.VATTaxID = taxRate1;
				line1.TaxMessageID = taxMessage1;
				line1.GLAccount = new GLAccount();
				line1.Description = $"Test Description for Line {line1.Sequence}";

				var line2 = CreatePostingJournal();
				line2.Sequence = 2;
				line2.ChargeCode.Code = "CC1";
				line2.ChargeCode.Description = "Test CC1";
				line2.LocalAmount = multiplier * -20m;
				line2.LocalTotalAmount = multiplier * -56m;
				line2.OSAmount = multiplier * -20m;
				line2.OSGSTVATAmount = multiplier * -16m;
				line2.LocalGSTVATAmount = multiplier * -8m;
				line2.VATTaxID = taxRate1;
				line2.TaxMessageID = taxMessage1;
				line2.GLAccount = new GLAccount();
				line2.Description = $"Test Description for Line {line2.Sequence}";

				transactionInfo.OSTotal = multiplier * 100m;
				transactionInfo.PostingJournalCollection.Add(line1);
				transactionInfo.PostingJournalCollection.Add(line2);

				var expectedSignMultiplier = (transactionInfo.TransactionType == TransactionType.CRD || transactionInfo.OSTotal < 0) ? -1 : 1;
				var expectedXml = $@"<DatiBeniServizi>
  <DettaglioLinee>
    <NumeroLinea>1</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC1</CodiceTipo>
      <CodiceValore />
    </CodiceArticolo>
    <Descrizione>Test Description for Line 1</Descrizione>
    <PrezzoUnitario>{FormatWithZerosAfterDecimalPoint(expectedSignMultiplier * line1.LocalAmount.Value)}</PrezzoUnitario>
    <PrezzoTotale>{FormatWithZerosAfterDecimalPoint(expectedSignMultiplier * line1.LocalAmount.Value)}</PrezzoTotale>
    <AliquotaIVA>30.00</AliquotaIVA>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>2</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC1</CodiceTipo>
      <CodiceValore />
    </CodiceArticolo>
    <Descrizione>Test Description for Line 2</Descrizione>
    <PrezzoUnitario>{FormatWithZerosAfterDecimalPoint(expectedSignMultiplier * line2.LocalAmount.Value)}</PrezzoUnitario>
    <PrezzoTotale>{FormatWithZerosAfterDecimalPoint(expectedSignMultiplier * line2.LocalAmount.Value)}</PrezzoTotale>
    <AliquotaIVA>30.00</AliquotaIVA>
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>30.00</AliquotaIVA>
    <ImponibileImporto>{FormatWithZerosAfterDecimalPoint(expectedSignMultiplier * (line1.LocalAmount.Value + line2.LocalAmount.Value))}</ImponibileImporto>
    <Imposta>{FormatWithZerosAfterDecimalPoint(expectedSignMultiplier * (line1.LocalGSTVATAmount.Value + line2.LocalGSTVATAmount.Value))}</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
</DatiBeniServizi>";

				var actualXml = datiBeniServizi.BuildXML(transactionInfo).ToString();
				AssertEquals(expectedXml, actualXml);
			}
		}

		public void TestDatiBeniServiziForSignsOnAmountsAP()
		{
			var datiBeniServizi = new DatiBeniServizi();

			var expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlFullAP, NaturaElementNewSchema, RiferimentoElementTm3, RiferimentoElementTm3, string.Empty);

			transactionInfoAP_INV.TransactionType = TransactionType.INV;
			var actualXml = datiBeniServizi.BuildXML(transactionInfoAP_INV).ToString();
			AssertEquals(expectedXml, actualXml);

			transactionInfoAP_INV.TransactionType = TransactionType.ADJ;
			actualXml = datiBeniServizi.BuildXML(transactionInfoAP_INV).ToString();
			AssertEquals(expectedXml, actualXml);

			expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlFullAP, NaturaElementNewSchema, RiferimentoElementTm3, RiferimentoElementTm3, NegativeSign);

			transactionInfoAP_CRD.TransactionType = TransactionType.ADJ;
			actualXml = datiBeniServizi.BuildXML(transactionInfoAP_CRD).ToString();
			AssertEquals(expectedXml, actualXml);

			transactionInfoAP_CRD.TransactionType = TransactionType.CRD;
			actualXml = datiBeniServizi.BuildXML(transactionInfoAP_CRD).ToString();
			AssertEquals(expectedXml, actualXml);
		}

		[TestDate(2020, 10, 15)]
		public void TestDatiBeniServizi()
		{
			BodyTestDatiBeniServiziAR(NaturaElement);
			BodyTestDatiBeniServiziAP(NaturaElement);
		}

		[TestDate(2020, 10, 15)]
		public void TestDatiBeniServiziNewSchema()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2020, 10, 10)))
			{
				BodyTestDatiBeniServiziAR(NaturaElementNewSchema);
				BodyTestDatiBeniServiziAP(NaturaElementNewSchema);
			}
		}

		void BodyTestDatiBeniServiziAR(string pNaturaElement)
		{
			var expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlFullAR, pNaturaElement, RiferimentoElementTm2, RiferimentoElementTm2);
			var actualXml = new DatiBeniServizi().BuildXML(transactionInfoAR).ToString();
			AssertEquals(expectedXml, actualXml);
		}

		void BodyTestDatiBeniServiziAP(string pNaturaElement)
		{
			var expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlFullAP, pNaturaElement, RiferimentoElementTm3, RiferimentoElementTm3, string.Empty);
			transactionInfoAP_INV.TransactionType = TransactionType.INV;
			var actualXml = new DatiBeniServizi().BuildXML(transactionInfoAP_INV).ToString();
			AssertEquals(expectedXml, actualXml);
		}

		[TestDate(2020, 10, 15)]
		public void TestCommentChargeCodeLinesInTransaction()
		{
			BodyTestCommentChargeCodeLinesInTransactionAR(NaturaElement);
			BodyTestCommentChargeCodeLinesInTransactionAP(NaturaElement);
		}

		[TestDate(2020, 10, 15)]
		public void TestCommentChargeCodeLinesInTransactionNewSchema()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2020, 10, 10)))
			{
				BodyTestCommentChargeCodeLinesInTransactionAR(NaturaElementNewSchema);
				BodyTestCommentChargeCodeLinesInTransactionAP(NaturaElementNewSchema);
			}
		}

		void BodyTestCommentChargeCodeLinesInTransactionAR(string pNaturaElement)
		{
			var expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlFullAR, pNaturaElement, RiferimentoElementTm2, RiferimentoElementTm2);

			// GLAccount is null on Comment charge code
			var line1 = CreatePostingJournal();
			line1.Sequence = 7;
			line1.ChargeCode.Code = "CUSDEF";
			line1.ChargeCode.Description = "Test CMT1";
			line1.Job.Key = "S00003";
			line1.VATTaxID = null;
			line1.TaxMessageID = null;
			line1.Description = $"Test Description for Line {line1.Sequence}";

			var line2 = CreatePostingJournal();
			line2.Sequence = 8;
			line2.ChargeCode.Code = "CUSDEF";
			line2.ChargeCode.Description = "Test CMT2";
			line2.Job.Key = "S00003";
			line2.VATTaxID = null;
			line2.TaxMessageID = null;
			line2.Description = $"Test Description for Line {line2.Sequence}";

			transactionInfoAR.PostingJournalCollection.Add(line1);
			transactionInfoAR.PostingJournalCollection.Add(line2);

			var actualXml = new DatiBeniServizi().BuildXML(transactionInfoAR).ToString();
			AssertEquals(expectedXml, actualXml);
		}

		void BodyTestCommentChargeCodeLinesInTransactionAP(string pNaturaElement)
		{
			var expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlFullAP, pNaturaElement, RiferimentoElementTm3, RiferimentoElementTm3, string.Empty);

			// GLAccount is null on Comment charge code
			var line1 = CreatePostingJournal();
			line1.Sequence = 7;
			line1.ChargeCode.Code = "CUSDEF";
			line1.ChargeCode.Description = "Test CMT1";
			line1.Job.Key = "S00003";
			line1.VATTaxID = null;
			line1.TaxMessageID = null;
			line1.Description = $"Test Description for Line {line1.Sequence}";

			var line2 = CreatePostingJournal();
			line2.Sequence = 8;
			line2.ChargeCode.Code = "CUSDEF";
			line2.ChargeCode.Description = "Test CMT2";
			line2.Job.Key = "S00003";
			line2.VATTaxID = null;
			line2.TaxMessageID = null;
			line2.Description = $"Test Description for Line {line2.Sequence}";

			transactionInfoAP_INV.PostingJournalCollection.Add(line1);
			transactionInfoAP_INV.PostingJournalCollection.Add(line2);
			transactionInfoAP_INV.TransactionType = TransactionType.INV;

			var actualXml = new DatiBeniServizi().BuildXML(transactionInfoAP_INV).ToString();
			AssertEquals(expectedXml, actualXml);
		}

		public void TestDatiBeniServizi_ImpostaAP()
		{
			var expectedEndXml = @"<DettaglioLinee>
    <NumeroLinea>11</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CCX</CodiceTipo>
      <CodiceValore>S00003</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 11</Descrizione>
    <PrezzoUnitario>41.97</PrezzoUnitario>
    <PrezzoTotale>41.97</PrezzoTotale>
    <AliquotaIVA>22.00</AliquotaIVA>
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>22.00</AliquotaIVA>
    <ImponibileImporto>461.67</ImponibileImporto>
    <Imposta>101.57</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>";

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2021, 01, 01)))
			{
				var transactionInfoAP_INV = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				transactionInfoAP_INV.Ledger = LedgerTypes.AccountsPayable;
				transactionInfoAP_INV.SetPostingJournalCollection(() => new List<PostingJournal>());
				transactionInfoAP_INV.TransactionType = TransactionType.INV;

				var taxRate = new TaxID();
				taxRate.TaxCode = "IVAREV";
				taxRate.TaxRate = 22m;
				taxRate.TaxType = new CodeDescriptionPair();
				taxRate.TaxType.Code = AccTaxRate.Types.ReverseRated;

				var taxMessage = new TaxMessageID();
				taxMessage.TaxMessageCode = "Test";
				taxMessage.Description = "Tets Desc";
				taxMessage.EnglishTaxMessage = "Test English MSG";

				for (int i = 1; i < 12; i++)
				{
					var line = CreatePostingJournal();
					line.Sequence = i;
					line.ChargeCode.Code = "CCX";
					line.ChargeCode.Description = $"Test CMT{i}";
					line.Job.Key = "S00003";
					line.LocalAmount = -41.97m;
					line.LocalTotalAmount = -51.20m;
					line.OSAmount = -41.97m;
					line.OSGSTVATAmount = -9.23m;
					line.LocalGSTVATAmount = -9.23m;
					line.VATTaxID = taxRate;
					line.TaxMessageID = taxMessage;
					line.GLAccount = new GLAccount();
					line.Description = $"Test Description for Line {i}";

					transactionInfoAP_INV.PostingJournalCollection.Add(line);
				}

				var actualXml = new DatiBeniServizi().BuildXML(transactionInfoAP_INV).ToString();

				AssertXMLContains(expectedEndXml, actualXml);
			}
		}

		public void TestDatiBeniServiziWithEmptyJobNumberAR()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRate1 = new TaxID();
			taxRate1.TaxCode = "GS1";
			taxRate1.TaxRate = 30m;

			var taxMessage1 = new TaxMessageID();
			taxMessage1.TaxMessageCode = "TM1";
			taxMessage1.Description = "TM1 Desc";

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.ChargeCode.Code = "CC1";
			line1.ChargeCode.Description = "Test CC1";
			line1.LocalAmount = 120m;
			line1.LocalTotalAmount = 156m;
			line1.OSAmount = 120m;
			line1.OSGSTVATAmount = 36m;
			line1.LocalGSTVATAmount = 18m;
			line1.VATTaxID = taxRate1;
			line1.TaxMessageID = taxMessage1;
			line1.GLAccount = new GLAccount();
			line1.Description = $"Test Description for Line {line1.Sequence}";

			transactionInfo.Number = "TRN00001";
			transactionInfo.PostingJournalCollection.Add(line1);

			var expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlWithoutJobNumberAR, "TRN00001");

			line1.Job.Key = null;

			var actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals("Job number is null.", expectedXml, actualXml);

			line1.Job.Key = "";

			actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals("Job number is empty.", expectedXml, actualXml);

			var longNumber = "0123456789012345678901234567890123456789";
			expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlWithoutJobNumberAR, "01234567890123456789012345678901234");

			line1.Job.Key = longNumber;

			actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals("Job number is too long.", expectedXml, actualXml);

			line1.Job.Key = null;
			transactionInfo.Number = longNumber;

			actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals("Transaction number is too long.", expectedXml, actualXml);
		}

		public void TestDatiBeniServiziWithEmptyJobNumberAP()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.Ledger = LedgerTypes.AccountsPayable;
			transactionInfo.TransactionType = TransactionType.INV;
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRate1 = new TaxID();
			taxRate1.TaxCode = "GS1";
			taxRate1.TaxRate = 30m;

			var taxMessage1 = new TaxMessageID();
			taxMessage1.TaxMessageCode = "TM1";
			taxMessage1.Description = "TM1 Desc";

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.ChargeCode.Code = "CC1";
			line1.ChargeCode.Description = "Test CC1";
			line1.LocalAmount = -120m;
			line1.LocalTotalAmount = -156m;
			line1.OSAmount = -120m;
			line1.OSGSTVATAmount = -36m;
			line1.LocalGSTVATAmount = -18m;
			line1.VATTaxID = taxRate1;
			line1.TaxMessageID = taxMessage1;
			line1.GLAccount = new GLAccount();
			line1.Description = $"Test Description for Line {line1.Sequence}";

			transactionInfo.Number = "TRN00001";
			transactionInfo.PostingJournalCollection.Add(line1);

			var expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlWithoutJobNumberAP, "TRN00001");

			line1.Job.Key = null;

			var actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals("Job number is null.", expectedXml, actualXml);

			line1.Job.Key = "";

			actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals("Job number is empty.", expectedXml, actualXml);

			var longNumber = "0123456789012345678901234567890123456789";
			expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlWithoutJobNumberAP, "01234567890123456789012345678901234");

			line1.Job.Key = longNumber;

			actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals("Job number is too long.", expectedXml, actualXml);

			line1.Job.Key = null;
			transactionInfo.Number = longNumber;

			actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals("Transaction number is too long.", expectedXml, actualXml);
		}

		public void TestDatiBeniServiziWithEmptyDataSource()
		{
			var emptyTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var expectedXml = @"<DatiBeniServizi />";
			var actualXml = new DatiBeniServizi().BuildXML(emptyTransaction).ToString();
			AssertEquals(expectedXml, actualXml);
		}

		[TestDate(2020, 10, 15)]
		public void TestDatiBeniServiziWithLongEnglishTaxMessage()
		{
			BodyTestDatiBeniServiziWithLongEnglishTaxMessageAR(NaturaElement);
			BodyTestDatiBeniServiziWithLongEnglishTaxMessageAP(NaturaElement);
		}

		[TestDate(2020, 10, 15)]
		public void TestTestDatiBeniServiziWithLongEnglishTaxMessageNewSchema()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2020, 10, 10)))
			{
				BodyTestDatiBeniServiziWithLongEnglishTaxMessageAR(NaturaElementNewSchema);
				BodyTestDatiBeniServiziWithLongEnglishTaxMessageAP(NaturaElementNewSchema);
			}
		}

		void BodyTestDatiBeniServiziWithLongEnglishTaxMessageAR(string pNaturaElement)
		{
			taxMessage2.EnglishTaxMessage = new string('x', 110);
			var expectedRiferimentoNormativo = new string('x', 100);
			var expectedRiferimentoAmministrazione = new string('x', 20);
			var expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlFullAR, pNaturaElement, expectedRiferimentoNormativo, expectedRiferimentoAmministrazione);
			var actualXml = new DatiBeniServizi().BuildXML(transactionInfoAR).ToString();
			AssertEquals(expectedXml, actualXml);
		}

		void BodyTestDatiBeniServiziWithLongEnglishTaxMessageAP(string pNaturaElement)
		{
			taxMessage3.EnglishTaxMessage = new string('x', 110);
			var expectedRiferimentoNormativo = new string('x', 100);
			var expectedRiferimentoAmministrazione = new string('x', 20);
			var expectedXml = string.Format(CultureInfo.InvariantCulture, expectedXmlFullAP, pNaturaElement, expectedRiferimentoNormativo, expectedRiferimentoAmministrazione, string.Empty);
			transactionInfoAP_INV.TransactionType = TransactionType.INV;
			var actualXml = new DatiBeniServizi().BuildXML(transactionInfoAP_INV).ToString();
			AssertEquals(expectedXml, actualXml);
		}

		public void TestDatiBeniServiziWithN0TaxGroupCodeAR()
		{
			taxMessage2.TaxGroupCode.Code = ItalyComplianceInfo.TaxMessageGroupCodes.N0;
			taxMessage2.TaxGroupCode.Description = ItalyComplianceInfo.TaxMessageGroupDescriptions.N0;
			taxMessage2.TaxGroupCode.GovernmentCode = ItalyComplianceInfo.TaxMessageGroupGovtCodes.N0;

			var actualXml = new DatiBeniServizi().BuildXML(transactionInfoAR).ToString();
			AssertEquals(string.Format(CultureInfo.InvariantCulture, expectedXmlWithN0TaxGroupCodeAR, string.Empty), actualXml);
		}

		public void TestDatiBeniServiziWithN0TaxGroupCodeAP()
		{
			taxMessage3.TaxGroupCode.Code = ItalyComplianceInfo.TaxMessageGroupCodes.N0;
			taxMessage3.TaxGroupCode.Description = ItalyComplianceInfo.TaxMessageGroupDescriptions.N0;
			taxMessage3.TaxGroupCode.GovernmentCode = ItalyComplianceInfo.TaxMessageGroupGovtCodes.N0;

			transactionInfoAP_INV.TransactionType = TransactionType.INV;
			var actualXml = new DatiBeniServizi().BuildXML(transactionInfoAP_INV).ToString();
			AssertEquals(string.Format(CultureInfo.InvariantCulture, expectedXmlWithN0TaxGroupCodeAP, string.Empty), actualXml);
		}

		public void TestDatiBeniServiziWithEmptyChargeCodeAR()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRate1 = new TaxID();
			taxRate1.TaxCode = "GS1";
			taxRate1.TaxRate = 30m;

			var taxMessage1 = new TaxMessageID();
			taxMessage1.TaxMessageCode = "TM1";
			taxMessage1.Description = "TM1 Desc";

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.ChargeCode = null;
			line1.GLAccount = new GLAccount();
			line1.GLAccount.AccountCode = "1000.00.01";
			line1.GLAccount.Description = "Test General Ledger";
			line1.Job.Key = "S00001";
			line1.LocalAmount = 120m;
			line1.LocalTotalAmount = 156m;
			line1.OSAmount = 120m;
			line1.OSGSTVATAmount = 36m;
			line1.LocalGSTVATAmount = 18m;
			line1.VATTaxID = taxRate1;
			line1.TaxMessageID = taxMessage1;
			line1.Description = $"Test Description for Line {line1.Sequence}";

			transactionInfo.PostingJournalCollection.Add(line1);

			var actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals(expectedXmlWithoutChargeCodeAR, actualXml);

			line1.ChargeCode = new ChargeCode();

			actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals(expectedXmlWithoutChargeCodeAR, actualXml);
		}

		public void TestDatiBeniServiziWithEmptyChargeCodeAP()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.Ledger = LedgerTypes.AccountsPayable;
			transactionInfo.TransactionType = TransactionType.INV;
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRate1 = new TaxID();
			taxRate1.TaxCode = "GS1";
			taxRate1.TaxRate = 30m;

			var taxMessage1 = new TaxMessageID();
			taxMessage1.TaxMessageCode = "TM1";
			taxMessage1.Description = "TM1 Desc";

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.ChargeCode = null;
			line1.GLAccount = new GLAccount();
			line1.GLAccount.AccountCode = "1000.00.01";
			line1.GLAccount.Description = "Test General Ledger";
			line1.Job.Key = "S00001";
			line1.LocalAmount = -120m;
			line1.LocalTotalAmount = -156m;
			line1.OSAmount = -120m;
			line1.OSGSTVATAmount = -36m;
			line1.LocalGSTVATAmount = -18m;
			line1.VATTaxID = taxRate1;
			line1.TaxMessageID = taxMessage1;
			line1.Description = $"Test Description for Line {line1.Sequence}";

			transactionInfo.PostingJournalCollection.Add(line1);

			var actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals(expectedXmlWithoutChargeCodeAP, actualXml);

			line1.ChargeCode = new ChargeCode();

			actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals(expectedXmlWithoutChargeCodeAP, actualXml);
		}

		public void TestDatiBeniServiziWithExtraTaxRateAR()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRate1 = new TaxID();
			taxRate1.TaxCode = "SP1";
			taxRate1.TaxRate = 20m;
			taxRate1.TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated };
			taxRate1.ExtraTaxType = new CodeDescriptionPair() { Code = AccTaxRate.ExtraTypes.VATRemittedByCustomer };

			var taxMessage1 = new TaxMessageID();
			taxMessage1.TaxMessageCode = "TM1";
			taxMessage1.Description = "TM1 Desc";

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.ChargeCode.Code = "CC1";
			line1.ChargeCode.Description = "Test CC1";
			line1.Job.Key = "S00001";
			line1.LocalAmount = 120m;
			line1.LocalTotalAmount = 120m;
			line1.OSAmount = 120m;
			line1.OSTotalAmount = 120m;
			line1.OSGSTVATAmount = 0m;
			line1.OSExtraVATAmount = -24m;
			line1.LocalExtraVATAmount = -12m;
			line1.VATTaxID = taxRate1;
			line1.TaxMessageID = taxMessage1;
			line1.GLAccount = new GLAccount();
			line1.Description = $"Test Description for Line {line1.Sequence}";

			transactionInfo.PostingJournalCollection.Add(line1);

			var actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals(expectedXmlWithExtraTaxTypeAR, actualXml);
		}

		public void TestDatiBeniServiziWithExtraTaxRateAP()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.Ledger = LedgerTypes.AccountsPayable;
			transactionInfo.TransactionType = TransactionType.INV;
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRate1 = new TaxID();
			taxRate1.TaxCode = "SP1";
			taxRate1.TaxRate = 20m;
			taxRate1.TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated };
			taxRate1.ExtraTaxType = new CodeDescriptionPair() { Code = AccTaxRate.ExtraTypes.VATRemittedByCustomer };

			var taxMessage1 = new TaxMessageID();
			taxMessage1.TaxMessageCode = "TM1";
			taxMessage1.Description = "TM1 Desc";

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.ChargeCode.Code = "CC1";
			line1.ChargeCode.Description = "Test CC1";
			line1.Job.Key = "S00001";
			line1.LocalAmount = -120m;
			line1.LocalTotalAmount = -120m;
			line1.OSAmount = -120m;
			line1.OSTotalAmount = -120m;
			line1.OSGSTVATAmount = 0m;
			line1.OSExtraVATAmount = 24m;
			line1.LocalExtraVATAmount = 12m;
			line1.VATTaxID = taxRate1;
			line1.TaxMessageID = taxMessage1;
			line1.GLAccount = new GLAccount();
			line1.Description = $"Test Description for Line {line1.Sequence}";

			transactionInfo.PostingJournalCollection.Add(line1);

			var actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals(expectedXmlWithExtraTaxTypeAP, actualXml);
		}

		public void TestDatiBeniServiziWithSpecialCharactersAR()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRate1 = new TaxID();
			taxRate1.TaxCode = "SP1";
			taxRate1.TaxRate = 20m;
			taxRate1.TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated };
			taxRate1.ExtraTaxType = new CodeDescriptionPair() { Code = AccTaxRate.ExtraTypes.VATRemittedByCustomer };

			var taxMessage1 = new TaxMessageID();
			taxMessage1.TaxMessageCode = "TM1";
			taxMessage1.Description = "TM1 Desc";

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.ChargeCode.Code = "CC1€test";
			line1.ChargeCode.Description = "Test CC1";
			line1.Job.Key = "S00001€test";
			line1.LocalAmount = 120m;
			line1.LocalTotalAmount = 120m;
			line1.OSAmount = 120m;
			line1.OSTotalAmount = 120m;
			line1.OSGSTVATAmount = 0m;
			line1.OSExtraVATAmount = -24m;
			line1.LocalExtraVATAmount = -12m;
			line1.VATTaxID = taxRate1;
			line1.TaxMessageID = taxMessage1;
			line1.GLAccount = new GLAccount();
			line1.Description = "€10/hr";

			transactionInfo.PostingJournalCollection.Add(line1);

			var actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals(expectedXmlWithSpecialCharacterAR, actualXml);
		}

		public void TestDatiBeniServiziWithAltriDatiGestionaliAR()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.Ledger = LedgerTypes.AccountsReceivable;
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRate1 = new TaxID();
			taxRate1.TaxCode = "DICH.INT";
			taxRate1.TaxRate = 0m;
			taxRate1.TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Exempt };

			var taxMessage1 = new TaxMessageID();
			taxMessage1.TaxMessageCode = "DICHINT";
			taxMessage1.Description = "Dichiarazione intento";
			taxMessage1.EnglishTaxMessage = "Dichiarazione intento";
			taxMessage1.TaxGroupCode = new TaxGroupCodeType();
			taxMessage1.TaxGroupCode.Code = ItalyComplianceInfo.TaxMessageGroupCodes.N35;
			taxMessage1.TaxGroupCode.Description = ItalyComplianceInfo.TaxMessageGroupDescriptions.N35;
			taxMessage1.TaxGroupCode.GovernmentCode = ItalyComplianceInfo.TaxMessageGroupGovtCodes.N35;

			var taxRate2 = new TaxID();
			taxRate2.TaxCode = "SP1";
			taxRate2.TaxRate = 20m;
			taxRate2.TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated };
			taxRate2.ExtraTaxType = new CodeDescriptionPair() { Code = AccTaxRate.ExtraTypes.VATRemittedByCustomer };

			var taxMessage2 = new TaxMessageID();
			taxMessage2.TaxMessageCode = "TM1";
			taxMessage2.Description = "TM1 Desc";

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.ChargeCode.Code = "Test";
			line1.ChargeCode.Description = "Test desc";
			line1.Job.Key = "S00001";
			line1.LocalAmount = 120m;
			line1.LocalTotalAmount = 120m;
			line1.OSAmount = 120m;
			line1.OSTotalAmount = 120m;
			line1.OSGSTVATAmount = 0m;
			line1.OSExtraVATAmount = -24m;
			line1.LocalExtraVATAmount = -12m;
			line1.VATTaxID = taxRate1;
			line1.TaxMessageID = taxMessage1;
			line1.GLAccount = new GLAccount();
			line1.Description = "Test Description for Line 1";

			var line2 = CreatePostingJournal();
			line2.Sequence = 2;
			line2.ChargeCode.Code = "CC2";
			line2.ChargeCode.Description = "Test CC2";
			line2.Job.Key = "S00002";
			line2.LocalAmount = 120m;
			line2.LocalTotalAmount = 120m;
			line2.OSAmount = 120m;
			line2.OSTotalAmount = 120m;
			line2.OSGSTVATAmount = 0m;
			line2.OSExtraVATAmount = -24m;
			line2.LocalExtraVATAmount = -12m;
			line2.VATTaxID = taxRate2;
			line2.TaxMessageID = taxMessage2;
			line2.GLAccount = new GLAccount();
			line2.Description = "Test Description for Line 2";

			transactionInfo.PostingJournalCollection.Add(line1);
			transactionInfo.PostingJournalCollection.Add(line2);

			var documentType = new CodeDescriptionPair();
			documentType.Code = Constants.RefDocTypes.VATExporterExemption;
			var documentUsage = new CodeDescriptionPair();
			documentUsage.Code = JobRequiredDocument.DocUsage.Debtor;
			var category = new CodeDescriptionPair();
			category.Code = Constants.ReferenceTypes.ClientSupplierRelationship;

			var exporterExemptionDocument = new DocumentTracking(DefaultDataObjectWriterStrategy.TestInstance);
			exporterExemptionDocument.DocumentType = documentType;
			exporterExemptionDocument.DocumentUsage = documentUsage;
			exporterExemptionDocument.Category = category;
			exporterExemptionDocument.DocumentNumber = "20190328";
			exporterExemptionDocument.ReceivedDate = new ZDateTime(2020, 10, 15);

			var exporterExemptionDoc2 = new DocumentTracking(DefaultDataObjectWriterStrategy.TestInstance);
			exporterExemptionDoc2.DocumentType = documentType;
			exporterExemptionDoc2.DocumentUsage = documentUsage;
			exporterExemptionDoc2.Category = category;
			exporterExemptionDoc2.DocumentNumber = "20191109";
			exporterExemptionDoc2.ReceivedDate = new ZDateTime(2020, 12, 06);

			var govAuthReference = new DocumentTrackingAttribute();
			govAuthReference.Type = JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference;
			govAuthReference.Value = "02345678912345678-000001";

			var govAuthRef2 = new DocumentTrackingAttribute();
			govAuthRef2.Type = JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference;
			govAuthRef2.Value = "02345678912345678-000002";
			var buyerIssueDate = new DocumentTrackingAttribute();
			buyerIssueDate.Type = JobRequiredDocAttribTypeList.Codes.BuyerIssueDate;
			buyerIssueDate.Value = "02-DEC-20";

			exporterExemptionDocument.SetDocumentTrackingAttributeCollection(() => new List<DocumentTrackingAttribute> { govAuthReference });
			exporterExemptionDoc2.SetDocumentTrackingAttributeCollection(() => new List<DocumentTrackingAttribute> { govAuthRef2, buyerIssueDate });

			var dbsBuilder = new DatiBeniServizi();

			var actualXml = dbsBuilder.BuildXML(transactionInfo).ToString();
			AssertEquals(expectedXmlWithAltriDatiGestionali.Replace("{AltriDatiGestionaliNode}", string.Empty), actualXml);

			transactionInfo.SetOrganizationDocumentTrackingCollection(() => new List<DocumentTracking> { exporterExemptionDocument });

			var expectedAltriDatiGestionali = @"
    <AltriDatiGestionali>
      <TipoDato>INTENTO</TipoDato>
      <RiferimentoTesto>02345678912345678-000001</RiferimentoTesto>
      <RiferimentoData>2020-10-15</RiferimentoData>
    </AltriDatiGestionali>";

			actualXml = dbsBuilder.BuildXML(transactionInfo).ToString();
			AssertEquals(expectedXmlWithAltriDatiGestionali.Replace("{AltriDatiGestionaliNode}", expectedAltriDatiGestionali), actualXml);

			transactionInfo.SetOrganizationDocumentTrackingCollection(() => new List<DocumentTracking> { exporterExemptionDocument, exporterExemptionDoc2 });

			expectedAltriDatiGestionali = @"
    <AltriDatiGestionali>
      <TipoDato>INTENTO</TipoDato>
      <RiferimentoTesto>02345678912345678-000001</RiferimentoTesto>
      <RiferimentoData>2020-10-15</RiferimentoData>
    </AltriDatiGestionali>
    <AltriDatiGestionali>
      <TipoDato>INTENTO</TipoDato>
      <RiferimentoTesto>02345678912345678-000002</RiferimentoTesto>
      <RiferimentoData>2020-12-02</RiferimentoData>
    </AltriDatiGestionali>";

			actualXml = dbsBuilder.BuildXML(transactionInfo).ToString();
			AssertEquals(expectedXmlWithAltriDatiGestionali.Replace("{AltriDatiGestionaliNode}", expectedAltriDatiGestionali), actualXml);

			exporterExemptionDocument.ReceivedDate = ZDateTime.Empty;
			exporterExemptionDoc2.SetDocumentTrackingAttributeCollection(() => new List<DocumentTrackingAttribute> { govAuthRef2 });
			transactionInfo.SetOrganizationDocumentTrackingCollection(() => new List<DocumentTracking> { exporterExemptionDocument, exporterExemptionDoc2 });

			expectedAltriDatiGestionali = @"
    <AltriDatiGestionali>
      <TipoDato>INTENTO</TipoDato>
      <RiferimentoTesto>02345678912345678-000001</RiferimentoTesto>
    </AltriDatiGestionali>
    <AltriDatiGestionali>
      <TipoDato>INTENTO</TipoDato>
      <RiferimentoTesto>02345678912345678-000002</RiferimentoTesto>
      <RiferimentoData>2020-12-06</RiferimentoData>
    </AltriDatiGestionali>";

			actualXml = dbsBuilder.BuildXML(transactionInfo).ToString();
			AssertEquals(expectedXmlWithAltriDatiGestionali.Replace("{AltriDatiGestionaliNode}", expectedAltriDatiGestionali), actualXml);

			exporterExemptionDoc2.ReceivedDate = ZDateTime.Empty;
			transactionInfo.SetOrganizationDocumentTrackingCollection(() => new List<DocumentTracking> { exporterExemptionDocument, exporterExemptionDoc2 });

			expectedAltriDatiGestionali = @"
    <AltriDatiGestionali>
      <TipoDato>INTENTO</TipoDato>
      <RiferimentoTesto>02345678912345678-000001</RiferimentoTesto>
    </AltriDatiGestionali>
    <AltriDatiGestionali>
      <TipoDato>INTENTO</TipoDato>
      <RiferimentoTesto>02345678912345678-000002</RiferimentoTesto>
    </AltriDatiGestionali>";

			actualXml = dbsBuilder.BuildXML(transactionInfo).ToString();
			AssertEquals(expectedXmlWithAltriDatiGestionali.Replace("{AltriDatiGestionaliNode}", expectedAltriDatiGestionali), actualXml);
		}

		public void TestGetFixedCollectionWithoutZeroSequence()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRate1 = new TaxID();
			taxRate1.TaxCode = "SP1";
			taxRate1.TaxRate = 20m;
			taxRate1.TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated };
			taxRate1.ExtraTaxType = new CodeDescriptionPair() { Code = AccTaxRate.ExtraTypes.VATRemittedByCustomer };

			var taxMessage1 = new TaxMessageID();
			taxMessage1.TaxMessageCode = "TM1";
			taxMessage1.Description = "TM1 Desc";

			var line1 = CreatePostingJournal();
			line1.Sequence = 0;
			line1.ChargeCode.Code = "CC1 test";
			line1.ChargeCode.Description = "Test CC1";
			line1.Job.Key = "S00001 test";
			line1.LocalAmount = 120m;
			line1.LocalTotalAmount = 120m;
			line1.OSAmount = 120m;
			line1.OSTotalAmount = 120m;
			line1.OSGSTVATAmount = 0m;
			line1.OSExtraVATAmount = -24m;
			line1.LocalExtraVATAmount = -12m;
			line1.VATTaxID = taxRate1;
			line1.TaxMessageID = taxMessage1;
			line1.GLAccount = new GLAccount();
			line1.Description = "Line description";

			var line2 = CreatePostingJournal();
			line2.Sequence = 50;
			line2.ChargeCode.Code = "CC2 test";
			line2.ChargeCode.Description = "Test CC2";
			line2.Job.Key = "S00002 test";
			line2.LocalAmount = 130m;
			line2.LocalTotalAmount = 130m;
			line2.OSAmount = 130m;
			line2.OSTotalAmount = 130m;
			line2.OSGSTVATAmount = 0m;
			line2.OSExtraVATAmount = -24m;
			line2.LocalExtraVATAmount = -12m;
			line2.VATTaxID = taxRate1;
			line2.TaxMessageID = taxMessage1;
			line2.GLAccount = new GLAccount();
			line2.Description = "Line description";

			transactionInfo.PostingJournalCollection.Add(line1);
			transactionInfo.PostingJournalCollection.Add(line2);

			var actualXml = new DatiBeniServizi().BuildXML(transactionInfo).ToString();
			AssertEquals(expectedXmlWithFixedSequenceLines, actualXml);
		}

		const string NegativeSign = "-";

		const string NaturaElement = @"
    <Natura>N2</Natura>";

		const string NaturaElementNewSchema = @"
    <Natura>N2.1</Natura>";

		const string RiferimentoElementTm2 = "TM2 English MSG";
		const string RiferimentoElementTm3 = "TM3 English MSG";

		const string expectedXmlFullAR = @"<DatiBeniServizi>
  <DettaglioLinee>
    <NumeroLinea>1</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC1</CodiceTipo>
      <CodiceValore>S00001</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 1</Descrizione>
    <PrezzoUnitario>120.00</PrezzoUnitario>
    <PrezzoTotale>120.00</PrezzoTotale>
    <AliquotaIVA>30.00</AliquotaIVA>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>2</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC2</CodiceTipo>
      <CodiceValore>S00002</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 2</Descrizione>
    <PrezzoUnitario>100.00</PrezzoUnitario>
    <PrezzoTotale>100.00</PrezzoTotale>
    <AliquotaIVA>10.00</AliquotaIVA>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>3</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC3</CodiceTipo>
      <CodiceValore>S00001</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 3</Descrizione>
    <PrezzoUnitario>300.00</PrezzoUnitario>
    <PrezzoTotale>300.00</PrezzoTotale>
    <AliquotaIVA>30.00</AliquotaIVA>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>4</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC2</CodiceTipo>
      <CodiceValore>S00002</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 4</Descrizione>
    <PrezzoUnitario>10.00</PrezzoUnitario>
    <PrezzoTotale>10.00</PrezzoTotale>
    <AliquotaIVA>10.00</AliquotaIVA>{0}
    <RiferimentoAmministrazione>{2}</RiferimentoAmministrazione>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>5</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC3</CodiceTipo>
      <CodiceValore>S00003</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 5</Descrizione>
    <PrezzoUnitario>170.00</PrezzoUnitario>
    <PrezzoTotale>170.00</PrezzoTotale>
    <AliquotaIVA>0.00</AliquotaIVA>{0}
    <RiferimentoAmministrazione>{2}</RiferimentoAmministrazione>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>6</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC3</CodiceTipo>
      <CodiceValore>S00003</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 6</Descrizione>
    <PrezzoUnitario>180.00</PrezzoUnitario>
    <PrezzoTotale>180.00</PrezzoTotale>
    <AliquotaIVA>0.00</AliquotaIVA>{0}
    <RiferimentoAmministrazione>{2}</RiferimentoAmministrazione>
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>30.00</AliquotaIVA>
    <ImponibileImporto>420.00</ImponibileImporto>
    <Imposta>84.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
  <DatiRiepilogo>
    <AliquotaIVA>10.00</AliquotaIVA>
    <ImponibileImporto>100.00</ImponibileImporto>
    <Imposta>10.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
  <DatiRiepilogo>
    <AliquotaIVA>10.00</AliquotaIVA>{0}
    <ImponibileImporto>10.00</ImponibileImporto>
    <Imposta>1.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
    <RiferimentoNormativo>{1}</RiferimentoNormativo>
  </DatiRiepilogo>
  <DatiRiepilogo>
    <AliquotaIVA>0.00</AliquotaIVA>{0}
    <ImponibileImporto>350.00</ImponibileImporto>
    <Imposta>0.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
    <RiferimentoNormativo>{1}</RiferimentoNormativo>
  </DatiRiepilogo>
</DatiBeniServizi>";

		const string expectedXmlFullAP = @"<DatiBeniServizi>
  <DettaglioLinee>
    <NumeroLinea>1</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC1</CodiceTipo>
      <CodiceValore>S00001</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 1</Descrizione>
    <PrezzoUnitario>{3}120.00</PrezzoUnitario>
    <PrezzoTotale>{3}120.00</PrezzoTotale>
    <AliquotaIVA>30.00</AliquotaIVA>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>2</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC2</CodiceTipo>
      <CodiceValore>S00002</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 2</Descrizione>
    <PrezzoUnitario>{3}100.00</PrezzoUnitario>
    <PrezzoTotale>{3}100.00</PrezzoTotale>
    <AliquotaIVA>10.00</AliquotaIVA>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>3</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC3</CodiceTipo>
      <CodiceValore>S00001</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 3</Descrizione>
    <PrezzoUnitario>{3}300.00</PrezzoUnitario>
    <PrezzoTotale>{3}300.00</PrezzoTotale>
    <AliquotaIVA>30.00</AliquotaIVA>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>4</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC2</CodiceTipo>
      <CodiceValore>S00002</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 4</Descrizione>
    <PrezzoUnitario>{3}10.00</PrezzoUnitario>
    <PrezzoTotale>{3}10.00</PrezzoTotale>
    <AliquotaIVA>10.00</AliquotaIVA>{0}
    <RiferimentoAmministrazione>{2}</RiferimentoAmministrazione>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>5</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC3</CodiceTipo>
      <CodiceValore>S00003</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 5</Descrizione>
    <PrezzoUnitario>{3}170.00</PrezzoUnitario>
    <PrezzoTotale>{3}170.00</PrezzoTotale>
    <AliquotaIVA>22.00</AliquotaIVA>{0}
    <RiferimentoAmministrazione>{2}</RiferimentoAmministrazione>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>6</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC3</CodiceTipo>
      <CodiceValore>S00003</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 6</Descrizione>
    <PrezzoUnitario>{3}180.00</PrezzoUnitario>
    <PrezzoTotale>{3}180.00</PrezzoTotale>
    <AliquotaIVA>22.00</AliquotaIVA>{0}
    <RiferimentoAmministrazione>{2}</RiferimentoAmministrazione>
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>30.00</AliquotaIVA>
    <ImponibileImporto>{3}420.00</ImponibileImporto>
    <Imposta>{3}84.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
  <DatiRiepilogo>
    <AliquotaIVA>10.00</AliquotaIVA>
    <ImponibileImporto>{3}100.00</ImponibileImporto>
    <Imposta>{3}10.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
  <DatiRiepilogo>
    <AliquotaIVA>10.00</AliquotaIVA>{0}
    <ImponibileImporto>{3}10.00</ImponibileImporto>
    <Imposta>{3}1.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
    <RiferimentoNormativo>{1}</RiferimentoNormativo>
  </DatiRiepilogo>
  <DatiRiepilogo>
    <AliquotaIVA>22.00</AliquotaIVA>{0}
    <ImponibileImporto>{3}350.00</ImponibileImporto>
    <Imposta>{3}77.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
    <RiferimentoNormativo>{1}</RiferimentoNormativo>
  </DatiRiepilogo>
</DatiBeniServizi>";

		const string expectedXmlWithN0TaxGroupCodeAR = @"<DatiBeniServizi>
  <DettaglioLinee>
    <NumeroLinea>1</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC1</CodiceTipo>
      <CodiceValore>S00001</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 1</Descrizione>
    <PrezzoUnitario>120.00</PrezzoUnitario>
    <PrezzoTotale>120.00</PrezzoTotale>
    <AliquotaIVA>30.00</AliquotaIVA>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>2</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC2</CodiceTipo>
      <CodiceValore>S00002</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 2</Descrizione>
    <PrezzoUnitario>100.00</PrezzoUnitario>
    <PrezzoTotale>100.00</PrezzoTotale>
    <AliquotaIVA>10.00</AliquotaIVA>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>3</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC3</CodiceTipo>
      <CodiceValore>S00001</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 3</Descrizione>
    <PrezzoUnitario>300.00</PrezzoUnitario>
    <PrezzoTotale>300.00</PrezzoTotale>
    <AliquotaIVA>30.00</AliquotaIVA>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>4</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC2</CodiceTipo>
      <CodiceValore>S00002</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 4</Descrizione>
    <PrezzoUnitario>10.00</PrezzoUnitario>
    <PrezzoTotale>10.00</PrezzoTotale>
    <AliquotaIVA>10.00</AliquotaIVA>{0}
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>5</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC3</CodiceTipo>
      <CodiceValore>S00003</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 5</Descrizione>
    <PrezzoUnitario>170.00</PrezzoUnitario>
    <PrezzoTotale>170.00</PrezzoTotale>
    <AliquotaIVA>0.00</AliquotaIVA>{0}
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>6</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC3</CodiceTipo>
      <CodiceValore>S00003</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 6</Descrizione>
    <PrezzoUnitario>180.00</PrezzoUnitario>
    <PrezzoTotale>180.00</PrezzoTotale>
    <AliquotaIVA>0.00</AliquotaIVA>{0}
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>30.00</AliquotaIVA>
    <ImponibileImporto>420.00</ImponibileImporto>
    <Imposta>84.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
  <DatiRiepilogo>
    <AliquotaIVA>10.00</AliquotaIVA>
    <ImponibileImporto>100.00</ImponibileImporto>
    <Imposta>10.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
  <DatiRiepilogo>
    <AliquotaIVA>10.00</AliquotaIVA>{0}
    <ImponibileImporto>10.00</ImponibileImporto>
    <Imposta>1.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
  <DatiRiepilogo>
    <AliquotaIVA>0.00</AliquotaIVA>{0}
    <ImponibileImporto>350.00</ImponibileImporto>
    <Imposta>0.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
</DatiBeniServizi>";

		const string expectedXmlWithAltriDatiGestionali = @"<DatiBeniServizi>
  <DettaglioLinee>
    <NumeroLinea>1</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>Test</CodiceTipo>
      <CodiceValore>S00001</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 1</Descrizione>
    <PrezzoUnitario>120.00</PrezzoUnitario>
    <PrezzoTotale>120.00</PrezzoTotale>
    <AliquotaIVA>0.00</AliquotaIVA>
    <Natura>N3.5</Natura>
    <RiferimentoAmministrazione>Dichiarazione intent</RiferimentoAmministrazione>{AltriDatiGestionaliNode}
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>2</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC2</CodiceTipo>
      <CodiceValore>S00002</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 2</Descrizione>
    <PrezzoUnitario>120.00</PrezzoUnitario>
    <PrezzoTotale>120.00</PrezzoTotale>
    <AliquotaIVA>20.00</AliquotaIVA>
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>0.00</AliquotaIVA>
    <Natura>N3.5</Natura>
    <ImponibileImporto>120.00</ImponibileImporto>
    <Imposta>0.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
    <RiferimentoNormativo>Dichiarazione intento</RiferimentoNormativo>
  </DatiRiepilogo>
  <DatiRiepilogo>
    <AliquotaIVA>20.00</AliquotaIVA>
    <ImponibileImporto>120.00</ImponibileImporto>
    <Imposta>12.00</Imposta>
    <EsigibilitaIVA>S</EsigibilitaIVA>
  </DatiRiepilogo>
</DatiBeniServizi>";

		const string expectedXmlWithN0TaxGroupCodeAP = @"<DatiBeniServizi>
  <DettaglioLinee>
    <NumeroLinea>1</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC1</CodiceTipo>
      <CodiceValore>S00001</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 1</Descrizione>
    <PrezzoUnitario>120.00</PrezzoUnitario>
    <PrezzoTotale>120.00</PrezzoTotale>
    <AliquotaIVA>30.00</AliquotaIVA>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>2</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC2</CodiceTipo>
      <CodiceValore>S00002</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 2</Descrizione>
    <PrezzoUnitario>100.00</PrezzoUnitario>
    <PrezzoTotale>100.00</PrezzoTotale>
    <AliquotaIVA>10.00</AliquotaIVA>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>3</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC3</CodiceTipo>
      <CodiceValore>S00001</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 3</Descrizione>
    <PrezzoUnitario>300.00</PrezzoUnitario>
    <PrezzoTotale>300.00</PrezzoTotale>
    <AliquotaIVA>30.00</AliquotaIVA>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>4</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC2</CodiceTipo>
      <CodiceValore>S00002</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 4</Descrizione>
    <PrezzoUnitario>10.00</PrezzoUnitario>
    <PrezzoTotale>10.00</PrezzoTotale>
    <AliquotaIVA>10.00</AliquotaIVA>{0}
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>5</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC3</CodiceTipo>
      <CodiceValore>S00003</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 5</Descrizione>
    <PrezzoUnitario>170.00</PrezzoUnitario>
    <PrezzoTotale>170.00</PrezzoTotale>
    <AliquotaIVA>22.00</AliquotaIVA>{0}
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>6</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC3</CodiceTipo>
      <CodiceValore>S00003</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 6</Descrizione>
    <PrezzoUnitario>180.00</PrezzoUnitario>
    <PrezzoTotale>180.00</PrezzoTotale>
    <AliquotaIVA>22.00</AliquotaIVA>{0}
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>30.00</AliquotaIVA>
    <ImponibileImporto>420.00</ImponibileImporto>
    <Imposta>84.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
  <DatiRiepilogo>
    <AliquotaIVA>10.00</AliquotaIVA>
    <ImponibileImporto>100.00</ImponibileImporto>
    <Imposta>10.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
  <DatiRiepilogo>
    <AliquotaIVA>10.00</AliquotaIVA>{0}
    <ImponibileImporto>10.00</ImponibileImporto>
    <Imposta>1.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
  <DatiRiepilogo>
    <AliquotaIVA>22.00</AliquotaIVA>{0}
    <ImponibileImporto>350.00</ImponibileImporto>
    <Imposta>77.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
</DatiBeniServizi>";

		const string expectedXmlWithoutChargeCodeAR = @"<DatiBeniServizi>
  <DettaglioLinee>
    <NumeroLinea>1</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>1000.00.01</CodiceTipo>
      <CodiceValore>S00001</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 1</Descrizione>
    <PrezzoUnitario>120.00</PrezzoUnitario>
    <PrezzoTotale>120.00</PrezzoTotale>
    <AliquotaIVA>30.00</AliquotaIVA>
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>30.00</AliquotaIVA>
    <ImponibileImporto>120.00</ImponibileImporto>
    <Imposta>18.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
</DatiBeniServizi>";

		const string expectedXmlWithoutChargeCodeAP = @"<DatiBeniServizi>
  <DettaglioLinee>
    <NumeroLinea>1</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>1000.00.01</CodiceTipo>
      <CodiceValore>S00001</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 1</Descrizione>
    <PrezzoUnitario>120.00</PrezzoUnitario>
    <PrezzoTotale>120.00</PrezzoTotale>
    <AliquotaIVA>30.00</AliquotaIVA>
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>30.00</AliquotaIVA>
    <ImponibileImporto>120.00</ImponibileImporto>
    <Imposta>18.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
</DatiBeniServizi>";

		const string expectedXmlWithoutJobNumberAR = @"<DatiBeniServizi>
  <DettaglioLinee>
    <NumeroLinea>1</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC1</CodiceTipo>
      <CodiceValore>{0}</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 1</Descrizione>
    <PrezzoUnitario>120.00</PrezzoUnitario>
    <PrezzoTotale>120.00</PrezzoTotale>
    <AliquotaIVA>30.00</AliquotaIVA>
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>30.00</AliquotaIVA>
    <ImponibileImporto>120.00</ImponibileImporto>
    <Imposta>18.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
</DatiBeniServizi>";

		const string expectedXmlWithoutJobNumberAP = @"<DatiBeniServizi>
  <DettaglioLinee>
    <NumeroLinea>1</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC1</CodiceTipo>
      <CodiceValore>{0}</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 1</Descrizione>
    <PrezzoUnitario>120.00</PrezzoUnitario>
    <PrezzoTotale>120.00</PrezzoTotale>
    <AliquotaIVA>30.00</AliquotaIVA>
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>30.00</AliquotaIVA>
    <ImponibileImporto>120.00</ImponibileImporto>
    <Imposta>18.00</Imposta>
    <EsigibilitaIVA>I</EsigibilitaIVA>
  </DatiRiepilogo>
</DatiBeniServizi>";

		const string expectedXmlWithExtraTaxTypeAR = @"<DatiBeniServizi>
  <DettaglioLinee>
    <NumeroLinea>1</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC1</CodiceTipo>
      <CodiceValore>S00001</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 1</Descrizione>
    <PrezzoUnitario>120.00</PrezzoUnitario>
    <PrezzoTotale>120.00</PrezzoTotale>
    <AliquotaIVA>20.00</AliquotaIVA>
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>20.00</AliquotaIVA>
    <ImponibileImporto>120.00</ImponibileImporto>
    <Imposta>12.00</Imposta>
    <EsigibilitaIVA>S</EsigibilitaIVA>
  </DatiRiepilogo>
</DatiBeniServizi>";

		const string expectedXmlWithExtraTaxTypeAP = @"<DatiBeniServizi>
  <DettaglioLinee>
    <NumeroLinea>1</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC1</CodiceTipo>
      <CodiceValore>S00001</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Test Description for Line 1</Descrizione>
    <PrezzoUnitario>120.00</PrezzoUnitario>
    <PrezzoTotale>120.00</PrezzoTotale>
    <AliquotaIVA>20.00</AliquotaIVA>
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>20.00</AliquotaIVA>
    <ImponibileImporto>120.00</ImponibileImporto>
    <Imposta>12.00</Imposta>
    <EsigibilitaIVA>S</EsigibilitaIVA>
  </DatiRiepilogo>
</DatiBeniServizi>";

		const string expectedXmlWithSpecialCharacterAR = @"<DatiBeniServizi>
  <DettaglioLinee>
    <NumeroLinea>1</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC1 test</CodiceTipo>
      <CodiceValore>S00001 test</CodiceValore>
    </CodiceArticolo>
    <Descrizione> 10/hr</Descrizione>
    <PrezzoUnitario>120.00</PrezzoUnitario>
    <PrezzoTotale>120.00</PrezzoTotale>
    <AliquotaIVA>20.00</AliquotaIVA>
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>20.00</AliquotaIVA>
    <ImponibileImporto>120.00</ImponibileImporto>
    <Imposta>12.00</Imposta>
    <EsigibilitaIVA>S</EsigibilitaIVA>
  </DatiRiepilogo>
</DatiBeniServizi>";

		const string expectedXmlWithFixedSequenceLines = @"<DatiBeniServizi>
  <DettaglioLinee>
    <NumeroLinea>1</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC1 test</CodiceTipo>
      <CodiceValore>S00001 test</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Line description</Descrizione>
    <PrezzoUnitario>120.00</PrezzoUnitario>
    <PrezzoTotale>120.00</PrezzoTotale>
    <AliquotaIVA>20.00</AliquotaIVA>
  </DettaglioLinee>
  <DettaglioLinee>
    <NumeroLinea>51</NumeroLinea>
    <CodiceArticolo>
      <CodiceTipo>CC2 test</CodiceTipo>
      <CodiceValore>S00002 test</CodiceValore>
    </CodiceArticolo>
    <Descrizione>Line description</Descrizione>
    <PrezzoUnitario>130.00</PrezzoUnitario>
    <PrezzoTotale>130.00</PrezzoTotale>
    <AliquotaIVA>20.00</AliquotaIVA>
  </DettaglioLinee>
  <DatiRiepilogo>
    <AliquotaIVA>20.00</AliquotaIVA>
    <ImponibileImporto>250.00</ImponibileImporto>
    <Imposta>24.00</Imposta>
    <EsigibilitaIVA>S</EsigibilitaIVA>
  </DatiRiepilogo>
</DatiBeniServizi>";

		protected override void SetUp()
		{
			base.SetUp();
			SetUpTransactionInfoAR();
			(transactionInfoAP_INV, taxMessage3) = SetUpTransactionInfoAP(TransactionType.INV);
			(transactionInfoAP_CRD, _) = SetUpTransactionInfoAP(TransactionType.CRD);
		}

		TransactionInfo transactionInfoAR;
		TransactionInfo transactionInfoAP_INV;
		TransactionInfo transactionInfoAP_CRD;
		TaxMessageID taxMessage2;
		TaxMessageID taxMessage3;

		void SetUpTransactionInfoAR()
		{
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2021, 01, 01));

			transactionInfoAR = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfoAR.Ledger = LedgerTypes.AccountsReceivable;
			transactionInfoAR.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRate1 = new TaxID();
			taxRate1.TaxCode = "GS1";
			taxRate1.TaxRate = 30m;

			var taxRate2 = new TaxID();
			taxRate2.TaxCode = "GS2";
			taxRate2.TaxRate = 10m;
			taxRate2.ExtraTaxType = new CodeDescriptionPair();
			taxRate2.ExtraTaxType.Code = AccTaxRate.ExtraTypes.VATRetention;

			var taxRate3 = new TaxID();
			taxRate3.TaxCode = "GS3";
			taxRate3.TaxRate = 0m;
			taxRate3.ExtraTaxType = new CodeDescriptionPair();
			taxRate3.ExtraTaxType.Code = AccTaxRate.ExtraTypes.ServiceTax;

			var taxMessage1 = new TaxMessageID();
			taxMessage1.TaxMessageCode = "TM1";
			taxMessage1.Description = "TM1 Desc";
			taxMessage1.EnglishTaxMessage = "TM1 English MSG";

			taxMessage2 = new TaxMessageID();
			taxMessage2.TaxMessageCode = "TM2";
			taxMessage2.Description = "TM2 Desc";
			taxMessage2.EnglishTaxMessage = "TM2 English MSG";

			taxMessage2.TaxGroupCode = new TaxGroupCodeType();
			taxMessage2.TaxGroupCode.Code = ItalyComplianceInfo.TaxMessageGroupCodes.N21;
			taxMessage2.TaxGroupCode.Description = ItalyComplianceInfo.TaxMessageGroupDescriptions.N21;
			taxMessage2.TaxGroupCode.GovernmentCode = ItalyComplianceInfo.TaxMessageGroupGovtCodes.N21;

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.ChargeCode.Code = "CC1";
			line1.ChargeCode.Description = "Test CC1";
			line1.Job.Key = "S00001";
			line1.LocalAmount = 120m;
			line1.LocalTotalAmount = 156m;
			line1.OSAmount = 240m;
			line1.OSGSTVATAmount = 48m;
			line1.LocalGSTVATAmount = 24m;
			line1.VATTaxID = taxRate1;
			line1.TaxMessageID = taxMessage1;

			var line2 = CreatePostingJournal();
			line2.Sequence = 2;
			line2.ChargeCode.Code = "CC2";
			line2.ChargeCode.Description = "Test CC2";
			line2.Job.Key = "S00002";
			line2.LocalAmount = 100m;
			line2.LocalTotalAmount = 110m;
			line2.OSAmount = 200m;
			line2.OSGSTVATAmount = 20m;
			line2.LocalGSTVATAmount = 10m;
			line2.VATTaxID = taxRate2;
			line2.TaxMessageID = taxMessage1;

			var line3 = CreatePostingJournal();
			line3.Sequence = 3;
			line3.ChargeCode.Code = "CC3";
			line3.ChargeCode.Description = "Test CC3";
			line3.Job.Key = "S00001";
			line3.LocalAmount = 300m;
			line3.LocalTotalAmount = 360m;
			line3.OSAmount = 600m;
			line3.OSGSTVATAmount = 120m;
			line3.LocalGSTVATAmount = 60m;
			line3.VATTaxID = taxRate1;
			line3.TaxMessageID = taxMessage1;

			var line4 = CreatePostingJournal();
			line4.Sequence = 4;
			line4.ChargeCode.Code = "CC2";
			line4.ChargeCode.Description = "Test CC2";
			line4.Job.Key = "S00002";
			line4.LocalAmount = 10m;
			line4.LocalTotalAmount = 11m;
			line4.OSAmount = 20m;
			line4.OSGSTVATAmount = 2m;
			line4.LocalGSTVATAmount = 1m;
			line4.VATTaxID = taxRate2;
			line4.TaxMessageID = taxMessage2;

			var line5 = CreatePostingJournal();
			line5.Sequence = 5;
			line5.ChargeCode.Code = "CC3";
			line5.ChargeCode.Description = "Test CC3";
			line5.Job.Key = "S00003";
			line5.LocalAmount = 170m;
			line5.LocalTotalAmount = 170m;
			line5.OSAmount = 340m;
			line5.OSGSTVATAmount = 0m;
			line5.LocalGSTVATAmount = 0m;
			line5.VATTaxID = taxRate3;
			line5.TaxMessageID = taxMessage2;

			var line6 = CreatePostingJournal();
			line6.Sequence = 6;
			line6.ChargeCode.Code = "CC3";
			line6.ChargeCode.Description = "Test CC3";
			line6.Job.Key = "S00003";
			line6.LocalAmount = 180m;
			line6.LocalTotalAmount = 180m;
			line6.OSAmount = 360m;
			line6.OSGSTVATAmount = 0m;
			line6.LocalGSTVATAmount = 0m;
			line6.VATTaxID = taxRate3;
			line6.TaxMessageID = taxMessage2;

			transactionInfoAR.PostingJournalCollection.Add(line1);
			transactionInfoAR.PostingJournalCollection.Add(line2);
			transactionInfoAR.PostingJournalCollection.Add(line3);
			transactionInfoAR.PostingJournalCollection.Add(line4);
			transactionInfoAR.PostingJournalCollection.Add(line5);
			transactionInfoAR.PostingJournalCollection.Add(line6);

			transactionInfoAR.PostingJournalCollection.ForEach(x => x.GLAccount = new GLAccount());
			transactionInfoAR.PostingJournalCollection.ForEach(x => x.Description = $"Test Description for Line {x.Sequence}");
		}

		(TransactionInfo, TaxMessageID) SetUpTransactionInfoAP(TransactionType transactionType)
		{
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2021, 01, 01));

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.Ledger = LedgerTypes.AccountsPayable;
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRate1 = new TaxID();
			taxRate1.TaxCode = "GS1";
			taxRate1.TaxRate = 30m;

			var taxRate2 = new TaxID();
			taxRate2.TaxCode = "GS2";
			taxRate2.TaxRate = 10m;
			taxRate2.ExtraTaxType = new CodeDescriptionPair();
			taxRate2.ExtraTaxType.Code = AccTaxRate.ExtraTypes.VATRetention;

			var taxRate3 = new TaxID();
			taxRate3.TaxCode = "GS3";
			taxRate3.TaxRate = 22m;
			taxRate3.TaxType = new CodeDescriptionPair();
			taxRate3.TaxType.Code = AccTaxRate.Types.ReverseRated;

			var taxMessage1 = new TaxMessageID();
			taxMessage1.TaxMessageCode = "TM1";
			taxMessage1.Description = "TM1 Desc";
			taxMessage1.EnglishTaxMessage = "TM1 English MSG";

			var taxMessage = new TaxMessageID();
			taxMessage.TaxMessageCode = "TM3";
			taxMessage.Description = "TM3 Desc";
			taxMessage.EnglishTaxMessage = "TM3 English MSG";

			taxMessage.TaxGroupCode = new TaxGroupCodeType();
			taxMessage.TaxGroupCode.Code = ItalyComplianceInfo.TaxMessageGroupCodes.N21;
			taxMessage.TaxGroupCode.Description = ItalyComplianceInfo.TaxMessageGroupDescriptions.N21;
			taxMessage.TaxGroupCode.GovernmentCode = ItalyComplianceInfo.TaxMessageGroupGovtCodes.N21;

			int multiplier = transactionType == TransactionType.INV ? -1 : 1;

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.ChargeCode.Code = "CC1";
			line1.ChargeCode.Description = "Test CC1";
			line1.Job.Key = "S00001";
			line1.LocalAmount = multiplier * 120m;
			line1.LocalTotalAmount = multiplier * 156m;
			line1.OSAmount = multiplier * 240m;
			line1.OSGSTVATAmount = multiplier * 48m;
			line1.LocalGSTVATAmount = multiplier * 24m;
			line1.VATTaxID = taxRate1;
			line1.TaxMessageID = taxMessage1;

			var line2 = CreatePostingJournal();
			line2.Sequence = 2;
			line2.ChargeCode.Code = "CC2";
			line2.ChargeCode.Description = "Test CC2";
			line2.Job.Key = "S00002";
			line2.LocalAmount = multiplier * 100m;
			line2.LocalTotalAmount = multiplier * 110m;
			line2.OSAmount = multiplier * 200m;
			line2.OSGSTVATAmount = multiplier * 20m;
			line2.LocalGSTVATAmount = multiplier * 10m;
			line2.VATTaxID = taxRate2;
			line2.TaxMessageID = taxMessage1;

			var line3 = CreatePostingJournal();
			line3.Sequence = 3;
			line3.ChargeCode.Code = "CC3";
			line3.ChargeCode.Description = "Test CC3";
			line3.Job.Key = "S00001";
			line3.LocalAmount = multiplier * 300m;
			line3.LocalTotalAmount = multiplier * 360m;
			line3.OSAmount = multiplier * 600m;
			line3.OSGSTVATAmount = multiplier * 120m;
			line3.LocalGSTVATAmount = multiplier * 60m;
			line3.VATTaxID = taxRate1;
			line3.TaxMessageID = taxMessage1;

			var line4 = CreatePostingJournal();
			line4.Sequence = 4;
			line4.ChargeCode.Code = "CC2";
			line4.ChargeCode.Description = "Test CC2";
			line4.Job.Key = "S00002";
			line4.LocalAmount = multiplier * 10m;
			line4.LocalTotalAmount = multiplier * 11m;
			line4.OSAmount = multiplier * 20m;
			line4.OSGSTVATAmount = multiplier * 2m;
			line4.LocalGSTVATAmount = multiplier * 1m;
			line4.VATTaxID = taxRate2;
			line4.TaxMessageID = taxMessage;

			var line5 = CreatePostingJournal();
			line5.Sequence = 5;
			line5.ChargeCode.Code = "CC3";
			line5.ChargeCode.Description = "Test CC3";
			line5.Job.Key = "S00003";
			line5.LocalAmount = multiplier * 170m;
			line5.LocalTotalAmount = multiplier * 170m;
			line5.OSAmount = multiplier * 340m;
			line5.OSGSTVATAmount = 0m;
			line5.LocalGSTVATAmount = 0m;
			line5.VATTaxID = taxRate3;
			line5.TaxMessageID = taxMessage;

			var line6 = CreatePostingJournal();
			line6.Sequence = 6;
			line6.ChargeCode.Code = "CC3";
			line6.ChargeCode.Description = "Test CC3";
			line6.Job.Key = "S00003";
			line6.LocalAmount = multiplier * 180m;
			line6.LocalTotalAmount = multiplier * 180m;
			line6.OSAmount = multiplier * 360m;
			line6.OSGSTVATAmount = 0m;
			line6.LocalGSTVATAmount = 0m;
			line6.VATTaxID = taxRate3;
			line6.TaxMessageID = taxMessage;

			transaction.PostingJournalCollection.Add(line1);
			transaction.PostingJournalCollection.Add(line2);
			transaction.PostingJournalCollection.Add(line3);
			transaction.PostingJournalCollection.Add(line4);
			transaction.PostingJournalCollection.Add(line5);
			transaction.PostingJournalCollection.Add(line6);

			transaction.PostingJournalCollection.ForEach(x => x.GLAccount = new GLAccount());
			transaction.PostingJournalCollection.ForEach(x => x.Description = $"Test Description for Line {x.Sequence}");

			transaction.OSTotal = multiplier * 1760m;

			return (transaction, taxMessage);
		}

		PostingJournal CreatePostingJournal()
		{
			var result = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);

			result.ChargeCode = new ChargeCode();
			result.Job = new EntityReference();
			result.VATTaxID = new TaxID();
			result.VATTaxID.ExtraTaxType = new CodeDescriptionPair();
			result.TaxMessageID = new TaxMessageID();

			return result;
		}
	}
}
