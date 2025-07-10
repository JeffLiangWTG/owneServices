using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Moq;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class CC007CSenderTest : NCTSMessageSenderTest<CC007CSender, ICC007C>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CC007CSender(null));
		}

		protected override string MovementType => NctsMovementType.Codes.Arrival;

		protected override ZString EntryType => NctsMessageTypeList.Codes.ArrivalNotification;

		protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.Arrival;

		protected override void SetUpMockProviderData(Mock<ICC007C> mockProvider)
		{
			var currentDateTime = new DateTime(2023, 01, 31, 15, 47, 45);
			mockProvider.Setup(m => m.MessageType).Returns(Constants.MessageTypes.CC007C);
			mockProvider.Setup(m => m.PreparationDateTime).Returns(currentDateTime);
			mockProvider.Setup(m => m.MessageRecipient).Returns("NCTS.BE");
			mockProvider.Setup(m => m.MessageIdentification).Returns("SENDERS REFERENCE PLACE HOLDER");
			mockProvider.Setup(m => m.MessageSender).Returns("CW1@");
			mockProvider.Setup(m => m.CorrelationIdentifier).Returns("CorrelationID");
			mockProvider.Setup(m => m.Consignment).Returns(Mock.Of<IConsignmentType01>(t =>
				t.LocationOfGoods == Mock.Of<ILocationOfGoods>(p =>
					p.AdditionalIdentifier == "AdditionalIdentifier" &&
					p.Address == Mock.Of<IAddress>(q =>
						q.City == "City" &&
						q.Country == "Country" &&
						q.Postcode == "Postcode" &&
						q.StreetAndNumber == "StreetAndNumber" &&
						q.StreetAndNumberMaxLength == 35) &&
					p.AuthorisationNumber == "AuthorizationNumber" &&
					p.ContactPerson == Mock.Of<IContactPerson>(q =>
						q.EMailAddress == "EmailAddress" &&
						q.Name == "Name" &&
						q.PhoneNumber == "PhoneNumber") &&
					p.CustomsOfficeReferenceNumber == "CustomsOfficeReferenceNumber" &&
					p.EconomicOperatorIdentificationNumber == "EconomicOperatorIdentificationNumber" &&
					p.GNSSLatitute == "GNSSLatitude" &&
					p.GNSSLongitude == "GNSSLongitude" &&
					p.PostCodeAddress == Mock.Of<IPostCodeAddress>(q =>
						q.HouseNumber == "HouseNumber" &&
						q.Country == "Country" &&
						q.Postcode == "Postcode") &&
					p.QualifierOfIdentification == "QualifierOfIdentification" &&
					p.TypeOfLocation == "TypeOfLocation" &&
					p.UNLocode == "UNLocode")));
			mockProvider.Setup(m => m.ArrivalNotificationDateAndTime).Returns(new DateTime(2023, 02, 24, 12, 55, 48));
			mockProvider.Setup(m => m.Discharge).Returns("Discharge");
			mockProvider.Setup(m => m.VoletPageNumber).Returns("VoletPageNumber");
			mockProvider.Setup(m => m.Authorisations).Returns(new List<IAuthorization> { Mock.Of<IAuthorization>(a => a.SequenceNumber == "1" && a.Type == "C524" && a.ReferenceNumber == "CPH") });
			mockProvider.Setup(m => m.CustomsOfficeOfDestination).Returns("DesID");
			mockProvider.Setup(m => m.IncidentFlag).Returns(true);
			mockProvider.Setup(m => m.MRN).Returns("MRN12345678");
			mockProvider.Setup(m => m.SimplifiedProcedure).Returns(true);
			mockProvider.Setup(m => m.TraderCommunicationLanguage).Returns("NL");
			mockProvider.Setup(m => m.TraderIdentificationNumber).Returns("TraderNumber");
		}
	}
}
