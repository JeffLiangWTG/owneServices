using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationPaymentProvider : IDeclarationPayment
	{
		public DeclarationPaymentProvider(ZString taxRevenueCode, decimal feeAmount)
		{
			TaxRevenueCode = taxRevenueCode;
			this.feeAmount = Argument.NotNull(feeAmount, nameof(feeAmount));
		}

		readonly decimal feeAmount;

		public static IEnumerable<DeclarationPaymentProvider> New(CusEntryHeader cusEntryHeader)
		{
			foreach (var mapping in BRRefCusMapper.GetTaxRevenueCodeMapping(cusEntryHeader.Factory))
			{
				var cusEntryLineFees = cusEntryHeader.AllMergedLinesFees.Where(e => e.CF_ChargeType == mapping.Key).ToArray();
				var amount = cusEntryLineFees?.Sum(x => x.CF_ChargeAmount) ?? 0m;
				if (amount > 0)
				{
					yield return new DeclarationPaymentProvider(mapping.Value, amount);
				}
			}
		}

		public string TaxRevenueCode { get; private set; }

		public decimal Amount => feeAmount;
	}
}

