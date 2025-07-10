using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderDepartureMessageSendingObjectParent))]
sealed class NctsHeaderDepartureMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null", () => new NctsHeaderDepartureMessageSendingObjectParent(null));
		AssertExceptionThrown<ArgumentException>("When nctsHeader is not a departure job", () => new NctsHeaderDepartureMessageSendingObjectParent(Factory.New<NctsHeader>()));
	}

	public void TestTopLevelBusinessObject()
	{
		var sendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(nctsHeader);
		AssertSame(nameof(sendingObjectParent.TopLevelBusinessObject), nctsHeader, sendingObjectParent.TopLevelBusinessObject);
	}

	public void TestSecurityCheckpointToSendWithMessageError()
	{
		var sendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(nctsHeader);
		AssertSame(nameof(sendingObjectParent.SecurityCheckpointToSendWithMessageError), Env.Security.EuNctsSendWithMessageErrors, sendingObjectParent.SecurityCheckpointToSendWithMessageError);
	}

	public void TestDefaultSendingObjectsCollection()
	{
		var sendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(nctsHeader);
		AssertSendingObjectsCollection<TransitMessageSendingObject>(sendingObjectParent);
	}

	public void TestTIRSendingObjectsCollection()
	{
		nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
		var sendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(nctsHeader);
		AssertSendingObjectsCollection<TIRMessageSendingObject>(sendingObjectParent);
	}

	public void TestTransitSendingObjectsCollection()
	{
		nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
		var sendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(nctsHeader);
		AssertSendingObjectsCollection<TransitMessageSendingObject>(sendingObjectParent);
	}

	public void TestNBStandaloneSendingObjectsCollection()
	{
		nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.NbRejected;
		var sendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(nctsHeader);
		AssertSendingObjectsCollection<NBStandaloneMessageSendingObject>(sendingObjectParent);
	}

	public void TestPhase4MessageSendingObjectProperties()
	{
		var sendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(nctsHeader);
		var sendingObjectPropertyNames = sendingObjectParent.MessageSendingObjectProperties.Select(x => x.PropertyName).ToArray();
		AssertArrayEqualsByElements("MessageSendingObjectProperties", new ZString[] { "DepartureStatus", "CombinedCustomsMessageSubType", "MessageStatus", "ReferenceNumber" }, sendingObjectPropertyNames);
	}

	public void TestCustomsMessageSendingMode()
	{
		var sendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(nctsHeader);
		CombineAssertions("PRE-CONDITION", () =>
		{
			AssertEquals("SendingObjectsCollection count", 1, sendingObjectParent.SendingObjectsCollection.Count);
			AssertEquals("CustomsMessageSendingMode for SendingObjectParent", ZString.Empty, sendingObjectParent.CustomsMessageSendingMode);
			AssertEquals("CustomsMessageSendingMode for SendingObjectParent.SendingObjectsCollection[0]", ZString.Empty, sendingObjectParent.SendingObjectsCollection[0].CustomsMessageSendingMode);
		});

		sendingObjectParent.CustomsMessageSendingMode = "XXX";
		CombineAssertions("POST-CONDITION", () =>
		{
			AssertEquals("CustomsMessageSendingMode for SendingObjectParent", "XXX", sendingObjectParent.CustomsMessageSendingMode);
			AssertEquals("CustomsMessageSendingMode for SendingObjectParent.SendingObjectsCollection[0]", "XXX", sendingObjectParent.SendingObjectsCollection[0].CustomsMessageSendingMode);
		});
	}

	public void TestCustomsProfile()
	{
		nctsHeader.BH_CustomsProfile = "1234";
		var sendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(nctsHeader);
		AssertEquals("CustomsProfile", "1234", sendingObjectParent.CustomsProfile);
	}

	protected override BusinessObject GetNewBusinessObject() => new NctsHeaderDepartureMessageSendingObjectParent(Factory.NewDepartureNctsHeader());

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
	}

	NctsHeader nctsHeader;

	void AssertSendingObjectsCollection<TCollectionSingleElement>(NctsHeaderDepartureMessageSendingObjectParent sendingObjectParent)
	{
		var sendingObjectsCollection = sendingObjectParent.SendingObjectsCollection;
		AssertNotNull(nameof(sendingObjectsCollection), sendingObjectsCollection);
		AssertEquals(nameof(sendingObjectsCollection.Count), 1, sendingObjectsCollection.Count);
		AssertType<TCollectionSingleElement>($"{nameof(sendingObjectsCollection)} single element type", sendingObjectsCollection[0]);
	}
}
