using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(csfn_LandedCostSupplierTariffProductDetailComplexValues))]
	class csfn_LandedCostSupplierTariffProductDetailComplexValuesTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestCsfn_LandedCostSupplierTariffProductDetailComplexValues_GC_IsReciprocal()
		{
			var declarationPK = Guid.NewGuid();
			var invoicePK = Guid.NewGuid();
			var invoiceLinePK = Guid.NewGuid();
			var landedCostHeaderPK = Guid.NewGuid();
			var today = DateTime.Today;
			var je_DateOfArrivalFrom = today.AddDays(-30);
			var je_DateOfArrivalTo = today;
			var je_DateOfArrival = today.AddDays(-2);

			var query = $@"
					DECLARE @BranchPK UNIQUEIDENTIFIER, @CompanyPK UNIQUEIDENTIFIER, @CountryCode VARCHAR(2), @JE_OH_Importer UNIQUEIDENTIFIER;
					SELECT TOP 1 @JE_OH_Importer = GC_OH_OrgProxy, @CountryCode = GC_RN_NKCountryCode, @BranchPK = GB_PK, @CompanyPK = GB_GC FROM dbo.GlbBranch INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK;
					INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_GB, JE_GC, JE_MessageType, JE_DateOfArrival, JE_OH_Importer, JE_ClusterKey) VALUES (@DeclarationPK, 'AU', @BranchPK, @CompanyPK, 'IMP', @JE_DateOfArrival, @JE_OH_Importer, 1);
					INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_JE, JZ_RX_NKInvoice_Currency, JZ_ClusterKey) VALUES (@InvoicePK, 'AU', @DeclarationPK, 'AUD', 1);
					INSERT INTO dbo.JobComInvoiceLine (JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey) VALUES (@InvoiceLinePK, 'AU', @InvoicePK, 1);
					INSERT INTO dbo.LandedCostHeader (LT_PK, LT_ParentID, LT_GC, LT_ParentTableCode, LT_ClusterKey) VALUES (@LandedCostHeaderPK, @DeclarationPK, @CompanyPK, 'JE', 1)
					INSERT INTO dbo.LandedCostHistory (LH_PK, LH_ParentID, LH_ParentTableCode, LH_LT, LH_ClusterKey) VALUES (NEWID(), @InvoiceLinePK, 'JI', @LandedCostHeaderPK, 1);
					SELECT * from dbo.csfn_LandedCostSupplierTariffProductDetailComplexValues(@CompanyPK, @JE_OH_Importer, @JE_DateOfArrivalFrom, @JE_DateOfArrivalTo, @TaxDate);
					";

			using (var cmd = TestConnection.Command(query))
			{
				cmd.AddParameter("@DeclarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				cmd.AddParameter("@InvoicePK", SqlDbType.UniqueIdentifier, invoicePK);
				cmd.AddParameter("@InvoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				cmd.AddParameter("@LandedCostHeaderPK", SqlDbType.UniqueIdentifier, landedCostHeaderPK);
				cmd.AddParameter("@JE_DateOfArrivalFrom", SqlDbType.SmallDateTime, je_DateOfArrivalFrom);
				cmd.AddParameter("@JE_DateOfArrivalTo", SqlDbType.SmallDateTime, je_DateOfArrivalTo);
				cmd.AddParameter("@JE_DateOfArrival", SqlDbType.SmallDateTime, je_DateOfArrival);
				cmd.AddParameter("@TaxDate", SqlDbType.Date, today);
				cmd.ExecuteScalar();
			}
		}
	}
}

