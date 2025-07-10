using System.Collections.Generic;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Schema;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine;
using CusTempStorageRegLineItem = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItem;
using CusTempStorageRegLineItemPivot = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot;
using CusTempStorageRegLineTransaction = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.STA
{
	class STACusTempStorageRegHeaderArchiveStageDescriptorTest : STAArchiveDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new STACusTempStorageRegHeaderArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Standalone Customs Temporary Storage Register Header Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> CusTempStorageRegHeaderSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> CusTempStorageRegHeaderSchema.SRH_Reference;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = CusTempStorageRegHeaderSchema.SRH_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedParentIDColumn
			=> null;

		protected override List<ArchiveRelationshipForSTATest> ExpectedRelationships
			=> new()
			{
				new ArchiveRelationshipForSTATest(CusTempStorageRegHeaderSchema.Constants.TableName, CusTempStorageRegHeaderSchema.PK, CusTempStorageRegLineSchema.Constants.TableName, CusTempStorageRegLineSchema.SRL_SRH, isReversed: false),
				new ArchiveRelationshipForSTATest(CusTempStorageRegLineSchema.Constants.TableName, CusTempStorageRegLineSchema.PK, CusTempStorageRegLineItemPivotSchema.Constants.TableName, CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, isReversed: false),
				new ArchiveRelationshipForSTATest(CusTempStorageRegLineItemSchema.Constants.TableName, CusTempStorageRegLineItemSchema.PK, CusTempStorageRegLineItemPivotSchema.Constants.TableName, CusTempStorageRegLineItemPivotSchema.SRV_SRI_Item, isReversed: false),
				new ArchiveRelationshipForSTATest(CusTempStorageRegLineSchema.Constants.TableName, CusTempStorageRegLineSchema.PK, CusTempStorageRegLineTransactionSchema.Constants.TableName, CusTempStorageRegLineTransactionSchema.SRT_SRL, isReversed: false),
				new ArchiveRelationshipForSTATest(CusTempStorageRegPremisesSchema.Constants.TableName, CusTempStorageRegPremisesSchema.PK, CusTempStorageRegHeaderSchema.Constants.TableName, CusTempStorageRegHeaderSchema.SRH_SRP_Premises, isReversed: false),
			};

		[UseSnapshotProtection]
		public void TestRunArchiveSystem_WhenArchivingCusTempStorageRegPremises()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_AppCode = "AAA";
			header.SRH_Reference = "reference";

			var line = header.CusTempStorageRegLines.AddNew();
			line.SRL_LineNumber = 4;
			line.SRL_CustomsStatus = "OPN";

			var lineItem = Factory.New<CusTempStorageRegLineItem>();
			lineItem.SRI_GoodsItemNumber = 1;

			var lineItemPivot = Factory.New<CusTempStorageRegLineItemPivot>();
			lineItemPivot.SRV_SRI_Item = lineItem.PK;
			lineItemPivot.SRV_SRL_Line = line.PK;

			var lineTransaction = Factory.New<CusTempStorageRegLineTransaction>();
			lineTransaction.SRT_SRL = line.PK;
			lineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			lineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Customs Temporary Storage Register Header to exist.", 1, Factory.GetDatabaseCount(typeof(CusTempStorageRegHeader), new ZQuery(CusTempStorageRegHeaderSchema.PK, header.PK)));
				AssertEquals("Expected Customs Temporary Storage Register Line to exist.", 1, Factory.GetDatabaseCount(typeof(CusTempStorageRegLine), new ZQuery(CusTempStorageRegLineSchema.PK, line.PK)));
				AssertEquals("Expected Customs Temporary Storage Register Line Item Pivot to exist.", 1, Factory.GetDatabaseCount(typeof(CusTempStorageRegLineItemPivot), new ZQuery(CusTempStorageRegLineItemPivotSchema.PK, lineItemPivot.PK)));
				AssertEquals("Expected Customs Temporary Storage Register Line Transaction to exist.", 1, Factory.GetDatabaseCount(typeof(CusTempStorageRegLineTransaction), new ZQuery(CusTempStorageRegLineTransactionSchema.PK, lineTransaction.PK)));
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.STA, config, logger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected Customs Temporary Storage Register Header to be deleted.", 0, Factory.GetDatabaseCount(typeof(CusTempStorageRegHeader), new ZQuery(CusTempStorageRegHeaderSchema.PK, header.PK)));
				AssertEquals("Expected Customs Temporary Storage Register Line to be deleted.", 0, Factory.GetDatabaseCount(typeof(CusTempStorageRegLine), new ZQuery(CusTempStorageRegLineSchema.PK, line.PK)));
				AssertEquals("Expected Customs Temporary Storage Register Line Item Pivot to be deleted.", 0, Factory.GetDatabaseCount(typeof(CusTempStorageRegLineItemPivot), new ZQuery(CusTempStorageRegLineItemPivotSchema.PK, lineItemPivot.PK)));
				AssertEquals("Expected Customs Temporary Storage Register Line Transaction to be deleted.", 0, Factory.GetDatabaseCount(typeof(CusTempStorageRegLineTransaction), new ZQuery(CusTempStorageRegLineTransactionSchema.PK, lineTransaction.PK)));
			});

			Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}
	}
}
