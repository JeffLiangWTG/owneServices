using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobCAComInvoiceHeader))]
	sealed class JobCAComInvoiceHeaderTest : IAddInfoChildUniqueIndexFailureHandlerSupporterTestCase<JobCAComInvoiceHeader>
	{
		protected override string ExpectedUniqueIndexName => JobCAComInvoiceHeaderSchema.Constants.Indexes.FK_UX__CAZ_JZ;
		protected override EnterpriseBusinessObject GetParent(JobCAComInvoiceHeader bizObj) => bizObj.InvoiceHeader;
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			return header.CAInvoiceHeader;
		}
	}
}
