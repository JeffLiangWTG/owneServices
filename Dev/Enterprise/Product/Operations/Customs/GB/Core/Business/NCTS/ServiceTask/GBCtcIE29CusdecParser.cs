using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase4.CC029B;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;

namespace Enterprise.Customs.GB.Business
{
	public class GBCtcIE29CusdecParser : NctsIE29CusdecParser
	{
		public GBCtcIE29CusdecParser()
			: base()
		{
		}

		public override NctsIE29CusdecResponseData Parse()
		{
			if (!(EdiMessage?.EM_MessageText ?? ZString.Empty).IsEmpty)
			{
				var message = EdiMessage.EM_MessageText;
				xmlMessage = CTCExtensions.Deserialize<Cc029BType>(message);
				ie29ResponseData = new NctsIE29CusdecResponseData(Factory);

				if (xmlMessage.Heahea != null)
				{
					ie29ResponseData.LocalReferenceNumber = xmlMessage.Heahea?.RefNumHea4;
					ie29ResponseData.MovementReferenceNumber = xmlMessage.Heahea?.DocNumHea5;
					ie29ResponseData.DeclarationType = xmlMessage.Heahea?.TypOfDecHea24;
					ie29ResponseData.CountryOfDestination = xmlMessage.Heahea?.CouOfDesCodHea30;
					ie29ResponseData.AgreedLocationOfGoodsCode = xmlMessage.Heahea?.AgrLocOfGooCodHea38;
					ie29ResponseData.AgreedLocationOfGoods = xmlMessage.Heahea?.AgrLocOfGooHea39;
					ie29ResponseData.AuthorisedLocationOfGoodsCode = xmlMessage.Heahea?.AutLocOfGooCodHea41;
					ie29ResponseData.PlaceOfLoadingCode = xmlMessage.Heahea?.PlaOfLoaCodHea46;
					ie29ResponseData.CountryOfDispatch = xmlMessage.Heahea?.CouOfDisCodHea55;
					ie29ResponseData.CustomsSubPlace = xmlMessage.Heahea?.CusSubPlaHea66;

					ie29ResponseData.InlandTransportMode = xmlMessage.Heahea?.InlTraModHea75;
					ie29ResponseData.TransportModeAtBorder = xmlMessage.Heahea?.TraModAtBorHea76;
					ie29ResponseData.MeansOfTransportCrossingBorderIdentity = xmlMessage.Heahea?.IdeOfMeaOfTraCroHea85;
					ie29ResponseData.MeansOfTransportCrossingBorderNationality = xmlMessage.Heahea?.NatOfMeaOfTraCroHea87;
					ie29ResponseData.MeansOfTransportCrossingBorderType = xmlMessage.Heahea?.TypOfMeaOfTraCroHea88;
					ie29ResponseData.MeansOfTransportAtDepartureIdentity = xmlMessage.Heahea?.IdeOfMeaOfTraAtDhea78;
					ie29ResponseData.MeansOfTransportAtDepartureNationality = xmlMessage.Heahea?.NatOfMeaOfTraAtDhea80;
					ie29ResponseData.IsContainerised = xmlMessage.Heahea?.ConIndHea96 != "0";
					ie29ResponseData.NctsReturnCopy = xmlMessage.Heahea?.NctRetCopHea104;
					ie29ResponseData.AcceptanceDate = xmlMessage.Heahea?.AccDatHea158;
					ie29ResponseData.IssuingDate = xmlMessage.Heahea?.IssDatHea186;
					ie29ResponseData.DialogLanguageIndicatorAtDeparture = xmlMessage.Heahea?.DiaLanIndAtDepHea254;
					ie29ResponseData.NctsAccompanyingDocumentLanguageCode = xmlMessage.Heahea?.NctsAccDocHea601Lng;
					ie29ResponseData.TotalNumberOfItems = xmlMessage.Heahea?.TotNumOfIteHea305;
					ie29ResponseData.TotalNumberOfPackages = xmlMessage.Heahea?.TotNumOfPacHea306;
					ie29ResponseData.TotalGrossMassInKilograms = xmlMessage.Heahea?.TotGroMasHea307;
					ie29ResponseData.BindingItinerary = xmlMessage.Heahea?.BinItiHea246;
					ie29ResponseData.AuthorisationId = xmlMessage.Heahea?.AutIdHea380;
					ie29ResponseData.DeclarationDate = xmlMessage.Heahea?.DecDatHea383;
					ie29ResponseData.DeclarationPlace = xmlMessage.Heahea?.DecPlaHea394;
					ie29ResponseData.SpecificCircumstanceIndicator = xmlMessage.Heahea?.SpeCirIndHea1;
					ie29ResponseData.TransportChargesMoP = xmlMessage.Heahea?.TraChaMetOfPayHea1;
					ie29ResponseData.CommercialReferenceNumber = xmlMessage.Heahea?.ComRefNumHea;
					ie29ResponseData.IsSecurity = xmlMessage.Heahea?.SecHea358 != "0";
					ie29ResponseData.ConveyanceReferenceNumber = xmlMessage.Heahea?.ConRefNumHea;
					ie29ResponseData.PlaceOfUnloadingCode = xmlMessage.Heahea?.CodPlUnHea357;
				}

				ie29ResponseData.Principal = GetPrincipalAddressResponseData(xmlMessage.Trapripc1);
				ie29ResponseData.Consignor = GetConsignorAddressResponseData(xmlMessage.Traconco1);
				ie29ResponseData.Consignee = GetConsigneeAddressResponseData(xmlMessage.Traconce1);
				ie29ResponseData.AuthorisedConsigneeEori = xmlMessage.Traautcontra?.Tintra59 ?? ZString.Empty;

				ie29ResponseData.DepartureCustomsOfficeCode = xmlMessage.Cusoffdepept?.RefNumEpt1 ?? ZString.Empty;

				SetTransitOffices();

				ie29ResponseData.DestinationCustomsOfficeCode = xmlMessage.Cusoffdesest?.RefNumEst1 ?? ZString.Empty;

				ie29ResponseData.ReturnCopiesCustomsOffice = GetOfficeAddressResponseData();

				ie29ResponseData.ControlResultControlDate = xmlMessage.Conresers?.ConDatErs14;
				ie29ResponseData.ControlResultControlResultCode = xmlMessage.Conresers?.ConResCodErs16;
				ie29ResponseData.ControlResultControlledBy = xmlMessage.Conresers?.ConByErs18;
				ie29ResponseData.ControlResultTimeLimit = xmlMessage.Conresers?.DatLimErs69;

				ie29ResponseData.RepresentativeName = xmlMessage.Reprep?.NamRep5 ?? ZString.Empty;
				ie29ResponseData.RepresentativeCapacity = xmlMessage.Reprep?.RepCapRep18 ?? ZString.Empty;

				SetGuarantees();
				SetGoodsItems();
				SetSeals();
				SetItinerary();

				ie29ResponseData.Carrier = GetCarrierAddressResponseData(xmlMessage.Cartra100);
				ie29ResponseData.SecurityConsignor = GetSecurityConsignorAddressResponseData(xmlMessage.Tracorsec037);
				ie29ResponseData.SecurityConsignee = GetSecurityConsigneeAddressResponseData(xmlMessage.Traconsec029);
			}
			return ie29ResponseData;
		}

