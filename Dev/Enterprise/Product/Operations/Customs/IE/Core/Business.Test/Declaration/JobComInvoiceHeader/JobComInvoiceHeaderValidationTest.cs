using System.Reflection;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	abstract class JobComInvoiceHeaderValidationTest<TValidation> : BusinessObjectValidationTestCase
		where TValidation : JobComInvoiceHeaderValidation
	{
		public void TestShouldCheckRelatedHouseBillEntered()
		{
			var (validation, _, _) = SetupData();
			AssertEquals("ShouldCheckRelatedHouseBillEntered", false, (bool)validation.GetType().GetProperty("ShouldCheckRelatedHouseBillEntered", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(validation));
		}

		protected abstract string MessageType { get; }

		protected (TValidation validation, JobComInvoiceHeader invoice, JobDeclaration declaration) SetupData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageType;
			var invoice = declaration.Invoices.AddNew();
			return ((TValidation)invoice.Validation, invoice, declaration);
		}
	}
}
