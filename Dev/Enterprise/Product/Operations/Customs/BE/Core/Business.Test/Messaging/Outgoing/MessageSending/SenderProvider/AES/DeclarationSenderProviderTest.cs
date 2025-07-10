using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class DeclarationSenderProviderTest : AESMessageSenderTest<DeclarationSenderProvider, ICC515CDataProvider>
{
	protected override ZString EntryType => BEExportEntryTypeList.Codes.ExportDeclaration;

	protected override ZString EntryStatus => ZString.Empty;

	protected override void SetUpMockProviderData(Mock<ICC515CDataProvider> mockProvider)
	{
		var currentDateTime = new System.DateTime(2023, 03, 09, 15, 24, 40);
		mockProvider.Setup(m => m.Authorizations).Returns(new List<IAuthorization> { Mock.Of<IAuthorization>(az =>
			az.SequenceNumber == "1" &&
			az.HolderOfAuthorisation == "HolderOfAuthorisation" &&
			az.ReferenceNumber == "ReferenceNumber" &&
			az.Type == "Type" ) });
		mockProvider.Setup(m => m.CorrelationIdentifier).Returns("CorrelationIdentifier");
		mockProvider.Setup(m => m.CurrencyExchange).Returns(Mock.Of<ICurrencyExchange>(ce =>
			ce.ExchangeRate == 1 &&
			ce.InternalCurrencyUnit == "EUR"));
		mockProvider.Setup(m => m.CustomsOfficeOfPresentationReferenceNumber).Returns("CustomsOfficeOfPresentationReferenceNumber");
		mockProvider.Setup(m => m.CustomsOfficeOfExportReferenceNumber).Returns("CustomsOfficeOfExportReferenceNumber");
		mockProvider.Setup(m => m.CustomsOfficeOfExitDeclaredReferenceNumber).Returns("CustomsOfficeOfExitDeclaredReferenceNumber");
		mockProvider.Setup(m => m.SupervisingCustomsOfficeReferenceNumber).Returns("SupervisingCustomsOfficeReferenceNumber");
		mockProvider.Setup(m => m.Declarant).Returns(Mock.Of<IParty>(dt =>
			dt.Address == Mock.Of<IAddress>(ad =>
				ad.Country == "Country" &&
				ad.City == "City" &&
				ad.Postcode == "Postcode" &&
				ad.StreetAndNumber == "StreetAndNumber" &&
				ad.StreetAndNumberMaxLength == 35) &&
			dt.ContactPerson == Mock.Of<IContactPerson>(cp =>
				cp.EMailAddress == "EMailAddress" &&
				cp.Name == "Name" &&
				cp.PhoneNumber == "PhoneNumber") &&
			dt.IdentificationNumber == "IdentificationNumber" &&
			dt.Name == "Name" &&
			dt.NameMaxlength == 35));
		mockProvider.Setup(m => m.DeferredPayment).Returns("DeferredPayment");
		mockProvider.Setup(m => m.Exporter).Returns(Mock.Of<IParty>(er =>
			er.Address == Mock.Of<IAddress>(ad =>
				ad.Country == "Country" &&
				ad.City == "City" &&
				ad.Postcode == "Postcode" &&
				ad.StreetAndNumber == "StreetAndNumber" &&
				ad.StreetAndNumberMaxLength == 35) &&
			er.ContactPerson == Mock.Of<IContactPerson>(cp =>
				cp.EMailAddress == "EMailAddress" &&
				cp.Name == "Name" &&
				cp.PhoneNumber == "PhoneNumber") &&
			er.IdentificationNumber == "IdentificationNumber" &&
			er.Name == "Name" &&
			er.NameMaxlength == 35));
		mockProvider.Setup(m => m.ExportOperation).Returns(Mock.Of<IExportOperation>(eo =>
			eo.AdditionalDeclarationType == "ADType" &&
			eo.DeclarationType == "DeclarationType" &&
			eo.InvalidationRequestDateTime == currentDateTime &&
			eo.InvalidationReason == "Reason" &&
			eo.InvoiceCurrency == "EUR" &&
			eo.LRN == "LRN" &&
			eo.MRN == "MRN" &&
			eo.PresentationOfTheGoodsDateAndTime == currentDateTime &&
			eo.Security == "Security" &&
			eo.SpecificCircumstanceIndicator == "SpecificCircumstanceIncicator" &&
			eo.TotalAmountInvoiced == 100));
		mockProvider.Setup(m => m.GoodsShipment).Returns(Mock.Of<IGoodsShipment>(gs =>
			gs.AdditionalInformations == new List<IAdditionalInformation> { Mock.Of<IAdditionalInformation>(ai =>
				ai.SequenceNumber == 2 &&
				ai.Code == "Code" &&
				ai.Text == "Text") } &&
			gs.AdditionalReferences == new List<IDocument> { Mock.Of<IDocument>(ar =>
				ar.SequenceNumber == 3 &&
				ar.ReferenceNumber == "ReferenceNumber" &&
				ar.Type == "Type") } &&
			gs.AdditionalSupplyChainActors == new List<IAdditionalSupplyChainActor> { Mock.Of<IAdditionalSupplyChainActor>(asc =>
				asc.SequenceNumber == 4 &&
				asc.IdentificationNumber == "IdentificationNumber" &&
				asc.Role == "Role") } &&
			gs.Consignment == Mock.Of<IConsignment>(ct =>
				ct.ActiveBorderTransportMeans == Mock.Of<IActiveBorderTransportMeans>(ab =>
					ab.IdentificationNumber == "IdentificationNumber" &&
					ab.Nationality == "Nationality" &&
					ab.TypeOfIdentification == 1) &&
				ct.CarrierIdentificationNumber == "CarrierIdentificationNumber" &&
				ct.ContainerIndicator &&
				ct.Consignee == Mock.Of<IParty>(ce =>
					ce.Address == Mock.Of<IAddress>(ad =>
						ad.Country == "Country" &&
						ad.City == "City" &&
						ad.Postcode == "Postcode" &&
						ad.StreetAndNumber == "StreetAndNumber" &&
						ad.StreetAndNumberMaxLength == 35) &&
					ce.ContactPerson == Mock.Of<IContactPerson>(cp =>
						cp.EMailAddress == "EMailAddress" &&
						cp.Name == "Name" &&
						cp.PhoneNumber == "PhoneNumber") &&
					ce.IdentificationNumber == "IdentificationNumber" &&
					ce.Name == "Name" &&
					ce.NameMaxlength == 35) &&
				ct.Consignor == Mock.Of<IParty>(cr =>
					cr.Address == Mock.Of<IAddress>(ad =>
						ad.Country == "Country" &&
						ad.City == "City" &&
						ad.Postcode == "Postcode" &&
						ad.StreetAndNumber == "StreetAndNumber" &&
						ad.StreetAndNumberMaxLength == 35) &&
					cr.ContactPerson == Mock.Of<IContactPerson>(cp =>
						cp.EMailAddress == "EMailAddress" &&
						cp.Name == "Name" &&
						cp.PhoneNumber == "PhoneNumber") &&
					cr.IdentificationNumber == "IdentificationNumber" &&
					cr.Name == "Name" &&
					cr.NameMaxlength == 35) &&
				ct.CountryOfRouting == new List<ICountryOfRoutingOfConsignment> { Mock.Of<ICountryOfRoutingOfConsignment>(co =>
					co.SequenceNumber == 5 &&
					co.Country == "Country") } &&
				ct.GrossMass == 500 &&
				ct.InlandModeOfTransport == "40" &&
				ct.LocationOfGoods == Mock.Of<ILocationOfGoods>(lg =>
					lg.AdditionalIdentifier == "AdditionalIdentifier" &&
					lg.Address == Mock.Of<IAddress>(ad =>
						ad.City == "City" &&
						ad.Country == "Country" &&
						ad.Postcode == "Postcode" &&
						ad.StreetAndNumber == "StreetAndNumber" &&
						ad.StreetAndNumberMaxLength == 35) &&
					lg.AuthorisationNumber == "AuthorizationNumber" &&
					lg.ContactPerson == Mock.Of<IContactPerson>(cp =>
						cp.EMailAddress == "EmailAddress" &&
						cp.Name == "Name" &&
						cp.PhoneNumber == "PhoneNumber") &&
					lg.CustomsOfficeReferenceNumber == "CustomsOfficeReferenceNumber" &&
					lg.EconomicOperatorIdentificationNumber == "EconomicOperatorIdentificationNumber" &&
					lg.GNSSLatitute == "GNSSLatitude" &&
					lg.GNSSLongitude == "GNSSLongitude" &&
					lg.PostCodeAddress == Mock.Of<IPostCodeAddress>(pc =>
						pc.HouseNumber == "HouseNumber" &&
						pc.Country == "Country" &&
						pc.Postcode == "Postcode") &&
					lg.QualifierOfIdentification == "QualifierOfIdentification" &&
					lg.TypeOfLocation == "TypeOfLocation" &&
					lg.UNLocode == "UNLocode") &&
				ct.ModeOfTransportAtTheBorder == "ModeOfTransportAtTheBorder" &&
				ct.ReferenceNumberUCR == "ReferenceNumberUCR" &&
				ct.TransportChargesMOP == "TransportChargesMOP" &&
				ct.TransportDocuments == new List<ITransportDocument> { Mock.Of<ITransportDocument>(td =>
					td.SequenceNumber == 6 &&
					td.ReferenceNumber == "ReferenceNumber" &&
					td.Type == "Type") } &&
				ct.TransportEquipments == new List<ITransportEquipment> { Mock.Of<ITransportEquipment>(te =>
					te.SequenceNumber == 7 &&
					te.ContainerIdentificationNumber == "ID" &&
					te.NumberOfSeals == 1 &&
					te.Seals == new List<ISeal> { Mock.Of<ISeal>(sl =>
						sl.SequenceNumber == 8 &&
						sl.Identifier == "Identifier") } &&
					te.GoodsReferences == new List<IGoodsReference> { Mock.Of<IGoodsReference>(gr =>
						gr.SequenceNumber == 9 &&
						gr.DeclarationGoodsItemNumber == 1) }) } &&
				ct.DepartureTransportMeans == new List<IDepartureTransportMeans> { Mock.Of<IDepartureTransportMeans>(dt =>
					dt.SequenceNumber == 10 &&
					dt.TypeOfIdentification == 1 &&
					dt.Nationality == "Nationality" &&
					dt.IdentificationNumber == "IdentificationNumber") }) &&
			gs.CountryOfExport == "CountryOfExport" &&
			gs.CountryOfDestination == "CountryOfDestination" &&
			gs.DeliveryTerms == Mock.Of<IDeliveryTerms>(dt =>
				dt.Country == "Country" &&
				dt.IncotermCode == "IncotermCode" &&
				dt.Location == "Location" &&
				dt.UNLocode == "UNLocode" &&
				dt.Text == "Text") &&
			gs.GoodsItems == new List<IGoodsItem> { Mock.Of<IGoodsItem>(gi =>
				gi.AdditionalInformations == new List<IAdditionalInformation> { Mock.Of<IAdditionalInformation>(ai =>
					ai.SequenceNumber == 11 &&
					ai.Code == "Code" &&
					ai.Text == "Text") } &&
				gi.AdditionalReferences == new List<IDocument> { Mock.Of<IDocument>(ar =>
					ar.SequenceNumber == 12 &&
					ar.ReferenceNumber == "ReferenceNumber" &&
					ar.Type == "Type") } &&
				gi.AdditionalSupplyChainActors == new List<IAdditionalSupplyChainActor> { Mock.Of<IAdditionalSupplyChainActor>(asc =>
					asc.SequenceNumber == 13 &&
					asc.IdentificationNumber == "IdentificationNumber" &&
					asc.Role == "Role") } &&
				gi.Authorisations == new List<IAuthorization> { Mock.Of<IAuthorization>(an =>
					an.SequenceNumber == "14" &&
					an.HolderOfAuthorisation == "HolderOfAuthorisation" &&
					an.ReferenceNumber == "ReferenceNumber" &&
					an.Type == "Type" ) } &&
				gi.Commodity == Mock.Of<ICommodity>(co =>
					co.CalculationOfTaxes == Mock.Of<ICalculationOfTaxes>() &&
					co.CombinedNomenclatureCode == "CombinedNomenclatureCode" &&
					co.CommodityCode == Mock.Of<ICommodityCode>() &&
					co.CusCode == "CusCode" &&
					co.DangerousGoods == new List<IDangerousGoods> { Mock.Of<IDangerousGoods>(dg =>
						dg.SequenceNumber == 15 &&
						dg.UNNumber == "UNNumber") } &&
					co.DescriptionOfGoods == "DescriptionOfGoods" &&
					co.GoodsMeasure == Mock.Of<IGoodsMeasure>(gm =>
						gm.GrossMass == 500 &&
						gm.NetMass == 400 &&
						gm.SupplementaryUnits == 1 &&
						gm.SupplementaryUnitsCode == "SupplementaryUnitsCode") &&
					co.GrossMass == 150 &&
					co.HarmonizedSystemSubHeadingCode == "HarmonizedSystemSubHeadingCode" &&
					co.NetMass == 100 &&
					co.SupplementaryQty == 15) &&
				gi.Consignee == Mock.Of<IParty>(ce =>
					ce.Address == Mock.Of<IAddress>(ad =>
						ad.Country == "Country" &&
						ad.City == "City" &&
						ad.Postcode == "Postcode" &&
						ad.StreetAndNumber == "StreetAndNumber" &&
						ad.StreetAndNumberMaxLength == 35) &&
					ce.ContactPerson == Mock.Of<IContactPerson>(cp =>
						cp.EMailAddress == "EMailAddress" &&
						cp.Name == "Name" &&
						cp.PhoneNumber == "PhoneNumber") &&
					ce.IdentificationNumber == "IdentificationNumber" &&
					ce.Name == "Name" &&
					ce.NameMaxlength == 35) &&
				gi.Consignor == Mock.Of<IParty>(cr =>
					cr.Address == Mock.Of<IAddress>(ad =>
						ad.Country == "Country" &&
						ad.City == "City" &&
						ad.Postcode == "Postcode" &&
						ad.StreetAndNumber == "StreetAndNumber" &&
						ad.StreetAndNumberMaxLength == 35) &&
					cr.ContactPerson == Mock.Of<IContactPerson>(cp =>
						cp.EMailAddress == "EMailAddress" &&
						cp.Name == "Name" &&
						cp.PhoneNumber == "PhoneNumber") &&
					cr.IdentificationNumber == "IdentificationNumber" &&
					cr.Name == "Name" &&
					cr.NameMaxlength == 35) &&
				gi.CountryOfDestination == "CountryOfDestination" &&
				gi.CountryOfExport == "CountryOfExport" &&
				gi.DeclarationGoodsItemNumber == "DeclarationGoodsItemNumber" &&
				gi.NatureOfTransaction == "NatureOfTransaction" &&
				gi.Origin == Mock.Of<IOrigin>(on =>
					on.CountryOfOrigin == "CountryOfOrigin" &&
					on.RegionOfDispatch == "RegionOfDispatch") &&
				gi.Packagings == new List<IPackaging> { Mock.Of<IPackaging>(pg =>
					pg.SequenceNumber == 16 &&
					pg.NumberOfPackages == 1 &&
					pg.ShippingMarks == "ShippingMarks" &&
					pg.TypeOfPackages == "TypeOfPackages") } &&
				gi.PreviousDocuments == new List<IPreviousDocumentExtended> { Mock.Of<IPreviousDocumentExtended>(pd =>
					pd.SequenceNumber == 17 &&
					pd.ComplementOfInformation == "ComplementOfInformation" &&
					pd.GoodsItemNumber == 1 &&
					pd.MeasurementUnitAndQualifier == "MeasurementUnitAndQualifier" &&
					pd.NumberOfPackages == 2 &&
					pd.ReferenceNumber == "ReferenceNumber" &&
					pd.Quantity == 10 &&
					pd.Type == "Type" &&
					pd.TypeOfPackages == "TypeOfPackages") } &&
				gi.Procedure == Mock.Of<IProcedure>(pr =>
					pr.AdditionalProcedure == new List<IAdditionalProcedure> { Mock.Of<IAdditionalProcedure>(ap =>
						ap.SequenceNumber == "18" &&
						ap.AdditionalProcedure == "AdditionalProcedure" ) } &&
					pr.PreviousProcedure == "PreviousProcedure" &&
					pr.RequestedProcedure == "RequestedProcedure") &&
				gi.ReferenceNumberUCR == "ReferenceNumberUCR" &&
				gi.StatisticalValue == 123 &&
				gi.SupportingDocuments == new List<ISupportingDocument> { Mock.Of<ISupportingDocument>(sd =>
					sd.SequenceNumber == 19 &&
					sd.Amount == 2000 &&
					sd.ComplementOfInformation == "ComplementOfInformation" &&
					sd.Currency == "EUR" &&
					sd.DocumentLineItemNumber == 1 &&
					sd.IssuingAuthorityName == "IssuingAuthorityName" &&
					sd.MeasurementUnitAndQualifier == "MeasurementUnitAndQualifier" &&
					sd.Quantity == 20 &&
					sd.ReferenceNumber == "ReferenceNumber" &&
					sd.Type == "Type" &&
					sd.ValidityDate == currentDateTime) } &&
				gi.TransportCharges == Mock.Of<ITransportCharges>(tc =>
					tc.MethodOfPayment == "MethodOfPayment") &&
				gi.TransportDocuments == new List<ITransportDocument>  { Mock.Of<ITransportDocument>(td =>
					td.SequenceNumber == 20 &&
					td.ReferenceNumber == "ReferenceNumber" &&
					td.Type == "Type") } ) } &&
			gs.NatureOfTransaction == "NatureOfTransaction" &&
			gs.PreviousDocuments == new List<IDocument> { Mock.Of<IDocument>(pd =>
				pd.SequenceNumber == 21 &&
				pd.ReferenceNumber == "ReferenceNumber" &&
				pd.Type == "Type") } &&
			gs.SupportingDocuments == new List<ISupportingDocument> { Mock.Of<ISupportingDocument>(sd =>
				sd.SequenceNumber == 22 &&
				sd.Amount == 2000 &&
				sd.ComplementOfInformation == "ComplementOfInformation" &&
				sd.Currency == "EUR" &&
				sd.DocumentLineItemNumber == 1 &&
				sd.IssuingAuthorityName == "IssuingAuthorityName" &&
				sd.MeasurementUnitAndQualifier == "MeasurementUnitAndQualifier" &&
				sd.Quantity == 20 &&
				sd.ReferenceNumber == "ReferenceNumber" &&
				sd.Type == "Type" &&
				sd.ValidityDate == currentDateTime) } &&
			gs.WarehouseType == "WarehouseType" &&
			gs.WarehouseIdentifier == "WarehouseIdentifier"));
		mockProvider.Setup(m => m.LanguageCode).Returns("NL");
		mockProvider.Setup(m => m.MessageIdentification).Returns("SENDERS REFERENCE PLACE HOLDER");
		mockProvider.Setup(m => m.MessageRecipient).Returns("NCTS.BE");
		mockProvider.Setup(m => m.MessageType).Returns("CC515C");
		mockProvider.Setup(m => m.PreparationDateTime).Returns(currentDateTime);
		mockProvider.Setup(m => m.Representative).Returns(Mock.Of<IAESRepresentative>(re =>
			re.Address == Mock.Of<IAddress>(ad =>
				ad.Country == "Country" &&
				ad.City == "City" &&
				ad.Postcode == "Postcode" &&
				ad.StreetAndNumber == "StreetAndNumber" &&
				ad.StreetAndNumberMaxLength == 35) &&
			re.ContactPerson == Mock.Of<IContactPerson>(cp =>
				cp.EMailAddress == "EMailAddress" &&
				cp.Name == "Name" &&
				cp.PhoneNumber == "PhoneNumber") &&
			re.IdentificationNumber == "IdentificationNumber" &&
			re.Name == "Name" &&
			re.Status == "Status"));
	}
}
