using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ComplementaryLogisticInvoiceCollection))]
	class ComplementaryLogisticInvoiceCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<ComplementaryLogisticInvoice>
	{
		protected override Customs.Business.CusSupportingInfoCollection<ComplementaryLogisticInvoice> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new ComplementaryLogisticInvoiceCollection(jobComInvoice);
		}
	}
}
