using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Test.Declaration
{
	[TestedType(typeof(AddInfoJobComInvoiceHeader))]
	public class AddInfoJobComInvoiceHeaderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<JobComInvoiceHeader>();
			return new AddInfoJobComInvoiceHeader(header.JZ_AddInfoInfo);
		}
	}
}
