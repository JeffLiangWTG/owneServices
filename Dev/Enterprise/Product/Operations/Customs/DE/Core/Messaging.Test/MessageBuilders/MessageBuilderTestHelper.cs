using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using Moq;

namespace Enterprise.Customs.DE.Messaging.Testing
{
	static class MessageBuilderTestHelper
	{
		public static Mock<IOutwardProcessing> MockOutwardProcessing(string[] reimportCountries
			, IReadOnlyCollection<IIdentificationMeans> identificationMeans
			, IReadOnlyCollection<IProduct> products)
		{
			var mock = new Mock<IOutwardProcessing>();
			mock.Setup(h => h.ReimportCountries).Returns(reimportCountries);
			mock.Setup(h => h.IdentificationMeans).Returns(identificationMeans);
			mock.Setup(m => m.Products).Returns(products);
			return mock;
		}

		public static Mock<IPartyID> MockPartyId(string eoriNumber
			, string eoriBranchSuffix)
		{
			var mock = new Mock<IPartyID>();
			mock.Setup(p => p.EoriNumber).Returns(eoriNumber);
			mock.Setup(m => m.EoriBranchSuffix).Returns(eoriBranchSuffix);
			return mock;
		}

		public static Mock<IAESParty> MockAESPartyEoriIdentification(string eoriNumber, string eoriBranchSuffix, string tcuNumber = null)
			=> MockParty(eoriNumber: eoriNumber, eoriBranchSuffix: eoriBranchSuffix, tcuNumber: tcuNumber);

		public static Mock<IAESParty> MockParty(string eoriNumber = ""
			, string eoriBranchSuffix = ""
			, string name = ""
			, string line = ""
			, string city = ""
			, string postcode = ""
			, string country = ""
			, string additionalAddressInfo = ""
			, string type = ""
			, IAESPartyContactPerson contactPerson = null
			, string tcuNumber = ""
			, string address2 = "")
		{
			var mock = new Mock<IAESParty>();
			mock.Setup(p => p.EoriNumber).Returns(eoriNumber);
			mock.Setup(p => p.EoriBranchSuffix).Returns(eoriBranchSuffix);
			mock.Setup(p => p.Type).Returns(type);
			mock.Setup(p => p.Name).Returns(name);
			mock.Setup(p => p.Address).Returns(line);
			mock.Setup(p => p.City).Returns(city);
			mock.Setup(p => p.Postcode).Returns(postcode);
			mock.Setup(p => p.Country).Returns(country);
			mock.Setup(p => p.AdditionalAddressInfo).Returns(additionalAddressInfo);
			mock.Setup(p => p.ContactPerson).Returns(contactPerson);
			mock.Setup(p => p.TCUNumber).Returns(tcuNumber);
			mock.Setup(m => m.Address2).Returns(address2);
			return mock;
		}

		public static Mock<IGoodsLoadingPlace> MockGoodsLoadingPlace(string loadingPlaceCode = ""
			, string line = ""
			, string city = ""
			, string postcode = ""
			, string additionalAddressInfo = "")
		{
			var mock = new Mock<IGoodsLoadingPlace>();
			mock.Setup(glp => glp.LoadingPlaceCode).Returns(loadingPlaceCode);
			mock.Setup(glp => glp.Address).Returns(line);
			mock.Setup(glp => glp.City).Returns(city);
			mock.Setup(glp => glp.Postcode).Returns(postcode);
			mock.Setup(glp => glp.AdditionalAddressInfo).Returns(additionalAddressInfo);
			return mock;
		}

		public static Mock<IAESPartyContactPerson> MockContactPerson(string position
			, string personName
			, string phoneNumber
			, string facsimileNumber
			, string mailAddress)
		{
			var mock = new Mock<IAESPartyContactPerson>();
			mock.Setup(cp => cp.Position).Returns(position);
			mock.Setup(cp => cp.PersonName).Returns(personName);
			mock.Setup(cp => cp.PhoneNumber).Returns(phoneNumber);
			mock.Setup(cp => cp.FacsimileNumber).Returns(facsimileNumber);
			mock.Setup(cp => cp.MailAddress).Returns(mailAddress);
			return mock;
		}

		public static Mock<IIdentificationMeans> MockIdentificationMean(string type
			, string description)
		{
			var mock = new Mock<IIdentificationMeans>();
			mock.Setup(im => im.Type).Returns(type);
			mock.Setup(im => im.Description).Returns(description);
			return mock;
		}

		public static Mock<IDeliveryTerms> MockDeliveryTerms(string incotermCode = null, string location = null, string unLocode = null, string country = null, string text = null)
		{
			var mock = new Mock<IDeliveryTerms>();
			mock.Setup(dt => dt.IncotermCode).Returns(incotermCode);
			mock.Setup(dt => dt.UNLocode).Returns(unLocode);
			mock.Setup(dt => dt.Location).Returns(location);
			mock.Setup(dt => dt.Country).Returns(country);
			mock.Setup(dt => dt.Text).Returns(text);
			return mock;
		}

