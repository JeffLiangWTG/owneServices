using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

[TestedType(typeof(NctsHeaderMessageSendingObjectParent))]
sealed class NctsHeaderMessageSendingObjectAdditionalValidationTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsHeaderMessageSendingObjectAdditionalValidation(null));
		AssertNoExceptionThrown(() => new NctsHeaderMessageSendingObjectAdditionalValidation(new NctsHeaderMessageSendingObject(nctsHeader)));
	}

	[TestDate(2023, 9, 22)]
	public void TestBM_PresentationDateTime_PastDate()
	{
		const string expectedMessage = "Date of presentation can not be in the past.";
		NctsHeaderMessageSendingObjectParent GetMessageSendingObjectParent() => new NctsHeaderMessageSendingObjectParent(nctsHeader);
		CombineAssertions("When BM_PresentationDateTime is", () =>
		{
			AssertNotContains("empty", expectedMessage, GetMessageSendingObjectParent().BizObjValidationMessageErrors);

			movementHeader.BM_PresentationDateTime = new DateTime(2023, 8, 1);
			AssertContains("past", expectedMessage, GetMessageSendingObjectParent().BizObjValidationMessageErrors);

			movementHeader.BM_PresentationDateTime = new DateTime(2023, 9, 22);
			AssertNotContains("present", expectedMessage, GetMessageSendingObjectParent().BizObjValidationMessageErrors);

			movementHeader.BM_PresentationDateTime = new DateTime(2023, 9, 23);
			AssertNotContains("future", expectedMessage, GetMessageSendingObjectParent().BizObjValidationMessageErrors);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
	}

	protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject() => new NctsHeaderMessageSendingObjectParent(nctsHeader);

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
}
