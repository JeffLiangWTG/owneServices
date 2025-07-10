using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusKRClassification))]
	sealed class CusKRClassificationTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetKRClassificationForTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetKRClassificationForTest(factory);

		CusKRClassification GetKRClassificationForTest(BusinessObjectFactory factory)
		{
			var product = factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test Baby formula";
			var pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			return pivot.KRClassification;
		}
	}
}
