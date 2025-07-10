using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(PreApprovalCollection))]
	sealed class PreApprovalCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<PreApproval>
	{
		protected override CusSupportingInfoCollection<PreApproval> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new PreApprovalCollection(jobComInvoice);
		}
	}
}
