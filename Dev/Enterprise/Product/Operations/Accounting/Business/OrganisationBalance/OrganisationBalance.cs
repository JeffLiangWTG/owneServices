using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.OrganisationBalance
{
	public class OrganisationBalance : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string TableName = "OrganisationBalance";
		}

		#endregion

		public OrganisationBalance(ZGuid orgPK, string ledgerType)
			: base(new BusinessObjectFactory())
		{
			OrganisationPK = orgPK;
			Ledger = ledgerType;
			fSum = -1;
		}

		public ZDecimal TotalOutstanding
		{
			get
			{
				if (!IsSumLoaded)
				{
					fSum = GetSum();
				}
				return fSum;
			}
		}

		public ZGuid OrgPK
		{
			set
			{
				OrganisationPK = value;
			}
		}

		ZDecimal GetSum()
		{
			string sql = @"SELECT SUM(" + AccTransactionHeaderSchema.Constants.AH_OutstandingAmount +
				") AS Sum FROM " + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName + " INNER JOIN " +
				GlbBranchSchema.Constants.SqlSchemaName + "." + GlbBranchSchema.Constants.TableName + " ON " + AccTransactionHeaderSchema.Constants.AH_GB + " = " +
				GlbBranchSchema.Constants.PK + " WHERE " +
				AccTransactionHeaderSchema.Constants.AH_IsCancelled + " = @Cancelled AND " +
				AccTransactionHeaderSchema.Constants.AH_OH + " = @OrgHeader AND " +
				AccTransactionHeaderSchema.Constants.AH_Ledger + " = @Ledger AND " +
				GlbBranchSchema.Constants.GB_GC + " = @Company AND " +
				AccTransactionHeaderSchema.Constants.AH_TransactionType + " != @TransactionType";

			DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add(ZSqlParameter.New("@OrgHeader", OrganisationPK, AccTransactionHeaderSchema.AH_OH));
			@params.Add(ZSqlParameter.New("@Ledger", Ledger, AccTransactionHeaderSchema.AH_Ledger));
			@params.Add(ZSqlParameter.New("@Cancelled", false, AccTransactionHeaderSchema.AH_IsCancelled));
			@params.Add(ZSqlParameter.New("@Company", GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC));
			@params.Add(ZSqlParameter.New("@TransactionType", TransactionTypes.InvoiceBatch, AccTransactionHeaderSchema.AH_TransactionType));

			dynBizOs.Load(sql, @params);

			ZDecimal sum = ((ZDecimal)dynBizOs[0]["Sum"]);

			return (Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable) ? sum : (ZDecimal)(sum * -1);
		}

		bool IsSumLoaded
		{
			get { return fSum != -1; }
		}

		ZGuid OrganisationPK;
		ZDecimal fSum;
		readonly string Ledger;
	}
}
