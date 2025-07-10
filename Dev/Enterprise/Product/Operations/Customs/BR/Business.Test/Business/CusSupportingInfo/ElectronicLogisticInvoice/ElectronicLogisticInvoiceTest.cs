using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ElectronicLogisticInvoice))]
	public class ElectronicLogisticInvoiceTest : Customs.Business.Testing.CusSupportingInfoTest<ElectronicLogisticInvoice>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ElectronicLogisticInvoiceCollection.AddNew();
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<ElectronicLogisticInvoice>();
			AssertEquals(CusSupportingInfoTypeList.Codes.ElectronicLogisticInvoice, supporting.CSI_Type);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		protected override IEnumerable<ElectronicLogisticInvoice> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var electronicInvoiceCusSupporting = factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ElectronicLogisticInvoiceCollection.AddNew();
			electronicInvoiceCusSupporting.CSI_ReferenceNumber = "1";
			electronicInvoiceCusSupporting.CSI_LineNo = 1;
			yield return electronicInvoiceCusSupporting;
		}
	}
}
