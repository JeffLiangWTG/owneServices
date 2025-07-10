using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAClassificationCollection))]
	sealed class CusCAClassificationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCCA_ParentIDAndParentTableCode()
		{
			var collectin = new CusCAClassificationCollection(Pivot);
			var item = collectin.AddNew();
			AssertEquals(Pivot.PK, item.CCA_ParentID);
			AssertEquals(Pivot.TablePrefix, item.CCA_ParentTableCode);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusCAClassificationCollection(Pivot);
		}

		CusClassPartPivot Pivot
		{
			get
			{
				if (pivot == null)
				{
					var classification = Factory.New<CusClassification>();
					OrgSupplierPart product = Factory.New<OrgSupplierPart>();
					product.OP_PartNum = product.PK.ToString().Replace("-", "");

					pivot = Factory.New<CusClassPartPivot>();
					pivot.CI_CC = classification.PK;
					pivot.CI_OP = product.PK;
				}

				return pivot;
			}
		}
		CusClassPartPivot pivot;
	}
}
