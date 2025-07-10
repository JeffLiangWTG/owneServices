using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Accounting.TaxFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	public class FEDetRequestBuilderTest : TestCaseWithFactory
	{
		const string FEV1NameSpace = "http://ar.gov.afip.dif.FEV1/";

		public void TestItUsedAsDependency()
		{
			AssertType<FEDetRequestBuilder>(new LocalEInvoiceXmlBuilder().FeDetRequestBuilder_ExposedForTestOnly);
		}

		#region FEDetRequestDatesAndConcept

		[TestDate(2020, 6, 21)]
		public void TestBuildXMLFECAEDetRequest_WithDatesAndConcept()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionDate = ZDateTime.Today;
			transaction.DueDate = ZDateTime.Today;

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString();

			AssertContains("<Concepto>2</Concepto>", actualXmlValue);
			AssertContains("<CbteFch>20200621</CbteFch>", actualXmlValue);
			AssertContains("<FchServDesde>20200621</FchServDesde>", actualXmlValue);
			AssertContains("<FchServHasta>20200621</FchServHasta>", actualXmlValue);
			AssertContains("<FchVtoPago>20200621</FchVtoPago>", actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_WithConceptAndEmptyDates()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionDate = ZDateTime.Empty;
			transaction.DueDate = ZDateTime.Empty;

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString();

			AssertContains("<Concepto>2</Concepto>", actualXmlValue);
			AssertContains("<CbteFch></CbteFch>", actualXmlValue);
			AssertContains("<FchServDesde></FchServDesde>", actualXmlValue);
			AssertContains("<FchServHasta></FchServHasta>", actualXmlValue);
			AssertContains("<FchVtoPago></FchVtoPago>", actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_WithConceptAndNullDates()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionDate = null;
			transaction.DueDate = null;

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString();

			AssertContains("<Concepto>2</Concepto>", actualXmlValue);
			AssertContains("<CbteFch></CbteFch>", actualXmlValue);
			AssertContains("<FchServDesde></FchServDesde>", actualXmlValue);
			AssertContains("<FchServHasta></FchServHasta>", actualXmlValue);
			AssertContains("<FchVtoPago></FchVtoPago>", actualXmlValue);
		}

		[TestDate(2020, 6, 21)]
		public void TestBuildXMLFECAEDetRequest_FchVtoPago_SubTypeCode()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.DueDate = ZDateTime.Today;

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();

			foreach (var subType in CountryComplianceFactory.GetIComplianceSubTypeCodeProvider(CountryCodes.Argentina)?.GetComplianceSubTypes())
			{
				var complianceSubType = subType.Code;

				transaction.ComplianceSubType = complianceSubType;

				var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString();

				if (ArgentinaConstants.MiPymeDebitOrCreditNoteComplianceSubTypeList.Contains(complianceSubType))
				{
					AssertNotContains($"When Compliance Sub Type is {complianceSubType}, FchVtoPago should be not included", "<FchVtoPago>", actualXmlValue);
				}
				else
				{
					AssertContains($"when Compliance Sub Type is {complianceSubType}, should be", "<FchVtoPago>20200621</FchVtoPago>", actualXmlValue);
				}
			}
		}

		#endregion

		#region FEDetRequestMonIdAndMonCotiz

		public void TestBuildXMLFECAEDetRequest_WithTransactionInForeignCurrency()
		{
			var transactionInForeignCurrency = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInForeignCurrency.ExchangeRate = 19.24M;
			transactionInForeignCurrency.OSCurrency = new Currency() { Code = "AUD" };

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionInForeignCurrency, FEV1NameSpace, Factory).ToString().Replace(" ", "");

			var expectedValue = @"
<MonId>026</MonId>
<MonCotiz>19.240000</MonCotiz>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_WithTransactionInLocalCurrency()
		{
			var transactionInLocalCurrency = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInLocalCurrency.ExchangeRate = 1.00M;
			transactionInLocalCurrency.OSCurrency = new Currency { Code = "ARS" };

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionInLocalCurrency, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<MonId>PES</MonId>
<MonCotiz>1</MonCotiz>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_WithMissingCurrency()
		{
			var transactionWithOutCurrency = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionWithOutCurrency, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<MonId></MonId>
<MonCotiz></MonCotiz>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_TransactionNonAcceptedOsCurrency()
		{
			var transactionWithnoValidCurrency = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionWithnoValidCurrency.ExchangeRate = 15.00M;
			transactionWithnoValidCurrency.OSCurrency = new Currency { Code = "XXX" };

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionWithnoValidCurrency, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<MonId></MonId>
<MonCotiz></MonCotiz>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		#endregion

		#region FEDetRequestTotalsAmount

		public void TestBuildXMLFECAEDetRequest_TotalsAmount_MissingPostingJournal()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<ImpTotal>0.00</ImpTotal>
<ImpTotConc>0.00</ImpTotConc>
<ImpNeto>0.00</ImpNeto>
<ImpOpEx>0.00</ImpOpEx>
<ImpTrib>0.00</ImpTrib>
<ImpIVA>0.00</ImpIVA>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_TotalsAmount_EmptyPostingJournal()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<ImpTotal>0.00</ImpTotal>
<ImpTotConc>0.00</ImpTotConc>
<ImpNeto>0.00</ImpNeto>
<ImpOpEx>0.00</ImpOpEx>
<ImpTrib>0.00</ImpTrib>
<ImpIVA>0.00</ImpIVA>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_InvoiceTotalAmount_ImpTotTag()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			var transactionLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 1,
				Description = "Description of line 1",
				OSAmount = 100m,
				OSGSTVATAmount = 21m,
				OSTotalAmount = 121m,
				LocalAmount = 100m,
				LocalGSTVATAmount = 21m,
				LocalTotalAmount = 121m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};
			var transactionLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 2,
				Description = "Description of line 1",
				OSAmount = 200m,
				OSGSTVATAmount = 42m,
				OSTotalAmount = 242m,
				LocalAmount = 200m,
				LocalGSTVATAmount = 42m,
				LocalTotalAmount = 242m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};

			transaction.PostingJournalCollection.Add(transactionLine1);
			transaction.PostingJournalCollection.Add(transactionLine2);
			transaction.OSTotal = 363m;

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<ImpTotal>363.00</ImpTotal>
<ImpTotConc>0.00</ImpTotConc>
<ImpNeto>0.00</ImpNeto>
<ImpOpEx>0.00</ImpOpEx>
<ImpTrib>0.00</ImpTrib>
<ImpIVA>0.00</ImpIVA>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_InvoiceTotalOSGSTVATAmount_ImpIVATag()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			var transactionLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 1,
				Description = "Description of line 1",
				OSAmount = 100m,
				OSGSTVATAmount = 21m,
				OSTotalAmount = 121m,
				LocalAmount = 100m,
				LocalGSTVATAmount = 21m,
				LocalTotalAmount = 121m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};
			var transactionLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 2,
				Description = "Description of line 1",
				OSAmount = 200m,
				OSGSTVATAmount = 42m,
				OSTotalAmount = 242m,
				LocalAmount = 200m,
				LocalGSTVATAmount = 42m,
				LocalTotalAmount = 242m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};

			transaction.PostingJournalCollection.Add(transactionLine1);
			transaction.PostingJournalCollection.Add(transactionLine2);
			transaction.OSTotal = 363m;
			transaction.OSGSTVATAmount = 63m;

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<ImpTotal>363.00</ImpTotal>
<ImpTotConc>0.00</ImpTotConc>
<ImpNeto>0.00</ImpNeto>
<ImpOpEx>0.00</ImpOpEx>
<ImpTrib>0.00</ImpTrib>
<ImpIVA>63.00</ImpIVA>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_InvoiceTotalTaxType_EXL_NOT_ImpTotConcTag()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.TransactionType = TransactionType.INV;

			var transactionLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 1,
				Description = "Description of line 1",
				OSAmount = 100m,
				OSGSTVATAmount = 21m,
				OSTotalAmount = 121m,
				LocalAmount = 100m,
				LocalGSTVATAmount = 21m,
				LocalTotalAmount = 121m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};
			var transactionLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 2,
				Description = "Description of line 1",
				OSAmount = 200m,
				OSGSTVATAmount = 42m,
				OSTotalAmount = 242m,
				LocalAmount = 200m,
				LocalGSTVATAmount = 42m,
				LocalTotalAmount = 242m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};

			var transactionLine3 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 3,
				Description = "Description of line 3",
				OSAmount = 100m,
				OSTotalAmount = 100m,
				LocalAmount = 100m,
				LocalTotalAmount = 100m,
				VATTaxID = new TaxID()
				{
					TaxCode = "NOT",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "NOT" }
				}
			};
			var transactionLine4 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 2,
				Description = "Description of line 4",
				OSAmount = 200m,
				OSTotalAmount = 200m,
				LocalAmount = 200m,
				LocalTotalAmount = 200m,
				VATTaxID = new TaxID()
				{
					TaxCode = "EXL",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "EXL" }
				}
			};

			transaction.PostingJournalCollection.Add(transactionLine1);
			transaction.PostingJournalCollection.Add(transactionLine2);
			transaction.PostingJournalCollection.Add(transactionLine3);
			transaction.PostingJournalCollection.Add(transactionLine4);
			transaction.OSTotal = 663m;
			transaction.OSGSTVATAmount = 63m;

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<ImpTotal>663.00</ImpTotal>
<ImpTotConc>300.00</ImpTotConc>
<ImpNeto>0.00</ImpNeto>
<ImpOpEx>0.00</ImpOpEx>
<ImpTrib>0.00</ImpTrib>
<ImpIVA>63.00</ImpIVA>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_InvoiceTotalTaxType_RAT_CAP_ImpNetoTag()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.TransactionType = TransactionType.INV;
			var transactionLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 1,
				Description = "Description of line 1",
				OSAmount = 100m,
				OSGSTVATAmount = 21m,
				OSTotalAmount = 121m,
				LocalAmount = 100m,
				LocalGSTVATAmount = 21m,
				LocalTotalAmount = 121m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};
			var transactionLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 2,
				Description = "Description of line 1",
				OSAmount = 200m,
				OSGSTVATAmount = 42m,
				OSTotalAmount = 242m,
				LocalAmount = 200m,
				LocalGSTVATAmount = 42m,
				LocalTotalAmount = 242m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};

			var transactionLine3 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 3,
				Description = "Description of line 3",
				OSAmount = 100m,
				OSTotalAmount = 100m,
				LocalAmount = 100m,
				LocalTotalAmount = 100m,
				VATTaxID = new TaxID()
				{
					TaxCode = "RAT",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "RAT" }
				}
			};
			var transactionLine4 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 2,
				Description = "Description of line 4",
				OSAmount = 200m,
				OSTotalAmount = 200m,
				LocalAmount = 200m,
				LocalTotalAmount = 200m,
				VATTaxID = new TaxID()
				{
					TaxCode = "CAP",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "CAP" }
				}
			};

			transaction.PostingJournalCollection.Add(transactionLine1);
			transaction.PostingJournalCollection.Add(transactionLine2);
			transaction.PostingJournalCollection.Add(transactionLine3);
			transaction.PostingJournalCollection.Add(transactionLine4);
			transaction.OSTotal = 663m;
			transaction.OSGSTVATAmount = 63m;

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<ImpTotal>663.00</ImpTotal>
<ImpTotConc>0.00</ImpTotConc>
<ImpNeto>300.00</ImpNeto>
<ImpOpEx>0.00</ImpOpEx>
<ImpTrib>0.00</ImpTrib>
<ImpIVA>63.00</ImpIVA>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_ImpTribTagAmount()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.TransactionType = TransactionType.INV;
			var transactionLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 1,
				Description = "Description of line 1",
				OSAmount = 100m,
				OSGSTVATAmount = 21m,
				OSTotalAmount = 121m,
				LocalAmount = 100m,
				LocalGSTVATAmount = 21m,
				LocalTotalAmount = 121m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};
			var transactionLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 2,
				Description = "Description of line 1",
				OSAmount = 200m,
				OSGSTVATAmount = 42m,
				OSTotalAmount = 242m,
				LocalAmount = 200m,
				LocalGSTVATAmount = 42m,
				LocalTotalAmount = 242m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};

			var transactionLine3 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 3,
				Description = "Description of line 3",
				OSAmount = 100m,
				OSTotalAmount = 100m,
				LocalAmount = 100m,
				LocalTotalAmount = 100m,
				VATTaxID = new TaxID()
				{
					TaxCode = "RAT",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "RAT" }
				}
			};
			var transactionLine4 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 2,
				Description = "Description of line 4",
				OSAmount = 200m,
				OSTotalAmount = 200m,
				LocalAmount = 200m,
				LocalTotalAmount = 200m,
				VATTaxID = new TaxID()
				{
					TaxCode = "CAP",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "CAP" }
				}
			};

			transaction.PostingJournalCollection.Add(transactionLine1);
			transaction.PostingJournalCollection.Add(transactionLine2);
			transaction.PostingJournalCollection.Add(transactionLine3);
			transaction.PostingJournalCollection.Add(transactionLine4);
			transaction.OSTotal = 663m;
			transaction.OSGSTVATAmount = 63m;
			transaction.OSTaxTransactionsAmount = 10.50m;

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<ImpTotal>663.00</ImpTotal>
<ImpTotConc>0.00</ImpTotConc>
<ImpNeto>300.00</ImpNeto>
<ImpOpEx>0.00</ImpOpEx>
<ImpTrib>10.50</ImpTrib>
<ImpIVA>63.00</ImpIVA>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_ImpTribTagAmount_RevertSing_ForCreditNotes()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionType = TransactionType.CRD;
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			var transactionLine1 = new PostingJournal()
			{
				Sequence = 1,
				Description = "Description of line 1",
				OSAmount = 100m,
				OSGSTVATAmount = 21m,
				OSTotalAmount = 121m,
				LocalAmount = 100m,
				LocalGSTVATAmount = 21m,
				LocalTotalAmount = 121m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};
			var transactionLine2 = new PostingJournal()
			{
				Sequence = 2,
				Description = "Description of line 1",
				OSAmount = 200m,
				OSGSTVATAmount = 42m,
				OSTotalAmount = 242m,
				LocalAmount = 200m,
				LocalGSTVATAmount = 42m,
				LocalTotalAmount = 242m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};

			var transactionLine3 = new PostingJournal()
			{
				Sequence = 3,
				Description = "Description of line 3",
				OSAmount = 100m,
				OSTotalAmount = 100m,
				LocalAmount = 100m,
				LocalTotalAmount = 100m,
				VATTaxID = new TaxID()
				{
					TaxCode = "RAT",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "RAT" }
				}
			};
			var transactionLine4 = new PostingJournal()
			{
				Sequence = 2,
				Description = "Description of line 4",
				OSAmount = 200m,
				OSTotalAmount = 200m,
				LocalAmount = 200m,
				LocalTotalAmount = 200m,
				VATTaxID = new TaxID()
				{
					TaxCode = "CAP",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "CAP" }
				}
			};

			transaction.PostingJournalCollection.Add(transactionLine1);
			transaction.PostingJournalCollection.Add(transactionLine2);
			transaction.PostingJournalCollection.Add(transactionLine3);
			transaction.PostingJournalCollection.Add(transactionLine4);
			transaction.OSTotal = 663m;
			transaction.OSGSTVATAmount = 63m;
			transaction.OSTaxTransactionsAmount = -10.50m;

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<ImpTotal>-663.00</ImpTotal>
<ImpTotConc>0.00</ImpTotConc>
<ImpNeto>-300.00</ImpNeto>
<ImpOpEx>0.00</ImpOpEx>
<ImpTrib>10.50</ImpTrib>
<ImpIVA>-63.00</ImpIVA>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_InvoiceTotalTaxType_EXT_ImpOpExTag()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.TransactionType = TransactionType.INV;
			var transactionLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 1,
				Description = "Description of line 1",
				OSAmount = 100m,
				OSGSTVATAmount = 21m,
				OSTotalAmount = 121m,
				LocalAmount = 100m,
				LocalGSTVATAmount = 21m,
				LocalTotalAmount = 121m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};
			var transactionLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 2,
				Description = "Description of line 1",
				OSAmount = 200m,
				OSGSTVATAmount = 42m,
				OSTotalAmount = 242m,
				LocalAmount = 200m,
				LocalGSTVATAmount = 42m,
				LocalTotalAmount = 242m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};

			var transactionLine3 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 3,
				Description = "Description of line 3",
				OSAmount = 100m,
				OSTotalAmount = 100m,
				LocalAmount = 100m,
				LocalTotalAmount = 100m,
				VATTaxID = new TaxID()
				{
					TaxCode = "RAT",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "RAT" }
				}
			};
			var transactionLine4 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 2,
				Description = "Description of line 4",
				OSAmount = 125.63m,
				OSTotalAmount = 125.63m,
				LocalAmount = 125.63m,
				LocalTotalAmount = 125.63m,
				VATTaxID = new TaxID()
				{
					TaxCode = "EXT",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "EXT" }
				}
			};

			var transactionLine5 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 2,
				Description = "Description of line 4",
				OSAmount = 25.85m,
				OSTotalAmount = 25.85m,
				LocalAmount = 25.85m,
				LocalTotalAmount = 25.85m,
				VATTaxID = new TaxID()
				{
					TaxCode = "EXT",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "EXT" }
				}
			};

			transaction.PostingJournalCollection.Add(transactionLine1);
			transaction.PostingJournalCollection.Add(transactionLine2);
			transaction.PostingJournalCollection.Add(transactionLine3);
			transaction.PostingJournalCollection.Add(transactionLine4);
			transaction.PostingJournalCollection.Add(transactionLine5);
			transaction.OSTotal = 614.48m;
			transaction.OSGSTVATAmount = 63m;

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<ImpTotal>614.48</ImpTotal>
<ImpTotConc>0.00</ImpTotConc>
<ImpNeto>100.00</ImpNeto>
<ImpOpEx>151.48</ImpOpEx>
<ImpTrib>0.00</ImpTrib>
<ImpIVA>63.00</ImpIVA>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_InvoiceTotalAmounts_IncludeAllImpTags()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.TransactionType = TransactionType.INV;

			var transactionLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 1,
				Description = "Description of line 1",
				OSAmount = 100m,
				OSGSTVATAmount = 21m,
				OSTotalAmount = 121m,
				LocalAmount = 100m,
				LocalGSTVATAmount = 21m,
				LocalTotalAmount = 121m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};
			var transactionLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 2,
				Description = "Description of line 2",
				OSAmount = 200m,
				OSTotalAmount = 200m,
				LocalAmount = 200m,
				LocalTotalAmount = 200m,
				VATTaxID = new TaxID()
				{
					TaxCode = "EXL",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "EXL" }
				}
			};

			var transactionLine3 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 3,
				Description = "Description of line 3",
				OSAmount = 100m,
				OSTotalAmount = 100m,
				LocalAmount = 100m,
				LocalTotalAmount = 100m,
				VATTaxID = new TaxID()
				{
					TaxCode = "NOT",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "NOT" }
				}
			};
			var transactionLine4 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 4,
				Description = "Description of line 4",
				OSAmount = 125.63m,
				OSTotalAmount = 125.63m,
				LocalAmount = 125.63m,
				LocalTotalAmount = 125.63m,
				VATTaxID = new TaxID()
				{
					TaxCode = "RAT",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "RAT" }
				}
			};

			var transactionLine5 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 5,
				Description = "Description of line 5",
				OSAmount = 25.85m,
				OSTotalAmount = 25.85m,
				LocalAmount = 25.85m,
				LocalTotalAmount = 25.85m,
				VATTaxID = new TaxID()
				{
					TaxCode = "CAP",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "CAP" }
				}
			};

			var transactionLine6 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 6,
				Description = "Description of line 6",
				OSAmount = 55.85m,
				OSTotalAmount = 55.85m,
				LocalAmount = 55.85m,
				LocalTotalAmount = 55.85m,
				VATTaxID = new TaxID()
				{
					TaxCode = "EXT",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "EXT" }
				}
			};

			transaction.PostingJournalCollection.Add(transactionLine1);
			transaction.PostingJournalCollection.Add(transactionLine2);
			transaction.PostingJournalCollection.Add(transactionLine3);
			transaction.PostingJournalCollection.Add(transactionLine4);
			transaction.PostingJournalCollection.Add(transactionLine5);
			transaction.PostingJournalCollection.Add(transactionLine6);
			transaction.OSTotal = 628.33m;
			transaction.OSGSTVATAmount = 21m;
			transaction.OSTaxTransactionsAmount = 10.50m;

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<ImpTotal>628.33</ImpTotal>
<ImpTotConc>300.00</ImpTotConc>
<ImpNeto>151.48</ImpNeto>
<ImpOpEx>55.85</ImpOpEx>
<ImpTrib>10.50</ImpTrib>
<ImpIVA>21.00</ImpIVA>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_CreditNoteTotalAmounts_MustPositiveAmounts()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionType = TransactionType.CRD;
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			var transactionLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 1,
				Description = "Description of line 1",
				OSAmount = -100m,
				OSGSTVATAmount = -21m,
				OSTotalAmount = -121m,
				LocalAmount = -100m,
				LocalGSTVATAmount = -21m,
				LocalTotalAmount = -121m,
				VATTaxID = new TaxID()
				{
					TaxCode = "GS1",
					TaxRate = 21,
					TaxType = new CodeDescriptionPair() { Code = "GST" }
				}
			};
			var transactionLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 2,
				Description = "Description of line 2",
				OSAmount = -200m,
				OSTotalAmount = -200m,
				LocalAmount = -200m,
				LocalTotalAmount = -200m,
				VATTaxID = new TaxID()
				{
					TaxCode = "EXL",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "EXL" }
				}
			};

			var transactionLine3 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 3,
				Description = "Description of line 3",
				OSAmount = -100m,
				OSTotalAmount = -100m,
				LocalAmount = -100m,
				LocalTotalAmount = -100m,
				VATTaxID = new TaxID()
				{
					TaxCode = "NOT",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "NOT" }
				}
			};
			var transactionLine4 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 4,
				Description = "Description of line 4",
				OSAmount = -125.63m,
				OSTotalAmount = -125.63m,
				LocalAmount = -125.63m,
				LocalTotalAmount = -125.63m,
				VATTaxID = new TaxID()
				{
					TaxCode = "RAT",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "RAT" }
				}
			};

			var transactionLine5 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 5,
				Description = "Description of line 5",
				OSAmount = -25.85m,
				OSTotalAmount = -25.85m,
				LocalAmount = -25.85m,
				LocalTotalAmount = -25.85m,
				VATTaxID = new TaxID()
				{
					TaxCode = "CAP",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "CAP" }
				}
			};

			var transactionLine6 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 6,
				Description = "Description of line 6",
				OSAmount = -55.85m,
				OSTotalAmount = -55.85m,
				LocalAmount = -55.85m,
				LocalTotalAmount = -55.85m,
				VATTaxID = new TaxID()
				{
					TaxCode = "EXT",
					TaxRate = 0,
					TaxType = new CodeDescriptionPair() { Code = "EXT" }
				}
			};

			transaction.PostingJournalCollection.Add(transactionLine1);
			transaction.PostingJournalCollection.Add(transactionLine2);
			transaction.PostingJournalCollection.Add(transactionLine3);
			transaction.PostingJournalCollection.Add(transactionLine4);
			transaction.PostingJournalCollection.Add(transactionLine5);
			transaction.PostingJournalCollection.Add(transactionLine6);
			transaction.OSTotal = -628.33m;
			transaction.OSGSTVATAmount = -21m;
			transaction.OSTaxTransactionsAmount = -10.50m;

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<ImpTotal>628.33</ImpTotal>
<ImpTotConc>300.00</ImpTotConc>
<ImpNeto>151.48</ImpNeto>
<ImpOpEx>55.85</ImpOpEx>
<ImpTrib>10.50</ImpTrib>
<ImpIVA>21.00</ImpIVA>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		#endregion

		#region FEDetRequestDocTipo_DocNro

		public void TestBuildBuyersInfo_MissingOrganizationAddress()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.Branch = new Branch() { Code = "BUE" };
			transactionInfo.OrganizationAddress = null;

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<DocTipo></DocTipo>
<DocNro></DocNro>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildBuyersInfo_MissingRegistrationNumberCollection()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => null);

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<DocTipo></DocTipo>
<DocNro></DocNro>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildBuyersInfo_EmptyRegistrationNumberCollection()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<DocTipo></DocTipo>
<DocNro></DocNro>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildBuyersInfo_ForArgentina_TaxCodeEqualsCUIT()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress.Country = new Country() { Code = CountryCodes.Argentina };
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());

			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType();
			regItem1.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL;
			regItem1.Value = "A-000000000 01";

			var regItem2 = new RegistrationNumber();
			regItem2.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem2.Type = new RegistrationNumberType();
			regItem2.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			regItem2.Value = "A-000000000 02";

			var regItem3 = new RegistrationNumber();
			regItem3.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem3.Type = new RegistrationNumberType();
			regItem3.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI;
			regItem3.Value = "A-000000000 03";

			var regItem4 = new RegistrationNumber();
			regItem4.CountryOfIssue = new Country() { Code = CountryCodes.Uruguay };
			regItem4.Type = new RegistrationNumberType();
			regItem4.Type.Code = UruguayOrgCusCodeInfo.OrgCusCodes.CID;
			regItem4.Value = "A-000000000 04";

			transactionInfo.OrganizationAddress.RegistrationNumberCollection.AddRange(new[] { regItem3, regItem2, regItem4, regItem1 });

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<DocTipo>80</DocTipo>
<DocNro>00000000002</DocNro>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildBuyersInfo_ForArgentina_TaxCodeEqualsCUIL()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress.Country = new Country() { Code = CountryCodes.Argentina };
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());

			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType();
			regItem1.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL;
			regItem1.Value = "A-000000000-01";

			var regItem2 = new RegistrationNumber();
			regItem2.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem2.Type = new RegistrationNumberType();
			regItem2.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUF;
			regItem2.Value = "00000000002";

			var regItem3 = new RegistrationNumber();
			regItem3.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem3.Type = new RegistrationNumberType();
			regItem3.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI;
			regItem3.Value = "00000000003";

			transactionInfo.OrganizationAddress.RegistrationNumberCollection.AddRange(new[] { regItem3, regItem2, regItem1 });

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<DocTipo>86</DocTipo>
<DocNro>00000000001</DocNro>
";
			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildBuyersInfo_ForArgentina_TaxCodeEqualsDNI()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress.Country = new Country() { Code = CountryCodes.Argentina };
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());

			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType();
			regItem1.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.IVI;
			regItem1.Value = "00000000001";

			var regItem2 = new RegistrationNumber();
			regItem2.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem2.Type = new RegistrationNumberType();
			regItem2.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.IVE;
			regItem2.Value = "00000000002";

			var regItem3 = new RegistrationNumber();
			regItem3.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem3.Type = new RegistrationNumberType();
			regItem3.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI;
			regItem3.Value = "AAA-0000000 0003";

			var regItem4 = new RegistrationNumber();
			regItem4.CountryOfIssue = new Country() { Code = CountryCodes.Uruguay };
			regItem4.Type = new RegistrationNumberType();
			regItem4.Type.Code = UruguayOrgCusCodeInfo.OrgCusCodes.CID;
			regItem4.Value = "00000000004";

			transactionInfo.OrganizationAddress.RegistrationNumberCollection.AddRange(new[] { regItem3, regItem2, regItem4, regItem1 });

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<DocTipo>96</DocTipo>
<DocNro>00000000003</DocNro>";

			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildBuyersInfo_ForeingOrg_TaxCodeEqualsCUF()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress.Country = new Country() { Code = CountryCodes.Japan };
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());

			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Japan };
			regItem1.Type = new RegistrationNumberType();
			regItem1.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			regItem1.Value = "00000000001";

			var regItem2 = new RegistrationNumber();
			regItem2.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem2.Type = new RegistrationNumberType();
			regItem2.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.IVE;
			regItem2.Value = "00000000002";

			var regItem3 = new RegistrationNumber();
			regItem3.CountryOfIssue = new Country() { Code = CountryCodes.Japan };
			regItem3.Type = new RegistrationNumberType();
			regItem3.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUF;
			regItem3.Value = "BB 000000000-03";

			var regItem4 = new RegistrationNumber();
			regItem4.CountryOfIssue = new Country() { Code = CountryCodes.Japan };
			regItem4.Type = new RegistrationNumberType();
			regItem4.Type.Code = OrgCusCode.JapanCodeTypes.CON;
			regItem4.Value = "00000000004";

			transactionInfo.OrganizationAddress.RegistrationNumberCollection.AddRange(new[] { regItem3, regItem2, regItem4, regItem1 });

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<DocTipo>80</DocTipo>
<DocNro>00000000003</DocNro>";

			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildBuyersInfo_ForeingOrg_TaxCodeEqualsCUIT()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress.Country = new Country() { Code = CountryCodes.Japan };
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());

			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType();
			regItem1.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.IVI;
			regItem1.Value = "00000000001";

			var regItem2 = new RegistrationNumber();
			regItem2.CountryOfIssue = new Country() { Code = CountryCodes.Japan };
			regItem2.Type = new RegistrationNumberType();
			regItem2.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			regItem2.Value = "CC-00000 000002";

			var regItem3 = new RegistrationNumber();
			regItem3.CountryOfIssue = new Country() { Code = CountryCodes.Japan };
			regItem3.Type = new RegistrationNumberType();
			regItem3.Type.Code = OrgCusCode.CodeTypes.PassportID;
			regItem3.Value = "00000000003";

			transactionInfo.OrganizationAddress.RegistrationNumberCollection.AddRange(new[] { regItem1, regItem3, regItem2 });
			var regItem4 = new RegistrationNumber();
			regItem4.CountryOfIssue = new Country() { Code = CountryCodes.Japan };
			regItem4.Type = new RegistrationNumberType();
			regItem4.Type.Code = OrgCusCode.JapanCodeTypes.CON;
			regItem4.Value = "00000000004";

			transactionInfo.OrganizationAddress.RegistrationNumberCollection.AddRange(new[] { regItem3, regItem2, regItem4, regItem1 });

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<DocTipo>80</DocTipo>
<DocNro>00000000002</DocNro>";

			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildBuyersInfo_ForeingOrg_TaxCodeEqualsPAS()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress.Country = new Country() { Code = CountryCodes.Japan };
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());

			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType();
			regItem1.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.IVI;
			regItem1.Value = "00000000001";

			var regItem2 = new RegistrationNumber();
			regItem2.CountryOfIssue = new Country() { Code = CountryCodes.Japan };
			regItem2.Type = new RegistrationNumberType();
			regItem2.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.IVE;
			regItem2.Value = "00000000002";

			var regItem3 = new RegistrationNumber();
			regItem3.CountryOfIssue = new Country() { Code = CountryCodes.Japan };
			regItem3.Type = new RegistrationNumberType();
			regItem3.Type.Code = OrgCusCode.CodeTypes.PassportID;
			regItem3.Value = "CUB-000000000 03";

			var regItem4 = new RegistrationNumber();
			regItem4.CountryOfIssue = new Country() { Code = CountryCodes.Japan };
			regItem4.Type = new RegistrationNumberType();
			regItem4.Type.Code = OrgCusCode.JapanCodeTypes.CON;
			regItem4.Value = "00000000004";

			transactionInfo.OrganizationAddress.RegistrationNumberCollection.AddRange(new[] { regItem3, regItem2, regItem4, regItem1 });

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<DocTipo>94</DocTipo>
<DocNro>00000000003</DocNro>";

			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildBuyersInfo_DocTipo_DocNro_ForeingOrg_InvalidTaxCode()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress.Country = new Country() { Code = CountryCodes.Japan };
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());

			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType();
			regItem1.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.IVI;
			regItem1.Value = "00000000001";

			var regItem2 = new RegistrationNumber();
			regItem2.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem2.Type = new RegistrationNumberType();
			regItem2.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.IVE;
			regItem2.Value = "00000000002";

			var regItem3 = new RegistrationNumber();
			regItem3.CountryOfIssue = new Country() { Code = CountryCodes.Japan };
			regItem3.Type = new RegistrationNumberType();
			regItem3.Type.Code = OrgCusCode.JapanCodeTypes.CON;
			regItem3.Value = "00000000003";

			transactionInfo.OrganizationAddress.RegistrationNumberCollection.AddRange(new[] { regItem3, regItem2, regItem1 });

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<DocTipo></DocTipo>
<DocNro></DocNro>";

			AssertContains(expectedValue, actualXmlValue);
		}

		public void TestBuildBuyersInfo_DocTipo_DocNro_ArgentinaOrg_InvalidTaxCode()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress.Country = new Country() { Code = CountryCodes.Argentina };
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());

			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType();
			regItem1.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.IVI;
			regItem1.Value = "00000000001";

			var regItem2 = new RegistrationNumber();
			regItem2.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem2.Type = new RegistrationNumberType();
			regItem2.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.IVE;
			regItem2.Value = "00000000002";

			var regItem3 = new RegistrationNumber();
			regItem3.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem3.Type = new RegistrationNumberType();
			regItem3.Type.Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.IVM;
			regItem3.Value = "00000000003";

			transactionInfo.OrganizationAddress.RegistrationNumberCollection.AddRange(new[] { regItem3, regItem2, regItem1 });

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			var expectedValue = @"
<DocTipo></DocTipo>
<DocNro></DocNro>";

			AssertContains(expectedValue, actualXmlValue);
		}

		#endregion

		#region CbteDesde_CbteHasta

		public void TestCbteDesdeHastaTags_WhenMissingTransactionReference()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var expectedXmlResult = @"
