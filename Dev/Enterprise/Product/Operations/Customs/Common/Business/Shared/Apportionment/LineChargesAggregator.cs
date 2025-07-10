using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common
{
	public class LineChargesAggregator
	{
		public Dictionary<ApportionChargeKey, Money> GetTotal(IChargeHolder invoice, IComparer<IChargeApportionee> apportioneeComparer)
		{
			var result = new Dictionary<ApportionChargeKey, Money>();

			var apportionees = new List<IChargeApportionee>(invoice.AllApportionees);
			apportionees.Sort(apportioneeComparer);

			foreach (IChargeApportionee apportionee in apportionees)
			{
				var charges = new List<JobComInvCharge>(new TypedEnumerable<JobComInvCharge>(apportionee.Charges));
				charges.AddRange(new TypedEnumerable<JobComInvCharge>(apportionee.ApportionedCharges));
				charges.Sort(new ChargeComparer());

				foreach (JobComInvCharge charge in charges)
				{
					if (charge.Currency != null && !charge.J7_IsSystem)
					{
						Money total;

						if (result.TryGetValue(charge.ApportionChargeKey, out total))
						{
							total = invoice.CurrencyConverter.Add(total, charge.Money);
						}
						else
						{
							total = charge.Money;
						}

						result[charge.ApportionChargeKey] = total;
					}
				}
			}

			return result;
		}

		class ChargeComparer : IComparer<JobComInvCharge>
		{
			public int Compare(JobComInvCharge x, JobComInvCharge y)
			{
				var result = 0;

				result = x.J7_ChargeType.CompareTo(y.J7_ChargeType);

				if (result == 0)
				{
					result = x.J7_RX_NKCurrency.CompareTo(y.J7_RX_NKCurrency);
				}

				if (result == 0)
				{
					result = x.J7_Amount.CompareTo(y.J7_Amount);
				}

				if (result == 0)
				{
					result = x.PK.CompareTo(y.PK);
				}

				return result;
			}
		}
	}
}
