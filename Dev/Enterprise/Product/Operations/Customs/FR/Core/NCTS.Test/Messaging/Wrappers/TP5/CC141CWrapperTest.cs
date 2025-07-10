using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CC141CWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC141CWrapper>
	{
		public void TestMessageSender()
		{
			AssertEquals("MessageSender should be OPE.FR", FRConstants.NCTSMessage.Operator, Provider.MessageSender);
		}

		public void TestMessageRecipient()
		{
			AssertEquals("MessageRecipient should be NTA.FR", FRConstants.NCTSMessage.NationalAdministration, Provider.MessageRecipient);
		}

		[TestDate(2024, 08, 30)]
		public void TestPreparationDateAndTime()
		{
			AssertEquals("PreparationDateAndTime should be equal to DateTime.UtcNow", new DateTime(2024, 08, 30), Provider.PreparationDateAndTime);
		}

		public void TestCorrelationIdentifier()
		{
			AssertEquals("CorrelationIdentifier should be empty", ZString.Empty, Provider.CorrelationIdentifier);
		}

		public void TestTransitOperation()
		{
			AssertEquals("TransitOperation should be mapped to NCTS Header.", "LRN001", Provider.TransitOperation.LRN);
		}

		public void TestCustomsOfficeOfDestinationActual()
		{
			AssertEquals("CustomsOfficeOfDestinationActual should be equal to FR101000", "FR101000", Provider.CustomsOfficeOfDestinationActual.ReferenceNumber);
		}

		public void TestCustomsOfficeOfEnquiryAtDeparture()
		{
			AssertEquals("CustomsOfficeOfEnquiryAtDeparture should be mapped to NCTS Header's Departure office", "FR000000", Provider.CustomsOfficeOfEnquiryAtDeparture.ReferenceNumber);
		}

		public void TestConsignment()
		{
			AssertEquals("Consignee should be using actualConsignee in OrganizationWrapper.", "AK CORP, FR12345678900001", $"{Provider.Consignment.Consignee.Name}, {Provider.Consignment.Consignee.IdentificationNumber}");
		}

		public void TestMessageEnveloppe()
		{
			AssertEquals("MessageEnveloppe should be using MessageEnveloppeWrapper - TransactionId.", "~CORRELATIONID~", Provider.MessageEnveloppe.TransactionId);
			AssertEquals("MessageEnveloppe should be using MessageEnveloppeWrapper - SchemaId.", "IE141", Provider.MessageEnveloppe.SchemaId);
		}

		public void TestEnquiry()
		{
			AssertEquals("Enquiry TC11DeliveryDate should be equal to DateTime(2024, 08, 29)", new DateTime(2024, 08, 29), Provider.Enquiry.TC11DeliveryDate);
			AssertEquals("Enquiry Text should be equal to queryinformation", "queryinformation", Provider.Enquiry.Text);
		}

		public void TestHolderOfTheTransitProcedure()
		{
			AssertEquals("HolderOfTheTransitProcedure should be using HolderOfTheTransitProcedureWrapper.", "FR987654321987654", Provider.HolderOfTheTransitProcedure.TIRHolderIdentificationNumber);
		}

		protected override CC141CWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.LocalReferenceNumber = "LRN001";
			nctsHeader.BH_JobReference = "MRN0121";

			var movementheader = nctsHeader.MovementHeader;
			movementheader.BM_RN_NKCountryOfDispatch = "TR";

			nctsHeader.MovementHeader.CustomsOffices.RemoveAndDeleteAll();
			var customsOfficeOfEnquiryAtDeparture = nctsHeader.MovementHeader.CustomsOffices.AddNew();
			customsOfficeOfEnquiryAtDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			customsOfficeOfEnquiryAtDeparture.CY_Data = "FR000000";

			var principal = Factory.New<OrgHeader>();
			principal.OH_FullName = "SJ CORP";
			principal.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "98765432198765432111", Core.Constants.CountryCodes.France);
			nctsHeader.Principal.OrganisationPK = principal.PK;

			var sendingObject = new TP5MessageSendingObject(nctsHeader);
			sendingObject.TC11DeliveryDate = new DateTime(2024, 08, 29);
			sendingObject.QueryInformation = "queryinformation";
			sendingObject.ActualOfficeOfDestination = "FR101000";

			var actualConsignee = Factory.New<OrgHeader>();
			actualConsignee.OH_FullName = "AK CORP";
			actualConsignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			actualConsignee.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			sendingObject.ActualConsignee.OrganisationPK = actualConsignee.PK;

			return CC141CWrapper.New(sendingObject);
		}
	}
}
