using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface ITaxTransactionToPivotsAmountsDistributor
	{
		void AdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(AccTaxTransaction taxRecord);
	}

	internal class TaxTransactionToPivotsAmountsDistributor : ITaxTransactionToPivotsAmountsDistributor
	{
		void ITaxTransactionToPivotsAmountsDistributor.AdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(AccTaxTransaction taxRecord)
		{
			var pivots = taxRecord.Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord.PK) { FetchOnlyFromLocalCache = true });

			if (pivots.IsNullOrEmpty())
			{
				return;
			}

			var pivotsAndLocalTaxAmountsNotRounded = new List<(AccTaxRecordTransactionLinePivot Pivot, ZDecimal LocalTaxAmountNotRounded)>(pivots.Length);
			var taxRecordLocalTaxAmountToOSTaxBaseAmountRatio = taxRecord.ATT_OSTaxBaseAmount != 0m ? taxRecord.ATT_LocalTaxAmount / taxRecord.ATT_OSTaxBaseAmount : 0m;
			var localDecimalsForRounding = taxRecord.Company.GetLocalDecimals();
			var sumOfPivotsLocalTaxAmounts = 0m;
			foreach (var pivot in pivots)
			{
				var localTaxAmountNotRounded = taxRecordLocalTaxAmountToOSTaxBaseAmountRatio * pivot.BaseOSAmount;
				var localTaxAmountRounded = GetPivotLocalTaxAmountRounded(localTaxAmountNotRounded);
				sumOfPivotsLocalTaxAmounts += localTaxAmountRounded;
				pivotsAndLocalTaxAmountsNotRounded.Add((pivot, localTaxAmountNotRounded));
			}

			var difference = sumOfPivotsLocalTaxAmounts - taxRecord.ATT_LocalTaxAmount;
			IEnumerable<(AccTaxRecordTransactionLinePivot Pivot, ZDecimal LocalTaxAmountNotRounded)> pivotsInOrderOfLocalTaxAmountsNotRounded = pivotsAndLocalTaxAmountsNotRounded.OrderBy(p => p.LocalTaxAmountNotRounded).ToArray();
			if (difference > 0m)
			{
				pivotsInOrderOfLocalTaxAmountsNotRounded = pivotsInOrderOfLocalTaxAmountsNotRounded.Reverse();
			}

			foreach (var item in pivotsInOrderOfLocalTaxAmountsNotRounded)
			{
				var localTaxAmountRounded = GetPivotLocalTaxAmountRounded(item.LocalTaxAmountNotRounded);

				if (difference != 0m && localTaxAmountRounded != 0m)
				{
					var differenceToDeduct = localTaxAmountRounded > 0m ? Math.Min(localTaxAmountRounded, difference) : Math.Max(localTaxAmountRounded, difference);

					localTaxAmountRounded -= differenceToDeduct;
					difference -= differenceToDeduct;
				}

				item.Pivot.ATP_LocalTaxAmount = localTaxAmountRounded;
			}

			if (difference != 0m)
			{
				var zeroLocalTaxAmountPivot = pivotsInOrderOfLocalTaxAmountsNotRounded.First().Pivot;
				zeroLocalTaxAmountPivot.ATP_LocalTaxAmount -= difference;
			}

			ZDecimal GetPivotLocalTaxAmountRounded(ZDecimal localTaxAmountNotRounded)
			{
				return Utilities.Round(localTaxAmountNotRounded, localDecimalsForRounding);
			}
		}
	}
}
