using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PresentationNotificationSenderProviderTest : AESMessageSenderTest<PresentationNotificationSenderProvider, ICC511CDataProvider>
{
	protected override ZString EntryType => BEExportEntryTypeList.Codes.PresentationNotification;

	protected override ZString EntryStatus => StatusCodes.Presented;

	protected override void SetUpMockProviderData(Mock<ICC511CDataProvider> mockProvider)
	{
		var currentDateTime = new System.DateTime(2023, 01, 31, 15, 47, 45);
		mockProvider.Setup(m => m.PreparationDateTime).Returns(currentDateTime);
		mockProvider.Setup(m => m.MessageType).Returns("CC511C");
		mockProvider.Setup(m => m.LanguageCode).Returns("BE");
		mockProvider.Setup(m => m.CustomsOfficeOfPresentationReferenceNumber).Returns("PresentationReference");
		mockProvider.Setup(m => m.CustomsOfficeOfExportReferenceNumber).Returns("ExportReference");
		mockProvider.Setup(m => m.PreparationDateTime).Returns(currentDateTime);

		mockProvider.Setup(m => m.ExportOperation).Returns(Mock.Of<IExportOperation>(e =>
			e.LRN == "lrn" &&
			e.MRN == "mrn" &&
			e.InvalidationRequestDateTime == currentDateTime &&
			e.InvalidationReason == "reason" &&
			e.DeclarationType == "A" &&
			e.AdditionalDeclarationType == "B" &&
			e.PresentationOfTheGoodsDateAndTime == currentDateTime &&
			e.Security == "NON" &&
			e.SpecificCircumstanceIndicator == "1" &&
			e.TotalAmountInvoiced == 10 &&
			e.InvoiceCurrency == "EUR"
		));
		mockProvider.Setup(m => m.Declarant).Returns(Mock.Of<IParty>(p =>
			p.Name == "Declarant" &&
			p.NameMaxlength == 35 &&
			p.IdentificationNumber == "ID" &&
			p.ContactPerson == Mock.Of<IContactPerson>(c =>
				c.Name == "person" &&
				c.PhoneNumber == "123456" &&
				c.EMailAddress == "person@email") &&
			p.Address == Mock.Of<IAddress>(a =>
				a.StreetAndNumber == "StreetAndNumber" &&
				a.StreetAndNumberMaxLength == 35 &&
				a.Postcode == "1000" &&
				a.Country == Core.Constants.CountryCodes.Belgium &&
				a.City == "Brussels")
		));
		mockProvider.Setup(m => m.Representative).Returns(Mock.Of<IAESRepresentative>(r =>
			r.Status == "2" &&
			r.Name == "Representative" &&
			r.IdentificationNumber == "ID" &&
			r.ContactPerson == Mock.Of<IContactPerson>(c =>
				c.Name == "person" &&
				c.PhoneNumber == "123456" &&
				c.EMailAddress == "person@email") &&
			r.Address == Mock.Of<IAddress>(a =>
				a.StreetAndNumber == "StreetAndNumber" &&
				a.StreetAndNumberMaxLength == 35 &&
				a.Postcode == "1000" &&
				a.Country == Core.Constants.CountryCodes.Belgium &&
				a.City == "Brussels")
		));
		mockProvider.Setup(m => m.Consignment).Returns(Mock.Of<IConsignment>(c =>
			c.ContainerIndicator &&
			c.InlandModeOfTransport == "40" &&
			c.LocationOfGoods == Mock.Of<ILocationOfGoods>(l =>
				l.AdditionalIdentifier == "AdditionalIdentifier" &&
				l.Address == Mock.Of<IAddress>(q =>
					q.City == "City" &&
					q.Country == "Country" &&
					q.Postcode == "Postcode" &&
					q.StreetAndNumber == "StreetAndNumber" &&
					q.StreetAndNumberMaxLength == 35) &&
				l.AuthorisationNumber == "AuthorizationNumber" &&
				l.ContactPerson == Mock.Of<IContactPerson>(q =>
					q.EMailAddress == "EmailAddress" &&
					q.Name == "Name" &&
					q.PhoneNumber == "PhoneNumber") &&
				l.CustomsOfficeReferenceNumber == "CustomsOfficeReferenceNumber" &&
				l.EconomicOperatorIdentificationNumber == "EconomicOperatorIdentificationNumber" &&
				l.GNSSLatitute == "GNSSLatitude" &&
				l.GNSSLongitude == "GNSSLongitude" &&
				l.PostCodeAddress == Mock.Of<IPostCodeAddress>(q =>
					q.HouseNumber == "HouseNumber" &&
					q.Country == "Country" &&
					q.Postcode == "Postcode") &&
				l.QualifierOfIdentification == "QualifierOfIdentification" &&
				l.TypeOfLocation == "TypeOfLocation" &&
				l.UNLocode == "UNLocode") &&
			c.TransportEquipments == new List<ITransportEquipment> { Mock.Of<ITransportEquipment>(t =>
				t.SequenceNumber == 2 &&
				t.ContainerIdentificationNumber == "ID" &&
				t.NumberOfSeals == 1 &&
				t.Seals == new List<ISeal> { Mock.Of<ISeal>(s =>
					s.SequenceNumber == 3 &&
					s.Identifier == "seal"
				) } &&
				t.GoodsReferences == new List<IGoodsReference> { Mock.Of<IGoodsReference>(g =>
					g.SequenceNumber == 4 &&
					g.DeclarationGoodsItemNumber == 5
				) }
			) } &&
			c.DepartureTransportMeans == new List<IDepartureTransportMeans> { Mock.Of<IDepartureTransportMeans>(d =>
				d.SequenceNumber == 6 &&
				d.TypeOfIdentification == 7 &&
				d.Nationality == "NL" &&
				d.IdentificationNumber == "DTM"
			) }
		));
	}
}