<CbteDesde></CbteDesde>
<CbteHasta></CbteHasta>";

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlResult = builder.BuildFEDetRequestInfo(transactionInfo, string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void Test_CbteDesdeHastaTags_WhenTransactionReferenceIsEmpty()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.TransactionReference = " ";

			var expectedXmlResult = @"
<CbteDesde></CbteDesde>
<CbteHasta></CbteHasta>";

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlResult = builder.BuildFEDetRequestInfo(transactionInfo, string.Empty, Factory).ToString().Replace(" ", "");

			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void Test_CbteDesdeHastaTags_FromAccComplianceSequence()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXA";
			transactionInfo.TransactionReference = "001300654089";

			accComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			accComplianceSequence.XD_SequenceClass = "TXA";
			accComplianceSequence.XD_Prefix = "0013";
			accComplianceSequence.XD_MaximumNumberDigits = 8;

			var fEDetRequestBuilder = new FEDetRequestBuilder();
			var ifEDetRequestBuilder = (IFEDetRequestBuilder)fEDetRequestBuilder;

			var expectedXmlResult = @"
<CbteDesde></CbteDesde>
<CbteHasta></CbteHasta>";

			var actualXmlResult = ifEDetRequestBuilder.BuildFEDetRequestInfo(transactionInfo, string.Empty, Factory).ToString().Replace(" ", "");
			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void Test_CbteDesdeHastaTags_AccComplianceSequenceUnMatch()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXA";
			transactionInfo.TransactionReference = "001300654089";

			accComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			accComplianceSequence.XD_SequenceClass = "TXB";
			accComplianceSequence.XD_Prefix = "0014";
			accComplianceSequence.XD_MaximumNumberDigits = 8;

			var expectedXmlResult = @"
<CbteDesde></CbteDesde>
<CbteHasta></CbteHasta>";

			var fEDetRequestBuilder = new FEDetRequestBuilder();
			var ifEDetRequestBuilder = (IFEDetRequestBuilder)fEDetRequestBuilder;
			var actualXmlResult = ifEDetRequestBuilder.BuildFEDetRequestInfo(transactionInfo, string.Empty, Factory).ToString().Replace(" ", "");
			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void Test_CbteDesdeHastaTags_WhenAccComplianceSequencePrefixIsMissing()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXA";
			transactionInfo.TransactionReference = "001300654089";

			accComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			accComplianceSequence.XD_SequenceClass = "TXA";
			accComplianceSequence.XD_Prefix = null;
			accComplianceSequence.XD_MaximumNumberDigits = 8;

			var fEDetRequestBuilder = new FEDetRequestBuilder();
			var ifEDetRequestBuilder = (IFEDetRequestBuilder)fEDetRequestBuilder;

			var expectedXmlResult = @"
<CbteDesde></CbteDesde>
<CbteHasta></CbteHasta>";

			var actualXmlResult = ifEDetRequestBuilder.BuildFEDetRequestInfo(transactionInfo, string.Empty, Factory).ToString().Replace(" ", "");
			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void Test_CbteDesdeHastaTags_ManualComplianceNumber_ContainsOnlyNumbers()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXA";
			transactionInfo.TransactionReference = "00023000000001";

			var fEDetRequestBuilder = new FEDetRequestBuilder();
			var ifEDetRequestBuilder = (IFEDetRequestBuilder)fEDetRequestBuilder;

			var expectedXmlResult = @"
<CbteDesde></CbteDesde>
<CbteHasta></CbteHasta>";

			var actualXmlResult = ifEDetRequestBuilder.BuildFEDetRequestInfo(transactionInfo, string.Empty, Factory).ToString().Replace(" ", "");
			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void Test_CbteDesdeHastaTags_ComplianceSequence_Prefix_Includes_Non_Numeric_Characters()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXB";
			transactionInfo.TransactionReference = "FCB0000004-000000001";

			accComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			accComplianceSequence.XD_SequenceClass = "TXB";
			accComplianceSequence.XD_Code = "FCB";
			accComplianceSequence.XD_Prefix = "FCB0000004-";
			accComplianceSequence.XD_MaximumNumberDigits = 9;

			var fEDetRequestBuilder = new FEDetRequestBuilder();
			var ifEDetRequestBuilder = (IFEDetRequestBuilder)fEDetRequestBuilder;

			var expectedXmlResult = @"
<CbteDesde></CbteDesde>
<CbteHasta></CbteHasta>";

			var actualXmlResult = ifEDetRequestBuilder.BuildFEDetRequestInfo(transactionInfo, string.Empty, Factory).ToString().Replace(" ", "");
			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void Test_CbteDesdeHastaTags_ComplianceSequence_Prefix_Lenght_LessThanFiveDigits()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXB";
			transactionInfo.TransactionReference = "FCB-000000001";

			accComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			accComplianceSequence.XD_SequenceClass = "TXB";
			accComplianceSequence.XD_Code = "FCB";
			accComplianceSequence.XD_Prefix = "FCB-";
			accComplianceSequence.XD_MaximumNumberDigits = 9;

			var fEDetRequestBuilder = new FEDetRequestBuilder();
			var ifEDetRequestBuilder = (IFEDetRequestBuilder)fEDetRequestBuilder;

			var expectedXmlResult = @"
<CbteDesde></CbteDesde>
<CbteHasta></CbteHasta>";

			var actualXmlResult = ifEDetRequestBuilder.BuildFEDetRequestInfo(transactionInfo, string.Empty, Factory).ToString().Replace(" ", "");
			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void Test_CbteDesdeHastaTags_ComplianceSequence_Prefix_Lenght_MoreThanFiveDigits()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXB";
			transactionInfo.TransactionReference = "00000015000000001";

			accComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			accComplianceSequence.XD_SequenceClass = "TXB";
			accComplianceSequence.XD_Code = "FCB";
			accComplianceSequence.XD_Prefix = "00000015";
			accComplianceSequence.XD_MaximumNumberDigits = 9;

			var fEDetRequestBuilder = new FEDetRequestBuilder();
			var ifEDetRequestBuilder = (IFEDetRequestBuilder)fEDetRequestBuilder;

			var expectedXmlResult = @"
<CbteDesde></CbteDesde>
<CbteHasta></CbteHasta>";

			var actualXmlResult = ifEDetRequestBuilder.BuildFEDetRequestInfo(transactionInfo, string.Empty, Factory).ToString().Replace(" ", "");
			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void Test_CbteDesdeHastaTags_ManualComplianceNumber_Includes_Non_Numeric_Characters()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXB";
			transactionInfo.TransactionReference = "FCA#00233-000002526";

			var fEDetRequestBuilder = new FEDetRequestBuilder();
			var ifEDetRequestBuilder = (IFEDetRequestBuilder)fEDetRequestBuilder;

			var expectedXmlResult = @"
<CbteDesde></CbteDesde>
<CbteHasta></CbteHasta>";

			var actualXmlResult = ifEDetRequestBuilder.BuildFEDetRequestInfo(transactionInfo, string.Empty, Factory).ToString().Replace(" ", "");
			AssertContains(expectedXmlResult, actualXmlResult);
		}

		#endregion

		#region FEDetRequestCbteAsoc

		public void TestBuildXMLFECAEDetRequest_CbteAsoc_OriginalReferenceIsNull()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.ComplianceSubType = "TDA";

			var expectedValue = @"
