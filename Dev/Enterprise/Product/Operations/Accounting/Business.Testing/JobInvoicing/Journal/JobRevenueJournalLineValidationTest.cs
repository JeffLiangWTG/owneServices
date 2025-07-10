using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobRevenueJournalLine))]
	public class JobRevenueJournalLineValidationTest : DependentTransactionLineValidationTest
	{
		public void TestJRJLineValidation_Exception_WhenClosedJobReopenerIsNull()
		{
			AssertNoExceptionThrown(() => new JobRevenueJournalLineValidation(JournalLine, null));
		}

		protected override Type GetExpectedParentBusinessObjectType()
		{
			return typeof(JobRevenueJournal);
		}

		public void TestCheckAL_JH_RevenueRecognitionDate()
		{
			RevenueRecognitionCollection registryCollection = new RevenueRecognitionCollection();
			RevenueRecognition registryValue = registryCollection.AddNew();
			registryValue.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			registryValue.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			registryValue.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);

			TestCaseHelper.ClearTable("AccPeriodManagement");
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupSinglePeriod(1, ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));

			Job testJob = TestObjectCreator.CreateJob("Z00001000", TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			testJob.Parent = shipment;
			TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Charge Code 1", TestObjectCreator.AUD, 100M, Creditor1,
			TestObjectCreator.AUD, 150M, TestObjectCreator.LocalClient);
			testJob.LocalChargesPK = ZGuid.Empty;
			Factory.Save();
			JobRevenueJournal invoice = Factory.New<JobRevenueJournal>();
			JobRevenueJournalLine line1 = (JobRevenueJournalLine)invoice.Lines.AddNew();

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			line1.AL_JH = testJob.PK;
			line1.AL_AC = testJob.Charges[0].ChargeCode.PK;
			AssertEquals("Precondition: AL_RevRecognitionType should be set correctly.", RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, line1.AL_RevRecognitionType);
			line1.Validation.ValidateAL_JH();
			AssertEquals("Line AL_JH should have error", true, line1.AL_JHInfo.HasErrors());
			AssertHasError("The error should be this:", line1.AL_JHInfo, "This journal cannot be posted until the 'Actual/Estimated Arrival Date' for this job is recorded. This job and charge code combination requires this date for revenue recognition purposes.");

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			line1.Validation.ValidateAL_JH();
			AssertEquals("Line AL_JH should have error", true, line1.AL_JHInfo.HasErrors());
			AssertHasError("The error should be this:", line1.AL_JHInfo, "This journal cannot be posted until the 'Actual/Estimated Arrival Date' for this job is recorded. This job and charge code combination requires this date for revenue recognition purposes.");

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			line1.AL_AC = line1.AL_AC; //to set revenue recognition type
			AssertEquals("Precondition: AL_RevRecognitionType should be set correctly.", RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate, line1.AL_RevRecognitionType);

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Now.AddDays(10);
			line1.Validation.ValidateAL_JH();
			AssertNoErrors("Line AL_JH should not have error", line1.AL_JHInfo);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.BrettsBirthday;
			line1.Validation.ValidateAL_JH();
			AssertHasErrorContaining(line1.AL_JHInfo, ZDateTime.BrettsBirthday.Date.ToShortDateString());

			TestObjectCreator.CreateJobChargeRevRecognition(testJob, "PIC", ZDateTime.Now);

			line1.Validation.ValidateAL_JH();
			AssertNoErrors("Line AL_JH should not have error", line1.AL_JHInfo);

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			line1.AL_AC = line1.AL_AC; //to set revenue recognition type
			AssertEquals("Precondition: AL_RevRecognitionType should be set correctly.", RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction, line1.AL_RevRecognitionType);

			line1.Validation.ValidateAL_JH();
			AssertNoErrors("Line AL_JH should not have error", line1.AL_JHInfo);

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			line1.AL_AC = line1.AL_AC; //to set revenue recognition type
			AssertEquals("Precondition: AL_RevRecognitionType should be set correctly.", RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, line1.AL_RevRecognitionType);

			line1.Validation.ValidateAL_JH();
			AssertHasError("The error should be this:", line1.AL_JHInfo, "This journal cannot be posted until the 'Customs Clearance Date' for this job is recorded. This job and charge code combination requires this date for revenue recognition purposes.");

			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			line1.Validation.ValidateAL_JH();
			AssertNoErrors("Line AL_JH should not have error", line1.AL_JHInfo);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestCheckAL_JH_WhenJournalHasNullClosedJobReopener()
		{
			Job job = (new BusinessObjectFactory()).NewJobWithValidTestDataForTesting<Job>();
			job.JH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			job.JH_GC = TestObjectCreator.NonCurrentCompanyBranch.Company.PK;
			job.Factory.Save();

			AssertNull("Precondition: JRJ ClosedJobReopener is null", JournalLine.ParentJournal.ClosedJobReopener);

			JournalLine.RunPreSaveValidation();
			AssertHasError(JournalLine.AL_JHInfo, "Please enter a Job Number.");

			JournalLine.AL_JH = job.PK;
			AssertHasError(JournalLine.AL_JHInfo, "Enter a valid Job Number.");
		}

		public void TestCheckAL_JH_ValidateClosedJob_GetCorrectPropertyInfo()
		{
			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			JournalLine.ParentJournal.SetupDependencies(mockClosedJobReopener.Object);

			AssertNotNull("Precondition: JRJ ClosedJobReopener is not null", JournalLine.ParentJournal.ClosedJobReopener);

			JournalLine.RunPreSaveValidation();

			mockClosedJobReopener.Verify(x => x.ValidateClosedJob(JournalLine.AL_JHInfo, It.IsAny<Job>()));
		}

		public void TestCheckAL_JH_ValidateClosedJob_GetNullJob()
		{
			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			JournalLine.ParentJournal.SetupDependencies(mockClosedJobReopener.Object);

			AssertNotNull("Precondition: JRJ ClosedJobReopener is not null", JournalLine.ParentJournal.ClosedJobReopener);

			JournalLine.RunPreSaveValidation();

			mockClosedJobReopener.Verify(x => x.ValidateClosedJob(It.IsAny<ZPropertyInfo>(), null));
		}

		public void TestCheckAL_JH_ValidateClosedJob_GetJobWithValue()
		{
			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			JournalLine.ParentJournal.SetupDependencies(mockClosedJobReopener.Object);

			AssertNotNull("Precondition: JRJ ClosedJobReopener is not null", JournalLine.ParentJournal.ClosedJobReopener);

			var job = Factory.NewJobForTesting<Job>();
			JournalLine.AL_JH = job.PK;

			mockClosedJobReopener.Verify(x => x.ValidateClosedJob(It.IsAny<ZPropertyInfo>(), job));
		}

		public void TestCheckAL_Desc()
		{
			JournalLine.AL_Desc = "";
			AssertHasError(JournalLine.AL_DescInfo, "Please enter a Description.");

			JournalLine.AL_Desc = "Desc";
			AssertNoError(JournalLine.AL_DescInfo, "Please enter a Description.");
		}

		public void TestCheckAL_ExchangeRate()
		{
			JournalLine.AL_ExchangeRate = 0M;
			AssertHasError(JournalLine.AL_ExchangeRateInfo, "Please enter an Exchange Rate.");

			JournalLine.AL_ExchangeRate = 1M;
			AssertNoError(JournalLine.AL_ExchangeRateInfo, "Please enter an Exchange Rate.");
		}

		public override void TestCheckAL_AG()
		{
			JournalLine.AL_AG = ZGuid.Empty;
			AssertNoErrors("Empty validation because user can't change AL_AG value and so can't fix errors here ", JournalLine.AL_AGInfo);
		}

		public override void TestCheckAL_SupplyType()
		{
			foreach (var regValue in new[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					JournalLine.AL_SupplyType = ZString.Empty;
					JournalLine.RunPreSaveValidation();
					AssertNoErrors("empty supply type value should not throw a validation error", JournalLine.AL_SupplyTypeInfo);

					JournalLine.AL_SupplyType = "XXX";
					JournalLine.RunPreSaveValidation();
					AssertNoErrors("invalid supply type value should not throw a validation error", JournalLine.AL_SupplyTypeInfo);
				}
			}
		}

		public override void TestCheckAL_AC()
		{
			JournalLine.AL_AC = ZGuid.Empty;
			AssertHasError(JournalLine.AL_ACInfo, "Please enter a Charge Code.");

			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "My";
			chargeCode.AC_Desc = "My Charge Code";
			chargeCode.AC_ChargeType = "CST";
			chargeCode.AC_AG_RevenueAccount = TestObjectCreator.GLHeader1.PK;

			JournalLine.AL_AC = chargeCode.PK;
			AssertNoErrors(JournalLine.AL_ACInfo);

			chargeCode.AC_AG_RevenueAccount = ZGuid.Empty;
			JournalLine.Validation.ValidateAL_AC();
			AssertHasError(JournalLine.AL_ACInfo, "Charge Code 'My' must have REV GL Account entered.");

			chargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			JournalLine.RunPreSaveValidation();
			AssertNoErrors(JournalLine.AL_ACInfo);
		}

		public void TestCheckDebitCreditSign()
		{
			JournalLine.DebitCreditSign = "";
			AssertHasError(JournalLine.DebitCreditSignInfo, "Please enter a value.");

			JournalLine.DebitCreditSign = DebitCreditDataEntry.CR;
			AssertNoErrors(JournalLine.DebitCreditSignInfo);

			JournalLine.DebitCreditSign = "XX";
			AssertHasError(JournalLine.DebitCreditSignInfo, "Enter a valid selection.");

			JournalLine.DebitCreditSign = DebitCreditDataEntry.CR;
			AssertNoErrors(JournalLine.DebitCreditSignInfo);
		}

		public void TestCheckCostRevenueType()
		{
			JournalLine.CostRevenueType = ZString.Empty;
			AssertHasError(JournalLine.CostRevenueTypeInfo, "Please enter a value.");

			JournalLine.CostRevenueType = "ABC";
			AssertHasError(JournalLine.CostRevenueTypeInfo, "Enter a valid selection.");

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code))
			{
				JournalLine.CostRevenueType = TransactionLineTypes.Revenue;
				AssertNoErrors(JournalLine.CostRevenueTypeInfo);

				JournalLine.ParentJournal.IsReverseTransaction = true;
				JournalLine.CostRevenueType = TransactionLineTypes.Cost;
				AssertNoErrors(JournalLine.CostRevenueTypeInfo);

				JournalLine.ParentJournal.IsReverseTransaction = false;
				JournalLine.CostRevenueType = TransactionLineTypes.Cost;
				AssertHasError(JournalLine.CostRevenueTypeInfo, "When 'Job Revenue Journal GL Account Defaulting Rules' Registry is set to REV, Line Cost/Revenue Type must be REV.");
			}

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code))
			{
				JournalLine.CostRevenueType = TransactionLineTypes.Cost;
				AssertNoErrors(JournalLine.CostRevenueTypeInfo);

				JournalLine.ParentJournal.IsReverseTransaction = true;
				JournalLine.CostRevenueType = TransactionLineTypes.Revenue;
				AssertNoErrors(JournalLine.CostRevenueTypeInfo);

				JournalLine.ParentJournal.IsReverseTransaction = false;
				JournalLine.CostRevenueType = TransactionLineTypes.Revenue;
				AssertHasError(JournalLine.CostRevenueTypeInfo, "When 'Job Revenue Journal GL Account Defaulting Rules' Registry is set to CST, Line Cost/Revenue Type must be CST.");
			}

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				JournalLine.CostRevenueType = TransactionLineTypes.Cost;
				AssertNoErrors(JournalLine.CostRevenueTypeInfo);

				JournalLine.CostRevenueType = TransactionLineTypes.Revenue;
				AssertNoErrors(JournalLine.CostRevenueTypeInfo);
			}
		}

		public void TestCheckOSUnsignedLineAmount()
		{
			JournalLine.OSUnsignedLineAmount = 0M;
			AssertHasError(JournalLine.OSUnsignedLineAmountInfo, "Please enter a value.");

			JournalLine.OSUnsignedLineAmount = 1M;
			AssertNoErrors(JournalLine.OSUnsignedLineAmountInfo);
		}

		public void TestCheckLocalUnsignedLineAmount_WhenValueIsEnteredAndNotEntered()
		{
			JournalLine.LocalUnsignedLineAmount = 0M;
			AssertHasError(JournalLine.LocalUnsignedLineAmountInfo, "Please enter a value.");

			JournalLine.LocalUnsignedLineAmount = 1M;
			AssertNoErrors(JournalLine.LocalUnsignedLineAmountInfo);
		}

		public void TestCheckLocalUnsignedLineAmount_WhenExRateIsOneAndLocalAndForeignAmountDoNotMatch()
		{
			var companyWthoutDecimalPoints = Factory.NewWithValidTestData<GlbCompany>();
			companyWthoutDecimalPoints.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.VietNam;
			companyWthoutDecimalPoints.GC_Name = "Vietnam Company";
			companyWthoutDecimalPoints.GC_RX_NKLocalCurrency = Enterprise.Core.Constants.CurrencyCodes.VietNam;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = companyWthoutDecimalPoints.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				JournalLine.AL_RX_NKTransactionCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;

				JournalLine.AL_ExchangeRate = 2m;
				JournalLine.OSUnsignedLineAmount = 200m;
				JournalLine.LocalUnsignedLineAmount = 100m;
				using (JournalLine.GetLocalAmountCalculationSuspender())
				{
					JournalLine.AL_ExchangeRate = 1m;

					Assert(JournalLine.AL_ExchangeRate == 1m);
					Assert(JournalLine.OSUnsignedLineAmount == 200m);
					Assert(JournalLine.LocalUnsignedLineAmount == 100m);
					Assert(JournalLine.OSUnsignedLineAmount != JournalLine.LocalUnsignedLineAmount);
					((JobRevenueJournalLineValidation)JournalLine.Validation).ValidateLocalUnsignedLineAmount();
					AssertNoError(JournalLine.LocalUnsignedLineAmountInfo, "The Local Amount should be equal to OS Amount when Local Currency is used");

					JournalLine.AL_RX_NKTransactionCurrency = Enterprise.Core.Constants.CurrencyCodes.VietNam;
					JournalLine.AL_ExchangeRate = 2m;
					JournalLine.OSUnsignedLineAmount = 200m;
					JournalLine.LocalUnsignedLineAmount = 100m;
					JournalLine.AL_ExchangeRate = 1m;
				}

				Assert(JournalLine.AL_ExchangeRate == 1m);
				Assert(JournalLine.OSUnsignedLineAmount == 200m);
				Assert(JournalLine.LocalUnsignedLineAmount == 100m);
				Assert(JournalLine.OSUnsignedLineAmount != JournalLine.LocalUnsignedLineAmount);
				((JobRevenueJournalLineValidation)JournalLine.Validation).ValidateLocalUnsignedLineAmount();
				AssertHasError(JournalLine.LocalUnsignedLineAmountInfo, "The Local Amount should be equal to OS Amount when Local Currency is used");

				JournalLine.AL_ExchangeRate = 1m;
				JournalLine.OSUnsignedLineAmount = 123m;
				AssertEquals(123m, JournalLine.LocalUnsignedLineAmount);
				Assert("Exchange Rate is 1", JournalLine.AL_ExchangeRate == 1);
				Assert("Local Amount and OS Amount is equal", JournalLine.OSUnsignedLineAmount == JournalLine.LocalUnsignedLineAmount);
				((JobRevenueJournalLineValidation)JournalLine.Validation).ValidateLocalUnsignedLineAmount();
				AssertNoError(JournalLine.LocalUnsignedLineAmountInfo, "The Local Amount should be equal to OS Amount when Local Currency is used");
			}
		}

		public void TestLocalUnsignedLineAmountValidationDoesNotThrowExceptionWhenTransactionCurrencyIsEmpty()
		{
			JournalLine.AL_RX_NKTransactionCurrency = ZString.Empty;
			JournalLine.OSUnsignedLineAmount = 100m;
			JournalLine.LocalUnsignedLineAmount = 200m;
			using (JournalLine.GetLocalAmountCalculationSuspender())
			{
				JournalLine.AL_ExchangeRate = 1m;
			}

			Assert("Transaction Currency is empty", JournalLine.AL_RX_NKTransactionCurrency.IsEmpty);
			AssertEquals("Exchange rate is 1", 1m, JournalLine.AL_ExchangeRate);
			AssertNotEquals("Local Amount and OS Amount is different", JournalLine.OSUnsignedLineAmount, JournalLine.LocalUnsignedLineAmount);
			AssertNoExceptionThrown("Null Reference Exception must no be thrown", () => ((JobRevenueJournalLineValidation)JournalLine.Validation).ValidateLocalUnsignedLineAmount());
		}

		public void TestRowWarningIfTheSameLineIsExist()
		{
			TestObjectCreator testObjectCreatorInNewFactory = new TestObjectCreator(new BusinessObjectFactory());
			Job job = testObjectCreatorInNewFactory.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			GlbDepartment nonMiscDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false));

			JournalLine.AL_AC = TestObjectCreator.CC1.PK;
			JournalLine.AL_JH = job.PK;
			JournalLine.AL_GB = Env.CurrentBranch.PK;
			JournalLine.AL_GE = nonMiscDepartment.PK;
			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			JournalLine.AL_ExchangeRate = 5M;
			JournalLine.OSUnsignedLineAmount = 100M;
			JournalLine.DebitCreditSign = DebitCreditDataEntry.DR;

			JournalLine.RunPreSaveValidation();
			AssertNoErrors("Precondition: JournalLine must not have errors.", JournalLine);

			JobRevenueJournalLine journalLine2 = (JobRevenueJournalLine)JournalLine.ParentJournal.Lines.AddNew();
			string expectedWarning = "This line is possibly wrong because line with the same data already exist in this journal.";

			journalLine2.CopyValuesFrom(JournalLine);
			AssertNoRowWarningContaining(JournalLine, expectedWarning);
			AssertNoRowWarningContaining(journalLine2, expectedWarning);

			journalLine2.ReverseDebitCreditSign();
			AssertNoRowWarningContaining(JournalLine, expectedWarning);
			AssertHasRowWarning(journalLine2, expectedWarning);

			JournalLine.ParentJournal.RunPreSaveValidation();
			AssertHasRowWarning(JournalLine, expectedWarning);
			AssertHasRowWarning(journalLine2, expectedWarning);

			journalLine2.AL_AC = ZGuid.Empty;
			journalLine2.AL_JH = ZGuid.Empty;
			journalLine2.AL_GB = ZGuid.Empty;
			journalLine2.AL_GE = ZGuid.Empty;
			journalLine2.AL_RX_NKTransactionCurrency = "";
			journalLine2.OSUnsignedLineAmount = 0M;
			journalLine2.LocalUnsignedLineAmount = 0M;

			JournalLine.AL_AC = ZGuid.Empty;
			JournalLine.AL_JH = ZGuid.Empty;
			JournalLine.AL_GB = ZGuid.Empty;
			JournalLine.AL_GE = ZGuid.Empty;
			JournalLine.AL_RX_NKTransactionCurrency = "";
			JournalLine.OSUnsignedLineAmount = 0M;
			JournalLine.LocalUnsignedLineAmount = 0M;
			AssertNoRowWarningContaining(JournalLine, expectedWarning);
			AssertNoRowWarningContaining(journalLine2, expectedWarning);

			JournalLine.AL_AC = TestObjectCreator.CC1.PK;
			JournalLine.AL_JH = job.PK;
			JournalLine.AL_GB = Env.CurrentBranch.PK;
			JournalLine.AL_GE = nonMiscDepartment.PK;
			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			JournalLine.AL_ExchangeRate = 5M;
			JournalLine.OSUnsignedLineAmount = 100M;
			journalLine2.CopyValuesFrom(JournalLine);
			journalLine2.ReverseDebitCreditSign();
			JournalLine.ParentJournal.RunPreSaveValidation();
			AssertHasRowWarning(JournalLine, expectedWarning);
			AssertHasRowWarning(journalLine2, expectedWarning);

			Factory.Save();
			JournalLine.ParentJournal.RunPreSaveValidation();
			AssertNoRowWarningContaining(JournalLine, expectedWarning);
			AssertNoRowWarningContaining(journalLine2, expectedWarning);
		}

		public void TestCheckAL_GEIsMiscellaneous()
		{
			string expectedMessage = "Cannot issue job charges for a miscellaneous department.";
			JournalLine.ParentJournal.IsReverseTransaction = true;
			JournalLine.AL_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, true)).PK;
			AssertNoError(JournalLine.AL_GEInfo, expectedMessage);

			JournalLine.ParentJournal.IsReverseTransaction = false;
			JournalLine.AL_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, true)).PK;
			AssertHasError(JournalLine.AL_GEInfo, expectedMessage);

			JournalLine.AL_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;
			AssertNoError(JournalLine.AL_GEInfo, expectedMessage);
		}

		public new void TestValidateAL_LocalExtraTaxAmountForIndia_STA_TaxID()
		{
			Assert("Not applicable as tax is not allowed for job revenue journal", true);
		}

		protected OrgHeader fCreditor1;
		protected OrgHeader Creditor1
		{
			get
			{
				if (fCreditor1 == null)
				{
					fCreditor1 = TestObjectCreator.CreateOrgHeader("CREDITOR1", true, false, true, false, false, false);
				}

				return fCreditor1;
			}
		}

		JobRevenueJournalLine JournalLine
		{
			get
			{
				if (JournalLine_cached == null)
				{
					JobRevenueJournal journal = Factory.New<JobRevenueJournal>();
					JournalLine_cached = (JobRevenueJournalLine)journal.Lines.AddNew();
				}

				return JournalLine_cached;
			}
		}
		JobRevenueJournalLine JournalLine_cached;

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;
	}
}
