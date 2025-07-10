using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccGeneralLedgerDataSubAccountFilter))]
	public class AccGeneralLedgerDataSubAccountFilterTest : SubAccountFilterTest
	{
		protected override SubAccountFilter GetNewModuleFilter()
		{
			return new AccGeneralLedgerDataSubAccountFilter("moo");
		}

		public void TestGetSubAccountQuery()
		{
			var querySqlText = "";
			var subAccountID = ZGuid.NewZGuid();

			AssertEquals(querySqlText, AccGeneralLedgerDataSubAccountFilter.GetSubAccountQuery("", ZGuid.Empty, true).LiteralTextSqlFormatted);

			querySqlText = @"(
	(
		GLD_AH_TransactionHeader is NULL 
		OR
		GLD_AH_TransactionHeader NOT IN 
		(
			SELECT AHS_AH FROM dbo.AccTransactionHeaderSubAccount WHERE AHS_SubClassParentTableCode = 'OH'
		)
	)
	AND
	(
		GLD_AL_TransactionLine is NULL 
		OR
		GLD_AL_TransactionLine NOT IN 
		(
			SELECT AL1_AL FROM dbo.AccTransactionLineSubAccount WHERE AL1_SubClassParentTableCode = 'OH'
		)
	)
)
AND
GLD_AG_GLAccount IN 
(
	SELECT ASA_AG FROM dbo.AccGLHeaderSubAccount WHERE ASA_SubClass = 'OH'
)
AND
(
	GLD_AH_TransactionHeader IN 
	(
		SELECT AH_PK FROM dbo.AccTransactionHeader WHERE AH_AG = GLD_AG_GLAccount
	)
	OR
	GLD_AL_TransactionLine IN 
	(
		SELECT AL_PK FROM dbo.AccTransactionLines WHERE AL_AG = GLD_AG_GLAccount
	)
)
";
			AssertEquals(querySqlText, AccGeneralLedgerDataSubAccountFilter.GetSubAccountQuery("ORG", ZGuid.Empty, true).LiteralTextSqlFormatted);

			querySqlText = $@"(
	(
		GLD_AH_TransactionHeader IN 
		(
			SELECT AHS_AH FROM dbo.AccTransactionHeaderSubAccount WHERE AHS_SubClassParentTableCode = 'OH' 
			AND
			AHS_SubClassParentId = '{subAccountID}'
		)
	)
	OR
	(
		GLD_AL_TransactionLine IN 
		(
			SELECT AL1_AL FROM dbo.AccTransactionLineSubAccount WHERE AL1_SubClassParentTableCode = 'OH' 
			AND
			AL1_SubClassParentId = '{subAccountID}'
		)
	)
)
AND
(
	GLD_AH_TransactionHeader IN 
	(
		SELECT AH_PK FROM dbo.AccTransactionHeader WHERE AH_AG = GLD_AG_GLAccount
	)
	OR
	GLD_AL_TransactionLine IN 
	(
		SELECT AL_PK FROM dbo.AccTransactionLines WHERE AL_AG = GLD_AG_GLAccount
	)
)
";
			AssertEquals(querySqlText, AccGeneralLedgerDataSubAccountFilter.GetSubAccountQuery("ORG", subAccountID).LiteralTextSqlFormatted);
		}
	}
}
