using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(RenameAdvancePaymentRelatedTransactionCategoryCodesNew))]
	internal class RenameAdvancePaymentRelatedTransactionCategoryCodesNewTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			ExtProperty.Database.Delete(Db.Connection, RenameAdvancePaymentRelatedTransactionCategoryCodesNew.LastProcessedDTPropertyStringNew);
			ExtProperty.Database.Delete(Db.Connection, RenameAdvancePaymentRelatedTransactionCategoryCodesNew.TransformationHasRunToCompletionNew);

			var helper = new TestDbHelper(TestConnection);
			var postDate = DateTime.Now;
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = TestDbHelper.BranchBrnPK;
			var departmentPK = TestDbHelper.DepartmentBrnPK;

			var org = helper.InsertOrgHeader("ZZZ", "ZZ Company");
			var job = helper.InsertJob("J00001", companyPK, branchPK, departmentPK, "JS", null, "WRK", DateTime.Now);
			var cah = helper.InsertCashAdvanceHeader("CAH001", "AP", companyPK, org, job, "AUD", 200M, 200M);

			helper.InsertTransactionHeader("AP", "JNL", "000001", 50m, postDate, branchPK, departmentPK, category: "ZZZ");
			helper.InsertTransactionHeader("AR", "JNL", "000002", 150m, postDate, branchPK, departmentPK, category: "RRR");

			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(Db.Connection, Db.SqlDbOwnerSchema, AccTransactionHeaderSchema.Constants.TableName, "Constraint_AH_CAH_CashAdvanceRequestHeader_NoCheck"))
			{
				var tranPk = helper.InsertTransactionHeader("AP", "JNL", "000003", 250m, postDate, branchPK, departmentPK, category: "CAI", cashAdvanceHeader: null);
				TestConnection.ExecuteNonQuery(FormattableString.Invariant($"UPDATE dbo.AccTransactionHeader SET AH_SystemCreateTimeUtc = '24 DEC 2023 00:00:00', AH_SystemCreateUser = '~BP', AH_SystemLastEditTimeUtc = '24 DEC 2023 00:00:00', AH_SystemLastEditUser = '~BP' WHERE AH_PK = '{tranPk}'"));

				var cahRelatedTransactions = new List<Guid>();
				for (int i = 0; i < 350; i++)
				{
					cahRelatedTransactions.Add(helper.InsertTransactionHeader("AP", "JNL", "CAP0" + i.ToString(), 50m, postDate, branchPK, departmentPK, category: "CAP", cashAdvanceHeader: cah));
					cahRelatedTransactions.Add(helper.InsertTransactionHeader("AR", "JNL", "CAR0" + i.ToString(), 150m, postDate, branchPK, departmentPK, category: "CAR", cashAdvanceHeader: cah));
					cahRelatedTransactions.Add(helper.InsertTransactionHeader("AP", "JNL", "CAI0" + i.ToString(), 250m, postDate, branchPK, departmentPK, category: "CAI", cashAdvanceHeader: null));
				}
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var transformationInstance = new RenameAdvancePaymentRelatedTransactionCategoryCodesNew();
			transformationInstance.Initialise(manager: new DummyUpgradeManager());
			return transformationInstance;
		}

		protected override void AssertTransformationResults()
		{
			var transactionCountWithOldCategoryCode = Db.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(AH_PK), 0) FROM dbo.AccTransactionHeader WHERE AH_TransactionCategory IN ('CAI', 'CAP', 'CAR') AND AH_SystemCreateTimeUtc > '31 JAN 2024 00:00:00'");
			var transactionCountWithNewCategoryCode = Db.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(AH_PK), 0) FROM dbo.AccTransactionHeader WHERE AH_TransactionCategory IN ('API', 'APP', 'APR') AND AH_SystemCreateTimeUtc > '31 JAN 2024 00:00:00'");
			var transactionCountThatAreNotRelatedToCah = Db.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(AH_PK), 0) FROM dbo.AccTransactionHeader WHERE AH_TransactionCategory NOT IN ('API', 'APP', 'APR') AND AH_SystemCreateTimeUtc > '31 JAN 2024 00:00:00'");
			var transactionCountWithOldCategoryCodeAndCreatedBefore31Jan24 = Db.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(AH_PK), 0) FROM dbo.AccTransactionHeader WHERE AH_TransactionCategory IN ('CAI', 'CAP', 'CAR') AND AH_SystemCreateTimeUtc < '31 JAN 2024 00:00:00'");

			CombineAssertions(() =>
			{
				AssertEquals("There should be NO transaction that were created after 31 Jan 24 with old advance payment related transaction category", 0, transactionCountWithOldCategoryCode);
				AssertEquals("All transactions that were created after 31 Jan 24 should have new advance payment related transaction category", 1050, transactionCountWithNewCategoryCode);
				AssertEquals("If transaction is not related to advance payment then Transaction Category should not change", 2, transactionCountThatAreNotRelatedToCah);
				AssertEquals("Any transaction that was created before 31 Jan 24 should not change.", 1, transactionCountWithOldCategoryCodeAndCreatedBefore31Jan24);
			});
		}

		public void TestTriggerIsCreatedCorrectly()
		{
			ExtProperty.Database.Delete(Db.Connection, RenameAdvancePaymentRelatedTransactionCategoryCodesNew.LastProcessedDTPropertyStringNew);
			ExtProperty.Database.Delete(Db.Connection, RenameAdvancePaymentRelatedTransactionCategoryCodesNew.TransformationHasRunToCompletionNew);

			var transform = GetNewTestTransformationInstance() as RenameAdvancePaymentRelatedTransactionCategoryCodesNew;
			var expectedTriggerDefinition = @"
CREATE TRIGGER dbo.TG_AccTransactionHeader_UpdateAdvancePaymentTransactionCategory
	ON dbo.AccTransactionHeader 
	AFTER INSERT
AS
SET NOCOUNT ON;
UPDATE AH
	SET
	AH.[CW!!CAH_Transaction_Flag] = 'Y',
	AH_SystemLastEditTimeUtc = GETUTCDATE(), 
	AH_SystemLastEditUser = COALESCE(i.AH_SystemLastEditUser, '~BP')
FROM inserted i
	INNER JOIN AccTransactionHeader AH ON AH.AH_PK = i.AH_PK
WHERE i.AH_TransactionType = 'JNL' 
	AND i.AH_Ledger IN ('AR', 'AP')
	AND i.AH_TransactionCategory IN ('CAI', 'CAR', 'CAP');";

			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, AccTransactionHeaderSchema.Constants.TableName, TriggerName));
			AssertEquals("Trigger Definition is correct", expectedTriggerDefinition, DbObjectCreator.GetTriggerDefinition(TestConnection, TriggerName));

			var helper = new TestDbHelper(TestConnection);
			var postDate = DateTime.Now;
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = TestDbHelper.BranchBrnPK;
			var departmentPK = TestDbHelper.DepartmentBrnPK;

			var org = helper.InsertOrgHeader("ZZZ", "ZZ Company");
			var job = helper.InsertJob("J00001", companyPK, branchPK, departmentPK, "JS", null, "WRK", DateTime.Now);
			var cah = helper.InsertCashAdvanceHeader("CAH001", "AP", companyPK, org, job, "AUD", 200M, 200M);

			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(Db.Connection, Db.SqlDbOwnerSchema, AccTransactionHeaderSchema.Constants.TableName, "Constraint_AH_CAH_CashAdvanceRequestHeader_NoCheck"))
			{
				helper.InsertTransactionHeader("AP", "JNL", "CAP01", 50m, postDate, branchPK, departmentPK, category: "CAP", cashAdvanceHeader: cah);
				helper.InsertTransactionHeader("AR", "JNL", "CAR02", 150m, postDate, branchPK, departmentPK, category: "CAR", cashAdvanceHeader: cah);
				helper.InsertTransactionHeader("AP", "JNL", "CAI03", 250m, postDate, branchPK, departmentPK, category: "CAI", cashAdvanceHeader: null);
			}

			var flaggedTransactionCountWithOldCategoryCode = Db.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(AH_PK), 0) FROM dbo.AccTransactionHeader WHERE AH_TransactionCategory IN ('CAI', 'CAP', 'CAR') AND [CW!!CAH_Transaction_Flag] = 'Y'");
			AssertEquals("There should be 3 transactions with old advance payment related transaction category", 3, flaggedTransactionCountWithOldCategoryCode);
		}

		public void TestTheScenarioWherePreviousVersionOfTheTransforMessedUp()
		{
			ExtProperty.Database.Delete(Db.Connection, RenameAdvancePaymentRelatedTransactionCategoryCodesNew.LastProcessedDTPropertyStringNew);
			ExtProperty.Database.Update(Db.Connection, "RenameAdvancePaymentRelatedTransactionCategoryCodes.TransformationRunToCompletion", bool.TrueString);

			CreatePreviousVersionOfTheTemmporaryTrigger();
			CreatePreviousVersionOfTheTemporaryConstraint();

			var transformmation = GetNewTestTransformationInstance();
			AssertNoExceptionThrown(() => transformmation.Run());
			Assert("Temporary constraint does not exist", !DbObjectCreator.ObjectExists(Db.Connection, "Constraint_AH_CAH_CashAdvanceRequestHeader_New"));
			Assert("Temporary Trigger does not exist", !DbObjectCreator.ObjectExists(Db.Connection, "TG_AccTransactionHeader_UpdateAdvancePaymentTransactionCategory"));

			void CreatePreviousVersionOfTheTemmporaryTrigger()
			{
				var sql = @"CREATE TRIGGER dbo.TG_AccTransactionHeader_UpdateAdvancePaymentTransactionCategory
	ON dbo.AccTransactionHeader 
	AFTER INSERT
AS
SET NOCOUNT ON;
UPDATE AH
	SET AH.AH_TransactionCategory = 
	CASE 
		WHEN i.AH_TransactionCategory='CAI' THEN 'API'
		WHEN i.AH_TransactionCategory='CAP' THEN 'APP'
		WHEN i.AH_TransactionCategory='CAR' THEN 'APR'
	END,
	AH_SystemLastEditTimeUtc = GETUTCDATE(), 
	AH_SystemLastEditUser = '~BP'
FROM inserted i
	INNER JOIN AccTransactionHeader AH ON AH.AH_PK = i.AH_PK
WHERE i.AH_TransactionType = 'JNL' 
	AND i.AH_Ledger IN ('AR', 'AP')
	AND i.AH_TransactionCategory IN ('CAI', 'CAR', 'CAP');";

				Db.Connection.ExecuteNonQuery(sql);
			}

			void CreatePreviousVersionOfTheTemporaryConstraint()
			{
				var sql = @"ALTER TABLE dbo.AccTransactionHeader  WITH NOCHECK 
ADD  CONSTRAINT Constraint_AH_CAH_CashAdvanceRequestHeader_New CHECK  (([AH_CAH_CashAdvanceRequestHeader] IS NULL OR [AH_TransactionType]='JNL' AND ([AH_TransactionCategory] IN ('APR', 'CAR') AND [AH_Ledger]='AR' OR [AH_TransactionCategory] IN ('APP', 'CAP') AND [AH_Ledger]='AP')))";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		const string TriggerName = "TG_AccTransactionHeader_UpdateAdvancePaymentTransactionCategory";
	}	
}
