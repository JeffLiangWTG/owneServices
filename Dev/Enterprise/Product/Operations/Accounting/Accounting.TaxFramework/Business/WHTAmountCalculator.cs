using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface IWHTAmountCalculator
	{
		IEnumerable<IWithheldTaxAmounts> Calculate(params ZGuid[] transactionPKs);
	}

	public class WHTAmountCalculator : IWHTAmountCalculator
	{
		public IEnumerable<IWithheldTaxAmounts> Calculate(params ZGuid[] transactionPKs)
		{
			var whtAmountDictionary = new Dictionary<ZGuid, WithheldTaxAmounts>();
			transactionPKs.Select(pk => new WithheldTaxAmounts() { TransactionPK = pk })
						.ForEach(a => whtAmountDictionary.Add(a.TransactionPK, a));

			var sql = @"SELECT	ATT_AH,
								SUM(CASE WHEN ATT_RealisationDate IS NOT NULL THEN ATT_LocalTaxAmount ELSE 0 END) as RealizedWHT,
								SUM(CASE WHEN ATT_RealisationDate IS NULL THEN ATT_LocalTaxAmount ELSE 0 END) as NotionalWHT
					 FROM		dbo.AccTaxTransaction
					 WHERE		ATT_IsCancelled = 0
								AND ATT_TaxSuperType = 'SPR'
								AND ATT_AH IN (SELECT value FROM @transactionPKs)
								AND ATT_Ledger = 'AP'
					 GROUP BY	ATT_AH";

			var parameters = new List<ZSqlParameter>();
			parameters.Add(ZSqlParameter.New("@transactionPKs", transactionPKs, AccTaxTransactionSchema.ATT_AH, true));

			var newFactory = new BusinessObjectFactory();
			var details = new DynamicBusinessObjectCollection(newFactory);
			details.Load(sql, parameters.ToArray());

			foreach (DynamicBusinessObject row in details)
			{
				var pk = (ZGuid)row[AccTaxTransactionSchema.Constants.ATT_AH];
				whtAmountDictionary[pk].RealizedAmount = (ZDecimal)row["RealizedWHT"];
				whtAmountDictionary[pk].NotionalAmount = (ZDecimal)row["NotionalWHT"];
			}

			return whtAmountDictionary.Values;
		}
	}
}