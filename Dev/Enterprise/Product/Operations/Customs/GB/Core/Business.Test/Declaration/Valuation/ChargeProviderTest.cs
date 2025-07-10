using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class ChargeProviderTest : TestCaseWithFactory
	{
		public void TestChargeMatrixAttribute()
		{
			var airCharge = ChargesProvider.AirFreight;
			AssertEquals("ParentTypes", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, airCharge.ParentTypes);
			var overseasFreight = ChargesProvider.InternationalFreight;
			AssertEquals("ParentTypes", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, overseasFreight.ParentTypes);
			var vatAdjustment = ChargesProvider.VATAdjustment;
			AssertEquals("ParentTypes", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, vatAdjustment.ParentTypes);
			var overseasInsurance = ChargesProvider.InternationalInsurance;
			AssertEquals("ParentTypes", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, overseasInsurance.ParentTypes);
			var additionCharge = ChargesProvider.AdditionCharge;
			AssertEquals("ParentTypes", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine, additionCharge.ParentTypes);
			var deductionCharge = ChargesProvider.DeductionCharge;
			AssertEquals("ParentTypes", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine, deductionCharge.ParentTypes);
			var discountCharge = ChargesProvider.Discount;
			AssertEquals("ParentTypes", ChargeParentTypes.GroupInvoice, discountCharge.ParentTypes);
		}
	}
}
