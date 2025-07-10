using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework.TestHelper;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public abstract class DependentTransactionLineValidationTest : TransactionLineValidation_InnerTest
	{
		#region TaxMessageIsEmptyTest

		public void TestTaxMessageWarningAndError()
		{
			var traHeader = (TransactionHeaderWithLines)Factory.NewWithValidTestData(GetExpectedParentBusinessObjectType());
			var dependentTransactionLine = traHeader.Lines.AddNew();
			(var isNeedTestNoTaxMessage, var isUseAPRegistry) = GetIsNeedTestNoTaxMessageAndIsAP(traHeader);

			if (isNeedTestNoTaxMessage)
			{
				RunTestTaxMessageWarningAndError(isUseAPRegistry, dependentTransactionLine);
			}
			else
			{
				Assert("Does not apply", true);
			}
		}

		protected virtual (bool isNeedTestNoTaxMessage, bool isUseAPRegistry) GetIsNeedTestNoTaxMessageAndIsAP(TransactionHeaderWithLines header) => (false, false);

		void RunTestTaxMessageWarningAndError(bool isAP, DependentTransactionLine dependentLine)
		{
			var nil = ZGuid.Empty;
			var msg = TestObjectCreator.CreateTaxMsg("MSG1", "MSG1", "english msg1", "local msg1").PK;

			CombineAssertions("", () =>
			{
				//change country hits db so better do it in bulk
				foreach (var country in new[] { "PT", "AU", "US", "JP" })
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
					{
						//when message is present, no notification
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "NOT", "NOT", 0m, msg, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "NOT", "RTZ", 0m, msg, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "NOT", "REQ", 0m, msg, false, Expect.Nothing);

						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RTZ", "NOT", 0m, msg, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RTZ", "RTZ", 0m, msg, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RTZ", "REQ", 0m, msg, false, Expect.Nothing);

						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REQ", "NOT", 0m, msg, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REQ", "RTZ", 0m, msg, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REQ", "REQ", 0m, msg, false, Expect.Nothing);

						RunTaxMessageIsEmptyTest(isAP, dependentLine, "NOT", "NOT", 70m, msg, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "NOT", "RTZ", 70m, msg, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "NOT", "REQ", 70m, msg, true, Expect.Nothing);

						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RTZ", "NOT", 70m, msg, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RTZ", "RTZ", 70m, msg, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RTZ", "REQ", 70m, msg, true, Expect.Nothing);

						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REQ", "NOT", 70m, msg, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REQ", "RTZ", 70m, msg, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REQ", "REQ", 70m, msg, true, Expect.Nothing);

						//when taxID is blank, no notification
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "NOT", "NOT", 70m, nil, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RTZ", "NOT", 70m, nil, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REQ", "NOT", 70m, nil, true, Expect.Nothing);

						RunTaxMessageIsEmptyTest(isAP, dependentLine, "NOT", "RTZ", 70m, nil, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RTZ", "RTZ", 70m, nil, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REQ", "RTZ", 70m, nil, true, Expect.Nothing);

						RunTaxMessageIsEmptyTest(isAP, dependentLine, "NOT", "REQ", 70m, nil, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RTZ", "REQ", 70m, nil, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REQ", "REQ", 70m, nil, true, Expect.Nothing);

						//Nothing when NOT
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "NOT", "NOT", 0m, nil, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RTZ", "NOT", 0m, nil, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REQ", "NOT", 0m, nil, false, Expect.Nothing);

						//Nothing when NOT
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "NOT", "NOT", 70m, nil, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RTZ", "NOT", 70m, nil, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REQ", "NOT", 70m, nil, false, Expect.Nothing);

						//Nothing when tax is entered and RTZ
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "NOT", "RTZ", 70m, nil, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RTZ", "RTZ", 70m, nil, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REQ", "RTZ", 70m, nil, false, Expect.Nothing);

						//Errors;
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "NOT", "RTZ", 70m, nil, false, Expect.Error, true);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RTZ", "RTZ", 70m, nil, false, Expect.Error, true);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REQ", "RTZ", 70m, nil, false, Expect.Error, true);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "NOT", "REQ", 0m, nil, false, Expect.Error);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RTZ", "REQ", 0m, nil, false, Expect.Error);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REQ", "REQ", 0m, nil, false, Expect.Error);

						//Nothing when taxID is blank and RET
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RET", "RET", 70m, msg, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RET", "RET", 0m, msg, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RET", "RET", 70m, nil, true, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RET", "RET", 0m, nil, true, Expect.Nothing);

						//Nothing when taxID is entered and RET
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RET", "RET", 70m, msg, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RET", "RET", 0m, msg, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RET", "RET", 70m, nil, false, Expect.Error);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "RET", "RET", 0m, nil, false, Expect.Nothing);

						//Nothing when taxID is entered and tax rate is 0 and REZ (FREEVAT, Exempt)
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REZ", "REZ", 70m, nil, false, Expect.Error);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REZ", "REZ", 70m, msg, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REZ", "REZ", 0m, nil, false, Expect.Nothing);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REZ", "REZ", 0m, msg, false, Expect.Nothing);

						//Nothing when taxID is extra tax rated and REZ
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REZ", "REZ", 70m, nil, false, Expect.Error, true);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REZ", "REZ", 70m, msg, false, Expect.Nothing, true);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REZ", "REZ", 0m, nil, false, Expect.Nothing, true);
						RunTaxMessageIsEmptyTest(isAP, dependentLine, "REZ", "REZ", 0m, msg, false, Expect.Nothing, true);
					}
				}
			});
		}

		enum Expect
		{
			Error, Warning, Nothing
		}

		void RunTaxMessageIsEmptyTest(bool isAP, DependentTransactionLine line, string registry1, string registry2, ZDecimal osAmount, ZGuid messagePK, bool taxIDEmpty, Expect notification, bool isTaxRateZero = false)
		{
			var notificationMessage = registry2 == Constants.TaxMessageMandatoryOptionConstants.RequiredAlways
				? "Tax Message is Required."
				: registry2 == Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxIsZero
				? "Tax Amount is zero, please enter a Tax Message."
				: "Please enter a valid Tax Message when using this Tax ID.";

			var taxID = taxIDEmpty ? ZGuid.Empty : isTaxRateZero ? TestObjectCreator.GSTFREE1.PK : TestObjectCreator.GST1.PK;
			if ((isAP && registry2 == Constants.TaxMessageMandatoryOptionConstants.RequiredWhenExtraTaxIsNotZero) || (!isAP && registry1 == Constants.TaxMessageMandatoryOptionConstants.RequiredWhenExtraTaxIsNotZero))
			{
				taxID = taxIDEmpty ? ZGuid.Empty : TestObjectCreator.KDV18W5.PK;
			}

			if ((isAP && registry2 == Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxHasExtraElementOrZeroTax) || (!isAP && registry1 == Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxHasExtraElementOrZeroTax))
			{
				if (!isTaxRateZero)
				{
					taxID = taxIDEmpty ? ZGuid.Empty : TestObjectCreator.KDV18W5.PK;
				}
				else
				{
					taxID = taxIDEmpty ? ZGuid.Empty : TestObjectCreator.GSTFREE1.PK;
				}
			}

			line.AL_AT = taxID;
			line.AL_OSExTaxAmount = osAmount;
			line.AL_OSAmount = osAmount;
			line.AL_A9_VATClass = messagePK;

			var registryAP = isAP ? registry2 : registry1;
			var registryAR = isAP ? registry1 : registry2;

			AccountingConfigurationRegistry.Instance.TaxMessageIsMandatoryPayables.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryAP);
			AccountingConfigurationRegistry.Instance.TaxMessageIsMandatoryReceivables.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryAR);

			line.Validation.ValidateAL_A9_VATClass();

			var failMessage = Invariant($"Type: {line.GetType().Name}, Country: {GlbCompany.CurrentCompany.Country.Code}, AP Registry Setting: {registryAP}, AR Registry Setting: {registryAR}, OSAmount: {osAmount}, Tax Message: {messagePK.IsEmpty}, Is Tax Id Empty: {taxIDEmpty}, Expect Option: {notification.ToString()}");

			switch (notification)
			{
				case Expect.Error:
					AssertHasError(failMessage, line.AL_A9_VATClassInfo, notificationMessage);
					AssertNoWarnings(failMessage, line.AL_A9_VATClassInfo);
					break;
				case Expect.Warning:
					AssertHasWarning(failMessage, line.AL_A9_VATClassInfo, notificationMessage);
					AssertNoErrors(failMessage, line.AL_A9_VATClassInfo);
					break;
				case Expect.Nothing:
					AssertNoWarnings(failMessage, line.AL_A9_VATClassInfo);
					AssertNoErrors(failMessage, line.AL_A9_VATClassInfo);
					break;
			}
		}

		#endregion

		public void TestAL_OSGSTAmount()
		{
			var line = TestBizO;

			line.AL_OSExTaxAmount = -100m;
			line.AL_OSGSTAmount = 10m;
			AssertHasError(line.AL_OSGSTAmountInfo, DependentTransactionLineValidation.AmountAndGSTAmountMustHaveSameSign_ForTestOnly);

			line.AL_OSExTaxAmount = 100m;
			line.AL_OSGSTAmount = 10m;
			AssertEquals(false, line.AL_OSGSTAmountInfo.HasErrors());

			using (line.GetValidationSuspender())
			{
				line.AL_OSExTaxAmount = 100m;
				line.AL_OSGSTAmount = -10m;
			}
			line.RunPreSaveValidation();
			AssertHasError(line.AL_OSGSTAmountInfo, DependentTransactionLineValidation.AmountAndGSTAmountMustHaveSameSign_ForTestOnly);
		}

		public virtual void TestCheckAL_LocalTaxAmount()
		{
			if (typeof(GLJournalLine).IsAssignableFrom(GetExpectedBusinessObjectType()) ||
				typeof(JobRevenueJournalLine).IsAssignableFrom(GetExpectedBusinessObjectType()))
			{
				Assert("Journal Lines don't have tax", true);
			}
			else
			{
				var companyWthoutDecimalPoints = Factory.NewWithValidTestData<GlbCompany>();
				companyWthoutDecimalPoints.GC_RN_NKCountryCode = Core.Constants.CountryCodes.VietNam;
				companyWthoutDecimalPoints.GC_Name = "Vietnam Company";
				companyWthoutDecimalPoints.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.VietNam;
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = companyWthoutDecimalPoints.PK;
				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var line = TestBizO;
					line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.VietNam;

					line.AL_ExchangeRate = 2m;
					line.AL_OSTaxAmount = 200m;
					line.AL_LocalTaxAmount = 100m;
					using (line.GetLocalAmountCalculationSuspender())
					{
						line.AL_ExchangeRate = 1m;
					}
					AssertEquals(200m, line.AL_OSTaxAmount);
					AssertEquals(100m, line.AL_LocalTaxAmount);
					AssertEquals(1m, line.AL_ExchangeRate);

					Assert("Local Tax Amount and OS Tax Amount is different", line.AL_OSTaxAmount != line.AL_LocalTaxAmount);
					((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalTaxAmount();
					AssertHasError(line.AL_LocalTaxAmountInfo, "The Local Tax Amount should be equal to OS Tax Amount when Local Currency is used");

					line.AL_ExchangeRate = 1m;
					line.AL_OSTaxAmount = 123m;
					AssertEquals(123m, line.AL_LocalTaxAmount);
					Assert("Exchange Rate is 1", line.AL_ExchangeRate == 1);
					Assert("Local Tax Amount and OS Tax Amount is equal", line.AL_OSTaxAmount == line.AL_LocalTaxAmount);
					((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalTaxAmount();
					AssertNoError(line.AL_LocalTaxAmountInfo, "The Local Tax Amount should be equal to OS Tax Amount when Local Currency is used");

					line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;

					line.AL_ExchangeRate = 1m;
					line.AL_OSTaxAmount = 123.4m;
					AssertEquals(123m, line.AL_LocalTaxAmount);
					Assert("Exchange Rate is 1", line.AL_ExchangeRate == 1);
					Assert(line.AL_OSTaxAmount != line.AL_LocalTaxAmount);
					((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalTaxAmount();
					AssertNoError(line.AL_LocalTaxAmountInfo, "The Local Tax Amount should be equal to OS Tax Amount when Local Currency is used");

					line.AL_ExchangeRate = 2m;
					line.AL_OSTaxAmount = 123.56m;
					AssertEquals(62m, line.AL_LocalTaxAmount);
					Assert("Exchange Rate is not 1", line.AL_ExchangeRate != 1);
					Assert("Local Tax Amount and OS Tax Amount is different", line.AL_OSTaxAmount != line.AL_LocalTaxAmount);
					((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalTaxAmount();
					AssertNoError(line.AL_LocalTaxAmountInfo, "The Local Tax Amount should be equal to OS Tax Amount when Local Currency is used");
				}
			}
		}

		public virtual void TestCheckAL_LocalExTaxAmount_WhenExRateIsOneAndLocalAndForeignAmountDoNotMatch()
		{
			if (typeof(GLJournalLine).IsAssignableFrom(GetExpectedBusinessObjectType()) ||
				typeof(JobRevenueJournalLine).IsAssignableFrom(GetExpectedBusinessObjectType()))
			{
				Assert("Journal Lines don't use AL_LocalExTaxAmount in GUI Form, they use UnsignedLocalLineAmount", true);
			}
			else
			{
				var companyWthoutDecimalPoints = Factory.NewWithValidTestData<GlbCompany>();
				companyWthoutDecimalPoints.GC_RN_NKCountryCode = Core.Constants.CountryCodes.VietNam;
				companyWthoutDecimalPoints.GC_Name = "Vietnam Company";
				companyWthoutDecimalPoints.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.VietNam;
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = companyWthoutDecimalPoints.PK;
				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var line = TestBizO;

					line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.VietNam;

					line.AL_ExchangeRate = 2m;
					line.AL_OSExTaxAmount = 200m;
					line.AL_LocalExTaxAmount = 100m;
					using (line.GetLocalAmountCalculationSuspender())
					{
						line.AL_ExchangeRate = 1m;
					}
					AssertEquals(200m, line.AL_OSExTaxAmount);
					AssertEquals(100m, line.AL_LocalExTaxAmount);
					AssertEquals(1m, line.AL_ExchangeRate);

					Assert("Local Amount and OS Amount is different", line.AL_OSExTaxAmount != line.AL_LocalExTaxAmount);
					((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalExTaxAmount();
					AssertHasError(line.AL_LocalExTaxAmountInfo, "The Local Amount should be equal to OS Amount when Local Currency is used");

					line.AL_ExchangeRate = 1m;
					line.AL_OSExTaxAmount = 123m;
					AssertEquals(123m, line.AL_LocalExTaxAmount);
					Assert("Exchange Rate is 1", line.AL_ExchangeRate == 1);
					Assert("Local Amount and OS Amount is equal", line.AL_OSExTaxAmount == line.AL_LocalExTaxAmount);
					((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalExTaxAmount();
					AssertNoError(line.AL_LocalExTaxAmountInfo, "The Local Amount should be equal to OS Amount when Local Currency is used");

					line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;

					line.AL_ExchangeRate = 1m;
					line.AL_OSExTaxAmount = 123.4m;
					AssertEquals(123m, line.AL_LocalExTaxAmount);
					Assert("Exchange Rate is 1", line.AL_ExchangeRate == 1);
					Assert(line.AL_OSExTaxAmount != line.AL_LocalExTaxAmount);
					((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalExTaxAmount();
					AssertNoError(line.AL_LocalExTaxAmountInfo, "The Local Amount should be equal to OS Amount when Local Currency is used");

					line.AL_ExchangeRate = 2m;
					line.AL_OSExTaxAmount = 123.56m;
					AssertEquals(62m, line.AL_LocalExTaxAmount);
					Assert("Exchange Rate is not 1", line.AL_ExchangeRate != 1);
					Assert("Local Amount and OS Amount is different", line.AL_OSExTaxAmount != line.AL_LocalExTaxAmount);
					((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalExTaxAmount();
					AssertNoError(line.AL_LocalExTaxAmountInfo, "The Local Amount should be equal to OS Amount when Local Currency is used");
				}
			}
		}

		public void TestCheckAL_LocalExTaxAmount_LocalAmountIsZeroWhenForeignAmountIsValid()
		{
			if (typeof(GLJournalLine).IsAssignableFrom(GetExpectedBusinessObjectType()) ||
				typeof(JobRevenueJournalLine).IsAssignableFrom(GetExpectedBusinessObjectType()))
			{
				Assert("Journal Lines don't use AL_LocalExTaxAmount in GUI Form, they use UnsignedLocalLineAmount", true);
			}
			else
			{
				var line = TestBizO;

				line.AL_OSExTaxAmount = 0m;
				line.AL_LineAmount = 0m;
				((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalExTaxAmount();
				//Should not have validation error, because OSExTaxAmount is zero
				AssertNoRowError(line, "Local Amount has been set to zero when foreign amount is valid.");

				line.AL_OSExTaxAmount = 20m;
				line.AL_LineAmount = 0m;
				((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalExTaxAmount();
				AssertHasRowError("Should have validation error, because LocalExTaxAmount is zero", line, "Local Amount has been set to zero when foreign amount is valid.");

				line.AL_OSExTaxAmount = 20m;
				line.AL_LineAmount = 0m;
				line.AL_ExchangeRate = 0m;
				((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalExTaxAmount();
				//Should not have validation error, because AL_ExchangeRate is zero
				AssertNoRowError(line, "Local Amount has been set to zero when foreign amount is valid.");

				line.AL_OSExTaxAmount = 20m;
				line.AL_LineAmount = 0m;
				line.AL_ExchangeRate = 1m;
				line.AL_RX_NKTransactionCurrency = ZString.Empty;
				((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalExTaxAmount();
				//Should not have validation error, because line transaction currency is empty
				AssertNoRowError(line, "Local Amount has been set to zero when foreign amount is valid.");

				line.AL_OSExTaxAmount = 20m;
				line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
				line.AL_LineAmount = 0m;
				((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalExTaxAmount();
				AssertHasRowError("Should have validation error, because LocalExTaxAmount is zero", line, "Local Amount has been set to zero when foreign amount is valid.");

				line.AL_ExchangeRate = 8000m;
				line.AL_OSExTaxAmount = 0.01m;
				((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalExTaxAmount();
				AssertEquals(ZDecimal.Zero, line.AL_LocalExTaxAmount);
				//Should not have validation error, because ForeignToLocal conversion is zero
				AssertNoRowError(line, "Local Amount has been set to zero when foreign amount is valid.");
			}
		}

		public void TestAL_LocalTaxAmountValidationDoesNotThrowExceptionWhenTransactionCurrencyIsEmpty()
		{
			if (typeof(GLJournalLine).IsAssignableFrom(GetExpectedBusinessObjectType()) ||
				typeof(JobRevenueJournalLine).IsAssignableFrom(GetExpectedBusinessObjectType()))
			{
				Assert("Journal Lines don't have tax", true);
			}
			else
			{
				var line = TestBizO;
				line.AL_RX_NKTransactionCurrency = ZString.Empty;
				line.AL_OSTaxAmount = 100m;
				line.AL_LocalTaxAmount = 200m;
				using (line.GetLocalAmountCalculationSuspender())
				{
					line.AL_ExchangeRate = 1m;
				}

				Assert("Transaction Currency is empty", line.AL_RX_NKTransactionCurrency.IsEmpty);
				AssertEquals("Exchange rate is 1", 1m, line.AL_ExchangeRate);
				AssertNotEquals("Local Tax Amount and OS Tax Amount is different", line.AL_OSTaxAmount, line.AL_LocalTaxAmount);
				AssertNoExceptionThrown("Null Reference Exception must no be thrown", () => ((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalTaxAmount());
			}
		}

		public void TestAL_LocalExTaxAmountValidationDoesNotThrowExceptionWhenTransactionCurrencyIsEmpty()
		{
			if (typeof(GLJournalLine).IsAssignableFrom(GetExpectedBusinessObjectType()) ||
				typeof(JobRevenueJournalLine).IsAssignableFrom(GetExpectedBusinessObjectType()))
			{
				Assert("Journal Lines don't use AL_LocalExTaxAmount in GUI Form, they use UnsignedLocalLineAmount", true);
			}
			else
			{
				var line = TestBizO;
				line.AL_RX_NKTransactionCurrency = ZString.Empty;
				line.AL_OSExTaxAmount = 100m;
				line.AL_LocalExTaxAmount = 200m;
				using (line.GetLocalAmountCalculationSuspender())
				{
					line.AL_ExchangeRate = 1m;
				}

				Assert("Transaction Currency is empty", line.AL_RX_NKTransactionCurrency.IsEmpty);
				AssertEquals("Exchange rate is 1", 1m, line.AL_ExchangeRate);
				AssertNotEquals("Local Amount and OS Amount is different", line.AL_OSExTaxAmount, line.AL_LocalExTaxAmount);
				AssertNoExceptionThrown("Null Reference Exception must no be thrown", () => ((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalExTaxAmount());
			}
		}

		public void TestValidateAL_LocalExtraTaxAmountForIndia_STA_TaxID()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var stateGST = testObjectCreator.STAGST;

				var expectedRowError = string.Format("CGST and SGST can not differ by more than rounding error. Please check tax setup for '{0}'. If tax setup is correct then please contact CargoWise Support.", stateGST.AT_Code);

				var line = TestBizO;
				line.AL_AC = testObjectCreator.CC3.PK;
				line.AL_OSExTaxAmount = 100m;
				line.AL_Sequence = 1;
				line.AL_AT = stateGST.PK;

				AssertEquals(18M, line.AL_LocalTaxAmount);
				AssertEquals(9M, line.AL_LocalGSTAmount);
				AssertEquals("SGST and CGST is same", line.AL_LocalExtraTaxAmount, line.AL_LocalGSTAmount);
				AssertNoRowError(line, expectedRowError);

				line.AL_LocalExtraTaxAmount = 9.01M;
				AssertNotEquals("SGST and CGST is not same", line.AL_LocalExtraTaxAmount, line.AL_LocalGSTAmount);
				Assert("Difference between SGST and CGST is within 1M", Math.Abs(line.AL_LocalExtraTaxAmount - line.AL_LocalGSTAmount) < 1);
				AssertNoRowError(line, expectedRowError);

				line.AL_LocalExtraTaxAmount = 10.01M;
				AssertNotEquals("SGST and CGST is not same", line.AL_LocalExtraTaxAmount, line.AL_LocalGSTAmount);
				Assert("Difference between SGST and CGST is more than 1M", Math.Abs(line.AL_LocalExtraTaxAmount - line.AL_LocalGSTAmount) > 1);
				AssertHasRowError(line, expectedRowError);

				line.AL_LocalExtraTaxAmount = 9M;
				AssertEquals("SGST and CGST is same", line.AL_LocalExtraTaxAmount, line.AL_LocalGSTAmount);
				AssertNoRowError(line, expectedRowError);

				line.AL_LocalGSTAmount = 10M;
				((DependentTransactionLineValidation)line.Validation).ValidateAL_LocalExtraTaxAmount();
				AssertNotEquals("SGST and CGST is not same", line.AL_LocalExtraTaxAmount, line.AL_LocalGSTAmount);
				Assert("Difference between SGST and CGST 1M", Math.Abs(line.AL_LocalExtraTaxAmount - line.AL_LocalGSTAmount) == 1);
				AssertHasRowError(line, expectedRowError);
			}
		}

		public void TestDisallowMixtureOfBranches_WhenBranchLevelPostingIsEnabled()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var branch1 = testObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = testObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);

			var headerWithLines = (TransactionHeaderWithLines)Factory.New(GetExpectedParentBusinessObjectType());

			if (headerWithLines.EnforceBranchLevelPostingRegistryItem != null)
			{
				var expectedError = @"Please review the charge lines entered and ensure all charges have been entered belong to the same Posting Group. All charges posted in the one transaction must be in the same Posting Group.
Posting is prevented because charges have been entered using a mix of Posting Groups.";

				foreach (var registryValue in new bool[] { true, false })
				{
					if (headerWithLines.GetType() == typeof(CashBook.Transfer.BankTransferCharge))
					{
						var defaultLine = headerWithLines.Lines[0];
						defaultLine.AL_GB = branch1.PK;
					}

					var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = registryValue };
					headerWithLines.EnforceBranchLevelPostingRegistryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

					var line1 = headerWithLines.Lines.AddNew();
					var line2 = headerWithLines.Lines.AddNew();

					//lines have different branches
					line1.AL_GB = branch1.PK;
					line2.AL_GB = branch2.PK;

					if (registryValue)
					{
						AssertHasError("Line 2 branch should show error as Branch level posting is enabled but two lines have got different branches", line2.AL_GBInfo, expectedError);
					}
					else
					{
						AssertNoError("Line 2 branch should not show any error as Branch level posting is not enabled", line2.AL_GBInfo, expectedError);
					}

					//line 2 branch is changed so now both lines have the same branch
					line2.AL_GB = branch1.PK;

					if (registryValue)
					{
						AssertNoError("Line 2 branch should no longer show error as it has the same branch as Line 1 now", line2.AL_GBInfo, expectedError);
					}
					else
					{
						AssertNoError("Line 2 branch should not show any error as Branch level posting is not enabled", line2.AL_GBInfo, expectedError);
					}

					line2.AL_GB = branch2.PK;

					if (registryValue)
					{
						AssertHasError("Line 2 branch should show error as Branch level posting is enabled but two lines have got different branches", line2.AL_GBInfo, expectedError);
					}
					else
					{
						AssertNoError("Line 2 branch should not show any error as Branch level posting is not enabled", line2.AL_GBInfo, expectedError);
					}

					branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
					var settings1 = new BranchGroupSettings();
					settings1.BranchPK = branch1.PK;
					settings1.GroupNumber = 1;
					settings1.IsParentBranch = false;

					var settings2 = new BranchGroupSettings();
					settings2.BranchPK = branch2.PK;
					settings2.GroupNumber = 1;
					settings2.IsParentBranch = true;

					branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
					branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);

					headerWithLines.EnforceBranchLevelPostingRegistryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

					line2.RunPreSaveValidation();

					if (registryValue)
					{
						AssertNoError("Line 2 branch should no longer show error as both branches are in the same posting group", line2.AL_GBInfo, expectedError);
					}
					else
					{
						AssertNoError("Line 2 branch should not show any error as Branch level posting is not enabled", line2.AL_GBInfo, expectedError);
					}
				}
			}
			else
			{
				Assert(string.Format("For {0}-{1} the system does not prevent having mixture of branch lines", headerWithLines.AH_Ledger, headerWithLines.AH_TransactionType), true);
			}
		}

		public void TestCheckAL_PlaceOfSupplyWhenMultipleFixedPlaceOfSupplyIsNotAllowed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var header = (TransactionHeaderWithLines)Factory.NewWithValidTestData(GetExpectedParentBusinessObjectType());
				header.Lines.RemoveAll();
				var line1 = (TransactionLine)header.Lines.AddNew();
				if (header.NeedPlaceOfSupplyAtLineLevel)
				{
					var placeOfSupplyCode1 = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
					var placeOfSupplyCode2 = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[1].Code;

					line1.AL_PlaceOfSupply = placeOfSupplyCode1;

					var line2 = (TransactionLine)header.Lines.AddNew();
					line2.AL_PlaceOfSupply = placeOfSupplyCode2;
					AssertHasError(line2.AL_PlaceOfSupplyInfo, FormattableString.Invariant($"Posting a {header.HumanReadableName} with more than one Place of Supply is not allowed. Make sure the Fixed place of supply selected on all the lines are same, or post separate transaction for each location."));

					line2.AL_PlaceOfSupply = placeOfSupplyCode1;
					AssertNoError(line2.AL_PlaceOfSupplyInfo, FormattableString.Invariant($"Posting a {header.HumanReadableName} with more than one Place of Supply is not allowed. Make sure the Fixed place of supply selected on all the lines are same, or post separate transaction for each location."));
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestCheckAL_PlaceOfSupplyWhenMultipleFixedPlaceOfSupplyIsAllowed()
		{
			var header = (TransactionHeaderWithLines)Factory.NewWithValidTestData(GetExpectedParentBusinessObjectType());
			var line1 = (TransactionLine)header.Lines.AddNew();
			if (header.NeedPlaceOfSupplyAtLineLevel)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					var placeOfSupplyCode1 = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
					var placeOfSupplyCode2 = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[1].Code;

					line1.AL_PlaceOfSupply = placeOfSupplyCode1;

					var line2 = (TransactionLine)header.Lines.AddNew();
					line2.AL_PlaceOfSupply = placeOfSupplyCode2;
					AssertNoErrors(line2.AL_PlaceOfSupplyInfo);

					line2.AL_PlaceOfSupply = placeOfSupplyCode1;
					AssertNoErrors(line2.AL_PlaceOfSupplyInfo);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckAL_Calc_FirstSubClassParentId()
		{
			var line = (DependentTransactionLine)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			if (line.IsMultiSubAccountsSupported)
			{
				var glHeader = TestObjectCreator.CreateGLHeader();
				var glHeaderSubAccount1 = TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, false);
				TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, false);

				AssertNoErrors("Pre-condition", line.AL_Calc_FirstSubClassParentIdInfo);

				line.AL_AG = glHeader.PK;

				AssertCheckSubClassParentId(line.SubAccounts.FirstSubAccount.AL1_SubClassParentIdInfo, line.AL_Calc_FirstSubClassParentIdInfo, glHeaderSubAccount1, TestObjectCreator.Creditor1.PK, true);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckAL_Calc_SecondSubClassParentId()
		{
			var line = (DependentTransactionLine)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			if (line.IsMultiSubAccountsSupported)
			{
				var glHeader = TestObjectCreator.CreateGLHeader();
				TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, false);
				var glHeaderSubAccount2 = TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, false);

				AssertNoErrors("Pre-condition", line.AL_Calc_SecondSubClassParentIdInfo);

				line.AL_AG = glHeader.PK;

				AssertCheckSubClassParentId(line.SubAccounts.SecondSubAccount.AL1_SubClassParentIdInfo, line.AL_Calc_SecondSubClassParentIdInfo, glHeaderSubAccount2, TestObjectCreator.Staff.PK, false);
			}
			else
			{
				Assert(true);
			}
		}

		void AssertCheckSubClassParentId(ZPropertyInfo subAccountIdInfo, ZPropertyInfo subClassParentIdInfo, AccGLHeaderSubAccount glHeaderSubAccount, ZGuid subClassParentIdValue, bool isFirstSubAccount)
		{
			var checkedEnterMessage = "Please enter a Sub Account.";
			var subAccountCheckedEnterMessage = $"Please enter a Sub Account {(isFirstSubAccount ? 1 : 2)}.";

			AssertEquals("Pre-condition", false, glHeaderSubAccount.ASA_IsSubClassValidationRuleMandatory);
			AssertEquals("Pre-condition", ZGuid.Empty, subClassParentIdInfo.Value);
			AssertNoErrors("Pre-condition", subAccountIdInfo);
			AssertNoErrors("Pre-condition", subClassParentIdInfo);

			glHeaderSubAccount.ASA_IsSubClassValidationRuleMandatory = true;
			subClassParentIdInfo.Value = ZGuid.Empty;

			AssertHasError("AL1_SubClassParentId should have errors when it is empty and relate to ASA_IsSubClassValidationRuleMandatory is true.", subAccountIdInfo, checkedEnterMessage);
			AssertHasError("line's SubClassParentId should have errors when it is empty and relate to ASA_IsSubClassValidationRuleMandatory is true.", subClassParentIdInfo, subAccountCheckedEnterMessage);

			subClassParentIdInfo.Value = subClassParentIdValue;

			AssertNoErrors("AL1_SubClassParentId should have no errors when it is not empty and relate to ASA_IsSubClassValidationRuleMandatory is true.", subAccountIdInfo);
			AssertNoErrors("line's SubClassParentId should have no errors when it is not empty and relate to ASA_IsSubClassValidationRuleMandatory is true", subClassParentIdInfo);

			subClassParentIdInfo.Value = ZGuid.NewZGuid();
			var invalidValueErrorMessage = "Enter a valid Sub Account.";
			var subAccountInvalidValueErrorMessage = $"Enter a valid Sub Account {(isFirstSubAccount ? 1 : 2)}.";
			AssertHasError("AL1_SubClassParentId should have errors when enter value is invalid", subAccountIdInfo, invalidValueErrorMessage);
			AssertHasError("line's SubClassParentId should have errors when enter value is invalid", subClassParentIdInfo, subAccountInvalidValueErrorMessage);
		}

		public void TestCheckAL_GB_TaxBranch()
		{
			var anotherBranchInCurrentCompany = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "TST");
			GlbCompany.CurrentCompany.Factory.Save();

			TestObjectCreator.TestOrganisation.CompanyData.SetARTaxApplicable(true);
			TestObjectCreator.TestOrganisation.CompanyData.SetAPTaxApplicable(true);
			var header = (TransactionHeaderWithLines)Factory.NewWithValidTestData(GetExpectedParentBusinessObjectType());
			header.AH_OH = TestObjectCreator.TestOrganisation.PK;

			var line = header.Lines.AddNew();

			line.AL_GB_TaxBranch = TestObjectCreator.NonCurrentCompanyBranch.PK;
			AssertHasError(line.AL_GB_TaxBranchInfo, "Enter a valid Tax Branch.");

			line.AL_GB_TaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			AssertNoErrors(line.AL_GB_TaxBranchInfo);

			line.AL_GB_TaxBranch = anotherBranchInCurrentCompany.PK;
			AssertNoErrors(line.AL_GB_TaxBranchInfo);

			line.AL_GB_TaxBranch = ZGuid.Empty;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("PreCondition",false, header.CanApplyTaxBranch);
			line.Validation.ValidateAL_GB_TaxBranch();
			AssertNoErrors(line.AL_GB_TaxBranchInfo);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			if (header.CanApplyTaxBranch)
			{
				line.Validation.ValidateAL_GB_TaxBranch();
				AssertHasError(line.AL_GB_TaxBranchInfo, "Please enter a Tax Branch.");
			}
			else
			{
				line.Validation.ValidateAL_GB_TaxBranch();
				AssertNoErrors(line.AL_GB_TaxBranchInfo);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
			var testParentBizO = (TransactionHeaderWithLines)Factory.NewWithValidTestData(GetExpectedParentBusinessObjectType());
			TestBizO = (DependentTransactionLine)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			TestBizO.AL_AH = testParentBizO.PK;
			TestBizO.AL_AG = TestObjectCreator.GLHeader1.PK;
		}
		TestObjectCreator TestObjectCreator;

		protected Type GetExpectedBusinessObjectType() =>
			TestedTypeHelper.GetTestedType(GetType());

		protected abstract Type GetExpectedParentBusinessObjectType();

		protected DependentTransactionLine TestBizO;
	}
}
