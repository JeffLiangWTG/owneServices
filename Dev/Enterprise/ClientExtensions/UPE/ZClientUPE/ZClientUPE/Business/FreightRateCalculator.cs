using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Rating.Business;

namespace Enterprise.Client.UPE.Business
{
	public class FreightRateCalculator
	{
		public KeyValuePair<ZDecimal, ZString> CalculateFreightRateOnDeclaration(BaseJobDeclaration declaration, bool accumulate = false)
		{
			BaseJobComInvHeaderCharge baseJobComInvHeaderCharge = null;

			var freightAutoRater = new FreightAutoRater(new RatingContext());
			var criteria = new RatingCriteria(((IRatingSupporterWithAdapter)declaration).RatingAdapter, declaration.Factory);

			var interactor = new LoggerDecorator();
			var options = new AutoRateOptions(autoRateRevenue: true, triggerSource: AutoRateTriggerSource.Unspecified);
			AutoRateResult result;

			using (_Rating.Start(interactor, options))
			using (_Rating.StartSell())
			{
				result = freightAutoRater.AutoRate(criteria, CostSell.Revenue);
			}

			foreach (AutoRateInfo autoRateInfo in result.RateInfoCollection)
			{
				if (autoRateInfo.ChargeCode.PK == Env.Registry.FreightChargeCode)
				{
					baseJobComInvHeaderCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.GetChargeByChargeName(CustomsChargeTypeList.Codes.OverseasFreight);
					if (baseJobComInvHeaderCharge != null)
					{
						if (accumulate)
						{
							baseJobComInvHeaderCharge.J7_Amount += autoRateInfo.Amount;
						}
						else
						{
							baseJobComInvHeaderCharge.J7_Amount = autoRateInfo.Amount;
						}

						baseJobComInvHeaderCharge.J7_RX_NKCurrency = autoRateInfo.Currency;
					}
					else
					{
						baseJobComInvHeaderCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, autoRateInfo.Amount, autoRateInfo.Currency);
					}
					break;
				}
			}

			return baseJobComInvHeaderCharge != null ? new KeyValuePair<ZDecimal, ZString>(baseJobComInvHeaderCharge.J7_Amount, baseJobComInvHeaderCharge.J7_RX_NKCurrency) : new KeyValuePair<ZDecimal, ZString>(ZDecimal.Zero, ZString.Empty);
		}
	}
}
