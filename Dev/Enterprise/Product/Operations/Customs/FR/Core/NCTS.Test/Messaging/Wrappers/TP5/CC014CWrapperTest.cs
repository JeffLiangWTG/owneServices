using System;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CC014CWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC014CWrapper>
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
			AssertEquals("TransitOperation should be mapped to NCTS Header.", "LRN001", Provider.TransitOperation.LRN);
		}

		public void TestCustomsOfficeOfDeparture()
		{
			AssertEquals("CustomsOfficeOfDeparture should be using CustomsOfficeWrapper.", "FR000000", Provider.CustomsOfficeOfDeparture.ReferenceNumber);
		}

		public void TestHolderOfTheTransitProcedure()
		{
			AssertEquals("HolderOfTheTransitProcedure should be using HolderOfTheTransitProcedureWrapper.", "FR987654321987654", Provider.HolderOfTheTransitProcedure.TIRHolderIdentificationNumber);
		}

		[TestDate(2024, 08, 29)]
		public void TestInvalidation()
		{
			AssertEquals("Invalidation should be using InvalidationWrapper.", new DateTime(2024, 08, 29), Provider.Invalidation.RequestDateAndTime);
			AssertEquals("Invalidation should be using InvalidationWrapper.", "This is the justification", Provider.Invalidation.Justification);
		}

		public void TestMessageEnveloppe()
		{
			AssertEquals("MessageEnveloppe should be using MessageEnveloppeWrapper - TransactionId.", "~CORRELATIONID~", Provider.MessageEnveloppe.TransactionId);
			AssertEquals("MessageEnveloppe should be using MessageEnveloppeWrapper - SchemaId.", "IE014", Provider.MessageEnveloppe.SchemaId);
		}

		protected override CC014CWrapper GetProvider()
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

			var destinationCustomsOffice = nctsHeader.MovementHeader.CustomsOffices.AddNew();
			destinationCustomsOffice.CY_Code = "DES";
			destinationCustomsOffice.CY_Data = "FR000001";

			var transitCustomsOffice1 = nctsHeader.MovementHeader.CustomsOffices.AddNew();
			transitCustomsOffice1.CY_Code = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			transitCustomsOffice1.CY_Data = "FR000002";
			transitCustomsOffice1.CY_Date = new DateTime(2024, 02, 02);
			var transitCustomsOffice2 = nctsHeader.MovementHeader.CustomsOffices.AddNew();
			transitCustomsOffice2.CY_Code = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			transitCustomsOffice2.CY_Data = "FR000003";
			transitCustomsOffice2.CY_Date = new DateTime(2024, 03, 03);

			var extCustomsOffice1 = nctsHeader.MovementHeader.CustomsOffices.AddNew();
			extCustomsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			extCustomsOffice1.CY_Data = "FR000004";
			var extCustomsOffice2 = nctsHeader.MovementHeader.CustomsOffices.AddNew();
			extCustomsOffice2.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			extCustomsOffice2.CY_Data = "FR000005";

			var authorisation1 = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
			authorisation1.AGC_Code = "AUT1";
			var authorisation2 = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
			authorisation2.AGC_Code = "AUT2";

			var principal = Factory.New<OrgHeader>();
			principal.OH_FullName = "SJ CORP";
			principal.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "98765432198765432111", Core.Constants.CountryCodes.France);
			nctsHeader.Principal.OrganisationPK = principal.PK;

			var sendingObject = new TP5MessageSendingObject(nctsHeader);
			sendingObject.JustificationCode = "1";
			sendingObject.Justification = "This is the justification";

			return CC014CWrapper.New(sendingObject);
		}
	}
}
