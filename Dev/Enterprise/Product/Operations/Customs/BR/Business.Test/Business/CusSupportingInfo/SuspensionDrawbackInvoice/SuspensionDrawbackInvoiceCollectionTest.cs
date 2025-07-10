using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(SuspensionDrawbackInvoiceCollection))]
	class SuspensionDrawbackInvoiceCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<SuspensionDrawbackInvoice>
	{
		protected override Customs.Business.CusSupportingInfoCollection<SuspensionDrawbackInvoice> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new SuspensionDrawbackInvoiceCollection(jobComInvoice);
		}
	}
}
