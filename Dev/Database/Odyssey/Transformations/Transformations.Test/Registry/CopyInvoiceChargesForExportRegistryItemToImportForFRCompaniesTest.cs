using System.Text;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	[TestedType(typeof(CopyInvoiceChargesForExportRegistryItemToImportForFRCompanies))]
	class CopyInvoiceChargesForExportRegistryItemToImportForFRCompaniesTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(5, Helper.GetStmDataRowCount("InvoiceChargesForExport"));
			AssertEquals(2, Helper.GetStmDataRowCount("InvoiceChargesForImport"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new CopyInvoiceChargesForExportRegistryItemToImportForFRCompanies();
		}

		protected override void PrepareTestData()
		{
			var helper = new TransformationTestDataCreator();

			var frenchCompany1PK = helper.CreateGlbCompany("DFR", "FR");
			var frenchCompany2PK = helper.CreateGlbCompany("DGP", "GP");
			var frenchCompany3PK = helper.CreateGlbCompany("DBL", "BL");
			var otherCompany1PK = helper.CreateGlbCompany("DDE", "DE");
			var otherCompany2PK = helper.CreateGlbCompany("DIT", "IT");

			Helper.InsertStmDataRow("InvoiceChargesForExport", frenchCompany1PK, "STR", Encoding.Unicode.GetBytes("VAL"));
			Helper.InsertStmDataRow("InvoiceChargesForExport", frenchCompany2PK, "STR", Encoding.Unicode.GetBytes("VOL"));
			Helper.InsertStmDataRow("InvoiceChargesForExport", frenchCompany3PK, "STR", Encoding.Unicode.GetBytes("VOL"));
			Helper.InsertStmDataRow("InvoiceChargesForExport", otherCompany1PK, "STR", Encoding.Unicode.GetBytes("VAL"));
			Helper.InsertStmDataRow("InvoiceChargesForExport", otherCompany2PK, "STR", Encoding.Unicode.GetBytes("VOL"));
		}
	}
}
