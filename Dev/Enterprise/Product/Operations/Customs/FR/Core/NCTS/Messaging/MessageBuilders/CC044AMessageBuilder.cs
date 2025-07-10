using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC044A;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.COMPLEX_NCTS;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL_NATIONAL;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.ZArchitecture.Core;
using ROC = Enterprise.Customs.EU.NCTS.Business.ResultOfCOntrol;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class CC044AMessageBuilder : NctsXmlMessageBuilder<ICC044ADeclaration, NctsMessageFunctionSet.UnloadingRemarksMessage, Cc044AType>
	{
		public CC044AMessageBuilder(ICC044ADeclaration wrapper, NctsMessageFunctionSet.UnloadingRemarksMessage messageFunction, ErrorCollector errorCollector) : base(wrapper, messageFunction, errorCollector)
		{
		}

		protected override void PopulateMessageHeader(Cc044AType message)
		{
			base.PopulateMessageHeader(message);
			message.MesTypMes20 = MessageTypes.Cc044A;
		}

		protected override void PopulateMessageBody(Cc044AType message)
		{
			base.PopulateMessageBody(message);
			message.Heahea = PopulateHEAHEA();
			message.Tradestrd = PopulateTradestrd();
			message.Cusoffpreoffres = PopulateCusoffpreoffresType();
			message.Unlremrem = PopulateUNLREMREM();
			message.Resofcon534 = PopulateResofcon534();
			message.Gooitegds = PopulateGooitegdsType();
			message.Seainfsli = PopulateSeainfsliType();
		}

		Collection<Seaidsi1Type> PopulateSeaidsidType(IEnumerable<IEnRouteEvent> enRouteEventList)
		{
			var sealList = new Collection<Seaidsi1Type>();
			foreach (var enRouteEvent in enRouteEventList)
			{
				var enRouteEventSeal = enRouteEvent.EnRouteEventSeal;
				if (enRouteEventSeal != null)
				{
					foreach (var sealCont in enRouteEventSeal.ContainerSeals)
					{
						var seal = new Seaidsi1Type
						{
							SeaIdeSi11 = sealCont.SealIdentity,
							SeaIdeSi11Lng = sealCont.SealIdentityLanguage,
							NatDesSceSi1015 = NatureDesScelles.Item4,
						};
						sealList.Add(seal);
					}
				}
			}
			return sealList;
		}

		SeainfsliType PopulateSeainfsliType()
		{
			SeainfsliType seainfsli = null;
			var noOfSeals = wrapper.UnloadingRemark.NoOfSeals;
			if (!noOfSeals.IsEmpty)
			{
				seainfsli = new SeainfsliType
				{
					SeaNumSli2 = Utilities.FormatNumberFromZIntNational(noOfSeals, 0),
					Seaidsid = PopulateSeaidsidType(wrapper.EnRouteEvents),
				};
			}
			return seainfsli;
		}

		CusoffpreoffresType PopulateCusoffpreoffresType()
		{
			var cusoffpreoffres = new CusoffpreoffresType
			{
				RefNumRes1 = wrapper.DestinationCustomsOfficeReferenceNumber
			};
			return cusoffpreoffres;
		}

		Collection<Resofcon534Type> PopulateResofcon534()
		{
			var results = new Collection<Resofcon534Type>();
			var listOfDifference = wrapper.ListOfDifferenceInHeader;
			foreach (var result in listOfDifference)
			{
				results.Add(new Resofcon534Type
				{
					ConInd424 = ROC.ResultOfControlCodes.Codes.Different,
					PoiToTheAttToc5 = result.Item1,
					CorValToc4 = result.Item2,
				});
			}

			foreach (var result in wrapper.ControlResultList)
			{
				if (result.ControlIndicator == ROC.ResultOfControlCodes.Codes.Other && !result.Description.IsEmpty)
				{
					results.Add(new Resofcon534Type
					{
						DesToc2 = result.Description,
						ConInd424 = ROC.ResultOfControlCodes.Codes.Other,
					});
				}
			}

			return results;
		}

		Collection<GooitegdsType> PopulateGooitegdsType()
		{
			var goodItemsList = new Collection<GooitegdsType>();
			if (wrapper.UnloadingRemark.Conform == YesNoList.Codes.No)
			{
				var unloadItemsWrapper = wrapper.UnloadedGoodsItems;
				var unloadedItems = unloadItemsWrapper.OrderBy(x => x.ItemNumber).ToArray();
				for (var i = 0; i < unloadItemsWrapper.Count; i++)
				{
					if (i < 9999)
					{
						var unloadedItem = unloadedItems[i];
						var loadedItemWrapper = wrapper.ExpectedGoodsItems;
						ICommonGoodsItem loadedItem = null;
						if (loadedItemWrapper.Count >= (i + 1))
						{
							loadedItem = loadedItemWrapper.ElementAt(i);
						}

						goodItemsList.Add(new GooitegdsType
						{
							IteNumGds7 = unloadedItem.ItemNumber.ToString(),
							ComCodTarCodGds10 = unloadedItem.CommodityCode.Left(8),
							GooDesGds23 = unloadedItem.GoodsDescription,
							GroMasGds46 = unloadedItem.GrossWeight.Round(3),
							GroMasGds46ValueSpecified = !unloadedItem.GrossWeight.IsEmpty,
							NetMasGds48 = unloadedItem.NetWeight.Round(3),
							NetMasGds48ValueSpecified = !unloadedItem.NetWeight.IsEmpty,
							Prodocdc2 = PopulateProdocdc2Type(unloadedItem),
							Resofconroc = PopulateResofconroc(loadedItem, unloadedItem),
							Connr2 = PopulateConnr2(unloadedItem),
							Pacgs2 = PopulatePacgs2(unloadedItem),
						});
					}
				}
			}

			return goodItemsList;
		}

		Collection<Pacgs2Type> PopulatePacgs2(IUnloadedGoodsItem item)
		{
			var packages = new Collection<Pacgs2Type>();
			foreach (IPackage packageWrapper in item.Packages)
			{
				var package = new Pacgs2Type
				{
					MarNumOfPacGs21 = packageWrapper.MarksAndNumbersOfPackages,
					MarNumOfPacGs21Lng = packageWrapper.MarksAndNumbersOfPackagesLanguage,
					KinOfPacGs23 = packageWrapper.KindOfPackages
				};

				if (!packageWrapper.IsBulk && !packageWrapper.IsUnpacked)
				{
					package.NumOfPacGs24 = packageWrapper.NumberOfPackages.ToString();
				}

				if (packageWrapper.IsUnpacked)
				{
					package.NumOfPieGs25 = packageWrapper.NumberOfPieces.ToString();
				}

				packages.Add(package);
			}
			return packages;
		}

		Collection<Connr2Type> PopulateConnr2(IUnloadedGoodsItem item)
		{
			var containers = new Collection<Connr2Type>();
			foreach (var container in item.Containers)
			{
				containers.Add(new Connr2Type
				{
					ConNumNr21 = container,
				});
			}
			return containers;
		}

		Collection<Prodocdc2Type> PopulateProdocdc2Type(IUnloadedGoodsItem item)
		{
			var supportingDocsList = new Collection<Prodocdc2Type>();
			foreach (FR.Messaging.Interfaces.Common.ISupportingDocument docWrapper in item.SupportingDocuments)
			{
				supportingDocsList.Add(new Prodocdc2Type
				{
					DocTypDc21 = docWrapper.Code.Left(4),
					DocRefDc23 = docWrapper.RefNumber,
					ComOfInfDc25 = docWrapper.Description,
				});
			}
			return supportingDocsList;
		}

		Collection<ResofconrocType> PopulateResofconroc(ICommonGoodsItem loadedItem, IUnloadedGoodsItem unloadedItem)
		{
			var resultsOfControl = new Collection<ResofconrocType>();
			if (!IsORIUnloadedItem(unloadedItem.ControlResults))
			{
				resultsOfControl.Add(new ResofconrocType
				{
					ConIndRoc1 = ROC.ResultOfControlCodes.Codes.Original,
				});
			}
			else
			{
				foreach (IControlResult controlResultWrapper in unloadedItem.ControlResults)
				{
					var pointerToAttribute = controlResultWrapper.PointerToTheAttribute;
					var desc = controlResultWrapper.Description;
					if (!pointerToAttribute.Equals("OK") && !(pointerToAttribute == ROC.ResultOfControlCodes.Codes.Other && desc.IsEmpty) && !(pointerToAttribute == ROC.ResultOfControlCodes.Codes.Missing))
					{
						resultsOfControl.Add(new ResofconrocType
						{
							DesRoc2 = desc,
							ConIndRoc1 = pointerToAttribute,
						});
					}
				}
			}

			IncludeMissingDocumentsInControlResults(resultsOfControl, loadedItem, unloadedItem);

			return resultsOfControl;
		}

		bool IsORIUnloadedItem(IEnumerable<IControlResult> controlResults)
		{
			return controlResults.Any(x => !ConditionsToORcontrolResultIndicator(x.PointerToTheAttribute, x.Description));
		}

		ZBool ConditionsToORcontrolResultIndicator(ZString pointerToAttribute, ZString description)
		{
			return (pointerToAttribute != ROC.ResultOfControlCodes.Codes.Different
				&& pointerToAttribute != ROC.ResultOfControlCodes.Codes.New
				&& pointerToAttribute != ROC.ResultOfControlCodes.Codes.NotPresent
				&& pointerToAttribute != ROC.ResultOfControlCodes.Codes.Missing
				&& pointerToAttribute != ROC.ResultOfControlCodes.Codes.Other)
				|| (pointerToAttribute == ROC.ResultOfControlCodes.Codes.Other && description.IsEmpty);
		}

		void IncludeMissingDocumentsInControlResults(Collection<ResofconrocType> controlResults, ICommonGoodsItem loadedItem, IUnloadedGoodsItem unloadedItem)
		{
			var missingDocs = GetMissingDocuments(loadedItem, unloadedItem);

			foreach (var missingDoc in missingDocs)
			{
				controlResults.Add(new ResofconrocType
				{
					ConIndRoc1 = ROC.ResultOfControlCodes.Codes.NotPresent,
					PoiToTheAttRoc51 = missingDoc,
				});
			}
		}

		List<ZString> GetMissingDocuments(ICommonGoodsItem line, IUnloadedGoodsItem unloadedLine)
		{
			var missingDocumentsList = new List<ZString>();
			int i = 1;
			if (line != null && line.ProducedDocumentsCertificates != null)
			{
				foreach (var doc in line.ProducedDocumentsCertificates)
				{
					if (!unloadedLine.SupportingDocuments.Any(r => r.RefNumber == doc.DocumentReference))
					{
						missingDocumentsList.Add(@"44#" + i);
					}
					i++;
				}
			}
			return missingDocumentsList;
		}

		UnlremremType PopulateUNLREMREM()
		{
			var result = new UnlremremType();
			var stateofseal = wrapper.UnloadingRemark.StateOfSealsOk;
			result.StaOfTheSeaOkrem19ValueSpecified = false;
			if (!stateofseal.IsEmpty)
			{
				result.StaOfTheSeaOkrem19 = stateofseal == "N" ? Flag.Item0 : Flag.Item1;
				result.StaOfTheSeaOkrem19ValueSpecified = true;
			}

			result.UnlRemRem53 = wrapper.HeaderUnloadingNotes;
			result.UnlRemRem53Lng = wrapper.HeaderUnloadingNotesLanguage;
			result.ConRem65 = wrapper.UnloadingRemark.Conform == "N" ? "0" : "1";
			result.UnlComRem66 = wrapper.UnloadingRemark.UnloadingCompletion == "N" ? Flag.Item0 : Flag.Item1;
			result.UnlDatRem67 = wrapper.UnloadingRemark.UnloadingDate;

			return result;
		}
		HeaheaType PopulateHEAHEA()
		{
			return new HeaheaType
			{
				DocNumHea5 = wrapper.MovementReferenceNumber,
				IdeOfMeaOfTraAtDhea78 = wrapper.IdentityOfMeansOfTransportAtDeparture,
				IdeOfMeaOfTraAtDhea78Lng = wrapper.IdentityOfMeansOfTransportAtDepartureLanguage,
				NatOfMeaOfTraAtDhea80 = wrapper.NationalityOfMeansOfTransportAtDeparture,
				TotNumOfIteHea305 = Utilities.FormatNumberFromZIntNational(wrapper.TotalNumberOfItems, 0),
				TotNumOfPacHea306 = Utilities.FormatNumberFromZLongNational(wrapper.TotalNumberOfPackages, 0),
				TotGroMasHea307 = wrapper.TotalGrossMass.Round(3)
			};
		}

		TradestrdType PopulateTradestrd()
		{
			TradestrdType result = null;
			var destinationTrader = wrapper.DestinationTrader;
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
					result.Nadlngrd = destinationTrader.NameAndAddressLanguage.Left(2);
				}
			}
			return result;
		}
	}
}
