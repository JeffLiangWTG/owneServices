using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ProductCondition))]
	public class ProductCondition_CusCodeDataTest : CusCodeDataTest<ProductCondition>
	{
		public void TestSetDefaultValues()
		{
			var productCondition = Factory.New<ProductCondition>();
			AssertEquals(CusCodeDataTypeList.Codes.EXDOCProductCondition, productCondition.CY_Type);
		}

		protected override IEnumerable<ProductCondition> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (ProductCondition)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var eXDOCLine = invoiceLine.QuarantineExDocLine;
			return invoiceLine.QuarantineExDocLine.ProductConditions.AddNew();
		}
	}
}
