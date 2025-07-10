using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.OrgConstants;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	class ClsFEXRequestBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<ClsFEXRequestBuilder>(new ExportEInvoiceXmlBuilder().ClsFEXRequestBuilder_ExposedForTestOnly);
		}

		public void TestInitialization()
		{
			AssertType<ComplianceSequenceRetriever>(new ClsFEXRequestBuilder().ComplianceSequenceRetriever_ExposedForTestOnly);
		}

		[ExpectNoExceptions]
		public void TestBuilder_ComplianceSequence()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			transaction.Branch.Code = branch.GB_Code;
			transaction.Department.Code = department.GE_Code;
			var originalAccComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			originalAccComplianceSequence.XD_GC_Company = branch.GB_GC;
			originalAccComplianceSequence.XD_GB_BranchOwner = branch.PK;
			originalAccComplianceSequence.XD_GE_Department = department.PK;
			originalAccComplianceSequence.XD_SequenceClass = "TXA";
			originalAccComplianceSequence.XD_Prefix = "00233";
			Factory.Save();

			var fexBuilder = new ClsFEXRequestBuilder();
			var complianceSequenceMock = new Mock<IComplianceSequenceRetriever>();
			complianceSequenceMock.Setup(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, branch.GB_GC, branch.PK, department.PK, It.IsAny<ZDateTime>())).Returns(originalAccComplianceSequence);
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequenceMock.Object);

			var builder = fexBuilder as IClsFEXRequestBuilder;
			var cfe = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory);
			complianceSequenceMock.Verify(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, branch.GB_GC, branch.PK, department.PK, It.IsAny<ZDateTime>()), Times.Once);

			transaction.Branch = null;
			originalAccComplianceSequence.XD_GC_Company = ZGuid.Empty;
			originalAccComplianceSequence.XD_GB_BranchOwner = ZGuid.Empty;
			complianceSequenceMock.Setup(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, ZGuid.Empty, ZGuid.Empty, department.PK, It.IsAny<ZDateTime>())).Returns(originalAccComplianceSequence);
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequenceMock.Object);
			cfe = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory);
			complianceSequenceMock.Verify(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, ZGuid.Empty, ZGuid.Empty, department.PK, It.IsAny<ZDateTime>()), Times.Once);

			transaction.Department = null;
			transaction.Branch = new Branch() { Code = branch.GB_Code };
			originalAccComplianceSequence.XD_GC_Company = branch.GB_GC;
			originalAccComplianceSequence.XD_GB_BranchOwner = branch.PK;
			originalAccComplianceSequence.XD_GE_Department = ZGuid.Empty;
			complianceSequenceMock.Setup(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, branch.GB_GC, branch.PK, ZGuid.Empty, It.IsAny<ZDateTime>())).Returns(originalAccComplianceSequence);
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequenceMock.Object);
			cfe = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory);
			complianceSequenceMock.Verify(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, branch.GB_GC, branch.PK, ZGuid.Empty, It.IsAny<ZDateTime>()), Times.Once);
		}

		#region MonIdAndMonCotiz

		public void TestMoneda_Id_And_Moneda_ctz_WithMissingCurrency()
		{
			var transactionWithOutCurrency = GetTransactionInfo();

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transactionWithOutCurrency, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Moneda_Id></Moneda_Id>", actualXmlValue);
			AssertContains("<Moneda_ctz></Moneda_ctz>", actualXmlValue);
		}

		public void TestMoneda_Id_And_Moneda_ctz_CurrencyIsNull()
		{
			var transactionWithNullCurrency = GetTransactionInfo();
			transactionWithNullCurrency.OSCurrency = null;

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transactionWithNullCurrency, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Moneda_Id></Moneda_Id>", actualXmlValue);
			AssertContains("<Moneda_ctz></Moneda_ctz>", actualXmlValue);
		}

		public void TestMoneda_Id_And_Moneda_ctz_CurrencyIsEmpty()
		{
			var transactionWithNullCurrency = GetTransactionInfo();
			transactionWithNullCurrency.OSCurrency = new Currency() { Code = string.Empty };

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transactionWithNullCurrency, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Moneda_Id></Moneda_Id>", actualXmlValue);
			AssertContains("<Moneda_ctz></Moneda_ctz>", actualXmlValue);
		}

		public void TestMoneda_Id_And_Moneda_ctz_WithTransactionInForeignCurrency()
		{
			var transactionInForeignCurrency = GetTransactionInfo();
			transactionInForeignCurrency.ExchangeRate = 19.24M;
			transactionInForeignCurrency.OSCurrency = new Currency() { Code = "AUD" };

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transactionInForeignCurrency, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Moneda_Id>026</Moneda_Id>", actualXmlValue);
			AssertContains("<Moneda_ctz>19.240000</Moneda_ctz>", actualXmlValue);
		}

		public void TestMoneda_Id_And_Moneda_ctz_WithTransactionInLocalCurrency()
		{
			var transactionInLocalCurrency = GetTransactionInfo();
			transactionInLocalCurrency.ExchangeRate = 1.00M;
			transactionInLocalCurrency.OSCurrency = new Currency { Code = "ARS" };

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transactionInLocalCurrency, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Moneda_Id>PES</Moneda_Id>", actualXmlValue);
			AssertContains("<Moneda_ctz>1</Moneda_ctz>", actualXmlValue);
		}

		public void TestMoneda_Id_And_Moneda_ctz_TransactionNonAcceptedOsCurrency()
		{
			var transactionWithnoValidCurrency = GetTransactionInfo();
			transactionWithnoValidCurrency.ExchangeRate = 15.00M;
			transactionWithnoValidCurrency.OSCurrency = new Currency { Code = "XXX" };

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transactionWithnoValidCurrency, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Moneda_Id></Moneda_Id>", actualXmlValue);
			AssertContains("<Moneda_ctz></Moneda_ctz>", actualXmlValue);
		}

		#endregion

		#region Dates

		[TestDate(2020, 10, 19)]
		public void TestFecha_cbte_Fecha_Pago_WithPopulateDates()
		{
			var transaction = GetTransactionInfo();
			transaction.TransactionDate = ZDateTime.Today;
			transaction.DueDate = new ZDateTime(2020, 10, 21);

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();

			var complianceList = ArgentinaEInvoiceAPICommandList.GetExportEInvoiceComplianceSubTypeList();

			RemoveFromListAndCheckFechaPago(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE, "20201021");
			RemoveFromListAndCheckFechaPago(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCE);
			RemoveFromListAndCheckFechaPago(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDE);

			Assert(complianceList.Count == 0);

			void RemoveFromListAndCheckFechaPago(string complianceSubType, string expectedFechapago = "")
			{
				Assert(complianceList.Remove(complianceSubType));

				transaction.ComplianceSubType = complianceSubType;

				var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString();

				AssertContains("<Fecha_cbte>20201019</Fecha_cbte>", actualXmlValue);

				if (!string.IsNullOrEmpty(expectedFechapago))
				{
					AssertContains($"<Fecha_pago>{expectedFechapago}</Fecha_pago>", actualXmlValue);
				}
				else
				{
					AssertNotContains("Fecha_pago node must not be present in actualXmlValue", $"<Fecha_pago>", actualXmlValue);
				}
			}
		}

		public void TestFecha_cbte_Fecha_Pago_WithEmptyDates()
		{
			var transaction = GetTransactionInfo();
			transaction.TransactionDate = ZDateTime.Empty;
			transaction.DueDate = ZDateTime.Empty;

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString();

			AssertContains("<Fecha_cbte></Fecha_cbte>", actualXmlValue);
			AssertNotContains("<Fecha_pago>", actualXmlValue);
		}

		public void TestFecha_cbte_Fecha_Pago_WithNullDates()
		{
			var transaction = GetTransactionInfo();
			transaction.TransactionDate = null;
			transaction.DueDate = null;

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString();

			AssertContains("<Fecha_cbte></Fecha_cbte>", actualXmlValue);
			AssertNotContains("<Fecha_pago>", actualXmlValue);
		}

		#endregion

		#region ExpoType_PaymentMethod_Incoterm

		public void TestIncoterm_Incoterms_Ds_IsNotInclude_InXMLMessage()
		{
			var transactionInfo = GetTransactionInfo();
			transactionInfo.ComplianceSubType = "TXE";

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertNotContains("<Incoterms></Incoterms>", actualXmlValue);
			AssertNotContains("<Incoterms_Ds></Incoterms_Ds>", actualXmlValue);
		}

		public void TestExpoType_AlwaysMap_2()
		{
			var transactionInfo = GetTransactionInfo();
			transactionInfo.ComplianceSubType = "TXE";

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Tipo_expo>2</Tipo_expo>", actualXmlValue);
		}

		public void TestForma_pago_TransactionComplianceSubType_Missing_Empty_NullChecks()
		{
			CombineAssertions(() =>
			{
				var transaction = GetTransactionInfo();
				AssertResult(transaction);

				transaction.ComplianceSubType = null;
				AssertResult(transaction);

				transaction.ComplianceSubType = string.Empty;
				AssertResult(transaction);

				void AssertResult(TransactionInfo transactionInfo)
				{
					var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
					var actualXmlValue = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

					AssertNotContains("<Forma_pago></Forma_pago>", actualXmlValue);
				}
			});
		}

		public void TestForma_pago_WhenTransactionComplianceSubtype_IsNotForExportInvoice()
		{
			var transactionInfo = GetTransactionInfo();
			transactionInfo.ComplianceSubType = "TXA";
			transactionInfo.AgreedPaymentMethod = CreditAgreedPaymentMethods.Code.BankTransfer;

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertNotContains("<Forma_pago></Forma_pago>", actualXmlValue);
		}

		public void TestForma_pago_TransactionAgreedPaymentMethod_Missing_Empty_NullChecks()
		{
			CombineAssertions(() =>
			{
				var transaction = GetTransactionInfo();
				transaction.ComplianceSubType = "TXE";
				AssertResult(transaction);

				transaction.AgreedPaymentMethod = null;
				AssertResult(transaction);

				transaction.AgreedPaymentMethod = string.Empty;
				AssertResult(transaction);

				void AssertResult(TransactionInfo transactionInfo)
				{
					var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
					var actualXmlValue = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

					AssertContains("<Forma_pago></Forma_pago>", actualXmlValue);
				}
			});
		}

		public void TestForma_pago_TransactionAgreedPaymentMethod_IsPopulate()
		{
			var transactionInfo = GetTransactionInfo();
			transactionInfo.ComplianceSubType = "TXE";
			transactionInfo.AgreedPaymentMethod = CreditAgreedPaymentMethods.Code.BankTransfer;

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString();

			AssertContains("<Forma_pago>Bank Transfer</Forma_pago>", actualXmlValue);
		}

		#endregion

		#region PuntoVenta And NumeroComprobante

		public void Test_PuntoVentaAndNumeroComprobante_ComplianceSequence_DoesNotMuch()
		{
			AccComplianceSequence accComplianceSequence = null;

			var complianceSequence = new Mock<IComplianceSequenceRetriever>();
			var fexBuilder = new ClsFEXRequestBuilder();
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequence.Object);
			var builder = fexBuilder as IClsFEXRequestBuilder;

			var transactionInfo = GetTransactionInfo();

			accComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			accComplianceSequence.XD_Prefix = "";

			AssertPuntoVentaAndNumeroComprobante("");

			accComplianceSequence.XD_Prefix = "0013";

			AssertPuntoVentaAndNumeroComprobante("0013");

			void AssertPuntoVentaAndNumeroComprobante(string expectedPunto_vta)
			{
				complianceSequence.Setup(x => x.GetComplianceSequenceFromSubType(It.IsAny<ZString>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>())).Returns(accComplianceSequence);
				var actualXmlValue = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString();

				complianceSequence.Verify(x => x.GetComplianceSequenceFromSubType(It.IsAny<ZString>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>()));
				AssertContains(string.Format("<Punto_vta>{0}</Punto_vta>", expectedPunto_vta), actualXmlValue);
				AssertContains("<Cbte_nro></Cbte_nro>", actualXmlValue);
			}
		}

		public void Test_PuntoVentaAndNumeroComprobante()
		{
			AccComplianceSequence accComplianceSequence = null;

			var complianceSequence = new Mock<IComplianceSequenceRetriever>();
			var fexBuilder = new ClsFEXRequestBuilder();
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequence.Object);
			var builder = fexBuilder as IClsFEXRequestBuilder;

			var transactionInfo = GetTransactionInfo();

			var prefixList = new List<string>() { "0012", "00013", "10013" };
			accComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();

			prefixList.ForEach(prefix =>
			{
				accComplianceSequence.XD_Prefix = prefix;
				AssertPuntoVentaAndNumeroComprobante(prefix, ZString.Empty);
			});

			accComplianceSequence.XD_Prefix = "0011";
			transactionInfo.TransactionReference = "000125000001001";

			AssertPuntoVentaAndNumeroComprobante("0011", ZString.Empty);

			void AssertPuntoVentaAndNumeroComprobante(string expectedPunto_vta, string expectedCbte_nro)
			{
				complianceSequence.Setup(x => x.GetComplianceSequenceFromSubType(It.IsAny<ZString>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>())).Returns(accComplianceSequence);
				var actualXmlValue = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString();

				complianceSequence.Verify(x => x.GetComplianceSequenceFromSubType(It.IsAny<ZString>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>()));
				AssertContains(string.Format("<Punto_vta>{0}</Punto_vta>", expectedPunto_vta), actualXmlValue);
				AssertContains(string.Format("<Cbte_nro>{0}</Cbte_nro>", expectedCbte_nro), actualXmlValue);
			}
		}

		#endregion

		#region Items

		public void TestClsFEXRequestBuilderItemsCheckNulls()
		{
			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();

			var transaction = GetTransactionInfo();
			AssertNulls();

			transaction.TransactionType = null;
			AssertNulls();

			transaction.TransactionType = TransactionType.INV;
			AssertNulls();

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			AssertNulls();

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance));
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].GLAccount = new GLAccount();
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].ChargeCode = new ChargeCode();
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].Description = null;
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].Description = ZString.Empty;
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].OSTotalAmount = null;
			AssertNullsWithChargeLines();

			transaction.PostingJournalCollection[0].OSTotalAmount = 0m;
			AssertNullsWithChargeLines();

			void AssertNulls()
			{
				var expectedValue = @"<Items>";

				var actualXmlValue = builder.BuildXML(transaction, "", string.Empty, Factory).ToString();

				AssertNotContains("Items sub node must not be present in actualXmlValue when there are not items", expectedValue, actualXmlValue);
			}

			void AssertNullsWithChargeLines()
			{
				var expectedValue = @"  <Items>
    <Item>
      <Pro_codigo></Pro_codigo>
      <Pro_ds></Pro_ds>
      <Pro_qty>1</Pro_qty>
      <Pro_umed>7</Pro_umed>
      <Pro_precio_uni>0.000000</Pro_precio_uni>
      <Pro_total_item>0.00</Pro_total_item>
    </Item>
  </Items>";
				AssertItems(transaction, expectedValue);
			}
		}

		public void TestClsFEXRequestBuilderItemsWithChargeLinesMissingChargeCode()
		{
			var transaction = GetTransactionInfo();
			transaction.TransactionType = TransactionType.INV;
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				GLAccount = new GLAccount() { AccountCode = "6051.10.21" },
				Description = "CONFECCION DE BL",
				OSTotalAmount = 890.5600m,
			});

			var expectedValue = @"  <Items>
    <Item>
      <Pro_codigo>6051.10.21</Pro_codigo>
      <Pro_ds>CONFECCION DE BL</Pro_ds>
      <Pro_qty>1</Pro_qty>
      <Pro_umed>7</Pro_umed>
      <Pro_precio_uni>890.560000</Pro_precio_uni>
      <Pro_total_item>890.56</Pro_total_item>
    </Item>
  </Items>";

			AssertItems(transaction, expectedValue);

			transaction.TransactionType = TransactionType.CRD;

			expectedValue = @"  <Items>
    <Item>
      <Pro_codigo>6051.10.21</Pro_codigo>
      <Pro_ds>CONFECCION DE BL</Pro_ds>
      <Pro_qty>1</Pro_qty>
      <Pro_umed>7</Pro_umed>
      <Pro_precio_uni>-890.560000</Pro_precio_uni>
      <Pro_total_item>-890.56</Pro_total_item>
    </Item>
  </Items>";

			AssertItems(transaction, expectedValue);
		}

		public void TestClsFEXRequestBuilderItemsWithChargeLinesWithChargeCode()
		{
			var transaction = GetTransactionInfo();
			transaction.TransactionType = TransactionType.INV;
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ChargeCode = new ChargeCode() { Code = "WOPK" },
				Description = "LOADING AND UNLOADING",
				OSTotalAmount = -178.0000m,
			});

			var expectedValue = @"  <Items>
    <Item>
      <Pro_codigo>WOPK</Pro_codigo>
      <Pro_ds>LOADING AND UNLOADING</Pro_ds>
      <Pro_qty>1</Pro_qty>
      <Pro_umed>7</Pro_umed>
      <Pro_precio_uni>-178.000000</Pro_precio_uni>
      <Pro_total_item>-178.00</Pro_total_item>
    </Item>
  </Items>";

			AssertItems(transaction, expectedValue);

			transaction.TransactionType = TransactionType.CRD;

			expectedValue = @"  <Items>
    <Item>
      <Pro_codigo>WOPK</Pro_codigo>
      <Pro_ds>LOADING AND UNLOADING</Pro_ds>
      <Pro_qty>1</Pro_qty>
      <Pro_umed>7</Pro_umed>
      <Pro_precio_uni>178.000000</Pro_precio_uni>
      <Pro_total_item>178.00</Pro_total_item>
    </Item>
  </Items>";

			AssertItems(transaction, expectedValue);
		}

		void AssertItems(TransactionInfo transaction, string expectedValue)
		{
			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();

			var actualXmlValue = builder.BuildXML(transaction, "", string.Empty, Factory).ToString();

			AssertContains(expectedValue, actualXmlValue);
			AssertNotContains("<Pro_bonificacion>", actualXmlValue);
		}

		#endregion

		#region CbteTipo

		public void TestCbte_Tipo()
		{
			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();

			var transactionInfo = GetTransactionInfo();

			AssertCbte_Tipo(null, "<Cbte_Tipo></Cbte_Tipo>");

			AssertCbte_Tipo(ZString.Empty, "<Cbte_Tipo></Cbte_Tipo>");

			AssertCbte_Tipo(ArgentinaComplianceInfo.ComplianceSubTypeCodes.OCZ, "<Cbte_Tipo></Cbte_Tipo>");

			AssertCbte_Tipo(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE, "<Cbte_Tipo>19</Cbte_Tipo>");

			AssertCbte_Tipo(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDE, "<Cbte_Tipo>20</Cbte_Tipo>");

			AssertCbte_Tipo(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCE, "<Cbte_Tipo>21</Cbte_Tipo>");

			void AssertCbte_Tipo(ZString? complianceSubtype, string expectedXmlResult)
			{
				transactionInfo.ComplianceSubType = complianceSubtype;

				var actualXmlResult = builder.BuildXML(transactionInfo, "", string.Empty, Factory).ToString();

				AssertContains(expectedXmlResult, actualXmlResult);
			}
		}

		#endregion

		#region AfipCodesCountries

		public void TestOrganizationAdress_WithMissingOrganizationAddress()
		{
			var transaction = GetTransactionInfo();

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Dst_cmp></Dst_cmp>", actualXmlValue);
		}

		public void TestOrganizationAdress_OrganizationAddressIsNull()
		{
			var transaction = GetTransactionInfo();
			transaction.OrganizationAddress = null;

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Dst_cmp></Dst_cmp>", actualXmlValue);
		}

		public void TestOrganizationAdress_OrganizationAddressIsEmpty()
		{
			var transaction = GetTransactionInfo();
			transaction.OrganizationAddress = new OrganizationAddress() { Address1 = string.Empty };

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Dst_cmp></Dst_cmp>", actualXmlValue);
		}

		public void TestCountry_CountryIsEmpty()
		{
			var transaction = GetTransactionInfo();
			transaction.OrganizationAddress = new OrganizationAddress() { Country = new Country() };

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Dst_cmp></Dst_cmp>", actualXmlValue);
		}

		public void TestCountry_CountryIsNull()
		{
			var transaction = GetTransactionInfo();
			transaction.OrganizationAddress = new OrganizationAddress() { Country = null };

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Dst_cmp></Dst_cmp>", actualXmlValue);
		}

		public void TestAfipCodeCountry_inValid()
		{
			var transaction = GetTransactionInfo();
			transaction.OrganizationAddress = new OrganizationAddress() { Country = new Country() { Code = "XX" } };

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Dst_cmp></Dst_cmp>", actualXmlValue);
		}

		public void TestAfipCodeCountry_Valid()
		{
			var transaction = GetTransactionInfo();
			transaction.OrganizationAddress = new OrganizationAddress() { Country = new Country() { Code = CountryCodes.Turkey } };

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Dst_cmp>436</Dst_cmp>", actualXmlValue);
		}
		#endregion

		#region Buyer's Info

		public void TestBuyerInfo_Obs_comerciales_Osb_IsRemoved_FromXML()
		{
			var transactionInfo = GetTransactionInfo();
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> { new RegistrationNumber() });

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.fexv1/", string.Empty, Factory).ToString();

			AssertNotContains("<Obs></Obs>", actualXmlValue);
			AssertNotContains("<Obs_comerciales></Obs_comerciales>", actualXmlValue);
		}

		public void TestBuyerInfo_OrganisationAddress_NullOrEmptyChecks()
		{
			var expectedResult = @"
<Cliente></Cliente>
<Domicilio_cliente></Domicilio_cliente>
";

			CombineAssertions(() =>
			{
				var transactionInfo = GetTransactionInfo();
				AssertResult(expectedResult);

				transactionInfo.OrganizationAddress = null;
				AssertResult(expectedResult);

				transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
				AssertResult(expectedResult);

				void AssertResult(string resultNotContain)
				{
					var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
					var actualXmlValue = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.fexv1/", string.Empty, Factory).ToString().Replace(" ", "");

					AssertContains(expectedResult, actualXmlValue);
					AssertNotContains("<Id_impositivo></Id_impositivo>", actualXmlValue);
					AssertNotContains("<Cuit_pais_cliente></Cuit_pais_cliente>", actualXmlValue);
				}
			});
		}

		public void TestBuyerInfo_RegistrationNumber_NullOrEmpty_Checks()
		{
			var expectedResult = @"
<Cliente>XXX</Cliente>
<Domicilio_cliente>Address1</Domicilio_cliente>
";

			CombineAssertions(() =>
			{
				var transactionInfo = GetTransactionInfo();
				transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
				transactionInfo.OrganizationAddress.CompanyName = "XXX";
				transactionInfo.OrganizationAddress.Address1 = "Address1";

				AssertResult(expectedResult);

				transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => null);
				AssertResult(expectedResult);

				transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> { new RegistrationNumber() });

				AssertResult(expectedResult);

				void AssertResult(string result)
				{
					var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
					var actualXmlValue = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.fexv1/", string.Empty, Factory).ToString().Replace(" ", "");

					AssertContains(expectedResult, actualXmlValue);
					AssertNotContains("<Id_impositivo></Id_impositivo>", actualXmlValue);
					AssertNotContains("<Cuit_pais_cliente></Cuit_pais_cliente>", actualXmlValue);
				}
			});
		}

		public void TestBuyerInfo_WhenOrganizationRegistrationCode_ContainsCUFCusCode()
		{
			var transactionInfo = GetTransactionInfo();
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Uruguay };
			transactionInfo.OrganizationAddress.CompanyName = "Argentina Test Export Invoice";
			transactionInfo.OrganizationAddress.Address1 = "Address1 Address1 Address1";

			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			{
				new RegistrationNumber() { CountryOfIssue = new Country { Code = "AR" }, Type = new RegistrationNumberType { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL }, Value = "123" },
				new RegistrationNumber() { CountryOfIssue = new Country { Code = "UY" }, Type = new RegistrationNumberType { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL }, Value = "345" },
				new RegistrationNumber() { CountryOfIssue = new Country { Code = "UY" }, Type = new RegistrationNumberType { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUF }, Value = "20-23454229-0" }
			});

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.fexv1/", string.Empty, Factory).ToString();

			AssertContains("<Cliente>Argentina Test Export Invoice</Cliente>", actualXmlValue);
			AssertContains("<Domicilio_cliente>Address1 Address1 Address1</Domicilio_cliente>", actualXmlValue);
			AssertContains("<Cuit_pais_cliente>20234542290</Cuit_pais_cliente>", actualXmlValue);

			AssertNotContains("<Id_impositivo></Id_impositivo>", actualXmlValue);

			transactionInfo.OrganizationAddress.RegistrationNumberCollection.First(x => x.Type.Code.Value == ArgentinaOrgCusCodeInfo.OrgCusCodes.CUF).Value = "*5454A.232-2323.+33";

			actualXmlValue = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.fexv1/", string.Empty, Factory).ToString();

			AssertContains("<Cuit_pais_cliente>5454232232333</Cuit_pais_cliente>", actualXmlValue);
		}

		public void TestBuyerInfo_WhenOrganizationRegistrationCode_Not_ContainsCUFCusCode()
		{
			var transactionInfo = GetTransactionInfo();
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress.CompanyName = "Argentina Test Export Invoice";
			transactionInfo.OrganizationAddress.Address1 = "Address1 Address1 Address1";

			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			{
				new RegistrationNumber() { CountryOfIssue = new Country { Code = "AR" }, Type = new RegistrationNumberType { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL }, Value = "123" },
				new RegistrationNumber() { CountryOfIssue = new Country { Code = "UY" }, Type = new RegistrationNumberType { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT }, Value = "345" },
				new RegistrationNumber() { CountryOfIssue = new Country { Code = "UY" }, Type = new RegistrationNumberType { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI }, Value = "678" }
			});

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.fexv1/", string.Empty, Factory).ToString();

			AssertContains("<Cliente>Argentina Test Export Invoice</Cliente>", actualXmlValue);
			AssertContains("<Domicilio_cliente>Address1 Address1 Address1</Domicilio_cliente>", actualXmlValue);
			AssertContains("<Id_impositivo>123</Id_impositivo>", actualXmlValue);

			AssertNotContains("<Cuit_pais_cliente></Cuit_pais_cliente>", actualXmlValue);
		}

		#endregion

		#region TotalAmount

		public void TestBuildXmlClsFEXRequestBuilder_TotalAmountNegative()
		{
			var transaction = GetTransactionInfo();
			transaction.OSTotal = 1500.00;

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXml = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Imp_total>-1500.00</Imp_total>", actualXml);
		}

		public void TestBuildXmlClsFEXRequestBuilder_TotalAmountPositive()
		{
			var transaction = GetTransactionInfo();
			transaction.OSTotal = -1458.00;

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXml = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Imp_total>1458.00</Imp_total>", actualXml);
		}

		public void TestBuildXmlClsFEXRequestBuilder_CbteINV()
		{
			var transaction = GetTransactionInfo();
			transaction.OSTotal = 180.00;
			transaction.TransactionType = TransactionType.INV;

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXml = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Imp_total>180.00</Imp_total>", actualXml);
		}

		public void TestBuildXmlClsFEXRequestBuilder_TotalAmountOnZero()
		{
			var transaction = GetTransactionInfo();

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXml = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Imp_total>0.00</Imp_total>", actualXml);
		}

		public void TestBuildXmlClsFEXRequestBuilder_PermisoExistenteEmpty()
		{
			var transaction = GetTransactionInfo();

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXml = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Permiso_existente></Permiso_existente>", actualXml);
		}

		public void TestBuildXmlClsFEXRequestBuilder_Idioma_Cbte_DefaultOne()
		{
			var transaction = GetTransactionInfo();

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXml = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("<Idioma_cbte>1</Idioma_cbte>", actualXml);
		}

		public void TestBuildXmlClsFEXRequestBuilder_PermisosOpcionalesNotContains()
		{
			var transaction = GetTransactionInfo();

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXml = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertNotContains("<Opcionales>", actualXml);
			AssertNotContains("<Permisos>", actualXml);
		}

		#endregion

		#region CmpsAsoc

		[ExpectNoExceptions]
		public void TestBuilder_ComplianceSequence_CbtesAsoc()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDE;
			transaction.TransactionReference = "000123000000012";
			transaction.Branch.Code = branch.GB_Code;
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDE;
			transaction.OriginalReference.OriginalTransactionReference = "000123000000011";
			transaction.Department.Code = department.GE_Code;

			Factory.Save();

			var fexBuilder = new ClsFEXRequestBuilder();
			var complianceSequenceMock = new Mock<IComplianceSequenceRetriever>();
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequenceMock.Object);

			var builder = fexBuilder as IClsFEXRequestBuilder;
			var cfe = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory);

			complianceSequenceMock.Verify(x => x.GetComplianceSequenceFromSubType(transaction.OriginalReference.OriginalTransactionComplianceSubType.GetValueOrDefault(),
				branch.GB_GC, branch.PK, department.PK, It.IsAny<ZDateTime>()), Times.Once);
		}

		public void TestCmps_asoc_OriginalReferenceIsNull()
		{
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = "TDE";

			var expectedValue = @"
<Cmps_asoc>
";
			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString();

			AssertNotContains("Cmps_asoc sub node must not be present in actualXmlValue when OriginalReference is null", expectedValue, actualXmlValue);
		}

		public void TestCmps_asoc_OriginalReferenceIsEmpty()
		{
			var transaction = GetTransactionInfo();
			transaction.OriginalReference = new OriginalReference();

			var expectedValue = @"
<Cmps_asoc>
";
			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString();

			AssertNotContains("Cmps_asoc sub node must not be present in actualXmlValue when OriginalReference is empty", expectedValue, actualXmlValue);
		}

		public void TestCmps_asoc_TransactionComplianceSubType_IsNotCreditNote()
		{
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = "TXE";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXE";

			var expectedValue = @"
<Cmps_asoc>
";

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString();

			AssertNotContains("Cmps_asoc sub node must  be present for transantion credit note type ", expectedValue, actualXmlValue);
		}

		public void TestCmps_asoc_TransactionComplianceSubType_IsNotDebitNote()
		{
			var transaction = GetTransactionInfo();
			transaction.TransactionType = TransactionType.INV;
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXE";

			var expectedValue = @"
<Cmps_asoc>
";

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString();

			AssertNotContains("Cmps_asoc sub node must not be present for non debit note transaction", expectedValue, actualXmlValue);
		}

		public void TestCmps_asoc_TransactionComplianceSubType_IsCreditNote()
		{
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = "TCE";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXE";
			var originalAccComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();

			var expectedValue = @"
<Cmps_asoc>
";
			var complianceSequence = new Mock<IComplianceSequenceRetriever>();
			var fexBuilder = new ClsFEXRequestBuilder();

			complianceSequence.Setup(x => x.GetComplianceSequenceFromSubType(It.IsAny<ZString>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>())).Returns(originalAccComplianceSequence);
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequence.Object);
			var builder = fexBuilder as IClsFEXRequestBuilder;
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("Cmps_asoc sub node must be present for credit note transactions", expectedValue, actualXmlValue);
		}

		public void TestCmps_asoc_Transaction_IsDebitNote()
		{
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = "TDE";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXA";
			transaction.IsCancelled = false;
			var originalAccComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();

			var expectedValue = @"
<Cmps_asoc>
";
			var complianceSequence = new Mock<IComplianceSequenceRetriever>();
			var fexBuilder = new ClsFEXRequestBuilder();

			complianceSequence.Setup(x => x.GetComplianceSequenceFromSubType(It.IsAny<ZString>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>())).Returns(originalAccComplianceSequence);
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequence.Object);
			var builder = fexBuilder as IClsFEXRequestBuilder;
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains("CbtesAsoc sub node must  be present for debit note transaction", expectedValue, actualXmlValue);
		}

		public void TestCmps_asoc__When_OriginalTransactionComplianceSubType_IsPopulate()
		{
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = "TCE";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXE";
			transaction.OriginalReference.OriginalTransactionReference = "FCA0023300002526";
			var originalAccComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			originalAccComplianceSequence.XD_SequenceClass = "TXE";
			originalAccComplianceSequence.XD_Prefix = "00233";

			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT };
			regItem1.Value = "30-12345678-0";
			transaction.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1 });

			var expectedValue = @"