		void SetSeals()
		{
			ie29ResponseData.SealsNumber = xmlMessage.Seainfsli?.SeaNumSli2 ?? ZString.Empty;
			if (xmlMessage.Seainfsli?.Seaidsid != null)
			{
				ie29ResponseData.Seals = new List<ZString>();

				foreach (var seals in xmlMessage.Seainfsli.Seaidsid)
				{
					ie29ResponseData.Seals.Add(seals.SeaIdeSid1);
				}
			}
		}

		void SetGuarantees()
		{
			if (xmlMessage.Guagua != null)
			{
				foreach (var guaranteeHeader in xmlMessage.Guagua)
				{
					var guarantee = new GuaranteeResponseData();
					guarantee.GuaranteeType = guaranteeHeader.GuaTypGua1;
					var guarefref = guaranteeHeader.Guarefref;
					if (guarefref != null && guarefref.Any())
					{
						var lastGuaRef = guarefref.Last();
						guarantee.GuaranteeReferenceNumber = lastGuaRef.GuaRefNumGrnref1;
						guarantee.OtherGuaranteeReference = lastGuaRef.OthGuaRefRef4;
						guarantee.AccessCode = lastGuaRef.AccCodRef6;
						guarantee.ValidityLimitationEC = lastGuaRef.Vallimecvle?.NotValForEcvle1 != "0";

						var vallimnoneclim = lastGuaRef.Vallimnoneclim;
						if (vallimnoneclim != null && vallimnoneclim.Any())
						{
							var lastValli = vallimnoneclim.Last();
							guarantee.ValidityLimitationOther = lastValli.NotValForOthConPlim2;
						}
					}
					ie29ResponseData.Guarantees.Add(guarantee);
				}
			}
		}

