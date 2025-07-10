using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC029B;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;

namespace Enterprise.Customs.FR.Business.ServiceTasks
{
	public class NctsIE29CusdecParser : EU.NCTS.Business.ServiceTask.NctsIE29CusdecParser
	{
		public NctsIE29CusdecParser() : base()
		{
		}

		public override NctsIE29CusdecResponseData Parse()
		{
			if (!(EdiMessage?.EM_MessageText ?? ZString.Empty).IsEmpty)
			{
				var message = MessageProcessors.ProcessorHelper.AdaptMessageToExpectedFormat(EdiMessage.EM_MessageText);
				cusdecMessage = Extensions.Deserialize<Cc029BType>(message);
				ie29ResponseData = new NctsIE29CusdecResponseData(Factory);

				if (cusdecMessage.Heahea != null)
				{
					ie29ResponseData.LocalReferenceNumber = cusdecMessage.Heahea.RefNumHea4;
					ie29ResponseData.MovementReferenceNumber = cusdecMessage.Heahea.DocNumHea5;
					ie29ResponseData.DeclarationType = cusdecMessage.Heahea.TypOfDecHea24;
					ie29ResponseData.CountryOfDestination = cusdecMessage.Heahea.CouOfDesCodHea30;
					ie29ResponseData.AgreedLocationOfGoodsCode = cusdecMessage.Heahea.AgrLocOfGooCodHea38;
					ie29ResponseData.AgreedLocationOfGoods = cusdecMessage.Heahea.AgrLocOfGooHea39;
					ie29ResponseData.AuthorisedLocationOfGoodsCode = cusdecMessage.Heahea.AutLocOfGooCodHea41;
					ie29ResponseData.PlaceOfLoadingCode = cusdecMessage.Heahea.PlaOfLoaCodHea46;
					ie29ResponseData.CountryOfDispatch = cusdecMessage.Heahea.CouOfDisCodHea55;
					ie29ResponseData.CustomsSubPlace = cusdecMessage.Heahea.CusSubPlaHea66;

					ie29ResponseData.InlandTransportMode = cusdecMessage.Heahea.InlTraModHea75;
					ie29ResponseData.TransportModeAtBorder = cusdecMessage.Heahea.TraModAtBorHea76;
					ie29ResponseData.MeansOfTransportCrossingBorderIdentity = cusdecMessage.Heahea.IdeOfMeaOfTraCroHea85;
					ie29ResponseData.MeansOfTransportCrossingBorderNationality = cusdecMessage.Heahea.NatOfMeaOfTraCroHea87;
					ie29ResponseData.MeansOfTransportCrossingBorderType = cusdecMessage.Heahea.TypOfMeaOfTraCroHea88;
					ie29ResponseData.MeansOfTransportAtDepartureIdentity = cusdecMessage.Heahea.IdeOfMeaOfTraAtDhea78;
					ie29ResponseData.MeansOfTransportAtDepartureNationality = cusdecMessage.Heahea.NatOfMeaOfTraAtDhea80;
					ie29ResponseData.IsContainerised = cusdecMessage.Heahea.ConIndHea96 == Flag.Item1;
					ie29ResponseData.NctsReturnCopy = cusdecMessage.Heahea.NctRetCopHea104 == Flag.Item1 ? "1" : "0";
					ie29ResponseData.AcceptanceDate = cusdecMessage.Heahea.AccDatHea158;
					ie29ResponseData.IssuingDate = cusdecMessage.Heahea.IssDatHea186;
					ie29ResponseData.DialogLanguageIndicatorAtDeparture = cusdecMessage.Heahea.DiaLanIndAtDepHea254;
					ie29ResponseData.NctsAccompanyingDocumentLanguageCode = cusdecMessage.Heahea.NctsAccDocHea601Lng;
					ie29ResponseData.TotalNumberOfItems = cusdecMessage.Heahea.TotNumOfIteHea305;
					ie29ResponseData.TotalNumberOfPackages = cusdecMessage.Heahea.TotNumOfPacHea306;
					ie29ResponseData.TotalGrossMassInKilograms = cusdecMessage.Heahea.TotGroMasHea307.ToString();
					ie29ResponseData.BindingItinerary = cusdecMessage.Heahea.BinItiHea246 == Flag.Item0 ? "0" : "1";
					ie29ResponseData.AuthorisationId = cusdecMessage.Heahea.AutIdHea380;
					ie29ResponseData.DeclarationDate = cusdecMessage.Heahea.DecDatHea383;
					ie29ResponseData.DeclarationPlace = cusdecMessage.Heahea.DecPlaHea394;
					ie29ResponseData.SpecificCircumstanceIndicator = cusdecMessage.Heahea.SpeCirIndHea1;
					ie29ResponseData.TransportChargesMoP = cusdecMessage.Heahea.TraChaMetOfPayHea1;
					ie29ResponseData.CommercialReferenceNumber = cusdecMessage.Heahea.ComRefNumHea;
					ie29ResponseData.IsSecurity = cusdecMessage.Heahea.SecHea358 == SecurityIndicator.Item1;
					ie29ResponseData.ConveyanceReferenceNumber = cusdecMessage.Heahea.ConRefNumHea;
					ie29ResponseData.PlaceOfUnloadingCode = cusdecMessage.Heahea.CodPlUnHea357;
				}

				ie29ResponseData.Principal = GetPrincipalAddressResponseData(cusdecMessage.Trapripc1);
				ie29ResponseData.Consignor = GetConsignorAddressResponseData(cusdecMessage.Traconco1);
				ie29ResponseData.Consignee = GetConsigneeAddressResponseData(cusdecMessage.Traconce1);
				ie29ResponseData.AuthorisedConsigneeEori = cusdecMessage.Traautcontra?.Tintra59 ?? ZString.Empty;

				ie29ResponseData.DepartureCustomsOfficeCode = cusdecMessage.Cusoffdepept?.RefNumEpt1 ?? ZString.Empty;

				SetTransitOffices();

				ie29ResponseData.DestinationCustomsOfficeCode = cusdecMessage.Cusoffdesest?.RefNumEst1 ?? ZString.Empty;

				ie29ResponseData.ReturnCopiesCustomsOffice = GetOfficeAddressResponseData();

				ie29ResponseData.ControlResultControlDate = cusdecMessage.Conresers.ConDatErs14;
				ie29ResponseData.ControlResultControlResultCode = cusdecMessage.Conresers.ConResCodErs16;
				ie29ResponseData.ControlResultControlledBy = cusdecMessage.Conresers.ConByErs18;
				ie29ResponseData.ControlResultTimeLimit = cusdecMessage.Conresers.DatLimErs69;

				ie29ResponseData.RepresentativeName = cusdecMessage.Reprep?.NamRep5 ?? ZString.Empty;
				ie29ResponseData.RepresentativeCapacity = cusdecMessage.Reprep?.RepCapRep18 ?? ZString.Empty;

				SetGuarantees();
				SetGoodsItems();
				SetSeals();
				SetItinerary();

				ie29ResponseData.Carrier = GetCarrierAddressResponseData(cusdecMessage.Cartra100);
				ie29ResponseData.SecurityConsignor = GetSecurityConsignorAddressResponseData(cusdecMessage.Tracorsec037);
				ie29ResponseData.SecurityConsignee = GetSecurityConsigneeAddressResponseData(cusdecMessage.Traconsec029);
			}
			return ie29ResponseData;
		}

