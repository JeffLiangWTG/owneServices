using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class NctsPhase5SendingMessageAuthorizerTest : TestCaseWithFactory
{
	public void TestGuardClause()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when parameter is null", () => new NctsPhase5SendingMessageAuthorizer(null));
			AssertExceptionThrown<ArgumentNullException>("Exception expected when MovementHeader parameter is null", () => new NctsPhase5SendingMessageAuthorizer(Factory.New<NctsHeader>()));
		});
	}

	public void TestGetAllowedSendingMessageTypes_ForNewDeclaration()
	{
		CombineAssertions(() =>
		{
			header.MovementHeader.BM_Phase = "015";
			AssertAllowedSendingMessageTypesContains("When Phase is not 013", "NEW");

			header.MovementHeader.BM_Phase = "013";
			AssertAllowedSendingMessageTypesNotContain("When Phase is 013", "NEW");
		});
	}

		public void TestGetAllowedSendingMessageTypes_ForCancellation()
		{
			CombineAssertions(() =>
			{
				header.MovementReferenceEntryNumber.CE_EntryNum = "MRN";
				var movementHeader = header.MovementHeader;
				movementHeader.BM_CustomsStatus = "MRN";
				movementHeader.BM_MessageStatus = "ACC";
				movementHeader.BM_Phase = "015";
				AssertAllowedSendingMessageTypesContains("MRN - ACC - 015", "CAN");

				movementHeader.BM_CustomsStatus = "REL";
				movementHeader.BM_MessageStatus = "ACC";
				movementHeader.BM_Phase = "015";
				AssertAllowedSendingMessageTypesContains("REL - ACC - 015", "CAN");

				movementHeader.BM_CustomsStatus = "NRL";
				movementHeader.BM_MessageStatus = "ACC";
				movementHeader.BM_Phase = "015";
				AssertAllowedSendingMessageTypesContains("NRL - ACC - 015", "CAN");

				movementHeader.BM_CustomsStatus = "CO3";
				movementHeader.BM_MessageStatus = "";
				movementHeader.BM_Phase = "015";
				AssertAllowedSendingMessageTypesContains("CO3 - empty - 015", "CAN");

				movementHeader.BM_CustomsStatus = "DEP";
				movementHeader.BM_MessageStatus = "";
				movementHeader.BM_Phase = "015";
				AssertAllowedSendingMessageTypesContains("DEP - Empty - 015", "CAN");

				movementHeader.BM_CustomsStatus = "WRO";
				movementHeader.BM_MessageStatus = "ACC";
				movementHeader.BM_Phase = "015";
				AssertAllowedSendingMessageTypesContains("WRO - ACC - 015", "CAN");

				movementHeader.BM_CustomsStatus = "";
				movementHeader.BM_MessageStatus = "FAL";
				movementHeader.BM_Phase = "014";
				AssertAllowedSendingMessageTypesContains("empty - FAL - 014", "CAN");

				movementHeader.BM_CustomsStatus = "";
				movementHeader.BM_MessageStatus = "ERR";
				movementHeader.BM_Phase = "014";
				AssertAllowedSendingMessageTypesContains("empty - ERR - 014", "CAN");

				movementHeader.BM_CustomsStatus = "";
				movementHeader.BM_MessageStatus = "";
				movementHeader.BM_Phase = "013";
				AssertAllowedSendingMessageTypesContains("empty - emtpy - 013", "CAN");

				movementHeader.BM_CustomsStatus = "";
				movementHeader.BM_MessageStatus = "FAL";
				movementHeader.BM_Phase = "013";
				AssertAllowedSendingMessageTypesContains("empty - FAL - 013", "CAN");

				movementHeader.BM_CustomsStatus = "";
				movementHeader.BM_MessageStatus = "ERR";
				movementHeader.BM_Phase = "013";
				AssertAllowedSendingMessageTypesContains("empty - ERR - 013", "CAN");

				movementHeader.BM_CustomsStatus = "ACS";
				movementHeader.BM_MessageStatus = "ACC";
				movementHeader.BM_Phase = "013";
				AssertAllowedSendingMessageTypesContains("ACS - ACC - 013", "CAN");

			header.MovementReferenceEntryNumber.CE_EntryNum = "";
			AssertAllowedSendingMessageTypesNotContain("When Ncts does not have the MRN", "CAN");

			movementHeader.BM_Phase = "014";
			movementHeader.BM_MessageStatus = "ACS";
			AssertAllowedSendingMessageTypesNotContain("When Ncts does not have the MRN", "CAN");
		});
	}

	public void TestGetAllowedSendingMessageTypes_ForAmendment()
	{
		CombineAssertions(() =>
		{
			header.MovementHeader.BM_Phase = "013";
			AssertAllowedSendingMessageTypesContains("When Phase is 013", "AMD");

			var movementHeader = header.MovementHeader;
			movementHeader.BM_MessageStatus = "SNT";
			AssertAllowedSendingMessageTypesNotContain("When MessageStatus is SNT", "CAN");

			movementHeader.BM_MessageStatus = "ERR";
			AssertAllowedSendingMessageTypesContains("When MessageStatus is ERR", "AMD");
		});
	}

	public void TestGetAllowedSendingMessageTypes()
	{
		CombineAssertions(() =>
		{
			header.MovementHeader.BM_Phase = "015";

			AssertArrayEqualsByElements("When Phase is 015",
				new[] { "NEW" },
				GetActualAllowedSendingMessageTypes());

			header.MovementReferenceEntryNumber.CE_EntryNum = "MRN";
			var movementHeader = header.MovementHeader;
			movementHeader.BM_CustomsStatus = "REL";
			movementHeader.BM_MessageStatus = "ACC";
			movementHeader.BM_Phase = "015";

			AssertArrayEqualsByElements("When Phase is 015",
				new[] { "NEW", "CAN" },
				GetActualAllowedSendingMessageTypes());

			movementHeader.BM_CustomsStatus = "";
			movementHeader.BM_MessageStatus = "";
			movementHeader.BM_Phase = "013";
			AssertArrayEqualsByElements("When Phase is 013",
				new[] { "CAN", "AMD" },
				GetActualAllowedSendingMessageTypes());

			movementHeader.BM_MessageStatus = "SNT";
			AssertArrayEqualsByElements("When message is in sending status",
				Array.Empty<string>(),
				GetActualAllowedSendingMessageTypes());
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.NewDepartureNctsHeader();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
	}

	void AssertAllowedSendingMessageTypesContains(string assertionMessage, string expectedMessageType)
	{
		AssertCollectionContains(assertionMessage + ", Allowed sending message types",
			expectedMessageType,
			GetActualAllowedSendingMessageTypes());
	}

	void AssertAllowedSendingMessageTypesNotContain(string assertionMessage, string expectedMessageType)
	{
		AssertCollectionNotContains(assertionMessage + ", Allowed sending message types",
			new[] { expectedMessageType },
			GetActualAllowedSendingMessageTypes());
	}

	string[] GetActualAllowedSendingMessageTypes()
		=> new NctsPhase5SendingMessageAuthorizer(header).GetAllowedSendingMessageTypes().ToArray();

	NctsHeader header;
}
