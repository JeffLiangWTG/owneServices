using System.Collections.Generic;
using System.Linq;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.EU.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.EU.ModelViews.EUCusEntryHeader))]
	sealed class EUCusEntryHeaderTest : BaseModelViewScriptTest
	{
		#region Overrides of ModelViewScriptTestBase

		protected override string ViewName => "EUCusEntryHeader";

		protected override string UnderlyingTableName => "CusEntryHeader";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => ExpectedColumns.ToArray();

		protected override bool HasIndexes => false;

		#endregion

		public static IEnumerable<TestDbViewHelper.DbColumn> ExpectedColumns =>
		[
			new TestDbViewHelper.DbColumn("CH_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CH_ClusterKey", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CH_AmendmentStatus", VarChar, 3),
			new TestDbViewHelper.DbColumn("CH_ATCPaymentProofNumber", VarChar, 13),
			new TestDbViewHelper.DbColumn("CH_Circuit", VarChar, 1),
			new TestDbViewHelper.DbColumn("CH_CircuitCan", VarChar, 2),
			new TestDbViewHelper.DbColumn("CH_ClearanceResult", VarChar, 2),
			new TestDbViewHelper.DbColumn("CH_CSVClearance", VarChar, 16),
			new TestDbViewHelper.DbColumn("CH_CSVExitCertificate", VarChar, 16),
			new TestDbViewHelper.DbColumn("CH_CSVImportCertificate", VarChar, 16),
			new TestDbViewHelper.DbColumn("CH_CSVT2L", VarChar, 16),
			new TestDbViewHelper.DbColumn("CH_DJPMRN", VarChar, 18),
			new TestDbViewHelper.DbColumn("CH_ExportMRN", VarChar, 18),
			new TestDbViewHelper.DbColumn("CH_NEStatus", VarChar, 3),
			new TestDbViewHelper.DbColumn("CH_PaymentProofNumber", VarChar, 13),
			new TestDbViewHelper.DbColumn("CH_RequestDispatch", VarChar, 1),
			new TestDbViewHelper.DbColumn("CH_AcceptanceDate", DateTime, -1),
			new TestDbViewHelper.DbColumn("CH_ATCLimitPaymentDate", DateTime, -1),
			new TestDbViewHelper.DbColumn("CH_LimitDateOfArrival", DateTime, -1),
			new TestDbViewHelper.DbColumn("CH_LimitPaymentDate", DateTime, -1),
			new TestDbViewHelper.DbColumn("CH_Parallel", Bit, -1),
			new TestDbViewHelper.DbColumn("CH_POUSVersion", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CH_SentEntryLinesCount", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CH_UCC6Version", Int, -1, 10, 0),
		];
	}
}