<Fecha_cbte></Fecha_cbte>
<Cbte_Tipo>21</Cbte_Tipo>
<Punto_vta>00233</Punto_vta>
<Cbte_nro></Cbte_nro>
<Tipo_expo>2</Tipo_expo>
<Permiso_existente></Permiso_existente>
<Dst_cmp></Dst_cmp>
<Cliente></Cliente>
<Domicilio_cliente></Domicilio_cliente>
<Moneda_Id></Moneda_Id>
<Moneda_ctz></Moneda_ctz>
<Imp_total>0.00</Imp_total>
<Cmps_asoc>
<Cmp_asoc>
<Cbte_tipo>19</Cbte_tipo>
<Cbte_punto_vta>FCA00</Cbte_punto_vta>
<Cbte_nro>23300002526</Cbte_nro>
<Cbte_cuit>30123456780</Cbte_cuit>
</Cmp_asoc>
</Cmps_asoc>
<Idioma_cbte>1</Idioma_cbte>
</Cmp>";
			var complianceSequence = new Mock<IComplianceSequenceRetriever>();
			var fexBuilder = new ClsFEXRequestBuilder();

			complianceSequence.Setup(x => x.GetComplianceSequenceFromSubType(It.IsAny<ZString>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>())).Returns(originalAccComplianceSequence);
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequence.Object);

			var builder = fexBuilder as IClsFEXRequestBuilder;
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", "30-12345678-0", Factory).ToString().Replace(" ", "");

			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestCmps_asoc_When_OriginalTransactionComplianceSubType_DontPopulate()
		{
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = "TCE";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionNumber = "TXE0023300002526";
			transaction.OriginalReference.OriginalTransactionDate = ZDateTime.Today;
			var originalAccComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			originalAccComplianceSequence.XD_SequenceClass = "TXA";
			originalAccComplianceSequence.XD_Prefix = "00233";

			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT };
			regItem1.Value = "30-12345678-0";
			transaction.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1 });

			var expectedValue = @"
