using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AccGeneralLedgerDataSubAccountFilter : SubAccountFilter
	{
		public AccGeneralLedgerDataSubAccountFilter(ZString description) : base(description)
		{
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var result = new ZQuery();
			result.AddToFilter(GetSubAccountQuery(SubAccountType, SubAccount));

			return result;
		}

		public static ZQuery GetSubAccountQuery(ZString subAccountType, ZGuid subAccountID, bool withoutValue = false)
		{
			var subAccountDBParentTableCode = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(subAccountType);

			var query = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));

			if (!subAccountType.IsEmpty && !subAccountID.IsEmpty)
			{
				var subAccountQueryForHeader = new ZDBOnlySubQuery(typeof(AccTransactionHeaderSubAccount), AccTransactionHeaderSubAccountSchema.AHS_AH);
				subAccountQueryForHeader.AddToFilter(AccTransactionHeaderSubAccountSchema.AHS_SubClassParentTableCode, subAccountDBParentTableCode);
				subAccountQueryForHeader.AddToFilter(AccTransactionHeaderSubAccountSchema.AHS_SubClassParentId, subAccountID);

				var subAccountQueryForLine = new ZDBOnlySubQuery(typeof(AccTransactionLineSubAccount), AccTransactionLineSubAccountSchema.AL1_AL);
				subAccountQueryForLine.AddToFilter(AccTransactionLineSubAccountSchema.AL1_SubClassParentTableCode, subAccountDBParentTableCode);
				subAccountQueryForLine.AddToFilter(AccTransactionLineSubAccountSchema.AL1_SubClassParentId, subAccountID);

				query.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, subAccountQueryForHeader, JoinCondition.Or);
				query.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, subAccountQueryForLine, JoinCondition.Or);
			}
			else if (!subAccountType.IsEmpty)
			{
				var subAccountQueryForGL = new ZDBOnlySubQuery(typeof(AccGLHeaderSubAccount), AccGLHeaderSubAccountSchema.ASA_AG);
				subAccountQueryForGL.AddToFilter(AccGLHeaderSubAccountSchema.ASA_SubClass, subAccountDBParentTableCode);

				if (withoutValue)
				{
					var subAccountQueryForHeader = new ZDBOnlySubQuery(typeof(AccTransactionHeaderSubAccount), AccTransactionHeaderSubAccountSchema.AHS_AH, true);
					subAccountQueryForHeader.AddToFilter(AccTransactionHeaderSubAccountSchema.AHS_SubClassParentTableCode, subAccountDBParentTableCode);

					var subAccountQueryForLine = new ZDBOnlySubQuery(typeof(AccTransactionLineSubAccount), AccTransactionLineSubAccountSchema.AL1_AL, true);
					subAccountQueryForLine.AddToFilter(AccTransactionLineSubAccountSchema.AL1_SubClassParentTableCode, subAccountDBParentTableCode);

					var queryHeader = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));
					queryHeader.AddToFilter(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, SQLComparisonOperator.Equal, null);
					queryHeader.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, subAccountQueryForHeader, JoinCondition.Or);

					var queryLine = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));
					queryLine.AddToFilter(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, SQLComparisonOperator.Equal, null);
					queryLine.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, subAccountQueryForLine, JoinCondition.Or);

					query.AddToFilter(queryHeader, JoinCondition.And);
					query.AddToFilter(queryLine, JoinCondition.And);
					query.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AG_GLAccount, subAccountQueryForGL, JoinCondition.And);
				}
				else
				{
					var subAccountQueryForHeader = new ZDBOnlySubQuery(typeof(AccTransactionHeaderSubAccount), AccTransactionHeaderSubAccountSchema.AHS_AH);
					subAccountQueryForHeader.AddToFilter(AccTransactionHeaderSubAccountSchema.AHS_SubClassParentTableCode, subAccountDBParentTableCode);

					var subAccountQueryForLine = new ZDBOnlySubQuery(typeof(AccTransactionLineSubAccount), AccTransactionLineSubAccountSchema.AL1_AL);
					subAccountQueryForLine.AddToFilter(AccTransactionLineSubAccountSchema.AL1_SubClassParentTableCode, subAccountDBParentTableCode);

					query.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, subAccountQueryForHeader, JoinCondition.Or);
					query.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, subAccountQueryForLine, JoinCondition.Or);
					query.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AG_GLAccount, subAccountQueryForGL, JoinCondition.Or);
				}
			}

			if (!subAccountType.IsEmpty || !subAccountID.IsEmpty)
			{
				var gLDAccountConsistencyQuery = GetGLDAccountConsistencyQuery();
				query.AddToFilter(gLDAccountConsistencyQuery, JoinCondition.And);
			}

			return query;
		}

		static ZQuery GetGLDAccountConsistencyQuery()
		{
			var query = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));

			var	subQueryForHeader = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			subQueryForHeader.AddToFilter(AccTransactionHeaderSchema.AH_AG, SQLComparisonOperator.Equal, AccGeneralLedgerDataSchema.GLD_AG_GLAccount);

			var subQueryForLine = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
			subQueryForLine.AddToFilter(AccTransactionLinesSchema.AL_AG, SQLComparisonOperator.Equal, AccGeneralLedgerDataSchema.GLD_AG_GLAccount);

			query.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, AccTransactionHeaderSchema.PK, subQueryForHeader, JoinCondition.Or);
			query.AddSubQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, AccTransactionLinesSchema.PK, subQueryForLine, JoinCondition.Or);

			return query;
		}
	}
}
