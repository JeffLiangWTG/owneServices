using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineItemPivotCollectionRelationship))]
sealed class CusTempStorageRegLineItemPivotCollectionRelationshipTest : TestCaseWithFactory
{
	public void TestFilter()
	{
		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "ADT";
		regHeader.SRH_Reference = "REF";
		var biz1 = regHeader.CusTempStorageRegLines.AddNew();
		var biz2 = Factory.New<CusTempStorageRegLineItem>();
		var biz3 = Factory.New<CusTempStorageRegLineItem>();

		var pivot12 = Factory.New<CusTempStorageRegLineItemPivot>();
		pivot12.SRV_SRL_Line = biz1.PK;
		pivot12.SRV_SRI_Item = biz2.PK;

		var pivot31 = Factory.New<CusTempStorageRegLineItemPivot>();
		pivot31.SRV_SRL_Line = biz1.PK;
		pivot31.SRV_SRI_Item = biz3.PK;

		var pivot23 = Factory.New<CusTempStorageRegLineItemPivot>();
		pivot23.SRV_SRL_Line = biz2.PK;
		pivot23.SRV_SRI_Item = biz3.PK;

		var pivot12Unrelated = Factory.New<CusTempStorageRegLineItemPivot>();
		pivot12Unrelated.SRV_SRL_Line = biz2.PK;
		pivot12Unrelated.SRV_SRI_Item = biz1.PK;

		var relationship = new CusTempStorageRegLineItemPivotCollectionRelationship(biz1);
		AssertContainsExactElementsInAnyOrder(
			new[] { pivot12, pivot31 },
			Factory.Load<CusTempStorageRegLineItemPivot>(relationship.RelationshipFilter)
		);
	}
}
