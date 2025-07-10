using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Export;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using WTG.TestHelpers.Xml;
using static Enterprise.Accounting.Business.EInvoicing.Testing.FatturaElettronicaXmlValueFormatterTest;
using static Enterprise.Accounting.ElectronicMessaging.Italy.Testing.FatturaElettronicaPolicyValidationTest;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using DataContext = Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11.DataContext;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	public class DatiGeneraliTest : TestCaseWithFactory
	{
		public void TestDatiGeneraliForSignsOnAmountsAR()
		{
			var multiplier = -1;
			var datiGenerali = new DatiGenerali(new BusinessObjectFactory());

			for (var i = 0; i < 4; i++)
			{
				multiplier *= -1;
				var transaction = createARTransaction();
				transaction.TransactionType = i < 2 ? TransactionType.CRD : TransactionType.ADJ;
				transaction.LocalExVATAmount = multiplier * 100m;

				var isCreditOrNegative = i < 2 || multiplier < 0;
				var expectedSignMultiplier = isCreditOrNegative ? -1 : 1;
				var expectedDocType = isCreditOrNegative ? "TD04" : "TD01";
				var expectedXmlResult = $@"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>{expectedDocType}</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>{FormatWithZerosAfterDecimalPoint(expectedSignMultiplier * transaction.LocalTotal.Value)}</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR {FormatWithZerosAfterDecimalPoint(expectedSignMultiplier * transaction.OSTotal.Value)}</Causale>
  </DatiGeneraliDocumento>
</DatiGenerali>";

				var actualXmlResult = datiGenerali.BuildXML(transaction, null).ToString();
				XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
			}
		}

		public void TestDatiGeneraliForSignsOnAmountsAP()
		{
			AssertDatiGeneraliForSignsOnAmountsAP(TransactionType.INV, -1);
			AssertDatiGeneraliForSignsOnAmountsAP(TransactionType.ADJ, -1);
			AssertDatiGeneraliForSignsOnAmountsAP(TransactionType.ADJ, 1);
			AssertDatiGeneraliForSignsOnAmountsAP(TransactionType.CRD, 1);
		}

		void AssertDatiGeneraliForSignsOnAmountsAP(TransactionType transactionType, int multiplier)
		{
			var datiGenerali = new DatiGenerali(new BusinessObjectFactory());
			var transaction = createAPTransaction(subType: ItalyComplianceInfo.ComplianceSubTypeCodes.INI);
			transaction.TransactionType = transactionType;

			transaction.LocalTotal = multiplier * 10m;
			transaction.OSTotal = multiplier * 100m;
			var amountSign = (transactionType == TransactionType.INV || (transactionType == TransactionType.ADJ && transaction.LocalTotal < 0)) ? "" : "-";
			var originalDocumentSign = (transactionType == TransactionType.INV && transaction.LocalTotal < 0) || transactionType == TransactionType.CRD ? "-" : "";

			var expectedXmlResult = $@"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>{amountSign}10.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2018-10-15 in EUR al cambio 1.00 per totali EUR {originalDocumentSign}100.00</Causale>
  </DatiGeneraliDocumento>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			var actualXmlResult = datiGenerali.BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		void AssertDatiGeneraliXmlResult(TransactionInfo transaction, TransactionType transType, string complSubType, ZDecimal localExVATAmount, OriginalReference origRef,
			string expectedDocType, string expectedXmlResult, NotificationContainer notifications)
		{
			transaction.TransactionType = transType;
			transaction.ComplianceSubType = complSubType;
			transaction.OriginalReference = origRef;
			transaction.LocalExVATAmount = localExVATAmount;

			var actualXmlResult = new DatiGenerali(Factory).BuildXML(transaction, null, notifications).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult.Replace("@TipoHolder", expectedDocType), actualXmlResult);
		}

		public void TestTipoDocumentoAR()
		{
			var transaction = createARTransaction(null);

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>@TipoHolder</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>00001234</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
</DatiGenerali>";

			var warningNotifications = new NotificationContainer();
			var originalReference = new OriginalReference();

			AssertDatiGeneraliXmlResult(transaction, TransactionType.INV, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, 100, null, "TD01", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.INV, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, 100, originalReference, "TD05", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.INV, ItalyComplianceInfo.ComplianceSubTypeCodes.G26, 100, null, "TD26", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.INV, ItalyComplianceInfo.ComplianceSubTypeCodes.G26, 100, originalReference, "TD26", expectedXmlResult, warningNotifications);

			AssertDatiGeneraliXmlResult(transaction, TransactionType.CRD, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, 100, null, "TD04", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.CRD, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, 100, originalReference, "TD04", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.CRD, ItalyComplianceInfo.ComplianceSubTypeCodes.G26, 100, null, "TD04", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.CRD, ItalyComplianceInfo.ComplianceSubTypeCodes.G26, 100, originalReference, "TD04", expectedXmlResult, warningNotifications);

			AssertDatiGeneraliXmlResult(transaction, TransactionType.ADJ, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, 100, null, "TD01", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.ADJ, ItalyComplianceInfo.ComplianceSubTypeCodes.G26, 100, null, "TD26", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.ADJ, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, -100, null, "TD04", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.ADJ, ItalyComplianceInfo.ComplianceSubTypeCodes.G26, -100, null, "TD04", expectedXmlResult, warningNotifications);
		}

		public void TestTipoDocumentoAP()
		{
			var transaction = createAPTransaction();

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>@TipoHolder</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2018-10-15 in EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			var warningNotifications = new NotificationContainer();

			AssertDatiGeneraliXmlResult(transaction, TransactionType.INV, ItalyComplianceInfo.ComplianceSubTypeCodes.INI, 100, null, "TD16", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.INV, ItalyComplianceInfo.ComplianceSubTypeCodes.INT, 100, null, "TD17", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.INV, ItalyComplianceInfo.ComplianceSubTypeCodes.APS, 100, null, "TD17", expectedXmlResult, warningNotifications);
		}

		public void TestDatiGenerali_BasicAR()
		{
			var transaction = createARTransaction();
			var factory = new BusinessObjectFactory();

			var expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD01</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);

			var numberRegistryOption = AccountingMasterFilesRegistry.Instance.EReportingTransactionNumber;

			using (numberRegistryOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EReportingTransactionNumberOptions.ComplianceNr.Code))
			{
				actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
				XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
			}

			using (numberRegistryOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EReportingTransactionNumberOptions.InvoiceNr.Code))
			{
				expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD01</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>00001234</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
  </DatiGeneraliDocumento>
</DatiGenerali>";

				transaction.OSCurrency = null;
				actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
				XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
			}
		}

		public void TestDatiGenerali_BasicAP()
		{
			var transaction = createAPTransaction(subType: ItalyComplianceInfo.ComplianceSubTypeCodes.INI);
			var factory = new BusinessObjectFactory();

			var expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2018-10-15 in EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);

			var numberRegistryOption = AccountingMasterFilesRegistry.Instance.EReportingTransactionNumber;

			using (numberRegistryOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EReportingTransactionNumberOptions.ComplianceNr.Code))
			{
				actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
				XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
			}

			using (numberRegistryOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EReportingTransactionNumberOptions.InvoiceNr.Code))
			{
				expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
  </DatiGeneraliDocumento>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

				transaction.OSCurrency = null;
				actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
				XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
			}
		}

		public void TestDatiGenerali_CurrencyAR()
		{
			var transaction = createARTransaction();
			transaction.LocalTotal = 10m;
			transaction.OSTotal = 10m;

			var factory = new BusinessObjectFactory();

			var expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD01</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>10.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 10.00</Causale>
  </DatiGeneraliDocumento>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);

			expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD01</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>10.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta USD al cambio 1.213 per totali USD 12.13</Causale>
  </DatiGeneraliDocumento>
