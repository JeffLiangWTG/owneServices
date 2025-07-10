using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.STA
{
	class STAAsycudaManifestHeaderArchiveStageDescriptorTest : STAArchiveDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new STAAsycudaManifestHeaderArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Standalone Asycuda Manifest Header Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> AsycudaManifestHeaderSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> AsycudaManifestHeaderSchema.AMA_JobReference;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = AsycudaManifestHeaderSchema.AMA_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedParentIDColumn
			=> AsycudaManifestHeaderSchema.AMA_ParentId;

		public override void TestGetArchiveAction()
		{
			var logger = new TestArchiveLogger();
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			var mainArchiveItem = new ArchiveItem(dummyBizo.PKSchemaColumn, dummyBizo.PK.ToGuid(), null, Guid.Empty, false, dummyBizo.TablePrefix);
			var archiveSet = new TestArchiveSet(SystemDescriptor, "Dummy Stage Name", Guid.Empty, mainArchiveItem);

			var archiveActions = StageDescriptor.GetArchiveAction(logger, archiveSet);

			AssertEquals(1, archiveActions.Count());
			AssertNull(archiveActions.ToArray()[0]);
		}

		protected override List<ArchiveRelationshipForSTATest> ExpectedRelationships
			=> new()
			{
				new ArchiveRelationshipForSTATest(AsycudaManifestHeaderSchema.Constants.TableName, AsycudaManifestHeaderSchema.PK, AsycudaArrivalHeaderSchema.Constants.TableName, AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaManifestHeaderSchema.Constants.TableName, AsycudaManifestHeaderSchema.PK, AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.ABL_AMA, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaManifestHeaderSchema.Constants.TableName, AsycudaManifestHeaderSchema.PK, AsycudaContainerSchema.Constants.TableName, AsycudaContainerSchema.ACN_AMA_Manifest, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaArrivalHeaderSchema.Constants.TableName, AsycudaArrivalHeaderSchema.PK, AsycudaTransferHeaderSchema.Constants.TableName, AsycudaTransferHeaderSchema.ATF_ATH_ArrivalHeader, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaArrivalHeaderSchema.Constants.TableName, AsycudaArrivalHeaderSchema.PK, AsycudaArrivalLineSchema.Constants.TableName, AsycudaArrivalLineSchema.ATL_ATH, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.PK, AsycudaArrivalLineSchema.Constants.TableName, AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.PK, AsycudaBillScreeningSchema.Constants.TableName, AsycudaBillScreeningSchema.ASR_ABL, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.PK, AsycudaContainerBillOrPackageLinkSchema.Constants.TableName, AsycudaContainerBillOrPackageLinkSchema.APC_ABL_Bill, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.PK, AsycudaPackedItemSchema.Constants.TableName, AsycudaPackedItemSchema.API_ABL_Bill, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.PK, AsycudaPackSchema.Constants.TableName, AsycudaPackSchema.APA_ABL_Bill, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.PK, AsycudaTaxSchema.Constants.TableName, AsycudaTaxSchema.AET_ABL, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.PK, AsycudaTransferBillSchema.Constants.TableName, AsycudaTransferBillSchema.ATB_ABL_Bill, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaContainerSchema.Constants.TableName, AsycudaContainerSchema.PK, AsycudaContainerBillOrPackageLinkSchema.Constants.TableName, AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaPackedItemSchema.Constants.TableName, AsycudaPackedItemSchema.PK, AsycudaPackPackedItemPivotSchema.Constants.TableName, AsycudaPackPackedItemPivotSchema.APP_API_Item, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaPackedItemSchema.Constants.TableName, AsycudaPackedItemSchema.PK, AsycudaTaxSchema.Constants.TableName, AsycudaTaxSchema.AET_API_AsycudaPackedItem, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaPackSchema.Constants.TableName, AsycudaPackSchema.PK, AsycudaArrivalLineSchema.Constants.TableName, AsycudaArrivalLineSchema.ATL_APA_AsycudaPack, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaPackSchema.Constants.TableName, AsycudaPackSchema.PK, AsycudaContainerBillOrPackageLinkSchema.Constants.TableName, AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaPackSchema.Constants.TableName, AsycudaPackSchema.PK, AsycudaPackPackedItemPivotSchema.Constants.TableName, AsycudaPackPackedItemPivotSchema.APP_APA_Pack, isReversed: false),
				new ArchiveRelationshipForSTATest(AsycudaTransferHeaderSchema.Constants.TableName, AsycudaTransferHeaderSchema.PK, AsycudaTransferBillSchema.Constants.TableName, AsycudaTransferBillSchema.ATB_ATF_TransferHeader, isReversed: false)
			};

		[UseSnapshotProtection]
		public void TestRunArchiveSystem_WhenAsycudaManifestHeader_LinkedToAsycudaArrivalHeaderWithGrandchildren()
		{
			var asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var asycudaArrivalHeader = Factory.NewWithValidTestData<AsycudaArrivalHeader>();
			asycudaArrivalHeader.ATH_AMA_ManifestHeader = asycudaManifestHeader.PK;

			var asycudaTransferHeader = Factory.NewWithValidTestData<AsycudaTransferHeader>();
			asycudaTransferHeader.ATF_TransferType = "D";
			asycudaTransferHeader.ATF_ATH_ArrivalHeader = asycudaArrivalHeader.PK;

			var asycudaTransferBill = Factory.New<AsycudaTransferBill>();
			asycudaTransferBill.ATB_ATF_TransferHeader = asycudaTransferHeader.PK;
			asycudaTransferBill.ATB_BillOfLadingType = "STD";

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Asycuda Manifest Header to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaManifestHeader), new ZQuery(AsycudaManifestHeaderSchema.PK, asycudaManifestHeader.PK)));
				AssertEquals("Expected Asycuda Arrival Header to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaArrivalHeader), new ZQuery(AsycudaArrivalHeaderSchema.PK, asycudaArrivalHeader.PK)));
				AssertEquals("Expected Asycuda Transfer Header to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaTransferHeader), new ZQuery(AsycudaTransferHeaderSchema.PK, asycudaTransferHeader.PK)));
				AssertEquals("Expected Asycuda Transfer Bill to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaTransferBill), new ZQuery(AsycudaTransferBillSchema.PK, asycudaTransferBill.PK)));
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.STA, config, logger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected Asycuda Manifest Header to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaManifestHeader), new ZQuery(AsycudaManifestHeaderSchema.PK, asycudaManifestHeader.PK)));
				AssertEquals("Expected Asycuda Arrival Header to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaArrivalHeader), new ZQuery(AsycudaArrivalHeaderSchema.PK, asycudaArrivalHeader.PK)));
				AssertEquals("Expected Asycuda Transfer Header to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaTransferHeader), new ZQuery(AsycudaTransferHeaderSchema.PK, asycudaTransferHeader.PK)));
				AssertEquals("Expected Asycuda Transfer Bill to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaTransferBill), new ZQuery(AsycudaTransferBillSchema.PK, asycudaTransferBill.PK)));
			});

			Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}

		[UseSnapshotProtection]
		public void TestRunArchiveSystem_WhenAsycudaManifestHeader_LinkedToAsycudaBillWithChildren()
		{
			var asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var asycudaBill = Factory.NewWithValidTestData<AsycudaBill>();
			asycudaBill.ABL_AMA = asycudaManifestHeader.PK;

			var asycudaArrivalLine = Factory.NewWithValidTestData<AsycudaArrivalLine>();
			asycudaArrivalLine.ATL_ABL_AsycudaBill = asycudaBill.PK;

			var asycudaBillScreening = Factory.NewWithValidTestData<AsycudaBillScreening>();
			asycudaBillScreening.ASR_ABL = asycudaBill.PK;

			var asycudaContainerBillOrPackageLink = Factory.NewWithValidTestData<AsycudaContainerBillOrPackageLink>();
			asycudaContainerBillOrPackageLink.APC_ABL_Bill = asycudaBill.PK;

			var asycudaTax = Factory.NewWithValidTestData<AsycudaTax>();
			asycudaTax.AET_ABL = asycudaBill.PK;

			var arrivalHeader = Factory.New<AsycudaArrivalHeader>();
			arrivalHeader.ATH_AMA_ManifestHeader = asycudaManifestHeader.PK;

			var transferHeader = Factory.NewWithValidTestData<AsycudaTransferHeader>();
			transferHeader.ATF_ATH_ArrivalHeader = arrivalHeader.PK;
			transferHeader.ATF_TransferType = "D";

			var transferBill = Factory.New<AsycudaTransferBill>();
			transferBill.ATB_ATF_TransferHeader = transferHeader.PK;
			transferBill.ATB_BillOfLadingType = "STD";
			transferBill.ATB_ATF_TransferHeader = transferHeader.PK;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Asycuda Manifest Header to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaManifestHeader), new ZQuery(AsycudaManifestHeaderSchema.PK, asycudaManifestHeader.PK)));
				AssertEquals("Expected Asycuda Bill to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaBill), new ZQuery(AsycudaBillSchema.PK, asycudaBill.PK)));
				AssertEquals("Expected Asycuda Arrival Line to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaArrivalLine), new ZQuery(AsycudaArrivalLineSchema.PK, asycudaArrivalLine.PK)));
				AssertEquals("Expected Asycuda Bill Screening to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaBillScreening), new ZQuery(AsycudaBillScreeningSchema.PK, asycudaBillScreening.PK)));
				AssertEquals("Expected Asycuda Container Bill Or Package Link to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaContainerBillOrPackageLink), new ZQuery(AsycudaContainerBillOrPackageLinkSchema.PK, asycudaContainerBillOrPackageLink.PK)));
				AssertEquals("Expected Asycuda Tax to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaTax), new ZQuery(AsycudaTaxSchema.PK, asycudaTax.PK)));
				AssertEquals("Expected Asycuda Transfer Bill to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaTransferBill), new ZQuery(AsycudaTransferBillSchema.PK, transferBill.PK)));
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.STA, config, logger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected Asycuda Manifest Header to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaManifestHeader), new ZQuery(AsycudaManifestHeaderSchema.PK, asycudaManifestHeader.PK)));
				AssertEquals("Expected Asycuda Bill to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaBill), new ZQuery(AsycudaBillSchema.PK, asycudaBill.PK)));
				AssertEquals("Expected Asycuda Arrival Line to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaArrivalLine), new ZQuery(AsycudaArrivalLineSchema.PK, asycudaArrivalLine.PK)));
				AssertEquals("Expected Asycuda Bill Screening to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaBillScreening), new ZQuery(AsycudaBillScreeningSchema.PK, asycudaBillScreening.PK)));
				AssertEquals("Expected Asycuda Container Bill Or Package Link to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaContainerBillOrPackageLink), new ZQuery(AsycudaContainerBillOrPackageLinkSchema.PK, asycudaContainerBillOrPackageLink.PK)));
				AssertEquals("Expected Asycuda Tax to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaTax), new ZQuery(AsycudaTaxSchema.PK, asycudaTax.PK)));
				AssertEquals("Expected Asycuda Transfer Bill to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaTransferBill), new ZQuery(AsycudaTransferBillSchema.PK, transferBill.PK)));
			});

			Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}

		[UseSnapshotProtection]
		public void TestRunArchiveSystem_WhenAsycudaManifestHeader_LinkedToAsycudaBillWithGrandchildren_AsycudaPackedItem()
		{
			var asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var asycudaBill = Factory.NewWithValidTestData<AsycudaBill>();
			asycudaBill.ABL_AMA = asycudaManifestHeader.PK;

			var asycudaPackedItem = Factory.NewWithValidTestData<AsycudaPackedItem>();
			asycudaPackedItem.API_ABL_Bill = asycudaBill.PK;

			Factory.Save();

			var asycudaPackPackedItemPivot = Factory.NewWithValidTestData<AsycudaPackPackedItemPivot>();
			asycudaPackPackedItemPivot.APP_API_Item = asycudaPackedItem.PK;

			var asycudaTax = Factory.NewWithValidTestData<AsycudaTax>();
			asycudaTax.AET_API_AsycudaPackedItem = asycudaPackedItem.PK;
			asycudaTax.AET_ClusterKey = 1;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Asycuda Manifest Header to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaManifestHeader), new ZQuery(AsycudaManifestHeaderSchema.PK, asycudaManifestHeader.PK)));
				AssertEquals("Expected Asycuda Bill to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaBill), new ZQuery(AsycudaBillSchema.PK, asycudaBill.PK)));
				AssertEquals("Expected Asycuda Packed Item to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaPackedItem), new ZQuery(AsycudaPackedItemSchema.PK, asycudaPackedItem.PK)));
				AssertEquals("Expected Asycuda Pack Packed Item Pivot to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaPackPackedItemPivot), new ZQuery(AsycudaPackPackedItemPivotSchema.PK, asycudaPackPackedItemPivot.PK)));
				AssertEquals("Expected Asycuda Tax to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaTax), new ZQuery(AsycudaTaxSchema.PK, asycudaTax.PK)));
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.STA, config, logger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected Asycuda Manifest Header to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaManifestHeader), new ZQuery(AsycudaManifestHeaderSchema.PK, asycudaManifestHeader.PK)));
				AssertEquals("Expected Asycuda Bill to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaBill), new ZQuery(AsycudaBillSchema.PK, asycudaBill.PK)));
				AssertEquals("Expected Asycuda Packed Item to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaPackedItem), new ZQuery(AsycudaPackedItemSchema.PK, asycudaPackedItem.PK)));
				AssertEquals("Expected Asycuda Pack Packed Item Pivot to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaPackPackedItemPivot), new ZQuery(AsycudaPackPackedItemPivotSchema.PK, asycudaPackPackedItemPivot.PK)));
				AssertEquals("Expected Asycuda Tax to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaTax), new ZQuery(AsycudaTaxSchema.PK, asycudaTax.PK)));
			});

			Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}

		[UseSnapshotProtection]
		public void TestRunArchiveSystem_WhenAsycudaManifestHeader_LinkedToAsycudaBillWithGrandchildren_AsycudaPack()
		{
			var asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var asycudaBill = Factory.NewWithValidTestData<AsycudaBill>();
			asycudaBill.ABL_AMA = asycudaManifestHeader.PK;

			var asycudaPack = Factory.NewWithValidTestData<AsycudaPack>();
			asycudaPack.APA_ABL_Bill = asycudaBill.PK;

			var asycudaArrivalLine = Factory.NewWithValidTestData<AsycudaArrivalLine>();
			asycudaArrivalLine.ATL_APA_AsycudaPack = asycudaPack.PK;
			var asycudaContainerBillOrPackageLink = Factory.NewWithValidTestData<AsycudaContainerBillOrPackageLink>();
			asycudaContainerBillOrPackageLink.APC_APA_Pack = asycudaPack.PK;

			var asycudaPackedItem = Factory.New<AsycudaPackedItem>();
			asycudaPackedItem.API_ABL_Bill = asycudaBill.PK;
			Factory.Save();

			var pivot = Factory.New<AsycudaPackPackedItemPivot>();
			pivot.APP_API_Item = asycudaPackedItem.PK;
			pivot.APP_APA_Pack = asycudaPack.PK;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Asycuda Manifest Header to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaManifestHeader), new ZQuery(AsycudaManifestHeaderSchema.PK, asycudaManifestHeader.PK)));
				AssertEquals("Expected Asycuda Bill to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaBill), new ZQuery(AsycudaBillSchema.PK, asycudaBill.PK)));
				AssertEquals("Expected Asycuda Pack to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaPack), new ZQuery(AsycudaPackSchema.PK, asycudaPack.PK)));
				AssertEquals("Expected Asycuda Arrival Line to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaArrivalLine), new ZQuery(AsycudaArrivalLineSchema.PK, asycudaArrivalLine.PK)));
				AssertEquals("Expected Asycuda Container Bill Or Package Link to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaContainerBillOrPackageLink), new ZQuery(AsycudaContainerBillOrPackageLinkSchema.PK, asycudaContainerBillOrPackageLink.PK)));
				AssertEquals("Expected Asycuda Pack Packed Item Pivot to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaPackPackedItemPivot), new ZQuery(AsycudaPackPackedItemPivotSchema.PK, pivot.PK)));
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.STA, config, logger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected Asycuda Manifest Header to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaManifestHeader), new ZQuery(AsycudaManifestHeaderSchema.PK, asycudaManifestHeader.PK)));
				AssertEquals("Expected Asycuda Bill to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaBill), new ZQuery(AsycudaBillSchema.PK, asycudaBill.PK)));
				AssertEquals("Expected Asycuda Pack to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaPack), new ZQuery(AsycudaPackSchema.PK, asycudaPack.PK)));
				AssertEquals("Expected Asycuda Arrival Line to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaArrivalLine), new ZQuery(AsycudaArrivalLineSchema.PK, asycudaArrivalLine.PK)));
				AssertEquals("Expected Asycuda Container Bill Or Package Link to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaContainerBillOrPackageLink), new ZQuery(AsycudaContainerBillOrPackageLinkSchema.PK, asycudaContainerBillOrPackageLink.PK)));
				AssertEquals("Expected Asycuda Pack Packed Item Pivot to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaPackPackedItemPivot), new ZQuery(AsycudaPackPackedItemPivotSchema.PK, pivot.PK)));
			});

			Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}

		[UseSnapshotProtection]
		public void TestRunArchiveSystem_WhenAsycudaManifestHeader_LinkedToAsycudaContainerWithAsycudaContainerBillOrPackageLink()
		{
			var asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var asycudaContainer = Factory.NewWithValidTestData<AsycudaContainer>();
			asycudaContainer.ACN_AMA_Manifest = asycudaManifestHeader.PK;

			var asycudaBill = Factory.NewWithValidTestData<AsycudaBill>();
			asycudaBill.ABL_AMA = asycudaManifestHeader.PK;

			var asycudaContainerBillOrPackageLink = Factory.NewWithValidTestData<AsycudaContainerBillOrPackageLink>();
			asycudaContainerBillOrPackageLink.APC_ABL_Bill = asycudaBill.PK;
			asycudaContainerBillOrPackageLink.APC_ClusterKey = 1;
			asycudaContainerBillOrPackageLink.APC_ACN_Container = asycudaContainer.PK;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Asycuda Manifest Header to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaManifestHeader), new ZQuery(AsycudaManifestHeaderSchema.PK, asycudaManifestHeader.PK)));
				AssertEquals("Expected Asycuda Container to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaContainer), new ZQuery(AsycudaContainerSchema.PK, asycudaContainer.PK)));
				AssertEquals("Expected Asycuda Container Bill Or Package Link to exist.", 1, Factory.GetDatabaseCount(typeof(AsycudaContainerBillOrPackageLink), new ZQuery(AsycudaContainerBillOrPackageLinkSchema.PK, asycudaContainerBillOrPackageLink.PK)));
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.STA, config, logger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected Asycuda Manifest Header to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaManifestHeader), new ZQuery(AsycudaManifestHeaderSchema.PK, asycudaManifestHeader.PK)));
				AssertEquals("Expected Asycuda Container to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaContainer), new ZQuery(AsycudaContainerSchema.PK, asycudaContainer.PK)));
				AssertEquals("Expected Asycuda Container Bill Or Package Link to be deleted.", 0, Factory.GetDatabaseCount(typeof(AsycudaContainerBillOrPackageLink), new ZQuery(AsycudaContainerBillOrPackageLinkSchema.PK, asycudaContainerBillOrPackageLink.PK)));
			});

			Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}
	}
}