		public static Mock<IPackage> MockPackage(int quantity
			, string kind
			, string marksNumbers
			, int positionNumber
			, bool isSupportEmptyPackType = false)
		{
			var mock = new Mock<IPackage>();
			mock.Setup(pack => pack.Quantity).Returns(quantity);
			mock.Setup(pack => pack.Kind).Returns(kind);
			mock.Setup(pack => pack.MarksNumbers).Returns(marksNumbers);
			mock.Setup(pack => pack.PositionNumber).Returns(positionNumber);
			mock.Setup(pack => pack.IsSupportEmptyPackType).Returns(isSupportEmptyPackType);
			return mock;
		}

		public static Mock<ISupportingDocument> MockDocument(string qualifier
			, string type
			, string referenceNumber
			, string complement
			, string detail
			, DateTime? issuingDate
			, DateTime? validityDate
			, decimal amount
			, string complementaryUnit
			, decimal quantity
			, int documentLineItemNumber = 0
			, string issuingAuthorityName = ""
			, string measurementUnitAndQualifier = ""
			, string currency = "")
		{
			var mock = new Mock<ISupportingDocument>();
			mock.Setup(d => d.Qualifier).Returns(qualifier);
			mock.Setup(d => d.Type).Returns(type);
			mock.Setup(d => d.ReferenceNumber).Returns(referenceNumber);
			mock.Setup(d => d.Complement).Returns(complement);
			mock.Setup(d => d.Detail).Returns(detail);
			mock.Setup(d => d.IssuingDate).Returns(issuingDate);
			mock.Setup(d => d.ValidityDate).Returns(validityDate);
			mock.Setup(d => d.Amount).Returns(amount);
			mock.Setup(d => d.ComplementaryUnit).Returns(complementaryUnit);
			mock.Setup(d => d.Quantity).Returns(quantity);
			mock.Setup(d => d.DocumentLineItemNumber).Returns(documentLineItemNumber);
			mock.Setup(d => d.IssuingAuthorityName).Returns(issuingAuthorityName);
			mock.Setup(d => d.MeasurementUnitAndQualifier).Returns(measurementUnitAndQualifier);
			mock.Setup(m => m.Currency).Returns(currency);
			return mock;
		}

		public static Mock<IPreviousDocument> MockPreviousDocument(string fullType = ""
			, string referenceNumber = ""
			, string complement = ""
			, string type = ""
			, string qualifier = null
			, int goodsItemNumber = 0
			, string measurementUnitAndQualifier = null
			, decimal quantity = 0m)
		{
			var mock = new Mock<IPreviousDocument>();
			mock.Setup(pd => pd.FullType).Returns(fullType);
			mock.Setup(pd => pd.Type).Returns(type);
			mock.Setup(pd => pd.Qualifier).Returns(qualifier);
			mock.Setup(pd => pd.ReferenceNumber).Returns(referenceNumber);
			mock.Setup(pd => pd.GoodsItemNumber).Returns(goodsItemNumber);
			mock.Setup(pd => pd.MeasurementUnitAndQualifier).Returns(measurementUnitAndQualifier);
			mock.Setup(pd => pd.Quantity).Returns(quantity);
			mock.Setup(pd => pd.Complement).Returns(complement);
			return mock;
		}

		public static Mock<IWarehouseProcedure> MockWarehouseProcedure(int referencedSequenceNumber
			, string registrationNumber
			, string accessViaAtlasFlag
			, string commodityCode
			, string usualProcessingFlag
			, string complement
			, IAmount debitAmount = null
			, IAmount commercialAmount = null
			, string mrn = ""
			, string harmonizedSystemSubHeadingCode = ""
			, string combinedNomenclatureCode = ""
			, string taricCode = ""
			, string nationalAdditionalCode = "")
		{
			var mock = new Mock<IWarehouseProcedure>();
			mock.Setup(wp => wp.ReferencedSequenceNumber).Returns(referencedSequenceNumber);
			mock.Setup(wp => wp.RegistrationNumber).Returns(registrationNumber);
			mock.Setup(wp => wp.AccessViaAtlasFlag).Returns(accessViaAtlasFlag);
			mock.Setup(wp => wp.CommodityCode).Returns(commodityCode);
			mock.Setup(wp => wp.UsualProcessingFlag).Returns(usualProcessingFlag);
			mock.Setup(wp => wp.Complement).Returns(complement);
			mock.Setup(wp => wp.DebitAmount).Returns(debitAmount);
			mock.Setup(wp => wp.CommercialAmount).Returns(commercialAmount);
			mock.Setup(wp => wp.MRN).Returns(mrn);
			mock.Setup(wp => wp.HarmonizedSystemSubHeadingCode).Returns(harmonizedSystemSubHeadingCode);
			mock.Setup(wp => wp.CombinedNomenclatureCode).Returns(combinedNomenclatureCode);
			mock.Setup(wp => wp.TaricCode).Returns(taricCode);
			mock.Setup(wp => wp.NationalAdditionalCode).Returns(nationalAdditionalCode);
			return mock;
		}

