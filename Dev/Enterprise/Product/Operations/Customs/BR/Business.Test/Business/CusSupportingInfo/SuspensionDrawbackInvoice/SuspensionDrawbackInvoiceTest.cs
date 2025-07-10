using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(SuspensionDrawbackInvoice))]
	public class SuspensionDrawbackInvoiceTest : Customs.Business.Testing.CusSupportingInfoTest<SuspensionDrawbackInvoice>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().SuspensionDrawbackCollection.AddNew().SuspensionDrawbackInvoiceCollection.AddNew();
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<SuspensionDrawbackInvoice>();
			AssertEquals(CusSupportingInfoTypeList.Codes.SuspensionDrawbackInvoice, supporting.CSI_Type);
		}

		protected override IEnumerable<SuspensionDrawbackInvoice> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var drawbackCusSupporting = factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().SuspensionDrawbackCollection.AddNew().SuspensionDrawbackInvoiceCollection.AddNew();
			drawbackCusSupporting.CSI_ReferenceNumber = "1";
			drawbackCusSupporting.CSI_LineNo = 1;
			yield return drawbackCusSupporting;
		}
	}
}