<CbtesAsoc>
";
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");

			AssertNotContains("CbtesAsoc sub node must not be present in actualXmlValue when OriginalReference is null", expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_CbteAsoc_OriginalReferenceIsEmpty()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OriginalReference = new OriginalReference();

			var expectedValue = @"
<CbtesAsoc>
";
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");

			AssertNotContains("CbtesAsoc sub node must not be present in actualXmlValue when OriginalReference is empty", expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_CbteAsoc_TransactionType_IsNull()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXA";

			var expectedValue = @"
<CbtesAsoc>
";
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");

			AssertNotContains("CbtesAsoc sub node must not be present when TransactionType is missing", expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_CbteAsoc_TransactionComplianceSubType_IsNotCreditNote()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.ComplianceSubType = "TXA";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXA";

			var expectedValue = @"
<CbtesAsoc>
";
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");

			AssertNotContains("CbtesAsoc sub node must  be present for transantion credit note type ", expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_CbteAsoc_TransactionComplianceSubType_IsNotDebitNote()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionType = TransactionType.INV;
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXA";

			var expectedValue = @"
<CbtesAsoc>
";
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");

			AssertNotContains("CbtesAsoc sub node must not be present for non debit note transaction", expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_CbteAsoc_TransactionComplianceSubType_IsCreditNote()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.ComplianceSubType = "TCA";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXA";

			var expectedValue = @"
<CbtesAsoc>
";
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");

			AssertContains("CbtesAsoc sub node must be present for credit note transactions", expectedValue, actualXmlValue);
		}

		public void TestBuildXMLFECAEDetRequest_CbteAsoc_Transaction_IsDebitNote()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.ComplianceSubType = "TDA";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXA";
			transaction.IsCancelled = false;

			var expectedValue = @"
<CbtesAsoc>
";
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");

			AssertContains("CbtesAsoc sub node must  be present for debit note transaction", expectedValue, actualXmlValue);
		}

		[TestDate(2020, 9, 07)]
		public void TestCbteAsoc_When_OriginalTransactionComplianceSubType_IsPopulate()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.ComplianceSubType = "TCA";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXA";
			transaction.OriginalReference.OriginalTransactionReference = "0023300002526";
			transaction.OriginalReference.OriginalTransactionDate = ZDateTime.Today;

			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT };
			regItem1.Value = "30123456780";
			transaction.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1 });

			var expectedValue = @"
