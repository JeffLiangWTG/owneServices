using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	sealed class BaseCusClassPartPivotBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Pivot;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return Pivot;
		}

		CusClassPartPivot Pivot
		{
			get
			{
				if (pivotCache != null)
				{
					return pivotCache;
				}
				var part = Factory.New<OrgSupplierPart>();
				part.OP_PartNum = "123";
				pivotCache = Factory.New<CusClassPartPivot>();
				pivotCache.CI_OP = part.PK;
				return pivotCache;
			}
		}
		CusClassPartPivot pivotCache;
	}
}
