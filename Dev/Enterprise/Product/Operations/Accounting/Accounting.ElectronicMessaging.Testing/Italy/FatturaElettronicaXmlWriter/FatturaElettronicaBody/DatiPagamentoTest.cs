using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.TestHelpers.Xml;
using static Enterprise.Accounting.Business.EInvoicing.Testing.FatturaElettronicaXmlValueFormatterTest;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	public class DatiPagamentoTest : TestCaseWithFactory
	{
		public void TestDatiPagamentoForSignsOnAmounts()
		{
			var multiplier = -1;
			var datiPagamento = new DatiPagamento();

			for (var i = 0; i < 4; i++)
			{
				multiplier *= -1;

				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				transaction.AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck;
				transaction.DueDate = new ZDateTime(2018, 05, 12);
				transaction.LocalTotal = multiplier * 120.52m;
				transaction.TransactionType = i < 2 ? TransactionType.CRD : TransactionType.ADJ;
				transaction.OSTotal = multiplier * 150m;

				var expectedSignMultiplier = (transaction.TransactionType == TransactionType.CRD || transaction.OSTotal < 0) ? -1 : 1;
				var expectedXml = $@"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP02</ModalitaPagamento>
    <DataScadenzaPagamento>2018-05-12</DataScadenzaPagamento>
    <ImportoPagamento>{FormatWithZerosAfterDecimalPoint(expectedSignMultiplier * transaction.LocalTotal.Value)}</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

				var actualXml = datiPagamento.BuildXML(transaction).ToString();
				XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
			}
		}

		[TestDate(2020, 10, 15)]
		public void TestAgreedPaymentMethod_GovernmentAllowedCodes()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2021, 01, 01)))
			{
				BodyTestAgreedPaymentMethod_GovernmentAllowedCodes(22);
			}
		}

		[TestDate(2020, 10, 15)]
		public void TestAgreedPaymentMethod_GovernmentAllowedCodesNewSchema()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2020, 10, 10)))
			{
				BodyTestAgreedPaymentMethod_GovernmentAllowedCodes(23);
			}
		}

		void BodyTestAgreedPaymentMethod_GovernmentAllowedCodes(int pMaxVal)
		{
			var expectedXml = ZString.Empty;
			var actualXml = ZString.Empty;
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.DueDate = new ZDateTime(2018, 05, 12);
			transaction.LocalTotal = 120.52m;
			var datiPagamento = new DatiPagamento();

			for (int i = -2; i < 25; i++)
			{
				transaction.AgreedPaymentMethod = string.Format("{0}{1}", i >= 0 && i <= 9 ? "0" : string.Empty, i.ToString());

				expectedXml = $@"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>{string.Format("MP{0}", i >= 1 && i <= pMaxVal ? string.Format("{0}{1}", i >= 0 && i <= 9 ? "0" : string.Empty, i.ToString()) : "05")}</ModalitaPagamento>
    <DataScadenzaPagamento>2018-05-12</DataScadenzaPagamento>
    <ImportoPagamento>120.52</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

				actualXml = datiPagamento.BuildXML(transaction).ToString();
				XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
			}

			for (int i = 0; i < 10; i++)
			{
				transaction.AgreedPaymentMethod = i.ToString();

				actualXml = datiPagamento.BuildXML(transaction).ToString();
				XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
			}

			transaction.AgreedPaymentMethod = "A1"; // Int32.TryParse returns false

			actualXml = datiPagamento.BuildXML(transaction).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		public void TestDatiPagamento_AgreedPayementTermIsCBC()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck;
			transaction.DueDate = new ZDateTime(2018, 05, 12);
			transaction.LocalTotal = 120.52m;

			var expectedXml = @"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP02</ModalitaPagamento>
    <DataScadenzaPagamento>2018-05-12</DataScadenzaPagamento>
    <ImportoPagamento>120.52</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

			var actualXml = new DatiPagamento().BuildXML(transaction).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		public void TestDatiPagamento_AgreedPayementTermIsCHK()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
			transaction.DueDate = new ZDateTime(2018, 05, 12);
			transaction.LocalTotal = 120.52m;

			var expectedXml = @"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP02</ModalitaPagamento>
    <DataScadenzaPagamento>2018-05-12</DataScadenzaPagamento>
    <ImportoPagamento>120.52</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

			var actualXml = new DatiPagamento().BuildXML(transaction).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		public void TestDatiPagamento_AgreedPayementTermIsTRF()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer;
			transaction.DueDate = new ZDateTime(2018, 05, 12);
			transaction.LocalTotal = 120.52m;

			var expectedXml = @"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP05</ModalitaPagamento>
    <DataScadenzaPagamento>2018-05-12</DataScadenzaPagamento>
    <ImportoPagamento>120.52</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

			var actualXml = new DatiPagamento().BuildXML(transaction).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		public void TestDatiPagamento_AgreedPayementTermIsCCD()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard;
			transaction.DueDate = new ZDateTime(2018, 05, 12);
			transaction.LocalTotal = 120.52m;

			var expectedXml = @"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP08</ModalitaPagamento>
    <DataScadenzaPagamento>2018-05-12</DataScadenzaPagamento>
    <ImportoPagamento>120.52</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

			var actualXml = new DatiPagamento().BuildXML(transaction).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		public void TestDatiPagamento_AgreedPayementTermIsCRQ()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest;
			transaction.DueDate = new ZDateTime(2018, 05, 12);
			transaction.LocalTotal = 120.52m;

			var expectedXml = @"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP12</ModalitaPagamento>
    <DataScadenzaPagamento>2018-05-12</DataScadenzaPagamento>
    <ImportoPagamento>120.52</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

			var actualXml = new DatiPagamento().BuildXML(transaction).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		public void TestDatiPagamento_AgreedPayementTermIsDBC()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard;
			transaction.DueDate = new ZDateTime(2018, 05, 12);
			transaction.LocalTotal = 120.52m;

			var expectedXml = @"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP08</ModalitaPagamento>
    <DataScadenzaPagamento>2018-05-12</DataScadenzaPagamento>
    <ImportoPagamento>120.52</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

			var actualXml = new DatiPagamento().BuildXML(transaction).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		public void TestDatiPagamento_AgreedPayementTermIsAUserCustomizedValue()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.AgreedPaymentMethod = "AAA";
			transaction.DueDate = new ZDateTime(2018, 05, 12);
			transaction.LocalTotal = 120.52m;

			var expectedXml = @"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP05</ModalitaPagamento>
    <DataScadenzaPagamento>2018-05-12</DataScadenzaPagamento>
    <ImportoPagamento>120.52</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

			var actualXml = new DatiPagamento().BuildXML(transaction).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		public void TestDatiPagamento_AgreedPayementTermIsNotSet()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.DueDate = new ZDateTime(2018, 05, 12);
			transaction.LocalTotal = 120.52m;

			var expectedXml = @"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento></ModalitaPagamento>
    <DataScadenzaPagamento>2018-05-12</DataScadenzaPagamento>
    <ImportoPagamento>120.52</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

			var actualXml = new DatiPagamento().BuildXML(transaction).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		public void TestDatiPagamento_DueDateIsEmpty()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck;
			transaction.LocalTotal = 120.52m;

			var expectedXml = @"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP02</ModalitaPagamento>
    <DataScadenzaPagamento></DataScadenzaPagamento>
    <ImportoPagamento>120.52</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

			var actualXml = new DatiPagamento().BuildXML(transaction).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		public void TestDatiPagamento_LocalTotalIsEmpty()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck;
			transaction.DueDate = new ZDateTime(2018, 05, 12);

			var expectedXml = @"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP02</ModalitaPagamento>
    <DataScadenzaPagamento>2018-05-12</DataScadenzaPagamento>
    <ImportoPagamento />
  </DettaglioPagamento>
</DatiPagamento>";

			var actualXml = new DatiPagamento().BuildXML(transaction).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		public void TestDatiPagamento_LocalTotalWithDifferentDecimalPlaces()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck;
			transaction.DueDate = new ZDateTime(2018, 05, 12);
			transaction.LocalTotal = 120m;

			var expectedXml = @"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP02</ModalitaPagamento>
    <DataScadenzaPagamento>2018-05-12</DataScadenzaPagamento>
    <ImportoPagamento>120.00</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

			var actualXml = new DatiPagamento().BuildXML(transaction).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);

			transaction.LocalTotal = 120.2m;

			expectedXml = @"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP02</ModalitaPagamento>
    <DataScadenzaPagamento>2018-05-12</DataScadenzaPagamento>
    <ImportoPagamento>120.20</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

			actualXml = new DatiPagamento().BuildXML(transaction).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);

			transaction.LocalTotal = 120.234m;

			expectedXml = @"
