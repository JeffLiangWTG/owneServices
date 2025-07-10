using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance.Argentina;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Accounting.TaxFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using WTG.TestHelpers.Xml;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	class ComprobanteCAERequestBuilderTest : TestCaseWithFactory
	{
		#region DatosAdicionales MiPymes

		public void TestDatosAdicionales_ComplianceSubTypeIsNullEmptyOrIsNotMiPyme()
		{
			var transactionInfo = CreateTransactionInfo(TransactionType.INV);

			CombineAssertions(() =>
			{
				transactionInfo.ComplianceSubType = null;
				AssertDatosAdicionalesMissing();

				transactionInfo.ComplianceSubType = "";
				AssertDatosAdicionalesMissing();

				transactionInfo.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
				AssertDatosAdicionalesMissing();

				void AssertDatosAdicionalesMissing()
				{
					var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
					var actualResult = builder.BuildXML(transactionInfo, Factory).ToString().Replace(" ", "");
					AssertNotContains("The arrayDatosAdicionales node should not be include in the Xml.", "<arrayDatosAdicionales>", actualResult);
				}
			});
		}

		public void TestDatosAdicionales_ComplianceSubTypeIsMiPymeDebitOrCreditNote()
		{
			var transactionInfo = CreateTransactionInfo(TransactionType.CRD);
			transactionInfo.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA;
			transactionInfo.OriginalReference = null;
			AssertDatosAdicionales("N");

			transactionInfo.OriginalReference = new OriginalReference();
			AssertDatosAdicionales("N");

			transactionInfo.OriginalReference.OriginalTransactionAmendingReversingReason = new CodeDescriptionPair();
			AssertDatosAdicionales("N");

			transactionInfo.OriginalReference.OriginalTransactionAmendingReversingReason.Code = null;
			AssertDatosAdicionales("N");

			transactionInfo.OriginalReference.OriginalTransactionAmendingReversingReason.Code = ZString.Empty;
			AssertDatosAdicionales("N");

			transactionInfo.OriginalReference.OriginalTransactionAmendingReversingReason.Code = "TXT";
			AssertDatosAdicionales("N");

			transactionInfo.OriginalReference.OriginalTransactionAmendingReversingReason.Code = "MIR";
			AssertDatosAdicionales("S");

			void AssertDatosAdicionales(string expectedNodeValue)
			{
				var expectedResult = $@"<arrayDatosAdicionales>
<datoAdicional>
<t>22</t>
<c1>{expectedNodeValue}</c1>
</datoAdicional>
</arrayDatosAdicionales>";

				var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
				var actualResult = builder.BuildXML(transactionInfo, Factory).ToString().Replace(" ", "");
				AssertContains($"For MiPymes Debit and Credit notes the Xml must contain the <arrayDatosAdicionales> node with <datoAdicional> sub node where <t>22</t> and <c1>{expectedNodeValue}</c1>.", expectedResult, actualResult);
			}
		}

		public void TestDatosAdicionales_ComplianceSubTypeIsMiPymeInvoice_UniqueAccountNumberIsEmpty()
		{
			// This test will not be neccesary when ArgentinaEInvoiceHelper is a DatosAdicionales.cs's dependency

			var (branch, bank, organization) = TestData();

			var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;

			var transactionInfo = CreateTransactionInfo(TransactionType.INV);
			transactionInfo.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA;
			transactionInfo.OrganizationAddress = null;
			AssertDatosAdicionales();

			transactionInfo.OrganizationAddress = new OrganizationAddress();
			transactionInfo.OrganizationAddress.OrganizationCode = null;
			AssertDatosAdicionales();

			transactionInfo.OrganizationAddress.OrganizationCode = ZString.Empty;
			AssertDatosAdicionales();

			transactionInfo.OrganizationAddress.OrganizationCode = organization.OH_Code;
			transactionInfo.Branch = null;
			AssertDatosAdicionales();

			transactionInfo.Branch = new Branch();
			AssertDatosAdicionales();

			transactionInfo.Branch.Code = ZString.Empty;
			AssertDatosAdicionales();

			transactionInfo.Branch.Code = branch.GB_Code;
			transactionInfo.OSCurrency = new Currency() { Code = "ARS" };
			AssertDatosAdicionales();

			bank.AB_IsDefaultReceiptBankAccount = true;
			bank.AB_FullAccountNumber = "";
			Factory.Save();
			AssertDatosAdicionales();

			void AssertDatosAdicionales()
			{
				var expectedResult = $@"<arrayDatosAdicionales>
<datoAdicional>
<t>21</t>
<c1></c1>
</datoAdicional>
<datoAdicional>
<t>27</t>
<c1>ADC</c1>
</datoAdicional>
</arrayDatosAdicionales>";

				var actualResult = builder.BuildXML(transactionInfo, Factory).ToString().Replace(" ", "");

				AssertContains($"For MiPymes Invoices the Xml must contain the <arrayDatosAdicionales> node with a <datoAdicional> sub node where <t>21</t> and <c1></c1> and another <datoAdicional> sub node where <t>27</t> and <c1>ADC</c1>.", expectedResult, actualResult);
			}
		}

		public void TestDatosAdicionales_ComplianceSubTypeIsMiPymeInvoice_UniqueAccountNumberHasValue()
		{
			var (branch, bank, organization) = TestData();
			bank.AB_IsDefaultReceiptBankAccount = true;
			bank.AB_FullAccountNumber = "1111111199999999988888";
			Factory.Save();

			var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;

			var transactionInfo = CreateTransactionInfo(TransactionType.INV);
			transactionInfo.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA;
			transactionInfo.OrganizationAddress = new OrganizationAddress() { OrganizationCode = organization.OH_Code };
			transactionInfo.Branch = new Branch() { Code = branch.GB_Code };
			transactionInfo.OSCurrency = new Currency() { Code = "ARS" };

			var expectedResult = $@"<arrayDatosAdicionales>
<datoAdicional>
<t>21</t>
<c1>1111111199999999988888</c1>
</datoAdicional>
<datoAdicional>
<t>27</t>
<c1>ADC</c1>
</datoAdicional>
</arrayDatosAdicionales>";

			var actualResult = builder.BuildXML(transactionInfo, Factory).ToString().Replace(" ", "");

			AssertContains($"For MiPymes Invoices the Xml must contain the <arrayDatosAdicionales> node with a <datoAdicional> sub node where <t>21</t> and <c1>1111111199999999988888</c1> and another <datoAdicional> sub node where <t>27</t> and <c1>ADC</c1>.", expectedResult, actualResult);
		}

		(GlbBranch branch, AccBankAccount bank, OrgHeader organization) TestData()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BR1";

			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = "AR";
			glbCompany.GC_RX_NKLocalCurrency = "ARS";
			glbCompany.Branches.Add(branch);

			var bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_GB = branch.PK;
			bank.AB_GC = branch.Company.PK;
			bank.AB_RX_NKAccountCurrency = branch.Company.GC_RX_NKLocalCurrency;
			bank.AB_IsActive = true;

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "Debtor";

			Factory.Save();

			return (branch, bank, organization);
		}

		TransactionInfo CreateTransactionInfo(TransactionType transactionType)
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionType = transactionType;
			return transaction;
		}

		#endregion

		#region Items

		public void TestComprobanteCAERequestBuilderItemsDetailCheckNulls()
		{
			var builder = (IComprobanteCAERequestBuilder)new ComprobanteCAERequestBuilder();

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			AssertNulls();

			transaction.TransactionType = TransactionType.INV;
			AssertNulls();

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			AssertNulls();

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance));
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].ChargeCode = new ChargeCode();
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].GLAccount = new GLAccount();
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].GovernmentReportingChargeCode = null;
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].GovernmentReportingChargeCode = ZString.Empty;
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].Description = null;
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].Description = ZString.Empty;
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].OSAmount = null;
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].OSAmount = 0m;
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].OSTotalAmount = null;
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].OSTotalAmount = 0m;
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].OSGSTVATAmount = null;
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].OSGSTVATAmount = 0m;
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].TaxMessageID = new TaxMessageID();
			AssertNullsWithChargeLines();

			void AssertNulls()
			{
				var expectedValue = @"<arrayItems>";
				var actualXmlValue = builder.BuildXML(transaction, Factory).ToString();

				AssertNotContains("Items sub node must not be present in actualXmlValue when there are not items", expectedValue, actualXmlValue);
			}

			void AssertNullsWithChargeLines()
			{
				var expectedValue = @"  <arrayItems>
    <item>
      <unidadesMtx>1</unidadesMtx>
      <codigoMtx></codigoMtx>
      <codigo></codigo>
      <descripcion></descripcion>
      <cantidad>1</cantidad>
      <codigoUnidadMedida>7</codigoUnidadMedida>
      <precioUnitario>0.000000</precioUnitario>
      <codigoCondicionIVA />
      <importeItem>0.00</importeItem>
    </item>
  </arrayItems>";

				AssertItems(transaction, expectedValue);
			}
		}

		public void TestComprobanteCAERequestBuilderItemsDetailWithSinglePostingJournal_TransactionType_INV()
		{
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ChargeCode = new ChargeCode() { ChargeType = new CodeDescriptionPair() { Code = "CC1" } },
				GovernmentReportingChargeCode = "7790001001078",
				Description = "DECLARATION FEE",
				OSAmount = 100m,
				OSGSTVATAmount = -200m,
				TaxMessageID = new TaxMessageID() { TaxGroupCode = new TaxGroupCodeType() { Code = "XXX" } }
			});

			var expectedValue = @"  <arrayItems>
    <item>
      <unidadesMtx>1</unidadesMtx>
      <codigoMtx>7790001001078</codigoMtx>
      <codigo></codigo>
      <descripcion>DECLARATION FEE</descripcion>
      <cantidad>1</cantidad>
      <codigoUnidadMedida>7</codigoUnidadMedida>
      <precioUnitario>100.000000</precioUnitario>
      <codigoCondicionIVA>XXX</codigoCondicionIVA>
      <importeIVA>-200.00</importeIVA>
      <importeItem>0.00</importeItem>
    </item>
  </arrayItems>";

			AssertItems(transaction, expectedValue);
		}

		public void TestComprobanteCAERequestBuilderItemsDetailWithSinglePostingJournal_TransactionType_CRD()
		{
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			transaction.TransactionType = TransactionType.CRD;

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ChargeCode = new ChargeCode() { ChargeType = new CodeDescriptionPair() { Code = "CC1" } },
				GovernmentReportingChargeCode = "7790001001078",
				Description = "DECLARATION FEE",
				OSAmount = 100m,
				OSGSTVATAmount = -200m,
				TaxMessageID = new TaxMessageID() { TaxGroupCode = new TaxGroupCodeType() { Code = "XXX" } }
			});

			var expectedValue = @" <arrayItems>
    <item>
      <unidadesMtx>1</unidadesMtx>
      <codigoMtx>7790001001078</codigoMtx>
      <codigo></codigo>
      <descripcion>DECLARATION FEE</descripcion>
      <cantidad>1</cantidad>
      <codigoUnidadMedida>7</codigoUnidadMedida>
      <precioUnitario>-100.000000</precioUnitario>
      <codigoCondicionIVA>XXX</codigoCondicionIVA>
      <importeIVA>200.00</importeIVA>
      <importeItem>0.00</importeItem>
    </item>
  </arrayItems>";

			AssertItems(transaction, expectedValue);
		}

		public void TestComprobanteCAERequestBuilderItemsDetailWithOneChargeCodeCMT()
		{
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ChargeCode = new ChargeCode() { ChargeType = new CodeDescriptionPair() { Code = "CMT" } },
			});

			var expectedValue = @"<arrayItems>";

			var builder = (IComprobanteCAERequestBuilder)new ComprobanteCAERequestBuilder();

			var actualXmlValue = builder.BuildXML(transaction, Factory).ToString();

			AssertNotContains("Items sub node must not be present in actualXmlValue when there are not items", expectedValue, actualXmlValue);
		}

		public void TestComprobanteCAERequestBuilderItemsDetailWithMultiplePostingJournalWithChargeCodeAndCMT()
		{
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ChargeCode = new ChargeCode() { ChargeType = new CodeDescriptionPair() { Code = "CMT" } },
			});
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ChargeCode = new ChargeCode() { ChargeType = new CodeDescriptionPair() { Code = "CC1" }, Code = new ZCodeMappedZString("OH1") },
				GovernmentReportingChargeCode = "1234",
				OSAmount = 890.5600m,
			});
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				GLAccount = new GLAccount() { AccountCode = "6051.10.21" },
				GovernmentReportingChargeCode = "12345678901234567",
				OSAmount = 890.99999999m,
			});

			var expectedValue = @"  <arrayItems>
    <item>
      <unidadesMtx>1</unidadesMtx>
      <codigoMtx>1234</codigoMtx>
      <codigo>OH1</codigo>
      <descripcion></descripcion>
      <cantidad>1</cantidad>
      <codigoUnidadMedida>7</codigoUnidadMedida>
      <precioUnitario>890.560000</precioUnitario>
      <codigoCondicionIVA />
      <importeIVA>0.00</importeIVA>
      <importeItem>0.00</importeItem>
    </item>
    <item>
      <unidadesMtx>1</unidadesMtx>
      <codigoMtx>1234567890123</codigoMtx>
      <codigo>6051.10.21</codigo>
      <descripcion></descripcion>
      <cantidad>1</cantidad>
      <codigoUnidadMedida>7</codigoUnidadMedida>
      <precioUnitario>891.000000</precioUnitario>
      <codigoCondicionIVA />
      <importeIVA>0.00</importeIVA>
      <importeItem>0.00</importeItem>
    </item>
  </arrayItems>";

			AssertItems(transaction, expectedValue);
		}

		public void TestComprobanteCAERequestBuilderItemsDetailWithMultiplePostingJournalWithClassAMComplianceSubTypeList()
		{
			var transaction = GetTransactionInfo();
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OSAmount = 100m,
				OSTotalAmount = 200m,
				OSGSTVATAmount = 300m,
			});

			var expectedValueClassAMComplianceSubType = @"  <arrayItems>
    <item>
      <unidadesMtx>1</unidadesMtx>
      <codigoMtx></codigoMtx>
      <codigo></codigo>
      <descripcion></descripcion>
      <cantidad>1</cantidad>
      <codigoUnidadMedida>7</codigoUnidadMedida>
      <precioUnitario>100.000000</precioUnitario>
      <codigoCondicionIVA />
      <importeIVA>300.00</importeIVA>
      <importeItem>200.00</importeItem>
    </item>
  </arrayItems>";

			foreach (var subType in ArgentinaConstants.ClassAMComplianceSubTypeList)
			{
				transaction.ComplianceSubType = subType;

				AssertItems(transaction, expectedValueClassAMComplianceSubType);
			}

			transaction.ComplianceSubType = "XXX";
			var expectedValueNotClassAMComplianceSubType = @"  <arrayItems>
    <item>
      <unidadesMtx>1</unidadesMtx>
      <codigoMtx></codigoMtx>
      <codigo></codigo>
      <descripcion></descripcion>
      <cantidad>1</cantidad>
      <codigoUnidadMedida>7</codigoUnidadMedida>
      <precioUnitario>200.000000</precioUnitario>
      <codigoCondicionIVA />
      <importeItem>200.00</importeItem>
    </item>
  </arrayItems>";

			AssertItems(transaction, expectedValueNotClassAMComplianceSubType);
		}

		void AssertItems(TransactionInfo transaction, string expectedValue)
		{
			var builder = (IComprobanteCAERequestBuilder)new ComprobanteCAERequestBuilder();

			var actualXmlValue = builder.BuildXML(transaction, Factory).ToString();

			AssertContains(expectedValue, actualXmlValue);
		}

		#endregion

		#region SubTotalesIva

		public void TestSubTotalesIVA_PostingJornalCollectionNullAndEmpty()
		{
			var transaction = CreateTransactionInfo(TransactionType.INV, false);
			AssertNullSubTotalesIVA(transaction);

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			AssertNullSubTotalesIVA(transaction);
		}

		public void TestSubTotalesIVA_PostingJornalEmpty()
		{
			var transaction = CreateTransactionInfo(TransactionType.INV, true);
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance));
			AssertNullSubTotalesIVA(transaction);
		}

		public void TestSubTotalesIVA_VATTaxIDNullAndEmpty()
		{
			var transaction = CreateTransactionInfo(TransactionType.INV, true);
			transaction.PostingJournalCollection.Add(AddTransactionLine(1200m, 252m, 1452m, 1200m, 252m, 1452m, null));
			AssertNullSubTotalesIVA(transaction);

			transaction.PostingJournalCollection[0].VATTaxID = new TaxID();
			AssertNullSubTotalesIVA(transaction);
		}

		public void TestSubTotalesIVA_VATTaxID_TaxTypeNullAndEmpty()
		{
			var transaction = CreateTransactionInfo(TransactionType.INV, true);
			transaction.PostingJournalCollection.Add(AddTransactionLine(1200m, 252m, 1452m, 1200m, 252m, 1452m, AddTaxID("IVA", 21, null)));
			AssertNullSubTotalesIVA(transaction);

			transaction.PostingJournalCollection[0].VATTaxID.TaxType = new CodeDescriptionPair();
			AssertNullSubTotalesIVA(transaction);
		}

		public void TestSubTotalesIVA_TaxMessageIDNullAndEmpty()
		{
			var transaction = CreateTransactionInfo(TransactionType.INV, true);
			transaction.PostingJournalCollection.Add(AddTransactionLine(1200m, 252m, 1452m, 1200m, 252m, 1452m, AddTaxID("IVA", 21m, "RAT")));
			transaction.PostingJournalCollection[0].TaxMessageID = null;
			AssertNullSubTotalesIVA(transaction);

			transaction.PostingJournalCollection[0].TaxMessageID = new TaxMessageID();
			AssertNullSubTotalesIVA(transaction);
		}

		public void TestSubTotalesIVA_TaxGroupCodeNullAndEmpty()
		{
			var transaction = CreateTransactionInfo(TransactionType.INV, true);
			transaction.PostingJournalCollection.Add(AddTransactionLine(1200m, 252m, 1452m, 1200m, 252m, 1452m, AddTaxID("IVA", 21m, "RAT"), AddTaxMessageID(null, null)));
			AssertNullSubTotalesIVA(transaction);

			transaction.PostingJournalCollection[0].TaxMessageID.TaxMessageCode = "IVA21";
			transaction.PostingJournalCollection[0].TaxMessageID.TaxGroupCode = new TaxGroupCodeType();
			AssertNullSubTotalesIVA(transaction);
		}

		public void TestSubTotalesIVA_WithTaxGroupCodeMatchesWith_4_5_6_INV()
		{
			var transaction = CreateTransactionInfo(TransactionType.INV, true);

			transaction.PostingJournalCollection.Add(AddTransactionLine(1200m, 126m, 1326m, 1200m, 126m, 1326m, AddTaxID("IVA10.5", 10.5m, "RAT"), AddTaxMessageID("IVA10.5", "4")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(58.36m, 12.26m, 70.62m, 58.36m, 12.26m, 70.62m, AddTaxID("IVA21", 21m, "RAT"), AddTaxMessageID("IVA21", "5")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(582.74m, 157.34m, 740.08m, 582.74m, 157.34m, 740.08m, AddTaxID("IVA27", 27m, "RAT"), AddTaxMessageID("IVA27", "6")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(-39.20m, -4.12m, -43.32m, -39.20m, -4.12m, -43.32m, AddTaxID("IVA10.5", 10.5m, "RAT"), AddTaxMessageID("IVA10.5", "4")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(1200m, 0m, 1200m, 1200m, 0m, 1200m, AddTaxID("FREEIVA", 0m, "RAT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(100.16m, 21.03m, 121.19m, 100.16m, 21.03m, 121.19m, AddTaxID("CAPIVA", 21m, "CAP"), AddTaxMessageID("IVA21", "5")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(698.75m, 0m, 698.75m, 698.75m, 0m, 698.75m, AddTaxID("EXCLUDE", 0m, "EXL"), AddTaxMessageID("NOGRAVADO", "1")));

			var expectedResult = $@"
<arraySubtotalesIVA>
<subtotalIVA>
<codigo>4</codigo>
<importe>121.88</importe>
</subtotalIVA>
<subtotalIVA>
<codigo>5</codigo>
<importe>33.29</importe>
</subtotalIVA>
<subtotalIVA>
<codigo>6</codigo>
<importe>157.34</importe>
</subtotalIVA>
</arraySubtotalesIVA>";

			AssertSubTotalesIVA(transaction, expectedResult);
		}

		public void TestSubTotalesIVA_WithTaxGroupCodeMatchesWith_4_5_6_CRD()
		{
			var transaction = CreateTransactionInfo(TransactionType.CRD, true);

			transaction.PostingJournalCollection.Add(AddTransactionLine(-1200m, -126m, -1326m, -1200m, -126m, -1326m, AddTaxID("IVA10.5", 10.5m, "RAT"), AddTaxMessageID("IVA10.5", "4")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(-58.36m, -12.26m, -70.62m, -58.36m, -12.26m, -70.62m, AddTaxID("IVA21", 21m, "RAT"), AddTaxMessageID("IVA21", "5")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(-582.74m, -157.34m, -740.08m, -582.74m, -157.34m, -740.08m, AddTaxID("IVA27", 27m, "RAT"), AddTaxMessageID("IVA27", "6")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(39.20m, 4.12m, 43.32m, 39.20m, 4.12m, 43.32m, AddTaxID("IVA10.5", 10.5m, "RAT"), AddTaxMessageID("IVA10.5", "4")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(-1200m, 0m, -1200m, -1200m, 0m, -1200m, AddTaxID("FREEIVA", 0m, "RAT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(-100.16m, -21.03m, -121.19m, -100.16m, -21.03m, -121.19m, AddTaxID("CAPIVA", 21m, "CAP"), AddTaxMessageID("IVA21", "5")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(-698.75m, 0m, -698.75m, -698.75m, 0m, -698.75m, AddTaxID("EXCLUDE", 0m, "EXL"), AddTaxMessageID("NOGRAVADO", "1")));

			var expectedResult = $@"
<arraySubtotalesIVA>
<subtotalIVA>
<codigo>4</codigo>
<importe>121.88</importe>
</subtotalIVA>
<subtotalIVA>
<codigo>5</codigo>
<importe>33.29</importe>
</subtotalIVA>
<subtotalIVA>
<codigo>6</codigo>
<importe>157.34</importe>
</subtotalIVA>
</arraySubtotalesIVA>";

			AssertSubTotalesIVA(transaction, expectedResult);
		}

		public void TestSubTotalesIVA_WithTaxGroupCodeNotMatchWith_4_5_6()
		{
			var transaction = CreateTransactionInfo(TransactionType.INV, true);
			transaction.PostingJournalCollection.Add(AddTransactionLine(100m, 21m, 121m, 100m, 21m, 121m, AddTaxID("IVA21", 21m, "RAT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(360m, 0m, 360m, 360m, 0m, 360m, AddTaxID("EXEMPT", 0m, "EXT"), AddTaxMessageID("EXENTO", "2")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(1212.95m, 0m, 1212.95m, 1212.95m, 0m, 1212.95m, AddTaxID("FREEIVA", 0m, "RAT"), AddTaxMessageID("IVA0", "3")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(698.75m, 0m, 698.75m, 698.75m, 0m, 698.75m, AddTaxID("EXCLUDE", 0m, "EXL"), AddTaxMessageID("NOGRAVADO", "1")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(100m, 10.5m, 110.5m, 100m, 10.5m, 110.5m, AddTaxID("IVA10.5", 10.5m, "RAT")));

			var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
			var actualResult = builder.BuildXML(transaction, Factory).ToString().Replace(" ", "");

			AssertNotContains("The <arraySubtotalesIVA> node should not be included in the Xml", "<arraySubtotalesIVA>", actualResult);
		}

		void AssertSubTotalesIVA(TransactionInfo transaction, string expectedResult)
		{
			var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;

			var actualResult = builder.BuildXML(transaction, Factory).ToString().Replace(" ", "");

			AssertContains("The result returned for BuildXML must contain the expectedResult", expectedResult, actualResult);
		}

		void AssertNullSubTotalesIVA(TransactionInfo transaction)
		{
			var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
			var actualResult = builder.BuildXML(transaction, Factory).ToString().Replace(" ", "");

			AssertNotContains("The result returned for BuildXML must contain the expectedResult", "<arraySubtotalesIVA>", actualResult);
		}

		#endregion

		#region Setup

		PostingJournal AddTransactionLine(ZDecimal osAmount, ZDecimal osGSTVATAmount, ZDecimal osTotalAmount, ZDecimal localAmount, ZDecimal localGSTVATAmount, ZDecimal localTotalAmount, TaxID taxID = null, TaxMessageID taxMessageID = null)
		{
			var transactionLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OSAmount = osAmount,
				OSGSTVATAmount = osGSTVATAmount,
				OSTotalAmount = osTotalAmount,
				LocalAmount = localAmount,
				LocalGSTVATAmount = localGSTVATAmount,
				LocalTotalAmount = localTotalAmount
			};

			if (taxID != null)
			{
				transactionLine.VATTaxID = taxID;
			}

			if (taxMessageID != null)
			{
				transactionLine.TaxMessageID = taxMessageID;
			}

			return transactionLine;
		}

		TaxMessageID AddTaxMessageID(ZString? taxMessageCode, ZString? taxGroupCode)
		{
			var taxMessageID = new TaxMessageID();
			if (taxMessageCode.HasValue)
			{
				taxMessageID.TaxMessageCode = taxMessageCode;
			}

			if (taxGroupCode.HasValue)
			{
				taxMessageID.TaxGroupCode = new TaxGroupCodeType() { Code = taxGroupCode };
			}

			return taxMessageID;
		}

		TransactionInfo CreateTransactionInfo(TransactionType transactionType, bool setPostingJournalCollection)
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionType = transactionType;
			transaction.SetPostingJournalCollection(() => setPostingJournalCollection ? new List<PostingJournal>() : null);
			return transaction;
		}

		#endregion

		#region OtrosTributos

		public void TestOtrosTributos_NodeIsNotAdded()
		{
			CombineAssertions(() =>
			{
				var exceptionBuilder = (IComprobanteCAERequestBuilder)new ComprobanteCAERequestBuilder();
				AssertExceptionThrown<NullReferenceException>(() => exceptionBuilder.BuildXML(null, Factory));

				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				AssertOtrosTributosIsNotPresent(transaction);

				transaction.OSTaxTransactionsAmount = null;
				AssertOtrosTributosIsNotPresent(transaction);

				transaction.OSTaxTransactionsAmount = 0m;
				AssertOtrosTributosIsNotPresent(transaction);

				transaction.OSTaxTransactionsAmount = 10m;
				transaction.SetTaxTransactionCollection(() => null);
				AssertOtrosTributosIsNotPresent(transaction);

				transaction.SetTaxTransactionCollection(() => new List<TaxTransaction>());
				AssertOtrosTributosIsNotPresent(transaction);

				var taxTransaction1 = new TaxTransaction(DefaultDataObjectWriterStrategy.TestInstance);
				taxTransaction1.OSTaxBase = 1255m;
				taxTransaction1.OSTaxAmount = 18.33m;
				taxTransaction1.TaxSuperType = new CodeDescriptionPair()
				{
					Code = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.RetentionInInvoice.Code,
					Description = "NOT_PERCEPTIONS"
				};
				transaction.TaxTransactionCollection.Add(taxTransaction1);
				AssertOtrosTributosIsNotPresent(transaction);

				void AssertOtrosTributosIsNotPresent(TransactionInfo transactionInfo)
				{
					var builder = (IComprobanteCAERequestBuilder)new ComprobanteCAERequestBuilder();
					var actualXmlValue = builder.BuildXML(transactionInfo, Factory).ToString().Replace(" ", "");
					AssertNotContains("<arrayOtrosTributos>", actualXmlValue);
				}
			});
		}

		public void TestOtrosTributos_NegativeOSTaxTransactionAmount_ShouldBeInformed()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OSTaxTransactionsAmount = -1m;
			transaction.SetTaxTransactionCollection(() => new List<TaxTransaction>());

			var taxTransaction1 = new TaxTransaction(DefaultDataObjectWriterStrategy.TestInstance);
			taxTransaction1.TaxConfiguration = new CodeDescriptionPair30Char();
			taxTransaction1.TaxConfiguration.Code = "TST";
			taxTransaction1.TaxConfiguration.Description = "TEST1";
			taxTransaction1.OSTaxBase = 10m;
			taxTransaction1.OSTaxAmount = 20.01m;
			taxTransaction1.TaxSuperType = new CodeDescriptionPair()
			{
				Code = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code,
				Description = "PERCEPTIONS"
			};

			transaction.TaxTransactionCollection.Add(taxTransaction1);

			var expectedXml = $@"<arrayOtrosTributos>
<otroTributo>
<codigo>7</codigo>
<descripcion>TEST1</descripcion>
<baseImponible>10.00</baseImponible>
<importe>20.01</importe>
</otroTributo>
</arrayOtrosTributos>";

			var builder = (IComprobanteCAERequestBuilder)new ComprobanteCAERequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, Factory).ToString().Replace(" ", "");
			AssertContains(expectedXml, actualXmlValue);
		}

		public void TestOtrosTributos_OneTaxTransaction()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OSTaxTransactionsAmount = 20.33m;
			transaction.SetTaxTransactionCollection(() => new List<TaxTransaction>());

			var taxTransaction1 = new TaxTransaction(DefaultDataObjectWriterStrategy.TestInstance);
			taxTransaction1.TaxConfiguration = new CodeDescriptionPair30Char();
			taxTransaction1.TaxConfiguration.Code = "TST";
			taxTransaction1.TaxConfiguration.Description = "TEST1";
			taxTransaction1.OSTaxBase = 10m;
			taxTransaction1.OSTaxAmount = 20.01m;
			taxTransaction1.TaxSuperType = new CodeDescriptionPair()
			{
				Code = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code,
				Description = "PERCEPTIONS"
			};

			transaction.TaxTransactionCollection.Add(taxTransaction1);

			var expectedXml = $@"<arrayOtrosTributos>
<otroTributo>
<codigo>7</codigo>
<descripcion>TEST1</descripcion>
<baseImponible>10.00</baseImponible>
<importe>20.01</importe>
</otroTributo>
</arrayOtrosTributos>";

			var builder = (IComprobanteCAERequestBuilder)new ComprobanteCAERequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, Factory).ToString().Replace(" ", "");
			AssertContains(expectedXml, actualXmlValue);
		}

		public void TestOtrosTributos_ManyTaxTransactions()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OSTaxTransactionsAmount = 20.33m;
			transaction.SetTaxTransactionCollection(() => new List<TaxTransaction>());

			var taxTransaction1 = new TaxTransaction(DefaultDataObjectWriterStrategy.TestInstance);
			taxTransaction1.TaxConfiguration = new CodeDescriptionPair30Char();
			taxTransaction1.TaxConfiguration.Description = "TEST1";
			taxTransaction1.OSTaxBase = 10m;
			taxTransaction1.OSTaxAmount = 20.01m;
			taxTransaction1.TaxSuperType = new CodeDescriptionPair()
			{
				Code = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code,
				Description = "PERCEPTIONS"
			};

			var taxTransaction2 = new TaxTransaction(DefaultDataObjectWriterStrategy.TestInstance);
			taxTransaction2.TaxConfiguration = new CodeDescriptionPair30Char();
			taxTransaction2.TaxConfiguration.Description = "TEST2";
			taxTransaction2.OSTaxBase = 20m;
			taxTransaction2.OSTaxAmount = 30.21m;
			taxTransaction2.TaxSuperType = new CodeDescriptionPair()
			{
				Code = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code,
				Description = "PERCEPTIONS"
			};

			var taxTransaction3 = new TaxTransaction(DefaultDataObjectWriterStrategy.TestInstance);
			taxTransaction3.TaxConfiguration = new CodeDescriptionPair30Char();
			taxTransaction3.TaxConfiguration.Description = "TEST3";
			taxTransaction3.OSTaxBase = 1255m;
			taxTransaction3.OSTaxAmount = 18.33m;
			taxTransaction3.TaxSuperType = new CodeDescriptionPair()
			{
				Code = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.RetentionInInvoice.Code,
				Description = "NOT_PERCEPTIONS"
			};

			transaction.TaxTransactionCollection.Add(taxTransaction1);
			transaction.TaxTransactionCollection.Add(taxTransaction2);
			transaction.TaxTransactionCollection.Add(taxTransaction3);

			var expectedXml = $@"<arrayOtrosTributos>
<otroTributo>
<codigo>7</codigo>
<descripcion>TEST1</descripcion>
<baseImponible>10.00</baseImponible>
<importe>20.01</importe>
</otroTributo>
<otroTributo>
<codigo>7</codigo>
<descripcion>TEST2</descripcion>
<baseImponible>20.00</baseImponible>
<importe>30.21</importe>
</otroTributo>
</arrayOtrosTributos>";

			var builder = (IComprobanteCAERequestBuilder)new ComprobanteCAERequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, Factory).ToString().Replace(" ", "");
			AssertContains(expectedXml, actualXmlValue);
		}

		public void TestOtrosTributos_ChangeSing()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionType = TransactionType.CRD;
			transaction.OSTaxTransactionsAmount = 20.33m;
			transaction.SetTaxTransactionCollection(() => new List<TaxTransaction>());

			var taxTransaction1 = new TaxTransaction(DefaultDataObjectWriterStrategy.TestInstance);
			taxTransaction1.TaxConfiguration = new CodeDescriptionPair30Char();
			taxTransaction1.TaxConfiguration.Description = "TEST1";
			taxTransaction1.TaxSuperType = new CodeDescriptionPair()
			{
				Code = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code,
				Description = "PERCEPTIONS"
			};
			taxTransaction1.OSTaxBase = 1255m;
			taxTransaction1.OSTaxAmount = 18.33m;

			var taxTransaction2 = new TaxTransaction(DefaultDataObjectWriterStrategy.TestInstance);
			taxTransaction2.TaxConfiguration = new CodeDescriptionPair30Char();
			taxTransaction2.TaxConfiguration.Description = "TEST2";
			taxTransaction2.TaxSuperType = new CodeDescriptionPair()
			{
				Code = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code,
				Description = "PERCEPTIONS"
			};
			taxTransaction2.OSTaxBase = -1000m;
			taxTransaction2.OSTaxAmount = -2m;

			transaction.TaxTransactionCollection.Add(taxTransaction1);
			transaction.TaxTransactionCollection.Add(taxTransaction2);

			var expectedXml = $@"<arrayOtrosTributos>
<otroTributo>
<codigo>7</codigo>
<descripcion>TEST1</descripcion>
<baseImponible>-1255.00</baseImponible>
<importe>-18.33</importe>
</otroTributo>
<otroTributo>
<codigo>7</codigo>
<descripcion>TEST2</descripcion>
<baseImponible>1000.00</baseImponible>
<importe>2.00</importe>
</otroTributo>
</arrayOtrosTributos>";

			var builder = (IComprobanteCAERequestBuilder)new ComprobanteCAERequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, Factory).ToString().Replace(" ", "");
			AssertContains(expectedXml, actualXmlValue);
		}

		#endregion

		#region Totales

		public void TestItemDetailEInvoice_Totales_MissingNullsAndEmpty()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionType = TransactionType.INV;
			AssertTotalesZero(transaction);

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			AssertTotalesZero(transaction);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance));
			AssertTotalesZero(transaction);

			transaction.PostingJournalCollection[0].VATTaxID = new TaxID();
			AssertTotalesZero(transaction);

			transaction.PostingJournalCollection[0].VATTaxID.TaxType = new CodeDescriptionPair();
			AssertTotalesZero(transaction);

			transaction.PostingJournalCollection[0].VATTaxID.TaxType.Code = ZString.Empty;
			AssertTotalesZero(transaction);
		}

		void AssertTotalesZero(TransactionInfo transaction)
		{
			var expectedResult = @"
<importeGravado>0.00</importeGravado>
<importeNoGravado>0.00</importeNoGravado>
<importeExento>0.00</importeExento>
<importeSubtotal>0.00</importeSubtotal>
<importeTotal>0.00</importeTotal>";

			AssertTotales(transaction, expectedResult);
		}

		public void TestItemDetailEInvoice_Totales_importeGravado()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			transaction.TransactionType = TransactionType.INV;
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.PostingJournalCollection.Add(AddTransactionLine(100.38m, 21.08m, 121.46m, 100.38m, 21.08m, 121.46m, AddTaxID("IVA", 21, "RAT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(20.18m, 4.24m, 24.42m, 20.18m, 4.24m, 24.42m, AddTaxID("CAPIVA", 21, "CAP")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(600m, 0m, 600m, 600m, 0m, 600m, AddTaxID("IVA", 0, "RAT")));
			transaction.OSTotal = 745.88;
			AssertImporteGravado();

			transaction.TransactionType = TransactionType.CRD;
			transaction.PostingJournalCollection.ForEach(x => ChangeSignTransactionLine(x));
			transaction.OSTotal = (-1) * transaction.OSTotal;
			AssertImporteGravado();

			void AssertImporteGravado()
			{
				var expectedResult = @"
<importeGravado>720.56</importeGravado>
<importeNoGravado>0.00</importeNoGravado>
<importeExento>0.00</importeExento>
<importeSubtotal>720.56</importeSubtotal>
<importeTotal>745.88</importeTotal>";

				AssertTotales(transaction, expectedResult);
			}
		}

		public void TestItemDetailEInvoice_Totales_importeNoGravado()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			transaction.TransactionType = TransactionType.INV;
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.PostingJournalCollection.Add(AddTransactionLine(1260.79m, 0.00m, 1260.79m, 1260.79m, 0.00m, 1260.79m, AddTaxID("EXCLUDE", 0, "EXL")));
			transaction.OSTotal = 1260.79;
			AssertImporteNoGravado();

			transaction.TransactionType = TransactionType.CRD;
			transaction.PostingJournalCollection.ForEach(x => ChangeSignTransactionLine(x));
			transaction.OSTotal = (-1) * transaction.OSTotal;
			AssertImporteNoGravado();

			void AssertImporteNoGravado()
			{
				var expectedResult = @"
<importeGravado>0.00</importeGravado>
<importeNoGravado>1260.79</importeNoGravado>
<importeExento>0.00</importeExento>
<importeSubtotal>1260.79</importeSubtotal>
<importeTotal>1260.79</importeTotal>";

				AssertTotales(transaction, expectedResult);
			}
		}

		public void TestItemDetailEInvoice_Totales_importeExento()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			transaction.TransactionType = TransactionType.INV;
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.PostingJournalCollection.Add(AddTransactionLine(585.39m, 0.00m, 585.39m, 585.39m, 0.00m, 585.39m, AddTaxID("EXEMPT", 0, "EXT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(-20.10m, 0.00m, -20.10m, -20.10m, 0.00m, -20.10m, AddTaxID("EXEMPT", 0, "EXT")));
			transaction.OSTotal = 565.29;
			AssertImporteExento();

			transaction.TransactionType = TransactionType.CRD;
			transaction.PostingJournalCollection.ForEach(x => ChangeSignTransactionLine(x));
			transaction.OSTotal = (-1) * transaction.OSTotal;
			AssertImporteExento();

			void AssertImporteExento()
			{
				var expectedResult = @"
<importeGravado>0.00</importeGravado>
<importeNoGravado>0.00</importeNoGravado>
<importeExento>565.29</importeExento>
<importeSubtotal>565.29</importeSubtotal>
<importeTotal>565.29</importeTotal>";

				AssertTotales(transaction, expectedResult);
			}
		}

		public void TestItemDetailEInvoice_Totales_importeOtrosTributos()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			transaction.TransactionType = TransactionType.INV;
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.PostingJournalCollection.Add(AddTransactionLine(100m, 21m, 121m, 100m, 21m, 121, AddTaxID("GST", 21, "GST")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(200m, 42m, 242m, 200m, 42m, 242m, AddTaxID("GST", 21, "GST")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(100m, 0m, 100m, 100m, 0m, 100m, AddTaxID("RAT", 0, "RAT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(200m, 0m, 200m, 200m, 0m, 200m, AddTaxID("CAP", 0, "CAP")));
			transaction.OSTotal = 663m;
			transaction.OSTaxTransactionsAmount = 10.50m;
			AssertImporteOtrosTributos();

			transaction.TransactionType = TransactionType.CRD;
			transaction.PostingJournalCollection.ForEach(x => ChangeSignTransactionLine(x));
			transaction.OSTotal = (-1) * transaction.OSTotal;
			transaction.OSTaxTransactionsAmount = (-1) * transaction.OSTaxTransactionsAmount;
			AssertImporteOtrosTributos();

			void AssertImporteOtrosTributos()
			{
				var expectedResult = @"
<importeGravado>300.00</importeGravado>
<importeNoGravado>0.00</importeNoGravado>
<importeExento>0.00</importeExento>
<importeSubtotal>300.00</importeSubtotal>
<importeOtrosTributos>10.50</importeOtrosTributos>
<importeTotal>663.00</importeTotal>";

				AssertTotales(transaction, expectedResult);
			}
		}

		public void TestItemDetailEInvoice_Totales_AllTaxTypeCodes()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			transaction.TransactionType = TransactionType.INV;
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			transaction.PostingJournalCollection.Add(AddTransactionLine(100.38m, 0m, 121.46m, 100.38m, 0m, 121.46m, AddTaxID("IVA", 0, "RAT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(20.18m, 4.24m, 24.42m, 20.18m, 4.24m, 24.42m, AddTaxID("CAPIVA", 21, "CAP")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(1260.79m, 0.00m, 1260.79m, 1260.79m, 0.00m, 1260.79m, AddTaxID("EXCLUDE", 0, "EXL")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(200.05m, 0.00m, 200.05m, 200.05m, 0.00m, 200.05m, AddTaxID("NOTREPORT", 0, "NOT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(585.39m, 0.00m, 585.39m, 585.39m, 0.00m, 585.39m, AddTaxID("EXEMPT", 0, "EXT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(-20.10m, 0.00m, -20.10m, -20.10m, 0.00m, -20.10m, AddTaxID("EXEMPT", 0, "EXT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(777.77m, 163.33m, 941.1m, 777.77m, 163.33m, 941.1m, AddTaxID("GST", 21, "GST")));
			transaction.OSTotal = 2928.70;
			AssertAllTaxTypeCodes();

			transaction.TransactionType = TransactionType.CRD;
			transaction.PostingJournalCollection.ForEach(x => ChangeSignTransactionLine(x));
			transaction.OSTotal = (-1) * transaction.OSTotal;
			AssertAllTaxTypeCodes();

			void AssertAllTaxTypeCodes()
			{
				var expectedResult = @"
<importeGravado>120.56</importeGravado>
<importeNoGravado>1460.84</importeNoGravado>
<importeExento>565.29</importeExento>
<importeSubtotal>2146.69</importeSubtotal>
<importeTotal>2928.70</importeTotal>";

				AssertTotales(transaction, expectedResult);
			}
		}

		PostingJournal AddTransactionLine(ZDecimal osAmount, ZDecimal osGSTVATAmount, ZDecimal osTotalAmount, ZDecimal localAmount, ZDecimal localGSTVATAmount, ZDecimal localTotalAmount, TaxID taxID)
		{
			return new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OSAmount = osAmount,
				OSGSTVATAmount = osGSTVATAmount,
				OSTotalAmount = osTotalAmount,
				LocalAmount = localAmount,
				LocalGSTVATAmount = localGSTVATAmount,
				LocalTotalAmount = localTotalAmount,
				VATTaxID = taxID
			};
		}

		void ChangeSignTransactionLine(PostingJournal postingJournal)
		{
			postingJournal.OSAmount = (-1) * postingJournal.OSAmount;
			postingJournal.OSGSTVATAmount = (-1) * postingJournal.OSGSTVATAmount;
			postingJournal.OSTotalAmount = (-1) * postingJournal.OSTotalAmount;
			postingJournal.LocalAmount = (-1) * postingJournal.LocalAmount;
			postingJournal.LocalGSTVATAmount = (-1) * postingJournal.LocalGSTVATAmount;
			postingJournal.LocalTotalAmount = (-1) * postingJournal.LocalTotalAmount;
		}

		TaxID AddTaxID(ZString taxCode, ZDecimal taxRate, ZString taxTypecode)
		{
			return new TaxID()
			{
				TaxCode = taxCode,
				TaxRate = taxRate,
				TaxType = new CodeDescriptionPair() { Code = taxTypecode }
			};
		}

		void AssertTotales(TransactionInfo transaction, string expectedResult)
		{
			var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;

			var actualResult = builder.BuildXML(transaction, Factory).ToString().Replace(" ", "");

			AssertContains("The result returned for BuildXML must contain the expectedResult", expectedResult, actualResult);
		}

		#endregion Totales

		public void Test_FixedValues_Concepto_NumeroComprobante()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var comprobanteCAERequestBuilder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
			var actualXmlValue = comprobanteCAERequestBuilder.BuildXML(transactionInfo, Factory).ToString();
			AssertContains("<codigoConcepto>2</codigoConcepto>", actualXmlValue);
			AssertContains("<numeroComprobante></numeroComprobante>", actualXmlValue);
		}

		public void Test_DatesFromTransactionDate()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var testCasesDates = new[]
			{
				new { TransactionDate = new ZDateTime(2021, 06, 12, 12, 0, 0), ExpectedValueFormat = "2021-06-12" },
				new { TransactionDate = new ZDateTime(2021, 06, 12, 12, 12, 12), ExpectedValueFormat = "2021-06-12" },
				new { TransactionDate = new ZDateTime(2021, 10, 18, 4, 0, 0), ExpectedValueFormat = "2021-10-18" },
				new { TransactionDate = new ZDateTime(2021, 01, 01, 0, 0, 0), ExpectedValueFormat = "2021-01-01" },
				new { TransactionDate = new ZDateTime(2021, 12, 31, 0, 0, 0), ExpectedValueFormat = "2021-12-31" },
				new { TransactionDate = ZDateTime.Empty, ExpectedValueFormat = "" },
				new { TransactionDate = new ZDateTime(null), ExpectedValueFormat = "" },
			};

			foreach (var testCase in testCasesDates)
			{
				transactionInfo.TransactionDate = testCase.TransactionDate;
				AssertDatesFromTransactionDate(testCase.ExpectedValueFormat);
			}

			void AssertDatesFromTransactionDate(string expectedXmlResult)
			{
				var comprobanteCAERequestBuilder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
				var actualXmlValue = comprobanteCAERequestBuilder.BuildXML(transactionInfo, Factory).ToString();

				AssertContains($"<fechaEmision>{expectedXmlResult}</fechaEmision>", actualXmlValue);
				AssertContains($"<fechaServicioDesde>{expectedXmlResult}</fechaServicioDesde>", actualXmlValue);
				AssertContains($"<fechaServicioHasta>{expectedXmlResult}</fechaServicioHasta>", actualXmlValue);
			}
		}

		public void Test_FechaVencimientoPago()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var testCasesDates = new[]
			{
				new { DueDate = new ZDateTime(2021, 06, 12, 12, 0, 0), ExpectedValueFormat = "2021-06-12" },
				new { DueDate = new ZDateTime(2021, 06, 12, 12, 12, 12), ExpectedValueFormat = "2021-06-12" },
				new { DueDate = new ZDateTime(2021, 10, 18, 4, 0, 0), ExpectedValueFormat = "2021-10-18" },
				new { DueDate = new ZDateTime(2021, 01, 01, 0, 0, 0), ExpectedValueFormat = "2021-01-01" },
				new { DueDate = new ZDateTime(2021, 12, 31, 0, 0, 0), ExpectedValueFormat = "2021-12-31" },
				new { DueDate = ZDateTime.Empty, ExpectedValueFormat = "" },
				new { DueDate = new ZDateTime(null), ExpectedValueFormat = "" },
			};

			foreach (var testCase in testCasesDates)
			{
				transactionInfo.DueDate = testCase.DueDate;
				AssertFechaVencimientoPago(testCase.ExpectedValueFormat);
			}

			void AssertFechaVencimientoPago(string expectedXmlResult)
			{
				var comprobanteCAERequestBuilder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
				var actualXmlValue = comprobanteCAERequestBuilder.BuildXML(transactionInfo, Factory).ToString();

				AssertContains($"<fechaVencimientoPago>{expectedXmlResult}</fechaVencimientoPago>", actualXmlValue);
			}
		}

		public void Test_CodigoTipoComprobante()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var complianceSubTypesTestCases = new[]
			{
				new { complianceSubType = (string)null, expectedXmlValue = "" },
				new { complianceSubType = string.Empty, expectedXmlValue = "" },
				new { complianceSubType = "XXX", expectedXmlValue = "" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA, expectedXmlValue = "1" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDA, expectedXmlValue = "2" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCA, expectedXmlValue = "3" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXB, expectedXmlValue = "6" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDB, expectedXmlValue = "7" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCB, expectedXmlValue = "8" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXC, expectedXmlValue = "11" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDC, expectedXmlValue = "12" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCC, expectedXmlValue = "13" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXM, expectedXmlValue = "51" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDM, expectedXmlValue = "52" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCM, expectedXmlValue = "53" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA, expectedXmlValue = "201" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDA, expectedXmlValue = "202" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA, expectedXmlValue = "203" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXB, expectedXmlValue = "206" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDB, expectedXmlValue = "207" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB, expectedXmlValue = "208" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXC, expectedXmlValue = "211" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDC, expectedXmlValue = "212" },
				new { complianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCC, expectedXmlValue = "213" },
			};

			foreach (var complianceSubTypeTestCase in complianceSubTypesTestCases)
			{
				transactionInfo.ComplianceSubType = complianceSubTypeTestCase.complianceSubType;
				AssertCodigoTipoComprobante(complianceSubTypeTestCase.expectedXmlValue);
			}

			void AssertCodigoTipoComprobante(string expectedXmlResult)
			{
				var comprobanteCAERequestBuilder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
				var actualXmlValue = comprobanteCAERequestBuilder.BuildXML(transactionInfo, Factory).ToString();
				AssertContains($"<codigoTipoComprobante>{expectedXmlResult}</codigoTipoComprobante>", actualXmlValue);
			}
		}

		public void Test_NumeroPuntoVenta()
		{
			var retrieverMock = new Mock<IComplianceSequenceRetriever>();
			var argentinaEInvoiceHelperMock = new Mock<IArgentinaEInvoiceHelper>();
			var argentinaDependencyMock = new Mock<IArgentinaEInvoicingDependencyFactory>();
			argentinaDependencyMock.Setup(x => x.GetComplianceSequenceRetriever()).Returns(retrieverMock.Object);
			argentinaDependencyMock.Setup(x => x.GetArgentinaEInvoiceHelper()).Returns(argentinaEInvoiceHelperMock.Object);

			var eInvoicinigDependencyMock = new Mock<IEInvoicingDependencyFactory>();
			eInvoicinigDependencyMock.Setup(x => x.GetArgentinaEInvoicingDependencyFactory()).Returns(argentinaDependencyMock.Object);

			using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
			{
				AssertNumeroPuntoVenta(ZString.Empty, null);

				var accComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				accComplianceSequence.XD_Prefix = ZString.Empty;
				AssertNumeroPuntoVenta(ZString.Empty, accComplianceSequence);

				accComplianceSequence.XD_Prefix = "00013";
				AssertNumeroPuntoVenta("00013", accComplianceSequence);
			}

			void AssertNumeroPuntoVenta(string expectedValue, AccComplianceSequence mockReturn = null)
			{
				var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				transactionInfo.ComplianceSubType = "ABC";

				retrieverMock.Setup(x => x.GetComplianceSequenceFromSubType(transactionInfo.ComplianceSubType.GetValueOrDefault(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>())).Returns(() => mockReturn);

				var comprobanteCAERequestBuilder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
				var actualXmlValue = comprobanteCAERequestBuilder.BuildXML(transactionInfo, Factory).ToString();
				AssertContains(string.Format("<numeroPuntoVenta>{0}</numeroPuntoVenta>", expectedValue), actualXmlValue);

				retrieverMock.Verify(x => x.GetComplianceSequenceFromSubType("ABC", It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>()), Times.Once);
				retrieverMock.Reset();
			}
		}

		#region CodigoMoneda_CotizacionMoneda

		public void Test_CodigoMoneda_CotizacionMoneda()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			AssertCurrencyAndExchageRate("PES", "1");
			AssertCurrencyAndExchageRate("AUD", "19.150000");
			AssertCurrencyAndExchageRate("","");

			void AssertCurrencyAndExchageRate(string currId, string currExRate)
			{
				var argentinaEInvoiceExtensionMock = new Mock<IArgentinaEInvoicingExtension>();

				var dependencyFactoryMock = new Mock<IAccountingMasterFilesDependencyFactory>();
				dependencyFactoryMock.Setup(x => x.GetArgentinaEInvoicingExtension()).Returns(argentinaEInvoiceExtensionMock.Object);

				argentinaEInvoiceExtensionMock.Setup(x => x.GetCurrencyAndExchangeRateTransactionInfo(transactionInfo)).Returns((currId,currExRate));

				using (ObjectFactory.Substitute(dependencyFactoryMock.Object))
				{
					var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
					var actualXmlValue = builder.BuildXML(transactionInfo, Factory).ToString().Replace(" ", "");

					AssertContains($"<codigoMoneda>{currId}</codigoMoneda>", actualXmlValue);
					AssertContains($"<cotizacionMoneda>{currExRate}</cotizacionMoneda>", actualXmlValue);

					argentinaEInvoiceExtensionMock.Verify(x => x.GetCurrencyAndExchangeRateTransactionInfo(transactionInfo));
				}
			}
		}

		#endregion

		#region CodigoTipoDocumento_NumeroDocumento

		public void Test_CodigoTipoDocumento_NumeroDocumento()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			AssertDocumentTypeAndNumber("80", "123");
			AssertDocumentTypeAndNumber("86", "12345");
			AssertDocumentTypeAndNumber("", "");

			void AssertDocumentTypeAndNumber(string docType, string docNro)
			{
				var argentinaEInvoiceExtensionMock = new Mock<IArgentinaEInvoicingExtension>();

				var dependencyFactoryMock = new Mock<IAccountingMasterFilesDependencyFactory>();
				dependencyFactoryMock.Setup(x => x.GetArgentinaEInvoicingExtension()).Returns(argentinaEInvoiceExtensionMock.Object);

				argentinaEInvoiceExtensionMock.Setup(x => x.GetRegistrationNumberTransactionInfo(transactionInfo)).Returns((docType, docNro));

				using (ObjectFactory.Substitute(dependencyFactoryMock.Object))
				{
					argentinaEInvoiceExtensionMock.Setup(x => x.GetRegistrationNumberTransactionInfo(transactionInfo)).Returns((docType, docNro));

					var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
					var actualXmlValue = builder.BuildXML(transactionInfo, Factory).ToString().Replace(" ", "");

					AssertContains($"<codigoTipoDocumento>{docType}</codigoTipoDocumento>", actualXmlValue);
					AssertContains($"<numeroDocumento>{docNro}</numeroDocumento>", actualXmlValue);

					argentinaEInvoiceExtensionMock.Verify(x => x.GetRegistrationNumberTransactionInfo(transactionInfo));
				}
			}
		}

		#endregion

		#region CondicionIVAReceptor

		public void Test_CondicionIvaReceptor_Included()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			var validRegistrationNumber = new RegistrationNumber
			{
				CountryOfIssue = new Country { Code = "AR" },
				Type = new RegistrationNumberType { Code = "IVI" }
			};
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> { validRegistrationNumber });
			var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;

			var xmlValue = builder.BuildXML(transactionInfo, Factory).ToString().Replace(" ", "");

			AssertContains("<condicionIVAReceptor>1</condicionIVAReceptor>", xmlValue);
		}

		public void Test_CondicionIvaReceptor_Excluded()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			var invalidRegistrationNumber = new RegistrationNumber
			{
				CountryOfIssue = new Country { Code = "AR" }, Type = new RegistrationNumberType { Code = "XXX" }
			};
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> { invalidRegistrationNumber });
			var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;

			var xmlValue = builder.BuildXML(transactionInfo, Factory).ToString().Replace(" ", "");

			AssertNotContains("<condicionIVAReceptor", xmlValue);
		}

		#endregion

		#region Comprobantes Asociados

		public void Test_ComprobantesAsociadosNode_IsNotPresent_When_IsNotCreditOrDebitNoteTransaction()
		{
			SetUpMocks();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
			{
				argentinaEInvoiceHelperMock.Setup(x => x.IsOriginalReferenceCreditOrDebitNoteTransaction(It.IsAny<TransactionInfo>())).Returns(false);

				var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
				var actualXmlValue = builder.BuildXML(transactionInfo, Factory).ToString().Replace(" ", "");

				AssertNotContains("</arrayComprobantesAsociados>", actualXmlValue);

				argentinaEInvoiceHelperMock.Verify(x => x.IsOriginalReferenceCreditOrDebitNoteTransaction(transactionInfo), Times.Once);
			}
		}

		public void Test_ComprobantesAsociadosNode_IsCreditOrDebitNoteTransaction()
		{
			SetUpMocks();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
			{
				argentinaEInvoiceHelperMock.Setup(x => x.IsOriginalReferenceCreditOrDebitNoteTransaction(It.IsAny<TransactionInfo>())).Returns(true);

				var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
				var actualXmlValue = builder.BuildXML(transactionInfo, Factory).ToString();

				AssertContains("<arrayComprobantesAsociados>", actualXmlValue);

				argentinaEInvoiceHelperMock.Verify(x => x.IsOriginalReferenceCreditOrDebitNoteTransaction(transactionInfo), Times.Once);
			}
		}

		public void Test_ComprobantesAsociadosNode_FechaEmision()
		{
			SetUpMocks();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var emptyComprobanteAsociadosXml = @"<arrayComprobantesAsociados>
    <comprobanteAsociado>
      <codigoTipoComprobante></codigoTipoComprobante>
      <numeroPuntoVenta></numeroPuntoVenta>
      <numeroComprobante></numeroComprobante>
      <fechaEmision>{FechaEmision}</fechaEmision>
    </comprobanteAsociado>
  </arrayComprobantesAsociados>";

			transactionInfo.OriginalReference = null;
			AssertComprobantesAsociadosFechaEmision("");

			transactionInfo.OriginalReference = new OriginalReference();
			AssertComprobantesAsociadosFechaEmision("");

			var testCasesFechaEmision = new[]
			{
				new { OriginalTransactionDate = new ZDateTime(2021, 06, 12, 12, 0, 0), ExpectedValueFormat = "2021-06-12" },
				new { OriginalTransactionDate = new ZDateTime(2021, 06, 12, 12, 12, 12), ExpectedValueFormat = "2021-06-12" },
				new { OriginalTransactionDate = new ZDateTime(2021, 10, 18, 4, 0, 0), ExpectedValueFormat = "2021-10-18" },
				new { OriginalTransactionDate = new ZDateTime(2021, 01, 01, 0, 0, 0), ExpectedValueFormat = "2021-01-01" },
				new { OriginalTransactionDate = new ZDateTime(2021, 12, 31, 0, 0, 0), ExpectedValueFormat = "2021-12-31" },
				new { OriginalTransactionDate = ZDateTime.Empty, ExpectedValueFormat = "" },
			};

			foreach (var testCase in testCasesFechaEmision)
			{
				transactionInfo.OriginalReference.OriginalTransactionDate = testCase.OriginalTransactionDate;
				AssertComprobantesAsociadosFechaEmision(testCase.ExpectedValueFormat);
			}

			void AssertComprobantesAsociadosFechaEmision(string expectedValue)
			{
				using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
				{
					var comprobanteCAERequestBuilder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
					var actualXmlValue = comprobanteCAERequestBuilder.BuildXML(transactionInfo, Factory).ToString();
					var expectedComprobantesAsociadosXml = emptyComprobanteAsociadosXml.Replace("{FechaEmision}", expectedValue);
					XmlComparison.CompareAndAssertXml(expectedEmptyXml.Replace("{ArrayComprobantesAsociados}", expectedComprobantesAsociadosXml), actualXmlValue);
				}
			}
		}

		public void Test_ComprobantesAsociadosNode_CodigoTipoComprobante()
		{
			SetUpMocks();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var emptyComprobanteAsociadosXml = @"<arrayComprobantesAsociados>
    <comprobanteAsociado>
      <codigoTipoComprobante>{CodigoTipoComprobante}</codigoTipoComprobante>
      <numeroPuntoVenta></numeroPuntoVenta>
      <numeroComprobante></numeroComprobante>
      <fechaEmision></fechaEmision>
    </comprobanteAsociado>
  </arrayComprobantesAsociados>";

			AssertComprobantesAsociadosCodigoComprobante("","");
			AssertComprobantesAsociadosCodigoComprobante("TXA", "1");
			AssertComprobantesAsociadosCodigoComprobante(null, "");

			void AssertComprobantesAsociadosCodigoComprobante(string originalComplianceSubtype, string expectedValue)
			{
				using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
				{
					argentinaEInvoiceHelperMock.Setup(x => x.GetOriginalTransactionComplianceSubType(It.IsAny<OriginalReference>())).Returns(() => originalComplianceSubtype);
					argentinaEInvoiceExtensionMock.Setup(x => x.GetDocumentType(It.IsAny<ZString>())).Returns(() => expectedValue);

					var comprobanteCAERequestBuilder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
					var actualXmlValue = comprobanteCAERequestBuilder.BuildXML(transactionInfo, Factory).ToString();

					var expectedComprobantesAsociadosXml = emptyComprobanteAsociadosXml.Replace("{CodigoTipoComprobante}", expectedValue);
					XmlComparison.CompareAndAssertXml(expectedEmptyXml.Replace("{ArrayComprobantesAsociados}", expectedComprobantesAsociadosXml), actualXmlValue);
				}
			}
		}

		public void Test_ComprobantesAsociadosNode_NumeroPuntoVenta()
		{
			SetUpMocks();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var emptyComprobanteAsociadosXml = @"<arrayComprobantesAsociados>
    <comprobanteAsociado>
      <codigoTipoComprobante></codigoTipoComprobante>
      <numeroPuntoVenta>{NumeroPtoVenta}</numeroPuntoVenta>
      <numeroComprobante></numeroComprobante>
      <fechaEmision></fechaEmision>
    </comprobanteAsociado>
  </arrayComprobantesAsociados>";

			AssertComprobantesAsociadosNumeroPtoVenta("");
			AssertComprobantesAsociadosNumeroPtoVenta("00012");

			void AssertComprobantesAsociadosNumeroPtoVenta(string expectedValue)
			{
				using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
				{
					argentinaEInvoiceHelperMock.Setup(x => x.GetComplianceNumberPrefixFromTransaction(It.IsAny<OriginalReference>())).Returns(() => expectedValue);

					var comprobanteCAERequestBuilder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
					var actualXmlValue = comprobanteCAERequestBuilder.BuildXML(transactionInfo, Factory).ToString();

					var expectedComprobantesAsociadosXml = emptyComprobanteAsociadosXml.Replace("{NumeroPtoVenta}", expectedValue);
					XmlComparison.CompareAndAssertXml(expectedEmptyXml.Replace("{ArrayComprobantesAsociados}", expectedComprobantesAsociadosXml), actualXmlValue);
				}
			}
		}

		public void Test_ComprobantesAsociadosNode_NumeroComprobante()
		{
			SetUpMocks();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var emptyComprobanteAsociadosXml = @"<arrayComprobantesAsociados>
    <comprobanteAsociado>
      <codigoTipoComprobante></codigoTipoComprobante>
      <numeroPuntoVenta></numeroPuntoVenta>
      <numeroComprobante>{NumeroComprobante}</numeroComprobante>
      <fechaEmision></fechaEmision>
    </comprobanteAsociado>
  </arrayComprobantesAsociados>";

			AssertComprobantesAsociadosNumeroComprobante("");
			AssertComprobantesAsociadosNumeroComprobante("12345678");

			void AssertComprobantesAsociadosNumeroComprobante(string expectedValue)
			{
				using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
				{
					argentinaEInvoiceHelperMock.Setup(x => x.GetComplianceNumberFromTransaction(It.IsAny<OriginalReference>())).Returns(() => expectedValue);

					var comprobanteCAERequestBuilder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
					var actualXmlValue = comprobanteCAERequestBuilder.BuildXML(transactionInfo, Factory).ToString();

					var expectedComprobantesAsociadosXml = emptyComprobanteAsociadosXml.Replace("{NumeroComprobante}", expectedValue);
					XmlComparison.CompareAndAssertXml(expectedEmptyXml.Replace("{ArrayComprobantesAsociados}", expectedComprobantesAsociadosXml), actualXmlValue);
				}
			}
		}

		public void Test_ComprobantesAsociadosNode_CuitFormat()
		{
			SetUpMocks();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA;

			var emptyComprobanteAsociadosXml = @"<arrayComprobantesAsociados>
    <comprobanteAsociado>
      <codigoTipoComprobante></codigoTipoComprobante>
      <numeroPuntoVenta></numeroPuntoVenta>
      <numeroComprobante></numeroComprobante>
      <cuit>{Cuit}</cuit>
      <fechaEmision></fechaEmision>
    </comprobanteAsociado>
  </arrayComprobantesAsociados>";

			AssertComprobantesAsociadosCuit("AAAA2099999999 - 2", "20999999992");
			AssertComprobantesAsociadosCuit("20999999992", "20999999992");
			AssertComprobantesAsociadosCuit("", "");
			AssertComprobantesAsociadosCuit(null, "");

			void AssertComprobantesAsociadosCuit(string mockResult, string expectedValue)
			{
				using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
				{
					transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), It.IsAny<ZString>())).Returns(() => mockResult);

					var comprobanteCAERequestBuilder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
					var actualXmlValue = comprobanteCAERequestBuilder.BuildXML(transactionInfo, Factory).ToString();
					var expectedComprobantesAsociadosXml = emptyComprobanteAsociadosXml.Replace("{Cuit}", expectedValue);

					AssertContains(expectedComprobantesAsociadosXml, actualXmlValue);
				}
			}
		}

		public void TestComprobantesAsociadosNode_CuitTag_PresentDependOfComplianceSubtype()
		{
			SetUpMocks();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var exppectedCbteAsocWithCuit = @"<arrayComprobantesAsociados>
    <comprobanteAsociado>
      <codigoTipoComprobante></codigoTipoComprobante>
      <numeroPuntoVenta></numeroPuntoVenta>
      <numeroComprobante></numeroComprobante>
      <cuit></cuit>
      <fechaEmision></fechaEmision>
    </comprobanteAsociado>
  </arrayComprobantesAsociados>";

			var expectedCbteAsocWithoutCuit = @"<arrayComprobantesAsociados>
    <comprobanteAsociado>
      <codigoTipoComprobante></codigoTipoComprobante>
      <numeroPuntoVenta></numeroPuntoVenta>
      <numeroComprobante></numeroComprobante>
      <fechaEmision></fechaEmision>
    </comprobanteAsociado>
  </arrayComprobantesAsociados>";

			var testCasesCuitPresence = new[]
			{
				new { ComplianceSubtype = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA, ExpectedXml= expectedCbteAsocWithoutCuit, DocumentType = "1" },
				new { ComplianceSubtype = "", ExpectedXml= expectedCbteAsocWithoutCuit,DocumentType = "" },
				new { ComplianceSubtype = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDA,ExpectedXml= exppectedCbteAsocWithCuit, DocumentType = "202" },
				new { ComplianceSubtype = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA, ExpectedXml= exppectedCbteAsocWithCuit, DocumentType = "203" },
				new { ComplianceSubtype = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDB,ExpectedXml= exppectedCbteAsocWithCuit, DocumentType = "207" },
				new { ComplianceSubtype = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB, ExpectedXml= exppectedCbteAsocWithCuit,DocumentType = "208" },
			};

			foreach (var testCase in testCasesCuitPresence)
			{
				transactionInfo.ComplianceSubType = testCase.ComplianceSubtype;
				AssertCuitTagIsPresent(testCase.DocumentType, testCase.ExpectedXml);
			}

			void AssertCuitTagIsPresent(string expectedDocumentType, string expectedXml)
			{
				using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
				{
					var builder = new ComprobanteCAERequestBuilder() as IComprobanteCAERequestBuilder;
					var actualXmlValue = builder.BuildXML(transactionInfo, Factory).ToString();

					AssertContains(expectedXml, actualXmlValue);
				}
			}
		}

		#endregion

		#region Implementation

		TransactionInfo GetTransactionInfo()
		{
			return new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.INV,
			};
		}

		void SetUpMocks()
		{
			argentinaEInvoiceHelperMock = new Mock<IArgentinaEInvoiceHelper>();
			retrieverMock = new Mock<IComplianceSequenceRetriever>();
			argentinaDependencyFactoryMock = new Mock<IArgentinaEInvoicingDependencyFactory>();
			argentinaEInvoiceExtensionMock = new Mock<IArgentinaEInvoicingExtension>();
			transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();

			dependencyFactoryMock = new Mock<IAccountingMasterFilesDependencyFactory>();
			dependencyFactoryMock.Setup(x => x.GetArgentinaEInvoicingExtension()).Returns(argentinaEInvoiceExtensionMock.Object);

			argentinaDependencyFactoryMock.Setup(x => x.GetArgentinaEInvoiceHelper()).Returns(argentinaEInvoiceHelperMock.Object);
			argentinaDependencyFactoryMock.Setup(x => x.GetComplianceSequenceRetriever()).Returns(retrieverMock.Object);
			argentinaDependencyFactoryMock.Setup(x => x.GetComplianceSequenceRetriever().GetComplianceSequenceFromSubType(It.IsAny<ZString>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>())).Returns(() => null);

			eInvoicinigDependencyMock = new Mock<IEInvoicingDependencyFactory>();
			eInvoicinigDependencyMock.Setup(x => x.GetArgentinaEInvoicingDependencyFactory()).Returns(argentinaDependencyFactoryMock.Object);
			eInvoicinigDependencyMock.Setup(x => x.GetTransactionInfoHelper()).Returns(transactionInfoHelperMock.Object);

			argentinaEInvoiceHelperMock.Setup(x => x.IsOriginalReferenceCreditOrDebitNoteTransaction(It.IsAny<TransactionInfo>())).Returns(true);
			argentinaEInvoiceHelperMock.Setup(x => x.GetOriginalTransactionComplianceSubType(It.IsAny<OriginalReference>())).Returns("");
			argentinaEInvoiceHelperMock.Setup(x => x.GetComplianceNumberPrefixFromTransaction(It.IsAny<OriginalReference>())).Returns("");
			argentinaEInvoiceHelperMock.Setup(x => x.GetComplianceNumberFromTransaction(It.IsAny<OriginalReference>())).Returns("");
			transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), It.IsAny<ZString>())).Returns("");
			argentinaEInvoiceExtensionMock.Setup(x => x.GetDocumentType(It.IsAny<ZString>())).Returns("");
		}

		Mock<IArgentinaEInvoiceHelper> argentinaEInvoiceHelperMock;
		Mock<IComplianceSequenceRetriever> retrieverMock;
		Mock<IArgentinaEInvoicingDependencyFactory> argentinaDependencyFactoryMock;
		Mock<IArgentinaEInvoicingExtension> argentinaEInvoiceExtensionMock;
		Mock<ITransactionInfoHelper> transactionInfoHelperMock;
		Mock<IAccountingMasterFilesDependencyFactory> dependencyFactoryMock;
		Mock<IEInvoicingDependencyFactory> eInvoicinigDependencyMock;

		string expectedEmptyXml => @"<comprobanteCAERequest>
  <codigoTipoComprobante></codigoTipoComprobante>
  <numeroPuntoVenta></numeroPuntoVenta>
  <numeroComprobante></numeroComprobante>
  <fechaEmision></fechaEmision>
  <codigoTipoDocumento></codigoTipoDocumento>
  <numeroDocumento></numeroDocumento>
  <importeGravado>0.00</importeGravado>
  <importeNoGravado>0.00</importeNoGravado>
  <importeExento>0.00</importeExento>
  <importeSubtotal>0.00</importeSubtotal>
  <importeTotal>0.00</importeTotal>
  <codigoMoneda></codigoMoneda>
  <cotizacionMoneda></cotizacionMoneda>
  <codigoConcepto>2</codigoConcepto>
  <fechaServicioDesde></fechaServicioDesde>
  <fechaServicioHasta></fechaServicioHasta>
  <fechaVencimientoPago></fechaVencimientoPago>
  {ArrayComprobantesAsociados}
</comprobanteCAERequest>";

		#endregion
	}
}
