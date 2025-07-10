using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderDepartureMessageSendingObjectCollection))]
sealed class NctsHeaderDepartureMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NctsHeaderDepartureMessageSendingObjectCollection>
{
	public void TestAllowNew()
	{
		AssertEquals("AllowNew", false, GetCollectionToTest().AllowNew);
	}

	protected override NctsHeaderDepartureMessageSendingObjectCollection GetCollectionToTest() => new NctsHeaderDepartureMessageSendingObjectCollection(Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection() => new NctsHeaderDepartureMessageSendingObjectOnlyForTestingPurposes(Factory.NewDepartureNctsHeader());
}

#region NctsHeaderDepartureMessageSendingObjectOnlyForTestingPurposes

[TestExcludeBusinessObjectsAllHaveTestCases]
class NctsHeaderDepartureMessageSendingObjectOnlyForTestingPurposes : NctsHeaderDepartureMessageSendingObject
{
	public NctsHeaderDepartureMessageSendingObjectOnlyForTestingPurposes(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	protected override IEnumerable<ISadCustomsMessage> GetMessageObjects() => null;

	protected override ZString GetMessageSubType() => ZString.Empty;
}

#endregion