		void SetGoodsItems()
		{
			var totalNetMassInKilograms = ZDecimal.Zero;

			ie29ResponseData.GoodsItems = new List<NctsGoodsItemResponseData>();

			if (xmlMessage.Gooitegds != null)
			{
				foreach (var goods in xmlMessage.Gooitegds)
				{
					var goodsItem = new NctsGoodsItemResponseData(ie29ResponseData);
					goodsItem.ItemNumber = goods.IteNumGds7;
					goodsItem.CommodityCode = goods.ComCodTarCodGds10;
					goodsItem.DeclarationType = goods.DecTypGds15;
					goodsItem.DescriptionOfGoods = goods.GooDesGds23;
					goodsItem.GrossMassInKilograms = goods.GroMasGds46;
					goodsItem.NetMassInKilograms = goods.NetMasGds48;
					goodsItem.GrossMassInKilograms = goods.GroMasGds46;
					goodsItem.CountryOfDispatch = goods.CouOfDisGds58;
					goodsItem.CountryOfDestination = goods.CouOfDesGds59;
					goodsItem.TransportChargesMoP = goods.MetOfPayGdi12;
					goodsItem.CommercialReferenceNumber = goods.ComRefNumGim1;
					goodsItem.UNDangerousGoodsCode = goods.UnDanGooCodGdi1;
					SetPreviousDocuments(goodsItem, goods);
					SetSupportingDocuments(goodsItem, goods);
					SetSpecialMentions(goodsItem, goods);

					if (goods.Traconco2 != null)
					{
						goodsItem.Consignor = GetGoodsItemConsignorAddressResponseData(goods.Traconco2);
					}

					if (goods.Traconce2 != null)
					{
						goodsItem.Consignee = GetGoodsItemConsigneeAddressResponseData(goods.Traconce2);
					}

					if (goods.Connr2 != null)
					{
						SetContainers(goodsItem, goods.Connr2);
					}

					if (goods.Pacgs2 != null)
					{
						SetPackages(goodsItem, goods.Pacgs2);
					}

					if (goods.Tracorsecgoo021 != null)
					{
						goodsItem.SecurityConsignor = GetGoodsItemSecurityConsignorAddressResponseData(goods.Tracorsecgoo021);
					}

					if (goods.Traconsecgoo013 != null)
					{
						goodsItem.SecurityConsignee = GetGoodsItemSecurityConsigneeAddressResponseData(goods.Traconsecgoo013);
					}

					ie29ResponseData.GoodsItems.Add(goodsItem);
				}
			}
			ie29ResponseData.TotalNettMassInKilograms = totalNetMassInKilograms.ToString();
		}

		void SetPackages(NctsGoodsItemResponseData goodsItem, IEnumerable<Pacgs2Type> packages)
		{
			foreach (var packageItem in packages) // 99 Packages
			{
				var package = new PackageResponseData();
				package.MarksAndNumbers = packageItem.MarNumOfPacGs21;
				package.PackageType = packageItem.KinOfPacGs23;
				package.NumberOfPackages = packageItem.NumOfPacGs24;
				package.NumberOfPieces = packageItem.NumOfPieGs25;
				goodsItem.Packages.Add(package);
			}
		}

		void SetContainers(NctsGoodsItemResponseData goodsItem, IEnumerable<Connr2Type> containers)
		{
			foreach (var container in containers) // 99 Containers
			{
				goodsItem.ContainerNumbers.Add(container.ConNumNr21);
			}
		}

		void SetSupportingDocuments(NctsGoodsItemResponseData goodsItem, GooitegdsType goods)
		{
			if (goods.Prodocdc2 != null)
			{
				foreach (var doc in goods.Prodocdc2)
				{
					var sd = new SupportingDocumentResponseData();
					sd.TypeCode = doc.DocTypDc21;
					sd.Reference = doc.DocRefDc23;
					sd.Reason = doc.ComOfInfDc25;
					goodsItem.SupportingDocuments.Add(sd);
				}
			}
		}

