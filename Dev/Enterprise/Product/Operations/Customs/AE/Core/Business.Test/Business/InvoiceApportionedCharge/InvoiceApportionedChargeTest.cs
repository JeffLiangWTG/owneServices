using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(InvoiceApportionedCharge))]
public class InvoiceApportionedChargeTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var charge = factory.New<InvoiceApportionedCharge>();
		charge.J7_ParentTableCode = "JZ";
		return charge;
	}
}
