using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class JobCriticalValidationTest : CriticalValidationTest<Job>
	{
		protected override List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases()
		{
			var result = new List<TestCaseDefinitionWithDelegate_Obsolete>();
			CreateTestCases(result);

			return result;
		}

		[TestDate(2019, 12, 19, 10, 11, 12)]
		public void TestCriticalValidationWithBrancheSetToNull()
		{
			var testDateTime = ZDateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var shipment = TestObjectCreator.CreateShipment("JobNumber 01");
			shipment.JS_HouseBill = "Test 0001";
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				job.JH_GB = ZGuid.Empty;

				var userErrorMsg = FormattableString.Invariant($@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Branch of the Job should not be null.");
				var exceptMsg1 = FormattableString.Invariant($@"Branch of the Job should not be null.

Argument 'Branch (PK: 00000000-0000-0000-0000-000000000000)' cannot be null.

{job.GetJobInfo()}
");
				var exceptMsg2 = FormattableString.Invariant($@"JH_GBChanged:
Branch: <NULL>
Branch Changed Time: {testDateTime}
JH_GB change StackTrace ->
   at Enterprise.MasterFiles.Business.JobHeaderDataCollectionExtensions.");
				var exceptMsg3 = FormattableString.Invariant($@"JH_GBChangedFromValidToEmpty:
Branch: <NULL>
Previous Branch: (Code: BNE)
Previous Branch Changed Time: {testDateTime}
JH_GB change from valid to empty StackTrace ->
   at Enterprise.MasterFiles.Business.JobHeaderDataCollectionExtensions.");
				var exceptMsg4 = FormattableString.Invariant($@"JH_GBChangedFromValidToEmptyForSavedJob: There is no data collected for this key.");
				var exceptMsg5 = FormattableString.Invariant($@"JobConstructorStackTrace:
Job Created Time: {testDateTime}
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)");

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(CriticalValidationMessageTemplate.BranchOfJobShouldNotBeNull, true, CriticalValidationErrorType.BranchOfJobShouldNotBeNull, userErrorMsg, exceptMsg1, exceptMsg2, exceptMsg3, exceptMsg4, exceptMsg5);
				AssertOnSavingCheck(job, testCase);
			}
		}

		[TestDate(2019, 12, 20, 10, 11, 12)]
		public void TestCriticalValidationWithDepartmentSetToNull()
		{
			var testDateTime = ZDateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var shipment = TestObjectCreator.CreateShipment("JobNumber 02");
			shipment.JS_HouseBill = "Test 0002";
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				job.JH_GE = ZGuid.Empty;

				var userErrorMsg = FormattableString.Invariant($@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Department of the Job should not be null.");
				var exceptMsg1 = FormattableString.Invariant($@"Department of the Job should not be null.

Argument 'Department (PK: 00000000-0000-0000-0000-000000000000)' cannot be null.

{job.GetJobInfo()}
");
				var exceptMsg2 = FormattableString.Invariant($@"
JH_GEChanged:
Department: <NULL>
Department Changed Time: {testDateTime}
JH_GE change StackTrace ->
   at Enterprise.MasterFiles.Business.JobHeaderDataCollectionExtensions.");
				var exceptMsg3 = FormattableString.Invariant($@"
JobConstructorStackTrace:
Job Created Time: {testDateTime}
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)");

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(CriticalValidationMessageTemplate.DepartmentOfJobShouldNotBeNull, true, CriticalValidationErrorType.DepartmentOfJobShouldNotBeNull, userErrorMsg, exceptMsg1, exceptMsg2, exceptMsg3);
				AssertOnSavingCheck(job, testCase);
			}
		}

		protected override ISupportCriticalValidation GetCriticalValidationParent()
		{
			var shipment = TestObjectCreator.CreateShipment("JobNumber 21");
			shipment.JS_HouseBill = "Test 0021";
			var jobLoader = new JobHeader.Loader(shipment);
			var job = (Job)jobLoader.TryCreateWithoutMutexForTestOnly();
			return job;
		}

		void CreateTestCases(List<TestCaseDefinitionWithDelegate_Obsolete> result)
		{
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete(
				"Job Critical Validation Test",
				delegate(BusinessObjectFactory factory)
				{
					var shipment = TestObjectCreator.CreateShipment("JobNumber 91");
					shipment.JS_HouseBill = "Test 0091";
					var jobLoader = new JobHeader.Loader(shipment);
					var job = (Job)jobLoader.TryCreateWithoutMutexForTestOnly();
					job.JH_GB = Env.CurrentBranchPK;
					job.JH_GE = Env.CurrentDepartmentPK;
					return job;
				},
				false,
				CriticalValidationErrorType.NoError,
				"Test should not failed"
				));
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;
	}
}
