using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business
{
	public class CusGuaranteeHeader : EU.Business.CusGuaranteeHeader, Integration.Customs.ES.ICusGuaranteeHeader
	{
		public CusGuaranteeHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public CusGuaranteeLineTransactionCollection ConfirmedCusGuaranteeLineTransactions
		{
			get
			{
				if (confirmedCusGuaranteeLineTransactions == null)
				{
					var confirmedCusGuaranteeLineTransactionsFilter = new ZQuery(CusPermitLineTransactionSchema.CPL_TransactionStatus, new[] { Customs.Business.PermitTransactionStatusList.Codes.Confirmed });
					confirmedCusGuaranteeLineTransactions = new CusGuaranteeLineTransactionCollection(this, confirmedCusGuaranteeLineTransactionsFilter);
				}
				return confirmedCusGuaranteeLineTransactions;
			}
		}
		CusGuaranteeLineTransactionCollection confirmedCusGuaranteeLineTransactions;

		public new CusGuaranteeLineTransactionCollection CusGuaranteeLineTransactions => (CusGuaranteeLineTransactionCollection)base.CusGuaranteeLineTransactions;

		protected override Customs.Business.CusGuaranteeLineTransactionCollection CreateNewCusGuaranteeLineTransactionCollection() => new CusGuaranteeLineTransactionCollection(this);

		public new GuaranteeCountrySpecificInstruction CountrySpecificInstruction => (GuaranteeCountrySpecificInstruction)base.CountrySpecificInstruction;

		public CusGuaranteeLineTransactionCollection GetPendingDebtTransactions()
		{
			var query = new ZDBOnlyQuery(typeof(CusGuaranteeLineTransaction));

			var additionalSql = FormattableString.Invariant($@"CPL_PK
																IN (SELECT CPL_PK
																FROM
																	(SELECT
																		CPL_PK,
																		TranValueTotal = SUM(CPL_TranValue) OVER (PARTITION BY CPL_Reference)
																	FROM
																		dbo.CusPermitLineTransaction
																	WHERE
																		CPL_TransactionStatus != @DELStatus
																		AND CPL_TransactionType IN (@ADJType, @TRAType, @CUSType)
																		AND CPL_CPH_PermitHeader = @PermitHeaderPK
																	) TotalTranValue
																WHERE
																	TranValueTotal < 0)");

			var sqlParameterCollection = new ZSqlParameterCollection(
												ZSqlParameter.New("@DELStatus", Customs.Business.PermitTransactionStatusList.Codes.Deleted, CusPermitLineTransactionSchema.CPL_TransactionStatus),
												ZSqlParameter.New("@ADJType", Customs.Business.PermitTransactionTypeList.Codes.ADJ, CusPermitLineTransactionSchema.CPL_TransactionType),
												ZSqlParameter.New("@TRAType", Customs.Business.PermitTransactionTypeList.Codes.TRA, CusPermitLineTransactionSchema.CPL_TransactionType),
												ZSqlParameter.New("@CUSType", Customs.Business.PermitTransactionTypeList.Codes.CUS, CusPermitLineTransactionSchema.CPL_TransactionType),
												ZSqlParameter.New("@PermitHeaderPK", PK, CusPermitLineTransactionSchema.CPL_CPH_PermitHeader));

			query.AddFilterAndZSQLParameterCollection(additionalSql, sqlParameterCollection);
			return new CusGuaranteeLineTransactionCollection(this, query);
		}

		public ZString DefaultPW_BondFiledPortForGuarantee => CusGuaranteeRules.FirstOrDefault(x => x.CPR_RuleCode == PermitRuleCodeList.Codes.CUS && !x.CPR_ValueFrom.IsEmpty)?.CPR_ValueFrom ?? ZString.Empty;
	}
}
