using System;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureCargoDescCustomsLineLinkedObjectAdapterTest : CustomsLineLinkedObjectAdapterTest<NctsDepartureCargoDesc>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("nctsDepartureCargoDesc required", () => new NctsDepartureCargoDescCustomsLineLinkedObjectAdapter(null));
		AssertNoExceptionThrown("Valid nctsDepartureCargoDesc", () => new NctsDepartureCargoDescCustomsLineLinkedObjectAdapter(Adaptee));
	}

	public override void TestLineNo()
	{
		Adaptee.BY_LineNo = 99;
		AssertEquals(nameof(Adapter.LineNo), Adaptee.BY_LineNo, Adapter.LineNo);
	}

	public override void TestNBStatus()
	{
		Adaptee.BY_Status = EntryLineCustomsStatusList.Codes.Approved;
		AssertEquals(nameof(Adapter.NBStatus), Adaptee.BY_Status, Adapter.NBStatus);
	}

	public override void TestSetNBStatus()
	{
		AssertEquals("PRE-CONDITION", ZString.Empty, Adaptee.BY_Status);

		Adapter.SetNBStatus(EntryLineCustomsStatusList.Codes.Rejected);
		AssertEquals("POST-CONDITION", EntryLineCustomsStatusList.Codes.Rejected, Adaptee.BY_Status);
	}

	protected override NctsDepartureCargoDesc GetAdaptee() => Factory.New<NctsDepartureCargoDesc>();

	protected override ISadCustomsLineLinkedObjectAdapter GetAdapter() => new NctsDepartureCargoDescCustomsLineLinkedObjectAdapter(Adaptee);
}
