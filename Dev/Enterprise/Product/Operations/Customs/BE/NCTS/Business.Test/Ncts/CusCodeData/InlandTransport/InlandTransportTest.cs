using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(InlandTransport))]
class InlandTransportTest : Customs.Business.Testing.CusCodeDataTest<InlandTransport>
{
	public void TestDefaultValues()
	{
		var bill = Factory.New<NctsBill>();
		var inlandTransport = bill.InlandTransports.AddNew();

		AssertEquals(Constants.CusCodeDataTypes.TransportInland, inlandTransport.CY_Type);
	}

	public void TestISequenceNumberLine()
	{
		var nctsBill = Factory.New<NctsBill>();
		var inlandTransport = nctsBill.InlandTransports.AddNew();

		CombineAssertions(() =>
		{
			var sequenceLine = (IShortSequenceNumberLine)inlandTransport;
			AssertEquals("FKToHeader", nctsBill.PK, sequenceLine.FKToHeader);
			AssertEquals("SequenceNumber", (ZShort)1, sequenceLine.SequenceNumber);
		});
	}

	public void TestCY_Order()
	{
		var nctsBill = Factory.New<NctsBill>();
		var inlandTransport = nctsBill.InlandTransports.AddNew();
		var inlandTransport2 = nctsBill.InlandTransports.AddNew();
		var inlandTransport3 = nctsBill.InlandTransports.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Order 1", (ZShort)1, inlandTransport.CY_Order);
			AssertEquals("Order 2", (ZShort)2, inlandTransport2.CY_Order);
			AssertEquals("Order 3", (ZShort)3, inlandTransport3.CY_Order);

			nctsBill.InlandTransports.Remove(inlandTransport2);
			AssertEquals("Order 1 stay same", (ZShort)1, inlandTransport.CY_Order);
			AssertEquals("Order 3 change to 2", (ZShort)2, inlandTransport3.CY_Order);
		});
	}

	public void TestValidationType()
	{
		var inlandTransport = Factory.New<InlandTransport>();
		AssertType<InlandTransportValidation>(inlandTransport.Validation);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override IEnumerable<InlandTransport> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return (InlandTransport)GetNewBusinessObjectForDeleteTest(factory);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var cusInBondHeader = factory.New<NctsHeader>();
		cusInBondHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		factory.Save();
		var bill = factory.New<NctsBill>();
		bill.B0_BH = cusInBondHeader.PK;

		return bill.InlandTransports.AddNew();
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad()
	{
		var inlandTransport = GetNewBusinessObject();
		inlandTransport.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections, Array.Empty<PropertyDescriptor>());
		return inlandTransport;
	}

	protected override void LoadParentIfNeeded(BusinessObjectFactory factory, InlandTransport bizObj)
	{
		factory.Load<NctsBill>(bizObj.CY_ParentID);
	}
}
