using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class ExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestFindRoyaltyChargesOnInvoiceOrGroup()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.Royalty);
			invoice.GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Royalty);
			AssertEquals(2, invoice.FindRoyaltyChargesOnInvoiceOrGroup().Count());
		}
	}
}
