using System;
using System.IO;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.SystemMerge.GUI.Testing
{
	sealed class SysMergeProductXmlDataTransferDirectorTest : TestCaseWithFactory
	{
		public void TestAdapter()
		{
			var director = new SysMergeProductXmlDataTransferDirectorForTest();
			AssertEquals("Adapter", typeof(SysMergeProductValueObjectDataAdapter), director.Adapter_Exposed.GetType());
		}

		/// <summary>
		/// TestProduct.xml has 6 products (4 _A_ctive and 3 _I_nactive):
		///  - I - (0ff9dc3c-eda8-4a88-b60a-b35b2c466f17) - 1     - 1
		///  - A - (e44f61ad-681c-4007-a14f-de2690946c65) - 1014  - OPTIVE 2 (CATHETER)
		///  - A - (e305f231-5543-466d-92b7-7ee3cf84f166) - 1010  - BRAS
		///  - I - (ba661c26-32eb-4f54-a242-f9ab826090dc) - LB1   - LADIES HANDBAGS FOR TRAVEL
		///  - I - (240c4e22-f8a8-4623-9a05-ca11ab7f6c31) - BOOTS - BOOTS
		///  - A - (6f35adc1-27d3-4a29-b93c-4a69dfa5b210) - 4415  - PACKAGING
		/// </summary>
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportMultipleProductsSavesSuccessfulOnesEvenWhenOthersFail()
		{
			// --------------------
			// - CREATE TEST DATA -
			// --------------------
			var product1 = Factory.NewWithPrimaryKey<OrgSupplierPart>(new Guid("e305f231-5543-466d-92b7-7ee3cf84f166"));
			product1.OP_PartNum = "PartNum1";

			var product2 = Factory.NewWithPrimaryKey<OrgSupplierPart>(new Guid("240c4e22-f8a8-4623-9a05-ca11ab7f6c31"));
			product2.OP_PartNum = "PartNum2";

			var org = Factory.NewWithPrimaryKey<OrgHeader>(new Guid("3088d562-2ec6-4854-89c2-c2e1415d52df"));
			org.OH_Code = "3088d562-2ec";
			Factory.Save();

			// ------------------
			// - PRE-CONDITIONS -
			// ------------------

			// Products which do not exist before import
			var pk_1 = new ZGuid("0ff9dc3c-eda8-4a88-b60a-b35b2c466f17");
			var pk_1014 = new ZGuid("e44f61ad-681c-4007-a14f-de2690946c65");
			var pk_LB1 = new ZGuid("ba661c26-32eb-4f54-a242-f9ab826090dc");
			var pk_4415 = new ZGuid("6f35adc1-27d3-4a29-b93c-4a69dfa5b210");

			var product_1 = Factory.Load<OrgSupplierPart>(pk_1);
			var product_1014 = Factory.Load<OrgSupplierPart>(pk_1014);
			var product_LB1 = Factory.Load<OrgSupplierPart>(pk_LB1);
			var product_4415 = Factory.Load<OrgSupplierPart>(pk_4415);

			AssertNull("[PRE-CONDITION] Product 1 should not exist", product_1);
			AssertNull("[PRE-CONDITION] Product 1014 should not exist", product_1014);
			AssertNull("[PRE-CONDITION] Product LB1 should not exist", product_LB1);
			AssertNull("[PRE-CONDITION] Product 4415 should not exist", product_4415);

			// Products which (PK) exists before import
			int product_1010_Count = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "1010")).Length;
			int product_BOOTS_Count = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOOTS")).Length;
			AssertEquals("[PRE-CONDITION] Product 1010 count", 0, product_1010_Count);
			AssertEquals("[PRE-CONDITION] Product BOOTS count", 0, product_BOOTS_Count);

			// --------------
			// - RUN IMPORT -
			// --------------

			// Add Dummy Test Check Constraint on OP_PartNum to force import of product [1014] to fail
			((IDbConnected)Factory).Connection.ExecuteNonQuery("ALTER TABLE dbo.OrgSupplierPart ADD CONSTRAINT OrgSupplierPart_DummyCheck CHECK (OP_PartNum != '1014')");

			// Import XML File To Database
			var director = new SysMergeProductXmlDataTransferDirectorForTest();
			var fileName = Path.Combine(BaseSourcePath, "Enterprise", "Product", "Core", "DataTransfer", "DataTransfer.SystemMerge", "GUI", "Testing", "TestProducts.xml");
			director.Import(fileName, new NotificationBuffer(), SourceInfo.EmptySourceInfo);

			// -------------------
			// - LOAD AND ASSERT -
			// -------------------

			// Load products from Database using a new factory
			var loadFactory = new BusinessObjectFactory();
			product_1 = loadFactory.Load<OrgSupplierPart>(pk_1);
			product_1014 = loadFactory.Load<OrgSupplierPart>(pk_1014);
			product_LB1 = loadFactory.Load<OrgSupplierPart>(pk_LB1);
			product_4415 = loadFactory.Load<OrgSupplierPart>(pk_4415);

			// Product 1
			AssertNotNull("Product 1 should be imported", product_1);
			AssertEquals("Product 1 - PartNum", "1", product_1.OP_PartNum);
			AssertEquals("Product 1 - Desc", "1", product_1.OP_Desc);
			AssertEquals("Product 1 - IsActive", true, product_1.OP_IsActive);
			int product_1_PartRelationCount = loadFactory.Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product_1.PK)).Length;
			AssertEquals("Product 1 - OrgPartRelation count", 1, product_1_PartRelationCount);
			int product_1_PartBarcodeCount = loadFactory.Load<OrgSupplierPartBarcode>(new ZQuery(OrgSupplierPartBarcodeSchema.PH_OP, product_1.PK)).Length;
			AssertEquals("Product 1 - OrgSupplierPartBarcode count", 0, product_1_PartBarcodeCount);
			AssertEquals("Product 1 should be flagged as Active ignoring the <IsActive> value in xml", true, product_1.OP_IsActive);

			// Product 1014
			AssertNull("Product 1014 should not be imported due to OrgSupplierPart_DummyCheck", product_1014);

			// Product LB1
			AssertNotNull("Product LB1 should be imported", product_LB1);
			AssertEquals("Product LB1 - PartNum", "LB1", product_LB1.OP_PartNum);
			AssertEquals("Product LB1 - Desc", "LADIES HANDBAGS FOR TRAVEL", product_LB1.OP_Desc);
			AssertEquals("Product LB1 - IsActive", false, product_LB1.OP_IsActive);
			int product_LB1_PartRelationCount = loadFactory.Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product_LB1.PK)).Length;
			AssertEquals("Product LB1 - OrgPartRelation count", 2, product_LB1_PartRelationCount);
			int product_LB1_PartUnitCount = loadFactory.Load<OrgPartUnit>(new ZQuery(OrgPartUnitSchema.OF_OP, product_LB1.PK)).Length;
			AssertEquals("Product LB1 - OrgPartUnit count", 2, product_LB1_PartUnitCount);

			// Product 4415
			AssertNotNull("Product 4415 should be imported", product_4415);
			AssertEquals("Product 4415 - PartNum", "4415", product_4415.OP_PartNum);
			AssertEquals("Product 4415 - Desc", "PACKAGING", product_4415.OP_Desc);
			AssertEquals("Product 4415 - IsActive", true, product_4415.OP_IsActive);
			var product_4415_PartRelationCount = loadFactory.Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product_4415.PK)).Length;
			AssertEquals("Product 4415 - OrgPartRelation count", 3, product_4415_PartRelationCount);
			var product_4415_PartLocationCount = loadFactory.Load<OrgPartLocation>(new ZQuery(OrgPartLocationSchema.OR_OP, product_4415.PK)).Length;
			AssertEquals("Product 4415- OrgSupplierPartLocation count", 0, product_4415_PartLocationCount);

			// Products which (PK) exist before import
			product_1010_Count = loadFactory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "1010")).Length;
			product_BOOTS_Count = loadFactory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOOTS")).Length;
			AssertEquals("Product 1010 skiped - same PK exists", 0, product_1010_Count);
			AssertEquals("Product BOOTS skiped - same PK exists", 0, product_BOOTS_Count);
		}

		/// <summary>
		/// TestProduct.xml has 6 products (4 _A_ctive and 3 _I_nactive):
		///  - I - (0ff9dc3c-eda8-4a88-b60a-b35b2c466f17) - 1     - 1
		///  - A - (e44f61ad-681c-4007-a14f-de2690946c65) - 1014  - OPTIVE 2 (CATHETER)
		///  - A - (e305f231-5543-466d-92b7-7ee3cf84f166) - 1010  - BRAS
		///  - I - (ba661c26-32eb-4f54-a242-f9ab826090dc) - LB1   - LADIES HANDBAGS FOR TRAVEL
		///  - I - (240c4e22-f8a8-4623-9a05-ca11ab7f6c31) - BOOTS - BOOTS
		///  - A - (6f35adc1-27d3-4a29-b93c-4a69dfa5b210) - 4415  - PACKAGING
		/// </summary>
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestImportMultipleProductsSavesWhenTriggerFails()
		{
			Db.Connection.RollbackTransaction();

			// --------------------
			// - CREATE TEST DATA -
			// --------------------
			var product1 = Factory.NewWithPrimaryKey<OrgSupplierPart>(new Guid("e305f231-5543-466d-92b7-7ee3cf84f166"));
			product1.OP_PartNum = "PartNum1";

			var product2 = Factory.NewWithPrimaryKey<OrgSupplierPart>(new Guid("240c4e22-f8a8-4623-9a05-ca11ab7f6c31"));
			product2.OP_PartNum = "PartNum2";

			var org = Factory.NewWithPrimaryKey<OrgHeader>(new Guid("3088d562-2ec6-4854-89c2-c2e1415d52df"));
			org.OH_Code = "3088d562-2ec";
			Factory.Save();

			// ------------------
			// - PRE-CONDITIONS -
			// ------------------

			// Products which do not exist before import
			var pk_1 = new ZGuid("0ff9dc3c-eda8-4a88-b60a-b35b2c466f17");
			var pk_1014 = new ZGuid("e44f61ad-681c-4007-a14f-de2690946c65");
			var pk_LB1 = new ZGuid("ba661c26-32eb-4f54-a242-f9ab826090dc");
			var pk_4415 = new ZGuid("6f35adc1-27d3-4a29-b93c-4a69dfa5b210");

			var product_1 = Factory.Load<OrgSupplierPart>(pk_1);
			var product_1014 = Factory.Load<OrgSupplierPart>(pk_1014);
			var product_LB1 = Factory.Load<OrgSupplierPart>(pk_LB1);
			var product_4415 = Factory.Load<OrgSupplierPart>(pk_4415);

			AssertNull("[PRE-CONDITION] Product 1 should not exist", product_1);
			AssertNull("[PRE-CONDITION] Product 1014 should not exist", product_1014);
			AssertNull("[PRE-CONDITION] Product LB1 should not exist", product_LB1);
			AssertNull("[PRE-CONDITION] Product 4415 should not exist", product_4415);

			// Products which (PK) exists before import
			var product_1010_Count = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "1010")).Length;
			var product_BOOTS_Count = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOOTS")).Length;
			AssertEquals("[PRE-CONDITION] Product 1010 count", 0, product_1010_Count);
			AssertEquals("[PRE-CONDITION] Product BOOTS count", 0, product_BOOTS_Count);

			var triggerName = "TG_UNITTEST_BLOCKINSERT_OrgSupplierPart";
			try
			{
				// --------------
				// - RUN IMPORT -
				// --------------

				// Add Dummy Test Check Constraint on OP_PartNum to force import of product [1014] to fail
				var addTrigger = $@"
IF OBJECT_ID(N'{triggerName}') IS NOT NULL
	DROP TRIGGER {triggerName};

EXEC(N'CREATE TRIGGER {triggerName} 
ON [dbo].[OrgSupplierPart] FOR INSERT
AS
BEGIN
	IF EXISTS(SELECT * FROM inserted WHERE OP_PartNum = ''1014'')
	BEGIN
		RAISERROR(''Emulate fail insert.'', 16, 1)
		ROLLBACK TRANSACTION
	END
	RETURN
END')";
				((IDbConnected)Factory).Connection.ExecuteNonQuery(addTrigger);

				// Import XML File To Database
				var director = new SysMergeProductXmlDataTransferDirectorForTest();
				var fileName = Path.Combine(BaseSourcePath, "Enterprise", "Product", "Core", "DataTransfer", "DataTransfer.SystemMerge", "GUI", "Testing", "TestProducts.xml");
				director.Import(fileName, new NotificationBuffer(), SourceInfo.EmptySourceInfo);

				// -------------------
				// - LOAD AND ASSERT -
				// -------------------

				// Load products from Database using a new factory
				var loadFactory = new BusinessObjectFactory();
				product_1 = loadFactory.Load<OrgSupplierPart>(pk_1);
				product_1014 = loadFactory.Load<OrgSupplierPart>(pk_1014);
				product_LB1 = loadFactory.Load<OrgSupplierPart>(pk_LB1);
				product_4415 = loadFactory.Load<OrgSupplierPart>(pk_4415);

				// Product 1
				AssertNotNull("Product 1 should be imported", product_1);
				AssertEquals("Product 1 - PartNum", "1", product_1.OP_PartNum);
				AssertEquals("Product 1 - Desc", "1", product_1.OP_Desc);
				AssertEquals("Product 1 - IsActive", true, product_1.OP_IsActive);
				var product_1_PartRelationCount = loadFactory.Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product_1.PK)).Length;
				AssertEquals("Product 1 - OrgPartRelation count", 1, product_1_PartRelationCount);
				var product_1_PartBarcodeCount = loadFactory.Load<OrgSupplierPartBarcode>(new ZQuery(OrgSupplierPartBarcodeSchema.PH_OP, product_1.PK)).Length;
				AssertEquals("Product 1 - OrgSupplierPartBarcode count", 0, product_1_PartBarcodeCount);
				AssertEquals("Product 1 should be flagged as Active ignoring the <IsActive> value in xml", true, product_1.OP_IsActive);

				// Product 1014
				AssertNull("Product 1014 should not be imported due to OrgSupplierPart_DummyCheck", product_1014);

				// Product LB1
				AssertNotNull("Product LB1 should be imported", product_LB1);
				AssertEquals("Product LB1 - PartNum", "LB1", product_LB1.OP_PartNum);
				AssertEquals("Product LB1 - Desc", "LADIES HANDBAGS FOR TRAVEL", product_LB1.OP_Desc);
				AssertEquals("Product LB1 - IsActive", false, product_LB1.OP_IsActive);
				var product_LB1_PartRelationCount = loadFactory.Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product_LB1.PK)).Length;
				AssertEquals("Product LB1 - OrgPartRelation count", 2, product_LB1_PartRelationCount);
				var product_LB1_PartUnitCount = loadFactory.Load<OrgPartUnit>(new ZQuery(OrgPartUnitSchema.OF_OP, product_LB1.PK)).Length;
				AssertEquals("Product LB1 - OrgPartUnit count", 2, product_LB1_PartUnitCount);

				// Product 4415
				AssertNotNull("Product 4415 should be imported", product_4415);
				AssertEquals("Product 4415 - PartNum", "4415", product_4415.OP_PartNum);
				AssertEquals("Product 4415 - Desc", "PACKAGING", product_4415.OP_Desc);
				AssertEquals("Product 4415 - IsActive", true, product_4415.OP_IsActive);
				var product_4415_PartRelationCount = loadFactory.Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product_4415.PK)).Length;
				AssertEquals("Product 4415 - OrgPartRelation count", 3, product_4415_PartRelationCount);
				var product_4415_PartLocationCount = loadFactory.Load<OrgPartLocation>(new ZQuery(OrgPartLocationSchema.OR_OP, product_4415.PK)).Length;
				AssertEquals("Product 4415- OrgSupplierPartLocation count", 0, product_4415_PartLocationCount);

				// Products which (PK) exist before import
				product_1010_Count = loadFactory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "1010")).Length;
				product_BOOTS_Count = loadFactory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOOTS")).Length;
				AssertEquals("Product 1010 skiped - same PK exists", 0, product_1010_Count);
				AssertEquals("Product BOOTS skiped - same PK exists", 0, product_BOOTS_Count);
			}
			finally
			{
				var dropTrigger = $@"IF OBJECT_ID(N'{triggerName}') IS NOT NULL DROP TRIGGER {triggerName};";
				((IDbConnected)Factory).Connection.ExecuteNonQuery(dropTrigger);
				Db.Connection.BeginTransaction();
			}
		}

		sealed class SysMergeProductXmlDataTransferDirectorForTest : SysMergeProductXmlDataTransferDirector
		{
			internal IValueObjectDataAdapter Adapter_Exposed => Adapter;
		}
	}
}