<CbtesAsoc>
<CbteAsoc>
<Tipo>1</Tipo>
<PtoVta>00233</PtoVta>
<Nro>00002526</Nro>
<Cuit>30123456780</Cuit>
<CbteFch>20200907</CbteFch>
</CbteAsoc>
</CbtesAsoc>
";
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory, "TXA", "30123456780").ToString().Replace(" ", "");

			AssertContains(expectedValue, actualXmlValue);
		}

		[TestDate(2020, 9, 07)]
		public void TestCbteAsoc_When_OriginalTransactionComplianceSubType_DontPopulate()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.ComplianceSubType = "TCA";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionNumber = "TXA0023300002526";
			transaction.OriginalReference.OriginalTransactionDate = ZDateTime.Today;

			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT };
			regItem1.Value = "30123456780";
			transaction.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1 });

			var expectedValue = @"
<CbtesAsoc>
<CbteAsoc>
<Tipo>1</Tipo>
<PtoVta>00233</PtoVta>
<Nro>00002526</Nro>
<Cuit>30123456780</Cuit>
<CbteFch>20200907</CbteFch>
</CbteAsoc>
</CbtesAsoc>
";
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory, "TXA", "30123456780").ToString().Replace(" ", "");

			AssertContains(expectedValue, actualXmlValue);
		}

		[TestDate(2020, 9, 07)]
		public void TestCbteAsoc_When_OriginalTransactionComplianceSubType_IsPopulate_And_ComplianceSequenceBookConfigure()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.ComplianceSubType = "TCA";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXA";
			transaction.OriginalReference.OriginalTransactionReference = "0023300002526";
			transaction.OriginalReference.OriginalTransactionDate = ZDateTime.Today;

			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT };
			regItem1.Value = "30123456780";
			transaction.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1 });

			accComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			accComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			accComplianceSequence.XD_SequenceClass = "TXA";
			accComplianceSequence.XD_Prefix = "FCA#00233-";

			var expectedValue = @"
