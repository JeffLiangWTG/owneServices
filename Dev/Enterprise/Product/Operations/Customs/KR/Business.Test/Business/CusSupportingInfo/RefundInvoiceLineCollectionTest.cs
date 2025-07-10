using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(RefundInvoiceLineCollection))]
	sealed class RefundInvoiceLineCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<RefundInvoiceLine>
	{
		protected override CusSupportingInfoCollection<RefundInvoiceLine> GetCusSupportingInfoCollection()
		{
			var cusReconEntryline = Factory.New<CusReconDeclaration>().CusReconEntryLines.AddNew();
			return new RefundInvoiceLineCollection(cusReconEntryline);
		}
	}
}
