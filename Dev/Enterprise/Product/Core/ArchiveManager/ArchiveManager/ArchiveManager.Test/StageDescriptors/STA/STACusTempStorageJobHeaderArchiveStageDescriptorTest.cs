using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.STA
{
	class STACusTempStorageJobHeaderArchiveStageDescriptorTest : STAArchiveDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new STACusTempStorageJobHeaderArchiveStageDescriptor();

		protected override string ExpectedName =>
			"Standalone Customs Temporary Storage Job Header Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> CusTempStorageJobHeaderSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> CusTempStorageJobHeaderSchema.SJH_JobReference;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = CusTempStorageJobHeaderSchema.SJH_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedParentIDColumn
			=> null;

		protected override List<ArchiveRelationshipForSTATest> ExpectedRelationships
			=> new()
			{
				new ArchiveRelationshipForSTATest(CusTempStorageJobHeaderSchema.Constants.TableName, CusTempStorageJobHeaderSchema.PK, CusTempStorageDecSchema.Constants.TableName, CusTempStorageDecSchema.STH_SJH, isReversed: false),
				new ArchiveRelationshipForSTATest(CusTempStorageDecSchema.Constants.TableName, CusTempStorageDecSchema.PK, CusTempStorageLineSchema.Constants.TableName, CusTempStorageLineSchema.TSL_STH, isReversed: false),
				new ArchiveRelationshipForSTATest(CusTempStorageLineSchema.Constants.TableName, CusTempStorageLineSchema.PK, CusTempStorageLineItemSchema.Constants.TableName, CusTempStorageLineItemSchema.TSI_TSL, isReversed: false),
				new ArchiveRelationshipForSTATest(CusTempStorageLineSchema.Constants.TableName, CusTempStorageLineSchema.PK, CusTempStorageLinePivotSchema.Constants.TableName, CusTempStorageLinePivotSchema.SLR_TSL_FromLine, isReversed: false),
				new ArchiveRelationshipForSTATest(CusTempStorageLineSchema.Constants.TableName, CusTempStorageLineSchema.PK, CusTempStorageLinePivotSchema.Constants.TableName, CusTempStorageLinePivotSchema.SLR_TSL_ToLine, isReversed: false)
			};

		[UseSnapshotProtection]
		public void TestRunArchiveSystem_WhenArchivingCusTempStorageJobHeader()
		{
			var header = Factory.NewWithValidTestData(ObjectFactory.GetType<ICusTempStorageJobHeader>());

			var storageDec = (ICusTempStorageDec)Factory.NewWithValidTestData(ObjectFactory.GetType<ICusTempStorageDec>());
			storageDec.STH_SJH = header.PK;

			var line = (ICusTempStorageLine)Factory.NewWithValidTestData(ObjectFactory.GetType<ICusTempStorageLine>());
			line.TSL_STH = storageDec.PK;

			var line2 = (ICusTempStorageLine)Factory.NewWithValidTestData(ObjectFactory.GetType<ICusTempStorageLine>());
			line2.TSL_STH = storageDec.PK;

			var lineItem = (ICusTempStorageLineItem)Factory.NewWithValidTestData(ObjectFactory.GetType<ICusTempStorageLineItem>());
			lineItem.TSI_TSL = line.PK;

			var pivot = (ICusTempStorageLinePivot)Factory.NewWithValidTestData(ObjectFactory.GetType<ICusTempStorageLinePivot>());
			pivot.SLR_TSL_FromLine = line.PK;
			pivot.SLR_TSL_ToLine = line2.PK;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Customs Temporary Storage Job Header to exist.", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusTempStorageJobHeader>(), new ZQuery(CusTempStorageJobHeaderSchema.PK, header.PK)));
				AssertEquals("Expected Customs Temporary Storage Dec Header to exist.", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusTempStorageDec>(), new ZQuery(CusTempStorageDecSchema.PK, storageDec.PK)));
				AssertEquals("Expected Customs Temporary Storage Line to exist.", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusTempStorageLine>(), new ZQuery(CusTempStorageLineSchema.PK, line.PK)));
				AssertEquals("Expected Customs Temporary Storage Line2 to exist.", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusTempStorageLine>(), new ZQuery(CusTempStorageLineSchema.PK, line2.PK)));
				AssertEquals("Expected Customs Temporary Storage Line Item to exist", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusTempStorageLineItem>(), new ZQuery(CusTempStorageLineItemSchema.PK, lineItem.PK)));
				AssertEquals("Expected Customs Temporary Storage Line Pivot to exist", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusTempStorageLinePivot>(), new ZQuery(CusTempStorageLinePivotSchema.PK, pivot.PK)));
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.STA, config, logger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected Customs Temporary Storage Job Header to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusTempStorageJobHeader>(), new ZQuery(CusTempStorageJobHeaderSchema.PK, header.PK)));
				AssertEquals("Expected Customs Temporary Storage Dec to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusTempStorageDec>(), new ZQuery(CusTempStorageDecSchema.PK, storageDec.PK)));
				AssertEquals("Expected Customs Temporary Storage Line to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusTempStorageLine>(), new ZQuery(CusTempStorageLineSchema.PK, line.PK)));
				AssertEquals("Expected Customs Temporary Storage Line2 to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusTempStorageLine>(), new ZQuery(CusTempStorageLineSchema.PK, line2.PK)));
				AssertEquals("Expected Customs Temporary Storage Line Item to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusTempStorageLineItem>(), new ZQuery(CusTempStorageLineItemSchema.PK, lineItem.PK)));
				AssertEquals("Expected  Customs Temporary Storage Line Pivot to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusTempStorageLinePivot>(), new ZQuery(CusTempStorageLinePivotSchema.PK, pivot.PK)));
			});

			Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}
	}
}
