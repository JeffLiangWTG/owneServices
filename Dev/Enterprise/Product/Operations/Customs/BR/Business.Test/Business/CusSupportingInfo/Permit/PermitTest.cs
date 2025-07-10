using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(Permit))]
	public class PermitTest : Customs.Business.Testing.CusSupportingInfoTest<Permit>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().Permits.AddNew();
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<Permit>();
			AssertEquals(CusSupportingInfoTypeList.Codes.Permit, supporting.CSI_Type);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		protected override IEnumerable<Permit> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var permitCusSupporting = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().Permits.AddNew();
			permitCusSupporting.CSI_ReferenceNumber = "123";
			permitCusSupporting.CSI_Quantity = 10;
			permitCusSupporting.CSI_UnitOfQuantity = "KG";
			yield return permitCusSupporting;
		}
	}
}