		void SetSeals()
		{
			ie29ResponseData.SealsNumber = cusdecMessage.Seainfsli?.SeaNumSli2 ?? ZString.Empty;
			if (cusdecMessage.Seainfsli?.Seaidsid != null)
			{
				ie29ResponseData.Seals = new List<ZString>();

				foreach (var seals in cusdecMessage.Seainfsli.Seaidsid)
				{
					ie29ResponseData.Seals.Add(seals.SeaIdeSid1);
				}
			}
		}

		void SetGuarantees()
		{
			if (cusdecMessage.Guagua != null)
			{
				foreach (var guaranteeHeader in cusdecMessage.Guagua)
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
						guarantee.ValidityLimitationEC = lastGuaRef.Vallimecvle?.NotValForEcvle1 == Flag.Item1;

						var vallimnoneclim = lastGuaRef.Vallimnoneclim;
						if (vallimnoneclim != null && vallimnoneclim.Any())
						{
							var lastValli = vallimnoneclim.Last();
							guarantee.ValidityLimitationOther = lastValli.NotValForOthConPlim2.ToString();
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

			if (cusdecMessage.Gooitegds != null)
			{
				foreach (var goods in cusdecMessage.Gooitegds)
				{
					var goodsItem = new NctsGoodsItemResponseData(ie29ResponseData);
					goodsItem.ItemNumber = goods.IteNumGds7;
					goodsItem.CommodityCode = goods.ComCodTarCodGds10;
					goodsItem.DeclarationType = goods.DecTypGds15;
					goodsItem.DescriptionOfGoods = goods.GooDesGds23;
					goodsItem.GrossMassInKilograms = goods.GroMasGds46.ToString();
					goodsItem.NetMassInKilograms = goods.NetMasGds48.ToString();
					totalNetMassInKilograms += goods.NetMasGds48 ?? decimal.Zero;
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

		void SetPackages(NctsGoodsItemResponseData goodsItem, Collection<Pacgs2Type> packages)
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

		void SetContainers(NctsGoodsItemResponseData goodsItem, Collection<Connr2Type> containers)
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

					sm.NctsExportFromEC = specialmention.ExpFroEcmt24 == Flag.Item1;
					sm.NctsExportFromCountry = specialmention.ExpFroCouMt25;
					goodsItem.SpecialMentions.Add(sm);
				}
			}
		}

		void SetTransitOffices()
		{
			if (cusdecMessage.Cusofftrarns != null)
			{
				foreach (var locSegment in cusdecMessage.Cusofftrarns)
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
			if (cusdecMessage.Cusoffretcopocp != null)
			{
				addressResponseData.CompanyName = cusdecMessage.Cusoffretcopocp.RefNumOcp1;
				addressResponseData.Address1 = cusdecMessage.Cusoffretcopocp.StrAndNumOcp3;
				addressResponseData.Postcode = cusdecMessage.Cusoffretcopocp.PosCodOcp6;
				addressResponseData.City = cusdecMessage.Cusoffretcopocp.CitOcp7;
				addressResponseData.CountryCode = cusdecMessage.Cusoffretcopocp.CouOcp4;
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
			if (cusdecMessage.Iti != null)
			{
				var sb = new ZStringBuilder();
				foreach (var eqd in cusdecMessage.Iti)
				{
					sb.Append(eqd.CouOfRouCodIti1);
				}
				ie29ResponseData.Itinerary = sb.ToStringWithDelimiterBetweenAppends(" ");
			}
		}
		Cc029BType cusdecMessage;
	}
}