<Cmps_asoc>
<Cmp_asoc>
<Cbte_tipo>19</Cbte_tipo>
<Cbte_punto_vta>00233</Cbte_punto_vta>
<Cbte_nro>00002526</Cbte_nro>
<Cbte_cuit>30123456780</Cbte_cuit>
</Cmp_asoc>
</Cmps_asoc>
";
			var complianceSequence = new Mock<IComplianceSequenceRetriever>();
			var fexBuilder = new ClsFEXRequestBuilder();
			complianceSequence.Setup(x => x.GetComplianceSequenceFromSubType(It.IsAny<ZString>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>())).Returns(originalAccComplianceSequence);
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequence.Object);

			var builder = (IClsFEXRequestBuilder)new ClsFEXRequestBuilder();
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", "30-12345678-0", Factory).ToString().Replace(" ", "");

			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestCmps_asoc__When_OriginalTransactionComplianceSubType_IsPopulate_And_ComplianceSequenceBookConfigure()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var transaction = GetTransactionInfo();

			transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			transaction.Branch.Code = branch.GB_Code;
			transaction.Department.Code = department.GE_Code;

			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXA";
			transaction.OriginalReference.OriginalTransactionNumber = "FCA0023300002526";
			var originalAccComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			originalAccComplianceSequence.XD_GC_Company = branch.GB_GC;
			originalAccComplianceSequence.XD_GB_BranchOwner = branch.PK;
			originalAccComplianceSequence.XD_GE_Department = department.PK;
			originalAccComplianceSequence.XD_SequenceClass = "TXA";
			originalAccComplianceSequence.XD_Prefix = "00233";
			Factory.Save();

			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT };
			regItem1.Value = "30-12345678-0";
			transaction.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1 });

			var expectedValue = @"
