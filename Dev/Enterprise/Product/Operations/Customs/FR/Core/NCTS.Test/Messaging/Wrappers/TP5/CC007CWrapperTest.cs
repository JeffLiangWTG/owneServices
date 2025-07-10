using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CC007CWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC007CWrapper>
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

		public void TestTransitOperation()
		{
			AssertEquals("TransitOperation should be mapped to NCTS Header.", "MRN001", Provider.TransitOperation.MRN);
		}

		public void TestAuthorisation()
		{
			AssertContainsExactElementsInAnyOrder("Authorisation should be using MovementAuthorisationWrapper.", new string[] { "AUT1" }, Provider.Authorisation.Select(x => x.ReferenceNumber));
		}

		public void TestCustomsOfficeOfDestinationActual()
		{
			AssertEquals("CustomsOfficeOfDestinationActual should be using CustomsOfficeWrapper.", "FR000001", Provider.CustomsOfficeOfDestinationActual.ReferenceNumber);
		}

		public void TestTraderAtDestination()
		{
			AssertContainsExactElementsInAnyOrder("TraderAtDestination should be using TraderWrapper.", "FR" , Provider.TraderAtDestination.CommunicationLanguageAtDestination);
		}

		public void TestConsignment()
		{
			AssertEquals("Consignment should be using ArrivalConsignmentWrapper.", 2, Provider.Consignment.Incident.Count);
		}

		public void TestMessageEnveloppe()
		{
			AssertEquals("MessageEnveloppe should be using MessageEnveloppeWrapper - TransactionId.", "~CORRELATIONID~", Provider.MessageEnveloppe.TransactionId);
			AssertEquals("MessageEnveloppe should be using MessageEnveloppeWrapper - SchemaId.", "IE007", Provider.MessageEnveloppe.SchemaId);
		}

		protected override CC007CWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeader.BH_JobReference = "MRN0121";

			var destinationTrader = Factory.New<OrgHeader>();
			destinationTrader.OH_FullName = "BN CORP";
			destinationTrader.OH_Language = "FR";
			nctsHeader.DestinationTrader.E2_OA_Address = destinationTrader.MainAddress.PK;

			var movementheader = nctsHeader.ArrivalMovementHeader;
			movementheader.BM_RN_NKCountryOfDispatch = "TR";
			movementheader.AuthorizationCode = "code";
			movementheader.AuthorizationNumber = "AUT1";

			var office = movementheader.CustomsOffices.AddNew();
			office.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
			office.CY_Data = "FR000001";

			movementheader.DestinationCustomsOfficeCodeForArrival = "FR000001";

			var mrn = CusEntryNumber.LoadOrCreate(nctsHeader, "MRN", Core.Constants.CountryCodes.France);
			mrn.CE_EntryNum = "MRN001";

			nctsHeader.EnRouteIncidents.AddNew();
			nctsHeader.EnRouteIncidents.AddNew();

			return CC007CWrapper.New(nctsHeader);
		}
	}
}
