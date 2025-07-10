using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.ICS.CC315A_v10_0;
using CargoWise.Customs.GB.MessageDefinitions.ICS.COMPLEX_ICS;
using CargoWise.Customs.GB.MessageDefinitions.ICS.TCL;
using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging.IE315
{
	public class CC315AMessageBuilder_v10 : MessageBuilder
	{
		public CC315AMessageBuilder_v10(IDeclaration inputDeclaration)
		{
			OutputMessageObject = new Cc315AType();
			InputDeclaration = inputDeclaration;
		}

		public static class Constants
		{
			public const string Unpacked = "UNPACKED";
		}

		internal Cc315AType OutputMessageObject { get; }
		IDeclaration InputDeclaration { get; }
		ZDateTime ProcessingStartTime { get; set; }

		public bool IsSendingTIN { get; set; } // TODO: To be implemented.
		bool HasMultiGoodsItems => InputDeclaration.GoodsItems.Take(2).Count() == 2;

		protected override object GetPopulatedMessageObject()
		{
			ProcessingStartTime = ZDateTime.UtcNow;
			PopulateMessageObject();
			return OutputMessageObject;
		}

		internal protected void PopulateMessageObject()
		{
			PopulateDeclarationFields();
			PopulateHeader();
			PopulateGoodsItems();
			PopulateItineraries();
			PopulateLodgementCustomsOffice();
			PopulateSealsIds();
			PopulateFirstEntry();
			PopulateSubsequentEntries();
			if (!IsSendingTIN)
			{
				PopulateConsignee();
				PopulateNotifyParty();
				PopulateLodgingSummaryDeclarationPerson();
				if (!HasMultiGoodsItems)
				{
					PopulateConsignor();
				}

				if (InputDeclaration.EntryCarrier != null && InputDeclaration.EntryCarrier.Name != InputDeclaration.LodgingSummaryDeclarationPerson.Name)
				{
					PopulateEntryCarrier();
				}
			}
		}

		protected void PopulateDeclarationFields()
		{
			OutputMessageObject.MesSenMes3 = InputDeclaration.MessageSender;
			OutputMessageObject.MesRecMes6 = InputDeclaration.MessageRecipient;
			OutputMessageObject.DatOfPreMes9 = ProcessingStartTime.ToString("ddMMyy");
			OutputMessageObject.TimOfPreMes10 = ProcessingStartTime.ToString("HHmm");
			OutputMessageObject.TesIndMes18 = InputDeclaration.TestIndicator ? Flag.Item1 : Flag.Item0;
			OutputMessageObject.MesIdeMes19 = InputDeclaration.MessageIdentification;
			OutputMessageObject.MesTypMes20 = MessageTypes.Cc315A;
		}

		protected void PopulateHeader()
		{
			var header = InputDeclaration.Header;
			OutputMessageObject.Heahea = new HeaheaType
			{
				RefNumHea4 = header.ReferenceNumber,
				TraModAtBorHea76 = header.TransportModeAtBorder,
				IdeOfMeaOfTraCroHea85 = header.IdentityOfMeansOfTransportCrossingBorder,
				NatOfMeaOfTraCroHea87 = header.NationalityOfMeansOfTransportCrossingBorder,
				TotNumOfIteHea305 = header.TotalNumberOfItems.ToString(),
				TotNumOfPacHea306 = header.TotalNumberOfPackages.ToString(),
				TotGroMasHea307 = header.TotalGrossMass,
				DecPlaHea394 = header.DeclarationPlace,
				SpeCirIndHea1 = header.SpecificCircumstanceIndicator,
				TraChaMetOfPayHea1 = header.TransportChargesMethodOfPayment,
				ComRefNumHea = header.CommercialReferenceNumber,
				ConRefNumHea = header.ConveyanceReferenceNumber,
				PlaLoaGooite334 = header.PlaceOfLoading,
				PlaUnlGooite334 = header.PlaceOfUnloading,
				DecDatTimHea114 = ProcessingStartTime.ToString("yyyyMMddHHmm"),
			};
		}

		protected void PopulateConsignor()
		{
			var consignor = InputDeclaration.Consignor;
			OutputMessageObject.Traconco1 = new Traconco1Type
			{
				NamCo17 = consignor.Name,
				StrAndNumCo122 = consignor.StreetAndNumber,
				PosCodCo123 = consignor.PostalCode,
				CitCo124 = consignor.City,
				CouCo125 = consignor.CountryCode,
				Tinco159 = consignor.ConfigCode,
			};
		}

		protected void PopulateConsignee()
		{
			var consignee = InputDeclaration.Consignee;
			OutputMessageObject.Traconce1 = new Traconce1Type
			{
				NamCe17 = consignee.Name,
				StrAndNumCe122 = consignee.StreetAndNumber,
				PosCodCe123 = consignee.PostalCode,
				CitCe124 = consignee.City,
				CouCe125 = consignee.CountryCode,
				Tince159 = consignee.ConfigCode,
			};
		}

		protected void PopulateNotifyParty()
		{
			var notifyParty = InputDeclaration.NotifyParty;
			OutputMessageObject.Notpar670 = new Notpar670Type
			{
				NamNotpar672 = notifyParty.Name,
				StrNumNotpar673 = notifyParty.StreetAndNumber,
				PosCodNotpar676 = notifyParty.PostalCode,
				CitNotpar674 = notifyParty.City,
				CouCodNotpar675 = notifyParty.CountryCode,
				Tinnotpar671 = notifyParty.ConfigCode,
			};
		}

		#region Populate GoodsItems

		protected void PopulateGoodsItems()
		{
			var result = new Collection<GooitegdsType>();

			foreach (var goodsItem in InputDeclaration.GoodsItems)
			{
				var item = new GooitegdsType
				{
					IteNumGds7 = goodsItem.ItemNumber.ToString(),
					GooDesGds23 = goodsItem.GoodsDescription,
					GroMasGds46 = goodsItem.GrossMass,
					ComRefNumGim1 = goodsItem.CommercialReferenceNumber,
					Spemenmt2 = PopulateGoodsItemSpecialMentions(goodsItem.SpecialMentions),
					Connr2 = PopulateGoodsItemContainers(goodsItem.Containers),
					Pacgs2 = PopulateGoodsItemPackages(goodsItem.Packages),
				};

				if (!IsSendingTIN)
				{
					item.Traconce2 = PopulateGoodsItemConsignee(goodsItem.Consignee);
					item.Prtnot640 = PopulateGoodsItemNotifyParty(goodsItem.NotifyParty);
					if (HasMultiGoodsItems)
					{
						item.Traconco2 = PopulateGoodsItemConsignor(goodsItem.Consignor);
					}
				}

				result.Add(item);
			}

			OutputMessageObject.Gooitegds = result;
		}

		Collection<Spemenmt2Type> PopulateGoodsItemSpecialMentions(IEnumerable<ISpecialMention> specialMentions)
		{
			var result = new Collection<Spemenmt2Type>();

			foreach (var specialMention in specialMentions)
			{
				result.Add(new Spemenmt2Type { AddInfCodMt23 = specialMention.AdditionalInformationCoded });
			}

			return result;
		}

		Traconco2Type PopulateGoodsItemConsignor(ITrader consignor) => new Traconco2Type
		{
			NamCo27 = consignor.Name,
			StrAndNumCo222 = consignor.StreetAndNumber,
			PosCodCo223 = consignor.PostalCode,
			CitCo224 = consignor.City,
			CouCo225 = consignor.CountryCode,
			Tinco259 = consignor.ConfigCode
		};

		Traconce2Type PopulateGoodsItemConsignee(ITrader consignee) => new Traconce2Type
		{
			NamCe27 = consignee.Name,
			StrAndNumCe222 = consignee.StreetAndNumber,
			PosCodCe223 = consignee.PostalCode,
			CitCe224 = consignee.City,
			CouCe225 = consignee.CountryCode,
			Tince259 = consignee.ConfigCode
		};

		Collection<Connr2Type> PopulateGoodsItemContainers(IEnumerable<IContainer> containers)
		{
			var result = new Collection<Connr2Type>();

			foreach (var container in containers)
			{
				result.Add(new Connr2Type { ConNumNr21 = container.ContainerNumber });
			}

			return result;
		}

		Collection<Pacgs2Type> PopulateGoodsItemPackages(IEnumerable<IPackage> packages)
		{
			var result = new Collection<Pacgs2Type>();

			foreach (var package in packages)
			{
				var item = new Pacgs2Type { KinOfPacGs23 = package.KindOfPackages };

				if (package.KindOfPackages != Constants.Unpacked)
				{
					item.NumOfPacGs24 = package.NumberOfPackages.ToString();
				}
				else
				{
					item.NumOfPieGs25 = package.NumberOfPieces.ToString();
				}

				result.Add(item);
			}

			return result;
		}

		Prtnot640Type PopulateGoodsItemNotifyParty(ITrader notifyParty) => new Prtnot640Type
		{
			NamPrtnot642 = notifyParty.Name,
			StrNumPrtnot646 = notifyParty.StreetAndNumber,
			PstCodPrtnot644 = notifyParty.PostalCode,
			CtyPrtnot643 = notifyParty.City,
			CouCodGinot647 = notifyParty.CountryCode,
			Tinprtnot641 = notifyParty.ConfigCode
		};

		#endregion

		protected void PopulateItineraries()
		{
			var result = new Collection<ItiType>();

			foreach (var itinerary in InputDeclaration.Itineraries)
			{
				result.Add(new ItiType { CouOfRouCodIti1 = itinerary.CountryOfRoutingCode });
			}

			OutputMessageObject.Iti = result;
		}

		protected void PopulateLodgementCustomsOffice()
		{
			OutputMessageObject.Cusofflon = new CusofflonType { RefNumCol1 = InputDeclaration.LodgementCustomsOffice.ReferenceNumber };
		}

		protected void PopulateRepresentative()
		{
			var representative = InputDeclaration.Representative;
			OutputMessageObject.Trarep = new TrarepType
			{
				NamTre1 = representative.Name,
				StrAndNumTre1 = representative.StreetAndNumber,
				PosCodTre1 = representative.PostalCode,
				CitTre1 = representative.City,
				CouCodTre1 = representative.CountryCode,
				Tintre1 = representative.ConfigCode,
			};
		}

		protected void PopulateLodgingSummaryDeclarationPerson()
		{
			var person = InputDeclaration.LodgingSummaryDeclarationPerson;
			OutputMessageObject.Perlodsumdec = new PerlodsumdecType
			{
				NamPld1 = person.Name,
				StrAndNumPld1 = person.StreetAndNumber,
				PosCodPld1 = person.PostalCode,
				CitPld1 = person.City,
				CouCodPld1 = person.CountryCode,
				Tinpld1 = person.ConfigCode,
			};
		}

		protected void PopulateSealsIds()
		{
			var result = new Collection<Seaid529Type>();

			foreach (var sealsId in InputDeclaration.SealsIds)
			{
				result.Add(new Seaid529Type { SeaIdSeaid530 = sealsId.SealsIdentity });
			}

			OutputMessageObject.Seaid529 = result;
		}

		protected void PopulateFirstEntry()
		{
			var firstEntry = InputDeclaration.FirstEntry;
			if (firstEntry != null)
			{
				OutputMessageObject.Cusofffent730 = new Cusofffent730Type
				{
					RefNumCusofffent731 = firstEntry.ReferenceNumber,
					ExpDatOfArrFirent733 = firstEntry.ExpectedDateAndTimeOfArrival.ToISO8601String()
				};
			}
		}

		protected void PopulateSubsequentEntries()
		{
			var result = new Collection<Cusoffsent740Type>();

			foreach (var subsequentEntry in InputDeclaration.SubsequentEntries)
			{
				result.Add(new Cusoffsent740Type { RefNumSubenr909 = subsequentEntry.ReferenceNumber });
			}

			OutputMessageObject.Cusoffsent740 = result;
		}

		protected void PopulateEntryCarrier()
		{
			var entryCarrier = InputDeclaration.EntryCarrier;
			if (entryCarrier != null)
			{
				OutputMessageObject.Tracarent601 = new Tracarent601Type
				{
					NamTracarent604 = entryCarrier.Name,
					StrNumTracarent607 = entryCarrier.StreetAndNumber,
					PstCodTracarent606 = entryCarrier.PostalCode,
					CtyTracarent603 = entryCarrier.City,
					CouCodTracarent605 = entryCarrier.CountryCode,
					Tintracarent602 = entryCarrier.ConfigCode,
				};
			}
		}
	}
}
