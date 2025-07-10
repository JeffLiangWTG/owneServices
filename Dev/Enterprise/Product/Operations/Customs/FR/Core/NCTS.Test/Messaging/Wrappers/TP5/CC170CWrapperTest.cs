using System;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CC170CWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC170CWrapper>
	{
		public void TestMessageSender()
		{
			AssertEquals("MessageSender is mapped.", FRConstants.NCTSMessage.Operator, Provider.MessageSender);
		}

		public void TestMessageRecipient()
		{
			AssertEquals("MessageRecipient is mapped.", FRConstants.NCTSMessage.NationalAdministration, Provider.MessageRecipient);
		}

		[TestDate(2024, 02, 09, 12, 11, 10)]
		public void TestPreparationDateAndTime()
		{
			AssertEquals("PreparationDateAndTime should equal DateTime.UtcNow", new DateTime(2024, 02, 09, 12, 11, 10), Provider.PreparationDateAndTime);
		}

		public void TestCorrelationIdentifier()
		{
			AssertEquals("CorrelationIdentifier should be empty", ZString.Empty, Provider.CorrelationIdentifier);
		}

		public void TestTransitOperation()
		{
			AssertEquals("TransitOperation should be mapped to NCTS Header.", "LRN001", Provider.TransitOperation.LRN);
		}

		public void TestCustomsOfficeOfDeparture()
		{
			AssertEquals("CustomsOfficeOfDeparture should be using CustomsOfficeWrapper.", "FR000000", Provider.CustomsOfficeOfDeparture.ReferenceNumber);
		}

		public void TestHolderOfTheTransitProcedure()
		{
			AssertEquals("HolderOfTheTransitProcedure should be using HolderOfTheTransitProcedureWrapper.", "FR111234567891234", Provider.HolderOfTheTransitProcedure.TIRHolderIdentificationNumber);
		}

		public void TestRepresentative()
		{
			AssertEquals("Representative should be using RepresentativeWrapper.", "GF CORP", Provider.Representative.Name);
		}

		public void TestConsignment()
		{
			AssertEquals("Consignment should be using ConsignmentWrapper.", "TR", Provider.Consignment.CountryOfDispatch);
		}

		public void TestMessageEnveloppe()
		{
			AssertEquals("MessageEnveloppe should be using MessageEnveloppeWrapper - TransactionId.", "~CORRELATIONID~", Provider.MessageEnveloppe.TransactionId);
			AssertEquals("MessageEnveloppe should be using MessageEnveloppeWrapper - SchemaId.", "IE170", Provider.MessageEnveloppe.SchemaId);
		}

		protected override CC170CWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.LocalReferenceNumber = "LRN001";
			nctsHeader.BH_JobReference = "MRN0121";

			var movementheader = nctsHeader.MovementHeader;
			movementheader.BM_RN_NKCountryOfDispatch = "TR";

			nctsHeader.MovementHeader.CustomsOffices.RemoveAndDeleteAll();
			var departureCustomsOffice = nctsHeader.MovementHeader.CustomsOffices.AddNew();
			departureCustomsOffice.CY_Code = "DEP";
			departureCustomsOffice.CY_Data = "FR000000";

			var principal = Factory.New<OrgHeader>();
			principal.OH_FullName = "SJ CORP";
			principal.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "98765432198765432111", Core.Constants.CountryCodes.France);
			nctsHeader.Principal.OrganisationPK = principal.PK;

			var representative = Factory.New<OrgHeader>();
			representative.OH_FullName = "GF CORP";
			representative.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "11123456789123456789", Core.Constants.CountryCodes.France);
			movementheader.Representative.OrganisationPK = representative.PK;

			return CC170CWrapper.New(new TP5MessageSendingObject(nctsHeader));
		}
	}
}