<Fecha_cbte></Fecha_cbte>
<Cbte_Tipo>1</Cbte_Tipo>
<Punto_vta>00233</Punto_vta>
<Cbte_nro></Cbte_nro>
<Tipo_expo>2</Tipo_expo>
<Permiso_existente></Permiso_existente>
<Dst_cmp></Dst_cmp>
<Cliente></Cliente>
<Domicilio_cliente></Domicilio_cliente>
<Moneda_Id></Moneda_Id>
<Moneda_ctz></Moneda_ctz>
<Imp_total>0.00</Imp_total>
<Idioma_cbte>1</Idioma_cbte>
</Cmp>";
			var fexBuilder = new ClsFEXRequestBuilder();
			var complianceSequenceMock = new Mock<IComplianceSequenceRetriever>();
			complianceSequenceMock.Setup(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, branch.GB_GC, branch.PK, department.PK, It.IsAny<ZDateTime>())).Returns(originalAccComplianceSequence);
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequenceMock.Object);

			var builder = fexBuilder as IClsFEXRequestBuilder;
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", "30-12345678-0", Factory).ToString().Replace(" ", "");

			complianceSequenceMock.Verify(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, branch.GB_GC, branch.PK, department.PK, It.IsAny<ZDateTime>()), Times.Once);
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestCmps_asoc_When_OriginalTransactionComplianceSubType_DontPopulate_And_ComplianceBookSequenceConfigured()
		{
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = "TDE";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionNumber = "TXE0023300002526";
			transaction.OriginalReference.OriginalTransactionDate = ZDateTime.Today;
			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT };
			regItem1.Value = "30-12345678-0";
			transaction.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1 });

			var originalAccComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			originalAccComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			originalAccComplianceSequence.XD_SequenceClass = "TXE";
			originalAccComplianceSequence.XD_Prefix = "FCA#00233-";

			var expectedValue = @"
