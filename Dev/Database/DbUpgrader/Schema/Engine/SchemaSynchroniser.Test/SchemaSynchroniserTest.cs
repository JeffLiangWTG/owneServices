using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using Moq;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class SchemaSynchroniserTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		public void TestCreateAndValidateCheckConstraints()
		{
			RunActionOnMockMainDb(() =>
			{
				var calledCount = 0;

				var taskLoggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
				taskLoggerMock
					.Setup(x => x.StartTask("Synchronising Check Constraints"))
					.Callback(() => AssertEquals(0, calledCount++));
				taskLoggerMock
					.Setup(x => x.StartSubtask("Create check constraints (WITH CHECK)"))
					.Callback(() => AssertEquals(1, calledCount++));
				taskLoggerMock
					.Setup(x => x.StartSubtask("Ensure check constraints are enabled and trusted"))
					.Callback(() => AssertEquals(2, calledCount++));

				var schemaSynchroniser = new SchemaSynchroniser(TestConnection, taskLoggerMock.Object, mockMainDb, mockTemplateDb);

				// Act
				schemaSynchroniser.CreateAndValidateCheckConstraints();

				// Assert
				AssertEquals(3, calledCount);
			});
		}

		public void TestCreateAndValidateCheckConstraints_EnsureCheckConstraintsAreEnabled()
		{
			RunActionOnMockMainDb(() =>
			{
				var calledCount = 0;

				var taskLoggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
				taskLoggerMock
					.Setup(x => x.StartTask("Synchronising Check Constraints"))
					.Callback(() => AssertEquals(0, calledCount++));
				taskLoggerMock
					.Setup(x => x.StartSubtask("Create check constraints (WITH CHECK)"))
					.Callback(() => AssertEquals(1, calledCount++));
				taskLoggerMock
					.Setup(x => x.StartSubtask("Ensure check constraints are enabled and trusted"))
					.Callback(() => AssertEquals(2, calledCount++));

				var schemaSynchroniser = new SchemaSynchroniser(TestConnection, taskLoggerMock.Object, mockMainDb, mockTemplateDb);

				// Act
				schemaSynchroniser.CreateAndValidateCheckConstraints();

				// Assert
				AssertEquals(3, calledCount);
			});
		}

		public void TestSynchroniseDatabaseSchemaDropOldClassificationOnOldTable()
		{
			RunActionOnMockMainDb(() =>
			{
				var schemaSynchroniser = new SchemaSynchroniser(TestConnection, Mock.Of<IUpgradeTaskWorkflowLogger>(), mockMainDb, mockTemplateDb);

				// Act
				schemaSynchroniser.SynchroniseDatabaseSchema();

				// Assert
				AssertClassificationDoesNotExistInMockMainDb("TABLEA", "Col1");
			});
		}

		public void TestSynchroniseDatabaseSchemaModifiesOldClassificationOnTable()
		{
			RunActionOnMockMainDb(() =>
			{
				var schemaSynchroniser = new SchemaSynchroniser(TestConnection, Mock.Of<IUpgradeTaskWorkflowLogger>(), mockMainDb, mockTemplateDb);

				// Act
				schemaSynchroniser.SynchroniseDatabaseSchema();

				// Assert
				AssertClassificationExistInMockMainDb("TABLEC", "Col1", label: "After1");
				AssertClassificationExistInMockMainDb("TABLED", "Col1", labelId: "After2");
				AssertClassificationExistInMockMainDb("TABLEE", "Col1", information: "After3");
				AssertClassificationExistInMockMainDb("TABLEF", "Col1", informationId: "After4");
				AssertClassificationExistInMockMainDb("TABLEG", "Col1", rank: "NONE");
				AssertClassificationExistInMockMainDb("TABLEH", "Col1", rank: "NONE", label: "After1");
				AssertClassificationExistInMockMainDb("TABLEI", "Col1", labelId: "After2", information: "After3");
				AssertClassificationExistInMockMainDb("TABLEJ", "Col1", rank: "NONE", labelId: "After2", informationId: "After4");
				AssertClassificationExistInMockMainDb("TABLEK", "Col1", label: "After1", information: "After3", informationId: "After4");
				AssertClassificationExistInMockMainDb("TABLEL", "Col1", rank: "NONE", label: "After1", labelId: "After2", information: "After3", informationId: "After4");
			});
		}

		public void TestSynchroniseDatabaseSchemaAddNewClassificationOnNewTable()
		{
			RunActionOnMockMainDb(() =>
			{
				var schemaSynchroniser = new SchemaSynchroniser(TestConnection, Mock.Of<IUpgradeTaskWorkflowLogger>(), mockMainDb, mockTemplateDb);

				// Act
				schemaSynchroniser.SynchroniseDatabaseSchema();

				// Assert
				AssertClassificationExistInMockMainDb("TABLEB", "Col1", rank: "CRITICAL");
			});
		}

		#region Implementation

		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, createTestMainDbObjectsScript);
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, createTestTemplateDbObjectsScript);
		}

		#region Scripts

		const string createTestMainDbObjectsScript = @"
