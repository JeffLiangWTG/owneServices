using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase4.cc015b;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.GB.GovernmentGateway.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public class CC015BXmlMessageBuilder : XmlMessageBuilder<ICC015BDeclaration, Cc015BType>
	{
		public CC015BXmlMessageBuilder(ICC015BDeclaration wrapper, ErrorCollector errorCollector)
			: base(wrapper, errorCollector)
		{
		}

		#region CC015B Message

		protected override void PopulateMessageHeader(Cc015BType message)
		{
			base.PopulateMessageHeader(message);
			message.MesTypMes20 = CTCMessageTypeList.Codes.CC015B;
		}

		protected override void PopulateMessageBody(Cc015BType message)
		{
			base.PopulateMessageBody(message);
			message.Heahea = PopulateHEAHEAType();
			message.Trapripc1 = PopulateTRAPRIPC1Type();
			message.Traconco1 = PopulateTRACONCO1Type();
			message.Traconce1 = PopulateTRACONCE1Type();
			message.Traautcontra = PopulateTRAAUTCONTRAType();
			message.Cusoffdepept = PopulateCUSOFFDEPEPTType();
			message.Cusofftrarns = PopulateCUSOFFTRARNSType();
			message.Cusoffdesest = PopulateCUSOFFDESESTType();
			message.Conresers = PopulateCONRESERSType();
			message.Reprep = PopulateREPREPType();
			message.Seainfsli = PopulateSEAINFSLIType();
			message.Guagua = PopulateGUAGUAType();
			message.Gooitegds = PopulateGOOITEGDSType();

			if (Wrapper.SafetyAndSecurityData)
			{
				message.Iti = PopulateITIType();
				message.Cartra100 = PopulateCARTRA100Type();
				message.Tracorsec037 = PopulateTRACORSEC037Type();
				message.Traconsec029 = PopulateTRACONSEC029Type();
			}
		}

		HeaheaType PopulateHEAHEAType()
		{
			var data = new HeaheaType();

			data.RefNumHea4 = Wrapper.LocalReferenceNumber;
			data.TypOfDecHea24 = Wrapper.TypeOfDeclaration;
			data.CouOfDesCodHea30 = Wrapper.CountryOfDestinationCode;

			if (wrapper.IsSimplifiedNctsProcedure)
			{
				data.AutLocOfGooCodHea41 = Wrapper.AuthorisedLocationOfGoodsCode;
			}
			else
			{
				data.AgrLocOfGooCodHea38 = Wrapper.AgreedLocationOfGoodsCode;
				data.AgrLocOfGooHea39 = Wrapper.AgreedLocationOfGoods;
				data.AgrLocOfGooHea39Lng = Wrapper.AgreedLocationOfGoodsLanguage.SubstringSafe(0, 2);
			}

			data.PlaOfLoaCodHea46 = Wrapper.PlaceOfLoadingCode;
			data.CouOfDisCodHea55 = Wrapper.CountryOfDispatchExportCode;
			data.CusSubPlaHea66 = Wrapper.CustomsSubPlace;
			data.InlTraModHea75 = Wrapper.InlandTransportMode;
			data.TraModAtBorHea76 = Wrapper.TransportModeAtBorder;
			data.IdeOfMeaOfTraAtDhea78 = Wrapper.IdentityOfMeansOfTransportAtDeparture;
			data.IdeOfMeaOfTraAtDhea78Lng = Wrapper.IdentityOfMeansOfTransportAtDepartureLanguage.SubstringSafe(0, 2);
			data.NatOfMeaOfTraAtDhea80 = Wrapper.NationalityOfMeansOfTransportAtDeparture;
			data.IdeOfMeaOfTraCroHea85 = Wrapper.IdentityOfMeansOfTransportCrossingBorder;
			data.IdeOfMeaOfTraCroHea85Lng = Wrapper.IdentityOfMeansOfTransportCrossingBorderLanguage.SubstringSafe(0, 2);
			data.NatOfMeaOfTraCroHea87 = Wrapper.NationalityOfMeansOfTransportCrossingBorder;
			data.TypOfMeaOfTraCroHea88 = Wrapper.TypeOfMeansOfTransportCrossingBorder;
			data.ConIndHea96 = CTCExtensions.GetBooleanAsString(Wrapper.IsContainerised);
			data.DiaLanIndAtDepHea254 = Wrapper.DialogLanguageIndicatorAtDeparture.SubstringSafe(0, 2);
			data.NctsAccDocHea601Lng = Core.SharedConstants.Languages.English;
			data.TotNumOfIteHea305 = CTCExtensions.GetNumberAsString((ZInt)Wrapper.GoodsItems.Count);
			data.TotNumOfPacHea306 = CTCExtensions.GetNumberAsString(Wrapper.TotalNumberOfPackages);
			data.TotGroMasHea307 = Wrapper.TotalGrossMass.ToStringTrimZeros(3);
			data.DecDatHea383 = Wrapper.DeclarationDate;
			data.DecPlaHea394 = Wrapper.DeclarationPlace;
			data.DecPlaHea394Lng = Wrapper.DeclarationPlaceLanguage.SubstringSafe(0, 2);

			if (Wrapper.SafetyAndSecurityData)
			{
				data.ComRefNumHea = Wrapper.CommercialReferenceNumber;
				data.TraChaMetOfPayHea1 = Wrapper.TransportChargesMethodOfPayment;
				data.SpeCirIndHea1 = Wrapper.SpecificCircumstanceIndicator;
				data.SecHea358 = CTCExtensions.GetBooleanAsString(Wrapper.SafetyAndSecurityData);
				data.ConRefNumHea = Wrapper.ConveyanceReferenceNumber;
				data.CodPlUnHea357 = Wrapper.PlaceOfUnloadingCode;
				data.CodPlUnHea357Lng = Wrapper.PlaceOfUnloadingCodeLanguage.SubstringSafe(0, 2);
			}

			return data;
		}

		Trapripc1Type PopulateTRAPRIPC1Type()
		{
			Trapripc1Type result = null;
			var principal = wrapper.Principal;
			if (principal != null)
			{
				result = new Trapripc1Type();
				if (!principal.TIN.IsEmpty)
				{
					result.Tinpc159 = principal.TIN;
				}
				result.NamPc17 = principal.Name;
				result.StrAndNumPc122 = principal.StreetAndNumber;
				result.PosCodPc123 = principal.PostalCode;
				result.CouPc125 = principal.CountryCode;
				result.CitPc124 = principal.City;
				if (wrapper.IsTIRDeclaration)
				{
					result.Hitpc126 = principal.HolderIDTIR;
				}
			}
			else
			{
				errorCollector.AddError("Principal is empty");
			}
			return result;
		}

		Traconco1Type PopulateTRACONCO1Type()
		{
			Traconco1Type result = null;
			if (!wrapper.IsConsignorDefinedAtGoodsItemLevel)
			{
				var consignor = wrapper.Consignor;
				if (consignor != null)
				{
					result = new Traconco1Type();
					if (!consignor.TIN.IsEmpty)
					{
						result.Tinco159 = consignor.TIN;
					}

					result.NamCo17 = consignor.Name;
					result.StrAndNumCo122 = consignor.StreetAndNumber;
					result.PosCodCo123 = consignor.PostalCode;
					result.CouCo125 = consignor.CountryCode;
					result.CitCo124 = consignor.City;
					result.Traconco1Lng = consignor.NameAndAddressLanguage.SubstringSafe(0, 2);
				}
				else
				{
					errorCollector.AddError("Consignor is empty");
				}
			}
			return result;
		}

		Traconce1Type PopulateTRACONCE1Type()
		{
			Traconce1Type result = null;
			if (!wrapper.IsConsigneeDefinedAtGoodsItemLevel)
			{
				var consignee = wrapper.Consignee;
				if (consignee != null)
				{
					result = new Traconce1Type();
					if (!consignee.TIN.IsEmpty)
					{
						result.Tince159 = consignee.TIN;
					}

					result.NamCe17 = consignee.Name;
					result.StrAndNumCe122 = consignee.StreetAndNumber;
					result.PosCodCe123 = consignee.PostalCode;
					result.CouCe125 = consignee.CountryCode;
					result.CitCe124 = consignee.City;
					result.Nadlngce = consignee.NameAndAddressLanguage.SubstringSafe(0, 2);
				}
				else
				{
					errorCollector.AddError("Consignee is empty");
				}
			}
			return result;
		}

		TraautcontraType PopulateTRAAUTCONTRAType()
		{
			return wrapper.AuthorisedConsigneeTIN.IsEmpty ? null : new TraautcontraType()
			{
				Tintra59 = wrapper.AuthorisedConsigneeTIN
			};
		}

		CusoffdepeptType PopulateCUSOFFDEPEPTType()
		{
			return new CusoffdepeptType()
			{
				RefNumEpt1 = Wrapper.DepartureCustomsOfficeReferenceNumber
			};
		}

		Collection<CusofftrarnsType> PopulateCUSOFFTRARNSType()
		{
			var data = new Collection<CusofftrarnsType>();
			foreach (var o in Wrapper.TransitCustomsOffices)
			{
				var item = new CusofftrarnsType
				{
					RefNumRns1 = o.ReferenceNumber,
					ArrTimTracus085 = o.ArrivalTime
				};
				data.Add(item);
			}
			return data;
		}

		CusoffdesestType PopulateCUSOFFDESESTType()
		{
			return new CusoffdesestType()
			{
				RefNumEst1 = Wrapper.DestinationCustomsOfficeReferenceNumber
			};
		}

		ConresersType PopulateCONRESERSType()
		{
			return (Wrapper.ControlResultCode.IsEmpty && Wrapper.ControlResultDateLimit.IsEmpty) ? null : new ConresersType()
			{
				ConResCodErs16 = Wrapper.ControlResultCode,
				DatLimErs69 = Wrapper.ControlResultDateLimit
			};
		}

		ReprepType PopulateREPREPType()
		{
			ReprepType result = null;
			var declarant = wrapper.Declarant;
			if (declarant != null)
			{
				if (!declarant.Name.IsEmpty)
				{
					result = new ReprepType();
					result.NamRep5 = declarant.Name;
					result.RepCapRep18 = declarant.RepresentativeCapacity;
					result.RepCapRep18Lng = declarant.RepresentativeCapacityLanguage.SubstringSafe(0, 2);
				}
			}
			return result;
		}

		SeainfsliType PopulateSEAINFSLIType()
		{
			int sealsCount = Wrapper.Seals.Count;

			return sealsCount == 0 ? null : new SeainfsliType()
			{
				SeaNumSli2 = sealsCount.ToString(), //check
				Seaidsid = PopulateSEAIDSID()
			};
		}

		Collection<SeaidsidType> PopulateSEAIDSID()
		{
			var data = new Collection<SeaidsidType>();
			foreach (var s in Wrapper.Seals)
			{
				var item = new SeaidsidType
				{
					SeaIdeSid1 = s.SealIdentity,
					SeaIdeSid1Lng = s.SealIdentityLanguage.SubstringSafe(0, 2)
				};
				data.Add(item);
			}
			return data;
		}

		#region Guarantees

		Collection<GuaguaType> PopulateGUAGUAType()
		{
			var data = new Collection<GuaguaType>();
			foreach (var g in wrapper.Guarantees.Cast<IGuarantee>().Take(9))
			{
				var guaGuaType = new GuaguaType
				{
					GuaTypGua1 = g.GuaranteeType,
					Guarefref = PopulateGUAREFREFTypeArray(g)
				};
				data.Add(guaGuaType);
			}
			return data;
		}

		Collection<GuarefrefType> PopulateGUAREFREFTypeArray(IGuarantee g)
		{
			var data = new Collection<GuarefrefType>();
			var guaRefRefType = new GuarefrefType
			{
				GuaRefNumGrnref1 = g.GuaranteeReferenceNumber,
				OthGuaRefRef4 = g.OtherGuaranteeReference,
				AccCodRef6 = g.AccessCode,
				Vallimecvle = new VallimecvleType
				{
					NotValForEcvle1 = g.NotValidForEC
				},
				Vallimnoneclim = PopulateVALLIMNONECLIMTypeArray(g.NotValidForOtherContractingParties)
			};
			data.Add(guaRefRefType);
			return data;
		}

		Collection<VallimnoneclimType> PopulateVALLIMNONECLIMTypeArray(IEnumerable<ZString> notValidForOtherContractingParties)
		{
			var data = new Collection<VallimnoneclimType>();
			foreach (var country in notValidForOtherContractingParties.Where(x => !x.Trim().IsEmpty))
			{
				var item = new VallimnoneclimType
				{
					NotValForOthConPlim2 = country
				};
				data.Add(item);
			}
			return data;
		}

		#endregion

		#region Goods Item

		Collection<GooitegdsType> PopulateGOOITEGDSType()
		{
			var data = new Collection<GooitegdsType>();
			bool firstGoodsItem = true;
			foreach (var giWrapper in Wrapper.GoodsItems)
			{
				var item = new GooitegdsType
				{
					IteNumGds7 = giWrapper.ItemNumber.ToString(),
					ComCodTarCodGds10 = giWrapper.CommodityCode.SubstringSafe(0, 8),
					DecTypGds15 = giWrapper.TypeOfDeclaration,
					GooDesGds23 = giWrapper.GoodsDescription,
					GooDesGds23Lng = giWrapper.GoodsDescriptionLanguage.SubstringSafe(0, 2),
					GroMasGds46 = giWrapper.GrossMass.ToStringTrimZeros(3),
					NetMasGds48 = giWrapper.NetMass.ToStringTrimZeros(3),
					CouOfDisGds58 = giWrapper.CountryOfDispatchExportCode,
					CouOfDesGds59 = giWrapper.CountryOfDestinationCode,
					Preadmrefar2 = PopulatePREADMREFAR2Type(giWrapper),
					Prodocdc2 = PopulatePRODOCDC2Type(giWrapper),
					Spemenmt2 = PopulateSPEMENMT2Type(giWrapper, Wrapper, firstGoodsItem),
					Traconco2 = PopulateTRACONCO2Type(giWrapper),
					Traconce2 = PopulateTRACONCE2Type(giWrapper),
					Connr2 = PopulateCONNR2Type(giWrapper),
					Pacgs2 = PopulatePACGS2Type(giWrapper),
				};

				if (Wrapper.SafetyAndSecurityData)
				{
					item.MetOfPayGdi12 = giWrapper.TransportChargesMethodOfPayment;
					item.ComRefNumGim1 = giWrapper.CommercialReferenceNumber;
					item.UnDanGooCodGdi1 = giWrapper.UNDangerousGoodsCode;
					item.Tracorsecgoo021 = PopulateTRACORSECGOO021Type(giWrapper);
					item.Traconsecgoo013 = PopulateTRACONSECGOO013Type(giWrapper);
				}

				data.Add(item);

				firstGoodsItem = false;
			}
			return data;
		}

		Collection<Preadmrefar2Type> PopulatePREADMREFAR2Type(IDepartureGoodsItem giWrapper)
		{
			var data = new Collection<Preadmrefar2Type>();
			foreach (var giPreviousReferenceWrapper in giWrapper.PreviousAdministrativeReferences)
			{
				var item = new Preadmrefar2Type
				{
					PreDocTypAr21 = giPreviousReferenceWrapper.PreviousDocumentType,
					PreDocRefAr26 = giPreviousReferenceWrapper.PreviousDocumentReference,
					PreDocRefLng = giPreviousReferenceWrapper.PreviousDocumentReferenceLanguage.SubstringSafe(0, 2),
					ComOfInfAr29 = giPreviousReferenceWrapper.ComplementOfInformation,
					ComOfInfAr29Lng = giPreviousReferenceWrapper.ComplementOfInformationLanguage.SubstringSafe(0, 2)
				};
				data.Add(item);
			}
			return data;
		}

		Collection<Prodocdc2Type> PopulatePRODOCDC2Type(IDepartureGoodsItem giWrapper)
		{
			var data = new Collection<Prodocdc2Type>();
			foreach (var giProducedDocumentsWrapper in giWrapper.ProducedDocumentsCertificates)
			{
				var item = new Prodocdc2Type
				{
					DocTypDc21 = giProducedDocumentsWrapper.DocumentType,
					DocRefDc23 = giProducedDocumentsWrapper.DocumentReference,
					DocRefDclng = giProducedDocumentsWrapper.DocumentReferenceLanguage.SubstringSafe(0, 2),
					ComOfInfDc25 = giProducedDocumentsWrapper.ComplementOfInformation,
					ComOfInfDc25Lng = giProducedDocumentsWrapper.ComplementOfInformationLanguage.SubstringSafe(0, 2)
				};
				data.Add(item);
			}
			return data;
		}

		Collection<Spemenmt2Type> PopulateSPEMENMT2Type(IDepartureGoodsItem giWrapper, ICC015BDeclaration wrapper, bool firstGoodsItem)
		{
			var data = new Collection<Spemenmt2Type>();

			if (firstGoodsItem)
			{
				foreach (var guarantee in wrapper.Guarantees.Cast<IGuarantee>().Take(9).Where(x => x.TaxAndDutyLiabiltyAmount > 0))
				{
					var guaranteeLiabiltyAmount = string.Format(CultureInfo.InvariantCulture, "{0:0.##}", guarantee.TaxAndDutyLiabiltyAmount) + "GBP" + guarantee.GuaranteeReferenceNumber;

					var item = new Spemenmt2Type
					{
						AddInfMt21 = guaranteeLiabiltyAmount,
						AddInfCodMt23 = "CAL",
					};
					data.Add(item);
				}
			}

			foreach (var giSpecialMentionsWrapper in giWrapper.SpecialMentions)
			{
				var item = new Spemenmt2Type
				{
					AddInfMt21 = giSpecialMentionsWrapper.StatementText,
					AddInfMt21Lng = ZString.Empty,
					AddInfCodMt23 = giSpecialMentionsWrapper.Statement,
					ExpFroEcmt24 = CTCExtensions.GetBooleanAsString(giSpecialMentionsWrapper.ExportFromEC),
					ExpFroCouMt25 = giSpecialMentionsWrapper.ExportFromCountry
				};
				data.Add(item);
			}
			return data;
		}

		Traconco2Type PopulateTRACONCO2Type(IDepartureGoodsItem giWrapper)
		{
			Traconco2Type result = null;
			if (wrapper.IsConsignorDefinedAtGoodsItemLevel)
			{
				var consignor = giWrapper.Consignor;
				if (consignor != null)
				{
					result = new Traconco2Type();
					if (!consignor.TIN.IsEmpty)
					{
						result.Tinco259 = consignor.TIN;
					}
					result.NamCo27 = consignor.Name;
					result.StrAndNumCo222 = consignor.StreetAndNumber;
					result.PosCodCo223 = consignor.PostalCode;
					result.CouCo225 = consignor.CountryCode;
					result.CitCo224 = consignor.City;
					result.Nadlnggtco = consignor.NameAndAddressLanguage.SubstringSafe(0, 2);
				}
			}
			return result;
		}

		Traconce2Type PopulateTRACONCE2Type(IDepartureGoodsItem giWrapper)
		{
			Traconce2Type result = null;
			if (wrapper.IsConsigneeDefinedAtGoodsItemLevel)
			{
				var consignee = giWrapper.Consignee;
				if (consignee != null)
				{
					result = new Traconce2Type();
					if (!consignee.TIN.IsEmpty)
					{
						result.Tince259 = consignee.TIN;
					}
					result.NamCe27 = consignee.Name;
					result.StrAndNumCe222 = consignee.StreetAndNumber;
					result.PosCodCe223 = consignee.PostalCode;
					result.CouCe225 = consignee.CountryCode;
					result.CitCe224 = consignee.City;
					result.Nadlnggice = consignee.NameAndAddressLanguage.SubstringSafe(0, 2);
				}
			}
			return result;
		}

		Collection<Connr2Type> PopulateCONNR2Type(IDepartureGoodsItem giWrapper)
		{
			var data = new Collection<Connr2Type>();
			foreach (var giContainerWrapper in giWrapper.Containers)
			{
				var item = new Connr2Type
				{
					ConNumNr21 = giContainerWrapper
				};
				data.Add(item);
			}
			return data;
		}

		Collection<Pacgs2Type> PopulatePACGS2Type(IDepartureGoodsItem giWrapper)
		{
			var data = new Collection<Pacgs2Type>();
			foreach (var packageWrapper in giWrapper.Packages)
			{
				var package = new Pacgs2Type
				{
					MarNumOfPacGs21 = packageWrapper.MarksAndNumbersOfPackages,
					MarNumOfPacGs21Lng = packageWrapper.MarksAndNumbersOfPackagesLanguage.SubstringSafe(0, 2),
					KinOfPacGs23 = packageWrapper.KindOfPackages
				};
				if (packageWrapper.IsUnpacked)
				{
					package.NumOfPieGs25 = packageWrapper.NumberOfPieces.ToString();
				}
				else if (!packageWrapper.IsBulk)
				{
					package.NumOfPacGs24 = packageWrapper.NumberOfPackages.ToString();
				}
				data.Add(package);
			}
			return data;
		}

		Tracorsecgoo021Type PopulateTRACORSECGOO021Type(IDepartureGoodsItem giWrapper)
		{
			Tracorsecgoo021Type result = null;
			if (wrapper.HasSecurityAtGoodsItemLevel)
			{
				var consignorSecurity = giWrapper.ConsignorSecurity;
				if (consignorSecurity != null)
				{
					result = new Tracorsecgoo021Type();
					if (!consignorSecurity.TIN.IsEmpty)
					{
						result.Tintracorsecgoo028 = consignorSecurity.TIN;
					}
					else
					{
						result.NamTracorsecgoo025 = consignorSecurity.Name;
						result.StrNumTracorsecgoo027 = consignorSecurity.StreetAndNumber;
						result.PosCodTracorsecgoo026 = consignorSecurity.PostalCode;
						result.CouCodTracorsecgoo023 = consignorSecurity.CountryCode;
						result.CitTracorsecgoo022 = consignorSecurity.City;
						result.Tracorsecgoo021Lng = consignorSecurity.NameAndAddressLanguage.SubstringSafe(0, 2);
					}
				}
			}
			return result;
		}

		Traconsecgoo013Type PopulateTRACONSECGOO013Type(IDepartureGoodsItem giWrapper)
		{
			Traconsecgoo013Type result = null;
			if (wrapper.HasSecurityAtGoodsItemLevel)
			{
				var consigneeSecurity = giWrapper.ConsigneeSecurity;
				if (consigneeSecurity != null)
				{
					result = new Traconsecgoo013Type();
					if (!consigneeSecurity.TIN.IsEmpty)
					{
						result.Tintraconsecgoo020 = consigneeSecurity.TIN;
					}
					else
					{
						result.NamTraconsecgoo017 = consigneeSecurity.Name;
						result.StrNumTraconsecgoo019 = consigneeSecurity.StreetAndNumber;
						result.PosCodTraconsecgoo018 = consigneeSecurity.PostalCode;
						result.CouCodTraconsecgoo015 = consigneeSecurity.CountryCode;
						result.CityTraconsecgoo014 = consigneeSecurity.City;
						result.Traconsecgoo013Lng = consigneeSecurity.NameAndAddressLanguage.SubstringSafe(0, 2);
					}
				}
			}
			return result;
		}

		#endregion Goods Item

		Collection<ItiType> PopulateITIType()
		{
			var data = new Collection<ItiType>();
			foreach (var countryOfRoutingCode in Wrapper.Itinerary)
			{
				var item = new ItiType()
				{
					CouOfRouCodIti1 = countryOfRoutingCode
				};
				data.Add(item);
			}
			return data;
		}

		Cartra100Type PopulateCARTRA100Type()
		{
			Cartra100Type result = null;
			var carrier = wrapper.Carrier;
			if (carrier != null)
			{
				result = new Cartra100Type();
				if (!carrier.TIN.IsEmpty)
				{
					result.Tincartra254 = carrier.TIN;
				}
				else
				{
					result.NamCartra121 = carrier.Name;
					result.StrAndNumCartra254 = carrier.StreetAndNumber;
					result.PosCodCartra121 = carrier.PostalCode;
					result.CouCodCartra587 = carrier.CountryCode;
					result.CitCartra789 = carrier.City;
					result.Nadcartra121 = carrier.NameAndAddressLanguage.SubstringSafe(0, 2);
				}
			}
			return result;
		}

		Tracorsec037Type PopulateTRACORSEC037Type()
		{
			Tracorsec037Type result = null;
			if (!wrapper.HasSecurityAtGoodsItemLevel)
			{
				var securityConsignor = wrapper.SecurityConsignor;
				if (securityConsignor != null)
				{
					result = new Tracorsec037Type();
					if (!securityConsignor.TIN.IsEmpty)
					{
						result.Tintracorsec044 = securityConsignor.TIN;
					}
					else
					{
						result.NamTracorsec041 = securityConsignor.Name;
						result.StrNumTracorsec043 = securityConsignor.StreetAndNumber;
						result.PosCodTracorsec042 = securityConsignor.PostalCode;
						result.CouCodTracorsec039 = securityConsignor.CountryCode;
						result.CitTracorsec038 = securityConsignor.City;
						result.Tracorsec037Lng = securityConsignor.NameAndAddressLanguage.SubstringSafe(0, 2);
					}
				}
			}
			return result;
		}

		Traconsec029Type PopulateTRACONSEC029Type()
		{
			Traconsec029Type result = null;
			if (!wrapper.HasSecurityAtGoodsItemLevel)
			{
				var securityConsignee = wrapper.SecurityConsignee;
				if (securityConsignee != null)
				{
					result = new Traconsec029Type();
					if (!securityConsignee.TIN.IsEmpty)
					{
						result.Tintraconsec036 = securityConsignee.TIN;
					}
					else
					{
						result.NameTraconsec033 = securityConsignee.Name;
						result.StrNumTraconsec035 = securityConsignee.StreetAndNumber;
						result.PosCodTraconsec034 = securityConsignee.PostalCode;
						result.CouCodTraconsec031 = securityConsignee.CountryCode;
						result.CitTraconsec030 = securityConsignee.City;
						result.Traconsec029Lng = securityConsignee.NameAndAddressLanguage.SubstringSafe(0, 2);
					}
				}
			}
			return result;
		}

		#endregion CC015B Message
	}
}
