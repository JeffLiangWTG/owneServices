using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ConsentingProcessCollection))]
	public class ConsentingProcessCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<ConsentingProcess>
	{
		protected override Customs.Business.CusSupportingInfoCollection<ConsentingProcess> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new ConsentingProcessCollection(jobComInvoice);
		}
	}
}
