using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsHeaderPhase5CustomsLinkedObjectAdapterTest : CustomsLinkedObjectAdapterTest<NctsHeader>
{
	public override void TestAddMessage()
	{
		var movementHeader = Adaptee.MovementHeader;
		AssertEquals($"[PRE-CONDITION] {nameof(Adaptee.Messages)} Count", 0, movementHeader.Messages.Count);

		var newMessage = Factory.New<EDIMessage>();
		Adapter.AddMessage(newMessage);
		AssertEquals($"[POST-CONDITION] {nameof(Adaptee.Messages)} Count", 1, movementHeader.Messages.Count);
	}

	public override void TestEntryReferenceNumber()
	{
		Adaptee.BH_JobReference = "BBB";
		AssertEquals(nameof(Adapter.EntryReferenceNumber), "BBB", Adapter.EntryReferenceNumber);
	}

	public override void TestFactory()
	{
		AssertSame(nameof(Adapter.Factory), Adaptee.Factory, Adapter.Factory);
	}

	public override void TestGetLastSuccessfullySentMessage()
	{
		AssertNull($"{nameof(Adapter.GetLastSuccessfullySentMessage)} not implemented.", Adapter.GetLastSuccessfullySentMessage());
	}

	public override void TestJobReferenceNumber()
	{
		Adaptee.BH_JobReference = "BBB";
		AssertEquals(nameof(Adapter.JobReferenceNumber), "BBB", Adapter.JobReferenceNumber);
	}

	public override void TestCustomsProfile()
	{
		Adaptee.BH_CustomsProfile = "12345";
		AssertEquals(nameof(Adapter.CustomsProfile), "12345", Adapter.CustomsProfile);
	}

	protected override NctsHeader GetAdaptee()
	{
		var header = Factory.NewDepartureNctsHeader();
		header.BH_ApplicationCode = "NC5";
		return header;
	}

	protected override ICustomsLinkedObjectAdapter GetAdapter() => new NctsHeaderPhase5CustomsLinkedObjectAdapter(Adaptee);
}
