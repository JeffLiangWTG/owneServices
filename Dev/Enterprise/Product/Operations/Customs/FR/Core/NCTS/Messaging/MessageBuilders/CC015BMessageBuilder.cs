using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC015B;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.COMPLEX_NCTS;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL_NATIONAL;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Core;
using REPREPType = CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC015B.ReprepType;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class CC015BMessageBuilder : NctsXmlMessageBuilder<ICC015BDeclaration, NctsMessageFunctionSet.DeclarationDataMessage, Cc015BType>
	{
		public CC015BMessageBuilder(ICC015BDeclaration wrapper, NctsMessageFunctionSet.DeclarationDataMessage messageFunction, ErrorCollector errorCollector) : base(wrapper, messageFunction, errorCollector)
		{
		}

		protected override void PopulateMessageHeader(Cc015BType message)
		{
			base.PopulateMessageHeader(message);
			message.MesTypMes20 = MessageTypes.Cc015B;
		}

		protected override void PopulateMessageBody(Cc015BType message)
		{
			base.PopulateMessageBody(message);
			message.Heahea = PopulateHeaheaType();
			message.Trapripc1 = PopulateTrapripc1();
			message.Traconco1 = PopulateTraconco1();
			message.Traconce1 = PopulateTraconce1();
			message.Traautcontra = PopulateTraautcontra();
			message.Cusoffdepept = PopulateCusoffdepept();
			message.Cusofftrarns = PopulateCusofftrarns();
			message.Cusoffdesest = PopulateCusoffdesest();
			message.Conresers = PopulateConresers();
			message.Reprep = PopulateREPREP();
			message.Seainfsli = PopulateSeainfsli();
			message.Guagua = PopulateGUAGUA();
			message.Gooitegds = PopulateGOOITEGDS();
			if (wrapper.SafetyAndSecurityData)
			{
				message.Iti = PopulateITI();
				message.Cartra100 = PopulateCartra100();
				message.Tracorsec037 = PopulateTracorsec037();
				message.Traconsec029 = PopulateTraconsec029();
			}
		}

		HeaheaType PopulateHeaheaType()
		{
			var data = new HeaheaType();

			data.NumAgrHea1005 = wrapper.AgreementNumber;
			data.TinOpeBenAgrHea1022 = wrapper.PrincipalTIN;
			data.RefNumHea4 = wrapper.LocalReferenceNumber;
			data.TypOfDecHea24 = wrapper.TypeOfDeclaration;
			data.CouOfDesCodHea30 = wrapper.CountryOfDestinationCode;
			data.AgrLocOfGooHea39 = wrapper.AgreedLocationOfGoods;
			data.AgrLocOfGooHea39Lng = wrapper.AgreedLocationOfGoodsLanguage;
			data.AutLocOfGooCodHea41 = wrapper.AuthorisedLocationOfGoodsCode;
			data.PlaOfLoaCodHea46 = wrapper.PlaceOfLoading;
			data.CouOfDisCodHea55 = wrapper.CountryOfDispatchExportCode;
			data.CusSubPlaHea66 = wrapper.CustomsSubPlace;
			data.InlTraModHea75 = wrapper.InlandTransportMode;
			data.TraModAtBorHea76 = wrapper.TransportModeAtBorder;
			data.IdeOfMeaOfTraAtDhea78 = wrapper.IdentityOfMeansOfTransportAtDeparture;
			data.IdeOfMeaOfTraAtDhea78Lng = wrapper.IdentityOfMeansOfTransportAtDepartureLanguage;
			data.NatOfMeaOfTraAtDhea80 = wrapper.NationalityOfMeansOfTransportAtDeparture;
			data.IdeOfMeaOfTraCroHea85 = wrapper.IdentityOfMeansOfTransportCrossingBorder;
			data.IdeOfMeaOfTraCroHea85Lng = wrapper.IdentityOfMeansOfTransportCrossingBorderLanguage;
			data.NatOfMeaOfTraCroHea87 = wrapper.NationalityOfMeansOfTransportCrossingBorder;
			data.TypOfMeaOfTraCroHea88 = wrapper.TypeOfMeansOfTransportCrossingBorder;
			data.ConIndHea96 = wrapper.IsContainerised ? Flag.Item1 : Flag.Item0;
			data.DiaLanIndAtDepHea254 = wrapper.DialogLanguageIndicatorAtDeparture;
			data.NctsAccDocHea601Lng = wrapper.AccompanyingDocumentLanguage;
			data.TotNumOfIteHea305 = Utilities.FormatNumberFromZIntNational(wrapper.TotalNumberOfItems, 0);
			data.TotNumOfPacHea306 = Utilities.FormatNumberFromZLongNational(wrapper.TotalNumberOfPackages, 0);
			data.TotGroMasHea307 = wrapper.TotalGrossMass.Round(3);
			data.ValFacTotHea1000 = MessageBuilderHelper.GetOptionalDecimal(wrapper.TotalInvoiceValue, 0);
			data.DecDatHea383 = wrapper.DeclarationDate;
			data.DecPlaHea394 = wrapper.DeclarationPlace;
			data.DecPlaHea394Lng = wrapper.DeclarationPlaceLanguage;

			if (wrapper.SafetyAndSecurityData)
			{
				data.SpeCirIndHea1 = wrapper.SpecificCircumstanceIndicator;
				data.TraChaMetOfPayHea1 = wrapper.TransportChargesMethodOfPayment;
				data.ComRefNumHea = wrapper.CommercialReferenceNumber;
				data.SecHea358 = SecurityIndicator.Item1;
				data.ConRefNumHea = wrapper.ConveyanceReferenceNumber;
				data.CodPlUnHea357 = wrapper.PlaceOfUnloading;
				data.CodPlUnHea357Lng = wrapper.PlaceOfUnloadingCodeLanguage;
			}

			var isPrelodge = wrapper.IsPrelodge;
			var donSurSecHea1001 = GetDonSurSecHea1001(isPrelodge);
			if (donSurSecHea1001 != null)
			{
				data.DonSurSecHea1001 = donSurSecHea1001.Value;
			}

			data.DecAntHea1002 = isPrelodge ? Flag.Item1 : Flag.Item0;
			if (isPrelodge)
			{
				data.DatPreDepEnTraHea1019 = wrapper.ProvisionalTransitDepartureDate;
			}
			data.ModDeRepHea1003 = MapValueToEnumWithPrefix(wrapper.ModeOfRepresentation, ModeDeRepresentation.Item1);
			data.NumRefTraHea1004 = wrapper.TransportReferenceNumber;

			return data;
		}

		DonneesSureteSecurite? GetDonSurSecHea1001(bool isPreLodge)
		{
			DonneesSureteSecurite? result = null;
			var safetyAndSecurityData = wrapper.SafetyAndSecurityData;

			if ((!safetyAndSecurityData)
				|| (wrapper.SecurityConsignorCountryGroup == EU.NCTS.Messaging.SecurityTraderCountryGroup.EuForSafetyAndSecurity && wrapper.SecurityConsigneeCountryGroup == EU.NCTS.Messaging.SecurityTraderCountryGroup.EuForSafetyAndSecurity))
			{
				result = DonneesSureteSecurite.Item0;
			}
			else if (isPreLodge && safetyAndSecurityData
				&& (wrapper.SecurityConsignorCountryGroup != EU.NCTS.Messaging.SecurityTraderCountryGroup.EuForSafetyAndSecurity && wrapper.SecurityConsignorCountryGroup != EU.NCTS.Messaging.SecurityTraderCountryGroup.NorthernIreland)
				&& wrapper.SecurityConsigneeCountryGroup == EU.NCTS.Messaging.SecurityTraderCountryGroup.EuForSafetyAndSecurity)
			{
				result = DonneesSureteSecurite.Item1;
			}
			else if (safetyAndSecurityData && wrapper.SecurityConsignorCountryGroup == EU.NCTS.Messaging.SecurityTraderCountryGroup.EuForSafetyAndSecurity
				&& (wrapper.SecurityConsigneeCountryGroup != EU.NCTS.Messaging.SecurityTraderCountryGroup.EuForSafetyAndSecurity && wrapper.SecurityConsigneeCountryGroup != EU.NCTS.Messaging.SecurityTraderCountryGroup.NorthernIreland))
			{
				result = DonneesSureteSecurite.Item2;
			}
			return result;
		}

		Trapripc1Type PopulateTrapripc1()
		{
			Trapripc1Type result = null;
			var principal = wrapper.Principal;
			if (principal != null)
			{
				result = new Trapripc1Type();
				if (wrapper.IsTIRDeclaration)
				{
					result.Hitpc126 = principal.HolderIDTIR;
				}

				if (!principal.TIN.IsEmpty)
				{
					result.Tinpc159 = principal.TIN;
					if (wrapper.IsTIRDeclaration)
					{
						result.NamPc17 = principal.CompanyName.TrimEnd();
					}
				}
				else
				{
					result.NamPc17 = principal.CompanyName.TrimEnd();
					result.StrAndNumPc122 = principal.StreetAndNumber.TrimEnd();
					result.PosCodPc123 = principal.PostalCode;
					result.CouPc125 = principal.CountryCode;
					result.CitPc124 = principal.City;
					result.Nadlngpc = principal.NameAndAddressLanguage.Left(2);
				}
			}
			else
			{
				errorCollector.AddError(Res.GetString("96854205-80A1-47B5-9D63-F75C62EBC6F1", "Principal is empty"));
			}
			return result;
		}

		Traconco1Type PopulateTraconco1()
		{
			Traconco1Type result = null;
			var consignor = wrapper.Consignor;
			if (consignor != null)
			{
				result = new Traconco1Type();
				if (!consignor.TIN.IsEmpty)
				{
					result.Tinco159 = consignor.TIN;
				}
				else
				{
					result.NamCo17 = consignor.CompanyName.TrimEnd();
					result.StrAndNumCo122 = consignor.StreetAndNumber.TrimEnd();
					result.PosCodCo123 = consignor.PostalCode;
					result.CouCo125 = consignor.CountryCode;
					result.CitCo124 = consignor.City;
					result.Nadlngco = consignor.NameAndAddressLanguage.Left(2);
				}
			}
			return result;
		}

		Traconce1Type PopulateTraconce1()
		{
			Traconce1Type result = null;
			var consignor = wrapper.Consignee;
			if (consignor != null)
			{
				result = new Traconce1Type();
				if (!consignor.TIN.IsEmpty)
				{
					result.Tince159 = consignor.TIN;
				}
				else
				{
					result.NamCe17 = consignor.CompanyName.TrimEnd();
					result.StrAndNumCe122 = consignor.StreetAndNumber.TrimEnd();
					result.PosCodCe123 = consignor.PostalCode;
					result.CouCe125 = consignor.CountryCode;
					result.CitCe124 = consignor.City;
					result.Nadlngce = consignor.NameAndAddressLanguage.Left(2);
				}
			}
			return result;
		}

		TraautcontraType PopulateTraautcontra()
		{
			if (wrapper.AuthorisedConsigneeTIN.IsEmpty)
			{
				return null;
			}
			else
			{
				return new TraautcontraType()
				{
					Tintra59 = wrapper.AuthorisedConsigneeTIN
				};
			}
		}

		CusoffdepeptType PopulateCusoffdepept()
		{
			return new CusoffdepeptType()
			{
				RefNumEpt1 = wrapper.DepartureCustomsOfficeReferenceNumber
			};
		}

		Collection<CusofftrarnsType> PopulateCusofftrarns()
		{
			var data = new Collection<CusofftrarnsType>();
			foreach (var o in wrapper.TransitCustomsOffices)
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

		CusoffdesestType PopulateCusoffdesest()
		{
			return new CusoffdesestType()
			{
				RefNumEst1 = wrapper.DestinationCustomsOfficeReferenceNumber
			};
		}

		ConresersType PopulateConresers()
		{
			return new ConresersType()
			{
				ConResCodErs16 = wrapper.ControlResultCode,
				DatLimErs69 = wrapper.ControlResultDateLimit
			};
		}

		REPREPType PopulateREPREP()
		{
			REPREPType result = null;
			var declarant = wrapper.Declarant;
			if (declarant != null)
			{
				result = new REPREPType();
				if (!declarant.TIN.IsEmpty)
				{
					result.NumEorRep1010 = declarant.TIN;
				}
				else
				{
					result.NamRep5 = declarant.Name.TrimEnd();
					result.RueEtNrep1006 = declarant.StreetAndNumber;
					result.CodPosRep1007 = declarant.PostalCode;
					result.PayRep1009 = declarant.CountryCode;
					result.VilRep1008 = declarant.City;
					result.RepCapRep18 = declarant.RepresentativeCapacity.TrimEnd();
					result.RepCapRep18Lng = declarant.RepresentativeCapacityLanguage;
				}
			}
			return result;
		}

		SeainfsliType PopulateSeainfsli()
		{
			if (wrapper.NatureOfSeals == NatureOfSealsList.Codes.NS2)
			{
				return null;
			}
			else
			{
				return new SeainfsliType()
				{
					SeaNumSli2 = Utilities.FormatNumberFromZIntNational(wrapper.NumberOfSeals, 0),
					NatDesSceSli1011 = MapValueToEnumWithPrefix(wrapper.NatureOfSeals, NatureDesScelles.Item1),
					Seaidsid = PopulateSeaidsid()
				};
			}
		}

		Collection<SeaidsidType> PopulateSeaidsid()
		{
			var data = new Collection<SeaidsidType>();
			foreach (var s in wrapper.Seals)
			{
				var item = new SeaidsidType
				{
					SeaIdeSid1 = s.SealIdentity,
					SeaIdeSid1Lng = s.SealIdentityLanguage
				};
				data.Add(item);
			}
			return data;
		}

		Collection<GuaguaType> PopulateGUAGUA()
		{
			var data = new Collection<GuaguaType>();
			foreach (var g in wrapper.Guarantees)
			{
				var item = new GuaguaType
				{
					GuaTypGua1 = g.GuaranteeType,
					Guarefref = GetGuaRefType(g)
				};
				data.Add(item);
			}
			return data;
		}

		GuarefrefType GetGuaRefType(IGuarantee g)
		{
			var guaranteeType = g.GuaranteeType;
			var (shouldWriteReferenceToRef1, shouldWriteReferenceToOther, shouldWriteLiability, shouldWriteExtraInfo) = GetGuaranteeMapOptions(guaranteeType);
			if (shouldWriteReferenceToRef1 || shouldWriteReferenceToOther)
			{
				var guaRef = new GuarefrefType();
				if (shouldWriteLiability)
				{
					guaRef.MonDetSucDeNaiRef1012 = g.TaxAndDutyLiabiltyAmount.Round(0).ToString();
				}

				if (shouldWriteExtraInfo)
				{
					guaRef.AccCodRef6 = g.AccessCode;
					guaRef.Vallimecvle = new VallimecvleType { NotValForEcvle1 = MapValueToEnumWithPrefix(g.NotValidForEC, Flag.Item0) };
					guaRef.Vallimnoneclim = PopulateValidityLimitationNonEC(g.NotValidForOtherContractingParties);
				}

				if (shouldWriteReferenceToRef1)
				{
					guaRef.GuaRefNumGrnref1 = g.GuaranteeReferenceNumber;
				}
				else
				{
					guaRef.OthGuaRefRef4 = g.GuaranteeReferenceNumber;
				}
				return guaRef;
			}
			else
			{
				return null;
			}
		}

		Collection<VallimnoneclimType> PopulateValidityLimitationNonEC(IEnumerable<ZString> notValidForOtherContractingParties)
		{
			var data = new Collection<VallimnoneclimType>();
			foreach (var country in notValidForOtherContractingParties)
			{
				var result = IsEnumValueValid<CountryCodesGuaranteeManagementNonEc>(country);
				if (result)
				{
					var item = new VallimnoneclimType
					{
						NotValForOthConPlim2 = MapValueToEnum(country, CountryCodesGuaranteeManagementNonEc.Ad)
					};
					data.Add(item);
				}
			}
			return data;
		}

		#region Goods Item

		Collection<GooitegdsType> PopulateGOOITEGDS()
		{
			var data = new Collection<GooitegdsType>();
			foreach (var giWrapper in wrapper.GoodsItems)
			{
				var item = new GooitegdsType
				{
					IteNumGds7 = giWrapper.ItemNumber.ToString(),
					ComCodTarCodGds10 = giWrapper.CommodityCode.Left(8),
					DecTypGds15 = giWrapper.TypeOfDeclaration,
					GooDesGds23 = giWrapper.GoodsDescription,
					GooDesGds23Lng = giWrapper.GoodsDescriptionLanguage,
					GroMasGds46 = giWrapper.GrossMass.Round(3),
					GroMasGds46ValueSpecified = !giWrapper.GrossMass.IsEmpty,
					NetMasGds48 = giWrapper.NetMass.Round(3),
					NetMasGds48ValueSpecified = !giWrapper.NetMass.IsEmpty,
					CouOfDisGds58 = giWrapper.CountryOfDispatchExportCode,
					CouOfDesGds59 = giWrapper.CountryOfDestinationCode,
					ValFacGds1013 = MessageBuilderHelper.GetOptionalDecimal(giWrapper.BillValue, 0),
					Preadmrefar2 = PopulatePreadmrefar2(giWrapper),
					Prodocdc2 = PopulateProdocdc2(giWrapper),
					Spemenmt2 = PopulateSpemenmt2(giWrapper),
					Traconco2 = PopulateTraconco2(giWrapper),
					Traconce2 = PopulateTraconce2(giWrapper),
					Connr2 = PopulateConnr2(giWrapper),
					Pacgs2 = PopulatePacgs2(giWrapper)
				};

				if (wrapper.SafetyAndSecurityData)
				{
					item.MetOfPayGdi12 = giWrapper.TransportChargesMethodOfPayment;
					item.ComRefNumGim1 = giWrapper.CommercialReferenceNumber;
					item.UnDanGooCodGdi1 = giWrapper.UNDangerousGoodsCode;
					item.Tracorsecgoo021 = PopulateTracorsecgoo021(giWrapper);
					item.Traconsecgoo013 = PopulateTraconsecgoo013(giWrapper);
				}

				data.Add(item);
			}
			return data;
		}

		Collection<Preadmrefar2Type> PopulatePreadmrefar2(IDepartureGoodsItem giWrapper)
		{
			var data = new Collection<Preadmrefar2Type>();
			foreach (var giPreviousReferenceWrapper in giWrapper.PreviousAdministrativeReferences)
			{
				var item = new Preadmrefar2Type
				{
					PreDocTypAr21 = giPreviousReferenceWrapper.PreviousDocumentType,
					PreDocRefAr26 = giPreviousReferenceWrapper.PreviousDocumentReference,
					PreDocRefLng = giPreviousReferenceWrapper.PreviousDocumentReferenceLanguage,
					ComOfInfAr29 = giPreviousReferenceWrapper.ComplementOfInformation,
					ComOfInfAr29Lng = giPreviousReferenceWrapper.ComplementOfInformationLanguage
				};
				data.Add(item);
			}
			return data;
		}

		Collection<Prodocdc2Type> PopulateProdocdc2(IDepartureGoodsItem giWrapper)
		{
			var data = new Collection<Prodocdc2Type>();
			foreach (var giProducedDocumentsWrapper in giWrapper.ProducedDocumentsCertificates)
			{
				var item = new Prodocdc2Type
				{
					DocTypDc21 = giProducedDocumentsWrapper.DocumentType,
					DocRefDc23 = giProducedDocumentsWrapper.DocumentReference,
					DocRefDclng = giProducedDocumentsWrapper.DocumentReferenceLanguage,
					ComOfInfDc25 = giProducedDocumentsWrapper.ComplementOfInformation,
					ComOfInfDc25Lng = giProducedDocumentsWrapper.ComplementOfInformationLanguage
				};
				data.Add(item);
			}
			return data;
		}

		Collection<Spemenmt2Type> PopulateSpemenmt2(IDepartureGoodsItem giWrapper)
		{
			var data = new Collection<Spemenmt2Type>();
			foreach (var giSpecialMentionsWrapper in giWrapper.SpecialMentions)
			{
				var item = new Spemenmt2Type
				{
					AddInfMt21 = giSpecialMentionsWrapper.StatementText,
					AddInfMt21Lng = ZString.Empty,
					AddInfCodMt23 = giSpecialMentionsWrapper.Statement,
					ExpFroEcmt24 = giSpecialMentionsWrapper.ExportFromEC ? Flag.Item1 : Flag.Item0,
					ExpFroEcmt24ValueSpecified = true,
					ExpFroCouMt25 = giSpecialMentionsWrapper.ExportFromCountry
				};
				data.Add(item);
			}
			return data;
		}

		Traconco2Type PopulateTraconco2(IDepartureGoodsItem giWrapper)
		{
			Traconco2Type result = null;
			var consignor = giWrapper.Consignor;
			var headerConsignor = wrapper.Consignor;
			if (consignor != null && headerConsignor == null)
			{
				result = new Traconco2Type();
				if (!consignor.TIN.IsEmpty)
				{
					result.Tinco259 = consignor.TIN;
				}
				else
				{
					result.NamCo27 = consignor.Name.TrimEnd();
					result.StrAndNumCo222 = consignor.StreetAndNumber.TrimEnd();
					result.PosCodCo223 = consignor.PostalCode;
					result.CouCo225 = consignor.CountryCode;
					result.CitCo224 = consignor.City;
					result.Nadlnggtco = consignor.NameAndAddressLanguage.Left(2);
				}
			}
			return result;
		}

		Traconce2Type PopulateTraconce2(IDepartureGoodsItem giWrapper)
		{
			Traconce2Type result = null;
			var consignee = giWrapper.Consignee;
			var headerConsignee = wrapper.Consignee;
			if (consignee != null && headerConsignee == null)
			{
				result = new Traconce2Type();
				if (!consignee.TIN.IsEmpty)
				{
					result.Tince259 = consignee.TIN;
				}
				else
				{
					result.NamCe27 = consignee.Name.TrimEnd();
					result.StrAndNumCe222 = consignee.StreetAndNumber.TrimEnd();
					result.PosCodCe223 = consignee.PostalCode;
					result.CouCe225 = consignee.CountryCode;
					result.CitCe224 = consignee.City;
					result.Nadlnggice = consignee.NameAndAddressLanguage.Left(2);
				}
			}
			return result;
		}

		Collection<Connr2Type> PopulateConnr2(IDepartureGoodsItem giWrapper)
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

		Collection<Pacgs2Type> PopulatePacgs2(IDepartureGoodsItem giWrapper)
		{
			var data = new Collection<Pacgs2Type>();
			foreach (var packageWrapper in giWrapper.Packages)
			{
				var package = new Pacgs2Type
				{
					MarNumOfPacGs21 = packageWrapper.MarksAndNumbersOfPackages,
					MarNumOfPacGs21Lng = packageWrapper.MarksAndNumbersOfPackagesLanguage,
					KinOfPacGs23 = packageWrapper.KindOfPackages
				};

				if (!packageWrapper.IsBulk)
				{
					if (packageWrapper.IsUnpacked)
					{
						package.NumOfPieGs25 = packageWrapper.NumberOfUnits.ToString();
						package.NumOfPacGs24 = "0";
					}
					else
					{
						package.NumOfPieGs25 = "0";
						package.NumOfPacGs24 = packageWrapper.NumberOfUnits.ToString();
					}
				}

				data.Add(package);
			}
			return data;
		}

		Tracorsecgoo021Type PopulateTracorsecgoo021(IDepartureGoodsItem giWrapper)
		{
			Tracorsecgoo021Type result = null;
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
					result.NamTracorsecgoo025 = consignorSecurity.Name.TrimEnd();
					result.StrNumTracorsecgoo027 = consignorSecurity.StreetAndNumber.TrimEnd();
					result.PosCodTracorsecgoo026 = consignorSecurity.PostalCode;
					result.CouCodTracorsecgoo023 = consignorSecurity.CountryCode;
					result.CitTracorsecgoo022 = consignorSecurity.City;
					result.Tracorsecgoo021Lng = consignorSecurity.NameAndAddressLanguage.Left(2);
				}
			}
			return result;
		}

		Traconsecgoo013Type PopulateTraconsecgoo013(IDepartureGoodsItem giWrapper)
		{
			Traconsecgoo013Type result = null;
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
					result.NamTraconsecgoo017 = consigneeSecurity.Name.TrimEnd();
					result.StrNumTraconsecgoo019 = consigneeSecurity.StreetAndNumber.TrimEnd();
					result.PosCodTraconsecgoo018 = consigneeSecurity.PostalCode;
					result.CouCodTraconsecgoo015 = consigneeSecurity.CountryCode;
					result.CityTraconsecgoo014 = consigneeSecurity.City;
					result.Traconsecgoo013Lng = consigneeSecurity.NameAndAddressLanguage.Left(2);
				}
			}
			return result;
		}

		#endregion Goods Item

		Collection<ItiType> PopulateITI()
		{
			var data = new Collection<ItiType>();
			foreach (var countryOfRoutingCode in wrapper.Itinerary)
			{
				var item = new ItiType()
				{
					CouOfRouCodIti1 = countryOfRoutingCode
				};
				data.Add(item);
			}
			return data;
		}

		Cartra100Type PopulateCartra100()
		{
			var isPrelodge = wrapper.IsPrelodge;
			var data = new Cartra100Type()
			{
				Tincartra254 = GetDonSurSecHea1001(isPrelodge) != DonneesSureteSecurite.Item2 ? wrapper.SecurityCarrierEORI : null
			};
			return string.IsNullOrEmpty(data.Tincartra254) ? null : data;
		}

		Tracorsec037Type PopulateTracorsec037()
		{
			Tracorsec037Type result = null;
			var securityConsignor = wrapper.SecurityConsignor;
			if (securityConsignor != null)
			{
				result = new Tracorsec037Type();
				if (!securityConsignor.TIN.IsEmpty)
				{
					result.Tintracorsec044 = securityConsignor.TIN;
					var isPrelodge = wrapper.IsPrelodge;
					if (GetDonSurSecHea1001(isPrelodge) == DonneesSureteSecurite.Item1)
					{
						result.CouCodTracorsec039 = securityConsignor.CountryCode;
					}
				}
				else
				{
					result.NamTracorsec041 = securityConsignor.Name.TrimEnd();
					result.StrNumTracorsec043 = securityConsignor.StreetAndNumber.TrimEnd();
					result.PosCodTracorsec042 = securityConsignor.PostalCode;
					result.CouCodTracorsec039 = securityConsignor.CountryCode;
					result.CitTracorsec038 = securityConsignor.City;
					result.Tracorsec037Lng = securityConsignor.NameAndAddressLanguage.Left(2);
				}
			}
			return result;
		}

		Traconsec029Type PopulateTraconsec029()
		{
			Traconsec029Type result = null;
			var securityConsignee = wrapper.SecurityConsignee;
			if (securityConsignee != null)
			{
				result = new Traconsec029Type();
				if (!securityConsignee.TIN.IsEmpty)
				{
					result.Tintraconsec036 = securityConsignee.TIN;
					var isPrelodge = wrapper.IsPrelodge;
					if (GetDonSurSecHea1001(isPrelodge) == DonneesSureteSecurite.Item2)
					{
						result.CouCodTraconsec031 = securityConsignee.CountryCode;
					}
				}
				else
				{
					result.NameTraconsec033 = securityConsignee.Name.TrimEnd();
					result.StrNumTraconsec035 = securityConsignee.StreetAndNumber.TrimEnd();
					result.PosCodTraconsec034 = securityConsignee.PostalCode;
					result.CouCodTraconsec031 = securityConsignee.CountryCode;
					result.CitTraconsec030 = securityConsignee.City;
					result.Traconsec029Lng = securityConsignee.NameAndAddressLanguage.Left(2);
				}
			}
			return result;
		}

		protected override void ValidateMessage()
		{
			base.ValidateMessage();
			if (wrapper.SafetyAndSecurityData)
			{
				var goodsItems = wrapper.GoodsItems.ToArray();

				if (wrapper.SecurityConsignor == null && goodsItems.Any(x => x.ConsignorSecurity == null))
				{
					errorCollector.AddError(errorWhenLacksConsignorOnGoodsItem);
				}

				if (goodsItems.SelectMany(x => x.SpecialMentions).All(x => x.Statement != "10600"))
				{
					if (wrapper.SecurityConsignee == null && goodsItems.Any(x => x.ConsigneeSecurity == null))
					{
						errorCollector.AddError(errorWhenLacksConsigneeOnGoodsItem);
					}
				}
			}
		}

		readonly ZString errorWhenLacksConsignorOnGoodsItem = Res.GetString("E4E747DD-2D16-4F11-9EF8-7E981F4695D8", "The 'safety-security' attribute being checked, the security shipper must be entered at Security Consignor level or at Articles / Goods Item level.");
		readonly ZString errorWhenLacksConsigneeOnGoodsItem = Res.GetString("0ACA224C-D4A2-4FA2-ABD1-B2E77458E71F", "The 'safety-security' attribute being checked, the safety recipient must be entered at Security Consignee level or at Articles / Goods Item level.");
	}
}
