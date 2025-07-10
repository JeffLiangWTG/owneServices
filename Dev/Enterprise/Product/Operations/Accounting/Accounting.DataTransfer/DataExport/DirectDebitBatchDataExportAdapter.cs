using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public static class DirectDebitBatchDataExportAdapter
	{
		public static TransactionMatchLink[] GetAllMatchlinksExcludingEXX(Payment payment, AccTransactionMatchLink paymentMatchLink)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionMatchLink));
			if (paymentMatchLink != null)
			{
				ZString whereClause = AccTransactionMatchLinkSchema.PK.Name + " IN (Select " + AccTransactionMatchLinkSchema.PK.Name +
					" from " + AccTransactionMatchLinkSchema.Constants.SqlSchemaName + "." + AccTransactionMatchLinkSchema.Constants.TableName + " join " + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName +
					@" on ap_ah = ah_pk join
                                    dbo.glbbranch on ah_gb = gb_pk
                                    where ap_matchgroupnum = @MatchGroupNum AND
                                    GB_GC = @CurrentCompany AND
                                    AP_PK != @PaymentMatchLinkPK AND
                                    AH_TransactionType != 'EXX')";
				ZSqlParameterCollection @params = new ZSqlParameterCollection();
				@params.Add("@MatchGroupNum", paymentMatchLink.AP_MatchGroupNum, AccTransactionMatchLinkSchema.AP_MatchGroupNum);
				@params.Add("@CurrentCompany", GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC);
				@params.Add("@PaymentMatchLinkPK", paymentMatchLink.PK, AccTransactionMatchLinkSchema.PK);

				query.AddFilterAndZSQLParameterCollection(whereClause, @params);
			}
			else
			{
				query.IsNoResultQuery = true;
			}
			TransactionMatchLink[] paymentMatchlinks = payment.Factory.Load<TransactionMatchLink>(query);
			return paymentMatchlinks;
		}
	}
}