<CbtesAsoc>
<CbteAsoc>
<Tipo>1</Tipo>
<PtoVta>00233</PtoVta>
<Nro>00002526</Nro>
<Cuit>30123456780</Cuit>
<CbteFch>20200907</CbteFch>
</CbteAsoc>
</CbtesAsoc>
";
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory, "TXA", "30123456780").ToString().Replace(" ", "");

			AssertContains(expectedValue, actualXmlValue);
		}

		[TestDate(2020, 9, 07)]
		public void TestCbteAsoc_When_OriginalTransactionComplianceSubType_DontPopulate_And_ComplianceBookSequenceConfigured()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.ComplianceSubType = "TDA";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionNumber = "TXA0023300002526";
			transaction.OriginalReference.OriginalTransactionDate = ZDateTime.Today;

			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT };
			regItem1.Value = "30123456780";
			transaction.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1 });

			accComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			accComplianceSequence.XD_SequenceClass = "TXA";
			accComplianceSequence.XD_Prefix = "FCA#00233-";

			var expectedValue = @"
<CbtesAsoc>
<CbteAsoc>
<Tipo>1</Tipo>
<PtoVta>00233</PtoVta>
<Nro>00002526</Nro>
<Cuit>30123456780</Cuit>
<CbteFch>20200907</CbteFch>
</CbteAsoc>
</CbtesAsoc>
";
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory, "TXA", "30123456780").ToString().Replace(" ", "");

			AssertContains(expectedValue, actualXmlValue);
		}

		[TestDate(2020, 9, 07)]
		public void TestCbteAsoc_OriginalTransactionComplianceSubType_IsPresent_And_OriginalTransactionReferece_DontPopulate()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.ComplianceSubType = "TDA";
			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXA";
			transaction.OriginalReference.OriginalTransactionJobInvoiceNumber = "S000001";
			transaction.OriginalReference.OriginalTransactionNumber = "00001010";
			transaction.OriginalReference.OriginalTransactionDate = ZDateTime.Today;

			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT };
			regItem1.Value = "30123456780";
			transaction.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1 });

			var expectedValue = @"