EXEC ('CREATE SCHEMA [TestSchema01]');
CREATE TABLE [TestSchema01].[TableA] (Col1 INT);

CREATE TABLE [TestSchema01].[TableC] (Col1 INT);
CREATE TABLE [TestSchema01].[TableD] (Col1 INT);
CREATE TABLE [TestSchema01].[TableE] (Col1 INT);
CREATE TABLE [TestSchema01].[TableF] (Col1 INT);
CREATE TABLE [TestSchema01].[TableG] (Col1 INT);
CREATE TABLE [TestSchema01].[TableH] (Col1 INT);
CREATE TABLE [TestSchema01].[TableI] (Col1 INT);
CREATE TABLE [TestSchema01].[TableJ] (Col1 INT);
CREATE TABLE [TestSchema01].[TableK] (Col1 INT);
CREATE TABLE [TestSchema01].[TableL] (Col1 INT);

ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEA].[Col1]
WITH (
	RANK = CRITICAL
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEC].[Col1]
WITH (
	LABEL = 'Before1'
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLED].[Col1]
WITH (
	LABEL_ID  = 'Before2'
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEE].[Col1]
WITH (
	INFORMATION_TYPE = 'Before3'
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEF].[Col1]
WITH (
	INFORMATION_TYPE_ID = 'Before4'
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEG].[Col1]
WITH (
	RANK = LOW
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEH].[Col1]
WITH (
	LABEL = 'Before1',
	RANK = LOW
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEI].[Col1]
WITH (
	LABEL_ID  = 'Before2',
	INFORMATION_TYPE = 'Before3'
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEJ].[Col1]
WITH (
	LABEL_ID  = 'Before2',
	INFORMATION_TYPE_ID = 'Before4',
	RANK = LOW
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEK].[Col1]
WITH (
	LABEL = 'Before1',
	INFORMATION_TYPE = 'Before3',
	INFORMATION_TYPE_ID = 'Before4'
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEL].[Col1]
WITH (
	LABEL = 'Before1',
	LABEL_ID  = 'Before2',
	INFORMATION_TYPE = 'Before3',
	INFORMATION_TYPE_ID = 'Before4',
	RANK = LOW
)
			";

		const string createTestTemplateDbObjectsScript = @"
EXEC ('CREATE SCHEMA [TestSchema01]');
CREATE TABLE [TestSchema01].[TABLEB] (Col1 INT);

CREATE TABLE [TestSchema01].[TableC] (Col1 INT);
CREATE TABLE [TestSchema01].[TableD] (Col1 INT);
CREATE TABLE [TestSchema01].[TableE] (Col1 INT);
CREATE TABLE [TestSchema01].[TableF] (Col1 INT);
CREATE TABLE [TestSchema01].[TableG] (Col1 INT);
CREATE TABLE [TestSchema01].[TableH] (Col1 INT);
CREATE TABLE [TestSchema01].[TableI] (Col1 INT);
CREATE TABLE [TestSchema01].[TableJ] (Col1 INT);
CREATE TABLE [TestSchema01].[TableK] (Col1 INT);
CREATE TABLE [TestSchema01].[TableL] (Col1 INT);

ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEB].[Col1]
WITH (
	RANK = CRITICAL
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEC].[Col1]
WITH (
	LABEL = 'After1'
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLED].[Col1]
WITH (
	LABEL_ID  = 'After2'
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEE].[Col1]
WITH (
	INFORMATION_TYPE = 'After3'
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEF].[Col1]
WITH (
	INFORMATION_TYPE_ID = 'After4'
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEG].[Col1]
WITH (
	RANK = NONE
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEH].[Col1]
WITH (
	LABEL = 'After1',
	RANK = NONE
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEI].[Col1]
WITH (
	LABEL_ID  = 'After2',
	INFORMATION_TYPE = 'After3'
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEJ].[Col1]
WITH (
	LABEL_ID  = 'After2',
	INFORMATION_TYPE_ID = 'After4',
	RANK = NONE
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEK].[Col1]
WITH (
	LABEL = 'After1',
	INFORMATION_TYPE = 'After3',
	INFORMATION_TYPE_ID = 'After4'
)
ADD SENSITIVITY CLASSIFICATION TO [TestSchema01].[TABLEL].[Col1]
WITH (
	LABEL  = 'After1',
	LABEL_ID  = 'After2',
	INFORMATION_TYPE = 'After3',
	INFORMATION_TYPE_ID = 'After4',
	RANK = NONE
)
			";

		#endregion

		#endregion
	}
}
