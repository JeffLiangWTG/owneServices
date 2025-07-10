using System;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryLineCustomsLineLinkedObjectAdapterTest : CustomsLineLinkedObjectAdapterTest<CusEntryLine>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("entryLine required", () => new CusEntryLineCustomsLineLinkedObjectAdapter(null));
		AssertNoExceptionThrown("Valid entryLine", () => new CusEntryLineCustomsLineLinkedObjectAdapter(Adaptee));
	}

	public override void TestLineNo()
	{
		Adaptee.CL_LineNumber = 99;
		AssertEquals(nameof(Adapter.LineNo), Adaptee.CL_LineNumber, Adapter.LineNo);
	}

	public override void TestNBStatus()
	{
		Adaptee.ZG_NBStatus = EntryLineCustomsStatusList.Codes.Approved;
		AssertEquals(nameof(Adapter.NBStatus), Adaptee.ZG_NBStatus, Adapter.NBStatus);
	}

	public override void TestSetNBStatus()
	{
		AssertEquals("PRE-CONDITION", ZString.Empty, Adaptee.ZG_NBStatus);

		Adapter.SetNBStatus(EntryLineCustomsStatusList.Codes.Rejected);
		AssertEquals("POST-CONDITION", EntryLineCustomsStatusList.Codes.Rejected, Adaptee.ZG_NBStatus);
	}

	protected override CusEntryLine GetAdaptee() => Factory.New<CusEntryLine>();

	protected override ISadCustomsLineLinkedObjectAdapter GetAdapter() => new CusEntryLineCustomsLineLinkedObjectAdapter(Adaptee);
}
