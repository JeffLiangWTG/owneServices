using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.TransitWarehouse;

[TestedType(typeof(ReplaceStmMenuDocumentConfigItemSectionItemName))]
public class ReplaceStmMenuDocumentConfigItemSectionItemNameTest : DataTransformationTestCase
{
	public override string[] expectedIndex => new string[]
	{
		"NONCLUSTERED INDEX [_WTG__Update StmMenuDocumentConfigItem.S4_SectionItemName's value from Packages (Start Of Package) to Pack_1] ON [dbo].[StmMenuDocumentConfigItem] ([S4_SectionItemName]) INCLUDE ([S4_SystemLastEditTimeUtc], [S4_SystemLastEditUser]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
	};

	protected override void PrepareTestData()
	{
		var sql = new StringBuilder();
		var stmMenuItem = new StmMenuItem("Cartage Advice","NON").InsertAndReturnObject(TestConnection);

		var stmTemplate = new StmTemplate(false, "None", "Enterprise\\Product\\Documents\\ExcelTemplates\\Reports\\AU BAS Summary Report.xls", true, true, true, true, "", "A8A", "", "DOC",  DateTime.UtcNow,"~BP").InsertAndReturnObject(TestConnection);

		var refDocType = new RefDocType("ABC", true, "Claim Log Test", "BTC", true, true, true, true, true, true, true, true, true, "", true, true, "PIN", true, "CAO").InsertAndReturnObject(TestConnection);

		var stmMenuTemplatePivot = new StmMenuTemplatePivot(stmMenuItem.PK, stmTemplate.PK, "Warehouse Order Copy", "", "", true, true, true, 0 , 0, refDocType.PK, true, "",true).InsertAndReturnObject(TestConnection);

		var stmMenuDocumentConfig = new StmMenuDocumentConfig(false, false, false , stmMenuTemplatePivot.PK, null, null, "", "POR", "", false, "Default", false).InsertAndReturnObject(TestConnection);
		stmMenuDocumentConfigItem1 = new StmMenuDocumentConfigItem(0,"",false, false, 0, stmMenuDocumentConfig.PK, "Packages (Start Of Package)", "BEX");
		stmMenuDocumentConfigItem1.AppendInsertAndReturnObject(sql);

		stmMenuDocumentConfigItem2 = new StmMenuDocumentConfigItem(0, "", false, true, 0, stmMenuDocumentConfig.PK, "Packages (Start Of Package)", "BEX");
		stmMenuDocumentConfigItem2.AppendInsertAndReturnObject(sql);

		stmMenuDocumentConfigItem3 = new StmMenuDocumentConfigItem(0, "", false, false, 0, stmMenuDocumentConfig.PK, "Custom Section", "BEX");
		stmMenuDocumentConfigItem3.AppendInsertAndReturnObject(sql);

		stmMenuDocumentConfigItem4 = new StmMenuDocumentConfigItem(0, "", false, true, 0, stmMenuDocumentConfig.PK, "Custom Section", "BEX");
		stmMenuDocumentConfigItem4.AppendInsertAndReturnObject(sql);

		TestConnection.ExecuteNonQuery(sql.ToString());
	}

	protected override void AssertTransformationResults()
	{
		StmMenuDocumentConfigItem.AssertFromDB(TestConnection, stmMenuDocumentConfigItem1.PK)
			.ExpectEquals("when S4_SectionItemName is Packages (Start Of Package) and isSystemDefined is 0, S4_SectionItemName's value should be replaced by Packages (Start of Package)", p => p.S4_SectionItemName, "Packages (Start of Package)")
			.VerifyAll();

		StmMenuDocumentConfigItem.AssertFromDB(TestConnection, stmMenuDocumentConfigItem2.PK)
			.ExpectEquals("when S4_SectionItemName is Packages (Start Of Package) and isSystemDefined is 1, S4_SectionItemName's value Packages (Start Of Package) should be replaced by Packages (Start of Package)", p => p.S4_SectionItemName, "Packages (Start of Package)")
			.VerifyAll();

		StmMenuDocumentConfigItem.AssertFromDB(TestConnection, stmMenuDocumentConfigItem3.PK)
			.ExpectEquals("when S4_SectionItemName is not Packages (Start Of Package) and isSystemDefined is 0, S4_SectionItemName's value should not be replaced", p => p.S4_SectionItemName, "Custom Section")
			.VerifyAll();

		StmMenuDocumentConfigItem.AssertFromDB(TestConnection, stmMenuDocumentConfigItem4.PK)
			.ExpectEquals("when S4_SectionItemName is not Packages (Start Of Package)and isSystemDefined is 1, S4_SectionItemName's value should not be replaced", p => p.S4_SectionItemName, "Custom Section")
			.VerifyAll();
	}

	StmMenuDocumentConfigItem stmMenuDocumentConfigItem1;
	StmMenuDocumentConfigItem stmMenuDocumentConfigItem2;
	StmMenuDocumentConfigItem stmMenuDocumentConfigItem3;
	StmMenuDocumentConfigItem stmMenuDocumentConfigItem4;

	protected override DataTransformation GetNewTestTransformationInstance() => new ReplaceStmMenuDocumentConfigItemSectionItemName();
}
