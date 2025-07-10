using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DK.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceLine))]
	class AddInfoJobComInvoiceLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<AddInfoJobComInvoiceLineLookups>(addInfo.Lookups);
		}

		public void TestValidation()
		{
			AssertType<AddInfoJobComInvoiceLineValidation>(addInfo.Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => addInfo;

		protected override void SetUp()
		{
			base.SetUp();
			addInfo = new AddInfoJobComInvoiceLine(Factory.New<JobComInvoiceLine>().JI_AddInfoInfo);
		}
		AddInfoJobComInvoiceLine addInfo;
	}
}
