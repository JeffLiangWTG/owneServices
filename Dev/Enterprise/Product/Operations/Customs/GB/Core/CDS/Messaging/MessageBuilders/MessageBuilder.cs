using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging.Wrappers;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.GB.Business.GBCommonConstants;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public interface IGbCDSMessageBuilder
	{
		ZString Build();
	}

	public class MessageBuilder : IGbCDSMessageBuilder
	{
		public MessageBuilder(BusinessObject messagingParent, EU.Business.ErrorCollector errorCollector, string functionCodeNewAmendDelete)
		{
			decMessage = new CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration();
			this.messagingParent = messagingParent;
			this.errorCollector = errorCollector;
			this.functionCodeNewAmendDelete = functionCodeNewAmendDelete;
		}
		protected readonly string functionCodeNewAmendDelete;
		protected readonly CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration decMessage;
		protected readonly BusinessObject messagingParent;
		protected DeclarationGoodsShipment decShipment;
		readonly EU.Business.ErrorCollector errorCollector;
		List<DeclarationGoodsShipmentGovernmentAgencyGoodsItem> decGoodsItems;

		protected CusEntryHeader cusEntryHeader => (CusEntryHeader)messagingParent;

		protected IDeclaration Wrapper => wrapper ?? (wrapper = GetCdsDeclarationFromEntry());
		IDeclaration wrapper;

		protected bool IsMovementThroughAnInventoryLinkingLocation() => cusEntryHeader.CH_MasterUCR != string.Empty;

		protected virtual IDeclaration GetCdsDeclarationFromEntry()
		{
			return new GbCDSImportDeclarationWrapper(cusEntryHeader);
		}

		ZString IGbCDSMessageBuilder.Build()
		{
			PopulateFields();
			var result = new MetaData()
				.SetDeclaration(decMessage)
				.Serialize();

			return XmlMessageHelper.RemoveEmptyXmlElements(result);
		}

		protected void PopulateFields()
		{
			PopulateFunctionCode();
			PopulateTypeCode();
			PopulateGoodsItemQuantity();
			PopulateAcceptanceDateTime();
			PopulateFunctionalReferenceID();
			PopulatePresentationOffice();
			PopulateSupervisingOffice();
			PopulateTotalPackageQuantity();
			PopulateExporter();
			PopulateDeclarant();
			PopulateAgent();
			PopulateAuthorisationHolders();
			PopulateCurrencyExchanges();
			PopulateBorderTransportMeans();
			PopulateObligationGuarantees();
			PopulateDecAdditionalDocuments();
			PopulateGoodsShipment();
			PopulateExitOffice();
			PopulateConsignment();
			PopulateInvoiceAmount();
			PopulateSpecificCircumstancesCodeCode();
			PopulateHeaderAddionalDocuments();
		}

		protected virtual void PopulateHeaderAddionalDocuments()
		{
			var headerAIstatements = Wrapper.AdditionalInformations;
			var allWais = new List<DeclarationAdditionalInformation>();
			foreach (var ai in headerAIstatements)
			{
				allWais.Add(
							new DeclarationAdditionalInformation()
							{
								StatementCode = new AdditionalInformationStatementCodeType() { Value = ai.Statement },
								StatementDescription = new AdditionalInformationStatementDescriptionTextType() { Value = ai.StatementText.StripNewlineCharacters().TrimEndSpaceTab().TrimStart() }
							}
							);
			}
			decMessage.AdditionalInformation = allWais.ToArray();
		}

		protected virtual void PopulateSpecificCircumstancesCodeCode()
		{
			var code = Wrapper.SpecificCircumstancesCodeCode;
			if (!code.IsEmpty)
			{
				decMessage.SpecificCircumstancesCodeCode = new DeclarationSpecificCircumstancesCodeCodeType { Value = code };
			}
		}

		protected virtual void PopulateInvoiceAmount()
		{
			var invoiceAmount = Wrapper.InvoiceAmount;
			if (invoiceAmount != null)
			{
				decMessage.InvoiceAmount = new DeclarationInvoiceAmountType
				{
					currencyID = invoiceAmount.Currency,
					Value = invoiceAmount.Amount
				};
			}
		}

		protected virtual void PopulateExitOffice()
		{
		}

		protected virtual void PopulateConsignment()
		{
		}

		protected virtual void PopulateFunctionCode()
		{
			decMessage.FunctionCode = new DeclarationFunctionCodeType() { Value = functionCodeNewAmendDelete };
		}

		protected virtual ZString TypeCode => Wrapper.DeclarationTypeCode;
		protected virtual void PopulateTypeCode()
		{
			var typeCode = new DeclarationTypeCodeType
			{
				Value = TypeCode
			};
			decMessage.TypeCode = typeCode;
		}

		protected virtual ZInt GoodsItemQuantity => Wrapper.GoodsItemQuantity;
		protected virtual void PopulateGoodsItemQuantity()
		{
			var goodsItemQuantity = new DeclarationGoodsItemQuantityType
			{
				Value = GoodsItemQuantity
			};
			decMessage.GoodsItemQuantity = goodsItemQuantity;
		}

		protected virtual ZString FunctionalReferenceID => Wrapper.FunctionalReferenceID;
		protected virtual void PopulateFunctionalReferenceID()
		{
			var functionalReferenceID = new DeclarationFunctionalReferenceIDType
			{
				Value = FunctionalReferenceID
			};
			decMessage.FunctionalReferenceID = functionalReferenceID;
		}

		protected virtual ZDateTime AcceptanceDateTime => Wrapper.AcceptanceDateTime;
		protected virtual void PopulateAcceptanceDateTime()
		{
			var date = AcceptanceDateTime.ToString("yyyyMMddHHmmssZ", CultureInfo.InvariantCulture);

			var acceptanceDateTime = new DeclarationAcceptanceDateTimeType1
			{
				Item = new DeclarationAcceptanceDateTimeTypeDateTimeString1 { Value = date, formatCode = FormatCodeType.Item304 }
			};
			decMessage.AcceptanceDateTime = acceptanceDateTime;
		}

		protected virtual ZString PresentationOffice => Wrapper.PresentationOffice;
		protected virtual void PopulatePresentationOffice()
		{
			var presentationOffice = new DeclarationPresentationOffice
			{
				ID = new PresentationOfficeIdentificationIDType { Value = PresentationOffice }
			};
			decMessage.PresentationOffice = presentationOffice;
		}

		protected virtual ZString SupervisingOffice => Wrapper.SupervisingOffice;
		protected virtual void PopulateSupervisingOffice()
		{
			var supervisingOffice = new DeclarationSupervisingOffice
			{
				ID = new SupervisingOfficeIdentificationIDType { Value = SupervisingOffice }
			};
			decMessage.SupervisingOffice = supervisingOffice;
		}

		protected virtual ZDecimal TotalPackageQuantity => Wrapper.TotalPackageQuantity;
		protected virtual void PopulateTotalPackageQuantity()
		{
			var totalPackageQuantity = new DeclarationTotalPackageQuantityType
			{
				Value = TotalPackageQuantity
			};
			decMessage.TotalPackageQuantity = totalPackageQuantity;
		}

		protected virtual IOrganisation ExporterNameAndAddress => Wrapper.ExporterNameAndAddress;
		protected virtual IOrganisation Exporter => Wrapper.Exporter;
		protected virtual void PopulateExporter()
		{
			if (Exporter != null)
			{
				var exporter = new DeclarationExporter
				{
					ID = !Exporter.ID.IsEmpty ? new ExporterIdentificationIDType { Value = Exporter.ID } : null,
					Name = new ExporterNameTextType { Value = Exporter.Name },
					Address = GetIOrgAddress<DeclarationExporterAddress>(Exporter)
				};
				decMessage.Exporter = exporter;
			}
		}

		protected virtual IOrganisation Declarant => Wrapper.Declarant;
		protected virtual void PopulateDeclarant()
		{
			if (Declarant != null)
			{
				if (Declarant.ID.IsEmpty)
				{
					var declarant = new DeclarationDeclarant
					{
						Name = new DeclarantNameTextType { Value = Declarant.Name },
						Address = GetIOrgAddress<DeclarationDeclarantAddress>(Declarant)
					};
					decMessage.Declarant = declarant;
				}
				else
				{
					var declarant = new DeclarationDeclarant
					{
						ID = new DeclarantIdentificationIDType { Value = Declarant.ID }
					};
					if (GBCustomsDataRegistry.Instance.SendCDS317.Value)
					{
						declarant.Name = new DeclarantNameTextType { Value = Declarant.Name };
						declarant.Address = GetIOrgAddress<DeclarationDeclarantAddress>(Declarant);
					}
					decMessage.Declarant = declarant;
				}
			}
		}

		protected virtual IAgent Agent => Wrapper.Agent;
		protected virtual void PopulateAgent()
		{
			var agent = Agent;
			if (agent != null)
			{
				var orgAgent = agent?.Agent;
				var orgAgentAddress = orgAgent?.Address;
				var hasEORI = !orgAgent?.ID.IsEmpty ?? false;
				if (!agent.FunctionCode.IsEmpty)
				{
					if (hasEORI)
					{
						decMessage.Agent = new DeclarationAgent
						{
							ID = new AgentIdentificationIDType { Value = orgAgent.ID },
							FunctionCode = new AgentFunctionCodeType { Value = agent.FunctionCode }
						};
					}
					else
					{
						decMessage.Agent = new DeclarationAgent
						{
							Name = new AgentNameTextType { Value = orgAgent?.Name ?? ZString.Empty },
							Address = new DeclarationAgentAddress
							{
								Line = new AddressLineTextType { Value = orgAgentAddress?.Line ?? ZString.Empty },
								CityName = new AddressCityNameTextType { Value = orgAgentAddress?.CityName ?? ZString.Empty },
								CountryCode = new AddressCountryCodeType { Value = ConvertCountryCodeIfNeeded(orgAgentAddress?.CountryCode ?? ZString.Empty) },
								PostcodeID = new AddressPostcodeIDType { Value = orgAgentAddress?.PostcodeID ?? ZString.Empty }
							},
							FunctionCode = new AgentFunctionCodeType { Value = agent.FunctionCode }
						};
					}
				}
			}
		}

		protected virtual IEnumerable<IAuthorisationHolder> AuthorisationHolders => Wrapper.AuthorisationHolders;
		protected virtual void PopulateAuthorisationHolders()
		{
			var authorisationHolders = new List<DeclarationAuthorisationHolder>();
			if (AuthorisationHolders != null)
			{
				foreach (var authorisationHolder in AuthorisationHolders)
				{
					authorisationHolders.Add(new DeclarationAuthorisationHolder
					{
						ID = new AuthorisationHolderIdentificationIDType { Value = authorisationHolder.ID },
						CategoryCode = new AuthorisationHolderCategoryCodeType { Value = authorisationHolder.CategoryCode }
					});
				}
			}

			decMessage.AuthorisationHolder = authorisationHolders.ToArray();
		}

		protected virtual IEnumerable<ICurrencyExchange> Exchanges => Wrapper.CurrencyExchanges;
		protected virtual void PopulateCurrencyExchanges()
		{
			var exchanges = new List<DeclarationCurrencyExchange>();
			if (Exchanges != null)
			{
				foreach (var exchange in Exchanges)
				{
					exchanges.Add(new DeclarationCurrencyExchange
					{
						RateNumeric = exchange.RateNumeric
					});
				}
			}
			decMessage.CurrencyExchange = exchanges.ToArray();
		}

		protected virtual ITransportMeans BorderTransportMeans => Wrapper.BorderTransportMeans;
		protected virtual void PopulateBorderTransportMeans()
		{
			if (BorderTransportMeans != null)
			{
				var borderTransportMeans = BorderTransportMeans == null ? new DeclarationBorderTransportMeans() : new DeclarationBorderTransportMeans
				{
					IdentificationTypeCode = new BorderTransportMeansIdentificationTypeCodeType { Value = BorderTransportMeans.IdentificationTypeCode },
					ModeCode = new BorderTransportMeansModeCodeType { Value = BorderTransportMeans.ModeCode },
					RegistrationNationalityCode = new BorderTransportMeansRegistrationNationalityCodeType { Value = BorderTransportMeans.RegistrationNationalityCode }
				};
				decMessage.BorderTransportMeans = borderTransportMeans;
			}
		}

		protected virtual IEnumerable<IObligationGuarantee> ObligationGuarantees => Wrapper.ObligationGuarantees;
		protected virtual void PopulateObligationGuarantees()
		{
			PopulateObligationGuaranteesBase();
		}

		protected void PopulateObligationGuaranteesBase()
		{
			var obligationGuarantees = new List<DeclarationObligationGuarantee>();
			if (ObligationGuarantees != null)
			{
				foreach (var obligationGuarantee in ObligationGuarantees)
				{
					obligationGuarantees.Add(new DeclarationObligationGuarantee
					{
						SecurityDetailsCode = new ObligationGuaranteeSecurityDetailsCodeType { Value = obligationGuarantee.SecurityDetailsCode },
						ID = new ObligationGuaranteeIdentificationIDType { Value = obligationGuarantee.ID },
						ReferenceID = new ObligationGuaranteeReferenceIDType { Value = obligationGuarantee.ReferenceID }
					});
				}
			}

			decMessage.ObligationGuarantee = obligationGuarantees.ToArray();
		}

		protected virtual IEnumerable<IDecAdditionalDocument> DecAdditionalDocuments => Wrapper.DecAdditionalDocuments;
		protected virtual void PopulateDecAdditionalDocuments()
		{
			var decAdditionalDocuments = new List<DeclarationAdditionalDocument>();
			if (DecAdditionalDocuments != null)
			{
				foreach (var decAdditionalDocument in DecAdditionalDocuments.OrderBy(x => x.SystemCreateTime).ThenBy(x => x.TypeCode).ThenBy(x => x.CategoryCode).ThenBy(x => x.ID))
				{
					decAdditionalDocuments.Add(new DeclarationAdditionalDocument()
					{
						ID = new AdditionalDocumentIdentificationIDType { Value = decAdditionalDocument.ID },
						CategoryCode = new AdditionalDocumentCategoryCodeType { Value = decAdditionalDocument.CategoryCode },
						TypeCode = new AdditionalDocumentTypeCodeType { Value = decAdditionalDocument.TypeCode }
					});
				}
			}

			decMessage.AdditionalDocument = decAdditionalDocuments.ToArray();
		}

		#region Goods Shipment
		protected virtual void PopulateGoodsShipment()
		{
			decShipment = new DeclarationGoodsShipment();
			PopulateDestination();
			PopulateUCRTraderAssignedReferenceID();
			PopulateExportCountryID();
			PopulateTransactionNatureCode();
			PopulateWareHouse();
			PopulateImporter();
			PopulateConsignee();
			PopulateAEOMutualRecognitionParties();
			PopulateDomesticDutyTaxParties();
			PopulateTradeTerms();
			PopulateCustomsValuations();
			PopulateConsignments();
			PopulatePreviousDocuments();
			PopulateGovernmentAgencyGoodsItems();

			decShipment.GovernmentAgencyGoodsItem = decGoodsItems.ToArray();
			decMessage.GoodsShipment = decShipment;
		}

		protected T GetIOrgAddress<T>(IOrganisation organisation) where T : IAddressForOrg, new()
		{
			var orgAddress = organisation.Address;
			return new T
			{
				CityName = new AddressCityNameTextType { Value = orgAddress?.CityName ?? ZString.Empty },
				CountryCode = new AddressCountryCodeType { Value = ConvertCountryCodeIfNeeded(orgAddress?.CountryCode ?? ZString.Empty) },
				Line = new AddressLineTextType { Value = orgAddress?.Line.SubstringSafe(0, AddressLineLength) ?? ZString.Empty },
				PostcodeID = new AddressPostcodeIDType { Value = orgAddress?.PostcodeID ?? ZString.Empty }
			};
		}

		protected string ConvertCountryCodeIfNeeded(ZString countryCode)
		{
			if (!countryCode.IsEmpty)
			{
				var convertedCode = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(messagingParent.Factory,
					Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
					RefCusMapTypeList.Codes.EUCTY, countryCode, ZDateTime.Today);
				if (!convertedCode.IsEmpty)
				{
					countryCode = convertedCode;
				}
			}
			return countryCode;
		}

		protected virtual ZInt AddressLineLength => 70;

		protected virtual ZString Destination => Wrapper.GoodsShipment.DestinationCountryCode;
		protected virtual void PopulateDestination()
		{
			var itemDestinations = GoodsItems?.Select(x => x.DestinationCountryCode)?.Where(x => !x.IsEmpty);

			if (itemDestinations.Distinct().Count() <= 1)
			{
				var destination = Destination.IsEmpty ? new DeclarationGoodsShipmentDestination() : new DeclarationGoodsShipmentDestination
				{
					CountryCode = new DestinationCountryCodeType { Value = Destination },
				};
				decShipment.Destination = destination;
			}
		}

		protected virtual ZString UCRTraderAssignedReferenceID => Wrapper.GoodsShipment.UCRTraderAssignedReferenceID;
		protected virtual void PopulateUCRTraderAssignedReferenceID()
		{
			var uCRTraderAssignedReferenceID = UCRTraderAssignedReferenceID.IsEmpty ? new DeclarationGoodsShipmentUCR() : new DeclarationGoodsShipmentUCR
			{
				TraderAssignedReferenceID = new UCRTraderAssignedReferenceIDType { Value = UCRTraderAssignedReferenceID }
			};
			decShipment.UCR = uCRTraderAssignedReferenceID;
		}

		protected virtual ZString ExportCountryID => Wrapper.GoodsShipment.ExportCountryID;
		protected virtual void PopulateExportCountryID()
		{
		}

		protected virtual ZString TransactionNatureCode => Wrapper.GoodsShipment.TransactionNatureCode;
		protected virtual void PopulateTransactionNatureCode()
		{
			var transactionNatureCode = TransactionNatureCode.IsEmpty ? new GoodsShipmentTransactionNatureCodeType() : new GoodsShipmentTransactionNatureCodeType
			{
				Value = TransactionNatureCode
			};
			decShipment.TransactionNatureCode = transactionNatureCode;
		}

		protected virtual IWareHouse WareHouse => Wrapper.GoodsShipment.Warehouse;
		protected virtual void PopulateWareHouse()
		{
			var wareHouse = WareHouse == null ? new DeclarationGoodsShipmentWarehouse() : new DeclarationGoodsShipmentWarehouse
			{
				ID = new WarehouseIdentificationIDType { Value = WareHouse.ID },
				TypeCode = new WarehouseTypeCodeType { Value = WareHouse.TypeCode }
			};
			decShipment.Warehouse = wareHouse;
		}

		protected virtual void PopulateConsignee()
		{
		}

		protected virtual IOrganisation Importer => Wrapper.GoodsShipment.Importer;
		protected virtual void PopulateImporter()
		{
			var importer = Importer == null ? new DeclarationGoodsShipmentImporter() : new DeclarationGoodsShipmentImporter
			{
				ID = new ImporterIdentificationIDType { Value = Importer.ID },
				Name = new ImporterNameTextType { Value = Importer.Name },
				Address = GetIOrgAddress<DeclarationGoodsShipmentImporterAddress>(Importer)
			};
			decShipment.Importer = importer;
		}

		protected virtual IEnumerable<IParty> AEOMutualRecognitionParties => Wrapper.GoodsShipment.AEOMutualRecognitionParties;
		protected virtual void PopulateAEOMutualRecognitionParties()
		{
			var aeoMutualRecognitionParties = new List<DeclarationGoodsShipmentAEOMutualRecognitionParty>();
			if (AEOMutualRecognitionParties != null)
			{
				foreach (var aEOMutualRecognitionParty in AEOMutualRecognitionParties)
				{
					aeoMutualRecognitionParties.Add(new DeclarationGoodsShipmentAEOMutualRecognitionParty
					{
						ID = new AEOMutualRecognitionPartyIdentificationIDType { Value = aEOMutualRecognitionParty.ID },
						RoleCode = new AEOMutualRecognitionPartyRoleCodeType { Value = aEOMutualRecognitionParty.RoleCode }
					});
				}
			}
			else
			{
				aeoMutualRecognitionParties.Add(new DeclarationGoodsShipmentAEOMutualRecognitionParty());
			}
			decShipment.AEOMutualRecognitionParty = aeoMutualRecognitionParties.ToArray();
		}

		protected virtual IEnumerable<IParty> DomesticDutyTaxParties => Wrapper.GoodsShipment.DomesticDutyTaxParties;
		protected virtual void PopulateDomesticDutyTaxParties()
		{
			PopulateDomesticDutyTaxPartiesBase();
		}

		protected void PopulateDomesticDutyTaxPartiesBase()
		{
			var domesticDutyTaxParties = new List<DeclarationGoodsShipmentDomesticDutyTaxParty>();
			if (DomesticDutyTaxParties != null)
			{
				foreach (var domesticDutyTaxParty in DomesticDutyTaxParties)
				{
					domesticDutyTaxParties.Add(new DeclarationGoodsShipmentDomesticDutyTaxParty
					{
						ID = new DomesticDutyTaxPartyIdentificationIDType { Value = domesticDutyTaxParty.ID },
						RoleCode = new DomesticDutyTaxPartyRoleCodeType { Value = domesticDutyTaxParty.RoleCode }
					});
				}
			}
			else
			{
				domesticDutyTaxParties.Add(new DeclarationGoodsShipmentDomesticDutyTaxParty());
			}
			decShipment.DomesticDutyTaxParty = domesticDutyTaxParties.ToArray();
		}

		protected virtual ITradeTerms TradeTerms => Wrapper.GoodsShipment.TradeTerms;
		protected virtual void PopulateTradeTerms()
		{
			var tradeTerms = TradeTerms == null ? new DeclarationGoodsShipmentTradeTerms() : new DeclarationGoodsShipmentTradeTerms
			{
				ConditionCode = new TradeTermsConditionCodeType { Value = TradeTerms.ConditionCode },
				LocationID = new TradeTermsLocationIDType { Value = TradeTerms.LocationID },
				LocationName = new TradeTermsLocationNameTextType { Value = TradeTerms.LocationName }
			};
			decShipment.TradeTerms = tradeTerms;
		}

		protected virtual ICustomsValuation CustomsValuation => Wrapper.GoodsShipment.CustomsValuation;
		protected virtual void PopulateCustomsValuations()
		{
			var giCustomsValuation = CustomsValuation;
			decShipment.CustomsValuation = giCustomsValuation != null ? new DeclarationGoodsShipmentCustomsValuation
			{
				ChargeDeduction = giCustomsValuation.ChargeDeductions?.Where(x => (x.OtherChargeDeductionAmount?.Amount ?? ZDecimal.Zero) != ZDecimal.Zero)?.Select(chargeDeduction => new DeclarationGoodsShipmentCustomsValuationChargeDeduction
				{
					ChargesTypeCode = new ChargeDeductionChargesTypeCodeType { Value = chargeDeduction.ChargesTypeCode },
					OtherChargeDeductionAmount = new ChargeDeductionOtherChargeDeductionAmountType { Value = chargeDeduction.OtherChargeDeductionAmount?.Amount ?? ZDecimal.Zero, currencyID = chargeDeduction.OtherChargeDeductionAmount?.Currency ?? ZString.Empty },
				}).ToArray() ?? Array.Empty<DeclarationGoodsShipmentCustomsValuationChargeDeduction>()
			} : new DeclarationGoodsShipmentCustomsValuation();
			PopulateCustomsValuationFreightChargeAmount(decShipment.CustomsValuation);
		}

		protected virtual void PopulateCustomsValuationFreightChargeAmount(DeclarationGoodsShipmentCustomsValuation customsValuation)
		{
		}

		protected virtual IEnumerable<IPreviousDocument> PreviousDocuments => Wrapper.GoodsShipment.PreviousDocuments;
		protected virtual void PopulatePreviousDocuments()
		{
			var goodsShipmentPreviousDocuments = new List<DeclarationGoodsShipmentPreviousDocument>();
			if (PreviousDocuments != null)
			{
				goodsShipmentPreviousDocuments.AddRange(PreviousDocuments.OrderBy(x => x.SystemCreateTime).ThenBy(x => x.TypeCode).ThenBy(x => x.CategoryCode).ThenBy(x => x.ID).Select(previousDocument => new DeclarationGoodsShipmentPreviousDocument
				{
					CategoryCode = new PreviousDocumentCategoryCodeType
					{
						Value = previousDocument.CategoryCode
					},
					ID = new PreviousDocumentIdentificationIDType
					{
						Value = previousDocument.ID
					},
					LineNumeric = previousDocument.LineNumeric,
					LineNumericSpecified = previousDocument.LineNumeric > 0,
					TypeCode = new PreviousDocumentTypeCodeType
					{
						Value = previousDocument.TypeCode
					}
				}));
			}

			decShipment.PreviousDocument = goodsShipmentPreviousDocuments.ToArray();
		}

		protected virtual IConsignment Consignment => Wrapper.GoodsShipment.Consignment;
		protected virtual void PopulateConsignments()
		{
			DeclarationGoodsShipmentConsignment consignment;
			if (Consignment == null)
			{
				consignment = new DeclarationGoodsShipmentConsignment();
			}
			else
			{
				consignment = new DeclarationGoodsShipmentConsignment
				{
					GoodsLocation = new DeclarationGoodsShipmentConsignmentGoodsLocation
					{
						Name = new GoodsLocationNameTextType { Value = Consignment.GoodsLocation.Name },
						TypeCode = new GoodsLocationTypeCodeType { Value = Consignment.GoodsLocation.TypeCode },
						Address = new DeclarationGoodsShipmentConsignmentGoodsLocationAddress
						{
							CountryCode = new AddressCountryCodeType { Value = ConvertCountryCodeIfNeeded(Consignment.GoodsLocation.CountryCode) },
							TypeCode = new AddressTypeCodeType { Value = Consignment.GoodsLocation.AddressTypeCode }
						}
					}
				};
				PopulateConsignmentContainerCode(consignment);
				PopulateConsignmentTransportEquipment(consignment);
				PopulateArrivalTransportMeans(consignment, Consignment);
				PopulateDepartureTransportMeans(consignment, Consignment);
				PopulateLoadingLocationID(consignment, Consignment);
			}
			decShipment.Consignment = consignment;
		}

		protected virtual void PopulateConsignmentContainerCode(DeclarationGoodsShipmentConsignment consignment)
		{
			consignment.ContainerCode = new ConsignmentContainerCodeType { Value = cusEntryHeader.Containers.Any() ? "1" : "0" };
		}

		protected virtual void PopulateConsignmentTransportEquipment(DeclarationGoodsShipmentConsignment consignment)
		{
			consignment.TransportEquipment = CreateTransportsForHeaderContainers() ?? Array.Empty<DeclarationGoodsShipmentConsignmentTransportEquipment>().ToArray();
		}

		protected virtual DeclarationGoodsShipmentConsignmentTransportEquipment[] CreateTransportsForHeaderContainers()
		{
			int index = 1;
			return Consignment.TransportEquipments?.Select(x => new DeclarationGoodsShipmentConsignmentTransportEquipment
			{
				ID = new TransportEquipmentIdentificationIDType { Value = x.ContainerNo },
				Seal = PopulateSeals(x),
				SequenceNumeric = index++
			}).ToArray();
		}

		protected virtual ZString[] GetSeals(ITransportEquipment transportEquipment)
		{
			var seals = new List<ZString>();
			if (!transportEquipment.SealNo.IsEmpty)
			{
				seals.Add(transportEquipment.SealNo);
			}

			if (!transportEquipment.SecondSealNo.IsEmpty)
			{
				seals.Add(transportEquipment.SecondSealNo);
			}

			return seals.ToArray();
		}

		protected virtual DeclarationGoodsShipmentConsignmentTransportEquipmentSeal[] PopulateSeals(ITransportEquipment transportEquipment)
		{
			return Array.Empty<DeclarationGoodsShipmentConsignmentTransportEquipmentSeal>();
		}

		protected virtual void PopulateDepartureTransportMeans(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
		}

		protected virtual void PopulateArrivalTransportMeans(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
			PopulateArrivalTransportMeansBase(consignment, consignmentData);
		}

		protected void PopulateArrivalTransportMeansBase(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
			var arrivalTransportMeans = consignmentData.ArrivalTransportMeans;
			if (IsArrivalTransportMeansDetailsRequired)
			{
				consignment.ArrivalTransportMeans = new DeclarationGoodsShipmentConsignmentArrivalTransportMeans
				{
					ID = new ArrivalTransportMeansIdentificationIDType { Value = arrivalTransportMeans?.ID },
					IdentificationTypeCode = new ArrivalTransportMeansIdentificationTypeCodeType { Value = arrivalTransportMeans?.IdentificationTypeCode },
					ModeCode = new ArrivalTransportMeansModeCodeType { Value = arrivalTransportMeans?.ModeCode }
				};
			}
			else
			{
				consignment.ArrivalTransportMeans = new DeclarationGoodsShipmentConsignmentArrivalTransportMeans
				{
					ModeCode = new ArrivalTransportMeansModeCodeType { Value = arrivalTransportMeans?.ModeCode }
				};
			}
		}

		protected bool IsArrivalTransportMeansDetailsRequired => IsArrivalTransportMeansDetailsRequiredCore;

		protected virtual bool IsArrivalTransportMeansDetailsRequiredCore =>
			cusEntryHeader.Declaration.JE_TransportMode != Enterprise.Customs.Business.TransportTypeList.Codes.Mail
			&& cusEntryHeader.Declaration.JE_TransportMode != Enterprise.Customs.Business.TransportTypeList.Codes.FixedTransportInstallations;

		protected virtual void PopulateLoadingLocationID(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
			PopulateLoadingLocationIDBase(consignment, consignmentData);
		}

		protected void PopulateLoadingLocationIDBase(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
			consignment.LoadingLocation = new DeclarationGoodsShipmentConsignmentLoadingLocation
			{
				ID = new LoadingLocationIdentificationIDType
				{
					Value = consignmentData.LoadingLocationID
				}
			};
		}

		#endregion

		#region Goods Item
		protected virtual IEnumerable<IGovernmentAgencyGoodsItem> GoodsItems => Wrapper.GoodsShipment.GovernmentAgencyGoodsItems;

		protected virtual void PopulateGovernmentAgencyGoodsItems()
		{
			decGoodsItems = new List<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();

			var index = 1;
			foreach (var goodsItem in GoodsItems)
			{
				decGoodsItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
				{
					SequenceNumeric = index
				};

				PopulateGovernmentAgencyGoodsItem(goodsItem, index);
				index++;
				decGoodsItems.Add(decGoodsItem);
			}
		}

		protected virtual void PopulateGovernmentAgencyGoodsItem(IGovernmentAgencyGoodsItem goodsItemWrapper, int index)
		{
			PopulateValuationAdjustmentAdditionCode(goodsItemWrapper);
			PopulateExportCountryID(goodsItemWrapper);
			PopulateTransactionNatureCode(goodsItemWrapper);
			PopulateItemConsignor(goodsItemWrapper);// 3/1 and 3/2, exporter at item level
			PopulateAEOMutualRecognitionParties(goodsItemWrapper);
			PopulateDomesticDutyTaxParties(goodsItemWrapper);
			PopulateDestinationCountryCode(goodsItemWrapper);
			PopulatePackagings(goodsItemWrapper);
			PopulateStatisticalValue(goodsItemWrapper);
			PopulateCustomsValuations(goodsItemWrapper);
			PopulatePreviousDocuments(goodsItemWrapper);
			PopulateCommodity(goodsItemWrapper);
			PopulateGovernmentProcedures(goodsItemWrapper);
			PopulateAdditionalDocuments(goodsItemWrapper);
			PopulateAdditionalInformations(goodsItemWrapper, index);
			PopulateOrigins(goodsItemWrapper);
			PopulateCustomsValueAmount(goodsItemWrapper);
		}

		protected virtual void PopulateCustomsValueAmount(IGovernmentAgencyGoodsItem goodsItemWrapper)
		{
		}

		protected DeclarationGoodsShipmentGovernmentAgencyGoodsItem decGoodsItem;

		protected virtual void PopulateOrigins(IGovernmentAgencyGoodsItem goodsItem)
		{
			decGoodsItem.Origin = goodsItem.Origins?.Select(origin => new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin
			{
				CountryCode = new OriginCountryCodeType { Value = origin.CountryCode },
				TypeCode = new OriginTypeCodeType { Value = origin.TypeCode }
			}).ToArray() ?? Array.Empty<DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin>();
		}

		protected virtual void PopulateAdditionalDocuments(IGovernmentAgencyGoodsItem goodsItem)
		{
			var additionalDocuments = new List<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument>();
			var goodsItemAdditionalDocuments = goodsItem.AdditionalDocuments;
			if (goodsItemAdditionalDocuments != null)
			{
				foreach (var additionalDocument in goodsItemAdditionalDocuments.OrderBy(x => x.SystemCreateTime).ThenBy(x => x.TypeCode).ThenBy(x => x.CategoryCode).ThenBy(x => x.ID))
				{
					var declarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument
					{
						CategoryCode = new AdditionalDocumentCategoryCodeType { Value = additionalDocument.CategoryCode },

						ID = new AdditionalDocumentIdentificationIDType { Value = additionalDocument.ID },
						LPCOExemptionCode = new AdditionalDocumentLPCOExemptionCodeType { Value = additionalDocument.LPCOExemptionCode },
						Name = new AdditionalDocumentNameTextType { Value = additionalDocument.Name },
						TypeCode = new AdditionalDocumentTypeCodeType { Value = additionalDocument.TypeCode },
					};
					PopulateSubmitter(declarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument, additionalDocument);
					PopulateEffectiveDateTime(declarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument, additionalDocument);
					PopulateWriteOff(declarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument, additionalDocument);
					additionalDocuments.Add(declarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument);
				}
			}

			decGoodsItem.AdditionalDocument = additionalDocuments.ToArray();
		}

		protected virtual void PopulateSubmitter(DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument result, IAdditionalDocument additionalDocument)
		{
			result.Submitter = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentSubmitter
			{
				Name = new SubmitterNameTextType { Value = additionalDocument.Submitter }
			};
		}

		protected virtual void PopulateEffectiveDateTime(DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument result, IAdditionalDocument additionalDocument)
		{
			result.EffectiveDateTime = new AdditionalDocumentEffectiveDateTimeType
			{
				Item = new AdditionalDocumentEffectiveDateTimeTypeDateTimeString
				{
					Value = additionalDocument.EffectiveDateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
					formatCode = FormatCodeType.Item102
				}
			};
		}

		protected virtual void PopulateWriteOff(DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument result, IAdditionalDocument additionalDocument)
		{
			var quantityQuantity = additionalDocument.WriteOff?.QuantityQuantity ?? ZDecimal.Zero;
			var quantityQuantityUnitCode = additionalDocument.WriteOff?.QuantityQuantityUnitCode ?? ZString.Empty;

			if (!quantityQuantity.IsEmpty || !quantityQuantityUnitCode.IsEmpty)
			{
				result.WriteOff = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentWriteOff
				{
					QuantityQuantity = new WriteOffQuantityQuantityType { Value = quantityQuantity, unitCode = quantityQuantityUnitCode },
				};
			}
		}

		protected virtual void PopulateAdditionalInformations(IGovernmentAgencyGoodsItem goodsItem, int index)
		{
			var additionalInformations = new List<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation>();

			var goodsItemAdditionalInformations = goodsItem.AdditionalInformations;
			if (goodsItemAdditionalInformations != null)
			{
				foreach (var additionalInformation in goodsItemAdditionalInformations)
				{
					additionalInformations.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation
					{
						StatementCode = new AdditionalInformationStatementCodeType { Value = additionalInformation.Statement },
						StatementDescription = new AdditionalInformationStatementDescriptionTextType { Value = additionalInformation.StatementText.StripNewlineCharacters().TrimEndSpaceTab().TrimStart() }
					});
				}
			}

			decGoodsItem.AdditionalInformation = additionalInformations.ToArray();
		}

		protected virtual void PopulateGovernmentProcedures(IGovernmentAgencyGoodsItem iGoodsItemWrapper)
		{
			var procedures = new List<DeclarationGoodsShipmentGovernmentAgencyGoodsItemGovernmentProcedure>();
			foreach (var procedure in iGoodsItemWrapper.GovernmentProcedures)
			{
				if (!procedure.CurrentCode.IsEmpty)
				{
					var proc = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGovernmentProcedure
					{
						CurrentCode = new GovernmentProcedureCurrentCodeType
						{
							Value = procedure.CurrentCode
						}
					};
					if (!procedure.PreviousCode.IsEmpty)
					{
						proc.PreviousCode = new GovernmentProcedurePreviousCodeType { Value = procedure.PreviousCode };
					}
					procedures.Add(proc);
				}
			}
			if (procedures.Count == 0)
			{
				errorCollector.AddError("At least one procedure is needed", new EU.Business.ErrorInfo("1/10, 1/11", "mandatory"));
			}
			decGoodsItem.GovernmentProcedure = procedures.ToArray();
		}

		protected virtual void PopulateValuationAdjustmentAdditionCode(IGovernmentAgencyGoodsItem goodsItem)
		{
			var goodsItemValuationAdjustmentAdditionCode = goodsItem.ValuationAdjustmentAdditionCode;
			if (!goodsItemValuationAdjustmentAdditionCode.IsEmpty)
			{
				decGoodsItem.ValuationAdjustment = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustment
				{
					AdditionCode = new ValuationAdjustmentAdditionCodeType { Value = goodsItemValuationAdjustmentAdditionCode }
				};
			}
		}

		protected virtual void PopulateExportCountryID(IGovernmentAgencyGoodsItem goodsItem)
		{
			var exportCountryCode = goodsItem.ExportCountryCode;
			var giConsignor = goodsItem.Consignor; // consignor will be null if only 1 exporter
			if (giConsignor != null)
			{
				decGoodsItem.ExportCountry = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemExportCountry
				{
					ID = new ExportCountryCountryCodeType { Value = exportCountryCode.IsEmpty ? ConvertCountryCodeIfNeeded(giConsignor.Address.CountryCode) : exportCountryCode }
				};
			}
			else if (!exportCountryCode.IsEmpty)
			{
				decGoodsItem.ExportCountry = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemExportCountry
				{
					ID = new ExportCountryCountryCodeType { Value = exportCountryCode }
				};
			}
			else
			{
				var itemExportCountryID = ExportCountryID.IsEmpty ? new DeclarationGoodsShipmentGovernmentAgencyGoodsItemExportCountry() : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemExportCountry
				{
					ID = new ExportCountryCountryCodeType { Value = ExportCountryID }
				};
				decGoodsItem.ExportCountry = itemExportCountryID;
			}
		}

		protected virtual void PopulateTransactionNatureCode(IGovernmentAgencyGoodsItem goodsItem)
		{
			var giTransactionNatureCode = goodsItem.TransactionNatureCode;
			var transactionNatureCode = giTransactionNatureCode.IsEmpty ? new GovernmentAgencyGoodsItemTransactionNatureCodeType() : new GovernmentAgencyGoodsItemTransactionNatureCodeType()
			{
				Value = giTransactionNatureCode
			};
			decGoodsItem.TransactionNatureCode = transactionNatureCode;
		}

		protected virtual void PopulateItemConsignor(IGovernmentAgencyGoodsItem goodsItem)
		{
			var giConsignor = goodsItem.Consignor;
			if (giConsignor != null)
			{
				decGoodsItem.Consignor = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemConsignor
				{
					ID = new ConsignorIdentificationIDType { Value = giConsignor.ID },
					Name = new ConsignorNameTextType { Value = giConsignor.Name },
					Address = GetIOrgAddress<DeclarationGoodsShipmentGovernmentAgencyGoodsItemConsignorAddress>(giConsignor)
				};
			}
		}

		protected virtual void PopulateAEOMutualRecognitionParties(IGovernmentAgencyGoodsItem goodsItem)
		{
			var giAEOMutualRecognitionParties = goodsItem.AEOMutualRecognitionParties;
			var aEOMutualRecognitionParties = new List<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAEOMutualRecognitionParty>();
			if (giAEOMutualRecognitionParties != null)
			{
				foreach (var aEOMutualRecognitionParty in giAEOMutualRecognitionParties)
				{
					aEOMutualRecognitionParties.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAEOMutualRecognitionParty
					{
						ID = new AEOMutualRecognitionPartyIdentificationIDType { Value = aEOMutualRecognitionParty.ID },
						RoleCode = new AEOMutualRecognitionPartyRoleCodeType { Value = aEOMutualRecognitionParty.RoleCode }
					});
				}
			}
			else
			{
				aEOMutualRecognitionParties.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAEOMutualRecognitionParty());
			}
			decGoodsItem.AEOMutualRecognitionParty = aEOMutualRecognitionParties.ToArray();
		}

		protected virtual void PopulateDomesticDutyTaxParties(IGovernmentAgencyGoodsItem goodsItem)
		{
			PopulateDomesticDutyTaxPartiesBase(goodsItem);
		}

		protected void PopulateDomesticDutyTaxPartiesBase(IGovernmentAgencyGoodsItem goodsItem)
		{
			var giDomesticDutyTaxParties = goodsItem.DomesticDutyTaxParties;
			var domesticDutyTaxParties = new List<DeclarationGoodsShipmentGovernmentAgencyGoodsItemDomesticDutyTaxParty>();
			if (giDomesticDutyTaxParties != null)
			{
				foreach (var domesticDutyTaxParty in giDomesticDutyTaxParties)
				{
					domesticDutyTaxParties.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemDomesticDutyTaxParty
					{
						ID = new DomesticDutyTaxPartyIdentificationIDType { Value = domesticDutyTaxParty.ID },
						RoleCode = new DomesticDutyTaxPartyRoleCodeType { Value = domesticDutyTaxParty.RoleCode }
					});
				}
			}
			else
			{
				domesticDutyTaxParties.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemDomesticDutyTaxParty());
			}
			decGoodsItem.DomesticDutyTaxParty = domesticDutyTaxParties.ToArray();
		}

		protected virtual void PopulateDestinationCountryCode(IGovernmentAgencyGoodsItem goodsItem)
		{
			if (decShipment.Destination == null)
			{
				var giDestination = goodsItem.DestinationCountryCode;
				var destination = giDestination.IsEmpty ? new DeclarationGoodsShipmentGovernmentAgencyGoodsItemDestination() : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemDestination
				{
					CountryCode = new DestinationCountryCodeType { Value = giDestination },
				};
				decGoodsItem.Destination = destination;
			}
		}

		protected virtual void PopulatePackagings(IGovernmentAgencyGoodsItem goodsItem)
		{
			var giPackagings = goodsItem.Packagings;
			var packagings = new List<DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging>();
			if (giPackagings != null)
			{
				int index = 1;
				foreach (var packaging in giPackagings)
				{
					var goodsItemPackaging = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging
					{
						SequenceNumeric = index++,
						SequenceNumericSpecified = true,
						QuantityQuantity = new PackagingQuantityQuantityType { Value = packaging.Quantity },
					};
					PopulateMarksNumbersID(goodsItemPackaging, packaging);
					PopulateTypeCode(goodsItemPackaging, packaging);
					packagings.Add(goodsItemPackaging);
				}
			}
			else
			{
				packagings.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging());
			}
			decGoodsItem.Packaging = packagings.ToArray();
		}

		protected virtual void PopulateMarksNumbersID(DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging goodsItemPackaging, IPackaging packaging)
		{
			goodsItemPackaging.MarksNumbersID = new PackagingMarksNumbersIDType { Value = packaging.MarksNumbersID };
		}

		protected virtual void PopulateTypeCode(DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging goodsItemPackaging, IPackaging packaging)
		{
			goodsItemPackaging.TypeCode = new PackagingTypeCodeType { Value = packaging.TypeCode };
		}

		protected virtual void PopulateStatisticalValue(IGovernmentAgencyGoodsItem goodsItem)
		{
			var giStatisticalValue = goodsItem.StatisticalValue;
			var statisticalValue = giStatisticalValue == null ? new GovernmentAgencyGoodsItemStatisticalValueAmountType() : new GovernmentAgencyGoodsItemStatisticalValueAmountType
			{
				Value = giStatisticalValue.Amount,
				currencyID = giStatisticalValue.Currency
			};
			decGoodsItem.StatisticalValueAmount = statisticalValue;
		}

		protected virtual void PopulateCustomsValuations(IGovernmentAgencyGoodsItem goodsItem)
		{
			var giCustomsValuation = goodsItem.CustomsValuation;
			decGoodsItem.CustomsValuation = giCustomsValuation != null ? new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCustomsValuation
			{
				MethodCode = new CustomsValuationMethodCodeType { Value = giCustomsValuation.MethodCode },
				ChargeDeduction = giCustomsValuation.ChargeDeductions?.Where(x => (x.OtherChargeDeductionAmount?.Amount ?? ZDecimal.Zero) != ZDecimal.Zero)?.Select(chargeDeduction => new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCustomsValuationChargeDeduction
				{
					ChargesTypeCode = new ChargeDeductionChargesTypeCodeType { Value = chargeDeduction.ChargesTypeCode },
					OtherChargeDeductionAmount = new ChargeDeductionOtherChargeDeductionAmountType { Value = chargeDeduction.OtherChargeDeductionAmount?.Amount ?? ZDecimal.Zero, currencyID = chargeDeduction.OtherChargeDeductionAmount?.Currency ?? ZString.Empty },
				}).ToArray() ?? Array.Empty<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCustomsValuationChargeDeduction>()
			} : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCustomsValuation();
		}

		protected virtual void PopulatePreviousDocuments(IGovernmentAgencyGoodsItem goodsItem)
		{
			var giPreviousDocuments = goodsItem.PreviousDocuments.OrderBy(x => x.SystemCreateTime).ThenBy(x => x.TypeCode).ThenBy(x => x.CategoryCode).ThenBy(x => x.ID);
			var previousDocuments = new List<DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocument>();
			if (giPreviousDocuments != null)
			{
				foreach (var previousDocument in giPreviousDocuments)
				{
					previousDocuments.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocument
					{
						CategoryCode = new PreviousDocumentCategoryCodeType { Value = previousDocument.CategoryCode },
						ID = new PreviousDocumentIdentificationIDType { Value = previousDocument.ID },
						LineNumeric = previousDocument.LineNumeric,
						LineNumericSpecified = previousDocument.LineNumeric > 0,
						TypeCode = new PreviousDocumentTypeCodeType { Value = previousDocument.TypeCode }
					});
				}
			}

			decGoodsItem.PreviousDocument = previousDocuments.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected virtual void PopulateCommodity(IGovernmentAgencyGoodsItem goodsItem)
		{
			var giCommodity = goodsItem.Commodity;
			DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity;
			if (giCommodity == null)
			{
				commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			}
			else
			{
				var index = 1;
				commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
				{
					Description = new CommodityDescriptionTextType { Value = giCommodity.Description },
					TransportEquipment = giCommodity.TransportEquipmentIDs?.Select(t => new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTransportEquipment
					{
						ID = new TransportEquipmentIdentificationIDType { Value = t },
						SequenceNumeric = index++
					}).ToArray() ?? Array.Empty<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTransportEquipment>()
				};
				PopulateDutyTaxFee(commodity, giCommodity);
				PopulateDangerousGoods(commodity, giCommodity);
				PopulateClassification(commodity, giCommodity);
				PopulateGoodsMeasure(commodity, giCommodity);
				PopulateInvoiceLineItemChargeAmount(commodity, giCommodity);
			}

			decGoodsItem.Commodity = commodity;
		}

		protected virtual void PopulateDangerousGoods(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
		}

		protected virtual void PopulateDutyTaxFee(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
			commodity.DutyTaxFee = giCommodity.DutyTaxFees?.Select(GetDutyTaxFee).ToArray()
									?? Array.Empty<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee>();
		}

		protected virtual void PopulateClassification(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
			commodity.Classification = giCommodity.Classifications?.Select(classification => new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification
			{
				ID = new ClassificationIdentificationIDType { Value = classification.ID },
				IdentificationTypeCode = new ClassificationIdentificationTypeCodeType { Value = classification.TypeCode }
			}).ToArray() ?? Array.Empty<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification>();
		}

		protected virtual void PopulateGoodsMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
			commodity.GoodsMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure
			{
				TariffQuantity = new GoodsMeasureTariffQuantityType { Value = giCommodity.TariffQuantity }
			};
			PopulateGrossMassMeasure(commodity.GoodsMeasure, giCommodity);
			SetItemGrossMassUnit(commodity.GoodsMeasure);
			PopulateNetNetWeightMeasure(commodity.GoodsMeasure, giCommodity);
		}

		protected virtual void SetItemGrossMassUnit(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure goodsMeasure)
		{
		}

		protected virtual void PopulateGrossMassMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure goodsMeasure, ICommodity giCommodity)
		{
			goodsMeasure.GrossMassMeasure = new GoodsMeasureGrossMassMeasureType { Value = giCommodity.GrossWeight };
		}

		protected virtual void PopulateNetNetWeightMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure goodsMeasure, ICommodity giCommodity)
		{
			goodsMeasure.NetNetWeightMeasure = new GoodsMeasureNetNetWeightMeasureType { Value = giCommodity.NetWeight };
		}

		protected virtual void PopulateInvoiceLineItemChargeAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
			commodity.InvoiceLine = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine
			{
				ItemChargeAmount = new InvoiceLineItemChargeAmountType
				{
					Value = giCommodity.InvoiceLineItemCharge.Amount,
					currencyID = giCommodity.InvoiceLineItemCharge.Currency
				}
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee GetDutyTaxFee(IDutyTaxFee dutyTaxFee)
		{
			var result = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee();

			PopulateDutyRegimeCode(result, dutyTaxFee);

			PopulatePayment(result, dutyTaxFee);

			PopulateQuotaOrderID(result, dutyTaxFee);

			PopulateSpecificTaxBaseQuantity(result, dutyTaxFee);

			PopulateTaxTypeCode(result, dutyTaxFee);

			if (dutyTaxFee.OverrideCode == TaxOverrideReasonCodes.Override)
			{
				PopulatePaymentAmount(result, dutyTaxFee);

				PopulateTaxAssessedAmount(result, dutyTaxFee);
			}
			return result;
		}

		protected virtual void PopulateDutyRegimeCode(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
			result.DutyRegimeCode = new DutyTaxFeeDutyRegimeCodeType
			{
				Value = dutyTaxFee.DutyRegimeCode
			};
		}

		protected virtual void PopulateQuotaOrderID(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
			result.QuotaOrderID = new DutyTaxFeeQuotaOrderIDType
			{
				Value = dutyTaxFee.QuotaOrderID
			};
		}

		protected virtual void PopulatePayment(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
			result.Payment = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeePayment
			{
				MethodCode = new PaymentMethodCodeType { Value = dutyTaxFee.Payment?.MethodCode }
			};
		}

		protected virtual void PopulateTaxAssessedAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
			var taxAssessedAmount = dutyTaxFee.Payment?.TaxAssessedAmount?.Amount ?? ZDecimal.Zero;
			result.Payment.TaxAssessedAmount = new PaymentTaxAssessedAmountType
			{
				Value = GetAmountWithTrailingZeros(taxAssessedAmount),
				currencyID = GetCurrencyCode(dutyTaxFee.Payment?.TaxAssessedAmount?.Currency)
			};
		}

		protected virtual void PopulatePaymentAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
			var paymentAmount = dutyTaxFee.Payment?.PaymentAmount?.Amount ?? ZDecimal.Zero;
			result.Payment.PaymentAmount = new PaymentPaymentAmountType
			{
				Value = GetAmountWithTrailingZeros(paymentAmount),
				currencyID = GetCurrencyCode(dutyTaxFee.Payment?.PaymentAmount?.Currency)
			};
		}

		static decimal GetAmountWithTrailingZeros(ZDecimal amount)
		{
			return decimal.Parse(amount.ToString("F" + noOfDecimalPlaces));
		}

		static ZString GetCurrencyCode(string currency)
		{
			return currency ?? gbpCurrencyCode;
		}

		const int noOfDecimalPlaces = 2;
		const string gbpCurrencyCode = Core.Constants.CurrencyCodes.UnitedKingdom;

		protected virtual void PopulateSpecificTaxBaseQuantity(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
			var specificTaxBaseQuantityValue = dutyTaxFee.SpecificTaxBaseQuantity?.MeasureValue ?? ZDecimal.Zero;
			if (!specificTaxBaseQuantityValue.IsEmpty)
			{
				result.SpecificTaxBaseQuantity = new DutyTaxFeeSpecificTaxBaseQuantityType
				{
					Value = specificTaxBaseQuantityValue,
					unitCode = dutyTaxFee.SpecificTaxBaseQuantity?.MeasureUQ ?? ZString.Empty
				};
			}
		}

		protected virtual void PopulateTaxTypeCode(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
			var taxTypeCodeValue = dutyTaxFee.OutputTypeCode ? dutyTaxFee.TypeCode : ZString.Empty;
			if (!taxTypeCodeValue.IsEmpty)
			{
				result.TypeCode = new DutyTaxFeeTypeCodeType
				{
					Value = taxTypeCodeValue
				};
			}
		}

		#endregion
	}
}