</DatiGenerali>";

			transaction.OSCurrency.Code = CurrencyCodes.UnitedStates;
			transaction.ExchangeRate = 1.213m;
			transaction.OSTotal = 12.13m;
			actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiGenerali_CurrencyAP()
		{
			var transaction = createAPTransaction(subType: ItalyComplianceInfo.ComplianceSubTypeCodes.INI);
			transaction.LocalTotal = 10m;
			transaction.OSTotal = 10m;

			var factory = new BusinessObjectFactory();

			var expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>-10.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2018-10-15 in EUR al cambio 1.00 per totali EUR 10.00</Causale>
  </DatiGeneraliDocumento>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);

			expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>-10.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2018-10-15 in USD al cambio 1.213 per totali USD 12.13</Causale>
  </DatiGeneraliDocumento>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			transaction.OSCurrency.Code = CurrencyCodes.UnitedStates;
			transaction.ExchangeRate = 1.213m;
			transaction.OSTotal = 12.13m;
			actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiGenerali_ImportoTotaleDocumentoAP()
		{
			var factory = new BusinessObjectFactory();

			const string expectedXml = @"<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2022-07-20</Data>
    <Numero>CD456</Numero>
    <ImportoTotaleDocumento>563.24</ImportoTotaleDocumento>
  </DatiGeneraliDocumento>
  <DatiFattureCollegate>
    <IdDocumento>AB123</IdDocumento>
    <Data>2022-07-19</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2021, 01, 01)))
			{
				var transactionInfoAP_INV = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				transactionInfoAP_INV.Ledger = LedgerTypes.AccountsPayable;
				transactionInfoAP_INV.SetPostingJournalCollection(() => new List<PostingJournal>());
				transactionInfoAP_INV.TransactionType = TransactionType.INV;
				transactionInfoAP_INV.Number = "AB123";
				transactionInfoAP_INV.DataContext = DataContextFactory.New();
				transactionInfoAP_INV.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				transactionInfoAP_INV.ExchangeRate = 1m;
				transactionInfoAP_INV.LocalTotal = -461.67m;
				transactionInfoAP_INV.OSTotal = -461.67m;
				transactionInfoAP_INV.TransactionReference = "CD456";
				transactionInfoAP_INV.TransactionDate = new ZDateTime(2022, 07, 19);
				transactionInfoAP_INV.PostDate = transactionInfoAP_INV.TransactionDate?.AddDays(1);
				transactionInfoAP_INV.ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.INI;

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
					line.LocalGSTVATAmount = -9.23m;
					line.OSAmount = -41.97m;
					line.OSTotalAmount = -51.20m;
					line.OSGSTVATAmount = -9.23m;

					line.VATTaxID = taxRate;
					line.TaxMessageID = taxMessage;
					line.GLAccount = new GLAccount();
					line.Description = $"Test Description for Line {i}";

					transactionInfoAP_INV.PostingJournalCollection.Add(line);
				}

				var actualXml = new DatiGenerali(factory).BuildXML(transactionInfoAP_INV, null).ToString();
				AssertXMLEquals(expectedXml, actualXml);
			}
		}

		PostingJournal CreatePostingJournal()
		{
			var result = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);

			result.ChargeCode = new ChargeCode();
			result.Job = new EntityReference();
			result.VATTaxID = new TaxID();
			result.VATTaxID.TaxType = new CodeDescriptionPair();
			result.TaxMessageID = new TaxMessageID();

			return result;
		}

		public void TestDatiGenerali_NoTransactionReferenceAR()
		{
			var transaction = createARTransaction(null);
			var factory = new BusinessObjectFactory();

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD01</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    @NumeroHolder
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult.Replace("@NumeroHolder", "<Numero>00001234</Numero>"), actualXmlResult);

			var numberRegistryOption = AccountingMasterFilesRegistry.Instance.EReportingTransactionNumber;

			using (numberRegistryOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EReportingTransactionNumberOptions.InvoiceNr.Code))
			{
				actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
				XmlComparison.CompareAndAssertXml(expectedXmlResult.Replace("@NumeroHolder", "<Numero>00001234</Numero>"), actualXmlResult);
			}

			using (numberRegistryOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EReportingTransactionNumberOptions.ComplianceNr.Code))
			{
				var errorNotifications = new NotificationContainer();
				var expectedError = "Reference Number cannot be empty. Please assign the Compliance Number to this transaction, then re-queue the transaction for sending.";

				actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, errorNotifications).ToString();
				XmlComparison.CompareAndAssertXml(expectedXmlResult.Replace("@NumeroHolder", "<Numero />"), actualXmlResult);
				AssertContainsExactElementsInAnyOrder(new string[] { expectedError }, errorNotifications.GetErrors().Select(x => x.Message));
			}
		}

		public void TestDatiGenerali_NoTransactionReferenceAP()
		{
			var transaction = createAPTransaction(null, ItalyComplianceInfo.ComplianceSubTypeCodes.INI);

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero />
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2018-10-15 in EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			var errorNotifications = new NotificationContainer();

			var actualXmlResult = new DatiGenerali(new BusinessObjectFactory()).BuildXML(transaction, errorNotifications).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
			AssertEquals("Currently there is no error on empty Compl.Nr. but it will fail on XSD validation.", 0, errorNotifications.GetErrors().Count());
		}

		public void TestDatiGenerali_InternalAR()
		{
			var idFiscale = CessionarioCommittenteTest.CreateOrgRegistrationNumberIVA();
			var idFiscaleList = new List<RegistrationNumber> { idFiscale };

			var transaction = createARTransaction(null);
			setupOrgAddress(transaction);
			var orgAddress = transaction.OrganizationAddress;
			orgAddress.SetRegistrationNumberCollection(() => idFiscaleList);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = orgAddress.OrganizationCode.Value;
			org.OH_Category = OrgConstants.Category.Business;
			org.OH_IsGlobalAccount = true;
			org.CustomsCodes.AddNew(idFiscale.Type.Code.Value, idFiscale.Value.Value, idFiscale.CountryOfIssue.Code.Value);

			var brnAddress = transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			brnAddress.OrganizationCode = new ZCodeMappedZString("IT1");
			brnAddress.Country = idFiscale.CountryOfIssue;
			brnAddress.SetRegistrationNumberCollection(() => idFiscaleList );

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>@TipoHolder</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>00001234</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
</DatiGenerali>";

			var warningNotifications = new NotificationContainer();
			var originalReference = new OriginalReference();

			AssertDatiGeneraliXmlResult(transaction, TransactionType.INV, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, 100, null, "TD27", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.INV, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, 100, originalReference, "TD27", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.INV, ItalyComplianceInfo.ComplianceSubTypeCodes.G26, 100, null, "TD27", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.INV, ItalyComplianceInfo.ComplianceSubTypeCodes.G26, 100, originalReference, "TD27", expectedXmlResult, warningNotifications);

			AssertDatiGeneraliXmlResult(transaction, TransactionType.CRD, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, 100, null, "TD04", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.CRD, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, 100, originalReference, "TD04", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.CRD, ItalyComplianceInfo.ComplianceSubTypeCodes.G26, 100, null, "TD04", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.CRD, ItalyComplianceInfo.ComplianceSubTypeCodes.G26, 100, originalReference, "TD04", expectedXmlResult, warningNotifications);

			AssertDatiGeneraliXmlResult(transaction, TransactionType.ADJ, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, 100, null, "TD27", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.ADJ, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, -100, null, "TD04", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.ADJ, ItalyComplianceInfo.ComplianceSubTypeCodes.G26, 100, null, "TD27", expectedXmlResult, warningNotifications);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.ADJ, ItalyComplianceInfo.ComplianceSubTypeCodes.G26, -100, null, "TD04", expectedXmlResult, warningNotifications);
		}

		public void TestDatiGenerali_GoodsAP()
		{
			var transaction = createAPTransaction(null, ItalyComplianceInfo.ComplianceSubTypeCodes.INI);

			var line = CreatePostingJournal();
			line.ChargeCode.Class = new CodeDescriptionPair() { Code = GoodServiceTypes.Codes.GDS, Description = GoodServiceTypes.Descriptions.GDS };
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.PostingJournalCollection.Add(line);

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>@TipoHolder</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero />
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2018-10-15 in EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			AssertDatiGeneraliXmlResult(transaction, TransactionType.INV, ItalyComplianceInfo.ComplianceSubTypeCodes.INI, 100, null, "TD16", expectedXmlResult, null);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.INV, ItalyComplianceInfo.ComplianceSubTypeCodes.INT, 100, null, "TD18", expectedXmlResult, null);
			AssertDatiGeneraliXmlResult(transaction, TransactionType.INV, ItalyComplianceInfo.ComplianceSubTypeCodes.APS, 100, null, "TD19", expectedXmlResult, null);
		}

		public void TestDatiGenerali_BolloAR()
		{
			var arInv = Factory.NewWithValidTestData<ARInvoice>();
			arInv.AH_TransactionNum = "00001000";
			arInv.Logs.AddNew(AutoEvents.StampDutyLiability, "test event message");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.StampDutyARDocumentMessage.SetValue(arInv.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, "stamp duty ar document message");
			AccountingConfigurationRegistry.Instance.StampDutyFixedAmount.SetValue(arInv.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, 12.56m);

			var transaction = createARTransaction();
			transaction.Number = "00001000";

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD01</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>00005678</Numero>
    <DatiBollo>
      <BolloVirtuale>SI</BolloVirtuale>
      <ImportoBollo>12.56</ImportoBollo>
    </DatiBollo>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(new BusinessObjectFactory()).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiGenerali_BolloAP()
		{
			var apInv = Factory.NewWithValidTestData<APInvoice>();
			apInv.AH_TransactionNum = "00001000";
			apInv.Logs.AddNew(AutoEvents.StampDutyLiability, "test event message");
			Factory.Save();

			var registry = AccountingConfigurationRegistry.Instance;
			registry.StampDutyARDocumentMessage.SetValue(apInv.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, "stamp duty ar document message");
			registry.StampDutyFixedAmount.SetValue(apInv.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, 12.56m);

			var transaction = createAPTransaction(subType: ItalyComplianceInfo.ComplianceSubTypeCodes.INI);

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2018-10-15 in EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(new BusinessObjectFactory()).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiGenerali_FattureCollegateAR()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "OH1";
			org.OH_Category = OrgConstants.Category.Business;
			Factory.Save();

			var transaction = createARTransaction();
			setupOrgAddress(transaction);
			transaction.TransactionType = TransactionType.CRD;
			transaction.TransactionDate = new ZDateTime(2020, 12, 05);

			const string expectedDatiGeneraliDocumento = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD04</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2020-12-05</Data>
    <Numero>0000@NumeroHolder</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>@FattureHolder
</DatiGenerali>";

			var expectedXmlResult_WithoutAnyOriginalRefsCIN_COM = expectedDatiGeneraliDocumento.Replace("@NumeroHolder", "5678").Replace("@FattureHolder", string.Empty);
			var expectedXmlResult_WithoutAnyOriginalRefsINV = expectedDatiGeneraliDocumento.Replace("@NumeroHolder", "1234").Replace("@FattureHolder", string.Empty);

			var datiGenerali = new DatiGenerali(new BusinessObjectFactory());

			AssertResultTransactionReference(transaction, expectedXmlResult_WithoutAnyOriginalRefsCIN_COM, datiGenerali, TransactionNumberCodes.ComplianceOrInvoiceNr, false);
			AssertResultTransactionReference(transaction, expectedXmlResult_WithoutAnyOriginalRefsINV, datiGenerali, TransactionNumberCodes.InvoiceNr, false);
			AssertResultTransactionReference(transaction, expectedXmlResult_WithoutAnyOriginalRefsCIN_COM, datiGenerali, TransactionNumberCodes.ComplianceNr, false);

			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionReference = ZString.Empty;
			transaction.OriginalReference.OriginalTransactionNumber = ZString.Empty;
			transaction.OriginalReference.OriginalTransactionDate = new ZDateTime(2020, 12, 01);

			AssertResultTransactionReference(transaction, expectedXmlResult_WithoutAnyOriginalRefsCIN_COM, datiGenerali, TransactionNumberCodes.ComplianceOrInvoiceNr, true);
			AssertResultTransactionReference(transaction, expectedXmlResult_WithoutAnyOriginalRefsINV, datiGenerali, TransactionNumberCodes.InvoiceNr, true);
			AssertResultTransactionReference(transaction, expectedXmlResult_WithoutAnyOriginalRefsCIN_COM, datiGenerali, TransactionNumberCodes.ComplianceNr, true);

			transaction.OriginalReference.OriginalTransactionReference = ZString.Empty;
			transaction.OriginalReference.OriginalTransactionNumber = "OTN0001";
			transaction.OriginalReference.OriginalTransactionDate = ZDateTime.Empty;

			AssertResultTransactionReference(transaction, expectedXmlResult_WithoutAnyOriginalRefsCIN_COM, datiGenerali, TransactionNumberCodes.ComplianceOrInvoiceNr, true);
			AssertResultTransactionReference(transaction, expectedXmlResult_WithoutAnyOriginalRefsINV, datiGenerali, TransactionNumberCodes.InvoiceNr, true);
			AssertResultTransactionReference(transaction, expectedXmlResult_WithoutAnyOriginalRefsCIN_COM, datiGenerali, TransactionNumberCodes.ComplianceNr, true);

			transaction.OriginalReference.OriginalTransactionReference = "OTR0001";
			transaction.OriginalReference.OriginalTransactionNumber = ZString.Empty;
			transaction.OriginalReference.OriginalTransactionDate = ZDateTime.Empty;

			AssertResultTransactionReference(transaction, expectedXmlResult_WithoutAnyOriginalRefsCIN_COM, datiGenerali, TransactionNumberCodes.ComplianceOrInvoiceNr, true);
			AssertResultTransactionReference(transaction, expectedXmlResult_WithoutAnyOriginalRefsINV, datiGenerali, TransactionNumberCodes.InvoiceNr, true);
			AssertResultTransactionReference(transaction, expectedXmlResult_WithoutAnyOriginalRefsCIN_COM, datiGenerali, TransactionNumberCodes.ComplianceNr, true);

			transaction.OriginalReference.OriginalTransactionReference = "OTR0001";
			transaction.OriginalReference.OriginalTransactionNumber = ZString.Empty;
			transaction.OriginalReference.OriginalTransactionDate = new ZDateTime(2020, 12, 01);

			const string expectedDatiFattureCollegate = @"
  <DatiFattureCollegate>
    <IdDocumento>@NrDocHolder</IdDocumento>
    <Data>2020-12-01</Data>
  </DatiFattureCollegate>";

			var expectedXmlResult_WithOriginalTransactionRefCIN_COM = expectedDatiGeneraliDocumento.Replace("@NumeroHolder", "5678").Replace("@FattureHolder",
				expectedDatiFattureCollegate.Replace("@NrDocHolder", "OTR0001"));

			AssertResultTransactionReference(transaction, expectedXmlResult_WithOriginalTransactionRefCIN_COM, datiGenerali, TransactionNumberCodes.ComplianceOrInvoiceNr, false);
			AssertResultTransactionReference(transaction, expectedXmlResult_WithoutAnyOriginalRefsINV, datiGenerali, TransactionNumberCodes.InvoiceNr, true);
			AssertResultTransactionReference(transaction, expectedXmlResult_WithOriginalTransactionRefCIN_COM, datiGenerali, TransactionNumberCodes.ComplianceNr, false);

			transaction.OriginalReference.OriginalTransactionReference = ZString.Empty;
			transaction.OriginalReference.OriginalTransactionNumber = "OTN0001";

			var expectedXmlResult_WithOriginalTransactionNumCIN = expectedDatiGeneraliDocumento.Replace("@NumeroHolder", "5678").Replace("@FattureHolder",
				expectedDatiFattureCollegate.Replace("@NrDocHolder", "OTN0001"));

			var expectedXmlResult_WithOriginalTransactionNumINV = expectedDatiGeneraliDocumento.Replace("@NumeroHolder", "1234").Replace("@FattureHolder",
				expectedDatiFattureCollegate.Replace("@NrDocHolder", "OTN0001"));

			AssertResultTransactionReference(transaction, expectedXmlResult_WithOriginalTransactionNumCIN, datiGenerali, TransactionNumberCodes.ComplianceOrInvoiceNr, false);
			AssertResultTransactionReference(transaction, expectedXmlResult_WithOriginalTransactionNumINV, datiGenerali, TransactionNumberCodes.InvoiceNr, false);
			AssertResultTransactionReference(transaction, expectedXmlResult_WithoutAnyOriginalRefsCIN_COM, datiGenerali, TransactionNumberCodes.ComplianceNr, true);

			transaction.OriginalReference.OriginalTransactionReference = "OTR0001";
			transaction.OriginalReference.OriginalTransactionNumber = "OTN0001";

			AssertResultTransactionReference(transaction, expectedXmlResult_WithOriginalTransactionRefCIN_COM, datiGenerali, TransactionNumberCodes.ComplianceOrInvoiceNr, false);
			AssertResultTransactionReference(transaction, expectedXmlResult_WithOriginalTransactionNumINV, datiGenerali, TransactionNumberCodes.InvoiceNr, false);
			AssertResultTransactionReference(transaction, expectedXmlResult_WithOriginalTransactionRefCIN_COM, datiGenerali, TransactionNumberCodes.ComplianceNr, false);

			void AssertResultTransactionReference(TransactionInfo testTransaction, string testExpectedXmlResult, DatiGenerali testDatiGenerali, string testTransactionNumberCodes, bool testReturnWarningMessage)
			{
				var errorNotifications = new NotificationContainer();
				var warningNotifications = new NotificationContainer();
				AccountingMasterFilesRegistry.Instance.EReportingTransactionNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testTransactionNumberCodes);
				var actualXmlResult = testDatiGenerali.BuildXML(testTransaction, errorNotifications, warningNotifications).ToString();
				XmlComparison.CompareAndAssertXml(testExpectedXmlResult, actualXmlResult);

				if (testReturnWarningMessage)
				{
					string expectedError = @"FatturaElettronicaBody/DatiGenerali/DatiFattureCollegate <IdDocumento>, <Data> should contain Original Invoice Number and Original Invoice Date of Original Transaction. Without these references, the invoice may be not linked to the original transaction.";
					AssertContainsExactElementsInAnyOrder(new string[] { expectedError }, warningNotifications.GetWarnings().Select(x => x.Message));
				}
				else
				{
					Assert("Warning message present when it shouldn't be", !warningNotifications.GetWarnings().Any());
				}
			}
		}

		public void TestDatiGenerali_FattureCollegateAP()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "OH1";
			org.OH_Category = OrgConstants.Category.Business;
			Factory.Save();

			var transaction = createAPTransaction(subType: ItalyComplianceInfo.ComplianceSubTypeCodes.INI);
			setupOrgAddress(transaction);
			transaction.TransactionType = TransactionType.CRD;
			transaction.TransactionDate = new ZDateTime(2020, 12, 05);
			transaction.PostDate = transaction.TransactionDate?.AddDays(1);

			const string expectedXml = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2020-12-06</Data>
    @NumeroHolder
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2020-12-05 in EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2020-12-05</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			var errorNotifications = new NotificationContainer();
			var warningNotifications = new NotificationContainer();
			var datiGenerali = new DatiGenerali(new BusinessObjectFactory());
			var actualXmlResult = datiGenerali.BuildXML(transaction, errorNotifications, warningNotifications).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml.Replace("@NumeroHolder", "<Numero>00005678</Numero>"), actualXmlResult);

			transaction.TransactionReference = ZString.Empty;
			actualXmlResult = datiGenerali.BuildXML(transaction, errorNotifications, warningNotifications).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml.Replace("@NumeroHolder", "<Numero />"), actualXmlResult);
		}

		void AssertResultOrdineAcquisto_WithoutShipment(BusinessObjectFactory factory, TransactionInfo pTransaction, string pExpectedXmlResult, string pExpectedError, bool insertCig, bool insertCup)
		{
			pTransaction.ShipmentCollection.Clear();
			var shipmentTest = createShipmentDataWithoutOrderNumberButWithCigOrCup("S00001234", insertCig, insertCup);
			pTransaction.ShipmentCollection.Add(shipmentTest);
			var errorNotifications = new NotificationContainer();
			var varActualXmlResult = new DatiGenerali(factory).BuildXML(pTransaction, errorNotifications).ToString();
			XmlComparison.CompareAndAssertXml(pExpectedXmlResult, varActualXmlResult);
			AssertContainsExactElementsInAnyOrder(new string[] { pExpectedError }, errorNotifications.GetErrors().Select(x => x.Message));
		}

		public void TestDatiGenerali_OrdineAcquistoAR()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "OH1";
			org.OH_Category = "GOV";
			Factory.Save();

			var transaction = createARTransaction();
			setupOrgAddress(transaction);
			transaction.Job = new EntityReference() { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S00001234" };

			var shipment = createShipmentData("S00001234");
			transaction.SetShipmentCollection(() => new List<Shipment>());
			transaction.ShipmentCollection.Add(shipment);

			var factory = new BusinessObjectFactory();

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD01</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>@OrdineHolder
</DatiGenerali>";

			var expectedXmlResult_WithShipment = expectedXmlResult.Replace("@OrdineHolder", @"
  <DatiOrdineAcquisto>
    <IdDocumento>abc0123456,xyz012345</IdDocumento>
    <CodiceCUP>CUP012345678901</CodiceCUP>
    <CodiceCIG>CIG987654321098</CodiceCIG>
  </DatiOrdineAcquisto>
");

			var actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult_WithShipment, actualXmlResult);

			org.OH_Category = "BUS";
			Factory.Save();

			actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult_WithShipment, actualXmlResult);

			var expectedXmlResult_WithoutShipment = expectedXmlResult.Replace("@OrdineHolder", string.Empty);

			var expectedError_OrgNonGOV = @"FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto <IdDocumento>, <CodiceCUP> and <CodiceCIG> Job must contain Order Reference when CIG and CUP are specified.";

			AssertResultOrdineAcquisto_WithoutShipment(factory, transaction, expectedXmlResult_WithoutShipment, expectedError_OrgNonGOV, true, true);

			AssertResultOrdineAcquisto_WithoutShipment(factory, transaction, expectedXmlResult_WithoutShipment, expectedError_OrgNonGOV, true, false);

			AssertResultOrdineAcquisto_WithoutShipment(factory, transaction, expectedXmlResult_WithoutShipment, expectedError_OrgNonGOV, false, true);

			org.OH_Category = "GOV";
			Factory.Save();

			var expectedError = @"FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto <IdDocumento>, <CodiceCUP> and <CodiceCIG> Job must contain Order Reference, CIG and CUP when Account is a GOV Organization.";

			AssertResultOrdineAcquisto_WithoutShipment(factory, transaction, expectedXmlResult_WithoutShipment, expectedError, true, true);

			AssertResultOrdineAcquisto_WithoutShipment(factory, transaction, expectedXmlResult_WithoutShipment, expectedError, true, false);

			AssertResultOrdineAcquisto_WithoutShipment(factory, transaction, expectedXmlResult_WithoutShipment, expectedError, false, true);
		}

		public void TestDatiGenerali_OrdineAcquistoAP()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "OH1";
			org.OH_Category = "GOV";
			Factory.Save();

			var transaction = createAPTransaction(subType: ItalyComplianceInfo.ComplianceSubTypeCodes.INI);
			setupOrgAddress(transaction);
			transaction.Job = new EntityReference() { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S00001234" };

			var shipment = createShipmentData("S00001234");
			transaction.SetShipmentCollection(() => new List<Shipment>());
			transaction.ShipmentCollection.Add(shipment);

			var factory = new BusinessObjectFactory();

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2018-10-15 in EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>@OrdineHolder
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			var expectedXmlResult_WithShipment = expectedXmlResult.Replace("@OrdineHolder", @"
  <DatiOrdineAcquisto>
    <IdDocumento>abc0123456,xyz012345</IdDocumento>
    <CodiceCUP>CUP012345678901</CodiceCUP>
    <CodiceCIG>CIG987654321098</CodiceCIG>
  </DatiOrdineAcquisto>
");

			var actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult_WithShipment, actualXmlResult);

			org.OH_Category = "BUS";
			Factory.Save();

			actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult_WithShipment, actualXmlResult);

			var expectedXmlResult_WithoutShipment = expectedXmlResult.Replace("@OrdineHolder", String.Empty);

			var expectedError_OrgNonGOV = @"FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto <IdDocumento>, <CodiceCUP> and <CodiceCIG> Job must contain Order Reference when CIG and CUP are specified.";

			AssertResultOrdineAcquisto_WithoutShipment(factory, transaction, expectedXmlResult_WithoutShipment, expectedError_OrgNonGOV, true, true);

			AssertResultOrdineAcquisto_WithoutShipment(factory, transaction, expectedXmlResult_WithoutShipment, expectedError_OrgNonGOV, true, false);

			AssertResultOrdineAcquisto_WithoutShipment(factory, transaction, expectedXmlResult_WithoutShipment, expectedError_OrgNonGOV, false, true);

			org.OH_Category = "GOV";
			Factory.Save();

			var expectedError = @"FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto <IdDocumento>, <CodiceCUP> and <CodiceCIG> Job must contain Order Reference, CIG and CUP when Account is a GOV Organization.";

			AssertResultOrdineAcquisto_WithoutShipment(factory, transaction, expectedXmlResult_WithoutShipment, expectedError, true, true);

			AssertResultOrdineAcquisto_WithoutShipment(factory, transaction, expectedXmlResult_WithoutShipment, expectedError, true, false);

			AssertResultOrdineAcquisto_WithoutShipment(factory, transaction, expectedXmlResult_WithoutShipment, expectedError, false, true);
		}

		void AddOrderToShipment(Shipment shipment, string orderReference)
		{
			var orderNumber = new OrderNumber();
			orderNumber.OrderReference = orderReference;
			shipment.LocalProcessing.OrderNumberCollection.Add(orderNumber);
		}

		public void TestDatiGenerali_OrdineAcquisto_MultipleOrderRefs_AR()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "OH1";
			org.OH_Category = "GOV";
			Factory.Save();

			var transaction = createARTransaction();
			setupOrgAddress(transaction);
			transaction.Job = new EntityReference() { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S00001234" };

			var shipment = createShipmentData("S00001234");
			shipment.LocalProcessing.OrderNumberCollection.Clear();

			var orderNumbers = new string[] { "abc", "def", "ghi", "lmn", "opq" };
			foreach (var orderNumber in orderNumbers)
			{
				AddOrderToShipment(shipment, orderNumber);
			}

			transaction.SetShipmentCollection(() => new List<Shipment>());
			transaction.ShipmentCollection.Add(shipment);

			var factory = new BusinessObjectFactory();

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD01</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
  <DatiOrdineAcquisto>
    <IdDocumento>abc,def,ghi,lmn,opq</IdDocumento>
    <CodiceCUP>CUP012345678901</CodiceCUP>
    <CodiceCIG>CIG987654321098</CodiceCIG>
  </DatiOrdineAcquisto>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiGenerali_OrdineAcquisto_MultipleOrderRefs_AP()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "OH1";
			org.OH_Category = "GOV";
			Factory.Save();

			var transaction = createAPTransaction(subType: ItalyComplianceInfo.ComplianceSubTypeCodes.INI);
			setupOrgAddress(transaction);
			transaction.Job = new EntityReference() { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S00001234" };

			var shipment = createShipmentData("S00001234");
			shipment.LocalProcessing.OrderNumberCollection.Clear();

			var orderNumbers = new string[] { "abc", "def", "ghi", "lmn", "opq" };
			foreach (var orderNumber in orderNumbers)
			{
				AddOrderToShipment(shipment, orderNumber);
			}

			transaction.SetShipmentCollection(() => new List<Shipment>());
			transaction.ShipmentCollection.Add(shipment);

			var factory = new BusinessObjectFactory();

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2018-10-15 in EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
  <DatiOrdineAcquisto>
    <IdDocumento>abc,def,ghi,lmn,opq</IdDocumento>
    <CodiceCUP>CUP012345678901</CodiceCUP>
    <CodiceCIG>CIG987654321098</CodiceCIG>
  </DatiOrdineAcquisto>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiGenerali_OrdineAcquisto_SubShipmentsAR()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "OH1";
			org.OH_Category = "GOV";
			Factory.Save();

			var transaction = createARTransaction(null);
			setupOrgAddress(transaction);
			transaction.Job = new EntityReference() { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S00001234" };

			var subShipment = createShipmentData("S00001234");
			var parentShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new DataContext()
				{
					DataSourceCollection = new List<DataSource>()
					{
						new DataSource() { Type = "ForwardingShipment", Key = "S00009999" }
					}
				}
			};
			parentShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });
			transaction.SetShipmentCollection(() => new List<Shipment> { parentShipment });

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD01</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>00001234</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
  <DatiOrdineAcquisto>
    <IdDocumento>abc0123456,xyz012345</IdDocumento>
    <CodiceCUP>CUP012345678901</CodiceCUP>
    <CodiceCIG>CIG987654321098</CodiceCIG>
  </DatiOrdineAcquisto>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(new BusinessObjectFactory()).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiGenerali_OrdineAcquisto_SubShipmentsAP()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "OH1";
			org.OH_Category = "GOV";
			Factory.Save();

			var transaction = createAPTransaction(subType: ItalyComplianceInfo.ComplianceSubTypeCodes.INI);
			setupOrgAddress(transaction);
			transaction.Job = new EntityReference() { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S00001234" };

			var subShipment = createShipmentData("S00001234");
			var parentShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new DataContext()
				{
					DataSourceCollection = new List<DataSource>()
					{
						new DataSource() { Type = "ForwardingShipment", Key = "S00009999" }
					}
				}
			};
			parentShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });
			transaction.SetShipmentCollection(() => new List<Shipment> { parentShipment });

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2018-10-15 in EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
  <DatiOrdineAcquisto>
    <IdDocumento>abc0123456,xyz012345</IdDocumento>
    <CodiceCUP>CUP012345678901</CodiceCUP>
    <CodiceCIG>CIG987654321098</CodiceCIG>
  </DatiOrdineAcquisto>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(new BusinessObjectFactory()).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiGenerali_OrdineAcquisto_SubShipmentsUnderConsolAR()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "OH1";
			org.OH_Category = "GOV";
			Factory.Save();

			var transaction = createARTransaction(null);
			setupOrgAddress(transaction);
			transaction.Job = new EntityReference() { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S00001234" };

			var parentSubShipment = createShipmentData("S00009999");
			var childSubShipment = createShipmentData("S00001234");
			childSubShipment.LocalProcessing.OrderNumberCollection.ForEach(x => x.OrderReference = x.OrderReference.Value.Left(3));
			childSubShipment.AdditionalReferenceCollection.Clear();
			parentSubShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { childSubShipment });
			var consolShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new DataContext()
				{
					DataSourceCollection = new List<DataSource>()
					{
						new DataSource() { Type = "ForwardingConsol", Key = "C00001111" },
						new DataSource() { Type = "ForwardingShipment", Key = "S00009999" }
					}
				}
			};
			consolShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { parentSubShipment });
			transaction.SetShipmentCollection(() => new List<Shipment> { consolShipment });

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD01</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>00001234</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
  <DatiOrdineAcquisto>
    <IdDocumento>abc,xyz</IdDocumento>
  </DatiOrdineAcquisto>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(new BusinessObjectFactory()).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiGenerali_OrdineAcquisto_SubShipmentsUnderConsolAP()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "OH1";
			org.OH_Category = "GOV";
			Factory.Save();

			var transaction = createAPTransaction(subType: ItalyComplianceInfo.ComplianceSubTypeCodes.INI);
			setupOrgAddress(transaction);
			transaction.Job = new EntityReference() { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S00001234" };

			var parentSubShipment = createShipmentData("S00009999");
			var childSubShipment = createShipmentData("S00001234");
			childSubShipment.LocalProcessing.OrderNumberCollection.ForEach(x => x.OrderReference = x.OrderReference.Value.Left(3));
			childSubShipment.AdditionalReferenceCollection.Clear();
			parentSubShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { childSubShipment });
			var consolShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new DataContext()
				{
					DataSourceCollection = new List<DataSource>()
					{
						new DataSource() { Type = "ForwardingConsol", Key = "C00001111" },
						new DataSource() { Type = "ForwardingShipment", Key = "S00009999" }
					}
				}
			};
			consolShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { parentSubShipment });
			transaction.SetShipmentCollection(() => new List<Shipment> { consolShipment });

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2018-10-15 in EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
  <DatiOrdineAcquisto>
    <IdDocumento>abc,xyz</IdDocumento>
  </DatiOrdineAcquisto>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(new BusinessObjectFactory()).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiGenerali_WithSpecialCharacters()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "OH1";
			org.OH_Category = "GOV";
			Factory.Save();

			var transaction = createARTransaction(null);
			setupOrgAddress(transaction);
			transaction.Job = new EntityReference() { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S00001234" };
			transaction.Number = "0000100€0";

			var parentSubShipment = createShipmentData("S00009999");
			var childSubShipment = createShipmentData("S00001234");
			childSubShipment.LocalProcessing.OrderNumberCollection.ForEach(x => x.OrderReference = x.OrderReference.Value.Left(3));
			childSubShipment.AdditionalReferenceCollection.Clear();
			parentSubShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { childSubShipment });
			var consolShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new DataContext()
				{
					DataSourceCollection = new List<DataSource>()
					{
						new DataSource() { Type = "ForwardingConsol", Key = "C00001111" },
						new DataSource() { Type = "ForwardingShipment", Key = "S00009999" }
					}
				}
			};
			consolShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { parentSubShipment });
			transaction.SetShipmentCollection(() => new List<Shipment> { consolShipment });

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD01</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>0000100 0</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
  <DatiOrdineAcquisto>
    <IdDocumento>abc,xyz</IdDocumento>
  </DatiOrdineAcquisto>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(new BusinessObjectFactory()).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiGenerali_DatiGeneraliDocumento_CausaleElementPresentWhenTransactionDescriptionExistsAR()
		{
			var transaction = createARTransaction(null);
			transaction.LocalExVATAmount = 100;
			transaction.Description = "This is a transaction header description";

			var factory = new BusinessObjectFactory();

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD01</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>00001234</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 0.00</Causale>@CausaleHolder
  </DatiGeneraliDocumento>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult.Replace("@CausaleHolder", $"\n<Causale>{transaction.Description}</Causale>\n"), actualXmlResult);

			transaction.Description = "";
			actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult.Replace("@CausaleHolder", String.Empty), actualXmlResult);
		}

		public void TestDatiGenerali_DatiGeneraliDocumento_CausaleElementPresentWhenTransactionDescriptionExistsAP()
		{
			var transaction = createAPTransaction(subType: ItalyComplianceInfo.ComplianceSubTypeCodes.INI);
			transaction.LocalExVATAmount = 100;
			transaction.Description = "This is a transaction header description";

			var factory = new BusinessObjectFactory();

			const string expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2018-10-15 in EUR al cambio 1.00 per totali EUR 0.00</Causale>@CausaleHolder
  </DatiGeneraliDocumento>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";

			var actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult.Replace("@CausaleHolder", $"\n<Causale>{transaction.Description}</Causale>\n"), actualXmlResult);

			transaction.Description = "";
			actualXmlResult = new DatiGenerali(factory).BuildXML(transaction, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult.Replace("@CausaleHolder", String.Empty), actualXmlResult);
		}

		public void TestDatiGenerali_DatiGeneraliDocumento_CausaleElementNotPresentWhenExporterExemptionDocumentExists()
		{
			var exporterExemptionDocument = new DocumentTracking(DefaultDataObjectWriterStrategy.TestInstance);
			var documentType = new CodeDescriptionPair();
			documentType.Code = Constants.RefDocTypes.VATExporterExemption;
			var documentUsage = new CodeDescriptionPair();
			documentUsage.Code = JobRequiredDocument.DocUsage.Debtor;
			var category = new CodeDescriptionPair();
			category.Code = Constants.ReferenceTypes.ClientSupplierRelationship;
			exporterExemptionDocument.DocumentType = documentType;
			exporterExemptionDocument.DocumentUsage = documentUsage;
			exporterExemptionDocument.Category = category;
			exporterExemptionDocument.DocumentNumber = "20190328";

			var attributesList = new List<DocumentTrackingAttribute>();
			var govAuthReference = new DocumentTrackingAttribute();
			govAuthReference.Type = JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference;
			govAuthReference.Value = "02345678912345678-654321";
			attributesList.Add(govAuthReference);
			exporterExemptionDocument.SetDocumentTrackingAttributeCollection(() => attributesList);

			var transaction = createARTransaction(null);
			transaction.LocalExVATAmount = 100;
			transaction.SetOrganizationDocumentTrackingCollection(() => new List<DocumentTracking> { exporterExemptionDocument });

			var expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD01</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-15</Data>
    <Numero>00001234</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
</DatiGenerali>";
			AssertForExemptionDocument();

			transaction = createAPTransaction(subType: ItalyComplianceInfo.ComplianceSubTypeCodes.INI);
			transaction.LocalExVATAmount = 100;
			transaction.SetOrganizationDocumentTrackingCollection(() => new List<DocumentTracking> { exporterExemptionDocument });

			expectedXmlResult = @"
<DatiGenerali>
  <DatiGeneraliDocumento>
    <TipoDocumento>TD16</TipoDocumento>
    <Divisa>EUR</Divisa>
    <Data>2018-10-16</Data>
    <Numero>00005678</Numero>
    <ImportoTotaleDocumento>0.00</ImportoTotaleDocumento>
    <Causale>Documento Autofattura/Integrazione relativo a documento originario numero T001234 del 2018-10-15 in EUR al cambio 1.00 per totali EUR 0.00</Causale>
  </DatiGeneraliDocumento>
  <DatiFattureCollegate>
    <IdDocumento>T001234</IdDocumento>
    <Data>2018-10-15</Data>
  </DatiFattureCollegate>
</DatiGenerali>";
			AssertForExemptionDocument();

			void AssertForExemptionDocument()
			{
				var factory = new BusinessObjectFactory();
				var dgBuilder = new DatiGenerali(factory);

				var actualXmlResult = dgBuilder.BuildXML(transaction, null).ToString();
				XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);

				exporterExemptionDocument.DocumentTrackingAttributeCollection.Clear();
				transaction.SetOrganizationDocumentTrackingCollection(() => new List<DocumentTracking> { exporterExemptionDocument });
				actualXmlResult = dgBuilder.BuildXML(transaction, null).ToString();
				XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);

				transaction.OrganizationDocumentTrackingCollection.Clear();
				actualXmlResult = dgBuilder.BuildXML(transaction, null).ToString();
				XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
			}
		}

		TransactionInfo createARTransaction(string transRef = "00005678")
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.DataContext = DataContextFactory.New();
			transaction.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			transaction.Ledger = LedgerTypes.AccountsReceivable;
			transaction.TransactionType = TransactionType.INV;
			if (transRef != null)
			{
				transaction.TransactionReference = transRef;
			}
			transaction.Number = "00001234";
			transaction.OSCurrency = new Currency() { Code = CurrencyCodes.EuropeanUnion };
			transaction.ExchangeRate = 1m;
			transaction.LocalTotal = 0;
			transaction.OSTotal = 0;
			transaction.TransactionDate = new ZDateTime(2018, 10, 15);
			return transaction;
		}

		TransactionInfo createAPTransaction(string transRef = "00005678", string subType = null)
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.DataContext = DataContextFactory.New();
			transaction.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			transaction.Ledger = LedgerTypes.AccountsPayable;
			transaction.TransactionType = TransactionType.INV;
			if (transRef != null)
			{
				transaction.TransactionReference = transRef;
			}
			transaction.Number = "T001234";
			transaction.OSCurrency = new Currency() { Code = CurrencyCodes.EuropeanUnion };
			transaction.ExchangeRate = 1m;
			transaction.LocalTotal = 0;
			transaction.OSTotal = 0;
			transaction.TransactionDate = new ZDateTime(2018, 10, 15);
			transaction.PostDate = transaction.TransactionDate?.AddDays(1);
			if (subType != null)
			{
				transaction.ComplianceSubType = subType;
			}

			return transaction;
		}

		void setupOrgAddress(TransactionInfo transaction)
		{
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.OrganizationCode = new ZCodeMappedZString("OH1");
			transaction.OrganizationAddress.Country = new Country() { Code = CountryCodes.Italy };
		}

		Shipment createShipmentData(string shipmentNumber)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new DataContext()
				{
					DataSourceCollection = new List<DataSource>()
					{
						new DataSource() { Type = "ForwardingShipment", Key = shipmentNumber }
					}
				}
			};
			shipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.Instance);
			shipment.LocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>());

			AddOrderToShipment(shipment, "abc0123456");
			AddOrderToShipment(shipment, "xyz0123456789");

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			var ref1 = new AdditionalReference();
			ref1.Type = new EntryType();
			ref1.Type.Code = "CUP";
			ref1.ReferenceNumber = "CUP01234567890123456789";
			shipment.AdditionalReferenceCollection.Add(ref1);
			var ref2 = new AdditionalReference();
			ref2.Type = new EntryType();
			ref2.Type.Code = "CIG";
			ref2.ReferenceNumber = "CIG98765432109876543210";
			shipment.AdditionalReferenceCollection.Add(ref2);
			var ref3 = new AdditionalReference();
			ref3.Type = new EntryType();
			ref3.Type.Code = "CUP";
			ref3.ReferenceNumber = "CUP67890";
			shipment.AdditionalReferenceCollection.Add(ref3);
			var ref4 = new AdditionalReference();
			ref4.Type = new EntryType();
			ref4.Type.Code = "CIG";
			ref4.ReferenceNumber = "CIG09876";
			shipment.AdditionalReferenceCollection.Add(ref4);
			return shipment;
		}

		Shipment createShipmentDataWithoutOrderNumberButWithCigOrCup(string shipmentNumber, bool insertCig, bool insertCup)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new DataContext()
				{
					DataSourceCollection = new List<DataSource>()
					{
						new DataSource() { Type = "ForwardingShipment", Key = shipmentNumber }
					}
				}
			};
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());

			if (insertCup)
			{
				var ref1 = new AdditionalReference();
				ref1.Type = new EntryType();
				ref1.Type.Code = "CUP";
				ref1.ReferenceNumber = "CUP01234567890123456789";
				shipment.AdditionalReferenceCollection.Add(ref1);
			}

			if (insertCig)
			{
				var ref2 = new AdditionalReference();
				ref2.Type = new EntryType();
				ref2.Type.Code = "CIG";
				ref2.ReferenceNumber = "CIG98765432109876543210";
				shipment.AdditionalReferenceCollection.Add(ref2);
			}

			return shipment;
		}
	}
}
