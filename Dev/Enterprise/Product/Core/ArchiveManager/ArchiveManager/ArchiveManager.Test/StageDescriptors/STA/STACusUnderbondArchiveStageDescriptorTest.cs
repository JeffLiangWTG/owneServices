using System.Collections.Generic;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.STA
{
	internal class STACusUnderbondArchiveStageDescriptorTest : STAArchiveDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new STACusUnderbondArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Standalone Customs Underbond Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> CusUnderbondSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> CusUnderbondSchema.C4_MovementReason;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = CusUnderbondSchema.C4_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedParentIDColumn
			=> CusUnderbondSchema.C4_ParentID;

		protected override List<ArchiveRelationshipForSTATest> ExpectedRelationships
			=> new()
			{
				new ArchiveRelationshipForSTATest(CusUnderbondSchema.Constants.TableName, CusUnderbondSchema.PK, CusOutturnSchema.Constants.TableName, CusOutturnSchema.C5_C4_Underbond, isReversed: false)
			};

		[UseSnapshotProtection]
		public void TestRunArchiveSystem_WhenArchivingCusUnderbond()
		{
			var underbond = Factory.New<CusUnderbond>();

			var outturn = Factory.New<CusOutturn>();
			outturn.C5_C4_Underbond = underbond.PK; 

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Customs Underbond to exist.", 1, Factory.GetDatabaseCount(typeof(CusUnderbond), new ZQuery(CusUnderbondSchema.PK, underbond.PK)));
				AssertEquals("Expected Customs Outturn to exist.", 1, Factory.GetDatabaseCount(typeof(CusOutturn), new ZQuery(CusOutturnSchema.PK, outturn.PK)));
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.STA, config, logger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected Customs Underbond to be deleted.", 0, Factory.GetDatabaseCount(typeof(CusUnderbond), new ZQuery(CusUnderbondSchema.PK, underbond.PK)));
				AssertEquals("Expected Customs Outturn to be deleted.", 0, Factory.GetDatabaseCount(typeof(CusOutturn), new ZQuery(CusOutturnSchema.PK, outturn.PK)));
			});

			Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}
	}
}
