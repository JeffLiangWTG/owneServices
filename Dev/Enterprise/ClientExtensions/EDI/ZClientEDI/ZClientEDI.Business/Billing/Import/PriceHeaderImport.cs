
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class PriceHeaderImport : AutoPriceHeaderImport
	{
		public override ZBool UseStdDiscount
		{
			get => base.UseStdDiscount;
			set
			{
				base.UseStdDiscount = value;
				UseStdDiscountIsDefined = true;
			}
		}
		public bool UseStdDiscountIsDefined;
	}
}

