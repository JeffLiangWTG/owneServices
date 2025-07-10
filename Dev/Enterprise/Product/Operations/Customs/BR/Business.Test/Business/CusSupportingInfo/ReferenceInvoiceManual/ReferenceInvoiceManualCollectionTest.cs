using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ReferenceInvoiceManualCollection))]
	class ReferenceInvoiceManualCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<ReferenceInvoiceManual>
	{
		protected override Customs.Business.CusSupportingInfoCollection<ReferenceInvoiceManual> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new ReferenceInvoiceManualCollection(jobComInvoice);
		}
	}
}
