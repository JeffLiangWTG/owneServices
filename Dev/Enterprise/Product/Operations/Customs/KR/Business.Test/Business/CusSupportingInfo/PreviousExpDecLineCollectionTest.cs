using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(PreviousExpDecLineCollection))]
	sealed class PreviousExpDecLineCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<PreviousExpDecLine>
	{
		protected override CusSupportingInfoCollection<PreviousExpDecLine> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new PreviousExpDecLineCollection(jobComInvoice);
		}
	}
}
