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
using static Enterprise.Integration.Customs.EUExitControl;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.STA
{
	class STACusExitHeaderArchiveStageDescriptorTest : STAArchiveDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new STACusExitHeaderArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Standalone Customs Exit Header Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> CusExitHeaderSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> CusExitHeaderSchema.CXH_JobReference;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = CusExitHeaderSchema.CXH_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedParentIDColumn
			=> CusExitHeaderSchema.CXH_ParentID;

		protected override List<ArchiveRelationshipForSTATest> ExpectedRelationships
			=> new()
			{
				new ArchiveRelationshipForSTATest(CusExitHeaderSchema.Constants.TableName, CusExitHeaderSchema.PK, CusExitConsignmentPackageSchema.Constants.TableName, CusExitConsignmentPackageSchema.CXP_CXH_Header, isReversed: false),
				new ArchiveRelationshipForSTATest(CusExitHeaderSchema.Constants.TableName, CusExitHeaderSchema.PK, CusExitConsignmentSchema.Constants.TableName, CusExitConsignmentSchema.CXC_CXH_Header, isReversed: false),
				new ArchiveRelationshipForSTATest(CusExitHeaderSchema.Constants.TableName, CusExitHeaderSchema.PK, CusExitContainerSchema.Constants.TableName, CusExitContainerSchema.CXN_CXH_Header, isReversed: false),
				new ArchiveRelationshipForSTATest(CusExitHeaderSchema.Constants.TableName, CusExitHeaderSchema.PK, CusExitReportSchema.Constants.TableName, CusExitReportSchema.CER_CXH_Header, isReversed: false),
				new ArchiveRelationshipForSTATest(CusExitConsignmentItemSchema.Constants.TableName, CusExitConsignmentItemSchema.PK, CusExitConsignmentPivotSchema.Constants.TableName, CusExitConsignmentPivotSchema.CNP_CCI_ConsignmentItem, isReversed: false),
				new ArchiveRelationshipForSTATest(CusExitConsignmentItemSchema.Constants.TableName, CusExitConsignmentItemSchema.PK, CusExitReportItemSchema.Constants.TableName, CusExitReportItemSchema.ERI_CCI_ConsignmentItem, isReversed: false),
				new ArchiveRelationshipForSTATest(CusExitConsignmentPackageSchema.Constants.TableName, CusExitConsignmentPackageSchema.PK, CusExitConsignmentPivotSchema.Constants.TableName, CusExitConsignmentPivotSchema.CNP_CXP_Package, isReversed: false),
				new ArchiveRelationshipForSTATest(CusExitConsignmentPackageSchema.Constants.TableName, CusExitConsignmentPackageSchema.PK, CusExitReportItemSchema.Constants.TableName, CusExitReportItemSchema.ERI_CXP_Package, isReversed: false),
				new ArchiveRelationshipForSTATest(CusExitConsignmentSchema.Constants.TableName, CusExitConsignmentSchema.PK, CusExitConsignmentItemSchema.Constants.TableName, CusExitConsignmentItemSchema.CCI_CXC_Consignment, isReversed: false),
				new ArchiveRelationshipForSTATest(CusExitConsignmentSchema.Constants.TableName, CusExitConsignmentSchema.PK, CusExitReportSchema.Constants.TableName, CusExitReportSchema.CER_CXC_Consignment, isReversed: false),
				new ArchiveRelationshipForSTATest(CusExitContainerSchema.Constants.TableName, CusExitContainerSchema.PK, CusExitConsignmentPivotSchema.Constants.TableName, CusExitConsignmentPivotSchema.CNP_CXN_Container, isReversed: false),
				new ArchiveRelationshipForSTATest(CusExitReportSchema.Constants.TableName, CusExitReportSchema.PK, CusExitReportItemSchema.Constants.TableName, CusExitReportItemSchema.ERI_CER_Report, isReversed: false),
				new ArchiveRelationshipForSTATest(CusExitReportSchema.Constants.TableName, CusExitReportSchema.PK, CusExitReportSchema.Constants.TableName, CusExitReportSchema.CER_CER_ExitReport, isReversed: false)
			};

		[UseSnapshotProtection]
		public void TestRunArchiveSystem_WhenCusExitHeader_LinkedToCusExitConsignmentPackage_WithChildren()
		{
			var header = (ICusExitHeader)Factory.NewWithValidTestData(ObjectFactory.GetType<ICusExitHeader>());

			var consignment = header.CusExitConsignments.AddNew();
			consignment.CXC_Status = "REJ";

			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 1;

			var package = (ICusExitConsignmentPackage)Factory.NewWithValidTestData(ObjectFactory.GetType<ICusExitConsignmentPackage>());
			package.CXP_CXH_Header = header.PK;
			package.CXP_Sequence = 1;

			var exitReport = header.CusExitReports.AddNew();
			exitReport.CER_CXC_Consignment = consignment.PK;

			var exitReportItem = exitReport.CusExitReportItems.AddNew();
			exitReportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			exitReportItem.ERI_CXP_Package = package.PK;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Customs Exit Header to exist.", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusExitHeader>(), new ZQuery(CusExitHeaderSchema.PK, header.PK)));
				AssertEquals("Expected Customs Exit Consignment to exist.", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusExitConsignment>(), new ZQuery(CusExitConsignmentSchema.PK, consignment.PK)));
				AssertEquals("Expected Customs Exit Consignment Item to exist.", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusExitConsignmentItem>(), new ZQuery(CusExitConsignmentItemSchema.PK, consignmentItem.PK)));
				AssertEquals("Expected Customs Exit Consignment Package to exist.", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusExitConsignmentPackage>(), new ZQuery(CusExitConsignmentPackageSchema.PK, package.PK)));
				AssertEquals("Expected Customs Exit Report to exist.", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusExitReport>(), new ZQuery(CusExitReportSchema.PK, exitReport.PK)));
				AssertEquals("Expected Customs Exit Report Item to exist.", 1, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusExitReportItem>(), new ZQuery(CusExitReportItemSchema.PK, exitReportItem.PK)));
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.STA, config, logger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected Customs Exit Header to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusExitHeader>(), new ZQuery(CusExitHeaderSchema.PK, header.PK)));
				AssertEquals("Expected Customs Exit Consignment to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusExitConsignment>(), new ZQuery(CusExitConsignmentSchema.PK, consignment.PK)));
				AssertEquals("Expected Customs Exit Consignment Item to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusExitConsignmentItem>(), new ZQuery(CusExitConsignmentItemSchema.PK, consignmentItem.PK)));
				AssertEquals("Expected Customs Exit Consignment Package to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusExitConsignmentPackage>(), new ZQuery(CusExitConsignmentPackageSchema.PK, package.PK)));
				AssertEquals("Expected Customs Exit Report to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusExitReport>(), new ZQuery(CusExitReportSchema.PK, exitReport.PK)));
				AssertEquals("Expected Customs Exit Report Item to be deleted.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<ICusExitReportItem>(), new ZQuery(CusExitReportItemSchema.PK, exitReportItem.PK)));
			});

			Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}
	}
}
