using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static CargoWise.Customs.DE.MessageContracts.MessageSchema.AESMessageSchema;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public sealed class EXPDATMessageBuilder : MessageBuilder<DEXPDF>
	{
		readonly IAESMessageHeader dataProvider;
		readonly IEXPDATHeader headerProvider;

		public EXPDATMessageBuilder(IAESMessageHeader dataProvider)
		{
			this.dataProvider = Argument.NotNull(dataProvider, nameof(dataProvider));
			headerProvider = (IEXPDATHeader)dataProvider.AESHeader;
		}

		protected override DEXPDF GetMessageCore()
		{
			return AESMessageBuilderHelper.CreateMessage<DEXPDF, DEXPDFMessageSender, DEXPDFMessageRecipient>(dataProvider, "F.1.10", m =>
			{
				m.messageGroup = DEXPDFMessageGroup.EXP;
				m.messageType = DEXPDFMessageType.DEXPDF;
				m.ExportOperation = PopulateExportOperation();
				m.Authorisation = headerProvider.Authorisations.Select(PopulateAuthorisation).ToArray();
				m.CustomsOfficeOfPresentation = new DEXPDFCustomsOfficeOfPresentation { referenceNumber = headerProvider.CustomsOfficeOfPresentation.LeftOrNull(CustomsOfficeReferenceNumberMaxLength) };
				m.CustomsOfficeOfExport = new DEXPDFCustomsOfficeOfExport { referenceNumber = headerProvider.ExportCustomsOffice.LeftOrNull(CustomsOfficeReferenceNumberMaxLength) };
				m.CustomsOfficeOfSupplement = new DEXPDFCustomsOfficeOfSupplement { referenceNumber = headerProvider.SupplementaryDeclarationCustomsOffice.LeftOrNull(CustomsOfficeReferenceNumberMaxLength) };
				m.CustomsOfficeOfExitDeclared = new DEXPDFCustomsOfficeOfExitDeclared { referenceNumber = headerProvider.IntendedExitCustomsOffice.Left(CustomsOfficeReferenceNumberMaxLength) };
				m.CustomsOfficeOfExitActual = new DEXPDFCustomsOfficeOfExitActual { referenceNumber = headerProvider.ActualExitCustomsOffice.Left(CustomsOfficeReferenceNumberMaxLength) };
				m.ContractualPartner = PopulateContractualPartner();
				m.Exporter = PopulateExporter();
				m.Declarant = PopulateDeclarant();
				m.Representative = PopulateRepresentative();
				m.SubContractor = PopulateSubContractor();
				m.GoodsShipment = PopulateGoodsShipment();
			});

			DEXPDFExportOperation PopulateExportOperation()
			{
				var invoiceAmountAndCurrencySpecified = headerProvider.InvoiceAmountAndCurrencySpecified;
				return new DEXPDFExportOperation
				{
					LRN = headerProvider.LocalReferenceNumber.LeftOrNull(LocalReferenceNumberMaxLength),
					declarationType = headerProvider.DeclarationType.Left(DeclarationTypeMaxLength),
					exportDeclarationType = headerProvider.ExportDeclarationType.MapCodeToEnumWithDefaultAndItemPrefix<DEXPDFExportOperationExportDeclarationType>(),
					partyConstellation = headerProvider.PartyConstellation.ToString().MapCodeToEnumWithDefaultAndItemPrefix<DEXPDFExportOperationPartyConstellation>(),
					declarationSubmissionDateAndTime = headerProvider.SubmissionDateAndTimeUtc.DateAndTime.ToUnspecified(),
					decisiveDate = headerProvider.DecisiveDate.SafeDate(),
					decisiveDateSpecified = headerProvider.DecisiveDate.IsValid,
					exitDate = headerProvider.ExitDate.SafeDate(),
					exitDateSpecified = headerProvider.ExitDate.IsValid,
					presentationStartDateAndTime = headerProvider.PresentationStartDateAndTimeUtc.ToUnspecified(),
					presentationStartDateAndTimeSpecified = headerProvider.PresentationStartDateAndTimeUtc != default,
					loadingEndDateAndTime = headerProvider.LoadingEndDateAndTimeUtc.ToUnspecified(),
					loadingEndDateAndTimeSpecified = headerProvider.LoadingEndDateAndTimeUtc != default,
					security = headerProvider.Security.LeftOrNull(SecurityMaxLength),
					specificCircumstanceIndicator = headerProvider.SpecificCircumstanceIndicator.Left(SpecificCircumstanceIndicatorMaxLength3),
					totalAmountInvoiced = headerProvider.InvoiceAmount,
					totalAmountInvoicedSpecified = invoiceAmountAndCurrencySpecified,
					invoiceCurrency = invoiceAmountAndCurrencySpecified ? headerProvider.Currency.LeftOrNull(CurrencyMaxLength) : null
				};
			}

			DEXPDFAuthorisation PopulateAuthorisation(IAuthorisation authorisation, int index)
			{
				return new DEXPDFAuthorisation
				{
					sequenceNumber = (index + 1).ToString(),
					type = authorisation.Type.MapCodeToEnumWithDefault<DEXPDFAuthorisationType>(),
					referenceNumber = authorisation.ReferenceNumber.LeftOrNull(AuthorisationReferenceNumberMaxLength)
				};
			}

			DEXPDFContractualPartner PopulateContractualPartner()
			{
				DEXPDFContractualPartner result = null;
				var contractualPartner = headerProvider.ContractualPartner;
				if (contractualPartner != null)
				{
					var eoriNumber = contractualPartner.EoriNumber;
					var eoriNumberIsEmpty = eoriNumber.IsEmpty();
					var tcuNumber = contractualPartner.TCUNumber;
					var eoriNumberAndTCUNumberAreEmpty = eoriNumberIsEmpty && tcuNumber.IsEmpty();
					result = new DEXPDFContractualPartner
					{
						identificationNumber = eoriNumberIsEmpty ? tcuNumber.LeftOrNull(EoriCodeMaxLength) : eoriNumber.LeftOrNull(EoriCodeMaxLength),
						subsidiaryNumber = contractualPartner.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength),
						name = eoriNumberAndTCUNumberAreEmpty ? contractualPartner.Name.LeftOrNull(PartyNameMaxLength70) : null,
						Address = eoriNumberAndTCUNumberAreEmpty ? new DEXPDFContractualPartnerAddress
						{
							streetAndNumber = (contractualPartner.Address + contractualPartner.Address2).LeftOrNull(StreetAndNumberMaxLength),
							postcode = contractualPartner.Postcode.LeftOrNull(AddressPostcodeMaxLength),
							city = contractualPartner.City.LeftOrNull(AddressCityMaxLength),
							country = contractualPartner.Country.LeftOrNull(AddressCountryMaxLength),
						} : null
					};
				}
				return result;
			}

			DEXPDFExporter PopulateExporter()
			{
				DEXPDFExporter result = null;
				var exporter = headerProvider.Exporter;
				if (exporter != null)
				{
					var eoriNumber = exporter.EoriNumber;
					var eoriNumberIsEmpty = eoriNumber.IsEmpty();
					result = new DEXPDFExporter
					{
						identificationNumber = eoriNumber.LeftOrNull(EoriCodeMaxLength),
						subsidiaryNumber = exporter.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength),
						name = eoriNumberIsEmpty ? exporter.Name.LeftOrNull(PartyNameMaxLength70) : null,
						Address = eoriNumberIsEmpty ? new DEXPDFExporterAddress
						{
							streetAndNumber = (exporter.Address + exporter.Address2).LeftOrNull(StreetAndNumberMaxLength),
							postcode = exporter.Postcode.LeftOrNull(AddressPostcodeMaxLength),
							city = exporter.City.LeftOrNull(AddressCityMaxLength),
							country = exporter.Country.LeftOrNull(AddressCountryMaxLength),
						} : null
					};
				}
				return result;
			}

			DEXPDFDeclarant PopulateDeclarant()
			{
				var result = new DEXPDFDeclarant { };
				var declarant = headerProvider.Declarant;
				if (declarant != null)
				{
					var eoriNumber = declarant.EoriNumber;
					var eoriNumberIsEmpty = eoriNumber.IsEmpty();
					var contactPerson = declarant.ContactPerson;
					result = new DEXPDFDeclarant
					{
						identificationNumber = eoriNumber.LeftOrNull(EoriCodeMaxLength),
						subsidiaryNumber = declarant.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength),
						name = eoriNumberIsEmpty ? declarant.Name.LeftOrNull(PartyNameMaxLength70) : null,
						Address = eoriNumberIsEmpty ? new DEXPDFDeclarantAddress
						{
							streetAndNumber = (declarant.Address + declarant.Address2).LeftOrNull(StreetAndNumberMaxLength),
							postcode = declarant.Postcode.LeftOrNull(AddressPostcodeMaxLength),
							city = declarant.City.LeftOrNull(AddressCityMaxLength),
							country = declarant.Country.LeftOrNull(AddressCountryMaxLength),
						} : null,
						ContactPerson = contactPerson != null ? new DEXPDFDeclarantContactPerson
						{
							name = contactPerson.PersonName.LeftOrNull(ContactNameMaxLength70),
							phoneNumber = contactPerson.PhoneNumber.LeftOrNull(ContactPhoneNumberMaxLength),
							eMailAddress = contactPerson.MailAddress.LeftOrNull(ContactMailAddressMaxLength)
						} : null
					};
				}
				return result;
			}

			DEXPDFRepresentative PopulateRepresentative()
			{
				DEXPDFRepresentative result = null;
				var representative = headerProvider.Representative;
				if (representative != null)
				{
					var contactPerson = representative.ContactPerson;
					result = new DEXPDFRepresentative
					{
						identificationNumber = representative.EoriNumber.LeftOrNull(EoriCodeMaxLength),
						subsidiaryNumber = representative.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength),
						ContactPerson = contactPerson == null ? new DEXPDFRepresentativeContactPerson() : new DEXPDFRepresentativeContactPerson
						{
							name = contactPerson.PersonName.LeftOrNull(ContactNameMaxLength70),
							phoneNumber = contactPerson.PhoneNumber.LeftOrNull(ContactPhoneNumberMaxLength),
							eMailAddress = contactPerson.MailAddress.LeftOrNull(ContactMailAddressMaxLength)
						}
					};
				}
				return result;
			}

			DEXPDFSubContractor PopulateSubContractor()
			{
				DEXPDFSubContractor result = null;
				var subContractor = headerProvider.SubContractor;
				if (subContractor != null)
				{
					var eoriNumber = subContractor.EoriNumber;
					var eoriNumberIsEmpty = eoriNumber.IsEmpty();
					result = new DEXPDFSubContractor
					{
						identificationNumber = subContractor.EoriNumber.LeftOrNull(EoriCodeMaxLength),
						subsidiaryNumber = subContractor.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength),
						name = eoriNumberIsEmpty ? subContractor.Name.LeftOrNull(PartyNameMaxLength70) : null,
						Address = eoriNumberIsEmpty ? new DEXPDFSubContractorAddress
						{
							streetAndNumber = (subContractor.Address + subContractor.Address2).LeftOrNull(StreetAndNumberMaxLength),
							postcode = subContractor.Postcode.LeftOrNull(AddressPostcodeMaxLength),
							city = subContractor.City.LeftOrNull(AddressCityMaxLength),
							country = subContractor.Country.LeftOrNull(AddressCountryMaxLength),
						} : null
					};
				}
				return result;
			}

			DEXPDFGoodsShipment PopulateGoodsShipment()
			{
				return new DEXPDFGoodsShipment
				{
					natureOfTransaction = headerProvider.TransactionType.LeftOrNull(TransactionTypeMaxLength),
					countryOfExport = headerProvider.ExportCountry.Left(CountryCodeMaxLength),
					countryOfDestination = headerProvider.DestinationCountry.Left(CountryCodeMaxLength),
					AdditionalSupplyChainActor = headerProvider.AdditionalSupplyChainActors.Select(PopulateAdditionalSupplyChainActor).ToArray(),
					DeliveryTerms = PopulateDeliveryTerms(),
					OutwardProcessing = PopulateOutwardProcessing(),
					PreviousDocument = headerProvider.PreviousDocuments.Select(PopulatePreviousDocument).ToArray(),
					SupportingDocument = headerProvider.SupportingDocuments.Select(PopulateSupportingDocument).ToArray(),
					AdditionalReference = headerProvider.AdditionalReferences.Select(PopulateAdditionalReference).ToArray(),
					AdditionalInformation = headerProvider.AdditionalInformations.Select(PopulateAdditionalInformation).ToArray(),
					Consignment = PopulateConsignment(),
					GoodsItem = headerProvider.Lines.Select(PopulateGoodsItem).ToArray(),
				};

				DEXPDFGoodsShipmentAdditionalSupplyChainActor PopulateAdditionalSupplyChainActor(ISupplyChainActor supplyChainActor, int index)
				{
					return new DEXPDFGoodsShipmentAdditionalSupplyChainActor
					{
						sequenceNumber = (index + 1).ToString(),
						role = supplyChainActor.Role.LeftOrNull(SupplyChainActorRoleMaxLength),
						identificationNumber = supplyChainActor.IdentificationNumber.LeftOrNull(SupplyChainActorIdentificationNumberMaxLength)
					};
				}

				DEXPDFGoodsShipmentDeliveryTerms PopulateDeliveryTerms() => new DEXPDFGoodsShipmentDeliveryTerms()
				{
					incotermCode = headerProvider.DeliveryTerms?.IncotermCode.LeftOrNull(IncotermCodeMaxLength),
					UNLocode = headerProvider.DeliveryTerms?.UNLocode.LeftOrNull(UNLocodeMaxLength),
					location = headerProvider.DeliveryTerms?.Location.LeftOrNull(DeliveryTermsLocationMaxLength),
					country = headerProvider.DeliveryTerms?.Country.LeftOrNull(CountryCodeMaxLength),
					text = headerProvider.DeliveryTerms?.Text.LeftOrNull(AdditionalInformationTextMaxLength)
				};

				DEXPDFGoodsShipmentOutwardProcessing PopulateOutwardProcessing()
				{
					DEXPDFGoodsShipmentOutwardProcessing result = null;
					var outwardProcessing = headerProvider.OutwardProcessing;
					if (outwardProcessing != null)
					{
						result = new DEXPDFGoodsShipmentOutwardProcessing
						{
							Reimport = outwardProcessing.ReimportCountries.Select(PopulateReimport).ToArray(),
							IdentificationMeans = outwardProcessing.IdentificationMeans.Select(PopulateIdentificationMeans).ToArray(),
							Product = outwardProcessing.Products.Select(PopulateProduct).ToArray(),
						};
					}
					return result;

					DEXPDFGoodsShipmentOutwardProcessingReimport PopulateReimport(string reimportCountry, int index)
					{
						return new DEXPDFGoodsShipmentOutwardProcessingReimport
						{
							sequenceNumber = (index + 1).ToString(),
							country = reimportCountry.LeftOrNull(CountryCodeMaxLength),
						};
					}

					DEXPDFGoodsShipmentOutwardProcessingIdentificationMeans PopulateIdentificationMeans(IIdentificationMeans identificationMeans, int index)
					{
						return new DEXPDFGoodsShipmentOutwardProcessingIdentificationMeans
						{
							sequenceNumber = (index + 1).ToString(),
							type = identificationMeans.Type.MapCodeToEnumWithDefault<DEXPDFGoodsShipmentOutwardProcessingIdentificationMeansType>(),
							description = identificationMeans.Description.LeftOrNull(IdentificationMeansDescriptionMaxLength)
						};
					}

					DEXPDFGoodsShipmentOutwardProcessingProduct PopulateProduct(IProduct product, int index)
					{
						return new DEXPDFGoodsShipmentOutwardProcessingProduct
						{
							sequenceNumber = (index + 1).ToString(),
							Commodity = new DEXPDFGoodsShipmentOutwardProcessingProductCommodity
							{
								descriptionOfGoods = product.GoodsDescription.LeftOrNull(GoodsDescriptionMaxLength512),
								CommodityCode = new DEXPDFGoodsShipmentOutwardProcessingProductCommodityCommodityCode
								{
									harmonizedSystemSubHeadingCode = product.HarmonizedSystemSubHeadingCode.LeftOrNull(HarmonizedSystemSubHeadingCodeMaxLength),
									combinedNomenclatureCode = product.CombinedNomenclatureCode.LeftOrNull(CombinedNomenclatureCodeMaxLength2),
								}
							}
						};
					}
				}

				DEXPDFGoodsShipmentPreviousDocument PopulatePreviousDocument(IPreviousDocument previousDocument, int index)
				{
					return new DEXPDFGoodsShipmentPreviousDocument
					{
						sequenceNumber = (index + 1).ToString(),
						type = previousDocument.Type.LeftOrNull(DocumentTypeMaxLength),
						qualifier = previousDocument.Qualifier.LeftOrNull(DocumentQualifierMaxLength),
						referenceNumber = previousDocument.ReferenceNumber.LeftOrNull(DocumentReferenceNumberMaxLength),
					};
				}

				DEXPDFGoodsShipmentSupportingDocument PopulateSupportingDocument(ISupportingDocument supportingDocument, int index)
				{
					var documentLineItemNumber = supportingDocument.DocumentLineItemNumber;
					return new DEXPDFGoodsShipmentSupportingDocument
					{
						sequenceNumber = (index + 1).ToString(),
						type = supportingDocument.Type.LeftOrNull(DocumentTypeMaxLength),
						qualifier = supportingDocument.Qualifier.LeftOrNull(DocumentQualifierMaxLength),
						referenceNumber = supportingDocument.ReferenceNumber.LeftOrNull(DocumentReferenceNumberMaxLength),
						documentLineItemNumber = documentLineItemNumber > 0 ? documentLineItemNumber.ToString() : null,
						issuingAuthorityName = supportingDocument.IssuingAuthorityName.LeftOrNull(DocumentAdditionalDescriptionMaxLength),
						issuingDate = supportingDocument.IssuingDate.GetValueOrDefault(),
						issuingDateSpecified = supportingDocument.IssuingDate.HasValue,
						validityDate = supportingDocument.ValidityDate.GetValueOrDefault(),
						validityDateSpecified = supportingDocument.ValidityDate.HasValue,
					};
				}

				DEXPDFGoodsShipmentAdditionalReference PopulateAdditionalReference(IReference reference, int index)
				{
					return new DEXPDFGoodsShipmentAdditionalReference
					{
						sequenceNumber = (index + 1).ToString(),
						type = reference.Type.LeftOrNull(DocumentTypeMaxLength),
						qualifier = reference.Qualifier.LeftOrNull(DocumentQualifierMaxLength),
						referenceNumber = reference.ReferenceNumber.LeftOrNull(DocumentReferenceNumberMaxLength),
					};
				}

				DEXPDFGoodsShipmentAdditionalInformation PopulateAdditionalInformation(IReference reference, int index)
				{
					return new DEXPDFGoodsShipmentAdditionalInformation
					{
						sequenceNumber = (index + 1).ToString(),
						code = reference.FullType.LeftOrNull(AdditionalInformationCodeMaxLength),
						text = reference.Complement.LeftOrNull(AdditionalInformationTextMaxLength),
					};
				}

				DEXPDFGoodsShipmentConsignment PopulateConsignment()
				{
					return new DEXPDFGoodsShipmentConsignment
					{
						containerIndicator = headerProvider.IsContainerized ? DEXPDFGoodsShipmentConsignmentContainerIndicator.Item1 : DEXPDFGoodsShipmentConsignmentContainerIndicator.Item0,
						containerIndicatorSpecified = headerProvider.ContainerIndicatorSpecified,
						inlandModeOfTransport = headerProvider.InlandTransportMeansMode.LeftOrNull(TransportMeansModeMaxLength),
						modeOfTransportAtTheBorder = headerProvider.BorderTransportMeansMode.LeftOrNull(TransportMeansModeMaxLength),
						grossMass = headerProvider.TotalGrossMass,
						referenceNumberUCR = headerProvider.CommercialReferenceNumber.LeftOrNull(ReferenceNumberMaxLength),
						registrationNumberExternal = headerProvider.RegistrationNumber.Left(RegistrationNumberMaxLength),
						Carrier = PopulateCarrier(),
						Consignor = PopulateConsignor(),
						Consignee = PopulateConsignee(),
						TransportEquipment = headerProvider.TransportEquipments.Select(PopulateTransportEquipment).ToArray(),
						LocationOfGoods = PopulateLocationOfGoods(),
						DepartureTransportMeans = headerProvider.DepartureTransportMeans.Select(PopulateDepartureTransportMeans).ToArray(),
						CountryOfRoutingOfConsignment = headerProvider.ItineraryCountries.Select(PopulateCountryOfRoutingOfConsignment).ToArray(),
						ActiveBorderTransportMeans = headerProvider.ActiveBorderTransportMeansSpecified ? PopulateActiveBorderTransportMeans() : null,
						TransportDocument = headerProvider.TransportDocuments.Select(PopulateTransportDocument).ToArray(),
						TransportCharges = new DEXPDFGoodsShipmentConsignmentTransportCharges { methodOfPayment = headerProvider.TransportChargesPaymentMethod.Left(TransportChargesPaymentMethodMaxLength) }
					};

					DEXPDFGoodsShipmentConsignmentCarrier PopulateCarrier()
					{
						DEXPDFGoodsShipmentConsignmentCarrier result = null;
						var carrier = headerProvider.Carrier;
						if (carrier != null)
						{
							var eoriNumber = carrier.EoriNumber;
							result = new DEXPDFGoodsShipmentConsignmentCarrier
							{
								identificationNumber = eoriNumber.IsEmpty() ? carrier.TCUNumber.LeftOrNull(EoriCodeMaxLength) : eoriNumber.LeftOrNull(EoriCodeMaxLength),
								subsidiaryNumber = carrier.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength),
							};
						}
						return result;
					}

					DEXPDFGoodsShipmentConsignmentConsignor PopulateConsignor()
					{
						DEXPDFGoodsShipmentConsignmentConsignor result = null;
						var consignor = headerProvider.Consignor;
						if (consignor != null)
						{
							var eoriNumber = consignor.EoriNumber;
							var eoriNumberIsEmpty = eoriNumber.IsEmpty();
							var tcuNumber = consignor.TCUNumber;
							var eoriNumberAndTCUNumberAreEmpty = eoriNumberIsEmpty && tcuNumber.IsEmpty();
							result = new DEXPDFGoodsShipmentConsignmentConsignor
							{
								identificationNumber = eoriNumberIsEmpty ? tcuNumber.LeftOrNull(EoriCodeMaxLength) : eoriNumber.LeftOrNull(EoriCodeMaxLength),
								subsidiaryNumber = consignor.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength),
								name = eoriNumberAndTCUNumberAreEmpty ? consignor.Name.LeftOrNull(PartyNameMaxLength70) : null,
								Address = eoriNumberAndTCUNumberAreEmpty ? new DEXPDFGoodsShipmentConsignmentConsignorAddress
								{
									streetAndNumber = (consignor.Address + consignor.Address2).LeftOrNull(StreetAndNumberMaxLength),
									postcode = consignor.Postcode.LeftOrNull(AddressPostcodeMaxLength),
									city = consignor.City.LeftOrNull(AddressCityMaxLength),
									country = consignor.Country.LeftOrNull(AddressCountryMaxLength),
								} : null
							};
						}
						return result;
					}

					DEXPDFGoodsShipmentConsignmentConsignee PopulateConsignee()
					{
						DEXPDFGoodsShipmentConsignmentConsignee result = null;
						var consignee = headerProvider.Consignee;
						if (consignee != null)
						{
							var eoriNumber = consignee.EoriNumber;
							var tcuNumber = consignee.TCUNumber;
							var eoriNumberAndTCUNumberAreEmpty = eoriNumber.IsEmpty() && tcuNumber.IsEmpty();
							result = new DEXPDFGoodsShipmentConsignmentConsignee
							{
								identificationNumber = eoriNumber.IsEmpty() ? tcuNumber.LeftOrNull(EoriCodeMaxLength) : eoriNumber.LeftOrNull(EoriCodeMaxLength),
								subsidiaryNumber = consignee.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength),
								name = eoriNumberAndTCUNumberAreEmpty ? consignee.Name.LeftOrNull(PartyNameMaxLength70) : null,
								Address = eoriNumberAndTCUNumberAreEmpty ? new DEXPDFGoodsShipmentConsignmentConsigneeAddress
								{
									streetAndNumber = (consignee.Address + consignee.Address2).LeftOrNull(StreetAndNumberMaxLength),
									postcode = consignee.Postcode.LeftOrNull(AddressPostcodeMaxLength),
									city = consignee.City.LeftOrNull(AddressCityMaxLength),
									country = consignee.Country.LeftOrNull(AddressCountryMaxLength),
								} : null
							};
						}
						return result;
					}

					DEXPDFGoodsShipmentConsignmentTransportEquipment PopulateTransportEquipment(ITransportEquipment transportEquipment, int index)
					{
						return new DEXPDFGoodsShipmentConsignmentTransportEquipment
						{
							sequenceNumber = (index + 1).ToString(),
							containerIdentificationNumber = transportEquipment.ContainerIdentificationNumber.LeftOrNull(ContainerIdentificationNumberMaxLength),
							numberOfSeals = "0",
							Seal = transportEquipment.SealIdentifiers.Select(PopulateSeal).ToArray(),
							GoodsReference = transportEquipment.DeclarationGoodsItemNumbers.Select(PopulateGoodsReference).ToArray(),
						};

						DEXPDFGoodsShipmentConsignmentTransportEquipmentSeal PopulateSeal(string identifier, int sealIndex)
						{
							return new DEXPDFGoodsShipmentConsignmentTransportEquipmentSeal
							{
								sequenceNumber = (sealIndex + 1).ToString(),
								identifier = identifier.LeftOrNull(SealIdentityMaxLength)
							};
						}

						DEXPDFGoodsShipmentConsignmentTransportEquipmentGoodsReference PopulateGoodsReference(int goodsItemNumber, int goodsReferenceIndex)
						{
							return new DEXPDFGoodsShipmentConsignmentTransportEquipmentGoodsReference
							{
								sequenceNumber = (goodsReferenceIndex + 1).ToString(),
								declarationGoodsItemNumber = goodsItemNumber.ToString()
							};
						}
					}

					DEXPDFGoodsShipmentConsignmentLocationOfGoods PopulateLocationOfGoods()
					{
						DEXPDFGoodsShipmentConsignmentLocationOfGoods result = null;
						if (headerProvider.LocationOfGoodsSpecified)
						{
							var locationOfGoodsParty = headerProvider.LocationOfGoodsParty;
							var locationOfGoodsContactPerson = headerProvider.LocationOfGoodsContactPerson;
							result = new DEXPDFGoodsShipmentConsignmentLocationOfGoods
							{
								typeOfLocation = headerProvider.TypeOfLocation.MapCodeToEnumWithDefault<DEXPDFGoodsShipmentConsignmentLocationOfGoodsTypeOfLocation>(),
								qualifierOfIdentification = headerProvider.QualifierOfIdentification.MapCodeToEnumWithDefault<DEXPDFGoodsShipmentConsignmentLocationOfGoodsQualifierOfIdentification>(),
								authorisationNumber = headerProvider.AuthorisationNumber.LeftOrNull(AuthorisationNumberMaxLength35),
								additionalIdentifier = headerProvider.AdditionalIdentifier.LeftOrNull(AdditionalIdentifierMaxLength),
								UNLocode = headerProvider.UNLocode.LeftOrNull(UNLocodeMaxLength),
								GNSS = headerProvider.GNSSSpecified ? new DEXPDFGoodsShipmentConsignmentLocationOfGoodsGNSS
								{
									latitude = headerProvider.GNSSLatitude.ToString(),
									longitude = headerProvider.GNSSLongitude.ToString()
								} : null,
								Address = locationOfGoodsParty != null ? new DEXPDFGoodsShipmentConsignmentLocationOfGoodsAddress
								{
									complementOfInformation = locationOfGoodsParty.AdditionalAddressInformation.LeftOrNull(AddressAdditionalAddressInfoMaxLength),
									streetAndNumber = (locationOfGoodsParty.Address + locationOfGoodsParty.Address2).LeftOrNull(StreetAndNumberMaxLength),
									postcode = locationOfGoodsParty.Postcode.LeftOrNull(AddressPostcodeMaxLength),
									city = locationOfGoodsParty.City.LeftOrNull(AddressCityMaxLength),
									country = locationOfGoodsParty.Country.LeftOrNull(CountryCodeMaxLength),
								} : null,
								ContactPerson = locationOfGoodsContactPerson != null ? new DEXPDFGoodsShipmentConsignmentLocationOfGoodsContactPerson
								{
									name = locationOfGoodsContactPerson.PersonName.LeftOrNull(ContactNameMaxLength70),
									phoneNumber = locationOfGoodsContactPerson.PhoneNumber.LeftOrNull(ContactPhoneNumberMaxLength),
									eMailAddress = locationOfGoodsContactPerson.MailAddress.LeftOrNull(ContactMailAddressMaxLength)
								} : null,
							};
						}
						return result;
					}

					DEXPDFGoodsShipmentConsignmentDepartureTransportMeans PopulateDepartureTransportMeans(IDepartureTransportMeans departureTransportMeans, int index)
					{
						return new DEXPDFGoodsShipmentConsignmentDepartureTransportMeans
						{
							sequenceNumber = (index + 1).ToString(),
							typeOfIdentification = departureTransportMeans.TypeOfIdentification.LeftOrNull(TransportMeansTypeMaxLength),
							identificationNumber = departureTransportMeans.IdentificationNumber.LeftOrNull(TransportMeansIdentityMaxLength),
							nationality = departureTransportMeans.Nationality.LeftOrNull(TransportMeansNationalityMaxLength),
						};
					}

					DEXPDFGoodsShipmentConsignmentCountryOfRoutingOfConsignment PopulateCountryOfRoutingOfConsignment(ZString countryOfRouting, int index)
					{
						return new DEXPDFGoodsShipmentConsignmentCountryOfRoutingOfConsignment
						{
							sequenceNumber = (index + 1).ToString(),
							country = countryOfRouting.Left(CountryCodeMaxLength)
						};
					}

					DEXPDFGoodsShipmentConsignmentActiveBorderTransportMeans PopulateActiveBorderTransportMeans()
					{
						return new DEXPDFGoodsShipmentConsignmentActiveBorderTransportMeans
						{
							typeOfIdentification = headerProvider.BorderTransportMeansType.LeftOrNull(TransportMeansTypeMaxLength),
							identificationNumber = headerProvider.BorderTransportMeansIdentity.LeftOrNull(TransportMeansIdentityMaxLength),
							nationality = headerProvider.BorderTransportMeansNationality.LeftOrNull(TransportMeansNationalityMaxLength),
						};
					}

					DEXPDFGoodsShipmentConsignmentTransportDocument PopulateTransportDocument(IReference reference, int index)
					{
						return new DEXPDFGoodsShipmentConsignmentTransportDocument
						{
							sequenceNumber = (index + 1).ToString(),
							type = reference.Type.LeftOrNull(DocumentTypeMaxLength),
							qualifier = reference.Qualifier.LeftOrNull(DocumentQualifierMaxLength),
							referenceNumber = reference.ReferenceNumber.LeftOrNull(DocumentReferenceNumberMaxLength),
						};
					}
				}

				DEXPDFGoodsShipmentGoodsItem PopulateGoodsItem(IEXPDATLine line, int index)
				{
					return new DEXPDFGoodsShipmentGoodsItem
					{
						sequenceNumber = (index + 1).ToString(),
						declarationGoodsItemNumber = line.LineNumber.ToString(),
						statisticalValue = line.StatisticalValue,
						statisticalValueSpecified = line.StatisticalValueSpecified,
						natureOfTransaction = line.TransactionType.LeftOrNull(TransactionTypeMaxLength),
						countryOfExport = line.CountryOfExport.LeftOrNull(CountryCodeMaxLength),
						countryOfDestination = line.CountryOfDestination.LeftOrNull(CountryCodeMaxLength),
						referenceNumberUCR = line.CommercialReferenceNumber.Left(ReferenceNumberMaxLength),
						Authorisation = line.Authorisations.Select(PopulateItemAuthorisation).ToArray(),
						Procedure = PopulateItemProcedure(),
						Consignor = PopulateItemConsignor(),
						Consignee = PopulateItemConsignee(),
						AdditionalSupplyChainActor = line.AdditionalSupplyChainActors.Select(PopulateItemAdditionalSupplyChainActor).ToArray(),
						Origin = PopulateItemOrigin(),
						Commodity = PopulateItemCommodity(),
						Packaging = PopulatePackages(line.Packages),
						PreviousDocument = line.PreviousDocuments.Select(PopulateItemPreviousDocument).ToArray(),
						SupportingDocument = line.Documents.Select(PopulateItemSupportingDocument).ToArray(),
						AdditionalReference = line.AdditionalReferences.Select(PopulateItemAdditionalReference).ToArray(),
						AdditionalInformation = line.AdditionalInformations.Select(PopulateItemAdditionalInformation).ToArray(),
						TransportCharges = PopulateItemTransportCharges(),
						OutwardProcessing = PopulateItemOutwardProcessing(),
						ProcedureTransference = PopulateItemProcedureTransference(),
					};

					DEXPDFGoodsShipmentGoodsItemAuthorisation PopulateItemAuthorisation(IReference reference, int itemAuthorisationIndex)
					{
						return new DEXPDFGoodsShipmentGoodsItemAuthorisation
						{
							sequenceNumber = (itemAuthorisationIndex + 1).ToString(),
							type = reference.Type.MapCodeToEnumWithDefault<DEXPDFGoodsShipmentGoodsItemAuthorisationType>(),
							referenceNumber = reference.ReferenceNumber.LeftOrNull(AuthorisationReferenceNumberMaxLength),
							holderOfAuthorisation = reference.Type.In(nameof(DEXPDFGoodsShipmentGoodsItemAuthorisationType.C626), nameof(DEXPDFGoodsShipmentGoodsItemAuthorisationType.C627)) ? reference.Detail.ValueOrNullIfEmpty() : null
						};
					}

					DEXPDFGoodsShipmentGoodsItemProcedure PopulateItemProcedure()
					{
						var additionalProcedure = line.AdditionalProcedure;
						return new DEXPDFGoodsShipmentGoodsItemProcedure()
						{
							requestedProcedure = line.RequestedProcedure.Left(RequestedProcedureMaxLength),
							previousProcedure = line.PreviousProcedure.Left(PreviousProcedureMaxlength),
							AdditionalProcedure = !additionalProcedure.IsEmpty ? new DEXPDFGoodsShipmentGoodsItemProcedureAdditionalProcedure
							{
								sequenceNumber = "1",
								additionalProcedure = additionalProcedure.Left(AdditionalProcedureMaxLength)
							} : null
						};
					}

					DEXPDFGoodsShipmentGoodsItemConsignor PopulateItemConsignor()
					{
						DEXPDFGoodsShipmentGoodsItemConsignor result = null;
						var consignor = line.Consignor;
						if (consignor != null)
						{
							var eoriNumber = consignor.EoriNumber;
							var eoriNumberIsEmpty = eoriNumber.IsEmpty();
							var tcuNumber = consignor.TCUNumber;
							var eoriNumberAndTCUNumberAreEmpty = eoriNumberIsEmpty && tcuNumber.IsEmpty();
							result = new DEXPDFGoodsShipmentGoodsItemConsignor
							{
								identificationNumber = eoriNumberIsEmpty ? tcuNumber.LeftOrNull(EoriCodeMaxLength) : eoriNumber.LeftOrNull(EoriCodeMaxLength),
								subsidiaryNumber = consignor.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength),
								name = eoriNumberAndTCUNumberAreEmpty ? consignor.Name.LeftOrNull(PartyNameMaxLength70) : null,
								Address = eoriNumberAndTCUNumberAreEmpty ? new DEXPDFGoodsShipmentGoodsItemConsignorAddress
								{
									streetAndNumber = (consignor.Address + consignor.Address2).LeftOrNull(StreetAndNumberMaxLength),
									postcode = consignor.Postcode.LeftOrNull(AddressPostcodeMaxLength),
									city = consignor.City.LeftOrNull(AddressCityMaxLength),
									country = consignor.Country.LeftOrNull(AddressCountryMaxLength),
								} : null
							};
						}
						return result;
					}

					DEXPDFGoodsShipmentGoodsItemConsignee PopulateItemConsignee()
					{
						DEXPDFGoodsShipmentGoodsItemConsignee result = null;
						var consignee = line.Consignee;
						if (consignee != null)
						{
							var eoriNumber = consignee.EoriNumber;
							var tcuNumber = consignee.TCUNumber;
							var eoriNumberAndTCUNumberAreEmpty = eoriNumber.IsEmpty() && tcuNumber.IsEmpty();
							result = new DEXPDFGoodsShipmentGoodsItemConsignee
							{
								identificationNumber = eoriNumber.IsEmpty() ? tcuNumber.LeftOrNull(EoriCodeMaxLength) : eoriNumber.LeftOrNull(EoriCodeMaxLength),
								subsidiaryNumber = consignee.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength),
								name = eoriNumberAndTCUNumberAreEmpty ? consignee.Name.LeftOrNull(PartyNameMaxLength70) : null,
								Address = eoriNumberAndTCUNumberAreEmpty ? new DEXPDFGoodsShipmentGoodsItemConsigneeAddress
								{
									streetAndNumber = (consignee.Address + consignee.Address2).LeftOrNull(StreetAndNumberMaxLength),
									postcode = consignee.Postcode.LeftOrNull(AddressPostcodeMaxLength),
									city = consignee.City.LeftOrNull(AddressCityMaxLength),
									country = consignee.Country.LeftOrNull(AddressCountryMaxLength),
								} : null
							};
						}
						return result;
					}

					DEXPDFGoodsShipmentGoodsItemAdditionalSupplyChainActor PopulateItemAdditionalSupplyChainActor(ISupplyChainActor supplyChainActor, int itemSupplyChainActorIndex)
					{
						return new DEXPDFGoodsShipmentGoodsItemAdditionalSupplyChainActor
						{
							sequenceNumber = (itemSupplyChainActorIndex + 1).ToString(),
							role = supplyChainActor.Role.LeftOrNull(SupplyChainActorRoleMaxLength),
							identificationNumber = supplyChainActor.IdentificationNumber.LeftOrNull(SupplyChainActorIdentificationNumberMaxLength)
						};
					}

					DEXPDFGoodsShipmentGoodsItemOrigin PopulateItemOrigin()
					{
						return new DEXPDFGoodsShipmentGoodsItemOrigin
						{
							countryOfOrigin = line.CountryOfOrigin.LeftOrNull(CountryCodeMaxLength),
							regionOfDispatch = line.OriginFederalState.Left(RegionOfDispatchMaxLength),
						};
					}

					DEXPDFGoodsShipmentGoodsItemCommodity PopulateItemCommodity()
					{
						return new DEXPDFGoodsShipmentGoodsItemCommodity
						{
							descriptionOfGoods = line.GoodsDescription.LeftOrNull(GoodsDescriptionMaxLength),
							cusCode = line.CusCode.LeftOrNull(CusCodeMaxLength),
							CommodityCode = new DEXPDFGoodsShipmentGoodsItemCommodityCommodityCode
							{
								harmonizedSystemSubHeadingCode = line.HarmonizedSystemSubHeadingCode.LeftOrNull(HarmonizedSystemSubHeadingCodeMaxLength),
								combinedNomenclatureCode = line.CombinedNomenclatureCode.Left(CombinedNomenclatureCodeMaxLength2),
								TARICAdditionalCode = PopulateTARICAdditionalCode()
							},
							DangerousGoods = PopulateDangerousGoods(),
							GoodsMeasure = new DEXPDFGoodsShipmentGoodsItemCommodityGoodsMeasure
							{
								grossMass = line.GrossMass,
								netMass = line.NetMass,
								supplementaryUnits = line.SupplementaryQuantity,
								supplementaryUnitsSpecified = line.SupplementaryQuantity > 0
							},
						};

						DEXPDFGoodsShipmentGoodsItemCommodityCommodityCodeTARICAdditionalCode[] PopulateTARICAdditionalCode()
						{
							var list = new List<DEXPDFGoodsShipmentGoodsItemCommodityCommodityCodeTARICAdditionalCode>();
							if (!line.TaricFirstAdditionalCode.IsEmpty)
							{
								AddTaricAdditionalCodeToList(line.TaricFirstAdditionalCode);
							}

							if (!line.TaricSecondAdditionalCode.IsEmpty)
							{
								AddTaricAdditionalCodeToList(line.TaricSecondAdditionalCode);
							}

							if (line.TaricOtherAdditionalCodesSpecified)
							{
								foreach (var additionalSupplementaryCode in line.TaricOtherAdditionalCodes)
								{
									AddTaricAdditionalCodeToList(additionalSupplementaryCode);
								}
							}
							return list.ToArray();

							void AddTaricAdditionalCodeToList(ZString code)
							{
								list.Add(new DEXPDFGoodsShipmentGoodsItemCommodityCommodityCodeTARICAdditionalCode
								{
									sequenceNumber = (list.Count + 1).ToString(),
									taricAdditionalCode = code.LeftOrNull(TaricAdditionalCodeMaxLength),
								});
							}
						}

						DEXPDFGoodsShipmentGoodsItemCommodityDangerousGoods[] PopulateDangerousGoods()
						{
							var result = Array.Empty<DEXPDFGoodsShipmentGoodsItemCommodityDangerousGoods>();
							var dangerousGoodsCodes = line.DangerousGoodsCodes;
							if (dangerousGoodsCodes.Count > 0)
							{
								int i = 1;
								result = dangerousGoodsCodes.Select(obj => new DEXPDFGoodsShipmentGoodsItemCommodityDangerousGoods { sequenceNumber = i++.ToString(), UNNumber = obj.LeftOrNull(DangerousGoodsCodeMaxLength) }).ToArray();
							}
							return result;
						}
					}

					DEXPDFGoodsShipmentGoodsItemPackaging[] PopulatePackages(IReadOnlyCollection<IPackage> packages)
					{
						var result = new List<DEXPDFGoodsShipmentGoodsItemPackaging>();
						var sequenceNum = 1;

						foreach (var groupedPackages in packages.GroupBy(p => new { p.MarksNumbers, p.Kind }))
						{
							var firstPackage = groupedPackages.First();
							var quantity = groupedPackages.Sum(p => p.Quantity);

							result.Add(new DEXPDFGoodsShipmentGoodsItemPackaging
							{
								sequenceNumber = sequenceNum.ToString(),
								typeOfPackages = groupedPackages.Key.Kind.LeftOrNull(PackageKindMaxLength2),
								numberOfPackages = firstPackage.IsSupportEmptyPackType ? quantity.ToString() : null,
								shippingMarks = groupedPackages.Key.MarksNumbers.LeftOrNull(PackageMarksNumbersMaxLength),
								PackageReference = quantity == 0 ? new DEXPDFGoodsShipmentGoodsItemPackagingPackageReference { declarationGoodsItemNumber = firstPackage.PositionNumber.ToString() } : null
							});

							sequenceNum++;
						}

						return result.ToArray();
					}

					DEXPDFGoodsShipmentGoodsItemPreviousDocument PopulateItemPreviousDocument(IPreviousDocument previousDocument, int itemPreviousDocumentIndex)
					{
						return new DEXPDFGoodsShipmentGoodsItemPreviousDocument
						{
							sequenceNumber = (itemPreviousDocumentIndex + 1).ToString(),
							type = previousDocument.Type.LeftOrNull(DocumentTypeMaxLength),
							qualifier = previousDocument.Qualifier.LeftOrNull(DocumentQualifierMaxLength),
							referenceNumber = previousDocument.ReferenceNumber.LeftOrNull(DocumentReferenceNumberMaxLength),
							goodsItemNumber = previousDocument.GoodsItemNumber > 0 ? previousDocument.GoodsItemNumber.ToString() : null,
							measurementUnitAndQualifier = previousDocument.MeasurementUnitAndQualifier.LeftOrNull(MeasurementUnitAndQualifierMaxLength),
							quantity = previousDocument.Quantity,
							quantitySpecified = previousDocument.Quantity > 0 && !previousDocument.MeasurementUnitAndQualifier.IsEmpty(),
							complementOfInformation = previousDocument.Complement.LeftOrNull(DocumentComplementMaxLength),
						};
					}

					DEXPDFGoodsShipmentGoodsItemSupportingDocument PopulateItemSupportingDocument(ISupportingDocument supportingDocument, int itemSupportingDocumentIndex)
					{
						var complementaryUnitMap = supportingDocument.ComplementaryUnit.MapCodeToEnum<DEXPDFGoodsShipmentGoodsItemSupportingDocumentComplementaryUnit>(ignoreCase: true);
						var currency = supportingDocument.Currency;
						var documentLineItemNumber = supportingDocument.DocumentLineItemNumber;
						return new DEXPDFGoodsShipmentGoodsItemSupportingDocument
						{
							sequenceNumber = (itemSupportingDocumentIndex + 1).ToString(),
							type = supportingDocument.Type.LeftOrNull(DocumentTypeMaxLength),
							qualifier = supportingDocument.Qualifier.LeftOrNull(DocumentQualifierMaxLength),
							referenceNumber = supportingDocument.ReferenceNumber.LeftOrNull(DocumentReferenceNumberMaxLength),
							documentLineItemNumber = documentLineItemNumber > 0 ? documentLineItemNumber.ToString() : null,
							complementOfInformation = supportingDocument.Complement.LeftOrNull(DocumentComplementMaxLength),
							detail = supportingDocument.Detail.LeftOrNull(DocumentDetailMaxLength),
							issuingAuthorityName = supportingDocument.IssuingAuthorityName.LeftOrNull(DocumentAdditionalDescriptionMaxLength),
							issuingDate = supportingDocument.IssuingDate.GetValueOrDefault(),
							issuingDateSpecified = supportingDocument.IssuingDate.HasValue,
							validityDate = supportingDocument.ValidityDate.GetValueOrDefault(),
							validityDateSpecified = supportingDocument.ValidityDate.HasValue,
							measurementUnitAndQualifier = supportingDocument.MeasurementUnitAndQualifier.LeftOrNull(MeasurementUnitAndQualifierMaxLength),
							complementaryUnit = complementaryUnitMap.EnumValue,
							complementaryUnitSpecified = complementaryUnitMap.Succeeded,
							quantity = supportingDocument.Quantity,
							quantitySpecified = supportingDocument.Quantity > 0 && (!supportingDocument.MeasurementUnitAndQualifier.IsEmpty() || !supportingDocument.ComplementaryUnit.In((NoResString)"div", "ltAnlage")), // Constant
							currency = currency.LeftOrNull(CurrencyMaxLength),
							amount = supportingDocument.Amount,
							amountSpecified = !currency.IsEmpty()
						};
					}

					DEXPDFGoodsShipmentGoodsItemAdditionalReference PopulateItemAdditionalReference(IReference reference, int itemAdditionalReferenceIndex)
					{
						var currency = reference.Currency;
						return new DEXPDFGoodsShipmentGoodsItemAdditionalReference
						{
							sequenceNumber = (itemAdditionalReferenceIndex + 1).ToString(),
							type = reference.Type.LeftOrNull(DocumentTypeMaxLength),
							qualifier = reference.Qualifier.LeftOrNull(DocumentQualifierMaxLength),
							referenceNumber = reference.ReferenceNumber.LeftOrNull(DocumentReferenceNumberMaxLength),
							detail = reference.Detail.LeftOrNull(DocumentDetailMaxLength),
							currency = currency.LeftOrNull(CurrencyMaxLength),
							amount = reference.Amount,
							amountSpecified = !currency.IsEmpty(),
						};
					}

					DEXPDFGoodsShipmentGoodsItemAdditionalInformation PopulateItemAdditionalInformation(IReference reference, int itemAdditionalInformationIndex)
					{
						return new DEXPDFGoodsShipmentGoodsItemAdditionalInformation
						{
							sequenceNumber = (itemAdditionalInformationIndex + 1).ToString(),
							code = reference.FullType.LeftOrNull(AdditionalInformationCodeMaxLength),
							text = reference.Complement.LeftOrNull(AdditionalInformationTextMaxLength),
						};
					}

					DEXPDFGoodsShipmentGoodsItemTransportCharges PopulateItemTransportCharges()
					{
						return new DEXPDFGoodsShipmentGoodsItemTransportCharges
						{
							methodOfPayment = line.TransportChargesPaymentMethod.Left(TransportChargesPaymentMethodMaxLength)
						};
					}

					DEXPDFGoodsShipmentGoodsItemOutwardProcessing PopulateItemOutwardProcessing()
					{
						var replacement = line.OutwardProcessingReplacement;
						return replacement.IsEmpty() ? null : new DEXPDFGoodsShipmentGoodsItemOutwardProcessing
						{
							replacement = replacement.MapCodeToEnumWithDefaultAndItemPrefix<DEXPDFGoodsShipmentGoodsItemOutwardProcessingReplacement>(),
							reimportDate = line.OutwardProcessingReimportDate,
							reimportDateSpecified = line.OutwardProcessingReimportDate != default
						};
					}

					DEXPDFGoodsShipmentGoodsItemProcedureTransference PopulateItemProcedureTransference()
					{
						var warehousingAuthorisation = line.CustomsWarehousingAuthorisation;
						var inwardProcessingSimplyGrantedAuthorisation = line.InwardProcessingSimplyGrantedAuthorisation;
						var inwardProcessingAuthorisation = line.InwardProcessingAuthorisation;
						return !line.ProcedureTransferenceSpecified ? null : new DEXPDFGoodsShipmentGoodsItemProcedureTransference
						{
							CustomsWarehousing = !line.IsWarehouseProcedure ? null : new DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousing
							{
								LRN = line.WarehouseLocalReferenceNumber.LeftOrNull(WarehouseLocalReferenceNumberMaxLength),
								Authorisation = warehousingAuthorisation == null ? new DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousingAuthorisation()
								: new DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousingAuthorisation
								{
									type = warehousingAuthorisation.Type.MapCodeToEnumWithDefault<DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousingAuthorisationType>(),
									referenceNumber = warehousingAuthorisation.ReferenceNumber.LeftOrNull(AuthorisationReferenceNumberMaxLength)
								},
								GoodsReference = line.WarehouseProcedures.Select(PopulateItemWarehousingGoodsReference).ToArray(),
							},
							InwardProcessing = !line.IsInwardProcessingProcedure ? null : new DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessing
							{
								simplyGrantedAuthorisation = inwardProcessingSimplyGrantedAuthorisation.MapCodeToEnumWithDefaultAndItemPrefix<DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessingSimplyGrantedAuthorisation>(),
								Authorisation = inwardProcessingAuthorisation == null || inwardProcessingSimplyGrantedAuthorisation != "0" ? null : new DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessingAuthorisation
								{
									type = inwardProcessingAuthorisation.Type.MapCodeToEnumWithDefault<DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessingAuthorisationType>(),
									referenceNumber = inwardProcessingAuthorisation.ReferenceNumber.LeftOrNull(AuthorisationReferenceNumberMaxLength)
								},
								CustomsOfficeOfSupervision = inwardProcessingSimplyGrantedAuthorisation != "1" ? null : new DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessingCustomsOfficeOfSupervision
								{
									referenceNumber = line.InwardProcessingCustomsOfficeOfSupervision.LeftOrNull(CustomsOfficeReferenceNumberMaxLength)
								},
								GoodsReference = line.InwardProcessingProcedures.Select(PopulateItemInwardProcessingGoodsReference).ToArray(),
							}
						};

						DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousingGoodsReference PopulateItemWarehousingGoodsReference(IWarehouseProcedure warehouseProcedure, int itemGoodsReferenceIndex)
						{
							var mrn = warehouseProcedure.MRN;
							return new DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousingGoodsReference
							{
								sequenceNumber = (itemGoodsReferenceIndex + 1).ToString(),
								accessViaATLAS = warehouseProcedure.AccessViaAtlasFlag.MapCodeToEnumWithDefaultAndItemPrefix<DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousingGoodsReferenceAccessViaATLAS>(),
								MRN = mrn.LeftOrNull(MRNMaxLength),
								registrationNumber = mrn.IsEmpty() ? warehouseProcedure.RegistrationNumber.LeftOrNull(PreviousProcedureRegistrationNumberMaxLength) : null,
								goodsItemNumber = warehouseProcedure.ReferencedSequenceNumber.ToString(),
								usualTreatment = warehouseProcedure.UsualProcessingFlag.MapCodeToEnumWithDefaultAndItemPrefix<DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousingGoodsReferenceUsualTreatment>(),
								complementOfInformation = warehouseProcedure.Complement.LeftOrNull(PreviousProcedureComplementMaxlength),
								Commodity = new DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousingGoodsReferenceCommodity
								{
									CommodityCode = new DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousingGoodsReferenceCommodityCommodityCode
									{
										harmonizedSystemSubHeadingCode = warehouseProcedure.HarmonizedSystemSubHeadingCode.LeftOrNull(HarmonizedSystemSubHeadingCodeMaxLength),
										combinedNomenclatureCode = warehouseProcedure.CombinedNomenclatureCode.LeftOrNull(CombinedNomenclatureCodeMaxLength2),
										taricCode = warehouseProcedure.TaricCode.LeftOrNull(TaricCodeMaxLength),
										NationalAdditionalCode = new DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousingGoodsReferenceCommodityCommodityCodeNationalAdditionalCode
										{
											nationalAdditionalCode = warehouseProcedure.NationalAdditionalCode.LeftOrNull(NationalAdditionalCodeMaxLength),
										}
									},
									GoodsReduction = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousingGoodsReferenceCommodityGoodsReduction>(warehouseProcedure.DebitAmount),
									GoodsReductionAfterTreatment = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousingGoodsReferenceCommodityGoodsReductionAfterTreatment>(warehouseProcedure.CommercialAmount),
								}
							};
						}

						DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessingGoodsReference PopulateItemInwardProcessingGoodsReference(IInwardProcessingProcedure inwardProcessingProcedure, int itemGoodsReferenceIndex)
						{
							var mrn = inwardProcessingProcedure.MRN;
							return new DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessingGoodsReference
							{
								sequenceNumber = (itemGoodsReferenceIndex + 1).ToString(),
								accessViaATLAS = inwardProcessingProcedure.AccessViaAtlasFlag.MapCodeToEnumWithDefaultAndItemPrefix<DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessingGoodsReferenceAccessViaATLAS>(),
								MRN = mrn.LeftOrNull(MRNMaxLength),
								registrationNumber = mrn.IsEmpty() ? inwardProcessingProcedure.RegistrationNumber.LeftOrNull(PreviousProcedureRegistrationNumberMaxLength) : null,
								goodsItemNumber = inwardProcessingProcedure.ReferencedSequenceNumber.ToString(),
								Commodity = new DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessingGoodsReferenceCommodity
								{
									goodsRelatedData = inwardProcessingProcedure.GoodsRelatedInformation.LeftOrNull(GoodsRelatedInformationMaxLength),
								}
							};
						}
					}
				}
			}
		}
	}
}
