using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class DSBJobCloseGJLHelper
	{
		public DSBJobCloseGJLHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public GLJournal CreateGLJournal(ZGuid batchPK)
		{
			var batch = Factory.Load<DsbJobCloseBatch>(batchPK);

			if (batch == null)
			{
				return null;
			}

			var sql = @"
SELECT AL_AG,
MIN(AC_AG_DisbursementSurplusAccount) SurplusAccount, MIN(AC_AG_DisbursementShortfallAccount) ShortfallAccount,
AL_GB, AL_GE, SUM(AL_LineAmount) SumAmount
FROM 
	dbo.DsbJobCloseBatch
	JOIN dbo.AccTransactionLines ON JBB_PK = AL_JBB
	JOIN dbo.AccChargeCode ON AC_PK = AL_AC
WHERE
	JBB_PK = @BatchPK
GROUP BY
	AL_AG, AL_GB, AL_GE";

			var collection = new DynamicBusinessObjectCollection(Factory);
			var param = ZSqlParameter.New("@BatchPK", batchPK, DsbJobCloseBatchSchema.PK);
			collection.Load(sql, new ZSqlParameter[1] { param });

			if (collection.Count <= 0)
			{
				return null;
			}

			var desc = CreateJournalDescription(batch.JBB_BatchNumber);
			var postDate = ZDateTime.Now;

			var journal = Factory.New<GLJournal>();
			journal.AH_Desc = desc;
			journal.AH_InvoiceDate = postDate;
			journal.AH_PostDate = postDate;

			ZDecimal total = 0m;
			collection.ForEach(x => total += new ZDecimal(x["SumAmount"]));

			foreach (DynamicBusinessObject row in collection)
			{
				var branchPK = new ZGuid(row["AL_GB"]).ToGuid();
				var departmentPK = new ZGuid(row["AL_GE"]).ToGuid();
				var accountPK = new ZGuid(row["AL_AG"]).ToGuid();
				var amount = new ZDecimal(row["SumAmount"]);
				var drOrCr = amount > 0 ? DebitCredit.DR : DebitCredit.CR;
				var balanceDrOrCr = drOrCr == DebitCredit.DR ? DebitCredit.CR : DebitCredit.DR;
				var balancePK = total > 0 ? new ZGuid(row["SurplusAccount"]) : new ZGuid(row["ShortfallAccount"]);

				var journalLine = journal.GLJournalLines.AddNew();
				journalLine.AL_AG = accountPK;
				journalLine.AL_Desc = desc;
				journalLine.AL_GB = branchPK;
				journalLine.AL_GE = departmentPK;
				journalLine.DebitCreditSign = drOrCr.ToString();
				journalLine.UnsignedOSLineAmount = amount;

				var journalBalanceLine = journal.GLJournalLines.AddNew();
				journalBalanceLine.AL_AG = balancePK;
				journalBalanceLine.AL_Desc = desc;
				journalBalanceLine.AL_GB = branchPK;
				journalBalanceLine.AL_GE = departmentPK;
				journalBalanceLine.DebitCreditSign = balanceDrOrCr.ToString();
				journalBalanceLine.UnsignedOSLineAmount = amount;
			}
			var securityOverrideProvider = new NonInteractiveGLJournalSecurityOverrideProvider(false);
			var provider = new NonInteractiveTransactionApprovalGUIProvider(Factory, securityOverrideProvider);
			new GLJournalLevelAuthorizationWithApprovalRequest(provider, journal, false).PerformLevelAuthorization();

			var aggregator = new AggregateWrapper(journal, journal);
			journal.Factory.SaveInTransactionActions.Add(aggregator);

			return journal;
		}

		string CreateJournalDescription(string batchNum)
		{
			var descFromRegistry = AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(
				AccountingConstants.DisbursementShortfallSurplusCode.DisbursementShortfallSurplus,
				AccountingConfigurationRegistry.Instance.GetDefaultDisbursementShortfallSurplusDesc());

			return Res.GetString("F55C27DB-F7C5-4f91-A21F-A04BA8B26904", "{0} - Bulk Closure [{1}]", descFromRegistry, batchNum);
		}

		BusinessObjectFactory Factory { get; set; }
	}
}