<Cmps_asoc>
<Cmp_asoc>
<Cbte_tipo>19</Cbte_tipo>
<Cbte_punto_vta>00233</Cbte_punto_vta>
<Cbte_nro>00002526</Cbte_nro>
<Cbte_cuit>30123456780</Cbte_cuit>
</Cmp_asoc>
</Cmps_asoc>
";
			var complianceSequence = new Mock<IComplianceSequenceRetriever>();
			var fexBuilder = new ClsFEXRequestBuilder();
			complianceSequence.Setup(x => x.GetComplianceSequenceFromSubType(It.IsAny<ZString>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>())).Returns(originalAccComplianceSequence);
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequence.Object);

			var builder = fexBuilder as IClsFEXRequestBuilder;
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", "30-12345678-0", Factory).ToString().Replace(" ", "");

			AssertContains(expectedValue, actualXmlValue);
		}

		[ExpectNoExceptions]
		public void TestGetComplianceSequenceFromSubTypeException()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXA";
			transaction.OriginalReference.OriginalTransactionReference = "FCA0023300002526";
			transaction.Branch.Code = branch.GB_Code;
			transaction.Department.Code = department.GE_Code;
			Factory.Save();

			var fexBuilder = new ClsFEXRequestBuilder();
			var complianceSequenceMock = new Mock<IComplianceSequenceRetriever>();

			complianceSequenceMock.Setup(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, branch.GB_GC, branch.PK, department.PK, It.IsAny<ZDateTime>())).Throws(new MultipleComplianceSequenceFoundException());
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequenceMock.Object);

			var builder = fexBuilder as IClsFEXRequestBuilder;
			var cfe = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", "30-12345678-0", Factory).ToString().Replace(" ", "");

			complianceSequenceMock.Verify(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, branch.GB_GC, branch.PK, department.PK, It.IsAny<ZDateTime>()), Times.Once);
		}

		public void TestCmps_asoc_OriginalTransactionComplianceSubType_IsPresent_And_OriginalTransactionReferece_DontPopulate()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var transaction = GetTransactionInfo();
			transaction.ComplianceSubType = "TDE";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXE";
			transaction.OriginalReference.OriginalTransactionJobInvoiceNumber = "S000001";
			transaction.OriginalReference.OriginalTransactionNumber = "00001010";
			transaction.OriginalReference.OriginalTransactionDate = ZDateTime.Today;
			transaction.Branch.Code = branch.GB_Code;
			transaction.Department.Code = department.GE_Code;
			var originalAccComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();

			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT };
			regItem1.Value = "30-12345678-0";
			transaction.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1 });

			var expectedValue = @"