		public static Mock<IAmount> MockAmount(string qualifier
			, string measurementUnit
			, decimal quantity)
		{
			var mock = new Mock<IAmount>();
			mock.Setup(m => m.Qualifier).Returns(qualifier);
			mock.Setup(m => m.MeasurementUnit).Returns(measurementUnit);
			mock.Setup(m => m.Quantity).Returns(quantity);
			return mock;
		}

		public static Mock<IInwardProcessingProcedure> MockInwardProcessingProcedure(int referencedSequenceNumber
			, string registrationNumber
			, string accessViaAtlasFlag
			, string goodsRelatedInformation
			, string mrn = "")
		{
			var mock = new Mock<IInwardProcessingProcedure>();
			mock.Setup(ip => ip.ReferencedSequenceNumber).Returns(referencedSequenceNumber);
			mock.Setup(ip => ip.RegistrationNumber).Returns(registrationNumber);
			mock.Setup(ip => ip.AccessViaAtlasFlag).Returns(accessViaAtlasFlag);
			mock.Setup(ip => ip.GoodsRelatedInformation).Returns(goodsRelatedInformation);
			mock.Setup(ip => ip.MRN).Returns(mrn);
			return mock;
		}

		public static Mock<IProduct> MockProduct(string commodityCode
			, string goodsDescription
			, string harmonizedSystemSubHeadingCode = ""
			, string combinedNomenclatureCode = "")
		{
			var mock = new Mock<IProduct>();
			mock.Setup(p => p.CommodityCode).Returns(commodityCode);
			mock.Setup(p => p.GoodsDescription).Returns(goodsDescription);
			mock.Setup(p => p.HarmonizedSystemSubHeadingCode).Returns(harmonizedSystemSubHeadingCode);
			mock.Setup(m => m.CombinedNomenclatureCode).Returns(combinedNomenclatureCode);
			return mock;
		}

		public static Mock<IDateTimeRange> MockDateTimeRange(DateTime startDateTime
			, DateTime endDateTime)
		{
			var mock = new Mock<IDateTimeRange>();
			mock.Setup(dr => dr.StartDateTime).Returns(startDateTime);
			mock.Setup(dr => dr.EndDateTime).Returns(endDateTime);
			return mock;
		}

		public static Mock<IAuthorisation> MockAuthorisation(string type, string referenceNumber)
		{
			var mock = new Mock<IAuthorisation>();
			mock.Setup(x => x.Type).Returns(type);
			mock.Setup(m => m.ReferenceNumber).Returns(referenceNumber);
			return mock;
		}

		public static Mock<ISupplyChainActor> MockSupplyChainActor(string role, string identificationNumber)
		{
			var mock = new Mock<ISupplyChainActor>();
			mock.Setup(x => x.Role).Returns(role);
			mock.Setup(m => m.IdentificationNumber).Returns(identificationNumber);
			return mock;
		}

		public static Mock<IReference> MockAdditionalInfo(string fullType = "", string type = "", string qualifier = "", string referenceNumber = "", string complement = "",
			string detail = "", string currency = "", decimal amount = 0)
		{
			var mock = new Mock<IReference>();
			mock.Setup(x => x.FullType).Returns(fullType);
			mock.Setup(x => x.Type).Returns(type);
			mock.Setup(x => x.Qualifier).Returns(qualifier);
			mock.Setup(x => x.ReferenceNumber).Returns(referenceNumber);
			mock.Setup(x => x.Complement).Returns(complement);
			mock.Setup(x => x.Detail).Returns(detail);
			mock.Setup(x => x.Currency).Returns(currency);
			mock.Setup(m => m.Amount).Returns(amount);
			return mock;
		}

		public static Mock<ITransportEquipment> MockTransportEquipment(string identificationNumber, string[] sealIdentifiers, int[] declarationGoodsItemNumbers)
		{
			var mock = new Mock<ITransportEquipment>();
			mock.Setup(x => x.ContainerIdentificationNumber).Returns(identificationNumber);
			mock.Setup(x => x.SealIdentifiers).Returns(sealIdentifiers);
			mock.Setup(m => m.DeclarationGoodsItemNumbers).Returns(declarationGoodsItemNumbers);
			return mock;
		}