		void SetPreviousDocuments(NctsGoodsItemResponseData goodsItem, GooitegdsType goods)
		{
			if (goods.Preadmrefar2 != null)
			{
				foreach (var doc in goods.Preadmrefar2)
				{
					var pd = new PreviousDocumentResponseData();
					pd.TypeCode = doc.PreDocTypAr21;
					pd.Reference = doc.PreDocRefAr26;
					pd.MoreInfo = doc.ComOfInfAr29;
					goodsItem.PreviousDocuments.Add(pd);
				}
			}
		}

		void SetSpecialMentions(NctsGoodsItemResponseData goodsItem, GooitegdsType goods)
		{
			if (goods.Spemenmt2 != null)
			{
				foreach (var specialmention in goods.Spemenmt2) // 99 Special Mentions
				{
					var sm = new SpecialMentionResponseData();
					sm.Description = specialmention.AddInfMt21;
					sm.TypeCode = specialmention.AddInfCodMt23;

					sm.NctsExportFromEC = specialmention.ExpFroEcmt24 != "0";
					sm.NctsExportFromCountry = specialmention.ExpFroCouMt25;
					goodsItem.SpecialMentions.Add(sm);
				}
			}
		}

		void SetTransitOffices()
		{
			if (xmlMessage.Cusofftrarns != null)
			{
				foreach (var locSegment in xmlMessage.Cusofftrarns)
				{
					var transitOffice = new CustomsOfficeResponseData();
					transitOffice.OfficeCode = locSegment.RefNumRns1;
					transitOffice.Time = locSegment.ArrTimTracus085;
					ie29ResponseData.TransitCustomsOffices.Add(transitOffice);
				}
			}
		}

		#region addresse
		AddressResponseData GetOfficeAddressResponseData()
		{
			var addressResponseData = new AddressResponseData();
			if (xmlMessage.Cusoffretcopocp != null)
			{
				addressResponseData.CompanyName = xmlMessage.Cusoffretcopocp.RefNumOcp1;
				addressResponseData.Address1 = xmlMessage.Cusoffretcopocp.StrAndNumOcp3;
				addressResponseData.Postcode = xmlMessage.Cusoffretcopocp.PosCodOcp6;
				addressResponseData.City = xmlMessage.Cusoffretcopocp.CitOcp7;
				addressResponseData.CountryCode = xmlMessage.Cusoffretcopocp.CouOcp4;
			}
			return addressResponseData;
		}

		AddressResponseData GetPrincipalAddressResponseData(Trapripc1Type tag)
		{
			var addressResponseData = new AddressResponseData();
			if (tag != null)
			{
				addressResponseData.CompanyName = tag.NamPc17;
				addressResponseData.Address1 = tag.StrAndNumPc122;
				addressResponseData.Postcode = tag.PosCodPc123;
				addressResponseData.City = tag.CitPc124;
				addressResponseData.CountryCode = tag.CouPc125;
				addressResponseData.GovRegNum = tag.Tinpc159;
				addressResponseData.AuthorisedNumber = tag.Hitpc126;
			}
			return addressResponseData;
		}

		AddressResponseData GetConsignorAddressResponseData(Traconco1Type tag)
		{
			var addressResponseData = new AddressResponseData();
			if (tag != null)
			{
				addressResponseData.CompanyName = tag.NamCo17;
				addressResponseData.Address1 = tag.StrAndNumCo122;
				addressResponseData.Postcode = tag.PosCodCo123;
				addressResponseData.City = tag.CitCo124;
				addressResponseData.CountryCode = tag.CouCo125;
				addressResponseData.GovRegNum = tag.Tinco159;
			}
			return addressResponseData;
		}

		AddressResponseData GetConsigneeAddressResponseData(Traconce1Type tag)
		{
			var addressResponseData = new AddressResponseData();
			if (tag != null)
			{
				addressResponseData.CompanyName = tag.NamCe17;
				addressResponseData.Address1 = tag.StrAndNumCe122;
				addressResponseData.Postcode = tag.PosCodCe123;
				addressResponseData.City = tag.CitCe124;
				addressResponseData.CountryCode = tag.CouCe125;
				addressResponseData.GovRegNum = tag.Tince159;
			}
			return addressResponseData;
		}

		AddressResponseData GetCarrierAddressResponseData(Cartra100Type tag)
		{
			var addressResponseData = new AddressResponseData();
			if (tag != null)
			{
				addressResponseData.CompanyName = tag.NamCartra121;
				addressResponseData.Address1 = tag.StrAndNumCartra254;
				addressResponseData.Postcode = tag.PosCodCartra121;
				addressResponseData.City = tag.CitCartra789;
				addressResponseData.CountryCode = tag.CouCodCartra587;
				addressResponseData.GovRegNum = tag.Tincartra254;
			}
			return addressResponseData;
		}

