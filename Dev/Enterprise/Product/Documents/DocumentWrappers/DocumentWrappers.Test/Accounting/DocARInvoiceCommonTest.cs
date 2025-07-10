using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort;
using Enterprise.DocumentWrappers.ProcessManagement;
using Enterprise.DocumentWrappers.Warehouse;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using TransactionTypes = Enterprise.ZArchitecture.Core.TransactionTypes;
using Transport = Enterprise.Freight.Business.Transport;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocARInvoice))]
	public abstract class DocARInvoiceCommonTest : DocARBaseInvoiceTest
	{
		readonly List<string> EUCountryCodes = new List<string> {
						Core.Constants.CountryCodes.Malta,
						Core.Constants.CountryCodes.Greece,
						Core.Constants.CountryCodes.Bulgaria,
						Core.Constants.CountryCodes.Cyprus,
						Core.Constants.CountryCodes.Hungary,
						Core.Constants.CountryCodes.IsleOfMan,
						Core.Constants.CountryCodes.Ireland,
						Core.Constants.CountryCodes.UnitedKingdom,
						Core.Constants.CountryCodes.Spain,
						Core.Constants.CountryCodes.Austria,
						Core.Constants.CountryCodes.Belgium,
						Core.Constants.CountryCodes.Denmark,
						Core.Constants.CountryCodes.Estonia,
						Core.Constants.CountryCodes.Finland,
						Core.Constants.CountryCodes.Germany,
						Core.Constants.CountryCodes.Italy,
						Core.Constants.CountryCodes.Netherlands,
						Core.Constants.CountryCodes.Poland,
						Core.Constants.CountryCodes.Latvia,
						Core.Constants.CountryCodes.Lithuania,
						Core.Constants.CountryCodes.Luxembourg,
						Core.Constants.CountryCodes.Portugal,
						Core.Constants.CountryCodes.Romania,
						Core.Constants.CountryCodes.Slovakia,
						Core.Constants.CountryCodes.Slovenia,
						Core.Constants.CountryCodes.Sweden,
						Core.Constants.CountryCodes.Monaco,
						Core.Constants.CountryCodes.France,
						Core.Constants.CountryCodes.CzechRepublic,
				};

		#region Tax Transactions

		public void TestTaxTransactions()
		{
			var invoiceWrapper = (DocARInvoiceCommon)InvoiceWrapper;

			var mockTaxProcessor = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(mockTaxProcessor.Object);

			var taxParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(Invoice as InvoicingBase);

			var taxTransaction1 = Factory.NewWithValidTestData<AccTaxTransaction>();
			var taxTransaction2 = Factory.NewWithValidTestData<AccTaxTransaction>();
			var taxTransaction3 = Factory.NewWithValidTestData<AccTaxTransaction>();

			var result = new AccTaxTransaction[]
				{
					taxTransaction1,
					taxTransaction2
				};

			mockTaxProcessor.Setup(x => x.GetTaxTransactions(taxParent)).Returns(result);

			AssertEquals(2, invoiceWrapper.TaxTransactions.Count);

			var wrappedTaxTransactions = invoiceWrapper.TaxTransactions.Cast<DocTaxTransaction>().Select(x => x.WrappedObject).ToList();
			AssertCollectionContains(taxTransaction1, wrappedTaxTransactions);
			AssertCollectionContains(taxTransaction2, wrappedTaxTransactions);
			AssertCollectionNotContains(taxTransaction3, wrappedTaxTransactions);
		}

		#endregion

		#region TaxTransactionLinePivots

		public void TestTaxTransactionLinePivots_ReturnsNonEmptyPivotCollection()
		{
			var invoiceWrapper = (DocARInvoiceCommon)InvoiceWrapper;
			var mockTaxProcessor = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(mockTaxProcessor.Object);

			var taxRecord1 = CreateTaxRecord(GlbCompany.CurrentCompany, CurrencyCodes.Australia);
			var taxRecord2 = CreateTaxRecord(GlbCompany.CurrentCompany, CurrencyCodes.Australia);
			var taxRecord3 = CreateTaxRecord(GlbCompany.CurrentCompany, CurrencyCodes.Australia);

			var line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			var line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			var line3 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();

			var pivot1 = CreateTaxRecordTransactionLinePivot(taxRecord1, line1);
			var pivot2 = CreateTaxRecordTransactionLinePivot(taxRecord2, line2);
			var pivot3 = CreateTaxRecordTransactionLinePivot(taxRecord3, line3);

			var expectedPivots = new AccTaxRecordTransactionLinePivot[] { pivot1, pivot3 };
			mockTaxProcessor.Setup(x => x.LoadTaxRecordPivots(It.IsAny<AccTaxTransaction[]>())).Returns(expectedPivots);

			var pivots = invoiceWrapper.TaxTransactionLinePivots.Cast<DocTaxTransactionLinePivot>().Select(x => x.WrappedObject).ToList();
			AssertContainsExactElementsInAnyOrder(expectedPivots, pivots);
		}

		public void TestTaxTransactionLinePivots_ReturnsEmptyPivotCollection()
		{
			var invoiceWrapper = (DocARInvoiceCommon)InvoiceWrapper;
			var mockTaxProcessor = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(mockTaxProcessor.Object);

			var expectedPivots = Array.Empty<AccTaxRecordTransactionLinePivot>();
			mockTaxProcessor.Setup(x => x.LoadTaxRecordPivots(It.IsAny<AccTaxTransaction[]>())).Returns(expectedPivots);

			var pivots = invoiceWrapper.TaxTransactionLinePivots.Cast<DocTaxTransactionLinePivot>().Select(x => x.WrappedObject).ToList();
			AssertContainsExactElementsInAnyOrder(expectedPivots, pivots);
		}

		[ExpectNoExceptions]
		public void TestTaxTransactionLinePivots_InvokesTaxProcessor_WithEmptyTaxRecords()
		{
			var invoiceWrapper = (DocARInvoiceCommon)InvoiceWrapper;
			var mockTaxProcessor = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(mockTaxProcessor.Object);

			AssertEquals("PreCondition: TaxTransactions count", 0, invoiceWrapper.TaxTransactions.Count);
			_ = invoiceWrapper.TaxTransactionLinePivots;
			mockTaxProcessor.Verify(x => x.LoadTaxRecordPivots(Array.Empty<AccTaxTransaction>()));
		}

		[ExpectNoExceptions]
		public void TestTaxTransactionLinePivots_InvokesTaxProcessor_WithProvidedInputTaxRecords()
		{
			var invoiceWrapper = (DocARInvoiceCommon)InvoiceWrapper;
			var mockTaxProcessor = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(mockTaxProcessor.Object);

			var taxRecord1 = Factory.New<AccTaxTransaction>();
			var taxRecord2 = Factory.New<AccTaxTransaction>();
			var taxRecord3 = Factory.New<AccTaxTransaction>();

			var docTaxRecord1 = DocTaxTransaction.New(taxRecord1, Factory);
			var docTaxRecord2 = DocTaxTransaction.New(taxRecord2, Factory);
			var docTaxRecord3 = DocTaxTransaction.New(taxRecord3, Factory);

			invoiceWrapper.TaxTransactions.AddRange(docTaxRecord1, docTaxRecord3);

			_ = invoiceWrapper.TaxTransactionLinePivots;
			mockTaxProcessor.Verify(x => x.LoadTaxRecordPivots(taxRecord1, taxRecord3));
		}

		public void TestTaxTransactionLinePivots_AreTaxTransactionLinePivotsInOrder()
		{
			var invoiceWrapper = (DocARInvoiceCommon)InvoiceWrapper;
			var mockTaxProcessor = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(mockTaxProcessor.Object);

			var taxRecord1 = CreateTaxRecord(GlbCompany.CurrentCompany, CurrencyCodes.Australia)
;
			taxRecord1.ATT_TaxSystemCode = "ISS";
			var taxRecord2 = CreateTaxRecord(GlbCompany.CurrentCompany, CurrencyCodes.Australia);
			taxRecord2.ATT_TaxSystemCode = "DNC";
			var taxRecord3 = CreateTaxRecord(GlbCompany.CurrentCompany, CurrencyCodes.Australia);
			taxRecord3.ATT_TaxSystemCode = "ISS";
			var taxRecords = new AccTaxTransaction[] { taxRecord1, taxRecord2, taxRecord3 };

			var line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_Sequence = 3;
			var line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_Sequence = 2;
			var line3 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line3.AL_Sequence = 1;

			var pivot1 = CreateTaxRecordTransactionLinePivot(taxRecord1, line1);
			var pivot2 = CreateTaxRecordTransactionLinePivot(taxRecord2, line2);
			var pivot3 = CreateTaxRecordTransactionLinePivot(taxRecord3, line3);

			var expectedPivots = new AccTaxRecordTransactionLinePivot[] { pivot1, pivot2, pivot3 };
			var expectedPivotsInOrder = new AccTaxRecordTransactionLinePivot[] { pivot2, pivot3, pivot1 };
			mockTaxProcessor.Setup(x => x.LoadTaxRecordPivots(It.IsAny<AccTaxTransaction[]>())).Returns(expectedPivots);

			var actualPivots = invoiceWrapper.TaxTransactionLinePivots.Cast<DocTaxTransactionLinePivot>().Select(x => x.WrappedObject).ToList();
			AssertContainsExactElementsInExactOrder(expectedPivotsInOrder, actualPivots);
		}

		#endregion

		public void TestOSTotalWhileCurrencyIsNull()
		{
			var invoiceWrapper = (DocARInvoiceCommon)InvoiceWrapper;
			var osTotal = invoiceWrapper.OSTotal;
			AssertNotNull(osTotal);
			invoiceWrapper.TransactionHeader.AH_RX_NKTransactionCurrency = null;
			osTotal = invoiceWrapper.OSTotal;
			AssertNotNull(osTotal);
		}

		public void TestIsTaxCoreEInvoicing()
		{
			foreach (var countryCode in TaxCoreCountryHelper.GetTaxCoreSupportedCountries())
			{
				var invoicingBase = Factory.New<ARInvoice>();
				GlbCompany.CurrentCompany.SetCountry(countryCode);
				invoicingBase.AH_Ledger = LedgerTypes.AccountsReceivable;
				invoicingBase.AH_TransactionType = TransactionTypes.Invoice;
				(var pivot, _) = CreateInvoicePivotAndBatch(invoicingBase.PK, 100, Core.Constants.EInvoicingPivotState.Succeed);

				var arInvoiceWrapper = DocARInvoice.New(invoicingBase, invoicingBase.Factory);
				Assert(invoicingBase.IsApprovedByGovt);
				Assert(arInvoiceWrapper.IsTaxCoreEInvoicing);

				pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Queued;
				Assert(!invoicingBase.IsApprovedByGovt);
				Assert(!arInvoiceWrapper.IsTaxCoreEInvoicing);
			}
		}

		public void TestInvoiceAuthorisationCreateUserIDNumber()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var wrapper = new DocARInvoiceCommonWrapperForTest(invoice, Factory);
			Factory.Save();

			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var staffCertificate = staff.Certificates.AddNew();
			staffCertificate.XZ_Type = CertificateTypePairList.Codes.TFN;
			staffCertificate.XZ_RefNumber = "225369147";
			staffCertificate.XZ_ParentID = staff.PK;
			staffCertificate.XZ_ParentTableCode = GlbStaffSchema.Constants.Prefix;

			AssertEquals("225369147", wrapper.InvoiceAuthorisationCreateUserIDNumber);
		}

		public void TestInvoiceAuthorisationRecordTime()
		{
			var wrapper = new DocARInvoiceCommonWrapperForTest(InvoicingBase, Factory);

			DateTimeOffset.TryParse("2020-12-09 06:29:16.7973 +01:00", out var dateTime);
			var time = new ZDateTimeOffset(dateTime);
			var authorizationRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_ParentId = InvoicingBase.PK;
			authorizationRecord.AHF_ParentTableCode = "AH";
			authorizationRecord.AHF_DateTime = time;

			AssertEquals("09 Dec 2020 06:29:16", wrapper.InvoiceAuthorisationRecordTime);
		}

		public void TestInvoiceAuthorisationRecord()
		{
			Assert(ARInvoiceWrapper.InvoiceAuthorisationRecordNumber.IsEmpty);
			Assert(ARInvoiceWrapper.InvoiceAuthorisationRecordCounter.IsEmpty);
			Assert(ARInvoiceWrapper.InvoiceAuthorisationRecordIDNumber.IsEmpty);
			Assert(ARInvoiceWrapper.InvoiceAuthorisationRecordVerificationURL.IsEmpty);
			Assert(ARInvoiceWrapper.InvoiceAuthorisationRecordPlaceOfIssue.IsEmpty);

			var authorizationRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_ParentId = InvoicingBase.PK;
			authorizationRecord.AHF_ParentTableCode = "AH";
			authorizationRecord.AHF_Number = "number132";
			authorizationRecord.AHF_Counter = "counter132";
			authorizationRecord.AHF_IDNumber = "AHF_IDNumber";
			authorizationRecord.AHF_VerificationUrl = "URL";
			authorizationRecord.AHF_PlaceOfIssue = "12010";

			AssertEquals("number132", ARInvoiceWrapper.InvoiceAuthorisationRecordNumber);
			AssertEquals("counter132", ARInvoiceWrapper.InvoiceAuthorisationRecordCounter);
			AssertEquals("AHF_IDNumber", ARInvoiceWrapper.InvoiceAuthorisationRecordIDNumber);
			AssertEquals("URL", ARInvoiceWrapper.InvoiceAuthorisationRecordVerificationURL);
			AssertEquals("12010", ARInvoiceWrapper.InvoiceAuthorisationRecordPlaceOfIssue);
		}

		public void TestInvoiceAuthorisationRecord_IssuerAuthorisationData()
		{
			var authorizationRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_ParentId = InvoicingBase.PK;
			authorizationRecord.AHF_ParentTableCode = "AH";

			var expectedResult = ZString.Empty;
			AssertAuthorisationData(null, expectedResult);
			AssertAuthorisationData(ZBlob.Empty, expectedResult);

			expectedResult = "T3IbVbFTnIPYUTCODK89pw+aaQcCBlYEESj6otOaeA2kxSvxSfrjSHJ8dO5ZhK5TyJK48qA+ns62ldBw/SvCC+h9xE0VEpvS7qw2bIhB67d6jYRfCaponFxbf50zs25AyLDuC+gNgrPEZ75Oh1zKJFUQgOtohcSV4ZnZBJrJMl4Zh8rRiPDupIc4d4kD3ny3GSnyIgty1j3ssXINXt01RKoadg63A6+wRFmP3i+Ci7bbQllYMfCOtSjO0CE7TQPlzjeRkA1Y2fOB7vsc5BRTw5BTHLxUa34OX5koY+mJUjkThL5dMsnKwb6E/KFK9XHa9+NBp+ILl3hAq4QY3dZVTQ==";
			var valueIssuerAuthorisationData = Convert.FromBase64String(expectedResult);
			AssertAuthorisationData(valueIssuerAuthorisationData, expectedResult);

			void AssertAuthorisationData(ZBlob issuerAuthorisationData, ZString result)
			{
				authorizationRecord.AHF_IssuerAuthorizationData = issuerAuthorisationData;
				AssertEquals(result, ARInvoiceWrapper.InvoiceAuthorisationRecordIssuerAuthorizationData);
			}
		}

		public void TestIsTransactionEligibleForEInvoicing()
		{
			var invoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			Assert("IsTransactionEligibleForEInvoicing is false if there is no pivot", !invoiceWrapper.IsTransactionEligibleForEInvoicing);

			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_ParentID = InvoicingBase.PK;

			invoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(invoiceWrapper.IsTransactionEligibleForEInvoicing);
		}

		public void TestEInvoicingGovernmentAllocatedID()
		{
			var invoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(string.Empty, InvoicingBase.AH_GovernmentAllocatedID);
			AssertEquals("not yet available", invoiceWrapper.EmptyEInvoicingGovernmentAllocatedIDMessage);
			AssertEquals(string.Empty, invoiceWrapper.EInvoicingGovernmentAllocatedID);

			InvoicingBase.AH_GovernmentAllocatedID = "newGovID123";
			invoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("not yet available", invoiceWrapper.EmptyEInvoicingGovernmentAllocatedIDMessage);
			AssertEquals("newGovID123", invoiceWrapper.EInvoicingGovernmentAllocatedID);
		}

		public void TestInvoiceAuthorisationRecordDebtorNumber_Country_Implement_IDebtorNumberWrapperProvider()
		{
			var expectedResult = "G03 - Gastos en general";
			var authorizationRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_ParentId = InvoicingBase.PK;

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IDebtorNumberProvider>().Setup(x => x.GetDebtorName(It.IsAny<AccTransactionHeaderAuthorisationRecord>())).Returns(expectedResult);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);
			var result = ARInvoiceWrapper.InvoiceAuthorisationRecordDebtorNumber;

			AssertEquals(expectedResult, result);
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Times.Once);
			mockIAccountingCountryFactory.As<IDebtorNumberProvider>().Verify(x => x.GetDebtorName(authorizationRecord), Times.Once);
		}

		public void TestInvoiceAuthorisationRecordDebtorNumber_Country_NotImplement_IDebtorNumberWrapperProvider()
		{
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);
			var result = ARInvoiceWrapper.InvoiceAuthorisationRecordDebtorNumber;

			AssertEquals(ZString.Empty, result);
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestInvoiceAuthorisationRecordDebtorNumber_LazyLoad()
		{
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);
			var result = ARInvoiceWrapper.InvoiceAuthorisationRecordDebtorNumber;

			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Times.Once);
			mockIGlobalAccountingCountryFactory.Reset();

			result = ARInvoiceWrapper.InvoiceAuthorisationRecordDebtorNumber;

			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Times.Never);
		}

		public void TestLablesEnumTranslationLabels()
		{
			var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
			var mockILabelTranslator = new Mock<ILabelTranslator>();
			var expectedTranslation = "XXXX";
			var labelsWithMoreThanOneParameterList = new[] { LabelsEnum.RecipientConsumptionTaxRegimeHeading };

			using (ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object))
			{
				foreach (LabelsEnum label in Enum.GetValues(typeof(LabelsEnum)))
				{
					if (labelsWithMoreThanOneParameterList.Contains(label))
					{
						continue;
					}

					var lblName = label.ToString();
					mockILabelTranslator.Setup(x => x.GetTranslation(label)).Returns(expectedTranslation);
					mockIAccountingDependencyFactory.Setup(x => x.GetCountrySpecificLabelTranslator()).Returns(mockILabelTranslator.Object);

					PropertyInfo pi = ARInvoiceWrapper.GetType().GetProperty(lblName);
					var actualResult = pi.GetValue(ARInvoiceWrapper);

					mockIAccountingDependencyFactory.Verify(x => x.GetCountrySpecificLabelTranslator().GetTranslation(label), Times.Once);
					AssertEquals(lblName, actualResult, expectedTranslation);
				}
			}
		}

		public void TestRecipientConsumptionTaxRegimeHeading()
		{
			var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
			var mockILabelTranslator = new Mock<ILabelTranslator>();

			AssertTranslationAndVerifyWithTaxDescription(Core.Constants.CountryCodes.Argentina, "IVA", "XXXXXXX IVA");
			AssertTranslationAndVerifyWithTaxDescription(Core.Constants.CountryCodes.Brazil, "CMT", "XXXXXXX CMT");

			void AssertTranslationAndVerifyWithTaxDescription(string country, string taxDescription, string expectedTraslation)
			{
				using (ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object))
				{
					InvoicingBase.Company.GC_RN_NKCountryCode = country;

					mockILabelTranslator.Setup(x => x.GetTranslation(LabelsEnum.RecipientConsumptionTaxRegimeHeading, It.IsAny<string>())).Returns(expectedTraslation);
					mockIAccountingDependencyFactory.Setup(x => x.GetCountrySpecificLabelTranslator()).Returns(mockILabelTranslator.Object);

					var actualResult = ARInvoiceWrapper.RecipientConsumptionTaxRegimeHeading;

					mockIAccountingDependencyFactory.Verify(x => x.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.RecipientConsumptionTaxRegimeHeading, taxDescription), Times.Once);
					AssertEquals(expectedTraslation, actualResult);
				}
			}
		}

		#region TestRecipientConsumptionTaxRegimeDescription
		[ExpectNoExceptions]
		public void TestRecipientConsumptionTaxRegimeDescription_IAccountingCountryComplianceGlobalFactory_GetsInvoiceCountryAsParameter()
		{
			var invoice = ARInvoiceWrapper;

			var mockIAccountingCountryComplianceGlobalFactory = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory();
			var recipientConsumptionTaxRegimeDescription = invoice.RecipientConsumptionTaxRegimeDescription;
			mockIAccountingCountryComplianceGlobalFactory.Verify(x => x.GetFeatureInterface<IRecipientConsumptionTaxRegime>("AU"));

			invoice.TransactionHeader.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;
			recipientConsumptionTaxRegimeDescription = invoice.RecipientConsumptionTaxRegimeDescription;
			mockIAccountingCountryComplianceGlobalFactory.Verify(x => x.GetFeatureInterface<IRecipientConsumptionTaxRegime>("AR"));
		}

		public void TestRecipientConsumptionTaxRegimeDescription_Empty_WhenCountryDoesNotImplement_IRecipientConsumptionTaxRegime()
		{
			var invoice = ARInvoiceWrapper;
			var mockIAccountingCountryComplianceGlobalFactory = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory();

			var recipientConsumptionTaxRegimeDescription = invoice.RecipientConsumptionTaxRegimeDescription;

			Assert(recipientConsumptionTaxRegimeDescription.IsEmpty);
		}

		public void TestRecipientConsumptionTaxRegimeDescription_Empty_WhenInvoiceHeaderIsNull()
		{
			var invoice = ARInvoiceWrapper;
			var mockIRecipientConsumptionTaxRegime = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory().SetupFeatureInterface<IRecipientConsumptionTaxRegime>();
			mockIRecipientConsumptionTaxRegime.Setup(x => x.GetOrgCusCodes()).Returns(["IVF"]);
			AssertNull("Precondition: Invoice.Header", invoice.InvoicingBase.Header);

			var recipientConsumptionTaxRegimeDescription = invoice.RecipientConsumptionTaxRegimeDescription;

			Assert(recipientConsumptionTaxRegimeDescription.IsEmpty);
		}

		public void TestRecipientConsumptionTaxRegimeDescription_Empty_WhenIRecipientConsumptionTaxRegime_GetOrgCusCodes_ReturnsCodeTypeNotAddedToInvoiceOrg()
		{
			var invoice = SetupInvoiceWithOrgHeader(["IVE"], Core.Constants.CountryCodes.Argentina);
			var mockIRecipientConsumptionTaxRegime = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory().SetupFeatureInterface<IRecipientConsumptionTaxRegime>();
			mockIRecipientConsumptionTaxRegime.Setup(x => x.GetOrgCusCodes()).Returns(["IVF"]);

			var recipientConsumptionTaxRegimeDescription = invoice.RecipientConsumptionTaxRegimeDescription;

			Assert(recipientConsumptionTaxRegimeDescription.IsEmpty);
		}

		public void TestRecipientConsumptionTaxRegimeDescription_Empty_WhenIRecipientConsumptionTaxRegime_GetOrgCusCodes_ReturnsCodeTypeNotAddedToCustomsCodesList()
		{
			var orgCusCodes = new ZString[] { "XXX" };
			var invoice = SetupInvoiceWithOrgHeader(orgCusCodes, Core.Constants.CountryCodes.Argentina);
			var mockIRecipientConsumptionTaxRegime = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory().SetupFeatureInterface<IRecipientConsumptionTaxRegime>();
			mockIRecipientConsumptionTaxRegime.Setup(x => x.GetOrgCusCodes()).Returns(orgCusCodes);

			var recipientConsumptionTaxRegimeDescription = invoice.RecipientConsumptionTaxRegimeDescription;

			Assert(recipientConsumptionTaxRegimeDescription.IsEmpty);
		}

		public void TestRecipientConsumptionTaxRegimeDescription_ReturnsCodeValue_WhenIRecipientConsumptionTaxRegime_GetOrgCusCodes_ReturnsOneCodeTypeAddedToInvoiceOrg()
		{
			var orgCusCodes = new ZString[] { "IVF" };
			var invoice = SetupInvoiceWithOrgHeader(orgCusCodes, Core.Constants.CountryCodes.Argentina);
			var mockIRecipientConsumptionTaxRegime = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory().SetupFeatureInterface<IRecipientConsumptionTaxRegime>();
			mockIRecipientConsumptionTaxRegime.Setup(x => x.GetOrgCusCodes()).Returns(orgCusCodes);

			var recipientConsumptionTaxRegimeDescription = invoice.RecipientConsumptionTaxRegimeDescription;

			AssertEquals((ZString)"IVA CONSUMIDOR FINAL", recipientConsumptionTaxRegimeDescription);

			orgCusCodes = new ZString[] { "GCR" };
			invoice = SetupInvoiceWithOrgHeader(orgCusCodes, Core.Constants.CountryCodes.Australia);
			mockIRecipientConsumptionTaxRegime.Setup(x => x.GetOrgCusCodes()).Returns(orgCusCodes);

			recipientConsumptionTaxRegimeDescription = invoice.RecipientConsumptionTaxRegimeDescription;

			AssertEquals((ZString)"Australian Corporation Number (Government Corporation Code)", recipientConsumptionTaxRegimeDescription);
		}

		public void TestRecipientConsumptionTaxRegimeDescription_ReturnsCodeValue_WhenIRecipientConsumptionTaxRegime_GetOrgCusCodes_ReturnsMutipleCodeTypesAddedToInvoiceOrg()
		{
			var orgCusCodes = new ZString[] { "IVE", "IVF" };
			var invoice = SetupInvoiceWithOrgHeader(orgCusCodes, Core.Constants.CountryCodes.Argentina);
			var mockIRecipientConsumptionTaxRegime = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory().SetupFeatureInterface<IRecipientConsumptionTaxRegime>();
			mockIRecipientConsumptionTaxRegime.Setup(x => x.GetOrgCusCodes()).Returns(orgCusCodes);

			var recipientConsumptionTaxRegimeDescription = invoice.RecipientConsumptionTaxRegimeDescription;

			AssertEquals((ZString)"IVA EXENTO", recipientConsumptionTaxRegimeDescription);

			orgCusCodes = new ZString[] { "MED", "GCR" };
			invoice = SetupInvoiceWithOrgHeader(orgCusCodes, Core.Constants.CountryCodes.Australia);
			mockIRecipientConsumptionTaxRegime.Setup(x => x.GetOrgCusCodes()).Returns(orgCusCodes);

			recipientConsumptionTaxRegimeDescription = invoice.RecipientConsumptionTaxRegimeDescription;

			AssertEquals((ZString)"Medicare Card Number", recipientConsumptionTaxRegimeDescription);
		}
		#endregion

		public void TestInvoiceAuthorisationBuyerIDNumber()
		{
			foreach (var countryCode in TaxCoreCountryHelper.GetTaxCoreSupportedCountries())
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
					InvoicingBase.AH_OH = orgHeader.PK;
					var cusCode = orgHeader.CustomsCodes.AddNew();
					cusCode.OK_CodeType = OrgCusCode.CodeTypes.TaxFileCode;
					cusCode.OK_CustomsRegNo = "AUS001";
					cusCode.OK_RN_NKCodeCountry = countryCode;

					AssertEquals("AUS001", ARInvoiceWrapper.InvoiceAuthorisationBuyerIDNumber);
				}
			}
		}

		public void TestInvoiceAuthorisationRecordIssuerCertificateIdentifier()
		{
			var docARInvoiceWrapper = new DocARInvoiceCommonWrapperForTest(InvoicingBase, Factory);

			AssertEquals($"Issuer Certificate Identifier must be return an empty value", ZString.Empty, docARInvoiceWrapper.InvoiceAuthorisationRecordIssuerCertificateIdentifier);

			var authorizationRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_ParentId = InvoicingBase.PK;

			AssertEquals($"Issuer Certificate Identifier must be return an empty value", ZString.Empty, docARInvoiceWrapper.InvoiceAuthorisationRecordIssuerCertificateIdentifier);

			var expectedValue = "20001000000300022824";

			authorizationRecord.AHF_IssuerCertificateIdentifier = expectedValue;

			AssertEquals($"Issuer Certificate Identifier must be return the same value as has been set", expectedValue, docARInvoiceWrapper.InvoiceAuthorisationRecordIssuerCertificateIdentifier);
		}

		public void TestFijiEInvoiceIdentifier()
		{
			InvoicingBase.AH_Ledger = LedgerTypes.AccountsReceivable;
			InvoicingBase.AH_TransactionType = TransactionTypes.Invoice;
			AssertEquals("NORMAL SALE", ARInvoiceWrapper.TaxCoreEInvoiceIdentifier);

			InvoicingBase.AH_TransactionType = TransactionTypes.CreditNote;
			AssertEquals("NORMAL REFUND", ARInvoiceWrapper.TaxCoreEInvoiceIdentifier);
		}

		public void TestEInvoicingGovernmentAllocatedNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Brazil))
			{
				var invoicingBase = Factory.New<ARInvoice>();
				(var pivot, var batch) = CreateInvoicePivotAndBatch(invoicingBase.PK, 100, Constants.EInvoicingPivotState.Succeed);

				var arInvoiceWrapper = DocARInvoice.New(invoicingBase, invoicingBase.Factory);
				Assert(batch.AIB_GovernmentAllocatedNumber.IsEmpty);
				Assert(invoicingBase.EInvoicingGovernmentAllocatedNumber.IsEmpty);
				Assert(arInvoiceWrapper.EInvoicingGovernmentAllocatedNumber.IsEmpty);

				batch.AIB_GovernmentAllocatedNumber = "NumberFromGovernment";
				AssertEquals("NumberFromGovernment", invoicingBase.EInvoicingGovernmentAllocatedNumber);
				AssertEquals("NumberFromGovernment", arInvoiceWrapper.EInvoicingGovernmentAllocatedNumber);
			}
		}

		public void TestEInvoicingAuthorisationDateTime()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Brazil))
			{
				var invoicingBase = Factory.New<ARInvoice>();
				(var pivot, var batch) = CreateInvoicePivotAndBatch(invoicingBase.PK, 100, Constants.EInvoicingPivotState.Succeed);

				var authRecord = CreateInvoiceAuthorisationRecord(invoicingBase.PK, AccTransactionHeaderAuthorisationRecordTypes.Brazil);

				var arInvoiceWrapper = DocARInvoice.New(invoicingBase, invoicingBase.Factory);
				Assert(invoicingBase.EInvoicingAuthorisationDateTime.IsEmpty);
				Assert(arInvoiceWrapper.EInvoicingAuthorisationDateTime.IsEmpty);

				var testTime = authRecord.AHF_DateTime = ZDateTimeOffset.Now.AddSeconds(-15);
				AssertEquals(testTime, invoicingBase.EInvoicingAuthorisationDateTime);
				AssertEquals(testTime, arInvoiceWrapper.EInvoicingAuthorisationDateTime);
			}
		}

		(AccEInvoicingTransactionPivot, AccEInvoicingBatch) CreateInvoicePivotAndBatch(ZGuid invoicePK, int batchNumber, string pivotStatus)
		{
			var batch = Factory.New<AccEInvoicingBatch>();
			batch.AIB_BatchNumber = batchNumber;
			batch.AIB_Status = Constants.EInvoicingBatchState.Ready;
			batch.AIB_GC = GlbCompany.CurrentCompany.PK;
			batch.AIB_SystemCreateTimeUtc = ZDateTime.UtcNow;
			batch.AIB_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;

			var pivot = batch.TransactionPivots.AddNew();
			pivot.AIP_ParentID = invoicePK;
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_Status = pivotStatus;
			pivot.SetCompanyAndCountryCode(batch.Company);

			return (pivot, batch);
		}

		AccTransactionHeaderAuthorisationRecord CreateInvoiceAuthorisationRecord(ZGuid invoicePK, string authorisationRecordType)
		{
			var result = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			result.AHF_RecordType = authorisationRecordType;
			result.AHF_ParentId = invoicePK;
			result.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			return result;
		}

		public void TestOSAndLocalTotalRounding()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Tunisia);

			InvoicingBase.AH_RX_NKTransactionCurrency = "TND";
			InvoicingBase.AH_OSTotalAmount = 202.785M;

			InvoicingBase.AH_LocalExTaxAmount = 150M;
			InvoicingBase.AH_LocalTaxAmount = 52.785M;

			DocARInvoiceCommon invoiceWrapper = (DocARInvoiceCommon)InvoiceWrapper;

			AssertEquals(202.785M, invoiceWrapper.OSTotal);
			AssertEquals(202.785M, invoiceWrapper.LocalTotal);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			AssertEquals(202.785M, invoiceWrapper.OSTotal);
			AssertEquals(202.79M, invoiceWrapper.LocalTotal);

			InvoicingBase.AH_RX_NKTransactionCurrency = "AUD";
			InvoicingBase.AH_OSTotalAmount = 202.785M;

			InvoicingBase.AH_LocalExTaxAmount = 150M;
			InvoicingBase.AH_LocalTaxAmount = 52.785M;

			AssertEquals(202.79M, invoiceWrapper.OSTotal);
			AssertEquals(202.79M, invoiceWrapper.LocalTotal);
		}

		public void TestGroupingByTaxMessageAndRate()
		{
			AccInvMsg msg1 = Factory.NewWithValidTestData<AccInvMsg>();
			msg1.A9_IsShownOnDocuments = true;
			msg1.A9_Description = "MSG1";
			msg1.A9_EnglishMsg = "English msg1";
			msg1.A9_LocalMsg = string.Empty;

			AccInvMsg msg2 = Factory.NewWithValidTestData<AccInvMsg>();
			msg2.A9_IsShownOnDocuments = true;
			msg2.A9_Description = "MSG1";
			msg2.A9_EnglishMsg = "English msg2 that is super long it needs to be longer then 65 characters and have asterisks on the end**";
			msg2.A9_LocalMsg = @"Local
msg2 that is super long it needs to be longer then 65 characters and have asterisks on the end**";

			AccInvMsg msg3 = Factory.NewWithValidTestData<AccInvMsg>();
			msg3.A9_IsShownOnDocuments = false;
			msg3.A9_Description = "MSG3";
			msg3.A9_EnglishMsg = "English msg3";
			msg3.A9_LocalMsg = "Local msg3";

			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_RL_NKClosestPort = "ITROM";
			InvoicingBase.AH_OH = header.PK;

			Factory.Save();

			AccTaxRate notReportableTax = Factory.NewWithValidTestData<AccTaxRate>();
			notReportableTax.AT_Type = AccTaxRate.Types.NotReportable;
			notReportableTax.AT_A9_DefaultVatClass = msg1.PK;

			AccTaxRate rated10 = Factory.NewWithValidTestData<AccTaxRate>();
			rated10.AT_Type = AccTaxRate.Types.Rated;
			rated10.SetRateNumerator_ForTestOnly(10);

			AccTaxRate reverseRated10 = Factory.NewWithValidTestData<AccTaxRate>();
			reverseRated10.AT_Type = AccTaxRate.Types.ReverseRated;
			reverseRated10.SetRateNumerator_ForTestOnly(10);

			AccTaxRate reverseRated15 = Factory.NewWithValidTestData<AccTaxRate>();
			reverseRated15.AT_Type = AccTaxRate.Types.ReverseRated;
			reverseRated15.SetRateNumerator_ForTestOnly(15);

			AccTaxRate exemptWithMessage = Factory.NewWithValidTestData<AccTaxRate>();
			exemptWithMessage.AT_Type = AccTaxRate.Types.Exempt;
			exemptWithMessage.AT_A9_DefaultVatClass = msg2.PK;

			AccTaxRate capRated10 = Factory.NewWithValidTestData<AccTaxRate>();
			capRated10.AT_Type = AccTaxRate.Types.CapitalRated;
			capRated10.SetRateNumerator_ForTestOnly(10);

			AccTaxRate rated0 = Factory.NewWithValidTestData<AccTaxRate>();
			rated0.AT_Type = AccTaxRate.Types.Rated;
			rated0.SetRateNumerator_ForTestOnly(0);

			var line0 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line0.AL_OSExTaxAmount = 100M;

			var line1 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line1.AL_OSExTaxAmount = 100M;
			line1.AL_AT = notReportableTax.PK;

			var line2 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line2.AL_OSExTaxAmount = 200M;
			line2.AL_AT = rated10.PK;

			var line3 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line3.AL_OSExTaxAmount = 300M;
			line3.AL_AT = reverseRated10.PK;

			var line4 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line4.AL_OSExTaxAmount = 400M;
			line4.AL_AT = reverseRated10.PK;

			var line5 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line5.AL_OSExTaxAmount = 500M;
			line5.AL_AT = exemptWithMessage.PK;

			var line6 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line6.AL_OSExTaxAmount = 600M;
			line6.AL_AT = capRated10.PK;

			var line7 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line7.AL_OSExTaxAmount = 700M;
			line7.AL_AT = capRated10.PK;

			var line8 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line8.AL_OSExTaxAmount = 800M;
			line8.AL_AT = rated10.PK;

			var line9 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line9.AL_OSExTaxAmount = 900M;
			line9.AL_AT = reverseRated15.PK;

			var line10 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line10.AL_OSExTaxAmount = 1000M;
			line10.AL_AT = rated0.PK;

			var line11 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line11.AL_OSExTaxAmount = 1100M;
			line11.AL_AT = rated0.PK;
			line11.AL_A9_VATClass = msg1.PK;

			var line12 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line12.AL_OSExTaxAmount = 99M;
			line12.AL_AT = capRated10.PK;
			line12.AL_A9_VATClass = msg2.PK;

			var line13 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line13.AL_OSExTaxAmount = 50M;
			line13.AL_AT = rated0.PK;
			line13.AL_A9_VATClass = msg3.PK;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			string expectedTotalExTaxByRateAndMessageColumn = @"1,600.00
1,300.00
1,100.00
1,050.00
1,000.00
500.00
100.00
99.00
";

			DocARInvoiceCommon invoiceWrapper = (DocARInvoiceCommon)InvoiceWrapper;
			AssertEquals(expectedTotalExTaxByRateAndMessageColumn, invoiceWrapper.TotalExTaxByRateAndMessageColumn);

			string expectedTaxByRateAndMessageColumn = @"
10%


10%


10%
";

			AssertEquals(expectedTaxByRateAndMessageColumn, invoiceWrapper.TaxRateByRateAndMessageColumn);
			AssertEquals(expectedTaxByRateAndMessageColumn, invoiceWrapper.TaxRateByRateAndMessageColumnAlwaysShowPercentage);

			string expectedTotalTaxByRateAndMessageColumn = @"0.00
130.00
0.00
0.00
100.00
0.00
0.00
9.90
";
			AssertEquals(expectedTotalTaxByRateAndMessageColumn, invoiceWrapper.TotalTaxByRateAndMessageColumn);

			string expectedTaxMessageByRateAndMessageColumn = @"Reverse GST

* English msg1
Zero Rated

** English msg2 that is super long it needs to be longer then 65 cha
* English msg1
** English msg2 that is super long it needs to be longer then 65 cha
";

			AssertEquals("Should show English message, as invoice is of Italy but login country is Australia", expectedTaxMessageByRateAndMessageColumn, invoiceWrapper.TaxMessagesByRateAndMessageColumn);

			expectedTaxMessageByRateAndMessageColumn = @"Reverse Charge

1. English msg1
Zero Rated

2. English msg2 that is super long it needs to be longer then 65 cha
1. English msg1
2. English msg2 that is super long it needs to be longer then 65 cha
";
			AssertEquals("Should show English message, as invoice is of Italy but login country is Australia", expectedTaxMessageByRateAndMessageColumn, invoiceWrapper.TaxMessagesByRateAndMessageColumnWithNumbers);

			invoiceWrapper.ResetLinesGroupedByRateAndMessageForTestOnly();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);

			expectedTaxMessageByRateAndMessageColumn = @"Reverse VAT

* English msg1
Zero Rated

** Local msg2 that is super long it needs to be longer then 65 chara
* English msg1
** Local msg2 that is super long it needs to be longer then 65 chara
";
			AssertEquals("Should show Local message, as both invoice country and login country is Australia", expectedTaxMessageByRateAndMessageColumn, invoiceWrapper.TaxMessagesByRateAndMessageColumn);

			expectedTaxMessageByRateAndMessageColumn = @"Reverse Charge

1. English msg1
Zero Rated

2. Local msg2 that is super long it needs to be longer then 65 chara
1. English msg1
2. Local msg2 that is super long it needs to be longer then 65 chara
";
			AssertEquals("Should show Local message, as both invoice country and login country is Australia", expectedTaxMessageByRateAndMessageColumn, invoiceWrapper.TaxMessagesByRateAndMessageColumnWithNumbers);
		}

		public void TestGroupingByTaxMessageAndRateInExtraTaxCountries()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_RL_NKClosestPort = "ITROM";
			InvoicingBase.AH_OH = header.PK;
			Factory.Save();

			AccTaxRate gSTANDQST = Factory.NewWithValidTestData<AccTaxRate>();
			gSTANDQST.AT_Code = "QST";
			gSTANDQST.AT_Type = AccTaxRate.Types.Rated;
			gSTANDQST.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			gSTANDQST.SetRateNumerator_ForTestOnly(10);
			gSTANDQST.SetExtraRate_ForTestOnly(5, 1);

			AccTaxRate gST = Factory.NewWithValidTestData<AccTaxRate>();
			gST.AT_Code = "GST";
			gST.AT_Type = AccTaxRate.Types.Rated;
			gST.SetRateNumerator_ForTestOnly(10);

			AccTaxRate sERANDEDU = Factory.NewWithValidTestData<AccTaxRate>();
			sERANDEDU.AT_Code = "EDU";
			sERANDEDU.AT_Type = AccTaxRate.Types.Rated;
			sERANDEDU.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
			sERANDEDU.SetRateNumerator_ForTestOnly(10);
			sERANDEDU.SetExtraRate_ForTestOnly(5, 1);

			AccTaxRate sER = Factory.NewWithValidTestData<AccTaxRate>();
			sER.AT_Code = "SER";
			sER.AT_Type = AccTaxRate.Types.Rated;
			sER.SetRateNumerator_ForTestOnly(10);

			AccTaxRate sERANDEDU1 = Factory.NewWithValidTestData<AccTaxRate>();
			sERANDEDU1.AT_Code = "EDU";
			sERANDEDU1.AT_Type = AccTaxRate.Types.ServiceTax;
			sERANDEDU1.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
			sERANDEDU1.SetRateNumerator_ForTestOnly(10);
			sERANDEDU1.SetExtraRate_ForTestOnly(5, 1);

			AccTaxRate sER1 = Factory.NewWithValidTestData<AccTaxRate>();
			sER1.AT_Code = "SER";
			sER1.AT_Type = AccTaxRate.Types.ServiceTax;
			sER1.SetRateNumerator_ForTestOnly(10);

			AccTaxRate iVARET = Factory.NewWithValidTestData<AccTaxRate>();
			iVARET.AT_Code = "RET";
			iVARET.AT_Type = AccTaxRate.Types.Rated;
			iVARET.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
			iVARET.SetRateNumerator_ForTestOnly(10);
			iVARET.SetExtraRate_ForTestOnly(5, 1);

			AccTaxRate iVAREF = Factory.NewWithValidTestData<AccTaxRate>();
			iVAREF.AT_Code = "RET";
			iVAREF.AT_Type = AccTaxRate.Types.Rated;
			iVAREF.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
			iVAREF.SetRateNumerator_ForTestOnly(10);
			iVAREF.SetExtraRate_ForTestOnly(1, 2);

			AccTaxRate iVA = Factory.NewWithValidTestData<AccTaxRate>();
			iVA.AT_Code = "IVA";
			iVA.AT_Type = AccTaxRate.Types.Rated;
			iVA.SetRateNumerator_ForTestOnly(10);

			AccTaxRate iNP = Factory.NewWithValidTestData<AccTaxRate>();
			iNP.AT_Code = "INP";
			iNP.AT_Type = AccTaxRate.Types.Rated;
			iNP.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
			iNP.SetRateNumerator_ForTestOnly(10);
			iNP.SetExtraRate_ForTestOnly(5, 1);

			AccTaxRate iNPREF = Factory.NewWithValidTestData<AccTaxRate>();
			iNPREF.AT_Code = "INP";
			iNPREF.AT_Type = AccTaxRate.Types.Rated;
			iNPREF.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
			iNPREF.SetRateNumerator_ForTestOnly(10);
			iNPREF.SetExtraRate_ForTestOnly(1, 2);

			AccTaxRate vAT = Factory.NewWithValidTestData<AccTaxRate>();
			vAT.AT_Code = "VAT";
			vAT.AT_Type = AccTaxRate.Types.Rated;
			vAT.SetRateNumerator_ForTestOnly(10);

			CreateAndAssertLines(gST, gSTANDQST, Constants.CountryCodes.Canada);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				CreateAndAssertLines(sER, sERANDEDU, Constants.CountryCodes.India);
				CreateAndAssertLines(sER1, sERANDEDU1, Constants.CountryCodes.India);
			}
			CreateAndAssertLines(iVA, iVARET, Constants.CountryCodes.Mexico);
			CreateAndAssertLines(iVA, iVAREF, Constants.CountryCodes.Mexico);
			CreateAndAssertLines(vAT, iNP, Constants.CountryCodes.China);
			CreateAndAssertLines(vAT, iNPREF, Constants.CountryCodes.China);
		}

		public void CreateAndAssertLines(AccTaxRate taxRate, AccTaxRate extraTaxRate, string countryCode)
		{
			AccInvMsg msg1 = Factory.NewWithValidTestData<AccInvMsg>();
			msg1.A9_IsShownOnDocuments = true;
			msg1.A9_Description = "MSG1";
			msg1.A9_EnglishMsg = "This is a really long tax message. It should be truncated to 65 characters ";

			AccInvMsg msg2 = Factory.NewWithValidTestData<AccInvMsg>();
			msg2.A9_IsShownOnDocuments = true;
			msg2.A9_Description = "MSG2";
			msg2.A9_EnglishMsg = "English msg2**";

			var line0 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line0.AL_OSExTaxAmount = 1000M;
			line0.AL_AT = taxRate.PK;
			line0.AL_A9_VATClass = msg1.PK;

			var line1 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line1.AL_OSExTaxAmount = 1000M;
			line1.AL_AT = extraTaxRate.PK;
			line1.AL_A9_VATClass = msg2.PK;

			var line2 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line2.AL_OSExTaxAmount = 500M;
			line2.AL_AT = taxRate.PK;
			line2.AL_A9_VATClass = msg1.PK;

			var line3 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line3.AL_OSExTaxAmount = 500M;
			line3.AL_AT = extraTaxRate.PK;
			line3.AL_A9_VATClass = msg2.PK;

			DocARInvoiceCommon invoiceWrapper = (DocARInvoiceCommon)InvoiceWrapper;
			invoiceWrapper.TransactionHeader.Company.GC_RN_NKCountryCode = countryCode;
			ZString expectedTotalExTaxByRateAndMessageColumn = @"1,500.00
1,500.00";
			AssertContains(expectedTotalExTaxByRateAndMessageColumn, invoiceWrapper.TotalExTaxByRateAndMessageColumn);

			ZString expectedTaxByRateAndMessageColumn = @"10%
10%";
			AssertContains(expectedTaxByRateAndMessageColumn, invoiceWrapper.TaxRateByRateAndMessageColumn);
			AssertContains(expectedTaxByRateAndMessageColumn, invoiceWrapper.TaxRateByRateAndMessageColumnAlwaysShowPercentage);

			ZString expectedTotalExtraTaxByRateAndMessageColumn = @"0.00
82.50";
			AssertContains(expectedTotalExtraTaxByRateAndMessageColumn, invoiceWrapper.TotalTaxByRateAndMessageColumn);

			ZString expectedExtraTaxRateByRateAndMessageColumn = @"
5%";
			AssertContains(expectedExtraTaxRateByRateAndMessageColumn, invoiceWrapper.TaxRateByRateAndMessageColumn);
			AssertContains(expectedExtraTaxRateByRateAndMessageColumn, invoiceWrapper.TaxRateByRateAndMessageColumnAlwaysShowPercentage);

			ZString expectedReasonColumn = @"1. This is a really long tax message. It should be truncated to 65 c
2. English msg2**";
			AssertContains(expectedReasonColumn, invoiceWrapper.TaxMessagesByRateAndMessageColumnWithNumbers);
		}

		public void TestTaxRateByRateAndMessageColumnForZeroRatedTaxes()
		{
			AccInvMsg msg1 = Factory.NewWithValidTestData<AccInvMsg>();
			msg1.A9_IsShownOnDocuments = true;
			msg1.A9_Description = "MSG1";
			msg1.A9_EnglishMsg = "English msg1 8";
			msg1.A9_LocalMsg = string.Empty;

			AccInvMsg msg2 = Factory.NewWithValidTestData<AccInvMsg>();
			msg2.A9_IsShownOnDocuments = true;
			msg2.A9_Description = "MSG2";
			msg2.A9_EnglishMsg = "English msg2 18 rated";
			msg2.A9_LocalMsg = string.Empty;

			AccInvMsg msg3 = Factory.NewWithValidTestData<AccInvMsg>();
			msg3.A9_IsShownOnDocuments = true;
			msg3.A9_Description = "MSG3";
			msg3.A9_EnglishMsg = "English msg3 0 Rated";
			msg3.A9_LocalMsg = string.Empty;

			AccInvMsg msg4 = Factory.NewWithValidTestData<AccInvMsg>();
			msg4.A9_IsShownOnDocuments = true;
			msg4.A9_Description = "MSG4";
			msg4.A9_EnglishMsg = "English msg4 Exempt";
			msg4.A9_LocalMsg = string.Empty;

			AccInvMsg msg5 = Factory.NewWithValidTestData<AccInvMsg>();
			msg5.A9_IsShownOnDocuments = true;
			msg5.A9_Description = "MSG5";
			msg5.A9_EnglishMsg = "English msg5 Suspended";
			msg5.A9_LocalMsg = string.Empty;

			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			InvoicingBase.AH_OH = header.PK;

			Factory.Save();

			AccTaxRate rated1 = Factory.NewWithValidTestData<AccTaxRate>();
			rated1.AT_Type = AccTaxRate.Types.Rated;
			rated1.SetRateNumerator_ForTestOnly(1);

			AccTaxRate rated8 = Factory.NewWithValidTestData<AccTaxRate>();
			rated8.AT_Type = AccTaxRate.Types.Rated;
			rated8.SetRateNumerator_ForTestOnly(8);

			AccTaxRate rated18 = Factory.NewWithValidTestData<AccTaxRate>();
			rated18.AT_Type = AccTaxRate.Types.Rated;
			rated18.SetRateNumerator_ForTestOnly(18);

			AccTaxRate rated22 = Factory.NewWithValidTestData<AccTaxRate>();
			rated22.AT_Type = AccTaxRate.Types.Rated;
			rated22.SetRateNumerator_ForTestOnly(22);

			AccTaxRate rated0 = Factory.NewWithValidTestData<AccTaxRate>();
			rated0.AT_Type = AccTaxRate.Types.Rated;
			rated0.SetRateNumerator_ForTestOnly(0);

			AccTaxRate exempt = Factory.NewWithValidTestData<AccTaxRate>();
			exempt.AT_Type = AccTaxRate.Types.Exempt;

			AccTaxRate suspended = Factory.NewWithValidTestData<AccTaxRate>();
			suspended.AT_Type = AccTaxRate.Types.Suspended;
			suspended.SetRateNumerator_ForTestOnly(20);

			var line1 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line1.AL_OSExTaxAmount = 200M;
			line1.AL_AT = rated8.PK;

			var line2 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line2.AL_OSExTaxAmount = 800M;
			line2.AL_AT = rated18.PK;

			var line3 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line3.AL_OSExTaxAmount = 1000M;
			line3.AL_AT = rated0.PK;

			var line4 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line4.AL_OSExTaxAmount = 1100M;
			line4.AL_AT = rated1.PK;
			line4.AL_A9_VATClass = msg2.PK;

			var line5 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line5.AL_OSExTaxAmount = 99M;
			line5.AL_AT = rated0.PK;
			line5.AL_A9_VATClass = msg3.PK;

			var line6 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line6.AL_OSExTaxAmount = 50M;
			line6.AL_AT = rated22.PK;
			line6.AL_A9_VATClass = msg1.PK;

			var line7 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line7.AL_OSExTaxAmount = 50M;
			line7.AL_AT = exempt.PK;
			line7.AL_A9_VATClass = msg4.PK;

			var line8 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line8.AL_OSExTaxAmount = 50M;

			var line9 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line9.AL_OSExTaxAmount = 50M;
			line9.AL_AT = suspended.PK;
			line9.AL_A9_VATClass = msg5.PK;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Portugal);

			AssertEquals(false, AccountingConfigurationRegistry.Instance.DisplayTaxRateInAllLinesOfTaxSummary.Value);

			DocARInvoiceCommon invoiceWrapper = (DocARInvoiceCommon)InvoiceWrapper;
			string expectedTaxByRateAndMessageColumn = @"1%

18%
8%

22%


";
			AssertEquals(expectedTaxByRateAndMessageColumn, invoiceWrapper.TaxRateByRateAndMessageColumn);
			AssertEquals(expectedTaxByRateAndMessageColumn, invoiceWrapper.TaxRateByRateAndMessageColumnAlwaysShowPercentage);

			AccountingConfigurationRegistry.Instance.DisplayTaxRateInAllLinesOfTaxSummary.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(true, AccountingConfigurationRegistry.Instance.DisplayTaxRateInAllLinesOfTaxSummary.Value);
			expectedTaxByRateAndMessageColumn = @"1%
0%
18%
8%
0%
22%
0%
0%
";
			AssertEquals(expectedTaxByRateAndMessageColumn, invoiceWrapper.TaxRateByRateAndMessageColumn);
			AssertEquals(expectedTaxByRateAndMessageColumn, invoiceWrapper.TaxRateByRateAndMessageColumnAlwaysShowPercentage);
		}

		public void TestTaxRateByRateAndMessageColumnAlwaysShowPercentage_WithCalculateTaxAtHeaderLevelInRegistry()
		{
			var defaultValue = OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch
			(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All,
				OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);

			// TaxRateByRateAndMessageColumn = ROL and GroupOrSubtotalStyle = ORF are essential.
			defaultValue.InvoiceLineDisplayOption = "ROL";
			defaultValue.GroupOrSubtotalStyle = "ORF";
			defaultValue.InvoiceLineDisplayOption = "ALX";

			var registryCollection = new InvoiceRollupOrGroupCollection();
			registryCollection.Add(defaultValue);

			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCollection);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);
			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			SetUpInvoiceWrapperForRollUp();

			var msg1 = Factory.NewWithValidTestData<AccInvMsg>();
			// A9_IsShownOnDocuments should be false to make sure TaxRateByRateAndMessageColumn is set properly.
			msg1.A9_IsShownOnDocuments = false;
			msg1.A9_Description = "MSG1";
			msg1.A9_EnglishMsg = "This is a normal tax message.";

			var line1 = AddOriginChargeToInvoice();
			// AL_PreventInvoicePrintGrouping should be true to reproduce.
			line1.AL_PreventInvoicePrintGrouping = true;
			line1.AL_A9_VATClass = ZGuid.NewZGuid();

			// We do need at least to lines to make it work, and AL_PreventInvoicePrintGrouping of this must be false.
			var line2 = AddFreightChargeToInvoice();
			line2.AL_PreventInvoicePrintGrouping = false;
			line2.AL_A9_VATClass = ZGuid.NewZGuid();

			DocARInvoiceCommon aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			aRInvoiceWrapper.ResetLinesGroupedByRateAndMessageForTestOnly();

			// Before retrieving LinesForInvoice, ShowPercentInGSTDisplay will be true without any change.
			AssertNotEquals(ZString.Empty, aRInvoiceWrapper.TaxRateByRateAndMessageColumn.Trim());
			AssertNotEquals(ZString.Empty, aRInvoiceWrapper.TaxRateByRateAndMessageColumnRaw.Trim());

			// Need to call LinesForInvoice to set the ShowPercentInGSTDisplay flag.
			var theLines = aRInvoiceWrapper.LinesForInvoice;
			var line2_wrapped = theLines[0];

			ZString expectedTaxByRateAndMessageColumn = "10,11%";

			AssertEquals(ZString.Empty, aRInvoiceWrapper.TaxRateByRateAndMessageColumn.Trim());
			AssertEquals(ZString.Empty, aRInvoiceWrapper.TaxRateByRateAndMessageColumnRaw.Trim());
			AssertEquals(ZString.Empty, line2_wrapped.Invoice.TaxRateByRateAndMessageColumn.Trim());
			AssertEquals(ZString.Empty, line2_wrapped.Invoice.TaxRateByRateAndMessageColumnRaw.Trim());
			AssertEquals(expectedTaxByRateAndMessageColumn, line2_wrapped.Invoice.TaxRateByRateAndMessageColumnAlwaysShowPercentage.Trim());
			AssertEquals(expectedTaxByRateAndMessageColumn, line2_wrapped.Invoice.TaxRateByRateAndMessageColumnRawAlwaysShowPercentage.Trim());
		}

		public void TestRelatedTransactionJobInvoiceNumber()
		{
			ARInvoice parentInvoice = Factory.NewWithValidTestData<ARInvoice>();
			parentInvoice.AH_TransactionNum = "0001001";
			parentInvoice.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			parentInvoice.AH_TransactionReference = "123456";
			InvoicingBase.AH_TransactionBelongsToGroup = parentInvoice.PK;
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Related transaction Job Invoice Number", "0001001", aRInvoiceWrapper.RelatedTransactionJobInvoiceNumber);

			InvoicingBase.AH_TransactionBelongsToGroup = ZGuid.Empty;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Related transaction Job Invoice Number", ZString.Empty, aRInvoiceWrapper.RelatedTransactionJobInvoiceNumber);
		}

		public void TestRelatedTransactionComplianceSubType()
		{
			ARInvoice parentInvoice = Factory.NewWithValidTestData<ARInvoice>();
			parentInvoice.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			parentInvoice.AH_TransactionReference = "123456";
			InvoicingBase.AH_TransactionBelongsToGroup = parentInvoice.PK;
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Related transaction compliance sub type", PeruComplianceInfo.ComplianceSubTypeCodes.TXI, aRInvoiceWrapper.RelatedTransactionComplianceSubType);

			InvoicingBase.AH_TransactionBelongsToGroup = ZGuid.Empty;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Related transaction compliance sub type", ZString.Empty, aRInvoiceWrapper.RelatedTransactionComplianceSubType);
		}

		public void TestRelatedTransactionGovernmentComplianceNumber()
		{
			ARInvoice parentInvoice = Factory.NewWithValidTestData<ARInvoice>();
			parentInvoice.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			parentInvoice.AH_TransactionReference = "123456";
			InvoicingBase.AH_TransactionBelongsToGroup = parentInvoice.PK;
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Related transaction government compliance number", "123456", aRInvoiceWrapper.RelatedTransactionGovernmentComplianceNumber);

			InvoicingBase.AH_TransactionBelongsToGroup = ZGuid.Empty;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Related transaction government compliance number", ZString.Empty, aRInvoiceWrapper.RelatedTransactionGovernmentComplianceNumber);
		}

		public void TestReasonForAmendmentOrReversal()
		{
			ARInvoice parentInvoice = Factory.NewWithValidTestData<ARInvoice>();
			InvoicingBase.AH_TransactionBelongsToGroup = parentInvoice.PK;
			InvoicingBase.AH_ReceiptType = "WOR";
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Reason for amendment or reversal", "Wrong Organization Code Used", aRInvoiceWrapper.ReasonForAmendmentOrReversal);

			InvoicingBase.AH_ReceiptType = Core.Constants.GenApprovalRequestReasonCode.Code.DamagedGoods;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Reason for amendment or reversal", "Damaged Goods", aRInvoiceWrapper.ReasonForAmendmentOrReversal);

			InvoicingBase.AH_TransactionBelongsToGroup = ZGuid.Empty;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Reason for amendment or reversal", ZString.Empty, aRInvoiceWrapper.ReasonForAmendmentOrReversal);
		}

		public void TestChargeSummaryLinesAmendment()
		{
			AccTaxRate taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Type = "RAT";
			taxRate1.SetRateNumerator_ForTestOnly(10);

			AccTaxRate taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Type = "CAP";
			taxRate2.SetRateNumerator_ForTestOnly(12);

			ARInvoice parentInvoice = Factory.NewWithValidTestData<ARInvoice>();
			parentInvoice.AH_PostDate = new ZDateTime(2012, 12, 01);
			InvoicingLineBase line1 = (InvoicingLineBase)parentInvoice.Lines.AddNew();
			line1.AL_Desc = "This line uses standard Rate";
			line1.AL_AT = taxRate1.PK;
			line1.AL_LineAmount = 100;

			InvoicingLineBase line2 = (InvoicingLineBase)parentInvoice.Lines.AddNew();
			line2.AL_Desc = "This line uses capital Rate";
			line2.AL_AT = taxRate2.PK;
			line2.AL_LineAmount = 200;

			ARCreditNote childInvoice1 = Factory.NewWithValidTestData<ARCreditNote>();
			childInvoice1.AH_PostDate = new ZDateTime(2012, 12, 02);
			childInvoice1.AH_TransactionBelongsToGroup = parentInvoice.PK;

			InvoicingLineBase line3 = (InvoicingLineBase)childInvoice1.Lines.AddNew();
			line3.AL_Desc = "This line uses standard Rate";
			line3.AL_AT = taxRate1.PK;
			line3.AL_LineAmount = -100;

			ARCreditNote childInvoice2 = Factory.NewWithValidTestData<ARCreditNote>();
			childInvoice2.AH_TransactionBelongsToGroup = parentInvoice.PK;
			childInvoice2.AH_PostDate = new ZDateTime(2012, 12, 03);

			InvoicingLineBase line4 = (InvoicingLineBase)childInvoice2.Lines.AddNew();
			line4.AL_Desc = "This line uses capital Rate";
			line4.AL_AT = taxRate2.PK;
			line4.AL_LineAmount = -200;

			DocARInvoice docChild1 = DocARInvoice.New(childInvoice1, Factory);
			AssertEquals("2 lines before the first amendment", 2, docChild1.ChargeSummaryLinesBeforeAmendment.Count);
			AssertEquals(300m, docChild1.ChargeSummaryLinesBeforeAmendment.Sum(x => ((DocChargeSummaryLine)x).TotalChargeAmountInOSCurrency));
			AssertEquals("1 line for the first amendment", 1, docChild1.ChargeSummaryLinesForAmendment.Count);
			AssertEquals(-100m, docChild1.ChargeSummaryLinesForAmendment[0].TotalChargeAmountInOSCurrency);
			AssertEquals("2 lines after the first amendment", 2, docChild1.ChargeSummaryLinesAfterAmendment.Count);
			AssertEquals(200m, docChild1.ChargeSummaryLinesAfterAmendment.Sum(x => ((DocChargeSummaryLine)x).TotalChargeAmountInOSCurrency));

			DocARInvoice docChild2 = DocARInvoice.New(childInvoice2, Factory);
			AssertEquals("2 lines before the second amendment", 2, docChild2.ChargeSummaryLinesBeforeAmendment.Count);
			AssertEquals(200m, docChild2.ChargeSummaryLinesBeforeAmendment.Sum(x => ((DocChargeSummaryLine)x).TotalChargeAmountInOSCurrency));
			AssertEquals("1 line for the second amendment", 1, docChild2.ChargeSummaryLinesForAmendment.Count);
			AssertEquals(-200m, docChild2.ChargeSummaryLinesForAmendment[0].TotalChargeAmountInOSCurrency);
			AssertEquals("2 lines after the second amendment", 2, docChild2.ChargeSummaryLinesAfterAmendment.Count);
			AssertEquals(0m, docChild2.ChargeSummaryLinesAfterAmendment.Sum(x => ((DocChargeSummaryLine)x).TotalChargeAmountInOSCurrency));
		}

		public void TestRecipientAddress()
		{
			OrgAddress address = Invoice.Branch.OrgProxy.Addresses[0];
			address.OA_Address1 = "TestAddress1";
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);

			address = Invoice.Branch.OrgProxy.Addresses[1];
			address.OA_Address1 = "TestAddress2";
			address.OA_Address2 = "222";
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);

			var inactiveARAddress = Invoice.Branch.OrgProxy.Addresses.AddNew();
			inactiveARAddress.OA_Address1 = "InactiveARAddress";
			inactiveARAddress.OA_Address2 = "333";
			inactiveARAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			inactiveARAddress.OA_IsActive = false;

			var inactiveOfficeAddress = Invoice.Branch.OrgProxy.Addresses.AddNew();
			inactiveOfficeAddress.OA_Address1 = "inactiveOfficeAddress";
			inactiveOfficeAddress.OA_Address2 = "444";
			inactiveOfficeAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			inactiveOfficeAddress.OA_IsActive = false;

			Factory.Save();
			DocARInvoiceCommon invoiceWrapper = GetBaseInvoiceWrapper() as DocARInvoiceCommon;
			AssertEquals("EDI CUSTOMS BROKERS" + System.Environment.NewLine + "TestAddress2" + System.Environment.NewLine + "222" + System.Environment.NewLine + "4010", invoiceWrapper.RecipientNameAddress);

			address = Invoice.Branch.OrgProxy.Addresses[0];
			address.OA_Address1 = "TestAddress1";
			address.OA_Address2 = "111";
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			Factory.Save();
			invoiceWrapper = GetBaseInvoiceWrapper() as DocARInvoiceCommon;
			AssertEquals("EDI CUSTOMS BROKERS" + System.Environment.NewLine + "TestAddress1" + System.Environment.NewLine + "111" + System.Environment.NewLine + "ALBION QLD ", invoiceWrapper.RecipientNameAddress);
		}

		public void TestTaxSummaryLinesHandlesDifferentTaxRateTypes()
		{
			AccTaxRate vat = Factory.NewWithValidTestData<AccTaxRate>();
			vat.AT_Type = AccTaxRate.Types.Rated;
			vat.SetRateNumerator_ForTestOnly(10);

			AccTaxRate cap = Factory.NewWithValidTestData<AccTaxRate>();
			cap.AT_Type = AccTaxRate.Types.CapitalRated;
			cap.SetRateNumerator_ForTestOnly(10);

			AccTaxRate exempt = Factory.NewWithValidTestData<AccTaxRate>();
			exempt.AT_Type = AccTaxRate.Types.Exempt;
			exempt.SetRateNumerator_ForTestOnly(0);

			AccTaxRate notReportable = Factory.NewWithValidTestData<AccTaxRate>();
			notReportable.AT_Type = AccTaxRate.Types.NotReportable;
			notReportable.SetRateNumerator_ForTestOnly(0);

			AccTaxRate suspended = Factory.NewWithValidTestData<AccTaxRate>();
			suspended.AT_Type = AccTaxRate.Types.Suspended;
			suspended.SetRateNumerator_ForTestOnly(0);

			AccTaxRate reverse = Factory.NewWithValidTestData<AccTaxRate>();
			reverse.AT_Type = AccTaxRate.Types.ReverseRated;
			reverse.SetRateNumerator_ForTestOnly(5);

			ARInvoiceLine line1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line1.AL_AT = vat.PK;
			line1.AL_OSExTaxAmount = 30.00M;
			InvoicingBase.Lines.Add(line1);

			ARInvoiceLine line2 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line2.AL_AT = cap.PK;
			line2.AL_OSExTaxAmount = 30.00M;
			InvoicingBase.Lines.Add(line2);

			ARInvoiceLine line3 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line3.AL_AT = exempt.PK;
			line3.AL_OSExTaxAmount = 30M;
			InvoicingBase.Lines.Add(line3);

			ARInvoiceLine line4 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line4.AL_AT = notReportable.PK;
			line4.AL_OSExTaxAmount = 30M;
			InvoicingBase.Lines.Add(line4);

			ARInvoiceLine line5 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line5.AL_AT = suspended.PK;
			line5.AL_OSExTaxAmount = 30.00M;
			InvoicingBase.Lines.Add(line5);

			ARInvoiceLine line6 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line6.AL_AT = reverse.PK;
			line6.AL_OSExTaxAmount = 30.00M;
			InvoicingBase.Lines.Add(line6);

			AssertEquals(6, ARInvoiceWrapper.TaxSummaryLines.Count);
			AssertTaxSummaryLine(ARInvoiceWrapper.TaxSummaryLines[0], "10%", "", "0%", 30m, 30m, 3m, 3m, 33m, 33m, 0m, 0m);
			AssertTaxSummaryLine(ARInvoiceWrapper.TaxSummaryLines[1], "10%", "", "0%", 30m, 30m, 3m, 3m, 33m, 33m, 0m, 0m);
			AssertTaxSummaryLine(ARInvoiceWrapper.TaxSummaryLines[2], "Exempt", "", "0%", 30m, 30m, 0m, 0m, 30m, 30m, 0m, 0m);
			AssertTaxSummaryLine(ARInvoiceWrapper.TaxSummaryLines[3], "N/A", "", "0%", 30m, 30m, 0m, 0m, 30m, 30m, 0m, 0m);
			AssertTaxSummaryLine(ARInvoiceWrapper.TaxSummaryLines[4], "Suspended", "", "0%", 30m, 30m, 0m, 0m, 30m, 30m, 0m, 0m);
			AssertTaxSummaryLine(ARInvoiceWrapper.TaxSummaryLines[5], "Reverse", "", "0%", 30m, 30m, 0m, 0m, 30m, 30m, 0m, 0m);
		}

		public void TestSPVLinesAddedToSummary()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				InvoicingBase.Lines.RemoveAll();

				AccTaxRate vatspv = Factory.NewWithValidTestData<AccTaxRate>();
				vatspv.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRemittedByCustomer;
				vatspv.AT_Type = AccTaxRate.Types.Rated;
				vatspv.SetRateNumerator_ForTestOnly(10);

				ARInvoiceLine line = Factory.NewWithValidTestData<ARInvoiceLine>();
				line.AL_AT = vatspv.PK;
				line.AL_OSExTaxAmount = 30.00M;
				InvoicingBase.Lines.Add(line);

				AssertEquals(2, ARInvoiceWrapper.TaxSummaryLines.Count);
				AssertTaxSummaryLine(ARInvoiceWrapper.TaxSummaryLines[0], "10%", "", "0%", 30m, 30m, 3m, 3m, 30m, 30m, -3m, -3m);
				AssertTaxSummaryLine(ARInvoiceWrapper.TaxSummaryLines[1], "N/A", "", "0%", 0, 0, -3m, -3m, 0, 0, 0m, 0m); // N/A as we don't use the tax rate description
				AssertEquals("SPV", ARInvoiceWrapper.TaxSummaryLines[1].TaxDescriptionWithDescriptionOverride);
			}
		}

		public void TestTaxSummaryLinesDoesMergesRVS()
		{
			AssertTaxSummaryLinesMergeTaxType("RVS", 0, "Reverse", "", true);
		}

		public void TestTaxSummaryLinesDoesMergesSUS()
		{
			AssertTaxSummaryLinesMergeTaxType("SUS", 0, "Suspended", "", true);
		}

		public void TestTaxSummaryLinesDoesMergesNOT()
		{
			AssertTaxSummaryLinesMergeTaxType("NOT", 0, "N/A", "", true);
		}

		public void TestTaxSummaryLinesDoesMergesEXT()
		{
			AssertTaxSummaryLinesMergeTaxType("EXT", 0, "Exempt", "", true);
		}

		public void TestTaxSummaryLinesDoesMergesBST()
		{
			AssertTaxSummaryLinesMergeTaxType("BST", 0, "N/A", "", true);
		}

		public void TestTaxSummaryLinesDoesMergesRAX()
		{
			AssertTaxSummaryLinesMergeTaxType("RAX", 0, "N/A", "", true);
		}

		public void TestTaxSummaryLinesDoesNotMergesRAT()
		{
			AssertTaxSummaryLinesMergeTaxType("RAT", 10, "10%", "CESS", false);
		}

		public void TestTaxSummaryLinesDoesNotMergesCAP()
		{
			AssertTaxSummaryLinesMergeTaxType("CAP", 10, "10%", "CESS", false);
		}

		void AssertTaxSummaryLinesMergeTaxType(ZString taxMainType, ZInt taxMainRate, ZString expectedTaxDescription, ZString expectedExtraTaxDescription, bool expectMerge)
		{
			AccTaxRate taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Type = taxMainType;
			taxRate1.SetRateNumerator_ForTestOnly(taxMainRate);
			taxRate1.AT_ExtraTaxRateType = "EDU";
			taxRate1.SetExtraRate_ForTestOnly(2, 1);

			ARInvoiceLine line1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line1.AL_AT = taxRate1.PK;
			line1.AL_OSExTaxAmount = 30.00M;
			InvoicingBase.Lines.Add(line1);

			AccTaxRate taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Type = taxMainType;
			taxRate2.SetRateNumerator_ForTestOnly(taxMainRate);
			taxRate2.AT_ExtraTaxRateType = "EDU";
			taxRate2.SetExtraRate_ForTestOnly(3, 1);

			ARInvoiceLine line2 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line2.AL_AT = taxRate2.PK;
			line2.AL_OSExTaxAmount = 30.00M;
			InvoicingBase.Lines.Add(line2);

			if (expectMerge)
			{
				AssertEquals(1, ARInvoiceWrapper.TaxSummaryLines.Count);
				AssertTaxSummaryLine(ARInvoiceWrapper.TaxSummaryLines[0], expectedTaxDescription, expectedExtraTaxDescription, "0%", 60m, 60m, 0m, 0m, 60m, 60m, 0m, 0m);
			}
			else
			{
				AssertEquals(2, ARInvoiceWrapper.TaxSummaryLines.Count);
				AssertTaxSummaryLine(ARInvoiceWrapper.TaxSummaryLines[0], expectedTaxDescription, expectedExtraTaxDescription, "2%", 30m, 30m, 3.06m, 3.06m, 33.06m, 33.06m, 0.06m, 0.06m);
				AssertTaxSummaryLine(ARInvoiceWrapper.TaxSummaryLines[1], expectedTaxDescription, expectedExtraTaxDescription, "3%", 30m, 30m, 3.09m, 3.09m, 33.09m, 33.09m, 0.09m, 0.09m);
			}
		}

		void AssertTaxSummaryLine(DocTaxSummaryLine line, ZString expectedTaxDescription, ZString expectedExtraTaxDescription, ZString expectedExtraTaxRate,
				ZDecimal totalExcludeTaxAmountInOSCurrency, ZDecimal totalExcludeTaxAmountInLocalCurrency, ZDecimal totalTaxAmountInOSCurrency, ZDecimal totalTaxAmountInLocalCurrency,
				ZDecimal totalIncludeTaxAmountInOSCurrency, ZDecimal totalIncludeTaxAmountInLocalCurrency, ZDecimal totalExtraTaxAmountInOSCurrency, ZDecimal totalExtraTaxAmountInLocalCurrency)
		{
			AssertEquals(expectedTaxDescription, line.TaxDescription);
			AssertEquals(expectedExtraTaxDescription, line.ExtraTaxDescription);
			AssertEquals(expectedExtraTaxRate, line.ExtraTaxRate);
			AssertEquals(totalExcludeTaxAmountInOSCurrency, line.TotalExcludeTaxAmountInOSCurrency);
			AssertEquals(totalExcludeTaxAmountInLocalCurrency, line.TotalExcludeTaxAmountInLocalCurrency);
			AssertEquals(totalTaxAmountInOSCurrency, line.TotalTaxAmountInOSCurrency);
			AssertEquals(totalTaxAmountInOSCurrency, line.TotalTaxAmountInOSCurrency);
			AssertEquals(totalIncludeTaxAmountInOSCurrency, line.TotalIncludeTaxAmountInOSCurrency);
			AssertEquals(totalIncludeTaxAmountInOSCurrency, line.TotalIncludeTaxAmountInOSCurrency);
			AssertEquals(totalExtraTaxAmountInOSCurrency, line.TotalExtraTaxAmountInOSCurrency);
			AssertEquals(totalExtraTaxAmountInOSCurrency, line.TotalExtraTaxAmountInOSCurrency);
		}

		public void TestTotalSummaryByTaxDoesNotRecalculateMainTax()
		{
			decimal invoiceAmount = 1275.99M;
			AccTaxRate vat1250 = Factory.NewWithValidTestData<AccTaxRate>();
			vat1250.AT_Type = AccTaxRate.Types.Rated;
			vat1250.SetRate_ForTestOnly(125, 10);

			ARInvoiceLine line1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line1.AL_AT = vat1250.PK;
			line1.AL_OSExTaxAmount = invoiceAmount;
			InvoicingBase.Lines.Add(line1);

			ARInvoiceLine line2 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line2.AL_AT = vat1250.PK;
			line2.AL_OSExTaxAmount = invoiceAmount;
			InvoicingBase.Lines.Add(line2);

			ARInvoiceLine line3 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line3.AL_AT = vat1250.PK;
			line3.AL_OSExTaxAmount = invoiceAmount;
			InvoicingBase.Lines.Add(line3);

			ARInvoiceLine line4 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line4.AL_AT = vat1250.PK;
			line4.AL_OSExTaxAmount = invoiceAmount;
			InvoicingBase.Lines.Add(line4);

			ARInvoiceLine line5 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line5.AL_AT = vat1250.PK;
			line5.AL_OSExTaxAmount = invoiceAmount;
			InvoicingBase.Lines.Add(line5);

			AssertEquals("Total summary by tax rate", @" 6,379.95@12.50%=797.50", ARInvoiceWrapper.TotalSummaryByTaxRate);

			AssertEquals("Total summary by tax rate", @" 6,379.95@12.50%=797.50", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestTotalSummaryByTaxRateEXL()
		{
			var excludedTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			excludedTaxRate.AT_Type = AccTaxRate.Types.ExcludedFromTheTaxBase;

			var line1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line1.AL_AT = excludedTaxRate.PK;
			line1.AL_OSExTaxAmount = 100.00M;
			InvoicingBase.Lines.Add(line1);

			AssertEquals("Total summary by tax rate", " 100.00 Excluded", ARInvoiceWrapper.TotalSummaryByTaxRate);
			AssertEquals("Total summary by tax rate", " 100.00 Excluded", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.ExcludedFromTheTaxBase, "Override Excluded");
			AssertEquals("Override Excluded", " 100.00 Override Excluded", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestTotalSummaryByTaxRateINP7()
		{
			AccTaxRate inp7 = Factory.NewWithValidTestData<AccTaxRate>();
			inp7.AT_Type = AccTaxRate.Types.Rated;
			inp7.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATClaimed;
			inp7.SetRateNumerator_ForTestOnly(0);
			inp7.SetExtraRate_ForTestOnly(7, 1);

			AccTaxRate vat6 = Factory.NewWithValidTestData<AccTaxRate>();
			vat6.AT_Type = AccTaxRate.Types.Rated;
			vat6.SetRateNumerator_ForTestOnly(6);

			AccTaxRate zeroRated = Factory.NewWithValidTestData<AccTaxRate>();
			zeroRated.AT_Type = AccTaxRate.Types.Rated;
			zeroRated.SetRateNumerator_ForTestOnly(0);

			ARInvoiceLine line1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line1.AL_AT = inp7.PK;
			line1.AL_OSExTaxAmount = 100.00M;
			InvoicingBase.Lines.Add(line1);

			ARInvoiceLine line2 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line2.AL_AT = inp7.PK;
			line2.AL_OSExTaxAmount = 100.00M;
			InvoicingBase.Lines.Add(line2);

			ARInvoiceLine line3 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line3.AL_AT = vat6.PK;
			line3.AL_OSExTaxAmount = 200.00M;
			InvoicingBase.Lines.Add(line3);

			ARInvoiceLine line4 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line4.AL_AT = vat6.PK;
			line4.AL_OSExTaxAmount = 200.00M;
			InvoicingBase.Lines.Add(line4);

			ARInvoiceLine line5 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line5.AL_AT = zeroRated.PK;
			line5.AL_OSExTaxAmount = 200.00M;
			InvoicingBase.Lines.Add(line5);

			AssertEquals("Total summary by tax rate", @" 200.00@0.00%=0.00 and 215.14@7.00%=15.06, 400.00@6.00%=24.00
 200.00 Zero Rated", ARInvoiceWrapper.TotalSummaryByTaxRate);

			AssertEquals("Total summary by tax rate", @" 200.00@0.00%=0.00 and 215.14@7.00%=15.06, 400.00@6.00%=24.00
 200.00 Zero Rated", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, "Override Zero Rated");
			AssertEquals("Total summary by tax rate", @" 200.00@0.00%=0.00 and 215.14@7.00%=15.06, 400.00@6.00%=24.00
 200.00 Override Zero Rated", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestTotalSummaryByTaxRateBST()
		{
			AccTaxRate bST = Factory.NewWithValidTestData<AccTaxRate>();
			bST.AT_Type = AccTaxRate.Types.ReportableUnderBusinessTax;
			bST.SetRateNumerator_ForTestOnly(0);

			ARInvoiceLine line1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line1.AL_AT = bST.PK;
			line1.AL_OSExTaxAmount = 100.00M;
			InvoicingBase.Lines.Add(line1);

			AssertEquals("Total summary by tax rate", @" 100.00 Reportable under Business Tax", ARInvoiceWrapper.TotalSummaryByTaxRate);
			AssertEquals("Total summary by tax rate", @" 100.00 Reportable Under Business Tax", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.ReportableUnderBusinessTax, "Override Reportable under Business Tax");
			AssertEquals("Override Reportable", @" 100.00 Override Reportable under Business Tax", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestTotalSummaryByTaxRateGSTANDQST()
		{
			AccTaxRate gstAndQst = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndQst.AT_Type = AccTaxRate.Types.Rated;
			gstAndQst.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			gstAndQst.SetRateNumerator_ForTestOnly(5);
			gstAndQst.SetExtraRate_ForTestOnly(75, 10);

			AccTaxRate gst5 = Factory.NewWithValidTestData<AccTaxRate>();
			gst5.AT_Type = AccTaxRate.Types.Rated;
			gst5.SetRateNumerator_ForTestOnly(5);

			AccTaxRate gst10 = Factory.NewWithValidTestData<AccTaxRate>();
			gst10.AT_Type = AccTaxRate.Types.Rated;
			gst10.SetRateNumerator_ForTestOnly(10);

			ARInvoiceLine line1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line1.AL_AT = gstAndQst.PK;
			line1.AL_OSExTaxAmount = 150.00M;
			line1.AL_Sequence = 2;
			InvoicingBase.Lines.Add(line1);

			ARInvoiceLine line2 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line2.AL_AT = gstAndQst.PK;
			line2.AL_OSExTaxAmount = 150.00M;
			line1.AL_Sequence = 3;
			InvoicingBase.Lines.Add(line2);

			ARInvoiceLine line3 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line3.AL_AT = gst10.PK;
			line3.AL_OSExTaxAmount = 100.00M;
			line3.AL_Sequence = 1;
			InvoicingBase.Lines.Add(line3);

			ARInvoiceLine line4 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line4.AL_AT = gst5.PK;
			line4.AL_OSExTaxAmount = 100.00M;
			line4.AL_Sequence = 4;
			InvoicingBase.Lines.Add(line4);

			ARInvoiceLine line5 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line5.AL_AT = ZGuid.Empty;
			line5.AL_OSExTaxAmount = 100.00M;
			line5.AL_Sequence = 5;
			InvoicingBase.Lines.Add(line5);

			AssertEquals("Total summary by tax rate", " 300.00@5.00%=15.00 and 315.00@7.50%=23.62, 100.00@10.00%=10.00, 100.00@5.00%=5.00", ARInvoiceWrapper.TotalSummaryByTaxRate);

			AssertEquals("Total summary by tax rate", " 300.00@5.00%=15.00 and 315.00@7.50%=23.62, 100.00@10.00%=10.00, 100.00@5.00%=5.00", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestTotalSummaryByTaxRateGSTANDQST_WithDoubleExtraRate()
		{
			AccTaxRate gstAndQst = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndQst.AT_Type = AccTaxRate.Types.Rated;
			gstAndQst.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			gstAndQst.SetRateNumerator_ForTestOnly(5);
			gstAndQst.SetExtraRate_ForTestOnly(75, 10);

			AccTaxRate gstAndQst775 = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndQst775.AT_Type = AccTaxRate.Types.Rated;
			gstAndQst775.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			gstAndQst775.SetRateNumerator_ForTestOnly(5);
			gstAndQst775.SetExtraRate_ForTestOnly(775, 100);

			ARInvoiceLine line1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line1.AL_AT = gstAndQst.PK;
			line1.AL_OSExTaxAmount = 150.00M;
			InvoicingBase.Lines.Add(line1);

			ARInvoiceLine line2 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line2.AL_AT = gstAndQst.PK;
			line2.AL_OSExTaxAmount = 150.00M;
			InvoicingBase.Lines.Add(line2);

			ARInvoiceLine line3 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line3.AL_AT = gstAndQst775.PK;
			line3.AL_OSExTaxAmount = 100.00M;
			InvoicingBase.Lines.Add(line3);

			ARInvoiceLine line4 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line4.AL_AT = gstAndQst775.PK;
			line4.AL_OSExTaxAmount = 100.00M;
			InvoicingBase.Lines.Add(line4);

			AssertEquals("Total summary by tax rate", " 500.00@5.00%=25.00 and 315.00@7.50%=23.62 and 210.00@7.75%=16.28", ARInvoiceWrapper.TotalSummaryByTaxRate);

			AssertEquals("Total summary by tax rate", " 500.00@5.00%=25.00 and 315.00@7.50%=23.62 and 210.00@7.75%=16.28", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestTotalSummaryByTaxRateSERANDEDU()
		{
			AccTaxRate serAndEdu = Factory.NewWithValidTestData<AccTaxRate>();
			serAndEdu.AT_Type = AccTaxRate.Types.Rated;
			serAndEdu.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
			serAndEdu.SetRateNumerator_ForTestOnly(5);
			serAndEdu.SetExtraRate_ForTestOnly(75, 10);

			AccTaxRate ser5 = Factory.NewWithValidTestData<AccTaxRate>();
			ser5.AT_Type = AccTaxRate.Types.Rated;
			ser5.SetRateNumerator_ForTestOnly(5);

			AccTaxRate ser10 = Factory.NewWithValidTestData<AccTaxRate>();
			ser10.AT_Type = AccTaxRate.Types.Rated;
			ser10.SetRateNumerator_ForTestOnly(10);

			ARInvoiceLine line1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line1.AL_AT = serAndEdu.PK;
			line1.AL_OSExTaxAmount = 150.00M;
			line1.AL_Sequence = 2;
			InvoicingBase.Lines.Add(line1);

			ARInvoiceLine line2 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line2.AL_AT = serAndEdu.PK;
			line2.AL_OSExTaxAmount = 150.00M;
			line1.AL_Sequence = 3;
			InvoicingBase.Lines.Add(line2);

			ARInvoiceLine line3 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line3.AL_AT = ser10.PK;
			line3.AL_OSExTaxAmount = 100.00M;
			line3.AL_Sequence = 1;
			InvoicingBase.Lines.Add(line3);

			ARInvoiceLine line4 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line4.AL_AT = ser5.PK;
			line4.AL_OSExTaxAmount = 100.00M;
			line4.AL_Sequence = 4;
			InvoicingBase.Lines.Add(line4);

			ARInvoiceLine line5 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line5.AL_AT = ZGuid.Empty;
			line5.AL_OSExTaxAmount = 100.00M;
			line5.AL_Sequence = 5;
			InvoicingBase.Lines.Add(line5);

			AssertEquals("Total summary by tax rate", " 300.00@5.00%=15.00 and 15.00@7.50%=1.12, 100.00@10.00%=10.00, 100.00@5.00%=5.00", ARInvoiceWrapper.TotalSummaryByTaxRate);

			AssertEquals("Total summary by tax rate", " 300.00@5.00%=15.00 and 15.00@7.50%=1.12, 100.00@10.00%=10.00, 100.00@5.00%=5.00", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestTotalSummaryByTaxRateGSTANDQST_QCTType()
		{
			AccTaxRate gstAndQst = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndQst.AT_Type = AccTaxRate.Types.Rated;
			gstAndQst.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			gstAndQst.SetRateNumerator_ForTestOnly(5);
			gstAndQst.SetExtraRate_ForTestOnly(9975, 1000);

			ARInvoiceLine line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AT = gstAndQst.PK;
			line.AL_OSExTaxAmount = 150.00M;
			InvoicingBase.Lines.Add(line);

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Total summary by tax rate", " 150.00@5.00%=7.50 and 150.00@9.975%=14.96", aRInvoiceWrapper.TotalSummaryByTaxRate);

			AssertEquals("Total summary by tax rate", " 150.00@5.00%=7.50 and 150.00@9.975%=14.96", aRInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestTotalSummaryByTaxRateVAT()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			line1.TaxRate.AT_Type = AccTaxRate.Types.Rated;
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			line2.TaxRate.AT_Type = AccTaxRate.Types.Rated;

			AssertEquals("Total summary by tax rate", " 350.00@10.11%=35.39", ARInvoiceWrapper.TotalSummaryByTaxRate);

			AssertEquals("Total summary by tax rate", " 350.00@10.11%=35.39", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestTotalSummaryByTaxRateRET()
		{
			AccTaxRate ret = Factory.NewWithValidTestData<AccTaxRate>();
			ret.AT_Type = AccTaxRate.Types.Rated;
			ret.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
			ret.SetRateNumerator_ForTestOnly(16);
			ret.SetExtraRate_ForTestOnly(4, 1);

			ARInvoiceLine line1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line1.AL_AT = ret.PK;
			line1.AL_OSExTaxAmount = 100M;
			InvoicingBase.Lines.Add(line1);

			ARInvoiceLine line2 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line2.AL_AT = ret.PK;
			line2.AL_OSExTaxAmount = 100M;
			InvoicingBase.Lines.Add(line2);

			AssertEquals("Total summary by tax rate", " 200.00@16.00%=32.00 minus 200.00@4.00%=8.00", ARInvoiceWrapper.TotalSummaryByTaxRate);

			AssertEquals("Total summary by tax rate", " 200.00@16.00%=32.00 minus 200.00@4.00%=8.00", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestTotalSummaryByTaxRateREF()
		{
			AccTaxRate ret = Factory.NewWithValidTestData<AccTaxRate>();
			ret.AT_Type = AccTaxRate.Types.Rated;
			ret.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
			ret.SetRateNumerator_ForTestOnly(16);
			ret.SetExtraRate_ForTestOnly(1, 4);

			ARInvoiceLine line1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line1.AL_AT = ret.PK;
			line1.AL_OSExTaxAmount = 100M;
			InvoicingBase.Lines.Add(line1);

			ARInvoiceLine line2 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line2.AL_AT = ret.PK;
			line2.AL_OSExTaxAmount = 100M;
			InvoicingBase.Lines.Add(line2);

			AssertEquals("Total summary by tax rate", " 200.00@16.00%=32.00 minus 200.00@4.00%=8.00", ARInvoiceWrapper.TotalSummaryByTaxRate);

			AssertEquals("Total summary by tax rate", " 200.00@16.00%=32.00 minus 200.00@4.00%=8.00", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestRollupByChargeCodeWhenTotalsAddToZero()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingBase.Header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			var group = InvoicingBase.Header.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			group.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CCD;
			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;

			InvoicingBase.Lines.RemoveAndDeleteAll();

			var chargeCode2 = TestObjectCreator.CC2;
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_AC = chargeCode2.PK;
			line1.AL_AT = TestObjectCreator.GST1.PK;
			line1.AL_OSExTaxAmount = 150.00M;
			line1.AL_OSExTaxAmount = 15.00M;

			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_AC = chargeCode2.PK;
			line2.AL_AT = TestObjectCreator.GST1.PK;
			line2.AL_OSExTaxAmount = -150.00M;
			line2.AL_OSExTaxAmount = -15.00M;

			Factory.Save();

			AssertEquals("Lines for Invoice.Count", 1, ARInvoiceWrapper.LinesForInvoice.Count);
			AssertEquals("Line.OSTaxDisplay", 1, ARInvoiceWrapper.LinesForInvoice.Count);

			AssertEquals("Total summary by tax rate", " 0.00@10.00%=0.00", ARInvoiceWrapper.TotalSummaryByTaxRate);

			AssertEquals("Total summary by tax rate", " 0.00@10.00%=0.00", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestTotalSummaryByTaxRateNoVAT()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			line3.TaxRate.AT_Type = AccTaxRate.Types.Exempt;
			line3.AL_Sequence = 1;
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			line4.TaxRate.AT_Type = AccTaxRate.Types.NotReportable;
			line4.AL_Sequence = 2;
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			line5.TaxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			line5.AL_Sequence = 4;
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			line6.TaxRate.AT_Type = AccTaxRate.Types.Suspended;
			line6.AL_Sequence = 3;

			AssertEquals("Total summary by tax rate", " 250.00 Exempt, 300.00 Not Applicable, 400.00 Suspended, 350.00 Tax Shifted", ARInvoiceWrapper.TotalSummaryByTaxRate);
			AssertEquals("Total summary by tax rate", " 250.00 Exempt, 300.00 Not Applicable, 400.00 Suspended, 350.00 Reverse Charge", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Exempt, "Override Exempt");
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.NotReportable, "Override Not Applicable");
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Suspended, "Override Suspended");
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.ReverseRated, "Override Reverse");
			AssertEquals("Total summary by tax rate", " 250.00 Override Exempt, 300.00 Override Not Applicable, 400.00 Override Suspended, 350.00 Override Reverse", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestTotalSummaryByTaxRateVATandNoVAT()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			line1.TaxRate.AT_Type = AccTaxRate.Types.Rated;
			line1.AL_Sequence = 1;
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			line2.TaxRate.AT_Type = AccTaxRate.Types.Rated;
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			line3.TaxRate.AT_Type = AccTaxRate.Types.Exempt;
			line1.AL_Sequence = 2;
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			line4.TaxRate.AT_Type = AccTaxRate.Types.NotReportable;
			line4.AL_Sequence = 3;
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			line5.TaxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			line5.AL_Sequence = 5;
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			line6.TaxRate.AT_Type = AccTaxRate.Types.Suspended;
			line6.AL_Sequence = 4;

			AssertEquals("Total summary by tax rate", @" 350.00@10.11%=35.39
 250.00 Exempt, 300.00 Not Applicable, 400.00 Suspended, 350.00 Tax Shifted", ARInvoiceWrapper.TotalSummaryByTaxRate);
			AssertEquals("Total summary by tax rate", @" 350.00@10.11%=35.39
 250.00 Exempt, 300.00 Not Applicable, 400.00 Suspended, 350.00 Reverse Charge", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Exempt, "Override Exempt");
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.NotReportable, "Override Not Applicable");
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Suspended, "Override Suspended");
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.ReverseRated, "Override Reverse");
			AssertEquals("Total summary by tax rate", @" 350.00@10.11%=35.39
 250.00 Override Exempt, 300.00 Override Not Applicable, 400.00 Override Suspended, 350.00 Override Reverse", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestTotalSummaryByTaxRateRAX()
		{
			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			line1.TaxRate.AT_Type = AccTaxRate.Types.RatedInAnotherCountry;
			AssertEquals("Total summary by tax rate", " 150.00@10.11%=15.17 VAT of another country/region", ARInvoiceWrapper.TotalSummaryByTaxRate);
		}

		public void TestTotalSummaryByTaxRateOnARCreditNote()
		{
			SetUpCreditNoteWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			line1.TaxRate.AT_Type = AccTaxRate.Types.Rated;
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			line2.TaxRate.AT_Type = AccTaxRate.Types.Rated;

			AssertEquals("Pre-condition: RegistryItem is False by default", false, AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.Value);
			AssertEquals("Total summary by tax rate", " 350.00@10.11%=35.39", ARInvoiceWrapper.TotalSummaryByTaxRate);
			AssertEquals("Total summary by tax rate", " 350.00@10.11%=35.39", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);

			AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Total summary by tax rate", " -350.00@10.11%=-35.39", ARInvoiceWrapper.TotalSummaryByTaxRate);
			AssertEquals("Total summary by tax rate", " -350.00@10.11%=-35.39", ARInvoiceWrapper.TotalSummaryByTaxRateWithDescriptionOverride);
		}

		public void TestTotalSummaryByTaxRateCoreDoesNotThrowException()
		{
			var types = typeof(AccTaxRate.Types).GetFields(BindingFlags.Public | BindingFlags.Static)
					.Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
					.Select(fieldInfo => (string)fieldInfo.GetValue(null));
			foreach (var type in types)
			{
				if (type != AccTaxRate.Types.ServiceTax) //SER tax type is only applicable for India; Should not check for SER in this generalized test
				{
					InvoicingLineBase line = AddOriginChargeToInvoice();
					line.TaxRate.AT_Type = type;
				}
			}
			try
			{
				var c = ARInvoiceWrapper.TotalSummaryByTaxRate;
			}
			catch (NotSupportedException e)
			{
				Fail("Exception should not be thrown, ensure that every tax type in AccTaxRate.Types is handled in TotalSummaryByTaxRateCore \nException: " + e.Message);
			}
			Assert(true);
		}

		public void TestGetInvoiceRollupOrGroupWithHeadlessConsol()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			consol.JK_UniqueConsignRef = "c1";

			invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef + "/A";

			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			JobHeader header = orgFactory.NewJobWithValidTestDataForTesting<JobHeader>();
			OrgHeader client = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollupAndSequence;
			group.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.OandF;
			orgFactory.Save();

			header.LocalChargesPK = client.PK;
			invoice.AH_OH = client.PK;

			DocARInvoiceCommonWrapperForTest wrapper = new DocARInvoiceCommonWrapperForTest(invoice, Factory);

			AssertEquals(group.PG_GroupOrSubTotal, wrapper.GroupOrSubtotal);
			AssertEquals(group.PG_GroupOrSubtotalStyle, wrapper.GroupOrSubtotalStyle);
		}

		public void TestGetInvoiceRollupOrGroupForMiscInvoices()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();

			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			JobHeader header = orgFactory.NewJobWithValidTestDataForTesting<JobHeader>();
			OrgHeader client = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code;
			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollupAndSequence;
			group.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.OandF;
			orgFactory.Save();

			header.LocalChargesPK = client.PK;
			invoice.AH_OH = client.PK;

			DocARInvoiceCommonWrapperForTest wrapper = new DocARInvoiceCommonWrapperForTest(invoice, Factory);

			AssertEquals(group.PG_GroupOrSubTotal, wrapper.GroupOrSubtotal);
			AssertEquals(group.PG_GroupOrSubtotalStyle, wrapper.GroupOrSubtotalStyle);
		}

		public void TestOSTaxDisplayHeadingTranslatesVATBasedOnCountryCode()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				SetLineDetails((InvoicingLineBase)InvoicingBase.Lines.AddNew(), TestObjectCreator.CC1, TestObjectCreator.GST1, 10m, 1m);
				DocARInvoice invoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);

				List<string> canada = new List<string> { Core.Constants.CountryCodes.Canada };
				AssertOSTaxDisplayHeadingTranslatedCorrectly(canada, Core.Constants.Languages.EnglishAmerican, "GST", invoiceWrapper);
				AssertOSTaxDisplayHeadingTranslatedCorrectly(canada, Core.Constants.Languages.French, "TPS", invoiceWrapper);
				AssertOSTaxDisplayHeadingTranslatedCorrectly(canada, Core.Constants.Languages.Mongolian, "GST", invoiceWrapper);

				List<string> norway = new List<string> { Core.Constants.CountryCodes.Norway };
				AssertOSTaxDisplayHeadingTranslatedCorrectly(norway, Core.Constants.Languages.EnglishAmerican, "VAT", invoiceWrapper);
				AssertOSTaxDisplayHeadingTranslatedCorrectly(norway, Core.Constants.Languages.Mongolian, "VAT", invoiceWrapper);

				List<string> malaysia = new List<string> { Core.Constants.CountryCodes.Malaysia };
				InvoicingBase.Lines[0].TaxRate.AT_ExtraTaxRateType = "SER";
				AssertOSTaxDisplayHeadingTranslatedCorrectly(malaysia, Core.Constants.Languages.EnglishAmerican, "SERVICE TAX", invoiceWrapper);
				AssertOSTaxDisplayHeadingTranslatedCorrectly(malaysia, Core.Constants.Languages.Mongolian, "SERVICE TAX", invoiceWrapper);
				InvoicingBase.Lines[0].TaxRate.AT_ExtraTaxRateType = "";

				AssertOSTaxDisplayHeadingTranslatedCorrectly(EUCountryCodes, Core.Constants.Languages.EnglishAmerican, "VAT", invoiceWrapper);
				AssertOSTaxDisplayHeadingTranslatedCorrectly(EUCountryCodes, Core.Constants.Languages.German, "MwSt", invoiceWrapper);
				AssertOSTaxDisplayHeadingTranslatedCorrectly(EUCountryCodes, Core.Constants.Languages.French, "TVA", invoiceWrapper);
				AssertOSTaxDisplayHeadingTranslatedCorrectly(EUCountryCodes, Core.Constants.Languages.Spanish, "IVA", invoiceWrapper);
				AssertOSTaxDisplayHeadingTranslatedCorrectly(EUCountryCodes, Core.Constants.Languages.Italian, "IVA", invoiceWrapper);
				AssertOSTaxDisplayHeadingTranslatedCorrectly(EUCountryCodes, Core.Constants.Languages.Dutch, "BTW", invoiceWrapper);
				AssertOSTaxDisplayHeadingTranslatedCorrectly(EUCountryCodes, Core.Constants.Languages.Mongolian, "VAT", invoiceWrapper);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		void AssertOSTaxDisplayHeadingTranslatedCorrectly(List<string> countryCodes, string language, string expected, DocARInvoice invoiceWrapper)
		{
			using (Res.TemporarilySwitchLanguage(language))
			{
				foreach (var countryCode in countryCodes)
				{
					GlbCompany.CurrentCompany.SetCountry(countryCode);
					AssertEquals(string.Format("OSTaxDisplayHeading translated to an unexpected value for {0}", language), expected, invoiceWrapper.OSTaxDisplayHeading);
				}
			}
		}

		public void TestOSPrimaryTaxDisplayHeadingTranslatesVATBasedOnCountryCode()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				SetLineDetails((InvoicingLineBase)InvoicingBase.Lines.AddNew(), TestObjectCreator.CC1, TestObjectCreator.GST1, 10m, 1m);
				DocARInvoice invoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);

				List<string> canada = new List<string> { Core.Constants.CountryCodes.Canada };
				AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(canada, Core.Constants.Languages.EnglishAmerican, "GST", invoiceWrapper);
				AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(canada, Core.Constants.Languages.French, "TPS", invoiceWrapper);
				AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(canada, Core.Constants.Languages.Mongolian, "GST", invoiceWrapper);

				List<string> norway = new List<string> { Core.Constants.CountryCodes.Norway };
				AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(norway, Core.Constants.Languages.EnglishAmerican, "VAT", invoiceWrapper);
				AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(norway, Core.Constants.Languages.Mongolian, "VAT", invoiceWrapper);

				List<string> malaysia = new List<string> { Core.Constants.CountryCodes.Malaysia };
				InvoicingBase.Lines[0].TaxRate.AT_ExtraTaxRateType = "SER";
				AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(malaysia, Core.Constants.Languages.EnglishAmerican, "SERVICE TAX", invoiceWrapper);
				AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(malaysia, Core.Constants.Languages.Mongolian, "SERVICE TAX", invoiceWrapper);
				InvoicingBase.Lines[0].TaxRate.AT_ExtraTaxRateType = "";

				AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(EUCountryCodes, Core.Constants.Languages.EnglishAmerican, "VAT", invoiceWrapper);
				AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(EUCountryCodes, Core.Constants.Languages.German, "MwSt", invoiceWrapper);
				AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(EUCountryCodes, Core.Constants.Languages.French, "TVA", invoiceWrapper);
				AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(EUCountryCodes, Core.Constants.Languages.Spanish, "IVA", invoiceWrapper);
				AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(EUCountryCodes, Core.Constants.Languages.Italian, "IVA", invoiceWrapper);
				AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(EUCountryCodes, Core.Constants.Languages.Dutch, "BTW", invoiceWrapper);
				AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(EUCountryCodes, Core.Constants.Languages.Mongolian, "VAT", invoiceWrapper);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		void AssertOSPrimaryTaxDisplayHeadingTranslatedCorrectly(List<string> countryCodes, string language, string expected, DocARInvoice invoiceWrapper)
		{
			using (Res.TemporarilySwitchLanguage(language))
			{
				foreach (var countryCode in countryCodes)
				{
					GlbCompany.CurrentCompany.SetCountry(countryCode);
					AssertEquals(string.Format("OSPrimaryTaxDisplayHeading translated to an unexpected value for {0}", language), expected, invoiceWrapper.OSPrimaryTaxDisplayHeading);
				}
			}
		}

		public void TestOSPrimaryTaxDisplayHeadingForMalaysia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malaysia))
			{
				var header = CreateOrgHeaderForMalaysia();
				Invoice.AH_OH = header.PK;

				var taxRate1 = Factory.New<AccTaxRate>();
				var taxRate2 = Factory.New<AccTaxRate>();

				InvoicingBase.Lines.AddNew().AL_AT = taxRate1.PK;
				InvoicingBase.Lines.AddNew().AL_AT = taxRate2.PK;

				var invoiceWrapper = DocARInvoice.New(Invoice, Factory);

				taxRate1.AT_Code = "GST";
				taxRate1.AT_ExtraTaxRateType = ZString.Empty;
				taxRate2.AT_Code = "GST";
				taxRate2.AT_ExtraTaxRateType = ZString.Empty;
				AssertEquals("GST", invoiceWrapper.OSPrimaryTaxDisplayHeading);

				taxRate1.AT_Code = "GST";
				taxRate1.AT_ExtraTaxRateType = ZString.Empty;
				taxRate2.AT_Code = "SVC";
				taxRate2.AT_ExtraTaxRateType = "SER";
				AssertEquals("GST", invoiceWrapper.OSPrimaryTaxDisplayHeading);

				taxRate1.AT_Code = "SVC";
				taxRate1.AT_ExtraTaxRateType = "SER";
				taxRate2.AT_Code = "SVC";
				taxRate2.AT_ExtraTaxRateType = "SER";
				AssertEquals("SERVICE TAX", invoiceWrapper.OSPrimaryTaxDisplayHeading);

				taxRate1.AT_Code = "XXX";
				taxRate1.AT_ExtraTaxRateType = "TST";
				taxRate2.AT_Code = "XXX";
				taxRate2.AT_ExtraTaxRateType = "TST";
				AssertEquals("SERVICE TAX", invoiceWrapper.OSPrimaryTaxDisplayHeading);
			}
		}

		public void TestDocument_ITAutofattura()
		{
			AccInvMsg msg1 = Factory.NewWithValidTestData<AccInvMsg>();
			msg1.A9_IsShownOnDocuments = true;
			msg1.A9_Description = "IVA 22";
			msg1.A9_EnglishMsg = "IVA 22";
			msg1.A9_LocalMsg = string.Empty;

			AccInvMsg msg2 = Factory.NewWithValidTestData<AccInvMsg>();
			msg2.A9_IsShownOnDocuments = true;
			msg2.A9_Description = "ARTICOLO 9";
			msg2.A9_EnglishMsg = "ARTICOLO 9";
			msg2.A9_LocalMsg = string.Empty;

			AccInvMsg msg3 = Factory.NewWithValidTestData<AccInvMsg>();
			msg3.A9_IsShownOnDocuments = true;
			msg3.A9_Description = "IVA 22 REV CHARGE";
			msg3.A9_EnglishMsg = "IVA 22 REV CHARGE";
			msg3.A9_LocalMsg = string.Empty;

			var invoice = Factory.New<APInvoice>();

			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_RL_NKClosestPort = "ITROM";
			invoice.AH_OH = header.PK;
			invoice.AH_TransactionNum = "00001";

			Factory.Save();

			AccTaxRate rated22 = Factory.NewWithValidTestData<AccTaxRate>();
			rated22.AT_Type = AccTaxRate.Types.Rated;
			rated22.AT_A9_DefaultVatClass = msg1.PK;
			rated22.SetRateNumerator_ForTestOnly(22);

			AccTaxRate reverseRated0 = Factory.NewWithValidTestData<AccTaxRate>();
			reverseRated0.AT_Type = AccTaxRate.Types.ReverseRated;
			reverseRated0.AT_A9_DefaultVatClass = msg2.PK;
			reverseRated0.SetRateNumerator_ForTestOnly(0);

			AccTaxRate reverseRated22 = Factory.NewWithValidTestData<AccTaxRate>();
			reverseRated22.AT_Type = AccTaxRate.Types.ReverseRated;
			reverseRated22.AT_A9_DefaultVatClass = msg3.PK;
			reverseRated22.SetRateNumerator_ForTestOnly(22);

			var line0 = (InvoicingLineBase)invoice.Lines.AddNew();
			line0.AL_AT = rated22.PK;
			line0.AL_OSExTaxAmount = 25.00;
			line0.AL_OSTaxAmount = 5.50;

			var line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			line1.AL_AT = reverseRated22.PK;
			line1.AL_OSExTaxAmount = 50.00;
			line1.AL_OSTaxAmount = 0.00;

			var line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			line2.AL_AT = rated22.PK;
			line2.AL_OSExTaxAmount = 47.00;
			line2.AL_OSTaxAmount = 10.34;

			var line3 = (InvoicingLineBase)invoice.Lines.AddNew();
			line3.AL_AT = reverseRated0.PK;
			line3.AL_OSExTaxAmount = 74.00;
			line3.AL_OSTaxAmount = 0.00;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var invoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(invoice, Factory);

			AssertEquals("OSDocumentTitleForITAutofattura is Autofattura", "Autofattura", invoiceWrapper.OSDocumentTitleForITAutofattura);

			var line0_wrapped = ((DocARInvoiceLine)invoiceWrapper.LinesForInvoice[0]);
			AssertEquals("Display tax for line 0", "22%=5.50", line0_wrapped.GetOSTaxAmountDisplay());
			AssertEquals("Raw tax for line 0", new ZDecimal(5.50), line0_wrapped.OSTaxAmount_Raw);

			var line1_wrapped = ((DocARInvoiceLine)invoiceWrapper.LinesForInvoice[1]);
			AssertEquals("Display tax for line 1", "Reverse Charge", line1_wrapped.GetOSTaxAmountDisplay());
			AssertEquals("Raw tax for line 1", new ZDecimal(11.00), line1_wrapped.OSTaxAmount_Raw);

			var line2_wrapped = ((DocARInvoiceLine)invoiceWrapper.LinesForInvoice[2]);
			AssertEquals("Display tax for line 2", "22%=10.34", line2_wrapped.GetOSTaxAmountDisplay());
			AssertEquals("Raw tax for line 2", new ZDecimal(10.34), line2_wrapped.OSTaxAmount_Raw);

			var line3_wrapped = ((DocARInvoiceLine)invoiceWrapper.LinesForInvoice[3]);
			AssertEquals("Display tax for line 3", "Reverse Charge", line3_wrapped.GetOSTaxAmountDisplay());
			AssertEquals("Raw tax for line 3", new ZDecimal(0.00), line3_wrapped.OSTaxAmount_Raw);

			AssertEquals("TaxRateByRateAndMessageColumnRaw", @"0%
22%
22%
", invoiceWrapper.TaxRateByRateAndMessageColumnRaw);

			AssertEquals("TotalTaxByRateAndMessageColumnRaw", @"0.00
15.84
11.00
", invoiceWrapper.TotalTaxByRateAndMessageColumnRaw);

			AssertEquals("TotalOSTaxAmountRawFormatted", "26.84", invoiceWrapper.TotalOSTaxAmountRawFormatted);
			AssertEquals("OSTotalRawFormatted", "222.84", invoiceWrapper.OSTotalRawFormatted);

			var creditNote = Factory.New<APCreditNote>();
			creditNote.AH_OH = header.PK;
			creditNote.AH_TransactionNum = "00002";

			var invoiceWrapper2 = (DocARInvoiceCommon)DocARInvoice.New(creditNote, Factory);

			AssertEquals("OSDocumentTitleForITAutofattura is Autoaccredito", "Autoaccredito", invoiceWrapper2.OSDocumentTitleForITAutofattura);
		}

		public void TestDocument_ITAutofattura_LocalAmounts()
		{
			AccInvMsg msg1 = Factory.NewWithValidTestData<AccInvMsg>();
			msg1.A9_IsShownOnDocuments = true;
			msg1.A9_Description = "IVA 22";
			msg1.A9_EnglishMsg = "IVA 22";
			msg1.A9_LocalMsg = string.Empty;

			AccInvMsg msg2 = Factory.NewWithValidTestData<AccInvMsg>();
			msg2.A9_IsShownOnDocuments = true;
			msg2.A9_Description = "ARTICOLO 9";
			msg2.A9_EnglishMsg = "ARTICOLO 9";
			msg2.A9_LocalMsg = string.Empty;

			AccInvMsg msg3 = Factory.NewWithValidTestData<AccInvMsg>();
			msg3.A9_IsShownOnDocuments = true;
			msg3.A9_Description = "IVA 22 REV CHARGE";
			msg3.A9_EnglishMsg = "IVA 22 REV CHARGE";
			msg3.A9_LocalMsg = string.Empty;

			var invoice = Factory.New<APInvoice>();

			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_RL_NKClosestPort = "ITROM";
			invoice.AH_OH = header.PK;
			invoice.AH_TransactionNum = "00001";
			invoice.ExchangeRate.Currency = TestObjectCreator.USD.RX_Code;
			invoice.ExchangeRate.Rate = 0.85;

			Factory.Save();

			AccTaxRate rated22 = Factory.NewWithValidTestData<AccTaxRate>();
			rated22.AT_Type = AccTaxRate.Types.Rated;
			rated22.AT_A9_DefaultVatClass = msg1.PK;
			rated22.SetRateNumerator_ForTestOnly(22);

			AccTaxRate reverseRated0 = Factory.NewWithValidTestData<AccTaxRate>();
			reverseRated0.AT_Type = AccTaxRate.Types.ReverseRated;
			reverseRated0.AT_A9_DefaultVatClass = msg2.PK;
			reverseRated0.SetRateNumerator_ForTestOnly(0);

			AccTaxRate reverseRated22 = Factory.NewWithValidTestData<AccTaxRate>();
			reverseRated22.AT_Type = AccTaxRate.Types.ReverseRated;
			reverseRated22.AT_A9_DefaultVatClass = msg3.PK;
			reverseRated22.SetRateNumerator_ForTestOnly(22);

			var line0 = (InvoicingLineBase)invoice.Lines.AddNew();
			line0.AL_AT = rated22.PK;
			line0.AL_OSExTaxAmount = 25.00;
			line0.AL_OSTaxAmount = 5.50;

			var line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			line1.AL_AT = reverseRated22.PK;
			line1.AL_OSExTaxAmount = 50.00;
			line1.AL_OSTaxAmount = 0.00;

			var line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			line2.AL_AT = rated22.PK;
			line2.AL_OSExTaxAmount = 47.00;
			line2.AL_OSTaxAmount = 10.34;

			var line3 = (InvoicingLineBase)invoice.Lines.AddNew();
			line3.AL_AT = reverseRated0.PK;
			line3.AL_OSExTaxAmount = 74.00;
			line3.AL_OSTaxAmount = 0.00;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);
			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var invoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(invoice, Factory);

			AssertEquals("OSDocumentTitleForITAutofattura is Autofattura", "Autofattura", invoiceWrapper.OSDocumentTitleForITAutofattura);

			var line0_wrapped = ((DocARInvoiceLine)invoiceWrapper.LinesForInvoice[0]);
			line0_wrapped.ShowPercentInGSTDisplay = true;
			AssertEquals("Display tax for line 0", "22%=5,50", line0_wrapped.GetOSTaxAmountDisplay());
			AssertEquals("Raw tax for line 0", new ZDecimal(5.50), line0_wrapped.OSTaxAmount_Raw);
			AssertEquals("Raw local tax for line 0", new ZDecimal(6.4702), line0_wrapped.LocalTaxAmount_Raw);

			var line1_wrapped = ((DocARInvoiceLine)invoiceWrapper.LinesForInvoice[1]);
			line1_wrapped.ShowPercentInGSTDisplay = true;
			AssertEquals("Display tax for line 1", "Reverse Charge", line1_wrapped.GetOSTaxAmountDisplay());
			AssertEquals("Raw tax for line 1", new ZDecimal(11.00), line1_wrapped.OSTaxAmount_Raw);
			AssertEquals("Raw local tax for line 1", new ZDecimal(12.9404), line1_wrapped.LocalTaxAmount_Raw);

			var line2_wrapped = ((DocARInvoiceLine)invoiceWrapper.LinesForInvoice[2]);
			line2_wrapped.ShowPercentInGSTDisplay = true;
			AssertEquals("Display tax for line 2", "22%=10,34", line2_wrapped.GetOSTaxAmountDisplay());
			AssertEquals("Raw tax for line 2", new ZDecimal(10.34), line2_wrapped.OSTaxAmount_Raw);
			AssertEquals("Raw local tax for line 2", new ZDecimal(12.1638), line2_wrapped.LocalTaxAmount_Raw);

			var line3_wrapped = ((DocARInvoiceLine)invoiceWrapper.LinesForInvoice[3]);
			line3_wrapped.ShowPercentInGSTDisplay = true;
			AssertEquals("Display tax for line 3", "Reverse Charge", line3_wrapped.GetOSTaxAmountDisplay());
			AssertEquals("Raw tax for line 3", new ZDecimal(0.00), line3_wrapped.OSTaxAmount_Raw);
			AssertEquals("Raw local tax for line 3", new ZDecimal(0.00), line3_wrapped.LocalTaxAmount_Raw);

			AssertEquals("TaxRateByRateAndMessageColumnRaw", @"0%
22%
22%
", invoiceWrapper.TaxRateByRateAndMessageColumnRaw);

			AssertEquals("TotalTaxByRateAndMessageColumnRaw", @"0,00
15,84
11,00
", invoiceWrapper.TotalTaxByRateAndMessageColumnRaw);

			AssertEquals("TotalOSTaxAmountRawFormatted", "26,84", invoiceWrapper.TotalOSTaxAmountRawFormatted);
			AssertEquals("OSTotalRawFormatted", "222,84", invoiceWrapper.OSTotalRawFormatted);

			AssertEquals("InvoiceLocalSubTotalFormatted", "230,58", invoiceWrapper.InvoiceLocalSubTotalFormatted);
			AssertEquals("TotalLocalTax_Raw", new ZDecimal(31.57), invoiceWrapper.TotalLocalTax_Raw);
			AssertEquals("TotalLocalInvoiceAmount_Raw", new ZDecimal(262.15), invoiceWrapper.TotalLocalInvoiceAmount_Raw);
			AssertEquals("TotalLocalInvoiceAmountRawFormatted", "262,15", invoiceWrapper.TotalLocalInvoiceAmountRawFormatted);

			var creditNote = Factory.New<APCreditNote>();
			creditNote.AH_OH = header.PK;
			creditNote.AH_TransactionNum = "00002";

			var invoiceWrapper2 = (DocARInvoiceCommon)DocARInvoice.New(creditNote, Factory);

			AssertEquals("OSDocumentTitleForITAutofattura is Autoaccredito", "Autoaccredito", invoiceWrapper2.OSDocumentTitleForITAutofattura);
		}

		public void TestDocument_ITAutofattura_TotalsAmounts()
		{
			AccInvMsg msg = Factory.NewWithValidTestData<AccInvMsg>();
			msg.A9_IsShownOnDocuments = true;
			msg.A9_Description = "IVA 22 REV CHARGE";
			msg.A9_EnglishMsg = "IVA 22 REV CHARGE";
			msg.A9_LocalMsg = string.Empty;

			var invoice = Factory.New<APInvoice>();

			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_RL_NKClosestPort = "ITROM";
			invoice.AH_OH = header.PK;
			invoice.AH_TransactionNum = "00001";
			invoice.ExchangeRate.Currency = TestObjectCreator.USD.RX_Code;
			invoice.ExchangeRate.Rate = 1.072194;

			Factory.Save();

			AccTaxRate reverseRated22 = Factory.NewWithValidTestData<AccTaxRate>();
			reverseRated22.AT_Type = AccTaxRate.Types.ReverseRated;
			reverseRated22.AT_A9_DefaultVatClass = msg.PK;
			reverseRated22.SetRateNumerator_ForTestOnly(22);

			for (int i = 1; i < 12; i++)
			{
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_AT = reverseRated22.PK;
				line.AL_ExchangeRate = 1.072194m;
				line.AL_OSExTaxAmount = 45m;
				line.AL_OSTaxAmount = 9.9m;
				line.AL_OSAmount = 54.9m;
			}

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);
			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var invoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(invoice, Factory);

			AssertEquals("OSDocumentTitleForITAutofattura is Autofattura", "Autofattura", invoiceWrapper.OSDocumentTitleForITAutofattura);

			var line_wrapped = ((DocARInvoiceLine)invoiceWrapper.LinesForInvoice[0]);
			AssertEquals("Display tax for line", "Reverse Charge", line_wrapped.GetOSTaxAmountDisplay());
			AssertEquals("Raw tax for line", new ZDecimal(9.90), line_wrapped.OSTaxAmount_Raw);
			AssertEquals("Raw local tax for line", new ZDecimal(9.2334), line_wrapped.LocalTaxAmount_Raw);

			AssertEquals("TotalOSTaxAmountRaw", new ZDecimal(108.90), invoiceWrapper.TotalOSTaxAmount);
			AssertEquals("TotalOSTaxAmountRaw", new ZDecimal(495.00), invoiceWrapper.OSExTaxAmount);
			AssertEquals("TotalOSTaxAmountRaw", "603,90", invoiceWrapper.OSTotalRawFormatted);

			AssertEquals("InvoiceLocalSubTotalFormatted", "461,67", invoiceWrapper.InvoiceLocalSubTotalFormatted);
			AssertEquals("TotalLocalTax_Raw", new ZDecimal(101.57), invoiceWrapper.TotalLocalTax_Raw);
			AssertEquals("TotalLocalInvoiceAmount_Raw", new ZDecimal(563.24), invoiceWrapper.TotalLocalInvoiceAmount_Raw);
		}

		public void TestOSPrimaryTaxDisplayHeading()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_OSExTaxAmount = 10;

			var arInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(ZString.Empty, arInvoiceWrapper.OSPrimaryTaxDisplayHeading);

			line.AL_AT = TestObjectCreator.GST1.PK;
			arInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Factory.ClearCachedValue<ZBool>(InvoicingBase.PK.ToStringKey());
			AssertEquals("GST", arInvoiceWrapper.OSPrimaryTaxDisplayHeading);
		}

		public void TestStaticNewMethodWithAccTransactionHeader()
		{
			DocARInvoice nullInvoice = DocARInvoice.New((AccTransactionHeader)null, Factory);
			AssertNull("Invoice wrapper is null", nullInvoice);

			var header = Factory.Load<AccTransactionHeader>(Invoice.PK);
			DocARInvoice invoiceWrapper = DocARInvoice.New(header, Factory);
			AssertNotNull("Invoice wrapper is not null", ARInvoiceWrapper);
		}

		public void TestShowWorkPhoneNumber()
		{
			Invoice.Factory.Save();
			Invoice.Creator.GS_FullName = "TEST NAME";
			Invoice.Creator.GS_WorkPhone = "0420 019 999";
			Invoice.Creator.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			DocARInvoice invoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AssertEquals("TEST NAME", invoiceWrapper.CreatedByUserName);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, invoiceWrapper.CreatedByUserInitials);

			Assert(!invoiceWrapper.ShowOperatorsName);
			AssertEquals("", invoiceWrapper.CreatedByUserPhone);

			AccountingConfigurationRegistry.Instance.ShowOperatorsNameOnInvoice.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Assert(invoiceWrapper.ShowOperatorsName);
			AssertEquals("", invoiceWrapper.CreatedByUserPhone);

			AccountingConfigurationRegistry.Instance.ShowOperatorsWorkPhoneNumberOnInvoice.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertEquals("Phone: +61 420 019 999", invoiceWrapper.CreatedByUserPhone);
		}

		public void TestShowOperatorSignatore()
		{
			Invoice.Factory.Save();
			Invoice.Creator.GS_FullName = "TEST NAME";
			Invoice.Creator.SignatureImage = new System.Drawing.Bitmap(1, 2);
			DocARInvoice invoiceWrapper = DocARInvoice.New(Invoice, Factory);
			var documentUsageReporter = new DocumentEngine.DocumentUsageReporter();
			Factory.ServiceContainer.AddService(new DocumentEngine.DocumentUsageDetailsCollector(documentUsageReporter));

			AccountingConfigurationRegistry.Instance.ShowOperatorsNameOnInvoice.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.ShowOperatorsSignature.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			AssertNull(invoiceWrapper.SignatureForInvoiceDocuments);

			AccountingConfigurationRegistry.Instance.ShowOperatorsNameOnInvoice.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.ShowOperatorsSignature.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertNull(invoiceWrapper.SignatureForInvoiceDocuments);

			AccountingConfigurationRegistry.Instance.ShowOperatorsNameOnInvoice.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ShowOperatorsSignature.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			AssertNull(invoiceWrapper.SignatureForInvoiceDocuments);

			AssertEquals("IsUserSignatureUsed", false, documentUsageReporter.IsUserSignatureUsed);

			AccountingConfigurationRegistry.Instance.ShowOperatorsNameOnInvoice.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ShowOperatorsSignature.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertEquals("IsUserSignatureUsed", false, documentUsageReporter.IsUserSignatureUsed);
			AssertNotNull(invoiceWrapper.SignatureForInvoiceDocuments);
			Assert("IsUserSignatureUsed", documentUsageReporter.IsUserSignatureUsed);
		}

		public void TestPIACreditTerms()
		{
			Invoice.AH_InvoiceTerm = InvoiceTermsList.PaymentInAdvance.Code;
			DocARInvoice invoiceWrapper = DocARInvoice.New(Invoice, Factory);

			Assert("PIA", invoiceWrapper.CreditTerms.ToString().Contains(InvoiceTermWithShortDescription.PaymentInAdvance.Description));
		}

		public void TestMLICreditTerms()
		{
			Invoice.AH_InvoiceTerm = InvoiceTerms.MultipleInstallments;
			var invoiceWrapper = DocARInvoice.New(Invoice, Factory);

			Assert("MLI", invoiceWrapper.CreditTerms.ToString().Contains(InvoiceTermWithShortDescription.MultipleInstallments.Description));
		}

		public void TestCreditTermsInEnglishWhenTheCountryIsPortugal()
		{
			Invoice.AH_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			DocARInvoice invoiceWrapper = DocARInvoice.New(Invoice, Factory);

			Assert("COD", invoiceWrapper.CreditTerms.ToString().Contains(InvoiceTermWithShortDescription.CashOnDelivery.Description));

			SetLineDetails((InvoicingLineBase)InvoicingBase.Lines.AddNew(), TestObjectCreator.CC1, TestObjectCreator.GST1, 10m, 1m);

			foreach (CodeDescriptionPair invoiceTerm in new ARInvoiceTermsList())
			{
				if (invoiceTerm.Code != Constants.InvoiceTerms.LaterOfShipmentOrInvoiceDate && invoiceTerm.Code != Constants.InvoiceTerms.FromDeliveryOrPickupDate) //Do not test LSI or DLP here as it has some logic in GetCreditTerms. Do test in TransactionHeaderHelperTest in accounting project as Unit Test
				{
					AssertCreditTermsByCountry(Core.Constants.CountryCodes.Portugal, Core.Constants.Languages.Portuguese, invoiceWrapper, invoiceTerm);
					AssertCreditTermsByCountry(Core.Constants.CountryCodes.France, Core.Constants.Languages.Portuguese, invoiceWrapper, invoiceTerm, false);
					AssertCreditTermsByCountry(Core.Constants.CountryCodes.Portugal, Core.Constants.Languages.French, invoiceWrapper, invoiceTerm);
					AssertCreditTermsByCountry(Core.Constants.CountryCodes.France, Core.Constants.Languages.French, invoiceWrapper, invoiceTerm, false);
				}
			}
		}

		void AssertCreditTermsByCountry(string country, string language, DocARInvoice invoiceWrapper, CodeDescriptionPair invoiceTerm, bool expectedEnglish = true)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			using (Res.TemporarilySwitchLanguage(language))
			{
				Invoice.AH_InvoiceTerm = invoiceTerm.Code;
				CodeDescriptionPairList termList = new InvoiceTermsListWithShortDescription();
				MultilingualString invoiceTermDescription = termList.GetMultilingualDescriptionFromCode(invoiceTerm.Code);

				AssertEquals(string.Format("Credit Terms", language), expectedEnglish ? invoiceTermDescription.GetUnresolvedString().Replace("{", "").Replace("}", "").Trim() : invoiceTermDescription.ToString().Replace("{", "").Replace("}", "").Trim(), invoiceWrapper.CreditTerms);
			}
		}

		void AssertShowLocalGSTAmount(string countryCode, string localCurrencyCode, string foreignCurrencyCode)
		{
			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AT = new TestObjectCreator(Factory).GST1.PK;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				AccountingConfigurationRegistry.Instance.ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				Invoice.AH_RX_NKTransactionCurrency = localCurrencyCode;
				AssertEquals("ShowLocalGSTAmount value when ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency is false and invoice is a local currency invoice", false, ARInvoiceWrapper.ShowLocalGSTAmount);

				Invoice.AH_RX_NKTransactionCurrency = foreignCurrencyCode;
				AssertEquals("ShowLocalGSTAmount value when ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency is false and invoice is a foreign currency invoice", true, ARInvoiceWrapper.ShowLocalGSTAmount);

				AccountingConfigurationRegistry.Instance.ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				Invoice.AH_RX_NKTransactionCurrency = localCurrencyCode;
				AssertEquals("ShowLocalGSTAmount value when ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency is false and invoice is a local currency invoice", false, ARInvoiceWrapper.ShowLocalGSTAmount);

				Invoice.AH_RX_NKTransactionCurrency = foreignCurrencyCode;
				AssertEquals("ShowLocalGSTAmount value when ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency is false and invoice is a foreign currency invoice", false, ARInvoiceWrapper.ShowLocalGSTAmount);
			}
		}

		public void TestShowLocalGSTAmount_Australia()
		{
			AssertShowLocalGSTAmount(CountryCodes.Australia, CurrencyCodes.Australia, CurrencyCodes.UnitedKingdom);
		}

		public void TestShowLocalGSTAmount_UnitedKingdom()
		{
			AssertShowLocalGSTAmount(CountryCodes.UnitedKingdom, CurrencyCodes.UnitedKingdom, CurrencyCodes.Australia);
		}

		public void TestShowLocalGSTAmount_CzechRepublic()
		{
			AssertShowLocalGSTAmount(CountryCodes.CzechRepublic, CurrencyCodes.CzechRepublic, CurrencyCodes.Australia);
		}

		public void TestShowLocalGSTAmount_Norway()
		{
			AssertShowLocalGSTAmount(CountryCodes.Norway, CurrencyCodes.Norway, CurrencyCodes.UnitedStates);
		}

		public void TestShowLocalGSTAmount_Serbia()
		{
			AssertShowLocalGSTAmount(CountryCodes.Serbia, "RSD", CurrencyCodes.UnitedStates);
		}

		public void TestShowLocalGSTAmount_UnitedArabEmirates()
		{
			AssertShowLocalGSTAmount(CountryCodes.UnitedArabEmirates, CurrencyCodes.UnitedArabEmirates, CurrencyCodes.UnitedStates);
		}

		public void TestShowLocalGSTAmount_Bahrain()
		{
			AssertShowLocalGSTAmount(CountryCodes.Bahrain, CurrencyCodes.Bahrain, CurrencyCodes.UnitedStates);
		}

		public void TestShowLocalGSTAmount_Kuwait()
		{
			AssertShowLocalGSTAmount(CountryCodes.Kuwait, CurrencyCodes.Kuwait, CurrencyCodes.UnitedStates);
		}

		public void TestShowLocalGSTAmount_Oman()
		{
			AssertShowLocalGSTAmount(CountryCodes.Oman, CurrencyCodes.Oman, CurrencyCodes.UnitedStates);
		}

		public void TestShowLocalGSTAmount_Qatar()
		{
			AssertShowLocalGSTAmount(CountryCodes.Qatar, CurrencyCodes.Qatar, CurrencyCodes.UnitedStates);
		}

		public void TestShowLocalGSTAmount_SaudiArabia()
		{
			AssertShowLocalGSTAmount(CountryCodes.SaudiArabia, CurrencyCodes.SaudiArabia, CurrencyCodes.UnitedStates);
		}

		public void TestShowLocalGSTAmount_NewCaledonia()
		{
			AssertShowLocalGSTAmount(CountryCodes.NewCaledonia, CurrencyCodes.NewCaledonia, CurrencyCodes.UnitedStates);
		}

		public void TestShowLocalGSTAmount_LaoPeoplesDemocraticRepublic()
		{
			AssertShowLocalGSTAmount(CountryCodes.LaoPeoplesDemocraticRepublic, CurrencyCodes.Lao, CurrencyCodes.UnitedStates);
		}

		public void TestShowLocalGSTAmountForBrexit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
				var vat = Factory.NewWithValidTestData<AccTaxRate>();
				line.AL_AT = vat.PK;
				var wrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);

				var gbCountry = RefCountry.LoadFromCountryCode(new BusinessObjectFactory(), Constants.CountryCodes.UnitedKingdom);
				gbCountry.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
				gbCountry.Factory.Save();
				Assert("Before Brexit", wrapper.ShowLocalGSTAmount);

				gbCountry.RN_EconomicGrouping = "";
				gbCountry.Factory.Save();
				Assert("After Brexit", wrapper.ShowLocalGSTAmount);
			}
		}

		public void TestShowLocalEquivalentTotalAmounts()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Invoice.AH_RX_NKTransactionCurrency = CurrencyCodes.Australia;
				AccountingConfigurationRegistry.Instance.ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				AssertEquals("ShowLocalEquivalentTotalAmounts value when ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency is false and invoice is local currency invoice", false, ARInvoiceWrapper.ShowLocalEquivalentTotalAmounts);

				Invoice.AH_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
				AssertEquals("ShowLocalEquivalentTotalAmounts value when ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency is false and invoice is a foreign currency invoice", false, ARInvoiceWrapper.ShowLocalEquivalentTotalAmounts);

				AccountingConfigurationRegistry.Instance.ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				Invoice.AH_RX_NKTransactionCurrency = CurrencyCodes.Australia;
				AssertEquals("ShowLocalEquivalentTotalAmounts value when ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency is true and invoice is local currency invoice", false, ARInvoiceWrapper.ShowLocalEquivalentTotalAmounts);

				Invoice.AH_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
				AssertEquals("ShowLocalEquivalentTotalAmounts value when ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency is true and invoice is a foreign currency invoice", true, ARInvoiceWrapper.ShowLocalEquivalentTotalAmounts);
			}
		}

		public void TestLocalTotalAndLocalTotalFormatted()
		{
			InvoicingBase.AH_InvoiceAmount = 1123.33M;
			InvoicingBase.AH_GSTAmount = 112.336M;
			AssertEquals("Pre-Condition for AH_LocalTotalAmount", InvoicingBase.AH_Ledger == LedgerTypes.AccountsPayable ? -1235.666M : 1235.666M, InvoicingBase.AH_LocalTotalAmount);
			AssertEquals("LocalTotal", InvoicingBase.AH_Ledger == LedgerTypes.AccountsPayable ? -1235.67M : 1235.67M, ARInvoiceWrapper.LocalTotal);
			AssertEquals("LocalTotalFormatted", InvoicingBase.AH_Ledger == LedgerTypes.AccountsPayable ? "-1,235.67" : "1,235.67", ARInvoiceWrapper.LocalTotalFormatted);
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "VND";
			AssertEquals("LocalTotalFormatted", InvoicingBase.AH_Ledger == LedgerTypes.AccountsPayable ? "-1,236" : "1,236", ARInvoiceWrapper.LocalTotalFormatted);
		}

		public void TestInvoiceLocalSubTotalAndInvoiceLocalSubTotalFormatted()
		{
			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();

			line1.AL_LocalExTaxAmount = 1123.33M;
			line1.AL_LocalTaxAmount = 112.333m;
			line2.AL_LocalExTaxAmount = 2115.66M;
			line2.AL_LocalTaxAmount = 211.566m;

			AssertNull("Not asigned yet", InvoicingBase.Header);
			AssertEquals("InvoiceLocalSubTotal", 3238.99m, ARInvoiceWrapper.InvoiceLocalSubTotal);
			AssertEquals("InvoiceLocalSubTotalFormatted", "3,238.99", ARInvoiceWrapper.InvoiceLocalSubTotalFormatted);

			InvoicingBase.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertNotNull("Should be asigned", InvoicingBase.Header);
			InvoicingBase.Header.MiscServ.OM_ARDontShowTaxOnDocs = false;
			AssertEquals(false, ARInvoiceWrapper.Organisation.MiscServ.ARDontShowTaxOnDocs);
			AssertEquals("InvoiceLocalSubTotal", 3238.99m, ARInvoiceWrapper.InvoiceLocalSubTotal);
			AssertEquals("InvoiceLocalSubTotalFormatted", "3,238.99", ARInvoiceWrapper.InvoiceLocalSubTotalFormatted);

			InvoicingBase.Header.MiscServ.OM_ARDontShowTaxOnDocs = true;
			line1.AL_LocalTaxAmount = 112.333m;
			line2.AL_LocalTaxAmount = 211.566m;
			AssertEquals(true, ARInvoiceWrapper.Organisation.MiscServ.ARDontShowTaxOnDocs);
			AssertEquals("InvoiceLocalSubTotal", 3562.89m, ARInvoiceWrapper.InvoiceLocalSubTotal);
			AssertEquals("InvoiceLocalSubTotalFormatted", "3,562.89", ARInvoiceWrapper.InvoiceLocalSubTotalFormatted);

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "VND";
			AssertEquals("InvoiceLocalSubTotalFormatted", "3,563", ARInvoiceWrapper.InvoiceLocalSubTotalFormatted);
		}

		public void TestLinesForVNInvoice()
		{
			for (int count = 1; count <= 5; count++)
			{
				InvoicingBase.Lines.AddNew();
			}
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("VN Invoice Lines Count", 5, aRInvoiceWrapper.LinesForVNInvoice.Count);
			AssertEquals("Too many Lines", false, aRInvoiceWrapper.TooManyLinesForVNInvoice);

			InvoicingBase.Lines.AddNew();
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("VN Invoice Lines Count", 5, aRInvoiceWrapper.LinesForVNInvoice.Count);
			AssertEquals("Too many Lines", true, aRInvoiceWrapper.TooManyLinesForVNInvoice);
		}

		public void TestCurrencyInVNForVNInvoice()
		{
			InvoicingBase.AH_RX_NKTransactionCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;

			TestObjectCreator creator = new TestObjectCreator(Factory);
			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_AT = creator.VATSPV.PK;
			line1.AL_OSExTaxAmount = 100m;
			line1.AL_RX_NKTransactionCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			line1.AL_AG = creator.GLHeader1.PK;

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => GetBaseInvoiceWrapper());

			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_UnitName", currency.PK, "VI-VN", "RX", "đô la mỹ");
			Factory.Save();
			AssertEquals("OSAmount", 100m, ARInvoiceWrapper.OSTotal);
			AssertEquals("CurrencyInVN", "Một trăm đô la mỹ", ARInvoiceWrapper.OSTotalInWordsVietnamese);
		}

		public void TestHasSuspendedTax()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_AT = creator.GST1.PK;
			line1.AL_AG = creator.GLHeader1.PK;
			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_AT = creator.GST1.PK;
			line2.AL_AG = creator.GLHeader1.PK;

			AssertEquals("HasSuspendedTax", false, ARInvoiceWrapper.HasSuspendedTax);

			line2.AL_AT = creator.SVAT1.PK;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => GetBaseInvoiceWrapper());
			AssertEquals("HasSuspendedTax", true, ARInvoiceWrapper.HasSuspendedTax);
		}

		public void TestSuspendedTaxTitle()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_AT = creator.GST1.PK;
			line1.AL_AG = creator.GLHeader1.PK;
			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_AT = creator.GST1.PK;
			line2.AL_AG = creator.GLHeader1.PK;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => GetBaseInvoiceWrapper());

			Assert("SuspendedTaxTitle must be empty with no Suspended Tax", ARInvoiceWrapper.SuspendedTaxTitle.IsEmpty);
			Assert("SuspendedTaxTitleWithDescriptionOverride must be empty with no Suspended Tax", ARInvoiceWrapper.SuspendedTaxTitleWithDescriptionOverride.IsEmpty);

			line1.AL_AT = creator.SVAT1.PK;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => GetBaseInvoiceWrapper());
			AssertEquals("SuspendedTaxTitle with single Suspended Tax", "SUSPENDED GST @12%", ARInvoiceWrapper.SuspendedTaxTitle);
			AssertEquals("SuspendedTaxTitleWithDescriptionOverride with single Suspended Tax", "SUSPENDED GST @12%", ARInvoiceWrapper.SuspendedTaxTitleWithDescriptionOverride);
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Suspended, "Override Suspended");
			AssertEquals("SuspendedTaxTitleWithDescriptionOverride with single Suspended Tax", "OVERRIDE SUSPENDED GST @12%", ARInvoiceWrapper.SuspendedTaxTitleWithDescriptionOverride);

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Suspended, string.Empty);

			line2.AL_AT = creator.SVAT2.PK;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => GetBaseInvoiceWrapper());
			AssertEquals("SuspendedTaxTitle with more than one Suspended Tax", "SUSPENDED GST", ARInvoiceWrapper.SuspendedTaxTitle);
			AssertEquals("SuspendedTaxTitleWithDescriptionOverride with more than one Suspended Tax", "SUSPENDED GST", ARInvoiceWrapper.SuspendedTaxTitleWithDescriptionOverride);
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Suspended, "Override Suspended");
			AssertEquals("SuspendedTaxTitleWithDescriptionOverride with more than one Suspended Tax", "OVERRIDE SUSPENDED GST", ARInvoiceWrapper.SuspendedTaxTitleWithDescriptionOverride);

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Suspended, string.Empty);

			line2.AL_TaxRateNumerator = 12;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => GetBaseInvoiceWrapper());
			AssertEquals("SuspendedTaxTitle with single Suspended Tax", "SUSPENDED GST @12%", ARInvoiceWrapper.SuspendedTaxTitle);
			AssertEquals("SuspendedTaxTitleWithDescriptionOverride with single Suspended Tax", "SUSPENDED GST @12%", ARInvoiceWrapper.SuspendedTaxTitleWithDescriptionOverride);
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Suspended, "Override Suspended");
			AssertEquals("SuspendedTaxTitleWithDescriptionOverride with single Suspended Tax", "OVERRIDE SUSPENDED GST @12%", ARInvoiceWrapper.SuspendedTaxTitleWithDescriptionOverride);
		}

		public void TestTotalValueIncludingSuspendedTaxTitle()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_AT = creator.GST1.PK;
			line1.AL_AG = creator.GLHeader1.PK;
			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_AT = creator.GST1.PK;
			line2.AL_AG = creator.GLHeader1.PK;

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => GetBaseInvoiceWrapper());
			Assert("TotalValueIncludingSuspendedTaxTitle must be empty with no Suspended Tax", ARInvoiceWrapper.TotalValueIncludingSuspendedTaxTitle.IsEmpty);
			Assert("TotalValueIncludingSuspendedTaxTitleWithDescriptionOverride must be empty with no Suspended Tax", ARInvoiceWrapper.TotalValueIncludingSuspendedTaxTitleWithDescriptionOverride.IsEmpty);

			line1.AL_AT = creator.SVAT1.PK;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => GetBaseInvoiceWrapper());
			AssertEquals("TotalValueIncludingSuspendedTaxTitle with single Suspended Tax", "TOTAL VALUE OF SUPPLY INCLUDING SUSPENDED GST", ARInvoiceWrapper.TotalValueIncludingSuspendedTaxTitle);
			AssertEquals("TotalValueIncludingSuspendedTaxTitleWithDescriptionOverride with single Suspended Tax", "TOTAL VALUE OF SUPPLY INCLUDING SUSPENDED GST", ARInvoiceWrapper.TotalValueIncludingSuspendedTaxTitleWithDescriptionOverride);
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Suspended, "Override Suspended");
			AssertEquals("TotalValueIncludingSuspendedTaxTitleWithDescriptionOverride with single Suspended Tax", "TOTAL VALUE OF SUPPLY INCLUDING OVERRIDE SUSPENDED GST", ARInvoiceWrapper.TotalValueIncludingSuspendedTaxTitleWithDescriptionOverride);

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Suspended, string.Empty);

			line2.AL_AT = creator.SVAT2.PK;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => GetBaseInvoiceWrapper());
			AssertEquals("TotalValueIncludingSuspendedTaxTitle with more than one Suspended Tax", "TOTAL VALUE OF SUPPLY INCLUDING SUSPENDED GST", ARInvoiceWrapper.TotalValueIncludingSuspendedTaxTitle);
			AssertEquals("TotalValueIncludingSuspendedTaxTitleWithDescriptionOverride with more than one Suspended Tax", "TOTAL VALUE OF SUPPLY INCLUDING SUSPENDED GST", ARInvoiceWrapper.TotalValueIncludingSuspendedTaxTitleWithDescriptionOverride);
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Suspended, "Override Suspended");
			AssertEquals("TotalValueIncludingSuspendedTaxTitleWithDescriptionOverride with more than one Suspended Tax", "TOTAL VALUE OF SUPPLY INCLUDING OVERRIDE SUSPENDED GST", ARInvoiceWrapper.TotalValueIncludingSuspendedTaxTitleWithDescriptionOverride);
		}

		public void TestSuspendedTaxAmount()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_AT = creator.GST1.PK;
			line1.AL_OSExTaxAmount = 100m;
			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_AT = creator.GST1.PK;
			line2.AL_OSExTaxAmount = 200m;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			InvoiceWrapper = GetBaseInvoiceWrapper();

			AssertEquals("SuspendedTaxAmount must be zero with no Suspended Tax", ZDecimal.Zero, ARInvoiceWrapper.SuspendedTaxAmount);

			line1.AL_AT = creator.SVAT1.PK;
			line1.AL_OSExTaxAmount = 100m;
			InvoiceWrapper = GetBaseInvoiceWrapper();
			AssertEquals("Should be no Tax Amount on the line", ZDecimal.Zero, line1.AL_OSGSTAmount);
			AssertEquals("SuspendedTaxAmount", 12m, ARInvoiceWrapper.SuspendedTaxAmount);

			line2.AL_AT = creator.SVAT2.PK;
			line2.AL_OSExTaxAmount = 200m;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => GetBaseInvoiceWrapper());
			AssertEquals("Should be no Tax Amount on the line", ZDecimal.Zero, line2.AL_OSGSTAmount);
			AssertEquals("SuspendedTaxAmount", 42m, ARInvoiceWrapper.SuspendedTaxAmount);

			line2.AL_TaxRateNumerator = 14;
			line2.AL_OSExTaxAmount = 200m;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => GetBaseInvoiceWrapper());
			AssertEquals("Should be no Tax Amount on the line", ZDecimal.Zero, line2.AL_OSGSTAmount);
			AssertEquals("SuspendedTaxAmount", 40m, ARInvoiceWrapper.SuspendedTaxAmount);
		}

		public void TestTotalValueIncludingSuspendedTax()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_AT = creator.GST1.PK;
			line1.AL_OSExTaxAmount = 100m;
			line1.AL_AG = creator.GLHeader1.PK;
			AssertEquals("Should be Tax Amount on the line1", 10m, line1.AL_OSGSTAmount);
			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_AT = creator.GST1.PK;
			line2.AL_OSExTaxAmount = 200m;
			AssertEquals("Should be Tax Amount on the line", 20m, line2.AL_OSGSTAmount);
			InvoiceWrapper = GetBaseInvoiceWrapper();

			AssertEquals("OSAmount", 330m, ARInvoiceWrapper.OSTotal);
			AssertEquals("TotalValueIncludingSuspendedTax must be equal to OSTotal with no Suspended Tax", ARInvoiceWrapper.OSTotal, ARInvoiceWrapper.TotalValueIncludingSuspendedTax);

			line1.AL_AT = creator.SVAT1.PK;
			line1.AL_OSExTaxAmount = 100m;
			InvoiceWrapper = GetBaseInvoiceWrapper();
			AssertEquals("Should be no Tax Amount on the line1", ZDecimal.Zero, line1.AL_OSGSTAmount);
			AssertEquals("Should be Tax Amount on the line", 20m, line2.AL_OSGSTAmount);
			AssertEquals("TotalValueIncludingSuspendedTax", 332m, ARInvoiceWrapper.TotalValueIncludingSuspendedTax);

			line2.AL_AT = creator.SVAT2.PK;
			line2.AL_OSExTaxAmount = 200m;
			line2.AL_AG = creator.GLHeader1.PK;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => GetBaseInvoiceWrapper());
			AssertEquals("Should be no Tax Amount on the line", ZDecimal.Zero, line2.AL_OSGSTAmount);
			AssertEquals("TotalValueIncludingSuspendedTax", 342m, ARInvoiceWrapper.TotalValueIncludingSuspendedTax);
		}

		public void TestOSTotalExcludeSPVTax()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				TestObjectCreator creator = new TestObjectCreator(Factory);
				InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
				line1.AL_AT = creator.VATSPV.PK;
				line1.AL_OSExTaxAmount = 100m;
				AssertEquals("Should be Tax Amount on the line1", 22m, line1.AL_OSGSTAmount);
				InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
				line2.AL_AT = creator.VATSPV.PK;
				line2.AL_OSExTaxAmount = 200m;
				line1.AL_AG = creator.GLHeader1.PK;
				line2.AL_AG = creator.GLHeader1.PK;
				AssertEquals("Should be Tax Amount on the line", 44m, line2.AL_OSGSTAmount);
				InvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => GetBaseInvoiceWrapper());

				AssertEquals("OSAmount", 300m, ARInvoiceWrapper.OSTotal);
				AssertEquals("OSTotalExcludeSPVAmount", 366m, ARInvoiceWrapper.OSTotalExcludeSPVAmount);
			}
		}

		public void TestTaxRateByRateAndMessageColumnForSPV()
		{
			Action<ZString, ZGuid> setupAndAssertTaxRateByRateAndMessageColumn = (expectedLabel, rate) =>
			{
				InvoicingBase.Lines[0].AL_AT = rate;
				InvoicingBase.Lines[0].AL_OSExTaxAmount = 100m;
				AssertEquals("Should be Tax Amount on the line1", 22m, InvoicingBase.Lines[0].AL_OSGSTAmount);
				InvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => GetBaseInvoiceWrapper());

				AssertMultilineASCIIEquals("the column", $"22%\n{expectedLabel}", ARInvoiceWrapper.TaxRateByRateAndMessageColumn);
				AssertMultilineASCIIEquals("the column", $"22%\n{expectedLabel}", ARInvoiceWrapper.TaxRateByRateAndMessageColumnAlwaysShowPercentage);
			};

			var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();

			TestObjectCreator creator = new TestObjectCreator(Factory);
			line.AL_AG = creator.GLHeader1.PK;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				setupAndAssertTaxRateByRateAndMessageColumn("SPV", creator.VATSPV.PK);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.CostaRica))
			{
				setupAndAssertTaxRateByRateAndMessageColumn("Exon.", creator.VATEXON.PK);
			}
		}

		public void TestOsInvoiceAmountInWords()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
				header.OH_RL_NKClosestPort = "USLAX";
				Invoice.AH_OH = header.PK;
				Invoice.AH_OSTotalAmount = 123456789.12M;

				ZString expected = NumberToString_EN.ConvertNumberToWords((long)ARInvoiceWrapper.OSTotal);
				AssertEquals("OS Invoice Amount In Words", expected, ARInvoiceWrapper.OsInvoiceAmountInWords);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.VietNam);
				header.OH_RL_NKClosestPort = "VNHAN";
				expected = NumberToString_VI_VN.VietnameseConvertNumberToWords((long)ARInvoiceWrapper.OSTotal);
				AssertEquals("OS Invoice Amount In Words", expected, ARInvoiceWrapper.OsInvoiceAmountInWords);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		public void TestOsInvoiceAmountInWords_MaxSupportedAmount()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_RL_NKClosestPort = "USLAX";
				Invoice.AH_OH = org.PK;
				Invoice.AH_OSTotalAmount = 999999999999.444M;

				ZString expectedAmount = "nine hundred and ninety nine billion, nine hundred and ninety nine million, nine hundred and ninety nine thousand, nine hundred and ninety nine";
				AssertEquals("OS Invoice Amount In Words", expectedAmount, ARInvoiceWrapper.OsInvoiceAmountInWords);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.VietNam);
				org.OH_RL_NKClosestPort = "VNHAN";
				expectedAmount = "chín trăm chín mươi chín tỷ chín trăm chín mươi chín triệu chín trăm chín mươi chín nghìn chín trăm chín mươi chín";
				AssertEquals("OS Invoice Amount In Words", expectedAmount, ARInvoiceWrapper.OsInvoiceAmountInWords);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		public void TestOsInvoiceAmountInWords_UnsupportedAmount()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_RL_NKClosestPort = "USLAX";
				Invoice.AH_OH = org.PK;
				Invoice.AH_OSTotalAmount = 99999999999999991234567899.12M;
				AssertExceptionThrown(String.Format("The number {0} is over 999,999,999,999. This number is not supported by the current version of the number to words converter.", Invoice.AH_OSTotalAmount), typeof(NotSupportedException), GetOsInvoiceAmountInWords);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.VietNam);
				org.OH_RL_NKClosestPort = "VNHAN";
				AssertExceptionThrown(String.Format("The number {0} is over 999,999,999,999. This number is not supported by the current version of the number to words converter.", Invoice.AH_OSTotalAmount), typeof(NotSupportedException), GetOsInvoiceAmountInWords);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		void GetOsInvoiceAmountInWords()
		{
			var amount = ARInvoiceWrapper.OsInvoiceAmountInWords;
		}

		public void TestOSOutstandingAmountFormatted()
		{
			Invoice.AH_OSExTaxAmount = 123456789.12M;
			Invoice.AH_OutstandingAmount = 123456789.12M;

			AssertEquals("Pre-condition: Wrapper's OSOutstandingAmount should be equal to Invoice AH_OSTotal", Invoice.AH_OSTotal, ARInvoiceWrapper.OSOutstandingAmount);
			AssertEquals("Wrapper's OSOutstandingAmountFormatted should be as expected", ARInvoiceWrapper.OSTotalFormatted, ARInvoiceWrapper.OSOutstandingAmountFormatted);

			if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				Invoice.AH_TransactionType = TransactionTypes.CreditNote;
				AssertEquals("Pre-condition: RegistryItem is False by default", false, AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.Value);

				AssertEquals("Pre-condition: Wrapper's OSOutstandingAmount should be equal to Invoice AH_OSTotal", Invoice.AH_OSTotal, ARInvoiceWrapper.OSOutstandingAmount);
				AssertEquals("Wrapper's OSOutstandingAmountFormatted should be as expected", ARInvoiceWrapper.OSTotalFormatted, ARInvoiceWrapper.OSOutstandingAmountFormatted);

				AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertEquals("Pre-condition: Wrapper's OSOutstandingAmount should be equal to Invoice AH_OSTotal with opposite sign", -Invoice.AH_OSTotal, ARInvoiceWrapper.OSOutstandingAmount);
				AssertEquals("Wrapper's OSOutstandingAmountFormatted should be as expected", ARInvoiceWrapper.OSTotalFormatted, ARInvoiceWrapper.OSOutstandingAmountFormatted);
			}
		}

		public void TestPrintAgentReferenceOnCustomInvoice()
		{
			AssertEquals("PrintAgentReferenceOnCustomInvoice", false, ARInvoiceWrapper.PrintAgentReferenceOnCustomInvoice);
		}

		public void TestSetTemplateConstants()
		{
			AssertEquals("DescriptionHeight", 1, ARInvoiceWrapper.DescriptionHeight);
			AssertEquals("DescriptionWidth", 1, ARInvoiceWrapper.DescriptionWidth);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.DescriptionHeight, 9);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.DescriptionWidth, 50);

			ARInvoiceWrapper.SetTemplateConstants(constants);
			AssertEquals("DescriptionHeight", 9, ARInvoiceWrapper.DescriptionHeight);
			AssertEquals("DescriptionWidth", 50, ARInvoiceWrapper.DescriptionWidth);
		}

		public void TestReversalOrAmendingReason()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			DocARInvoice wrappedInvoice = DocARInvoice.New(invoice, Factory);
			Assert("Reason should be empty", wrappedInvoice.ReversalOrAmendingReason.IsEmpty);

			var original = invoice as IAmending;
			IAmending amending = original.GenerateAmendingTransaction(TransactionTypes.Invoice);
			DocARInvoice wrappedAmendingInvoice = DocARInvoice.New((InvoicingBase)amending, Factory);
			AssertEquals("", wrappedAmendingInvoice.ReversalOrAmendingReason);

			amending.AmendingReasonCode = "IAM";
			AssertEquals("Incorrect Amounts", wrappedAmendingInvoice.ReversalOrAmendingReason);

			invoice = Factory.NewWithValidTestData<ARInvoice>();
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_OSExTaxAmount = 100;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			ARInvoiceReversing reverser = new ARInvoiceReversing(invoice);
			reverser.Reverse();
			Factory.Save();

			InvoicingBase creditNoteBizO = invoice.CorrespondingReversedTransaction;
			creditNoteBizO.AH_ReceiptType = "IAM";

			DocARInvoice wrappedCreditNote = DocARInvoice.New(creditNoteBizO, Factory);
			wrappedInvoice = DocARInvoice.New(invoice, Factory);

			AssertEquals("Incorrect Amounts", wrappedCreditNote.ReversalOrAmendingReason);
			AssertEquals("", wrappedInvoice.ReversalOrAmendingReason);
		}

		public void TestCompanyTaxRegistrationNumber()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("CompanyTaxRegistrationNumber", GlbCompany.CurrentCompany.GC_BusinessRegNo, aRInvoiceWrapper.CompanyTaxRegistrationNumber);
		}

		public void TestCompanyTaxRegistrationExtraNumber()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "GRAGO";
			Factory.Save();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			AssertEquals("CompanyTaxRegistrationExtraNumber", "", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);
			OrgCusCode taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Greece;
			taxCode.OK_CodeType = OrgCusCode.GreeceCodeTypes.DOY;
			taxCode.OK_CustomsRegNo = "345543";
			AssertEquals("CompanyTaxRegistrationExtraNumber", "345543", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);

			org.OH_RL_NKClosestPort = "LKCMB";
			Factory.Save();
			AssertEquals("CompanyTaxRegistrationExtraNumber", "", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);
			taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.SriLanka;
			taxCode.OK_CodeType = OrgCusCode.SriLankaCodeTypes.SVATBusinessRegistrationNumber;
			taxCode.OK_CustomsRegNo = "787898";
			AssertEquals("CompanyTaxRegistrationExtraNumber", "787898", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);

			org.OH_RL_NKClosestPort = "AZAST";
			Factory.Save();
			AssertEquals("CompanyTaxRegistrationExtraNumber", "", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);
			taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Azerbaijan;
			taxCode.OK_CodeType = OrgCusCode.AzerbaijanCodeTypes.TIN;
			taxCode.OK_CustomsRegNo = "123344";
			AssertEquals("CompanyTaxRegistrationExtraNumber", "123344", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);

			org.OH_RL_NKClosestPort = "KEARI";
			Factory.Save();
			AssertEquals("CompanyTaxRegistrationExtraNumber", "", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);
			taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Kenya;
			taxCode.OK_CodeType = OrgCusCode.KenyaCodeTypes.PIN;
			taxCode.OK_CustomsRegNo = "998878";
			AssertEquals("CompanyTaxRegistrationExtraNumber", "998878", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);

			org.OH_RL_NKClosestPort = "MUBAM";
			Factory.Save();
			AssertEquals("CompanyTaxRegistrationExtraNumber", "", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);
			taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Mauritius;
			taxCode.OK_CodeType = OrgCusCode.CodeTypes.BusinessRegistrationNumber;
			taxCode.OK_CustomsRegNo = "567787";
			AssertEquals("CompanyTaxRegistrationExtraNumber", "567787", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);

			org.OH_RL_NKClosestPort = "TZMSH";
			Factory.Save();
			AssertEquals("CompanyTaxRegistrationExtraNumber", "", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);
			taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Tanzania;
			taxCode.OK_CodeType = OrgCusCode.TanzaniaCodeTypes.TIN;
			taxCode.OK_CustomsRegNo = "123345";
			AssertEquals("CompanyTaxRegistrationExtraNumber", "123345", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);

			org.OH_RL_NKClosestPort = "MGAHY";
			Factory.Save();
			AssertEquals("CompanyTaxRegistrationExtraNumber", "", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);
			taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Madagascar;
			taxCode.OK_CodeType = OrgCusCode.MadagascarCodeTypes.NIS;
			taxCode.OK_CustomsRegNo = "555222";
			AssertEquals("CompanyTaxRegistrationExtraNumber", "555222", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);

			org.OH_RL_NKClosestPort = "RUMOW";
			Factory.Save();
			AssertEquals("CompanyTaxRegistrationExtraNumber", "", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);
			taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Russia;
			taxCode.OK_CodeType = OrgCusCode.RussiaCodeTypes.KPP;
			taxCode.OK_CustomsRegNo = "777888";
			AssertEquals("CompanyTaxRegistrationExtraNumber", "777888", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);

			org.OH_RL_NKClosestPort = "XK111";
			Factory.Save();
			AssertEquals("CompanyTaxRegistrationExtraNumber", "", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);
			taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Kosovo;
			taxCode.OK_CodeType = OrgCusCode.KosovoCodeTypes.NFK;
			taxCode.OK_CustomsRegNo = "435675";
			AssertEquals("CompanyTaxRegistrationExtraNumber", "435675", aRInvoiceWrapper.CompanyTaxRegistrationExtraNumber);
		}

		public void TestCompanyTaxRegistrationPANNumber()
		{
			InvoicingBase.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			InvoicingBase.Header.OH_RL_NKClosestPort = "INDEL";
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("CompanyTaxRegistrationPANNumber", "", aRInvoiceWrapper.CompanyTaxRegistrationPANNumber);
			OrgCusCode taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			taxCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.PAN;
			taxCode.OK_CustomsRegNo = "0123456789";
			AssertEquals("CompanyTaxRegistrationPANNumber", "0123456789", aRInvoiceWrapper.CompanyTaxRegistrationPANNumber);
		}

		public void TestCompanyTaxRegistrationQSTNumber()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("CompanyTaxRegistrationQSTNumber", "", aRInvoiceWrapper.CompanyTaxRegistrationQSTNumber);
			OrgCusCode taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;
			taxCode.OK_CodeType = OrgCusCode.CACodeTypes.QuebecSalesTaxID;
			taxCode.OK_CustomsRegNo = "QST123456";

			InvoicingLineBase invoiceLine = InvoicingBase.Lines.AddNew() as InvoicingLineBase;
			invoiceLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			invoiceLine.TaxRate.AT_Type = AccTaxRate.Types.Rated;
			invoiceLine.TaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("CompanyTaxRegistrationQSTNumber", "QST123456", aRInvoiceWrapper.CompanyTaxRegistrationQSTNumber);

			invoiceLine.TaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("CompanyTaxRegistrationQSTNumber", "QST123456", aRInvoiceWrapper.CompanyTaxRegistrationQSTNumber);
		}

		public void TestTaxID_DisableDisplayTaxRegistrationNumberRegistryIsFalse()
		{
			SetupTaxRateLine();

			AccountingConfigurationRegistry.Instance.DisplayTaxRegistrationNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertNotNullOrEmpty("TaxID", aRInvoiceWrapper.TaxId);

			AccountingConfigurationRegistry.Instance.DisplayTaxRegistrationNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("TaxID", ZString.Empty, aRInvoiceWrapper.TaxId);
		}

		#region TaxIDMacroDataProvider

		public void TestTaxID_WhenAccountingFactoryIsNull()
		{
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns<IAccountingCountryFactory>(null);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			SetupTaxRateLine();

			AccountingConfigurationRegistry.Instance.DisplayTaxRegistrationNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertNoExceptionThrown("TaxID", () => _ = aRInvoiceWrapper.TaxId);
		}

		public void TestTaxID_UsesInvoiceCompanyToGetAccountingCountryFactory()
		{
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			SetupTaxRateLine();
			AccountingConfigurationRegistry.Instance.DisplayTaxRegistrationNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			mockIGlobalAccountingCountryFactory.Invocations.Clear();
			InvoicingBase.Company.GC_RN_NKCountryCode = CountryCodes.Andorra;
			var aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertNoExceptionThrown("TaxID", () => _ = aRInvoiceWrapper.TaxId);
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(CountryCodes.Andorra));
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(It.IsAny<ZString>()), Times.Once);

			mockIGlobalAccountingCountryFactory.Invocations.Clear();
			InvoicingBase.Company.GC_RN_NKCountryCode = CountryCodes.Brazil;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertNoExceptionThrown("TaxID", () => _ = aRInvoiceWrapper.TaxId);
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(CountryCodes.Brazil));
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(It.IsAny<ZString>()), Times.Once);
		}

		public void TestTaxID_AccountingFactory_DoesNotImplementITaxIDMacroDataProvider()
		{
			SetupMockDataForTaxID(isITaxIDMacroDataProviderImplemented: false, returnValidTaxIDMacroData: false, taxIDMacroDataToReturn: null);

			var accountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(InvoicingBase.Company.GC_RN_NKCountryCode);
			AssertNotNull("Pre-condition: AccountingCountryFactory", accountingCountryFactory);
			AssertNull("Pre-condition: ITaxIDMacroDataProvider implementation", accountingCountryFactory as IInstanceProvider<ITaxIDMacroDataProvider>);

			SetupTaxRateLine();

			AccountingConfigurationRegistry.Instance.DisplayTaxRegistrationNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertNoExceptionThrown("TaxID", () => _ = aRInvoiceWrapper.TaxId);
		}

		public void TestTaxID_AccountingFactory_ImplementsITaxIDMacroDataProvider_GetReturnsNull()
		{
			SetupMockDataForTaxID(isITaxIDMacroDataProviderImplemented: true, returnValidTaxIDMacroData: false, taxIDMacroDataToReturn: null);

			var accountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(InvoicingBase.Company.GC_RN_NKCountryCode);
			AssertNotNull("Pre-condition: AccountingCountryFactory", accountingCountryFactory);
			AssertNotNull("Pre-condition: ITaxIDMacroDataProvider implementation", accountingCountryFactory as IInstanceProvider<ITaxIDMacroDataProvider>);
			AssertNull("Pre-condition: Country specific TaxID Macro data object", (accountingCountryFactory as IInstanceProvider<ITaxIDMacroDataProvider>).Get());

			AccountingConfigurationRegistry.Instance.DisplayTaxRegistrationNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			SetupTaxRateLine();

			var aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertNoExceptionThrown("TaxID", () => _ = aRInvoiceWrapper.TaxId);
		}

		public void TestTaxID_AccountingFactory_ImplementsITaxIDMacroDataProvider_TaxBranchOrgProxy()
		{
			var codeType = "TST";
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var codeTypeLabel = "dummyOrgTaxRegistrationPrefix #:";

			SetupMockDataForTaxID(isITaxIDMacroDataProviderImplemented: true, returnValidTaxIDMacroData: true, taxIDMacroDataToReturn: new TaxIDMacroData(codeTypeLabel, codeType));
			SetupTransactionOrgProxiesForTaxIDTest(true, true);

			var accountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(InvoicingBase.Company.GC_RN_NKCountryCode);
			AssertNotNull("Pre-condition: AccountingCountryFactory", accountingCountryFactory);
			AssertNotNull("Pre-condition: ITaxIDMacroDataProvider implementation", accountingCountryFactory as IInstanceProvider<ITaxIDMacroDataProvider>);
			AssertNotNull("Pre-condition: Country specific TaxID Macro data object", (accountingCountryFactory as IInstanceProvider<ITaxIDMacroDataProvider>).Get());

			AssertNotNull("Pre-condition: Tax Branch Organization Proxy", InvoicingBase.TaxBranch.OrgProxy);
			AssertNotNull("Pre-condition: Transaction Branch Organization Proxy", InvoicingBase.Branch.OrgProxy);
			AssertNotNull("Pre-condition: Company Organization Proxy", InvoicingBase.Company.OrgProxy);

			var companyOrgProxyCode = "3333333333";
			var transactionHeaderBranchOrgProxyCode = "5555555555";
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, codeType, countryCode, companyOrgProxyCode);
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Branch.OrgProxy, codeType, countryCode, transactionHeaderBranchOrgProxyCode);

			AccountingConfigurationRegistry.Instance.DisplayTaxRegistrationNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var wrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Tax Branch Org proxy exists but has no customCode", string.Empty, wrapper.TaxId);

			var taxBranchOrgProxyCode = "7777777777";
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.TaxBranch.OrgProxy, codeType, countryCode, taxBranchOrgProxyCode);

			wrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Tax Branch Org proxy exists and has customCode", codeTypeLabel + taxBranchOrgProxyCode, wrapper.TaxId);
		}

		public void TestTaxID_AccountingFactory_ImplementsITaxIDMacroDataProvider_BranchOrgProxy()
		{
			var codeType = "TST";
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var codeTypeLabel = "dummyOrgTaxRegistrationPrefix #:";

			SetupMockDataForTaxID(isITaxIDMacroDataProviderImplemented: true, returnValidTaxIDMacroData: true, taxIDMacroDataToReturn: new TaxIDMacroData(codeTypeLabel, codeType));
			SetupTransactionOrgProxiesForTaxIDTest(isTransactionBranchOrgProxyExist: true);

			var accountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(InvoicingBase.Company.GC_RN_NKCountryCode);
			AssertNotNull("Pre-condition: AccountingCountryFactory", accountingCountryFactory);
			AssertNotNull("Pre-condition: ITaxIDMacroDataProvider implementation", accountingCountryFactory as IInstanceProvider<ITaxIDMacroDataProvider>);
			AssertNotNull("Pre-condition: Country specific TaxID Macro data object", (accountingCountryFactory as IInstanceProvider<ITaxIDMacroDataProvider>).Get());

			AssertNull("Pre-condition: Tax Branch Organization Proxy", InvoicingBase.TaxBranch.OrgProxy);
			AssertNotNull("Pre-condition: Transaction Branch Organization Proxy", InvoicingBase.Branch.OrgProxy);
			AssertNotNull("Pre-condition: Company Organization Proxy", InvoicingBase.Company.OrgProxy);

			var companyOrgProxyCode = "3333333333";
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, codeType, countryCode, companyOrgProxyCode);

			AccountingConfigurationRegistry.Instance.DisplayTaxRegistrationNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var wrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Transaction Branch Org proxy exists but has no customCode", string.Empty, wrapper.TaxId);

			var transactionHeaderBranchOrgProxyCode = "5555555555";
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Branch.OrgProxy, codeType, countryCode, transactionHeaderBranchOrgProxyCode);

			wrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Transaction Branch Org proxy exists and has customCode", codeTypeLabel + transactionHeaderBranchOrgProxyCode, wrapper.TaxId);
		}

		public void TestTaxID_AccountingFactory_ImplementsITaxIDMacroDataProvider_CompanyOrgProxy()
		{
			var codeType = "TST";
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var codeTypeLabel = "dummyOrgTaxRegistrationPrefix #:";

			SetupMockDataForTaxID(isITaxIDMacroDataProviderImplemented: true, returnValidTaxIDMacroData: true, taxIDMacroDataToReturn: new TaxIDMacroData(codeTypeLabel, codeType));
			SetupTransactionOrgProxiesForTaxIDTest();

			var accountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(InvoicingBase.Company.GC_RN_NKCountryCode);
			AssertNotNull("Pre-condition: AccountingCountryFactory", accountingCountryFactory);
			AssertNotNull("Pre-condition: ITaxIDMacroDataProvider implementation", accountingCountryFactory as IInstanceProvider<ITaxIDMacroDataProvider>);
			AssertNotNull("Pre-condition: Country specific TaxID Macro data object", (accountingCountryFactory as IInstanceProvider<ITaxIDMacroDataProvider>).Get());

			AssertNull("Pre-condition: Tax Branch Organization Proxy", InvoicingBase.TaxBranch.OrgProxy);
			AssertNull("Pre-condition: Transaction Branch Organization Proxy", InvoicingBase.Branch.OrgProxy);
			AssertNotNull("Pre-condition: Company Organization Proxy", InvoicingBase.Company.OrgProxy);

			AccountingConfigurationRegistry.Instance.DisplayTaxRegistrationNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var wrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Transaction Company Org proxy exists but has no customCode", string.Empty, wrapper.TaxId);

			var companyOrgProxyCode = "3333333333";
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, codeType, countryCode, companyOrgProxyCode);

			wrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Transaction Company Org proxy exists and has customCode", codeTypeLabel + companyOrgProxyCode, wrapper.TaxId);
		}

		public void TestTaxID_GetTaxBranchOrgProxyRegistrationNumberWithFallback_UsesInvoiceCountry()
		{
			var codeType = "TST";
			var countryCode1 = CountryCodes.Andorra;
			var countryCode2 = CountryCodes.Brazil;
			var codeTypeLabel = "dummyOrgTaxRegistrationPrefix #:";

			SetupMockDataForTaxID(isITaxIDMacroDataProviderImplemented: true, returnValidTaxIDMacroData: true, taxIDMacroDataToReturn: new TaxIDMacroData(codeTypeLabel, codeType));
			SetupTransactionOrgProxiesForTaxIDTest();

			var accountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(InvoicingBase.Company.GC_RN_NKCountryCode);
			AssertNotNull("Pre-condition: AccountingCountryFactory", accountingCountryFactory);
			AssertNotNull("Pre-condition: ITaxIDMacroDataProvider implementation", accountingCountryFactory as IInstanceProvider<ITaxIDMacroDataProvider>);
			AssertNotNull("Pre-condition: Country specific TaxID Macro data object", (accountingCountryFactory as IInstanceProvider<ITaxIDMacroDataProvider>).Get());

			AssertNull("Pre-condition: Tax Branch Organization Proxy", InvoicingBase.TaxBranch.OrgProxy);
			AssertNull("Pre-condition: Transaction Branch Organization Proxy", InvoicingBase.Branch.OrgProxy);
			AssertNotNull("Pre-condition: Company Organization Proxy", InvoicingBase.Company.OrgProxy);

			AccountingConfigurationRegistry.Instance.DisplayTaxRegistrationNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var companyOrgProxyCode1 = "3333333333";
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, codeType, countryCode1, companyOrgProxyCode1);
			var companyOrgProxyCode2 = "5555555555";
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, codeType, countryCode2, companyOrgProxyCode2);

			InvoicingBase.Company.GC_RN_NKCountryCode = countryCode1;
			var wrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("When Invoice has CountryCode1 and transaction Company Org proxy exists and has customCode", codeTypeLabel + companyOrgProxyCode1, wrapper.TaxId);

			InvoicingBase.Company.GC_RN_NKCountryCode = countryCode2;
			wrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("When Invoice has CountryCode2 and transaction Company Org proxy exists and has customCode", codeTypeLabel + companyOrgProxyCode2, wrapper.TaxId);
		}

		public void TestTaxIDWithExtraOrgTaxRegistrationPrefix_AccountingFactory_ImplementsITaxIDMacroDataProvider_TaxBranchOrgProxy()
		{
			var codeType = "TST";
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var codeTypeLabel = "dummyOrgTaxRegistrationPrefix #:";
			var extraCodeType = "EXT";
			var extraCodeTypeLabel = "dummyExtraOrgTaxRegistrationPrefix #:";

			SetupMockDataForTaxID(isITaxIDMacroDataProviderImplemented: true, returnValidTaxIDMacroData: true, taxIDMacroDataToReturn: new TaxIDMacroData(codeTypeLabel, codeType, extraCodeTypeLabel, extraCodeType));
			SetupTransactionOrgProxiesForTaxIDTest(true, true);

			AssertNotNull("Pre-condition: Tax Branch Organization Proxy", InvoicingBase.TaxBranch.OrgProxy);
			AssertNotNull("Pre-condition: Transaction Branch Organization Proxy", InvoicingBase.Branch.OrgProxy);
			AssertNotNull("Pre-condition: Company Organization Proxy", InvoicingBase.Company.OrgProxy);

			var companyOrgProxyCode = "3333333333";
			var transactionHeaderBranchOrgProxyCode = "5555555555";
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, codeType, countryCode, companyOrgProxyCode);
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, extraCodeType, countryCode, companyOrgProxyCode);

			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Branch.OrgProxy, codeType, countryCode, transactionHeaderBranchOrgProxyCode);
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Branch.OrgProxy, extraCodeType, countryCode, transactionHeaderBranchOrgProxyCode);

			var taxBranchOrgProxyCode = "7777777777";
			var taxBranchOrgProxyExtraCode = "9999999999";
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.TaxBranch.OrgProxy, codeType, countryCode, taxBranchOrgProxyCode);
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.TaxBranch.OrgProxy, extraCodeType, countryCode, taxBranchOrgProxyExtraCode);

			var wrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Tax Branch Org proxy exists and has extra customCode", "dummyOrgTaxRegistrationPrefix #:7777777777 dummyExtraOrgTaxRegistrationPrefix #:9999999999", wrapper.TaxId);
		}

		public void TestTaxIDWithExtraOrgTaxRegistrationPrefix_AccountingFactory_ImplementsITaxIDMacroDataProvider_BranchOrgProxy()
		{
			var codeType = "TST";
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var codeTypeLabel = "dummyOrgTaxRegistrationPrefix #:";
			var extraCodeType = "EXT";
			var extraCodeTypeLabel = "dummyExtraOrgTaxRegistrationPrefix #:";

			SetupMockDataForTaxID(isITaxIDMacroDataProviderImplemented: true, returnValidTaxIDMacroData: true, taxIDMacroDataToReturn: new TaxIDMacroData(codeTypeLabel, codeType, extraCodeTypeLabel, extraCodeType));
			SetupTransactionOrgProxiesForTaxIDTest(isTransactionBranchOrgProxyExist: true);

			AssertNull("Pre-condition: Tax Branch Organization Proxy", InvoicingBase.TaxBranch.OrgProxy);
			AssertNotNull("Pre-condition: Transaction Branch Organization Proxy", InvoicingBase.Branch.OrgProxy);
			AssertNotNull("Pre-condition: Company Organization Proxy", InvoicingBase.Company.OrgProxy);

			var companyOrgProxyCode = "3333333333";
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, codeType, countryCode, companyOrgProxyCode);
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, extraCodeType, countryCode, companyOrgProxyCode);
			var transactionHeaderBranchOrgProxyCode = "5555555555";
			var transactionHeaderBranchOrgProxyExtraCode = "8888888888";
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Branch.OrgProxy, codeType, countryCode, transactionHeaderBranchOrgProxyCode);
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Branch.OrgProxy, extraCodeType, countryCode, transactionHeaderBranchOrgProxyExtraCode);

			var wrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Transaction Branch Org proxy exists and has extra customCode", "dummyOrgTaxRegistrationPrefix #:5555555555 dummyExtraOrgTaxRegistrationPrefix #:8888888888", wrapper.TaxId);
		}

		public void TestTaxIDWithExtraOrgTaxRegistrationPrefix_AccountingFactory_ImplementsITaxIDMacroDataProvider_CompanyOrgProxy()
		{
			var codeType = "TST";
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var codeTypeLabel = "dummyOrgTaxRegistrationPrefix #:";
			var extraCodeType = "EXT";
			var extraCodeTypeLabel = "dummyExtraOrgTaxRegistrationPrefix #:";

			SetupMockDataForTaxID(isITaxIDMacroDataProviderImplemented: true, returnValidTaxIDMacroData: true, taxIDMacroDataToReturn: new TaxIDMacroData(codeTypeLabel, codeType, extraCodeTypeLabel, extraCodeType));
			SetupTransactionOrgProxiesForTaxIDTest();

			AssertNull("Pre-condition: Tax Branch Organization Proxy", InvoicingBase.TaxBranch.OrgProxy);
			AssertNull("Pre-condition: Transaction Branch Organization Proxy", InvoicingBase.Branch.OrgProxy);
			AssertNotNull("Pre-condition: Company Organization Proxy", InvoicingBase.Company.OrgProxy);

			var companyOrgProxyCode = "3333333333";
			var companyOrgProxyExtraCode = "5555555555";
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, codeType, countryCode, companyOrgProxyCode);
			TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, extraCodeType, countryCode, companyOrgProxyExtraCode);

			var wrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Transaction Company Org proxy exists and has extra customCode", "dummyOrgTaxRegistrationPrefix #:3333333333 dummyExtraOrgTaxRegistrationPrefix #:5555555555", wrapper.TaxId);
		}

		void SetupMockDataForTaxID(bool isITaxIDMacroDataProviderImplemented = true, bool returnValidTaxIDMacroData = true, TaxIDMacroData taxIDMacroDataToReturn = null)
		{
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();

			if (isITaxIDMacroDataProviderImplemented)
			{
				var mockITaxIDMacroDataProvider = new Mock<ITaxIDMacroDataProvider>();
				mockITaxIDMacroDataProvider.Setup(x => x.GetTaxIDMacroData()).Returns(taxIDMacroDataToReturn);
				mockIAccountingCountryFactory.As<IInstanceProvider<ITaxIDMacroDataProvider>>().Setup(x => x.Get()).Returns(returnValidTaxIDMacroData ? mockITaxIDMacroDataProvider.Object : null);
			}

			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);
		}

		#endregion

		public void TestTaxIDForAustralia()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Australia, "ABN: ", false);
		}

		public void TestTaxIDForSouthAfrica()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.SouthAfrica, " #: ");
		}

		public void TestTaxIdForBangladesh()
		{
			CoreTestForTaxID(Constants.CountryCodes.Bangladesh, "BIN #: ", false);
		}

		public void TestTaxIdForPapuaNewGuinea()
		{
			CoreTestForTaxID(Constants.CountryCodes.PapuaNewGuinea, "TIN #: ", false);
		}

		public virtual void TestPapuaNewGuineaRecipientTaxIDBehaviours()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				OrgHeader header = Factory.New<OrgHeader>();
				header.OH_RL_NKClosestPort = "GBLON";
				header.LocalBusinessRegNo = "ABC123";
				OrgCusCode taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				taxCode.OK_CodeType = Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.UnitedKingdom);
				taxCode.OK_CustomsRegNo = "123456";

				AssertEquals("PreCondition: Local Business Reg Number", "ABC123", header.LocalBusinessRegNo);

				taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				taxCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
				taxCode.OK_CustomsRegNo = "VAT12345";

				Invoice.AH_OH = header.PK;
				InvoiceBatch = Factory.New<InvoiceBatchHeader>();
				InvoiceBatch.AH_OH = header.PK;

				var transactionLine = (AccTransactionLines)InvoicingBase.Lines.AddNew();
				transactionLine.AL_AT = Factory.New<AccTaxRate>().PK;

				InvoiceBatch.Line.Add(InvoicingBase);

				InvoiceWrapper = GetBaseInvoiceWrapper();

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.PapuaNewGuinea);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a Papua New Guinea Company should be empty by default", ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax Id Number for a Papua New Guinea Company should be empty by default", ZString.Empty, InvoiceWrapper.RecipientTaxIDNumber);

				using (AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					header.ResetCodeForTaxRegistration_ForTestOnly();
					AssertEquals("Recipient Tax ID Heading for a Papua New Guinea Company", "Client TIN #:", InvoiceWrapper.RecipientTaxIDHeading);
					AssertEquals("Recipient Tax Id Number for a Papua New Guinea Company", header.LocalVATCode, InvoiceWrapper.RecipientTaxIDNumber);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public void TestTaxIDForMongolia()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Mongolia, " #: ");
		}

		public void TestTaxIDForSingapore()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Singapore, " Reg No: ");
		}

		public void TestTaxIDForIndia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var india = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.India);
				var branch = TestObjectCreator.CreateBranch("TBR", GlbCompany.CurrentCompany, TestObjectCreator.ABIGAS);
				InvoicingBase.AH_GB = branch.PK;
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);

				AssertEquals("TaxID", ZString.Empty, aRInvoiceWrapper.TaxId);

				InvoicingLineBase invoiceLine = InvoicingBase.Lines.AddNew() as InvoicingLineBase;
				invoiceLine.AL_AT = TestObjectCreator.SERANDEDU1.PK;

				string expectedTaxID = "Service Tax Registration No: " + GlbCompany.CurrentCompany.GC_BusinessRegNo;
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				Factory.ClearCachedValue<ZBool>(InvoicingBase.PK.ToStringKey());
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				invoiceLine.AL_AT = TestObjectCreator.STAGST.PK;

				expectedTaxID = "ABI GAS & TOOLS";
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				TestObjectCreator.ABIGAS.SetCustomsCode("PAN", india, "GST456");

				expectedTaxID = "PAN: GST456, ABI GAS & TOOLS";
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				TestObjectCreator.ABIGAS.SetCustomsCode("GST", india, "GST123");

				TestObjectCreator.ABIGAS.ResetCodeForTaxRegistration_ForTestOnly();

				expectedTaxID = "GSTIN: GST123, PAN: GST456, ABI GAS & TOOLS";
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				//Check RVS
				var rsvTaxRate_Service = TestObjectCreator.CreateTaxRate("RSV", "Reverse Rated", AccTaxRate.Types.ReverseRated, 0, AccTaxRate.ExtraTypes.ServiceTax, 0, 1);
				invoiceLine.AL_AT = rsvTaxRate_Service.PK;

				expectedTaxID = "Service Tax Registration No: 41 065 894 724";
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				var rsvTaxRate = TestObjectCreator.CreateTaxRate("RSV", "Reverse Rated", AccTaxRate.Types.ReverseRated, 0, string.Empty, 0, 1);
				invoiceLine.AL_AT = rsvTaxRate.PK;

				expectedTaxID = "GSTIN: GST123, PAN: GST456, ABI GAS & TOOLS";
				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				//Check RVS + STA
				rsvTaxRate = TestObjectCreator.CreateTaxRate("RSV", "Reverse Rated", AccTaxRate.Types.ReverseRated, 0, AccTaxRate.ExtraTypes.StateGST, 0, 1);
				invoiceLine.AL_AT = rsvTaxRate.PK;

				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				//Check NOT
				var notTaxRate_Service = TestObjectCreator.CreateTaxRate("NOT", "Not Reported", AccTaxRate.Types.NotReportable, 0, AccTaxRate.ExtraTypes.ServiceTax, 0, 1);
				invoiceLine.AL_AT = notTaxRate_Service.PK;

				expectedTaxID = "Service Tax Registration No: 41 065 894 724";
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				var notTaxRate = TestObjectCreator.CreateTaxRate("NOT", "Not Reported", AccTaxRate.Types.NotReportable, 0, string.Empty, 0, 1);
				invoiceLine.AL_AT = notTaxRate.PK;

				expectedTaxID = "GSTIN: GST123, PAN: GST456, ABI GAS & TOOLS";
				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				//Check NOT + STA
				notTaxRate = TestObjectCreator.CreateTaxRate("NOT", "Not Reported", AccTaxRate.Types.NotReportable, 0, AccTaxRate.ExtraTypes.StateGST, 0, 1);
				invoiceLine.AL_AT = notTaxRate.PK;

				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				//Check EXT
				var extRaxRate_Service = TestObjectCreator.CreateTaxRate("EXT", "Exempt", AccTaxRate.Types.Exempt, 0, AccTaxRate.ExtraTypes.ServiceTax, 0, 1);
				invoiceLine.AL_AT = extRaxRate_Service.PK;

				expectedTaxID = "Service Tax Registration No: 41 065 894 724";
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				var extTaxRate = TestObjectCreator.CreateTaxRate("EXT", "Exempt", AccTaxRate.Types.Exempt, 0, string.Empty, 0, 1);
				invoiceLine.AL_AT = extTaxRate.PK;

				expectedTaxID = "GSTIN: GST123, PAN: GST456, ABI GAS & TOOLS";
				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				//Check EXT + STA
				extTaxRate = TestObjectCreator.CreateTaxRate("EXT", "Exempt", AccTaxRate.Types.Exempt, 0, AccTaxRate.ExtraTypes.StateGST, 0, 1);
				invoiceLine.AL_AT = extTaxRate.PK;

				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);
			}
		}

		public void TestTaxIDForGCC6Countries()
		{
			var refCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedArabEmirates);
			var branch = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);

			var companyOrgProxy = Invoice.Company.OrgProxy;
			companyOrgProxy.OH_Code = "OH123";
			companyOrgProxy.OH_RL_NKClosestPort = "INBOM";
			companyOrgProxy.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, refCountry, "123");

			var branchOrgProxy = Factory.New<OrgHeader>();
			branchOrgProxy.OH_Code = "OH456";
			branchOrgProxy.OH_RL_NKClosestPort = "INBOM";
			branchOrgProxy.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, refCountry, "456");

			Factory.Save();

			InvoicingBase.AH_GB = branch.PK;
			InvoicingBase.AH_OH = companyOrgProxy.PK;
			var invoiceLine = InvoicingBase.Lines.AddNew() as InvoicingLineBase;
			invoiceLine.AL_AT = TestObjectCreator.SERANDEDU1.PK;

			var arInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedArabEmirates))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);
				AssertEquals("UnitedArabEmirates TaxID from branch since branch org proxy exists", "VAT #: 456", arInvoiceWrapper.TaxId);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("UnitedArabEmirates TaxID from company since company org proxy exists", "VAT #: 123", arInvoiceWrapper.TaxId);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Bahrain))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);
				AssertEquals("Bahrain TaxID from branch since branch org proxy exists", "VAT #: 456", arInvoiceWrapper.TaxId);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("Bahrain TaxID from company since company org proxy exists", "VAT #: 123", arInvoiceWrapper.TaxId);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Kuwait))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);
				AssertEquals("Kuwait TaxID from branch since branch org proxy exists", "VAT #: 456", arInvoiceWrapper.TaxId);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("Kuwait TaxID from company since company org proxy exists", "VAT #: 123", arInvoiceWrapper.TaxId);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Oman))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);
				AssertEquals("Oman TaxID from branch since branch org proxy exists", "VAT #: 456", arInvoiceWrapper.TaxId);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("Oman TaxID from company since company org proxy exists", "VAT #: 123", arInvoiceWrapper.TaxId);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Qatar))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);
				AssertEquals("Qatar TaxID from branch since branch org proxy exists", "VAT #: 456", arInvoiceWrapper.TaxId);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("Qatar TaxID from company since company org proxy exists", "VAT #: 123", arInvoiceWrapper.TaxId);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SaudiArabia))
			{
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				AssertNotNull("Precondition", branch.OrgProxy);
				AssertEquals("SaudiArabia TaxID from branch since branch org proxy exists", "VAT #: 456", arInvoiceWrapper.TaxId);

				branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("Precondition", branch.OrgProxy);
				AssertEquals("SaudiArabia TaxID from company since company org proxy exists", "VAT #: 123", arInvoiceWrapper.TaxId);
			}
		}

		public void TestTaxIDForAndorra_TaxBranchOrgProxy()
		{
			var countryCode = CountryCodes.Andorra;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				if (InvoicingBase is ARInvoice)
				{
					var codeType = "IGI";
					var codeTypeLabel = "NRT #: ";

					SetupTransactionOrgProxiesForTaxIDTest(isTransactionBranchOrgProxyExist: true, isTaxBranchOrgProxyExist: true);

					AssertNotNull("Pre-condition: Tax Branch Organization Proxy", InvoicingBase.TaxBranch.OrgProxy);
					AssertNotNull("Pre-condition: Transaction Branch Organization Proxy", InvoicingBase.Branch.OrgProxy);
					AssertNotNull("Pre-condition: Company Organization Proxy", InvoicingBase.Company.OrgProxy);

					var companyOrgProxyCode = "3333333333";
					var transactionHeaderBranchOrgProxyCode = "5555555555";
					TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, codeType, countryCode, companyOrgProxyCode);
					TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Branch.OrgProxy, codeType, countryCode, transactionHeaderBranchOrgProxyCode);

					var wrapper = DocARInvoice.New(InvoicingBase, Factory);
					AssertEquals("Tax Branch Org proxy exists but has no customCode", string.Empty, wrapper.TaxId);

					var taxBranchOrgProxyCode = "7777777777";
					TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.TaxBranch.OrgProxy, codeType, countryCode, taxBranchOrgProxyCode);

					wrapper = DocARInvoice.New(InvoicingBase, Factory);
					AssertEquals("Tax Branch Org proxy exists and has customCode", codeTypeLabel + taxBranchOrgProxyCode, wrapper.TaxId);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestTaxIDForAndorra_TransactionBranchOrgProxy()
		{
			var countryCode = CountryCodes.Andorra;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				if (InvoicingBase is ARInvoice)
				{
					var codeType = "IGI";
					var codeTypeLabel = "NRT #: ";

					SetupTransactionOrgProxiesForTaxIDTest(isTransactionBranchOrgProxyExist: true);

					AssertNull("Pre-condition: Tax Branch Organization Proxy", InvoicingBase.TaxBranch.OrgProxy);
					AssertNotNull("Pre-condition: Transaction Branch Organization Proxy", InvoicingBase.Branch.OrgProxy);
					AssertNotNull("Pre-condition: Company Organization Proxy", InvoicingBase.Company.OrgProxy);

					var companyOrgProxyCode = "3333333333";
					TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, codeType, countryCode, companyOrgProxyCode);

					var wrapper = DocARInvoice.New(InvoicingBase, Factory);
					AssertEquals("Transaction Branch Org proxy exists but has no customCode", string.Empty, wrapper.TaxId);

					var transactionHeaderBranchOrgProxyCode = "5555555555";
					TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Branch.OrgProxy, codeType, countryCode, transactionHeaderBranchOrgProxyCode);

					wrapper = DocARInvoice.New(InvoicingBase, Factory);
					AssertEquals("Transaction Branch Org proxy exists and has customCode", codeTypeLabel + transactionHeaderBranchOrgProxyCode, wrapper.TaxId);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestTaxIDForAndorra_CompanyOrgProxy()
		{
			var countryCode = CountryCodes.Andorra;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				if (InvoicingBase is ARInvoice)
				{
					var codeType = "IGI";
					var codeTypeLabel = "NRT #: ";

					SetupTransactionOrgProxiesForTaxIDTest();

					AssertNull("Pre-condition: Tax Branch Organization Proxy", InvoicingBase.TaxBranch.OrgProxy);
					AssertNull("Pre-condition: Transaction Branch Organization Proxy", InvoicingBase.Branch.OrgProxy);
					AssertNotNull("Pre-condition: Company Organization Proxy", InvoicingBase.Company.OrgProxy);

					var wrapper = DocARInvoice.New(InvoicingBase, Factory);
					AssertEquals("Transaction Company Org proxy exists but has no customCode", string.Empty, wrapper.TaxId);

					var companyOrgProxyCode = "3333333333";
					TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, codeType, countryCode, companyOrgProxyCode);

					wrapper = DocARInvoice.New(InvoicingBase, Factory);
					AssertEquals("Transaction Company Org proxy exists and has customCode", codeTypeLabel + companyOrgProxyCode, wrapper.TaxId);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestTaxIDForFiji()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Fiji, "TIN: ", false);
		}

		public void TestTaxIDForLaoPeoplesDemocraticRepublic()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.LaoPeoplesDemocraticRepublic, "VAT #: ", false);
		}

		public void TestTaxIDForMaldives()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Maldives, "TIN: ", false);
		}

		public void TestTaxIDForTonga()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Tonga, "TIN: ", false);
		}

		public void TestTaxIDForNetherlands()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Netherlands, " #: " + Core.Constants.CountryCodes.Netherlands);
		}

		public void TestTaxIDForVanuatu()
		{
			AssertSingleTaxIdForARInvoice(Constants.CountryCodes.Vanuatu, OrgCusCode.CodeTypes.VATCode, "TIN #: ");
		}

		public void TestTaxIDForVietnam()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.VietNam, "MST #: ", false);
		}

		public void TestTaxIDForPeru()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Peru, "R.U.C. #: ", false);
		}

		public void TestTaxIDForGermany()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Germany, "VAT ID No: " + Core.Constants.CountryCodes.Germany, false);
		}

		public void TestTaxIDForColombia()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Colombia, "NIT #: ", false);
		}

		public void TestTaxIDForGuatemala()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Guatemala, "NIT #: ", false);
		}

		public void TestTaxIDForVenezuela()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Venezuela, "RIF #: ", false);
		}

		public void TestTaxIDForNigeria()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Nigeria, "TIN: ", false);
		}

		public void TestTaxIDForEcuador()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Ecuador, "RUC #: ", false);
		}

		public void TestTaxIDForPuertoRico()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.PuertoRico, "NRC #: ", false);
		}

		public void TestTaxIDForParaguay()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Paraguay, "RUC #: ", false);
		}

		public void TestTaxIDForUruguay()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Uruguay, "RUT #: ", false);
		}

		public void TestTaxIDForBolivia()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Bolivia, "NIT #: ", false);
		}

		public void TestTaxIDForHonduras()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Honduras, "RTN #: ", false);
		}

		public void TestTaxIDForFrenchPolynesia()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.FrenchPolynesia, "TAHITI #: ", false);
		}

		public void TestTaxIDForNicaragua()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Nicaragua, "RUC #: ", false);
		}

		public void TestTaxIDForMali()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Mali, "NIF #: ", false);
		}

		public void TestTaxIDForEquatorialGuinea()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.EquatorialGuinea, "NIF #: ", false);
		}

		public void TestTaxIDForLebanon()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Lebanon, "VAT #: ", false);
		}

		public void TestTaxIDForSenegal()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Senegal, "NINEA #: ", false);
		}

		public void TestTaxIDForCoteDivoire()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.CoteDivoire, "CC #: ", false);
		}

		public void TestTaxIDForCameroon()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Cameroon, "NIU #: ", false);
		}

		public void TestTaxIDForMozambique()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Mozambique, "NUIT #: ", false);
		}

		public void TestTaxIDForDominicanRepublic()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.DominicanRepublic, "RNC #: ", false);
		}

		public void TestTaxIDForYemen()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Yemen, "GST #: ", false);
		}

		public void TestTaxIDForPanama()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Panama, "RUC #: ", false);
		}

		public void TestTaxIDForAlgeria()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Algeria, "NIF #: ", false);
		}

		public void TestTaxIDForMalawi()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Malawi, "TPIN #: ", false);
		}

		public void TestTaxIDForNiger()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Niger, "NIF #: ", false);
		}

		public void TestTaxIDForTurkey()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Turkey, "TIN: ", false);
		}

		public void TestTaxIDForCostaRica()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.CostaRica, "CÉDULA JURÍDICA #: ", false);
		}

		public void TestTaxIDForGhana()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Ghana, "TIN #: ", false);
		}

		public void TestTaxIDForBelarus()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Belarus, "TIN #: ", false);
		}

		public void TestTaxIDForMalta()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Malta, "VAT #: " + Core.Constants.CountryCodes.Malta, false);
		}

		public void TestTaxIDForSierraLeone()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.SierraLeone, "TIN #: ", false);
		}

		public void TestTaxIDForCambodia()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Cambodia, "VATTIN #: ", false);
		}

		public void TestTaxIDForKiribati()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Kiribati, "TIN #: ", false);
		}

		public void TestTaxIDForRussia()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Russia, "VAT #: ", false);
		}

		public void TestTaxIDForNepal()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Nepal, "VAT #: ", false);
		}

		public void TestTaxIDForIran()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Iran, "VAT #: ", false);
		}

		public void TestTaxIDForGeorgia()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Georgia, "VAT #: ", false);
		}

		public void TestTaxIDForBarbados()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Barbados, "TIN #: ", false);
		}

		public void TestTaxIDForJamaica()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Jamaica, "GCT #: ", false);
		}

		public void TestTaxIDForTrinidadAndTobago()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.TrinidadAndTobago, "VAT #: ", false);
		}

		public void TestTaxIDForCroatia()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Croatia, "VAT #: " + Core.Constants.CountryCodes.Croatia, false);
		}

		public void TestTaxIDForMacedonia()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Macedonia, "VAT #: ", false);
		}

		public void TestTaxIDForRwanda()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.Rwanda, "TIN #: ", false);
		}

		public void TestTaxIDForNewCaledonia()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.NewCaledonia, "TGC #: ", false);
		}

		public void TestTaxIDForPortugal_ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				AccountingConfigurationRegistry.Instance.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var invoicingBase = Factory.New<ARInvoice>();
				var docAddress = invoicingBase.DocAddresses.AddNew(DocAddressType.BranchOrCompanyProxyARAdress);
				docAddress.E2_AddressOverride = true;
				docAddress.E2_CompanyName = GlbCompany.CurrentCompany.CompanyName;
				docAddress.E2_GovRegNum = "Branch Or Company Proxy AR Adress";

				var invoiceWrapper = DocARInvoice.New(invoicingBase, Factory);

				AssertEquals("Branch Or Company Proxy AR Adress", invoiceWrapper.TaxId);
			}
		}

		public void TestTranslatedTaxCode()
		{
			ZString currentCountryCode = GlbCompany.CurrentCompany.Country.Code;

			try
			{
				string countryCode = Core.Constants.CountryCodes.Canada;
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
				GlbCompany.CurrentCompany.SetCountry(countryCode);
				AssertEquals("TranslatedTaxCode", "GST", DocARInvoiceCommon.TranslatedTaxCode);

				using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.French))
				{
					AssertEquals("TranslatedTaxCode", "TPS", DocARInvoiceCommon.TranslatedTaxCode);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		public void TestTaxIDForGreeceIncludesDOYCode()
		{
			ZString currentCountryCode = GlbCompany.CurrentCompany.Country.Code;

			try
			{
				string countryCode = Core.Constants.CountryCodes.UnitedKingdom;
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
				GlbCompany.CurrentCompany.SetCountry(countryCode);
				AssertEquals("TaxID", ZString.Empty, aRInvoiceWrapper.TaxId);

				InvoicingLineBase invoiceLine = InvoicingBase.Lines.AddNew() as InvoicingLineBase;
				invoiceLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				invoiceLine.TaxRate.AT_Type = AccTaxRate.Types.Rated;

				string expectedTaxID = "VAT #: GB" + GlbCompany.CurrentCompany.GC_BusinessRegNo;
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				Factory.ClearCachedValue<ZBool>(InvoicingBase.PK.ToStringKey());
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "GRAGO";
				OrgCusCode taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Greece;
				taxCode.OK_CodeType = OrgCusCode.GreeceCodeTypes.DOY;
				taxCode.OK_CustomsRegNo = "345543";
				expectedTaxID += " DOY: 345543";
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				countryCode = Core.Constants.CountryCodes.Greece;
				expectedTaxID = "VAT #: EL" + GlbCompany.CurrentCompany.GC_BusinessRegNo;
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				GlbCompany.CurrentCompany.SetCountry(countryCode);
				expectedTaxID += " DOY: 345543";
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		public void TestTaxIDForTurkeyIncludesVDMCode()
		{
			ZString currentCountryCode = GlbCompany.CurrentCompany.Country.Code;

			try
			{
				string countryCode = Core.Constants.CountryCodes.Turkey;
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
				GlbCompany.CurrentCompany.SetCountry(countryCode);
				AssertEquals("TaxID", ZString.Empty, aRInvoiceWrapper.TaxId);

				InvoicingLineBase invoiceLine = InvoicingBase.Lines.AddNew() as InvoicingLineBase;
				invoiceLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				invoiceLine.TaxRate.AT_Type = AccTaxRate.Types.Rated;

				string expectedTaxID = "TIN: " + GlbCompany.CurrentCompany.GC_BusinessRegNo;
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				Factory.ClearCachedValue<ZBool>(InvoicingBase.PK.ToStringKey());
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "TRABK";
				OrgCusCode taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Turkey;
				taxCode.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.VDM;
				taxCode.OK_CustomsRegNo = "345543";
				expectedTaxID += " TAX OFFICE: 345543";
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		public void TestTaxIDForCanadaQuebecQST()
		{
			ZString currentCountryCode = GlbCompany.CurrentCompany.Country.Code;
			OrgCusCode forFinalDispose1 = null;
			OrgCusCode forFinalDispose2 = null;

			try
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
				AssertEquals("TaxID", ZString.Empty, aRInvoiceWrapper.TaxId);

				InvoicingLineBase invoiceLine = InvoicingBase.Lines.AddNew() as InvoicingLineBase;
				invoiceLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				invoiceLine.TaxRate.AT_Type = AccTaxRate.Types.Rated;

				string taxCode = GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription;
				string expectedTaxID = taxCode + " #: " + GlbCompany.CurrentCompany.GC_BusinessRegNo;
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				Factory.ClearCachedValue<ZBool>(InvoicingBase.PK.ToStringKey());
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				invoiceLine.TaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				string expectedQSTRegistrationNumber = "QST123456";
				forFinalDispose1 = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.QuebecSalesTaxID, expectedQSTRegistrationNumber, Factory.Load<RefCountry>(Core.CountryGuids.Instance.Canada));
				expectedTaxID = taxCode + " #: " + GlbCompany.CurrentCompany.GC_BusinessRegNo + "  QST #: " + expectedQSTRegistrationNumber;
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				invoiceLine.TaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				forFinalDispose2 = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.QuebecSalesTaxID, expectedQSTRegistrationNumber, Factory.Load<RefCountry>(Core.CountryGuids.Instance.Canada));
				expectedTaxID = taxCode + " #: " + GlbCompany.CurrentCompany.GC_BusinessRegNo + "  QST #: " + expectedQSTRegistrationNumber;
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);
			}
			finally
			{
				if (forFinalDispose1 != null)
				{
					forFinalDispose1.Delete();
				}
				if (forFinalDispose2 != null)
				{
					forFinalDispose2.Delete();
				}
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		public void TestTaxIDForMalaysia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malaysia))
			{
				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				orgProxy.OH_RL_NKClosestPort = "MYABU";

				var taxCode1 = orgProxy.CustomsCodes.AddNew();
				taxCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Malaysia;
				taxCode1.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
				taxCode1.OK_CustomsRegNo = "PXYGST123456";

				var taxCode2 = orgProxy.CustomsCodes.AddNew();
				taxCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Malaysia;
				taxCode2.OK_CodeType = MalaysiaOrgCusCodeInfo.OrgCusCodes.SER;
				taxCode2.OK_CustomsRegNo = "PXYSER123456";

				var taxCode3 = orgProxy.CustomsCodes.AddNew();
				taxCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				taxCode3.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
				taxCode3.OK_CustomsRegNo = "PXYGSTTW1234";

				var header = CreateOrgHeaderForMalaysia();
				Invoice.AH_OH = header.PK;

				var taxRate1 = Factory.New<AccTaxRate>();
				var taxRate2 = Factory.New<AccTaxRate>();

				InvoicingBase.Lines.AddNew().AL_AT = taxRate1.PK;
				InvoicingBase.Lines.AddNew().AL_AT = taxRate2.PK;

				var invoiceWrapper = DocARInvoice.New(Invoice, Factory);

				taxRate1.AT_Code = "GST";
				taxRate1.AT_ExtraTaxRateType = ZString.Empty;
				taxRate2.AT_Code = "GST";
				taxRate2.AT_ExtraTaxRateType = ZString.Empty;
				AssertEquals("REGISTRATION #: PXYGST123456", invoiceWrapper.TaxId);

				taxRate1.AT_Code = "GST";
				taxRate1.AT_ExtraTaxRateType = ZString.Empty;
				taxRate2.AT_Code = "SVC";
				taxRate2.AT_ExtraTaxRateType = "SER";
				AssertEquals("REGISTRATION #: PXYGST123456", invoiceWrapper.TaxId);

				taxRate1.AT_Code = "SVC";
				taxRate1.AT_ExtraTaxRateType = "SER";
				taxRate2.AT_Code = "SVC";
				taxRate2.AT_ExtraTaxRateType = "SER";
				AssertEquals("REGISTRATION #: PXYSER123456", invoiceWrapper.TaxId);

				taxRate1.AT_Code = "XXX";
				taxRate1.AT_ExtraTaxRateType = "TST";
				taxRate2.AT_Code = "XXX";
				taxRate2.AT_ExtraTaxRateType = "TST";
				AssertEquals("REGISTRATION #: ", invoiceWrapper.TaxId);
			}
		}

		public void TestTaxIDForBrexit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				GlbCompany.CurrentCompany.GC_BusinessRegNo = "1111111";

				var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
				var vat = Factory.NewWithValidTestData<AccTaxRate>();
				line.AL_AT = vat.PK;
				var wrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);

				var gbCountry = RefCountry.LoadFromCountryCode(new BusinessObjectFactory(), Constants.CountryCodes.UnitedKingdom);
				gbCountry.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
				gbCountry.Factory.Save();
				AssertEquals("Before Brexit", "VAT #: GB1111111", wrapper.TaxId);

				gbCountry.RN_EconomicGrouping = "";
				gbCountry.Factory.Save();
				AssertEquals("After Brexit", "VAT #: GB1111111", wrapper.TaxId);
			}
		}

		public void TestTaxIDForSpain()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
			{
				if (InvoicingBase is ARInvoice)
				{
					GlbCompany.CurrentCompany.GC_BusinessRegNo = "1111111";

					var branch = TestObjectCreator.CreateNewBranch(GlbCompany.GetCurrentCompany(Factory), "DPC");
					var branchOrgProxy = TestObjectCreator.AALSHI;
					branch.GB_OH_OrgProxy = branchOrgProxy.PK;
					Factory.Save();

					var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
					{
						var vat = Factory.NewWithValidTestData<AccTaxRate>();
						line.AL_AT = vat.PK;
						InvoicingBase.AH_GB = branch.PK;
						var wrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
						AssertEquals("VAT #: ES1111111", wrapper.TaxId);

						var igic = Factory.NewWithValidTestData<AccTaxRate>();
						igic.AT_RN_NKCountry = Constants.CountryCodes.Spain;
						igic.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.RegionalTax;
						line.AL_AT = igic.PK;
						wrapper = DocAPInvoice.New(InvoicingBase, Factory);
						AssertEquals("VAT #: ES1111111", wrapper.TaxId);

						var taxCode = branchOrgProxy.CustomsCodes.AddNew();
						taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Spain;
						taxCode.OK_CodeType = OrgCusCode.SpainCodeTypes.IGC;
						taxCode.OK_CustomsRegNo = "123456";
						wrapper = DocAPInvoice.New(InvoicingBase, Factory);
						AssertEquals("NIF #: 123456", wrapper.TaxId);
					}
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestTaxIDForChad()
		{
			AssertSingleTaxIdForARInvoice(Constants.CountryCodes.Chad, OrgCusCode.ChadCodeTypes.NIF, "NIF #: ");
		}

		public void TestTaxIDForPalestine()
		{
			AssertSingleTaxIdForARInvoice(Constants.CountryCodes.PalestinianTerritory, OrgCusCode.CodeTypes.VATCode, "VAT #: ");
		}

		public void TestTaxIDForSaintMartin()
		{
			AssertSingleTaxIdForARInvoice(Constants.CountryCodes.SaintMartin, SaintMartinOrgCusCodeInfo.OrgCusCodes.TGC, "TGCA #: ");
		}

		public void TestTaxIDForBurundi()
		{
			AssertSingleTaxIdForARInvoice(Constants.CountryCodes.Burundi, OrgCusCode.CodeTypes.TVACode, "TVA #: ");
		}

		public void TestTaxIDForMauritania()
		{
			AssertDualTaxIdForARInvoice(Constants.CountryCodes.Mauritania, OrgCusCode.CodeTypes.TVACode, MauritaniaOrgCusCodeInfo.OrgCusCodes.NIF, "TVA #: ", "NIF #: ");
		}

		public void TestTaxIDForGuinea()
		{
			AssertDualTaxIdForARInvoice(Constants.CountryCodes.Guinea, OrgCusCode.CodeTypes.TVACode, GuineaOrgCusCodeInfo.OrgCusCodes.NIF, "TVA #: ", "NIF #: ");
		}

		public void TestTaxIDForDjibouti()
		{
			AssertSingleTaxIdForARInvoice(Constants.CountryCodes.Djibouti, DjiboutiOrgCusCodeInfo.OrgCusCodes.NIF, "NIF #: ");
		}

		void AssertSingleTaxIdForARInvoice(string countryCode, string codeType, string codeTypeLabel)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				if (InvoicingBase is ARInvoice)
				{
					GlbCompany.CurrentCompany.GC_BusinessRegNo = "1111111";

					var transactionBranchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
					transactionBranchOrgProxy.OH_IsForwarder = true;

					var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
					branchOrgProxy.OH_IsForwarder = true;

					var invoiceBranch = Factory.NewWithValidTestData<GlbBranch>();
					invoiceBranch.GB_GC = GlbCompany.CurrentCompany.PK;
					invoiceBranch.GB_OH_OrgProxy = ZGuid.Empty;
					InvoicingBase.AH_GB = invoiceBranch.PK;
					GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgProxy.PK;
					Factory.Save();

					AssertNotEquals(GlbBranch.CurrentBranch.GB_OH_OrgProxy, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
					AssertNotEquals(GlbBranch.CurrentBranch.GB_OH_OrgProxy, InvoicingBase.Branch.GB_OH_OrgProxy);

					var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
					line.AL_AG = TestObjectCreator.GLHeader1.PK;
					var vat = Factory.NewWithValidTestData<AccTaxRate>();
					line.AL_AT = vat.PK;

					var wrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
					AssertEquals(string.Empty, wrapper.TaxId);

					var companyOrgProxyCode = "3333333333";
					var transactionHeaderBranchOrgProxyCode = "5555555555";

					TestObjectCreator.SetCustomsCodeForOrgHeader(GlbBranch.CurrentBranch.OrgProxy, codeType, countryCode, "1111111111");
					TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, codeType, countryCode, companyOrgProxyCode);
					Factory.Save();

					wrapper = DocAPInvoice.New(InvoicingBase, Factory);
					AssertEquals("if Transaction Branch org proxy doesn't exists, we fallback to company customCode", codeTypeLabel + companyOrgProxyCode, wrapper.TaxId);

					invoiceBranch.GB_OH_OrgProxy = transactionBranchOrgProxy.PK;
					wrapper = DocAPInvoice.New(InvoicingBase, Factory);
					AssertEquals("if Transaction Branch org proxy exists but has no customCode, we don't fallback to company customCode", string.Empty, wrapper.TaxId);

					TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, codeType, countryCode, companyOrgProxyCode);
					TestObjectCreator.SetCustomsCodeForOrgHeader(invoiceBranch.OrgProxy, codeType, countryCode, transactionHeaderBranchOrgProxyCode);
					Factory.Save();

					wrapper = DocAPInvoice.New(InvoicingBase, Factory);
					AssertEquals("if both Transaction Branch org proxy and company orgProxy have customCode, we use transaction branch org proxy customCode", codeTypeLabel + transactionHeaderBranchOrgProxyCode, wrapper.TaxId);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestTaxIDForBosnia()
		{
			AssertDualTaxIdForARInvoice(Constants.CountryCodes.BosniaAndHerzegovina, OrgCusCode.BosniaAndHerzegovinaCodeTypes.PDV, OrgCusCode.BosniaAndHerzegovinaCodeTypes.IDB, "PDV #: ", "ID #: ");
		}

		public void TestTaxIDForUzbekistan()
		{
			AssertDualTaxIdForARInvoice(Constants.CountryCodes.Uzbekistan, UzbekistanOrgCusCodeInfo.OrgCusCodes.QQS, UzbekistanOrgCusCodeInfo.OrgCusCodes.STR, "QQS #: ", "STIR #: ");
		}

		public void TestTaxIDForMontenegro()
		{
			AssertDualTaxIdForARInvoice(Constants.CountryCodes.Montenegro, MontenegroOrgCusCodeInfo.OrgCusCodes.PDV, MontenegroOrgCusCodeInfo.OrgCusCodes.PIB, "PDV #: ", "PIB #: ");
		}

		public void TestTaxIDForBahamas()
		{
			AssertDualTaxIdForARInvoice(Constants.CountryCodes.Bahamas, OrgCusCode.CodeTypes.VATCode, BahamasOrgCusCodeInfo.OrgCusCodes.TIN, "VAT #: ", "TIN #: ");
		}

		public void TestTaxIDForCyprus()
		{
			AssertDualTaxIdForARInvoice(Constants.CountryCodes.Cyprus, CyprusOrgCusCodeInfo.OrgCusCodes.TIC, OrgCusCode.CodeTypes.VATCode, "TIC #: ", "VAT #: ");
		}

		public void TestTaxIDForAngola()
		{
			AssertDualTaxIdForARInvoice(Constants.CountryCodes.Angola, OrgCusCode.CodeTypes.IVA, OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal, "IVA #: ", "NIF #: ");
		}

		public void TestTaxIDForLesotho()
		{
			AssertDualTaxIdForARInvoice(Constants.CountryCodes.Lesotho, OrgCusCode.CodeTypes.VATCode, LesothoOrgCusCodeInfo.OrgCusCodes.TIN, "VAT #: ", "TIN #: ");
		}

		public void TestTaxIDForGuyana()
		{
			AssertDualTaxIdForARInvoice(Constants.CountryCodes.Guyana, GuyanaOrgCusCodeInfo.OrgCusCodes.TIN, OrgCusCode.CodeTypes.VATCode, "TIN #: ", "VAT #: ");
		}

		public void TestTaxIDForPakistan()
		{
			AssertDualTaxIdForARInvoice(Constants.CountryCodes.Pakistan, OrgCusCode.CodeTypes.VATCode, PakistanOrgCusCodeInfo.OrgCusCodes.NTN, "STRN #: ", "NTN #: ");
		}

		public void TestTaxIDForCongo()
		{
			AssertSingleTaxIdForARInvoice(Constants.CountryCodes.Congo, CongoOrgCusCodeInfo.OrgCusCodes.NIU, "NIU #: ");
		}

		public void TestTaxIDForMoldova()
		{
			AssertDualTaxIdForARInvoice(Constants.CountryCodes.Moldova, OrgCusCode.CodeTypes.TVACode, MoldovaOrgCusCodeInfo.OrgCusCodes.NCF, "TVA #: ", "CF #: ");
		}

		public void TestTaxIDForElSalvador()
		{
			AssertDualTaxIdForARInvoice(Constants.CountryCodes.ElSalvador, ElSalvadorOrgCusCodeInfo.OrgCusCodes.NRC, ElSalvadorOrgCusCodeInfo.OrgCusCodes.NIT, "NRC #: ", "NIT #: ");
		}

		public void TestTaxIDForBrazil()
		{
			AssertDualTaxIdForARInvoice(Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalTaxPayerRegistration, "CNPJ: ", "IM: ");
		}

		public void AssertDualTaxIdForARInvoice(string countryCode, string orgProxyCusCodeType1, string orgProxyCusCodeType2, string orgProxyCusCodeType1Label, string orgProxyCusCodeType2Label)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				if (InvoicingBase is ARInvoice)
				{
					GlbCompany.CurrentCompany.GC_BusinessRegNo = "1111111";

					var transactionBranchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
					transactionBranchOrgProxy.OH_IsForwarder = true;
					var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
					branchOrgProxy.OH_IsForwarder = true;

					var invoiceBranch = Factory.NewWithValidTestData<GlbBranch>();
					invoiceBranch.GB_GC = GlbCompany.CurrentCompany.PK;
					invoiceBranch.GB_OH_OrgProxy = ZGuid.Empty;
					InvoicingBase.AH_GB = invoiceBranch.PK;
					GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgProxy.PK;
					Factory.Save();

					AssertNotEquals(GlbBranch.CurrentBranch.GB_OH_OrgProxy, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
					AssertNotEquals(GlbBranch.CurrentBranch.GB_OH_OrgProxy, InvoicingBase.Branch.GB_OH_OrgProxy);

					var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
					line.AL_AG = TestObjectCreator.GLHeader1.PK;
					var vat = Factory.NewWithValidTestData<AccTaxRate>();
					line.AL_AT = vat.PK;
					var wrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
					AssertEquals(string.Empty, wrapper.TaxId);

					var companyOrgProxyCode1 = "3333333333";
					var companyOrgProxyCode2 = "4444444444";
					var transactionHeaderBranchOrgProxyCode1 = "5555555555";
					var transactionHeaderBranchOrgProxyCode2 = "6666666666";

					TestObjectCreator.SetCustomsCodeForOrgHeader(GlbBranch.CurrentBranch.OrgProxy, orgProxyCusCodeType1, countryCode, "1111111111");
					TestObjectCreator.SetCustomsCodeForOrgHeader(GlbBranch.CurrentBranch.OrgProxy, orgProxyCusCodeType2, countryCode, "2222222222");

					TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, orgProxyCusCodeType1, countryCode, companyOrgProxyCode1);
					Factory.Save();

					wrapper = DocAPInvoice.New(InvoicingBase, Factory);
					AssertEquals(string.Format("{0}{1} ", orgProxyCusCodeType1Label, companyOrgProxyCode1), wrapper.TaxId);

					InvoicingBase.Company.OrgProxy.CustomsCodes.DeleteAll();
					TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, orgProxyCusCodeType2, countryCode, companyOrgProxyCode2);
					Factory.Save();

					wrapper = DocAPInvoice.New(InvoicingBase, Factory);
					AssertEquals(string.Format("{0}{1}", orgProxyCusCodeType2Label, companyOrgProxyCode2), wrapper.TaxId);

					TestObjectCreator.SetCustomsCodeForOrgHeader(InvoicingBase.Company.OrgProxy, orgProxyCusCodeType1, countryCode, companyOrgProxyCode1);
					Factory.Save();

					wrapper = DocAPInvoice.New(InvoicingBase, Factory);
					AssertEquals(string.Format("{0}{1} {2}{3}", orgProxyCusCodeType1Label, companyOrgProxyCode1, orgProxyCusCodeType2Label, companyOrgProxyCode2), wrapper.TaxId);

					invoiceBranch.GB_OH_OrgProxy = transactionBranchOrgProxy.PK;
					TestObjectCreator.SetCustomsCodeForOrgHeader(transactionBranchOrgProxy, orgProxyCusCodeType1, countryCode, transactionHeaderBranchOrgProxyCode1);
					Factory.Save();

					wrapper = DocAPInvoice.New(InvoicingBase, Factory);
					AssertEquals(string.Format("{0}{1} ", orgProxyCusCodeType1Label, transactionHeaderBranchOrgProxyCode1), wrapper.TaxId);

					TestObjectCreator.SetCustomsCodeForOrgHeader(transactionBranchOrgProxy, orgProxyCusCodeType2, countryCode, transactionHeaderBranchOrgProxyCode2);
					Factory.Save();

					wrapper = DocAPInvoice.New(InvoicingBase, Factory);
					AssertEquals(string.Format("{0}{1} {2}{3}", orgProxyCusCodeType1Label, transactionHeaderBranchOrgProxyCode1, orgProxyCusCodeType2Label, transactionHeaderBranchOrgProxyCode2), wrapper.TaxId);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestTaxIDForLithuania()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Lithuania))
			{
				if (InvoicingBase is ARInvoice)
				{
					GlbCompany.CurrentCompany.GC_BusinessRegNo = "1111111";
					GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Lithuania;
					Factory.Save();

					var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
					var vat = Factory.NewWithValidTestData<AccTaxRate>();
					line.AL_AT = vat.PK;
					var wrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
					AssertEquals("VAT #: LT1111111", wrapper.TaxId);

					var taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
					taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Lithuania;
					taxCode.OK_CodeType = OrgCusCode.LithuaniaCodeTypes.IMK;
					taxCode.OK_CustomsRegNo = "123456";
					wrapper = DocAPInvoice.New(InvoicingBase, Factory);
					AssertEquals("VAT #: LT1111111  ĮM.KODA # 123456", wrapper.TaxId);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestTaxIDForGB()
		{
			CoreTestForTaxID(Core.Constants.CountryCodes.UnitedKingdom, " #: GB");
		}

		public void TestTaxIDForGBWhereRegistrationNumberAlreadyStartsWithCountryCode()
		{
			ZString oldRegNumber = GlbCompany.CurrentCompany.GC_BusinessRegNo;
			try
			{
				GlbCompany.CurrentCompany.GC_BusinessRegNo = "GB123456789";
				CoreTestForTaxID(Core.Constants.CountryCodes.UnitedKingdom, " #: GB");
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_BusinessRegNo = oldRegNumber;
			}
		}

		public void TestTaxIDForMoroccoIncludesTVAAndICECode()
		{
			var companyOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			companyOrgProxy.OH_RL_NKClosestPort = "MACAS";

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchOrgProxy.OH_RL_NKClosestPort = "MACAS";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Morocco;
			company.GC_RX_NKLocalCurrency = "MAD";
			company.GC_OH_OrgProxy = companyOrgProxy.PK;
			company.GC_BusinessRegNo = "123456789";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = branchOrgProxy.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var customsCode1 = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
				customsCode1.OK_CodeType = OrgCusCode.MoroccoCodeTypes.ICE;
				customsCode1.OK_CustomsRegNo = "MA ICE Company";
				customsCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Morocco;

				InvoicingBase.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
				InvoicingBase.Header.OH_RL_NKClosestPort = "MACAS";
				InvoicingBase.AH_GC = company.PK;
				InvoicingBase.AH_GB = branch.PK;

				var invoiceLine = InvoicingBase.Lines.AddNew() as InvoicingLineBase;
				invoiceLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;

				var aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				var expectedTaxId = "TVA #: " + GlbCompany.CurrentCompany.GC_BusinessRegNo + "  ICE #: MA ICE Company";
				AssertEquals(expectedTaxId, aRInvoiceWrapper.TaxId);

				var customsCode2 = branch.OrgProxy.CustomsCodes.AddNew();
				customsCode2.OK_CodeType = OrgCusCode.MoroccoCodeTypes.ICE;
				customsCode2.OK_CustomsRegNo = "MA ICE Branch";
				customsCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Morocco;
				expectedTaxId = "TVA #: " + GlbCompany.CurrentCompany.GC_BusinessRegNo + "  ICE #: MA ICE Branch";
				AssertEquals(expectedTaxId, aRInvoiceWrapper.TaxId);
			}
		}

		public void TestTaxIDForEcuadorIncludesSRFAndSRICode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Ecuador))
			{
				var branchOrgProxy = Factory.New<OrgHeader>();
				branchOrgProxy.OH_Code = "OH456";
				branchOrgProxy.OH_RL_NKClosestPort = "INBOM";
				var companySRI = "1231231234";
				var companySRF = "12-01-12";
				var ecuador = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Ecuador);
				branchOrgProxy.SetCustomsCode(OrgCusCode.EcuadorCodeTypes.SRI, ecuador, companySRI);
				branchOrgProxy.SetCustomsCode(OrgCusCode.EcuadorCodeTypes.SRF, ecuador, companySRF);

				var branch = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
				this.InvoicingBase.AH_GB = branch.PK;

				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("TaxID", ZString.Empty, aRInvoiceWrapper.TaxId);

				var invoiceLine = InvoicingBase.Lines.AddNew() as InvoicingLineBase;
				invoiceLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				invoiceLine.TaxRate.AT_Type = AccTaxRate.Types.Rated;

				branch.GB_OH_OrgProxy = branchOrgProxy.PK;

				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				Factory.ClearCachedValue<ZBool>(InvoicingBase.PK.ToStringKey());
				AssertEndsWith("TaxID", $"{aRInvoiceWrapper.BranchTaxIDHeading}: {companySRI}, {aRInvoiceWrapper.BranchBusRegHeading}: {companySRF}", aRInvoiceWrapper.TaxId);
			}
		}

		public void TestTaxIDForSriLankaIncludesSVATCode()
		{
			AssertTaxIDIncludesExtraCusCode("LKCMB", Constants.CountryCodes.SriLanka, OrgCusCode.SriLankaCodeTypes.SVATBusinessRegistrationNumber, "VAT #:", "SVAT#:");
		}

		public void TestTaxIDForAzerbaijanIncludesTINCode()
		{
			AssertTaxIDIncludesExtraCusCode("AZAST", Constants.CountryCodes.Azerbaijan, OrgCusCode.AzerbaijanCodeTypes.TIN, "VAT #:", "TIN#:");
		}

		public void TestTaxIDForKenyaIncludesPINCode()
		{
			AssertTaxIDIncludesExtraCusCode("KEARI", Constants.CountryCodes.Kenya, OrgCusCode.KenyaCodeTypes.PIN, "VAT #:", "PIN#:");
		}

		public void TestTaxIDForMauritiusIncludesBRNCode()
		{
			AssertTaxIDIncludesExtraCusCode("MUBAM", Constants.CountryCodes.Mauritius, OrgCusCode.CodeTypes.BusinessRegistrationNumber, "VAT #:", "BRN#:");
		}

		public void TestTaxIDForTanzaniaIncludesTINCode()
		{
			AssertTaxIDIncludesExtraCusCode("TZMSH", Constants.CountryCodes.Tanzania, OrgCusCode.TanzaniaCodeTypes.TIN, "VAT #:", "TIN#:");
		}

		public void TestTaxIDForMadagascarIncludesNISCode()
		{
			AssertTaxIDIncludesExtraCusCode("MGAHY", Constants.CountryCodes.Madagascar, OrgCusCode.MadagascarCodeTypes.NIS, "NIF #:", "N° Statistique:");
		}

		public void TestTaxIDForCuracaoIncludesCCRCode()
		{
			AssertTaxIDIncludesExtraCusCode("CWCUR", Constants.CountryCodes.Curacao, OrgCusCode.CuracaoCodeTypes.CCR, "CRIB #:", "CCRN #:");
		}

		public void TestTaxIDForTogoIncludesNICCode()
		{
			AssertTaxIDIncludesExtraCusCode("TGABG", Constants.CountryCodes.Togo, OrgCusCode.TogoCodeTypes.NIC, "NIF #:", "NIC #:");
		}

		public void TestTaxIDForBeninIncludesNRCCode()
		{
			AssertTaxIDIncludesExtraCusCode("BJPTN", Constants.CountryCodes.Benin, OrgCusCode.BeninCodeTypes.NRC, "IFU #:", "NRC #:");
		}

		public void TestTaxIDForKosovoIncludesNFKCode()
		{
			AssertTaxIDIncludesExtraCusCode("XK111", Constants.CountryCodes.Kosovo, OrgCusCode.KosovoCodeTypes.NFK, "TVSH #:", "NFK #:");
		}

		void AssertTaxIDIncludesExtraCusCode(string unloco, string country, string extraCusCodeType, string expectedMainTaxPrefix, string expectedExtraCusCodePrefix)
		{
			ZString currentCountryCode = GlbCompany.CurrentCompany.Country.Code;
			try
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
				AssertEquals("TaxID", ZString.Empty, aRInvoiceWrapper.TaxId);

				InvoicingLineBase invoiceLine = InvoicingBase.Lines.AddNew() as InvoicingLineBase;
				invoiceLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				invoiceLine.TaxRate.AT_Type = AccTaxRate.Types.Rated;

				string expectedTaxID = "VAT #: GB" + GlbCompany.CurrentCompany.GC_BusinessRegNo;
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				Factory.ClearCachedValue<ZBool>(InvoicingBase.PK.ToStringKey());
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = unloco;
				OrgCusCode taxCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = country;
				taxCode.OK_CodeType = extraCusCodeType;
				taxCode.OK_CustomsRegNo = "345543";
				expectedTaxID += "  " + expectedExtraCusCodePrefix + " 345543";
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);

				expectedTaxID = expectedMainTaxPrefix + " " + GlbCompany.CurrentCompany.GC_BusinessRegNo;
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				GlbCompany.CurrentCompany.SetCountry(country);
				expectedTaxID += "  " + expectedExtraCusCodePrefix + " 345543";
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		void CoreTestForTaxID(string countryCode, string expectedTaxCodeSuffix)
		{
			CoreTestForTaxID(countryCode, expectedTaxCodeSuffix, true);
		}

		void CoreTestForTaxID(string countryCode, string expectedTaxCodeSuffix, bool useDefaultTaxCode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("Precondition: Country Code", countryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("TaxID", ZString.Empty, aRInvoiceWrapper.TaxId);

				InvoicingLineBase invoiceLine = InvoicingBase.Lines.AddNew() as InvoicingLineBase;
				invoiceLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;

				string taxCode = useDefaultTaxCode ? DocARBaseInvoice.GetTranslatedTaxCodeFromCountryCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) : null;
				string expectedTaxID = taxCode + expectedTaxCodeSuffix + GlbCompany.CurrentCompany.GC_BusinessRegNo;
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				Factory.ClearCachedValue<ZBool>(InvoicingBase.PK.ToStringKey());
				AssertEquals("TaxID", expectedTaxID, aRInvoiceWrapper.TaxId);
			}
		}

		public void TestTaxIDWithIndianPAN()
		{
			TestObjectCreator.NonCurrentCompanyBranch.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.India;
			TestObjectCreator.NonCurrentCompanyBranch.Company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.India;
			RefUNLOCO indianLocation = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "INDEL"));
			TestObjectCreator.NonCurrentCompanyBranch.GB_RL_NKHomePort = indianLocation.RL_Code;
			Factory.Save();

			using (IDisposable context = TestObjectCreator.NonCurrentCompanyBranch.SetAsTemporaryContext())
			{
				var pAN = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
				pAN.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.PAN;
				pAN.OK_CustomsRegNo = "0123456789";
				pAN.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;

				InvoicingBase.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
				InvoicingBase.Header.OH_RL_NKClosestPort = "INDEL";

				InvoicingLineBase invoiceLine = InvoicingBase.Lines.AddNew() as InvoicingLineBase;
				invoiceLine.AL_AT = TestObjectCreator.SERANDEDU1.PK;

				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("Service Tax Registration No: 9999  PAN: 0123456789", aRInvoiceWrapper.TaxId);
			}
		}

		public void TestTaxIDWithRUKPP()
		{
			var companyOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			companyOrgProxy.OH_Code = "Moscow";
			companyOrgProxy.OH_RL_NKClosestPort = "RUMOW";

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchOrgProxy.OH_Code = "PETERSBURG";
			branchOrgProxy.OH_RL_NKClosestPort = "RULED";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Russia;
			company.GC_RX_NKLocalCurrency = "RUB";
			company.GC_OH_OrgProxy = companyOrgProxy.PK;
			company.GC_BusinessRegNo = "123456789";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = branchOrgProxy.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var customsCode1 = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
				customsCode1.OK_CodeType = OrgCusCode.RussiaCodeTypes.KPP;
				customsCode1.OK_CustomsRegNo = "RU KPP Company";
				customsCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Russia;

				InvoicingBase.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
				InvoicingBase.Header.OH_RL_NKClosestPort = "RUMOW";
				InvoicingBase.AH_GC = company.PK;
				InvoicingBase.AH_GB = branch.PK;

				var invoiceLine = InvoicingBase.Lines.AddNew() as InvoicingLineBase;
				invoiceLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;

				var aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				var expectedTaxId = "VAT #: " + GlbCompany.CurrentCompany.GC_BusinessRegNo + "  KPP #: RU KPP Company";
				AssertEquals(expectedTaxId, aRInvoiceWrapper.TaxId);

				var customsCode2 = branch.OrgProxy.CustomsCodes.AddNew();
				customsCode2.OK_CodeType = OrgCusCode.RussiaCodeTypes.KPP;
				customsCode2.OK_CustomsRegNo = "RU KPP Branch";
				customsCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Russia;
				expectedTaxId = "VAT #: " + GlbCompany.CurrentCompany.GC_BusinessRegNo + "  KPP #: RU KPP Branch";
				AssertEquals(expectedTaxId, aRInvoiceWrapper.TaxId);
			}
		}

		public void TestInvoiceSubTotal()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG1";
			Invoice.AH_OH = org.PK;
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("InvoiceSubTotal", 0M, aRInvoiceWrapper.InvoiceSubTotal);

			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("InvoiceSubTotal", 0M, aRInvoiceWrapper.InvoiceSubTotal);

			line1.AL_OSExTaxAmount = 123.00M;
			line1.AL_OSTaxAmount = 12.30M;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_OSExTaxAmount = 234.00M;
			line2.AL_OSTaxAmount = 23.40M;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			AssertEquals("InvoiceSubTotal when OM_ARDontShowTaxOnDocs is false", 357.00M, aRInvoiceWrapper.InvoiceSubTotal);

			OrgMiscServ miscServ = null;
			try
			{
				miscServ = Factory.LoadTop1<OrgMiscServ>(new ZQuery(OrgMiscServSchema.OM_OH, org.PK));
				miscServ.OM_ARDontShowTaxOnDocs = true;
				Invoice.AH_FullyPaidDate = ZDateTime.Empty;
				Factory.Save();
				AssertEquals("InvoiceSubTotal when OM_ARDontShowTaxOnDocs is true", 392.70M, aRInvoiceWrapper.InvoiceSubTotal);
			}
			finally
			{
				miscServ.OM_ARDontShowTaxOnDocs = false;
			}
		}

		public void TestInvoiceSubTotalFormatted()
		{
			CultureInfo oldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

			try
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TESTORG1";
				Invoice.AH_OH = org.PK;
				Factory.Save();
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("InvoiceSubTotal", 0M, aRInvoiceWrapper.InvoiceSubTotal);

				InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
				InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;
				line2.AL_AG = TestObjectCreator.GLHeader1.PK;
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("InvoiceSubTotal", 0M, aRInvoiceWrapper.InvoiceSubTotal);

				line1.AL_OSExTaxAmount = 1123.00M;
				line1.AL_OverseasTotal = 135.30M;
				line2.AL_OSExTaxAmount = 234.00M;
				line2.AL_OverseasTotal = 257.40M;

				Invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				AssertEquals("InvoiceSubTotal when OM_ARDontShowTaxOnDocs is false", 1357.00M, aRInvoiceWrapper.InvoiceSubTotal);
				AssertEquals("InvoiceSubTotalFormatted", "1,357.00", aRInvoiceWrapper.InvoiceSubTotalFormatted);
				((RefCurrency)aRInvoiceWrapper.Currency.WrappedObject).RX_SubUnitRatio = 1;
				AssertEquals("InvoiceSubTotalFormatted", "1,357", aRInvoiceWrapper.InvoiceSubTotalFormatted);
				((RefCurrency)aRInvoiceWrapper.Currency.WrappedObject).RX_SubUnitRatio = 100;

				using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.France))
				{
					Invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					AssertEquals("InvoiceSubTotal when OM_ARDontShowTaxOnDocs is false", 1357.00M, aRInvoiceWrapper.InvoiceSubTotal);
					AssertEquals("InvoiceSubTotalFormatted", "1 357,00", aRInvoiceWrapper.InvoiceSubTotalFormatted);
					((RefCurrency)aRInvoiceWrapper.Currency.WrappedObject).RX_SubUnitRatio = 1;
					AssertEquals("InvoiceSubTotalFormatted", "1 357", aRInvoiceWrapper.InvoiceSubTotalFormatted);
					((RefCurrency)aRInvoiceWrapper.Currency.WrappedObject).RX_SubUnitRatio = 100;
				}
			}
			finally
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = oldCulture;
			}
		}

		public void TestShowInvoiceTaxDate()
		{
			var validInvoiceTaxDate = ZDate.Today.AddDays(-2);
			var invoiceDate = ZDateTime.Today.AddDays(-1);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG1";
			org.MiscServ.OM_ARDontShowTaxOnDocs = false;
			Invoice.AH_OH = org.PK;
			Invoice.AH_InvoiceDate = invoiceDate;

			var line1 = TestObjectCreator.CreateInvoiceLine(InvoicingBase, 127m);
			var line2 = TestObjectCreator.CreateInvoiceLine(InvoicingBase, 234m);

			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Type = AccTaxRate.Types.Rated;
			line1.AL_AT = taxRate1.PK;
			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Type = AccTaxRate.Types.Rated;
			line2.AL_AT = taxRate2.PK;
			line1.AL_TaxDate = validInvoiceTaxDate;
			line2.AL_TaxDate = validInvoiceTaxDate;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			line1.AL_AC = chargeCode.PK;
			line2.AL_AC = chargeCode.PK;

			AssertShowInvoiceTaxDate(validInvoiceTaxDate, "we show the invoice tax date when registry is HDR, HDT or BOD");

			line1.AL_AT = ZGuid.Empty;
			line2.AL_AT = ZGuid.Empty;
			AssertWeDontShowInvoiceTaxDate("we hide the invoice tax date when there is no taxID");
			line1.AL_AT = taxRate1.PK;
			line2.AL_AT = taxRate2.PK;
			line1.AL_TaxDate = validInvoiceTaxDate;
			line2.AL_TaxDate = validInvoiceTaxDate;

			taxRate1.AT_Type = AccTaxRate.Types.ExcludedFromTheTaxBase;
			AssertShowInvoiceTaxDate(validInvoiceTaxDate, "we show the invoice tax date when at least one taxId is reportable");
			taxRate2.AT_Type = AccTaxRate.Types.NotReportable;
			AssertWeDontShowInvoiceTaxDate("we hide the invoice tax date when there is no reportable taxID");
			taxRate1.AT_Type = AccTaxRate.Types.Rated;
			taxRate2.AT_Type = AccTaxRate.Types.Rated;

			chargeCode.AC_ChargeType = "CMT";
			Assert(line1.IsCommentCharge);
			Assert(line2.IsCommentCharge);
			AssertWeDontShowInvoiceTaxDate("comment line doesn't count as a reportable taxID line");
			chargeCode.AC_ChargeType = "FRT";

			line1.AL_TaxDate = ZDate.Empty;
			line2.AL_TaxDate = ZDate.Empty;
			AssertShowInvoiceTaxDate(invoiceDate, "we use the invoice date when it can't find a tax date");
			line1.AL_TaxDate = validInvoiceTaxDate;
			line2.AL_TaxDate = validInvoiceTaxDate;

			InvoicingBase.AH_InvoiceDate = validInvoiceTaxDate;
			AssertEquals(InvoicingBase.AH_InvoiceDate.Date, line1.AL_TaxDate);
			AssertEquals(InvoicingBase.AH_InvoiceDate.Date, line2.AL_TaxDate);
			AssertShowInvoiceTaxDate(validInvoiceTaxDate, "we show the invoice tax date when Invoice Date is equals to TaxDate");

			if (InvoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				org.MiscServ.OM_ARDontShowTaxOnDocs = true;
				var wrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
				AssertEquals(validInvoiceTaxDate, wrapper.InvoiceTaxDate);
				Assert("we hide the invoice tax date when the org doesn't allow us to show Tax on docs", !wrapper.ShowInvoiceTaxDate);
			}

			void AssertWeDontShowInvoiceTaxDate(string message)
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);

				var printTaxDateInARInvoiceDocumentOptions = new string[] {
					AccountingConstants.PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInHeaderEarliest.Code,
					AccountingConstants.PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInHeaderLatest.Code };

				foreach (var printTaxDateInARInvoiceDocumentOption in printTaxDateInARInvoiceDocumentOptions)
				{
					using (AccountingConfigurationRegistry.Instance.PrintTaxDateInARInvoiceDocument.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, printTaxDateInARInvoiceDocumentOption))
					{
						Assert(message, !aRInvoiceWrapper.ShowInvoiceTaxDate);
						Assert(message, !aRInvoiceWrapper.ShowInvoiceLineTaxDate);
					}
				}
			}

			void AssertShowInvoiceTaxDate(ZDateTime expectedInvoiceTaxDate, string message = "")
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(expectedInvoiceTaxDate, aRInvoiceWrapper.InvoiceTaxDate);

				foreach (var registryValue in AccountingConstants.PrintTaxDateInARInvoiceDocumentOption.CodeList.GetAllCodes())
				{
					using (AccountingConfigurationRegistry.Instance.PrintTaxDateInARInvoiceDocument.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue))
					{
						if (registryValue == AccountingConstants.PrintTaxDateInARInvoiceDocumentOption.DoNotPrintTaxDate.Code)
						{
							Assert("We don't print the tax date when registry is NOT. " + message, !aRInvoiceWrapper.ShowInvoiceTaxDate);
							Assert("We don't print the tax date when registry is NOT. " + message, !aRInvoiceWrapper.ShowInvoiceLineTaxDate);
						}
						else if (registryValue == AccountingConstants.PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInHeaderEarliest.Code
							|| registryValue == AccountingConstants.PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInHeaderLatest.Code)
						{
							Assert($"We print the tax date in the header when registry is {registryValue}. " + message, aRInvoiceWrapper.ShowInvoiceTaxDate);
							Assert($"We don't print the tax date for each lines when registry is {registryValue}. " + message, !aRInvoiceWrapper.ShowInvoiceLineTaxDate);
						}
						else if (registryValue == AccountingConstants.PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInBody.Code)
						{
							Assert("We don't print the tax date in the header when registry is BOD. " + message, !aRInvoiceWrapper.ShowInvoiceTaxDate);
							Assert("We print the tax date for each lines when registry is BOD. " + message, aRInvoiceWrapper.ShowInvoiceLineTaxDate);
						}
					}
				}
			}
		}

		public void TestIsTaxed()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG1";
			Invoice.AH_OH = org.PK;
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("IsTaxed:1", false, aRInvoiceWrapper.IsTaxed);

			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("IsTaxed:2", false, aRInvoiceWrapper.IsTaxed);

			line1.AL_OSExTaxAmount = 123.00M;
			line1.AL_OSTaxAmount = 35.30M;
			line2.AL_OSExTaxAmount = 234.00M;
			line2.AL_OSTaxAmount = 57.40M;
			InvoicingBase.AH_FullyPaidDate = ZDateTime.Empty;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("IsTaxed:3", false, aRInvoiceWrapper.IsTaxed);

			line1.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			Factory.ClearCachedValue<ZBool>(InvoicingBase.PK.ToStringKey());
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("IsTaxed:4", true, aRInvoiceWrapper.IsTaxed);

			try
			{
				org.MiscServ.OM_ARDontShowTaxOnDocs = true;
				aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
				AssertEquals("IsTaxed:5", false, aRInvoiceWrapper.IsTaxed);
			}
			finally
			{
				GlbBranch.CurrentBranch.OrgProxy.MiscServ.OM_ARDontShowTaxOnDocs = false;
			}
		}

		public void TestIsTaxedIsCachedInFactory()
		{
			InvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(false, InvoiceWrapper.IsTaxed);

			var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_OSExTaxAmount = 123.00M;
			line.AL_OSTaxAmount = 35.30M;
			line.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			InvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("IsTaxed should be true as it calculates based on lines when invoice is not in database.", true, InvoicingBase.IsTaxed);
			AssertEquals(InvoicingBase.IsTaxed, InvoiceWrapper.IsTaxed);

			line.AL_AT = ZGuid.Empty;
			AssertEquals("IsTaxed should be false as it calculates based on lines when invoice is not in database.", false, InvoicingBase.IsTaxed);
			AssertEquals(InvoicingBase.IsTaxed, InvoiceWrapper.IsTaxed);

			Factory.Save();
			Assert(InvoicingBase.IsInDatabase);
			Factory.ClearCachedValue<ZBool>("IsTaxed:" + InvoicingBase.PK.ToStringKey());
			Factory.GetCachedValue("IsTaxed:" + InvoicingBase.PK.ToStringKey(), () => ZBool.True);

			AssertEquals("IsTaxed should be true as it reads from the cache when invoice is in database.", true, InvoicingBase.IsTaxed);
			AssertEquals(InvoicingBase.IsTaxed, InvoiceWrapper.IsTaxed);
		}

		public void TestShipmentWeight()
		{
			AssertEquals("Shipment Weight", ZString.Empty, ARInvoiceWrapper.ShipmentWeight);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsForwarder, true));
			SetupForShipmentTest(header);
			AssertEquals("Shipment weight is client weight", "234", ARInvoiceWrapper.ShipmentWeight);

			header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsForwarder, false));
			Invoice.AH_OH = header.PK;
			AssertEquals("Shipment weight is actual weight", "123", ARInvoiceWrapper.ShipmentWeight);
		}

		ForwardingShipment SetupForShipmentTest(OrgHeader orgHeader)
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1234";
			shipment.JS_ActualWeight = 123.00M;
			shipment.JS_DocumentedWeight = 234.000M;
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			Invoice.AH_OH = orgHeader.PK;
			JobHeader job = GetInvoiceJob(shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			return shipment;
		}

		public void TestShipmentVolume()
		{
			AssertEquals("ShipmentVolume", ZString.Empty, ARInvoiceWrapper.ShipmentVolume);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsForwarder, true));
			ForwardingShipment shipment = SetupForShipmentTest(header);

			shipment.JS_ActualVolume = 123.00M;
			shipment.JS_DocumentedVolume = 234.000M;
			AssertEquals("ShipmentVolume is client weight", "234", ARInvoiceWrapper.ShipmentVolume);

			header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsForwarder, false));
			Invoice.AH_OH = header.PK;
			AssertEquals("ShipmentVolume is actual weight", "123", ARInvoiceWrapper.ShipmentVolume);
		}

		public void TestShipmentCharageable()
		{
			AssertEquals("ShipmentCharageable", ZString.Empty, ARInvoiceWrapper.ShipmentCharageable);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsForwarder, true));
			ForwardingShipment shipment = SetupForShipmentTest(header);
			shipment.JS_ActualChargeable = 123.00M;
			shipment.JS_DocumentedChargeable = 234.000M;

			AssertEquals("ShipmentCharageable is client weight", "234", ARInvoiceWrapper.ShipmentCharageable);

			header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsForwarder, false));
			Invoice.AH_OH = header.PK;
			AssertEquals("ShipmentCharageable is actual weight", "123", ARInvoiceWrapper.ShipmentCharageable);
		}

		public void TestVesselVoyageFlight()
		{
			AssertEquals("VesselVoyageFlight", ZString.Empty, ARInvoiceWrapper.VesselVoyageFlight);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";

			Transport transport = consol.Transports[0];
			transport.JW_Vessel = "Vessel2";
			transport.JW_VoyageFlight = "2222";
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			AssertEquals("VesselVoyageFlight", "Vessel2 / 2222", ARInvoiceWrapper.VesselVoyageFlight);

			transport.JW_Vessel = "";
			transport.JW_VoyageFlight = "QF11";
			AssertEquals("VesselVoyageFlight", "QF11", ARInvoiceWrapper.VesselVoyageFlight);

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_TransportMode = "SEA";

			loadList.JK_UniqueConsignRef = "L00003333";

			Transport cFSTransport = loadList.Transports[0];
			cFSTransport.JW_Vessel = vessel.RV_Code;
			cFSTransport.JW_VoyageFlight = "4444";
			cFSTransport.JW_RL_NKLoadPort = "AUSYD";
			cFSTransport.JW_RL_NKDiscPort = "SGSIN";
			cFSTransport.JW_ETD = ZDateTime.Today;
			cFSTransport.JW_ETA = ZDateTime.Today.AddDays(23);
			Invoice.AH_ConsolidatedInvoiceRef = loadList.JK_UniqueConsignRef;
			AssertEquals("VesselVoyageFlig ht", vessel.RV_Code + " / 4444", ARInvoiceWrapper.VesselVoyageFlight);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001122";
			var voyage = Factory.New<JobVoyage>();
			JobHeader job = GetInvoiceJob(shipment, Invoice);

			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "3344";
			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			shipment.JS_JX = voyage.Sailings[0].PK;
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			AssertEquals("VesselVoyageFlight", vessel.RV_Code + " / 3344", ARInvoiceWrapper.VesselVoyageFlight);

			CFSShipment cFSShipment = Factory.New<CFSShipment>();
			cFSShipment.JS_UniqueConsignRef = "H00003344";
			voyage.JV_VoyageFlight = "5566";
			cFSShipment.JS_JX = voyage.Sailings[0].PK;
			Invoice.AH_ConsolidatedInvoiceRef = cFSShipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(cFSShipment, Invoice);
			AssertEquals("VesselVoyageFlight", vessel.RV_Code + " / 5566", ARInvoiceWrapper.VesselVoyageFlight);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00007777";
			declaration.JE_VesselName = vessel.RV_Code;
			declaration.JE_VoyageFlightNo = "8888";
			Factory.Save();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			job = GetInvoiceJob(declaration, Invoice);
			AssertEquals("VesselVoyageFlight", vessel.RV_Code + " / 8888", ARInvoiceWrapper.VesselVoyageFlight);

			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_JX_Sailing = loadList.Schedule.PK;
			cartage.JJ_ConsignmentID = "T00009999";
			Invoice.AH_ConsolidatedInvoiceRef = cartage.JJ_ConsignmentID;
			job = GetInvoiceJob(cartage, Invoice);
			AssertEquals("VesselVoyageFlight", vessel.RV_Code + " / 4444", ARInvoiceWrapper.VesselVoyageFlight);

			var containerRego = Factory.New<CFSContainer>();
			containerRego.JC_ContainerJobID = "D00006789";
			voyage.JV_VoyageFlight = "8899";
			containerRego.JC_JX = voyage.Sailings[0].PK;
			Invoice.AH_ConsolidatedInvoiceRef = containerRego.JC_ContainerJobID;
			SetCFSContainerJob(containerRego, Invoice);
			AssertEquals("VesselVoyageFlight", vessel.RV_Code + " / 8899", ARInvoiceWrapper.VesselVoyageFlight);

			AgencyShipment agencyShipment = Factory.New<AgencyShipment>();
			agencyShipment.JS_UniqueConsignRef = "V00001122";
			job = GetInvoiceJob(agencyShipment, Invoice);
			agencyShipment.JS_JX = voyage.Sailings[0].PK;
			Invoice.AH_ConsolidatedInvoiceRef = agencyShipment.JS_UniqueConsignRef;
			AssertEquals("VesselVoyageFlight", vessel.RV_Code + " / 8899", ARInvoiceWrapper.VesselVoyageFlight);
		}

		public void TestHouseBillNumber()
		{
			var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_GC = currentCompany.PK;

			AssertEquals("HouseBillNumber", ZString.Empty, ARInvoiceWrapper.HouseBillNumber);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			AssertEquals("HouseBillNumber", ZString.Empty, ARInvoiceWrapper.HouseBillNumber);

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "L00003333";
			Invoice.AH_ConsolidatedInvoiceRef = loadList.JK_UniqueConsignRef;
			AssertEquals("HouseBillNumber", ZString.Empty, ARInvoiceWrapper.HouseBillNumber);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001122";
			shipment.JS_HouseBill = "H1234";
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(shipment, Invoice);
			AssertEquals("HouseBillNumber", "H1234", ARInvoiceWrapper.HouseBillNumber);

			Invoice.AH_JH = ZGuid.Empty;
			job.Delete();

			var cFSShipment = Factory.New<CFSShipment>();
			cFSShipment.JS_UniqueConsignRef = "H00003344";
			cFSShipment.JS_HouseBill = "H5678";
			Invoice.AH_ConsolidatedInvoiceRef = cFSShipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(cFSShipment, Invoice);
			AssertEquals("HouseBillNumber", "H5678", ARInvoiceWrapper.HouseBillNumber);

			Invoice.AH_JH = ZGuid.Empty;
			job.Delete();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00007777";
			declaration.JE_HouseBill = "H9999";
			declaration.JE_GB = currentBranch.PK;
			Factory.Save();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			job = GetInvoiceJob(declaration, Invoice);
			AssertEquals("HouseBillNumber", "H9999", ARInvoiceWrapper.HouseBillNumber);

			Invoice.AH_JH = ZGuid.Empty;
			job.Delete();

			var containerRego = Factory.New<CFSContainer>();
			containerRego.JC_ContainerJobID = "D00006789";
			Invoice.AH_ConsolidatedInvoiceRef = containerRego.JC_ContainerJobID;

			job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobContainerSchema.Constants.Prefix;
			job.JH_ParentID = containerRego.PK;
			Invoice.AH_JH = job.PK;

			AssertEquals("HouseBillNumber", ZString.Empty, ARInvoiceWrapper.HouseBillNumber);
		}

		public void TestETD()
		{
			AssertEquals("ETD", ZString.Empty, ARInvoiceWrapper.ETD);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			Transport transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2004, 10, 06);
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			AssertEquals("ETD", "06/10/2004", ARInvoiceWrapper.ETD);

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "L00003333";
			Transport cFSTransport = loadList.Transports[0];
			cFSTransport.JW_ETD = new ZDateTime(2004, 10, 07);
			JobHeader job = GetInvoiceJob(loadList, Invoice);
			Invoice.AH_ConsolidatedInvoiceRef = loadList.JK_UniqueConsignRef;
			AssertEquals("ETD", "07/10/2004", ARInvoiceWrapper.ETD);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001122";
			shipment.JS_E_DEP = new ZDateTime(2004, 10, 08);
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(shipment, Invoice);
			AssertEquals("ETD", "08/10/2004", ARInvoiceWrapper.ETD);

			Invoice.AH_JH = ZGuid.Empty;
			job.Delete();

			var cFSShipment = Factory.New<CFSShipment>();
			cFSShipment.JS_UniqueConsignRef = "H00003344";
			cFSShipment.JS_E_DEP = new ZDateTime(2004, 10, 09);
			Invoice.AH_ConsolidatedInvoiceRef = cFSShipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(cFSShipment, Invoice);
			AssertEquals("ETD", "09/10/2004", ARInvoiceWrapper.ETD);

			Invoice.AH_JH = ZGuid.Empty;
			job.Delete();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00007777";
			declaration.JE_ExportDate = new ZDateTime(2004, 10, 10);
			Factory.Save();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			job = GetInvoiceJob(declaration, Invoice);
			AssertEquals("ETD", "10/10/2004", ARInvoiceWrapper.ETD);

			Invoice.AH_JH = ZGuid.Empty;
			job.Delete();

			var containerRego = Factory.New<CFSContainer>();
			containerRego.JC_ContainerJobID = "D00006789";
			var voyage = Factory.New<JobVoyage>();
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2004, 10, 11);
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			containerRego.JC_JX = voyage.Sailings[0].PK;
			Invoice.AH_ConsolidatedInvoiceRef = containerRego.JC_ContainerJobID;

			job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobContainerSchema.Constants.Prefix;
			job.JH_ParentID = containerRego.PK;
			Invoice.AH_JH = job.PK;

			AssertEquals("ETD", "11/10/2004", ARInvoiceWrapper.ETD);
		}

		public void TestPortOfLoading()
		{
			AssertEquals("PortOfLoading", ZString.Empty, ARInvoiceWrapper.PortOfLoading);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Transport transport = consol.Transports[0];

			consol.JK_UniqueConsignRef = "C00001111";
			RefUNLOCO sydney = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			consol.JK_RL_NKLoadPort = sydney.Code;
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			AssertEquals("PortOfLoading", sydney.RL_PortName, ARInvoiceWrapper.PortOfLoading);

			RefUNLOCO singapore = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN");
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "L00003333";
			loadList.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			loadList.JK_RL_NKLoadPort = singapore.Code;
			loadList.JK_RL_NKDischargePort = sydney.Code;
			Transport cFSTransport = loadList.Transports[0];
			cFSTransport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;
			cFSTransport.JW_VoyageFlight = "4112";
			cFSTransport.JW_ATD = ZDateTime.Now;

			Invoice.AH_ConsolidatedInvoiceRef = loadList.JK_UniqueConsignRef;
			AssertEquals("PortOfLoading", singapore.RL_PortName, ARInvoiceWrapper.PortOfLoading);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001122";
			shipment.Consols.Add(consol);
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(shipment, Invoice);
			AssertEquals("PortOfLoading", sydney.RL_PortName, ARInvoiceWrapper.PortOfLoading);

			CFSShipment cFSShipment = Factory.New<CFSShipment>();
			cFSShipment.JS_UniqueConsignRef = "H00003344";
			cFSShipment.Consols.Add(loadList);

			Invoice = Factory.New<ARInvoice>();
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			Invoice.AH_ConsolidatedInvoiceRef = cFSShipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(cFSShipment, Invoice);
			AssertEquals("PortOfLoading", singapore.RL_PortName, ARInvoiceWrapper.PortOfLoading);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00007777";
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			job = GetInvoiceJob(shipment, Invoice);
			AssertEquals("PortOfLoading", sydney.RL_PortName, ARInvoiceWrapper.PortOfLoading);

			CFSContainer containerRego = Factory.New<CFSContainer>();
			containerRego.JC_ContainerJobID = "D00006789";
			containerRego.JC_JK = consol.PK;
			Invoice.AH_ConsolidatedInvoiceRef = containerRego.JC_ContainerJobID;
			SetCFSContainerJob(containerRego, Invoice);
			AssertEquals("PortOfLoading", sydney.RL_PortName, ARInvoiceWrapper.PortOfLoading);

			AgencyShipment agencyShipment = Factory.New<AgencyShipment>();
			agencyShipment.JS_UniqueConsignRef = "V00001122";
			agencyShipment.JS_NKLoadPort = sydney.Code;
			Invoice.AH_ConsolidatedInvoiceRef = agencyShipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(agencyShipment, Invoice);
			AssertEquals("PortOfLoading", sydney.RL_PortName, ARInvoiceWrapper.PortOfLoading);
		}

		public void TestPortOfDischarge()
		{
			AssertEquals("PortOfDischarge", ZString.Empty, ARInvoiceWrapper.PortOfDischarge);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Transport transport = consol.Transports[0];

			consol.JK_UniqueConsignRef = "C00001111";
			RefUNLOCO sydney = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			consol.JK_RL_NKDischargePort = sydney.Code;
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			AssertEquals("PortOfDischarge", sydney.RL_PortName, ARInvoiceWrapper.PortOfDischarge);

			RefUNLOCO singapore = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN");
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "L00003333";
			loadList.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			loadList.JK_RL_NKLoadPort = sydney.Code;
			loadList.JK_RL_NKDischargePort = singapore.Code;

			Transport cFSTransport = loadList.Transports[0];
			cFSTransport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;
			cFSTransport.JW_VoyageFlight = "4112";
			cFSTransport.JW_ATD = ZDateTime.Now;

			Invoice.AH_ConsolidatedInvoiceRef = loadList.JK_UniqueConsignRef;
			AssertEquals("PortOfDischarge", singapore.RL_PortName, ARInvoiceWrapper.PortOfDischarge);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001122";
			shipment.Consols.Add(consol);
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(shipment, Invoice);
			AssertEquals("PortOfDischarge", sydney.RL_PortName, ARInvoiceWrapper.PortOfDischarge);

			CFSShipment cFSShipment = Factory.New<CFSShipment>();
			cFSShipment.JS_UniqueConsignRef = "H00003344";
			cFSShipment.Consols.Add(loadList);
			Invoice.AH_ConsolidatedInvoiceRef = cFSShipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(cFSShipment, Invoice);
			AssertEquals("PortOfDischarge", singapore.RL_PortName, ARInvoiceWrapper.PortOfDischarge);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00007777";
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			job = GetInvoiceJob(shipment, Invoice);
			AssertEquals("PortOfDischarge", sydney.RL_PortName, ARInvoiceWrapper.PortOfDischarge);

			CFSContainer containerRego = Factory.New<CFSContainer>();
			containerRego.JC_ContainerJobID = "D00006789";
			containerRego.JC_JK = consol.PK;
			Invoice.AH_ConsolidatedInvoiceRef = containerRego.JC_ContainerJobID;
			SetCFSContainerJob(containerRego, Invoice);
			AssertEquals("PortOfDischarge", sydney.RL_PortName, ARInvoiceWrapper.PortOfDischarge);

			AgencyShipment agencyShipment = Factory.New<AgencyShipment>();
			agencyShipment.JS_UniqueConsignRef = "V00001122";
			agencyShipment.JS_NKDischargePort = singapore.Code;
			Invoice.AH_ConsolidatedInvoiceRef = agencyShipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(agencyShipment, Invoice);
			AssertEquals("PortOfDischarge", singapore.RL_PortName, ARInvoiceWrapper.PortOfDischarge);
		}

		public void TestDestination()
		{
			AssertEquals("Destination", ZString.Empty, ARInvoiceWrapper.Destination);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			AssertEquals("Destination", ZString.Empty, ARInvoiceWrapper.Destination);

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "L00003333";
			Invoice.AH_ConsolidatedInvoiceRef = loadList.JK_UniqueConsignRef;
			AssertEquals("Destination", ZString.Empty, ARInvoiceWrapper.Destination);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001122";
			var sydney = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			shipment.JS_RL_NKDestination = sydney.Code;
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(shipment, Invoice);
			AssertEquals("Destination", sydney.RL_PortName, ARInvoiceWrapper.Destination);

			Invoice.AH_JH = ZGuid.Empty;
			job.Delete();

			var cFSShipment = Factory.New<CFSShipment>();
			cFSShipment.JS_UniqueConsignRef = "H00003344";
			var singapore = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN");
			cFSShipment.JS_RL_NKDestination = singapore.Code;
			Invoice.AH_ConsolidatedInvoiceRef = cFSShipment.JS_UniqueConsignRef;

			job = GetInvoiceJob(cFSShipment, Invoice);
			AssertEquals("Destination", singapore.RL_PortName, ARInvoiceWrapper.Destination);

			Invoice.AH_JH = ZGuid.Empty;
			job.Delete();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00007777";
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			job = GetInvoiceJob(shipment, Invoice);
			AssertEquals("Destination", sydney.RL_PortName, ARInvoiceWrapper.Destination);

			Invoice.AH_JH = ZGuid.Empty;
			job.Delete();

			var containerRego = Factory.New<CFSContainer>();
			containerRego.JC_ContainerJobID = "D00006789";
			Invoice.AH_ConsolidatedInvoiceRef = containerRego.JC_ContainerJobID;
			AssertEquals("Destination", ZString.Empty, ARInvoiceWrapper.Destination);
		}

		public void TestDescription()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Description", ZString.Empty, aRInvoiceWrapper.Description);

			InvoicingBase.Lines.AddNew();
			InvoicingBase.Lines.AddNew();
			InvoicingBase.Lines.AddNew();
			string sequenceColumn = Enterprise.Accounting.Business.Base.Transaction.TransactionLine.Schema.AL_Sequence;
			string descColumn = Enterprise.Accounting.Business.Base.Transaction.TransactionLine.Schema.AL_Desc;
			InvoicingBase.Lines[0][sequenceColumn] = (ZShort)1;
			InvoicingBase.Lines[0][descColumn] = "Desc11\nDesc12";
			InvoicingBase.Lines[1][sequenceColumn] = (ZShort)2;
			InvoicingBase.Lines[1][descColumn] = "Desc21\nDesc22";
			InvoicingBase.Lines[2][sequenceColumn] = (ZShort)3;
			InvoicingBase.Lines[2][descColumn] = "Desc31\nDesc32";
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Description", "D", aRInvoiceWrapper.Description);

			aRInvoiceWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.DescriptionWidth, 3);
			AssertEquals("Description", "Des", aRInvoiceWrapper.Description);

			aRInvoiceWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.DescriptionWidth, 20);
			AssertEquals("Description", "Desc11", aRInvoiceWrapper.Description);

			aRInvoiceWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.DescriptionHeight, 2);
			AssertEquals("Description", "Desc11\nDesc21", aRInvoiceWrapper.Description);

			aRInvoiceWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.DescriptionHeight, 3);
			AssertEquals("Description", "Desc11\nDesc21\nDesc31", aRInvoiceWrapper.Description);

			aRInvoiceWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.DescriptionHeight, 4);
			AssertEquals("Description", "Desc11\nDesc21\nDesc31", aRInvoiceWrapper.Description);

			aRInvoiceWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.DescriptionWidth, 4);
			AssertEquals("Description", "Desc\nDesc\nDesc", aRInvoiceWrapper.Description);
		}

		public void TestAmount()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Amount", ZString.Empty, aRInvoiceWrapper.Amount);

			InvoicingBase.Lines.AddNew();
			InvoicingBase.Lines.AddNew();
			InvoicingBase.Lines.AddNew();
			string sequenceColumn = Enterprise.Accounting.Business.Base.Transaction.TransactionLine.Schema.AL_Sequence;
			string amountColumn = Enterprise.Accounting.Business.Base.Transaction.TransactionLine.Schema.AL_OSExTaxAmount;
			ZDecimal amount1 = new ZDecimal(333.33);
			ZDecimal amount2 = new ZDecimal(4444.44);
			ZDecimal amount3 = new ZDecimal(55555.55);
			InvoicingBase.Lines[0][sequenceColumn] = (ZShort)1;
			InvoicingBase.Lines[0][amountColumn] = amount1;
			InvoicingBase.Lines[1][sequenceColumn] = (ZShort)2;
			InvoicingBase.Lines[1][amountColumn] = amount2;
			InvoicingBase.Lines[2][sequenceColumn] = (ZShort)3;
			InvoicingBase.Lines[2][amountColumn] = amount3;
			string expected = amount1.ToString("N");
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Description", expected, aRInvoiceWrapper.Amount);

			aRInvoiceWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.DescriptionHeight, 2);
			expected = amount1.ToString("N") + "\n" + amount2.ToString("N");
			AssertEquals("Description", expected, aRInvoiceWrapper.Amount);

			aRInvoiceWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.DescriptionHeight, 3);
			expected = amount1.ToString("N") + "\n" + amount2.ToString("N") + "\n" + amount3.ToString("N");
			AssertEquals("Description", expected, aRInvoiceWrapper.Amount);

			aRInvoiceWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.DescriptionHeight, 4);
			AssertEquals("Description", expected, aRInvoiceWrapper.Amount);
		}

		public void TestPostedBy()
		{
			AssertEquals("User Name And Initials", GlbStaff.CurrentUser.GS_FullName + " (" + GlbStaff.CurrentUser.GS_Code + ")", ARInvoiceWrapper.PostedBy);
		}

		public void TestPostedByForInvoicePostedFromTransactionPendingAllocation()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			var testStaff = TestObjectCreator.CreateStaff("AAA");
			testStaff.GS_LoginName = "testAAA";
			Factory.Save();

			APInvoice invoiceTransaction = null;
			using (Env.SetTemporaryUserContext(testStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				invoiceTransaction = (APInvoice)TransactionAllocationConverter.ConvertUnallocatedToAP(transaction).Invoice;
				var line = (InvoicingLineBase)invoiceTransaction.Lines.AddNew();
				line.AL_OSExTaxAmount = invoiceTransaction.AH_OSExTaxAmount;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				invoiceTransaction.Factory.Save();
			}

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(invoiceTransaction, Factory);

			AssertEquals("User Name And Initials", $"{testStaff.GS_FullName} ({testStaff.GS_Code})", aRInvoiceWrapper.PostedBy);
		}

		public void TestCostConfirmationHeadingText()
		{
			AccountingConfigurationRegistry.Instance.CostConfirmationHeadingText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test Value!");
			AssertEquals("Cost Confirmation Heading Text", "Test Value!", ARInvoiceWrapper.CostConfirmationHeadingText);
		}

		public void TestCostConfirmationDocumentTitle()
		{
			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentTitle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test Value!");
			AssertEquals("Cost Confirmation Document Title", "Test Value!", ARInvoiceWrapper.CostConfirmationDocumentTitle);
		}

		public void TestConsolNumberForForwardingConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			AssertEquals("ConsolNumber", "C00001111", ARInvoiceWrapper.ConsolNumber);
		}

		public void TestConsolNumberForLoadListConsol()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "L00003333";
			Invoice.AH_ConsolidatedInvoiceRef = loadList.JK_UniqueConsignRef;
			AssertEquals("ConsolNumber", "L00003333", ARInvoiceWrapper.ConsolNumber);
		}

		public void TestConsolNumberForForwardingShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001122";
			shipment.Consols.Add(consol);
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(shipment, Invoice);

			AssertEquals("ConsolNumber", "C00001111", ARInvoiceWrapper.ConsolNumber);
		}

		public void TestConsolNumberForCFSShipment()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "L00003333";

			var cFSShipment = Factory.New<CFSShipment>();
			cFSShipment.JS_UniqueConsignRef = "H00003344";
			cFSShipment.Consols.Add(loadList);
			Invoice.AH_ConsolidatedInvoiceRef = cFSShipment.JS_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(cFSShipment, Invoice);

			AssertEquals("ConsolNumber", "L00003333", ARInvoiceWrapper.ConsolNumber);
		}

		public void TestConsolNumberForDeclarationAttachedToShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001122";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";

			shipment.Consols.Add(consol);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00007777";
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;

			JobHeader job = GetInvoiceJob(shipment, Invoice);
			AssertEquals("ConsolNumber", "C00001111", ARInvoiceWrapper.ConsolNumber);
		}

		public void TestConsolNumberForCFSContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";

			var containerRego = Factory.New<CFSContainer>();
			containerRego.JC_ContainerJobID = "D00006789";
			containerRego.JC_JK = consol.PK;
			Invoice.AH_ConsolidatedInvoiceRef = containerRego.JC_ContainerJobID;

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobContainerSchema.Constants.Prefix;
			job.JH_ParentID = containerRego.PK;
			Invoice.AH_JH = job.PK;

			AssertEquals("ConsolNumber", "C00001111", ARInvoiceWrapper.ConsolNumber);
		}

		public void TestConsolNumber()
		{
			AssertEquals("ConsolNumber", ZString.Empty, ARInvoiceWrapper.ConsolNumber);
		}

		public void TestShipmentNumber()
		{
			AssertEquals("ShipmentNumber", ZString.Empty, ARInvoiceWrapper.ShipmentNumber);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			AssertEquals("ShipmentNumber", ZString.Empty, ARInvoiceWrapper.ShipmentNumber);

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "L00003333";
			Invoice.AH_ConsolidatedInvoiceRef = loadList.JK_UniqueConsignRef;
			AssertEquals("ShipmentNumber", ZString.Empty, ARInvoiceWrapper.ShipmentNumber);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001122";
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(shipment, Invoice);
			AssertEquals("ShipmentNumber", "S00001122", ARInvoiceWrapper.ShipmentNumber);

			var cFSShipment = Factory.New<CFSShipment>();
			cFSShipment.JS_UniqueConsignRef = "H00003344";
			Invoice.AH_ConsolidatedInvoiceRef = cFSShipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(cFSShipment, Invoice);
			AssertEquals("ShipmentNumber", "H00003344", ARInvoiceWrapper.ShipmentNumber);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00007777";
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			job = GetInvoiceJob(shipment, Invoice);
			AssertEquals("ShipmentNumber", "S00001122", ARInvoiceWrapper.ShipmentNumber);

			var containerRego = Factory.New<CFSContainer>();
			containerRego.JC_ContainerJobID = "D00006789";
			Invoice.AH_ConsolidatedInvoiceRef = containerRego.JC_ContainerJobID;
			SetCFSContainerJob(containerRego, Invoice);
			AssertEquals("ShipmentNumber", ZString.Empty, ARInvoiceWrapper.ShipmentNumber);
		}

		public void TestHXDNumber()
		{
			AssertEquals("HXDNumber", ZString.Empty, ARInvoiceWrapper.HXDNumber);

			var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_GC = currentCompany.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			AssertEquals("HXDNumber", ZString.Empty, ARInvoiceWrapper.HXDNumber);

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "L00003333";
			Invoice.AH_ConsolidatedInvoiceRef = loadList.JK_UniqueConsignRef;
			AssertEquals("HXDNumber", ZString.Empty, ARInvoiceWrapper.HXDNumber);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001122";
			JobRequiredDocument hXDDoc = shipment.DocsAndCartage.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.HeXiaoDan);
			hXDDoc.EQ_DocNumber = "HXD1234";
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(shipment, Invoice);
			AssertEquals("HXDNumber", "HXD1234", ARInvoiceWrapper.HXDNumber);

			Invoice.AH_JH = ZGuid.Empty;
			job.Delete();

			var cFSShipment = Factory.New<CFSShipment>();
			cFSShipment.JS_UniqueConsignRef = "H00003344";
			hXDDoc = cFSShipment.DocsAndCartage.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.HeXiaoDan);
			hXDDoc.EQ_DocNumber = "HXD1234";
			Invoice.AH_ConsolidatedInvoiceRef = cFSShipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(cFSShipment, Invoice);
			AssertEquals("HXDNumber", "HXD1234", ARInvoiceWrapper.HXDNumber);

			Invoice.AH_JH = ZGuid.Empty;
			job.Delete();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00007777";
			declaration.JE_JS = shipment.PK;
			declaration.JE_GB = currentBranch.PK;
			Factory.Save();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			job = GetInvoiceJob(shipment, Invoice);
			AssertEquals("HXDNumber", "HXD1234", ARInvoiceWrapper.HXDNumber);

			var containerRego = Factory.New<CFSContainer>();
			containerRego.JC_ContainerJobID = "D00006789";
			Invoice.AH_ConsolidatedInvoiceRef = containerRego.JC_ContainerJobID;

			SetCFSContainerJob(containerRego, Invoice);

			AssertEquals("HXDNumber", ZString.Empty, ARInvoiceWrapper.HXDNumber);
		}

		public void TestJobChargesExchangeRateAndCurrency()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = "ZUB";
			currency.RX_SubUnitRatio = 100;

			RefCurrency currency2 = Factory.NewWithValidTestData<RefCurrency>();
			currency2.RX_Code = "RAK";
			currency2.RX_SubUnitRatio = 1;

			Invoice = Factory.New<ARInvoice>();

			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line1.AL_Desc = "Origin Charge 1";
			line1.AL_OSExTaxAmount = 104.00M;
			line1.AL_LocalExTaxAmount = 175M;
			line1.AL_AC = chargeCode1.PK;
			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_RX_NKSellCurrency = currency.RX_Code;
			charge1.JR_OSSellAmt = 175M;
			charge1.JR_OSSellExRate = 1.33M;
			charge1.JR_AL_ARLine = line1.PK;

			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line2.AL_Desc = "Origin Charge 2";
			line2.AL_OSExTaxAmount = 150.00M;
			line2.AL_LocalExTaxAmount = 200M;
			line2.AL_AC = chargeCode2.PK;
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_RX_NKSellCurrency = currency2.RX_Code;
			charge2.JR_OSSellAmt = 200M;
			charge2.JR_OSSellExRate = 1M;
			charge2.JR_AL_ARLine = line2.PK;

			DocARInvoice invoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertContains("JobChargesExchangeRateAndCurrency contains currency and exchange rate of charge 1", "ZUB 1.3300", invoiceWrapper.JobChargesExchangeRateAndCurrency);
			AssertContains("JobChargesExchangeRateAndCurrency contains currency and exchange rate of charge 2", "RAK 1.0000", invoiceWrapper.JobChargesExchangeRateAndCurrency);
		}

		public void TestIsCompanyRegisteredForGST()
		{
			AssertEquals("IsCompanyRegisteredForGST", GlbCompany.CurrentCompany.GC_IsGSTRegistered, ARInvoiceWrapper.IsCompanyRegisteredForGST);
		}

		public void TestShowOperatorsName()
		{
			AssertEquals(AccountingConfigurationRegistry.Instance.ShowOperatorsNameOnInvoice.Value, ARInvoiceWrapper.ShowOperatorsName);
		}

		public void TestShowLocalAmountAndExRateOnInvoice()
		{
			AssertEquals("Show Local Amount + Ex Rate", false, ARInvoiceWrapper.ShowLocalAmountAndExRateOnInvoice);

			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader header = orgFactory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("To create CompanyData", header.CompanyData);
			orgFactory.Save();
			Invoice.AH_OH = header.PK;
			Invoice.AH_JH = Factory.NewJobWithValidTestDataForTesting<JobHeader>().PK;

			Assert("!ShowLocalAmountAndExRateOnInvoice", !ARInvoiceWrapper.ShowLocalAmountAndExRateOnInvoice);

			header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup group = header.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			orgFactory.Save();
			AssertEquals("Show Local Amount + Ex Rate", true, ARInvoiceWrapper.ShowLocalAmountAndExRateOnInvoice);

			AssertNull("Consol", ARInvoiceWrapper.Consol);
			Invoice.Delete();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Consol", ((DocARInvoice)InvoiceWrapper).Consol);
			Assert("JobHeader is not set", Invoice.AH_JH.IsEmpty);
			Assert("OrgHeader is not set", Invoice.AH_OH.IsEmpty);
			AssertEquals("ShowLocalAmountAndExRateOnInvoice should be false", false, InvoiceWrapper.ShowLocalAmountAndExRateOnInvoice);

			Invoice.AH_OH = header.PK;
			AssertNotNull(Invoice.Header);
			AssertEquals("ShowLocalAmountAndExRateOnInvoice should be true when OrgHeader and Consol are not null", true, InvoiceWrapper.ShowLocalAmountAndExRateOnInvoice);

			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			orgFactory.Save();
			AssertEquals("ShowLocalAmountAndExRateOnInvoice should be false because we changed PG_InvoiceLineDisplayOption", false, InvoiceWrapper.ShowLocalAmountAndExRateOnInvoice);
		}

		public void TestRecipientTaxIDForZA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				AssertEquals("Recipient Tax ID Heading before setup", ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number before setup", ZString.Empty, InvoiceWrapper.RecipientTaxIDNumber);

				ZString companyRegistrationNumber = "123456789";
				ZString vATNumber = "4120232253";
				ZString orgCode = "ORGCOD";

				var debtor = Factory.New<OrgHeader>();
				debtor.OH_Code = orgCode;
				debtor.OH_IsDebtor = new ZBool(true);
				debtor.CompanyData.SetARTaxApplicable(ZBool.True);
				debtor.PrimaryRegistrationNumber.Number = companyRegistrationNumber;
				debtor.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, GlbCompany.CurrentCompany.Country, vATNumber);
				Invoice.AH_OH = debtor.PK;

				((InvoicingLineBase)InvoicingBase.Lines.AddNew()).AL_AT = Factory.NewWithValidTestData(typeof(AccTaxRate)).PK;
				Factory.ClearCachedValue<ZBool>(InvoicingBase.PK.ToStringKey());
				InvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals("Recipient Tax ID Heading", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID", vATNumber, InvoiceWrapper.RecipientTaxIDNumber);
			}
		}

		public void TestDocwrappperMacrosForGA()
		{
			AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var testObjectCreator = new TestObjectCreator(Factory);

			var countryCode = Constants.CountryCodes.Gabon;
			GlbCompany.CurrentCompany.SetCountry(countryCode);

			var org = testObjectCreator.CreateOrgHeader("ORGHDR", true, true);
			testObjectCreator.SetCustomsCodeForOrgHeader(org, GabonOrgCusCodeInfo.OrgCusCodes.NIF, countryCode, "4444");
			testObjectCreator.SetCustomsCodeForOrgHeader(org, OrgCusCode.CodeTypes.TVACode, countryCode, "55555");
			testObjectCreator.SetCustomsCodeForOrgHeader(org, GabonOrgCusCodeInfo.OrgCusCodes.RCM, countryCode, "666666");
			Invoice.AH_OH = org.PK;

			var taxRate = testObjectCreator.CreateTaxRate("TVA10", "TVA 10%", 10, 1, countryCode);
			var line = testObjectCreator.CreateInvoiceLine((InvoicingBase)Invoice, testObjectCreator.AUD, 1M, 100M, GlbBranch.CurrentBranch.PK);
			line.AL_AT = taxRate.PK;

			var orgProxy = Invoice.Branch.OrgProxy;
			testObjectCreator.SetCustomsCodeForOrgHeader(orgProxy, GabonOrgCusCodeInfo.OrgCusCodes.NIF, countryCode, "1");
			testObjectCreator.SetCustomsCodeForOrgHeader(orgProxy, OrgCusCode.CodeTypes.TVACode, countryCode, "22");
			testObjectCreator.SetCustomsCodeForOrgHeader(orgProxy, GabonOrgCusCodeInfo.OrgCusCodes.RCM, countryCode, "333");

			Factory.Save();

			var invoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);

			AssertEquals("RecipientLocalBusinessRegHeading", "Client NIF #", invoiceWrapper.RecipientLocalBusinessRegHeading);
			AssertEquals("RecipientLocalBusinessRegNumber", "4444", invoiceWrapper.RecipientLocalBusinessRegNumber);

			AssertEquals("RecipientTaxIDHeading", "Client TVA #:", invoiceWrapper.RecipientTaxIDHeading);
			AssertEquals("RecipientTaxIDNumber", "55555", invoiceWrapper.RecipientTaxIDNumber);

			AssertEquals("TaxId", "NIF #: 1 RCCM #: 333", invoiceWrapper.TaxId);
		}

		public void TestToString()
		{
			ZString transactionNumber = new ZString("TransNum");
			Invoice.AH_TransactionNum = transactionNumber;
			AssertEquals("TransactionNumber", transactionNumber, InvoiceWrapper.ToString());
		}

		public void TestLines()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Lines count", 0, aRInvoiceWrapper.Lines.Count);

			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Lines count", 1, aRInvoiceWrapper.Lines.Count);

			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			InvoicingLineBase line3 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Lines count", 3, aRInvoiceWrapper.Lines.Count);
		}

		public void TestInvoiceFooterMessage()
		{
			ZString expectedResult = "Please return a copy of this " + ARInvoiceWrapper.DocumentTitle.ToLower() + " with your payment if paying by cheque";
			AssertEquals("Invoice Footer Message", expectedResult, ARInvoiceWrapper.InvoiceFooterMessage);

			Invoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
			expectedResult = "Please return a copy of this " + ARInvoiceWrapper.DocumentTitle.ToLower() + " with your payment if paying by cheque";
			AssertEquals("Invoice Footer Message", expectedResult, ARInvoiceWrapper.InvoiceFooterMessage);

			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			expectedResult = "Please return a copy of this " + ARInvoiceWrapper.DocumentTitle.ToLower() + " with your payment if paying by cheque";
			AssertEquals("Invoice Footer Message", expectedResult, ARInvoiceWrapper.InvoiceFooterMessage);
		}

		public void TestJobInvoiceNumber()
		{
			bool oldUseJobNumberBasedInvoiceNumbers = AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.Value;
			string oldInvoiceTransactionNumberPrefix = AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.Value;
			try
			{
				Invoice.AH_TransactionNum = "1234";

				AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AssertEquals("AH_ConsolidatedInvoiceRef", ZString.Empty, Invoice.AH_ConsolidatedInvoiceRef);
				AssertEquals("Consolidated inovice ref is empty, so we don't care about registry", "1234", ARInvoiceWrapper.JobInvoiceNumber);

				Invoice.AH_ConsolidatedInvoiceRef = "S12345";
				AssertEquals("should be equal to Consolidated inovice ref", "S12345", ARInvoiceWrapper.JobInvoiceNumber);

				AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				AssertEquals("Should be equal to invoice number", "1234", ARInvoiceWrapper.JobInvoiceNumber);
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABC");
				AssertEquals("Should be equal to invoice number", "ABC1234", ARInvoiceWrapper.JobInvoiceNumber); // this is thoroughly tested in Accounting; here we just making sure that the right property is used
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldUseJobNumberBasedInvoiceNumbers);
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldInvoiceTransactionNumberPrefix);
			}
		}

		public void TestPaymentReference()
		{
			string oldInvoiceTransactionNumberPrefix = AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABC");
				Invoice.AH_TransactionNum = "1234";
				ZString paymentReference = ARInvoiceWrapper.PaymentReference;
				Assert("PaymentReference", paymentReference == InvoicingBase.TransactionNumberPrefixed || paymentReference == ZString.Empty);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldInvoiceTransactionNumberPrefix);
			}
		}

		public void TestMessage()
		{
			AssertEquals("Invoice message", AccountingConfigurationRegistry.Instance.InvoiceMessage.Value, ARInvoiceWrapper.Message.ToString());

			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			AssertEquals("Invoice and Disbursement message", AccountingConfigurationRegistry.Instance.DisbursementMessage.Value, ARInvoiceWrapper.Message.ToString());

			Invoice.AH_TransactionCategory = "";
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			AssertEquals("Credit note message", AccountingConfigurationRegistry.Instance.CreditNoteMessage.Value, ARInvoiceWrapper.Message.ToString());

			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.AdjustmentNote;
			AssertEquals("Adjustment note message", AccountingConfigurationRegistry.Instance.AdjustmentNoteMessage.Value, ARInvoiceWrapper.Message.ToString());
		}

		public void TestCopy()
		{
			InvoiceCopyCollection collection = (InvoiceCopyCollection)AccountingConfigurationRegistry.Instance.InvoiceCopies.Value.Clone(null, null);

			AssertEquals("", ARInvoiceWrapper.Copy);
			AccountingConfigurationRegistry.Instance.ShouldInvoiceShowCopyWhenPrintedSubsequentTimes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Invoice.AH_InvoicePrinted = true;
			AssertEquals("Copy", ARInvoiceWrapper.Copy);
			AccountingConfigurationRegistry.Instance.ShouldInvoiceShowCopyWhenPrintedSubsequentTimes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			foreach (InvoiceCopy entry in collection)
			{
				if (entry.IsOriginal)
				{
					entry.Name = (NoResString)"Original";
				}
			}

			AccountingConfigurationRegistry.Instance.InvoiceCopies.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Copy should take the information from the registry item", "Original", ARInvoiceWrapper.Copy);

			Invoice.AH_InvoicePrinted = true;
			AccountingConfigurationRegistry.Instance.ShouldInvoiceShowCopyWhenPrintedSubsequentTimes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Copy", ARInvoiceWrapper.Copy);
		}

		public void TestCreditTerms()
		{
			CodeDescriptionPairList termList = new InvoiceTermsListWithShortDescription();
			Invoice.AH_PostDate = ZDateTime.Today;
			Invoice.AH_InvoiceDate = ZDateTime.Today;
			Invoice.AH_InvoiceTermDays = 12;
			var expected = termList.GetMultilingualDescriptionFromCode("COD");
			AssertEquals("Credit Terms", expected, ARInvoiceWrapper.CreditTerms);

			Invoice.AH_TransactionType = TransactionTypes.CreditNote;
			AssertEquals("Credit Terms for CRD", ZString.Empty, ARInvoiceWrapper.CreditTerms);

			Invoice.AH_TransactionType = TransactionTypes.Invoice;
			Invoice.AH_InvoiceTerm = "INV";
			expected = (NoResString)string.Format(termList.GetDescriptionFromCode(Invoice.AH_InvoiceTerm).Trim(), Invoice.AH_InvoiceTermDays.ToString());
			AssertEquals("Credit Terms INV", expected, ARInvoiceWrapper.CreditTerms);

			Invoice.AH_InvoiceTerm = "COD";
			expected = termList.GetMultilingualDescriptionFromCode(Invoice.AH_InvoiceTerm);
			AssertEquals("Credit Terms COD", expected, ARInvoiceWrapper.CreditTerms);

			Invoice.AH_InvoiceTerm = "PIA";
			expected = termList.GetMultilingualDescriptionFromCode(Invoice.AH_InvoiceTerm);
			AssertEquals("Credit Terms PIA", expected, ARInvoiceWrapper.CreditTerms);

			Invoice.AH_InvoiceTerm = "MLI";
			expected = termList.GetMultilingualDescriptionFromCode(Invoice.AH_InvoiceTerm);
			AssertEquals("Credit Terms MLI", expected, ARInvoiceWrapper.CreditTerms);

			Invoice.AH_InvoiceTerm = "XXX";
			expected = (NoResString)"0 days ";
			AssertEquals("Credit Terms unknown", expected, ARInvoiceWrapper.CreditTerms);
			AssertEquals("Developer exception must be raised.", "Can not determine description from invoice term code 'XXX'.", ErrorReporter.LastMessageReported);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestOSTaxDisplayHeadingForCanada()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			Assert("Tax display heading", aRInvoiceWrapper.OSTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			AccTaxRate gstAndQst = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndQst.AT_Type = AccTaxRate.Types.Rated;
			gstAndQst.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			gstAndQst.SetRateNumerator_ForTestOnly(5);
			gstAndQst.SetExtraRate_ForTestOnly(5, 1);
			ARInvoiceLine line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AT = gstAndQst.PK;
			InvoicingBase.Lines.Add(line);
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Tax display heading", "GST/QST", aRInvoiceWrapper.OSTaxDisplayHeading);
		}

		public void TestOSTaxDisplayHeading()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_OSExTaxAmount = 10;

			var arInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(ZString.Empty, arInvoiceWrapper.OSTaxDisplayHeading);

			line.AL_AT = TestObjectCreator.GST1.PK;
			arInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Factory.ClearCachedValue<ZBool>(InvoicingBase.PK.ToStringKey());
			AssertEquals("GST", arInvoiceWrapper.OSTaxDisplayHeading);
		}

		public void TestOSTaxDisplayHeadingForMalaysia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malaysia))
			{
				var header = CreateOrgHeaderForMalaysia();
				Invoice.AH_OH = header.PK;

				var taxRate1 = Factory.New<AccTaxRate>();
				var taxRate2 = Factory.New<AccTaxRate>();

				InvoicingBase.Lines.AddNew().AL_AT = taxRate1.PK;
				InvoicingBase.Lines.AddNew().AL_AT = taxRate2.PK;

				var invoiceWrapper = DocARInvoice.New(Invoice, Factory);

				taxRate1.AT_Code = "GST";
				taxRate1.AT_ExtraTaxRateType = ZString.Empty;
				taxRate2.AT_Code = "GST";
				taxRate2.AT_ExtraTaxRateType = ZString.Empty;
				AssertEquals("GST", invoiceWrapper.OSTaxDisplayHeading);

				taxRate1.AT_Code = "GST";
				taxRate1.AT_ExtraTaxRateType = ZString.Empty;
				taxRate2.AT_Code = "SVC";
				taxRate2.AT_ExtraTaxRateType = "SER";
				AssertEquals("GST", invoiceWrapper.OSTaxDisplayHeading);

				taxRate1.AT_Code = "SVC";
				taxRate1.AT_ExtraTaxRateType = "SER";
				taxRate2.AT_Code = "SVC";
				taxRate2.AT_ExtraTaxRateType = "SER";
				AssertEquals("SERVICE TAX", invoiceWrapper.OSTaxDisplayHeading);

				taxRate1.AT_Code = "XXX";
				taxRate1.AT_ExtraTaxRateType = "TST";
				taxRate2.AT_Code = "XXX";
				taxRate2.AT_ExtraTaxRateType = "TST";
				AssertEquals("SERVICE TAX", invoiceWrapper.OSTaxDisplayHeading);
			}
		}

		public void TestOSTaxDisplayHeadingQCT()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			Assert("Tax display heading", aRInvoiceWrapper.OSTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			AccTaxRate gstAndQst = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndQst.AT_Type = AccTaxRate.Types.Rated;
			gstAndQst.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			gstAndQst.SetRateNumerator_ForTestOnly(5);
			gstAndQst.SetExtraRate_ForTestOnly(5, 1);
			ARInvoiceLine line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AT = gstAndQst.PK;
			InvoicingBase.Lines.Add(line);
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Tax display heading", "GST/QST", aRInvoiceWrapper.OSTaxDisplayHeading);
		}

		public void TestOSTaxDisplayHeadingIGIC()
		{
			if (InvoicingBase is ARInvoice)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
				{
					var igic = Factory.NewWithValidTestData<AccTaxRate>();
					igic.AT_RN_NKCountry = Core.Constants.CountryCodes.Spain;
					igic.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.RegionalTax;

					var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
					line.AL_AT = igic.PK;

					var wrapper = (DocARInvoiceCommon)GetBaseInvoiceWrapper();
					AssertEquals("Tax display heading", "IGIC", wrapper.OSTaxDisplayHeading);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestOSTaxDisplayHeadingHST()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			Assert("Tax display heading", aRInvoiceWrapper.OSTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			Assert("Primary tax display heading", aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);

			AccTaxRate hst = Factory.NewWithValidTestData<AccTaxRate>();
			hst.AT_RN_NKCountry = Core.Constants.CountryCodes.Canada;
			hst.AT_Code = "HST12";
			hst.AT_Type = AccTaxRate.Types.Rated;
			hst.SetRateNumerator_ForTestOnly(12);

			ARInvoiceLine line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AT = hst.PK;
			InvoicingBase.Lines.Add(line);

			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Tax display heading", "GST/HST", aRInvoiceWrapper.OSTaxDisplayHeading);
			AssertEquals("Primary tax display heading", "GST/HST", aRInvoiceWrapper.OSPrimaryTaxDisplayHeading);

			AccTaxRate gstAndQst = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndQst.AT_RN_NKCountry = Core.Constants.CountryCodes.Canada;
			gstAndQst.AT_Type = AccTaxRate.Types.Rated;
			gstAndQst.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			gstAndQst.SetRateNumerator_ForTestOnly(5);
			gstAndQst.SetExtraRate_ForTestOnly(5, 1);

			line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AT = gstAndQst.PK;
			InvoicingBase.Lines.Add(line);

			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Tax display heading", "GST/HST/QST", aRInvoiceWrapper.OSTaxDisplayHeading);
			AssertEquals("Primary tax display heading", "GST/HST", aRInvoiceWrapper.OSPrimaryTaxDisplayHeading);
		}

		public void TestOSTaxDisplayHeadingNHIL()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			Assert("Tax display heading", aRInvoiceWrapper.OSTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			Assert("Primary tax display heading", aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Ghana;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			taxRate.AT_Code = "VATNHIL";
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.SetRateNumerator_ForTestOnly(15);
			taxRate.SetExtraRate_ForTestOnly(25, 10);

			var line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AT = taxRate.PK;
			InvoicingBase.Lines.Add(line);

			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Tax display heading", "VAT/NHIL/GETFL", aRInvoiceWrapper.OSTaxDisplayHeading);
		}

		public void TestOSTaxDisplayHeadingSurtax()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
				Assert("Tax display heading", aRInvoiceWrapper.OSTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
				Assert("Primary tax display heading", aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);

				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Congo;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				taxRate.AT_Code = "CAPTVASTX";
				taxRate.AT_Type = AccTaxRate.Types.CapitalRated;
				taxRate.SetRateNumerator_ForTestOnly(15);
				taxRate.SetExtraRate_ForTestOnly(25, 10);

				var line = Factory.NewWithValidTestData<ARInvoiceLine>();
				line.AL_AT = taxRate.PK;
				InvoicingBase.Lines.Add(line);

				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("Tax display heading", "TVA/SURTAX", aRInvoiceWrapper.OSTaxDisplayHeading);
			}
		}

		public void TestOSTaxDisplayHeadingCSS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Gabon))
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
				Assert("Tax display heading", aRInvoiceWrapper.OSTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
				Assert("Primary tax display heading", aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);

				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Gabon;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				taxRate.AT_Code = "TVACSS";
				taxRate.AT_Type = AccTaxRate.Types.Rated;
				taxRate.SetRateNumerator_ForTestOnly(15);
				taxRate.SetExtraRate_ForTestOnly(25, 10);

				var line = Factory.NewWithValidTestData<ARInvoiceLine>();
				line.AL_AT = taxRate.PK;
				InvoicingBase.Lines.Add(line);

				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("Tax display heading", "TVA/CSS", aRInvoiceWrapper.OSTaxDisplayHeading);
			}
		}

		public void TestOSTaxExtraRateQCTDisPlay()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Canada;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.SetRateNumerator_ForTestOnly(15);
			taxRate.SetExtraRate_ForTestOnly(25, 10);

			var line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AT = taxRate.PK;
			InvoicingBase.Lines.Add(line);

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Should show QST", "QST", aRInvoiceWrapper.OSTaxExtraRateQCTDisPlay);

			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Ghana;
			taxRate.AT_Code = "VATNHIL";

			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Should show NHIL/GETFL", "NHIL/GETFL", aRInvoiceWrapper.OSTaxExtraRateQCTDisPlay);

			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			AssertEquals("Should show CESS", "CESS", aRInvoiceWrapper.OSTaxExtraRateQCTDisPlay);
		}

		public void TestOSTaxExtraRateQCTSBCKKCDisPlay_India()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.India);

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.SetRateNumerator_ForTestOnly(15);
			taxRate.SetExtraRate_ForTestOnly(1, 1);

			var line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AT = taxRate.PK;
			InvoicingBase.Lines.Add(line);

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Should show SBC", "SBC", aRInvoiceWrapper.OSTaxExtraRateSBCDisPlay);
			AssertEquals("Should show KKC", "KKC", aRInvoiceWrapper.OSTaxExtraRateKKCDisPlay);
		}

		public void TestOSTaxExtraRateQCTSBCKKCDisPlay_SER_India()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.India);

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			taxRate.AT_Type = AccTaxRate.Types.ServiceTax;
			taxRate.SetRateNumerator_ForTestOnly(15);
			taxRate.SetExtraRate_ForTestOnly(1, 1);

			var line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AT = taxRate.PK;
			InvoicingBase.Lines.Add(line);

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Should show SBC", "SBC", aRInvoiceWrapper.OSTaxExtraRateSBCDisPlay);
			AssertEquals("Should show KKC", "KKC", aRInvoiceWrapper.OSTaxExtraRateKKCDisPlay);
		}

		[TestDate(2019, 5, 29)]
		public void TestInvoiceTaxDate()
		{
			var line1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			AssertNull(line1.TaxRate);
			InvoicingBase.Lines.Add(line1);

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			Assert("When transaction has NO TAX IDS, print blank", aRInvoiceWrapper.InvoiceTaxDate.IsEmpty);

			var line2 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line2.AL_AT = GetRate();
			line2.AL_TaxDate = new ZDate(2019, 05, 25);
			AssertNotNull(line2.TaxRate);
			InvoicingBase.Lines.Add(line2);

			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals(line2.AL_TaxDate.ToZDateTime(), aRInvoiceWrapper.InvoiceTaxDate);

			var line3 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line3.AL_AT = GetRate();
			line3.AL_TaxDate = new ZDate(2019, 05, 15);
			AssertNotNull(line3.TaxRate);
			InvoicingBase.Lines.Add(line3);

			var line4 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line4.AL_AT = GetRate();
			line4.AL_TaxDate = new ZDate(2019, 05, 20);
			AssertNotNull(line4.TaxRate);
			InvoicingBase.Lines.Add(line4);

			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals(line3.AL_TaxDate.ToZDateTime(), aRInvoiceWrapper.InvoiceTaxDate);

			var line5 = Factory.NewWithValidTestData<ARInvoiceLine>();
			line5.AL_AC = TestObjectCreator.CommentChargeCode.PK;
			line5.AL_AT = GetRate();
			line5.AL_TaxDate = new ZDate(2019, 05, 10);
			AssertNotNull(line5.TaxRate);
			Assert(line5.IsCommentCharge);
			InvoicingBase.Lines.Add(line5);

			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals(line3.AL_TaxDate.ToZDateTime(), aRInvoiceWrapper.InvoiceTaxDate);

			using (AccountingConfigurationRegistry.Instance.PrintTaxDateInARInvoiceDocument.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInHeaderLatest.Code))
			{
				AssertEquals(line2.AL_TaxDate.ToZDateTime(), aRInvoiceWrapper.InvoiceTaxDate);
			}

			line2.AL_TaxDate = ZDate.Empty;
			line3.AL_TaxDate = ZDate.Empty;
			line4.AL_TaxDate = ZDate.Empty;

			var expectedInvoiceDate = new ZDate(2019, 5, 29);
			AssertEquals(expectedInvoiceDate, InvoicingBase.AH_InvoiceDate);
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals(expectedInvoiceDate, aRInvoiceWrapper.InvoiceTaxDate);
		}

		public void TestInvoiceTaxDateHeading()
		{
			var line1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			InvoicingBase.Lines.Add(line1);

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			var expectedLabel = InvoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable ? "Date of Supply" : "Tax Date";
			AssertEquals(expectedLabel, aRInvoiceWrapper.InvoiceTaxDateHeading);
		}

		public void TestOSSPVExtraTaxLabel()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.CostaRica))
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "CRSJO";
				SetupAndAssertOSSPVExtraTaxLabel("EXONERATION");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "ITROM";
				SetupAndAssertOSSPVExtraTaxLabel("SPLIT PAYMENT");
			}
		}

		public void TestOSSPVExtraTaxLabelWhenOrgProxyCountryIsMissing()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_RN_NKCountryCode = "";
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_RL_NKRelatedPortCode = "";
			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "";

			AssertNull(GlbCompany.CurrentCompany.OrgProxy.MainAddress.RelatedCountry);

			SetupAndAssertOSSPVExtraTaxLabel("");
		}

		void SetupAndAssertOSSPVExtraTaxLabel(string expectedLabel)
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRemittedByCustomer;
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.SetRateNumerator_ForTestOnly(13);

			var line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AT = taxRate.PK;
			InvoicingBase.Lines.Add(line);

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Label for " + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, expectedLabel, aRInvoiceWrapper.OSSPVExtraTaxLabel);
		}

		public void TestOSSPVExtraTaxCodeLabel()
		{
			Action<ZString> setupAndAssertOSSPVExtraTaxCodeLabel = (expectedLabel) =>
			{
				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRemittedByCustomer;
				taxRate.AT_Type = AccTaxRate.Types.Rated;
				taxRate.SetRateNumerator_ForTestOnly(13);

				var line = Factory.NewWithValidTestData<ARInvoiceLine>();
				line.AL_AT = taxRate.PK;
				InvoicingBase.Lines.Add(line);

				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("Label for " + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, expectedLabel, aRInvoiceWrapper.OSSPVExtraTaxCodeLabel);
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.CostaRica))
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "CRSJO";
				setupAndAssertOSSPVExtraTaxCodeLabel("Exon.");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "ITROM";
				setupAndAssertOSSPVExtraTaxCodeLabel("SPV");
			}
		}

		public void TestOSTaxDisplayHeadingQCT_India()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.India);

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			Assert("Tax display heading", aRInvoiceWrapper.OSTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			AccTaxRate gstAndQst = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndQst.AT_RN_NKCountry = Constants.CountryCodes.India;
			gstAndQst.AT_Type = AccTaxRate.Types.Rated;
			gstAndQst.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			gstAndQst.SetRateNumerator_ForTestOnly(5);
			gstAndQst.SetExtraRate_ForTestOnly(5, 1);
			ARInvoiceLine line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AT = gstAndQst.PK;
			InvoicingBase.Lines.Add(line);
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Tax display heading", "SER/CESS", aRInvoiceWrapper.OSTaxDisplayHeading);
		}

		public void TestOSTaxDisplayHeadingQCT_SER_India()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.India);

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			Assert("Tax display heading", aRInvoiceWrapper.OSTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			AccTaxRate gstAndQst = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndQst.AT_RN_NKCountry = Constants.CountryCodes.India;
			gstAndQst.AT_Type = AccTaxRate.Types.ServiceTax;
			gstAndQst.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			gstAndQst.SetRateNumerator_ForTestOnly(5);
			gstAndQst.SetExtraRate_ForTestOnly(5, 1);
			ARInvoiceLine line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AT = gstAndQst.PK;
			InvoicingBase.Lines.Add(line);
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Tax display heading", "SER/CESS", aRInvoiceWrapper.OSTaxDisplayHeading);
		}

		public void TestOSTaxDisplayHeadingEDU()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.India);

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			Assert("Tax display heading", aRInvoiceWrapper.OSTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			Assert("Primary tax display heading", aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);

			AccTaxRate gstAndEdu = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndEdu.AT_RN_NKCountry = Constants.CountryCodes.India;
			gstAndEdu.AT_Type = AccTaxRate.Types.Rated;
			gstAndEdu.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
			gstAndEdu.SetRateNumerator_ForTestOnly(3);
			gstAndEdu.SetExtraRate_ForTestOnly(3, 1);
			ARInvoiceLine lineEDU = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineEDU = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineEDU.AL_AT = gstAndEdu.PK;
			InvoicingBase.Lines.Add(lineEDU);
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Tax display heading", "SER/EDU", aRInvoiceWrapper.OSTaxDisplayHeading);
		}

		public void TestOSTaxDisplayHeadingEDU_SER()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.India);

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			Assert("Tax display heading", aRInvoiceWrapper.OSTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			Assert("Primary tax display heading", aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);

			AccTaxRate gstAndEdu = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndEdu.AT_RN_NKCountry = Constants.CountryCodes.India;
			gstAndEdu.AT_Type = AccTaxRate.Types.ServiceTax;
			gstAndEdu.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
			gstAndEdu.SetRateNumerator_ForTestOnly(3);
			gstAndEdu.SetExtraRate_ForTestOnly(3, 1);
			ARInvoiceLine lineEDU = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineEDU = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineEDU.AL_AT = gstAndEdu.PK;
			InvoicingBase.Lines.Add(lineEDU);
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Tax display heading", "SER/EDU", aRInvoiceWrapper.OSTaxDisplayHeading);
		}

		public void TestOSTaxDisplayHeadingQCTAndEDUAndIGSTAndCSGST()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
				Assert("Tax display heading", aRInvoiceWrapper.OSTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
				Assert("Primary tax display heading", aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);

				AccTaxRate gstAndEdu = Factory.NewWithValidTestData<AccTaxRate>();
				gstAndEdu.AT_RN_NKCountry = Constants.CountryCodes.India;
				gstAndEdu.AT_Type = AccTaxRate.Types.Rated;
				gstAndEdu.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				gstAndEdu.SetRateNumerator_ForTestOnly(3);
				gstAndEdu.SetExtraRate_ForTestOnly(3, 1);
				AccTaxRate gstAndQct = Factory.NewWithValidTestData<AccTaxRate>();
				gstAndQct.AT_RN_NKCountry = Constants.CountryCodes.India;
				gstAndQct.AT_Type = AccTaxRate.Types.Rated;
				gstAndQct.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				gstAndQct.SetRateNumerator_ForTestOnly(3);
				gstAndQct.SetExtraRate_ForTestOnly(3, 1);
				var lineEDU = Factory.NewWithValidTestData<ARInvoiceLine>();
				lineEDU.AL_AT = gstAndEdu.PK;
				var lineQCT = Factory.NewWithValidTestData<ARInvoiceLine>();
				lineQCT.AL_AT = gstAndQct.PK;
				var lineIGST = Factory.NewWithValidTestData<ARInvoiceLine>();
				lineIGST.AL_AT = TestObjectCreator.IntegratedGST.PK;
				var lineStateGST = Factory.NewWithValidTestData<ARInvoiceLine>();
				lineStateGST.AL_AT = TestObjectCreator.STAGST.PK;
				InvoicingBase.Lines.Add(lineEDU);
				InvoicingBase.Lines.Add(lineQCT);
				InvoicingBase.Lines.Add(lineIGST);
				InvoicingBase.Lines.Add(lineStateGST);
				aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("Tax display heading", "SER/CESS/EDU/IGST/CGST/SGST", aRInvoiceWrapper.OSTaxDisplayHeading);
			}
		}

		public void TestOSTaxDisplayHeadingIGSTAndCSGST()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var lineIGST = Factory.NewWithValidTestData<ARInvoiceLine>();
				lineIGST.AL_AT = TestObjectCreator.IntegratedGST.PK;
				var lineStateGST = Factory.NewWithValidTestData<ARInvoiceLine>();
				lineStateGST.AL_AT = TestObjectCreator.STAGST.PK;

				InvoicingBase.Lines.Add(lineIGST);
				InvoicingBase.Lines.Add(lineStateGST);

				var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
				AssertEquals("Tax display heading", "IGST/CGST/SGST", aRInvoiceWrapper.OSTaxDisplayHeading);
			}
		}

		public void TestOSTaxDisplayHeadingWithZeroAmountIGST()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.India);

			//Check EXL
			var exlTaxRate = TestObjectCreator.CreateTaxRate("EXL", "Excluded", AccTaxRate.Types.ExcludedFromTheTaxBase, 0, string.Empty, 0, 1);
			var lineEXL1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineEXL1.AL_AT = exlTaxRate.PK;

			InvoicingBase.Lines.Add(lineEXL1);

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			Assert(aRInvoiceWrapper.HasZeroIGSTAmount);
			AssertEquals("Tax display heading", "IGST", aRInvoiceWrapper.OSTaxDisplayHeading);

			InvoicingBase.Lines.Remove(lineEXL1);

			//Check NOT
			var notTaxRate_Service = TestObjectCreator.CreateTaxRate("NOT", "Not Reported", AccTaxRate.Types.NotReportable, 0, AccTaxRate.ExtraTypes.ServiceTax, 0, 1);

			var lineNot = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineNot.AL_AT = notTaxRate_Service.PK;

			InvoicingBase.Lines.Add(lineNot);

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(!aRInvoiceWrapper.HasZeroIGSTAmount);
			AssertEquals("Tax display heading", "SER", aRInvoiceWrapper.OSTaxDisplayHeading);

			var notTaxRate = TestObjectCreator.CreateTaxRate("NOT", "Not Reported", AccTaxRate.Types.NotReportable, 0, string.Empty, 0, 1);

			var intTaxRate = TestObjectCreator.IntegratedGST;
			lineNot.AL_AT = notTaxRate.PK;

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(aRInvoiceWrapper.HasZeroIGSTAmount);
			AssertEquals("Tax display heading", "IGST", aRInvoiceWrapper.OSTaxDisplayHeading);

			InvoicingBase.Lines.Remove(lineNot);

			//Check RVS
			var rsvTaxRate_Service = TestObjectCreator.CreateTaxRate("RSV", "Reverse Rated", AccTaxRate.Types.ReverseRated, 0, AccTaxRate.ExtraTypes.ServiceTax, 0, 1);

			var lineRsv = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineRsv.AL_AT = rsvTaxRate_Service.PK;

			InvoicingBase.Lines.Add(lineRsv);

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(!aRInvoiceWrapper.HasZeroIGSTAmount);
			AssertEquals("Tax display heading", "SER", aRInvoiceWrapper.OSTaxDisplayHeading);

			var rsvTaxRate = TestObjectCreator.CreateTaxRate("RSV", "Reverse Rated", AccTaxRate.Types.ReverseRated, 0, string.Empty, 0, 1);
			lineRsv.AL_AT = rsvTaxRate.PK;

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(aRInvoiceWrapper.HasZeroIGSTAmount);
			AssertEquals("Tax display heading", "IGST", aRInvoiceWrapper.OSTaxDisplayHeading);

			InvoicingBase.Lines.Remove(lineRsv);

			//Check EXT
			var extRaxRate_Service = TestObjectCreator.CreateTaxRate("EXT", "Exempt", AccTaxRate.Types.Exempt, 0, AccTaxRate.ExtraTypes.ServiceTax, 0, 1);

			var lineExt = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineExt.AL_AT = extRaxRate_Service.PK;

			InvoicingBase.Lines.Add(lineExt);

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(!aRInvoiceWrapper.HasZeroIGSTAmount);
			AssertEquals("Tax display heading", "SER", aRInvoiceWrapper.OSTaxDisplayHeading);

			var extTaxRate = TestObjectCreator.CreateTaxRate("EXT", "Exempt", AccTaxRate.Types.Exempt, 0, string.Empty, 0, 1);
			lineExt.AL_AT = extTaxRate.PK;

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(aRInvoiceWrapper.HasZeroIGSTAmount);
			AssertEquals("Tax display heading", "IGST", aRInvoiceWrapper.OSTaxDisplayHeading);

			InvoicingBase.Lines.Remove(lineExt);

			//Check RVS + STA
			rsvTaxRate_Service = TestObjectCreator.CreateTaxRate("RSV", "Reverse Rated", AccTaxRate.Types.ReverseRated, 0, AccTaxRate.ExtraTypes.ServiceTax, 0, 1);

			lineRsv = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineRsv.AL_AT = rsvTaxRate_Service.PK;

			InvoicingBase.Lines.Add(lineRsv);

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(!aRInvoiceWrapper.HasZeroRatedStateGST);
			AssertEquals("Tax display heading", "SER", aRInvoiceWrapper.OSTaxDisplayHeading);

			rsvTaxRate = TestObjectCreator.CreateTaxRate("RSV", "Reverse Rated", AccTaxRate.Types.ReverseRated, 0, AccTaxRate.ExtraTypes.StateGST, 0, 1);
			lineRsv.AL_AT = rsvTaxRate.PK;

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(!aRInvoiceWrapper.HasZeroIGSTAmount);
			Assert(aRInvoiceWrapper.HasZeroRatedStateGST);
			AssertEquals("Tax display heading", "CGST/SGST", aRInvoiceWrapper.OSTaxDisplayHeading);

			InvoicingBase.Lines.Remove(lineRsv);

			//Check NOT + STA
			notTaxRate_Service = TestObjectCreator.CreateTaxRate("NOT", "Not Reported", AccTaxRate.Types.NotReportable, 0, AccTaxRate.ExtraTypes.ServiceTax, 0, 1);

			lineNot = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineNot.AL_AT = notTaxRate_Service.PK;

			InvoicingBase.Lines.Add(lineNot);

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(!aRInvoiceWrapper.HasZeroRatedStateGST);
			AssertEquals("Tax display heading", "SER", aRInvoiceWrapper.OSTaxDisplayHeading);

			notTaxRate = TestObjectCreator.CreateTaxRate("NOT", "Not Reported", AccTaxRate.Types.NotReportable, 0, AccTaxRate.ExtraTypes.StateGST, 0, 1);
			lineNot.AL_AT = notTaxRate.PK;

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(!aRInvoiceWrapper.HasZeroIGSTAmount);
			Assert(aRInvoiceWrapper.HasZeroRatedStateGST);
			AssertEquals("Tax display heading", "CGST/SGST", aRInvoiceWrapper.OSTaxDisplayHeading);

			InvoicingBase.Lines.Remove(lineNot);

			//Check EXT + STA
			extRaxRate_Service = TestObjectCreator.CreateTaxRate("EXT", "Exempt", AccTaxRate.Types.Exempt, 0, AccTaxRate.ExtraTypes.ServiceTax, 0, 1);

			lineExt = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineExt.AL_AT = extRaxRate_Service.PK;

			InvoicingBase.Lines.Add(lineExt);

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(!aRInvoiceWrapper.HasZeroRatedStateGST);
			AssertEquals("Tax display heading", "SER", aRInvoiceWrapper.OSTaxDisplayHeading);

			extTaxRate = TestObjectCreator.CreateTaxRate("EXT", "Exempt", AccTaxRate.Types.Exempt, 0, AccTaxRate.ExtraTypes.StateGST, 0, 1);
			lineExt.AL_AT = extTaxRate.PK;

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(!aRInvoiceWrapper.HasZeroIGSTAmount);
			Assert(aRInvoiceWrapper.HasZeroRatedStateGST);
			AssertEquals("Tax display heading", "CGST/SGST", aRInvoiceWrapper.OSTaxDisplayHeading);

			InvoicingBase.Lines.Remove(lineExt);
			extTaxRate = TestObjectCreator.CreateTaxRate("EXT", "Exempt", AccTaxRate.Types.Exempt, 0, string.Empty, 0, 1);
			lineExt.AL_AT = extTaxRate.PK;
			InvoicingBase.Lines.Add(lineExt);

			AccTaxRate gstAndEdu = TestObjectCreator.CreateTaxRate("EDU", "GST and EDU", AccTaxRate.Types.Rated, 3, AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, 3, 1);
			AccTaxRate gstAndQct = TestObjectCreator.CreateTaxRate("QCT", "GST and QST", AccTaxRate.Types.Rated, 3, AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase, 3, 1);
			var lineEDU = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineEDU.AL_AT = gstAndEdu.PK;
			var lineQCT = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineQCT.AL_AT = gstAndQct.PK;
			var lineIGST = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineIGST.AL_AT = intTaxRate.PK;
			var lineStateGST = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineStateGST.AL_AT = TestObjectCreator.STAGST.PK;
			InvoicingBase.Lines.Add(lineEDU);
			InvoicingBase.Lines.Add(lineQCT);
			InvoicingBase.Lines.Add(lineIGST);
			InvoicingBase.Lines.Add(lineStateGST);
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(aRInvoiceWrapper.HasZeroIGSTAmount);
			AssertEquals("Tax display heading", "SER/CESS/EDU/IGST/CGST/SGST", aRInvoiceWrapper.OSTaxDisplayHeading);
		}

		public void TestOSTaxDisplayHeadingQCTAndEDU_SER()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.India);

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			Assert("Tax display heading", aRInvoiceWrapper.OSTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			Assert("Primary tax display heading", aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == ZString.Empty || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == "GST" || aRInvoiceWrapper.OSPrimaryTaxDisplayHeading == GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);

			AccTaxRate gstAndEdu = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndEdu.AT_RN_NKCountry = Constants.CountryCodes.India;
			gstAndEdu.AT_Type = AccTaxRate.Types.ServiceTax;
			gstAndEdu.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
			gstAndEdu.SetRateNumerator_ForTestOnly(3);
			gstAndEdu.SetExtraRate_ForTestOnly(3, 1);
			AccTaxRate gstAndQct = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndQct.AT_RN_NKCountry = Constants.CountryCodes.India;
			gstAndQct.AT_Type = AccTaxRate.Types.ServiceTax;
			gstAndQct.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			gstAndQct.SetRateNumerator_ForTestOnly(3);
			gstAndQct.SetExtraRate_ForTestOnly(3, 1);
			ARInvoiceLine lineEDU = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineEDU = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineEDU.AL_AT = gstAndEdu.PK;
			ARInvoiceLine lineQCT = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineQCT = Factory.NewWithValidTestData<ARInvoiceLine>();
			lineQCT.AL_AT = gstAndQct.PK;
			InvoicingBase.Lines.Add(lineEDU);
			InvoicingBase.Lines.Add(lineQCT);
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("Tax display heading", "SER/CESS/EDU", aRInvoiceWrapper.OSTaxDisplayHeading);
		}

		public void TestHasSERLine()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Malaysia);

			var gstTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			gstTaxRate.AT_ExtraTaxRateType = "";
			var serTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			serTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ServiceTax;

			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();

			line1.AL_AT = gstTaxRate.PK;
			line2.AL_AT = gstTaxRate.PK;
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(false, aRInvoiceWrapper.HasSERLineOnly);
			AssertEquals(false, aRInvoiceWrapper.HasAtLeastOneSERLine);

			line1.AL_AT = gstTaxRate.PK;
			line2.AL_AT = serTaxRate.PK;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(false, aRInvoiceWrapper.HasSERLineOnly);
			AssertEquals(true, aRInvoiceWrapper.HasAtLeastOneSERLine);

			line1.AL_AT = serTaxRate.PK;
			line2.AL_AT = serTaxRate.PK;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(true, aRInvoiceWrapper.HasSERLineOnly);
			AssertEquals(true, aRInvoiceWrapper.HasAtLeastOneSERLine);
		}

		public void TestCurrentCompany()
		{
			AssertNotNull(ARInvoiceWrapper.CurrentCompany);
			AssertEquals(GlbCompany.CurrentCompany.GC_Address1, ARInvoiceWrapper.CurrentCompany.Address1);
		}

		public void TestIsExportDepartment()
		{
			var nonExportDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Desc, SQLComparisonOperator.NotContains, "export"));
			Invoice.AH_GE = nonExportDepartment.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AssertEquals("IsExportDepartment", false, ARInvoiceWrapper.IsExportDepartment);

			var exportDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Desc, SQLComparisonOperator.Contains, "export"));
			Invoice.AH_GE = exportDepartment.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AssertEquals("IsExportDepartment", true, ARInvoiceWrapper.IsExportDepartment);
		}

		public void TestIsImportDepartment()
		{
			var nonImportDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Desc, SQLComparisonOperator.NotContains, "import"));
			Invoice.AH_GE = nonImportDepartment.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AssertEquals("IsImportDepartment", false, ARInvoiceWrapper.IsImportDepartment);

			var importDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Desc, SQLComparisonOperator.Contains, "import"));
			Invoice.AH_GE = importDepartment.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AssertEquals("IsImportDepartment", true, ARInvoiceWrapper.IsImportDepartment);
		}

		public void TestCreditBalanceMessage()
		{
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			AssertEquals("Show Credit Balance Message", false, ARInvoiceWrapper.ShowCreditBalanceMessage);
			AssertEquals("Credit Balance Message", ZString.Empty, ARInvoiceWrapper.CreditBalanceMessage);

			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.AdjustmentNote;
			Invoice.AH_InvoiceAmount = -100;
			ZString expectedMessage = "THIS IS AN ADJUSTMENT NOTE, NO PAYMENT REQUIRED";
			AssertEquals("Show Credit Balance Message", true, ARInvoiceWrapper.ShowCreditBalanceMessage);
			AssertEquals("Credit Balance Message", expectedMessage, ARInvoiceWrapper.CreditBalanceMessage);

			Invoice.AH_InvoiceAmount = 100;
			AssertEquals("Show Credit Balance Message", false, ARInvoiceWrapper.ShowCreditBalanceMessage);
			AssertEquals("Credit Balance Message", ZString.Empty, ARInvoiceWrapper.CreditBalanceMessage);

			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			expectedMessage = "THIS IS A CREDIT NOTE, NO PAYMENT REQUIRED";
			AssertEquals("Show Credit Balance Message", true, ARInvoiceWrapper.ShowCreditBalanceMessage);
			AssertEquals("Credit Balance Message", expectedMessage, ARInvoiceWrapper.CreditBalanceMessage);
		}

		public void TestInvoiceMailToAddress_SameCountry()
		{
			DocARInvoice invoiceWrapper = DocARInvoice.New(Invoice, Factory);

			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			address.OA_Address1 = "Address Line 1";
			address.OA_Address2 = "Address Line 2";
			address.OA_City = "SYDNEY";
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables.Code);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			AssertEquals("Mail to Address Country", Core.Constants.CountryCodes.Australia, invoiceWrapper.Branch.MailToAddress.Country.Code);
			AssertEquals("Mail to Address", "EAGLE DATAMATION INTERNATIONAL\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY\nAUSTRALIA", invoiceWrapper.MailToAddressWithCountry.ToString());
		}

		public void TestInvoiceMailToAddress_DifferentCountry()
		{
			DocARInvoice invoiceWrapper = DocARInvoice.New(Invoice, Factory);

			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			address.OA_Address1 = "Address Line 1";
			address.OA_Address2 = "Address Line 2";
			address.OA_City = "SYDNEY";
			address.OA_RL_NKRelatedPortCode = "NZAKL";
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables.Code);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			AssertEquals("Mail to Address Country", Core.Constants.CountryCodes.NewZealand, invoiceWrapper.Branch.MailToAddress.Country.Code);
			AssertEquals("Mail to Address", "EAGLE DATAMATION INTERNATIONAL\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY\nNEW ZEALAND", invoiceWrapper.MailToAddressWithCountry.ToString());
		}

		public void TestCachingOfTermsAndConditions()
		{
			AccountingConfigurationRegistry.Instance.InvoiceTradingTerms.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new System.Drawing.Bitmap(1, 2));
			var termsAndConditions = ARInvoiceWrapper.TermsAndConditions;
			AssertNotNull("Terms and Conditions image is not null", termsAndConditions);

			AccountingConfigurationRegistry.Instance.InvoiceTradingTerms.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new System.Drawing.Bitmap(1, 2));
			AssertEquals("Terms and Conditions image is reused", termsAndConditions, ARInvoiceWrapper.TermsAndConditions);
		}

		public void TestPrintBackPageAndTermsAndConditions()
		{
			AccountingConfigurationRegistry.Instance.InvoiceTradingTerms.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, null);

			AssertEquals("Print Back Page", false, ARInvoiceWrapper.PrintBackPage);
			AssertNull("Terms and Conditions (Image)", ARInvoiceWrapper.TermsAndConditions);

			AccountingConfigurationRegistry.Instance.InvoiceTradingTerms.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new System.Drawing.Bitmap(1, 2));

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Print Back Page", true, ARInvoiceWrapper.PrintBackPage);
			AssertNotNull("Terms and Conditions (Image)", ARInvoiceWrapper.TermsAndConditions);
			AssertEquals("Terms And Conditions (Image Size)", new System.Drawing.Size(1, 2), AccountingConfigurationRegistry.Instance.InvoiceTradingTerms.Value.Size);

			InvoiceCopyCollection collection = (InvoiceCopyCollection)AccountingConfigurationRegistry.Instance.InvoiceCopies.Value.Clone(null, null);

			foreach (InvoiceCopy entry in collection)
			{
				if (entry.IsOriginal)
				{
					entry.IncludeTradingTerms = false;
				}
			}

			AccountingConfigurationRegistry.Instance.InvoiceCopies.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Print Back Page", false, ARInvoiceWrapper.PrintBackPage);
			AssertNull("Terms and Conditions (Image)", ARInvoiceWrapper.TermsAndConditions);
		}

		public void TestWrapperInfo()
		{
			InvoiceCopyCollection collection = (InvoiceCopyCollection)AccountingConfigurationRegistry.Instance.InvoiceCopies.Value.Clone(null, null);

			foreach (InvoiceCopy entry in collection)
			{
				if (entry.IsOriginal)
				{
					entry.Name = (NoResString)"Hello!";
					entry.IncludeTradingTerms = false;
				}
			}

			AccountingConfigurationRegistry.Instance.InvoiceCopies.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("DocInvoiceWrapper.Copy (References WrapperInfo which is private)", "Hello!", ARInvoiceWrapper.Copy);
			AssertEquals("DocInvoiceWrapper.IncludeTradingTerms", false, ARInvoiceWrapper.IncludeTradingTerms);
		}

		public void TestDisplayRemittancePaymentDetails()
		{
			Invoice.AH_IsCancelled = true;
			Invoice.AH_TransactionType = TransactionTypes.Invoice;
			Invoice.AH_InvoiceAmount = 1;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals(false, ARInvoiceWrapper.DisplayRemittancePaymentDetails);

			Invoice.AH_IsCancelled = false;
			Invoice.AH_TransactionType = TransactionTypes.Invoice;
			Invoice.AH_InvoiceAmount = 1;
			AssertEquals(true, ARInvoiceWrapper.DisplayRemittancePaymentDetails);

			Invoice.AH_IsCancelled = false;
			Invoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
			Invoice.AH_InvoiceAmount = 1;
			AssertEquals(true, ARInvoiceWrapper.DisplayRemittancePaymentDetails);

			Invoice.AH_IsCancelled = false;
			Invoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
			Invoice.AH_InvoiceAmount = -1;
			AssertEquals(false, ARInvoiceWrapper.DisplayRemittancePaymentDetails);

			Invoice.AH_IsCancelled = false;
			Invoice.AH_TransactionType = TransactionTypes.Contra;
			Invoice.AH_InvoiceAmount = 1;
			AssertEquals(false, ARInvoiceWrapper.DisplayRemittancePaymentDetails);
		}

		public void TestShowCreditBalanceMessage()
		{
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Invoice.AH_TransactionType = TransactionTypes.Invoice;
			AssertEquals(false, ARInvoiceWrapper.ShowCreditBalanceMessage);

			Invoice.AH_TransactionType = TransactionTypes.CreditNote;
			AssertEquals(true, ARInvoiceWrapper.ShowCreditBalanceMessage);

			Invoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
			Invoice.AH_InvoiceAmount = 100;
			AssertEquals(false, ARInvoiceWrapper.ShowCreditBalanceMessage);

			Invoice.AH_InvoiceAmount = -100;
			AssertEquals(true, ARInvoiceWrapper.ShowCreditBalanceMessage);

			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Invoice.AH_TransactionType = TransactionTypes.Invoice;
			AssertEquals(false, ARInvoiceWrapper.ShowCreditBalanceMessage);

			Invoice.AH_TransactionType = TransactionTypes.CreditNote;
			AssertEquals(true, ARInvoiceWrapper.ShowCreditBalanceMessage);
		}

		public void TestInvoiceReversalData()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_OSExTaxAmount = 100;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			ARInvoiceReversing reverser = new ARInvoiceReversing(invoice);
			reverser.Reverse();
			Factory.Save();

			InvoicingBase creditNoteBizO = invoice.CorrespondingReversedTransaction;

			DocARInvoice wrappedInvoice = DocARInvoice.New(invoice, Factory);
			DocARInvoice wrappedCreditNote = DocARInvoice.New(creditNoteBizO, Factory);

			ZString expected = wrappedInvoice.DocumentTitle + " " + wrappedInvoice.JobInvoiceNumber + " WAS CANCELLED BY "
												 + wrappedCreditNote.InvoiceDate.ToShortDateString() + " " + wrappedCreditNote.DocumentTitle + " " + wrappedCreditNote.JobInvoiceNumber;

			AssertEquals(expected, wrappedInvoice.InvoiceReversalData);

			expected = wrappedCreditNote.DocumentTitle + " " + wrappedCreditNote.JobInvoiceNumber + " CANCELS "
								 + wrappedInvoice.InvoiceDate.ToShortDateString() + " " + wrappedInvoice.DocumentTitle + " " + wrappedInvoice.JobInvoiceNumber;

			AssertEquals(expected, wrappedCreditNote.InvoiceReversalData);

			creditNoteBizO.AH_TransactionBelongsToGroup = ZGuid.Empty;
			AssertNull("CorrespondingReversedTransaction", creditNoteBizO.CorrespondingReversedTransaction);
			AssertEquals("Should not blow up but return an empty string when CorrespondingReversedTransaction is null", ZString.Empty, wrappedCreditNote.InvoiceReversalData);
		}

		public void TestReversalReason()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_OSExTaxAmount = 100;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			ARInvoiceReversing reverser = new ARInvoiceReversing(invoice);
			reverser.Reverse();
			Factory.Save();

			InvoicingBase creditNoteBizO = invoice.CorrespondingReversedTransaction;
			creditNoteBizO.AH_ReceiptType = "IAM";

			DocARInvoice wrappedCreditNote = DocARInvoice.New(creditNoteBizO, Factory);
			DocARInvoice wrappedInvoice = DocARInvoice.New(invoice, Factory);

			AssertEquals("Reversal reason must be incorrect data entry", "Reason for Reversal: Incorrect Amounts", wrappedCreditNote.ReversalReason);
			AssertEquals("Reversal reason must be incorrect data entry", "Reason for Reversal: Incorrect Amounts", wrappedInvoice.ReversalReason);

			creditNoteBizO.AH_ReceiptType = Core.Constants.GenApprovalRequestReasonCode.Code.DamagedGoods;

			wrappedCreditNote = DocARInvoice.New(creditNoteBizO, Factory);
			wrappedInvoice = DocARInvoice.New(invoice, Factory);

			AssertEquals("Reversal reason must be incorrect data entry", "Reason for Reversal: Damaged Goods", wrappedCreditNote.ReversalReason);
			AssertEquals("Reversal reason must be incorrect data entry", "Reason for Reversal: Damaged Goods", wrappedInvoice.ReversalReason);

			using (InitaliseComplianceFactory("Incorrect Amounts"))
			{
				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				wrappedInvoice = DocARInvoice.New(invoice2, Factory);
				AssertEquals("Reason for Reversal: Incorrect Amounts", wrappedInvoice.ReversalReason);
			}
		}

		public void TestDisplayReversalInfo()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 100;
			Factory.Save();

			ARInvoiceReversing reverser = new ARInvoiceReversing(invoice);
			reverser.Reverse();
			Factory.Save();

			InvoicingBase creditNoteBizO = invoice.CorrespondingReversedTransaction;

			DocARInvoice wrappedCreditNote = DocARInvoice.New(creditNoteBizO, Factory);
			DocARInvoice wrappedInvoice = DocARInvoice.New(invoice, Factory);
			AssertEquals("Should not be Amending", false, wrappedCreditNote.IsAmendingTransaction);

			AssertEquals("Reversal reason must not display when ah_receipttype is empty", false, wrappedCreditNote.DisplayReversalInfo);
			AssertEquals("Reversal reason must not display when ah_receipttype is empty", false, wrappedInvoice.DisplayReversalInfo);

			creditNoteBizO.AH_ReceiptType = "IAM";
			wrappedCreditNote = DocARInvoice.New(creditNoteBizO, Factory);
			wrappedInvoice = DocARInvoice.New(invoice, Factory);

			AssertEquals("Reversal reason must display when ah_receipttype is set", true, wrappedCreditNote.DisplayReversalInfo);
			AssertEquals("Reversal reason must display when ah_receipttype is set", true, wrappedInvoice.DisplayReversalInfo);
			AssertEquals("Should not be Amending", false, wrappedCreditNote.IsAmendingTransaction);

			invoice = Factory.NewWithValidTestData<ARInvoice>();

			line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 100;
			Factory.Save();
			wrappedInvoice = DocARInvoice.New(invoice, Factory);
			AssertEquals("Reversal reason must not display when the invoice is not reversed", false, wrappedInvoice.DisplayReversalInfo);

			using (InitaliseComplianceFactory("fallbackOriginalReferenceReasonValue"))
			{
				wrappedInvoice = DocARInvoice.New(invoice, Factory);
				AssertEquals("Reversal reason must display when OriginalReferenceReason is not empty", true, wrappedInvoice.DisplayReversalInfo);
			}
		}

		[TestDate(2021, 5, 29)]
		public void TestOriginalReferenceReason()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var wrappedInvoice = DocARInvoice.New(invoice, Factory);
			AssertEquals(string.Empty, invoice.ReasonDescription);
			AssertEquals(string.Empty, wrappedInvoice.OriginalReferenceReason);

			invoice.ReasonCode = "IAM";
			AssertEquals("Incorrect Amounts", invoice.ReasonDescription);
			AssertEquals("Incorrect Amounts", wrappedInvoice.OriginalReferenceReason);

			foreach (var reason in new string[] { "my reason", string.Empty })
			{
				using (InitaliseComplianceFactory(reason))
				{
					invoice = Factory.NewWithValidTestData<ARInvoice>();
					wrappedInvoice = DocARInvoice.New(invoice, Factory);
					AssertEquals(reason, wrappedInvoice.OriginalReferenceReason);
				}
			}
		}

		IDisposable InitaliseComplianceFactory(string docOriginalReferenceReason)
		{
			var originalInvoiceReferenceMock = new Mock<IOriginalInvoiceReference>();
			originalInvoiceReferenceMock.Setup(x => x.GetDocOriginalReferenceReason(It.IsAny<string>(), It.IsAny<ZDate>(), It.IsAny<ZDate>())).Returns(docOriginalReferenceReason);
			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
			countryComplianceFactoryMock.Setup(c => c.GetIOriginalInvoiceReference(It.IsAny<ZString>())).Returns(originalInvoiceReferenceMock.Object);
			return ObjectFactory.Substitute(countryComplianceFactoryMock.Object);
		}

		public void TestIsAmendingTransaction()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<ARInvoice>();
			DocARInvoice wrappedInvoice = DocARInvoice.New(invoice, Factory);
			AssertEquals("Should not be Amending", false, wrappedInvoice.IsAmendingTransaction);

			IAmending original = invoice as IAmending;
			AssertNotNull(original);

			IAmending amending = original.GenerateAmendingTransaction(TransactionTypes.CreditNote);
			amending.AmendingReasonCode = "IAM";
			DocARInvoice wrappedCreditNote = DocARInvoice.New((InvoicingBase)amending, Factory);
			AssertEquals("Should be Amending", true, wrappedCreditNote.IsAmendingTransaction);
			AssertEquals("Amending reason description should not be empty", false, wrappedCreditNote.AmendmentReasonDescription.IsEmpty);
			AssertEquals("Should not DisplayReversalInfo", false, wrappedCreditNote.DisplayReversalInfo);

			using (InitaliseComplianceFactory("reasonDesc"))
			{
				wrappedCreditNote = DocARInvoice.New((InvoicingBase)amending, Factory);
				AssertEquals("Should not DisplayReversalInfo", false, wrappedCreditNote.DisplayReversalInfo);
			}
		}

		public void TestAmendmentData()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var wrappedInvoice = DocARInvoice.New(invoice, Factory);
			AssertEquals("Should not be Amending", false, wrappedInvoice.IsAmendingTransaction);
			Assert("InvoiceAmendmentData should be empty", wrappedInvoice.AmendmentData.IsEmpty);

			var original = invoice as IAmending;
			AssertNotNull(original);

			var amending = original.GenerateAmendingTransaction(TransactionTypes.Invoice);
			var wrappedAmendingInvoice = DocARInvoice.New((InvoicingBase)amending, Factory);
			AssertEquals("Should be Amending", true, wrappedAmendingInvoice.IsAmendingTransaction);
			AssertEquals("Should not DisplayReversalInfo", false, wrappedAmendingInvoice.DisplayReversalInfo);

			ZString expected = wrappedAmendingInvoice.DocumentTitle + " " + wrappedAmendingInvoice.JobInvoiceNumber + " IS AN AMENDMENT TO "
												 + wrappedInvoice.InvoiceDate.ToShortDateString() + " " + wrappedInvoice.DocumentTitle + " " + wrappedInvoice.JobInvoiceNumber;

			AssertEquals("InvoiceAmendmentData", expected, wrappedAmendingInvoice.AmendmentData);

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_OriginalTransactionNum = "ABC123";
			creditNote.AH_OriginalInvoiceDate = new ZDate(2020, 08, 12);
			AssertNull(creditNote.OriginalTransaction);
			wrappedAmendingInvoice = DocARInvoice.New(creditNote, Factory);
			AssertEquals("Should be Amending", true, wrappedAmendingInvoice.IsAmendingTransaction);
			expected = wrappedAmendingInvoice.DocumentTitle + " " + wrappedAmendingInvoice.JobInvoiceNumber + " IS AN AMENDMENT TO TRANSACTION 12-Aug-20 ABC123";
			AssertEquals("InvoiceAmendmentData", expected, wrappedAmendingInvoice.AmendmentData);
		}

		public void TestAmendmentReason()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var wrappedInvoice = DocARInvoice.New(invoice, Factory);
			AssertEquals("Should not be Amending", false, wrappedInvoice.IsAmendingTransaction);
			Assert("AmendmentReasonCode should be empty", wrappedInvoice.AmendmentReasonCode.IsEmpty);
			Assert("AmendmentReasonDescription should be empty", wrappedInvoice.AmendmentReasonDescription.IsEmpty);
			Assert("AmendmentReason should be empty", wrappedInvoice.AmendmentReason.IsEmpty);

			var original = invoice as IAmending;
			AssertNotNull(original);

			var amending = original.GenerateAmendingTransaction(TransactionTypes.Invoice);
			var wrappedAmendingInvoice = DocARInvoice.New((InvoicingBase)amending, Factory);
			AssertEquals("Should be Amending", true, wrappedAmendingInvoice.IsAmendingTransaction);
			AssertEquals("Should not DisplayReversalInfo", false, wrappedAmendingInvoice.DisplayReversalInfo);
			Assert("AmendmentReason is not set yet", wrappedAmendingInvoice.AmendmentReason.IsEmpty);

			amending.AmendingReasonCode = "IAM";
			AssertEquals("IAM", wrappedAmendingInvoice.AmendmentReasonCode);
			AssertEquals("Incorrect Amounts", wrappedAmendingInvoice.AmendmentReasonDescription);
			AssertEquals("Amendment reason must be incorrect data entry", "Reason for Amendment: Incorrect Amounts", wrappedAmendingInvoice.AmendmentReason);
		}

		public void TestTransactionVerb()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var wrappedInvoice = DocARInvoice.New(invoice, Factory);
			Assert("Should not be Amending", !wrappedInvoice.IsAmendingTransaction);
			Assert("TransactionVerb should be empty", wrappedInvoice.TransactionVerb.IsEmpty);

			var originalForAmending = invoice as IAmending;
			AssertNotNull("Original as IAmending", originalForAmending);

			var amending = originalForAmending.GenerateAmendingTransaction(TransactionTypes.Invoice);
			var wrappedAmendingInvoice = DocARInvoice.New((InvoicingBase)amending, Factory);
			Assert("Should be Amending", wrappedAmendingInvoice.IsAmendingTransaction);
			AssertEquals("TransactionVerb should be 'amends'", "amends", wrappedAmendingInvoice.TransactionVerb);

			Assert("Origianl should not be Amending", !wrappedInvoice.IsAmendingTransaction);
			Assert("TransactionVerb should be empty on the original transaction", wrappedInvoice.TransactionVerb.IsEmpty);

			var originalForReversing = new ReversingFactory().NewReversing(invoice);
			AssertNotNull("Origianl as ReversingBase", originalForReversing);

			originalForReversing.Reverse();
			var reversing = originalForReversing.ReverseTransaction as InvoicingBase;
			AssertNotNull("Reverse transaction", reversing);
			AssertNotNull("Reversed transaction", reversing.CorrespondingReversedTransaction);
			AssertEquals("Reversed transaction PK", invoice.PK, reversing.CorrespondingReversedTransaction.PK);
			Assert("Should be reverse transaction", reversing.IsReverseTransaction);
			Assert("Should be reversed", reversing.IsReversed);

			var wrappedReversingInvoice = DocARInvoice.New(reversing, Factory);
			AssertEquals("TransactionVerb should be 'reverses'", "reverses", wrappedReversingInvoice.TransactionVerb);

			Assert("Original should not be Amending", !wrappedInvoice.IsAmendingTransaction);
			Assert("Original should not be reversing", !invoice.IsReverseTransaction);
			Assert("Original should be reversed", invoice.IsReversed);
			Assert("TransactionVerb should be empty on the original transaction", wrappedInvoice.TransactionVerb.IsEmpty);
		}

		public void TestTotalOSSERAmount()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Malaysia);

			var gstTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			gstTaxRate.SetRateNumerator_ForTestOnly(10);
			gstTaxRate.AT_ExtraTaxRateType = "";
			var serTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			serTaxRate.SetRateNumerator_ForTestOnly(10);
			serTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ServiceTax;

			InvoicingLineBase gstLine = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			gstLine.AL_OSExTaxAmount = 100m;
			gstLine.AL_AT = gstTaxRate.PK;
			InvoicingLineBase serLine = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			serLine.AL_OSExTaxAmount = 100m;
			serLine.AL_AT = serTaxRate.PK;

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals(10m, aRInvoiceWrapper.TotalOSTaxAmount);
			AssertEquals(10m, aRInvoiceWrapper.TotalOSSERAmount);
		}

		#region India GST tests

		public void TestHasIntegratedAndStateGSTLine()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.India);

			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_OSExTaxAmount = 100m;
			line1.AL_AT = TestObjectCreator.IntegratedGST.PK;

			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_OSExTaxAmount = 100m;
			line2.AL_AT = TestObjectCreator.STAGST.PK;

			var invoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			Assert(invoiceWrapper.HasIntegratedGSTLine);
			Assert(invoiceWrapper.HasStateGSTLine);

			line1.AL_AT = TestObjectCreator.STAGST.PK;

			invoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(!invoiceWrapper.HasIntegratedGSTLine);
			Assert(invoiceWrapper.HasStateGSTLine);

			line1.AL_AT = TestObjectCreator.IntegratedGST.PK;
			line2.AL_AT = TestObjectCreator.IntegratedGST.PK;

			invoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			Assert(invoiceWrapper.HasIntegratedGSTLine);
			Assert(!invoiceWrapper.HasStateGSTLine);
		}

		public void TestHasIndiaServiceTax()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var serTaxRate = TestObjectCreator.CreateTaxRate("SER1", "Service tax1", AccTaxRate.Types.ServiceTax, 0, ZString.Empty, 0, 1);
				serTaxRate.SetRateNumerator_ForTestOnly(10);

				var serTaxRate1 = TestObjectCreator.CreateTaxRate("SER1", "Service tax1", AccTaxRate.Types.Rated, 0, AccTaxRate.ExtraTypes.ServiceTax, 0, 1);
				serTaxRate1.SetRateNumerator_ForTestOnly(10);

				InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
				line1.AL_OSExTaxAmount = 100m;
				line1.AL_AT = serTaxRate.PK;

				InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
				line2.AL_OSExTaxAmount = 100m;
				line2.AL_AT = serTaxRate1.PK;

				InvoicingLineBase line3 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
				line3.AL_OSExTaxAmount = 100m;
				line3.AL_AT = TestObjectCreator.GST1.PK;

				var invoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
				Assert(invoiceWrapper.HasIndiaServiceTax);

				line1.Delete();

				invoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				Assert(invoiceWrapper.HasIndiaServiceTax);

				line2.Delete();

				invoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				Assert(!invoiceWrapper.HasIndiaServiceTax);
			}
		}

		public void TestTotalOSIntegratedGSTAmount()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.India);

			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_OSExTaxAmount = 100m;
			line1.AL_AT = TestObjectCreator.IntegratedGST.PK;

			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_OSExTaxAmount = 200m;
			line2.AL_AT = TestObjectCreator.STAGST.PK;

			InvoicingLineBase line3 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line3.AL_OSExTaxAmount = 300m;
			line3.AL_AT = TestObjectCreator.IntegratedGST.PK;

			AssertEquals("Precondition: Integrated GST rate is 18%", 18M, TestObjectCreator.IntegratedGST.GetRateRaw_ForTestOnly());

			var invoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Total Integrated GST calculated from line1 + line3", 72M, invoiceWrapper.TotalOSIntegratedGSTAmount);
			AssertEquals("Total Formatted Integrated GST amount", "72.00", invoiceWrapper.TotalOSIntegratedGSTAmountFormatted);
		}

		public void TestTotalOSCAndSGSTAmount()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.India);

			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_OSExTaxAmount = 100m;
			line1.AL_AT = TestObjectCreator.STAGST.PK;

			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_OSExTaxAmount = 200m;
			line2.AL_AT = TestObjectCreator.IntegratedGST.PK;

			InvoicingLineBase line3 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line3.AL_OSExTaxAmount = 300m;
			line3.AL_AT = TestObjectCreator.STAGST.PK;

			AssertEquals("Precondition: Centre GST rate is 9%", 9M, TestObjectCreator.STAGST.GetRateRaw_ForTestOnly());
			AssertEquals("Precondition: State GST rate is 9%", 9M, TestObjectCreator.STAGST.GetExtraRate_ForTestOnly());

			var invoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("Total Centre GST calculated from line1 + line3", 36M, invoiceWrapper.TotalOSCentreGSTAmount);
			AssertEquals("Total Formatted Centre GST amount", "36.00", invoiceWrapper.TotalOSCentreGSTAmountFormatted);
			AssertEquals("Total State GST calculated from line1 + line3", 36M, invoiceWrapper.TotalOSStateGSTAmount);
			AssertEquals("Total Formatted State GST amount", "36.00", invoiceWrapper.TotalOSStateGSTAmountFormatted);
		}

		#endregion

		#region Indonesia Invoice

		public void TestTotalLocalExTaxForIDInvoice()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<ARInvoice>();
			setupIndonesianInvoice(invoice);
			DocARInvoice wrappedInvoice = DocARInvoice.New(invoice, Factory);
			AssertEquals("TotalLocalExTaxForIDInvoice", 1200m, wrappedInvoice.TotalLocalExTaxForIDInvoice);
		}

		public void TestTotalLocalTaxForIDInvoice()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<ARInvoice>();
			setupIndonesianInvoice(invoice);
			DocARInvoice wrappedInvoice = DocARInvoice.New(invoice, Factory);
			AssertEquals("TotalLocalTaxForIDInvoice", 48m, wrappedInvoice.TotalLocalTaxForIDInvoice);
		}

		public void TestLinesForIDInvoice()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<ARInvoice>();
			setupIndonesianInvoice(invoice);
			DocARInvoice wrappedInvoice = DocARInvoice.New(invoice, Factory);

			AssertEquals("Unwrapped Count", 5, invoice.Lines.Count);
			AssertEquals("LinesForIDInvoice.Count", 4, wrappedInvoice.LinesForIDInvoice.Count);

			foreach (DocARInvoiceLine line in wrappedInvoice.LinesForIDInvoice)
			{
				AssertNotEquals("GSTVAT", 0m, line.GSTVAT);
			}
		}

		public void TestIDInvoiceTaxInfoSummary()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<ARInvoice>();
			setupIndonesianInvoice(invoice);
			DocARInvoice wrappedInvoice = DocARInvoice.New(invoice, Factory);

			AssertEquals("IDInvoiceTaxInfoSummary.Count", 3, wrappedInvoice.IDInvoiceTaxInfoSummary.Count);
			AssertEquals("TaxRate", "1%", wrappedInvoice.IDInvoiceTaxInfoSummary[0].TaxInfo.TaxRate);
			AssertEquals("TotalLocalExTaxAmount", 400m, wrappedInvoice.IDInvoiceTaxInfoSummary[0].TaxInfo.TotalLocalExTaxAmount);
			AssertEquals("TotalLocalTaxAmount", 4m, wrappedInvoice.IDInvoiceTaxInfoSummary[0].TaxInfo.TotalLocalTaxAmount);
			AssertEquals("TaxRate", "2%", wrappedInvoice.IDInvoiceTaxInfoSummary[1].TaxInfo.TaxRate);
			AssertEquals("TotalLocalExTaxAmount", 400m, wrappedInvoice.IDInvoiceTaxInfoSummary[1].TaxInfo.TotalLocalExTaxAmount);
			AssertEquals("TotalLocalTaxAmount", 4m, wrappedInvoice.IDInvoiceTaxInfoSummary[1].TaxInfo.TotalLocalTaxAmount);
			AssertEquals("TaxRate", "10%", wrappedInvoice.IDInvoiceTaxInfoSummary[2].TaxInfo.TaxRate);
			AssertEquals("TotalLocalExTaxAmount", 400m, wrappedInvoice.IDInvoiceTaxInfoSummary[2].TaxInfo.TotalLocalExTaxAmount);
			AssertEquals("TotalLocalTaxAmount", 40m, wrappedInvoice.IDInvoiceTaxInfoSummary[2].TaxInfo.TotalLocalTaxAmount);
		}

		void setupIndonesianInvoice(InvoicingBase invoice)
		{
			InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			InvoicingLineBase line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			InvoicingLineBase line3 = (InvoicingLineBase)invoice.Lines.AddNew();
			InvoicingLineBase line4 = (InvoicingLineBase)invoice.Lines.AddNew();
			InvoicingLineBase line5 = (InvoicingLineBase)invoice.Lines.AddNew();
			AccTaxRate capGST = TestObjectCreator.CreateTaxRate("CAPGST", "Capital", 10);
			AccTaxRate lowGST = TestObjectCreator.CreateTaxRate("LOWGST", "Low GST", 1);
			line1.AL_AT = TestObjectCreator.GST1.PK;
			line2.AL_AT = TestObjectCreator.GSTFREE1.PK;
			line3.AL_AT = capGST.PK;
			line4.AL_AT = lowGST.PK;
			line5.AL_AT = lowGST.PK;
			line5.AL_TaxRateNumerator = 2;
			line1.AL_LocalExTaxAmount = 100m;
			line2.AL_LocalExTaxAmount = 200m;
			line3.AL_LocalExTaxAmount = 300m;
			line4.AL_LocalExTaxAmount = 400m;
			line5.AL_LocalExTaxAmount = 400m;
			line1.AL_LocalTaxAmount = 10m;
			line2.AL_LocalTaxAmount = 0m;
			line3.AL_LocalTaxAmount = 30m;
			line4.AL_LocalTaxAmount = 4m;
			line5.AL_LocalTaxAmount = 4m;
		}

		#endregion

		public void TestOrgProxyTaxId()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Indonesia);

				OrgCusCode orgCusCode = Invoice.Branch.OrgProxy.CustomsCodes.AddNew();
				orgCusCode.OK_CodeType = "PPN";
				orgCusCode.OK_RN_NKCodeCountry = "ID";
				orgCusCode.OK_CustomsRegNo = "99.999.999.9.999.000";
				Factory.Save();

				AssertEquals("GlbCompany.CurrentCompany.GC_RN_NKCountryCode", Core.Constants.CountryCodes.Indonesia, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				DocARInvoiceCommon invoiceWrapper = GetBaseInvoiceWrapper() as DocARInvoiceCommon;
				AssertEquals("OrgProxyTaxId", "99.999.999.9.999.000", invoiceWrapper.OrgProxyTaxId);

				OrgHeader companyOrgProxy = TestObjectCreator.CreateOrgHeader("AAAXXX", false, false);
				Invoice.Company.GC_OH_OrgProxy = companyOrgProxy.PK;
				AssertNotEquals("OrgProxies not equal", Invoice.Branch.GB_OH_OrgProxy, Invoice.Company.GC_OH_OrgProxy);

				Invoice.Branch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull(Invoice.Branch.OrgProxy);
				AssertEquals("Company OrgProxy", companyOrgProxy.PK, Invoice.Company.OrgProxy.PK);

				OrgCusCode orgCusCode2 = Invoice.Company.OrgProxy.CustomsCodes.AddNew();
				orgCusCode2.OK_CodeType = "PPN";
				orgCusCode2.OK_RN_NKCodeCountry = "ID";
				orgCusCode2.OK_CustomsRegNo = "12.123.123.1.123.000";

				AssertEquals("GlbCompany.CurrentCompany.GC_RN_NKCountryCode", Core.Constants.CountryCodes.Indonesia, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				Factory.Save();
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Indonesia);

				AssertEquals("GlbCompany.CurrentCompany.GC_RN_NKCountryCode", Core.Constants.CountryCodes.Indonesia, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				invoiceWrapper = GetBaseInvoiceWrapper() as DocARInvoiceCommon;
				AssertEquals("OrgProxyTaxId", "12.123.123.1.123.000", invoiceWrapper.OrgProxyTaxId);

				Invoice.Company.GC_OH_OrgProxy = ZGuid.Empty;

				invoiceWrapper = GetBaseInvoiceWrapper() as DocARInvoiceCommon;
				AssertEquals("OrgProxyTaxId", "", invoiceWrapper.OrgProxyTaxId);

				OrgHeader branchOrgProxy = TestObjectCreator.CreateOrgHeader("BBBXXX", false, false);
				Invoice.Branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				Invoice.Company.GC_OH_OrgProxy = companyOrgProxy.PK;
				invoiceWrapper = GetBaseInvoiceWrapper() as DocARInvoiceCommon;
				AssertEquals("OrgProxyTaxId", "12.123.123.1.123.000", invoiceWrapper.OrgProxyTaxId);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		#region Tests For Invoice Type

		public void TestInvoiceType_NoWarehouseAdHocJobLinked()
		{
			var adHocServiceJob = Factory.NewWithValidTestData<WhsAdHocServiceJob>();
			adHocServiceJob.WSJ_JobNumber = "WI00001234";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = adHocServiceJob.WSJ_JobNumber;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			Factory.Save();
			AssertEquals("Invoice Type", "Miscellaneous", ARInvoiceWrapper.InvoiceType);
		}

		public void TestMiscInvoiceType()
		{
			Invoice.AH_ConsolidatedInvoiceRef = ZString.Empty;
			AssertEquals("Invoice Type", "Miscellaneous", ARInvoiceWrapper.InvoiceType);
		}

		public void TestCustomsInvoiceType()
		{
			Invoice.AH_ConsolidatedInvoiceRef = "B120";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B120";
			JobHeader job = GetInvoiceJob(declaration, Invoice);
			AssertEquals("Invoice Type", "Customs", ARInvoiceWrapper.InvoiceType);
		}

		public void TestCustomsInvoiceType_NoCustomsJobLinked()
		{
			Invoice.AH_ConsolidatedInvoiceRef = "B120";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B120";
			JobHeader job = GetInvoiceJob(declaration, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			job.JH_ParentID = ZGuid.Empty;
			AssertEquals("Invoice Type", "Customs", ARInvoiceWrapper.InvoiceType);
		}

		public void TestReconciliationInvoiceType()
		{
			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);

			Invoice.AH_ConsolidatedInvoiceRef = "B120";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B120";
			declaration.JE_MessageType = "REC";

			var job = GetInvoiceJob(declaration, Invoice);
			AssertEquals("Invoice Type", "ReconDeclaration", ARInvoiceWrapper.InvoiceType);
			AssertEquals("Invoice Type Reference Number", "B120", ARInvoiceWrapper.InvoiceTypeReferenceNumber);
			GlbCompany.CurrentCompany.SetCountry(country);
		}

		public void TestConsolInvoiceType()
		{
			Invoice.Delete();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C120";
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "Consol", ARInvoiceWrapper.InvoiceType);

			Invoice.Delete();
			consol.JK_IsCFS = ZBool.True;
			consol.JK_IsForwarding = ZBool.True;
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "Consol", ARInvoiceWrapper.InvoiceType);
		}

		public void TestLoadListInvoiceType()
		{
			Invoice.Delete();
			var loadListConsol = Factory.New<CFSLoadListConsol>();
			loadListConsol.JK_IsForwarding = ZBool.False;
			loadListConsol.JK_IsCFS = ZBool.True;
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = loadListConsol.JK_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(loadListConsol, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "LoadList", ARInvoiceWrapper.InvoiceType);

			Invoice.Delete();

			loadListConsol.JK_IsForwarding = ZBool.True;
			loadListConsol.JK_IsCFS = ZBool.True;
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = loadListConsol.JK_UniqueConsignRef;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "Consol", ARInvoiceWrapper.InvoiceType);

			loadListConsol.JK_IsForwarding = ZBool.True;
			loadListConsol.JK_IsCFS = ZBool.True;
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_JH = job.PK;
			Invoice.AH_ConsolidatedInvoiceRef = loadListConsol.JK_UniqueConsignRef;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "LoadList", ARInvoiceWrapper.InvoiceType);
		}

		public void TestAgencyShipmentInvoiceType()
		{
			Invoice.Delete();
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_UniqueConsignRef = "S1234";
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AssertEquals("Invoice Type", "AgencyShipment", ARInvoiceWrapper.InvoiceType);
			AssertNotNull("AgencyShipment", ARInvoiceWrapper.AgencyShipment);
			AssertEquals("Shipment", null, ARInvoiceWrapper.Shipment);
		}

		public void TestAgencyShipmentJobType()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_UniqueConsignRef = "S0000100";
			Factory.Save();

			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

			JobHeader job = GetInvoiceJob(shipment, invoice);
			InvoiceWrapper = DocARInvoice.New(invoice, Factory);

			AssertEquals("Invoice Type", "AgencyShipment", ARInvoiceWrapper.InvoiceType);
			AssertEquals("Invoice Type Reference Number", "S0000100", ARInvoiceWrapper.InvoiceTypeReferenceNumber);
			AssertEquals("Invoice Type ReferenceNumber Heading", "Shipment", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeadingNoColon);
		}

		public void TestShipmentInvoiceType()
		{
			Invoice.Delete();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1234";
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "Shipment", ARInvoiceWrapper.InvoiceType);
			AssertNotNull("Shipment", ARInvoiceWrapper.Shipment);
			AssertEquals("AgencyShipment", null, ARInvoiceWrapper.AgencyShipment);

			job.Delete();
			Invoice.Delete();

			shipment.JS_IsCFSRegistered = ZBool.True;
			shipment.JS_IsForwardRegistered = ZBool.False;
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "ShipmentReceivalsAndGatePass", ARInvoiceWrapper.InvoiceType);
			AssertNotNull("Shipment", ARInvoiceWrapper.Shipment);
			AssertEquals("AgencyShipment", null, ARInvoiceWrapper.AgencyShipment);

			job.Delete();
			Invoice.Delete();

			shipment.JS_IsCFSRegistered = ZBool.True;
			shipment.JS_IsForwardRegistered = ZBool.True;
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "Shipment", ARInvoiceWrapper.InvoiceType);
			AssertNotNull("Shipment", ARInvoiceWrapper.Shipment);
			AssertEquals("AgencyShipment", null, ARInvoiceWrapper.AgencyShipment);
		}

		public void TestTransportTypeInvoice()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "T0374839";
			Invoice.AH_ConsolidatedInvoiceRef = cartage.JJ_ConsignmentID;
			Factory.Save();
			JobHeader job = GetInvoiceJob(cartage, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "Transport", ARInvoiceWrapper.InvoiceType);
		}

		public void TestWarehousePeriodicInvoiceType()
		{
			var warehouseInvoice = Factory.NewWithValidTestData<WhsInvoice>();
			warehouseInvoice.ET_StorageJobNumber = "12345";
			Invoice.AH_ConsolidatedInvoiceRef = warehouseInvoice.ET_StorageJobNumber;
			JobHeader job = GetInvoiceJob(warehouseInvoice, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "WarehousePeriodic", ARInvoiceWrapper.InvoiceType);
			AssertEquals("Invoice Type", "12345", ARInvoiceWrapper.InvoiceTypeReferenceNumber);
		}

		public void TestWarehouseStocktakeInvoiceType()
		{
			var warehouseStocktake = Factory.NewWithValidTestData<WhsStocktake>();
			warehouseStocktake.WS_StocktakeNumber = "S00001234";
			Invoice.AH_ConsolidatedInvoiceRef = warehouseStocktake.WS_StocktakeNumber;
			JobHeader job = GetInvoiceJob(warehouseStocktake, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "WarehouseWhsStocktake", ARInvoiceWrapper.InvoiceType);
			AssertEquals("Invoice Type ReferenceNumber Heading", "Stocktake", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeadingNoColon);
			AssertEquals("Invoice Type ReferenceNumber", "S00001234", ARInvoiceWrapper.InvoiceTypeReferenceNumber);
		}

		public void TestWarehouseReceiveInvoiceType()
		{
			var docket = Factory.NewWithValidTestData<WhsReceive>();
			Factory.Save(); // force system to allocate WD_DocketID first, before we can override it
			docket.WD_DocketID = "W00001234";
			Invoice.AH_ConsolidatedInvoiceRef = docket.WD_DocketID;
			Factory.Save();
			JobHeader job = GetInvoiceJob(docket, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "WarehouseWhsInwards", ARInvoiceWrapper.InvoiceType);
			AssertEquals("Invoice Type", "W00001234", ARInvoiceWrapper.InvoiceTypeReferenceNumber);
		}

		public void TestWarehouseOrderInvoiceType()
		{
			var docket = Factory.NewWithValidTestData<WhsOrder>();
			Factory.Save(); // force system to allocate WD_DocketID first, before we can override it
			docket.WD_DocketID = "W00004321";
			Invoice.AH_ConsolidatedInvoiceRef = docket.WD_DocketID;
			Factory.Save();
			JobHeader job = GetInvoiceJob(docket, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "WarehouseWhsOrder", ARInvoiceWrapper.InvoiceType);
		}

		public void TestWarehouseInvoice()
		{
			WhsInvoice warehouseInvoice = Factory.New<WhsInvoice>();
			warehouseInvoice.ET_StorageJobNumber = "12345";
			Invoice.AH_ConsolidatedInvoiceRef = warehouseInvoice.ET_StorageJobNumber;

			JobHeader job = GetInvoiceJob(warehouseInvoice, Invoice);
			job.JH_ParentTableCode = "";
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AssertEquals(null, ARInvoiceWrapper.WarehouseInvoice);
			job.JH_ParentTableCode = JobStorageSchema.Constants.Prefix;
			AssertNotNull(((DocARInvoice)InvoiceWrapper).WarehouseInvoice);
			AssertEquals(typeof(DocWhsInvoice), ((DocARInvoice)InvoiceWrapper).WarehouseInvoice.GetType());
		}

		public void TestWarehouseStocktake()
		{
			var warehouseStocktake = Factory.NewWithValidTestData<WhsStocktake>();
			warehouseStocktake.WS_StocktakeNumber = "S00001234";
			Invoice.AH_ConsolidatedInvoiceRef = warehouseStocktake.WS_StocktakeNumber;

			JobHeader job = GetInvoiceJob(warehouseStocktake, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull(ARInvoiceWrapper.WarehouseStocktake);
			AssertEquals(typeof(DocWhsStocktake), ARInvoiceWrapper.WarehouseStocktake.GetType());
		}

		#region AmendingTransaction

		public void TestInvoiceType_AmendingTransaction_WarehouseAdHocJob()
		{
			var adHocServiceJob = Factory.NewWithValidTestData<WhsAdHocServiceJob>();
			adHocServiceJob.WSJ_JobNumber = "WI00001234";

			Factory.Save();

			GenerateAmendingTransactionAndAssertInvoiceType(adHocServiceJob.JobHeader, "WarehouseAdHocServiceJob");

			adHocServiceJob.Delete();
		}

		public void TestInvoiceType_AmendingTransaction_CustomsInvoiceType()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B120";
			var job = GetInvoiceJob(declaration, Invoice);

			Factory.Save();

			GenerateAmendingTransactionAndAssertInvoiceType(job, "Customs");
		}

		public void TestInvoiceType_AmendingTransaction_ReconciliationInvoiceType()
		{
			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);

			Invoice.AH_ConsolidatedInvoiceRef = "B120";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B120";
			declaration.JE_MessageType = "REC";

			var job = GetInvoiceJob(declaration, Invoice);

			Factory.Save();

			GenerateAmendingTransactionAndAssertInvoiceType(job, "ReconDeclaration");
		}

		public void TestInvoiceType_AmendingTransaction_LoadListInvoiceType()
		{
			var loadListConsol = Factory.New<CFSLoadListConsol>();
			loadListConsol.JK_IsForwarding = ZBool.False;
			loadListConsol.JK_IsCFS = ZBool.True;
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = loadListConsol.JK_UniqueConsignRef;
			var job = GetInvoiceJob(loadListConsol, Invoice);

			Factory.Save();

			GenerateAmendingTransactionAndAssertInvoiceType(job, "LoadList");
		}

		public void TestInvoiceType_AmendingTransaction_AgencyShipmentJobType()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_UniqueConsignRef = "S1234";
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			var job = GetInvoiceJob(shipment, Invoice);

			Factory.Save();

			GenerateAmendingTransactionAndAssertInvoiceType(job, "AgencyShipment");
		}

		public void TestInvoiceType_AmendingTransaction_ShipmentInvoiceType()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1234";
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			var job = GetInvoiceJob(shipment, Invoice);

			Factory.Save();

			GenerateAmendingTransactionAndAssertInvoiceType(job, "Shipment");
		}

		public void TestInvoiceType_AmendingTransaction_TransportTypeInvoice()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "T0374839";
			Invoice.AH_ConsolidatedInvoiceRef = cartage.JJ_ConsignmentID;
			Factory.Save();
			var job = GetInvoiceJob(cartage, Invoice);

			Factory.Save();

			GenerateAmendingTransactionAndAssertInvoiceType(job, "Transport");
		}

		public void TestInvoiceType_AmendingTransaction_WarehousePeriodicInvoiceType()
		{
			var warehouseInvoice = Factory.NewWithValidTestData<WhsInvoice>();
			warehouseInvoice.ET_StorageJobNumber = "12345";
			Invoice.AH_ConsolidatedInvoiceRef = warehouseInvoice.ET_StorageJobNumber;
			var job = GetInvoiceJob(warehouseInvoice, Invoice);

			Factory.Save();

			GenerateAmendingTransactionAndAssertInvoiceType(job, "WarehousePeriodic");
		}

		public void TestInvoiceType_AmendingTransaction_WarehouseStocktakeInvoiceType()
		{
			var warehouseStocktake = Factory.NewWithValidTestData<WhsStocktake>();
			warehouseStocktake.WS_StocktakeNumber = "S00001234";
			Invoice.AH_ConsolidatedInvoiceRef = warehouseStocktake.WS_StocktakeNumber;
			var job = GetInvoiceJob(warehouseStocktake, Invoice);

			Factory.Save();

			GenerateAmendingTransactionAndAssertInvoiceType(job, "WarehouseWhsStocktake");
		}

		void GenerateAmendingTransactionAndAssertInvoiceType(JobHeader jobHeader, string expectInvoiceType)
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AT = Factory.NewWithValidTestData(typeof(AccTaxRate)).PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_JH = jobHeader.PK;

			var creditNote = invoice.GenerateAmendingTransaction<ARCreditNote>();

			InvoiceWrapper = DocARInvoice.New(creditNote, Factory);
			AssertEquals("Invoice Type", expectInvoiceType, ARInvoiceWrapper.InvoiceType);
		}

		#endregion

		#endregion

		#region TestAccQueryClaims

		ARAccQueryClaim CreateClaim(ARInvoice invoice, OrgContact contact, ZString desc, ZString details)
		{
			ARAccQueryClaim claim = Factory.New<ARAccQueryClaim>();
			claim.AY_AH = invoice.PK;
			claim.AY_OH_Debtor = invoice.AH_OH;
			claim.AY_ShortDescriptionOfClaim = desc;
			claim.Details = details;
			claim.AY_OC = contact.PK;
			Factory.Save();

			return claim;
		}

		OrgHeader GetDebtor()
		{
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "Debtor";
			OrgContact contact = debtor.Contacts.AddNew();
			contact.OC_ContactName = "ABC";
			Factory.Save();
			return debtor;
		}

		public void TestAccClaimsForInvoice()
		{
			OrgHeader debtor = GetDebtor();
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = debtor.PK;

			ARAccQueryClaim claim1 = CreateClaim(invoice, debtor.Contacts[0], "Desc1", "Details1");

			DocARInvoice wrapper = DocARInvoice.New(invoice, Factory);
			AssertEquals("Num of Claims link to this invoice", 1, wrapper.AccClaimsForInvoice.Count);

			DocAccQueryClaim docClaim = wrapper.AccClaimsForInvoice[0];
			AssertEquals("Claim Reference", "RQ00001000", docClaim.Reference);
		}

		public void TestClaimInfoAsString()
		{
			OrgHeader debtor = GetDebtor();
			ARInvoice invoice = Factory.New<ARInvoice>();

			invoice.AH_OH = debtor.PK;

			AccQueryClaim claim1 = CreateClaim(invoice, debtor.Contacts[0], "Desc1", "Details1");
			AccQueryClaim claim2 = CreateClaim(invoice, debtor.Contacts[0], "Desc2", "Details2");

			DocARInvoice wrapper = DocARInvoice.New(invoice, Factory);

			ZString expectedStr = "(RQ00001000) Desc1 Details1 , (RQ00001001) Desc2 Details2";
			AssertEquals("ClaimInfoASString", expectedStr, wrapper.ClaimInfoAsString);
		}

		#endregion

		#region Test Cost/SellPaymentBases

		public void TestPaymentBases()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "FRT";
			chargeCode.AC_Desc = "Freight";

			var line1 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line1.AL_OSExTaxAmount = 1000M;
			var line2 = (InvoiceLine)InvoicingBase.Lines.AddNew();
			line2.AL_OSExTaxAmount = 50M;
			var lineWithoutCharge = (InvoiceLine)InvoicingBase.Lines.AddNew();
			lineWithoutCharge.AL_OSExTaxAmount = 50M;

			var charge1 = Factory.NewWithValidTestData<Charge>();
			charge1.JR_AC = chargeCode.PK;
			charge1.JR_AL_ARLine = line1.PK;

			var charge2 = Factory.NewWithValidTestData<Charge>();
			charge2.JR_AC = chargeCode.PK;
			charge2.JR_AL_ARLine = line2.PK;

			var pay1Charge1 = charge1.PaymentBases.AddNew();
			pay1Charge1.PBS_JR = charge1.PK;
			pay1Charge1.PBS_ChargeableDescription = "Cost1";
			pay1Charge1.PBS_ChargeableAmount = 10;
			pay1Charge1.PBS_ChargeableUnit = "KG";
			pay1Charge1.PBS_PerUnitRate = 6;
			pay1Charge1.PBS_RateUnit = "KG";
			pay1Charge1.PBS_RX_NKRateCurrency = "BTC";
			pay1Charge1.PBS_AdapterID = "Container";
			pay1Charge1.PBS_IsCost = true;

			var pay2Charge1 = charge1.PaymentBases.AddNew();
			pay2Charge1.PBS_JR = charge1.PK;
			pay2Charge1.PBS_ChargeableDescription = "Sell1";
			pay2Charge1.PBS_ChargeableAmount = 20;
			pay2Charge1.PBS_ChargeableUnit = "KG";
			pay2Charge1.PBS_PerUnitRate = 6;
			pay2Charge1.PBS_RateUnit = "KG";
			pay2Charge1.PBS_RX_NKRateCurrency = "BTC";
			pay2Charge1.PBS_AdapterID = "Container";
			pay2Charge1.PBS_IsCost = false;

			var pay1Charge2 = charge2.PaymentBases.AddNew();
			pay1Charge2.PBS_JR = charge2.PK;
			pay1Charge2.PBS_ChargeableDescription = "Cost2";
			pay1Charge2.PBS_ChargeableAmount = 10;
			pay1Charge2.PBS_ChargeableUnit = "KG";
			pay1Charge2.PBS_PerUnitRate = 6;
			pay1Charge2.PBS_RateUnit = "KG";
			pay1Charge2.PBS_RX_NKRateCurrency = "BTC";
			pay1Charge2.PBS_AdapterID = "Container";
			pay1Charge2.PBS_IsCost = true;

			var pay2Charge2 = charge2.PaymentBases.AddNew();
			pay2Charge2.PBS_JR = charge2.PK;
			pay2Charge2.PBS_ChargeableDescription = "Sell2";
			pay2Charge2.PBS_ChargeableAmount = 10;
			pay2Charge2.PBS_ChargeableUnit = "KG";
			pay2Charge2.PBS_PerUnitRate = 6;
			pay2Charge2.PBS_RateUnit = "KG";
			pay2Charge2.PBS_RX_NKRateCurrency = "BTC";
			pay2Charge2.PBS_AdapterID = "Container";
			pay2Charge2.PBS_IsCost = false;

			DocARInvoice wrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals("CostPaymentBases", "Cost1, Cost2", string.Join(", ", wrapper.CostPaymentBases.Cast<DocJobPaymentBasis>().Select(x => x.Reference).OrderBy(x => x)));
			AssertEquals("SellPaymentBases", "Sell1, Sell2", string.Join(", ", wrapper.SellPaymentBases.Cast<DocJobPaymentBasis>().Select(x => x.Reference).OrderBy(x => x)));
		}

		#endregion

		#region InvoiceAuthorisationRecordAuthorisationData

		public void TestDocWrapperInvoiceAuthorisationRecordAuthorisationData()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			{
				var wrapper = new DocARInvoiceCommonWrapperForTest(InvoicingBase, Factory);
				var expectedValue = "Some string";

				var countryFactoryMock = new Mock<ICountryComplianceFactory>();
				var transactionAuthorisationRecordProvider = new Mock<ITransactionAuthorisationRecordProvider>();

				countryFactoryMock.Setup(x => x.GetTransactionAuthorisationRecordProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
					.Returns(transactionAuthorisationRecordProvider.Object);

				transactionAuthorisationRecordProvider.Setup(x => x.GetDecodedAuthorationData(It.IsAny<ZBlob>()))
					.Returns(expectedValue);

				ObjectFactory.Substitute(countryFactoryMock.Object);
				ObjectFactory.Substitute(transactionAuthorisationRecordProvider.Object);

				var result = wrapper.InvoiceAuthorisationRecordAuthorisationData;

				AssertEquals(expectedValue, result);

				countryFactoryMock.Verify(x => x.GetTransactionAuthorisationRecordProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Times.Once);
				transactionAuthorisationRecordProvider.Verify(x => x.GetDecodedAuthorationData(It.IsAny<ZBlob>()), Times.Once);
			}
		}

		public void TestDocWrapperInvoiceAuthorisationRecordAuthorisationData_GetTransactionAuthorisationRecordProvider_IsNull()
		{
			var countryFactoryMock = new Mock<ICountryComplianceFactory>();
			var transactionAuthorisationRecordProvider = new Mock<ITransactionAuthorisationRecordProvider>();

			countryFactoryMock.Setup(x => x.GetTransactionAuthorisationRecordProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Returns(null as ITransactionAuthorisationRecordProvider);

			ObjectFactory.Substitute(countryFactoryMock.Object);
			ObjectFactory.Substitute(transactionAuthorisationRecordProvider.Object);

			var wrapper = new DocARInvoiceCommonWrapperForTest(InvoicingBase, Factory);
			var result = wrapper.InvoiceAuthorisationRecordAuthorisationData;

			transactionAuthorisationRecordProvider.Verify(x => x.GetDecodedAuthorationData(It.IsAny<ZBlob>()), Times.Never);
			AssertEquals(ZString.Empty, result);
		}

		#endregion

		#region Wrappers Created From Key For Invoice

		#region Declaration

		public void TestAUDeclaration()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertNull("Declaration", ARInvoiceWrapper.Declaration);

			Invoice.Delete();
			Enterprise.Customs.AU.Declaration.Business.JobDeclaration declaration = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			Invoice = Factory.New<ARInvoice>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			JobHeader job = GetInvoiceJob(declaration, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Declaration", ARInvoiceWrapper.Declaration);
			Assert("Declaration is of type Enterprise.DocumentWrappers.Customs.AU.DocDeclaration", ARInvoiceWrapper.Declaration is Customs.AU.DocDeclaration);

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference + " / A";
			job = GetInvoiceJob(declaration, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Declaration", ARInvoiceWrapper.Declaration);
			Assert("Declaration is of type Enterprise.DocumentWrappers.Customs.AU.DocDeclaration", ARInvoiceWrapper.Declaration is Customs.AU.DocDeclaration);

			GlbCompany.CurrentCompany.SetCountry(storedCountry);
		}

		public void TestNZDeclaration()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertNull("Declaration", ARInvoiceWrapper.Declaration);

			Invoice.Delete();
			Enterprise.Customs.NZ.Business.Declaration.JobDeclaration declaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration.New(Factory);
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);

			Invoice = Factory.New<ARInvoice>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			JobHeader job = GetInvoiceJob(declaration, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Declaration", ARInvoiceWrapper.Declaration);
			Assert("Declaration is of type Enterprise.DocumentWrappers.Customs.NZ.DocDeclaration", ARInvoiceWrapper.Declaration is Customs.NZ.DocDeclaration);

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference + " / A";
			job = GetInvoiceJob(declaration, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Declaration", ARInvoiceWrapper.Declaration);
			Assert("Declaration is of type Enterprise.DocumentWrappers.Customs.NZ.DocDeclaration", ARInvoiceWrapper.Declaration is Customs.NZ.DocDeclaration);

			GlbCompany.CurrentCompany.SetCountry(storedCountry);
		}

		public void TestFJDeclaration()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertNull("Declaration", ARInvoiceWrapper.Declaration);

			Invoice.Delete();
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Fiji);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();

			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			JobHeader job = GetInvoiceJob(declaration, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Declaration", ARInvoiceWrapper.Declaration);
			Assert("Declaration is of type Enterprise.DocumentWrappers.Customs.General.DocDeclaration", ARInvoiceWrapper.Declaration is Customs.General.DocDeclaration);

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference + " / A";
			job = GetInvoiceJob(declaration, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Declaration", ARInvoiceWrapper.Declaration);
			Assert("Declaration is of type Enterprise.DocumentWrappers.Customs.General.DocDeclaration", ARInvoiceWrapper.Declaration is Customs.General.DocDeclaration);

			GlbCompany.CurrentCompany.SetCountry(storedCountry);
		}

		#endregion

		#region Shipment

		public void TestShipment()
		{
			AssertNull("Shipment", ARInvoiceWrapper.Shipment);

			Invoice.Delete();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Shipment", ARInvoiceWrapper.Shipment);
			AssertEquals("Shipment is of type DocShipment", typeof(DocForwardingShipment), ARInvoiceWrapper.Shipment.GetType());

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef + " / B";
			job = GetInvoiceJob(shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Shipment", ARInvoiceWrapper.Shipment);
			AssertEquals("Shipment is of type DocShipment", typeof(DocForwardingShipment), ARInvoiceWrapper.Shipment.GetType());
		}

		public void TestCartage()
		{
			AssertNull("Cartage null", ARInvoiceWrapper.Cartage);

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "T9384772";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = cartage.JJ_ConsignmentID;
			JobHeader job = GetInvoiceJob(cartage, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Cartage null", ARInvoiceWrapper.Cartage);

			var cartageShipment = Factory.New<CommonCartage>();
			cartageShipment.JJ_ConsignmentID = "S9384772/I";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = cartageShipment.JJ_ConsignmentID;
			job = GetInvoiceJob(cartageShipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Cartage null", ARInvoiceWrapper.Cartage);
		}

		public void TestCartageAfterDeleting()
		{
			AssertNull("Cartage null", ARInvoiceWrapper.Cartage);

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "T9384772";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = cartage.JJ_ConsignmentID;
			JobHeader job = GetInvoiceJob(cartage, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Cartage null", ARInvoiceWrapper.Cartage);

			Invoice.Delete();
			AssertNull("Cartage null", ARInvoiceWrapper.Cartage);
		}

		public void TestContainerRegistration()
		{
			AssertNull("Container", ARInvoiceWrapper.ContainerRegistration);

			Invoice.Delete();
			var containerRego = Factory.New<CFSContainer>();
			containerRego.JC_ContainerJobID = "D1234";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = containerRego.JC_ContainerJobID;

			SetCFSContainerJob(containerRego, Invoice);

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Container", ARInvoiceWrapper.ContainerRegistration);
			AssertEquals("Container is of type DocContainerRego", typeof(DocPackUnpackContainerRego), ARInvoiceWrapper.ContainerRegistration.GetType());

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = containerRego.JC_ContainerJobID + " / B";

			SetCFSContainerJob(containerRego, Invoice);

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Container", ARInvoiceWrapper.ContainerRegistration);
			AssertEquals("Container is of type DocContainerRego", typeof(DocPackUnpackContainerRego), ARInvoiceWrapper.ContainerRegistration.GetType());
		}

		public void TestConsol()
		{
			AssertNull("Consol", ARInvoiceWrapper.Consol);

			Invoice.Delete();
			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Consol", ARInvoiceWrapper.Consol);
			AssertEquals("Consol is of type DocForwardingConsol", typeof(DocForwardingConsol), ARInvoiceWrapper.Consol.GetType());

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef + " / C";
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Consol", ARInvoiceWrapper.Consol);
			AssertEquals("Consol is of type DocForwardingConsol", typeof(DocForwardingConsol), ARInvoiceWrapper.Consol.GetType());
		}

		public void TestLoadList()
		{
			AssertNull("LoadList", ARInvoiceWrapper.LoadList);

			Invoice.Delete();
			var loadList = Factory.New<CFSLoadListConsol>();
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = loadList.JK_UniqueConsignRef;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			JobHeader job = GetInvoiceJob(loadList, Invoice);
			AssertNotNull("LoadList", ARInvoiceWrapper.LoadList);
			AssertEquals("LoadList is of type DocLoadListConsol", typeof(DocLoadListConsol), ARInvoiceWrapper.LoadList.GetType());

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = loadList.JK_UniqueConsignRef + " / C";
			job = GetInvoiceJob(loadList, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("LoadList", ARInvoiceWrapper.LoadList);
			AssertEquals("LoadList is of type DocLoadListConsol", typeof(DocLoadListConsol), ARInvoiceWrapper.LoadList.GetType());
		}

		protected JobHeader SetCFSContainerJob(CFSContainer containerRego, TransactionHeader wrappedInvoice)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobContainerSchema.Constants.Prefix;
			job.JH_ParentID = containerRego.PK;
			wrappedInvoice.AH_JH = job.PK;
			return job;
		}

		#endregion

		#region Wrappers Created From Key For Invoice

		public void TestWorkItem()
		{
			AssertNull("WorkItem", ARInvoiceWrapper.WorkItem);

			Invoice.Delete();
			var workItem = Factory.New<IWorkItem>();
			((BusinessObject)workItem).FillWithValidTestData();
			workItem.WKI_WorkItemNumber = "WI00001234";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = workItem.WKI_WorkItemNumber;
			var job = GetInvoiceJob((IJobInvoicingPlugIn)workItem, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("WorkItem", ARInvoiceWrapper.WorkItem);
			AssertEquals("WorkItem is of type DocBaseWorkItem", typeof(DocBaseWorkItem), ARInvoiceWrapper.WorkItem.GetType());

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = workItem.WKI_WorkItemNumber + " / C";
			job = GetInvoiceJob((IJobInvoicingPlugIn)workItem, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("WorkItem", ARInvoiceWrapper.WorkItem);
			AssertEquals("WorkItem is of type DocBaseWorkItem", typeof(DocBaseWorkItem), ARInvoiceWrapper.WorkItem.GetType());
		}

		public void TestWarehouseAdHocServiceJob()
		{
			AssertNull("WarehouseAdHocServiceJob", ARInvoiceWrapper.WarehouseAdHocServiceJob);

			Invoice.Delete();
			var adHocServiceJob = Factory.NewWithValidTestData<WhsAdHocServiceJob>();
			adHocServiceJob.WSJ_JobNumber = "WI00001234";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = adHocServiceJob.WSJ_JobNumber;
			JobHeader job = GetInvoiceJob(adHocServiceJob, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("WarehouseAdHocServiceJob", ARInvoiceWrapper.WarehouseAdHocServiceJob);
			AssertEquals("WarehouseAdHocServiceJob", ARInvoiceWrapper.InvoiceType);
			AssertEquals("WarehouseAdHocServiceJob is of type DocWarehouseAdHocServiceJob", typeof(DocWarehouseAdHocServiceJob), ARInvoiceWrapper.WarehouseAdHocServiceJob.GetType());

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = adHocServiceJob.WSJ_JobNumber + " / C";
			job = GetInvoiceJob(adHocServiceJob, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("WarehouseAdHocServiceJob", ARInvoiceWrapper.WarehouseAdHocServiceJob);
			AssertEquals("WarehouseAdHocServiceJob is of type DocWarehouseAdHocServiceJob", typeof(DocWarehouseAdHocServiceJob), ARInvoiceWrapper.WarehouseAdHocServiceJob.GetType());
		}

		public void TestWarehouseAdHocServiceJobWhenWorkItemIsNull()
		{
			var adHocServiceJob = Factory.NewWithValidTestData<WhsAdHocServiceJob>();
			adHocServiceJob.WSJ_JobNumber = "WI00001234";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = adHocServiceJob.WSJ_JobNumber;
			GetInvoiceJob(adHocServiceJob, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			var job = Factory.Load<WhsAdHocServiceJob>(ARInvoiceWrapper.TransactionHeader.Job.JH_ParentID);

			job.JobHeader.MarkAsInactive();
			AssertNull("WarehouseAdHocServiceJob", ARInvoiceWrapper.WarehouseAdHocServiceJob);
		}

		public void TestWarehouseAdHocServiceJobWhenPKIsAWorkItem()
		{
			Invoice.Delete();
			var workItem = Factory.New<IWorkItem>();
			((BusinessObject)workItem).FillWithValidTestData();
			workItem.WKI_WorkItemNumber = "WI00001234";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = workItem.WKI_WorkItemNumber;
			var job = GetInvoiceJob((IJobInvoicingPlugIn)workItem, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("WorkItem", ARInvoiceWrapper.WorkItem);
			AssertNull("WarehouseAdHocServiceJob", ARInvoiceWrapper.WarehouseAdHocServiceJob);
		}

		public void TestWhsDocketForWhsReceive()
		{
			AssertNull("WarehouseDocket", ARInvoiceWrapper.WarehouseDocket);

			Invoice.Delete();
			var docket = Factory.NewWithValidTestData<WhsReceive>();
			docket.WD_DocketID = "W00001234";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = docket.WD_DocketID;
			JobHeader job = GetInvoiceJob(docket, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("WarehouseDocket", ARInvoiceWrapper.WarehouseDocket);
			AssertEquals("WarehouseDocket is of type DocWhsReceive", typeof(DocWhsReceive), ARInvoiceWrapper.WarehouseDocket.GetType());

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = docket.WD_DocketID + " / C";
			job = GetInvoiceJob(docket, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("WarehouseDocket", ARInvoiceWrapper.WarehouseDocket);
			AssertEquals("WarehouseDocket is of type DocWhsReceive", typeof(DocWhsReceive), ARInvoiceWrapper.WarehouseDocket.GetType());
		}

		public void TestWhsDocketForOrdersDocket()
		{
			AssertNull("WarehouseDocket", ARInvoiceWrapper.WarehouseDocket);

			Invoice.Delete();
			var docket = Factory.NewWithValidTestData<WhsOrder>();
			docket.WD_DocketID = "W00001234";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = docket.WD_DocketID;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			JobHeader job = GetInvoiceJob(docket, Invoice);
			AssertNotNull("WarehouseDocket", ARInvoiceWrapper.WarehouseDocket);
			AssertEquals("WarehouseDocket is of type DocWhsOrder", typeof(DocWhsOrder), ARInvoiceWrapper.WarehouseDocket.GetType());

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = docket.WD_DocketID + " / C";
			job = GetInvoiceJob(docket, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("WarehouseDocket", ARInvoiceWrapper.WarehouseDocket);
			AssertEquals("WarehouseDocket is of type DocWhsOrder", typeof(DocWhsOrder), ARInvoiceWrapper.WarehouseDocket.GetType());
		}

		public void TestPrintingDeclarationDetailsFromAnotherCompanyInSameCountry()
		{
			var newZealand = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.NewZealand);
			var australia = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);

			ZQuery branchThatIsNotInCurrentCompanyFilter = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var branchFromAnotherCompanyWithSameCountry = Factory.LoadTop1<GlbBranch>(branchThatIsNotInCurrentCompanyFilter);

			branchFromAnotherCompanyWithSameCountry.Company.GC_RN_NKCountryCode = newZealand.Code;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = newZealand.Code;

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GB = branchFromAnotherCompanyWithSameCountry.PK;
			declaration.JE_DeclarationReference = NumberFountains.CustomsJobNumberFountainPrefix + "00001234";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference + "/A";
			JobHeader job = GetInvoiceJob(declaration, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull("Declaration Wrapper should not be null", ARInvoiceWrapper.Declaration);
			AssertEquals("Declaration Reference", declaration.JE_DeclarationReference, ARInvoiceWrapper.Declaration.DeclarationReference);
		}

		public void TestPrintingDeclarationDetailsFromAnotherCompanyInAnotherCountry()
		{
			var newZealand = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.NewZealand);
			var australia = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);

			ZQuery branchThatIsNotInCurrentCompanyFilter = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var branchFromAnotherCompanyWithSameCountry = Factory.LoadTop1<GlbBranch>(branchThatIsNotInCurrentCompanyFilter);

			branchFromAnotherCompanyWithSameCountry.Company.GC_RN_NKCountryCode = australia.Code;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = newZealand.Code;

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GB = branchFromAnotherCompanyWithSameCountry.PK;
			declaration.JE_DeclarationReference = NumberFountains.CustomsJobNumberFountainPrefix + "00001234";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference + "/A";
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNull("Declaration Wrapper should be null", ARInvoiceWrapper.Declaration);
		}

		#endregion

		#region Tests For Invoice Type Reference Number

		#region Declaration

		public void TestInvoiceTypeReferenceNumberForAUDeclaration()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			Invoice.AH_ConsolidatedInvoiceRef = ZString.Empty;
			AssertEquals("InvoiceTypeReferenceNumber", ZString.Empty, ARInvoiceWrapper.InvoiceTypeReferenceNumber);

			Invoice.Delete();
			Enterprise.Customs.AU.Declaration.Business.JobDeclaration declaration = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
			declaration.JE_DeclarationReference = "B1234";
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			JobHeader job = GetInvoiceJob(declaration, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("InvoiceTypeReferenceNumber", declaration.JE_DeclarationReference, ARInvoiceWrapper.InvoiceTypeReferenceNumber);

			GlbCompany.CurrentCompany.SetCountry(storedCountry);
		}

		public void TestInvoiceTypeReferenceNumberForNZDeclaration()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Invoice.AH_ConsolidatedInvoiceRef = ZString.Empty;
			AssertEquals("InvoiceTypeReferenceNumber", ZString.Empty, ARInvoiceWrapper.InvoiceTypeReferenceNumber);

			Invoice.Delete();
			Enterprise.Customs.NZ.Business.Declaration.JobDeclaration declaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B1234";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			JobHeader job = GetInvoiceJob(declaration, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("InvoiceTypeReferenceNumber", declaration.JE_DeclarationReference, ARInvoiceWrapper.InvoiceTypeReferenceNumber);

			GlbCompany.CurrentCompany.SetCountry(storedCountry);
		}

		public void TestInvoiceTypeReferenceNumberForFJDeclaration()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Invoice.AH_ConsolidatedInvoiceRef = ZString.Empty;
			AssertEquals("InvoiceTypeReferenceNumber", ZString.Empty, ARInvoiceWrapper.InvoiceTypeReferenceNumber);

			Invoice.Delete();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B1234";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Fiji);

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			JobHeader job = GetInvoiceJob(declaration, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("InvoiceTypeReferenceNumber", declaration.JE_DeclarationReference, ARInvoiceWrapper.InvoiceTypeReferenceNumber);

			GlbCompany.CurrentCompany.SetCountry(storedCountry);
		}

		#endregion

		public virtual void TestInvoiceTypeReferenceNumberForTransport()
		{
			if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				var cartage = Factory.New<CommonCartage>();
				cartage.JJ_ConsignmentID = "T0374839";
				Invoice.AH_ConsolidatedInvoiceRef = cartage.JJ_ConsignmentID;
				Factory.Save();
				cartage.JJ_ConsignmentID = "T0374839";
				Factory.Save();

				JobHeader job = GetInvoiceJob(cartage, Invoice);
				InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
				AssertEquals("InvoiceTypeReferenceNumber", cartage.JJ_ConsignmentID, ARInvoiceWrapper.InvoiceTypeReferenceNumber);
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestInvoiceTypeReferenceNumberForContainerRego()
		{
			if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				var containerRego = Factory.New<CFSContainer>();
				containerRego.JC_ContainerJobID = "D12345678";
				Invoice.AH_ConsolidatedInvoiceRef = containerRego.JC_ContainerJobID;
				Factory.Save();

				SetCFSContainerJob(containerRego, Invoice);
				InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
				AssertEquals("InvoiceTypeReferenceNumber", containerRego.JC_ContainerJobID, ARInvoiceWrapper.InvoiceTypeReferenceNumber);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestInvoiceTypeReferenceNumberForShipment()
		{
			Invoice.Delete();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1234";
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("InvoiceTypeReferenceNumber", shipment.JS_UniqueConsignRef, ARInvoiceWrapper.InvoiceTypeReferenceNumber);

			job.Delete();

			shipment.JS_IsCFSRegistered = ZBool.True;
			Invoice.AH_JH = ZGuid.Empty;
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("InvoiceTypeReferenceNumber", shipment.JS_UniqueConsignRef, ARInvoiceWrapper.InvoiceTypeReferenceNumber);

			shipment.JS_UniqueConsignRef = "H1234";
			shipment.JS_IsCFSRegistered = ZBool.True;
			shipment.JS_IsForwardRegistered = ZBool.False;

			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("InvoiceTypeReferenceNumber", shipment.JS_UniqueConsignRef, ARInvoiceWrapper.InvoiceTypeReferenceNumber);
		}

		public void TestInvoiceTypeReferenceNumberForConsol()
		{
			Invoice.Delete();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C1234";
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("InvoiceTypeReferenceNumber", consol.JK_UniqueConsignRef, ARInvoiceWrapper.InvoiceTypeReferenceNumber);

			Invoice.Delete();
			consol.JK_IsCFS = ZBool.True;
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("InvoiceTypeReferenceNumber", consol.JK_UniqueConsignRef, ARInvoiceWrapper.InvoiceTypeReferenceNumber);

			consol.JK_UniqueConsignRef = "L1234";
			consol.JK_IsCFS = ZBool.True;
			consol.JK_IsForwarding = ZBool.False;
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("InvoiceTypeReferenceNumber", consol.JK_UniqueConsignRef, ARInvoiceWrapper.InvoiceTypeReferenceNumber);
		}

		public void TestInvoiceTypeReferenceNumberForWarehousePeriodic()
		{
			var warehouseInvoice = Factory.NewWithValidTestData<WhsInvoice>();
			warehouseInvoice.ET_StorageJobNumber = "00001234";
			Invoice.AH_ConsolidatedInvoiceRef = warehouseInvoice.ET_StorageJobNumber;
			JobHeader job = GetInvoiceJob(warehouseInvoice, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "WarehousePeriodic", ARInvoiceWrapper.InvoiceType);
			AssertEquals("Invoice Type", "00001234", ARInvoiceWrapper.InvoiceTypeReferenceNumber);
		}

		public void TestInvoiceTypeReferenceNumberForWarehouseReceive()
		{
			var docket = Factory.NewWithValidTestData<WhsReceive>();
			Factory.Save(); // force system to allocate WD_DocketID first, before we can override it
			docket.WD_DocketID = "W00001234";
			Invoice.AH_ConsolidatedInvoiceRef = docket.WD_DocketID;
			Factory.Save();
			JobHeader job = GetInvoiceJob(docket, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "WarehouseWhsInwards", ARInvoiceWrapper.InvoiceType);
			AssertEquals("Invoice Type", "W00001234", ARInvoiceWrapper.InvoiceTypeReferenceNumber);
		}

		public void TestInvoiceTypeReferenceNumberForWarehouseOrders()
		{
			var docket = Factory.NewWithValidTestData<WhsOrder>();
			Factory.Save(); // force system to allocate WD_DocketID first, before we can override it
			docket.WD_DocketID = "W00004321";
			Invoice.AH_ConsolidatedInvoiceRef = docket.WD_DocketID;
			Factory.Save();
			JobHeader job = GetInvoiceJob(docket, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "W00004321", ARInvoiceWrapper.InvoiceTypeReferenceNumber);
		}

		public void TestInvoiceTypeReferenceNumberForWarehouseAdHocServiceJob()
		{
			var adHocServiceJob = Factory.NewWithValidTestData<WhsAdHocServiceJob>();
			Factory.Save();

			adHocServiceJob.WSJ_CustomerReference = "Ad Hoc Service Job 001";
			Invoice.AH_ConsolidatedInvoiceRef = adHocServiceJob.WSJ_CustomerReference;
			Factory.Save();

			var job = GetInvoiceJob(adHocServiceJob, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("InvoiceTypeReferenceNumber", "Ad Hoc Service Job 001", ARInvoiceWrapper.InvoiceTypeReferenceNumber);
		}

		public void TestInvoiceTypeReferenceNumberForWarehouseAdHocServiceJob_WhenWarehouseAdHocServiceJobIsNull()
		{
			var adHocServiceJob = Factory.NewWithValidTestData<WhsAdHocServiceJob>();
			Factory.Save();

			var job = GetInvoiceJob(adHocServiceJob);
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;
			Factory.Save();

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AT = Factory.NewWithValidTestData(typeof(AccTaxRate)).PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_JH = job.PK;

			var creditNote = invoice.GenerateAmendingTransaction<ARCreditNote>();
			InvoiceWrapper = DocARInvoice.New(creditNote, Factory);

			AssertEquals("InvoiceType Should Be WarehouseAdHocServiceJob", "WarehouseAdHocServiceJob", ARInvoiceWrapper.InvoiceType);
			AssertEquals("WarehouseAdHocServiceJob Should Be null", null, ARInvoiceWrapper.WarehouseAdHocServiceJob);
			AssertEquals("InvoiceTypeReferenceNumber Should Be Empty", ZString.Empty, ARInvoiceWrapper.InvoiceTypeReferenceNumber);
		}

		#endregion

		#region Tests for Invoice Type Reference Number Heading

		public void TestInvoiceTypeReferenceNumberHeadingForWhsAdHocServiceJob()
		{
			Invoice.AH_ConsolidatedInvoiceRef = ZString.Empty;
			AssertEquals("InvoiceTypeReferenceNumberHeading", ZString.Empty, ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);

			Invoice.AH_ConsolidatedInvoiceRef = "ADHoc001";
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			adHocServiceJob.WSJ_CustomerReference = "ADHoc001";

			var job = GetInvoiceJob(adHocServiceJob, Invoice);
			AssertEquals("InvoiceTypeReferenceNumberHeading", "CUSTOMER REF:", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);
			AssertEquals("InvoiceTypeReferenceNumberHeadingNoColon", "Customer Ref", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeadingNoColon);
		}

		public void TestInvoiceTypeReferenceNumberHeadingForDeclaration()
		{
			Invoice.AH_ConsolidatedInvoiceRef = ZString.Empty;
			AssertEquals("InvoiceTypeReferenceNumberHeading", ZString.Empty, ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);

			Invoice.AH_ConsolidatedInvoiceRef = "B120";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B120";
			JobHeader job = GetInvoiceJob(declaration, Invoice);
			AssertEquals("InvoiceTypeReferenceNumberHeading", "DECLARATION:", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);
			AssertEquals("InvoiceTypeReferenceNumberHeadingNoColon", "Declaration", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeadingNoColon);
		}

		public void TestInvoiceTypeReferenceNumberHeadingForReconDeclaration()
		{
			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);

			Invoice.AH_ConsolidatedInvoiceRef = ZString.Empty;
			AssertEquals("InvoiceTypeReferenceNumberHeading", ZString.Empty, ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);

			Invoice.AH_ConsolidatedInvoiceRef = "B120";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B120";
			declaration.JE_MessageType = "REC";

			var job = GetInvoiceJob(declaration, Invoice);
			AssertEquals("InvoiceTypeReferenceNumberHeading", "RECONCILIATION:", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);
			AssertEquals("InvoiceTypeReferenceNumberHeadingNoColon", "Reconciliation", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeadingNoColon);

			GlbCompany.CurrentCompany.SetCountry(country);
		}

		public void TestInvoiceTypeReferenceNumberHeadingForTransport()
		{
			var cartage = Factory.New<CommonCartage>();
			Invoice.AH_ConsolidatedInvoiceRef = "T92844728";
			JobHeader job = GetInvoiceJob(cartage, Invoice);
			AssertEquals("InvoiceTypeReferenceNumberHeading", "TRANSPORT:", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);
			AssertEquals("InvoiceTypeReferenceNumberHeadingNoColon", "Transport", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeadingNoColon);
		}

		public void TestInvoiceTypeReferenceNumberHeadingForContainerReg()
		{
			Invoice.AH_ConsolidatedInvoiceRef = "D120";
			AssertEquals("InvoiceTypeReferenceNumberHeading", "", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);
			AssertEquals("InvoiceTypeReferenceNumberHeadingNoColon", "", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeadingNoColon);
		}

		public void TestInvoiceTypeReferenceNumberHeadingForShipment()
		{
			Invoice.Delete();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1234";
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "SHIPMENT:", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);
			AssertEquals("Invoice Type", "Shipment", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeadingNoColon);

			job.Delete();
			Invoice.Delete();

			shipment.JS_IsCFSRegistered = ZBool.True;
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			job = GetInvoiceJob(shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Invoice Type", "SHIPMENT:", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);
			AssertEquals("Invoice Type", "Shipment", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeadingNoColon);
		}

		public void TestInvoiceTypeReferenceNumberHeadingForConsol()
		{
			Invoice.Delete();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C120";
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("InvoiceTypeReferenceNumberHeading", "CONSOL:", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);
			AssertEquals("InvoiceTypeReferenceNumberHeadingNoColon", "Consol", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeadingNoColon);
		}

		public void TestInvoiceTypeReferenceNumberHeadingForLoadList()
		{
			Invoice.Delete();
			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "C120";
			loadList.JK_IsCFS = ZBool.True;
			loadList.JK_IsForwarding = ZBool.False;
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = loadList.JK_UniqueConsignRef;
			JobHeader job = GetInvoiceJob(loadList, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("InvoiceTypeReferenceNumberHeadinge", "LOAD LIST:", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);
			AssertEquals("InvoiceTypeReferenceNumberHeadingNoColon", "Load List", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeadingNoColon);

			Invoice.Delete();
			loadList.JK_IsCFS = ZBool.True;
			loadList.JK_IsForwarding = ZBool.True;
			Factory.Save();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = loadList.JK_UniqueConsignRef;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("InvoiceTypeReferenceNumberHeadinge", "CONSOL:", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);
			AssertEquals("InvoiceTypeReferenceNumberHeadingNoColon", "Consol", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeadingNoColon);
		}

		#endregion

		#region Sub-Totals

		public void TestSubTotalRollUpForNonShipment()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = "S1234";
			JobHeader job = GetInvoiceJob(dec, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddCustomsChargeToInvoice();

			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpAll();
			Factory.Save();
			job.LocalChargesPK = Invoice.Header.PK;
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "All charges except Customs Duty and Tax", theLines[0].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Customs Charge", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[1].OSTaxDisplay);

			InvoicingBase.Lines.RemoveAndDeleteAll();
			line1 = AddOriginChargeToInvoice();
			line2 = AddCustomsChargeToInvoice();
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17", theLines[1].OSTaxDisplay);
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22", theLines[0].OSTaxDisplay);
		}

		public void TestSubTotalOfLinesForCurrency()
		{
			Shipment = Factory.New<ForwardingShipment>();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = "S1234";
			JobHeader job = GetInvoiceJob(Shipment, Invoice);

			AddChargeWithSpecifiedCurrency((ARInvoice)Invoice, "Freight", "AUD", "JPY");
			AddChargeWithSpecifiedCurrency((ARInvoice)Invoice, "Origin", "AUD", "JPY");
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpSubTotals();
			job.LocalChargesPK = Invoice.Header.PK;
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			rollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CLC;
			Factory.Save();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals(4, theLines.Count);
			AssertEquals("JPY", theLines[0].Currency.Code);
			AssertEquals("JPY", theLines[1].Currency.Code);
			AssertEquals("This is the total line", "JPY", theLines[2].Currency.Code);
			AssertNull("This is the spacer line", theLines[3].Currency);
		}

		public void TestSubTotalOfLinesForChargeOSAmountForCLC()
		{
			Shipment = Factory.New<ForwardingShipment>();
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = "S1234";
			JobHeader job = GetInvoiceJob(Shipment, Invoice);

			AddChargeWithSpecifiedCurrency((ARInvoice)Invoice, "Freight", "AUD");
			AddChargeWithSpecifiedCurrency((ARInvoice)Invoice, "Origin", "AUD");
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpSubTotals();
			job.LocalChargesPK = Invoice.Header.PK;
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			rollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CLC;
			Factory.Save();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals(4, theLines.Count);
			AssertEquals(250m, theLines[0].ChargeOSAmountForCLC);
			AssertEquals(250m, theLines[1].ChargeOSAmountForCLC);
			AssertEquals("This is the total line", 500m, theLines[2].ChargeOSAmountForCLC);
			AssertEquals("This is spacer line", 0m, theLines[3].ChargeOSAmountForCLC);
		}

		public void TestSubTotalOfLinesWithNoGrouping()
		{
			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.All;
			Shipment.JS_IsCFSRegistered = ZBool.True;
			Shipment.JS_IsForwardRegistered = ZBool.False;
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = "S1234";
			JobHeader job = GetInvoiceJob(Shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AddFreightChargeToInvoice();
			AddOriginChargeToInvoice();

			AddFreightChargeToInvoice();
			AddLoadChargeToInvoice();
			AddLoadChargeToInvoice();

			AccChargeCode cfsChargeCode = Factory.New<AccChargeCode>();
			cfsChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSShipment;
			cfsChargeCode.AC_AG_RevenueAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;
			ARInvoiceLine line = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line.AL_Desc = "CFS Charge";
			line.AL_OSExTaxAmount = 350.00M;
			line.AL_OSTaxAmount = 30.00M;
			line.AL_AC = cfsChargeCode.PK;
			line.AL_AT = GetRate();

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpSubTotals();
			job.LocalChargesPK = Invoice.Header.PK;
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			rollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.None;
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.CFSShipment.Code;
			Factory.Save();
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("The lines must not group.", 6, theLines.Count);

			AssertSubTotalLine(theLines[0], "CFS Charge", 350m, false, false, false);
			AssertSubTotalLine(theLines[1], "Freight", 250m, false, false, false);
			AssertSubTotalLine(theLines[2], "Freight", 250m, false, false, false);
			AssertSubTotalLine(theLines[3], "Loading", 350m, false, false, false);
			AssertSubTotalLine(theLines[4], "Origin", 150m, false, false, false);
			AssertSubTotalLine(theLines[5], "Loading", 350m, false, false, false);
		}

		public void TestPrintSequenceIsMaintainedForSubTotalAndSquence()
		{
			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.All;
			Shipment.JS_IsCFSRegistered = ZBool.True;
			Shipment.JS_IsForwardRegistered = ZBool.False;
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = "S1234";
			JobHeader job = GetInvoiceJob(Shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AddFreightChargeToInvoice("", 2);
			AddOriginChargeToInvoice("", 3);
			AddFreightChargeToInvoice("", 2);
			AddLoadChargeToInvoice("", 1);
			AddLoadChargeToInvoice("", 1);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpSubTotals();
			job.LocalChargesPK = Invoice.Header.PK;
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			rollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CLC;
			Factory.Save();

			var theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Line count.", 7, theLines.Count);
			AssertSubTotalLine(theLines[0], "Freight", 250m, false, true, false);
			AssertSubTotalLine(theLines[1], "Origin", 150m, false, true, false);
			AssertSubTotalLine(theLines[2], "Freight", 250m, false, true, false);
			AssertSubTotalLine(theLines[3], "Loading", 350m, false, true, false);
			AssertSubTotalLine(theLines[4], "Loading", 350m, false, true, false);
			AssertSubTotalLine(theLines[5], ZString.Empty, 1350m, false, false, true, 136.51m);
			AssertSubTotalLine(theLines[6], ZString.Empty, 0m, true, false, false);

			rollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence;
			Factory.Save();

			theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Line count.", 7, theLines.Count);
			AssertSubTotalLine(theLines[0], "Loading", 350m, false, true, false);
			AssertSubTotalLine(theLines[1], "Loading", 350m, false, true, false);
			AssertSubTotalLine(theLines[2], "Freight", 250m, false, true, false);
			AssertSubTotalLine(theLines[3], "Freight", 250m, false, true, false);
			AssertSubTotalLine(theLines[4], "Origin", 150m, false, true, false);
			AssertSubTotalLine(theLines[5], ZString.Empty, 1350m, false, false, true, 136.51m);
			AssertSubTotalLine(theLines[6], ZString.Empty, 0m, true, false, false);
		}

		public void TestSubTotalOfLinesWithNoGroupingConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000111";

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AccChargeCode shippingChargeCode = Factory.New<AccChargeCode>();
			shippingChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ShippingDisbursements;
			shippingChargeCode.SetGLAccountDataForTesting();

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpSubTotals();
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			rollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.None;
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;

			Invoice.AH_FullyPaidDate = ZDateTime.Empty;

			for (int i = 0; i < 2; i++)
			{
				Shipment = consol.Shipments.AddNew();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.All;

				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = Invoice.Header.PK;

				foreach (InvoicingLineBase line in new[] {
										AddFreightChargeToInvoice(),
										AddOriginChargeToInvoice(),
										AddFreightChargeToInvoice(),
										AddLoadChargeToInvoice(),
										AddLoadChargeToInvoice() }
				)
				{
					line.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, TestObjectCreator.AUD);
				}

				ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				line1.AL_Desc = "Other Charge";
				line1.AL_OSExTaxAmount = 350.00M;
				line1.AL_OSTaxAmount = 30.00M;
				line1.AL_AC = shippingChargeCode.PK;
				line1.AL_AT = GetRate();
				line1.AL_JH = job.PK;
				TestObjectCreator.CreateJobCharge(line1, job, shippingChargeCode, TestObjectCreator.AUD);
			}

			Factory.Save();
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("The line must not be grouped", 12, theLines.Count);

			AssertSubTotalLine(theLines[0], "Other Charge", 350m, false, false, false, consol.Shipments[0]);
			AssertSubTotalLine(theLines[1], "Freight", 250m, false, false, false, consol.Shipments[0]);
			AssertSubTotalLine(theLines[2], "Freight", 250m, false, false, false, consol.Shipments[0]);
			AssertSubTotalLine(theLines[3], "Loading", 350m, false, false, false, consol.Shipments[0]);
			AssertSubTotalLine(theLines[4], "Origin", 150m, false, false, false, consol.Shipments[0]);
			AssertSubTotalLine(theLines[5], "Loading", 350m, false, false, false, consol.Shipments[0]);

			AssertSubTotalLine(theLines[6], "Other Charge", 350m, false, false, false, consol.Shipments[1]);
			AssertSubTotalLine(theLines[7], "Loading", 350m, false, false, false, consol.Shipments[1]);
			AssertSubTotalLine(theLines[8], "Freight", 250m, false, false, false, consol.Shipments[1]);
			AssertSubTotalLine(theLines[9], "Freight", 250m, false, false, false, consol.Shipments[1]);
			AssertSubTotalLine(theLines[10], "Loading", 350m, false, false, false, consol.Shipments[1]);
			AssertSubTotalLine(theLines[11], "Origin", 150m, false, false, false, consol.Shipments[1]);
		}

		public void TestGroupOrSubtotalWithDifferentCotainerModes()
		{
			var rollupOrGroupCollection = TestObjectCreator.AALSHI.CompanyData.InvoiceRollupOrGroups;
			rollupOrGroupCollection.RemoveAndDeleteAll();
			Factory.Save();
			rollupOrGroupCollection.RemoveAndDeleteAll();

			var invoiceRollupOrGroup = rollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.FRT;

			invoiceRollupOrGroup = rollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.Sequence;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.None;

			Factory.Save();

			var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("1", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("2", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var shipment1 = TestObjectCreator.CreateShipment("S1");
			shipment1.JS_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			var shipment2 = TestObjectCreator.CreateShipment("S2");
			shipment2.JS_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			invoice1.AH_JH = job1.PK;
			invoice2.AH_JH = job2.PK;

			var docInvoice1 = (DocARInvoiceCommon)DocARInvoice.New(invoice1, Factory);
			var docInvoice2 = (DocARInvoiceCommon)DocARInvoice.New(invoice2, Factory);

			AssertGroupOrSubtotal(shipment1, docInvoice1, OrgConstants.GroupOrSubTotalCharges.Code.RollUp);
			AssertGroupOrSubtotal(shipment2, docInvoice2, OrgConstants.GroupOrSubTotalCharges.Code.Sequence);

			AssertGroupOrSubtotalStyle(shipment1, docInvoice1, OrgConstants.InvoiceLineGroupings.Code.FRT);
			AssertGroupOrSubtotalStyle(shipment2, docInvoice2, OrgConstants.InvoiceLineGroupings.Code.None);
		}

		void AssertGroupOrSubtotal(ForwardingShipment shipment, DocARInvoiceCommon docInvoice, ZString expected)
		{
			foreach (CodeDescriptionPair mode in FreightCodePairLists.JS_PackingModeList(shipment.JS_TransportMode))
			{
				shipment.JS_PackingMode = mode.Code;
				AssertEquals(expected, docInvoice.GroupOrSubtotal);
			}
		}

		void AssertGroupOrSubtotalStyle(ForwardingShipment shipment, DocARInvoiceCommon docInvoice, ZString expected)
		{
			foreach (CodeDescriptionPair mode in FreightCodePairLists.JS_PackingModeList(shipment.JS_TransportMode))
			{
				shipment.JS_PackingMode = mode.Code;
				AssertEquals(expected, docInvoice.GroupOrSubtotalStyle);
			}
		}

		public void TestGroupOrSubtotalStyleEmpty()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			DocARInvoice docInvoice = DocARInvoice.New(Invoice, Factory);
			AssertEquals("", docInvoice.GroupOrSubtotalStyle);
		}

		public void TestSubTotalOfLinesWithGroupByChargeCode()
		{
			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.All;
			Shipment.JS_IsCFSRegistered = ZBool.True;
			Shipment.JS_IsForwardRegistered = ZBool.False;
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = "S1234";
			JobHeader job = GetInvoiceJob(Shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Desc = "Charge Code 1 Description";

			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line1.AL_Desc = "CFS Charge 1";
			line1.AL_OSExTaxAmount = 350.00M;
			line1.AL_OSTaxAmount = 30.00M;
			line1.AL_AC = chargeCode1.PK;
			line1.AL_AT = GetRate();

			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line2.AL_Desc = "CFS Charge 2";
			line2.AL_OSExTaxAmount = 400.00M;
			line2.AL_OSTaxAmount = 40.00M;
			line2.AL_AC = chargeCode1.PK;
			line2.AL_AT = GetRate();

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Desc = "Charge Code 2 Description";

			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line3.AL_Desc = "CFS Charge 3";
			line3.AL_OSExTaxAmount = 500.00M;
			line3.AL_OSTaxAmount = 50.00M;
			line3.AL_AC = chargeCode2.PK;
			line3.AL_AT = GetRate();

			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode3.AC_Desc = "Charge Code 3 Description";

			ARInvoiceLine line4 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line4.AL_Desc = "Charge Without Tax";
			line4.AL_AC = chargeCode3.PK;
			line4.AL_OSExTaxAmount = 300.00M;

			var chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode4.AC_Desc = "Charge Code 4 Description";

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>(TestBusinessObjectKind.MinimumRequiredToSave);
			taxRate.AT_Code = "XXXXX";
			taxRate.AT_Type = AccTaxRate.Types.NotReportable;
			taxRate.SetRateNumerator_ForTestOnly(0);

			ARInvoiceLine line5 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line5.AL_Desc = "Charge Not Reportable Tax";
			line5.AL_AC = chargeCode4.PK;
			line5.AL_OSExTaxAmount = 350.00M;

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpSubTotals();
			job.LocalChargesPK = Invoice.Header.PK;

			line4.AL_AT = ZGuid.Empty;
			line4.AL_OSTaxAmount = 0M;
			line5.AL_AT = taxRate.PK;
			line5.AL_OSTaxAmount = 0M;

			AccGLHeader gLAccount1 = Factory.NewWithValidTestData<AccGLHeader>();
			gLAccount1.AG_Description = "GL Account 1 Description";

			ARInvoiceLine line6 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line6.AL_Desc = "GL Account 1 first";
			line6.AL_AG = gLAccount1.PK;
			line6.AL_OSExTaxAmount = 100.00M;

			ARInvoiceLine line7 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line7.AL_Desc = "GL Account 1 second";
			line7.AL_AG = gLAccount1.PK;
			line7.AL_OSExTaxAmount = 200.00M;

			AccGLHeader gLAccount2 = Factory.NewWithValidTestData<AccGLHeader>();
			gLAccount2.AG_Description = "GL Account 2 Description";

			ARInvoiceLine line8 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line8.AL_Desc = "GL Account 2 first";
			line8.AL_AG = gLAccount2.PK;
			line8.AL_OSExTaxAmount = 110.00M;

			ARInvoiceLine line9 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line9.AL_Desc = "GL Account 2 second";
			line9.AL_AG = gLAccount2.PK;
			line9.AL_OSExTaxAmount = 230.00M;

			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			rollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CCD;
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.CFSShipment.Code;
			Factory.Save();
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("17 lines = 5 charges + 4 GL accounts + 6 sub total + 6 spacer lines", 21, theLines.Count);

			AssertSubTotalLine(theLines[0], "CFS Charge 1", 350m, false, true, false);
			AssertSubTotalLine(theLines[1], "CFS Charge 2", 400m, false, true, false);
			AssertSubTotalLine(theLines[2], "Charge Code 1 Description", 750m, false, false, true);
			AssertSubTotalLine(theLines[3], "", 0m, true, false, false);
			AssertSubTotalLine(theLines[4], "CFS Charge 3", 500m, false, true, false);
			AssertSubTotalLine(theLines[5], "Charge Code 2 Description", 500m, false, false, true);
			AssertSubTotalLine(theLines[6], "", 0m, true, false, false);
			AssertSubTotalLine(theLines[7], "Charge Without Tax", 300m, false, true, false);
			AssertEquals("OSTaxDisplay", "N/A", theLines[7].OSTaxDisplay);
			AssertSubTotalLine(theLines[8], "Charge Code 3 Description", 300m, false, false, true);
			AssertEquals("OSTaxDisplay", "", theLines[8].OSTaxDisplay);
			AssertSubTotalLine(theLines[9], "", 0m, true, false, false);
			AssertEquals("OSTaxDisplay", "", theLines[9].OSTaxDisplay);
			AssertSubTotalLine(theLines[10], "Charge Not Reportable Tax", 350m, false, true, false);
			AssertEquals("OSTaxDisplay", "Not Applicable", theLines[10].OSTaxDisplay);
			AssertSubTotalLine(theLines[11], "Charge Code 4 Description", 350m, false, false, true);
			AssertEquals("OSTaxDisplay", "", theLines[11].OSTaxDisplay);
			AssertSubTotalLine(theLines[12], "", 0m, true, false, false);
			AssertEquals("OSTaxDisplay", "", theLines[12].OSTaxDisplay);
			AssertSubTotalLine(theLines[13], "GL Account 1 first", 100m, false, true, false);
			AssertSubTotalLine(theLines[14], "GL Account 1 second", 200m, false, true, false);
			AssertSubTotalLine(theLines[15], "GL Account 1 Description", 300m, false, false, true);
			AssertSubTotalLine(theLines[16], "", 0m, true, false, false);
			AssertEquals("OSTaxDisplay", "", theLines[16].OSTaxDisplay);
			AssertSubTotalLine(theLines[17], "GL Account 2 first", 110m, false, true, false);
			AssertSubTotalLine(theLines[18], "GL Account 2 second", 230m, false, true, false);
			AssertSubTotalLine(theLines[19], "GL Account 2 Description", 340m, false, false, true);
			AssertSubTotalLine(theLines[20], "", 0m, true, false, false);
			AssertEquals("OSTaxDisplay", "", theLines[20].OSTaxDisplay);
		}

		public void TestSubTotalOfLinesByChargeCodeGroup()
		{
			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.All;
			Shipment.JS_IsCFSRegistered = ZBool.True;
			Shipment.JS_IsForwardRegistered = ZBool.False;
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = "S1234";
			JobHeader job = GetInvoiceJob(Shipment, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSLoadList;
			chargeCode1.AC_Desc = "Charge Code 1 Description";

			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line1.AL_Desc = "CFS Charge 1";
			line1.AL_OSExTaxAmount = 350.00M;
			line1.AL_OSTaxAmount = 30.00M;
			line1.AL_AC = chargeCode1.PK;
			line1.AL_AT = GetRate();

			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line2.AL_Desc = "CFS Charge 2";
			line2.AL_OSExTaxAmount = 400.00M;
			line2.AL_OSTaxAmount = 40.00M;
			line2.AL_AC = chargeCode1.PK;
			line2.AL_AT = GetRate();

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSShipment;
			chargeCode2.AC_Desc = "Charge Code 2 Description";

			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line3.AL_Desc = "CFS Charge 3";
			line3.AL_OSExTaxAmount = 500.00M;
			line3.AL_OSTaxAmount = 50.00M;
			line3.AL_AC = chargeCode2.PK;
			line3.AL_AT = GetRate();

			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode3.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode3.AC_Desc = "Charge Code 3 Description";

			ARInvoiceLine line4 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line4.AL_Desc = "Charge Without Tax";
			line4.AL_AC = chargeCode3.PK;
			line4.AL_OSExTaxAmount = 300.00M;

			var chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode4.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			chargeCode4.AC_Desc = "Charge Code 4 Description";

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>(TestBusinessObjectKind.MinimumRequiredToSave);
			taxRate.AT_Code = "XXXXX";
			taxRate.AT_Type = AccTaxRate.Types.NotReportable;
			taxRate.SetRateNumerator_ForTestOnly(0);

			ARInvoiceLine line5 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line5.AL_Desc = "Charge Not Reportable Tax";
			line5.AL_AC = chargeCode4.PK;
			line5.AL_OSExTaxAmount = 350.00M;

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpSubTotals();
			job.LocalChargesPK = Invoice.Header.PK;

			line4.AL_AT = ZGuid.Empty;
			line4.AL_OSTaxAmount = 0M;
			line5.AL_AT = taxRate.PK;
			line5.AL_OSTaxAmount = 0M;

			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			rollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CCG;
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.CFSShipment.Code;
			Factory.Save();
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("7 lines - 5 charges + 4 sub total + 4 spacer lines", 13, theLines.Count);

			AssertSubTotalLine(theLines[0], "Charge Without Tax", 300m, false, true, false);
			AssertEquals("OSTaxDisplay", "N/A", theLines[0].OSTaxDisplay);
			AssertSubTotalLine(theLines[1], DocRollUpConstants.RollupAndSubTotalDescriptions.Origin, 300m, false, false, true);
			AssertEquals("OSTaxDisplay", "", theLines[1].OSTaxDisplay);
			AssertSubTotalLine(theLines[2], "", 0m, true, false, false);
			AssertEquals("OSTaxDisplay", "", theLines[2].OSTaxDisplay);

			AssertSubTotalLine(theLines[3], "Charge Not Reportable Tax", 350m, false, true, false);
			AssertEquals("OSTaxDisplay", "Not Applicable", theLines[3].OSTaxDisplay);
			AssertSubTotalLine(theLines[4], DocRollUpConstants.RollupAndSubTotalDescriptions.Freight, 350m, false, false, true);
			AssertEquals("OSTaxDisplay", "", theLines[4].OSTaxDisplay);
			AssertSubTotalLine(theLines[5], "", 0m, true, false, false);
			AssertEquals("OSTaxDisplay", "", theLines[5].OSTaxDisplay);

			AssertSubTotalLine(theLines[6], "CFS Charge 1", 350m, false, true, false);
			AssertSubTotalLine(theLines[7], "CFS Charge 2", 400m, false, true, false);
			AssertSubTotalLine(theLines[8], ChargeCodeGroupList.Descriptions.CFSLoadList, 750m, false, false, true);
			AssertSubTotalLine(theLines[9], "", 0m, true, false, false);
			AssertSubTotalLine(theLines[10], "CFS Charge 3", 500m, false, true, false);
			AssertSubTotalLine(theLines[11], ChargeCodeGroupList.Descriptions.CFSShipment, 500m, false, false, true);
			AssertSubTotalLine(theLines[12], "", 0m, true, false, false);
		}

		public void TestSubTotalOfLinesWithGroupByChargeCodeConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000111";

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Desc = "Charge Code 1 Description";

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Desc = "Charge Code 2 Description";

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpSubTotals();
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			rollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CCD;
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;

			Invoice.AH_FullyPaidDate = ZDateTime.Empty;

			for (int i = 0; i < 2; i++)
			{
				Shipment = consol.Shipments.AddNew();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.All;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = Invoice.Header.PK;

				ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				line1.AL_Desc = "CFS Charge 1";
				line1.AL_OSExTaxAmount = 350.00M;
				line1.AL_OSTaxAmount = 30.00M;
				line1.AL_AC = chargeCode1.PK;
				line1.AL_AT = GetRate();
				line1.AL_JH = job.PK;
				TestObjectCreator.CreateJobCharge(line1, job, chargeCode1, TestObjectCreator.AUD);

				ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				line2.AL_Desc = "CFS Charge 2";
				line2.AL_OSExTaxAmount = 400.00M;
				line2.AL_OSTaxAmount = 40.00M;
				line2.AL_AC = chargeCode1.PK;
				line2.AL_AT = GetRate();
				line2.AL_JH = job.PK;
				TestObjectCreator.CreateJobCharge(line2, job, chargeCode1, TestObjectCreator.AUD);

				ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				line3.AL_Desc = "CFS Charge 3";
				line3.AL_OSExTaxAmount = 500.00M;
				line3.AL_OSTaxAmount = 50.00M;
				line3.AL_AC = chargeCode2.PK;
				line3.AL_AT = GetRate();
				line3.AL_JH = job.PK;
				TestObjectCreator.CreateJobCharge(line3, job, chargeCode2, TestObjectCreator.AUD);
			}
			Factory.Save();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("7*2 lines - 3*2 charges + 2*2 sub total + 2*2 spacer lines", 7 * 2, theLines.Count);

			for (int i = 0; i < 2; i++)
			{
				AssertSubTotalLine(theLines[i * 7 + 0], "CFS Charge 1", 350m, false, true, false, consol.Shipments[i]);
				AssertSubTotalLine(theLines[i * 7 + 1], "CFS Charge 2", 400m, false, true, false, consol.Shipments[i]);
				AssertSubTotalLine(theLines[i * 7 + 2], "Charge Code 1 Description", 750m, false, false, true, consol.Shipments[i]);
				AssertSubTotalLine(theLines[i * 7 + 3], "", 0m, true, false, false, consol.Shipments[i]);
				AssertSubTotalLine(theLines[i * 7 + 4], "CFS Charge 3", 500m, false, true, false, consol.Shipments[i]);
				AssertSubTotalLine(theLines[i * 7 + 5], "Charge Code 2 Description", 500m, false, false, true, consol.Shipments[i]);
				AssertSubTotalLine(theLines[i * 7 + 6], "", 0m, true, false, false, consol.Shipments[i]);
			}
		}

		void AssertSubTotalLine(IDocARInvoiceLine line, ZString description, ZDecimal oSExTaxAmount, bool isSpacerLine, bool hasBeenSubTotalled, bool isSubTotalLine, decimal? osTaxDisplay = null)
		{
			AssertEquals("LineDescription", description, line.LineDescription);
			AssertEquals("OSExTaxAmount", oSExTaxAmount, line.OSExTaxAmount);
			AssertEquals("IsSpacerLine", isSpacerLine, line.IsSpacerLine);
			AssertEquals("HasBeenSubTotalled", hasBeenSubTotalled, line.HasBeenSubTotalled);
			AssertEquals("IsSubTotalLine", isSubTotalLine, line.IsSubTotalLine);
			if (isSpacerLine || isSubTotalLine)
			{
				if (osTaxDisplay != null)
				{
					AssertEquals("OSTaxDisplay", new ZString(osTaxDisplay.ToString()), line.OSTaxDisplay);
				}
				else
				{
					Assert("OSTaxDisplay should be empty for Spacer and Subtotal", line.OSTaxDisplay.IsEmpty);
				}
				Assert("TaxAmountDisplay should be empty for Spacer and Subtotal", line.TaxAmountDisplay.IsEmpty);
			}
		}

		void AssertSubTotalLine(IDocARInvoiceLine line, ZString description, ZDecimal oSExTaxAmount, bool isSpacerLine, bool hasBeenSubTotalled, bool isSubTotalLine, CommonShipment shipment)
		{
			AssertSubTotalLine(line, description, oSExTaxAmount, isSpacerLine, hasBeenSubTotalled, isSubTotalLine);
			AssertEquals("Shipment", shipment.PK.ToString(), line.FKToShipment);
		}

		public void TestSubTotalOfLinesWithOFIGrouping()
		{
			SetUpInvoiceWrapperForRollUp();

			AddFreightChargeToInvoice();
			AddOriginChargeToInvoice();

			AddFreightChargeToInvoice();
			AddLoadChargeToInvoice();
			AddLoadChargeToInvoice();

			AccChargeCode cfsChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			cfsChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSShipment;
			ARInvoiceLine line = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line.AL_Desc = "CFS Charge";
			line.AL_OSExTaxAmount = 350.00M;
			line.AL_OSTaxAmount = 30.00M;
			line.AL_AC = cfsChargeCode.PK;
			line.AL_AT = GetRate();

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpSubTotals();
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.OFI;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			Factory.Save();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("10 lines - 6 charges + 2 sub total + 2 spacer lines", 10, theLines.Count);
			AssertEquals("4th line is the sub-total of origin charges with correct description", "Origin Charges", theLines[3].LineDescription);
			AssertEquals("4th line is the sub-total and has correct amount", 850m, theLines[3].OSExTaxAmount);
			AssertEquals("5th line is loading charges sub-total spacer line", "", theLines[4].LineDescription);
			AssertEquals("5th line has correct amount", 0m, theLines[4].OSExTaxAmount);
			AssertEquals("5th line is spacer line", true, theLines[4].IsSpacerLine);

			AssertEquals("8th line is the sub-total with correct description", "Freight Charges", theLines[7].LineDescription);
			AssertEquals("8th line is the sub-total and has correct amount", 500m, theLines[7].OSExTaxAmount);
			AssertEquals("9th line is loading charges sub-total spacer line", "", theLines[8].LineDescription);
			AssertEquals("9th line has correct amount", 0m, theLines[8].OSExTaxAmount);
			AssertEquals("9th line is spacer line", true, theLines[8].IsSpacerLine);

			Assert(theLines[0].HasBeenSubTotalled);
			Assert(theLines[1].HasBeenSubTotalled);
			Assert(theLines[2].HasBeenSubTotalled);
			Assert("This is a sub total line", !theLines[3].HasBeenSubTotalled);
			Assert("This is a sub total line", theLines[3].IsSubTotalLine);

			Assert(theLines[5].HasBeenSubTotalled);
			Assert(theLines[6].HasBeenSubTotalled);
			Assert("This is a sub total line", !theLines[7].HasBeenSubTotalled);
			Assert("This is a sub total line", theLines[7].IsSubTotalLine);

			Assert("This charge is left separate and not sub-totalled", !theLines[9].HasBeenSubTotalled);
			Assert("Not a sub-total line", !theLines[9].IsSubTotalLine);
		}

		public void TestSubTotalOfLinesWithOFIGroupingConsol()
		{
			OrgHeader debtor = SetUpDebtorForRollUp();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000111";

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AccChargeCode cfsChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			cfsChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSShipment;

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpSubTotals();
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.OFI;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;

			Factory.Save();
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;

			for (int i = 0; i < 2; i++)
			{
				Shipment = consol.Shipments.AddNew();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.All;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = debtor.PK;

				foreach (ARInvoiceLine line in new[] {
										AddFreightChargeToInvoice(),
										AddOriginChargeToInvoice(),
										AddFreightChargeToInvoice(),
										AddLoadChargeToInvoice(),
										AddLoadChargeToInvoice() }
				)
				{
					line.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, TestObjectCreator.AUD);
				}

				ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				line1.AL_Desc = "CFS Charge";
				line1.AL_OSExTaxAmount = 350.00M;
				line1.AL_OSTaxAmount = 30.00M;
				line1.AL_AC = cfsChargeCode.PK;
				line1.AL_AT = GetRate();
				line1.AL_JH = job.PK;
				TestObjectCreator.CreateJobCharge(line1, job, cfsChargeCode, TestObjectCreator.AUD);
			}

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("10*2 lines - 6*2 charges + 2*2 sub total + 2*2 spacer lines", 10 * 2, theLines.Count);

			for (int i = 0; i < 20; i += 10)
			{
				AssertEquals("4th line is the sub-total of origin charges with correct description", "Origin Charges", theLines[i + 3].LineDescription);
				AssertEquals("4th line is the sub-total and has correct amount", 850m, theLines[i + 3].OSExTaxAmount);
				AssertEquals("5th line is loading charges sub-total spacer line", "", theLines[i + 4].LineDescription);
				AssertEquals("5th line has correct amount", 0m, theLines[i + 4].OSExTaxAmount);
				AssertEquals("5th line is spacer line", true, theLines[i + 4].IsSpacerLine);

				AssertEquals("8th line is the sub-total with correct description", "Freight Charges", theLines[i + 7].LineDescription);
				AssertEquals("8th line is the sub-total and has correct amount", 500m, theLines[i + 7].OSExTaxAmount);
				AssertEquals("9th line is loading charges sub-total spacer line", "", theLines[i + 8].LineDescription);
				AssertEquals("9th line has correct amount", 0m, theLines[i + 8].OSExTaxAmount);
				AssertEquals("9th line is spacer line", true, theLines[i + 8].IsSpacerLine);

				Assert(theLines[i + 0].HasBeenSubTotalled);
				Assert(theLines[i + 1].HasBeenSubTotalled);
				Assert(theLines[i + 2].HasBeenSubTotalled);
				Assert("This is a sub total line", !theLines[i + 3].HasBeenSubTotalled);
				Assert("This is a sub total line", theLines[i + 3].IsSubTotalLine);

				Assert(theLines[i + 5].HasBeenSubTotalled);
				Assert(theLines[i + 6].HasBeenSubTotalled);
				Assert("This is a sub total line", !theLines[i + 7].HasBeenSubTotalled);
				Assert("This is a sub total line", theLines[i + 7].IsSubTotalLine);

				Assert("This charge is left separate and not sub-totalled", !theLines[i + 9].HasBeenSubTotalled);
				Assert("Not a sub-total line", !theLines[i + 9].IsSubTotalLine);

				for (int j = 0; j < 10; j++)
				{
					AssertEquals("Linked to shipment", consol.Shipments[i / 10].PK.ToString(), theLines[i + j].FKToShipment);
				}
			}
		}

		public void TestSubTotalOfLinesWithICRGrouping()
		{
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "TEST3";
			debtor.OH_IsDebtor = true;
			debtor.OH_IsConsignor = false;
			debtor.OH_IsConsignee = false;
			debtor.OH_IsBroker = false;
			debtor.OH_IsForwarder = false;

			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			ARInvoice invoiceHeader = Factory.New<ARInvoice>();
			invoiceHeader.AH_ConsolidatedInvoiceRef = "S1234";
			invoiceHeader.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			invoiceHeader.AH_OH = debtor.PK;
			JobHeader job = GetInvoiceJob(Shipment, invoiceHeader);
			job.LocalChargesPK = debtor.PK;
			DocARInvoice invoiceWrapper = DocARInvoice.New(invoiceHeader, Factory);
			Factory.Save();

			Enterprise.Accounting.Business.JobInvoicing.ExchangeRate rateUSD = Factory.New<Enterprise.Accounting.Business.JobInvoicing.ExchangeRate>();
			rateUSD.JF_RX_NKRateCurrency = "USD";
			rateUSD.JF_BaseRate = 2;
			rateUSD.JF_JH = job.PK;

			AddChargeWithSpecifiedCurrency(invoiceHeader, "Freight", "AUD");
			AddChargeWithSpecifiedCurrency(invoiceHeader, "Origin", "AUD");
			AddChargeWithSpecifiedCurrency(invoiceHeader, "Freight", "USD");

			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = invoiceHeader.Header.CompanyData.InvoiceRollupOrGroups[0];
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.SubTotal;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CLC;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;

			DocARInvoiceLineCollection lines = invoiceWrapper.LinesForInvoice;

			AssertEquals(7, lines.Count);

			AssertEquals("Freight", lines[0].LineDescription);
			AssertEquals("AUD", lines[0].ChargeCurrency);
			AssertEquals(250m, lines[0].ChargeOSAmount);
			AssertEquals("10.11%=25.28 *", lines[0].OSTaxDisplay);
			AssertEquals(250m, lines[0].OSExTaxAmount);

			AssertEquals("Origin", lines[1].LineDescription);
			AssertEquals("AUD", lines[1].ChargeCurrency);
			AssertEquals(250m, lines[1].OSExTaxAmount);
			AssertEquals("10.11%=25.28 **", lines[1].OSTaxDisplay);
			AssertEquals(250m, lines[1].OSExTaxAmount);

			AssertEquals("AUD", lines[2].LineDescription);
			AssertEquals("AUD", lines[2].ChargeCurrency);
			AssertEquals(500m, lines[2].OSExTaxAmount);
			AssertEquals("50.56", lines[2].OSTaxDisplay);
			AssertEquals(500m, lines[2].OSExTaxAmount);

			AssertEquals(ZString.Empty, lines[3].LineDescription);
			AssertEquals(ZString.Empty, lines[3].ChargeCurrency);
			AssertEquals(0m, lines[3].OSExTaxAmount);
			AssertEquals(ZString.Empty, lines[3].OSTaxDisplay);
			AssertEquals(0m, lines[3].OSExTaxAmount);

			AssertEquals("Freight", lines[4].LineDescription);
			AssertEquals("USD", lines[4].ChargeCurrency);
			AssertEquals(250m, lines[4].OSExTaxAmount);
			AssertEquals("10.11%=25.28 ***", lines[4].OSTaxDisplay);
			AssertEquals(250m, lines[4].OSExTaxAmount);

			AssertEquals("USD", lines[5].LineDescription);
			AssertEquals("USD", lines[5].ChargeCurrency);
			AssertEquals(250m, lines[5].OSExTaxAmount);
			AssertEquals("25.28", lines[5].OSTaxDisplay);
			AssertEquals(250m, lines[5].OSExTaxAmount);

			AssertEquals(ZString.Empty, lines[6].LineDescription);
			AssertEquals(ZString.Empty, lines[6].ChargeCurrency);
			AssertEquals(0m, lines[6].OSExTaxAmount);
			AssertEquals(ZString.Empty, lines[6].OSTaxDisplay);
			AssertEquals(0m, lines[6].OSExTaxAmount);
		}

		public void TestSubTotalOfLinesWithICRGroupingConsol()
		{
			OrgHeader debtor = SetUpDebtorForRollUp();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000111";

			ARInvoice invoiceHeader = Factory.New<ARInvoice>();
			invoiceHeader.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			invoiceHeader.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			invoiceHeader.AH_OH = debtor.PK;
			DocARInvoice invoiceWrapper = DocARInvoice.New(invoiceHeader, Factory);

			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = invoiceHeader.Header.CompanyData.InvoiceRollupOrGroups[0];
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.SubTotal;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CLC;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;

			ExchangeRateReader.GetReaderInstance().ClearCache();

			RefExchangeRate rate = Factory.New<RefExchangeRate>();
			rate.RE_StartDate = ZDateTime.Now.Date;
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(1);
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			rate.RE_SellRate = 1.0m;
			rate.RE_RX_NKExCurrency = "USD";

			Factory.Save();

			for (int i = 0; i < 2; i++)
			{
				Shipment = consol.Shipments.AddNew();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = debtor.PK;

				Enterprise.Accounting.Business.JobInvoicing.ExchangeRate rateUSD = Factory.New<Enterprise.Accounting.Business.JobInvoicing.ExchangeRate>();
				rateUSD.JF_RX_NKRateCurrency = "USD";
				rateUSD.JF_BaseRate = 2;
				rateUSD.JF_JH = job.PK;

				AddChargeWithSpecifiedCurrency(invoiceHeader, "Freight", "AUD").AL_JH = job.PK;
				AddChargeWithSpecifiedCurrency(invoiceHeader, "Origin", "AUD").AL_JH = job.PK;
				AddChargeWithSpecifiedCurrency(invoiceHeader, "Freight", "USD").AL_JH = job.PK;
			}

			DocARInvoiceLineCollection lines = invoiceWrapper.LinesForInvoice;

			AssertEquals(7 * 2, lines.Count);

			AssertEquals("Freight", lines[0].LineDescription);
			AssertEquals("AUD", lines[0].ChargeCurrency);
			AssertEquals(250m, lines[0].ChargeOSAmount);
			AssertEquals("10.11%=25.28 *", lines[0].OSTaxDisplay);
			AssertEquals(250m, lines[0].OSExTaxAmount);

			AssertEquals("Origin", lines[1].LineDescription);
			AssertEquals("AUD", lines[1].ChargeCurrency);
			AssertEquals(250m, lines[1].OSExTaxAmount);
			AssertEquals("10.11%=25.28 **", lines[1].OSTaxDisplay);
			AssertEquals(250m, lines[1].OSExTaxAmount);

			AssertEquals("AUD", lines[2].LineDescription);
			AssertEquals("AUD", lines[2].ChargeCurrency);
			AssertEquals(500m, lines[2].OSExTaxAmount);
			AssertEquals("50.56", lines[2].OSTaxDisplay);
			AssertEquals(500m, lines[2].OSExTaxAmount);

			AssertEquals(ZString.Empty, lines[3].LineDescription);
			AssertEquals(ZString.Empty, lines[3].ChargeCurrency);
			AssertEquals(0m, lines[3].OSExTaxAmount);
			AssertEquals(ZString.Empty, lines[3].OSTaxDisplay);
			AssertEquals(0m, lines[3].OSExTaxAmount);

			AssertEquals("Freight", lines[4].LineDescription);
			AssertEquals("USD", lines[4].ChargeCurrency);
			AssertEquals(250m, lines[4].OSExTaxAmount);
			AssertEquals("10.11%=25.28 ***", lines[4].OSTaxDisplay);
			AssertEquals(250m, lines[4].OSExTaxAmount);

			AssertEquals("USD", lines[5].LineDescription);
			AssertEquals("USD", lines[5].ChargeCurrency);
			AssertEquals(250m, lines[5].OSExTaxAmount);
			AssertEquals("25.28", lines[5].OSTaxDisplay);
			AssertEquals(250m, lines[5].OSExTaxAmount);

			AssertEquals(ZString.Empty, lines[6].LineDescription);
			AssertEquals(ZString.Empty, lines[6].ChargeCurrency);
			AssertEquals(0m, lines[6].OSExTaxAmount);
			AssertEquals(ZString.Empty, lines[6].OSTaxDisplay);
			AssertEquals(0m, lines[6].OSExTaxAmount);

			AssertEquals("Freight", lines[7].LineDescription);
			AssertEquals("AUD", lines[7].ChargeCurrency);
			AssertEquals(250m, lines[7].ChargeOSAmount);
			AssertEquals("10.11%=25.28 ****", lines[7].OSTaxDisplay);
			AssertEquals(250m, lines[7].OSExTaxAmount);

			AssertEquals("Origin", lines[8].LineDescription);
			AssertEquals("AUD", lines[8].ChargeCurrency);
			AssertEquals(250m, lines[8].OSExTaxAmount);
			AssertEquals("10.11%=25.28 *****", lines[8].OSTaxDisplay);
			AssertEquals(250m, lines[8].OSExTaxAmount);

			AssertEquals("AUD", lines[9].LineDescription);
			AssertEquals("AUD", lines[9].ChargeCurrency);
			AssertEquals(500m, lines[9].OSExTaxAmount);
			AssertEquals("50.56", lines[9].OSTaxDisplay);
			AssertEquals(500m, lines[9].OSExTaxAmount);

			AssertEquals(ZString.Empty, lines[10].LineDescription);
			AssertEquals(ZString.Empty, lines[10].ChargeCurrency);
			AssertEquals(0m, lines[10].OSExTaxAmount);
			AssertEquals(ZString.Empty, lines[10].OSTaxDisplay);
			AssertEquals(0m, lines[10].OSExTaxAmount);

			AssertEquals("Freight", lines[11].LineDescription);
			AssertEquals("USD", lines[11].ChargeCurrency);
			AssertEquals(250m, lines[11].OSExTaxAmount);
			AssertEquals("10.11%=25.28 ******", lines[11].OSTaxDisplay);
			AssertEquals(250m, lines[11].OSExTaxAmount);

			AssertEquals("USD", lines[12].LineDescription);
			AssertEquals("USD", lines[12].ChargeCurrency);
			AssertEquals(250m, lines[12].OSExTaxAmount);
			AssertEquals("25.28", lines[12].OSTaxDisplay);
			AssertEquals(250m, lines[12].OSExTaxAmount);

			AssertEquals(ZString.Empty, lines[13].LineDescription);
			AssertEquals(ZString.Empty, lines[13].ChargeCurrency);
			AssertEquals(0m, lines[13].OSExTaxAmount);
			AssertEquals(ZString.Empty, lines[13].OSTaxDisplay);
			AssertEquals(0m, lines[13].OSExTaxAmount);

			for (int j = 0; j < 7; j++)
			{
				AssertEquals("Linked to shipment", consol.Shipments[0].PK.ToString(), lines[j].FKToShipment);
				AssertEquals("Linked to shipment", consol.Shipments[1].PK.ToString(), lines[j + 7].FKToShipment);
			}
		}

		public void TestSubTotalOfLinesWithNoGrouping_MiscInvoice()
		{
			Invoice = Factory.New<ARInvoice>();
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AddFreightChargeToInvoice();
			AddOriginChargeToInvoice();

			AddFreightChargeToInvoice();
			AddLoadChargeToInvoice();
			AddLoadChargeToInvoice();

			AccChargeCode cfsChargeCode = Factory.New<AccChargeCode>();
			cfsChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSShipment;
			cfsChargeCode.AC_AG_RevenueAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;
			ARInvoiceLine line = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line.AL_Desc = "CFS Charge";
			line.AL_OSExTaxAmount = 350.00M;
			line.AL_OSTaxAmount = 30.00M;
			line.AL_AC = cfsChargeCode.PK;
			line.AL_AT = GetRate();

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpSubTotals();
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code;
			rollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.None;
			Factory.Save();
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("The lines must not group.", 6, theLines.Count);

			AssertSubTotalLine(theLines[0], "CFS Charge", 350m, false, false, false);
			AssertSubTotalLine(theLines[1], "Freight", 250m, false, false, false);
			AssertSubTotalLine(theLines[2], "Freight", 250m, false, false, false);
			AssertSubTotalLine(theLines[3], "Loading", 350m, false, false, false);
			AssertSubTotalLine(theLines[4], "Origin", 150m, false, false, false);
			AssertSubTotalLine(theLines[5], "Loading", 350m, false, false, false);
		}

		public void TestSubTotalOfLinesWithGroupByChargeCode_MiscInvoice()
		{
			Invoice = Factory.New<ARInvoice>();
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Desc = "Charge Code 1 Description";

			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line1.AL_Desc = "CFS Charge 1";
			line1.AL_OSExTaxAmount = 350.00M;
			line1.AL_OSTaxAmount = 30.00M;
			line1.AL_AC = chargeCode1.PK;
			line1.AL_AT = GetRate();

			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line2.AL_Desc = "CFS Charge 2";
			line2.AL_OSExTaxAmount = 400.00M;
			line2.AL_OSTaxAmount = 40.00M;
			line2.AL_AC = chargeCode1.PK;
			line2.AL_AT = GetRate();

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Desc = "Charge Code 2 Description";

			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line3.AL_Desc = "CFS Charge 3";
			line3.AL_OSExTaxAmount = 500.00M;
			line3.AL_OSTaxAmount = 50.00M;
			line3.AL_AC = chargeCode2.PK;
			line3.AL_AT = GetRate();

			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode3.AC_Desc = "Charge Code 3 Description";

			ARInvoiceLine line4 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line4.AL_Desc = "Charge Without Tax";
			line4.AL_AC = chargeCode3.PK;
			line4.AL_OSExTaxAmount = 300.00M;

			var chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode4.AC_Desc = "Charge Code 4 Description";

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>(TestBusinessObjectKind.MinimumRequiredToSave);
			taxRate.AT_Code = "XXXX";
			taxRate.AT_Type = AccTaxRate.Types.NotReportable;
			taxRate.SetRateNumerator_ForTestOnly(0);

			ARInvoiceLine line5 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line5.AL_Desc = "Charge Not Reportable Tax";
			line5.AL_AC = chargeCode4.PK;
			line5.AL_OSExTaxAmount = 350.00M;

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpSubTotals();

			line4.AL_AT = ZGuid.Empty;
			line4.AL_OSTaxAmount = 0M;
			line5.AL_AT = taxRate.PK;
			line5.AL_OSTaxAmount = 0M;

			AccGLHeader gLAccount1 = Factory.NewWithValidTestData<AccGLHeader>();
			gLAccount1.AG_Description = "GL Account 1 Description";

			ARInvoiceLine line6 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line6.AL_Desc = "GL Account 1 first";
			line6.AL_AG = gLAccount1.PK;
			line6.AL_OSExTaxAmount = 100.00M;

			ARInvoiceLine line7 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line7.AL_Desc = "GL Account 1 second";
			line7.AL_AG = gLAccount1.PK;
			line7.AL_OSExTaxAmount = 200.00M;

			AccGLHeader gLAccount2 = Factory.NewWithValidTestData<AccGLHeader>();
			gLAccount2.AG_Description = "GL Account 2 Description";

			ARInvoiceLine line8 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line8.AL_Desc = "GL Account 2 first";
			line8.AL_AG = gLAccount2.PK;
			line8.AL_OSExTaxAmount = 110.00M;

			ARInvoiceLine line9 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line9.AL_Desc = "GL Account 2 second";
			line9.AL_AG = gLAccount2.PK;
			line9.AL_OSExTaxAmount = 230.00M;

			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code;
			rollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CCD;
			Factory.Save();
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("17 lines = 5 charges + 4 accounts + 6 sub total + 6 spacer lines", 21, theLines.Count);

			AssertSubTotalLine(theLines[0], "CFS Charge 1", 350m, false, true, false);
			AssertSubTotalLine(theLines[1], "CFS Charge 2", 400m, false, true, false);
			AssertSubTotalLine(theLines[2], "Charge Code 1 Description", 750m, false, false, true);
			AssertSubTotalLine(theLines[3], "", 0m, true, false, false);
			AssertSubTotalLine(theLines[4], "CFS Charge 3", 500m, false, true, false);
			AssertSubTotalLine(theLines[5], "Charge Code 2 Description", 500m, false, false, true);
			AssertSubTotalLine(theLines[6], "", 0m, true, false, false);
			AssertSubTotalLine(theLines[7], "Charge Without Tax", 300m, false, true, false);
			AssertEquals("OSTaxDisplay", "N/A", theLines[7].OSTaxDisplay);
			AssertSubTotalLine(theLines[8], "Charge Code 3 Description", 300m, false, false, true);
			AssertEquals("OSTaxDisplay", "", theLines[8].OSTaxDisplay);
			AssertSubTotalLine(theLines[9], "", 0m, true, false, false);
			AssertEquals("OSTaxDisplay", "", theLines[9].OSTaxDisplay);
			AssertSubTotalLine(theLines[10], "Charge Not Reportable Tax", 350m, false, true, false);
			AssertEquals("OSTaxDisplay", "Not Applicable", theLines[10].OSTaxDisplay);
			AssertSubTotalLine(theLines[11], "Charge Code 4 Description", 350m, false, false, true);
			AssertEquals("OSTaxDisplay", "", theLines[11].OSTaxDisplay);
			AssertSubTotalLine(theLines[12], "", 0m, true, false, false);
			AssertEquals("OSTaxDisplay", "", theLines[12].OSTaxDisplay);
			AssertSubTotalLine(theLines[13], "GL Account 1 first", 100m, false, true, false);
			AssertSubTotalLine(theLines[14], "GL Account 1 second", 200m, false, true, false);
			AssertSubTotalLine(theLines[15], "GL Account 1 Description", 300m, false, false, true);
			AssertSubTotalLine(theLines[16], "", 0m, true, false, false);
			AssertEquals("OSTaxDisplay", "", theLines[16].OSTaxDisplay);
			AssertSubTotalLine(theLines[17], "GL Account 2 first", 110m, false, true, false);
			AssertSubTotalLine(theLines[18], "GL Account 2 second", 230m, false, true, false);
			AssertSubTotalLine(theLines[19], "GL Account 2 Description", 340m, false, false, true);
			AssertSubTotalLine(theLines[20], "", 0m, true, false, false);
			AssertEquals("OSTaxDisplay", "", theLines[20].OSTaxDisplay);
		}

		public void TestSubTotalOfLinesByChargeCodeGroup_MiscInvoice()
		{
			Invoice = Factory.New<ARInvoice>();
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSLoadList;
			chargeCode1.AC_Desc = "Charge Code 1 Description";

			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line1.AL_Desc = "CFS Charge 1";
			line1.AL_OSExTaxAmount = 350.00M;
			line1.AL_OSTaxAmount = 30.00M;
			line1.AL_AC = chargeCode1.PK;
			line1.AL_AT = GetRate();

			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line2.AL_Desc = "CFS Charge 2";
			line2.AL_OSExTaxAmount = 400.00M;
			line2.AL_OSTaxAmount = 40.00M;
			line2.AL_AC = chargeCode1.PK;
			line2.AL_AT = GetRate();

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSShipment;
			chargeCode2.AC_Desc = "Charge Code 2 Description";

			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line3.AL_Desc = "CFS Charge 3";
			line3.AL_OSExTaxAmount = 500.00M;
			line3.AL_OSTaxAmount = 50.00M;
			line3.AL_AC = chargeCode2.PK;
			line3.AL_AT = GetRate();

			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode3.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode3.AC_Desc = "Charge Code 3 Description";

			ARInvoiceLine line4 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line4.AL_Desc = "Charge Without Tax";
			line4.AL_AC = chargeCode3.PK;
			line4.AL_OSExTaxAmount = 300.00M;

			var chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode4.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			chargeCode4.AC_Desc = "Charge Code 4 Description";

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>(TestBusinessObjectKind.MinimumRequiredToSave);
			taxRate.AT_Code = "XXXXX";
			taxRate.AT_Type = AccTaxRate.Types.NotReportable;
			taxRate.SetRateNumerator_ForTestOnly(0);

			ARInvoiceLine line5 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line5.AL_Desc = "Charge Not Reportable Tax";
			line5.AL_AC = chargeCode4.PK;
			line5.AL_OSExTaxAmount = 350.00M;

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpSubTotals();

			line4.AL_AT = ZGuid.Empty;
			line4.AL_OSTaxAmount = 0M;
			line5.AL_AT = taxRate.PK;
			line5.AL_OSTaxAmount = 0M;

			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code;
			rollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CCG;
			Factory.Save();
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("7 lines - 5 charges + 4 sub total + 4 spacer lines", 13, theLines.Count);

			AssertSubTotalLine(theLines[0], "Charge Without Tax", 300m, false, true, false);
			AssertEquals("OSTaxDisplay", "N/A", theLines[0].OSTaxDisplay);
			AssertSubTotalLine(theLines[1], DocRollUpConstants.RollupAndSubTotalDescriptions.Origin, 300m, false, false, true);
			AssertEquals("OSTaxDisplay", "", theLines[1].OSTaxDisplay);
			AssertSubTotalLine(theLines[2], "", 0m, true, false, false);
			AssertEquals("OSTaxDisplay", "", theLines[2].OSTaxDisplay);

			AssertSubTotalLine(theLines[3], "Charge Not Reportable Tax", 350m, false, true, false);
			AssertEquals("OSTaxDisplay", "Not Applicable", theLines[3].OSTaxDisplay);
			AssertSubTotalLine(theLines[4], DocRollUpConstants.RollupAndSubTotalDescriptions.Freight, 350m, false, false, true);
			AssertEquals("OSTaxDisplay", "", theLines[4].OSTaxDisplay);
			AssertSubTotalLine(theLines[5], "", 0m, true, false, false);
			AssertEquals("OSTaxDisplay", "", theLines[5].OSTaxDisplay);

			AssertSubTotalLine(theLines[6], "CFS Charge 1", 350m, false, true, false);
			AssertSubTotalLine(theLines[7], "CFS Charge 2", 400m, false, true, false);
			AssertSubTotalLine(theLines[8], ChargeCodeGroupList.Descriptions.CFSLoadList, 750m, false, false, true);
			AssertSubTotalLine(theLines[9], "", 0m, true, false, false);
			AssertSubTotalLine(theLines[10], "CFS Charge 3", 500m, false, true, false);
			AssertSubTotalLine(theLines[11], ChargeCodeGroupList.Descriptions.CFSShipment, 500m, false, false, true);
			AssertSubTotalLine(theLines[12], "", 0m, true, false, false);
		}

		#endregion

		#region RollUp Tests

		(ARInvoiceLine Line1, ARInvoiceLine Line2, JobCharge Charge1, JobCharge Charge2) CreateRollUpTestData(ZDecimal exchangeRate)
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = "ZUB";

			RefCurrency currency2 = Factory.NewWithValidTestData<RefCurrency>();
			currency2.RX_Code = "RAK";

			Factory.Save();

			SetUpInvoiceWrapperForRollUp();
			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpAll();

			Invoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			Invoice.AH_ExchangeRate = exchangeRate;

			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line1.AL_Desc = "Origin Charge 1";
			line1.AL_OSExTaxAmount = 104.00M;
			line1.AL_AC = chargeCode1.PK;
			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_RX_NKSellCurrency = currency.RX_Code;
			charge1.JR_OSSellAmt = 104M;
			charge1.JR_OSSellExRate = 1.3M;
			charge1.JR_AL_ARLine = line1.PK;
			charge1.SetAmountsFromLinkedLinesForTests();

			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line2.AL_Desc = "Origin Charge 2";
			line2.AL_OSExTaxAmount = 150.00M;
			line2.AL_AC = chargeCode2.PK;
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_RX_NKSellCurrency = currency.RX_Code;
			charge2.JR_OSSellAmt = 150M;
			charge2.JR_OSSellExRate = 1.3M;
			charge2.JR_AL_ARLine = line2.PK;
			charge2.SetAmountsFromLinkedLinesForTests();

			AccChargeCode chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode3.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line3.AL_Desc = "Customs Charge 1";
			line3.AL_OSExTaxAmount = 200.00M;
			line3.AL_AC = chargeCode3.PK;
			JobCharge charge3 = Factory.NewWithValidTestData<JobCharge>();
			charge3.JR_RX_NKSellCurrency = currency2.RX_Code;
			charge3.JR_OSSellAmt = 200M;
			charge3.JR_OSSellExRate = 1M;
			charge3.JR_AL_ARLine = line3.PK;
			charge3.SetAmountsFromLinkedLinesForTests();

			Factory.Save();

			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			var client = orgFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Invoice.AH_OH));
			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			group.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.All;
			orgFactory.Save();

			return (line1, line2, charge1, charge2);
		}

		[SuspendCriticalValidation]
		public void TestRollUpSingleCurrencyIncludesForeignAmountWithExRate_NoForexDetails()
		{
			CreateRollUpTestData(exchangeRate: 1.3m);

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			var theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("Expect 2 lines", 2, theLines.Count);
			AssertEquals("Description of not rolled up line", "Customs Charge 1", theLines[0].LineDescription);
			AssertEquals("Description of rolled up line", "All charges except Customs Duty and Tax", theLines[1].LineDescription);
			AssertEquals("Description - No Forex details", "All charges except Customs Duty and Tax ", theLines[1].LineDescriptionAndExchangeRate);
			AssertEquals("OSExTaxAmount", 254.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0.00M, theLines[1].OSTaxAmount);
			AssertEquals("GSTVAT", 0.00M, theLines[1].GSTVAT);
			AssertEquals("LineAmount", Env.CurrentCompany.ExchangeRate.ForeignToLocal(254.00m, 1.3m), theLines[1].LineAmount);
		}

		[SuspendCriticalValidation]
		public void TestRollUpSingleCurrencyIncludesForeignAmountWithExRate_ForeignAmountAndExchangeRate()
		{
			var testData = CreateRollUpTestData(exchangeRate: 1m);

			Invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			testData.Charge1.JR_OSSellExRate = 1.3M;
			testData.Charge1.JR_LocalSellAmt = testData.Line1.AL_LineAmount;

			testData.Charge2.JR_OSSellExRate = 1.3M;
			testData.Charge2.JR_LocalSellAmt = testData.Line2.AL_LineAmount;

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			var theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("Expect 2 lines", 2, theLines.Count);
			AssertEquals("Description of not rolled up line", "Customs Charge 1", theLines[0].LineDescription);
			AssertEquals("Description of rolled up line", "All charges except Customs Duty and Tax", theLines[1].LineDescription);
			AssertEquals("Description with Foreign Amount + Exchange Rate", "All charges except Customs Duty and Tax ZUB 330.20 @ 1.300000", theLines[1].LineDescriptionAndExchangeRate);
			AssertEquals("OSExTaxAmount", 254.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0.00M, theLines[1].OSTaxAmount);
			AssertEquals("GSTVAT", 0.00M, theLines[1].GSTVAT);
			AssertEquals("LineAmount", 254.0M, theLines[1].LineAmount);
		}

		[SuspendCriticalValidation]
		public void TestRollUpSingleCurrencyIncludesForeignAmountWithExRate_AllLinesSameCurrency()
		{
			CreateRollUpTestData(exchangeRate: 1m);

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			var theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("Expect 2 lines", 2, theLines.Count);
			AssertEquals("Description of not rolled up line", "Customs Charge 1", theLines[0].LineDescription);
			AssertEquals("Description of rolled up line", "All charges except Customs Duty and Tax", theLines[1].LineDescription);
			AssertEquals("Description with no Forex, as not all lines are same currency", "All charges except Customs Duty and Tax ", theLines[1].LineDescriptionAndExchangeRate);
			AssertEquals("OSExTaxAmount", 254.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0.00M, theLines[1].OSTaxAmount);
			AssertEquals("GSTVAT", 0.00M, theLines[1].GSTVAT);
			AssertEquals("LineAmount", 254.0M, theLines[1].LineAmount);
		}

		#region RollUp ALL

		public void TestRollUpLinesForALLWhenPreventInvoicePrintGroupingIsFalse()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			SetUpDepartmentAndDirection();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Description", "Origin", theLines[0].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[0].OSTaxDisplay);
			AssertEquals("IsRollUpLine", false, theLines[0].IsRollUpLine);

			SetUpOrganisationForRollUpAll();
			Factory.Save();
			theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Description", "All charges except Customs Duty and Tax", theLines[0].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[0].OSTaxDisplay);
			AssertEquals("IsRollUpLine", true, theLines[0].IsRollUpLine);
		}

		public void TestRollUpLinesForALLWhenPreventInvoicePrintGroupingIsTrue()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpAll();
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Description", "Origin", theLines[0].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[0].OSTaxDisplay);
			AssertEquals("IsRollUpLine", false, theLines[0].IsRollUpLine);
		}

		public void TestRollUpLinesForALLForOneLineAndOneCDS()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddCustomsChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpAll();
			Factory.Save();
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "All charges except Customs Duty and Tax", theLines[0].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Customs Charge", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[1].OSTaxDisplay);

			InvoicingBase.Lines.RemoveAndDeleteAll();
			line1 = AddOriginChargeToInvoice();
			line2 = AddCustomsChargeToInvoice();
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17", theLines[1].OSTaxDisplay);

			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22", theLines[0].OSTaxDisplay);
		}

		public void TestRollUpLinesForALLForOneLineAndOneNGC()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			AddOriginChargeToInvoice();
			AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddNotGroupedChargeToInvoice();
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 4, theLines.Count);

			int lineNumber = 0;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 ****", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Origin", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Origin", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 **", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Origin", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 ***", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);

			SetUpDepartmentAndDirection();
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 4, theLines.Count);

			lineNumber = 0;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 ****", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Origin", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Origin", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 **", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Origin", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 ***", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);

			SetUpOrganisationForRollUpAll(false);
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			lineNumber = 0;

			AssertEquals("Description", "All charges except Customs Duty and Tax", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 450.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "45.51 *,**,***", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 ****", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);

			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 3, theLines.Count);

			lineNumber = 0;

			AssertEquals("Description", "All charges except Customs Duty and Tax", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "30.34 **,***", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 ****", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Origin", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollUpLinesForALLForOneLineAndOneNJR()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddNotJobChargeToInvoice();
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpAll(false);
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 1, theLines.Count);

			//First Line
			AssertEquals("Description", "All charges except Customs Duty and Tax", theLines[0].LineDescription);
			AssertEquals("Amount", 400.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "40.45 *,**", theLines[0].OSTaxDisplay);

			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "All charges except Customs Duty and Tax", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLineForALLForAllCharges()
		{
			SetUpInvoiceWrapperForRollUp();
			AddAllChargesToInvoice();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 17, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpAll();
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 4, theLines.Count);

			int lineNumber = 0;

			AssertEquals("Description", "All charges except Customs Duty and Tax", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 2900.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=293.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Customs Charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollUpLineForALLForAllChargesConsol()
		{
			SetUpConsolInvoiceWrapperForRollUp();
			OrgHeader debtor = Invoice.Header;

			for (int i = 0; i < 2; i++)
			{
				Shipment = Consol.Shipments.AddNew();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = debtor.PK;
				AddAllChargesToInvoice(job);
			}

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 17 * 2, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpAll();
			Factory.Save();
			Invoice.Header.CompanyData.InvoiceRollupOrGroups[0].PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("Count of lines", 4 * 2, theLines.Count);

			for (int i = 0; i < 2; i++)
			{
				DocARInvoiceLineCollection lines = new DocARInvoiceLineCollection(Factory);
				for (int j = 0; j < theLines.Count / 2; j++)
				{
					lines.Add((BusinessObject)theLines[i * theLines.Count / 2 + j]);
				}

				lines.Sort("LineDescription", ListSortDirection.Ascending);

				int lineNumber = 0;

				AssertEquals("Description", "All charges except Customs Duty and Tax", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 2900.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=293.22 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 0M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Customs Charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Not grouped charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("All lines must be tested.", lines.Count, lineNumber);

				for (int j = 0; j < lines.Count; j++)
				{
					AssertEquals("Linked to shipment", Consol.Shipments[i].PK.ToString(), lines[j].FKToShipment);
				}
			}
		}

		#endregion

		#region RollUp AEC

		public void TestRollUpLinesForAECForOneLine()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpOrganisationForRollUpAEC();
			SetUpShipmentDirection();
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin, Freight, Insurance and Destination Charges", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);
		}

		public void TestRollUpLinesForAECForOneLine_DepartmentAndDirection()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			SetUpDepartmentAndDirection();
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);
		}

		public void TestRollUpLinesForAECForOneLineAndOneCDS()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddCustomsChargeToInvoice();
			DocARInvoiceCommon aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpAEC(false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin, Freight, Insurance and Destination Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddCustomsChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);
		}

		public void TestRollUpLinesForAECForOneLineAndOneNGC()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddNotGroupedChargeToInvoice();
			DocARInvoiceCommon aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpAEC(false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin, Freight, Insurance and Destination Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotGroupedChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForAECForOneLineAndOneNJR()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddNotJobChargeToInvoice();
			DocARInvoiceCommon aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpAEC();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin, Freight, Insurance and Destination Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotJobChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLineForAECForAllCharges()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddNotJobChargeToInvoice();
			InvoicingLineBase line11 = AddCommentChargeToInvoice();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpAEC();
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));

			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 6, theLines.Count);

			int lineNumber = 0;
			AssertEquals("Description", "Brokerage", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 30.33M, theLines[lineNumber].GSTVAT);
			AssertEquals("GST", 300.0M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 0M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 0M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Customs Charge", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 20.22M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 200.0M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Non job related charge", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 25.28M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 250.0M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 25.28M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 250.0M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Origin, Freight, Insurance and Destination Charges", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 1800.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=182.00 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 182.00M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 1800.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollUpLineForAECForAllChargesConsol()
		{
			SetUpConsolInvoiceWrapperForRollUp();
			OrgHeader debtor = Invoice.Header;

			for (int i = 0; i < 2; i++)
			{
				Shipment = Consol.Shipments.AddNew();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = debtor.PK;

				foreach (InvoicingLineBase line in new[] {
										AddOriginChargeToInvoice(),
										AddDestinationChargeToInvoice(),
										AddFreightChargeToInvoice(),
										AddBrokerageChargeToInvoice(),
										AddLoadChargeToInvoice(),
										AddUnLoadChargeToInvoice(),
										AddInsuranceChargeToInvoice(),
										AddCustomsChargeToInvoice(),
										AddNotGroupedChargeToInvoice(),
										AddCommentChargeToInvoice(),
										AddNotJobChargeToInvoice() }
				)
				{
					line.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, TestObjectCreator.AUD);
				}
			}

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11 * 2, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpAEC();
			Invoice.Header.CompanyData.InvoiceRollupOrGroups[0].PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			Factory.Save();

			theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("Count of lines", 6 * 2, theLines.Count);

			for (int i = 0; i < 2; i++)
			{
				DocARInvoiceLineCollection lines = new DocARInvoiceLineCollection(Factory);
				for (int j = 0; j < theLines.Count / 2; j++)
				{
					lines.Add((BusinessObject)theLines[i * theLines.Count / 2 + j]);
				}

				lines.Sort("LineDescription", ListSortDirection.Ascending);

				int lineNumber = 0;

				AssertEquals("Description", "Brokerage", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 300.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=30.33 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 30.33M, lines[lineNumber].GSTVAT);
				AssertEquals("GST", 300.0M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 0M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 0M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 0M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Customs Charge", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 20.22M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 200.0M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Non job related charge", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 25.28M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 250.0M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Not grouped charge", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 25.28M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 250.0M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Origin, Freight, Insurance and Destination Charges", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 1800.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=182.00 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 182.00M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 1800.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("All lines must be tested.", lines.Count, lineNumber);

				for (int j = 0; j < lines.Count; j++)
				{
					AssertEquals("Linked to shipment", Consol.Shipments[i].PK.ToString(), lines[j].FKToShipment);
				}
			}
		}

		#endregion

		#region RollUp O&F

		public void TestRollUpLinesForOAndFForOneLine()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("O&F");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin, Freight and Insurance Charges", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);
		}

		public void TestRollUpLinesForOAndFForOneLineAndOneCDS()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddCustomsChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("O&F", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin, Freight and Insurance Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddCustomsChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForOAndFForOneLineAndOneNGC()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddNotGroupedChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("O&F", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin, Freight and Insurance Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotGroupedChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForOAndFForOneLineAndOneNJR()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddNotJobChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("O&F", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin, Freight and Insurance Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotJobChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLineForOAndFForAllCharges()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddNotJobChargeToInvoice();
			InvoicingLineBase line11 = AddCommentChargeToInvoice();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("O&F");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 8, theLines.Count);

			int lineNumber = 0;

			AssertEquals("Description", "Brokerage", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Customs Charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Destination", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Non job related charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Origin, Freight and Insurance Charges", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 1200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=121.34 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Unloading", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 400.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=40.44 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollUpLineForLineWithEmptyChargeCode()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddCommentChargeToInvoice();

			ARInvoiceLine line11 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line11.AL_Desc = "EmptyChargeCode";
			line11.AL_OSExTaxAmount = 450.00M;
			line11.AL_OSTaxAmount = 40.00M;
			line11.AL_AC = ZGuid.Empty;
			line11.AL_AT = GetRate();
			line11.AL_AG = TestObjectCreator.GLHeader1.PK;

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("O&F");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 8, theLines.Count);

			int lineNumber = 0;

			AssertEquals("Description", "Brokerage", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Customs Charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Destination", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "EmptyChargeCode", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 450.00m, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=45.50 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Origin, Freight and Insurance Charges", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 1200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=121.34 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Unloading", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 400.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=40.44 *", theLines[lineNumber].OSTaxDisplay);
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
			{
				AssertEquals("Formatting for Iceland", "10,11%=40,44 *", theLines[lineNumber].OSTaxDisplay);
			}
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollUpLineForLineWithEmptyChargeCodeAndCCD()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddCommentChargeToInvoice();

			ARInvoiceLine line11 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line11.AL_Desc = "EmptyChargeCode";
			line11.AL_OSExTaxAmount = 450.00M;
			line11.AL_OSTaxAmount = 40.00M;
			line11.AL_AC = ZGuid.Empty;
			line11.AL_AT = GetRate();
			line11.AL_AG = TestObjectCreator.GLHeader1.PK;

			AccGLHeader gLAccount1 = Factory.NewWithValidTestData<AccGLHeader>();
			gLAccount1.AG_Description = "GL Account 1 Description";

			ARInvoiceLine line12 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line12.AL_Desc = "GL Account 1 first";
			line12.AL_AG = gLAccount1.PK;
			line12.AL_OSExTaxAmount = 100.00M;

			ARInvoiceLine line13 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line13.AL_Desc = "GL Account 1 second";
			line13.AL_AG = gLAccount1.PK;
			line13.AL_OSExTaxAmount = 200.00M;

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 13, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode(OrgConstants.InvoiceLineGroupings.Code.CCD);
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 12, theLines.Count);

			int lineNumber = 0;

			AssertEquals("Description", "Brokerage", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Customs Charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Destination", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "EmptyChargeCode", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 450.00m, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=45.50 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Freight", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "GL Account 1 Description", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Insurance", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 450.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=45.50 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Loading", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 350.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=35.39 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Origin", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Unloading", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 400.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=40.44 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollUpLineForOAndFForAllChargesConsol()
		{
			SetUpConsolInvoiceWrapperForRollUp();
			OrgHeader debtor = Invoice.Header;

			for (int i = 0; i < 2; i++)
			{
				Shipment = Consol.Shipments.AddNew();
				Shipment.JS_UniqueConsignRef = "S000" + i;
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = debtor.PK;

				foreach (InvoicingLineBase line in new[] {
										AddOriginChargeToInvoice(),
										AddDestinationChargeToInvoice(),
										AddFreightChargeToInvoice(),
										AddBrokerageChargeToInvoice(),
										AddLoadChargeToInvoice(),
										AddUnLoadChargeToInvoice(),
										AddInsuranceChargeToInvoice(),
										AddCustomsChargeToInvoice(),
										AddNotGroupedChargeToInvoice(),
										AddCommentChargeToInvoice(),
										AddNotJobChargeToInvoice() }
				)
				{
					line.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, TestObjectCreator.AUD);
				}
			}

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11 * 2, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("O&F");
			Invoice.Header.CompanyData.InvoiceRollupOrGroups[0].PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			Factory.Save();
			theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("Count of lines", 8 * 2, theLines.Count);

			for (int i = 0; i < 2; i++)
			{
				DocARInvoiceLineCollection lines = new DocARInvoiceLineCollection(Factory);
				for (int j = 0; j < theLines.Count / 2; j++)
				{
					lines.Add((BusinessObject)theLines[i * theLines.Count / 2 + j]);
				}

				lines.Sort("LineDescription", ListSortDirection.Ascending);

				int lineNumber = 0;

				AssertEquals("Description", "Brokerage", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 300.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=30.33 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 0M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Customs Charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Destination", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Non job related charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Not grouped charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Origin, Freight and Insurance Charges", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 1200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=121.34 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Unloading", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 400.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=40.44 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("All lines must be tested.", lines.Count, lineNumber);

				for (int j = 0; j < lines.Count; j++)
				{
					AssertEquals("Linked to shipment", Consol.Shipments[i].PK.ToString(), lines[j].FKToShipment);
				}
			}
		}

		#endregion

		#region RollUp FRT

		public void TestRollUpLinesForFRTForOneLine()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddFreightChargeToInvoice();
			AssertEquals("Count of lines", 1, ARInvoiceWrapper.LinesForInvoice.Count);
			AssertEquals("Description", "Freight", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 250.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			AssertEquals("Description", "Freight", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 250.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("FRT");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Freight Charges", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 250.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Freight", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 250.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);
		}

		public void TestRollUpLinesForFRTForMoreLines()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddFreightChargeToInvoice();
			InvoicingLineBase line2 = AddFreightChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddOriginChargeToInvoice();
			InvoicingLineBase line5 = AddDestinationChargeToInvoice();

			AssertEquals("precondition: count of lines", 5, ARInvoiceWrapper.LinesForInvoice.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("FRT");

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Count of lines", 3, ARInvoiceWrapper.LinesForInvoice.Count);

			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			AssertEquals("Description", "Destination", ARInvoiceWrapper.LinesForInvoice[1].LineDescription);
			AssertEquals("Amount", 200.00M, ARInvoiceWrapper.LinesForInvoice[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", ARInvoiceWrapper.LinesForInvoice[1].OSTaxDisplay);

			AssertEquals("Description", "Freight Charges", ARInvoiceWrapper.LinesForInvoice[2].LineDescription);
			AssertEquals("Amount", 750.00M, ARInvoiceWrapper.LinesForInvoice[2].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=75.84 *", ARInvoiceWrapper.LinesForInvoice[2].OSTaxDisplay);
		}

		public void TestRollUpLinesForFRTForMoreLinesPrevent()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddFreightChargeToInvoice();
			InvoicingLineBase line2 = AddFreightChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddOriginChargeToInvoice();
			InvoicingLineBase line5 = AddDestinationChargeToInvoice();

			line2.AL_PreventInvoicePrintGrouping = ZBool.True;

			AssertEquals("precondition: count of lines", 5, ARInvoiceWrapper.LinesForInvoice.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("FRT");

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Count of lines", 4, ARInvoiceWrapper.LinesForInvoice.Count);

			AssertEquals("Description", "Freight", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 250.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[1].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[1].OSTaxDisplay);

			AssertEquals("Description", "Destination", ARInvoiceWrapper.LinesForInvoice[2].LineDescription);
			AssertEquals("Amount", 200.00M, ARInvoiceWrapper.LinesForInvoice[2].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", ARInvoiceWrapper.LinesForInvoice[2].OSTaxDisplay);

			AssertEquals("Description", "Freight Charges", ARInvoiceWrapper.LinesForInvoice[3].LineDescription);
			AssertEquals("Amount", 500.00M, ARInvoiceWrapper.LinesForInvoice[3].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=50.56 *", ARInvoiceWrapper.LinesForInvoice[3].OSTaxDisplay);
		}

		public void TestRollUpLinesForFRTForOneLineAndOneCDS()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddFreightChargeToInvoice();
			InvoicingLineBase line2 = AddCustomsChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Freight", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Freight", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("FRT");
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Freight Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddFreightChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddCustomsChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Freight", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForFRTForOneLineAndOneNGC()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddFreightChargeToInvoice();
			InvoicingLineBase line2 = AddNotGroupedChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Freight", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Freight", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("FRT", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Freight Charges", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddFreightChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotGroupedChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Freight", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForFRTForOneLineAndOneNJR()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddFreightChargeToInvoice();
			InvoicingLineBase line2 = AddNotJobChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Freight", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Freight", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("FRT", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Freight Charges", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddFreightChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotJobChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Freight", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLineForFRTForAllCharges()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddNotJobChargeToInvoice();
			InvoicingLineBase line11 = AddCommentChargeToInvoice();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("FRT");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 11, theLines.Count);

			int lineNumber = 0;

			AssertEquals("Description", "Brokerage", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Customs Charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Destination", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Freight Charges", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Insurance", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 450.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=45.50 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Loading", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 350.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=35.39 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Non job related charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Origin", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Unloading", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 400.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=40.44 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollUpLineForFRTForAllChargesConsol()
		{
			SetUpConsolInvoiceWrapperForRollUp();
			OrgHeader debtor = Invoice.Header;

			for (int i = 0; i < 2; i++)
			{
				Shipment = Consol.Shipments.AddNew();
				Shipment.JS_UniqueConsignRef = "S000" + i;
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = debtor.PK;

				foreach (InvoicingLineBase line in new[] {
										AddOriginChargeToInvoice(),
										AddDestinationChargeToInvoice(),
										AddFreightChargeToInvoice(),
										AddBrokerageChargeToInvoice(),
										AddLoadChargeToInvoice(),
										AddUnLoadChargeToInvoice(),
										AddInsuranceChargeToInvoice(),
										AddCustomsChargeToInvoice(),
										AddNotGroupedChargeToInvoice(),
										AddCommentChargeToInvoice(),
										AddNotJobChargeToInvoice() }
				)
				{
					line.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, TestObjectCreator.AUD);
				}
			}

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11 * 2, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("FRT");
			Invoice.Header.CompanyData.InvoiceRollupOrGroups[0].PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			Factory.Save();
			theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("Count of lines", 11 * 2, theLines.Count);

			for (int i = 0; i < 2; i++)
			{
				DocARInvoiceLineCollection lines = new DocARInvoiceLineCollection(Factory);
				for (int j = 0; j < theLines.Count / 2; j++)
				{
					lines.Add((BusinessObject)theLines[i * theLines.Count / 2 + j]);
				}

				lines.Sort("LineDescription", ListSortDirection.Ascending);

				int lineNumber = 0;

				AssertEquals("Description", "Brokerage", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 300.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=30.33 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 0M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Customs Charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Destination", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Freight Charges", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Insurance", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 450.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=45.50 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Loading", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 350.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=35.39 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Non job related charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Not grouped charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Origin", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 150.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=15.17 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Unloading", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 400.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=40.44 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("All lines must be tested.", lines.Count, lineNumber);

				for (int j = 0; j < lines.Count; j++)
				{
					AssertEquals("Linked to shipment", Consol.Shipments[i].PK.ToString(), lines[j].FKToShipment);
				}
			}
		}

		#endregion

		#region RollUp ORF

		public void TestRollUpLinesForORFForOneLine()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("ORF");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin and Freight Charges", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);
		}

		public void TestRollUpLinesForORFForOneLineAndOneCDS()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddCustomsChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("ORF", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin and Freight Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddCustomsChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForORFForOneLineAndOneNGC()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddNotGroupedChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("ORF", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin and Freight Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotGroupedChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForORFForOneLineAndOneNJR()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddNotJobChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("ORF", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin and Freight Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotJobChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLineForORFForAllCharges()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddNotJobChargeToInvoice();
			InvoicingLineBase line11 = AddCommentChargeToInvoice();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("ORF");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 10, theLines.Count);

			int lineNumber = 0;

			AssertEquals("Description", "Brokerage", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Customs Charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Destination", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Insurance", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 450.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=45.50 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Loading", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 350.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=35.39 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Non job related charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Origin and Freight Charges", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 400.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=40.45 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Unloading", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 400.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=40.44 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollUpLineForORFForAllChargesConsol()
		{
			SetUpConsolInvoiceWrapperForRollUp();
			OrgHeader debtor = Invoice.Header;

			for (int i = 0; i < 2; i++)
			{
				Shipment = Consol.Shipments.AddNew();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = debtor.PK;

				foreach (InvoicingLineBase line in new[] {
										AddOriginChargeToInvoice(),
										AddDestinationChargeToInvoice(),
										AddFreightChargeToInvoice(),
										AddBrokerageChargeToInvoice(),
										AddLoadChargeToInvoice(),
										AddUnLoadChargeToInvoice(),
										AddInsuranceChargeToInvoice(),
										AddCustomsChargeToInvoice(),
										AddNotGroupedChargeToInvoice(),
										AddCommentChargeToInvoice(),
										AddNotJobChargeToInvoice() }
				)
				{
					line.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, TestObjectCreator.AUD);
				}
			}

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11 * 2, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("ORF");
			Invoice.Header.CompanyData.InvoiceRollupOrGroups[0].PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			Factory.Save();

			theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 10 * 2, theLines.Count);

			for (int i = 0; i < 2; i++)
			{
				DocARInvoiceLineCollection lines = new DocARInvoiceLineCollection(Factory);
				for (int j = 0; j < theLines.Count / 2; j++)
				{
					lines.Add((BusinessObject)theLines[i * theLines.Count / 2 + j]);
				}

				lines.Sort("LineDescription", ListSortDirection.Ascending);

				int lineNumber = 0;

				AssertEquals("Description", "Brokerage", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 300.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=30.33 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 0M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Customs Charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Destination", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Insurance", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 450.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=45.50 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Loading", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 350.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=35.39 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Non job related charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Not grouped charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Origin and Freight Charges", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 400.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=40.45 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Unloading", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 400.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=40.44 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("All lines must be tested.", lines.Count, lineNumber);

				for (int j = 0; j < lines.Count; j++)
				{
					AssertEquals("Linked to shipment", Consol.Shipments[i].PK.ToString(), lines[j].FKToShipment);
				}
			}
		}

		#endregion

		#region RollUp F&D

		public void TestRollUpLinesForFAndDForOneLine()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddDestinationChargeToInvoice();
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Destination", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 200.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("F&D");
			SetUpShipmentDirection();
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Freight, Insurance and Destination Charges", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 200.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Destination", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 200.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);
		}

		public void TestRollUpLinesForFAndDForOneLine_DepartmentAndDirection()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddDestinationChargeToInvoice();

			SetUpDepartmentAndDirection();
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Destination", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 200.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);
		}

		public void TestRollUpLinesForFAndDForOneLineAndOneCDS()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddDestinationChargeToInvoice();
			InvoicingLineBase line2 = AddCustomsChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Destination", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Destination", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("F&D", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Freight, Insurance and Destination Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddDestinationChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddCustomsChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Destination", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForFAndDForOneLineAndOneNGC()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddDestinationChargeToInvoice();
			InvoicingLineBase line2 = AddNotGroupedChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("F&D");
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Freight, Insurance and Destination Charges", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddDestinationChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotGroupedChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForFAndDForOneLineAndOneNJR()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddDestinationChargeToInvoice();
			InvoicingLineBase line2 = AddNotJobChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("F&D", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Freight, Insurance and Destination Charges", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddDestinationChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotJobChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLineForFAndDForAllCharges()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddNotJobChargeToInvoice();
			InvoicingLineBase line11 = AddCommentChargeToInvoice();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("F&D");

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 8, theLines.Count);

			int lineNumber = 0;

			AssertEquals("Description", "Brokerage", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Customs Charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Freight, Insurance and Destination Charges", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 1300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=131.44 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Loading", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 350.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=35.39 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Non job related charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Origin", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollUpLineForFAndDForAllChargesConsol()
		{
			SetUpConsolInvoiceWrapperForRollUp();
			OrgHeader debtor = Invoice.Header;

			for (int i = 0; i < 2; i++)
			{
				Shipment = Consol.Shipments.AddNew();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = debtor.PK;

				foreach (InvoicingLineBase line in new[] {
										AddOriginChargeToInvoice(),
										AddDestinationChargeToInvoice(),
										AddFreightChargeToInvoice(),
										AddBrokerageChargeToInvoice(),
										AddLoadChargeToInvoice(),
										AddUnLoadChargeToInvoice(),
										AddInsuranceChargeToInvoice(),
										AddCustomsChargeToInvoice(),
										AddNotGroupedChargeToInvoice(),
										AddCommentChargeToInvoice(),
										AddNotJobChargeToInvoice() }
				)
				{
					line.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, TestObjectCreator.AUD);
				}
			}

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11 * 2, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("F&D");
			Invoice.Header.CompanyData.InvoiceRollupOrGroups[0].PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			Factory.Save();

			theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("Count of lines", 8 * 2, theLines.Count);

			for (int i = 0; i < 2; i++)
			{
				DocARInvoiceLineCollection lines = new DocARInvoiceLineCollection(Factory);
				for (int j = 0; j < theLines.Count / 2; j++)
				{
					lines.Add((BusinessObject)theLines[i * theLines.Count / 2 + j]);
				}

				lines.Sort("LineDescription", ListSortDirection.Ascending);

				int lineNumber = 0;

				AssertEquals("Description", "Brokerage", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 300.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=30.33 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 0M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Customs Charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Freight, Insurance and Destination Charges", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 1300.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=131.44 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Loading", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 350.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=35.39 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Non job related charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Not grouped charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Origin", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 150.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=15.17 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("All lines must be tested.", lines.Count, lineNumber);

				for (int j = 0; j < lines.Count; j++)
				{
					AssertEquals("Linked to shipment", Consol.Shipments[i].PK.ToString(), lines[j].FKToShipment);
				}
			}
		}

		#endregion

		#region RollUp OFD

		public void TestRollUpLinesForOFDForOneLine()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddDestinationChargeToInvoice();
			AssertEquals("Description", "Destination", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 200.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Destination", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 200.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFD");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Destination Charges", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 200.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Destination", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 200.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);
		}

		public void TestRollUpLinesForOFDForOneLineAndOneCDS()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddDestinationChargeToInvoice();
			InvoicingLineBase line2 = AddCustomsChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Destination", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Destination", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFD", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Destination Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddDestinationChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddCustomsChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Destination", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForOFDForOneLineAndOneNGC()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddDestinationChargeToInvoice();
			InvoicingLineBase line2 = AddNotGroupedChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFD", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination Charges", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddDestinationChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotGroupedChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForOFDForOneLineAndOneNJR()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddDestinationChargeToInvoice();
			InvoicingLineBase line2 = AddNotJobChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFD", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination Charges", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddDestinationChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotJobChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLineForOFDForAllCharges()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddNotJobChargeToInvoice();
			InvoicingLineBase line11 = AddWarehouseInwardschargeToInvoice();
			InvoicingLineBase line12 = AddWarehouseOutwardsChargeToInvoice();
			InvoicingLineBase line13 = AddWarehouseStorageChargeToInvoice();
			InvoicingLineBase line14 = AddCommentChargeToInvoice();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 14, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("OFD");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 11, theLines.Count);

			int lineNumber = 0;

			AssertEquals("Description", "Brokerage", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 30.33M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 330.33M, theLines[lineNumber].OSAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 30.33M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 300.0M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 0M, theLines[lineNumber].OSAmount);
			AssertEquals("GST", "", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 0M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 0M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Customs Charge", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 20.22M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 220.22M, theLines[lineNumber].OSAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 20.22M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 200.0M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Destination Charges", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 600.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 60.66M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 660.66M, theLines[lineNumber].OSAmount);
			AssertEquals("OSTaxDisplay", "10.11%=60.66 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 60.66M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 600.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Freight and Insurance Charges", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 700.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 70.78M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 770.78M, theLines[lineNumber].OSAmount);
			AssertEquals("OSTaxDisplay", "10.11%=70.78 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 70.78M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 700.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Non job related charge", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 25.28M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 275.28M, theLines[lineNumber].OSAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 25.28M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 250.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("OSAmount", 275.28M, theLines[lineNumber].OSAmount);
			AssertEquals("GSTVAT", 25.28M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 250.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Origin Charges", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 500.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 50.56M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 550.56M, theLines[lineNumber].OSAmount);
			AssertEquals("OSTaxDisplay", "10.11%=50.56 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 50.56M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 500.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			//Others
			AssertEquals("Description", "Warehouse Orders", theLines[lineNumber].LineDescription);
			lineNumber++;
			AssertEquals("Description", "Warehouse Receiving", theLines[lineNumber].LineDescription);
			lineNumber++;
			AssertEquals("Description", "Warehouse Storage", theLines[lineNumber].LineDescription);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollUpLineForOFDForAllChargesConsol()
		{
			SetUpConsolInvoiceWrapperForRollUp();
			OrgHeader debtor = Invoice.Header;

			for (int i = 0; i < 2; i++)
			{
				Shipment = Consol.Shipments.AddNew();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = debtor.PK;

				foreach (InvoicingLineBase line in new[] {
										AddOriginChargeToInvoice(),
										AddDestinationChargeToInvoice(),
										AddFreightChargeToInvoice(),
										AddBrokerageChargeToInvoice(),
										AddLoadChargeToInvoice(),
										AddUnLoadChargeToInvoice(),
										AddInsuranceChargeToInvoice(),
										AddCustomsChargeToInvoice(),
										AddNotGroupedChargeToInvoice(),
										AddCommentChargeToInvoice(),
										AddNotJobChargeToInvoice(),
										AddWarehouseInwardschargeToInvoice(),
										AddWarehouseOutwardsChargeToInvoice(),
										AddWarehouseStorageChargeToInvoice() }
				)
				{
					line.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, TestObjectCreator.AUD);
				}
			}

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 14 * 2, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("OFD");
			Invoice.Header.CompanyData.InvoiceRollupOrGroups[0].PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			Factory.Save();
			theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("Count of lines", 11 * 2, theLines.Count);

			for (int i = 0; i < 2; i++)
			{
				DocARInvoiceLineCollection lines = new DocARInvoiceLineCollection(Factory);
				for (int j = 0; j < theLines.Count / 2; j++)
				{
					lines.Add((BusinessObject)theLines[i * theLines.Count / 2 + j]);
				}

				lines.Sort("LineDescription", ListSortDirection.Ascending);

				int lineNumber = 0;

				AssertEquals("Description", "Brokerage", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 300.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 30.33M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 330.33M, lines[lineNumber].OSAmount);
				AssertEquals("GST", "10.11%=30.33 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 30.33M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 300.0M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 0M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 0M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 0M, lines[lineNumber].OSAmount);
				AssertEquals("GST", "", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 0M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 0M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Customs Charge", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 20.22M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 220.22M, lines[lineNumber].OSAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 20.22M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 200.0M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Destination Charges", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 600.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 60.66M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 660.66M, lines[lineNumber].OSAmount);
				AssertEquals("OSTaxDisplay", "10.11%=60.66 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 60.66M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 600.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Freight and Insurance Charges", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 700.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 70.78M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 770.78M, lines[lineNumber].OSAmount);
				AssertEquals("GST", "10.11%=70.78 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 70.78M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 700.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Non job related charge", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 25.28M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 275.28M, lines[lineNumber].OSAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 25.28M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 250.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Not grouped charge", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("OSAmount", 275.28M, lines[lineNumber].OSAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 25.28M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 250.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Origin Charges", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 500.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 50.56M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 550.56M, lines[lineNumber].OSAmount);
				AssertEquals("GST", "10.11%=50.56 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 50.56M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 500.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				//Others
				AssertEquals("Description", "Warehouse Orders", lines[lineNumber].LineDescription);
				lineNumber++;
				AssertEquals("Description", "Warehouse Receiving", lines[lineNumber].LineDescription);
				lineNumber++;
				AssertEquals("Description", "Warehouse Storage", lines[lineNumber].LineDescription);
				lineNumber++;

				AssertEquals("All lines must be tested.", lines.Count, lineNumber);

				for (int j = 0; j < lines.Count; j++)
				{
					AssertEquals("Linked to shipment", Consol.Shipments[i].PK.ToString(), lines[j].FKToShipment);
				}
			}
		}

		#endregion

		#region RollUp OFO

		public void TestRollUpLinesForOFOForOneLine()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFO");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin Charges", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);
		}

		public void TestRollUpLinesForOFOForOneLineAndOneCDS()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddCustomsChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFO", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddCustomsChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForOFOForOneLineAndOneNGC()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddNotGroupedChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFO", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotGroupedChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForOFOForOneLineAndOneNJR()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddNotJobChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//First Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFO", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotJobChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLineForOFOForAllCharges()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddNotJobChargeToInvoice();
			InvoicingLineBase line11 = AddWarehouseInwardschargeToInvoice();
			InvoicingLineBase line12 = AddWarehouseOutwardsChargeToInvoice();
			InvoicingLineBase line13 = AddWarehouseStorageChargeToInvoice();
			InvoicingLineBase line14 = AddCommentChargeToInvoice();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 14, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("OFO");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 12, theLines.Count);

			int lineNumber = 0;

			AssertEquals("Description", "Brokerage", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 30.33M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 300.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 0M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 0M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Customs Charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 20.22M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 200.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Destination", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 20.22M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 200.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Freight and Insurance Charges", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 700.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=70.78 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 70.78M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 700.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Non job related charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 25.28M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 250.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 25.28M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 250.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Origin Charges", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 500.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=50.56 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 50.56M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 500.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Unloading", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 400.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=40.44 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 40.44M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 400.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			//Others
			AssertEquals("Description", "Warehouse Orders", theLines[lineNumber].LineDescription);
			lineNumber++;
			AssertEquals("Description", "Warehouse Receiving", theLines[lineNumber].LineDescription);
			lineNumber++;
			AssertEquals("Description", "Warehouse Storage", theLines[lineNumber].LineDescription);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollUpLineForOFOForAllChargesConsol()
		{
			SetUpConsolInvoiceWrapperForRollUp();
			OrgHeader debtor = Invoice.Header;

			for (int i = 0; i < 2; i++)
			{
				Shipment = Consol.Shipments.AddNew();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = debtor.PK;

				foreach (InvoicingLineBase line in new[] {
										AddOriginChargeToInvoice(),
										AddDestinationChargeToInvoice(),
										AddFreightChargeToInvoice(),
										AddBrokerageChargeToInvoice(),
										AddLoadChargeToInvoice(),
										AddUnLoadChargeToInvoice(),
										AddInsuranceChargeToInvoice(),
										AddCustomsChargeToInvoice(),
										AddNotGroupedChargeToInvoice(),
										AddCommentChargeToInvoice(),
										AddNotJobChargeToInvoice(),
										AddWarehouseInwardschargeToInvoice(),
										AddWarehouseOutwardsChargeToInvoice(),
										AddWarehouseStorageChargeToInvoice() }
				)
				{
					line.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, TestObjectCreator.AUD);
				}
			}

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 14 * 2, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("OFO");
			Invoice.Header.CompanyData.InvoiceRollupOrGroups[0].PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			Factory.Save();
			theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("Count of lines", 12 * 2, theLines.Count);

			for (int i = 0; i < 2; i++)
			{
				DocARInvoiceLineCollection lines = new DocARInvoiceLineCollection(Factory);
				for (int j = 0; j < theLines.Count / 2; j++)
				{
					lines.Add((BusinessObject)theLines[i * theLines.Count / 2 + j]);
				}

				lines.Sort("LineDescription", ListSortDirection.Ascending);

				int lineNumber = 0;

				AssertEquals("Description", "Brokerage", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 300.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=30.33 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 30.33M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 300.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 0M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 0M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 0M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Customs Charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 20.22M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 200.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Destination", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 20.22M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 200.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Freight and Insurance Charges", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 700.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=70.78 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 70.78M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 700.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Non job related charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 25.28M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 250.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Not grouped charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 25.28M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 250.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Origin Charges", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 500.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=50.56 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 50.56M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 500.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Unloading", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 400.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=40.44 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 40.44M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 400.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				//Others
				AssertEquals("Description", "Warehouse Orders", lines[lineNumber].LineDescription);
				lineNumber++;
				AssertEquals("Description", "Warehouse Receiving", lines[lineNumber].LineDescription);
				lineNumber++;
				AssertEquals("Description", "Warehouse Storage", lines[lineNumber].LineDescription);
				lineNumber++;

				AssertEquals("All lines must be tested.", lines.Count, lineNumber);

				for (int j = 0; j < lines.Count; j++)
				{
					AssertEquals("Linked to shipment", Consol.Shipments[i].PK.ToString(), lines[j].FKToShipment);
				}
			}
		}

		#endregion

		#region RollUp OFF

		public void TestRollUpLinesForOFFForOneLine()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddDestinationChargeToInvoice();
			AssertEquals("Description", "Destination", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 200.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Destination", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 200.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFF");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Destination Charges", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 200.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Destination", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 200.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);
		}

		public void TestRollUpLinesForOFFForOneLineAndOneCDS()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddDestinationChargeToInvoice();
			InvoicingLineBase line2 = AddCustomsChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Destination", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Destination", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFF", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Destination Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddDestinationChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddCustomsChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Destination", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForOFFForOneLineAndOneNGC()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddDestinationChargeToInvoice();
			InvoicingLineBase line2 = AddNotGroupedChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFF", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination Charges", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddDestinationChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotGroupedChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForOFFForOneLineAndOneNJR()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddDestinationChargeToInvoice();
			InvoicingLineBase line2 = AddNotJobChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFF", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination Charges", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddDestinationChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotJobChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Non job related charge", theLines[1].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForOFFForMultipleChargesPerCategory()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line2 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line3 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line6 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line7 = AddFreightChargeToInvoice();
			InvoicingLineBase line8 = AddFreightChargeToInvoice();
			InvoicingLineBase line9 = AddFreightChargeToInvoice();

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 9, theLines.Count);
		}

		public void TestRollUpLinesForOFFForMultipleChargesPerCategory_DepartmentAndDirection()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line2 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line3 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line6 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line7 = AddFreightChargeToInvoice();
			InvoicingLineBase line8 = AddFreightChargeToInvoice();
			InvoicingLineBase line9 = AddFreightChargeToInvoice();

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("OFF");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			var theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 5, theLines.Count);

			//First Line
			AssertEquals("Description", "Brokerage", theLines[0].LineDescription);
			AssertEquals("OSExTaxAmount", 300.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 30.33M, theLines[0].OSTaxAmount);
			AssertEquals("OSAmount", 330.33M, theLines[0].OSAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[0].OSTaxDisplay);
			AssertEquals("GSTVAT", 30.33M, theLines[0].GSTVAT);
			AssertEquals("LineAmount", 300.0M, theLines[0].LineAmount);

			//Second Line
			AssertEquals("Description", "Brokerage", theLines[1].LineDescription);
			AssertEquals("OSExTaxAmount", 300.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 30.33M, theLines[1].OSTaxAmount);
			AssertEquals("OSAmount", 330.33M, theLines[1].OSAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[1].OSTaxDisplay);
			AssertEquals("GSTVAT", 30.33M, theLines[1].GSTVAT);
			AssertEquals("LineAmount", 300.0M, theLines[1].LineAmount);

			//Third Line
			AssertEquals("Description", "Brokerage", theLines[2].LineDescription);
			AssertEquals("OSExTaxAmount", 300.00M, theLines[2].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 30.33M, theLines[2].OSTaxAmount);
			AssertEquals("OSAmount", 330.33M, theLines[2].OSAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[2].OSTaxDisplay);
			AssertEquals("GSTVAT", 30.33M, theLines[2].GSTVAT);
			AssertEquals("LineAmount", 300.0M, theLines[2].LineAmount);

			//Fourth Line
			AssertEquals("Description", "Freight Charges", theLines[3].LineDescription);
			AssertEquals("OSExTaxAmount", 750.00M, theLines[3].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 75.84M, theLines[3].OSTaxAmount);
			AssertEquals("OSAmount", 825.84M, theLines[3].OSAmount);
			AssertEquals("OSTaxDisplay", "10.11%=75.84 *", theLines[3].OSTaxDisplay);
			AssertEquals("GSTVAT", 75.84M, theLines[3].GSTVAT);
			AssertEquals("LineAmount", 750.00M, theLines[3].LineAmount);

			//Fifth Line
			AssertEquals("Description", "Insurance Charges", theLines[4].LineDescription);
			AssertEquals("OSExTaxAmount", 1350.00M, theLines[4].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 136.50M, theLines[4].OSTaxAmount);
			AssertEquals("OSAmount", 1486.50M, theLines[4].OSAmount);
			AssertEquals("OSTaxDisplay", "10.11%=136.50 *", theLines[4].OSTaxDisplay);
			AssertEquals("GSTVAT", 136.50M, theLines[4].GSTVAT);
			AssertEquals("LineAmount", 1350.00M, theLines[4].LineAmount);
		}

		public void TestRollUpLineForOFFForAllCharges()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddNotJobChargeToInvoice();
			InvoicingLineBase line11 = AddWarehouseInwardschargeToInvoice();
			InvoicingLineBase line12 = AddWarehouseOutwardsChargeToInvoice();
			InvoicingLineBase line13 = AddWarehouseStorageChargeToInvoice();
			InvoicingLineBase line14 = AddCommentChargeToInvoice();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 14, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("OFF");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 12, theLines.Count);

			int lineNumber = 0;

			AssertEquals("Description", "Brokerage", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 30.33M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 330.33M, theLines[lineNumber].OSAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 30.33M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 300.0M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 0M, theLines[lineNumber].OSAmount);
			AssertEquals("GST", "", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 0M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 0M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Customs Charge", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 20.22M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 220.22M, theLines[lineNumber].OSAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 20.22M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 200.0M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Destination Charges", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 600.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 60.66M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 660.66M, theLines[lineNumber].OSAmount);
			AssertEquals("OSTaxDisplay", "10.11%=60.66 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 60.66M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 600.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Freight Charges", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 25.28M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 275.28M, theLines[lineNumber].OSAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 25.28M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 250.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Insurance Charges", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 450.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 45.50M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 495.50M, theLines[lineNumber].OSAmount);
			AssertEquals("GST", "10.11%=45.50 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 45.50M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 450.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Non job related charge", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 25.28M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 275.28M, theLines[lineNumber].OSAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 25.28M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 250.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("OSAmount", 275.28M, theLines[lineNumber].OSAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 25.28M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 250.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Origin Charges", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 500.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 50.56M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 550.56M, theLines[lineNumber].OSAmount);
			AssertEquals("GST", "10.11%=50.56 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 50.56M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 500.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			//Others
			AssertEquals("Description", "Warehouse Orders", theLines[lineNumber].LineDescription);
			lineNumber++;
			AssertEquals("Description", "Warehouse Receiving", theLines[lineNumber].LineDescription);
			lineNumber++;
			AssertEquals("Description", "Warehouse Storage", theLines[lineNumber].LineDescription);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollUpLineForOFFForAllChargesConsol()
		{
			SetUpConsolInvoiceWrapperForRollUp();
			OrgHeader debtor = Invoice.Header;

			for (int i = 0; i < 2; i++)
			{
				Shipment = Consol.Shipments.AddNew();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = debtor.PK;

				foreach (InvoicingLineBase line in new[] {
										AddOriginChargeToInvoice(),
										AddDestinationChargeToInvoice(),
										AddFreightChargeToInvoice(),
										AddBrokerageChargeToInvoice(),
										AddLoadChargeToInvoice(),
										AddUnLoadChargeToInvoice(),
										AddInsuranceChargeToInvoice(),
										AddCustomsChargeToInvoice(),
										AddNotGroupedChargeToInvoice(),
										AddCommentChargeToInvoice(),
										AddNotJobChargeToInvoice(),
										AddWarehouseInwardschargeToInvoice(),
										AddWarehouseOutwardsChargeToInvoice(),
										AddWarehouseStorageChargeToInvoice() }
				)
				{
					line.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, TestObjectCreator.AUD);
				}
			}

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 14 * 2, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("OFF");
			Invoice.Header.CompanyData.InvoiceRollupOrGroups[0].PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			Factory.Save();
			theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("Count of lines", 12 * 2, theLines.Count);

			for (int i = 0; i < 2; i++)
			{
				DocARInvoiceLineCollection lines = new DocARInvoiceLineCollection(Factory);
				for (int j = 0; j < theLines.Count / 2; j++)
				{
					lines.Add((BusinessObject)theLines[i * theLines.Count / 2 + j]);
				}

				lines.Sort("LineDescription", ListSortDirection.Ascending);

				int lineNumber = 0;

				AssertEquals("Description", "Brokerage", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 300.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 30.33M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 330.33M, lines[lineNumber].OSAmount);
				AssertEquals("GST", "10.11%=30.33 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 30.33M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 300.0M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 0M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 0M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 0M, lines[lineNumber].OSAmount);
				AssertEquals("GST", "", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 0M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 0M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Customs Charge", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 20.22M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 220.22M, lines[lineNumber].OSAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 20.22M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 200.0M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Destination Charges", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 600.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 60.66M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 660.66M, lines[lineNumber].OSAmount);
				AssertEquals("OSTaxDisplay", "10.11%=60.66 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 60.66M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 600.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Freight Charges", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 25.28M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 275.28M, lines[lineNumber].OSAmount);
				AssertEquals("OSTaxDisplay", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 25.28M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 250.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Insurance Charges", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 450.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 45.50M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 495.50M, lines[lineNumber].OSAmount);
				AssertEquals("GST", "10.11%=45.50 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 45.50M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 450.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Non job related charge", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 25.28M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 275.28M, lines[lineNumber].OSAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 25.28M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 250.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Not grouped charge", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("OSAmount", 275.28M, lines[lineNumber].OSAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 25.28M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 250.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Origin Charges", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 500.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 50.56M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 550.56M, lines[lineNumber].OSAmount);
				AssertEquals("GST", "10.11%=50.56 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 50.56M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 500.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Warehouse Orders", lines[lineNumber].LineDescription);
				lineNumber++;
				AssertEquals("Description", "Warehouse Receiving", lines[lineNumber].LineDescription);
				lineNumber++;
				AssertEquals("Description", "Warehouse Storage", lines[lineNumber].LineDescription);
				lineNumber++;

				AssertEquals("All lines must be tested.", lines.Count, lineNumber);

				for (int j = 0; j < lines.Count; j++)
				{
					AssertEquals("Linked to shipment", Consol.Shipments[i].PK.ToString(), lines[j].FKToShipment);
				}
			}
		}

		#endregion

		#region RollUp OFI

		public void TestRollUpLinesForOFIForOneLine()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFI");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin Charges", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);

			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Description", "Origin", ARInvoiceWrapper.LinesForInvoice[0].LineDescription);
			AssertEquals("Amount", 150.00M, ARInvoiceWrapper.LinesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", ARInvoiceWrapper.LinesForInvoice[0].OSTaxDisplay);
		}

		public void TestRollUpLinesForOFIForOneLineAndOneCDS()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddCustomsChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFI", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddCustomsChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForOFIForOneLineAndOneNGC()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddNotGroupedChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFI", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotGroupedChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Not grouped charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForOFIForOneLineAndOneNJR()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddNotJobChargeToInvoice();
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			DocARInvoiceLineCollection theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//First Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpDepartmentAndDirection();
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			SetUpOrganisationForRollUpWithCode("OFI", false);
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin Charges", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", theLines[1].OSTaxDisplay);

			var newInvoicingBase = Factory.New<ARInvoice>();
			line1 = AddOriginChargeToInvoice("", 0, newInvoicingBase);
			line2 = AddNotJobChargeToInvoice("", 0, newInvoicingBase);
			line1.AL_PreventInvoicePrintGrouping = ZBool.True;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(newInvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = aRInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			//First Line
			AssertEquals("Description", "Non job related charge", theLines[0].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 **", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Origin", theLines[1].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", theLines[1].OSTaxDisplay);
		}

		public void TestRollUpLinesForOFIForMultipleChargesPerCategory()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line2 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line3 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line4 = AddDestinationChargeToInvoice();
			InvoicingLineBase line5 = AddDestinationChargeToInvoice();
			InvoicingLineBase line6 = AddDestinationChargeToInvoice();
			InvoicingLineBase line7 = AddFreightChargeToInvoice();
			InvoicingLineBase line8 = AddFreightChargeToInvoice();
			InvoicingLineBase line9 = AddFreightChargeToInvoice();

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 9, theLines.Count);
		}

		public void TestRollUpLinesForOFIForMultipleChargesPerCategory_DepartmentAndDirection()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line2 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line3 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line4 = AddDestinationChargeToInvoice();
			InvoicingLineBase line5 = AddDestinationChargeToInvoice();
			InvoicingLineBase line6 = AddDestinationChargeToInvoice();
			InvoicingLineBase line7 = AddFreightChargeToInvoice();
			InvoicingLineBase line8 = AddFreightChargeToInvoice();
			InvoicingLineBase line9 = AddFreightChargeToInvoice();

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("OFI");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			var theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 5, theLines.Count);

			//First Line
			AssertEquals("Description", "Destination", theLines[0].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[0].OSTaxDisplay);
			AssertEquals("GSTVAT", 20.22M, theLines[0].GSTVAT);
			AssertEquals("LineAmount", 200.00M, theLines[0].LineAmount);

			//Second Line
			AssertEquals("Description", "Destination", theLines[1].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[1].OSTaxDisplay);
			AssertEquals("GSTVAT", 20.22M, theLines[1].GSTVAT);
			AssertEquals("LineAmount", 200.00M, theLines[1].LineAmount);

			//Third Line
			AssertEquals("Description", "Destination", theLines[2].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[2].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[2].OSTaxDisplay);
			AssertEquals("GSTVAT", 20.22M, theLines[2].GSTVAT);
			AssertEquals("LineAmount", 200.00M, theLines[2].LineAmount);

			//Fourth Line
			AssertEquals("Description", "Freight Charges", theLines[3].LineDescription);
			AssertEquals("OSExTaxAmount", 750.00M, theLines[3].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 75.84M, theLines[3].OSTaxAmount);
			AssertEquals("OSAmount", 825.84M, theLines[3].OSAmount);
			AssertEquals("OSTaxDisplay", "10.11%=75.84 *", theLines[3].OSTaxDisplay);
			AssertEquals("GSTVAT", 75.84M, theLines[3].GSTVAT);
			AssertEquals("LineAmount", 750.00M, theLines[3].LineAmount);

			//Fifth Line
			AssertEquals("Description", "Insurance Charges", theLines[4].LineDescription);
			AssertEquals("OSExTaxAmount", 1350.00M, theLines[4].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 136.50M, theLines[4].OSTaxAmount);
			AssertEquals("OSAmount", 1486.50M, theLines[4].OSAmount);
			AssertEquals("OSTaxDisplay", "10.11%=136.50 *", theLines[4].OSTaxDisplay);
			AssertEquals("GSTVAT", 136.50M, theLines[4].GSTVAT);
			AssertEquals("LineAmount", 1350.00M, theLines[4].LineAmount);
		}

		public void TestRollUpLineForOFIForAllCharges()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddNotJobChargeToInvoice();
			InvoicingLineBase line11 = AddWarehouseInwardschargeToInvoice();
			InvoicingLineBase line12 = AddWarehouseOutwardsChargeToInvoice();
			InvoicingLineBase line13 = AddWarehouseStorageChargeToInvoice();
			InvoicingLineBase line14 = AddCommentChargeToInvoice();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 14, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("OFI");
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 13, theLines.Count);

			int lineNumber = 0;

			AssertEquals("Description", "Brokerage", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 30.33M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 300.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 0M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 0M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Customs Charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 20.22M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 200.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Destination", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 20.22M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 200.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Freight Charges", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 25.28M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 275.28M, theLines[lineNumber].OSAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 25.28M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 250.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Insurance Charges", theLines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 450.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 45.50M, theLines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 495.50M, theLines[lineNumber].OSAmount);
			AssertEquals("OSTaxDisplay", "10.11%=45.50 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 45.50M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 450.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Non job related charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 25.28M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 250.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 25.28M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 250.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Origin Charges", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 500.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=50.56 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 50.56M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 500.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Unloading", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 400.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=40.44 *", theLines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 40.44M, theLines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 400.00M, theLines[lineNumber].LineAmount);
			lineNumber++;

			//Others
			AssertEquals("Description", "Warehouse Orders", theLines[lineNumber].LineDescription);
			lineNumber++;
			AssertEquals("Description", "Warehouse Receiving", theLines[lineNumber].LineDescription);
			lineNumber++;
			AssertEquals("Description", "Warehouse Storage", theLines[lineNumber].LineDescription);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollUpLineForOFIForAllChargesConsol()
		{
			SetUpConsolInvoiceWrapperForRollUp();
			OrgHeader debtor = Invoice.Header;

			for (int i = 0; i < 2; i++)
			{
				Shipment = Consol.Shipments.AddNew();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = debtor.PK;

				foreach (InvoicingLineBase line in new[] {
										AddOriginChargeToInvoice(),
										AddDestinationChargeToInvoice(),
										AddFreightChargeToInvoice(),
										AddBrokerageChargeToInvoice(),
										AddLoadChargeToInvoice(),
										AddUnLoadChargeToInvoice(),
										AddInsuranceChargeToInvoice(),
										AddCustomsChargeToInvoice(),
										AddNotGroupedChargeToInvoice(),
										AddCommentChargeToInvoice(),
										AddNotJobChargeToInvoice(),
										AddWarehouseInwardschargeToInvoice(),
										AddWarehouseOutwardsChargeToInvoice(),
										AddWarehouseStorageChargeToInvoice() }
				)
				{
					line.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, TestObjectCreator.AUD);
				}
			}

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 14 * 2, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("OFI");
			Invoice.Header.CompanyData.InvoiceRollupOrGroups[0].PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			Factory.Save();
			theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("Count of lines", 13 * 2, theLines.Count);

			for (int i = 0; i < 2; i++)
			{
				DocARInvoiceLineCollection lines = new DocARInvoiceLineCollection(Factory);
				for (int j = 0; j < theLines.Count / 2; j++)
				{
					lines.Add((BusinessObject)theLines[i * theLines.Count / 2 + j]);
				}

				lines.Sort("LineDescription", ListSortDirection.Ascending);

				int lineNumber = 0;

				AssertEquals("Description", "Brokerage", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 300.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=30.33 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 30.33M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 300.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 0M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 0M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 0M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Customs Charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 20.22M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 200.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Destination", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 20.22M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 200.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Freight Charges", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 25.28M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 275.28M, lines[lineNumber].OSAmount);
				AssertEquals("OSTaxDisplay", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 25.28M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 250.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Insurance Charges", lines[lineNumber].LineDescription);
				AssertEquals("OSExTaxAmount", 450.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxAmount", 45.50M, lines[lineNumber].OSTaxAmount);
				AssertEquals("OSAmount", 495.50M, lines[lineNumber].OSAmount);
				AssertEquals("OSTaxDisplay", "10.11%=45.50 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 45.50M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 450.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Non job related charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 25.28M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 250.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Not grouped charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 25.28M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 250.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Origin Charges", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 500.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=50.56 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 50.56M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 500.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				AssertEquals("Description", "Unloading", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 400.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("GST", "10.11%=40.44 *", lines[lineNumber].OSTaxDisplay);
				AssertEquals("GSTVAT", 40.44M, lines[lineNumber].GSTVAT);
				AssertEquals("LineAmount", 400.00M, lines[lineNumber].LineAmount);
				lineNumber++;

				//Others
				AssertEquals("Description", "Warehouse Orders", lines[lineNumber].LineDescription);
				lineNumber++;
				AssertEquals("Description", "Warehouse Receiving", lines[lineNumber].LineDescription);
				lineNumber++;
				AssertEquals("Description", "Warehouse Storage", lines[lineNumber].LineDescription);
				lineNumber++;

				AssertEquals("All lines must be tested.", lines.Count, lineNumber);

				for (int j = 0; j < lines.Count; j++)
				{
					AssertEquals("Linked to shipment", Consol.Shipments[i].PK.ToString(), lines[j].FKToShipment);
				}
			}
		}

		public void TestRollUpLineForOFIShowsCurrenciesCorrectly()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line = AddOriginChargeToInvoice();
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = line.PK;
			charge.SetAmountsFromLinkedLinesForTests();
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellExRate = 0.777m;
			charge.JR_LocalSellAmt = line.AL_LineAmount;

			line = AddFreightChargeToInvoice();
			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = line.PK;
			charge.SetAmountsFromLinkedLinesForTests();
			charge.JR_RX_NKSellCurrency = "EUR";
			charge.JR_OSSellExRate = 1.2m;
			charge.JR_LocalSellAmt = line.AL_LineAmount;

			DocARInvoiceLineCollection lines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 2, lines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode("OFI").PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			lines = ARInvoiceWrapper.LinesForInvoice;
			lines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, lines.Count);

			AssertEquals("Description", "Freight Charges EUR 300.00 @ 1.200000", lines[0].LineDescriptionAndExchangeRate);
			AssertEquals("OSExTaxAmount", 250.00M, lines[0].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 25.28M, lines[0].OSTaxAmount);
			AssertEquals("OSAmount", 275.28M, lines[0].OSAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", lines[0].OSTaxDisplay);
			AssertEquals("GSTVAT", 25.28M, lines[0].GSTVAT);
			AssertEquals("LineAmount", 250.00M, lines[0].LineAmount);

			AssertEquals("Description", "Origin Charges USD 116.55 @ 0.777000", lines[1].LineDescriptionAndExchangeRate);
			AssertEquals("Amount", 150.00M, lines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.17 *", lines[1].OSTaxDisplay);
			AssertEquals("GSTVAT", 15.17M, lines[1].GSTVAT);
			AssertEquals("LineAmount", 150.00M, lines[1].LineAmount);
		}

		#endregion

		#region Roll Up Other Tests

		public void TestRollUpForImportThenExportDirection()
		{
			#region Import Direction

			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddNotJobChargeToInvoice();
			InvoicingLineBase line11 = AddCommentChargeToInvoice();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11, theLines.Count);

			Shipment.JS_RL_NKOrigin = "NZAKL";
			Shipment.JS_RL_NKDestination = "AUBNE";
			//SetUpDepartment();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;

			AssertLines();

			#endregion

			#region Export Direction

			//Export Department

			Shipment.JS_RL_NKOrigin = "AUBNE";
			Shipment.JS_RL_NKDestination = "GBLON";
			SetUpOrganisationForRollUpAEC();

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 6, theLines.Count);

			int lineNumber = 0;
			//First Line
			AssertEquals("Description", "Brokerage", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=30.33 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			//Second Line
			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			//Third Line
			AssertEquals("Description", "Customs Charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			//Fourth Line
			AssertEquals("Description", "Non job related charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			//Fifth Line
			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			//Sixth Line
			AssertEquals("Description", "Origin, Freight, Insurance and Destination Charges", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 1800.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=182.00 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("All lines must be checked.", theLines.Count, lineNumber);
			#endregion
		}

		#region Domestic and CrossTrade Direction

		public void TestRollupForDomesticDirection()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddNotJobChargeToInvoice();
			InvoicingLineBase line11 = AddCommentChargeToInvoice();

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11, theLines.Count);

			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "AUBNE";

			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Domestic;

			AssertLines();
		}

		public void TestRolllupForCrossTradeDirection()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddNotJobChargeToInvoice();
			InvoicingLineBase line11 = AddCommentChargeToInvoice();

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11, theLines.Count);

			Shipment.JS_RL_NKOrigin = "NZAKL";
			Shipment.JS_RL_NKDestination = "USLAX";

			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.CrossTrade;

			AssertLines();
		}

		#endregion

		#endregion

		#region TestRollUpLinesByChargeCodeGroup

		public void TestRollupByChargeCodeGroupForLargeInvoice()
		{
			JobStorage storage = (JobStorage)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Warehouse.IWhsInvoice)));
			Job invoiceJob = new Job.Loader(storage).TryCreate();

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = header.CompanyData.InvoiceRollupOrGroups.AddNew();
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CCG;
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.WarehouseStorage.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			header.CompanyData.SetARTaxApplicable(ZBool.True);

			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_OH = header.PK;
			Invoice.AH_ConsolidatedInvoiceRef = "I00001234";
			Invoice.AH_JH = invoiceJob.PK;

			for (int index = 0; index <= 60; index++)
			{
				AddWarehouseStorageChargeToInvoice(generateRandomUniqueStringForProperties: true);
				AddWarehouseInwardschargeToInvoice(generateRandomUniqueStringForProperties: true);
				AddWarehouseOutwardsChargeToInvoice(generateRandomUniqueStringForProperties: true);
			}

			InvoicingBase.Lines.IsManagedForDataRefresh = false;

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			DocARInvoiceLineCollection lines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals(3, lines.Count);
		}

		public void TestRolledUpLineOSTaxDisplayForNoTaxRate()
		{
			AssertEquals("N/A", GetRollUpLineForTest(new AccTaxRate[] { null }).OSTaxDisplay);
		}

		public void TestRolledUpLineOSTaxDisplayForNotReportable()
		{
			var rate = Factory.NewWithValidTestData<AccTaxRate>(TestBusinessObjectKind.MinimumRequiredToSave);
			rate.AT_Type = AccTaxRate.Types.NotReportable;

			AssertEquals("Not Applicable", GetRollUpLineForTest(rate, null).OSTaxDisplay);
		}

		public void TestRolledUpLineOSTaxDisplayForReportable()
		{
			var rate = Factory.NewWithValidTestData<AccTaxRate>(TestBusinessObjectKind.MinimumRequiredToSave);
			rate.AT_Type = AccTaxRate.Types.Rated;
			rate.SetRateNumerator_ForTestOnly(10);

			AssertEquals("10%=24.00", GetRollUpLineForTest(rate, null).OSTaxDisplay);
		}

		public void TestRolledUpLineOSTaxDisplayForReportableWithZeroRate()
		{
			var rate = Factory.NewWithValidTestData<AccTaxRate>(TestBusinessObjectKind.MinimumRequiredToSave);
			rate.AT_Type = AccTaxRate.Types.Rated;
			rate.SetRateNumerator_ForTestOnly(0);

			AssertEquals("Zero Rated", GetRollUpLineForTest(rate, null).OSTaxDisplay);
		}

		public void TestRolledUpLineOSTaxDisplayForAllReportableWithExtraTaxRate()
		{
			var rate = Factory.NewWithValidTestData<AccTaxRate>(TestBusinessObjectKind.MinimumRequiredToSave);
			rate.AT_Type = AccTaxRate.Types.Rated;
			rate.SetRateNumerator_ForTestOnly(10);
			rate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			rate.SetExtraRate_ForTestOnly(5, 1);

			AssertEquals("GST 10%=24.00,\r\nQST 5%=13.20", GetRollUpLineForTest(rate).OSTaxDisplay);
		}

		public void TestRolledUpLineOSTaxDisplayForAllReportableWithExtraTaxRate_QCT()
		{
			var rate = Factory.NewWithValidTestData<AccTaxRate>(TestBusinessObjectKind.MinimumRequiredToSave);
			rate.AT_Type = AccTaxRate.Types.Rated;
			rate.SetRateNumerator_ForTestOnly(10);
			rate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			rate.SetExtraRate_ForTestOnly(5, 1);

			AssertEquals("GST 10%=24.00,\r\nQST 5%=12.00", GetRollUpLineForTest(rate).OSTaxDisplay);
		}

		DocARInvoiceLineForRollUp GetRollUpLineForTest(params AccTaxRate[] taxRates)
		{
			JobStorage storage = (JobStorage)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Warehouse.IWhsInvoice)));
			Job invoiceJob = new Job.Loader(storage).TryCreate();

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = header.CompanyData.InvoiceRollupOrGroups.AddNew();
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CCD;
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.WarehouseStorage.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			header.CompanyData.SetARTaxApplicable(ZBool.True);

			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_OH = header.PK;
			Invoice.AH_ConsolidatedInvoiceRef = "I00001234";
			Invoice.AH_JH = invoiceJob.PK;

			InvoicingLineBase[] invoiceLines = {
								AddWarehouseStorageChargeToInvoice(),
								AddWarehouseInwardschargeToInvoice(),
								AddWarehouseOutwardsChargeToInvoice()
						};

			for (int i = 0; i < invoiceLines.Length; i++)
			{
				AccTaxRate rate = i < taxRates.Length ? taxRates[i] : taxRates[taxRates.Length - 1];
				invoiceLines[i].AL_AT = rate != null ? rate.PK : ZGuid.Empty;
			}

			InvoicingBase.Lines.IsManagedForDataRefresh = false;

			ARInvoiceLine newline = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			newline.AL_AC = InvoicingBase.Lines[0].AL_AC;
			newline.AL_Desc = InvoicingBase.Lines[0].AL_Desc;
			newline.AL_OSExTaxAmount = InvoicingBase.Lines[0].AL_OSExTaxAmount;
			newline.AL_AT = InvoicingBase.Lines[0].AL_AT;

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			DocARInvoiceLineCollection lines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals(3, lines.Count);

			IDocARInvoiceLine[] wSTLines = lines.GetLinesByOSEXTaxAmount(240m);
			AssertEquals("There should be 1 and only 1 240 Line", wSTLines.Length, 1);
			AssertEquals("Line Description used as only 1 line", "Warehouse Storage", wSTLines[0].LineDescription);

			DocARInvoiceLineForRollUp result = wSTLines[0] as DocARInvoiceLineForRollUp;
			AssertNotNull("RollUp line", result);

			return result;
		}

		public void TestRollupByChargeCode()
		{
			JobStorage storage = (JobStorage)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Warehouse.IWhsInvoice)));
			Job invoiceJob = new Job.Loader(storage).TryCreate();

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = header.CompanyData.InvoiceRollupOrGroups.AddNew();
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CCD;
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.WarehouseStorage.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			header.CompanyData.SetARTaxApplicable(ZBool.True);

			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_OH = header.PK;
			Invoice.AH_ConsolidatedInvoiceRef = "I00001234";
			Invoice.AH_JH = invoiceJob.PK;

			AddWarehouseStorageChargeToInvoice();
			AddWarehouseInwardschargeToInvoice();
			AddWarehouseOutwardsChargeToInvoice();

			InvoicingBase.Lines.IsManagedForDataRefresh = false;

			ARInvoiceLine newline = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			newline.AL_AC = InvoicingBase.Lines[0].AL_AC;
			newline.AL_Desc = InvoicingBase.Lines[0].AL_Desc;
			newline.AL_OSExTaxAmount = InvoicingBase.Lines[0].AL_OSExTaxAmount;

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));

			DocARInvoiceLineCollection lines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals(3, lines.Count);

			IDocARInvoiceLine[] wSTLines = lines.GetLinesByOSEXTaxAmount(240m);
			AssertEquals("There should be 1 and only 1 240 Line", wSTLines.Length, 1);
			AssertEquals("Line Description used as only 1 line", "Warehouse Storage", wSTLines[0].LineDescription);

			IDocARInvoiceLine[] wINLines = lines.GetLinesByOSEXTaxAmount(150m);
			AssertEquals("There should be 1 and only 1 150 Line", wINLines.Length, 1);
			AssertEquals("Line Description used as only 1 line", "Warehouse Receiving", wINLines[0].LineDescription);

			IDocARInvoiceLine[] wOULines = lines.GetLinesByOSEXTaxAmount(75m);
			AssertEquals("There should be 1 and only 1 75 Line", wOULines.Length, 1);
			AssertEquals("Line Description used as only 1 line", "Warehouse Orders", wOULines[0].LineDescription);

			InvoicingBase.Lines[0].AL_PreventInvoicePrintGrouping = true;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			lines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Setting prevent grouping should separate one of the lines out", 4, lines.Count);

			wINLines = lines.GetLinesByOSEXTaxAmount(150m);
			AssertEquals("There should be 1 and only 1 150 Line", wINLines.Length, 1);
			AssertEquals("Line Description used as only 1 line", "Warehouse Receiving", wINLines[0].LineDescription);

			wOULines = lines.GetLinesByOSEXTaxAmount(75m);
			AssertEquals("There should be 1 and only 1 75 Line", wOULines.Length, 1);
			AssertEquals("Line Description used as only 1 line", "Warehouse Orders", wOULines[0].LineDescription);

			wSTLines = lines.GetLinesByOSEXTaxAmount(120m);
			AssertEquals("There should be 1 and only 1 120 Line", wSTLines.Length, 2);
		}

		public void TestRollupByChargeCodeWithTaxRate()
		{
			JobStorage storage = (JobStorage)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Warehouse.IWhsInvoice)));
			Job invoiceJob = new Job.Loader(storage).TryCreate();

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = header.CompanyData.InvoiceRollupOrGroups.AddNew();
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CCD;
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.WarehouseStorage.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			header.CompanyData.SetARTaxApplicable(ZBool.True);

			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_OH = header.PK;
			Invoice.AH_ConsolidatedInvoiceRef = "I00001234";
			Invoice.AH_JH = invoiceJob.PK;

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>(TestBusinessObjectKind.MinimumRequiredToSave);
			chargeCode.AC_ChargeGroup = "WST";
			chargeCode.AC_Desc = "default description";

			ARInvoiceLine line = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line.AL_Desc = "Warehouse Storage";
			line.AL_OSExTaxAmount = 120m;
			line.AL_OSTaxAmount = 12m;
			line.AL_AC = chargeCode.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;

			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line2.AL_Desc = "Warehouse Storage Again";
			line2.AL_OSExTaxAmount = 200m;
			line2.AL_OSTaxAmount = 0m;
			line2.AL_AC = chargeCode.PK;
			line2.AL_AT = TestObjectCreator.GSTFREE1.PK;

			InvoicingBase.Lines.IsManagedForDataRefresh = false;

			ARInvoiceLine newline = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			newline.AL_AC = InvoicingBase.Lines[0].AL_AC;
			newline.AL_AT = InvoicingBase.Lines[0].AL_AT;
			newline.AL_Desc = InvoicingBase.Lines[0].AL_Desc;
			newline.AL_OSExTaxAmount = InvoicingBase.Lines[0].AL_OSExTaxAmount;
			newline.AL_OSTaxAmount = InvoicingBase.Lines[0].AL_OSTaxAmount;

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));

			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
			DocARInvoiceLineCollection lines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals(1, lines.Count);
			AssertEquals("24.00", lines[0].OSTaxDisplayNoAsterisksWithRegistryRule);

			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code);
			lines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals(2, lines.Count);
			AssertEquals("10%", lines[0].OSTaxDisplayNoAsterisksWithRegistryRule);
			AssertEquals("Zero Rated", lines[1].OSTaxDisplayNoAsterisksWithRegistryRule);

			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);
			lines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals(2, lines.Count);
			AssertEquals("10%=24.00", lines[0].OSTaxDisplayNoAsterisksWithRegistryRule);
			AssertEquals("Zero Rated", lines[1].OSTaxDisplayNoAsterisksWithRegistryRule);
		}

		public void TestRollUpByChargeCodeLocalDescription()
		{
			bool cachedDefault = AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			string cachedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				string defaultDescription = "default description";
				string localDescription = "local language description";

				JobStorage storage = (JobStorage)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Warehouse.IWhsInvoice)));
				Job invoiceJob = new Job.Loader(storage).TryCreate();

				var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
				header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
				OrgInvoiceRollupOrGroup invoiceRollupOrGroup = header.CompanyData.InvoiceRollupOrGroups.AddNew();
				invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
				invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CCD;
				invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.WarehouseStorage.Code;
				invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
				invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
				header.CompanyData.SetARTaxApplicable(ZBool.True);

				header.OH_RL_NKClosestPort = "AUSYD";
				AssertEquals("Set country NZ", "AU", header.Country.RN_Code);
				GlbCompany.CurrentCompany.SetCountry("AU");

				Factory.Save();

				Invoice = Factory.New<ARInvoice>();
				Invoice.AH_OH = header.PK;
				Invoice.AH_ConsolidatedInvoiceRef = "I00001234";
				Invoice.AH_JH = invoiceJob.PK;

				AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>(TestBusinessObjectKind.MinimumRequiredToSave);
				chargeCode.AC_ChargeGroup = "WST";
				chargeCode.AC_Desc = defaultDescription;

				ARInvoiceLine line = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				line.AL_Desc = "Warehouse Storage";
				line.AL_OSExTaxAmount = 120m;
				line.AL_OSTaxAmount = 12m;
				line.AL_AC = chargeCode.PK;
				line.AL_AT = GetRate();

				ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				line2.AL_Desc = "Warehouse Storage Again";
				line2.AL_OSExTaxAmount = 120m;
				line2.AL_OSTaxAmount = 12m;
				line2.AL_AC = chargeCode.PK;
				line2.AL_AT = GetRate();

				InvoicingBase.Lines.IsManagedForDataRefresh = false;

				ARInvoiceLine newline = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				newline.AL_AC = InvoicingBase.Lines[0].AL_AC;
				newline.AL_Desc = InvoicingBase.Lines[0].AL_Desc;
				newline.AL_OSExTaxAmount = InvoicingBase.Lines[0].AL_OSExTaxAmount;

				DocARInvoiceLine invoiceLineWrapper = DocARInvoiceLine.New(newline, Factory);
				DocARInvoiceCommon invoiceWrapper = DocARInvoice.New(Invoice, Factory);

				AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertEquals("no local language description, registry enabled", defaultDescription, invoiceWrapper.LinesForInvoice[0].LineDescription);
				invoiceLineWrapper.ChargeCode.AccChargeCode.AC_LocalLanguageDescription = localDescription;
				AssertEquals("local language description, registry enabled", localDescription, invoiceWrapper.LinesForInvoice[0].LineDescription);
				invoiceLineWrapper.ChargeCode.AccChargeCode.AC_LocalLanguageDescription = string.Empty;
				AssertEquals("no local language description, registry disabled", defaultDescription, invoiceWrapper.LinesForInvoice[0].LineDescription);

				AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				invoiceLineWrapper.ChargeCode.AccChargeCode.AC_LocalLanguageDescription = localDescription;
				AssertEquals("no local language description, registry disabled", defaultDescription, invoiceWrapper.LinesForInvoice[0].LineDescription);

				header.OH_RL_NKClosestPort = "NZAKL";
				AssertEquals("Set country NZ", "NZ", header.Country.RN_Code);

				AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertEquals("no local language description, registry enabled", defaultDescription, invoiceWrapper.LinesForInvoice[0].LineDescription);
				invoiceLineWrapper.ChargeCode.AccChargeCode.AC_LocalLanguageDescription = localDescription;
				AssertEquals("local language description, registry enabled", defaultDescription, invoiceWrapper.LinesForInvoice[0].LineDescription);
				invoiceLineWrapper.ChargeCode.AccChargeCode.AC_LocalLanguageDescription = string.Empty;
				AssertEquals("no local language description, registry disabled", defaultDescription, invoiceWrapper.LinesForInvoice[0].LineDescription);

				AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				invoiceLineWrapper.ChargeCode.AccChargeCode.AC_LocalLanguageDescription = localDescription;
				AssertEquals("no local language description, registry disabled", defaultDescription, invoiceWrapper.LinesForInvoice[0].LineDescription);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, cachedDefault);
				GlbCompany.CurrentCompany.SetCountry(cachedCountry);
			}
		}

		public void TestRollupByChargeCodeForFFXInvoice()
		{
			JobStorage storage = (JobStorage)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Warehouse.IWhsInvoice)));
			Job invoiceJob = new Job.Loader(storage).TryCreate();

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = header.CompanyData.InvoiceRollupOrGroups.AddNew();
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CCD;
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.WarehouseStorage.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceRollupOrGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.FreightFOBExRate;
			header.CompanyData.SetARTaxApplicable(ZBool.True);

			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_OH = header.PK;
			Invoice.AH_ConsolidatedInvoiceRef = "I00001234";
			Invoice.AH_JH = invoiceJob.PK;

			ZQuery queryForCode1 = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			queryForCode1.AddToFilter(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, "MRG");

			AccChargeCode code1 = Factory.LoadTop1<AccChargeCode>(queryForCode1);
			AssertNotNull(code1);
			ZQuery queryForCode2 = new ZQuery(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.NotEqual, code1.AC_Code);
			queryForCode2.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			queryForCode2.AddToFilter(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, "MRG");
			AccChargeCode code2 = Factory.LoadTop1<AccChargeCode>(queryForCode2);
			AssertNotNull(code2);
			AssertNotEquals(code1.AC_Code, code2.AC_Code);

			RefCurrency currency1 = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
			AssertNotNull(currency1);

			ZQuery queryForCurrency2 = new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, currency1.RX_Code);
			queryForCurrency2.AddToFilter(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency).AddToFilter(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, currency1.RX_Code);
			RefCurrency currency2 = Factory.LoadTop1<RefCurrency>(queryForCurrency2);
			AssertNotNull(currency2);

			ZDecimal exchangeRate = 0.42;

			InvoicingBase.Lines.AddNewBasedOnChargeSellValues(invoiceJob.Charges.AddNewWithSellValues(code1.PK, header.PK, 100m, currency1.RX_Code, exchangeRate, "Whatever"));
			InvoicingBase.Lines.AddNewBasedOnChargeSellValues(invoiceJob.Charges.AddNewWithSellValues(code1.PK, header.PK, 200m, currency1.RX_Code, exchangeRate, "Whatever"));
			InvoicingBase.Lines.AddNewBasedOnChargeSellValues(invoiceJob.Charges.AddNewWithSellValues(code2.PK, header.PK, 50m, currency1.RX_Code, exchangeRate, "Whatever"));
			InvoicingBase.Lines.AddNewBasedOnChargeSellValues(invoiceJob.Charges.AddNewWithSellValues(code2.PK, header.PK, 25m, currency1.RX_Code, exchangeRate, "Whatever"));

			InvoicingBase.Lines.IsManagedForDataRefresh = false;

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			DocARInvoiceLineCollection lines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("There should be 2 rollup items", lines.Count, 2);

			IDocARInvoiceLine[] linesForCode1 = lines.GetLinesByOSEXTaxAmount(714.29m);
			AssertEquals("There should be only 1 line for code1", 1, linesForCode1.Length);
			ZDecimal expectedFXAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(714.29m, exchangeRate, currency1.RX_Code);
			string expectedEndofDescriptionForCode1 = string.Join(" ", new string[] { currency1.RX_Code.ToString(), FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(expectedFXAmount, currency1), "@", exchangeRate.ToString() });

			string actualEndOfDescriptionForCode1 = linesForCode1[0].LineDescriptionAndExchangeRate.Right(expectedEndofDescriptionForCode1.Length).TrimEnd();
			AssertEquals("The description for line for code1 should include OS currency, amount and exchange rate", expectedEndofDescriptionForCode1, actualEndOfDescriptionForCode1);

			IDocARInvoiceLine[] linesForCode2 = lines.GetLinesByOSEXTaxAmount(178.57m);
			AssertEquals("There should be only 1 line for code1", linesForCode2.Length, 1);
			expectedFXAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(178.57m, exchangeRate, currency1.RX_Code);
			string expectedEndofDescriptionForCode2 = string.Join(" ", new string[] { currency1.RX_Code.ToString(), FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(expectedFXAmount, currency1), "@", exchangeRate.ToString() });
			string actualEndOfDescriptionForCode2 = linesForCode2[0].LineDescriptionAndExchangeRate.Right(expectedEndofDescriptionForCode2.Length).TrimEnd();
			AssertEquals("The description for line for code2 should include OS currency, amount and exchange rate", expectedEndofDescriptionForCode2, actualEndOfDescriptionForCode2);

			InvoicingLineBase[] lineWithCode2Amount25 = InvoicingBase.Lines.GetLinesByChargeCodeAndOSExTaxAmount(code2.AC_Code, 25m);
			AssertEquals("There should be only one line with code1 and 200", lineWithCode2Amount25.Length, 1);
			AssertEquals("That line should currently be currency1", lineWithCode2Amount25[0].AL_RX_NKTransactionCurrency, currency1.RX_Code);

			invoiceJob.Charges.RemoveAll();
			InvoicingBase.Lines.RemoveAll();
			InvoicingBase.Lines.AddNewBasedOnChargeSellValues(invoiceJob.Charges.AddNewWithSellValues(code1.PK, header.PK, 100m, currency1.RX_Code, exchangeRate, "Whatever"));
			InvoicingBase.Lines.AddNewBasedOnChargeSellValues(invoiceJob.Charges.AddNewWithSellValues(code1.PK, header.PK, 200m, currency1.RX_Code, exchangeRate, "Whatever"));
			InvoicingBase.Lines.AddNewBasedOnChargeSellValues(invoiceJob.Charges.AddNewWithSellValues(code2.PK, header.PK, 50m, currency1.RX_Code, exchangeRate, "Whatever"));
			InvoicingBase.Lines.AddNewBasedOnChargeSellValues(invoiceJob.Charges.AddNewWithSellValues(code2.PK, header.PK, 25m, currency2.RX_Code, exchangeRate, "Whatever"));

			lineWithCode2Amount25 = InvoicingBase.Lines.GetLinesByChargeCodeAndOSExTaxAmount(code2.AC_Code, 25m);
			AssertEquals("That line should now be currency2", lineWithCode2Amount25[0].AL_RX_NKTransactionCurrency, currency2.RX_Code);

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			lines = ARInvoiceWrapper.LinesForInvoice;

			linesForCode1 = lines.GetLinesByOSEXTaxAmount(714.29m);
			AssertEquals("There should be only 1 line for code1", linesForCode1.Length, 1);
			actualEndOfDescriptionForCode1 = linesForCode1[0].LineDescriptionAndExchangeRate.Right(expectedEndofDescriptionForCode1.Length).TrimEnd();
			AssertEquals("The description for line for code1 should include OS currency, amount and exchange rate", expectedEndofDescriptionForCode1, actualEndOfDescriptionForCode1);

			linesForCode2 = lines.GetLinesByOSEXTaxAmount(178.57m);
			AssertEquals("There should be only 1 line for code1", linesForCode2.Length, 1);
			expectedEndofDescriptionForCode2 = "Whatever";
			AssertNotEquals("The description for line for code2 should NOT include OS currency, amount and exchange rate", expectedEndofDescriptionForCode2, linesForCode2[0].LineDescriptionAndExchangeRate.Right(expectedEndofDescriptionForCode2.Length));
		}

		public void TestRollUpLinesByChargeCodeGroup()
		{
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "TEST3";
			debtor.OH_IsDebtor = true;
			debtor.OH_IsConsignor = false;
			debtor.OH_IsConsignee = false;
			debtor.OH_IsBroker = false;
			debtor.OH_IsForwarder = false;

			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = "S1234";
			Shipment.JS_IsCFSRegistered = ZBool.True;
			Shipment.JS_IsForwardRegistered = ZBool.False;

			JobHeader job = GetInvoiceJob(Shipment, Invoice);
			job.LocalChargesPK = debtor.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AddAllChargesToInvoice();
			AddAllChargesToInvoice();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("should be 32, 2 of each charge", 34, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollByChargeCodeGroup();
			Factory.Save();
			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 19, theLines.Count);

			int lineNumber = 0;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.CFSLoadList, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.CFSShipment, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 120.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=12.14 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.Brokerage, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 600.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=60.66 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.CustomsDuty, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 400.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=40.44 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Destination, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 400.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=40.44 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Freight, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 500.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=50.56 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Insurance, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 900.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=91.00 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.Loading, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 700.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=70.78 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.NonJobRelated, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 500.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=50.56 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Origin, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=30.34 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.Transport, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 90.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=9.10 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.Unloading, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 800.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=80.88 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.WHSOutwards, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.16 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.WHSInwards, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=30.34 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.WHSStorage, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 240.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=24.26 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollUpLinesByChargeCodeGroupConsol()
		{
			SetUpConsolInvoiceWrapperForRollUp();
			OrgHeader debtor = Invoice.Header;

			for (int i = 0; i < 2; i++)
			{
				Shipment = Consol.Shipments.AddNew();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = debtor.PK;

				AddAllChargesToInvoice(job);
				AddAllChargesToInvoice(job);
			}

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("should be 34 * 2, 2 of each charge", 34 * 2, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollByChargeCodeGroup();
			Invoice.Header.CompanyData.InvoiceRollupOrGroups[0].PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			Factory.Save();
			theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("Count of lines", 19 * 2, theLines.Count);

			for (int i = 0; i < 2; i++)
			{
				DocARInvoiceLineCollection lines = new DocARInvoiceLineCollection(Factory);
				for (int j = 0; j < theLines.Count / 2; j++)
				{
					lines.Add((BusinessObject)theLines[i * theLines.Count / 2 + j]);
				}

				lines.Sort("LineDescription", ListSortDirection.Ascending);

				int lineNumber = 0;

				AssertEquals("Description", ChargeCodeGroupList.Descriptions.CFSLoadList, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 200.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=20.22 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", ChargeCodeGroupList.Descriptions.CFSShipment, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 120.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=12.14 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 0M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 0M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", ChargeCodeGroupList.Descriptions.Brokerage, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 600.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=60.66 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", ChargeCodeGroupList.Descriptions.CustomsDuty, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 400.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=40.44 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Destination, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 400.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=40.44 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Freight, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 500.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=50.56 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Insurance, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 900.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=91.00 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", ChargeCodeGroupList.Descriptions.Loading, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 700.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=70.78 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", ChargeCodeGroupList.Descriptions.NonJobRelated, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 500.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=50.56 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Not grouped charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", "Not grouped charge", lines[lineNumber].LineDescription);
				AssertEquals("Amount", 250.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=25.28 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Origin, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 300.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=30.34 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", ChargeCodeGroupList.Descriptions.Transport, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 90.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=9.10 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", ChargeCodeGroupList.Descriptions.Unloading, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 800.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=80.88 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", ChargeCodeGroupList.Descriptions.WHSOutwards, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 150.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=15.16 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", ChargeCodeGroupList.Descriptions.WHSInwards, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 300.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=30.34 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("Description", ChargeCodeGroupList.Descriptions.WHSStorage, lines[lineNumber].LineDescription);
				AssertEquals("Amount", 240.00M, lines[lineNumber].OSExTaxAmount);
				AssertEquals("OSTaxDisplay", "10.11%=24.26 *", lines[lineNumber].OSTaxDisplay);
				lineNumber++;

				AssertEquals("All lines must be tested.", lines.Count, lineNumber);

				for (int j = 0; j < lines.Count; j++)
				{
					AssertEquals("Linked to shipment", Consol.Shipments[i].PK.ToString(), lines[j].FKToShipment);
				}
			}
		}

		public void TestRollUpLinesByChargeCodeGroupConsol_EntireConsol()
		{
			SetUpConsolInvoiceWrapperForRollUp();
			OrgHeader debtor = Invoice.Header;

			for (int i = 0; i < 2; i++)
			{
				Shipment = Consol.Shipments.AddNew();
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				JobHeader job = GetInvoiceJob(Shipment);
				job.LocalChargesPK = debtor.PK;

				AddAllChargesToInvoice(job);
				AddAllChargesToInvoice(job);
			}

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("should be 34 * 2, 2 of each charge", 34 * 2, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollByChargeCodeGroup();
			Invoice.Header.CompanyData.InvoiceRollupOrGroups[0].PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			Invoice.Header.CompanyData.InvoiceRollupOrGroups[0].PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUpEntireConsol;
			Factory.Save();
			theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("Count of lines", 23, theLines.Count);

			foreach (IDocARInvoiceLine line in theLines)
			{
				if (line is DocARInvoiceLine)
				{
					AssertNotEquals("FKToShipment should be specified when rolling up by entire Consol, but line has 'not grouped' type", ZString.Empty, line.FKToShipment);
				}
				else
				{
					AssertEquals("FKToShipment not specified when rolling up by entire Consol", ZString.Empty, line.FKToShipment);
				}
			}

			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			int lineNumber = 0;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.CFSLoadList, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 400.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=40.44 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.CFSShipment, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 240.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=24.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.Brokerage, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 1200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=121.32 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.CustomsDuty, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 800.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=80.88 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Destination, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 800.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=80.88 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Freight, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 1000.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=101.12 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Insurance, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 1800.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=182.00 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.Loading, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 1400.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=141.56 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.NonJobRelated, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 1000.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=101.12 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Origin, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 600.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=60.68 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.Transport, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 180.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=18.20 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.Unloading, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 1600.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=161.76 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.WHSOutwards, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=30.32 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.WHSInwards, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 600.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=60.68 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.WHSStorage, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 480.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=48.52 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestRollupByChargeCode_MiscInvoice()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = header.CompanyData.InvoiceRollupOrGroups.AddNew();
			invoiceRollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code;
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CCD;
			header.CompanyData.SetARTaxApplicable(ZBool.True);

			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_OH = header.PK;

			AddWarehouseStorageChargeToInvoice();
			AddWarehouseInwardschargeToInvoice();
			AddWarehouseOutwardsChargeToInvoice();

			InvoicingBase.Lines.IsManagedForDataRefresh = false;

			ARInvoiceLine newline = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			newline.AL_AC = InvoicingBase.Lines[0].AL_AC;
			newline.AL_Desc = InvoicingBase.Lines[0].AL_Desc;
			newline.AL_OSExTaxAmount = InvoicingBase.Lines[0].AL_OSExTaxAmount;

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));

			DocARInvoiceLineCollection lines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals(3, lines.Count);

			IDocARInvoiceLine[] wSTLines = lines.GetLinesByOSEXTaxAmount(240m);
			AssertEquals("There should be 1 and only 1 240 Line", wSTLines.Length, 1);
			AssertEquals("Line Description used as only 1 line", "Warehouse Storage", wSTLines[0].LineDescription);

			IDocARInvoiceLine[] wINLines = lines.GetLinesByOSEXTaxAmount(150m);
			AssertEquals("There should be 1 and only 1 150 Line", wINLines.Length, 1);
			AssertEquals("Line Description used as only 1 line", "Warehouse Receiving", wINLines[0].LineDescription);

			IDocARInvoiceLine[] wOULines = lines.GetLinesByOSEXTaxAmount(75m);
			AssertEquals("There should be 1 and only 1 75 Line", wOULines.Length, 1);
			AssertEquals("Line Description used as only 1 line", "Warehouse Orders", wOULines[0].LineDescription);

			InvoicingBase.Lines[0].AL_PreventInvoicePrintGrouping = true;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			lines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Setting prevent grouping should separate one of the lines out", 4, lines.Count);

			wINLines = lines.GetLinesByOSEXTaxAmount(150m);
			AssertEquals("There should be 1 and only 1 150 Line", wINLines.Length, 1);
			AssertEquals("Line Description used as only 1 line", "Warehouse Receiving", wINLines[0].LineDescription);

			wOULines = lines.GetLinesByOSEXTaxAmount(75m);
			AssertEquals("There should be 1 and only 1 75 Line", wOULines.Length, 1);
			AssertEquals("Line Description used as only 1 line", "Warehouse Orders", wOULines[0].LineDescription);

			wSTLines = lines.GetLinesByOSEXTaxAmount(120m);
			AssertEquals("There should be 1 and only 1 120 Line", wSTLines.Length, 2);
		}

		public void TestRollUpLinesByChargeCodeGroup_MiscInvoice()
		{
			Invoice = Factory.New<ARInvoice>();
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AddAllChargesToInvoice();
			AddAllChargesToInvoice();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("should be 32, 2 of each charge", 34, theLines.Count);

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollByChargeCodeGroup();

			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code;
			Factory.Save();

			theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 19, theLines.Count);

			int lineNumber = 0;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.CFSLoadList, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.CFSShipment, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 120.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=12.14 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Comment charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 0M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.Brokerage, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 600.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=60.66 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.CustomsDuty, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 400.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=40.44 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Destination, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 400.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=40.44 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Freight, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 500.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=50.56 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Insurance, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 900.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=91.00 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.Loading, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 700.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=70.78 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.NonJobRelated, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 500.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("GST", "10.11%=50.56 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", DocRollUpConstants.RollupAndSubTotalDescriptions.Origin, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=30.34 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.Transport, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 90.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=9.10 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.Unloading, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 800.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=80.88 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.WHSOutwards, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 150.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=15.16 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.WHSInwards, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=30.34 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("Description", ChargeCodeGroupList.Descriptions.WHSStorage, theLines[lineNumber].LineDescription);
			AssertEquals("Amount", 240.00M, theLines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=24.26 *", theLines[lineNumber].OSTaxDisplay);
			lineNumber++;

			AssertEquals("All lines must be tested.", theLines.Count, lineNumber);
		}

		public void TestUpdatingDescriptionRegistryChangesLineDescriptions()
		{
			AssertDescriptionsAreOverridenForRollupStyle(OrgConstants.InvoiceLineGroupings.Code.CCG, DocRollUpConstants.RollupAndSubTotalDescriptions.Freight, DocRollUpConstants.RollupAndSubTotalDescriptions.Insurance);
			AssertDescriptionsAreOverridenForRollupStyle(OrgConstants.InvoiceLineGroupings.Code.OFF, DocRollUpConstants.RollupAndSubTotalDescriptions.Freight, DocRollUpConstants.RollupAndSubTotalDescriptions.Insurance);

			var systemRollupDescRegistryCollection = getNewRollupCollection("freight for system CCG", "insurance for system CCG", "freight for system OFF", "insurance for system OFF");
			AccountingConfigurationRegistry.Instance.InvoiceRollupAndGroupDescriptionRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemRollupDescRegistryCollection);

			AssertDescriptionsAreOverridenForRollupStyle(OrgConstants.InvoiceLineGroupings.Code.CCG, "freight for system CCG", "insurance for system CCG");
			AssertDescriptionsAreOverridenForRollupStyle(OrgConstants.InvoiceLineGroupings.Code.OFF, "freight for system OFF", "insurance for system OFF");

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company1Branch = Factory.NewWithValidTestData<GlbBranch>();
			company1Branch.GB_GC = company1.PK;
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var company2Branch = Factory.NewWithValidTestData<GlbBranch>();
			company2Branch.GB_GC = company2.PK;
			Factory.Save();

			var company1RollupDescRegistryCollection = getNewRollupCollection("freight for company1 CCG", "insurance for company1 CCG", "freight for company1 OFF", "insurance for company1 OFF");
			AccountingConfigurationRegistry.Instance.InvoiceRollupAndGroupDescriptionRegistryItem.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RollupDescRegistryCollection);

			var company2RollupDescRegistryCollection = getNewRollupCollection("freight for company2 CCG", "insurance for company2 CCG", "freight for company2 OFF", "insurance for company2 OFF");
			AccountingConfigurationRegistry.Instance.InvoiceRollupAndGroupDescriptionRegistryItem.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, company2RollupDescRegistryCollection);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company1Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertDescriptionsAreOverridenForRollupStyle(OrgConstants.InvoiceLineGroupings.Code.CCG, "freight for company1 CCG", "insurance for company1 CCG");
				AssertDescriptionsAreOverridenForRollupStyle(OrgConstants.InvoiceLineGroupings.Code.OFF, "freight for company1 OFF", "insurance for company1 OFF");
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company2Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertDescriptionsAreOverridenForRollupStyle(OrgConstants.InvoiceLineGroupings.Code.CCG, "freight for company2 CCG", "insurance for company2 CCG");
				AssertDescriptionsAreOverridenForRollupStyle(OrgConstants.InvoiceLineGroupings.Code.OFF, "freight for company2 OFF", "insurance for company2 OFF");
			}
		}

		void AssertDescriptionsAreOverridenForRollupStyle(string rollupStyle, string freightDescription, string insuranceDescription)
		{
			Invoice = InvoicingBase = Factory.New<ARInvoice>();
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AddFreightChargeToInvoice();
			AddInsuranceChargeToInvoice();

			SetUpDepartmentAndDirection();
			SetUpOrganisationForRollUpWithCode(rollupStyle);

			var rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code;
			Factory.Save();

			var theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, theLines.Count);

			AssertEquals(freightDescription, theLines[0].LineDescription);
			AssertEquals(insuranceDescription, theLines[1].LineDescription);
		}

		InvoiceRollupAndGroupDescriptionCollection getNewRollupCollection(string ccgFreightDescription, string ccgInsuranceDescription, string offFreightDescription, string offInsuranceDescription)
		{
			var rollupDescRegistryCollection = AccountingConfigurationRegistry.Instance.LookupInvoiceRollupAndGroupDescriptionDefaultValues();
			var ccgFRT = getRegistryObjectByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.CCG, DocRollUpConstants.RollupAndSubTotalGroups.Freight, rollupDescRegistryCollection);
			ccgFRT.EnglishDescription = ccgFreightDescription;
			var ccgINS = getRegistryObjectByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.CCG, DocRollUpConstants.RollupAndSubTotalGroups.Insurance, rollupDescRegistryCollection);
			ccgINS.EnglishDescription = ccgInsuranceDescription;
			var offFRT = getRegistryObjectByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OFF, DocRollUpConstants.RollupAndSubTotalGroups.Freight, rollupDescRegistryCollection);
			offFRT.EnglishDescription = offFreightDescription;
			var offINS = getRegistryObjectByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OFF, DocRollUpConstants.RollupAndSubTotalGroups.Insurance, rollupDescRegistryCollection);
			offINS.EnglishDescription = offInsuranceDescription;
			return rollupDescRegistryCollection;
		}

		InvoiceRollupAndGroupDescription getRegistryObjectByStyleAndGroup(ZString style, ZString group, InvoiceRollupAndGroupDescriptionCollection collection)
		{
			return collection.Cast<InvoiceRollupAndGroupDescription>().First(x => x.Style == style && x.Group == group);
		}

		#endregion

		void AddAllChargesToInvoice()
		{
			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddNotJobChargeToInvoice();
			InvoicingLineBase line11 = AddCFSLoadListJobChargeToInvoice();
			InvoicingLineBase line12 = AddCFSShipmentJobChargeToInvoice();
			InvoicingLineBase line13 = AddTransportJobChargeToInvoice();
			InvoicingLineBase line14 = AddWarehouseStorageChargeToInvoice();
			InvoicingLineBase line15 = AddWarehouseInwardschargeToInvoice();
			InvoicingLineBase line16 = AddWarehouseOutwardsChargeToInvoice();
			InvoicingLineBase line17 = AddCommentChargeToInvoice();
		}

		void AddAllChargesToInvoice(JobHeader job)
		{
			foreach (InvoicingLineBase line in new[]
			{
								AddOriginChargeToInvoice(),
								AddDestinationChargeToInvoice(),
								AddFreightChargeToInvoice(),
								AddBrokerageChargeToInvoice(),
								AddLoadChargeToInvoice(),
								AddUnLoadChargeToInvoice(),
								AddInsuranceChargeToInvoice(),
								AddCustomsChargeToInvoice(),
								AddNotGroupedChargeToInvoice(),
								AddCommentChargeToInvoice(),
								AddNotJobChargeToInvoice(),
								AddCFSLoadListJobChargeToInvoice(),
								AddCFSShipmentJobChargeToInvoice(),
								AddTransportJobChargeToInvoice(),
								AddWarehouseStorageChargeToInvoice(),
								AddWarehouseInwardschargeToInvoice(),
								AddWarehouseOutwardsChargeToInvoice() })
			{
				line.AL_JH = job.PK;
				var charge = TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, TestObjectCreator.AUD);
				AssertEquals(line.AL_LineType == "REV" ? charge.JR_AT_SellGSTRate : charge.JR_AT_CostGSTRate, line.AL_AT);
				AssertEquals(line.AL_LineType == "REV" ? charge.JR_A9_SellVATClass : charge.JR_A9_CostVATClass, line.AL_A9_VATClass);
			}
		}

		#endregion

		#region Title Tests

		public void TestTaxInvoiceTitle()
		{
			AccountingConfigurationRegistry.Instance.TaxInvoiceAmendmentTitle.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TAX INVOICE AMENDMENT");
			AccountingConfigurationRegistry.Instance.TaxCreditNoteReversalTitle.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TAX CREDIT NOTE REVERSAL");

			var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AT = Factory.NewWithValidTestData(typeof(AccTaxRate)).PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			Factory.Save();
			AssertEquals("TAX INVOICE", ARInvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this tax invoice with your payment if paying by cheque", ARInvoiceWrapper.InvoiceFooterMessage);

			var original = Invoice as IAmending;
			var amending = original.GenerateAmendingTransaction(TransactionTypes.Invoice);
			var wrappedAmendingInvoice = DocARInvoice.New((InvoicingBase)amending, Factory);
			Factory.Save();
			AssertEquals("TAX INVOICE AMENDMENT", wrappedAmendingInvoice.DocumentTitle);

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(Invoice);
			reversing.Reverse();
			var wrappedReversedInvoice = DocARInvoice.New((InvoicingBase)Invoice, Factory);
			var wrappedReversingInvoice = DocARInvoice.New((InvoicingBase)reversing.ReverseTransaction, Factory);
			Factory.Save();
			AssertEquals("TAX INVOICE", wrappedReversedInvoice.DocumentTitle);
			AssertEquals("TAX CREDIT NOTE REVERSAL", wrappedReversingInvoice.DocumentTitle);
		}

		public void TestTaxInvoiceAmendmentTitleForMalaysia()
		{
			if (ExpectedBusinessObjectType != typeof(DocAPInvoice))
			{
				AccTaxRate myGSTTaxRate = null;

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malaysia))
				{
					myGSTTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 3333, 100);
					AssertNotNull(nameof(myGSTTaxRate), myGSTTaxRate);

					Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
					var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
					line.AL_AT = myGSTTaxRate.PK;
					line.AL_AG = TestObjectCreator.GLHeader1.PK;
					Factory.Save();
				}
				var wrappedInvoice = DocARInvoice.New((InvoicingBase)Invoice, Factory);
				AssertEquals("TAX INVOICE", ARInvoiceWrapper.DocumentTitle);

				var original = Invoice as IAmending;
				var amending = original.GenerateAmendingTransaction(TransactionTypes.Invoice);
				Factory.Save();

				var amendingTransactionLine = ((InvoicingBase)amending).Lines[0];
				amendingTransactionLine.AL_AT = myGSTTaxRate.PK;
				AssertNotEquals("Transaction has at least one non SER tax rate.", AccTaxRate.ExtraTypes.ServiceTax, amendingTransactionLine.TaxRate.AT_ExtraTaxRateType);

				wrappedInvoice = DocARInvoice.New((InvoicingBase)amending, Factory);
				AssertEquals("TAX INVOICE", wrappedInvoice.DocumentTitle);
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malaysia))
				{
					AssertEquals("TAX INVOICE (DEBIT NOTE)", wrappedInvoice.DocumentTitle);

					AccountingConfigurationRegistry.Instance.TaxInvoiceAmendmentTitle.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TAX INVOICE OVERRIDDEN");
					AssertEquals("TAX INVOICE OVERRIDDEN", wrappedInvoice.DocumentTitle);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestNonTaxInvoiceTitle()
		{
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			Factory.Save();
			AssertEquals("INVOICE", ARInvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this invoice with your payment if paying by cheque", ARInvoiceWrapper.InvoiceFooterMessage);
		}

		public void TestNonTaxInvoiceTitleWithNullTaxAndNotReportableChargeCodes()
		{
			var line = ((InvoicingLineBase)InvoicingBase.Lines.AddNew());
			line.AL_AT = ZGuid.Empty;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			var notReportTaxRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "NOTREPORT"));
			var line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_AT = notReportTaxRate.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			Factory.Save();
			AssertEquals("INVOICE", ARInvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this invoice with your payment if paying by cheque", ARInvoiceWrapper.InvoiceFooterMessage);

			notReportTaxRate.AT_Type = AccTaxRate.Types.Exempt;
			AssertEquals("TAX INVOICE", ARInvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this tax invoice with your payment if paying by cheque", ARInvoiceWrapper.InvoiceFooterMessage);

			notReportTaxRate.AT_Type = AccTaxRate.Types.ExcludedFromTheTaxBase;
			AssertEquals("INVOICE", ARInvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this invoice with your payment if paying by cheque", ARInvoiceWrapper.InvoiceFooterMessage);
		}

		public void TestNonTaxInvoiceTitleWithAllNotReportableChargeCodes()
		{
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			var notReportTaxRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "NOTREPORT"));
			var line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_AT = notReportTaxRate.PK;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			var line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_AT = notReportTaxRate.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();
			AssertEquals("INVOICE", ARInvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this invoice with your payment if paying by cheque", ARInvoiceWrapper.InvoiceFooterMessage);
		}

		public void TestTaxDisbursementInvoiceTitle()
		{
			AccountingConfigurationRegistry.Instance.TaxDisbursementInvoiceAmendmentTitle.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TAX INVOICE DISBURSEMENT AMENDMENT");
			AccountingConfigurationRegistry.Instance.TaxDisbursementCreditNoteReversalTitle.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TAX CREDIT NOTE DISBURSEMENT REVERSAL");

			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Factory.Save();

			var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AT = Factory.NewWithValidTestData(typeof(AccTaxRate)).PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			AssertEquals("TAX DISBURSEMENT INVOICE", ARInvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this tax disbursement invoice with your payment if paying by cheque", ARInvoiceWrapper.InvoiceFooterMessage);

			var original = Invoice as IAmending;
			var amending = original.GenerateAmendingTransaction(TransactionTypes.Invoice);
			var wrappedAmendingInvoice = DocARInvoice.New((InvoicingBase)amending, Factory);
			Factory.Save();
			AssertEquals("TAX INVOICE DISBURSEMENT AMENDMENT", wrappedAmendingInvoice.DocumentTitle);

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(Invoice);
			reversing.Reverse();
			var wrappedReversedInvoice = DocARInvoice.New((InvoicingBase)Invoice, Factory);
			var wrappedReversingInvoice = DocARInvoice.New((InvoicingBase)reversing.ReverseTransaction, Factory);
			Factory.Save();
			AssertEquals("TAX DISBURSEMENT INVOICE", wrappedReversedInvoice.DocumentTitle);
			AssertEquals("TAX CREDIT NOTE DISBURSEMENT REVERSAL", wrappedReversingInvoice.DocumentTitle);
		}

		public void TestTaxDisbursementInvoiceAmendmentTitleForMalaysia()
		{
			AccTaxRate myGSTTaxRate = null;

			if (ExpectedBusinessObjectType != typeof(DocAPInvoice))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malaysia))
				{
					myGSTTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 3333, 100);
					AssertNotNull(nameof(myGSTTaxRate), myGSTTaxRate);

					Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
					Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
					var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
					line.AL_AT = myGSTTaxRate.PK;
					line.AL_AG = TestObjectCreator.GLHeader1.PK;

					Factory.Save();
				}
				var wrappedInvoice = DocARInvoice.New((InvoicingBase)Invoice, Factory);
				AssertEquals("TAX DISBURSEMENT INVOICE", ARInvoiceWrapper.DocumentTitle);

				var original = Invoice as IAmending;
				var amending = original.GenerateAmendingTransaction(TransactionTypes.Invoice);
				Factory.Save();

				var amendingTransactionLine = ((InvoicingBase)amending).Lines[0];
				amendingTransactionLine.AL_AT = myGSTTaxRate.PK;
				AssertNotEquals("Transaction has at least one non SER tax rate.", AccTaxRate.ExtraTypes.ServiceTax, amendingTransactionLine.TaxRate.AT_ExtraTaxRateType);

				wrappedInvoice = DocARInvoice.New((InvoicingBase)amending, Factory);
				AssertEquals("TAX DISBURSEMENT INVOICE", wrappedInvoice.DocumentTitle);
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malaysia))
				{
					AssertEquals("TAX DISBURSEMENT INVOICE (DEBIT NOTE)", wrappedInvoice.DocumentTitle);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestNonTaxDisbursementInvoiceTitle()
		{
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Factory.Save();
			AssertEquals("DISBURSEMENT INVOICE", ARInvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this disbursement invoice with your payment if paying by cheque", ARInvoiceWrapper.InvoiceFooterMessage);
		}

		public void TestNonTaxDisbursementInvoiceTitleWithAllNotReportableChargeCodes()
		{
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			var notReportTaxRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "NOTREPORT"));
			var line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line1.AL_AT = notReportTaxRate.PK;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			var line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_AT = notReportTaxRate.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Factory.Save();
			AssertEquals("DISBURSEMENT INVOICE", ARInvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this disbursement invoice with your payment if paying by cheque", ARInvoiceWrapper.InvoiceFooterMessage);
		}

		public void TestTaxCreditNoteTitle()
		{
			AccountingConfigurationRegistry.Instance.TaxCreditNoteAmendmentTitle.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TAX CREDIT NOTE AMENDMENT");
			AccountingConfigurationRegistry.Instance.TaxInvoiceReversalTitle.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TAX INVOICE REVERSAL");

			Invoice = Factory.New<ARCreditNote>();
			Invoice.AH_TransactionType = TransactionTypes.CreditNote;
			var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AT = Factory.NewWithValidTestData(typeof(AccTaxRate)).PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AssertEquals("TAX CREDIT NOTE", ARInvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this tax credit note with your payment if paying by cheque", ARInvoiceWrapper.InvoiceFooterMessage);

			var original = Invoice as IAmending;
			var amending = original.GenerateAmendingTransaction(TransactionTypes.CreditNote);
			var wrappedAmendingInvoice = DocARInvoice.New((InvoicingBase)amending, Factory);
			Factory.Save();
			AssertEquals("TAX CREDIT NOTE AMENDMENT", wrappedAmendingInvoice.DocumentTitle);

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(Invoice);
			reversing.Reverse();
			var wrappedReversedInvoice = DocARInvoice.New((InvoicingBase)Invoice, Factory);
			var wrappedReversingInvoice = DocARInvoice.New((InvoicingBase)reversing.ReverseTransaction, Factory);
			Factory.Save();
			AssertEquals("TAX CREDIT NOTE", wrappedReversedInvoice.DocumentTitle);
			AssertEquals("TAX INVOICE REVERSAL", wrappedReversingInvoice.DocumentTitle);
		}

		public void TestTaxDisbursementCreditNoteTitle()
		{
			AccountingConfigurationRegistry.Instance.TaxDisbursementCreditNoteAmendmentTitle.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TAX CREDIT NOTE DISBURSEMENT AMENDMENT");
			AccountingConfigurationRegistry.Instance.TaxDisbursementInvoiceReversalTitle.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TAX INVOICE DISBURSEMENT REVERSAL");

			Invoice = Factory.New<ARCreditNote>();
			Invoice.AH_TransactionType = TransactionTypes.CreditNote;
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			var line = ((InvoicingLineBase)InvoicingBase.Lines.AddNew());
			line.AL_AT = Factory.NewWithValidTestData(typeof(AccTaxRate)).PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			AssertEquals("TAX CREDIT NOTE", ARInvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this tax credit note with your payment if paying by cheque", ARInvoiceWrapper.InvoiceFooterMessage);

			var original = Invoice as IAmending;
			var amending = original.GenerateAmendingTransaction(TransactionTypes.CreditNote);
			var wrappedAmendingInvoice = DocARInvoice.New((InvoicingBase)amending, Factory);
			Factory.Save();
			AssertEquals("TAX CREDIT NOTE DISBURSEMENT AMENDMENT", wrappedAmendingInvoice.DocumentTitle);

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(Invoice);
			reversing.Reverse();
			var wrappedReversedInvoice = DocARInvoice.New((InvoicingBase)Invoice, Factory);
			var wrappedReversingInvoice = DocARInvoice.New((InvoicingBase)reversing.ReverseTransaction, Factory);
			Factory.Save();
			AssertEquals("TAX CREDIT NOTE", wrappedReversedInvoice.DocumentTitle);
			AssertEquals("TAX INVOICE DISBURSEMENT REVERSAL", wrappedReversingInvoice.DocumentTitle);
		}

		public void TestNonTaxCreditNoteTitle()
		{
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			Invoice = Factory.New<ARCreditNote>();
			Invoice.AH_TransactionType = TransactionTypes.CreditNote;
			Factory.Save();
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("CREDIT NOTE", ARInvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this credit note with your payment if paying by cheque", ARInvoiceWrapper.InvoiceFooterMessage);
		}

		public void TestTaxAdjustmentNoteTitle()
		{
			AccountingConfigurationRegistry.Instance.TaxAdjustmentNoteReversalTitle.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TAX ADJUSTMENT NOTE REVERSAL");

			Invoice = Factory.New<ARAdjustmentNote>();
			Invoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
			var line = ((InvoicingLineBase)InvoicingBase.Lines.AddNew());
			line.AL_AT = Factory.NewWithValidTestData(typeof(AccTaxRate)).PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			var wrappedInvoice = DocARInvoice.New((InvoicingBase)Invoice, Factory);
			Factory.Save();
			AssertEquals("TAX ADJUSTMENT NOTE", wrappedInvoice.DocumentTitle);
			AssertEquals("Please return a copy of this tax adjustment note with your payment if paying by cheque", wrappedInvoice.InvoiceFooterMessage);

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(Invoice);
			reversing.Reverse();
			var wrappedReversedInvoice = DocARInvoice.New((InvoicingBase)Invoice, Factory);
			var wrappedReversingInvoice = DocARInvoice.New((InvoicingBase)reversing.ReverseTransaction, Factory);
			Factory.Save();
			AssertEquals("TAX ADJUSTMENT NOTE", wrappedReversedInvoice.DocumentTitle);
			AssertEquals("TAX ADJUSTMENT NOTE REVERSAL", wrappedReversingInvoice.DocumentTitle);
		}

		public void TestNonTaxAdjustmentNoteTitle()
		{
			Invoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
			Factory.Save();
			AssertEquals("ADJUSTMENT NOTE", ARInvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this adjustment note with your payment if paying by cheque", ARInvoiceWrapper.InvoiceFooterMessage);
		}

		#endregion

		#region Invoice Fields

		public void TestAgePeriod()
		{
			ZInt agePeriod = new ZInt(1);
			Invoice.AH_AgePeriod = agePeriod;
			AssertEquals("AgePeriod", agePeriod, ARInvoiceWrapper.AgePeriod);
		}

		public void TestCashBasisGSTIndicator()
		{
			Invoice.AH_CashBasisGSTIndicator = ZBool.False;
			Assert("!CashBasisGSTIndicator", !ARInvoiceWrapper.CashBasisGSTIndicator);

			Invoice.AH_CashBasisGSTIndicator = ZBool.True;
			Assert("CashBasisGSTIndicator", InvoiceWrapper.CashBasisGSTIndicator);
		}

		public void TestInvoiceApproved()
		{
			Invoice.AH_InvoiceApproved = ZBool.False;
			Assert("!InvoiceApproved", !ARInvoiceWrapper.InvoiceApproved);

			Invoice.AH_InvoiceApproved = ZBool.True;
			Assert("InvoiceApproved", InvoiceWrapper.InvoiceApproved);
		}

		public void TestIsClearedInCashbook()
		{
			Invoice.AH_DateClearedInCashbook = ZDateTime.Empty;
			Assert("!IsClearedInCashbook", !ARInvoiceWrapper.IsClearedInCashbook);

			Invoice.AH_DateClearedInCashbook = ZDateTime.Today;
			Assert("IsClearedInCashbook", InvoiceWrapper.IsClearedInCashbook);
		}

		public void TestJobHeader()
		{
			AssertNull("JobHeader", ARInvoiceWrapper.JobHeader);

			var header = Factory.NewJobForTesting<JobHeader>();
			Invoice.AH_JH = header.PK;
			AssertNotNull("JobHeader", InvoiceWrapper.JobHeader);
			AssertEquals("JobHeader is of type DocJobHeader", typeof(DocJobHeader), InvoiceWrapper.JobHeader.GetType());
		}

		public void TestNotAllocated()
		{
			Invoice.AH_NotAllocated = ZBool.False;
			Assert("!NotAllocated", !ARInvoiceWrapper.NotAllocated);

			Invoice.AH_NotAllocated = ZBool.True;
			Assert("NotAllocated", InvoiceWrapper.NotAllocated);
		}

		public void TestPOST1()
		{
			Invoice.AH_POST1 = ZBool.False;
			Assert("!POST1", !ARInvoiceWrapper.POST1);

			Invoice.AH_POST1 = ZBool.True;
			Assert("POST1", ARInvoiceWrapper.POST1);
		}

		public void TestPOST2()
		{
			Invoice.AH_POST2 = ZBool.False;
			Assert("!POST2", !ARInvoiceWrapper.POST2);

			Invoice.AH_POST2 = ZBool.True;
			Assert("POST2", ARInvoiceWrapper.POST2);
		}

		public void TestPOST3()
		{
			Invoice.AH_POST3 = ZBool.False;
			Assert("!POST3", !ARInvoiceWrapper.POST3);

			Invoice.AH_POST3 = ZBool.True;
			Assert("POST3", ARInvoiceWrapper.POST3);
		}

		public void TestPOST4()
		{
			Invoice.AH_POST4 = ZBool.False;
			Assert("!POST4", !ARInvoiceWrapper.POST4);

			Invoice.AH_POST4 = ZBool.True;
			Assert("POST4", InvoiceWrapper.POST4);
		}

		public void TestPostedToEFT()
		{
			Invoice.AH_PostedToEFT = ZBool.False;
			Assert("!PostedToEFT", !ARInvoiceWrapper.PostedToEFT);

			Invoice.AH_PostedToEFT = ZBool.True;
			Assert("PostedToEFT", ARInvoiceWrapper.PostedToEFT);
		}

		public void TestPostPeriod()
		{
			ZInt postPeriod = new ZInt(1);
			Invoice.AH_PostPeriod = postPeriod;
			AssertEquals("PostPeriod", postPeriod, ARInvoiceWrapper.PostPeriod);
		}

		public void TestPostToGL()
		{
			Invoice.AH_PostToGL = "N";
			Assert("!PostToGL", !ARInvoiceWrapper.PostToGL);

			Invoice.AH_PostToGL = "Y";
			Assert("PostToGL", ARInvoiceWrapper.PostToGL);
			Invoice.AH_PostToGL = "M";
			Assert("PostToGL", !ARInvoiceWrapper.PostToGL);
		}

		public void TestInvoiceLinesWithOSValue()
		{
			var line1 = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = line1.PK;
			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_OSSellAmt = 1000m;
			charge.JR_OSSellExRate = 1m;

			InvoicingBase.Lines.Add(line1);
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("InvoiceLinesWithOSValue", false, aRInvoiceWrapper.InvoiceLinesWithOSValue);

			var line2 = Factory.New<ARInvoiceLine>();

			var charge2 = Factory.New<Charge>();
			charge2.JR_AL_ARLine = line2.PK;
			charge2.JR_RX_NKSellCurrency = "USD";
			charge2.JR_OSSellAmt = 1000m;
			charge2.JR_OSSellExRate = 1m;

			InvoicingBase.Lines.Add(line2);
			aRInvoiceWrapper = DocAPInvoice.New(InvoicingBase, Factory);
			AssertEquals("InvoiceLinesWithOSValue", true, aRInvoiceWrapper.InvoiceLinesWithOSValue);
		}

		public void TestBusinessRegistrationNo()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);

			OrgHeader testOrgHeader = Factory.NewWithValidTestData(typeof(OrgHeader)) as OrgHeader;

			OrgAddress testAddress1 = Factory.NewWithValidTestData(typeof(OrgAddress)) as OrgAddress;
			OrgAddress testAddress2 = Factory.NewWithValidTestData(typeof(OrgAddress)) as OrgAddress;
			OrgAddress testAddress3 = Factory.NewWithValidTestData(typeof(OrgAddress)) as OrgAddress;

			testOrgHeader.CustomsCodes.AddNew();
			testOrgHeader.CustomsCodes.AddNew();
			testOrgHeader.CustomsCodes.AddNew();

			testOrgHeader.CustomsCodes[0].OK_CodeType = OrgCusCode.CodeTypes.GovBusinessCode;
			testOrgHeader.CustomsCodes[0].OK_CustomsRegNo = "CHINA001";
			testOrgHeader.CustomsCodes[0].OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			testOrgHeader.CustomsCodes[0].OK_OA_PremisesAddress = testAddress1.PK;

			testOrgHeader.CustomsCodes[1].OK_CodeType = OrgCusCode.CodeTypes.GovBusinessCode;
			testOrgHeader.CustomsCodes[1].OK_CustomsRegNo = "AUS001";
			testOrgHeader.CustomsCodes[1].OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			testOrgHeader.CustomsCodes[1].OK_OA_PremisesAddress = testAddress2.PK;

			testOrgHeader.CustomsCodes[2].OK_CodeType = OrgCusCode.CodeTypes.TaxFileCode;
			testOrgHeader.CustomsCodes[2].OK_CustomsRegNo = "CHINATAX";
			testOrgHeader.CustomsCodes[2].OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			testOrgHeader.CustomsCodes[2].OK_OA_PremisesAddress = testAddress3.PK;

			Factory.Save();

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = testOrgHeader.PK;

			AssertEquals("Business Reg No for Current Branch", "CHINA001", ARInvoiceWrapper.BusinessRegNo);
		}

		public void TestTaxRegistrationNo()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);

			OrgHeader testOrgHeader = Factory.NewWithValidTestData(typeof(OrgHeader)) as OrgHeader;

			OrgAddress testAddress1 = Factory.NewWithValidTestData(typeof(OrgAddress)) as OrgAddress;
			OrgAddress testAddress2 = Factory.NewWithValidTestData(typeof(OrgAddress)) as OrgAddress;
			OrgAddress testAddress3 = Factory.NewWithValidTestData(typeof(OrgAddress)) as OrgAddress;

			testOrgHeader.CustomsCodes.AddNew();
			testOrgHeader.CustomsCodes.AddNew();
			testOrgHeader.CustomsCodes.AddNew();

			testOrgHeader.CustomsCodes[0].OK_CodeType = OrgCusCode.CodeTypes.GovBusinessCode;
			testOrgHeader.CustomsCodes[0].OK_CustomsRegNo = "CHINA001";
			testOrgHeader.CustomsCodes[0].OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			testOrgHeader.CustomsCodes[0].OK_OA_PremisesAddress = testAddress1.PK;

			testOrgHeader.CustomsCodes[1].OK_CodeType = OrgCusCode.CodeTypes.TaxFileCode;
			testOrgHeader.CustomsCodes[1].OK_CustomsRegNo = "AUS001";
			testOrgHeader.CustomsCodes[1].OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			testOrgHeader.CustomsCodes[1].OK_OA_PremisesAddress = testAddress2.PK;

			testOrgHeader.CustomsCodes[2].OK_CodeType = OrgCusCode.CodeTypes.TaxFileCode;
			testOrgHeader.CustomsCodes[2].OK_CustomsRegNo = "CHINATAX";
			testOrgHeader.CustomsCodes[2].OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			testOrgHeader.CustomsCodes[2].OK_OA_PremisesAddress = testAddress3.PK;

			Factory.Save();

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = testOrgHeader.PK;

			AssertEquals("Tax Reg No for Current Branch", "CHINATAX", ARInvoiceWrapper.TaxRegNo);
		}

		public void TestBusinessSeal()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);

			OrgHeader testOrgHeader = Factory.NewWithValidTestData(typeof(OrgHeader)) as OrgHeader;

			OrgAddress testAddress1 = testOrgHeader.Addresses.AddNew();
			OrgAddress testAddress2 = testOrgHeader.Addresses.AddNew();
			OrgAddress testAddress3 = testOrgHeader.Addresses.AddNew();
			OrgAddress testAddress4 = testOrgHeader.Addresses.AddNew();

			testAddress1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			testAddress2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			testAddress3.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Miscellaneous);
			testAddress4.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);

			testAddress1.OA_CompanyNameOverride = "AussieCOMPANY";
			testAddress2.OA_CompanyNameOverride = "CHINACOMPANY";
			testAddress3.OA_CompanyNameOverride = "USCompany";
			testAddress3.OA_CompanyNameOverride = "CHINACOMPANYNonDefault";

			testAddress1.OA_Address1 = "Address1";
			testAddress2.OA_Address1 = "Address2";
			testAddress3.OA_Address1 = "Address3";
			testAddress4.OA_Address1 = "Address4";

			testAddress1.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);
			testAddress2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Receivables);
			testAddress3.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Miscellaneous);
			testAddress4.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.Receivables);

			Factory.Save();

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = testOrgHeader.PK;
			AssertEquals("Business Seal", "CHINACOMPANY", ARInvoiceWrapper.BusinessSeal);
		}

		public void TestTransactionOrganizationPeruDNI()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Invoice.AH_OH = header.PK;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Invoice.Header.CustomsCodes.RemoveAll();
				AssertEquals(ZString.Empty, ARInvoiceWrapper.TransactionOrganizationPeruDNI);

				Invoice.Header.CustomsCodes.AddNew("DNI", "12345", "PE");
				AssertEquals(ZString.Empty, ARInvoiceWrapper.TransactionOrganizationPeruDNI);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Peru))
			{
				Invoice.Header.CustomsCodes.RemoveAll();
				AssertEquals(ZString.Empty, ARInvoiceWrapper.TransactionOrganizationPeruDNI);

				Invoice.Header.CustomsCodes.AddNew("DNI", "12345", "PE");
				AssertEquals("12345", ARInvoiceWrapper.TransactionOrganizationPeruDNI);
			}
		}

		#endregion

		#region Test for Job Invoicing Plugin Details

		public void TestARInvoiceWithShipmentStartingWithSLetter()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			shipment.JS_UniqueConsignRef = "S001884";

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

			GetInvoiceJob(shipment, Invoice);

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull(ARInvoiceWrapper.Shipment);
		}

		public void TestARInvoiceWithShipmentStartingWithNonSLetter()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			shipment.JS_UniqueConsignRef = "TESTNO";

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

			GetInvoiceJob(shipment, Invoice);

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNotNull(ARInvoiceWrapper.Shipment);
		}

		public Job GetInvoiceJob(IJobInvoicingPlugIn plugIn, TransactionHeader invoice)
		{
			var job = new Job.Loader(plugIn).TryCreateWithoutMutexForTestOnly();
			invoice.AH_JH = job.PK;
			return job;
		}

		protected Job GetInvoiceJob(IJobInvoicingPlugIn plugIn)
		{
			Job accountJob = new Job.Loader(plugIn).TryCreateWithoutMutexForTestOnly();
			accountJob.PlugInData = plugIn;
			return accountJob;
		}

		public void TestARInvoiceWithConsolidatedInvoiceSLetterButNoShipment()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			shipment.JS_UniqueConsignRef = "S001884";

			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertNull(ARInvoiceWrapper.Shipment);
		}

		public void TestARInvoiceWithLimitedContainers()
		{
			BaseJobDeclaration dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			dec.JE_DeclarationReference = "B00007777";

			BaseCusContainer container1 = dec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "C09999999901";
			BaseCusContainer container2 = dec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C09999999902";
			BaseCusContainer container3 = dec.CusContainers.AddNew();
			container3.CO_ContainerNumber = "C09999999903";
			BaseCusContainer container4 = dec.CusContainers.AddNew();
			container4.CO_ContainerNumber = "C09999999904";
			BaseCusContainer container5 = dec.CusContainers.AddNew();
			container5.CO_ContainerNumber = "C09999999905";

			ARInvoice ourInvoice = Factory.New<ARInvoice>();
			GetInvoiceJob(dec, ourInvoice);
			ourInvoice.AH_ConsolidatedInvoiceRef = dec.JE_DeclarationReference;
			Factory.Save();

			DocARInvoice invoiceRapper = DocARInvoice.New(ourInvoice, Factory);
			AssertEquals("The number of Containers displayed is NOT correct", 5, invoiceRapper.ContainerNumbersForDeclaration.Split(',').Length);

			BaseCusContainer container6 = dec.CusContainers.AddNew();
			container6.CO_ContainerNumber = "C09999999906";
			BaseCusContainer container7 = dec.CusContainers.AddNew();
			container7.CO_ContainerNumber = "C09999999907";
			BaseCusContainer container8 = dec.CusContainers.AddNew();
			container8.CO_ContainerNumber = "C09999999908";
			BaseCusContainer container9 = dec.CusContainers.AddNew();
			container9.CO_ContainerNumber = "C09999999909";
			BaseCusContainer container10 = dec.CusContainers.AddNew();
			container10.CO_ContainerNumber = "C09999999910";

			Factory.Save();

			AssertEquals("The number of Containers exceeds the maximum allowed", 7, invoiceRapper.ContainerNumbersForDeclaration.Split(',').Length);
		}

		#endregion

		#region Test for different modes

		public void TestAirMode()
		{
			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.Air);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;

			AssertLines();
		}

		public void TestSeaMode()
		{
			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.Sea);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;

			AssertLines();
		}

		public void TestRailMode()
		{
			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.Rail);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Rail;

			AssertLines();
		}

		public void TestRoadMode()
		{
			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.Road);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Road;

			AssertLines();
		}

		public void TestFCLMode()
		{
			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.FCL);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.FCL;

			AssertLines();
		}

		public void TestLCLMode()
		{
			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.LCL);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.LCL;

			AssertLines();
		}

		#endregion

		#region Test Sorting Strategies

		public void TestAlphabeticalSortingStrategy()
		{
			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.Air);
			SetupInvoiceLinesAndChargesWithPrintSequence();

			InvoicingLineBase line = AddLoadChargeToInvoice("LDC", 0);
			line.AL_OSExTaxAmount = 351.00M;
			line.AL_Sequence = 100;

			line = AddLoadChargeToInvoice("LDC", 0);
			line.AL_OSExTaxAmount = 352.00M;
			line.AL_Sequence = 99;

			line = AddInsuranceChargeToInvoice("INS", 0);
			line.AL_OSExTaxAmount = 353.00M;
			line.AL_Sequence = 88;

			SetUpOrganisationForSortingWithCode("ALP");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;

			Factory.Save();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 15, theLines.Count);

			int currentIndex = 0;
			AssertLine(theLines[currentIndex], "Brokerage", 300.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Comment charge", 0M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Customs Charge", 200.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Destination", 200.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Freight", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Insurance", 450.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Insurance", 353.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Loading", 350.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Loading", 352.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Loading", 351.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Not grouped charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Non job related charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Origin", 150.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Unloading", 400.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "GL Account Line", 100.00M);
			AssertEquals("Last element.", theLines.Count - 1, currentIndex);
		}

		public void TestSequenceSortingStrategy()
		{
			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.Air);
			SetupInvoiceLinesAndChargesWithPrintSequence();

			InvoicingLineBase line = AddLoadChargeToInvoice("LDC", 0);
			line.AL_OSExTaxAmount = 351.00M;
			line.AL_Sequence = 100;

			line = AddLoadChargeToInvoice("LDC", 0);
			line.AL_OSExTaxAmount = 352.00M;
			line.AL_Sequence = 99;

			line = AddInsuranceChargeToInvoice("INS", 0);
			line.AL_OSExTaxAmount = 353.00M;
			line.AL_Sequence = 88;

			SetUpOrganisationForSortingWithCode("SEQ");
			SetUpOrganisationForInvoiceOrder();
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;

			Factory.Save();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 15, theLines.Count);

			int currentIndex = 0;
			AssertLine(theLines[currentIndex], "Non job related charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Loading", 350.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Loading", 352.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Loading", 351.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Customs Charge", 200.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Unloading", 400.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Freight", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Destination", 200.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Origin", 150.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Brokerage", 300.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Insurance", 450.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Not grouped charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Comment charge", 0m);
			currentIndex++;
			AssertLine(theLines[currentIndex], "GL Account Line", 100.00m);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Insurance", 353.00M);
			AssertEquals("Last element.", theLines.Count - 1, currentIndex);
		}

		public void TestSequenceSortingStrategyWithTaxMessage()
		{
			AccTaxRate gstFree = GetNewUniqueBizo<AccTaxRate>();
			gstFree.SetRateNumerator_ForTestOnly(0);

			AccInvMsg msg1 = Factory.NewWithValidTestData<AccInvMsg>();
			msg1.A9_IsShownOnDocuments = true;
			msg1.A9_Description = "MSG1";
			msg1.A9_EnglishMsg = "English msg1";

			AccInvMsg msg2 = Factory.NewWithValidTestData<AccInvMsg>();
			msg2.A9_IsShownOnDocuments = true;
			msg2.A9_Description = "MSG2";
			msg2.A9_EnglishMsg = "English msg2";

			Factory.Save();

			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.Air);
			SetUpDepartmentAndDirection();

			InvoicingLineBase line = AddLoadChargeToInvoice("FR1", 3, gstFree);
			line.AL_OSExTaxAmount = 351.00M;
			line.AL_Sequence = 1;
			line.AL_Desc = "Freight 1";
			line.AL_A9_VATClass = msg1.PK;

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_LocalSellAmt = 351.00M;
			charge.JR_OSSellAmt = 351.00M;
			charge.JR_AT_SellGSTRate = gstFree.PK;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_A9_SellVATClass = msg1.PK;

			line = AddLoadChargeToInvoice("FR2", 2, gstFree);
			line.AL_OSExTaxAmount = 352.00M;
			line.AL_Sequence = 2;
			line.AL_Desc = "Freight 2";
			line.AL_A9_VATClass = msg2.PK;

			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_LocalSellAmt = 352.00M;
			charge.JR_OSSellAmt = 352.00M;
			charge.JR_AT_SellGSTRate = gstFree.PK;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_A9_SellVATClass = msg2.PK;

			line = AddLoadChargeToInvoice("FR3", 1, gstFree);
			line.AL_OSExTaxAmount = 353.00M;
			line.AL_Sequence = 3;
			line.AL_Desc = "Freight 3";
			line.AL_A9_VATClass = msg2.PK;

			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_LocalSellAmt = 353.00M;
			charge.JR_OSSellAmt = 353.00M;
			charge.JR_AT_SellGSTRate = gstFree.PK;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_A9_SellVATClass = msg2.PK;

			SetUpOrganisationForSortingWithCode("SEQ", false);

			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;

			Factory.Save();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertLine(theLines[0], "Freight 3", 353.00, " *");
			AssertLine(theLines[1], "Freight 2", 352.00, " *");
			AssertLine(theLines[2], "Freight 1", 351.00, " **");

			string expectedEnglishLanguageTaxMessage = @"* English msg2
** English msg1";
			AssertEquals(expectedEnglishLanguageTaxMessage, ARInvoiceWrapper.EnglishLanguageTaxMessages);
		}

		public void TestAlphabeticSortingStrategyWithTaxMessage()
		{
			AccTaxRate gstFree = GetNewUniqueBizo<AccTaxRate>();
			gstFree.SetRateNumerator_ForTestOnly(0);

			AccInvMsg msg1 = Factory.NewWithValidTestData<AccInvMsg>();
			msg1.A9_IsShownOnDocuments = true;
			msg1.A9_Description = "MSG1";
			msg1.A9_EnglishMsg = "English msg1";

			AccInvMsg msg2 = Factory.NewWithValidTestData<AccInvMsg>();
			msg2.A9_IsShownOnDocuments = true;
			msg2.A9_Description = "MSG2";
			msg2.A9_EnglishMsg = "English msg2";

			Factory.Save();

			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.Air);
			SetUpDepartmentAndDirection();

			InvoicingLineBase line = AddLoadChargeToInvoice("ZFR1", 3, gstFree);
			line.AL_OSExTaxAmount = 351.00M;
			line.AL_Sequence = 1;
			line.AL_Desc = "Freight 1";
			line.AL_A9_VATClass = msg1.PK;

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_LocalSellAmt = 351.00M;
			charge.JR_OSSellAmt = 351.00M;
			charge.JR_AT_SellGSTRate = gstFree.PK;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_A9_SellVATClass = msg1.PK;

			line = AddLoadChargeToInvoice("YFR2", 2, gstFree);
			line.AL_OSExTaxAmount = 352.00M;
			line.AL_Sequence = 2;
			line.AL_Desc = "Freight 2";
			line.AL_A9_VATClass = msg2.PK;

			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_LocalSellAmt = 352.00M;
			charge.JR_OSSellAmt = 352.00M;
			charge.JR_AT_SellGSTRate = gstFree.PK;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_A9_SellVATClass = msg2.PK;

			line = AddLoadChargeToInvoice("XFR3", 1, gstFree);
			line.AL_OSExTaxAmount = 353.00M;
			line.AL_Sequence = 3;
			line.AL_Desc = "Freight 3";
			line.AL_A9_VATClass = msg2.PK;

			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_LocalSellAmt = 353.00M;
			charge.JR_OSSellAmt = 353.00M;
			charge.JR_AT_SellGSTRate = gstFree.PK;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_A9_SellVATClass = msg2.PK;

			SetUpOrganisationForSortingWithCode("ALP", false);

			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;

			Factory.Save();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertLine(theLines[0], "Freight 3", 353.00, " *");
			AssertLine(theLines[1], "Freight 2", 352.00, " *");
			AssertLine(theLines[2], "Freight 1", 351.00, " **");

			string expectedEnglishLanguageTaxMessage = @"* English msg2
** English msg1";
			AssertEquals(expectedEnglishLanguageTaxMessage, ARInvoiceWrapper.EnglishLanguageTaxMessages);
		}

		public void TestSubTotalAndSequenceSortingStrategy()
		{
			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.Air);
			SetupInvoiceLinesAndManyChargesWithPrintSequence();
			SetUpOrganisationForSortingWithCode("SSQ");
			SetUpOrganisationForInvoiceOrder();
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("36 lines - 33 charges + 3 sub total + 3 spacer lines", 39, theLines.Count);

			int currentIndex = 0;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "LDC", "Loading", 350m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "ORG", "Origin", 150m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "8_LDC", "Loading", 350m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "2_ORG", "Origin", 150m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "1_ORG", "Origin", 150m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "7_LDC", "Loading", 350m, false, true, false);
			currentIndex++;
			AssertSubTotalLine(theLines[currentIndex], "Origin Charges", 1500m, false, false, true);
			currentIndex++;
			AssertSubTotalLine(theLines[currentIndex], "", 0m, true, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "FRC", "Freight", 250m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "2_FRC", "Freight", 250m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "1_FRC", "Freight", 250m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "INS", "Insurance", 450m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "7_INS", "Insurance", 450m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "4_INS", "Insurance", 450m, false, true, false);
			currentIndex++;
			AssertSubTotalLine(theLines[currentIndex], "Freight and Insurance Charges", 2100m, false, false, true);
			currentIndex++;
			AssertSubTotalLine(theLines[currentIndex], "", 0m, true, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "DST", "Destination", 200m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "ULC", "Unloading", 400m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "3_DST", "Destination", 200m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "2_DST", "Destination", 200m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "3_ULC", "Unloading", 400m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "6_ULC", "Unloading", 400m, false, true, false);
			currentIndex++;
			AssertSubTotalLine(theLines[currentIndex], "Destination Charges", 1800m, false, false, true);
			currentIndex++;
			AssertSubTotalLine(theLines[currentIndex], "", 0m, true, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "NJC", "Non job related charge", 250m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "2_NJC", "Non job related charge", 250m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "5_BRK", "Brokerage", 300m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "1_NJC", "Non job related charge", 250m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "CUS", "Customs Charge", 200m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "6_CUS", "Customs Charge", 200m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "4_CUS", "Customs Charge", 200m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "BRK", "Brokerage", 300m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "6_BRK", "Brokerage", 300m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "NGC", "Not grouped charge", 250m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "4_NGC", "Not grouped charge", 250m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "5_NGC", "Not grouped charge", 250m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "CMT", "Comment charge", 0m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "4_CMT", "Comment charge", 0m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "5_CMT", "Comment charge", 0m, false, false, false);
			AssertEquals("Last element.", theLines.Count - 1, currentIndex);
		}

		public void TestRollUpSortingStrategy()
		{
			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.Air);
			SetupInvoiceLinesAndManyChargesWithPrintSequence();
			SetUpOrganisationForSortingWithCode("ROL");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 18, theLines.Count);

			int currentIndex = 0;
			AssertLine(theLines[currentIndex], "Brokerage", 300.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Brokerage", 300.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Brokerage", 300.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Customs Charge", 200.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Customs Charge", 200.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Customs Charge", 200.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Not grouped charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Not grouped charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Not grouped charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Comment charge", 0M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Comment charge", 0M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Comment charge", 0M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Non job related charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Non job related charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Non job related charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Origin Charges", 1500.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Freight and Insurance Charges", 2100.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Destination Charges", 1800.00M);
			AssertEquals("Last element.", theLines.Count - 1, currentIndex);
		}

		public void TestRollUpAndSequenceWithCCDGroupingSortingStrategy()
		{
			var shipment = TestObjectCreator.CreateShipment("S10111");
			shipment.JS_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_ConsolidatedInvoiceRef = "S10111";
			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			orgHeader.OH_IsDebtor = true;
			orgHeader.OH_IsConsignor = false;
			orgHeader.OH_IsConsignee = false;
			orgHeader.OH_IsBroker = false;
			orgHeader.OH_IsForwarder = false;

			JobHeader job = TestObjectCreator.CreateJob(shipment, false, false);
			invoice.AH_JH = job.PK;
			job.LocalChargesPK = orgHeader.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			SetUpDepartmentAndDirection();

			if (invoice.IsInDatabase)
			{
				invoice.AH_FullyPaidDate = ZDateTime.Empty;
			}

			var chargeCodeA = TestObjectCreator.CreateChargeCode("ACC", "A Charge Code", ChargeCodeGroupList.Codes.Freight, 100M, TestObjectCreator.GST1, null);
			chargeCodeA.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeCodeB = TestObjectCreator.CreateChargeCode("BCC", "B Charge Code", ChargeCodeGroupList.Codes.Freight, 100M, TestObjectCreator.GST1, null);
			chargeCodeB.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeCodeC = TestObjectCreator.CreateChargeCode("CCC", "C Charge Code", ChargeCodeGroupList.Codes.Freight, 100M, TestObjectCreator.GST1, null);
			chargeCodeC.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeCodeD = TestObjectCreator.CreateChargeCode("DCC", "D Charge Code", ChargeCodeGroupList.Codes.Freight, 100M, TestObjectCreator.GST1, null);
			chargeCodeD.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeCodeE = TestObjectCreator.CreateChargeCode("DAC", "E Charge Code", ChargeCodeGroupList.Codes.Freight, 100M, TestObjectCreator.GST1, null);
			chargeCodeE.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCodeA.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_OSExTaxAmount = 10M;

			line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCodeA.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_OSExTaxAmount = 20M;

			line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCodeB.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_OSExTaxAmount = 30M;

			line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCodeC.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_OSExTaxAmount = 40M;

			line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCodeE.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_OSExTaxAmount = 50M;

			line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCodeD.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_OSExTaxAmount = 60M;

			invoice.AH_OH = orgHeader.PK;

			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = orgHeader.CompanyData.InvoiceRollupOrGroups.AddNew();

			AccClientInvoiceOrder invoiceOrder = orgHeader.InvoiceOrders.AddNew();
			invoiceOrder.AI_AC = chargeCodeC.PK;
			invoiceOrder.AI_InvoiceType = "ALL";
			invoiceOrder.AI_PrintOrder = 1;

			invoiceOrder = orgHeader.InvoiceOrders.AddNew();
			invoiceOrder.AI_AC = chargeCodeB.PK;
			invoiceOrder.AI_InvoiceType = "ALL";
			invoiceOrder.AI_PrintOrder = 2;

			invoiceOrder = orgHeader.InvoiceOrders.AddNew();
			invoiceOrder.AI_AC = chargeCodeA.PK;
			invoiceOrder.AI_InvoiceType = "ALL";
			invoiceOrder.AI_PrintOrder = 3;

			Factory.Save();

			OrgInvoiceRollupOrGroup rollupOrGroup = invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			rollupOrGroup.PG_GroupOrSubtotalStyle = "CCD";
			rollupOrGroup.PG_GroupOrSubTotal = "RSQ";
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceRollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(invoice, (inv, factory) => DocARInvoice.New(inv, factory));

			DocARInvoiceLineCollection lines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 5, lines.Count);

			AssertEquals("Charge Code C is the first line as per PrintOrder sequence", chargeCodeC.AC_Desc, lines[0].ChargeCode.Desc);
			AssertEquals("Charge Code B is the second line as per PrintOrder sequence", chargeCodeB.AC_Desc, lines[1].ChargeCode.Desc);
			AssertEquals("Charge Code A is the third line as per PrintOrder sequence", chargeCodeA.AC_Desc, lines[2].ChargeCode.Desc);
			AssertEquals("Charge Code A lines are grouped together", 30M, lines[2].LineAmount);
			AssertEquals("Charge Code D is next as per Description - not per charge code", chargeCodeD.AC_Desc, lines[3].ChargeCode.Desc);
			AssertEquals("Charge Code E follows as per Description - not per charge code", chargeCodeE.AC_Desc, lines[4].ChargeCode.Desc);
		}

		public void TestRollUpWithCLCGroupingSortingStrategy()
		{
			var shipment = TestObjectCreator.CreateShipment("S10111");
			shipment.JS_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_ConsolidatedInvoiceRef = "S10111";
			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			orgHeader.OH_IsDebtor = true;
			orgHeader.OH_IsConsignor = false;
			orgHeader.OH_IsConsignee = false;
			orgHeader.OH_IsBroker = false;
			orgHeader.OH_IsForwarder = false;

			var job = TestObjectCreator.CreateJob(shipment, false, false);
			invoice.AH_JH = job.PK;
			job.LocalChargesPK = orgHeader.PK;

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);

			Enterprise.Accounting.Business.JobInvoicing.ExchangeRate rateUSD = Factory.New<Enterprise.Accounting.Business.JobInvoicing.ExchangeRate>();
			rateUSD.JF_RX_NKRateCurrency = "USD";
			rateUSD.JF_BaseRate = 7.1M;
			rateUSD.JF_JH = job.PK;

			SetUpDepartmentAndDirection();

			var chargeCodeA = TestObjectCreator.CreateChargeCode("ACC", "A Charge Code", ChargeCodeGroupList.Codes.Freight, 100M, TestObjectCreator.GST1, null);
			chargeCodeA.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeCodeB = TestObjectCreator.CreateChargeCode("BCC", "B Charge Code", ChargeCodeGroupList.Codes.Freight, 100M, TestObjectCreator.GST1, null);
			chargeCodeB.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeCodeC = TestObjectCreator.CreateChargeCode("CCC", "C Charge Code", ChargeCodeGroupList.Codes.Freight, 100M, TestObjectCreator.GST1, null);
			chargeCodeC.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			invoice.AH_OH = orgHeader.PK;

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCodeA.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_OSExTaxAmount = 45.00M;
			line.AL_Sequence = 4;
			TestObjectCreator.CreateCharge(line, job, chargeCodeA, line.TransactionCurrency);

			line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCodeA.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_OSExTaxAmount = 23.00M;
			line.AL_Sequence = 2;
			TestObjectCreator.CreateCharge(line, job, chargeCodeA, line.TransactionCurrency);

			line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCodeB.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_OSExTaxAmount = 89M;
			line.AL_Sequence = 8;
			line.AL_Desc = "B Desc";
			TestObjectCreator.CreateCharge(line, job, chargeCodeB, line.TransactionCurrency);

			line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCodeC.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_OSExTaxAmount = 67.00M;
			line.AL_Sequence = 6;
			TestObjectCreator.CreateCharge(line, job, chargeCodeC, line.TransactionCurrency);

			line = TestObjectCreator.CreateInvoiceLine(invoice, job, chargeCodeA, 56.00M, TestObjectCreator.USD, 0.71m);
			TestObjectCreator.CreateCharge(line);
			line.AL_Sequence = 5;

			line = TestObjectCreator.CreateInvoiceLine(invoice, job, chargeCodeA, 10.00M, TestObjectCreator.USD, 0.71m);
			TestObjectCreator.CreateCharge(line);

			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = orgHeader.CompanyData.InvoiceRollupOrGroups.AddNew();

			Factory.Save();

			var rollupOrGroup = invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			rollupOrGroup.PG_GroupOrSubtotalStyle = "CLC";
			rollupOrGroup.PG_GroupOrSubTotal = "ROL";
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceRollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(invoice, (inv, factory) => DocARInvoice.New(inv, factory));

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 4, theLines.Count);

			int currentIndex = 0;
			AssertLine(theLines[currentIndex], "A Charge Code", 68.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "A Charge Code", 92.95M);
			AssertEquals("ChargeOSAmountForCLC", 66.00M, theLines[currentIndex].ChargeOSAmountForCLC);
			AssertEquals("ChargeCurrency", "USD", theLines[currentIndex].ChargeCurrency);
			AssertEquals("ChargeExchangeRate", 0.71M, theLines[currentIndex].ChargeExchangeRate);
			currentIndex++;
			AssertLine(theLines[currentIndex], "C Charge Code", 67.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "B Charge Code", 89.00M);
		}

		public void TestRollUpAndSequenceSortingStrategy()
		{
			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.Air);
			SetupInvoiceLinesAndManyChargesWithPrintSequence();

			InvoicingLineBase line = AddLoadChargeToInvoice("LDC", 0);
			line.AL_OSExTaxAmount = 351.00M;
			line.AL_Sequence = 100;

			line = AddLoadChargeToInvoice("LDC", 0);
			line.AL_OSExTaxAmount = 352.00M;
			line.AL_Sequence = 99;

			line = AddInsuranceChargeToInvoice("INS", 0);
			line.AL_OSExTaxAmount = 353.00M;
			line.AL_Sequence = 88;

			SetUpOrganisationForSortingWithCode("RSQ");
			SetUpOrganisationForInvoiceOrder();
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 18, theLines.Count);

			int currentIndex = 0;
			AssertLine(theLines[currentIndex], "Destination Charges", 1800.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Freight and Insurance Charges", 2453.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Origin Charges", 2203.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Non job related charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Non job related charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Brokerage", 300.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Non job related charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Customs Charge", 200.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Customs Charge", 200.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Customs Charge", 200.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Brokerage", 300.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Brokerage", 300.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Not grouped charge", 250.00m);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Not grouped charge", 250.00m);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Not grouped charge", 250.00m);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Comment charge", 0m);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Comment charge", 0m);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Comment charge", 0m);
			AssertEquals("Last element.", theLines.Count - 1, currentIndex);
		}

		public void TestSubTotalSortingStrategy()
		{
			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.Air);
			SetupInvoiceLinesAndManyChargesWithPrintSequence();
			SetUpOrganisationForSortingWithCode("SUB");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;

			Factory.Save();

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;

			AssertEquals("36 lines - 33 charges + 3 sub total + 3 spacer lines", 39, theLines.Count);

			int currentIndex = 0;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "1_ORG", "Origin", 150m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "2_ORG", "Origin", 150m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "7_LDC", "Loading", 350m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "8_LDC", "Loading", 350m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "LDC", "Loading", 350m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "ORG", "Origin", 150m, false, true, false);
			currentIndex++;
			AssertSubTotalLine(theLines[currentIndex], "Origin Charges", 1500m, false, false, true);
			currentIndex++;
			AssertSubTotalLine(theLines[currentIndex], "", 0m, true, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "1_FRC", "Freight", 250m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "2_FRC", "Freight", 250m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "4_INS", "Insurance", 450m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "7_INS", "Insurance", 450m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "FRC", "Freight", 250m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "INS", "Insurance", 450m, false, true, false);
			currentIndex++;
			AssertSubTotalLine(theLines[currentIndex], "Freight and Insurance Charges", 2100m, false, false, true);
			currentIndex++;
			AssertSubTotalLine(theLines[currentIndex], "", 0m, true, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "2_DST", "Destination", 200m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "3_DST", "Destination", 200m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "3_ULC", "Unloading", 400m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "6_ULC", "Unloading", 400m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "DST", "Destination", 200m, false, true, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "ULC", "Unloading", 400m, false, true, false);
			currentIndex++;
			AssertSubTotalLine(theLines[currentIndex], "Destination Charges", 1800m, false, false, true);
			currentIndex++;
			AssertSubTotalLine(theLines[currentIndex], "", 0m, true, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "1_NJC", "Non job related charge", 250m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "2_NJC", "Non job related charge", 250m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "4_CMT", "Comment charge", 0m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "4_CUS", "Customs Charge", 200m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "4_NGC", "Not grouped charge", 250m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "5_BRK", "Brokerage", 300m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "5_CMT", "Comment charge", 0m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "5_NGC", "Not grouped charge", 250m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "6_BRK", "Brokerage", 300m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "6_CUS", "Customs Charge", 200m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "BRK", "Brokerage", 300m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "CMT", "Comment charge", 0m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "CUS", "Customs Charge", 200m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "NGC", "Not grouped charge", 250m, false, false, false);
			currentIndex++;
			AssertSubTotalLineWithChargeCode(theLines[currentIndex], "NJC", "Non job related charge", 250m, false, false, false);
			AssertEquals("Last element.", theLines.Count - 1, currentIndex);
		}

		public void TestUserSortingStrategyForLinesWithGLAccount()
		{
			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.Air);
			SetUpDepartmentAndDirection();
			InvoicingLineBase line = AddGLAccountLineToInvoice("GL 1", "GL Account 1");
			line.AL_Sequence = 6;
			line = AddGLAccountLineToInvoice("GL 2", "GL Account 2");
			line.AL_Sequence = 6;
			SetUpOrganisationForSortingWithCode("USR");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			Factory.Save();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 2, theLines.Count);
		}

		public void TestUserSortingStrategy()
		{
			SetUpInvoiceWrapperForRollUp(OrgConstants.ModesForGroupOrSubTotal.Codes.Air);
			SetupInvoiceLinesAndChargesWithPrintSequence();

			InvoicingLineBase line = AddLoadChargeToInvoice("LDC", 0);
			line.AL_OSExTaxAmount = 351.00M;
			line.AL_Sequence = 100;

			line = AddLoadChargeToInvoice("LDC", 0);
			line.AL_OSExTaxAmount = 352.00M;
			line.AL_Sequence = 0;

			line = AddDestinationChargeToInvoice("DST", 0);
			line.AL_OSExTaxAmount = 452.00M;
			line.AL_Sequence = 0;

			line = AddInsuranceChargeToInvoice("INS", 0);
			line.AL_OSExTaxAmount = 353.00M;
			line.AL_Sequence = 88;

			SetUpOrganisationForSortingWithCode("USR");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;

			Factory.Save();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 16, theLines.Count);

			int currentIndex = 0;
			AssertLine(theLines[currentIndex], "Destination", 200.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Freight", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Brokerage", 300.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Loading", 350.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Unloading", 400.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Insurance", 450.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Customs Charge", 200.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Not grouped charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Comment charge", 0M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Non job related charge", 250.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "GL Account Line", 100m);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Insurance", 353.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Loading", 351.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Destination", 452.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Loading", 352.00M);
			currentIndex++;
			AssertLine(theLines[currentIndex], "Origin", 150.00M);
			AssertEquals("Last element.", theLines.Count - 1, currentIndex);
		}

		void AssertSubTotalLineWithChargeCode(IDocARInvoiceLine line, ZString code, ZString description, ZDecimal oSExTaxAmount, bool isSpacerLine, bool hasBeenSubTotalled, bool isSubTotalLine)
		{
			AssertEquals("LineChargeCode", code, line.ChargeCode.Code);
			AssertEquals("LineDescription", description, line.LineDescription);
			AssertEquals("OSExTaxAmount", oSExTaxAmount, line.OSExTaxAmount);
			AssertEquals("IsSpacerLine", isSpacerLine, line.IsSpacerLine);
			AssertEquals("HasBeenSubTotalled", hasBeenSubTotalled, line.HasBeenSubTotalled);
			AssertEquals("IsSubTotalLine", isSubTotalLine, line.IsSubTotalLine);
		}

		void AssertLine(IDocARInvoiceLine line, ZString description, ZDecimal oSExTaxAmount)
		{
			AssertEquals("LineDescription", description, line.LineDescription);
			AssertEquals("OSExTaxAmount", oSExTaxAmount, line.OSExTaxAmount);
		}

		void AssertLine(IDocARInvoiceLine line, ZString description, ZDecimal oSExTaxAmount, string taxRateAsterisks)
		{
			AssertLine(line, description, oSExTaxAmount);
			AssertEquals(taxRateAsterisks, line.TaxRateAsterisks);
		}

		#endregion

		#region CreditNote Amounts with AmountMultiplier

		public void TestCreditNoteAmountsWithAmountMultiplier()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			{
				Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
				Invoice = Factory.New<ARCreditNote>();
				Invoice.AH_TransactionType = TransactionTypes.CreditNote;
				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Type = AccTaxRate.Types.Rated;
				taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				taxRate.SetRate_ForTestOnly(3333, 100);
				Factory.Save();
				InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
				DocARInvoice creditNoteWrapper = (DocARInvoice)InvoiceWrapper;

				AssertEquals("Credit note amount", 0M, InvoiceWrapper.InvoiceAmountWithGST);
				AssertEquals("Credit note balance", 0M, InvoiceWrapper.Balance);

				InvoicingLineBase crdLine = (InvoicingLineBase)((InvoicingBase)Invoice).Lines.AddNew();
				Invoice.AH_InvoiceDate = ZDateTime.Now;
				Invoice.AH_PostDate = ZDateTime.Now;
				Invoice.AH_RX_NKTransactionCurrency = "GBP";
				Invoice.AH_ExchangeRate = 2M;
				crdLine.AL_AT = taxRate.PK;
				crdLine.AL_OSTaxAmount = 50M;
				crdLine.AL_OSExTaxAmount = 150M;
				crdLine.AL_OSExtraTaxAmount = 9M;
				Invoice.AH_OSTaxAmount = 50M;
				Invoice.AH_OSExTaxAmount = 150M;
				Invoice.AH_LocalOutstandingAmount = 100M;
				Invoice.AH_DueDate = ZDateTime.Today;
				Invoice.AH_FullyPaidDate = ZDateTime.Empty;

				AssertEquals("Credit note due date", ZDateTime.Today, InvoiceWrapper.TransactionDueDate);

				AssertEquals("Pre-condition: RegistryItem is False by default", false, AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.Value);

				AssertEquals("OSOutstandingAmount", 200M, creditNoteWrapper.OSOutstandingAmount);
				AssertEquals("OSOutstandingAmount", "-200.00", creditNoteWrapper.OSOutstandingAmountFormatted);
				AssertEquals("OSTotal", 200M, creditNoteWrapper.OSTotal);
				AssertEquals("OSTotalFormatted", "200.00", creditNoteWrapper.OSTotalFormatted);
				AssertEquals("OSTotalFormattedWithSign", "-200.00", creditNoteWrapper.OSTotalFormattedWithSign);
				AssertEquals("LocalTotal", 100M, creditNoteWrapper.LocalTotal);
				AssertEquals("LocalTotalFormatted", "100.00", creditNoteWrapper.LocalTotalFormatted);
				AssertEquals("LocalTotalFormattedWithSign", "-100.00", creditNoteWrapper.LocalTotalFormattedWithSign);
				AssertEquals("TotalOSTaxAmount", 50M, creditNoteWrapper.TotalOSTaxAmount);
				AssertEquals("TotalOSTaxAmountFormatted", "50.00", creditNoteWrapper.TotalOSTaxAmountFormatted);
				AssertEquals("TotalOSTaxAmountFormattedWithSign", "-50.00", creditNoteWrapper.TotalOSTaxAmountFormattedWithSign);
				AssertEquals("InvoiceSubTotal", 150M, creditNoteWrapper.InvoiceSubTotal);
				AssertEquals("InvoiceSubTotalFormatted", "150.00", creditNoteWrapper.InvoiceSubTotalFormatted);
				AssertEquals("InvoiceSubTotalFormattedWithSign", "-150.00", creditNoteWrapper.InvoiceSubTotalFormattedWithSign);
				AssertEquals("InvoiceLocalSubTotal", 75M, creditNoteWrapper.InvoiceLocalSubTotal);
				AssertEquals("InvoiceLocalSubTotalFormatted", "75.00", creditNoteWrapper.InvoiceLocalSubTotalFormatted);
				AssertEquals("InvoiceLocalSubTotalFormattedWithSign", "-75.00", creditNoteWrapper.InvoiceLocalSubTotalFormattedWithSign);
				AssertEquals("TotalOSQSTAmount", 0M, creditNoteWrapper.TotalOSQSTAmount);
				AssertEquals("TotalOSQSTAmountFormatted", "0.00", creditNoteWrapper.TotalOSQSTAmountFormatted);
				AssertEquals("TotalOSQSTAmountFormattedWithSign", "0.00", creditNoteWrapper.TotalOSQSTAmountFormattedWithSign);
				AssertEquals("TotalOSEDUAmount", 9M, creditNoteWrapper.TotalOSEDUAmount);
				AssertEquals("TotalOSEDUAmountFormatted", "9.00", creditNoteWrapper.TotalOSEDUAmountFormatted);
				AssertEquals("TotalOSEDUAmountFormattedWithSign", "-9.00", creditNoteWrapper.TotalOSEDUAmountFormattedWithSign);
				AssertEquals("TotalOSEDUPrimaryAmount", 6M, creditNoteWrapper.TotalOSEDUPrimaryAmount);
				AssertEquals("TotalOSEDUPrimaryAmountFormatted", "6.00", creditNoteWrapper.TotalOSEDUPrimaryAmountFormatted);
				AssertEquals("TotalOSEDUPrimaryAmountFormattedWithSign", "-6.00", creditNoteWrapper.TotalOSEDUPrimaryAmountFormattedWithSign);
				AssertEquals("TotalOSEDUSecondaryAmount", 3M, creditNoteWrapper.TotalOSEDUSecondaryAmount);
				AssertEquals("TotalOSEDUSecondaryAmountFormatted", "3.00", creditNoteWrapper.TotalOSEDUSecondaryAmountFormatted);
				AssertEquals("TotalOSEDUSecondaryAmountFormattedWithSign", "-3.00", creditNoteWrapper.TotalOSEDUSecondaryAmountFormattedWithSign);
				AssertEquals("TotalOSRETAmount", -9M, creditNoteWrapper.TotalOSRETAmount);
				AssertEquals("TotalOSRETAmountFormatted", "-9.00", creditNoteWrapper.TotalOSRETAmountFormatted);
				AssertEquals("TotalOSRETAmountFormattedWithSign", "9.00", creditNoteWrapper.TotalOSRETAmountFormattedWithSign);

				using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
				{
					AssertEquals("OSOutstandingAmount", "-200,00", creditNoteWrapper.OSOutstandingAmountFormatted);
					AssertEquals("OSTotalFormatted", "200,00", creditNoteWrapper.OSTotalFormatted);
					AssertEquals("LocalTotalFormatted", "100,00", creditNoteWrapper.LocalTotalFormatted);
					AssertEquals("TotalOSTaxAmountFormatted", "50,00", creditNoteWrapper.TotalOSTaxAmountFormatted);
					AssertEquals("InvoiceSubTotalFormatted", "150,00", creditNoteWrapper.InvoiceSubTotalFormatted);
					AssertEquals("InvoiceLocalSubTotalFormatted", "75,00", creditNoteWrapper.InvoiceLocalSubTotalFormatted);
					AssertEquals("TotalOSQSTAmountFormatted", "0,00", creditNoteWrapper.TotalOSQSTAmountFormatted);
					AssertEquals("TotalOSEDUAmountFormatted", "9,00", creditNoteWrapper.TotalOSEDUAmountFormatted);
					AssertEquals("TotalOSEDUPrimaryAmountFormatted", "6,00", creditNoteWrapper.TotalOSEDUPrimaryAmountFormatted);
					AssertEquals("TotalOSEDUSecondaryAmountFormatted", "3,00", creditNoteWrapper.TotalOSEDUSecondaryAmountFormatted);
					AssertEquals("TotalOSRETAmountFormatted", "-9,00", creditNoteWrapper.TotalOSRETAmountFormatted);
				}

				AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					AssertEquals("OSOutstandingAmount should be with opposite sign", -200M, creditNoteWrapper.OSOutstandingAmount);
					AssertEquals("OSOutstandingAmount", "200.00", creditNoteWrapper.OSOutstandingAmountFormatted);
					AssertEquals("OSTotal should be with opposite sign", -200M, creditNoteWrapper.OSTotal);
					AssertEquals("OSTotalFormatted", "-200.00", creditNoteWrapper.OSTotalFormatted);
					AssertEquals("OSTotalFormattedWithSign", "-200.00", creditNoteWrapper.OSTotalFormattedWithSign);
					AssertEquals("LocalTotal", -100M, creditNoteWrapper.LocalTotal);
					AssertEquals("LocalTotalFormatted", "-100.00", creditNoteWrapper.LocalTotalFormatted);
					AssertEquals("LocalTotalFormattedWithSign", "-100.00", creditNoteWrapper.LocalTotalFormattedWithSign);
					AssertEquals("TotalOSTaxAmount", -50M, creditNoteWrapper.TotalOSTaxAmount);
					AssertEquals("TotalOSTaxAmountFormatted", "-50.00", creditNoteWrapper.TotalOSTaxAmountFormatted);
					AssertEquals("TotalOSTaxAmountFormattedWithSign", "-50.00", creditNoteWrapper.TotalOSTaxAmountFormattedWithSign);
					AssertEquals("InvoiceSubTotal", -150M, creditNoteWrapper.InvoiceSubTotal);
					AssertEquals("InvoiceSubTotalFormatted", "-150.00", creditNoteWrapper.InvoiceSubTotalFormatted);
					AssertEquals("InvoiceSubTotalFormattedWithSign", "-150.00", creditNoteWrapper.InvoiceSubTotalFormattedWithSign);
					AssertEquals("InvoiceLocalSubTotal", -75M, creditNoteWrapper.InvoiceLocalSubTotal);
					AssertEquals("InvoiceLocalSubTotalFormatted", "-75.00", creditNoteWrapper.InvoiceLocalSubTotalFormatted);
					AssertEquals("InvoiceLocalSubTotalFormattedWithSign", "-75.00", creditNoteWrapper.InvoiceLocalSubTotalFormattedWithSign);
					AssertEquals("TotalOSQSTAmount", 0M, creditNoteWrapper.TotalOSQSTAmount);
					AssertEquals("TotalOSQSTAmountFormatted", "0.00", creditNoteWrapper.TotalOSQSTAmountFormatted);
					AssertEquals("TotalOSQSTAmountFormattedWithSign", "0.00", creditNoteWrapper.TotalOSQSTAmountFormattedWithSign);
					AssertEquals("TotalOSEDUAmount", -9M, creditNoteWrapper.TotalOSEDUAmount);
					AssertEquals("TotalOSEDUAmountFormatted", "-9.00", creditNoteWrapper.TotalOSEDUAmountFormatted);
					AssertEquals("TotalOSEDUAmountFormattedWithSign", "-9.00", creditNoteWrapper.TotalOSEDUAmountFormattedWithSign);
					AssertEquals("TotalOSEDUPrimaryAmount", -6M, creditNoteWrapper.TotalOSEDUPrimaryAmount);
					AssertEquals("TotalOSEDUPrimaryAmountFormatted", "-6.00", creditNoteWrapper.TotalOSEDUPrimaryAmountFormatted);
					AssertEquals("TotalOSEDUPrimaryAmountFormattedWithSign", "-6.00", creditNoteWrapper.TotalOSEDUPrimaryAmountFormattedWithSign);
					AssertEquals("TotalOSEDUSecondaryAmount", -3M, creditNoteWrapper.TotalOSEDUSecondaryAmount);
					AssertEquals("TotalOSEDUSecondaryAmountFormatted", "-3.00", creditNoteWrapper.TotalOSEDUSecondaryAmountFormatted);
					AssertEquals("TotalOSEDUSecondaryAmountFormattedWithSign", "-3.00", creditNoteWrapper.TotalOSEDUSecondaryAmountFormattedWithSign);
					AssertEquals("TotalOSRETAmount", 9M, creditNoteWrapper.TotalOSRETAmount);
					AssertEquals("TotalOSRETAmountFormatted", "9.00", creditNoteWrapper.TotalOSRETAmountFormatted);
					AssertEquals("TotalOSRETAmountFormattedWithSign", "9.00", creditNoteWrapper.TotalOSRETAmountFormattedWithSign);
					AssertEquals("OSTotalExcludeSPVAmountFormattedIgnoreRegistry", "-200.00", creditNoteWrapper.OSTotalExcludeSPVAmountFormattedWithSign);
				}
				else
				{
					AssertEquals("OSOutstandingAmount", 200M, creditNoteWrapper.OSOutstandingAmount);
					AssertEquals("OSOutstandingAmount", "-200.00", creditNoteWrapper.OSOutstandingAmountFormatted);
					AssertEquals("OSTotal", 200M, creditNoteWrapper.OSTotal);
					AssertEquals("OSTotalFormatted", "200.00", creditNoteWrapper.OSTotalFormatted);
					AssertEquals("OSTotalFormattedWithSign", "200.00", creditNoteWrapper.OSTotalFormattedWithSign);
					AssertEquals("LocalTotal", 100M, creditNoteWrapper.LocalTotal);
					AssertEquals("LocalTotalFormatted", "100.00", creditNoteWrapper.LocalTotalFormatted);
					AssertEquals("LocalTotalFormattedWithSign", "100.00", creditNoteWrapper.LocalTotalFormattedWithSign);
					AssertEquals("TotalOSTaxAmount", 50M, creditNoteWrapper.TotalOSTaxAmount);
					AssertEquals("TotalOSTaxAmountFormatted", "50.00", creditNoteWrapper.TotalOSTaxAmountFormatted);
					AssertEquals("TotalOSTaxAmountFormattedWithSign", "50.00", creditNoteWrapper.TotalOSTaxAmountFormattedWithSign);
					AssertEquals("InvoiceSubTotal", 150M, creditNoteWrapper.InvoiceSubTotal);
					AssertEquals("InvoiceSubTotalFormatted", "150.00", creditNoteWrapper.InvoiceSubTotalFormatted);
					AssertEquals("InvoiceSubTotalFormattedWithSign", "150.00", creditNoteWrapper.InvoiceSubTotalFormattedWithSign);
					AssertEquals("InvoiceLocalSubTotal", 75M, creditNoteWrapper.InvoiceLocalSubTotal);
					AssertEquals("InvoiceLocalSubTotalFormatted", "75.00", creditNoteWrapper.InvoiceLocalSubTotalFormatted);
					AssertEquals("InvoiceLocalSubTotalFormattedWithSign", "75.00", creditNoteWrapper.InvoiceLocalSubTotalFormattedWithSign);
					AssertEquals("TotalOSQSTAmount", 0M, creditNoteWrapper.TotalOSQSTAmount);
					AssertEquals("TotalOSQSTAmountFormatted", "0.00", creditNoteWrapper.TotalOSQSTAmountFormatted);
					AssertEquals("TotalOSQSTAmountFormattedWithSign", "0.00", creditNoteWrapper.TotalOSQSTAmountFormattedWithSign);
					AssertEquals("TotalOSEDUAmount", 9M, creditNoteWrapper.TotalOSEDUAmount);
					AssertEquals("TotalOSEDUAmountFormatted", "9.00", creditNoteWrapper.TotalOSEDUAmountFormatted);
					AssertEquals("TotalOSEDUAmountFormattedWithSign", "9.00", creditNoteWrapper.TotalOSEDUAmountFormattedWithSign);
					AssertEquals("TotalOSEDUPrimaryAmount", 6M, creditNoteWrapper.TotalOSEDUPrimaryAmount);
					AssertEquals("TotalOSEDUPrimaryAmountFormatted", "6.00", creditNoteWrapper.TotalOSEDUPrimaryAmountFormatted);
					AssertEquals("TotalOSEDUPrimaryAmountFormattedWithSign", "6.00", creditNoteWrapper.TotalOSEDUPrimaryAmountFormattedWithSign);
					AssertEquals("TotalOSEDUSecondaryAmount", 3M, creditNoteWrapper.TotalOSEDUSecondaryAmount);
					AssertEquals("TotalOSEDUSecondaryAmountFormatted", "3.00", creditNoteWrapper.TotalOSEDUSecondaryAmountFormatted);
					AssertEquals("TotalOSEDUSecondaryAmountFormattedWithSign", "3.00", creditNoteWrapper.TotalOSEDUSecondaryAmountFormattedWithSign);
					AssertEquals("TotalOSRETAmount", -9M, creditNoteWrapper.TotalOSRETAmount);
					AssertEquals("TotalOSRETAmountFormatted", "-9.00", creditNoteWrapper.TotalOSRETAmountFormatted);
					AssertEquals("TotalOSRETAmountFormattedWithSign", "-9.00", creditNoteWrapper.TotalOSRETAmountFormattedWithSign);
					AssertEquals("OSTotalExcludeSPVAmountFormattedIgnoreRegistry", "-200.00", creditNoteWrapper.OSTotalExcludeSPVAmountFormattedWithSign);
				}
			}
		}

		public void TestCreditNoteAmountsWithAmountMultiplier_SER()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			{
				Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
				Invoice = Factory.New<ARCreditNote>();
				Invoice.AH_TransactionType = TransactionTypes.CreditNote;
				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Type = AccTaxRate.Types.ServiceTax;
				taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				taxRate.SetRate_ForTestOnly(3333, 100);
				Factory.Save();
				InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
				DocARInvoice creditNoteWrapper = (DocARInvoice)InvoiceWrapper;

				AssertEquals("Credit note amount", 0M, InvoiceWrapper.InvoiceAmountWithGST);
				AssertEquals("Credit note balance", 0M, InvoiceWrapper.Balance);

				InvoicingLineBase crdLine = (InvoicingLineBase)((InvoicingBase)Invoice).Lines.AddNew();
				Invoice.AH_InvoiceDate = ZDateTime.Now;
				Invoice.AH_PostDate = ZDateTime.Now;
				Invoice.AH_RX_NKTransactionCurrency = "GBP";
				Invoice.AH_ExchangeRate = 2M;
				crdLine.AL_AT = taxRate.PK;
				crdLine.AL_OSTaxAmount = 50M;
				crdLine.AL_OSExTaxAmount = 150M;
				crdLine.AL_OSExtraTaxAmount = 9M;
				Invoice.AH_OSTaxAmount = 50M;
				Invoice.AH_OSExTaxAmount = 150M;
				Invoice.AH_LocalOutstandingAmount = 100M;
				Invoice.AH_DueDate = ZDateTime.Today;
				Invoice.AH_FullyPaidDate = ZDateTime.Empty;

				AssertEquals("Credit note due date", ZDateTime.Today, InvoiceWrapper.TransactionDueDate);

				AssertEquals("Pre-condition: RegistryItem is False by default", false, AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.Value);

				AssertEquals("OSOutstandingAmount", 200M, creditNoteWrapper.OSOutstandingAmount);
				AssertEquals("OSOutstandingAmount", "-200.00", creditNoteWrapper.OSOutstandingAmountFormatted);
				AssertEquals("OSTotal", 200M, creditNoteWrapper.OSTotal);
				AssertEquals("OSTotalFormatted", "200.00", creditNoteWrapper.OSTotalFormatted);
				AssertEquals("OSTotalFormattedWithSign", "-200.00", creditNoteWrapper.OSTotalFormattedWithSign);
				AssertEquals("LocalTotal", 100M, creditNoteWrapper.LocalTotal);
				AssertEquals("LocalTotalFormatted", "100.00", creditNoteWrapper.LocalTotalFormatted);
				AssertEquals("LocalTotalFormattedWithSign", "-100.00", creditNoteWrapper.LocalTotalFormattedWithSign);
				AssertEquals("TotalOSTaxAmount", 50M, creditNoteWrapper.TotalOSTaxAmount);
				AssertEquals("TotalOSTaxAmountFormatted", "50.00", creditNoteWrapper.TotalOSTaxAmountFormatted);
				AssertEquals("TotalOSTaxAmountFormattedWithSign", "-50.00", creditNoteWrapper.TotalOSTaxAmountFormattedWithSign);
				AssertEquals("InvoiceSubTotal", 150M, creditNoteWrapper.InvoiceSubTotal);
				AssertEquals("InvoiceSubTotalFormatted", "150.00", creditNoteWrapper.InvoiceSubTotalFormatted);
				AssertEquals("InvoiceSubTotalFormattedWithSign", "-150.00", creditNoteWrapper.InvoiceSubTotalFormattedWithSign);
				AssertEquals("InvoiceLocalSubTotal", 75M, creditNoteWrapper.InvoiceLocalSubTotal);
				AssertEquals("InvoiceLocalSubTotalFormatted", "75.00", creditNoteWrapper.InvoiceLocalSubTotalFormatted);
				AssertEquals("InvoiceLocalSubTotalFormattedWithSign", "-75.00", creditNoteWrapper.InvoiceLocalSubTotalFormattedWithSign);
				AssertEquals("TotalOSQSTAmount", 0M, creditNoteWrapper.TotalOSQSTAmount);
				AssertEquals("TotalOSQSTAmountFormatted", "0.00", creditNoteWrapper.TotalOSQSTAmountFormatted);
				AssertEquals("TotalOSQSTAmountFormattedWithSign", "0.00", creditNoteWrapper.TotalOSQSTAmountFormattedWithSign);
				AssertEquals("TotalOSEDUAmount", 9M, creditNoteWrapper.TotalOSEDUAmount);
				AssertEquals("TotalOSEDUAmountFormatted", "9.00", creditNoteWrapper.TotalOSEDUAmountFormatted);
				AssertEquals("TotalOSEDUAmountFormattedWithSign", "-9.00", creditNoteWrapper.TotalOSEDUAmountFormattedWithSign);
				AssertEquals("TotalOSEDUPrimaryAmount", 6M, creditNoteWrapper.TotalOSEDUPrimaryAmount);
				AssertEquals("TotalOSEDUPrimaryAmountFormatted", "6.00", creditNoteWrapper.TotalOSEDUPrimaryAmountFormatted);
				AssertEquals("TotalOSEDUPrimaryAmountFormattedWithSign", "-6.00", creditNoteWrapper.TotalOSEDUPrimaryAmountFormattedWithSign);
				AssertEquals("TotalOSEDUSecondaryAmount", 3M, creditNoteWrapper.TotalOSEDUSecondaryAmount);
				AssertEquals("TotalOSEDUSecondaryAmountFormatted", "3.00", creditNoteWrapper.TotalOSEDUSecondaryAmountFormatted);
				AssertEquals("TotalOSEDUSecondaryAmountFormattedWithSign", "-3.00", creditNoteWrapper.TotalOSEDUSecondaryAmountFormattedWithSign);
				AssertEquals("TotalOSRETAmount", -9M, creditNoteWrapper.TotalOSRETAmount);
				AssertEquals("TotalOSRETAmountFormatted", "-9.00", creditNoteWrapper.TotalOSRETAmountFormatted);
				AssertEquals("TotalOSRETAmountFormattedWithSign", "9.00", creditNoteWrapper.TotalOSRETAmountFormattedWithSign);

				using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
				{
					AssertEquals("OSOutstandingAmount", "-200,00", creditNoteWrapper.OSOutstandingAmountFormatted);
					AssertEquals("OSTotalFormatted", "200,00", creditNoteWrapper.OSTotalFormatted);
					AssertEquals("LocalTotalFormatted", "100,00", creditNoteWrapper.LocalTotalFormatted);
					AssertEquals("TotalOSTaxAmountFormatted", "50,00", creditNoteWrapper.TotalOSTaxAmountFormatted);
					AssertEquals("InvoiceSubTotalFormatted", "150,00", creditNoteWrapper.InvoiceSubTotalFormatted);
					AssertEquals("InvoiceLocalSubTotalFormatted", "75,00", creditNoteWrapper.InvoiceLocalSubTotalFormatted);
					AssertEquals("TotalOSQSTAmountFormatted", "0,00", creditNoteWrapper.TotalOSQSTAmountFormatted);
					AssertEquals("TotalOSEDUAmountFormatted", "9,00", creditNoteWrapper.TotalOSEDUAmountFormatted);
					AssertEquals("TotalOSEDUPrimaryAmountFormatted", "6,00", creditNoteWrapper.TotalOSEDUPrimaryAmountFormatted);
					AssertEquals("TotalOSEDUSecondaryAmountFormatted", "3,00", creditNoteWrapper.TotalOSEDUSecondaryAmountFormatted);
					AssertEquals("TotalOSRETAmountFormatted", "-9,00", creditNoteWrapper.TotalOSRETAmountFormatted);
				}

				AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					AssertEquals("OSOutstandingAmount should be with opposite sign", -200M, creditNoteWrapper.OSOutstandingAmount);
					AssertEquals("OSOutstandingAmount", "200.00", creditNoteWrapper.OSOutstandingAmountFormatted);
					AssertEquals("OSTotal should be with opposite sign", -200M, creditNoteWrapper.OSTotal);
					AssertEquals("OSTotalFormatted", "-200.00", creditNoteWrapper.OSTotalFormatted);
					AssertEquals("OSTotalFormattedWithSign", "-200.00", creditNoteWrapper.OSTotalFormattedWithSign);
					AssertEquals("LocalTotal", -100M, creditNoteWrapper.LocalTotal);
					AssertEquals("LocalTotalFormatted", "-100.00", creditNoteWrapper.LocalTotalFormatted);
					AssertEquals("LocalTotalFormattedWithSign", "-100.00", creditNoteWrapper.LocalTotalFormattedWithSign);
					AssertEquals("TotalOSTaxAmount", -50M, creditNoteWrapper.TotalOSTaxAmount);
					AssertEquals("TotalOSTaxAmountFormatted", "-50.00", creditNoteWrapper.TotalOSTaxAmountFormatted);
					AssertEquals("TotalOSTaxAmountFormattedWithSign", "-50.00", creditNoteWrapper.TotalOSTaxAmountFormattedWithSign);
					AssertEquals("InvoiceSubTotal", -150M, creditNoteWrapper.InvoiceSubTotal);
					AssertEquals("InvoiceSubTotalFormatted", "-150.00", creditNoteWrapper.InvoiceSubTotalFormatted);
					AssertEquals("InvoiceSubTotalFormattedWithSign", "-150.00", creditNoteWrapper.InvoiceSubTotalFormattedWithSign);
					AssertEquals("InvoiceLocalSubTotal", -75M, creditNoteWrapper.InvoiceLocalSubTotal);
					AssertEquals("InvoiceLocalSubTotalFormatted", "-75.00", creditNoteWrapper.InvoiceLocalSubTotalFormatted);
					AssertEquals("InvoiceLocalSubTotalFormattedWithSign", "-75.00", creditNoteWrapper.InvoiceLocalSubTotalFormattedWithSign);
					AssertEquals("TotalOSQSTAmount", 0M, creditNoteWrapper.TotalOSQSTAmount);
					AssertEquals("TotalOSQSTAmountFormatted", "0.00", creditNoteWrapper.TotalOSQSTAmountFormatted);
					AssertEquals("TotalOSQSTAmountFormattedWithSign", "0.00", creditNoteWrapper.TotalOSQSTAmountFormattedWithSign);
					AssertEquals("TotalOSEDUAmount", -9M, creditNoteWrapper.TotalOSEDUAmount);
					AssertEquals("TotalOSEDUAmountFormatted", "-9.00", creditNoteWrapper.TotalOSEDUAmountFormatted);
					AssertEquals("TotalOSEDUAmountFormattedWithSign", "-9.00", creditNoteWrapper.TotalOSEDUAmountFormattedWithSign);
					AssertEquals("TotalOSEDUPrimaryAmount", -6M, creditNoteWrapper.TotalOSEDUPrimaryAmount);
					AssertEquals("TotalOSEDUPrimaryAmountFormatted", "-6.00", creditNoteWrapper.TotalOSEDUPrimaryAmountFormatted);
					AssertEquals("TotalOSEDUPrimaryAmountFormattedWithSign", "-6.00", creditNoteWrapper.TotalOSEDUPrimaryAmountFormattedWithSign);
					AssertEquals("TotalOSEDUSecondaryAmount", -3M, creditNoteWrapper.TotalOSEDUSecondaryAmount);
					AssertEquals("TotalOSEDUSecondaryAmountFormatted", "-3.00", creditNoteWrapper.TotalOSEDUSecondaryAmountFormatted);
					AssertEquals("TotalOSEDUSecondaryAmountFormattedWithSign", "-3.00", creditNoteWrapper.TotalOSEDUSecondaryAmountFormattedWithSign);
					AssertEquals("TotalOSRETAmount", 9M, creditNoteWrapper.TotalOSRETAmount);
					AssertEquals("TotalOSRETAmountFormatted", "9.00", creditNoteWrapper.TotalOSRETAmountFormatted);
					AssertEquals("TotalOSRETAmountFormattedWithSign", "9.00", creditNoteWrapper.TotalOSRETAmountFormattedWithSign);
				}
				else
				{
					AssertEquals("OSOutstandingAmount", 200M, creditNoteWrapper.OSOutstandingAmount);
					AssertEquals("OSOutstandingAmount", "-200.00", creditNoteWrapper.OSOutstandingAmountFormatted);
					AssertEquals("OSTotal", 200M, creditNoteWrapper.OSTotal);
					AssertEquals("OSTotalFormatted", "200.00", creditNoteWrapper.OSTotalFormatted);
					AssertEquals("OSTotalFormattedWithSign", "200.00", creditNoteWrapper.OSTotalFormattedWithSign);
					AssertEquals("LocalTotal", 100M, creditNoteWrapper.LocalTotal);
					AssertEquals("LocalTotalFormatted", "100.00", creditNoteWrapper.LocalTotalFormatted);
					AssertEquals("LocalTotalFormattedWithSign", "100.00", creditNoteWrapper.LocalTotalFormattedWithSign);
					AssertEquals("TotalOSTaxAmount", 50M, creditNoteWrapper.TotalOSTaxAmount);
					AssertEquals("TotalOSTaxAmountFormatted", "50.00", creditNoteWrapper.TotalOSTaxAmountFormatted);
					AssertEquals("TotalOSTaxAmountFormattedWithSign", "50.00", creditNoteWrapper.TotalOSTaxAmountFormattedWithSign);
					AssertEquals("InvoiceSubTotal", 150M, creditNoteWrapper.InvoiceSubTotal);
					AssertEquals("InvoiceSubTotalFormatted", "150.00", creditNoteWrapper.InvoiceSubTotalFormatted);
					AssertEquals("InvoiceSubTotalFormattedWithSign", "150.00", creditNoteWrapper.InvoiceSubTotalFormattedWithSign);
					AssertEquals("InvoiceLocalSubTotal", 75M, creditNoteWrapper.InvoiceLocalSubTotal);
					AssertEquals("InvoiceLocalSubTotalFormatted", "75.00", creditNoteWrapper.InvoiceLocalSubTotalFormatted);
					AssertEquals("InvoiceLocalSubTotalFormattedWithSign", "75.00", creditNoteWrapper.InvoiceLocalSubTotalFormattedWithSign);
					AssertEquals("TotalOSQSTAmount", 0M, creditNoteWrapper.TotalOSQSTAmount);
					AssertEquals("TotalOSQSTAmountFormatted", "0.00", creditNoteWrapper.TotalOSQSTAmountFormatted);
					AssertEquals("TotalOSQSTAmountFormattedWithSign", "0.00", creditNoteWrapper.TotalOSQSTAmountFormattedWithSign);
					AssertEquals("TotalOSEDUAmount", 9M, creditNoteWrapper.TotalOSEDUAmount);
					AssertEquals("TotalOSEDUAmountFormatted", "9.00", creditNoteWrapper.TotalOSEDUAmountFormatted);
					AssertEquals("TotalOSEDUAmountFormattedWithSign", "9.00", creditNoteWrapper.TotalOSEDUAmountFormattedWithSign);
					AssertEquals("TotalOSEDUPrimaryAmount", 6M, creditNoteWrapper.TotalOSEDUPrimaryAmount);
					AssertEquals("TotalOSEDUPrimaryAmountFormatted", "6.00", creditNoteWrapper.TotalOSEDUPrimaryAmountFormatted);
					AssertEquals("TotalOSEDUPrimaryAmountFormattedWithSign", "6.00", creditNoteWrapper.TotalOSEDUPrimaryAmountFormattedWithSign);
					AssertEquals("TotalOSEDUSecondaryAmount", 3M, creditNoteWrapper.TotalOSEDUSecondaryAmount);
					AssertEquals("TotalOSEDUSecondaryAmountFormatted", "3.00", creditNoteWrapper.TotalOSEDUSecondaryAmountFormatted);
					AssertEquals("TotalOSEDUSecondaryAmountFormattedWithSign", "3.00", creditNoteWrapper.TotalOSEDUSecondaryAmountFormattedWithSign);
					AssertEquals("TotalOSRETAmount", -9M, creditNoteWrapper.TotalOSRETAmount);
					AssertEquals("TotalOSRETAmountFormatted", "-9.00", creditNoteWrapper.TotalOSRETAmountFormatted);
					AssertEquals("TotalOSRETAmountFormattedWithSign", "-9.00", creditNoteWrapper.TotalOSRETAmountFormattedWithSign);
				}
			}
		}

		public void TestCreditNoteTotalOSSPVAmountsWithShowARCreditNoteAmountsWithOppositeSignRegistry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var creditNote = Factory.New<ARCreditNote>();
				var creditNoteLine = (InvoicingLineBase)creditNote.Lines.AddNew();
				creditNoteLine.AL_AT = TestObjectCreator.VATSPV.PK;
				creditNoteLine.AL_OSExTaxAmount = 45.47M;
				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "VND";
				creditNote.AH_RX_NKTransactionCurrency = "USD";
				creditNote.AH_ExchangeRate = 0.5;
				AssertEquals("Line should have tax amount", 10m, creditNoteLine.AL_OSGSTAmount);
				var creditNoteWrapper = DocARInvoice.New(creditNote, Factory);

				using (AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("Subtotal", 45.47m, creditNoteWrapper.InvoiceSubTotal);
					AssertEquals("Add VAT", "10,00", creditNoteWrapper.TotalOSVATExcludeSPVAmountFormatted);
					AssertEquals("Add VAT", "-10,00", creditNoteWrapper.TotalOSVATExcludeSPVAmountFormattedWithSign.Replace(" ", ZString.Empty));
					AssertEquals("OS Total excluding SPV amount", 55.47m, creditNoteWrapper.OSTotalExcludeSPVAmount);
					AssertEquals("Split Payment", "-10,00", creditNoteWrapper.TotalOSSPVAmountFormatted.Replace(" ", ZString.Empty));
					AssertEquals("ignore registry", "10,00", creditNoteWrapper.TotalOSSPVAmountFormattedWithSign);
					AssertEquals("Local Split Payment", "20", creditNoteWrapper.TotalLocalSPVAmountFormatted);
					AssertEquals("OS Total", 45.47m, creditNoteWrapper.OSTotal);
				}

				using (AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Subtotal", -45.47m, creditNoteWrapper.InvoiceSubTotal);
					AssertEquals("Add VAT", "-10,00", creditNoteWrapper.TotalOSVATExcludeSPVAmountFormatted.Replace(" ", ZString.Empty));
					AssertEquals("Add VAT", "-10,00", creditNoteWrapper.TotalOSVATExcludeSPVAmountFormattedWithSign.Replace(" ", ZString.Empty));
					AssertEquals("OS Total excluding SPV amount", -55.47m, creditNoteWrapper.OSTotalExcludeSPVAmount);
					AssertEquals("Split Payment", "10,00", creditNoteWrapper.TotalOSSPVAmountFormatted);
					AssertEquals("ignore registry", "10,00", creditNoteWrapper.TotalOSSPVAmountFormattedWithSign);
					AssertEquals("OS Total", -45.47m, creditNoteWrapper.OSTotal);
				}
			}
		}

		#endregion

		#region Multiple Installments
		[TestDate(2022, 5, 25)]
		public void TestMultipleInstallments()
		{
			if (InvoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				var currCompany = GlbCompany.CurrentCompany;
				var arTerms = TestObjectCreator.AALSHI.CompanyData.ARTerms;
				AssertNotNull(arTerms);

				var arTermMLI = arTerms.AddNew();
				arTermMLI.PY_JobType = "ALL";
				arTermMLI.PY_GB_Branch = InvoicingBase.AH_GB;
				arTermMLI.PY_GE_Department = InvoicingBase.AH_GE;
				arTermMLI.PY_Direction = "ALL";
				arTermMLI.PY_TransportMode = "ALL";
				arTermMLI.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
				arTermMLI.PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

				var arTermMLInstalments = arTermMLI.ARTermsInstallments;
				AssertNotNull(arTermMLInstalments);

				var installment1 = arTermMLInstalments.AddNew();
				installment1.ML_SequenceNumber = 1;
				installment1.ML_SplitPercentage = 33.33;
				installment1.ML_DaysFromInvoiceDate = 30;
				installment1.ML_AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer;

				var installment2 = arTermMLInstalments.AddNew();
				installment2.ML_SequenceNumber = 2;
				installment2.ML_SplitPercentage = 33.33;
				installment2.ML_DaysFromInvoiceDate = 60;
				installment2.ML_AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;

				var installment3 = arTermMLInstalments.AddNew();
				installment3.ML_SequenceNumber = 3;
				installment3.ML_SplitPercentage = 33.34;
				installment3.ML_DaysFromInvoiceDate = 90;
				installment3.ML_AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck;

				var today = ZDateTime.Today;
				InvoicingBase.AH_InvoiceDate = new ZDateTime(today.Year, today.Month, 1);
				InvoicingBase.AH_PostDate = today;
				InvoicingBase.AH_DueDate = today.AddMonths(1);

				InvoicingBase.AH_OH = TestObjectCreator.AALSHI.PK;
				InvoicingBase.AH_TransactionNum = "00001000";
				const string subType = "TXI";
				InvoicingBase.AH_ComplianceSubType = subType;
				InvoicingBase.AH_TransactionReference = $"{subType}001";
				var currency = TestObjectCreator.EUR;
				InvoicingBase.AH_RX_NKTransactionCurrency = currency.RX_Code;
				InvoicingBase.AH_ExchangeRate = 1;
				InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
				InvoicingBase.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest;

				InvoicingBase.Lines.DeleteAll();
				var line = InvoicingBase.Lines.AddNew();
				line.AL_GB = GlbBranch.CurrentBranch.PK;
				line.AL_GE = GlbDepartment.CurrentDepartment.PK;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.AL_OSExTaxAmount = 111.10;
				line.AL_OSTaxAmount = 11.11;

				InvoicingBase.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
				Factory.Save();
				AssertEquals(Constants.InvoiceTerms.MultipleInstallments, InvoicingBase.AH_InvoiceTerm);

				var testMatchingBase = new ARMatchingBase(Factory);
				testMatchingBase.PrimaryOrganization = TestObjectCreator.AALSHI.PK;

				var testARReceipt = Factory.NewWithValidTestData<ARReceipt>();
				testARReceipt.AH_OH = TestObjectCreator.AALSHI.PK;
				testARReceipt.AH_LocalExTaxAmount = 40.73M;
				testARReceipt.AH_LocalOutstandingAmount = 40.73M;
				testARReceipt.AH_OSTotalAmount = 40.730M;

				Dictionary<BusinessObject, ZDecimal> transactionsToMatch = new Dictionary<BusinessObject, ZDecimal>();
				var firstInstallment = InvoicingBase.GetMultipleInstallmentsJournals(Constants.TransactionCategory.Codes.InstalmentJournal)[0];
				transactionsToMatch.Add(firstInstallment, firstInstallment.AH_OSTotal);
				transactionsToMatch.Add(testARReceipt, testARReceipt.AH_OSTotal);
				testMatchingBase.MoveFromUnmatchToMatch(transactionsToMatch);

				var matchingResult = testMatchingBase.MatchAndClearTransactions();
				AssertEquals("The 1st installment is payed", true, matchingResult);

				var savedInvoicingBase = Factory.Load<InvoicingBase>(InvoicingBase.PK);
				var aRInvoiceWrapper = DocARInvoice.New(savedInvoicingBase, Factory);

				AssertNotNull(aRInvoiceWrapper.MultipleInstallments);
				AssertEquals(3, aRInvoiceWrapper.MultipleInstallments.Count);

				AssertEquals("1\r\n2\r\n3\r\n", aRInvoiceWrapper.MultipleInstallmentsSequencesColumn);
				AssertEquals("Bank Transfer\r\nBusiness Check\r\nCash and/or Bank Check\r\n", aRInvoiceWrapper.MultipleInstallmentsPaymentMethodsColumn);
				AssertEquals("EUR 40.73\r\nEUR 40.73\r\nEUR 40.75\r\n", aRInvoiceWrapper.MultipleInstallmentsInvoicedAmountsColumn);
				AssertEquals("EUR 0.00\r\nEUR 40.73\r\nEUR 40.75\r\n", aRInvoiceWrapper.MultipleInstallmentsBalanceDueAmountsColumn);
				AssertEquals("31-May-22\r\n30-Jun-22\r\n30-Jul-22\r\n", aRInvoiceWrapper.MultipleInstallmentsDueDatesColumn);
				AssertEquals("122.21", aRInvoiceWrapper.OSOutstandingAmountFormatted);
				AssertEquals("81.48", aRInvoiceWrapper.OSOutstandingAmountForMLIFormatted);
			}
			else
			{
				Assert(true);
			}
		}
		#endregion

		public void TestInvoiceHeaderReferenceValues()
		{
			var creator = new TestObjectCreator(Factory);
			var configurationCollection = InvoiceRemittanceConfigurationCollectionTest.GetConfigurationCollectionForTest(Factory);
			AccountingMasterFilesRegistry.Instance.InvoiceRemittanceConfiguration.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, configurationCollection);
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AG = creator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 100;

			var orgCompanyData = Factory.LoadTop1<OrgCompanyData>(new ZQuery(OrgCompanyDataSchema.OB_IsDebtor, true));
			orgCompanyData.Header.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			invoice.AH_OH = orgCompanyData.Header.PK;
			Factory.Save();

			var aRInvoiceWrapper = DocARInvoice.New(invoice, Factory);

			AssertEquals("InvoiceRemittanceType", "BBB", aRInvoiceWrapper.InvoiceRemittanceType);
			AssertEquals("InvoiceRemittanceReference", "XX123456", aRInvoiceWrapper.InvoiceRemittanceReference);
			AssertEquals("InvoiceNumber", invoice.AH_TransactionNum, aRInvoiceWrapper.InvoiceNumber);
			AssertEquals("InvoiceTotalInLocalCurrency", "", aRInvoiceWrapper.InvoiceTotalInLocalCurrency);
			AssertEquals("InvoiceTotalInInvoiceCurrency", "", aRInvoiceWrapper.InvoiceTotalInInvoiceCurrency);
			AssertEquals("InvoiceRemittanceMessage", "This is description", aRInvoiceWrapper.InvoiceRemittanceMessage);
			AssertEquals("InvoiceRemittanceBillerCode", "PAY", aRInvoiceWrapper.InvoiceRemittanceBillerCode);
			AssertEquals("InvoiceRemittanceBillerAccountNumber", "123456", aRInvoiceWrapper.InvoiceRemittanceBillerAccountNumber);
			AssertEquals("DebtorOrganizationCode", orgCompanyData.Header.OH_Code, aRInvoiceWrapper.DebtorOrganizationCode);
			AssertEquals("DebtorClientNumber", invoice.Header.CompanyData.OB_ARClientNumber, aRInvoiceWrapper.DebtorClientNumber);
			AssertEquals("InvoiceTransactionReference", invoice.InvoiceTransactionReference, aRInvoiceWrapper.InvoiceTransactionReference);
		}

		#region Tests for ARCashAdvanceReceivedAmountFormatted
		public JobCharge CreateChargeForCashAdvanceInvoiceLine(Job job, ARInvoiceLine line, ZGuid cashAdvanceLinePK, ZString sellReference)
		{
			var charge = TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
			charge.JR_IsARCashAdvance = true;
			charge.JR_SellReference = sellReference;
			charge.JR_CAL_ARLine = cashAdvanceLinePK;
			charge.JR_AL_ARLine = line.PK;
			return charge;
		}

		public void TestARAdvancePaymentReceivedAmountFormattedSingleCashAdvanceRequestWithMultipleInvoices()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001111";
			var job = GetInvoiceJob(shipment, Invoice);

			var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Debtor, LedgerTypes.AccountsReceivable, 1400M, 1400M, "AUD");
			var cal1 = TestObjectCreator.CreateCashAdvanceRequestLine(cah, 200M, 200M);
			var cal2 = TestObjectCreator.CreateCashAdvanceRequestLine(cah, 300M, 300M);
			var cal3 = TestObjectCreator.CreateCashAdvanceRequestLine(cah, 400M, 400M);
			var cal4 = TestObjectCreator.CreateCashAdvanceRequestLine(cah, 500M, 500M);

			cah.Lines.Add(cal1);
			cah.Lines.Add(cal2);
			cal1.CAL_LocalPaidAmount = cal1.CAL_OSPaidAmount = 200M;
			cal2.CAL_LocalPaidAmount = cal2.CAL_OSPaidAmount = 300M;
			cal3.CAL_LocalPaidAmount = cal3.CAL_OSPaidAmount = 400M;
			cal4.CAL_LocalPaidAmount = cal4.CAL_OSPaidAmount = 500M;
			cal1.CAL_Status = cal2.CAL_Status = cal3.CAL_Status = cal4.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;

			Factory.Save();

			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();

			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var line1 = TestObjectCreator.CreateARInvoiceLine(invoice1, job, TestObjectCreator.CC1, audCurrency, 1.0M, "DESC01", 200M);
			var line2 = TestObjectCreator.CreateARInvoiceLine(invoice1, job, TestObjectCreator.CC2, audCurrency, 1.0M, "DESC02", 300M);
			CreateChargeForCashAdvanceInvoiceLine(job, line1, cal1.PK, "1234");
			CreateChargeForCashAdvanceInvoiceLine(job, line2, cal2.PK, "1234");
			cal1.CAL_Status = cal2.CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;

			var line3 = TestObjectCreator.CreateARInvoiceLine(invoice2, job, TestObjectCreator.CC3, audCurrency, 1.0M, "DESC03", 400M);
			var line4 = TestObjectCreator.CreateARInvoiceLine(invoice2, job, TestObjectCreator.CC4, audCurrency, 1.0M, "DESC04", 500M);
			CreateChargeForCashAdvanceInvoiceLine(job, line3, cal3.PK, "2345");
			CreateChargeForCashAdvanceInvoiceLine(job, line4, cal4.PK, "2345");
			cal3.CAL_Status = cal4.CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;

			var invoice1Wrapper = DocARInvoice.New(invoice1, Factory);
			var invoice2Wrapper = DocARInvoice.New(invoice2, Factory);
			AssertEquals(500.00m, invoice1Wrapper.ARAdvancePaymentReceivedAmount);
			AssertEquals("500.00", invoice1Wrapper.ARAdvancePaymentReceivedAmountFormatted);

			AssertEquals(900.00m, invoice2Wrapper.ARAdvancePaymentReceivedAmount);
			AssertEquals("900.00", invoice2Wrapper.ARAdvancePaymentReceivedAmountFormatted);
		}

		public void TestARAdvancePaymentReceivedAmountFormattedDoesNotThrow()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.Lines.AddNew();

			var invoiceWrapper = DocARInvoice.New(invoice, Factory);
			var lineWrapper = invoiceWrapper.Lines.FirstOrDefault() as DocARInvoiceLine;

			AssertNotNull(lineWrapper);
			AssertNull(lineWrapper.Charge);

			AssertNoExceptionThrown("Calculated property should not throw exceptions for lines without Charges.", () =>
			{
				AssertEquals("0.00", invoiceWrapper.ARAdvancePaymentReceivedAmountFormatted);
			});
		}

		#endregion

		#region Implementation

		protected TResult RecreateTestingInvoiceDocWrapper<T, TResult>(T invoice, Func<T, BusinessObjectFactory, TResult> createWrapper)
				where T : TransactionHeader
				where TResult : DocBaseWrapper
		{
			if (invoice.IsInDatabase)
			{
				invoice.AH_FullyPaidDate = ZDateTime.Empty;
			}
			return RecreateTestingDocWrapper(invoice, createWrapper);
		}

		#region test class

		public class DocARInvoiceCommonWrapperForTest : DocARInvoiceCommon
		{
			public DocARInvoiceCommonWrapperForTest(InvoicingBase invbase, BusinessObjectFactory fuct)
					: base(invbase, fuct)
			{
			}

			protected override ZString TaxInvoiceTitle
			{
				get { return ""; }
			}

			protected override ZString NonTaxInvoiceTitle
			{
				get { return ""; }
			}

			protected override ZString TaxCreditNoteTitle
			{
				get { return ""; }
			}

			protected override ZString NonTaxCreditNoteTitle
			{
				get { return ""; }
			}

			protected override ZString TaxAdjustmentNoteTitle
			{
				get { return ""; }
			}

			protected override ZString NonTaxAdjustmentNoteTitle
			{
				get { return ""; }
			}

			protected override string InvoiceMessage
			{
				get { return ""; }
			}

			protected override string CreditNoteMessage
			{
				get { return ""; }
			}

			protected override string AdjustmentNoteMessage
			{
				get { return ""; }
			}

			protected override DocARInvoiceLineCollection GetInvoiceLines()
			{
				return new DocARInvoiceLineCollection(Factory);
			}
		}

		#endregion

		DocARInvoiceCommon ARInvoiceWrapper
		{
			get { return (DocARInvoiceCommon)InvoiceWrapper; }
		}

		protected ZString LocalCurrency;

		protected override void SetUp()
		{
			base.SetUp();

			LocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
		}

		#region SetUp Methods For Roll Up

		protected void SetUpInvoiceWrapperForRollUp()
		{
			SetUpInvoiceWrapperForRollUp(Core.Constants.TransportModes.All);
		}

		ForwardingShipment Shipment;
		protected void SetUpInvoiceWrapperForRollUp(ZString transportMode)
		{
			OrgHeader debtor = SetUpDebtorForRollUp();

			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = transportMode;
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = "S1234";
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			Invoice.AH_OH = debtor.PK;
			if (Invoice.IsInDatabase)
			{
				Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			}
			JobHeader job = GetInvoiceJob(Shipment, Invoice);
			job.LocalChargesPK = debtor.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;
		}

		protected void SetUpCreditNoteWrapperForRollUp()
		{
			OrgHeader debtor = SetUpDebtorForRollUp();

			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.All;
			Invoice = Factory.New<ARCreditNote>();
			Invoice.AH_ConsolidatedInvoiceRef = "S1234";
			Invoice.AH_OH = debtor.PK;
			if (Invoice.IsInDatabase)
			{
				Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			}
			JobHeader job = GetInvoiceJob(Shipment, Invoice);
			job.LocalChargesPK = debtor.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			Factory.Save();
		}

		OrgHeader SetUpDebtorForRollUp()
		{
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "TEST3";
			debtor.OH_IsDebtor = true;
			debtor.OH_IsConsignor = false;
			debtor.OH_IsConsignee = false;
			debtor.OH_IsBroker = false;
			debtor.OH_IsForwarder = false;
			debtor.MainAddress.OA_RL_NKRelatedPortCode = "CNSHA";
			return debtor;
		}

		protected void SetUpConsolInvoiceWrapperForRollUp()
		{
			SetUpConsolInvoiceWrapperForRollUp(Core.Constants.TransportModes.All);
		}

		ForwardingConsol Consol;
		void SetUpConsolInvoiceWrapperForRollUp(ZString transportMode)
		{
			OrgHeader debtor = SetUpDebtorForRollUp();
			Consol = Factory.New<ForwardingConsol>();
			Consol.JK_TransportMode = transportMode;
			Consol.JK_UniqueConsignRef = "C0001234";
			Invoice = Factory.New<ARInvoice>();
			Invoice.AH_ConsolidatedInvoiceRef = Consol.JK_UniqueConsignRef;
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			Invoice.AH_OH = debtor.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
		}

		protected void SetUpDepartmentAndDirection()
		{
			var importDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Desc, SQLComparisonOperator.Contains, "import"));
			Invoice.AH_GE = importDepartment.PK;
			SetUpShipmentDirection();
		}

		protected void SetUpShipmentDirection()
		{
			if (Shipment != null)
			{
				Shipment.JS_RL_NKOrigin = "NZAKL";
				Shipment.JS_RL_NKDestination = "AUSYD";
			}
		}

		protected void SetUpOrganisationForRollUpSubTotals()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = header.CompanyData.InvoiceRollupOrGroups.AddNew();
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.SubTotal;
			invoiceRollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			header.CompanyData.SetARTaxApplicable(ZBool.True);
			Invoice.AH_OH = header.PK;
			if (Invoice.IsInDatabase)
			{
				Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			}
			SetLineTaxRate(InvoicingBase, GetRate());
		}

		protected void SetUpOrganisationForRollUpAll(bool setLineTaxRates = true)
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = header.CompanyData.InvoiceRollupOrGroups.AddNew();
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.All;
			invoiceRollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			header.CompanyData.SetARTaxApplicable(ZBool.True);
			Invoice.AH_OH = header.PK;
			if (Invoice.IsInDatabase)
			{
				Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			}
			if (setLineTaxRates)
			{
				SetLineTaxRate(InvoicingBase, GetRate());
			}
		}

		protected void SetUpOrganisationForRollByChargeCodeGroup()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = header.CompanyData.InvoiceRollupOrGroups.AddNew();
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CCG;
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.CFSShipment.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			header.CompanyData.SetARTaxApplicable(ZBool.True);
			Invoice.AH_OH = header.PK;
			if (Invoice.IsInDatabase)
			{
				Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			}
			SetLineTaxRate(InvoicingBase, GetRate());
		}

		protected void SetUpOrganisationForRollUpAEC(bool setLineTaxRates = true)
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = header.CompanyData.InvoiceRollupOrGroups.AddNew();
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.AEC;
			invoiceRollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;

			invoiceRollupOrGroup = header.CompanyData.InvoiceRollupOrGroups.AddNew();
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.AEC;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;

			Invoice.AH_OH = header.PK;
			if (Invoice.IsInDatabase)
			{
				Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			}
			if (setLineTaxRates)
			{
				SetLineTaxRate(InvoicingBase, GetRate());
			}
		}

		protected OrgInvoiceRollupOrGroup SetUpOrganisationForRollUpWithCode(string code, bool setLineTaxRates = true)
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = header.CompanyData.InvoiceRollupOrGroups.AddNew();
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = code;
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceRollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;

			Invoice.AH_OH = header.PK;
			if (Invoice.IsInDatabase)
			{
				Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			}
			if (setLineTaxRates)
			{
				SetLineTaxRate(InvoicingBase, GetRate());
			}

			return invoiceRollupOrGroup;
		}

		protected void SetUpOrganisationForSortingWithCode(string groupOrSubTotal, bool setLineTaxRates = true)
		{
			OrgHeader header = Invoice.AH_OH.IsEmpty ? Factory.LoadTop1<OrgHeader>(new ZQuery()) :
					Factory.Load<OrgHeader>(Invoice.AH_OH);
			header.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = header.CompanyData.InvoiceRollupOrGroups.AddNew();
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = "OFD";
			invoiceRollupOrGroup.PG_GroupOrSubTotal = groupOrSubTotal;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceRollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;

			Invoice.AH_OH = header.PK;
			if (Invoice.IsInDatabase)
			{
				Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			}
			if (setLineTaxRates)
			{
				SetLineTaxRate(InvoicingBase, GetRate());
			}
		}

		protected void SetUpOrganisationForInvoiceOrder()
		{
			OrgHeader header = Invoice.AH_OH.IsEmpty ? Factory.LoadTop1<OrgHeader>(new ZQuery()) :
					Factory.Load<OrgHeader>(Invoice.AH_OH);
			header.InvoiceOrders.DeleteAll();
			AccClientInvoiceOrder invoiceOrder = header.InvoiceOrders.AddNew();
			invoiceOrder.AI_AC = Factory.LoadFromNaturalKey<AccChargeCode>(AccChargeCodeSchema.AC_Code, "NJC").PK;
			invoiceOrder.AI_InvoiceType = "ALL";
			invoiceOrder.AI_PrintOrder = 1;

			invoiceOrder = header.InvoiceOrders.AddNew();
			invoiceOrder.AI_AC = Factory.LoadFromNaturalKey<AccChargeCode>(AccChargeCodeSchema.AC_Code, "LDC").PK;
			invoiceOrder.AI_InvoiceType = "FIN";
			invoiceOrder.AI_PrintOrder = 2;

			invoiceOrder = header.InvoiceOrders.AddNew();
			invoiceOrder.AI_AC = Factory.LoadFromNaturalKey<AccChargeCode>(AccChargeCodeSchema.AC_Code, "ORG").PK;
			invoiceOrder.AI_InvoiceType = "DBT";
			invoiceOrder.AI_PrintOrder = 3;

			Invoice.AH_OH = header.PK;
			if (Invoice.IsInDatabase)
			{
				Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			}
		}

		protected InvoicingLineBase AddOriginChargeToInvoice()
		{
			return AddOriginChargeToInvoice("", 0);
		}

		protected InvoicingLineBase AddOriginChargeToInvoice(string chargeCode_Code, short printSequence, InvoicingBase invoicingBase = null)
		{
			AccChargeCode chargeCode = Factory.LoadFromNaturalKey<AccChargeCode>(AccChargeCodeSchema.AC_Code, chargeCode_Code);
			chargeCode = chargeCode ?? GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_PrintSequence = printSequence;
			if (!string.IsNullOrEmpty(chargeCode_Code))
			{
				chargeCode.AC_Code = chargeCode_Code;
			}

			if (invoicingBase != null)
			{
				InvoicingBase = invoicingBase;
			}
			if (chargeCode.Factory != InvoicingBase.Factory)
			{
				chargeCode.Factory.Save();
			}

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate();
			line.AL_Desc = "Origin";
			line.AL_OSExTaxAmount = 150.00M;

			return line;
		}

		protected InvoicingLineBase AddDestinationChargeToInvoice()
		{
			return AddDestinationChargeToInvoice("", 0);
		}

		protected InvoicingLineBase AddDestinationChargeToInvoice(string chargeCode_Code, short printSequence, InvoicingBase invoicingBase = null)
		{
			AccChargeCode chargeCode = Factory.LoadFromNaturalKey<AccChargeCode>(AccChargeCodeSchema.AC_Code, chargeCode_Code);
			chargeCode = chargeCode ?? GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			chargeCode.AC_PrintSequence = printSequence;
			if (!string.IsNullOrEmpty(chargeCode_Code))
			{
				chargeCode.AC_Code = chargeCode_Code;
			}

			if (invoicingBase != null)
			{
				InvoicingBase = invoicingBase;
			}
			if (chargeCode.Factory != InvoicingBase.Factory)
			{
				chargeCode.Factory.Save();
			}

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK; //This must be set before AL_Desc
			line.AL_AT = GetRate();
			line.AL_Desc = "Destination";
			line.AL_OSExTaxAmount = 200.00M;

			return line;
		}

		protected InvoicingLineBase AddChargeWithSpecifiedCurrency(ARInvoice invoice, ZString description, ZString currencyCode, string sellInvoiceCurrencyCode = "")
		{
			AccChargeCode chargeCode = GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			chargeCode.AC_PrintSequence = 0;
			chargeCode.Factory.Save();

			RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
			Charge charge = Factory.New<Charge>();
			if (GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency == charge.JR_RX_NKSellCurrency)
			{
				charge.JR_OSSellExRate = 1m;
			}
			else if (currencyCode == Core.Constants.CurrencyCodes.UnitedStates)
			{
				charge.JR_OSSellExRate = 2;
			}

			charge.JR_RX_NKSellCurrency = currency.RX_Code;
			charge.JR_RX_NKSellInvoiceCurrency = sellInvoiceCurrencyCode;
			charge.JR_OSSellAmt = 250;

			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate();
			line.AL_RX_NKTransactionCurrency = !string.IsNullOrEmpty(sellInvoiceCurrencyCode) ? (ZString)sellInvoiceCurrencyCode : currency.RX_Code;
			line.AL_Desc = description;
			line.AL_OSExTaxAmount = 250.00M;

			charge.JR_AL_ARLine = line.PK;

			return line;
		}

		protected InvoicingLineBase AddFreightChargeToInvoice()
		{
			return AddFreightChargeToInvoice("", 0);
		}

		protected InvoicingLineBase AddFreightChargeToInvoice(string chargeCode_Code, short printSequence, InvoicingBase invoicingBase = null)
		{
			AccChargeCode chargeCode = Factory.LoadFromNaturalKey<AccChargeCode>(AccChargeCodeSchema.AC_Code, chargeCode_Code);
			chargeCode = chargeCode ?? GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			chargeCode.AC_PrintSequence = printSequence;
			if (!string.IsNullOrEmpty(chargeCode_Code))
			{
				chargeCode.AC_Code = chargeCode_Code;
			}

			if (invoicingBase != null)
			{
				InvoicingBase = invoicingBase;
			}
			if (chargeCode.Factory != InvoicingBase.Factory)
			{
				chargeCode.Factory.Save();
			}

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate();
			line.AL_Desc = "Freight";
			line.AL_OSExTaxAmount = 250.00M;

			return line;
		}

		protected InvoicingLineBase AddBrokerageChargeToInvoice()
		{
			return AddBrokerageChargeToInvoice("", 0);
		}

		protected InvoicingLineBase AddBrokerageChargeToInvoice(string chargeCode_Code, short printSequence)
		{
			AccChargeCode chargeCode = Factory.LoadFromNaturalKey<AccChargeCode>(AccChargeCodeSchema.AC_Code, chargeCode_Code);
			chargeCode = chargeCode ?? GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;
			chargeCode.AC_PrintSequence = printSequence;
			if (!string.IsNullOrEmpty(chargeCode_Code))
			{
				chargeCode.AC_Code = chargeCode_Code;
			}
			if (chargeCode.Factory != InvoicingBase.Factory)
			{
				chargeCode.Factory.Save();
			}

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate();
			line.AL_Desc = "Brokerage";
			line.AL_OSExTaxAmount = 300.00M;

			return line;
		}

		protected InvoicingLineBase AddLoadChargeToInvoice()
		{
			return AddLoadChargeToInvoice("", 0);
		}

		protected InvoicingLineBase AddLoadChargeToInvoice(string chargeCode_Code, short printSequence)
		{
			return AddLoadChargeToInvoice(chargeCode_Code, printSequence, null);
		}

		protected InvoicingLineBase AddLoadChargeToInvoice(string chargeCode_Code, short printSequence, AccTaxRate tax)
		{
			AccChargeCode chargeCode = Factory.LoadFromNaturalKey<AccChargeCode>(AccChargeCodeSchema.AC_Code, chargeCode_Code);
			chargeCode = chargeCode ?? GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Loading;
			chargeCode.AC_PrintSequence = printSequence;
			if (!string.IsNullOrEmpty(chargeCode_Code))
			{
				chargeCode.AC_Code = chargeCode_Code;
			}
			if (chargeCode.Factory != InvoicingBase.Factory)
			{
				chargeCode.Factory.Save();
			}

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			if (tax != null)
			{
				line.AL_AT = tax.PK;
			}
			else
			{
				line.AL_AT = GetRate();
			}
			line.AL_Desc = "Loading";
			line.AL_OSExTaxAmount = 350.00M;

			return line;
		}

		protected InvoicingLineBase AddUnLoadChargeToInvoice()
		{
			return AddUnLoadChargeToInvoice("", 0);
		}

		protected InvoicingLineBase AddUnLoadChargeToInvoice(string chargeCode_Code, short printSequence)
		{
			AccChargeCode chargeCode = Factory.LoadFromNaturalKey<AccChargeCode>(AccChargeCodeSchema.AC_Code, chargeCode_Code);
			chargeCode = chargeCode ?? GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Unloading;
			chargeCode.AC_PrintSequence = printSequence;
			if (!string.IsNullOrEmpty(chargeCode_Code))
			{
				chargeCode.AC_Code = chargeCode_Code;
			}
			if (chargeCode.Factory != InvoicingBase.Factory)
			{
				chargeCode.Factory.Save();
			}

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate();
			line.AL_Desc = "Unloading";
			line.AL_OSExTaxAmount = 400.00M;

			return line;
		}

		protected InvoicingLineBase AddInsuranceChargeToInvoice()
		{
			return AddInsuranceChargeToInvoice("", 0);
		}

		protected InvoicingLineBase AddInsuranceChargeToInvoice(string chargeCode_Code, short printSequence, InvoicingBase invoicingBase = null)
		{
			AccChargeCode chargeCode = Factory.LoadFromNaturalKey<AccChargeCode>(AccChargeCodeSchema.AC_Code, chargeCode_Code);
			chargeCode = chargeCode ?? GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Insurance;
			chargeCode.AC_PrintSequence = printSequence;
			if (invoicingBase != null)
			{
				InvoicingBase = invoicingBase;
			}
			if (!string.IsNullOrEmpty(chargeCode_Code))
			{
				chargeCode.AC_Code = chargeCode_Code;
			}
			if (chargeCode.Factory != InvoicingBase.Factory)
			{
				chargeCode.Factory.Save();
			}

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate();
			line.AL_Desc = "Insurance";
			line.AL_OSExTaxAmount = 450.00M;

			return line;
		}

		protected InvoicingLineBase AddCustomsChargeToInvoice()
		{
			return AddCustomsChargeToInvoice("", 0);
		}

		protected InvoicingLineBase AddCustomsChargeToInvoice(string chargeCode_Code, short printSequence, InvoicingBase invoicingBase = null)
		{
			AccChargeCode chargeCode = Factory.LoadFromNaturalKey<AccChargeCode>(AccChargeCodeSchema.AC_Code, chargeCode_Code);
			chargeCode = chargeCode ?? GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
			chargeCode.AC_PrintSequence = printSequence;
			if (!string.IsNullOrEmpty(chargeCode_Code))
			{
				chargeCode.AC_Code = chargeCode_Code;
			}

			if (invoicingBase != null)
			{
				InvoicingBase = invoicingBase;
			}
			if (chargeCode.Factory != InvoicingBase.Factory)
			{
				chargeCode.Factory.Save();
			}

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate();
			line.AL_Desc = "Customs Charge";
			line.AL_OSExTaxAmount = 200.00M;
			line.AL_Sequence = (ZShort)2;

			return line;
		}

		protected InvoicingLineBase AddNotGroupedChargeToInvoice()
		{
			return AddNotGroupedChargeToInvoice("", 0);
		}

		protected InvoicingLineBase AddNotGroupedChargeToInvoice(string chargeCode_Code, short printSequence, InvoicingBase invoicingBase = null)
		{
			AccChargeCode chargeCode = Factory.LoadFromNaturalKey<AccChargeCode>(AccChargeCodeSchema.AC_Code, chargeCode_Code);
			chargeCode = chargeCode ?? GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeGroup = "NGC";
			chargeCode.AC_PrintSequence = printSequence;
			if (!string.IsNullOrEmpty(chargeCode_Code))
			{
				chargeCode.AC_Code = chargeCode_Code;
			}

			if (invoicingBase != null)
			{
				InvoicingBase = invoicingBase;
			}
			if (chargeCode.Factory != InvoicingBase.Factory)
			{
				chargeCode.Factory.Save();
			}

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate();
			line.AL_Desc = "Not grouped charge";
			line.AL_OSExTaxAmount = 250.00M;

			return line;
		}

		protected InvoicingLineBase AddCommentChargeToInvoice()
		{
			return AddCommentChargeToInvoice("", 0);
		}

		protected InvoicingLineBase AddCommentChargeToInvoice(string chargeCode_Code, short printSequence)
		{
			AccChargeCode chargeCode = Factory.LoadFromNaturalKey<AccChargeCode>(AccChargeCodeSchema.AC_Code, chargeCode_Code);
			chargeCode = chargeCode ?? GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeType = "CMT";
			chargeCode.AC_ChargeGroup = "NGC";
			chargeCode.AC_PrintSequence = printSequence;
			if (!string.IsNullOrEmpty(chargeCode_Code))
			{
				chargeCode.AC_Code = chargeCode_Code;
			}
			if (chargeCode.Factory != InvoicingBase.Factory)
			{
				chargeCode.Factory.Save();
			}

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_Desc = "Comment charge";
			line.AL_AC = chargeCode.PK;

			return line;
		}

		protected InvoicingLineBase AddGLAccountLineToInvoice(string accountNum, string lineDescription)
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_Description = "TEST DESCRIPTION";
			glHeader.AG_AccountNum = accountNum;

			var result = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			result.AL_AG = glHeader.PK;
			result.AL_AT = GetRate();
			result.AL_Desc = lineDescription;
			result.AL_OSExTaxAmount = 100m;
			return result;
		}

		protected InvoicingLineBase AddGLAccountLineToInvoice()
		{
			return AddGLAccountLineToInvoice("XXXX.XX.XX", "GL Account Line");
		}

		protected InvoicingLineBase AddNotJobChargeToInvoice()
		{
			return AddNotJobChargeToInvoice("", 0);
		}

		protected InvoicingLineBase AddNotJobChargeToInvoice(string chargeCode_Code, short printSequence, InvoicingBase invoicingBase = null)
		{
			AccChargeCode chargeCode = Factory.LoadFromNaturalKey<AccChargeCode>(AccChargeCodeSchema.AC_Code, chargeCode_Code);
			chargeCode = chargeCode ?? GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeGroup = "NJR";
			chargeCode.AC_PrintSequence = printSequence;
			if (!string.IsNullOrEmpty(chargeCode_Code))
			{
				chargeCode.AC_Code = chargeCode_Code;
			}

			if (invoicingBase != null)
			{
				InvoicingBase = invoicingBase;
			}
			if (chargeCode.Factory != InvoicingBase.Factory)
			{
				chargeCode.Factory.Save();
			}

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate();
			line.AL_Desc = "Non job related charge";
			line.AL_OSExTaxAmount = 250.00M;

			return line;
		}

		protected InvoicingLineBase AddCFSLoadListJobChargeToInvoice()
		{
			AccChargeCode chargeCode = GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeGroup = "CLL";
			chargeCode.Factory.Save();

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate();
			line.AL_Desc = "CFS Unpacking Charges";
			line.AL_OSExTaxAmount = 100m;

			return line;
		}

		protected InvoicingLineBase AddCFSShipmentJobChargeToInvoice()
		{
			AccChargeCode chargeCode = GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeGroup = "CSH";
			chargeCode.Factory.Save();

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate();
			line.AL_Desc = "CFS Customs Hold";
			line.AL_OSExTaxAmount = 60m;

			return line;
		}

		protected InvoicingLineBase AddTransportJobChargeToInvoice()
		{
			AccChargeCode chargeCode = GetNewUniqueBizo<AccChargeCode>();
			chargeCode.AC_ChargeGroup = "TRN";
			chargeCode.Factory.Save();

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate();
			line.AL_Desc = "Port Transport Charges";
			line.AL_OSExTaxAmount = 45m;

			return line;
		}

		protected InvoicingLineBase AddWarehouseStorageChargeToInvoice(bool generateRandomUniqueStringForProperties = false)
		{
			AccChargeCode chargeCode = GetNewUniqueBizo<AccChargeCode>(generateRandomUniqueStringForProperties);
			chargeCode.AC_ChargeGroup = "WST";
			chargeCode.AC_Desc = "WST";
			chargeCode.Factory.Save();

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate(generateRandomUniqueStringForProperties);
			line.AL_Desc = "Warehouse Storage";
			line.AL_OSExTaxAmount = 120m;

			return line;
		}

		protected InvoicingLineBase AddWarehouseInwardschargeToInvoice(bool generateRandomUniqueStringForProperties = false)
		{
			AccChargeCode chargeCode = GetNewUniqueBizo<AccChargeCode>(generateRandomUniqueStringForProperties);
			chargeCode.AC_ChargeGroup = "WIN";
			chargeCode.AC_Desc = "WIN";
			chargeCode.Factory.Save();

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate(generateRandomUniqueStringForProperties);
			line.AL_Desc = "Warehouse Receiving";
			line.AL_OSExTaxAmount = 150m;

			return line;
		}

		protected InvoicingLineBase AddWarehouseOutwardsChargeToInvoice(bool generateRandomUniqueStringForProperties = false)
		{
			AccChargeCode chargeCode = GetNewUniqueBizo<AccChargeCode>(generateRandomUniqueStringForProperties);
			chargeCode.AC_ChargeGroup = "WOU";
			chargeCode.AC_Desc = "WOU";
			chargeCode.Factory.Save();

			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GetRate(generateRandomUniqueStringForProperties);
			line.AL_Desc = "Warehouse Orders";
			line.AL_OSExTaxAmount = 75m;

			return line;
		}

		protected ZGuid GetRate(bool generateRandomUniqueStringForProperties = false)
		{
			AccTaxRate taxRate = GetNewUniqueBizo<AccTaxRate>(generateRandomUniqueStringForProperties);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.SetRate_ForTestOnly(1011, 100);
			AccInvMsg message = taxRate.Factory.NewWithValidTestData<AccInvMsg>();
			message.A9_EnglishMsg = taxRate.AT_Code;
			message.A9_LocalMsg = taxRate.AT_Code + " Local";
			taxRate.AT_A9_DefaultVatClass = message.PK;
			taxRate.Factory.Save();
			return taxRate.PK;
		}

		T GetNewUniqueBizo<T>(bool generateRandomUniqueStringForProperties = false) where T : BusinessObject
		{
			var newFactory = new BusinessObjectFactory(generateRandomUniqueStringForProperties);
			T bizo = null;
			ZQuery filter = null;

			do
			{
				if (bizo != null)
				{
					bizo.Delete();
				}
				bizo = newFactory.NewWithValidTestData<T>(TestBusinessObjectKind.MinimumRequiredToSave);
				filter = new ZQuery(ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(CodePropertyAttribute.CodePropertyNameFromType(typeof(T)), bizo.TableName), CodePropertyAttribute.CodeFromBusinessObject(bizo));
				filter.AddToFilter(ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(bizo.TableName), SQLComparisonOperator.NotEqual, bizo.PK);
			}
			while (newFactory.LoadTop1<T>(filter) != null);

			newFactory.Save();

			return bizo;
		}

		void SetLineTaxRate(InvoicingBase invoice, ZGuid rate)
		{
			foreach (InvoicingLineBase line in InvoicingBase.Lines)
			{
				line.AL_AT = rate;

				if (((ILineMatching)line).Charge != null)
				{
					if (line.AL_LineType == "REV" || line.AL_LineType == "WIP")
					{
						((ILineMatching)line).Charge.JR_AT_SellGSTRate = line.AL_AT;
					}
					else
					{
						((ILineMatching)line).Charge.JR_AT_CostGSTRate = line.AL_AT;
					}
				}
			}
		}

		protected void SetupInvoiceLinesAndCharges()
		{
			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddDestinationChargeToInvoice();
			InvoicingLineBase line3 = AddFreightChargeToInvoice();
			InvoicingLineBase line4 = AddBrokerageChargeToInvoice();
			InvoicingLineBase line5 = AddLoadChargeToInvoice();
			InvoicingLineBase line6 = AddUnLoadChargeToInvoice();
			InvoicingLineBase line7 = AddInsuranceChargeToInvoice();
			InvoicingLineBase line8 = AddCustomsChargeToInvoice();
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice();
			InvoicingLineBase line10 = AddNotJobChargeToInvoice();
			InvoicingLineBase line11 = AddCommentChargeToInvoice();

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 11, theLines.Count);

			SetUpDepartmentAndDirection();
		}

		void SetupInvoiceLinesAndChargesWithPrintSequence()
		{
			int currentIndex = 0;
			InvoicingLineBase line = AddOriginChargeToInvoice("ORG", 10);
			line.AL_Sequence = (short)currentIndex;
			line = AddDestinationChargeToInvoice("DST", 7);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddFreightChargeToInvoice("FRC", 4);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddBrokerageChargeToInvoice("BRK", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddLoadChargeToInvoice("LDC", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddUnLoadChargeToInvoice("ULC", 3);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddInsuranceChargeToInvoice("INS", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddCustomsChargeToInvoice("CUS", 2);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddNotGroupedChargeToInvoice("NGC", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddCommentChargeToInvoice("CMT", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddNotJobChargeToInvoice("NJC", 1);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddGLAccountLineToInvoice();
			line.AL_Sequence = (short)(++currentIndex);
			SetUpDepartmentAndDirection();
		}

		void SetupInvoiceLinesAndManyChargesWithPrintSequence()
		{
			int currentIndex = 0;
			InvoicingLineBase line = AddOriginChargeToInvoice("ORG", 10);
			line.AL_Sequence = (short)currentIndex;
			line = AddOriginChargeToInvoice("1_ORG", 103);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddOriginChargeToInvoice("2_ORG", 101);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddDestinationChargeToInvoice("DST", 7);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddDestinationChargeToInvoice("3_DST", 72);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddDestinationChargeToInvoice("2_DST", 73);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddFreightChargeToInvoice("FRC", 4);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddFreightChargeToInvoice("1_FRC", 43);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddFreightChargeToInvoice("2_FRC", 41);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddBrokerageChargeToInvoice("BRK", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddBrokerageChargeToInvoice("5_BRK", 2);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddBrokerageChargeToInvoice("6_BRK", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddLoadChargeToInvoice("LDC", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddLoadChargeToInvoice("7_LDC", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddLoadChargeToInvoice("8_LDC", 55);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddUnLoadChargeToInvoice("ULC", 61);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddUnLoadChargeToInvoice("3_ULC", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddUnLoadChargeToInvoice("6_ULC", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddInsuranceChargeToInvoice("INS", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddInsuranceChargeToInvoice("7_INS", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddInsuranceChargeToInvoice("4_INS", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddCustomsChargeToInvoice("CUS", 21);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddCustomsChargeToInvoice("6_CUS", 22);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddCustomsChargeToInvoice("4_CUS", 23);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddNotGroupedChargeToInvoice("NGC", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddNotGroupedChargeToInvoice("4_NGC", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddNotGroupedChargeToInvoice("5_NGC", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddCommentChargeToInvoice("CMT", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddCommentChargeToInvoice("4_CMT", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddCommentChargeToInvoice("5_CMT", 0);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddNotJobChargeToInvoice("NJC", 12);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddNotJobChargeToInvoice("1_NJC", 14);
			line.AL_Sequence = (short)(++currentIndex);
			line = AddNotJobChargeToInvoice("2_NJC", 1);
			line.AL_Sequence = (short)(++currentIndex);

			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			AssertEquals("Count of lines", 33, theLines.Count);

			SetUpDepartmentAndDirection();
		}

		void SetupTransactionOrgProxiesForTaxIDTest(bool isTransactionBranchOrgProxyExist = false, bool isTaxBranchOrgProxyExist = false)
		{
			var invoiceBranch = Factory.NewWithValidTestData<GlbBranch>();
			invoiceBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			InvoicingBase.AH_GB = invoiceBranch.PK;

			var taxBranch = Factory.NewWithValidTestData<GlbBranch>();
			InvoicingBase.AH_GB_TaxBranch = taxBranch.PK;

			if (isTransactionBranchOrgProxyExist)
			{
				var transactionBranchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
				invoiceBranch.GB_OH_OrgProxy = transactionBranchOrgProxy.PK;
			}
			else
			{
				invoiceBranch.GB_OH_OrgProxy = ZGuid.Empty;
			}

			if (isTaxBranchOrgProxyExist)
			{
				var taxBranchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
				taxBranch.GB_OH_OrgProxy = taxBranchOrgProxy.PK;
			}
			else
			{
				taxBranch.GB_OH_OrgProxy = ZGuid.Empty;
			}

			SetupTaxRateLine();
		}

		void SetupTaxRateLine()
		{
			var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			var vat = Factory.NewWithValidTestData<AccTaxRate>();
			line.AL_AT = vat.PK;
		}

		protected void AssertLines(bool isWarehouseBilling = false)
		{
			//Factory.Save();

			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(Invoice, (invoice, factory) => DocARInvoice.New(invoice, factory));
			DocARInvoiceLineCollection theLines = ARInvoiceWrapper.LinesForInvoice;
			theLines.Sort("LineDescription", ListSortDirection.Ascending);
			AssertEquals("Count of lines", 8, theLines.Count);

			//First Line
			AssertEquals("Description", "Brokerage", theLines[0].LineDescription);
			AssertEquals("Amount", 300.00M, theLines[0].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=30.33 *", theLines[0].OSTaxDisplay);

			//Second Line
			AssertEquals("Description", "Comment charge", theLines[1].LineDescription);
			AssertEquals("Amount", 0M, theLines[1].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "", theLines[1].OSTaxDisplay);

			//Third Line
			AssertEquals("Description", "Customs Charge", theLines[2].LineDescription);
			AssertEquals("Amount", 200.00M, theLines[2].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=20.22 *", theLines[2].OSTaxDisplay);

			//Fourth Line
			AssertEquals("Description", "Destination Charges", theLines[3].LineDescription);
			AssertEquals("Amount", 600.00M, theLines[3].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=60.66 *", theLines[3].OSTaxDisplay);

			//Fifth Line
			AssertEquals("Description", "Freight and Insurance Charges", theLines[4].LineDescription);
			AssertEquals("Amount", 700.00M, theLines[4].OSExTaxAmount);
			if (isWarehouseBilling)
			{
				AssertEquals("OSTaxDisplay", "10.11%=70.75 *", theLines[4].OSTaxDisplay);
			}
			else
			{
				AssertEquals("OSTaxDisplay", "10.11%=70.78 *", theLines[4].OSTaxDisplay);
			}

			//Sixth Line
			AssertEquals("Description", "Non job related charge", theLines[5].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[5].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", theLines[5].OSTaxDisplay);

			//Seventh Line
			AssertEquals("Description", "Not grouped charge", theLines[6].LineDescription);
			AssertEquals("Amount", 250.00M, theLines[6].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=25.28 *", theLines[6].OSTaxDisplay);

			//Eighth Line
			AssertEquals("Description", "Origin Charges", theLines[7].LineDescription);
			AssertEquals("Amount", 500.00M, theLines[7].OSExTaxAmount);
			AssertEquals("OSTaxDisplay", "10.11%=50.56 *", theLines[7].OSTaxDisplay);
		}

		#endregion

		AccTaxTransaction CreateTaxRecord(GlbCompany currentCompany, ZString currencyCode)
		{
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_GC = currentCompany.PK;
			taxRecord.ATT_RX_NKOSTaxCurrency = currencyCode;
			return taxRecord;
		}

		AccTaxRecordTransactionLinePivot CreateTaxRecordTransactionLinePivot(AccTaxTransaction taxRecord, InvoicingLineBase line)
		{
			var linePivot = Factory.New<AccTaxRecordTransactionLinePivot>();
			linePivot.ATP_ATT = taxRecord.PK;
			var taxableLine = (ITaxableTransactionLine)TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line);
			linePivot.LinkLine(taxableLine);
			return linePivot;
		}

		#endregion

		#endregion
		DocARInvoiceCommon SetupInvoiceWithOrgHeader(ZString[] orgCodeTypes, ZString country)
		{
			var invoice = ARInvoiceWrapper;
			invoice.TransactionHeader.Company.GC_RN_NKCountryCode = country;
			var orgHeader = Factory.New<OrgHeader>();
			InvoicingBase.AH_OH = orgHeader.PK;
			foreach (ZString orgCodeType in orgCodeTypes)
			{
				orgHeader.CustomsCodes.AddNew(orgCodeType, "100", country);
			}
			return invoice;
		}
	}
}
