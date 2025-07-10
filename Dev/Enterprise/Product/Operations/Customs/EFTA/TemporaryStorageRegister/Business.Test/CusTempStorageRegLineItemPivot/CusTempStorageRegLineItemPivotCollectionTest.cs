using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>))]
sealed class CusTempStorageRegLineItemPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>>
{
	public void TestAddChild()
	{
		var regLine = Factory.New<CusTempStorageRegLine>();
		var regItem = (BusinessObject)Factory.New<CusTempStorageRegLineItem>();
		var collection = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regLine);
		var pivot = collection.AddChild(regItem);

		CombineAssertions(() =>
		{
			AssertEquals("Relation1ID", regLine.PK, pivot.Relation1ID);
			AssertEquals("Relation2ID", regItem.PK, pivot.Relation2ID);
		});
	}

	public void TestAddParent()
	{
		var regItem = Factory.New<CusTempStorageRegLineItem>();
		var regLine = Factory.New<CusTempStorageRegLine>();
		var collection = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regItem);
		var pivot = collection.AddParent(regLine);

		CombineAssertions(() =>
		{
			AssertEquals("Relation1ID", regLine.PK, pivot.Relation1ID);
			AssertEquals("Relation2ID", regItem.PK, pivot.Relation2ID);
		});
	}

	public void TestLoadCollection()
	{
		CombineAssertions(() =>
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "ADT";
			regHeader.SRH_Reference = "REF";
			var regLine1 = regHeader.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			var regItem11 = (BusinessObject)Factory.New<CusTempStorageRegLineItem>();
			var regItem12 = (BusinessObject)Factory.New<CusTempStorageRegLineItem>();
			var collection = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regLine1);
			_ = collection.AddChild(regItem11);
			_ = collection.AddChild(regItem12);

			var regItem21 = (BusinessObject)Factory.New<CusTempStorageRegLineItem>();
			var collection2 = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regLine2);
			_ = collection2.AddChild(regItem21);
			_ = collection2.AddChild(regItem11);

			var otherRegHeader = Factory.New<CusTempStorageRegHeader>();
			otherRegHeader.SRH_AppCode = "ADT";
			otherRegHeader.SRH_Reference = "REF2";
			var otherRegLine = otherRegHeader.CusTempStorageRegLines.AddNew();
			otherRegLine.SRL_LineNumber = 3;
			var otherRegItem = (BusinessObject)Factory.New<CusTempStorageRegLineItem>();
			var otherCollection = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(otherRegLine);
			_ = otherCollection.AddChild(otherRegItem);
			_ = collection2.AddChild(otherRegItem);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var header = newFactory.Load<CusTempStorageRegHeader>(regHeader.PK);
			var line1 = header.CusTempStorageRegLines[0];
			AssertEquals("Line1 has its own items", 2, line1.RegLineItemPivots.Count);

			var line2 = header.CusTempStorageRegLines[1];
			AssertEquals("Line2 has 2 items and share 1 with otherLine", 3, line2.RegLineItemPivots.Count);
		});
	}

	protected override CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot> GetCollectionToTest() => new(RegLine);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var child = Factory.NewWithValidTestData<CusTempStorageRegLineItem>();
		var pivot = Factory.New<CusTempStorageRegLineItemPivot>();
		pivot.SRV_SRL_Line = RegLine.PK;
		pivot.SRV_SRI_Item = child.PK;
		return pivot;
	}

	CusTempStorageRegLine RegLine => regLine ??= Factory.NewWithValidTestData<CusTempStorageRegLine>();
	CusTempStorageRegLine regLine;
}