		AddressResponseData GetSecurityConsignorAddressResponseData(Tracorsec037Type tag)
		{
			var addressResponseData = new AddressResponseData();
			if (tag != null)
			{
				addressResponseData.CompanyName = tag.NamTracorsec041;
				addressResponseData.Address1 = tag.StrNumTracorsec043;
				addressResponseData.Postcode = tag.PosCodTracorsec042;
				addressResponseData.City = tag.CitTracorsec038;
				addressResponseData.CountryCode = tag.CouCodTracorsec039;
				addressResponseData.GovRegNum = tag.Tintracorsec044;
			}
			return addressResponseData;
		}

		AddressResponseData GetSecurityConsigneeAddressResponseData(Traconsec029Type tag)
		{
			var addressResponseData = new AddressResponseData();
			if (tag != null)
			{
				addressResponseData.CompanyName = tag.NameTraconsec033;
				addressResponseData.Address1 = tag.StrNumTraconsec035;
				addressResponseData.Postcode = tag.PosCodTraconsec034;
				addressResponseData.City = tag.CitTraconsec030;
				addressResponseData.CountryCode = tag.CouCodTraconsec031;
				addressResponseData.GovRegNum = tag.Tintraconsec036;
			}
			return addressResponseData;
		}

		AddressResponseData GetGoodsItemConsigneeAddressResponseData(Traconce2Type tag)
		{
			var addressResponseData = new AddressResponseData();
			if (tag != null)
			{
				addressResponseData.CompanyName = tag.NamCe27;
				addressResponseData.Address1 = tag.StrAndNumCe222;
				addressResponseData.Postcode = tag.PosCodCe223;
				addressResponseData.City = tag.CitCe224;
				addressResponseData.CountryCode = tag.CouCe225;
				addressResponseData.GovRegNum = tag.Tince259;
			}
			return addressResponseData;
		}

		AddressResponseData GetGoodsItemConsignorAddressResponseData(Traconco2Type tag)
		{
			var addressResponseData = new AddressResponseData();
			if (tag != null)
			{
				addressResponseData.CompanyName = tag.NamCo27;
				addressResponseData.Address1 = tag.StrAndNumCo222;
				addressResponseData.Postcode = tag.PosCodCo223;
				addressResponseData.City = tag.CitCo224;
				addressResponseData.CountryCode = tag.CouCo225;
				addressResponseData.GovRegNum = tag.Tinco259;
			}
			return addressResponseData;
		}

		AddressResponseData GetGoodsItemSecurityConsignorAddressResponseData(Tracorsecgoo021Type tag)
		{
			var addressResponseData = new AddressResponseData();
			if (tag != null)
			{
				addressResponseData.CompanyName = tag.NamTracorsecgoo025;
				addressResponseData.Address1 = tag.StrNumTracorsecgoo027;
				addressResponseData.Postcode = tag.PosCodTracorsecgoo026;
				addressResponseData.City = tag.CitTracorsecgoo022;
				addressResponseData.CountryCode = tag.CouCodTracorsecgoo023;
				addressResponseData.GovRegNum = tag.Tintracorsecgoo028;
			}
			return addressResponseData;
		}

		AddressResponseData GetGoodsItemSecurityConsigneeAddressResponseData(Traconsecgoo013Type tag)
		{
			var addressResponseData = new AddressResponseData();
			if (tag != null)
			{
				addressResponseData.CompanyName = tag.NamTraconsecgoo017;
				addressResponseData.Address1 = tag.StrNumTraconsecgoo019;
				addressResponseData.Postcode = tag.PosCodTraconsecgoo018;
				addressResponseData.City = tag.CityTraconsecgoo014;
				addressResponseData.CountryCode = tag.CouCodTraconsecgoo015;
				addressResponseData.GovRegNum = tag.Tintraconsecgoo020;
			}
			return addressResponseData;
		}
		#endregion

		void SetItinerary()
		{
			if (xmlMessage.Iti != null)
			{
				var sb = new ZStringBuilder();
				foreach (var eqd in xmlMessage.Iti)
				{
					sb.Append(eqd.CouOfRouCodIti1);
				}
				ie29ResponseData.Itinerary = sb.ToStringWithDelimiterBetweenAppends(" ");
			}
		}
		Cc029BType xmlMessage;
	}
}
