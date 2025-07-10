using CargoWise.Types;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceFeeFlattened : AutoClientLicenceFeeFlattened
	{
		public ClientLicenceFeeFlattened()
		{
			TaxDate = BillingConstants.Fee.TaxDateCode.FeeTaxAtCurrentDate;
		}

		public override ZString AmountAsText
		{
			set
			{
				base.AmountAsText = value;

				if (!AmountAsText.IsEmpty)
				{
					ZDecimal decimalValue;
					if (ZDecimal.TryParse(AmountAsText, out decimalValue))
					{
						Amount = decimalValue;
					}
					else
					{
						AmountInfo.AddError(Res.GetString("78b687c7-26c4-496e-bf95-92e013ae7a6e", "{0} is not a valid number", AmountAsText));
					}
				}
			}
		}
	}
}