<CbtesAsoc>
<CbteAsoc>
<Tipo>1</Tipo>
<PtoVta></PtoVta>
<Nro></Nro>
<Cuit>30123456780</Cuit>
<CbteFch>20200907</CbteFch>
</CbteAsoc>
</CbtesAsoc>
";
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory, "TXA", "30123456780").ToString().Replace(" ", "");

			AssertContains(expectedValue, actualXmlValue);
		}

		#endregion

		#region TaxSummaryAlicIva

		public void TestAlicsIvaTaxSummary_MissingPostingJournal()
		{
			var alicsIva = (IFEDetRequestBuilder)new FEDetRequestBuilder();

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var actualXml = alicsIva.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, null).ToString().Replace(" ", "");
			AssertNotContains("<Iva>", actualXml);
		}

		public void TestAlicsIvaTaxSummary_EmptyPostingJournal()
		{
			var alicsIva = (IFEDetRequestBuilder)new FEDetRequestBuilder();

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var actualXml = alicsIva.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, null).ToString().Replace(" ", "");
			AssertNotContains("<Iva>", actualXml);
		}

		public void TestAlicsIvaTaxSummary_MissingVATTaxId()
		{
			var alicsIva = (IFEDetRequestBuilder)new FEDetRequestBuilder();

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var line = CreatePostingJournal();
			line.Sequence = 1;
			line.LocalAmount = 100m;
			line.LocalTotalAmount = 121m;
			line.OSAmount = 100m;
			line.Description = $"Test Description for Line {line.Sequence}";

			transactionInfo.OSTotal = 121m;
			transactionInfo.PostingJournalCollection.Add(line);

			var actualXml = alicsIva.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, null).ToString().Replace(" ", "");
			AssertNotContains("<Iva>", actualXml);
		}

		public void TestAlicsIvaTaxSummary_EmptyVATTaxId()
		{
			var alicsIva = (IFEDetRequestBuilder)new FEDetRequestBuilder();

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRate = new TaxID();

			var line = CreatePostingJournal();
			line.Sequence = 1;
			line.LocalAmount = 100m;
			line.LocalTotalAmount = 121m;
			line.OSAmount = 100m;
			line.Description = $"Test Description for Line {line.Sequence}";
			transactionInfo.OSTotal = 121m;
			transactionInfo.PostingJournalCollection.Add(line);

			var actualXml = alicsIva.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, null).ToString().Replace(" ", "");
			AssertNotContains("<Iva>", actualXml);
		}

		public void TestAlicsIvaTaxSummary_MissingTaxMessageID()
		{
			var alicsIva = (IFEDetRequestBuilder)new FEDetRequestBuilder();

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRate = new TaxID();
			taxRate.TaxCode = "IVA21";
			taxRate.TaxRate = 21m;

			var line = CreatePostingJournal();
			line.Sequence = 1;
			line.LocalAmount = 100m;
			line.LocalTotalAmount = 121m;
			line.OSAmount = 100m;
			line.VATTaxID = taxRate;
			line.TaxMessageID = null;
			line.Description = $"Test Description for Line {line.Sequence}";
			transactionInfo.OSTotal = 121m;
			transactionInfo.PostingJournalCollection.Add(line);

			var actualXml = alicsIva.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, null).ToString().Replace(" ", "");
			AssertNotContains("<Iva>", actualXml);
		}

		public void TestAlicsIvaTaxSummary_EmptyTaxMessageID()
		{
			var alicsIva = (IFEDetRequestBuilder)new FEDetRequestBuilder();

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRate = new TaxID();
			taxRate.TaxCode = "IVA21";
			taxRate.TaxRate = 21m;

			var line = CreatePostingJournal();
			line.Sequence = 1;
			line.LocalAmount = 100m;
			line.LocalTotalAmount = 121m;
			line.OSAmount = 100m;
			line.VATTaxID = taxRate;
			line.TaxMessageID = new TaxMessageID();
			line.Description = $"Test Description for Line {line.Sequence}";
			transactionInfo.OSTotal = 121m;
			transactionInfo.PostingJournalCollection.Add(line);

			var actualXml = alicsIva.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, null).ToString().Replace(" ", "");
			AssertNotContains("<Iva>", actualXml);
		}

		public void TestAlicsIvaTaxSummary()
		{
			var alicsIva = (IFEDetRequestBuilder)new FEDetRequestBuilder();

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRateIVA21 = new TaxID();
			taxRateIVA21.TaxCode = "IVA21";
			taxRateIVA21.TaxRate = 21m;

			var taxRateCAPIVA = new TaxID();
			taxRateCAPIVA.TaxCode = "CAPIVA";
			taxRateCAPIVA.TaxRate = 21m;

			var taxRateIVA27 = new TaxID();
			taxRateIVA27.TaxCode = "IVA27";
			taxRateIVA27.TaxRate = 10.5m;

			var taxRateIVA10_5 = new TaxID();
			taxRateIVA10_5.TaxCode = "IVA10.5";
			taxRateIVA10_5.TaxRate = 10.5m;

			var taxRateFreeIVA = new TaxID();
			taxRateFreeIVA.TaxCode = "FREEIVA";
			taxRateFreeIVA.TaxRate = 0m;

			var taxMessageIVA0 = new TaxMessageID();
			taxMessageIVA0.TaxMessageCode = "IVA0";
			taxMessageIVA0.TaxGroupCode = new TaxGroupCodeType();
			taxMessageIVA0.TaxGroupCode.Code = ArgentinaComplianceInfo.TaxMessageGroupCodes.N3;
			taxMessageIVA0.TaxGroupCode.Description = "IVA 0%";
			taxMessageIVA0.Description = "IVA 0% Desc";

			var taxMessageIVA10_5 = new TaxMessageID();
			taxMessageIVA10_5.TaxMessageCode = "IVA10.5";
			taxMessageIVA10_5.TaxGroupCode = new TaxGroupCodeType();
			taxMessageIVA10_5.TaxGroupCode.Code = ArgentinaComplianceInfo.TaxMessageGroupCodes.N4;
			taxMessageIVA10_5.TaxGroupCode.Description = "IVA10.5%";
			taxMessageIVA10_5.Description = "IVA10.5 Desc";

			var taxMessageIVA21 = new TaxMessageID();
			taxMessageIVA21.TaxMessageCode = "IVA21";
			taxMessageIVA21.TaxGroupCode = new TaxGroupCodeType();
			taxMessageIVA21.TaxGroupCode.Code = ArgentinaComplianceInfo.TaxMessageGroupCodes.N5;
			taxMessageIVA21.TaxGroupCode.Description = "IVA21%";
			taxMessageIVA21.Description = "IVA21 Desc";

			var taxMessageIVA27 = new TaxMessageID();
			taxMessageIVA27.TaxMessageCode = "IVA27";
			taxMessageIVA27.TaxGroupCode = new TaxGroupCodeType();
			taxMessageIVA27.TaxGroupCode.Code = ArgentinaComplianceInfo.TaxMessageGroupCodes.N6;
			taxMessageIVA27.TaxGroupCode.Description = "IVA27%";
			taxMessageIVA27.Description = "IVA27 Desc";

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.OSAmount = 1200m;
			line1.OSGSTVATAmount = 252m;
			line1.OSTotalAmount = 1452m;
			line1.VATTaxID = taxRateIVA21;
			line1.TaxMessageID = taxMessageIVA21;
			line1.Description = $"Test Description for Line {line1.Sequence}";

			var line2 = CreatePostingJournal();
			line2.Sequence = 2;
			line2.OSAmount = 126m;
			line2.OSGSTVATAmount = 26.46m;
			line2.OSTotalAmount = 152.46m;
			line2.VATTaxID = taxRateCAPIVA;
			line2.TaxMessageID = taxMessageIVA21;
			line2.Description = $"Test Description for Line {line2.Sequence}";

			var line3 = CreatePostingJournal();
			line3.Sequence = 3;
			line3.OSAmount = 126m;
			line3.OSGSTVATAmount = 26.46m;
			line3.OSTotalAmount = 152.46m;
			line3.VATTaxID = taxRateIVA21;
			line3.TaxMessageID = taxMessageIVA21;
			line3.Description = $"Test Description for Line {line3.Sequence}";

			var line4 = CreatePostingJournal();
			line4.Sequence = 4;
			line4.OSAmount = 500m;
			line4.OSGSTVATAmount = 135m;
			line4.OSTotalAmount = 635m;
			line4.VATTaxID = taxRateIVA27;
			line4.TaxMessageID = taxMessageIVA27;
			line4.Description = $"Test Description for Line {line4.Sequence}";

			var line5 = CreatePostingJournal();
			line5.Sequence = 5;
			line5.OSAmount = 440;
			line5.OSGSTVATAmount = 46.20m;
			line5.OSTotalAmount = 486.20m;
			line5.VATTaxID = taxRateIVA10_5;
			line5.TaxMessageID = taxMessageIVA10_5;
			line5.Description = $"Test Description for Line {line5.Sequence}";

			var line6 = CreatePostingJournal();
			line6.Sequence = 6;
			line6.OSAmount = 330m;
			line6.OSGSTVATAmount = 0m;
			line6.OSTotalAmount = 330m;
			line6.VATTaxID = taxRateFreeIVA;
			line6.TaxMessageID = taxMessageIVA0;
			line6.Description = $"Test Description for Line {line6.Sequence}";

			var line7 = CreatePostingJournal();
			line7.Sequence = 7;
			line7.OSAmount = 230m;
			line7.OSGSTVATAmount = 0m;
			line7.OSTotalAmount = 230m;
			line7.Description = $"Test Description for Line {line6.Sequence}";

			transactionInfo.PostingJournalCollection.Add(line1);
			transactionInfo.PostingJournalCollection.Add(line2);
			transactionInfo.PostingJournalCollection.Add(line3);
			transactionInfo.PostingJournalCollection.Add(line4);
			transactionInfo.PostingJournalCollection.Add(line5);
			transactionInfo.PostingJournalCollection.Add(line6);

			var expectedXml = $@"<Iva>
      <AlicIva>
        <Id>3</Id>
        <BaseImp>330.00</BaseImp>
        <Importe>0.00</Importe>
      </AlicIva>
      <AlicIva>
        <Id>4</Id>
        <BaseImp>440.00</BaseImp>
        <Importe>46.20</Importe>
      </AlicIva>
      <AlicIva>
        <Id>5</Id>
        <BaseImp>1452.00</BaseImp>
        <Importe>304.92</Importe>
      </AlicIva>
      <AlicIva>
        <Id>6</Id>
        <BaseImp>500.00</BaseImp>
        <Importe>135.00</Importe>
      </AlicIva>
    </Iva>";

			var actualXml = alicsIva.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, null).ToString();
			AssertContains(expectedXml, actualXml);
		}

		public void TestAlicsIvaTaxSummary_TaxMessageCodes_NotSupported()
		{
			var alicsIva = (IFEDetRequestBuilder)new FEDetRequestBuilder();

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());

			var taxRateIVA21 = new TaxID();
			taxRateIVA21.TaxCode = "IVA21";
			taxRateIVA21.TaxRate = 21m;

			var taxRateCAPIVA = new TaxID();
			taxRateCAPIVA.TaxCode = "CAPIVA";
			taxRateCAPIVA.TaxRate = 21m;

			var taxMessageIVA5 = new TaxMessageID();
			taxMessageIVA5.TaxMessageCode = "IVA 5";
			taxMessageIVA5.TaxGroupCode = new TaxGroupCodeType();
			taxMessageIVA5.TaxGroupCode.Code = ArgentinaComplianceInfo.TaxMessageGroupCodes.N8;
			taxMessageIVA5.TaxGroupCode.Description = "IVA 5%";
			taxMessageIVA5.Description = "IVA 5% Desc";

			var taxMessageIVA2_5 = new TaxMessageID();
			taxMessageIVA2_5.TaxMessageCode = "IVA2.5";
			taxMessageIVA2_5.TaxGroupCode = new TaxGroupCodeType();
			taxMessageIVA2_5.TaxGroupCode.Code = ArgentinaComplianceInfo.TaxMessageGroupCodes.N9;
			taxMessageIVA2_5.TaxGroupCode.Description = "IVA2.5%";
			taxMessageIVA2_5.Description = "IVA2.5 Desc";

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.OSAmount = 1200m;
			line1.OSGSTVATAmount = 252m;
			line1.OSTotalAmount = 1452m;
			line1.VATTaxID = taxRateIVA21;
			line1.TaxMessageID = taxMessageIVA5;
			line1.Description = $"Test Description for Line {line1.Sequence}";

			var line2 = CreatePostingJournal();
			line2.Sequence = 2;
			line2.OSAmount = 126m;
			line2.OSGSTVATAmount = 26.46m;
			line2.OSTotalAmount = 152.46m;
			line2.VATTaxID = taxRateCAPIVA;
			line2.TaxMessageID = taxMessageIVA2_5;
			line2.Description = $"Test Description for Line {line2.Sequence}";

			transactionInfo.PostingJournalCollection.Add(line1);
			transactionInfo.PostingJournalCollection.Add(line2);

			var actualXml = alicsIva.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, null).ToString().Replace(" ", "");
			AssertNotContains("<Iva>", actualXml);
		}

		public void TestAlicsIvaTaxSummary_ChangeSing()
		{
			var alicsIva = (IFEDetRequestBuilder)new FEDetRequestBuilder();

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());
			transactionInfo.TransactionType = TransactionType.CRD;

			var taxRateIVA21 = new TaxID();
			taxRateIVA21.TaxCode = "IVA21";
			taxRateIVA21.TaxRate = 21m;

			var taxMessageIVA21 = new TaxMessageID();
			taxMessageIVA21.TaxMessageCode = "IVA 21";
			taxMessageIVA21.TaxGroupCode = new TaxGroupCodeType();
			taxMessageIVA21.TaxGroupCode.Code = ArgentinaComplianceInfo.TaxMessageGroupCodes.N5;
			taxMessageIVA21.TaxGroupCode.Description = "IVA 21%";
			taxMessageIVA21.Description = "IVA 21% Desc";

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.OSAmount = -100m;
			line1.OSGSTVATAmount = -21m;
			line1.OSTotalAmount = -121m;
			line1.VATTaxID = taxRateIVA21;
			line1.TaxMessageID = taxMessageIVA21;
			line1.Description = $"Test Description for Line {line1.Sequence}";

			transactionInfo.PostingJournalCollection.Add(line1);

			var expectedXml = $@"<Iva>
      <AlicIva>
        <Id>5</Id>
        <BaseImp>100.00</BaseImp>
        <Importe>21.00</Importe>
      </AlicIva>
    </Iva>";
			var actualXml = alicsIva.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, null).ToString();
			AssertContains(expectedXml, actualXml);
		}

		#endregion

		#region Tributos

		public void TestFECAEDetRequest_TributosNodeIsNotAdded()
		{
			CombineAssertions(() =>
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				AssertTributosIsNotPresent(transaction);

				transaction.OSTaxTransactionsAmount = 0m;
				AssertTributosIsNotPresent(transaction);

				transaction.OSTaxTransactionsAmount = 10m;
				transaction.SetTaxTransactionCollection(() => null);
				AssertTributosIsNotPresent(transaction);

				transaction.SetTaxTransactionCollection(() => new List<TaxTransaction>());
				AssertTributosIsNotPresent(transaction);

				void AssertTributosIsNotPresent(TransactionInfo transactionInfo)
				{
					var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
					var actualXmlValue = builder.BuildFEDetRequestInfo(transactionInfo, FEV1NameSpace, Factory).ToString().Replace(" ", "");
					AssertNotContains("<Tributos>", actualXmlValue);
				}
			});
		}

		public void TestFECAEDetRequest_TributosNode()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OSTaxTransactionsAmount = 20.33m;
			transaction.SetTaxTransactionCollection(() => new List<TaxTransaction>());

			var taxTransaction1 = new TaxTransaction(DefaultDataObjectWriterStrategy.TestInstance);
			taxTransaction1.TaxConfiguration = new CodeDescriptionPair30Char();
			taxTransaction1.TaxConfiguration.Code = "TST";
			taxTransaction1.TaxConfiguration.Description = "TEST1";
			taxTransaction1.TaxID = new TaxID()
			{
				TaxCode = "PIB",
				TaxRate = 1.5m,
				TaxType = new CodeDescriptionPair() { Code = "PIB" }
			};
			taxTransaction1.OSTaxBase = 1255m;
			taxTransaction1.OSTaxAmount = 18.33m;
			taxTransaction1.TaxSuperType = new CodeDescriptionPair()
			{
				Code = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code,
				Description = "PERCEPTIONS"
			};

			var taxTransaction2 = new TaxTransaction(DefaultDataObjectWriterStrategy.TestInstance);
			taxTransaction2.TaxConfiguration = new CodeDescriptionPair30Char();
			taxTransaction2.TaxConfiguration.Code = "TST";
			taxTransaction2.TaxConfiguration.Description = "TEST2";
			taxTransaction2.TaxID = new TaxID()
			{
				TaxCode = "PIB",
				TaxRate = 0.2m,
				TaxType = new CodeDescriptionPair() { Code = "PIB" }
			};
			taxTransaction2.TaxSuperType = new CodeDescriptionPair()
			{
				Code = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code,
				Description = "PERCEPTIONS"
			};
			taxTransaction2.OSTaxBase = 1000m;
			taxTransaction2.OSTaxAmount = 2m;

			transaction.TaxTransactionCollection.Add(taxTransaction1);
			transaction.TaxTransactionCollection.Add(taxTransaction2);

			var expectedXml = $@"<Tributos>
