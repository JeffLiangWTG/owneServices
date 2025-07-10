using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ReferenceInvoiceManual))]
	public class ReferenceInvoiceManualTest : Customs.Business.Testing.CusSupportingInfoTest<ReferenceInvoiceManual>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ReferenceInvoiceManualCollection.AddNew();
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<ReferenceInvoiceManual>();
			AssertEquals(CusSupportingInfoTypeList.Codes.ReferenceInvoiceManual, supporting.CSI_Type);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		protected override IEnumerable<ReferenceInvoiceManual> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var permitCusSupporting = factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ReferenceInvoiceManualCollection.AddNew();
			permitCusSupporting.CSI_ReferenceNumber = "1";
			permitCusSupporting.CSI_LineNo = 1;
			yield return permitCusSupporting;
		}
	}
}
