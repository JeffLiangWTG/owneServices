using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceHeaderDescription))]
	public class InvoiceHeaderDescriptionTest : Customs.Business.Testing.CusCodeDataTest<InvoiceHeaderDescription>
	{
		public void TestDefaultValue()
		{
			var desc = Factory.New<InvoiceHeaderDescription>();

			AssertEquals(JobComInvoiceHeaderSchema.Constants.Prefix, desc.CY_ParentTableCode);
			AssertEquals(CusCodeDataTypeList.Codes.DescriptionCode, desc.CY_Type);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewDescription(Factory);

		protected virtual InvoiceHeaderDescription GetNewDescription(BusinessObjectFactory factory)
		{
			return factory.New<InvoiceHeaderDescription>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			return declaration.Invoices.AddNew().HeaderDescriptions.AddNew();
		}
	}
}
