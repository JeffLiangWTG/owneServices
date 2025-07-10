using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public sealed class CostWrapper : GenericWrapper
	{
		public CostWrapper(JobConsolCost cost, BusinessObjectFactory factory)
			: base(cost, factory)
		{
			this.cost = cost;
		}

		public CodeAndDescriptionWrapper ChargeCode
		{
			get
			{
				if (chargeCode == null)
				{
					AccChargeCode ac = cost.ChargeCode;
					chargeCode = new CodeAndDescriptionWrapper(ac == null ? ZString.Empty : ac.AC_Code, cost.Lookups.ChargeCodes, Factory);
				}

				return chargeCode;
			}
		}
		CodeAndDescriptionWrapper chargeCode;

		public MoneyWrapper GSTAmount
		{
			get { return new MoneyWrapper(new Money(cost.E6_OSGSTAmount_Calc, cost.Currency), Factory); }
		}

		public MoneyWrapper OSCost
		{
			get { return new MoneyWrapper(new Money(cost.E6_OSCostAmount, cost.Currency), Factory); }
		}

		public MoneyWrapper LocalCost
		{
			get { return new MoneyWrapper(new Money(cost.E6_LocalCostAmount, RefCurrency.LoadFromCurrencyCode(Factory, cost.LocalCurrency)), Factory); }
		}

		public OrganisationWrapper Creditor
		{
			get { return new OrganisationWrapper(OrganisationUsageType.Creditor, cost.Creditor, ContactType.Payables, Factory); }
		}

		public ForwardingConsol Consol
		{
			get { return cost.Consol as ForwardingConsol; }
		}

		readonly JobConsolCost cost;
	}
}
