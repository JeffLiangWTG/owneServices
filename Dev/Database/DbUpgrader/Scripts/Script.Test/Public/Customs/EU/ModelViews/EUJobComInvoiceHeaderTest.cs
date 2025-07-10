using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.EU.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.EU.ModelViews.EUJobComInvoiceHeader))]
	class EUJobComInvoiceHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"EUJobComInvoiceHeader",
				"JobComInvoiceHeader",
				new []
				{
					new TestDbViewHelper.DbColumn("JZ_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JZ_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JZ_TransportChargesMethodOfPayment", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_RelatedIndicator2", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_RelatedIndicator3", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_RelatedIndicator4", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_AgreedPlaceCode", VarChar, 5),
					new TestDbViewHelper.DbColumn("JZ_CommercialPaymentCode", VarChar, 3),
					new TestDbViewHelper.DbColumn("JZ_CustomsAuthorisationReferenceForExportFallback", VarChar, 15),
					new TestDbViewHelper.DbColumn("JZ_HouseSplitReference", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_ValuationMethod", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_IncoTermDescription", VarChar, 100),
					new TestDbViewHelper.DbColumn("JZ_IncotermCountry", VarChar, 2),
				}
			);
		}
	}
}
