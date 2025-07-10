using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface IMatchTransactionDetails
	{
		ZGuid PK { get; set; }
		ZDate RealisationDate { get; set; }
		ZGuid BranchPK { get; }
		ZGuid DepartmenPK { get; }
		ZString Currency { get; }
		ZDecimal LocalAmount { get; }
		ZDecimal OSAmount { get; }
		ZGuid GLAccountPK { get; }
		IEnumerable<ZGuid> GetTaxRecordPKs();
	}

	public class MatchTransactionDetails : IMatchTransactionDetails
	{
		public MatchTransactionDetails(ZGuid branchPK, ZGuid departmentPK, ZString currency, ZDecimal localTaxAmount, ZDecimal osTaxAmount, ZGuid glAccountPK, ZGuid[] taxRecordPKs)
		{
			BranchPK = branchPK;
			DepartmenPK = departmentPK;
			Currency = currency;
			LocalAmount = localTaxAmount;
			OSAmount = osTaxAmount;
			GLAccountPK = glAccountPK;
			TaxRecordPKs = taxRecordPKs;
		}

		public ZGuid PK { get; set; }
		public ZDate RealisationDate { get; set; }
		public ZGuid BranchPK { get; }
		public ZGuid DepartmenPK { get; }
		public ZString Currency { get; }
		public ZDecimal LocalAmount { get; }
		public ZDecimal OSAmount { get; }
		public ZGuid GLAccountPK { get; }
		public IEnumerable<ZGuid> GetTaxRecordPKs()
		{
			return TaxRecordPKs;
		}

		readonly ZGuid[] TaxRecordPKs;
	}
}
