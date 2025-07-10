using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.TaxFramework
{
	public class TaxLineFromPostingCharge : ITaxableTransactionLineBase
	{
		public TaxLineFromPostingCharge(PostingChargeKey key, IReceivablesPostingCharge postingCharge, ReadOnlyBusinessObjectFactory factory)
		{
			Argument.NotNull(key, "PostingChargeKey");
			Argument.NotNull(postingCharge, "IReceivablesPostingCharge");
			Argument.NotNull(factory, "ReadOnlyBusinessObjectFactory");

			Key = key;
			PostingCharge = postingCharge;
			Factory = factory;
			pk = ZGuid.NewZGuid();
		}

		readonly PostingChargeKey Key;
		readonly public IReceivablesPostingCharge PostingCharge;
		readonly ReadOnlyBusinessObjectFactory Factory;

		public ZGuid PK => pk;
		readonly ZGuid pk;

		BusinessObjectFactory ITaxableTransactionLineBase.Factory => Factory;

		GlbBranch ITaxableTransactionLineBase.Branch => branch ?? (branch = Factory.Load<GlbBranch>(PostingCharge.SellTaxBranch) ?? Factory.Load<GlbBranch>(PostingCharge.Branch));
		GlbBranch branch;

		public AccChargeCode ChargeCode => chargeCode ?? (chargeCode = Factory.Load<AccChargeCode>(PostingCharge.ChargeCode));
		AccChargeCode chargeCode;

		ZString ITaxableTransactionLineBase.Currency => PostingCharge.SellCurrency.RX_Code;

		ZDate ITaxableTransactionLineBase.TaxDate => PostingCharge.SellTaxDate;

		ZGuid ITaxableTransactionLineBase.JobPK => Job?.PK ?? ZGuid.Empty;

		AccChargeTaxOverrideMatcher.TaxCalculationParameters ITaxableTransactionLineBase.GetTaxCalculationParameters()
		{
			AccChargeTaxOverrideMatcher.TaxCalculationParameters parameters = null;

			if (ChargePoster.IsConsolRelated(Key))
			{
				var consolNumber = Key.JobNumber;
				var consol = ChargePoster.GetConsol(consolNumber, Factory);
				if (consol != null)
				{
					parameters = consol.GetTaxCalculationParameters();
					parameters.Organisation = PostingCharge.Debtor;
				}
			}

			if (parameters == null)
			{
				parameters = Job?.GetTaxCalculationParameters();
				if (parameters != null)
				{
					AddParameters();
				}
			}

			if (parameters == null)
			{
				parameters = new AccChargeTaxOverrideMatcher.TaxCalculationParameters();
				AddParameters();
			}

			var placeOfSupplyLocation = PlaceOfSupplyHelper.TryConvertToLocation(((ITaxableTransactionLineBase)this).Branch.Company, PostingCharge.SellPlaceOfSupply);
			if (placeOfSupplyLocation != null)
			{
				parameters.FixedPlaceOfSupply = placeOfSupplyLocation;
			}

			if (!PostingCharge.SellSupplyType.IsEmpty)
			{
				parameters.SupplyType = PostingCharge.SellSupplyType;
			}

			return parameters;

			void AddParameters()
			{
				parameters.CostOrSell = CostSell.Revenue;
				parameters.Organisation = PostingCharge.Debtor;
				parameters.Branch = ((ITaxableTransactionLineBase)this).Branch;
			}
		}

		ZDecimal ITaxableTransactionLineBase.BaseOSAmount => 0M;

		ZDecimal ITaxableTransactionLineBase.LocalAmount => 0M;

		#region Implementation

		Job Job => job ?? (job = PostingCharge.Job != null ? Factory.Load<Job>(PostingCharge.Job.PK) : null);

		Job job;

		#endregion
	}
}
