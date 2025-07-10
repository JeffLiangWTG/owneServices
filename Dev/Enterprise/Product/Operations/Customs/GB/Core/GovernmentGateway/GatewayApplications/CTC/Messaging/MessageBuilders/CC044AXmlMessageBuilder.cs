using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase4.cc044a;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.GB.GovernmentGateway.CodeDescriptionPairLists;
using ROC = Enterprise.Customs.EU.NCTS.Business.ResultOfCOntrol;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public class CC044AXmlMessageBuilder : XmlMessageBuilder<ICC044ADeclaration, Cc044AType>
	{
		public CC044AXmlMessageBuilder(ICC044ADeclaration expectedData, ICC044ADeclaration actualData, ErrorCollector errorCollector)
			: base(expectedData, errorCollector)
		{
			this.expectedData = expectedData;
			this.actualData = actualData;
		}

		readonly ICC044ADeclaration expectedData;
		readonly ICC044ADeclaration actualData;

		#region CC044A Message

		protected override void PopulateMessageHeader(Cc044AType message)
		{
			base.PopulateMessageHeader(message);
			message.MesTypMes20 = CTCMessageTypeList.Codes.CC044A;
		}

		protected override void PopulateMessageBody(Cc044AType message)
		{
			base.PopulateMessageBody(message);
			message.Heahea = PopulateHEAHEAType();
			message.Tradestrd = PopulateTRADESTRDType();
			message.Cusoffpreoffres = PopulateCUSOFFPREOFFRESType();
			message.Unlremrem = PopulateUNLREMREMType();
			message.Resofcon534 = new Collection<Resofcon534Type>(PopulateRESOFCON534Type().ToList());
			message.Seainfsli = PopulateSEAINFSLIType();
			message.Gooitegds = PopulateGOOITEGDSType();
		}

		HeaheaType PopulateHEAHEAType()
		{
			return new HeaheaType
			{
				DocNumHea5 = expectedData.MovementReferenceNumber,
				IdeOfMeaOfTraAtDhea78 = expectedData.IdentityOfMeansOfTransportAtDeparture,
				IdeOfMeaOfTraAtDhea78Lng = expectedData.IdentityOfMeansOfTransportAtDepartureLanguage.SubstringSafe(0, 2),
				NatOfMeaOfTraAtDhea80 = expectedData.NationalityOfMeansOfTransportAtDeparture,
				TotNumOfIteHea305 = CTCExtensions.GetNumberAsString(expectedData.TotalNumberOfItems),
				TotNumOfPacHea306 = CTCExtensions.GetNumberAsString(expectedData.TotalNumberOfPackages),
				TotGroMasHea307 = expectedData.TotalGrossMass.ToStringTrimZeros(3)
			};
		}

		TradestrdType PopulateTRADESTRDType()
		{
			TradestrdType result = null;
			var destinationTrader = expectedData.DestinationTrader;
			if (destinationTrader != null)
			{
				result = new TradestrdType();
				if (!destinationTrader.TIN.IsEmpty)
				{
					result.Tintrd59 = destinationTrader.TIN;
				}
				else
				{
					result.NamTrd7 = destinationTrader.Name;
					result.StrAndNumTrd22 = destinationTrader.StreetAndNumber;
					result.PosCodTrd23 = destinationTrader.PostalCode;
					result.CouTrd25 = destinationTrader.CountryCode;
					result.CitTrd24 = destinationTrader.City;
					result.Nadlngrd = destinationTrader.NameAndAddressLanguage.SubstringSafe(0, 2);
				}
			}
			return result;
		}

		CusoffpreoffresType PopulateCUSOFFPREOFFRESType()
		{
			var cusoffpreoffres = new CusoffpreoffresType
			{
				RefNumRes1 = expectedData.DestinationCustomsOfficeReferenceNumber
			};
			return cusoffpreoffres;
		}

		UnlremremType PopulateUNLREMREMType()
		{
			return new UnlremremType
			{
				StaOfTheSeaOkrem19 = ConvertYesNoEmptyToBit(actualData.UnloadingRemark?.StateOfSealsOk, true),
				UnlRemRem53 = actualData.HeaderUnloadingNotes,
				UnlRemRem53Lng = actualData.HeaderUnloadingNotesLanguage.SubstringSafe(0, 2),
				ConRem65 = ConvertYesNoEmptyToBit(actualData.UnloadingRemark?.Conform),
				UnlComRem66 = ConvertYesNoEmptyToBit(actualData.UnloadingRemark?.UnloadingCompletion),
				UnlDatRem67 = actualData.UnloadingRemark?.UnloadingDate
			};
		}

		ZString ConvertYesNoEmptyToBit(ZString? value, bool keepEmpty = false)
		{
			var result = Customs.Business.YesNoList.IsYes(value) ? "1" : "0";

			if ((value?.IsEmpty ?? true) && keepEmpty)
			{
				result = value;
			}

			return result;
		}

		const string TranportPointerPrefix = "18#";
		const string TotalGrossMassInKilogramsPointer = "35";
		const string TotalNumberOfPackagesPointer = "6";

		IEnumerable<Resofcon534Type> PopulateRESOFCON534Type()
		{
			// here is where we compare acutal versus expected and MAKE OUR OWN calls about what is different (not looking at base EU's header ROC at all)
			int transportPointerCounter = 0;

			var id = new ResultsOfControlHelper<ZString>(expectedData.IdentityOfMeansOfTransportAtDeparture,
															actualData.IdentityOfMeansOfTransportAtDeparture,
															TranportPointerPrefix,
															() => ++transportPointerCounter);

			var nationality = new ResultsOfControlHelper<ZString>(expectedData.NationalityOfMeansOfTransportAtDeparture,
															actualData.NationalityOfMeansOfTransportAtDeparture,
															TranportPointerPrefix,
															() => ++transportPointerCounter);

			var grossMass = new ResultsOfControlHelper<ZDecimal>(expectedData.TotalGrossMass,
															actualData.TotalGrossMass,
															TotalGrossMassInKilogramsPointer);

			var packs = new ResultsOfControlHelper<ZLong>(expectedData.TotalNumberOfPackages,
															actualData.TotalNumberOfPackages,
															TotalNumberOfPackagesPointer);

			yield return id.GetResultOfControlDifference();
			yield return nationality.GetResultOfControlDifference();
			yield return grossMass.GetResultOfControlDifference();
			yield return packs.GetResultOfControlDifference();
		}

		SeainfsliType PopulateSEAINFSLIType()
		{
			SeainfsliType seainfsli = null;
			var stateOfSeals = actualData.UnloadingRemark.StateOfSealsOk;
			if (!stateOfSeals.IsEmpty)
			{
				var noOfSeals = actualData.UnloadingRemark.RawNoOfSeals;
				seainfsli = new SeainfsliType
				{
					SeaNumSli2 = CTCExtensions.GetNumberAsString(noOfSeals),
					Seaidsid = PopulateSEAIDSIDType(actualData.Seals),
				};
			}
			return seainfsli;
		}

		Collection<SeaidsidType> PopulateSEAIDSIDType(IEnumerable<ISealID> seals)
		{
			var sealList = new Collection<SeaidsidType>();
			foreach (var seal in seals.Where(s => !s.SealIdentity.IsEmpty))
			{
				var sealType = new SeaidsidType
				{
					SeaIdeSid1 = seal.SealIdentity,
					SeaIdeSid1Lng = seal.SealIdentityLanguage.SubstringSafe(0, 2),
				};
				sealList.Add(sealType);
			}
			return sealList.Any() ? sealList : null;
		}

		#region Unloaded Goods Item

		Collection<GooitegdsType> PopulateGOOITEGDSType()
		{
			var goodItemsList = new Collection<GooitegdsType>();
			foreach (var unloadedGoodsItemWrapper in actualData.UnloadedGoodsItems)
			{
				var expectedGoodsItemWrapper = expectedData.ExpectedGoodsItems?.FirstOrDefault(x => x.ItemNumber.ToString() == unloadedGoodsItemWrapper.ItemNumber);
				PopulateGoodsItem(goodItemsList, expectedGoodsItemWrapper, unloadedGoodsItemWrapper);
			}
			return goodItemsList.Any() ? goodItemsList : null;
		}

		void PopulateGoodsItem(Collection<GooitegdsType> goodItemsList, ICommonGoodsItem expectedGoodsItemWrapper, IUnloadedGoodsItem unloadedGoodsItemWrapper)
		{
			if (unloadedGoodsItemWrapper.IsNew)
			{
				AddGoodsItem(goodItemsList, unloadedGoodsItemWrapper, "");
			}
			else if (unloadedGoodsItemWrapper.HasDifferences)
			{
				AddGoodsItem(goodItemsList, expectedGoodsItemWrapper, ROC.ResultOfControlCodes.Codes.Original);
				AddGoodsItem(goodItemsList, unloadedGoodsItemWrapper, "");
			}
			else if (unloadedGoodsItemWrapper.IsMissing)
			{
				AddGoodsItem(goodItemsList, expectedGoodsItemWrapper, ROC.ResultOfControlCodes.Codes.Original);
				AddGoodsItem(goodItemsList, unloadedGoodsItemWrapper, "");
			}
		}

		string ToStringWithThreeDecimalPlacesOrNullIfZero(ZDecimal value) => value < 0.0005 && value > -0.0005
			? null
			: value.ToStringTrimZeros(3);

		void AddGoodsItem(Collection<GooitegdsType> goodItemsList, IUnloadedGoodsItem unloadedGoodsItemWrapper, string controlIndicator)
		{
			if (unloadedGoodsItemWrapper != null)
			{
				if (unloadedGoodsItemWrapper.IsMissing)
				{
					goodItemsList.Add(new GooitegdsType
					{
						IteNumGds7 = unloadedGoodsItemWrapper.ItemNumber,
						Resofconroc = PopulateRESOFCONROCTypeForItems(unloadedGoodsItemWrapper.ResultsOfControl, controlIndicator)
					});
				}
				else
				{
					goodItemsList.Add(new GooitegdsType
					{
						IteNumGds7 = unloadedGoodsItemWrapper.ItemNumber,
						ComCodTarCodGds10 = unloadedGoodsItemWrapper.CommodityCode.SubstringSafe(0, 8),
						GooDesGds23 = unloadedGoodsItemWrapper.GoodsDescription,
						GooDesGds23Lng = unloadedGoodsItemWrapper.GoodsDescriptionLanguage.SubstringSafe(0, 2),
						GroMasGds46 = ToStringWithThreeDecimalPlacesOrNullIfZero(unloadedGoodsItemWrapper.GrossMass),
						NetMasGds48 = ToStringWithThreeDecimalPlacesOrNullIfZero(unloadedGoodsItemWrapper.NetMass),
						Prodocdc2 = PopulatePRODOCDC2Type(unloadedGoodsItemWrapper.ProducedDocumentsCertificates),
						Resofconroc = PopulateRESOFCONROCTypeForItems(unloadedGoodsItemWrapper.ResultsOfControl, controlIndicator),
						Connr2 = PopulateCONNR2Type(unloadedGoodsItemWrapper.Containers),
						Pacgs2 = PopulatePACGS2Type(unloadedGoodsItemWrapper.Packages),
						Sgicodsd2 = PopulateSGICODSD2Type(unloadedGoodsItemWrapper.SGICodes)
					});
				}
			}
		}

		void AddGoodsItem(Collection<GooitegdsType> goodItemsList, ICommonGoodsItem expectedGoodsItemWrapper, string controlIndicator)
		{
			if (expectedGoodsItemWrapper != null)
			{
				goodItemsList.Add(new GooitegdsType
				{
					IteNumGds7 = expectedGoodsItemWrapper.ItemNumber.ToString(),
					ComCodTarCodGds10 = expectedGoodsItemWrapper.CommodityCode.SubstringSafe(0, 8),
					GooDesGds23 = expectedGoodsItemWrapper.GoodsDescription,
					GooDesGds23Lng = expectedGoodsItemWrapper.GoodsDescriptionLanguage.SubstringSafe(0, 2),
					GroMasGds46 = ToStringWithThreeDecimalPlacesOrNullIfZero(expectedGoodsItemWrapper.GrossMass),
					NetMasGds48 = ToStringWithThreeDecimalPlacesOrNullIfZero(expectedGoodsItemWrapper.NetMass),
					Prodocdc2 = PopulatePRODOCDC2Type(expectedGoodsItemWrapper.ProducedDocumentsCertificates),
					Resofconroc = PopulateRESOFCONROCTypeForItems(Enumerable.Empty<IControlResult>(), controlIndicator),
					Connr2 = PopulateCONNR2Type(expectedGoodsItemWrapper.Containers),
					Pacgs2 = PopulatePACGS2Type(expectedGoodsItemWrapper.Packages)
				});
			}
		}

		Collection<Prodocdc2Type> PopulatePRODOCDC2Type(IEnumerable<IProducedDocumentCertificate> producedDocumentsCertificates)
		{
			var producedDocsList = new Collection<Prodocdc2Type>();
			foreach (var producedDocumentsCertificatesWrapper in producedDocumentsCertificates)
			{
				producedDocsList.Add(new Prodocdc2Type
				{
					DocTypDc21 = producedDocumentsCertificatesWrapper.DocumentType,
					DocRefDc23 = producedDocumentsCertificatesWrapper.DocumentReference,
					DocRefDclng = producedDocumentsCertificatesWrapper.DocumentReferenceLanguage.SubstringSafe(0, 2),
					ComOfInfDc25 = producedDocumentsCertificatesWrapper.ComplementOfInformation,
					ComOfInfDc25Lng = producedDocumentsCertificatesWrapper.ComplementOfInformationLanguage.SubstringSafe(0, 2)
				});
			}
			return producedDocsList.Any() ? producedDocsList : null;
		}

		Collection<ResofconrocType> PopulateRESOFCONROCTypeForItems(IEnumerable<IControlResult> controlResults, ZString controlIndicator)
		{
			const string ItemCheckedPointer = "OK";
			var resultsOfControl = new List<ResofconrocType>();

			ResofconrocType CreateRESOFCONROCType(IControlResult controlResult) => new ResofconrocType
			{
				DesRoc2 = controlResult.Description,
				ConIndRoc1 = controlResult.PointerToTheAttribute == ROC.ResultOfControlCodes.Codes.Missing
					? (ZString)ROC.ResultOfControlCodes.Codes.Different
					: controlResult.PointerToTheAttribute
			};

			var existingControlResults = controlResults.Where(p => p.PointerToTheAttribute != ItemCheckedPointer)
				.Select(x => CreateRESOFCONROCType(x));
			resultsOfControl.AddRange(existingControlResults);

			if (!controlIndicator.IsEmpty)
			{
				resultsOfControl.Add(new ResofconrocType { ConIndRoc1 = controlIndicator });
			}

			return resultsOfControl.Any() ? new Collection<ResofconrocType>(resultsOfControl) : null;
		}

		Collection<Connr2Type> PopulateCONNR2Type(IEnumerable<ZString> containersList)
		{
			var containers = new Collection<Connr2Type>();
			if (containersList != null)
			{
				foreach (var container in containersList)
				{
					containers.Add(new Connr2Type
					{
						ConNumNr21 = container,
					});
				}
			}
			return containers.Any() ? containers : null;
		}

		Collection<Pacgs2Type> PopulatePACGS2Type(IEnumerable<IPackage> packagesList)
		{
			var packages = new Collection<Pacgs2Type>();
			foreach (IPackage packageWrapper in packagesList)
			{
				var packType = packageWrapper.KindOfPackages;
				var package = new Pacgs2Type
				{
					MarNumOfPacGs21 = packageWrapper.MarksAndNumbersOfPackages,
					MarNumOfPacGs21Lng = packageWrapper.MarksAndNumbersOfPackagesLanguage.SubstringSafe(0, 2),
					KinOfPacGs23 = packType
				};
				if (packageWrapper.IsUnpacked)
				{
					package.NumOfPieGs25 = packageWrapper.NumberOfPieces.ToString();
				}
				else if (!packageWrapper.IsBulk)
				{
					package.NumOfPacGs24 = packageWrapper.NumberOfPackages.ToString();
				}
				packages.Add(package);
			}
			return packages.Any() ? packages : null;
		}

		Collection<Sgicodsd2Type> PopulateSGICODSD2Type(IEnumerable<ISgiCode> sgiCodesList)
		{
			var sgiCodes = new Collection<Sgicodsd2Type>();
			foreach (var sgi in sgiCodesList)
			{
				sgiCodes.Add(new Sgicodsd2Type
				{
					SenGooCodSd22 = sgi.Code,
					SenQuaSd23 = sgi.Qty.ToStringTrimZeros(3)
				});
			}
			return sgiCodes.Any() ? sgiCodes : null;
		}

		#endregion

		#endregion
	}
}
