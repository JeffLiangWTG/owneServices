using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ElectronicLogisticInvoiceCollection))]
	class ElectronicLogisticInvoiceCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<ElectronicLogisticInvoice>
	{
		protected override Customs.Business.CusSupportingInfoCollection<ElectronicLogisticInvoice> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new ElectronicLogisticInvoiceCollection(jobComInvoice);
		}
	}
}
