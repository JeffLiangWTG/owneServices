using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.FR.Business.NCTS.NctsHeader;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CC034CWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC034CWrapper>
	{
		public void TestMessageSender()
		{
			AssertEquals("MessageSender should be OPE.FR", FRConstants.NCTSMessage.Operator, Provider.MessageSender);
		}

		public void TestMessageRecipient()
		{
			AssertEquals("MessageRecipient should be NTA.FR", FRConstants.NCTSMessage.NationalAdministration, Provider.MessageRecipient);
		}

		[TestDate(2024, 02, 09, 12, 11, 10)]
		public void TestPreparationDateAndTime()
		{
			AssertEquals("PreparationDateAndTime should equal DateTime.Now", new DateTime(2024, 02, 09, 12, 11, 10), Provider.PreparationDateAndTime);
		}

		public void TestCorrelationIdentifier()
		{
			AssertEquals("CorrelationIdentifier should be empty", ZString.Empty, Provider.CorrelationIdentifier);
		}

		public void TestMessageEnveloppe()
		{
			AssertEquals("MessageEnveloppe should be using MessageEnveloppeWrapper - TransactionId.", "~CORRELATIONID~", Provider.MessageEnveloppe.TransactionId);
			AssertEquals("MessageEnveloppe should be using MessageEnveloppeWrapper - SchemaId.", "IE034", Provider.MessageEnveloppe.SchemaId);
		}

		public void TestRequester()
		{
			AssertNull("Requester should be null when both Representative and Principal are empty.", Provider.Requester);

			nctsHeader.Principal.OrganisationPK = GetOrganisationPKWithEORICode("0001");
			sendingObject.RequesterRole = "Role1";
			AssertEquals("Requester IdentificationNumber should be mapped to Principal EORI when only Principal is filled.", "FR0001", Provider.Requester.IdentificationNumber);
			AssertEquals("Requester Role should be mapped to sendingObject RequesterRole.", "Role1", Provider.Requester.Role);

			nctsHeader.MovementHeader.Representative.OrganisationPK = GetOrganisationPKWithEORICode("0002");
			var newProvider = CC034CWrapper.New(sendingObject);
			AssertEquals("Requester IdentificationNumber should be mapped to Representative EORI when both Representative and Principal are filled.", "FR0002", newProvider.Requester.IdentificationNumber);
		}

		public void TestGuaranteeReference()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondNumber = "GRN1";
			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondNumber = "GRN2";

			AssertContainsExactElementsInAnyOrder("GuaranteeReference should be using GuaranteeReferenceForQueryWrapper.", new string[] { "GRN1", "GRN2" }, Provider.GuaranteeReference.Select(x => x.Grn));
		}

		ZGuid GetOrganisationPKWithEORICode(string code)
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			orgCusCode.OK_CustomsRegNo = code;
			return orgHeader.PK;
		}

		protected override CC034CWrapper GetProvider()
		{
			if (sendingObject == null)
			{
				if (nctsHeader == null)
				{
					nctsHeader = Factory.New<NctsHeader>();
				}
				nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
				sendingObject = new TP5MessageSendingObject(nctsHeader);
			}

			return CC034CWrapper.New(sendingObject);
		}

		NctsHeader nctsHeader;
		TP5MessageSendingObject sendingObject;
	}
}
