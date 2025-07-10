using CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.COM;
using CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE615V4Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.SIM;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[CodeAlive("This class will be used in next Work Items")]
	public class EXSMessageBuilder : SummaryDeclarationsCommonMessageBuilder<IEXSMessageDataProvider, Cc615A>
	{
		public EXSMessageBuilder(IEXSMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override Cc615A GenerateXMLMessage()
		{
			var declaration = new Cc615A
			{
				MesTypMes20 = nameof(Cc615A).ToUpper(),

				Heahea = GetPopulatedHeader(),
				Transdoc1 = GetPopulatedDocumentCommon<Transdoc1>(provider.TransportDocument),
				Traconco1 = GetPopulatedAddressInformationCommon<Traconco1>(provider.Consignor),
				Traconce1 = GetPopulatedAddressInformationCommon<Traconce1>(provider.Consignee),
				Asca1 = provider.AdditionalActors.ConvertToCollection(GetPopulatedAdditionalActor<Asca1>),
				Addinf1 = provider.AdditionalInfo.ConvertToCollection(GetPopulatedAdditionalInfo<Addinf1>),
				Gooitegds = provider.Lines.ConvertToCollection(GetPopulatedLine),
				Iti = provider.ItineraryCountries.ConvertToCollection(GetPopulatedItineraryCountry<Iti>),
				Cusofflon = GetPopulatedCustomsOffice<Cusofflon>(provider.CustomsOffice),
				Perlodsumdec = GetPopulatedLodgingPerson(),
				Replodper = GetPopulatedRepresentative(),
				Carrier = GetPopulatedCarrier(),
				Seaid529 = provider.Seals.ConvertToCollection(GetPopulatedSeal)
			};

			PopulateSummaryDeclarationMessageData(declaration);

			return declaration;
		}

		Heahea GetPopulatedHeader()
		{
			var header = provider.Header;
			return header == null ? null : new Heahea
			{
				RefNumHea4 = header.ReferenceNumber,
				CusSubPlaHea66 = header.GoodsLocation,
				TotNumOfIteHea305 = Utilities.FormatNumberFromZIntNational(header.TotalLinesNum, 0),
				TotNumOfPacHea306 = Utilities.FormatNumberFromZIntNational(header.TotalPackagesQty, 0),
				TotGroMasHea307 = header.TotalGrossWeight,
				DecDatTimHea114 = header.DeclarationDate.ToLongCustomsFormatDateTimeString(),
				DecPlaHea394 = header.DeclarationPlace,
				SpeCirIndHea1 = header.SpecificCircumstanceInd,
				NatSpeCirIndHea = header.NatSpecificCircumstanceInd,
				DocOpeHea = header.DocumentOperationIndicator,
				DocNumHea5 = header.DocumentReferenceNumber,
				TraChaMetOfPayHea1 = header.MethodOfPayment
			};
		}

		T GetPopulatedAdditionalActor<T>(IEXSAdditionalActor actor) where T : IAdditionalActor, new()
		{
			var declaration = default(T);
			if (actor != null)
			{
				declaration = new T()
				{
					Role = actor.Role,
					Id = actor.Id,
				};
			}
			return declaration;
		}

		T GetPopulatedAdditionalInfo<T>(IDocumentsCommon info) where T : IAdditionalInfo, new()
		{
			var declaration = default(T);
			if (info != null)
			{
				declaration = new T()
				{
					Code = info.Name,
					Text = info.Number,
				};
			}
			return declaration;
		}

		Gooitegds GetPopulatedLine(IEXSLine line)
		{
			return new Gooitegds
			{
				IteNumGds7 = Utilities.FormatNumberFromZIntNational(line.LineNumber, 0),
				GooDesGds23 = line.GoodsDescription,
				GroMasGds46 = line.GrossWeight,
				MetOfPayGdi12 = line.MethodOfPayment,
				UnDanGooCodGdi1 = line.UNDangerousCode,
				Ucr2 = line.ReferenceNumber,
				Prodocdc2 = line.Certificates.ConvertToCollection(GetPopulatedDocumentCommon<Prodocdc2>),
				Predocgoditm1 = GetPopulatedDocument(),
				Traconco2 = GetPopulatedAddressInformationCommon<Traconco2>(line.Consignor),
				Comcodgoditm = GetPopulatedCommodityCode(),
				Traconce2 = GetPopulatedAddressInformationCommon<Traconce2>(line.Consignee),
				Connr2 = line.Containers.ConvertToCollection(GetPopulatedContainer),
				Pacgs2 = line.Packages.ConvertToCollection(GetPopulatedPackage),
				Cuscode = GetPopulatedCusCode(),
				Asca2 = line.AdditionalActors.ConvertToCollection(GetPopulatedAdditionalActor<Asca2>),
				Addinf2 = line.AdditionalInfo.ConvertToCollection(GetPopulatedAdditionalInfo<Addinf2>)
			};

			Predocgoditm1 GetPopulatedDocument()
			{
				var doc = line.PreviousDocument;
				return doc == null ? null : new Predocgoditm1()
				{
					DocTypPd11 = doc.Name,
					DocRefPd12 = doc.Number,
					DocGdsIteNumPd13 = doc.LineNumber
				};
			}

			Comcodgoditm GetPopulatedCommodityCode()
			{
				var commodityCode = line.CommodityCode;
				return commodityCode.IsEmpty ? null : new Comcodgoditm
				{
					ComNomCmd1 = commodityCode
				};
			}

			Connr2 GetPopulatedContainer(ZString contNumber)
			{
				return new Connr2
				{
					ConNumNr21 = contNumber
				};
			}

			Pacgs2 GetPopulatedPackage(IEXSPackage packageProvider)
			{
				var package = GetPopulatedPackage<Pacgs2>(packageProvider);
				if (package != null)
				{
					package.NumOfPacGs24 = packageProvider.IsPackTypeBulk ? string.Empty : packageProvider.PackagesQty.ToString();
				}
				return package;
			}

			Cuscode GetPopulatedCusCode()
			{
				var cusCode = line.CusCode;
				return cusCode.IsEmpty ? null : new Cuscode
				{
					CusCode = cusCode
				};
			}
		}

		Perlodsumdec GetPopulatedLodgingPerson()
		{
			var person = provider.LodgingPerson;
			var declaration = GetPopulatedAddressInformationCommon<Perlodsumdec>(person, false);
			if (declaration != null)
			{
				declaration.EmailPld1 = person.EmailAddress;
			}
			return declaration;
		}

		Replodper GetPopulatedRepresentative()
		{
			var representative = provider.Representative;
			return (representative == null || representative.DirectRepresentation == ZString.Empty) ? null : new Replodper
			{
				Tinrep1 = representative.Id,
				StatusRep1 = representative.DirectRepresentation == DirectRepresentation ? StatusType.Item2 : StatusType.Item3,
				ContactPersonRep1 = new ContactPersonRep1Type()
				{
					NameRep1 = representative.Name,
					PhoneNumberRep1 = representative.Phone,
					EmailRep1 = representative.EmailAddress
				}
			};
		}

		Carrier GetPopulatedCarrier()
		{
			var carrier = provider.Carrier;
			return carrier == null ? null : new Carrier
			{
				Tincar1 = carrier.Id,
				ContactPersonCar1 = new ContactPersonCar1Type()
				{
					NameCar1 = carrier.Name,
					PhoneNumberCar1 = carrier.Phone,
					EmailCar1 = carrier.EmailAddress
				}
			};
		}

		Seaid529 GetPopulatedSeal(ZString sealId)
		{
			return new Seaid529
			{
				SeaIdSeaid530 = sealId
			};
		}
		const string DirectRepresentation = "2";
	}
}
