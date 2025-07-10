using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GAApprovalCollection))]
	sealed class GAApprovalCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<GAApproval>
	{
		protected override CusSupportingInfoCollection<GAApproval> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new GAApprovalCollection(jobComInvoice);
		}
	}
}
