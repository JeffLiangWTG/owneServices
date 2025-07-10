using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	public abstract class BaseWIPAccrualTest : TransactionLineTest
	{
		public override void TestAL_JHValidationOnAL_RevRecognitionType()
		{
			Assert("Test is not applicable as revenue recognition validation is not run for this line type", true);
		}

		[TestDate(2023, 03, 23, 23, 23, 23)]
		public void TestDeleteWhenIsInDataBaseIsTrue()
		{
			var line = CreateNewLine();
			Factory.Save();
			Assert(line.IsInDatabase);

			var innerExceptionMsg = $@"Line PK: {line.PK}, Line is in db.";

			AssertNoExceptionThrown("Delete", line.Delete);
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertContains("Reported error message basic", innerExceptionMsg, ExceptionReporterTestListener.Instance[0].InnerException.Message);
			AssertContains("Reported error message charge info",
				$@"ChargeInfo:
{line.RelatedJobCharge.GetAllPropertyValues()}"
				, ExceptionReporterTestListener.Instance[0].InnerException.Message);
			AssertContains("Reported error message line info",
				$@"LineInfo:
{line.GetAllPropertyValues()}"
				, ExceptionReporterTestListener.Instance[0].InnerException.Message);

			ErrorReporter.Clear();
		}

		public void TestDeleteByCouncurrencyResolverWhenLineWasNotInsertedInDbDueToServerConnectionProblem()
		{
			var line = (BaseWIPAccrual)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			((INeedRow)line).Row.AcceptChanges(); //When sql server kills connection, Factory.Save does this, but line is not inserted in db
			line.AL_ReverseDate = ZDateTime.Today;
			Assert(line.HasChanges);

			var ex = AssertExceptionThrown<ZSaveConcurrencyException>("Delete Exception", Factory.Save);
			var handler = new Mock<INotificationHandler>().Object;
			AssertNoExceptionThrown(() => ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler));

			Assert(line.IsDeleted);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			ErrorReporter.Clear();
		}

		protected override bool CanPersistedObjectBeDeleted
		{
			get
			{
				return false;
			}
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesBaseWIPAccrual()
		{
			var osList = new List<string>
			{
				nameof(ConcreteWipAccrual.ForeignAmountValue),
				nameof(ConcreteWipAccrual.AL_Calc_DisplayAmount)
			};

			var tester = new DecimalPlacesAttributeTester(ConcreteWipAccrual, ConcreteWipAccrual.Company);
			tester.CheckNonLocalCurrency(osList, nameof(ConcreteWipAccrual.CurrencyDecimals), nameof(ConcreteWipAccrual.AL_RX_NKTransactionCurrency), ConcreteWipAccrual);
		}

		protected override void SetupRelatedObjectsForFetchHintTest(TransactionLinesCollection collection)
		{
			base.SetupRelatedObjectsForFetchHintTest(collection);
			foreach (TransactionLine line in collection)
			{
				var charge = line.RelatedJobCharge ?? line.InvoicingJob.Charges.AddNew();
				charge.JR_AC = line.AL_AC;
				charge.JR_JH = line.AL_JH;
				charge.JR_GB = line.AL_GB;
				charge.JR_GE = line.AL_GE;
				if (line is WIP)
				{
					if (line.Header != null)
					{
						line.Header.OH_IsDebtor = true;
					}

					charge.JR_OH_SellAccount = line.AL_OH;
					if (charge.JR_AL_ARLine != line.PK)
					{
						charge.ReverseWIP(ZDateTime.Now);
						charge.JR_AL_ARLine = line.PK;
					}
				}
				else
				{
					if (line.Header != null)
					{
						line.Header.OH_IsCreditor = true;
					}

					charge.JR_OH_CostAccount = line.AL_OH;
					if (charge.JR_AL_APLine != line.PK)
					{
						charge.ReverseAccrual(ZDateTime.Now);
						charge.JR_AL_APLine = line.PK;
					}
				}
			}
		}

		[MasterFiles.Business.Testing.SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestRemoveReversedEventIfSavedWithNullReverseDate()
		{
			ConcreteWipAccrual.Logs.AddNew(Events.TransactionReversed);
			ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Empty;
			Factory.Save();
			ZQuery logFilter = new ZQuery(StmALogSchema.SL_Parent, ConcreteWipAccrual.PK);
			logFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.TransactionReversed.Code);
			StmALog[] reverseEvents = Factory.Load<StmALog>(logFilter);
			AssertEquals("Unsaved Log Events should be removed", 0, reverseEvents.Length);
			AssertEquals("A developer exception should be raised", 1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Unsaved Reverse Event Removed on non-reversed WIP/ACR PK = " + ConcreteWipAccrual.PK.ToString(), ExceptionReporterTestListener.Instance[0].Message);
			ExceptionReporterTestListener.Instance.Clear();

			ConcreteWipAccrual.AL_ReverseDate = ZDateTime.BrettsBirthday;

			ClearRelatedChargeLinkIfReversed(ConcreteWipAccrual);
			Factory.Save();
			reverseEvents = ConcreteWipAccrual.Logs.Find(logFilter);
			AssertEquals("There should be 1 reverse event", 1, reverseEvents.Length);
			ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Empty;
			AssertEquals("Reverse Date should still have a value because it can't be set on saved WIP/ACR.  If this fails consider adding code to report reverse events in DB when line not reversed.", ZDateTime.BrettsBirthday, ConcreteWipAccrual.AL_ReverseDate);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestExportBatchNumbers()
		{
			TestObjectCreator.CreateGenExportBatchSequencePostLine(1454, ConcreteWipAccrual.PK, 532);
			TestObjectCreator.CreateGenExportBatchSequenceReverseLine(95138, ConcreteWipAccrual.PK, 32767);
			AssertEquals("Export Batch Transaction Reference", "000145400532", ConcreteWipAccrual.ExportBatchTransactionReference);
			AssertEquals("Reverse Export Batch Transaction Reference", "009513832767", ConcreteWipAccrual.ExportReverseBatchTransactionReference);
		}

		public void TestExportBatchSequenceObjects()
		{
			AssertNull(ConcreteWipAccrual.ExportBatchSequencePostedObject);
			AssertNull(ConcreteWipAccrual.ExportBatchSequenceReversedObject);
			var batchSequencePostLine = TestObjectCreator.CreateGenExportBatchSequencePostLine(100, ConcreteWipAccrual.PK, 1);
			TestObjectCreator.CreateGenExportBatchSequencePostLine(100, ZGuid.NewZGuid(), 2);
			var batchSequenceReverseLine = TestObjectCreator.CreateGenExportBatchSequenceReverseLine(100, ConcreteWipAccrual.PK, 3);
			TestObjectCreator.CreateGenExportBatchSequenceReverseLine(100, ZGuid.NewZGuid(), 4);
			AssertNotNull(ConcreteWipAccrual.ExportBatchSequencePostedObject);
			AssertNotNull(ConcreteWipAccrual.ExportReverseBatchTransactionReference);
			AssertEquals(batchSequencePostLine.PK, ConcreteWipAccrual.ExportBatchSequencePostedObject.PK);
			AssertEquals(batchSequenceReverseLine.PK, ConcreteWipAccrual.ExportBatchSequenceReversedObject.PK);
		}

		[MasterFiles.Business.Testing.SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2013, 10, 15)]
		public void TestUpdateAL_ReverseDate_ToValidFutureDate()
		{
			ZDateTime jobDate = new ZDateTime(2013, 09, 10);
			var testJob = Factory.NewJobForTesting<Job>();
			testJob = TestObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			TestObjectCreator.CreateJobChargeRevRecognition(testJob, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, jobDate);
			var wip = TestObjectCreator.CreateWIP(testJob, TestObjectCreator.CC1, 1M, "test WIP", 100);
			wip.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			PeriodManager manager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2013, 09, 01));
			Period periodWithClosedSubLeger = manager.Periods[0]; // Job 1 Date's Period
			periodWithClosedSubLeger.AM_IsSubLedgerClosed = true;
			AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			wip.UpdateAL_ReverseDate();
			AssertEquals("AL_ReverseDate must be equal Today's for closed period.", ZDateTime.Today, wip.AL_ReverseDate.Date);
			AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			wip.SetModeToReversing();
			Assert(wip.Factory.HasContext(BusinessContext.SkipJobHeaderRefreshParentDuringWIPAccrualReversing));
			AssertEquals("Must equal the end date of period 2", manager.Periods[1].AM_EndDate, wip.AL_ReverseDate);
			Assert("Invoice line reverse date is greater than today.", wip.AL_ReverseDate > ZDateTime.Now);
			Assert("AL_ReverseDate_FutureSystemCalculatedValue is set", wip.AL_ReverseDate_FutureSystemCalculatedValue != DateTime.MinValue);
			AssertEquals("AL_ReverseDate is equal to AL_ReverseDate_FutureSystemCalculatedValue", wip.AL_ReverseDate, wip.AL_ReverseDate_FutureSystemCalculatedValue);
			Assert("Though WIP is beging reversed to future date, no validation error is caught as future reverse date is equal to system calculated valid future date except reverse date already reversed", !wip.AL_ReverseDateInfo.GetErrors().Any(x => !x.Message.Contains(ErrorToIgnoreWhenSetMoreThanOnceReverseDate)));
			wip.AL_ReverseDate = (ZDateTime)(ZDateTime.Now.ToDateTime().AddDays(5));
			AssertNotEquals("AL_ReverseDate is not equal to AL_ReverseDate_FutureSystemCalculatedValue", wip.AL_ReverseDate, wip.AL_ReverseDate_FutureSystemCalculatedValue);
			AssertHasError("Should show validation error as reverse date is future date and not equal to system calculated valid future date", wip.AL_ReverseDateInfo, "You can only reverse this transaction up to today's date or use Revenue Recognition date - 31-Oct-13");
			wip.AL_ReverseDate_FutureSystemCalculatedValue = DateTime.MinValue;
			wip.AL_ReverseDate = (ZDateTime)(ZDateTime.Now.ToDateTime().AddDays(5));
			AssertHasError("Error message should not show the revenue recognition date as it is not set.", wip.AL_ReverseDateInfo, "You can only reverse this transaction up to today's date");
		}

		[MasterFiles.Business.Testing.SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public override void TestUpdateAL_ReverseDate()
		{
			ZDateTime job1Date = new ZDateTime(2005, 2, 10);
			ZDateTime job2Date = new ZDateTime(2005, 5, 10);
			ZDateTime postDate = new ZDateTime(2004, 1, 10);
			Job testJob1 = Factory.NewJobForTesting<Job>();
			testJob1 = TestObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			TestObjectCreator.CreateJobChargeRevRecognition(testJob1, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, job1Date);
			Job testJob2 = Factory.NewJobForTesting<Job>();
			testJob2 = TestObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			TestObjectCreator.CreateJobChargeRevRecognition(testJob2, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, job2Date);
			Job testJob3 = Factory.NewJobForTesting<Job>();
			testJob3 = TestObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			TestObjectCreator.CreateJobChargeRevRecognition(testJob3, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, AccountingConstants.RevenueRecognitionDateConstants.Immediate);
			Job testJob4 = Factory.NewJobForTesting<Job>();
			testJob4 = TestObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			TestObjectCreator.CreateJobChargeRevRecognition(testJob4, RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob, AccountingConstants.RevenueRecognitionDateConstants.JobClosure);
			PeriodManager manager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2005, 01, 01));
			Period periodWithClosedSubLeger = Factory.New<Period>();
			periodWithClosedSubLeger = manager.Periods[1];
			periodWithClosedSubLeger.AM_IsSubLedgerClosed = true;
			Period nextOpenPeriod = manager.Periods[2];
			nextOpenPeriod.AM_IsSubLedgerClosed = false;
			Period periodWithOpenedSubLeger = Factory.New<Period>();
			periodWithOpenedSubLeger = manager.Periods[4];
			periodWithOpenedSubLeger.AM_IsSubLedgerClosed = false;
			ConcreteWipAccrual.AL_PostDate = postDate;
			ConcreteWipAccrual.AL_AC = TestObjectCreator.CC1.PK;
			ConcreteWipAccrual.AL_JH = testJob1.PK;
			ConcreteWipAccrual.RelatedJobCharge.JR_JH = testJob1.PK;
			ConcreteWipAccrual.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			ConcreteWipAccrual.UpdateAL_ReverseDate();
			AssertEquals("AL_ReverseDate must be equal ZDateTime.Now", ZDateTime.Now.Date, ConcreteWipAccrual.AL_ReverseDate.Date);
			bool originalRegistryValue = AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.Value;
			AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			try
			{
				ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Empty;
				ConcreteWipAccrual.UpdateAL_ReverseDate();
				AssertEquals("Must equal the end date of period 2", manager.Periods[2].AM_EndDate, ConcreteWipAccrual.AL_ReverseDate);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, originalRegistryValue);
			}

			ConcreteWipAccrual.AL_JH = testJob2.PK;
			ConcreteWipAccrual.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			ConcreteWipAccrual.UpdateAL_ReverseDate();
			AssertEquals("AL_ReverseDate must be equal Revenue Recognition Date", job2Date, ConcreteWipAccrual.AL_ReverseDate);
			ConcreteWipAccrual.AL_JH = testJob3.PK;
			ConcreteWipAccrual.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			ConcreteWipAccrual.UpdateAL_ReverseDate();
			AssertEquals("AL_ReverseDate must be equal ZDateTime.Now", ZDateTime.Now.Date, ConcreteWipAccrual.AL_ReverseDate.Date);
			ConcreteWipAccrual.AL_JH = testJob4.PK;
			ConcreteWipAccrual.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			ConcreteWipAccrual.UpdateAL_ReverseDate();
			AssertEquals("AL_ReverseDate must be equal ZDateTime.Now", ZDateTime.Now.Date, ConcreteWipAccrual.AL_ReverseDate.Date);
			ConcreteWipAccrual.AL_JH = Guid.Empty;
			ConcreteWipAccrual.RelatedJobCharge.JR_JH = ZGuid.Empty;
			ConcreteWipAccrual.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			ConcreteWipAccrual.UpdateAL_ReverseDate();
			AssertEquals("AL_ReverseDate must be equal ZDateTime.Now", ZDateTime.Now.Date, ConcreteWipAccrual.AL_ReverseDate.Date);
		}

		public void TestIHandleDeleteError_Saved()
		{
			Assert("Precondition: IsDeleted false", !ConcreteWipAccrual.IsDeleted);
			Assert("Precondition: IsInDatabase false", !ConcreteWipAccrual.IsInDatabase);
			var handleError = ConcreteWipAccrual as IHandleDeleteError;
			AssertNotNull("Implements IHandleDeleteError", handleError);
			Assert("RollbackAfterDeleteError is false on new WIP/ACR", !handleError.RollbackAfterDeleteError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);
			Factory.Save();
			Assert("IsInDatabase", ConcreteWipAccrual.IsInDatabase);
			Assert("RollbackAfterDeleteError is false on saved WIP/ACR", !handleError.RollbackAfterDeleteError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);
		}

		public void TestIHandleDeleteError_Deleted()
		{
			Assert("Precondition: IsDeleted false", !ConcreteWipAccrual.IsDeleted);
			Assert("Precondition: IsInDatabase false", !ConcreteWipAccrual.IsInDatabase);
			var handleError = ConcreteWipAccrual as IHandleDeleteError;
			AssertNotNull("Implements IHandleDeleteError", handleError);
			Assert("RollbackAfterDeleteError is false on new WIP/ACR", !handleError.RollbackAfterDeleteError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);
			ConcreteWipAccrual.Delete();
			Assert("IsDeleted", ConcreteWipAccrual.IsDeleted);
			Assert("RollbackAfterDeleteError is false on new deleted WIP/ACR", !handleError.RollbackAfterDeleteError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);
		}

		public void TestIsApportioned()
		{
			AssertEquals("IsApportioned", false, ConcreteWipAccrual.IsApportioned);
			JobCharge charge1 = Factory.New<JobCharge>();
			if (ConcreteWipAccrual.AL_LineType == ZArchitecture.Core.TransactionLineTypes.WIP)
			{
				ConcreteWipAccrual.RelatedJobCharge.ReverseWIP(ZDateTime.Now);
				ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Empty;
				charge1.JR_AL_ARLine = ConcreteWipAccrual.PK;
			}

			if (ConcreteWipAccrual.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Accrual)
			{
				ConcreteWipAccrual.RelatedJobCharge.ReverseAccrual(ZDateTime.Now);
				ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Empty;
				charge1.JR_AL_APLine = ConcreteWipAccrual.PK;
			}

			AssertEquals("RelatedCharge", charge1.PK, ConcreteWipAccrual.RelatedJobCharge.PK);
			AssertEquals("IsApportioned", false, ConcreteWipAccrual.IsApportioned);
			charge1.JR_E6 = ZGuid.NewZGuid();
			AssertEquals("IsApportioned", false, ConcreteWipAccrual.IsApportioned);
			JobCharge charge2 = Factory.New<JobCharge>();
			charge2.JR_E6 = charge1.JR_E6;
			AssertEquals("IsApportioned", ConcreteWipAccrual.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Accrual, ConcreteWipAccrual.IsApportioned);
		}

		public void TestChargeCodeFilter()
		{
			ConcreteWipAccrual.AL_GE = GlbDepartment.CurrentDepartment.PK;
			ConcreteWipAccrual.ChargeCodeCollection.Load();
			Assert("Should be some charge codes in collection", ConcreteWipAccrual.ChargeCodeCollection.Count > 0);
			foreach (AccChargeCode chargeCode in ConcreteWipAccrual.ChargeCodeCollection)
			{
				string chargeType = chargeCode.AC_ChargeType;
				decimal marginPercentage = chargeCode.AC_MarginPercentage;
				bool condition = (chargeType == Core.Constants.ChargeType.Margin && marginPercentage > 0) || chargeType == Core.Constants.ChargeType.Disbursement;
				Assert("Charge Type of " + chargeCode.AC_ChargeType + " should not be in collection", condition);
			}
		}

		public void TestJK_UniqueConsignRef()
		{
			JobHeader header = GetNewJobHeader();
			ForwardingShipment jobShipment = GetNewJobShipment();
			header.JH_ParentID = jobShipment.PK;
			ConcreteWipAccrual.AL_JH = header.PK;
			ConcreteWipAccrual.RelatedJobCharge.JR_JH = header.PK;
			JobConShipLink anotherJobConShipLink = GetJobConShipLink();
			ForwardingConsol anotherJobConsol = GetJobConsol();
			anotherJobConsol.JK_UniqueConsignRef = "OtherReference";
			anotherJobConsol.Shipments.Add(jobShipment);
			Factory.Save();
			AssertEquals("Consol relatted to SHipment", anotherJobConsol.JK_UniqueConsignRef, ConcreteWipAccrual.JK_UniqueConsignRef);
			JobConShipLink jobConShipLink = GetJobConShipLink();
			ForwardingConsol jobConsol = GetJobConsol();
			jobConsol.JK_UniqueConsignRef = "TestReference";
			jobConShipLink.JN_JK = jobConsol.PK;
			jobConShipLink.JN_JS = jobShipment.PK;
			JobConShipLink anotherJobConShipLink2 = GetJobConShipLink();
			ForwardingConsol anotherJobConsol2 = GetJobConsol();
			anotherJobConsol2.JK_UniqueConsignRef = "OtherReference2";
			anotherJobConShipLink2.JN_JK = anotherJobConsol2.PK;
			anotherJobConShipLink2.JN_JS = jobShipment.PK;
			var consolCost = jobConsol.GetApportionments().CostsCollection.TryAddNew();
			consolCost.FillWithValidTestData();
			ConcreteWipAccrual.RelatedJobCharge.JR_E6 = consolCost.PK;
			Factory.Save();
			AssertEquals(jobConsol.JK_UniqueConsignRef, ConcreteWipAccrual.JK_UniqueConsignRef);
		}

		public void TestEmptyJK_UniqueConsignRef()
		{
			AssertEquals(ZString.Empty, ConcreteWipAccrual.JK_UniqueConsignRef);
		}

		public void TestJK_MasterBillNum()
		{
			JobHeader header = GetNewJobHeader();
			ForwardingShipment jobShipment = GetNewJobShipment();
			header.JH_ParentID = jobShipment.PK;
			ConcreteWipAccrual.AL_JH = header.PK;
			ConcreteWipAccrual.RelatedJobCharge.JR_JH = header.PK;
			JobConShipLink anotherJobConShipLink = GetJobConShipLink();
			ForwardingConsol anotherJobConsol = GetJobConsol();
			anotherJobConsol.JK_MasterBillNum = "OtherNumber";
			anotherJobConsol.Shipments.Add(jobShipment);
			Factory.Save();
			AssertEquals("Master Bill Number from Consol related to Shipment", anotherJobConsol.JK_MasterBillNum, ConcreteWipAccrual.JK_MasterBillNum);
			JobConShipLink jobConShipLink = GetJobConShipLink();
			ForwardingConsol jobConsol = GetJobConsol();
			jobConsol.JK_MasterBillNum = "TestNumber";
			jobConShipLink.JN_JK = jobConsol.PK;
			jobConShipLink.JN_JS = jobShipment.PK;
			JobConShipLink anotherJobConShipLink2 = GetJobConShipLink();
			ForwardingConsol anotherJobConsol2 = GetJobConsol();
			anotherJobConsol2.JK_MasterBillNum = "OtherNumber2";
			anotherJobConShipLink2.JN_JK = anotherJobConsol2.PK;
			anotherJobConShipLink2.JN_JS = jobShipment.PK;
			var consolCost = jobConsol.GetApportionments().CostsCollection.TryAddNew();
			consolCost.FillWithValidTestData();
			ConcreteWipAccrual.RelatedJobCharge.JR_E6 = consolCost.PK;
			Factory.Save();
			AssertEquals(jobConsol.JK_MasterBillNum, ConcreteWipAccrual.JK_MasterBillNum);
		}

		public void TestEmptyJK_MasterBillNum()
		{
			AssertEquals(ZString.Empty, ConcreteWipAccrual.JK_MasterBillNum);
		}

		public void TestJK_CoLoadMasterBill()
		{
			var accrual = TestObjectCreator.CreateAccrual();
			accrual.AL_AH = ZGuid.Empty;

			var shipment = Factory.New<ForwardingShipment>();
			var jobHeader = TestObjectCreator.CreateJobHeader();
			jobHeader.JH_ParentID = shipment.PK;
			var accrual1 = TestObjectCreator.CreateAccrual(jobHeader);

			var consol = TestObjectCreator.CreateConsol();
			consol.JK_AgentType = "CLD";
			consol.JK_CoLoadMasterBill = "CLD1";

			var consol1 = TestObjectCreator.CreateConsol(consolNum: "C002");
			consol1.JK_AgentType = "CLD";
			consol1.JK_CoLoadMasterBill = "CLD1";
			var shipment1 = Factory.New<ForwardingShipment>();
			var jobHeader1 = TestObjectCreator.CreateJobHeader();
			jobHeader1.JH_ParentID = shipment1.PK;
			consol1.Shipments.Add(shipment1);
			var accrual2 = TestObjectCreator.CreateAccrual(jobHeader1);

			Factory.Save();
			AssertEquals("Accrual without job should have no coload MBL",ZString.Empty, accrual.JK_CoLoadMasterBill);
			AssertEquals("Accrual which has a job without related consol should have no coload MBL", ZString.Empty, accrual1.JK_CoLoadMasterBill);
			AssertEquals("CLD1", accrual2.JK_CoLoadMasterBill);
		}

		public void TestJS_HouseBill()
		{
			JobHeader header = GetNewJobHeader();
			ForwardingShipment jobShipment = GetNewJobShipment();
			jobShipment.JS_HouseBill = "TestNumber";
			header.JH_ParentID = jobShipment.PK;
			ConcreteWipAccrual.AL_JH = header.PK;
			ConcreteWipAccrual.RelatedJobCharge.JR_JH = header.PK;
			Factory.Save();
			AssertEquals(jobShipment.JS_HouseBill, ConcreteWipAccrual.JS_HouseBill);
		}

		public void TestEmptyJS_HouseBill()
		{
			AssertEquals(ZString.Empty, ConcreteWipAccrual.JS_HouseBill);
		}

		public void TestConsignorCode()
		{
			JobHeader header = GetNewJobHeader();
			ForwardingShipment jobShipment = GetNewJobShipment();
			jobShipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			header.JH_ParentID = jobShipment.PK;
			ConcreteWipAccrual.AL_JH = header.PK;
			ConcreteWipAccrual.RelatedJobCharge.JR_JH = header.PK;
			Factory.Save();
			AssertEquals(jobShipment.ConsignorDocumentaryAddress.Organisation.OH_Code, ConcreteWipAccrual.ConsignorCode);
		}

		public void TestEmptyConsignorCode()
		{
			AssertEquals(ZString.Empty, ConcreteWipAccrual.JK_MasterBillNum);
			JobHeader header = GetNewJobHeader();
			ForwardingShipment jobShipment = GetNewJobShipment();
			jobShipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			header.JH_ParentID = jobShipment.PK;
			ConcreteWipAccrual.AL_JH = header.PK;
			ConcreteWipAccrual.RelatedJobCharge.JR_JH = header.PK;
			Factory.Save();
			AssertEquals(ZString.Empty, ConcreteWipAccrual.ConsignorCode);
		}

		public void TestConsigneeCode()
		{
			JobHeader header = GetNewJobHeader();
			ForwardingShipment jobShipment = GetNewJobShipment();
			jobShipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			header.JH_ParentID = jobShipment.PK;
			ConcreteWipAccrual.AL_JH = header.PK;
			ConcreteWipAccrual.RelatedJobCharge.JR_JH = header.PK;
			Factory.Save();
			AssertEquals(jobShipment.ConsigneeDocumentaryAddress.Organisation.OH_Code, ConcreteWipAccrual.ConsigneeCode);
		}

		public void TestEmptyConsigneeCode()
		{
			AssertEquals(ZString.Empty, ConcreteWipAccrual.JK_MasterBillNum);
			JobHeader header = GetNewJobHeader();
			ForwardingShipment jobShipment = GetNewJobShipment();
			jobShipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			header.JH_ParentID = jobShipment.PK;
			ConcreteWipAccrual.AL_JH = header.PK;
			ConcreteWipAccrual.RelatedJobCharge.JR_JH = header.PK;
			Factory.Save();
			AssertEquals(ZString.Empty, ConcreteWipAccrual.ConsigneeCode);
		}

		public void TestRefreshBusInsertsCorrectTypeIntoCollection()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			WIPAccrualCollection collection = new WIPAccrualCollection(factory, new ZQuery());
			collection.Load();
			collection.IsManagedForDataRefresh = true;
			try
			{
				SetupDataAndSaveWIPAccrual();
				Assert("WIP/Accrual should be added to Collection", collection.Contains(ConcreteWipAccrual.PK));
				Assert("WIP/Accrual inserted should be same type as concrete type", collection.FindByPK(ConcreteWipAccrual.PK).GetType().IsAssignableFrom(ConcreteWipAccrual.GetType()));
			}
			finally
			{
				collection.IsManagedForDataRefresh = false;
			}
		}

		public void TestResettingReverseDateReportsDeveloperError()
		{
			ZDateTime fOriginalReverseDate = ZDateTime.Now;
			ErrorReporter.Clear();

			if (ConcreteWipAccrual.AL_LineType == TransactionLineTypes.Accrual)
			{
				ConcreteWipAccrual.RelatedJobCharge.ReverseAccrual(ZDateTime.Now.AddDays(-2));
			}
			else if (ConcreteWipAccrual.AL_LineType == TransactionLineTypes.WIP)
			{
				ConcreteWipAccrual.RelatedJobCharge.ReverseWIP(ZDateTime.Now.AddDays(-2));
			}
			else
			{
				ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Now.AddDays(-2);
			}

			SetupAndSaveLine();
			AssertEquals("No error", 0, ErrorReporter.TotalErrorCount);
			ConcreteWipAccrual.AL_ReverseDate = fOriginalReverseDate;
			Factory.Save();
			ConcreteWipAccrual.AL_ReverseDate = fOriginalReverseDate;
			ConcreteWipAccrual.AL_Desc += "line needs to have changes to save and report";
			Factory.Save();
			AssertResetReverseDateErrorMessageHasCorrectData(ConcreteWipAccrual, ErrorReporter.LastMessageReported);
			ConcreteWipAccrual.AL_ReverseDate = (ZDateTime)ConcreteWipAccrual.AL_ReverseDateInfo.OriginalValue;
			ErrorReporter.Clear();
			Factory.Save();
			AssertEquals("Should be no error because property prevented changing of reversedate", 0, ErrorReporter.TotalErrorCount);
		}

		public abstract void TestForeignCurrencyAndAmount();
		public void TestReversalEventDate()
		{
			ConcreteWipAccrual.Reverse();
			var lastReverseEvent = ConcreteWipAccrual.Logs.AddNew();
			using (lastReverseEvent.LockForUpdatingKeyFieldsForTesting())
			{
				lastReverseEvent.SL_SE_NKEvent = Events.TransactionReversed.Code;
				Factory.Save();
				AssertEquals("Reverse Date should equal current Date", lastReverseEvent.SL_EventTime, ConcreteWipAccrual.ReversalEventDate);
			}
		}

		public void TestIsReversing()
		{
			ConcreteWipAccrual.IsReversing = false;
			Assert("wipAccrual has no ReverseWipAccrual Context", !ConcreteWipAccrual.HasContext(BusinessContext.WipAccrualReversing));
			ConcreteWipAccrual.IsReversing = true;
			Assert("wipAccrual has ReverseWipAccrual Context", ConcreteWipAccrual.HasContext(BusinessContext.WipAccrualReversing));
			ConcreteWipAccrual.IsReversing = false;
			Assert("ReverseWipAccrual Context removed", !ConcreteWipAccrual.HasContext(BusinessContext.WipAccrualReversing));
		}

		public void TestReverse()
		{
			ConcreteWipAccrual.Reverse();
			AssertEquals("Reverse Date should equal current Date", Env.Time.CurrentLocalDateTime.Date, ConcreteWipAccrual.AL_ReverseDate.Date);
		}

		public void TestReverseWithPostAhead()
		{
			ZDateTime postDate = ZDateTime.Now.AddDays(10);
			ConcreteWipAccrual.AL_PostDate = postDate;
			ConcreteWipAccrual.Reverse();
			AssertEquals("Reverse Date should be Post Date", postDate, ConcreteWipAccrual.AL_ReverseDate);
		}

		public void TestReverseWithPreviousPostDate()
		{
			ZDateTime postDate = ZDateTime.Now.AddDays(-10);
			ConcreteWipAccrual.AL_PostDate = postDate;
			ConcreteWipAccrual.Reverse();
			AssertEquals("Reverse Date should be Today's Date", ZDateTime.Now.Date, ConcreteWipAccrual.AL_ReverseDate.Date);
		}

		public void TestReverseWithRevenueRecognitionDate()
		{
			ZDateTime postDate = ZDateTime.Now.AddDays(-10);
			ConcreteWipAccrual.AL_PostDate = postDate;
			ConcreteWipAccrual.AL_AC = TestObjectCreator.CC1.PK;
			Job testJob = Factory.NewJobForTesting<Job>();
			ConcreteWipAccrual.AL_JH = testJob.PK;
			ConcreteWipAccrual.Reverse();
			AssertEquals("Reverse Date should be Today's Date", ZDateTime.Now.Date, ConcreteWipAccrual.AL_ReverseDate.Date);
			ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Empty;
			ZDateTime expectedDate = ZDateTime.Now.AddDays(10);
			TestObjectCreator.CreateJobChargeRevRecognition(testJob, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, expectedDate);
			ConcreteWipAccrual.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			ConcreteWipAccrual.Reverse();
			AssertEquals("Reverse Date should be ExpectedDate", expectedDate, ConcreteWipAccrual.AL_ReverseDate);
			ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Empty;
			AccPeriodManagement period = new AccountingPeriodCalculator(Factory).GetPeriodManagementFromDate(expectedDate);
			period.AM_IsSubLedgerClosed = true;
			ConcreteWipAccrual.Reverse();
			AssertEquals("Reverse Date should be Today's Date", ZDateTime.Now.Date, ConcreteWipAccrual.AL_ReverseDate.Date);
			ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Empty;
			TestObjectCreator.CreateJobChargeRevRecognition(testJob, RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob, AccountingConstants.RevenueRecognitionDateConstants.JobClosure);
			ConcreteWipAccrual.Reverse();
			AssertEquals("Reverse Date should be Today's Date", ZDateTime.Now.Date, ConcreteWipAccrual.AL_ReverseDate.Date);
		}

		public void TestReverseDateValidation()
		{
			ConcreteWipAccrual.SetModeToReversing();
			ZDateTime postDate = ZDateTime.Now;
			ZDateTime reverseDate = ZDateTime.Now;
			ConcreteWipAccrual.AL_PostDate = postDate;
			ConcreteWipAccrual.AL_ReverseDate = reverseDate;
			Assert("Should be no errors on Reverse Date. We ignore the error: Transaction already reversed", !ConcreteWipAccrual.AL_ReverseDateInfo.GetErrors().Any(x => !x.Message.Contains(ErrorToIgnoreWhenSetMoreThanOnceReverseDate)));
			reverseDate = ZDateTime.Empty;
			ConcreteWipAccrual.AL_ReverseDate = reverseDate;
			Assert("Should be error on reverse date because it is mandatory when reversing", ConcreteWipAccrual.AL_ReverseDateInfo.HasErrors());
			reverseDate = postDate;
			ConcreteWipAccrual.AL_ReverseDate = reverseDate;
			Assert("Should now be no error on reverse date because it is valid. We ignore the error: Transaction already reversed", !ConcreteWipAccrual.AL_ReverseDateInfo.GetErrors().Any(x => !x.Message.Contains(ErrorToIgnoreWhenSetMoreThanOnceReverseDate)));
		}

		public void TestReverseValidationWithFuturePostings()
		{
			ConcreteWipAccrual.SetModeToReversing();
			ZDateTime postDate = ZDateTime.Now;
			ZDateTime reverseDate = ZDateTime.Now;
			postDate = postDate.AddMonths(1);
			ConcreteWipAccrual.AL_PostDate = postDate;
			ConcreteWipAccrual.AL_ReverseDate = reverseDate;
			Assert(@"Should not be an error on reverse date because you can have a reverse date that is earlier than the post date. We ignore the error: Transaction already reversed", !ConcreteWipAccrual.AL_ReverseDateInfo.GetErrors().Any(x => !x.Message.Contains(ErrorToIgnoreWhenSetMoreThanOnceReverseDate)));
			reverseDate = postDate.AddDays(1);
			ConcreteWipAccrual.AL_ReverseDate = reverseDate;
			AssertHasError("Error about not being able to reverse future to the post date", ConcreteWipAccrual.AL_ReverseDateInfo, "This transaction has been posted into the future. You can only reverse this transaction up to its post date.");
		}

		public void TestReverseDateValidationWithBackPosting()
		{
			ConcreteWipAccrual.SetModeToReversing();
			ZDateTime postDate = ZDateTime.Now;
			ZDateTime reverseDate = ZDateTime.Now;
			postDate = PreviousSubLedgerClosedPeriod.AM_StartDate.AddDays(5);
			ConcreteWipAccrual.AL_PostDate = postDate;
			ConcreteWipAccrual.AL_ReverseDate = reverseDate;
			Assert("Should be no errors on reverse date because you can only reverse a WIP/ACR to today's date. We ignore the error: Transaction already reversed", !ConcreteWipAccrual.AL_ReverseDateInfo.GetErrors().Any(x => !x.Message.Contains(ErrorToIgnoreWhenSetMoreThanOnceReverseDate)));
			reverseDate = PreviousSubLedgerClosedPeriod.AM_StartDate.AddDays(6);
			ConcreteWipAccrual.AL_ReverseDate = reverseDate;
			Assert(@"Should be an error on reverse date because you can't 
								reverse to a closed subledger period, even though the 
								reverse date is in the future when compared with the post date", ConcreteWipAccrual.AL_ReverseDateInfo.HasErrors());
			reverseDate = ZDateTime.Now;
			ConcreteWipAccrual.AL_ReverseDate = reverseDate;
			Assert("Should be no errors on reverse date because you can only reverse a WIP/ACR to today's date. We ignore the error: Transaction already reversed", !ConcreteWipAccrual.AL_ReverseDateInfo.GetErrors().Any(x => !x.Message.Contains(ErrorToIgnoreWhenSetMoreThanOnceReverseDate)));
			reverseDate = ZDateTime.Now.AddDays(5);
			ConcreteWipAccrual.AL_ReverseDate = reverseDate;
			Assert("Should be an error on reverse date because you can't reverse into the future with a back posted WIP/Accrual", ConcreteWipAccrual.AL_ReverseDateInfo.HasErrors());
			AssertHasError("Error on reverse date due to above error", ConcreteWipAccrual.AL_ReverseDateInfo, "You can only reverse this transaction up to today's date");
		}

		public void TestReverseDateValidationNotPresentWhenNotReversing()
		{
			// No call to set the mode to reversing, i.e. normal mode
			ConcreteWipAccrual.AL_PostDate = ZDateTime.Now;
			ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Empty;
			Assert("Should be no error on empty reverse date when not reversing. We ignore the error: Transaction already reversed", !ConcreteWipAccrual.AL_ReverseDateInfo.GetErrors().Any(x => !x.Message.Contains(ErrorToIgnoreWhenSetMoreThanOnceReverseDate)));
		}

		[MasterFiles.Business.Testing.SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestDefaultReverseDateOnReversing()
		{
			ZDateTime postDate = ZDateTime.Now.AddMonths(1);
			ConcreteWipAccrual.AL_PostDate = postDate;
			ConcreteWipAccrual.UpdateAL_ReverseDate();
			AssertEquals("Default reverse date", postDate, ConcreteWipAccrual.AL_ReverseDate);
		}

		public void TestIsReversed()
		{
			ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Empty;
			Assert("Should not be reversed", !ConcreteWipAccrual.IsReversed);
			if (ConcreteWipAccrual.RelatedJobCharge != null && ConcreteWipAccrual.AL_LineType == TransactionLineTypes.Accrual)
			{
				ConcreteWipAccrual.RelatedJobCharge.ReverseAccrual(ZDateTime.Now);
			}
			else if (ConcreteWipAccrual.RelatedJobCharge != null && ConcreteWipAccrual.AL_LineType == TransactionLineTypes.WIP)
			{
				ConcreteWipAccrual.RelatedJobCharge.ReverseWIP(ZDateTime.Now);
			}
			else
			{
				ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Now;
			}
			Assert("Should be reversed", ConcreteWipAccrual.IsReversed);
		}

		public virtual void TestDontProcessConsolCostAfterReversingAccrual()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			JobConsolCost cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			JobCharge charge = ConcreteWipAccrual.RelatedJobCharge;
			charge.JR_E6 = cost.PK;
			Factory.Save();
			ConcreteWipAccrual.Reverse(false);
			ClearRelatedChargeLinkIfReversed(ConcreteWipAccrual);
			Factory.Save();
			AssertNotEquals("JR_E6", ZGuid.Empty, charge.JR_E6);
			AssertNotNull(Factory.Load<JobConsolCost>(cost.PK));
		}

		public virtual void TestProcessConsolCostAfterReversingAccrual()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			JobConsolCost cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_APLine = ConcreteWipAccrual.PK;
			charge.JR_E6 = cost.PK;
			Factory.Save();
			ConcreteWipAccrual.Reverse(true);
			Factory.Save();
			AssertNotEquals("JR_E6", ZGuid.Empty, charge.JR_E6);
			AssertNotNull(Factory.Load<JobConsolCost>(cost.PK));
		}

		public void TestReversingIsLogged()
		{
			Factory.Save();
			AssertNull("Should not create event if transaction is not reversed", ConcreteWipAccrual.Logs.MostRecentLogByEventTime(Events.TransactionReversed));

			if (ConcreteWipAccrual.RelatedJobCharge != null && ConcreteWipAccrual.AL_LineType == TransactionLineTypes.Accrual)
			{
				ConcreteWipAccrual.RelatedJobCharge.ReverseAccrual(ZDateTime.Now);
			}
			else if (ConcreteWipAccrual.RelatedJobCharge != null && ConcreteWipAccrual.AL_LineType == TransactionLineTypes.WIP)
			{
				ConcreteWipAccrual.RelatedJobCharge.ReverseWIP(ZDateTime.Now);
			}
			else
			{
				ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Now;
				ClearRelatedChargeLinkIfReversed(ConcreteWipAccrual);
			}
			Factory.Save();
			StmALog lastReverseEvent = ConcreteWipAccrual.Logs.MostRecentLogByEventTime(Events.TransactionReversed);
			AssertNotNull("Should create event when AL_ReverseDate is changed to non-empty value", lastReverseEvent);
			Factory.Save();
			AssertSame("Should not create more events for an already reversed transaction", lastReverseEvent, ConcreteWipAccrual.Logs.MostRecentLogByEventTime(Events.TransactionReversed));
		}

		public void TestSequence()
		{
			AssertEquals("Default Sequence", (short)1, ConcreteWipAccrual.AL_Sequence);
		}

		public void TestAL_Desc()
		{
			JobHeader header = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			ForwardingShipment jobShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			header.JH_ParentID = jobShipment.PK;
			Charge charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_JH = header.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_OSCostAmt = 100;
			charge.JR_Desc = "Charge description";
			Factory.Save();
			AccTransactionLines accrual = charge.APLine;
			AccTransactionLines wIP = charge.ARLine;
			AssertEquals(accrual.AL_Desc, "Charge description");
			AssertEquals(wIP.AL_Desc, "Charge description");
		}

		public void TestAL_JH()
		{
			ConcreteWipAccrual.AL_JH = ZGuid.Empty;
			Assert("Should validate on emtpy", ConcreteWipAccrual.HasErrors);
			Job job = GetSavedJob(GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
			ConcreteWipAccrual.AL_JH = job.PK;
			AssertEquals("Branch should be set", job.JH_GB, ConcreteWipAccrual.AL_GB);
			AssertEquals("Department should be set", job.JH_GE, ConcreteWipAccrual.AL_GE);
		}

		public void TestJob()
		{
			ConcreteWipAccrual.AL_JH = ZGuid.Empty; // Because SetUp sets Job for passing CriticalValidation
			AssertNull("Now Job should be null", ConcreteWipAccrual.Job);
			Job job = GetSavedJob(GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
			ConcreteWipAccrual.AL_JH = job.PK;
			AssertEquals("Should return Referenced Job PK", job.PK, ConcreteWipAccrual.Job.PK);
		}

		public void TestAL_GB()
		{
			ConcreteWipAccrual.AL_GB = ZGuid.Invalid;
			Assert("Branch should validate", ConcreteWipAccrual.AL_GBInfo.HasErrors());
		}

		public void TestAL_GE()
		{
			ConcreteWipAccrual.AL_GE = ZGuid.Invalid;
			Assert("Department should validate", ConcreteWipAccrual.AL_GEInfo.HasErrors());
		}

		public void TestAL_AC()
		{
			ConcreteWipAccrual.AL_AC = ZGuid.Empty;
			Assert("Charge Code should validate", ConcreteWipAccrual.AL_ACInfo.HasErrors());
			ConcreteWipAccrual.ChargeCodeCollection.Load();
			AccChargeCode validChargeCode = ConcreteWipAccrual.ChargeCodeCollection[0];
			ConcreteWipAccrual.AL_AC = validChargeCode.PK;
			Assert("Charge Code should now be valid", !ConcreteWipAccrual.AL_ACInfo.HasErrors());
		}

		public void TestAL_RX_NKTransactionCurrency()
		{
			ConcreteWipAccrual.AL_RX_NKTransactionCurrency = ZString.Empty;
			Assert("AL_RX_NKTransactionCurrency should validate", ConcreteWipAccrual.AL_RX_NKTransactionCurrencyInfo.HasErrors());
			ConcreteWipAccrual.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Assert("AL_RX_NKTransactionCurrency should not validate", !ConcreteWipAccrual.AL_RX_NKTransactionCurrencyInfo.HasErrors());
		}

		public void TestAL_PostDateIfAbleToReverse()
		{
			ConcreteWipAccrual.AL_PostDate = ZDateTime.Today.Date;
			Assert("Can post to date up to today's date", !ConcreteWipAccrual.AL_PostDateInfo.HasErrors());
			ConcreteWipAccrual.AL_PostDate = ZDateTime.Today.Date.AddDays(1);
			Assert("Cannot post to date greater than today's date", ConcreteWipAccrual.AL_PostDateInfo.HasErrors());
			AssertEquals("Message for above error", ConcreteWipAccrual.LineValidation_MightBeNull.PostDateMustBeAtOrBeforeTodaysDate, ConcreteWipAccrual.AL_PostDateInfo.GetErrors().GetFirstMessage());
		}

		public void TestNoPostDateValidationOnReversing()
		{
			ConcreteWipAccrual.SetModeToReversing();
			ConcreteWipAccrual.AL_PostDate = ZDateTime.Now.AddDays(5);
			Assert("Should be no errors on post date when reversing", !ConcreteWipAccrual.AL_PostDateInfo.HasErrors());
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			PeriodManager periodManager = new PeriodManager(Factory);
			int thisPeriod = periodCalculator.GetPeriodFromDate(ZDateTime.Today);
			ZDateTime startDate = new ZDateTime(ZDateTime.Today.Year, ZDateTime.Today.Month, 1);
			ZDateTime endDate = startDate.AddMonths(1).AddDays(-1);
			AccPeriodManagement period = periodManager.CreateOnePeriod(thisPeriod, startDate, endDate, Factory);
			period.AM_IsGeneralLedgerClosed = false;
			period.AM_IsSubLedgerClosed = false;
			int prevPeriod = periodCalculator.GetPreviousPeriod(thisPeriod);
			int startDateYear = ZDateTime.Today.Month == 1 ? startDate.Year - 1 : startDate.Year;
			startDate = new ZDateTime(startDateYear, startDate.AddMonths(-1).Month, 1);
			endDate = startDate.AddMonths(1).AddDays(-1);
			AccPeriodManagement closedPeriod = periodCalculator.GetPeriodManagementFromDate(startDate);
			closedPeriod.AM_IsGeneralLedgerClosed = true;
			closedPeriod.AM_IsSubLedgerClosed = true;
			BaseWIPAccrual newWIPAccrual = (BaseWIPAccrual)Factory.New(GetExpectedBusinessObjectType());
			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			if (newWIPAccrual.AL_LineType == TransactionLineTypes.WIP)
			{
				charge.JR_AL_ARLine = newWIPAccrual.PK;
			}

			if (newWIPAccrual.AL_LineType == TransactionLineTypes.Accrual)
			{
				charge.JR_AL_APLine = newWIPAccrual.PK;
			}

			newWIPAccrual.AL_AG = TestObjectCreator.GLHeader1.PK;
			newWIPAccrual.AL_JH = charge.JR_JH;
			newWIPAccrual.AL_PostDate = closedPeriod.AM_StartDate;
			AssertEquals("Posting to closed period should be error", true, newWIPAccrual.AL_PostDateInfo.HasErrors());
			newWIPAccrual.AL_PostDate = ZDateTime.Today;
			AssertEquals("Valid Post Date", false, newWIPAccrual.AL_PostDateInfo.HasErrors());
			Factory.Save();
			period.AM_IsGeneralLedgerClosed = true;
			period.AM_IsSubLedgerClosed = true;
			Factory.Save();
			newWIPAccrual.Validation.ValidateAL_PostDate();
			AssertEquals("Period for existing transaction closed. Should not validate", false, newWIPAccrual.AL_PostDateInfo.HasErrors());
		}

		public void TestAL_OSExTaxAmountValidates()
		{
			ConcreteWipAccrual.AL_OSExTaxAmount = 0m;
			Assert("AL_OSExTaxAmount should validate", ConcreteWipAccrual.AL_OSExTaxAmountInfo.HasErrors());
			ConcreteWipAccrual.AL_OSExTaxAmount = 50m;
			Assert("AL_OSExTaxAmount should not validate", !ConcreteWipAccrual.AL_OSExTaxAmountInfo.HasErrors());
		}

		public void TestOSAmountUpdatedOnSave()
		{
			ConcreteWipAccrual.AL_OSExTaxAmount = 50m;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(ConcreteWipAccrual);
			ConcreteWipAccrual.OnSaving();
			AssertEquals("OSAmount should equal LineAmount", 50m, ConcreteWipAccrual.AL_OverseasTotal);
		}

		public void TestGridDisplayAmount()
		{
			ConcreteWipAccrual.AL_OSExTaxAmount = 50.00m;
			AssertEquals("Grid Display Amount", 50m * ConcreteWipAccrual.DisplayMultiplier, ConcreteWipAccrual.AL_Calc_DisplayAmount);
		}

		public override void TestInternalOSAmountFieldsSetOnLoadCorrectly()
		{
			SetupLineExRatesAndAmounts(0.57m, 1000, 200.453m, 0.00m);
			Line.AL_RX_NKTransactionCurrency = "AUD";
			Line.Factory.Save();
			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();
			TransactionLine loadedLine = (TransactionLine)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Line.PK);
			AssertEquals("OS Amount should equal LineAmount", Line.AL_OSExTaxAmount, Line.AL_OverseasTotal);
		}

		public override void TestOnSavingDateDifference()
		{
			try
			{
				ErrorReporter.Clear();
				BaseWIPAccrual testLine = GetNewBusinessObject() as BaseWIPAccrual;
				testLine.AL_GB = GlbBranch.CurrentBranch.PK;
				testLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
				testLine.AL_ReverseDate = ZDateTime.Empty;
				ClearRelatedChargeLinkIfReversed(testLine);
				ErrorReporter.Clear();
				Factory.Save();
				AssertEquals("No error", 0, ErrorReporter.TotalErrorCount);
				testLine.AL_ReverseDate = new ZDateTime(2000, 1, 1);
				ClearRelatedChargeLinkIfReversed(testLine);
				ErrorReporter.Clear();
				Factory.Save();
				AssertEquals("No error", 0, ErrorReporter.TotalErrorCount);
				ErrorReporter.Clear();
				ZDateTime newValue = ZDateTime.Empty;
				testLine.AL_ReverseDate = newValue;
				ClearRelatedChargeLinkIfReversed(testLine);
				Factory.Save();
				AssertEquals("Should report all new errors since last save", 0, ErrorReporter.TotalErrorCount);
				ErrorReporter.Clear();
				newValue = new ZDateTime(2007, 6, 22);
				testLine.AL_ReverseDate = newValue;
				testLine.AL_Desc += "line needs to have changes to save and report";
				ClearRelatedChargeLinkIfReversed(testLine);
				Factory.Save();
				AssertEquals("Should report all new errors since last save", 1, ErrorReporter.TotalErrorCount);
				AssertResetReverseDateErrorMessageHasCorrectData(testLine, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
				if (testLine.IsSavedByFactory)
				{
					newValue = testLine.AL_ReverseDate.AddDays(1);
					testLine.AL_ReverseDate = newValue;
					testLine.AL_Desc += "line needs to have changes to save and report";
					ClearRelatedChargeLinkIfReversed(testLine);
					Factory.Save();
					AssertResetReverseDateErrorMessageHasCorrectData(testLine, ErrorReporter.LastMessageReported);
				}
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		protected override string ARLineType
		{
			get
			{
				return TransactionLineTypes.WIP;
			}
		}

		protected override string APLineType
		{
			get
			{
				return TransactionLineTypes.Accrual;
			}
		}

		public override void TestRelatedJobChargeWithoutDbHit()
		{
			Line = base.CreateNewLine();
			Line.AL_AG = TestObjectCreator.GLHeader1.PK;
			Line.UpdateAL_ReverseDate();

			AssertDbHitCount(0);

			Factory.Save();

			AssertDbHitCount(1);
		}

		[MasterFiles.Business.Testing.SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAL_ReverseDateConcurrency()
		{
			Factory.RefreshEnabled = false;
			Factory.Save();
			Line.AL_ReverseDate = new ZDateTime(2001, 1, 4);
			if (Line.RelatedJobCharge != null)
			{
				Line.RelatedJobCharge.JR_AL_ARLine = ZGuid.Empty;
			}

			if (Line.RelatedJobCharge != null)
			{
				Line.RelatedJobCharge.JR_AL_APLine = ZGuid.Empty;
			}

			if (Line.TransactionHeader != null)
			{
				Line.TransactionHeader.AH_DueDate = Line.AL_ReverseDate; // For GLJournalLine
			}

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			TransactionLine loadedLine = (TransactionLine)newFactory.Load(GetExpectedBusinessObjectType(), Line.PK);
			loadedLine.AL_ReverseDate = new ZDateTime(2001, 1, 9);
			if (loadedLine.TransactionHeader != null)
			{
				loadedLine.TransactionHeader.AH_DueDate = loadedLine.AL_ReverseDate; // For GLJournalLine
			}

			Factory.Save();
			try
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				newFactory.Save();
				Fail("Saving should fail");
			}
			catch (Exception e)
			{
				ZExceptionReporting.HandleSaveException(e);
			}

			Assert(UnitTestUserNotification.Instance.LastMessage.Text, UnitTestUserNotification.Instance.LastMessage.Text.Contains("The system cannot automatically merge your changes because there are conflicts with critical fields."));
		}

		public void TestGetNewValidationCore()
		{
			Assert("Validation before saving", typeof(BaseWIPAccrualValidation).IsAssignableFrom(ConcreteWipAccrual.Validation.GetType()));
			Factory.Save();
			Assert("Validation after saving", typeof(TransactionLineEmptyValidation).IsAssignableFrom(ConcreteWipAccrual.Validation.GetType()));
			ConcreteWipAccrual.SetModeToReversing();
			Assert("Precondition: IsReversing", ConcreteWipAccrual.IsReversing);
			Assert("Validation on reversing", typeof(BaseWIPAccrualReverseValidation).IsAssignableFrom(ConcreteWipAccrual.Validation.GetType()));
		}

		protected override bool LineCanHaveTaxComponent
		{
			get
			{
				return false;
			}
		}

		protected override bool LineCanHaveForeignCurrency
		{
			get
			{
				return false;
			}
		}

		protected JobHeader GetNewJobHeader()
		{
			JobHeader header = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return header;
		}

		protected JobConShipLink GetJobConShipLink()
		{
			return Factory.NewWithValidTestData<JobConShipLink>();
		}

		protected ForwardingConsol GetJobConsol()
		{
			ForwardingConsol fW = Factory.NewWithValidTestData<ForwardingConsol>();
			return fW;
		}

		protected ForwardingShipment GetNewJobShipment()
		{
			return Factory.NewWithValidTestData<ForwardingShipment>();
		}

		protected BaseWIPAccrual ConcreteWipAccrual
		{
			get
			{
				return (BaseWIPAccrual)Line;
			}
		}

		readonly string ErrorToIgnoreWhenSetMoreThanOnceReverseDate = "This transaction has already been reversed";
		protected override bool AcceptAL_AG
		{
			get
			{
				return false;
			}
		}

		protected override string ExpectedEmptyAL_ACErrorMessage
		{
			get
			{
				return "Please enter a Charge Code.";
			}
		}

		protected override TransactionLine CreateNewLine()
		{
			var line = base.CreateNewLine();
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			return line;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Line;
		}

		protected Job GetSavedJob(ZGuid branch, ZGuid department)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var job = factory.NewJobForTesting<JobForTest>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = TestObjectCreator.GetRandomString(9);
			factory.Save();
			return job;
		}

		protected void SetupDataAndSaveWIPAccrual()
		{
			ConcreteWipAccrual.AL_JH = GetSavedJob(GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK).PK;
			ConcreteWipAccrual.ChargeCodeCollection.Load();
			ConcreteWipAccrual.AL_AC = ConcreteWipAccrual.ChargeCodeCollection[0].PK;
			ConcreteWipAccrual.AL_OSExTaxAmount = 50m;
			ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Now;
			if (ConcreteWipAccrual.AL_LineType == TransactionLineTypes.WIP)
			{
				ConcreteWipAccrual.RelatedJobCharge.JR_AL_ARLine = ZGuid.Empty;
			}

			if (ConcreteWipAccrual.AL_LineType == TransactionLineTypes.Accrual)
			{
				ConcreteWipAccrual.RelatedJobCharge.JR_AL_APLine = ZGuid.Empty;
			}

			ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Empty;
			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			if (ConcreteWipAccrual.AL_LineType == TransactionLineTypes.WIP)
			{
				charge.JR_AL_ARLine = ConcreteWipAccrual.PK;
			}

			if (ConcreteWipAccrual.AL_LineType == TransactionLineTypes.Accrual)
			{
				charge.JR_AL_APLine = ConcreteWipAccrual.PK;
			}

			charge.JR_JH = ConcreteWipAccrual.AL_JH;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(ConcreteWipAccrual);
			ConcreteWipAccrual.RunPreSaveValidation();
			Assert("Business Object has not been setup for saving correctly", !ConcreteWipAccrual.HasErrors);
			ConcreteWipAccrual.Factory.Save();
		}

		protected AccChargeCode GetFirstChargeCode()
		{
			return ConcreteWipAccrual.Factory.Load<AccChargeCode>(TestCaseHelper.GetFirstPKFromTable(AccChargeCodeSchema.Constants.TableName));
		}

		protected class JobForTest : Job
		{
			public JobForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				JH_JobNum = Env.NumberFountains.JobShipmentNumber.GetNextFormatted(Factory);
			}
		}

		void ClearRelatedChargeLinkIfReversed(BaseWIPAccrual wipAccrual)
		{
			if (wipAccrual.IsReversed && wipAccrual.RelatedJobCharge != null)
			{
				if (wipAccrual.AL_LineType == TransactionLineTypes.WIP)
				{
					wipAccrual.RelatedJobCharge.ClearRevenueLink();
				}

				if (wipAccrual.AL_LineType == TransactionLineTypes.Accrual)
				{
					wipAccrual.RelatedJobCharge.ClearCostLink();
				}
			}
		}
	}

	public class WIPAccrualTypeDeciderTest : TestCaseWithFactory
	{
		public void TestIsApplicableTo()
		{
			AccTransactionLines line = Factory.New<AccTransactionLines>();
			DataRow row = ((INeedDataSet)Factory).Data.Tables[AccTransactionLinesSchema.Constants.TableName].Rows[0];
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			Assert("WIP", WIPAccrualTypeDecider.IsApplicableTo(row));
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			Assert("Accrual", WIPAccrualTypeDecider.IsApplicableTo(row));
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			AssertEquals("Cost", false, WIPAccrualTypeDecider.IsApplicableTo(row));
			line.AL_LineType = "BLA";
			AssertEquals("BLA", false, WIPAccrualTypeDecider.IsApplicableTo(row));
			line.AL_LineType = "";
			AssertEquals("Empty string", false, WIPAccrualTypeDecider.IsApplicableTo(row));
		}
	}
}
