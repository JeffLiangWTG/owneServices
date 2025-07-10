using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(InvoiceCharge))]
sealed class InvoiceChargeTest : EnterpriseBusinessObjectTestCase
{
	public void TestTypeDecider()
	{
		Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
	}

	public void TestShouldIncludedInInvoice()
	{
		var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
		invoice.JZ_IncoTerm = "EXW";
		var charge = invoice.Charges.AddNew();
		charge.Parent.JobDeclaration.RefreshIncotermAndChargeFactory();
		charge.J7_ChargeType = "CBR";
		charge.ResetDefaultIsIncludedInAmountAndIsIncludedInITOTIfDetermined();
		Assert("EXW does not include CBR", !charge.J7_Calc_IsIncludedInInvoiceAmount);
		charge.J7_ChargeType = "CPA";
		charge.ResetDefaultIsIncludedInAmountAndIsIncludedInITOTIfDetermined();
		Assert("EXW includes CPA", charge.J7_Calc_IsIncludedInInvoiceAmount);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return Factory.New<JobDeclaration>().Invoices.AddNew().Charges.AddNew();
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		return GetNewBusinessObject();
	}
}
