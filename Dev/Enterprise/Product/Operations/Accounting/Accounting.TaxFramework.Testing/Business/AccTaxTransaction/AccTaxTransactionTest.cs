using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	[TestedType(typeof(AccTaxTransaction))]
	public class AccTaxTransactionTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var taxRecord = Factory.New<AccTaxTransaction>();
			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			taxRecord.ATT_ETC = taxConfig.PK;

			taxRecord.ATT_GC = GlbCompany.CurrentCompany.PK;

			return taxRecord;
		}

		public void TestEffectiveRate()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;
			AssertEquals(0m, taxRecord.EffectiveRate);

			AssertExceptionThrown<InvalidOperationException>("", "EffectiveRate is not set.", () => taxRecord.CalculateTaxAmountFromTaxBaseAmounts());

			ZDecimal expectedRate = -12.321;
			taxRecord.SetEffectiveRateOnTaxRecordCreation(expectedRate);
			AssertEquals(expectedRate, taxRecord.EffectiveRate);

			AssertExceptionThrown<InvalidOperationException>("Set another rate", "EffectiveRate is already set.", () => taxRecord.SetEffectiveRateOnTaxRecordCreation(1));
			AssertNoExceptionThrown("Set the same rate as before", () => taxRecord.SetEffectiveRateOnTaxRecordCreation(expectedRate));

			taxRecord.ATT_OSTaxBaseAmount = 100;
			taxRecord.ATT_LocalTaxBaseAmount = 200;
			taxRecord.CalculateTaxAmountFromTaxBaseAmounts();
			AssertEquals(-1232.1m, taxRecord.ATT_OSTaxAmount);
			AssertEquals(-2464.2m, taxRecord.ATT_LocalTaxAmount);
		}

		public void TestExchangeRate()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;
			taxRecord.ATT_OSTaxAmount = 11m;
			taxRecord.ATT_LocalTaxAmount = 110m;
			taxRecord.TransactionHeader.AH_ExchangeRate = 0.1m;
			taxRecord.TransactionHeader.AH_RX_NKTransactionCurrency = "USD";

			AssertEquals(0.1m, taxRecord.ExchangeRate);

			taxRecord.Company.GC_IsReciprocal = true;

			AssertEquals(10m, taxRecord.ExchangeRate);
		}

		public void TestExchangeRate_WhenCompanyIsNull_AfterRestoringTaxTransaction()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;
			taxRecord.ATT_GC = ZGuid.Empty;
			taxRecord.ATT_OSTaxAmount = 11m;
			taxRecord.ATT_LocalTaxAmount = 110m;

			AssertEquals(0.1m, taxRecord.ExchangeRate);
		}

		public void TestCalculateTaxAmountFromTaxBaseAmounts()
		{
			SetupRoundingMethodApplierMock();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;
			AssertEquals(0m, taxRecord.EffectiveRate);

			AssertExceptionThrown<InvalidOperationException>("", "EffectiveRate is not set.", () => taxRecord.CalculateTaxAmountFromTaxBaseAmounts());

			ZDecimal expectedRate = -12.14;
			taxRecord.SetEffectiveRateOnTaxRecordCreation(expectedRate);
			AssertEquals(expectedRate, taxRecord.EffectiveRate);

			taxRecord.ATT_OSTaxBaseAmount = 100.64;
			taxRecord.ATT_LocalTaxBaseAmount = 200.57;

			var validator = new Mock<IAccTaxTransactionCriticalValidator>();
			taxFrameworkDependencyFactory.Setup(x => x.GetAccTaxTransactionCriticalValidator(taxRecord)).Returns(validator.Object);

			Factory.Save();

			foreach (var taxRoundingMethod in new ZString[] { TaxAmountRoundingMethods.Standard.Code, TaxAmountRoundingMethods.RoundDownToMinorUnit.Code })
			{
				var taxConfigurationInNewFactory = new BusinessObjectFactory().Load<AccTaxConfiguration>(taxRecord.ATT_ETC);
				taxConfigurationInNewFactory.ETC_TaxAmountRounding = taxRoundingMethod;
				taxConfigurationInNewFactory.Factory.Save();

				foreach (var osCurrency in new ZString[] { CurrencyCodes.Kuwait, taxRecord.Company.GC_RX_NKLocalCurrency })
				{
					var osCurrencyDecimalPlaces = osCurrency == CurrencyCodes.Kuwait ? 3 : 2;
					var localCurrencyDecimalPlaces = 2;

					ChangeOSCurrencyButRetainAmounts(taxRecord, osCurrency);
					roundingMethodApplier.Invocations.Clear();

					AssertNoExceptionThrown(() => taxRecord.CalculateTaxAmountFromTaxBaseAmounts());
					AssertRoundingMethodInvoked(taxRoundingMethod, -1221.7696, -2434.9198, osCurrency != taxRecord.Company.GC_RX_NKLocalCurrency, osCurrencyDecimalPlaces, localCurrencyDecimalPlaces, true);
				}
			}
		}

		void AssertRoundingMethodInvoked(ZString roundingMethod, ZDecimal osTaxAmount, ZDecimal localTaxAmount, ZBool isTaxAmountInForeignCurrency, ZInt osCurrencyDecimalPlaces, ZInt localCurrencyDecimalPlaces, bool clearMockInvocations = true)
		{
			roundingMethodApplier.Verify(x => x.ApplyRounding(It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZBool>(), It.IsAny<ZInt>()), Times.Exactly(2));
			roundingMethodApplier.Verify(x => x.ApplyRounding(roundingMethod, osTaxAmount, isTaxAmountInForeignCurrency, osCurrencyDecimalPlaces), Times.Once);
			roundingMethodApplier.Verify(x => x.ApplyRounding(roundingMethod, localTaxAmount, false, localCurrencyDecimalPlaces), Times.Once);

			if (clearMockInvocations)
			{
				roundingMethodApplier.Invocations.Clear();
			}
		}

		public void TestTaxTransactionLocalAmountAdjustedAmongstPivotsOnSaving()
		{
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();

			var pivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord.PK));
			AssertEquals(1, pivots.Length);
			var pivot1 = pivots[0];
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord.PK;
			pivot2.FillWithValidTestData();
			pivot2.ATP_LocalTaxAmount = 10m;

			var amountsDistributorMock = new Mock<ITaxTransactionToPivotsAmountsDistributor>();
			taxRecord.SubstituteTaxTransactionToPivotsAmountsDistributor_ForTestOnly(amountsDistributorMock.Object);
			Factory.Save();
			amountsDistributorMock.Verify(d => d.AdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(taxRecord), Times.Once);

			Assert(taxRecord.IsInDatabase);
			Factory.Save();
			amountsDistributorMock.Verify(d => d.AdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(taxRecord), Times.Once);
		}

		public void TestTaxTransactionLocalAmountNotAdjustedAmongstPivotsForCancelledTransactions()
		{
			var amountsDistributorMock = new Mock<ITaxTransactionToPivotsAmountsDistributor>();
			var taxRecord1 = CreateTaxRecord();
			Factory.Save();

			Assert("Precondition: Transaction is not cancelled", !taxRecord1.IsCancelled);
			amountsDistributorMock.Verify(d => d.AdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(taxRecord1), Times.Once);
			amountsDistributorMock.Reset();

			var taxRecord2 = CreateTaxRecord();
			taxRecord2.IsCancelled = true;
			Factory.Save();

			Assert("Pre-condition: Transaction is Cancelled", taxRecord2.IsCancelled);
			amountsDistributorMock.Verify(d => d.AdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(taxRecord2), Times.Never);

			AccTaxTransaction CreateTaxRecord()
			{
				var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
				var pivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord.PK));
				AssertEquals("Pivots Count", 1, pivots.Length);

				pivots[0].ATP_LocalTaxAmount = 10m;

				taxRecord.SubstituteTaxTransactionToPivotsAmountsDistributor_ForTestOnly(amountsDistributorMock.Object);
				return taxRecord;
			}
		}

		public void TestATT_TaxAuthorityServiceCodeMaxLength()
		{
			AssertEquals(AccTaxOverrideGroupTaxConfigurationPivot.Schema.AXP_TaxAuthorityServiceCodeMaxLength, AccTaxTransaction.Schema.ATT_TaxAuthorityServiceCodeMaxLength);
			AssertEquals(AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_TaxAuthorityServiceCode.MaxLength, AccTaxTransactionSchema.ATT_TaxAuthorityServiceCode.MaxLength);
		}

		public void TestATT_TaxAuthorityServiceCodeDescriptionMaxLength()
		{
			AssertEquals(AccTaxOverrideGroupTaxConfigurationPivot.Schema.AXP_TaxAuthorityServiceCodeDescriptionMaxLength, AccTaxTransaction.Schema.ATT_TaxAuthorityServiceCodeDescriptionMaxLength);
			AssertEquals(AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_TaxAuthorityServiceCodeDescription.MaxLength, AccTaxTransactionSchema.ATT_TaxAuthorityServiceCodeDescription.MaxLength);
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		public void TestDelete()
		{
			var taxRecord1 = Factory.New<AccTaxTransaction>();
			Assert("Precondition: The Tax Record created is not in database", !taxRecord1.IsInDatabase);

			taxRecord1.Delete();

			Assert(taxRecord1.IsDeleted);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			var taxRecord2 = Factory.NewWithValidTestData<AccTaxTransaction>();
			Factory.Save();
			Assert("Precondition: The Tax Record created is in database", taxRecord2.IsInDatabase);

			taxRecord2.Delete();

			AssertEquals("AccTaxTransactionInDBCannotBeDeleted", ErrorReporter.LastKeyReported);
			AssertContains($@"Tax Record in database cannot be deleted.
	PK = {taxRecord2.PK}
	Type = AccTaxTransaction", ErrorReporter.LastMessageReported);
			Assert(!taxRecord2.IsDeleted);
			ErrorReporter.Clear();
		}

		public void TestDependencies()
		{
			var taxRecord = Factory.New<AccTaxTransaction>();
			AssertType<GLMovementProcessor>(taxRecord.GLMovementProcessor_ExposedForTestOnly);
		}

		public void TestAnyFactorySaveCallsCreateGLMovements()
		{
			var glMovementProcessorMock = new Mock<IGLMovementProcessor>();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;
			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxRecord.SubstituteGLMovementProcessor_ForTestOnly(glMovementProcessorMock.Object);

			var taxRecord2 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord2.ATT_AH = invoice.PK;
			taxRecord2.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxRecord2.SubstituteGLMovementProcessor_ForTestOnly(glMovementProcessorMock.Object);

			var invoice3 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord3 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord3.ATT_AH = invoice.PK;
			taxRecord3.ATT_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			taxRecord3.SubstituteGLMovementProcessor_ForTestOnly(glMovementProcessorMock.Object);

			Assert("Precondition: taxRecord.IsInDatabase", !taxRecord.IsInDatabase);
			Assert("Precondition: taxRecord2.IsInDatabase", !taxRecord2.IsInDatabase);
			Assert("Precondition: taxRecord3.IsInDatabase", !taxRecord3.IsInDatabase);

			Factory.Save();
			glMovementProcessorMock.Verify(x => x.CreateGLMovements(It.IsAny<AccTaxTransaction>()), Times.Exactly(3));
			glMovementProcessorMock.Verify(x => x.CreateGLMovements(taxRecord), Times.Once);
			glMovementProcessorMock.Verify(x => x.CreateGLMovements(taxRecord2), Times.Once);
			glMovementProcessorMock.Verify(x => x.CreateGLMovements(taxRecord3), Times.Once);
			glMovementProcessorMock.Verify(x => x.DeleteGLMovementsNotInDB(It.IsAny<AccTaxTransaction>()), Times.Never);
			glMovementProcessorMock.Reset();

			Assert("Postcondition: taxRecord.IsInDatabase", taxRecord.IsInDatabase);
			Assert("Postcondition: taxRecord2.IsInDatabase", taxRecord2.IsInDatabase);
			Assert("Postcondition: taxRecord3.IsInDatabase", taxRecord3.IsInDatabase);
			Assert("Postcondition: taxRecord.HasChanges", !taxRecord.HasChanges);
			Assert("Postcondition: taxRecord2.HasChanges", !taxRecord2.HasChanges);
			Assert("Postcondition: taxRecord3.HasChanges", !taxRecord3.HasChanges);

			Factory.Save();
			glMovementProcessorMock.Verify(x => x.CreateGLMovements(It.IsAny<AccTaxTransaction>()), Times.Never);
		}

		public void TestAnyFactorySaveCallsDeleteGLMovementsNotInDBOnFailedSaving()
		{
			var glMovementProcessorMock = new Mock<IGLMovementProcessor>();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxRecord.SubstituteGLMovementProcessor_ForTestOnly(glMovementProcessorMock.Object);

			Assert("Precondition: taxRecord.IsInDatabase", !taxRecord.IsInDatabase);

			taxRecord.ATT_AH = invoice.PK;
			Factory.Save();
			glMovementProcessorMock.Verify(x => x.CreateGLMovements(It.IsAny<AccTaxTransaction>()), Times.Once);
			glMovementProcessorMock.Verify(x => x.CreateGLMovements(taxRecord), Times.Once);
			glMovementProcessorMock.Verify(x => x.DeleteGLMovementsNotInDB(It.IsAny<AccTaxTransaction>()), Times.Never);
			glMovementProcessorMock.Reset();
			Assert("Postcondition: taxRecord.IsInDatabase", taxRecord.IsInDatabase);
			Assert("Postcondition: taxRecord.HasChanges", !taxRecord.HasChanges);

			taxRecord.ATT_AH = ZGuid.NewZGuid();
			AssertExceptionThrown(typeof(ZSaveException), () => Factory.Save());
			glMovementProcessorMock.Verify(x => x.CreateGLMovements(It.IsAny<AccTaxTransaction>()), Times.Once);
			glMovementProcessorMock.Verify(x => x.CreateGLMovements(taxRecord), Times.Once);
			glMovementProcessorMock.Verify(x => x.DeleteGLMovementsNotInDB(It.IsAny<AccTaxTransaction>()), Times.Once);
			glMovementProcessorMock.Verify(x => x.DeleteGLMovementsNotInDB(taxRecord), Times.Once);
		}

		#region IDataVersionLoggingSupported

		[SuspendCriticalValidation]
		public void TestIsDataVersionsAutoLogged()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;
			taxRecord.FillWithValidTestData();
			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;

			Assert("Precondition: IsInDatabase", !taxRecord.IsInDatabase);
			AssertEquals("A taxRecord that should not log all it's data on the first save if nothing has changed. This is because there is no need to create a copy of every tax record if nobody has changed it.", false, ((IDataVersionLoggingSupported)taxRecord).IsDataVersionsAutoLogged);

			var originalValue = new ZDecimal(-100M);
			var newValue = new ZDecimal(-180M);

			Factory.SetContext(BusinessContext.SystemGeneratedTaxRecordCreationInProgress);
			taxRecord.ATT_OSTaxBaseAmount = originalValue;
			taxRecord.SetTaxTransactionsSystemCalculatedValues(originalValue, taxRecord.ATT_OSTaxAmount, taxRecord.ATT_RateNumerator, taxRecord.ATT_RateDenominator, taxRecord.ATT_TaxDate, taxRecord.ATT_TaxAuthorityServiceCode, taxRecord.ATT_TaxAuthorityServiceCodeDescription);
			Factory.RemoveContext(BusinessContext.SystemGeneratedTaxRecordCreationInProgress);
			AssertEquals("Still nothing to be logged since user has not changed any values yet.", false, ((IDataVersionLoggingSupported)taxRecord).IsDataVersionsAutoLogged);

			taxRecord.ATT_OSTaxBaseAmount = newValue;
			AssertEquals("This time the tax record will log data version log since a value has changed.", true, ((IDataVersionLoggingSupported)taxRecord).IsDataVersionsAutoLogged);

			taxRecord.Factory.Save();
			Assert("Precondition: IsInDatabase", taxRecord.IsInDatabase);
			AssertEquals("Changes to saved taxRecord triggers auto logging.", true, ((IDataVersionLoggingSupported)taxRecord).IsDataVersionsAutoLogged);

			foreach (var ledger in new TaxConfigurationLedgers().GetAllCodes().Where(x => x != TaxConfigurationLedgers.AccountsPayable.Code))
			{
				taxRecord.ATT_Ledger = ledger;
				AssertEquals("Only AP tax records triggers auto logging.", false, ((IDataVersionLoggingSupported)taxRecord).IsDataVersionsAutoLogged);
			}

			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			foreach (var type in new TaxSuperTypeList().GetAllCodes())
			{
				taxRecord.ATT_TaxSuperType = type;
				AssertEquals("Only AP tax records triggers auto logging.", true, ((IDataVersionLoggingSupported)taxRecord).IsDataVersionsAutoLogged);
			}
		}

		public void TestEnterpriseBusinessObjectIsAudited()
		{
			var columns = new SchemaColumn[]
			{
				AccTaxTransactionSchema.PK,
				AccTaxTransactionSchema.ATT_OSTaxBaseAmount,
				AccTaxTransactionSchema.ATT_OSTaxAmount,
				AccTaxTransactionSchema.ATT_RateNumerator,
				AccTaxTransactionSchema.ATT_RateDenominator,
				AccTaxTransactionSchema.ATT_TaxDate,
				AccTaxTransactionSchema.ATT_TaxAuthorityServiceCode,
				AccTaxTransactionSchema.ATT_TaxAuthorityServiceCodeDescription
			};
			AuditLogsHelperForTesting.AssertColumnsExistsInAuditDb(Factory, AccTaxTransactionSchema.PK.TableSchema.SqlSchemaName, AccTaxTransactionSchema.PK.TableName, columns);
		}

		#endregion

		public void TestIsInDatabase_ForBindingHasPropertyInfo()
		{
			var taxRecord = Factory.New<AccTaxTransaction>();
			AssertNotNull("ZPropertyInfo is required for IsInDatabase_ForBinding for ZGrid.TickUntickAll() to work without NullReferenceException on 'Saved' column of Tax Records grid", taxRecord.ZPropertyInfoHash.GetPropertySafe(nameof(taxRecord.IsInDatabase_ForBinding)));
		}

		public void TestATT_AH_MatchTransaction_ForBinding()
		{
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			AssertEquals(ZGuid.Empty, taxRecord.ATT_AH_MatchTransaction);
			AssertEquals(ZString.Empty, taxRecord.ATT_AH_MatchTransaction_ForBinding);

			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			transaction.AH_TransactionType = TransactionTypes.Journal;
			transaction.AH_TransactionNum = "123";
			taxRecord.ATT_AH_MatchTransaction = transaction.PK;
			AssertEquals("AP JNL 123", taxRecord.ATT_AH_MatchTransaction_ForBinding);

			Factory.Save();
			taxRecord = NewFactory().Load<AccTaxTransaction>(taxRecord.PK);
			AssertEquals("AP JNL 123", taxRecord.ATT_AH_MatchTransaction_ForBinding);
		}

		public void TestTaxAuthorityCode()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;
			taxRecord.ATT_LocalTaxAmount = 0;   // To allow changes to ATT_ETC
			taxRecord.ATT_OSTaxAmount = 0;      // To allow changes to ATT_ETC

			taxRecord.ATT_ETC = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, taxRecord.ATT_ETC);
			AssertEquals(ZString.Empty, taxRecord.TaxAuthorityCode);

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			taxConfig.ETC_TaxAuthorityCode = "AUT";
			taxRecord.ATT_ETC = taxConfig.PK;
			AssertEquals("AUT", taxRecord.TaxAuthorityCode);

			Factory.Save();
			taxRecord = NewFactory().Load<AccTaxTransaction>(taxRecord.PK);
			AssertEquals("AUT", taxRecord.TaxAuthorityCode);
		}

		public void TestIsOSTaxCurrencyLocal()
		{
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_GC = Factory.New<GlbCompany>().PK;
			taxRecord.ATT_RX_NKOSTaxCurrency = "USD";
			taxRecord.Company.GC_RX_NKLocalCurrency = "USD";
			Assert(taxRecord.IsOSTaxCurrencyLocal);

			taxRecord.Company.GC_RX_NKLocalCurrency = "NZD";
			Assert(!taxRecord.IsOSTaxCurrencyLocal);

			taxRecord.ATT_RX_NKOSTaxCurrency = "NZD";
			Assert(taxRecord.IsOSTaxCurrencyLocal);
		}

		public void TestIsOSTaxCurrencyLocal_False_WhenCompanyIsNull_AfterRestoringTaxTransaction()
		{
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_GC = ZGuid.Empty;
			taxRecord.ATT_RX_NKOSTaxCurrency = "USD";
			AssertEquals(expected: false, taxRecord.IsOSTaxCurrencyLocal);
		}

		public void TestHumanReadableName()
		{
			var taxRecord = Factory.New<AccTaxTransaction>();
			AssertEquals("Tax record", taxRecord.HumanReadableName);
		}

		public void TestConcurrencyPolicy()
		{
			var taxRecordSubscriber = (AccTaxTransaction)GetNewBusinessObject();
			AssertConcurrencyPolicy(taxRecordSubscriber.ATT_AH_MatchTransactionInfo, ConcurrencyPolicy.Strict);
			AssertConcurrencyPolicy(taxRecordSubscriber.ATT_IsCancelledInfo, ConcurrencyPolicy.Strict);
			AssertConcurrencyPolicy(taxRecordSubscriber.ATT_LocalTaxAmountInfo, ConcurrencyPolicy.Strict);
			AssertConcurrencyPolicy(taxRecordSubscriber.ATT_LocalTaxBaseAmountInfo, ConcurrencyPolicy.Strict);
			AssertConcurrencyPolicy(taxRecordSubscriber.ATT_OSTaxAmountInfo, ConcurrencyPolicy.Strict);
			AssertConcurrencyPolicy(taxRecordSubscriber.ATT_OSTaxBaseAmountInfo, ConcurrencyPolicy.Strict);
			AssertConcurrencyPolicy(taxRecordSubscriber.ATT_RealisationDateInfo, ConcurrencyPolicy.Strict);

			void AssertConcurrencyPolicy(ZPropertyInfo propertyInfo, ConcurrencyPolicy policy)
			{
				AssertEquals(propertyInfo.Name, policy, propertyInfo.ConcurrencyPolicy);
			}
		}

		[SuspendCriticalValidation]
		public void TestShouldApplyDataRefreshBusUpdate()
		{
			var expectedType = GetExpectedBusinessObjectType();
			var taxRecordSubscriber = (AccTaxTransaction)Factory.NewWithValidTestData(expectedType);
			Factory.Save();

			var factoryOfPublisher = new BusinessObjectFactory();
			var taxRecordPublisher = factoryOfPublisher.Load<AccTaxTransaction>(taxRecordSubscriber.PK);

			var dataRefreshBusUpdateDeciderMock = new Mock<IDataRefreshBusUpdateActionDecider>();

			AccTaxTransaction passedSubscriber = null, passedPublisher = null;
			ZPropertyInfo[] passedStrictPropertyInfo = null;
			dataRefreshBusUpdateDeciderMock
				.Setup(x => x.ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, It.IsAny<AccTaxTransaction>(), It.IsAny<AccTaxTransaction>(), It.IsAny<ZPropertyInfo[]>()))
				.Returns(true)
				.Callback<DataRefreshAction, BusinessObject, BusinessObject, ZPropertyInfo[]>((action, subscriber, publisher, strictPropertyInfo) =>
				{
					passedSubscriber = (AccTaxTransaction)subscriber;
					passedPublisher = (AccTaxTransaction)publisher;
					passedStrictPropertyInfo = strictPropertyInfo;
				});
			ObjectFactory.Substitute(dataRefreshBusUpdateDeciderMock.Object);

			taxRecordPublisher.ATT_TaxAuthorityServiceCode = "some code";
			taxRecordPublisher.ATT_TaxAuthorityServiceCodeDescription = "some code desc";
			factoryOfPublisher.Save();

			dataRefreshBusUpdateDeciderMock.Verify(x => x.ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, It.IsAny<AccTaxTransaction>(), It.IsAny<AccTaxTransaction>(), It.IsAny<ZPropertyInfo[]>()));
			AssertEquals("Subscriber passed into data refresh bus decider.", taxRecordSubscriber, passedSubscriber);
			AssertEquals("Publisher passed into data refresh bus decider.", taxRecordPublisher, passedPublisher);

			var currentPropertiesWithStrictConcurrency = taxRecordSubscriber.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(info => info.IsPersistent && info.ConcurrencyPolicy == ConcurrencyPolicy.Strict);
			var typeName = GetExpectedBusinessObjectType().Name;
			var errorMessage = $"Concurrency policy of some {typeName} Properties have changed. Please update list of strict concurrency property infos passed into data refresh bus decider in {typeName} class.";
			AssertContainsExactElementsInAnyOrder(errorMessage, currentPropertiesWithStrictConcurrency, passedStrictPropertyInfo);
		}

		[SuspendCriticalValidation]
		public void TestCanDeleteAndReasonForNotAbleToDelete()
		{
			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			AssertEquals("Only unsaved records with AP ledger can be deleted.", taxTransaction.ReasonForNotAbleToDelete);

			taxTransaction.ATT_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			Assert(!taxTransaction.CanDelete);

			taxTransaction.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			Assert(taxTransaction.CanDelete);

			Environment.Env.Security.PayableAllowOverrideTaxTransaction.IsAllowed = false;
			Assert("Not able to delete", !taxTransaction.CanDelete);
			Environment.Env.Security.PayableAllowOverrideTaxTransaction.IsAllowed = true;
			Assert("Able to delete", taxTransaction.CanDelete);

			foreach (var type in new TaxSuperTypeList().GetAllCodes())
			{
				taxTransaction.ATT_TaxSuperType = type;
				Assert(taxTransaction.CanDelete);
			}

			Factory.Save();
			Assert(!taxTransaction.CanDelete);
		}

		[SuspendCriticalValidation]
		public void TestATT_IsCancelled_ReadOnly()
		{
			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxTransaction.ATT_TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code;
			Assert(taxTransaction.ATT_IsCancelledInfo.ReadOnly);

			Factory.Save();
			Assert(!taxTransaction.ATT_IsCancelledInfo.ReadOnly);

			taxTransaction.ATT_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			Assert(taxTransaction.ATT_IsCancelledInfo.ReadOnly);

			taxTransaction.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			Assert(!taxTransaction.ATT_IsCancelledInfo.ReadOnly);

			taxTransaction.ATT_TaxSuperType = TaxSuperTypeList.Perceptions.Code;
			Assert(taxTransaction.ATT_IsCancelledInfo.ReadOnly);

			taxTransaction.ATT_TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code;
			Assert(!taxTransaction.ATT_IsCancelledInfo.ReadOnly);

			taxTransaction.TransactionHeader.AH_IsCancelled = true;
			Assert("Must be read only for reversed transaction", taxTransaction.ATT_IsCancelledInfo.ReadOnly);

			var taxParentPK = taxTransaction.ATT_AH;
			taxTransaction.ATT_AH = ZGuid.Empty;
			Assert("This is not real case, it is just to test null reference case for TransactionHeader", !taxTransaction.ATT_IsCancelledInfo.ReadOnly);
			taxTransaction.ATT_AH = taxParentPK;
			taxTransaction.TransactionHeader.AH_IsCancelled = false;

			AssertSecurityRighsAllowOverrideTaxTransaction("Precondition: ATT_IsCancelled is ", taxTransaction.ATT_IsCancelledInfo);

			taxTransaction.ATT_RealisationDate = ZDate.Today;
			Assert(taxTransaction.ATT_IsCancelledInfo.ReadOnly);
		}

		public void TestSettingTaxRate_SetsNumeratorAndDenominator()
		{
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;

			AssertSecurityRighsAllowOverrideTaxTransaction("Precondition: ATT_Rate is ", taxRecord.ATT_RateInfo);

			taxRecord.ATT_Rate = 10M;
			AssertEquals("Numerator", 10, taxRecord.ATT_RateNumerator);
			AssertEquals("Denominator", 1, taxRecord.ATT_RateDenominator);

			taxRecord.ATT_Rate = 8.75M;
			AssertEquals("Numerator", 875, taxRecord.ATT_RateNumerator);
			AssertEquals("Denominator", 100, taxRecord.ATT_RateDenominator);

			taxRecord.ATT_Rate = 8.00000M;
			AssertEquals("Numerator", 8, taxRecord.ATT_RateNumerator);
			AssertEquals("Denominator", 1, taxRecord.ATT_RateDenominator);

			taxRecord.ATT_Rate = 0.25M;
			AssertEquals("Numerator", 25, taxRecord.ATT_RateNumerator);
			AssertEquals("Denominator", 100, taxRecord.ATT_RateDenominator);

			taxRecord.ATT_Rate = 4.475M;
			AssertEquals("Numerator", 4475, taxRecord.ATT_RateNumerator);
			AssertEquals("Denominator", 1000, taxRecord.ATT_RateDenominator);
		}

		public void TestATT_OSTaxAmountRounding()
		{
			SetupRoundingMethodApplierMock();
			roundingMethodApplier.Setup(x => x.ApplyRounding(It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZBool>(), It.IsAny<ZInt>())).
				Returns((ZString roundingMethod, ZDecimal amount, ZBool isForiegnCurrency, ZInt currencyDecimals) => amount);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;

			AssertExceptionThrown<InvalidOperationException>("There is no valid tax configuration", "Could not find a valid tax configuration.", () => taxRecord.ATT_OSTaxAmount = 15.31M);
			AssertEquals(0M, taxRecord.ATT_OSTaxAmount);

			taxRecord.FillWithValidTestData();
			var osTaxAmount = (ZDecimal)15.31M;
			AssertEquals(10M, taxRecord.ATT_OSTaxAmount);
			taxRecord.ATT_LocalTaxAmount = 20M; // This sets local tax amount to OS tax amount ratio of 2.
			ZDecimal ratio = 2;
			roundingMethodApplier.Invocations.Clear();

			roundingMethodApplier.Setup(x => x.ApplyRounding(TaxAmountRoundingMethods.Standard.Code, osTaxAmount, false, 2)).
				Returns((ZString roundingMethod, ZDecimal amount, ZBool isForiegnCurrency, ZInt currencyDecimals) => 15M);

			AssertNoExceptionThrown(() => taxRecord.ATT_OSTaxAmount = osTaxAmount);
			roundingMethodApplier.Verify(x => x.ApplyRounding(It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZBool>(), It.IsAny<ZInt>()), Times.Exactly(2));
			roundingMethodApplier.Verify(x => x.ApplyRounding(TaxAmountRoundingMethods.Standard.Code, osTaxAmount, false, 2), Times.Once);
			roundingMethodApplier.Verify(x => x.ApplyRounding(TaxAmountRoundingMethods.Standard.Code, ratio * 15M, false, 2), Times.Once);
			roundingMethodApplier.Invocations.Clear();

			osTaxAmount = 12.32M;
			Factory.SetContext(BusinessContext.CopyingPersistentValues);
			AssertNoExceptionThrown(() => taxRecord.ATT_OSTaxAmount = osTaxAmount);
			Factory.RemoveContext(BusinessContext.CopyingPersistentValues);
			roundingMethodApplier.Verify(x => x.ApplyRounding(It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZBool>(), It.IsAny<ZInt>()), Times.Never);
		}

		public void TestATT_LocalTaxAmountRounding()
		{
			SetupRoundingMethodApplierMock();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;

			AssertExceptionThrown<InvalidOperationException>("There is no valid tax configuration", "Could not find a valid tax configuration.", () => taxRecord.ATT_LocalTaxAmount = 15.31M);
			AssertEquals(0M, taxRecord.ATT_LocalTaxAmount);

			taxRecord.FillWithValidTestData();
			var localTaxAmount = (ZDecimal)15.31M;
			roundingMethodApplier.Invocations.Clear();

			AssertNoExceptionThrown(() => taxRecord.ATT_LocalTaxAmount = localTaxAmount);
			roundingMethodApplier.Verify(x => x.ApplyRounding(It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZBool>(), It.IsAny<ZInt>()), Times.Exactly(1));
			roundingMethodApplier.Verify(x => x.ApplyRounding(TaxAmountRoundingMethods.Standard.Code, localTaxAmount, false, 2), Times.Once);
			roundingMethodApplier.Invocations.Clear();

			localTaxAmount = 12.32M;
			Factory.SetContext(BusinessContext.CopyingPersistentValues);
			AssertNoExceptionThrown(() => taxRecord.ATT_LocalTaxAmount = localTaxAmount);
			Factory.RemoveContext(BusinessContext.CopyingPersistentValues);
			roundingMethodApplier.Verify(x => x.ApplyRounding(It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZBool>(), It.IsAny<ZInt>()), Times.Never);
		}

		public void TestATT_LocalTaxAmountRounding_WhenCompanyIsNull_AfterRestoringTaxTransaction()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;

			taxRecord.FillWithValidTestData();
			taxRecord.ATT_LocalTaxAmount = (ZDecimal)15.31M;

			AssertEquals((ZDecimal)15.31, taxRecord.ATT_LocalTaxAmount);

			taxRecord.ATT_GC = ZGuid.Empty;
			taxRecord.ATT_LocalTaxAmount = 47.63;

			AssertEquals((ZDecimal)48, taxRecord.ATT_LocalTaxAmount);
		}

		public void TestATT_OSTaxAmount_IsAssignedExactValue_When_CopyingPersistentValues_ContextSet()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;

			taxRecord.ATT_OSTaxAmount = 11.2345m;
			AssertEquals(11.23m, taxRecord.ATT_OSTaxAmount);

			Factory.SetContext(BusinessContext.CopyingPersistentValues);
			taxRecord.ATT_OSTaxAmount = 20.5641m;
			Factory.RemoveContext(BusinessContext.CopyingPersistentValues);

			AssertEquals(20.5641m, taxRecord.ATT_OSTaxAmount);
		}

		public void TestATT_LocalTaxAmount_IsAssignedExactValue_When_CopyingPersistentValues_ContextSet()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;

			taxRecord.ATT_LocalTaxAmount = 11.2345m;
			AssertEquals(11.23m, taxRecord.ATT_LocalTaxAmount);

			Factory.SetContext(BusinessContext.CopyingPersistentValues);
			taxRecord.ATT_LocalTaxAmount = 20.5641m;
			Factory.RemoveContext(BusinessContext.CopyingPersistentValues);

			AssertEquals(20.5641m, taxRecord.ATT_LocalTaxAmount);
		}

		public void TestATT_OSTaxBaseAmount_IsAssignedExactValue_When_CopyingPersistentValues_ContextSet()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;

			taxRecord.ATT_OSTaxBaseAmount = 11.2345m;
			AssertEquals(11.23m, taxRecord.ATT_OSTaxBaseAmount);

			Factory.SetContext(BusinessContext.CopyingPersistentValues);
			taxRecord.ATT_OSTaxBaseAmount = 20.5641m;
			Factory.RemoveContext(BusinessContext.CopyingPersistentValues);

			AssertEquals(20.5641m, taxRecord.ATT_OSTaxBaseAmount);
		}

		public void TestATT_LocalTaxBaseAmount_IsAssignedExactValue_When_CopyingPersistentValues_ContextSet()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;

			taxRecord.ATT_LocalTaxBaseAmount = 11.2345m;
			AssertEquals(11.23m, taxRecord.ATT_LocalTaxBaseAmount);

			Factory.SetContext(BusinessContext.CopyingPersistentValues);
			taxRecord.ATT_LocalTaxBaseAmount = 20.5641m;
			Factory.RemoveContext(BusinessContext.CopyingPersistentValues);

			AssertEquals(20.5641m, taxRecord.ATT_LocalTaxBaseAmount);
		}

		public void TestATT_LocalBaseTaxAmountRounding_WhenCompanyIsNull_AfterRestoringTaxTransaction()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;

			taxRecord.FillWithValidTestData();
			taxRecord.ATT_LocalTaxBaseAmount = (ZDecimal)15.31M;

			AssertEquals((ZDecimal)15.31, taxRecord.ATT_LocalTaxBaseAmount);

			taxRecord.ATT_GC = ZGuid.Empty;
			taxRecord.ATT_LocalTaxBaseAmount = 47.63;

			AssertEquals((ZDecimal)48, taxRecord.ATT_LocalTaxBaseAmount);
		}

		public void TestATT_OSTaxBaseAmount_DoNotRecalculateLocalTaxBaseAmount_InContext()
		{
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxRecord.ATT_OSTaxBaseAmount = 20M;
			taxRecord.ATT_LocalTaxBaseAmount = 10M;
			var taxRecordBusinessContext_CopyingPersistentValues = BusinessContext.CopyingPersistentValues;

			Factory.SetContext(taxRecordBusinessContext_CopyingPersistentValues);
			AssertATT_OSTaxBaseAmount("ATT_OSTaxBaseAmount and ATT_LocalTaxBaseAmount in Context", true, 30M, 10M);
			Factory.RemoveContext(taxRecordBusinessContext_CopyingPersistentValues);

			AssertATT_OSTaxBaseAmount("ATT_OSTaxBaseAmount and ATT_LocalTaxBaseAmount when Not in Context", false, 300M, 100M);

			void AssertATT_OSTaxBaseAmount(string message, bool hasTaxRecordBusinessContext, ZDecimal oSTaxBaseAmount, ZDecimal expectedLocalTaxBaseAmount)
			{
				AssertEquals("Precondition:", hasTaxRecordBusinessContext, Factory.HasContext(taxRecordBusinessContext_CopyingPersistentValues));
				taxRecord.ATT_OSTaxBaseAmount = oSTaxBaseAmount;
				AssertEquals(oSTaxBaseAmount, taxRecord.ATT_OSTaxBaseAmount);
				AssertEquals(message, expectedLocalTaxBaseAmount, taxRecord.ATT_LocalTaxBaseAmount);
			}
		}

		public void TestSettingOSTaxBaseAmount_SetsLocalTaxBaseAmount()
		{
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;

			AssertSecurityRighsAllowOverrideTaxTransaction("Precondition: ATT_OSTaxBaseAmount is not editable", taxRecord.ATT_OSTaxBaseAmountInfo);

			AssertEquals("Precondition: OS tax base amount equal to Local tax base amount", taxRecord.ATT_LocalTaxBaseAmount, taxRecord.ATT_OSTaxBaseAmount);
			AssertEquals("Precondition: OS tax amount", 10M, taxRecord.ATT_OSTaxAmount);

			taxRecord.ATT_LocalTaxBaseAmount = 0M; //setting ATT_LocalTaxBaseAmount independently

			taxRecord.ATT_OSTaxBaseAmount = 20M;
			AssertEquals("Setting ATT_OSTaxBaseAmount sets ATT_LocalTaxBaseAmount, but ATT_LocalTaxBaseAmount still remains 0 as base amount is zero.", 0M, taxRecord.ATT_LocalTaxBaseAmount);

			taxRecord.ATT_LocalTaxBaseAmount = 100M; //setting ATT_LocalTaxBaseAmount independently

			taxRecord.ATT_OSTaxBaseAmount = 40M;
			AssertEquals("Setting ATT_OSTaxBaseAmount sets ATT_LocalTaxBaseAmount propotionally", 200M, taxRecord.ATT_LocalTaxBaseAmount);

			taxRecord.ATT_OSTaxBaseAmount = 0M;
			AssertEquals("Setting ATT_OSTaxBaseAmount sets ATT_LocalTaxBaseAmount propotionally", 0M, taxRecord.ATT_LocalTaxBaseAmount);

			taxRecord.ATT_OSTaxBaseAmount = 15M;
			AssertEquals("Setting ATT_OSTaxBaseAmount sets ATT_LocalTaxAmount propotionally using previous ratio", 75M, taxRecord.ATT_LocalTaxBaseAmount);

			taxRecord.ATT_LocalTaxBaseAmount = 0M; //simulating 0 Local Tax Amount but OS tax amount is non zero.

			taxRecord.ATT_OSTaxBaseAmount = 10M;
			AssertEquals("Setting ATT_OSTaxAmount sets ATT_LocalTaxAmount propotionally using previous ratio", 50M, taxRecord.ATT_LocalTaxBaseAmount);

			AssertEquals("OS tax amount remains the same", 10M, taxRecord.ATT_OSTaxAmount);
		}

		public void TestSettingOSTaxAmount_SetsLocalTaxAmount()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;
			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;

			AssertSecurityRighsAllowOverrideTaxTransaction("Precondition: ATT_OSTaxAmount is not editable", taxRecord.ATT_OSTaxAmountInfo);

			AssertEquals("Precondition: OS tax base amount equal to Local tax base amount", taxRecord.ATT_LocalTaxAmount, taxRecord.ATT_OSTaxAmount);
			AssertEquals("Precondition: OS tax amount", 10M, taxRecord.ATT_OSTaxAmount);

			taxRecord.ATT_LocalTaxAmount = 0M; //setting ATT_LocalTaxAmount independently

			taxRecord.ATT_OSTaxAmount = 20M;
			AssertEquals("Setting ATT_OSTaxAmount sets ATT_LocalTaxAmount, but ATT_LocalTaxAmount still remains 0 as base amount is zero.", 0M, taxRecord.ATT_LocalTaxAmount);

			taxRecord.ATT_LocalTaxAmount = 100M; //setting ATT_LocalTaxAmount independently

			taxRecord.ATT_OSTaxAmount = 40M;
			AssertEquals("Setting ATT_OSTaxAmount sets ATT_LocalTaxAmount propotionally", 200M, taxRecord.ATT_LocalTaxAmount);

			taxRecord.ATT_OSTaxAmount = 0M;
			AssertEquals("Setting ATT_OSTaxAmount sets ATT_LocalTaxAmount propotionally", 0M, taxRecord.ATT_LocalTaxAmount);

			taxRecord.ATT_OSTaxAmount = 15M;
			AssertEquals("Setting ATT_OSTaxAmount sets ATT_LocalTaxAmount propotionally using previous ratio", 75M, taxRecord.ATT_LocalTaxAmount);

			taxRecord.ATT_LocalTaxAmount = 0M; //simulating 0 Local Tax Amount but OS tax amount is non zero.

			taxRecord.ATT_OSTaxAmount = 10M;
			AssertEquals("Setting ATT_OSTaxAmount sets ATT_LocalTaxAmount propotionally using previous ratio", 50M, taxRecord.ATT_LocalTaxAmount);
		}

		public void TestSettingTaxRecordOSTaxAmountsUpdatesTaxAmountOfTaxParent()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AffectsSourceTransactionTotal = true;
			taxRecord.ATT_AH = invoice.PK;
			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxRecord.TaxParent.OSTaxAmount = taxRecord.ATT_OSTaxAmount;
			taxRecord.TaxParent.LocalTaxAmount = taxRecord.ATT_LocalTaxAmount;

			AssertSecurityRighsAllowOverrideTaxTransaction("Precondition: ATT_OSTaxAmount is editable", taxRecord.ATT_OSTaxAmountInfo);

			AssertEquals("Precondition: OS tax amount", 10M, taxRecord.ATT_OSTaxAmount);
			AssertEquals("Precondition: Local tax amount", 10M, taxRecord.ATT_LocalTaxAmount);

			AssertEquals("Precondition: TaxParent OS tax amount", 10M, taxRecord.TaxParent.OSTaxAmount);
			AssertEquals("Precondition: TaxParent Local tax amount", 10M, taxRecord.TaxParent.LocalTaxAmount);

			var newTaxAmount = 12M;
			taxRecord.ATT_OSTaxAmount = newTaxAmount;

			AssertEquals("TaxParent OS tax amount", newTaxAmount, taxRecord.TaxParent.OSTaxAmount);
			AssertEquals("TaxParent Local tax amount", newTaxAmount, taxRecord.TaxParent.LocalTaxAmount);

			taxRecord.ATT_AffectsSourceTransactionTotal = false;
			taxRecord.ATT_OSTaxAmount = 15M;

			AssertEquals("TaxParent OS tax amount still remains as previously set tax amount as ATT_AffectsSourceTransactionTotal is false", newTaxAmount, taxRecord.TaxParent.OSTaxAmount);
			AssertEquals("TaxParent Local tax amount still remains as previously set tax amount as ATT_AffectsSourceTransactionTotal is false", newTaxAmount, taxRecord.TaxParent.LocalTaxAmount);
		}

		void AssertSecurityRighsAllowOverrideTaxTransaction(string message, ZPropertyInfo zPropertyInfo)
		{
			Environment.Env.Security.PayableAllowOverrideTaxTransaction.IsAllowed = false;
			Assert(message + "not editable", zPropertyInfo.ReadOnly);
			Environment.Env.Security.PayableAllowOverrideTaxTransaction.IsAllowed = true;
			Assert(message + "editable", !zPropertyInfo.ReadOnly);
		}

		public void TestTaxParent()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			var taxParentPK = invoice.PK;

			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = taxParentPK;

			var taxParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			var taxRecordParentLoaderMock = new Mock<ITaxFrameworkBOLoader>();
			taxRecordParentLoaderMock.Setup(x => x.LoadTaxRecordParent(It.IsAny<BusinessObjectFactory>(), taxParentPK)).Callback<BusinessObjectFactory, ZGuid>((factory, pk) => { AssertEquals(taxRecord.Factory, factory); }).Returns(taxParent);

			ObjectFactory.Substitute(taxRecordParentLoaderMock.Object);
			AssertEquals(taxParent, taxRecord.TaxParent);
			taxRecordParentLoaderMock.Verify(x => x.LoadTaxRecordParent(It.IsAny<BusinessObjectFactory>(), taxParentPK), Times.Once);

			taxRecordParentLoaderMock.Invocations.Clear();

			AssertEquals(taxParent, taxRecord.TaxParent);
			taxRecordParentLoaderMock.Verify(x => x.LoadTaxRecordParent(It.IsAny<BusinessObjectFactory>(), taxParentPK), Times.Never);

			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			var taxParent1PK = invoice1.PK;

			taxRecord.ATT_AH = taxParent1PK; //Changing ATT_AH to a different tax parent

			var taxParent1 = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice1);
			taxRecordParentLoaderMock.Setup(x => x.LoadTaxRecordParent(It.IsAny<BusinessObjectFactory>(), taxParent1PK)).Callback<BusinessObjectFactory, ZGuid>((factory, pk) => { AssertEquals(taxRecord.Factory, factory); }).Returns(taxParent1);

			AssertEquals(taxParent1, taxRecord.TaxParent);
			taxRecordParentLoaderMock.Verify(x => x.LoadTaxRecordParent(It.IsAny<BusinessObjectFactory>(), taxParent1PK), Times.Once);
		}

		[SuspendCriticalValidation]
		public void TestReadOnly()
		{
			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			AssertReadonlyness("By default all fields are read only", false);

			//AP non persistent
			taxTransaction.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			foreach (var type in new TaxSuperTypeList().GetAllCodes())
			{
				taxTransaction.ATT_TaxSuperType = type;

				if (type == TaxSuperTypeList.StandardPaymentRetention.Code)
				{
					AssertReadonlyness("For AP SPR tax transactions fields are read only", isEditable: false);
				}
				else
				{
					AssertReadonlyness($"For AP {type} tax transactions certain fields are editable", isEditable: true);
				}
			}

			Environment.Env.Security.PayableAllowOverrideTaxTransaction.IsAllowed = false;
			AssertReadonlyness("The Allow Override Tax Transaction security rights are not allowed to edit", false);

			Environment.Env.Security.PayableAllowOverrideTaxTransaction.IsAllowed = true;
			AssertReadonlyness("The Allow Override Tax Transaction security rights are allowed to edit", true);

			//AR non persistent
			taxTransaction.ATT_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			foreach (var type in new TaxSuperTypeList().GetAllCodes())
			{
				taxTransaction.ATT_TaxSuperType = type;

				AssertReadonlyness($"For AR {type} tax transactions fields are read only", isEditable: false);
			}

			Factory.Save();

			//AP persistent
			taxTransaction.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			foreach (var type in new TaxSuperTypeList().GetAllCodes())
			{
				taxTransaction.ATT_TaxSuperType = type;

				Assert("Tax transaction is persistent", taxTransaction.IsInDatabase);
				AssertReadonlyness($"For AP {type} tax transactions fields are read only for persistent records", isEditable: false);
			}

			//AR persistent
			taxTransaction.ATT_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			foreach (var type in new TaxSuperTypeList().GetAllCodes())
			{
				taxTransaction.ATT_TaxSuperType = type;

				Assert("Tax transaction is persistent", taxTransaction.IsInDatabase);
				AssertReadonlyness($"For AR {type} tax transactions fields are read only for persistent records", isEditable: false);
			}

			void AssertReadonlyness(string message, bool isEditable)
			{
				Assert(message, taxTransaction.ATT_AT_TaxIDInfo.ReadOnly);
				Assert(message, taxTransaction.ATT_A9_TaxMessageInfo.ReadOnly);
				Assert(message, taxTransaction.ATT_RX_NKOSTaxCurrencyInfo.ReadOnly);
				Assert(message, taxTransaction.ATT_LocalTaxBaseAmountInfo.ReadOnly);
				Assert(message, taxTransaction.ATT_LocalTaxAmountInfo.ReadOnly);
				Assert(message, taxTransaction.ATT_TaxSystemCodeInfo.ReadOnly);
				Assert(message, taxTransaction.ATT_BasisInfo.ReadOnly);
				Assert(message, taxTransaction.ATT_AffectsSourceTransactionTotalInfo.ReadOnly);
				Assert(message, taxTransaction.ATT_PostDateInfo.ReadOnly);
				Assert(message, taxTransaction.ATT_LedgerInfo.ReadOnly);
				Assert(message, taxTransaction.ATT_TaxSuperTypeInfo.ReadOnly);
				Assert(message, taxTransaction.ATT_RealisationDateInfo.ReadOnly);
				AssertEquals(message, !isEditable, taxTransaction.ATT_OSTaxBaseAmountInfo.ReadOnly);
				AssertEquals(message, !isEditable, taxTransaction.ATT_OSTaxAmountInfo.ReadOnly);
				AssertEquals(message, !isEditable, taxTransaction.ATT_TaxAuthorityServiceCodeInfo.ReadOnly);
				AssertEquals(message, !isEditable, taxTransaction.ATT_TaxDateInfo.ReadOnly);
				AssertEquals(message, !isEditable, taxTransaction.ATT_RateInfo.ReadOnly);
				AssertEquals(message, !isEditable, taxTransaction.ATT_TaxAuthorityServiceCodeDescriptionInfo.ReadOnly);
			}
		}

		public void TestTransactionLinesLinkedToOtherTaxesCollection()
		{
			var accTaxTransaction1 = Factory.NewWithValidTestData<AccTaxTransaction>();
			var accTaxTransaction2 = Factory.NewWithValidTestData<AccTaxTransaction>();

			var pivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, accTaxTransaction1.PK));
			AssertEquals(1, pivots.Length);
			var pivot1 = pivots[0];
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, accTaxTransaction2.PK));
			AssertEquals(1, pivots.Length);
			var pivot3 = pivots[0];

			var creator = new TestObjectCreator(Factory);
			var transaction = creator.CreateInvoiceWithLine(typeof(ARInvoice), "AR101010101", creator.AUD, 1, 100, 0, 100, 0);
			var line1 = transaction.Lines[0];
			transaction = creator.CreateInvoiceWithLine(typeof(APCreditNote), "AP101010101", creator.AUD, 1, 100, 0, 100, 0);
			var line2 = transaction.Lines[0];

			var taxableTransactionMock = new Mock<ITaxableTransactionLine>();
			taxableTransactionMock.Setup(x => x.PK).Returns(line1.PK);
			pivot1.ATP_ATT = accTaxTransaction1.PK;
			pivot1.LinkLine(taxableTransactionMock.Object);
			taxableTransactionMock.Setup(x => x.PK).Returns(line2.PK);
			pivot2.ATP_ATT = accTaxTransaction1.PK;
			pivot2.LinkLine(taxableTransactionMock.Object);
			pivot3.ATP_ATT = accTaxTransaction2.PK;
			pivot3.LinkLine(taxableTransactionMock.Object);

			var collection = accTaxTransaction1.TransactionLinesLinkedToOtherTaxesCollection;
			AssertType<TransactionLineForOtherTaxesDisplayCollection>(collection);
			AssertContainsExactElementsInAnyOrder(new[] { TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(line1), TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(line2) }, collection);

			collection = accTaxTransaction2.TransactionLinesLinkedToOtherTaxesCollection;
			AssertType<TransactionLineForOtherTaxesDisplayCollection>(collection);
			AssertContainsExactElementsInAnyOrder(new[] { TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(line2) }, collection);
		}

		public void TestTaxRate()
		{
			var accTaxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			accTaxTransaction.ATT_RateNumerator = 22;
			accTaxTransaction.ATT_RateDenominator = 0;
			AssertEquals(0m, accTaxTransaction.ATT_Rate);
			accTaxTransaction.ATT_RateDenominator = 7;
			AssertEquals(3.1428571428571428571428571429m, accTaxTransaction.ATT_Rate);
			accTaxTransaction.ATT_RateDenominator = 11;
			AssertEquals(2m, accTaxTransaction.ATT_Rate);
		}

		public void TestRoundingTaxAmountsWithEmptyCurrency()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var accTaxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			accTaxTransaction.ATT_AH = invoice.PK;

			ChangeOSCurrencyButRetainAmounts(accTaxTransaction, ZString.Empty);

			accTaxTransaction.ATT_OSTaxBaseAmount = 100.2589m;
			accTaxTransaction.ATT_LocalTaxBaseAmount = 200.2365m;
			accTaxTransaction.ATT_OSTaxAmount = 21.05m;
			accTaxTransaction.ATT_LocalTaxAmount = 42.05m;

			AssertEquals("OSTaxBaseAmount", 100.26m, accTaxTransaction.ATT_OSTaxBaseAmount);
			AssertEquals("LocalTaxBaseAmount", 200.24m, accTaxTransaction.ATT_LocalTaxBaseAmount);
			AssertEquals("OSTaxAmount", 21.05m, accTaxTransaction.ATT_OSTaxAmount);
			AssertEquals("LocalTaxAmount", 42.05m, accTaxTransaction.ATT_LocalTaxAmount);
		}

		public void TestRoundingTaxAmountsOSCurrency()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var accTaxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			accTaxTransaction.ATT_AH = invoice.PK;

			ChangeOSCurrencyButRetainAmounts(accTaxTransaction, CurrencyCodes.Chile);

			accTaxTransaction.ATT_OSTaxBaseAmount = 100.2589m;
			accTaxTransaction.ATT_LocalTaxBaseAmount = 200.2365m;
			accTaxTransaction.ATT_OSTaxAmount = 21m;
			accTaxTransaction.ATT_LocalTaxAmount = 42.05m;

			AssertEquals("OSTaxBaseAmount", 100m, accTaxTransaction.ATT_OSTaxBaseAmount);
			AssertEquals("LocalTaxBaseAmount", 200.24m, accTaxTransaction.ATT_LocalTaxBaseAmount);
			AssertEquals("OSTaxAmount", 21m, accTaxTransaction.ATT_OSTaxAmount);
			AssertEquals("LocalTaxAmount", 42.05m, accTaxTransaction.ATT_LocalTaxAmount);
		}

		public void TestATT_RX_NKOSTaxCurrency()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var accTaxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			accTaxTransaction.ATT_AH = invoice.PK;

			AssertEquals(CurrencyCodes.Australia, accTaxTransaction.ATT_RX_NKOSTaxCurrency);

			accTaxTransaction.ATT_OSTaxBaseAmount = 100.7;
			accTaxTransaction.ATT_OSTaxAmount = 5.54;

			var expectedMessage = "Cannot modify currency when OS tax base amount and OS tax amount are not 0.";
			Assert("Precondition: ", accTaxTransaction.ATT_OSTaxBaseAmount != 0);
			Assert("Precondition: ", accTaxTransaction.ATT_OSTaxAmount != 0);
			Assert("Precondition: ", !Factory.HasContext(BusinessContext.CopyingPersistentValues));

			AssertExceptionThrown<InvalidOperationException>("when OSTaxBaseAmount and OSTaxAmount are not 0", expectedMessage, 
				() => accTaxTransaction.ATT_RX_NKOSTaxCurrency = CurrencyCodes.India);
			AssertEquals(CurrencyCodes.Australia, accTaxTransaction.ATT_RX_NKOSTaxCurrency);

			AssertATT_RX_NKOSTaxCurrency_NoExceptionThrownInContext(CurrencyCodes.India);

			accTaxTransaction.ATT_OSTaxBaseAmount = 0;
			AssertExceptionThrown<InvalidOperationException>("when OSTaxBaseAmount and OSTaxAmount are not 0", expectedMessage,
				() => accTaxTransaction.ATT_RX_NKOSTaxCurrency = CurrencyCodes.UnitedStates);
			AssertEquals(CurrencyCodes.India, accTaxTransaction.ATT_RX_NKOSTaxCurrency);

			AssertATT_RX_NKOSTaxCurrency_NoExceptionThrownInContext(CurrencyCodes.UnitedStates);

			accTaxTransaction.ATT_OSTaxAmount = 0;
			AssertNoExceptionThrown(() => accTaxTransaction.ATT_RX_NKOSTaxCurrency = CurrencyCodes.Australia);
			AssertEquals(CurrencyCodes.Australia, accTaxTransaction.ATT_RX_NKOSTaxCurrency);

			void AssertATT_RX_NKOSTaxCurrency_NoExceptionThrownInContext(string currencyCode)
			{
				Factory.SetContext(BusinessContext.CopyingPersistentValues);
				AssertNoExceptionThrown(() => accTaxTransaction.ATT_RX_NKOSTaxCurrency = currencyCode);
				Factory.RemoveContext(BusinessContext.CopyingPersistentValues);

				AssertEquals(currencyCode, accTaxTransaction.ATT_RX_NKOSTaxCurrency);
			}
		}

		public void TestATT_ETC()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);

			var expectedMessage = "Cannot modify ATT_ETC when Local and OS tax amounts are not 0.";
			Assert("Precondition: ", taxRecord.ATT_OSTaxAmount != 0);
			Assert("Precondition: ", taxRecord.ATT_LocalTaxAmount != 0);
			Assert("Precondition: ", !Factory.HasContext(BusinessContext.CopyingPersistentValues));

			AssertExceptionThrown<InvalidOperationException>("when Local and OS tax amounts are not 0", expectedMessage, () => taxRecord.ATT_ETC = taxConfig.PK);

			AssertATT_ETC_NoExceptionThrownInContext(taxConfig.PK);

			taxRecord.ATT_LocalTaxAmount = 0;
			AssertExceptionThrown<InvalidOperationException>("when Local and OS tax amounts are not 0", expectedMessage, () => taxRecord.ATT_ETC = taxConfig.PK);

			AssertATT_ETC_NoExceptionThrownInContext(taxConfig.PK);

			taxRecord.ATT_OSTaxAmount = 0;
			AssertNoExceptionThrown(() => taxRecord.ATT_ETC = taxConfig.PK);
			AssertEquals(taxConfig.PK, taxRecord.ATT_ETC);

			void AssertATT_ETC_NoExceptionThrownInContext(ZGuid taxConfigurationPK)
			{
				Factory.SetContext(BusinessContext.CopyingPersistentValues);
				AssertNoExceptionThrown(() => taxRecord.ATT_ETC = taxConfigurationPK);
				Factory.RemoveContext(BusinessContext.CopyingPersistentValues);

				AssertEquals(taxConfigurationPK, taxRecord.ATT_ETC);
			}
		}

		public void TestTaxTransactionCancelledWhenAllAmountsZero_DifferentBasis()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord1 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord1.ATT_AH = invoice.PK;

			var taxRecord2 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord2.ATT_AH = invoice.PK;

			var taxRecord3 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord3.ATT_AH = invoice.PK;

			taxRecord1.ATT_Basis = TaxBasisList.Posting.Code;
			taxRecord2.ATT_Basis = TaxBasisList.Matching.Code;
			taxRecord3.ATT_Basis = TaxBasisList.PostingOnMatching.Code;

			taxRecord1.ATT_PostDate = ZDate.Today;
			taxRecord1.ATT_RealisationDate = taxRecord1.ATT_PostDate;
			taxRecord2.ATT_PostDate = ZDate.Today;
			taxRecord3.ATT_PostDate = ZDate.Today;

			taxRecord1.ATT_OSTaxBaseAmount = taxRecord1.ATT_LocalTaxBaseAmount = taxRecord1.ATT_OSTaxAmount = taxRecord1.ATT_LocalTaxAmount = 0m;
			taxRecord2.ATT_OSTaxBaseAmount = taxRecord2.ATT_LocalTaxBaseAmount = taxRecord2.ATT_OSTaxAmount = taxRecord2.ATT_LocalTaxAmount = 0m;
			taxRecord3.ATT_OSTaxBaseAmount = taxRecord3.ATT_LocalTaxBaseAmount = taxRecord3.ATT_OSTaxAmount = taxRecord3.ATT_LocalTaxAmount = 0m;

			Assert("Precondition:", !taxRecord1.IsCancelled);
			Assert("Precondition:", !taxRecord2.IsCancelled);
			Assert("Precondition:", !taxRecord3.IsCancelled);
			AssertEquals("Precondition:", taxRecord1.ATT_PostDate, taxRecord1.ATT_RealisationDate);
			AssertEquals("Precondition:", ZDate.Empty, taxRecord2.ATT_RealisationDate);
			AssertEquals("Precondition:", ZDate.Empty, taxRecord3.ATT_RealisationDate);

			Factory.Save();

			AssertTaxTransactionCancelledWhenAllAmountsZero(taxRecord1, true, taxRecord1.ATT_PostDate);
			AssertTaxTransactionCancelledWhenAllAmountsZero(taxRecord2, true, taxRecord2.ATT_PostDate);
			AssertTaxTransactionCancelledWhenAllAmountsZero(taxRecord3, true, ZDate.Empty);
		}

		public void TestTaxTransactionCancelledWhenAllAmountsZero_IsInDatabase()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;
			taxRecord.ATT_OSTaxBaseAmount = taxRecord.ATT_LocalTaxBaseAmount = taxRecord.ATT_OSTaxAmount = taxRecord.ATT_LocalTaxAmount = 0m;

			Assert("Precondition:", !taxRecord.IsInDatabase);

			Factory.Save();

			AssertTaxTransactionCancelledWhenAllAmountsZero(taxRecord, true, ZDate.Empty);

			taxRecord.IsCancelled = false;

			Assert("Precondition:", taxRecord.IsInDatabase);
			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				Factory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}
			AssertTaxTransactionCancelledWhenAllAmountsZero(taxRecord, false, ZDate.Empty);
		}

		public void TestTaxTransactionNotCancelledWhenAnyAmountIsNonZero()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord1 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord1.ATT_AH = invoice.PK;
			var taxRecord2 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord2.ATT_AH = invoice.PK;
			var taxRecord3 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord3.ATT_AH = invoice.PK;
			var taxRecord4 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord4.ATT_AH = invoice.PK;

			taxRecord1.ATT_OSTaxBaseAmount = taxRecord1.ATT_LocalTaxBaseAmount = taxRecord1.ATT_OSTaxAmount = taxRecord1.ATT_LocalTaxAmount = 0m;
			taxRecord2.ATT_OSTaxBaseAmount = taxRecord2.ATT_LocalTaxBaseAmount = taxRecord2.ATT_OSTaxAmount = taxRecord2.ATT_LocalTaxAmount = 0m;
			taxRecord3.ATT_OSTaxBaseAmount = taxRecord3.ATT_LocalTaxBaseAmount = taxRecord3.ATT_OSTaxAmount = taxRecord3.ATT_LocalTaxAmount = 0m;
			taxRecord4.ATT_OSTaxBaseAmount = taxRecord4.ATT_LocalTaxBaseAmount = taxRecord4.ATT_OSTaxAmount = taxRecord4.ATT_LocalTaxAmount = 0m;

			taxRecord1.ATT_OSTaxBaseAmount = 10m;
			taxRecord2.ATT_LocalTaxBaseAmount = 10m;
			taxRecord3.ATT_OSTaxAmount = 10m;
			taxRecord4.ATT_LocalTaxAmount = 10m;

			Factory.Save();

			AssertTaxTransactionCancelledWhenAllAmountsZero(taxRecord1, false, ZDate.Empty);
			AssertTaxTransactionCancelledWhenAllAmountsZero(taxRecord2, false, ZDate.Empty);
			AssertTaxTransactionCancelledWhenAllAmountsZero(taxRecord3, false, ZDate.Empty);
			AssertTaxTransactionCancelledWhenAllAmountsZero(taxRecord4, false, ZDate.Empty);
		}

		void AssertTaxTransactionCancelledWhenAllAmountsZero(AccTaxTransaction taxRecord, bool expectedIsCancelled, ZDate expectedRealisationDate)
		{
			AssertEquals(expectedIsCancelled, taxRecord.IsCancelled);
			AssertEquals(expectedRealisationDate, taxRecord.ATT_RealisationDate);
		}

		[TestDate(2020, 01, 15)]
		public void TestATT_RealisationDate_CannotBeSetSecondTime()
		{
			var taxRecord = Factory.New<AccTaxTransaction>();
			AssertEquals("Precondition: ATT_RealisationDate is empty", ZDate.Empty, taxRecord.ATT_RealisationDate);

			var realisationDate = ZDate.Today;
			taxRecord.ATT_RealisationDate = realisationDate;
			AssertEquals("ATT_RealisationDate set successfully", realisationDate, taxRecord.ATT_RealisationDate);

			var expectedErrorMessage = "Attempted to set Realisation Date on an AccTaxTransaction record which already have been realised.";
			taxRecord.ATT_RealisationDate = ZDate.Empty;
			AssertContains("LastExceptionReported.Message", expectedErrorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertEquals("ATT_RealisationDate is unchanged", realisationDate, taxRecord.ATT_RealisationDate);

			taxRecord.ATT_RealisationDate = ZDate.Today.AddDays(10);
			AssertContains("LastExceptionReported.Message", expectedErrorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertEquals("ATT_RealisationDate is unchanged", realisationDate, taxRecord.ATT_RealisationDate);

			var newRealisationDate = ZDate.Today.AddDays(5);
			taxRecord.ATT_IsCancelled = true;
			taxRecord.ATT_RealisationDate = newRealisationDate;
			AssertEquals("ATT_RealisationDate set successfully", newRealisationDate, taxRecord.ATT_RealisationDate);
		}

		public void TestAccTaxTransactionEmptyValidationIsInvoked_WhenHasContextSavingIncompleteTransaction()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var taxTransaction = invoice.TaxTransactionCollection.AddNew();
			Assert("Precondition: tax transaction cancellation status", !taxTransaction.ATT_IsCancelled);

			Factory.SetContext(Enterprise.Integration.Accounting.BusinessContext.SavingIncompleteTransaction);
			AssertEquals(typeof(AccTaxTransactionEmptyValidation), taxTransaction.Validation.GetType());

			Factory.RemoveContext(Enterprise.Integration.Accounting.BusinessContext.SavingIncompleteTransaction);
			AssertNotEquals(typeof(AccTaxTransactionEmptyValidation), taxTransaction.Validation.GetType());
		}

		void ChangeOSCurrencyButRetainAmounts(AccTaxTransaction taxRecord, ZString newCurrency)
		{
			var osTaxBasAmount = taxRecord.ATT_OSTaxBaseAmount;
			var osTaxAmount = taxRecord.ATT_OSTaxAmount;
			taxRecord.ATT_OSTaxBaseAmount = 0;
			taxRecord.ATT_OSTaxAmount = 0;

			AssertNoExceptionThrown(() => taxRecord.ATT_RX_NKOSTaxCurrency = newCurrency);

			taxRecord.ATT_OSTaxBaseAmount = osTaxBasAmount;
			taxRecord.ATT_OSTaxAmount = osTaxAmount;
		}

		void SetupRoundingMethodApplierMock()
		{
			taxFrameworkDependencyFactory = new Mock<ITaxFrameworkDependencyFactory>();
			ObjectFactory.Substitute(taxFrameworkDependencyFactory.Object);

			roundingMethodApplier = new Mock<IRoundingMethodApplier>();
			taxFrameworkDependencyFactory.Setup(x => x.GetRoundingMethodApplier()).Returns(roundingMethodApplier.Object);
		}

		Mock<ITaxFrameworkDependencyFactory> taxFrameworkDependencyFactory;
		Mock<IRoundingMethodApplier> roundingMethodApplier;

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		protected TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator
		{
			get { return taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory)); }
		}
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
	}
}
