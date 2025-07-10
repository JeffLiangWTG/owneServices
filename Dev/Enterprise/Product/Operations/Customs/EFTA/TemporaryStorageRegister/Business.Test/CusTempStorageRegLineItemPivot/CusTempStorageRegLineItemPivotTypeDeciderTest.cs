using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineItemPivotTypeDecider))]
sealed class CusTempStorageRegLineItemPivotTypeDeciderTest : TestCaseWithFactory
{
	public void TestGetTypeForLoad_Default()
	{
		var lineItem = Factory.New<CusTempStorageRegLineItem>();
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "A1";
		header.SRH_Reference = "REF";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		var regItemPivot = line.RegLineItemPivots.AddNew();
		regItemPivot.SRV_SRI_Item = lineItem.PK;
		Factory.Save();

		AssertEquals("Type", typeof(CusTempStorageRegLineItemPivot), new BusinessObjectFactory().Load<CusTempStorageRegLineItemPivot>(regItemPivot.PK).GetType());
	}

	public void TestGetTypeForLoad_SUM()
	{
		var lineItem = Factory.New<Integration.Customs.EU.ICusTempStorageRegLineItem>();
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "SUM";
		header.SRH_Reference = "REF";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		var regItemPivot = line.RegLineItemPivots.AddNew();
		regItemPivot.SRV_SRI_Item = lineItem.PK;
		Factory.Save();

		AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.EU.ICusTempStorageRegLineItemPivot>(), new BusinessObjectFactory().Load<CusTempStorageRegLineItemPivot>(regItemPivot.PK).GetType());
	}

	public void TestGetTypeForBinding()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLineItemPivot), Decider.GetTypeForBinding());
	}

	public void TestGetTypeForNew()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLineItemPivot), Decider.GetTypeForNew());
	}

	public void TestGetRegLineType()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLine), Decider.GetRegLineType());
	}

	public void TestGetRegLineItemType()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLineItem), Decider.GetRegLineItemType());
	}

	CusTempStorageRegLineItemPivotTypeDecider Decider => decider ??= new();
	CusTempStorageRegLineItemPivotTypeDecider decider;
}