<DatiPagamento>
  <CondizioniPagamento>TP02</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP02</ModalitaPagamento>
    <DataScadenzaPagamento>2018-05-12</DataScadenzaPagamento>
    <ImportoPagamento>120.234</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

			actualXml = new DatiPagamento().BuildXML(transaction).ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		[TestDate(2022, 5, 27)]
		public void TestDatiPagamento_MultipleInstallments()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			CreateMultipleInstalments(testObjectCreator, arInvoice);
			Factory.Save();

			var batchAR = testObjectCreator.CreateEInvoicingBatch(100, Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivotAR = testObjectCreator.CreateEInvoicingTransactionPivot(batchAR, arInvoice, Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var accRegistry = AccountingConfigurationRegistry.Instance;
			using (accRegistry.ARSuspenseControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.CreateARSuspenseControlAccount().PK.ToGuid()))
			using (accRegistry.APSuspenseControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.CreateAPSuspenseControlAccount().PK.ToGuid()))
			using (accRegistry.JobRevenueJournalControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.CreateJobRevenueJournalControlAccount().PK.ToGuid()))
			{
				var dataAccessAR = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
				var exporterAR = new TransactionBatchExporter(dataAccessAR, PopulateOptionalXUTFieldsSetting.AllTrue());
				var transactionBatchAR = exporterAR.CreateTransactionBatch(batchAR);

				var transaction = transactionBatchAR.TransactionCollection.FirstOrDefault();
				AssertNotNull(transaction);

				var expectedXml = @"
<DatiPagamento>
  <CondizioniPagamento>TP01</CondizioniPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP12</ModalitaPagamento>
    <DataScadenzaPagamento>2022-05-31</DataScadenzaPagamento>
    <ImportoPagamento>40.73</ImportoPagamento>
  </DettaglioPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP12</ModalitaPagamento>
    <DataScadenzaPagamento>2022-06-30</DataScadenzaPagamento>
    <ImportoPagamento>40.73</ImportoPagamento>
  </DettaglioPagamento>
  <DettaglioPagamento>
    <ModalitaPagamento>MP05</ModalitaPagamento>
    <DataScadenzaPagamento>2022-07-30</DataScadenzaPagamento>
    <ImportoPagamento>40.75</ImportoPagamento>
  </DettaglioPagamento>
</DatiPagamento>";

				var actualXml = new DatiPagamento().BuildXML(transaction).ToString();
				AssertMultilineASCIIEquals(expectedXml, actualXml);
			}
		}

		void CreateMultipleInstalments(TestObjectCreator testObjectCreator, InvoicingBase arInvoice)
		{
			var arTerms = testObjectCreator.AALSHI.CompanyData.ARTerms;
			AssertNotNull(arTerms);

			var arTermMLI = arTerms.AddNew();
			arTermMLI.PY_JobType = "ALL";
			arTermMLI.PY_GB_Branch = arInvoice.AH_GB;
			arTermMLI.PY_GE_Department = arInvoice.AH_GE;
			arTermMLI.PY_Direction = "ALL";
			arTermMLI.PY_TransportMode = "ALL";
			arTermMLI.PY_InvoiceClass = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			arTermMLI.PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			var arTermMLInstalments = arTermMLI.ARTermsInstallments;
			AssertNotNull(arTermMLInstalments);

			var installment1 = arTermMLInstalments.AddNew();
			installment1.ML_SequenceNumber = 1;
			installment1.ML_SplitPercentage = 33.33;
			installment1.ML_DaysFromInvoiceDate = 30;
			installment1.ML_AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest;

			var installment2 = arTermMLInstalments.AddNew();
			installment2.ML_SequenceNumber = 2;
			installment2.ML_SplitPercentage = 33.33;
			installment2.ML_DaysFromInvoiceDate = 60;
			installment2.ML_AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest;

			var installment3 = arTermMLInstalments.AddNew();
			installment3.ML_SequenceNumber = 3;
			installment3.ML_SplitPercentage = 33.34;
			installment3.ML_DaysFromInvoiceDate = 90;
			installment3.ML_AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer;

			var today = ZDateTime.Today;
			arInvoice.AH_InvoiceDate = new ZDateTime(today.Year, today.Month, 1);
			arInvoice.AH_PostDate = today;
			arInvoice.AH_DueDate = today.AddMonths(1);

			arInvoice.AH_OH = testObjectCreator.AALSHI.PK;
			arInvoice.AH_TransactionNum = "00001000";
			var currency = testObjectCreator.EUR;
			arInvoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			arInvoice.AH_ExchangeRate = 1;
			arInvoice.AH_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			arInvoice.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest;

			var line = arInvoice.Lines.AddNew();
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_AG = testObjectCreator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 111.10;
			line.AL_OSTaxAmount = 11.11;
		}
	}
}
