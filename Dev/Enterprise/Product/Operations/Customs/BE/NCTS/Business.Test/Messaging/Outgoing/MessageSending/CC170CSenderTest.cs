using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Moq;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class CC170CSenderTest : NCTSMessageSenderTest<CC170CSender, ICC170C>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CC170CSender(null));
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override ZString EntryType => NctsMessageTypeList.Codes.PresentationNotification;

		protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.Presentation;

		protected override ZString ParentTableName => Customs.Business.AutoCusInBondMoveHeader.Schema.TableName;

		protected override void SetUpMockProviderData(Mock<ICC170C> mockProvider)
		{
			var currentDateTime = new DateTime(2023, 01, 31, 15, 47, 45);
			mockProvider.Setup(m => m.MessageType).Returns(Constants.MessageTypes.CC170C);
			mockProvider.Setup(m => m.PreparationDateTime).Returns(currentDateTime);
			mockProvider.Setup(m => m.MessageRecipient).Returns("NCTS.BE");
			mockProvider.Setup(m => m.MessageIdentification).Returns("SENDERS REFERENCE PLACE HOLDER");
			mockProvider.Setup(m => m.MessageSender).Returns("CW1@");

			mockProvider.Setup(m => m.CorrelationIdentifier).Returns("CorrelationID");
			mockProvider.Setup(m => m.CustomsOfficeOfDeparture).Returns("DepId");
			mockProvider.Setup(m => m.Consignment).Returns(Mock.Of<IConsignmentType08>(t =>
				t.ActiveBorderTransportMeans == new List<IActiveBorderTransportMeans> { Mock.Of<IActiveBorderTransportMeans>(p =>
					p.ConveyanceReferenceNumber == "ConveyanceReferenceNumber" &&
					p.CustomsOfficeAtBorderReferenceNumber == "CustomsOfficeAtBorderReferenceNumber" &&
					p.IdentificationNumber == "IdentificationNumber" &&
					p.Nationality == "Nationality" &&
					p.SequenceNumber == 1 &&
					p.TypeOfIdentification == 2) } &&
				t.ContainerIndicator &&
				t.DepartureTransportMeans == new List<IDepartureTransportMeans> { Mock.Of<IDepartureTransportMeans>(p =>
					p.SequenceNumber == 3 &&
					p.TypeOfIdentification == 4 &&
					p.Nationality == "Nationality" &&
					p.IdentificationNumber == "IdentificationNumber") } &&
				t.HouseConsignments == new List<IHouseConsignmentType06> { Mock.Of<IHouseConsignmentType06>(p =>
					p.DepartureTransportMeans == new List<IDepartureTransportMeans> { Mock.Of<IDepartureTransportMeans>(q =>
						q.IdentificationNumber == "IdentificationNumber" &&
						q.Nationality == "Nationality" &&
						q.TypeOfIdentification == 5 &&
						q.SequenceNumber == 6) } &&
					p.SequenceNumber == 7) } &&
				t.InlandModeOfTransport == 8 &&
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
					p.UNLocode == "UNLocode") &&
				t.ModeOfTransportAtTheBorder == 9 &&
				t.PlaceOfLoading == Mock.Of<IPlace>(p =>
					p.UnLocode == "UNLocode" &&
					p.Country == "Country" &&
					p.Location == "Location" &&
					p.UnLocode == "UNLocode") &&
				t.TransportEquipments == new List<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment> { Mock.Of<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment>(p =>
					p.ContainerIdentificationNumber == "ContainerIdentificationNumber" &&
					p.GoodsReferences == new List<IGoodsReference> { Mock.Of<IGoodsReference>(q =>
						q.SequenceNumber == 10 &&
						q.DeclarationGoodsItemNumber == 11) } &&
					p.NumberOfSeals == 12 &&
					p.Seals == new List<ISeal> { Mock.Of<ISeal>(q =>
						q.Identifier == "Identifier" &&
						q.SequenceNumber == 13) } &&
					p.SequenceNumber == 14) }));
			mockProvider.Setup(m => m.HolderOfTheTransitProcedureIdentificationNumber).Returns("HolderOfTheTransitProcedureIdentificationNumber");
			mockProvider.Setup(m => m.HolderOfTheTransitProcedureTIRNumber).Returns("HolderOfTheTransitProcedureTIRNumber");
			mockProvider.Setup(m => m.LimitDate).Returns(currentDateTime);
			mockProvider.Setup(m => m.LRN).Returns("LRN");
			mockProvider.Setup(m => m.ReducedDatasetIndicator).Returns(true);
			mockProvider.Setup(m => m.Representative).Returns(Mock.Of<INCTSRepresentative>(t =>
				t.Address == Mock.Of<IAddress>(m =>
					m.StreetAndNumber == "StreetAndNumber" &&
					m.City == "City" &&
					m.Country == "Country" &&
					m.Postcode == "Postcode" &&
					m.StreetAndNumberMaxLength == 35) &&
				t.ContactPerson == Mock.Of<IContactPerson>(m =>
					m.EMailAddress == "EmailAddress" &&
					m.Name == "Name" &&
					m.PhoneNumber == "PhoneNumber") &&
				t.IdentificationNumber == "IdentificationNumber" &&
				t.Name == "Name" &&
				t.Status == 15 &&
				t.NameMaxlength == 35));
		}
	}
}