		public static Mock<IPartyDocAddress> MockPartyDocAddress(string additionalAddressInformation, string address, string address2, string postcode, string city, string country)
		{
			var mock = new Mock<IPartyDocAddress>();
			mock.Setup(x => x.AdditionalAddressInformation).Returns(additionalAddressInformation);
			mock.Setup(x => x.Address).Returns(address);
			mock.Setup(x => x.Address2).Returns(address2);
			mock.Setup(x => x.Postcode).Returns(postcode);
			mock.Setup(x => x.City).Returns(city);
			mock.Setup(m => m.Country).Returns(country);
			return mock;
		}

		public static Mock<IDepartureTransportMeans> MockDepartureTransportMeans(string typeOfIdentification, string identificationNumber, string nationality)
		{
			var mock = new Mock<IDepartureTransportMeans>();
			mock.Setup(x => x.TypeOfIdentification).Returns(typeOfIdentification);
			mock.Setup(x => x.IdentificationNumber).Returns(identificationNumber);
			mock.Setup(m => m.Nationality).Returns(nationality);
			return mock;
		}

		public static string GetLongString(string element, int length) => string.Join(string.Empty
			, Enumerable.Repeat(element, length).ToArray());

		public static Mock<IImportCosts> MockImportCosts(decimal value, string currencyCode, bool currencyRateAgreedFlag, decimal currencyRate)
		{
			var mock = new Mock<IImportCosts>();
			mock.Setup(x => x.Value).Returns(value);
			mock.Setup(x => x.CurrencyCode).Returns(currencyCode);
			mock.Setup(x => x.CurrencyRateAgreedFlag).Returns(currencyRateAgreedFlag);
			mock.Setup(m => m.CurrencyRate).Returns(currencyRate);
			return mock;
		}

		public static Mock<IImportSpecificRate> MockImportSpecificRate(string type, decimal value)
		{
			var mock = new Mock<IImportSpecificRate>();
			mock.Setup(x => x.Type).Returns(type);
			mock.Setup(m => m.Value).Returns(value);
			return mock;
		}

		public static Mock<IContentInformation> MockContentInformation(string contentType, decimal degreePrecentage)
		{
			var mock = new Mock<IContentInformation>();
			mock.Setup(x => x.ContentType).Returns(contentType);
			mock.Setup(m => m.DegreePercentage).Returns(degreePrecentage);
			return mock;
		}

		public static Mock<IExciseDuty> MockExciseDuty(string code, decimal degreePrecentaged, decimal value, int quantity, string measurementUnit, string qualifier)
		{
			var mock = new Mock<IExciseDuty>();
			mock.Setup(x => x.Code).Returns(code);
			mock.Setup(x => x.DegreePercentage).Returns(degreePrecentaged);
			mock.Setup(x => x.Value).Returns(value);
			mock.Setup(m => m.Amount).Returns(GetAmount(quantity, measurementUnit, qualifier).Object);
			return mock;
		}

		public static Mock<IAmount> GetAmount(decimal quantity, string measurementUnit, string qualifier)
		{
			var mock = new Mock<IAmount>();
			mock.Setup(a => a.Quantity).Returns(quantity);
			mock.Setup(a => a.MeasurementUnit).Returns(measurementUnit);
			mock.Setup(m => m.Qualifier).Returns(qualifier);
			return mock;
		}

		public static Mock<IImportSpecialCase> GetImportSpecialCase(string group, string applicationType, decimal rateOrAmountOrFactor)
		{
			var mock = new Mock<IImportSpecialCase>();
			mock.Setup(x => x.Group).Returns(group);
			mock.Setup(x => x.ApplicationType).Returns(applicationType);
			mock.Setup(m => m.RateOrAmountOrFactor).Returns(rateOrAmountOrFactor);
			return mock;
		}

		public static Mock<IImportDocument> GetImportDocument(string type, string referenceNumber, DateTime? issuingDate)
		{
			var mock = new Mock<IImportDocument>();
			mock.Setup(d => d.Type).Returns(type);
			mock.Setup(d => d.ReferenceNumber).Returns(referenceNumber);
			mock.Setup(m => m.IssuingDate).Returns(issuingDate);
			return mock;
		}

		public static Mock<IImportPartyIdAddress> GetImportPartyIdAddress(string name, string postcode, string city, string district, string address, string country)
		{
			var mock = new Mock<IImportPartyIdAddress>();
			mock.Setup(x => x.Name).Returns(name);
			mock.Setup(x => x.Postcode).Returns(postcode);
			mock.Setup(x => x.City).Returns(city);
			mock.Setup(x => x.District).Returns(district);
			mock.Setup(x => x.Address).Returns(address);
			mock.Setup(m => m.Country).Returns(country);
			return mock;
		}

		internal static string AsString(this Stream stream)
		{
			if (stream == null)
			{
				return null;
			}
			using (var readerSource = new StreamReader(stream))
			{
				return readerSource.ReadToEnd();
			}
		}
	}
}
