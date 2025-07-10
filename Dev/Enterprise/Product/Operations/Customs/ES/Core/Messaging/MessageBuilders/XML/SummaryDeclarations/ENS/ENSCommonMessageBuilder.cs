using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.COMPLEX_ICS;
using CargoWise.Customs.ES.MessageDefinitions.Version1.ENS.Outgoing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public abstract class ENSCommonMessageBuilder<TProvider, TObject> : SummaryDeclarationsCommonMessageBuilder<TProvider, TObject>
		where TProvider : IENSCommonMessageDataProvider
	{
		protected ENSCommonMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected void PopulateENSCommonMessageData<R, F, E>(IENSCommonMessage declaration)
			where R : IENSOrgInfo, new()
			where F : IENSFirstEntryCustomsOffice, new()
			where E : IENSOrgInfo, new()
		{
			PopulateENSGenericMessageData(declaration, provider);

			declaration.Consignor = GetPopulatedAddressInformationENS<Traconco1Type>(provider.Consignor);
			declaration.Consignee = GetPopulatedAddressInformationENS<Traconce1Type>(provider.Consignee);
			declaration.NotifyParty = GetPopulatedAddressInformationENS<Notpar670Type>(provider.NotifyParty);
			declaration.ItineraryCountries = provider.ItineraryCountries.ConvertToCollection(GetPopulatedItineraryCountry<ItiType>);
			declaration.RepresentativeTrader = GetPopulatedAddressInformationENS<R>(provider.RepresentativeTrader);
			declaration.LodgingPerson = GetPopulatedAddressInformationENS<PerlodsumdecType>(provider.LodgingPerson);
			declaration.Seals = provider.Seals.ConvertToCollection(GetPopulatedSeal);
			declaration.FirstEntryCustomsOffice = GetPopulatedFirstEntryCustomsOffice<F>(provider.FirstEntryCustomsOffice, provider.FirstEntryExpectedArrivalDate);
			declaration.SubsequentEntriesCustomsOffices = provider.SubsequentEntriesCustomsOffices.ConvertToCollection(GetPopulatedCustomsOffice<Cusoffsent740Type>);
			declaration.EntryCarrierTrader = GetPopulatedAddressInformationENS<E>(provider.EntryCarrierTrader);
		}

		protected void PopulateENSHeaderCommon(IENSHeaderCommonMessage declaration, IENSCommonHeader commonHeader)
		{
			declaration.BorderTransportMode = commonHeader.BorderTransportMode;
			declaration.BorderTransportId = commonHeader.BorderTransportInfo.Id;
			declaration.BorderTransportLanguage = commonHeader.BorderTransportInfo.Language;
			declaration.BorderTransportNationality = commonHeader.BorderTransportInfo.Nationality;
			declaration.TotalLinesNum = Utilities.FormatNumberFromZIntNational(commonHeader.TotalLinesNum, 0);
			declaration.TotalPackagesQty = Utilities.FormatNumberFromZIntNational(commonHeader.TotalPackagesQty, 0);
			declaration.TotalGrossWeight = commonHeader.TotalGrossWeight;
			declaration.SpecificCircumstanceInd = commonHeader.SpecificCircumstanceInd;
			declaration.MethodOfPayment = commonHeader.MethodOfPayment;
			declaration.CommercialReferenceNumber = commonHeader.CommercialReferenceNumber;
			declaration.ConveyanceReferenceNumber = commonHeader.ConveyanceReferenceNumber;
			declaration.PlaceOfLoading = commonHeader.PlaceOfLoading;
			declaration.PlaceOfLoadingLanguage = commonHeader.PlaceOfLoadingLanguage;
			declaration.PlaceOfUnloading = commonHeader.PlaceOfUnloading;
			declaration.PlaceOfUnloadingLanguage = commonHeader.PlaceOfUnloadingLanguage;
		}

		protected T GetPopulatedLine<T, D, C, B, P>(IENSCommonLine line)
			where T : IENSLineCommonMessage, new()
			where D : IENSCertificate, new()
			where C : IENSContainer, new()
			where B : IENSTransportMeans, new()
			where P : IENSInternalPackages, new()
		{
			var ensCertificateCollection = new Collection<IENSCertificate>();
			var ensContainerCollection = new Collection<IENSContainer>();
			var ensTransportMeansCollection = new Collection<IENSTransportMeans>();
			var ensPackageCollection = new Collection<IENSInternalPackages>();
			foreach (var certificate in line.Certificates.ConvertToCollection(GetPopulatedCertificate) ?? Enumerable.Empty<D>())
			{
				ensCertificateCollection.Add(certificate);
			}

			foreach (var container in line.Containers.ConvertToCollection(GetPopulatedContainer) ?? Enumerable.Empty<C>())
			{
				ensContainerCollection.Add(container);
			}

			foreach (var transportMean in line.BorderTransportMeans.ConvertToCollection(GetPopulatedBorderTransport) ?? Enumerable.Empty<B>())
			{
				ensTransportMeansCollection.Add(transportMean);
			}

			foreach (var package in line.Packages.ConvertToCollection(GetPopulatedENSPackage) ?? Enumerable.Empty<P>())
			{
				ensPackageCollection.Add(package);
			}

			return new T
			{
				LineNumber = Utilities.FormatNumberFromZIntNational(line.LineNumber, 0),
				GoodsDescription = line.GoodsDescription,
				GoodsDescriptionLanguage = line.GoodsDescriptionLanguage,
				GrossWeight = line.GrossWeight,
				MethodOfPayment = line.MethodOfPayment,
				CommercialReferenceNumber = line.CommercialReferenceNumber,
				UNDangerousCode = line.UNDangerousCode,
				PlaceOfLoading = line.PlaceOfLoading,
				PlaceOfLoadingLanguage = line.PlaceOfLoadingLanguage,
				PlaceOfUnloading = line.PlaceOfUnloading,
				PlaceOfUnloadingLanguage = line.PlaceOfUnloadingLanguage,
				Certificates = ensCertificateCollection,
				SpecialMentions = line.SpecialMentions.ConvertToCollection(GetPopulatedSpecialMention),
				Consignor = GetPopulatedAddressInformationENS<Traconco2Type>(line.Consignor),
				CommodityCode = GetPopulatedCommodityCode(line.CommodityCode),
				Consignee = GetPopulatedAddressInformationENS<Traconce2Type>(line.Consignee),
				Containers = ensContainerCollection,
				BorderTransportMeans = ensTransportMeansCollection,
				Packages = ensPackageCollection,
				NotifyParty = GetPopulatedAddressInformationENS<Prtnot640Type>(line.NotifyParty)
			};

			D GetPopulatedCertificate(IENSDocument doc)
			{
				var declarationDoc = GetPopulatedDocumentCommon<D>(doc);
				declarationDoc.DocRefLanguage = doc.Language;

				return declarationDoc;
			}

			Spemenmt2Type GetPopulatedSpecialMention(ZString mention)
			{
				return new Spemenmt2Type
				{
					AddInfCodMt23 = mention
				};
			}

			ComcodgoditmType GetPopulatedCommodityCode(ZString commodityCode)
			{
				return commodityCode.IsEmpty ? null : new ComcodgoditmType
				{
					ComNomCmd1 = commodityCode
				};
			}

			C GetPopulatedContainer(ZString contNumber)
			{
				return new C
				{
					Code = contNumber
				};
			}

			B GetPopulatedBorderTransport(IENSBorderTransport transport)
			{
				return new B
				{
					Nationality = transport.Nationality,
					Id = transport.Id,
					Language = transport.Language
				};
			}

			P GetPopulatedENSPackage(IENSPackage pack)
			{
				var package = GetPopulatedPackageNumbers<P>(pack);
				package.MarksLanguage = pack.MarksLanguage;
				package.PackagesNumberString = pack.PackagesQty.ToString();
				package.PiecesNumberString = pack.PiecesQty.ToString();

				return package;
			}
		}

		Seaid529Type GetPopulatedSeal(IENSSeal seal)
		{
			return new Seaid529Type
			{
				SeaIdSeaid530 = seal.Id,
				SeaIdSeaid530Lng = seal.Language
			};
		}

		T GetPopulatedFirstEntryCustomsOffice<T>(ZString customsOfficeCode, ZDateTime expectedDate) where T : IENSFirstEntryCustomsOffice, new()
		{
			var firstEntryOffice = default(T);
			if (!customsOfficeCode.IsEmpty || !expectedDate.IsEmpty)
			{
				firstEntryOffice = new T()
				{
					OfficeCode = customsOfficeCode,
					ExpectedArrivalDate = expectedDate.ToLongCustomsFormatDateTimeString()
				};
			}
			return firstEntryOffice;
		}
	}
}