<Cmps_asoc>
<Cmp_asoc>
<Cbte_tipo>19</Cbte_tipo>
<Cbte_punto_vta></Cbte_punto_vta>
<Cbte_nro></Cbte_nro>
<Cbte_cuit>30123456780</Cbte_cuit>
</Cmp_asoc>
</Cmps_asoc>
";
			var complianceSequence = new Mock<IComplianceSequenceRetriever>();
			var fexBuilder = new ClsFEXRequestBuilder();

			complianceSequence.Setup(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, branch.GB_GC, branch.PK, department.PK, It.IsAny<ZDateTime>())).Returns(originalAccComplianceSequence);
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequence.Object);

			var builder = fexBuilder as IClsFEXRequestBuilder;
			var actualXmlValue = builder.BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", "30-12345678-0", Factory).ToString().Replace(" ", "");

			AssertContains(expectedValue, actualXmlValue);
		}

		#endregion

		#region Id

		public void TestId()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { Branch = new Branch() };
			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();

			var exportEInvoiceBuilder = new ClsFEXRequestBuilder();
			exportEInvoiceBuilder.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

			var returnGetRandomLog = 123456789012345;

			transactionInfoHelperMock.Setup(x => x.GetRandomLog(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>())).Returns(returnGetRandomLog);

			var actualXmlValue = ((IClsFEXRequestBuilder)exportEInvoiceBuilder).BuildXML(transaction, "http://ar.gov.afip.dif.FEV1/", "30-12345678-0", Factory).ToString().Replace(" ", "");

			transactionInfoHelperMock.Verify(x => x.GetRandomLog(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()), Times.Once);

			AssertContains("Id", $"<Id>{returnGetRandomLog}</Id>", actualXmlValue);
		}

		#endregion

		#region Implementation

		TransactionInfo GetTransactionInfo()
		{
			return new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch() { Code = "BUE" },
				TransactionType = TransactionType.CRD,
				Department = new Department() { Code = "BUE" }
			};
		}

		#endregion
	}
}
