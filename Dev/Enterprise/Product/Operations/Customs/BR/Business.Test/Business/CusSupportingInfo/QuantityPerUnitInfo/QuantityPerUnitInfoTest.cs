using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(QuantityPerUnitInfo))]
	public class QuantityPerUnitInfoTest : Customs.Business.Testing.CusSupportingInfoTest<QuantityPerUnitInfo>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().QuantityPerUnitInfos.AddNew();
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<QuantityPerUnitInfo>();
			AssertEquals(CusSupportingInfoTypeList.Codes.QuantityPerUnit, supporting.CSI_Type);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		protected override IEnumerable<QuantityPerUnitInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var quantityPerUnitInfo = factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().QuantityPerUnitInfos.AddNew();
			quantityPerUnitInfo.CSI_UnitOfQuantity = "KG";
			quantityPerUnitInfo.CSI_Quantity = 1000;
			yield return quantityPerUnitInfo;
		}

		public void TestValidation()
		{
			AssertType<QuantityPerUnitInfoValidation>(Factory.New<QuantityPerUnitInfo>().Validation);
		}
	}
}
