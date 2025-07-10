using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ICSPermitCollection))]
	public class ICSPermitCollection_CusClassPivotTest : CusCodeDataCollectionTest<ICSPermit>
	{
		protected override CusCodeDataCollection<ICSPermit> GetCusCodeDataCollection()
		{
			return new ICSPermitCollection(Pivot);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var esmLineSequence = Factory.New<ICSPermit>();
			esmLineSequence.CY_ParentID = Pivot.PK;
			esmLineSequence.CY_ParentTableCode = Pivot.TablePrefix;
			return esmLineSequence;
		}

		CusClassPartPivot Pivot
		{
			get
			{
				if (fPivot == null)
				{
					fPivot = Factory.New<CusClassPartPivot>();
					fPivot.CI_CC = Factory.New<Classification>().PK;
					fPivot.CI_OP = Product.PK;
				}
				return fPivot;
			}
		}
		CusClassPartPivot fPivot;

		AUOrgSupplierPart Product
		{
			get
			{
				if (fProduct == null)
				{
					fProduct = Factory.New<AUOrgSupplierPart>();
					fProduct.OP_PartNum = fProduct.PK.ToString().Replace("-", "");
				}
				return fProduct;
			}
		}
		AUOrgSupplierPart fProduct;
	}
}