<Tributo>
<Id>7</Id>
<Desc>TEST1</Desc>
<BaseImp>1255.00</BaseImp>
<Alic>1.5</Alic>
<Importe>18.33</Importe>
</Tributo>
<Tributo>
<Id>7</Id>
<Desc>TEST2</Desc>
<BaseImp>1000.00</BaseImp>
<Alic>0.2</Alic>
<Importe>2.00</Importe>
</Tributo>
</Tributos>";

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			AssertContains(expectedXml, actualXmlValue);
		}

		public void TestFECAEDetRequest_TributosNode_ChangeSing_ForCreditNotes()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionType = TransactionType.CRD;
			transaction.OSTaxTransactionsAmount = 20.33m;
			transaction.SetTaxTransactionCollection(() => new List<TaxTransaction>());

			var taxTransaction1 = new TaxTransaction(DefaultDataObjectWriterStrategy.TestInstance);
			taxTransaction1.TaxConfiguration = new CodeDescriptionPair30Char();
			taxTransaction1.TaxConfiguration.Code = "TST";
			taxTransaction1.TaxConfiguration.Description = "TEST1";
			taxTransaction1.TaxID = new TaxID()
			{
				TaxCode = "PIB",
				TaxRate = 1.5m,
				TaxType = new CodeDescriptionPair() { Code = "PIB" }
			};
			taxTransaction1.TaxSuperType = new CodeDescriptionPair()
			{
				Code = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code,
				Description = "PERCPTIONS"
			};
			taxTransaction1.OSTaxBase = 1255m;
			taxTransaction1.OSTaxAmount = 18.33m;

			var taxTransaction2 = new TaxTransaction(DefaultDataObjectWriterStrategy.TestInstance);
			taxTransaction2.TaxConfiguration = new CodeDescriptionPair30Char();
			taxTransaction2.TaxConfiguration.Code = "TST";
			taxTransaction2.TaxConfiguration.Description = "TEST2";
			taxTransaction2.TaxID = new TaxID()
			{
				TaxCode = "PIB",
				TaxRate = 0.2m,
				TaxType = new CodeDescriptionPair() { Code = "PIB" }
			};
			taxTransaction2.TaxSuperType = new CodeDescriptionPair()
			{
				Code = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code,
				Description = "PERCERPTIONS"
			};
			taxTransaction2.OSTaxBase = -1000m;
			taxTransaction2.OSTaxAmount = -2m;

			transaction.TaxTransactionCollection.Add(taxTransaction1);
			transaction.TaxTransactionCollection.Add(taxTransaction2);

			var expectedXml = $@"<Tributos>
