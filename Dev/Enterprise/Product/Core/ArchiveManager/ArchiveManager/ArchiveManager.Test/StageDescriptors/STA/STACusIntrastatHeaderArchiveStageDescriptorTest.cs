using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.STA
{
	class STACusIntrastatHeaderArchiveStageDescriptorTest : STAArchiveDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new STACusIntrastatHeaderArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Standalone Customs Intrastat Header Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> CusIntrastatHeaderSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> null;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = CusIntrastatHeaderSchema.CIH_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedParentIDColumn
			=> null;

		protected override List<ArchiveRelationshipForSTATest> ExpectedRelationships
			=> new()
			{
				new ArchiveRelationshipForSTATest(CusIntrastatHeaderSchema.Constants.TableName, CusIntrastatHeaderSchema.PK, CusIntrastatMergedLineSchema.Constants.TableName, CusIntrastatMergedLineSchema.CIM_CIH_Header, isReversed: false),
				new ArchiveRelationshipForSTATest(CusIntrastatHeaderSchema.Constants.TableName, CusIntrastatHeaderSchema.PK, CusIntrastatLineSchema.Constants.TableName, CusIntrastatLineSchema.CIL_CIH_Header, isReversed: false),
				new ArchiveRelationshipForSTATest(CusIntrastatMergedLineSchema.Constants.TableName, CusIntrastatMergedLineSchema.PK, CusIntrastatLineSchema.Constants.TableName, CusIntrastatLineSchema.CIL_CIM_MergedLine, isReversed: false)
			};

		[UseSnapshotProtection]
		public void TestRunArchiveSystem_WhenArchivingCusIntrastatHeader()
		{
			var group = (ICusIntrastatGroup)Factory.NewWithValidTestData(ObjectFactory.GetType<ICusIntrastatGroup>());
			group.CIG_Flow = "E";

			var header = (ICusIntrastatHeader)Factory.NewWithValidTestData(ObjectFactory.GetType<ICusIntrastatHeader>());
			header.CIH_CIG_MergedGroup = group.PK;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var mergedLine = Factory.New<ICusIntrastatMergedLine>();
			mergedLine.CIM_CIG_Group = group.PK;
			mergedLine.CIM_CIH_Header = header.PK;
			mergedLine.CIM_OH_Trader = orgHeader.PK;
			mergedLine.CIM_ClusterKey = 1;
			mergedLine.CIM_InvoiceValue = 1;
			mergedLine.CIM_MassInKilograms = 1;
			mergedLine.CIM_MemberState = "cd";
			mergedLine.CIM_Tariff = "sdrfesdfs";

			var line = Factory.New<ICusIntrastatLine>();
			line.CIL_CIH_Header = header.PK;
			line.CIL_CIM_MergedLine = mergedLine.PK;
			line.CIL_Tariff = "102939292939";

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Customs Intrastat Header to exist.", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusIntrastatHeader>(), new ZQuery(CusIntrastatHeaderSchema.PK, header.PK)));
				AssertEquals("Expected Customs Intrastat Merged Line to exist.", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusIntrastatMergedLine>(), new ZQuery(CusIntrastatMergedLineSchema.PK, mergedLine.PK)));
				AssertEquals("Expected Customs Intrastat Line to exist.", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusIntrastatLine>(), new ZQuery(CusIntrastatLineSchema.PK, line.PK)));
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.STA, config, logger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected Customs Intrastat Header to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusIntrastatHeader>(), new ZQuery(CusIntrastatHeaderSchema.PK, header.PK)));
				AssertEquals("Expected Customs Intrasta Merged Line to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusIntrastatMergedLine>(), new ZQuery(CusIntrastatMergedLineSchema.PK, mergedLine.PK)));
				AssertEquals("ExpectedCustoms Intrastat Line to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusIntrastatLine>(), new ZQuery(CusIntrastatLineSchema.PK, line.PK)));
			});

			Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}
	}
}
