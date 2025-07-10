using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(PermitCollection))]
	public class PermitCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<Permit>
	{
		protected override CusSupportingInfoCollection<Permit> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new PermitCollection(jobComInvoice);
		}
	}
}