<Tributo>
<Id>7</Id>
<Desc>TEST1</Desc>
<BaseImp>-1255.00</BaseImp>
<Alic>1.5</Alic>
<Importe>-18.33</Importe>
</Tributo>
<Tributo>
<Id>7</Id>
<Desc>TEST2</Desc>
<BaseImp>1000.00</BaseImp>
<Alic>0.2</Alic>
<Importe>2.00</Importe>
</Tributo>
</Tributos>";

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			AssertContains(expectedXml, actualXmlValue);
		}

		public void TestFECAEDetRequest_TributosNode_NotPresent_WhenTaxSuperTypeListCode_IsNotPerceptions()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OSTaxTransactionsAmount = 20.33m;
			transaction.SetTaxTransactionCollection(() => new List<TaxTransaction>());

			var taxTransaction1 = new TaxTransaction(DefaultDataObjectWriterStrategy.TestInstance);
			taxTransaction1.TaxConfiguration = new CodeDescriptionPair30Char();
			taxTransaction1.TaxConfiguration.Code = "TST";
			taxTransaction1.TaxConfiguration.Description = "TEST1";
			taxTransaction1.TaxSuperType = new CodeDescriptionPair()
			{
				Code = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.TurnoverTax.Code,
				Description = "TESTSUPERTYPE"
			};

			taxTransaction1.TaxID = new TaxID()
			{
				TaxCode = "YYY",
				TaxRate = 1.5m,
				TaxType = new CodeDescriptionPair() { Code = "YYY" },
			};
			taxTransaction1.OSTaxBase = 1255m;
			taxTransaction1.OSTaxAmount = 18.33m;

			transaction.TaxTransactionCollection.Add(taxTransaction1);

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlValue = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString().Replace(" ", "");
			AssertNotContains("<Tributos>", actualXmlValue);
			AssertEquals("Unsupported TaxSuperTypeList code TRX", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Opcionales Node

		public void TestOpcionalesNodeForComplianceSubTypeElegibilityIsNotMiPyme()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			var expectedXmlResult = "<Opcionales>";

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var actualXmlResult = builder.BuildFEDetRequestInfo(transaction, "", Factory).ToString();
			AssertNotContains("Opcionales node must not be present if the compliance subtype is not MiPyme", expectedXmlResult, actualXmlResult);
		}

		public void TestOpcionalesNodeForComplianceSubTypeNullAndEmpty()
		{
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.ComplianceSubType = null;
			AssertComplianceSubTypeHasNoValue();

			transaction.ComplianceSubType = ZString.Empty;
			AssertComplianceSubTypeHasNoValue();

			void AssertComplianceSubTypeHasNoValue()
			{
				var actualXmlResult = builder.BuildFEDetRequestInfo(transaction, "", Factory).ToString().Replace(" ", "");
				AssertNotContains("<Opcionales>", actualXmlResult);
			}
		}

		public void TestOpcionalesNodeForComplianceSubTypeMiPymeCreditOrDebitNote()
		{
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();

			TestAllReasonsForAmendingReversing(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDA);
			TestAllReasonsForAmendingReversing(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDB);
			TestAllReasonsForAmendingReversing(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDC);
			TestAllReasonsForAmendingReversing(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB);
			TestAllReasonsForAmendingReversing(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB);
			TestAllReasonsForAmendingReversing(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCC);

			void TestAllReasonsForAmendingReversing(string complianceSubType)
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				transaction.ComplianceSubType = complianceSubType;

				var expectedId = "22";
				var expectedValue = "N";

				AssertOpcionales(transaction, builder, expectedId, expectedValue);

				transaction.OriginalReference = null;
				AssertOpcionales(transaction, builder, expectedId, expectedValue);

				transaction.OriginalReference = new OriginalReference();
				transaction.OriginalReference.OriginalTransactionAmendingReversingReason = null;
				AssertOpcionales(transaction, builder, expectedId, expectedValue);

				transaction.OriginalReference.OriginalTransactionAmendingReversingReason = new CodeDescriptionPair();
				transaction.OriginalReference.OriginalTransactionAmendingReversingReason.Code = null;
				AssertOpcionales(transaction, builder, expectedId, expectedValue);

				transaction.OriginalReference.OriginalTransactionAmendingReversingReason.Code = ZString.Empty;
				AssertOpcionales(transaction, builder, expectedId, expectedValue);

				transaction.OriginalReference.OriginalTransactionAmendingReversingReason.Code = "IAM";
				AssertOpcionales(transaction, builder, expectedId, expectedValue);

				expectedValue = "S";

				transaction.OriginalReference.OriginalTransactionAmendingReversingReason.Code = "MIR";
				AssertOpcionales(transaction, builder, expectedId, expectedValue);
			}
		}

		public void TestOpcionalesNodeForComplianceSubTypeMiPymeInvoice_CheckNullAndEmptys()
		{
			var (branch, bank, organization) = TestData();

			var builderXml = new FEDetRequestBuilder();
			var expectedId = "2101";
			var expectedValue = ZString.Empty;

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA;
			transaction.OrganizationAddress = null;
			AssertOpcionales(transaction, builderXml, expectedId, expectedValue);

			transaction.OrganizationAddress = new OrganizationAddress() { OrganizationCode = null };
			AssertOpcionales(transaction, builderXml, expectedId, expectedValue);

			transaction.OrganizationAddress.OrganizationCode = ZString.Empty;
			AssertOpcionales(transaction, builderXml, expectedId, expectedValue);

			transaction.OrganizationAddress.OrganizationCode = organization.OH_Code;
			transaction.Branch = new Branch() { Code = null };
			AssertOpcionales(transaction, builderXml, expectedId, expectedValue);

			transaction.Branch.Code = ZString.Empty;
			AssertOpcionales(transaction, builderXml, expectedId, expectedValue);

			expectedValue = "1111111199999999988888";

			transaction.Branch.Code = branch.GB_Code;
			transaction.OSCurrency = null;
			AssertOpcionales(transaction, builderXml, expectedId, expectedValue);

			transaction.OSCurrency = new Currency() { Code = null };
			AssertOpcionales(transaction, builderXml, expectedId, expectedValue);

			transaction.OSCurrency.Code = ZString.Empty;
			AssertOpcionales(transaction, builderXml, expectedId, expectedValue);
		}

		public void TestOpcionalesNodeForComplianceSubTypeMiPymeInvoice()
		{
			var (branch, bank, organization) = TestData();
			var builderXml = new FEDetRequestBuilder();

			AssertUniqueAccountNumber(false);

			bank.AB_FullAccountNumber = ZString.Empty;
			Factory.Save();

			AssertUniqueAccountNumber(true);

			bank.Delete();
			Factory.Save();

			AssertHasNoBankAccount();

			void AssertUniqueAccountNumber(bool isEmptyUniqueAccountNumber)
			{
				var expectedId = "2101";
				var expectedValue = ZString.Empty;

				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA;
				AssertOpcionales(transaction, builderXml, expectedId, expectedValue);

				transaction.OrganizationAddress = new OrganizationAddress();
				transaction.OrganizationAddress.OrganizationCode = "XXXXX";
				AssertOpcionales(transaction, builderXml, expectedId, expectedValue);

				transaction.OrganizationAddress.OrganizationCode = organization.OH_Code;
				transaction.Branch = new Branch() { Code = "XXX" };
				AssertOpcionales(transaction, builderXml, expectedId, expectedValue);

				if (!isEmptyUniqueAccountNumber)
				{
					expectedValue = "1111111199999999988888";
				}

				transaction.Branch.Code = branch.GB_Code;
				transaction.OSCurrency = new Currency() { Code = "---" };
				AssertOpcionales(transaction, builderXml, expectedId, expectedValue);

				transaction.OSCurrency.Code = "EUR";
				AssertOpcionales(transaction, builderXml, expectedId, expectedValue);

				transaction.OSCurrency.Code = bank.AB_RX_NKAccountCurrency;
				AssertOpcionales(transaction, builderXml, expectedId, expectedValue);

				transaction.Branch.Code = "BR2";
				AssertOpcionales(transaction, builderXml, expectedId, ZString.Empty);
			}

			void AssertHasNoBankAccount()
			{
				var expectedId = "2101";
				var expectedValue = ZString.Empty;

				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA;
				transaction.OrganizationAddress = new OrganizationAddress() { OrganizationCode = organization.OH_Code };
				transaction.OSCurrency = new Currency() { Code = "AUD" };
				transaction.Branch = new Branch() { Code = branch.GB_Code };
				AssertOpcionales(transaction, builderXml, expectedId, expectedValue);
			}
		}

		void AssertOpcionales(TransactionInfo transaction, IFEDetRequestBuilder builder, string id, string valor)
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var expectedXmlResult = $@"<Opcional>
        <Id>{id}</Id>
        <Valor>{valor}</Valor>
      </Opcional>";

			var actualXmlResult = builder.BuildFEDetRequestInfo(transaction, "", Factory).ToString();
			AssertContains(expectedXmlResult, actualXmlResult);
		}

		(GlbBranch branch, AccBankAccount bank, OrgHeader organization) TestData()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BR1";

			var bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_GB = branch.PK;
			bank.AB_GC = branch.Company.PK;
			bank.AB_RX_NKAccountCurrency = branch.Company.GC_RX_NKLocalCurrency;
			bank.AB_IsDefaultReceiptBankAccount = true;
			bank.AB_IsActive = true;
			bank.AB_FullAccountNumber = "1111111199999999988888";

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "Debtor";

			Factory.Save();

			return (branch, bank, organization);
		}

		public void TestOpcionalesNodeForComplianceSubTypeMiPymeOpcionalNodeID27()
		{
			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA;

			var expectedXmlResult = $@"<Opcional>
        <Id>27</Id>
        <Valor>ADC</Valor>
      </Opcional>";

			var actualXmlResult = builder.BuildFEDetRequestInfo(transaction, "", Factory).ToString();
			AssertContains(expectedXmlResult, actualXmlResult);

			transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA;
			actualXmlResult = builder.BuildFEDetRequestInfo(transaction, "", Factory).ToString();
			AssertNotContains(expectedXmlResult, actualXmlResult);

			transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDB;
			actualXmlResult = builder.BuildFEDetRequestInfo(transaction, "", Factory).ToString();
			AssertNotContains(expectedXmlResult, actualXmlResult);
		}

		#endregion

		#region CondicionIVAReceptor

		public void TestFECAEDetRequest_HasCondicionIvaReceptor()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(new RegistrationNumber
			{
				Type = new RegistrationNumberType { Code = "IVX" },
				CountryOfIssue = new Country { Code = CountryCodes.Argentina }
			});

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();

			var xmlResult = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString();

			AssertContains("<CondicionIVAReceptorId>9</CondicionIVAReceptorId>", xmlResult);
		}

		public void TestFECAEDetRequest_NoCondicionIvaReceptor()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(new RegistrationNumber
			{
				Type = new RegistrationNumberType { Code = "XXX" },
				CountryOfIssue = new Country { Code = CountryCodes.Argentina }
			});

			var builder = (IFEDetRequestBuilder)new FEDetRequestBuilder();

			var xmlResult = builder.BuildFEDetRequestInfo(transaction, FEV1NameSpace, Factory).ToString();

			AssertNotContains("<CondicionIVAReceptorId", xmlResult);
		}

		#endregion

		PostingJournal CreatePostingJournal()
		{
			var result = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);

			result.VATTaxID = new TaxID();
			result.TaxMessageID = new TaxMessageID();

			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			accComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
		}

		AccComplianceSequence accComplianceSequence;
	}
}
