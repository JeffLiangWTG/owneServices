using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(NonGADetailCollection))]
	sealed class NonGADetailCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<NonGADetail>
	{
		protected override CusSupportingInfoCollection<NonGADetail> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new NonGADetailCollection(jobComInvoice);
		}
	}
}
