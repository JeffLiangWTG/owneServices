using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR929;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._929)]
	public class GOVCBR929MessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImportEntryHeader dataProvider;
		public GOVCBR929MessageBuilder(IImportEntryHeader dataProvider)
		{
			this.dataProvider = dataProvider;
		}
		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				Id = PopulateID(),
				InvoiceAmount = PopulateInvoiceAmount(),
				IssueDateTime = PopulateIssueDateTime(),
				TotalGrossMassMeasure = PopulateTotalGrossMassMeasure(),
				TotalPackageQuantity = PopulateTotalPackageQuantity(),
				TypeCode = PopulateTypeCode(),
				TransactionNatureCode = PopulateTransactionNatureCode(),
				AggregationPriceDeclarationId = PopulateAggregationPriceDeclarationIDType(),
				ResponseTypeCode = PopulateResponseTypeCode(),
				PaymentTypeCode = PopulatePaymentTypeCode(),
				Reason = PopulateReason(),
				AdditionalCode = PopulateAdditionalCode(),
				AdditionalInformation = PopulateAdditionalInformation(),
				Agent = PopulateAgent(),
				BorderTransportMeans = PopulateBorderTransportMeans(),
				CurrencyExchange = PopulateCurrencyExchange(),
				CustomsProcedure = PopulateCustomsProcedure(),
				DutyTaxFee = PopulateDutyTaxFee(),
				GoodsShipment = PopulateGoodsShipment(),
				GovernmentProcedure = PopulateGovernmentProcedure(),
				Importer = PopulateImporter(),
				SouthNorthTrade = PopulateSouthNorthTrade(),
				Submitter = PopulateSubmitter(),
				Payer = PopulatePayer(),
				TransportContractDocument = PopulateTransportContractDocument()
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision };
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.ImportDeclarationNumber };
		}

		DeclarationInvoiceAmountType PopulateInvoiceAmount()
		{
			return new DeclarationInvoiceAmountType
			{
				CurrencyId = dataProvider.InvoiceAmountCurrency.MapCodeToEnumWithDefault<Iso3AlphaCurrencyCodeContentType>(),
				Value = dataProvider.TotalInvoiceAmount
			};
		}

		string PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTotalGrossMassMeasureType PopulateTotalGrossMassMeasure()
		{
			return new DeclarationTotalGrossMassMeasureType
			{
				KcsUnitCode = Constants.DefaultWeightUnit,
				Value = dataProvider.TotalGrossWeightInKG
			};
		}

		DeclarationTotalPackageQuantityType PopulateTotalPackageQuantity()
		{
			return dataProvider.PackType.IsEmpty && dataProvider.TotalPackQty == 0 ? null : new DeclarationTotalPackageQuantityType
			{
				KcsUnitCode = dataProvider.PackType,
				Value = dataProvider.TotalPackQty
			};
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._929 };
		}

		DeclarationTransactionNatureCodeType PopulateTransactionNatureCode()
		{
			return new DeclarationTransactionNatureCodeType { Value = dataProvider.TradeType };
		}

		DeclarationAggregationPriceDeclarationIdType PopulateAggregationPriceDeclarationIDType()
		{
			return dataProvider.BlanketValuationDeclarationNumber.IsEmpty ? null : new DeclarationAggregationPriceDeclarationIdType { Value = dataProvider.BlanketValuationDeclarationNumber };
		}

		DeclarationResponseTypeCodeType PopulateResponseTypeCode()
		{
			return new DeclarationResponseTypeCodeType { Value = ResponseTypeCode.Required };
		}

		DeclarationPaymentTypeCodeType PopulatePaymentTypeCode()
		{
			return new DeclarationPaymentTypeCodeType { Value = dataProvider.PaymentType };
		}

		DeclarationReasonTextType PopulateReason()
		{
			return dataProvider.HouseBillSplitDeclarationReasonDescription.IsEmpty ? null : new DeclarationReasonTextType { Value = dataProvider.HouseBillSplitDeclarationReasonDescription };
		}

		DeclarationAdditionalCode PopulateAdditionalCode()
		{
			var certificateOfOriginIndicatorCode = dataProvider.CertificateOfOriginIssued.IsEmpty ? null : new AdditionalCodeCertificateOfOriginIndicatorCodeType { Value = dataProvider.CertificateOfOriginIssued };
			var valueDeclarationFormIndicatorCode = dataProvider.ValueDeclarationAttached.IsEmpty ? null : new AdditionalCodeValueDeclarationFormIndicatorCodeType { Value = dataProvider.ValueDeclarationAttached };
			var billOfLadingSplitCode = dataProvider.HouseBillSplitDeclarationReasonCode.IsEmpty ? null : new AdditionalCodeBillOfLadingSplitCodeType { Value = dataProvider.HouseBillSplitDeclarationReasonCode };
			var goldTradeAcountIndicatorCode = dataProvider.GoldTradeTransactionYN.IsEmpty ? null : new AdditionalCodeGoldTradeAcountIndicatorCodeType { Value = dataProvider.GoldTradeTransactionYN };
			CodeType[] values = { certificateOfOriginIndicatorCode, valueDeclarationFormIndicatorCode, billOfLadingSplitCode, goldTradeAcountIndicatorCode };
			return !values.Any(x => x != null) ? null : new DeclarationAdditionalCode
			{
				CertificateOfOriginIndicatorCode = certificateOfOriginIndicatorCode,
				ValueDeclarationFormIndicatorCode = valueDeclarationFormIndicatorCode,
				BillOfLadingSplitCode = billOfLadingSplitCode,
				GoldTradeAcountIndicatorCode = goldTradeAcountIndicatorCode
			};
		}

		DeclarationAdditionalInformation PopulateAdditionalInformation()
		{
			var statementCodes = new Collection<AdditionalInformationStatementCodeType>();
			if (!dataProvider.CustomsBrokerCommentCode1.IsEmpty)
			{
				statementCodes.Add(new AdditionalInformationStatementCodeType
				{
					Name = Constants.AdditionalInformationStatementCodes_929._257,
					Value = dataProvider.CustomsBrokerCommentCode1
				});
			}
			if (!dataProvider.CustomsBrokerCommentCode2.IsEmpty)
			{
				statementCodes.Add(new AdditionalInformationStatementCodeType
				{
					Name = Constants.AdditionalInformationStatementCodes_929._258,
					Value = dataProvider.CustomsBrokerCommentCode2
				});
			}
			if (!dataProvider.CustomsBrokerCommentCode3.IsEmpty)
			{
				statementCodes.Add(new AdditionalInformationStatementCodeType
				{
					Name = Constants.AdditionalInformationStatementCodes_929._259,
					Value = dataProvider.CustomsBrokerCommentCode3
				});
			}

			var content = dataProvider.CustomsBrokerComment1.IsEmpty ? null : new AdditionalInformationContentTextType { Value = dataProvider.CustomsBrokerComment1 };
			var statementCode = statementCodes.Count == 0 ? null : statementCodes;
			var statementDescription = dataProvider.CustomsBrokerComment2.IsEmpty ? null : new AdditionalInformationStatementDescriptionTextType { Value = dataProvider.CustomsBrokerComment2 };
			var usageDclarationCode = dataProvider.BondedFactoryUseCode.IsEmpty ? null : new AdditionalInformationUsageDclarationCodeType { Value = dataProvider.BondedFactoryUseCode };
			var usageDateTime = !dataProvider.BondedFactoryUseDate.IsValid ? null : dataProvider.BondedFactoryUseDate.ToString(DateFormatType.DateTime);

			object[] values = { content, statementCode, statementDescription, usageDclarationCode, usageDateTime };

			var result = !values.Any(x => x != null) ? null : new DeclarationAdditionalInformation
			{
				Content = content,
				StatementCode = statementCode,
				StatementDescription = statementDescription,
				UsageDclarationCode = usageDclarationCode,
				UsageDateTime = usageDateTime
			};
			return result;
		}

		DeclarationAgent PopulateAgent()
		{
			return dataProvider.AuthorizedImporterRegNo.IsEmpty ? null : new DeclarationAgent
			{
				Id = new AgentIdentificationIdType { Value = dataProvider.AuthorizedImporterRegNo }
			};
		}

		DeclarationBorderTransportMeans PopulateBorderTransportMeans()
		{
			return new DeclarationBorderTransportMeans
			{
				ArrivalDateTime = !dataProvider.ArrivalDateAtDischargePort.IsValid ? string.Empty : dataProvider.ArrivalDateAtDischargePort.ToString(DateFormatType.Date),
				Name = dataProvider.VesselOrFlightNo.IsEmpty ? null : new BorderTransportMeansNameTextType { Value = dataProvider.VesselOrFlightNo },
				RegistrationNationalityId = dataProvider.VesselCountryCode.IsEmpty ? null : new BorderTransportMeansRegistrationNationalityIdType { Value = dataProvider.VesselCountryCode },
				TypeCode = new BorderTransportMeansTypeCodeType { Value = dataProvider.TransportMode },
			};
		}

		DeclarationCurrencyExchange PopulateCurrencyExchange()
		{
			return new DeclarationCurrencyExchange
			{
				RateNumeric = Convert.ToDecimal(dataProvider.ExchangeRate)
			};
		}

		DeclarationCustomsProcedure PopulateCustomsProcedure()
		{
			return new DeclarationCustomsProcedure
			{
				ProcessTypeCode = new CustomsProcedureProcessTypeCodeType { Value = dataProvider.ImportTypeCode },
				TypeCode = new CustomsProcedureTypeCodeType { Value = dataProvider.DeclarationProcedureType },
			};
		}

		Collection<DeclarationDutyTaxFee> PopulateDutyTaxFee()
		{
			var result = new Collection<DeclarationDutyTaxFee>
			{
				new DeclarationDutyTaxFee
				{
					AdValoremTaxBaseAmount = new Collection<DutyTaxFeeAdValoremTaxBaseAmountType>
					{
						new DutyTaxFeeAdValoremTaxBaseAmountType
						{
							CurrencyId = Iso3AlphaCurrencyCodeContentType.Krw,
							Value = dataProvider.TotalCustomsValueKRW
						},
						new DutyTaxFeeAdValoremTaxBaseAmountType
						{
							CurrencyId = Iso3AlphaCurrencyCodeContentType.Usd,
							Value = dataProvider.TotalCustomsValueUSD
						}
					},
					TypeCode = new DutyTaxFeeTypeCodeType { Value = EntryTaxTypeList.Codes.CUD },
					Payment = new DeclarationDutyTaxFeePayment
					{
						TaxAssessedAmount = new PaymentTaxAssessedAmountType { Value = dataProvider.TotalDutyAmount }
					}
				},
			};

			AddDeclarationDutyTaxFee(result, EntryTaxTypeList.Codes.IND, dataProvider.TotalSpecialConsumptionTax);
			AddDeclarationDutyTaxFee(result, EntryTaxTypeList.Codes._5AA, dataProvider.TotalTransportationTax);
			AddDeclarationDutyTaxFee(result, EntryTaxTypeList.Codes.ACT, dataProvider.TotalLiquorTax);
			AddDeclarationDutyTaxFee(result, EntryTaxTypeList.Codes._5AB, dataProvider.TotalEducationTax);
			AddDeclarationDutyTaxFee(result, EntryTaxTypeList.Codes.CAP, dataProvider.TotalAgricultureTax);
			AddDeclarationDutyTaxFee(result, EntryTaxTypeList.Codes.VAT, dataProvider.TotalVAT);
			AddDeclarationDutyTaxFee(result, EntryTaxTypeList.Codes._5AC, dataProvider.PenaltyForLateDeclaration);
			AddDeclarationDutyTaxFee(result, EntryTaxTypeList.Codes._5AY, dataProvider.PenaltyForMissedDeclaration);

			DutyTaxFeeTotalVatBaseAmountType totalVATBaseAmount = dataProvider.TotalValueForVAT == 0 ? null : new DutyTaxFeeTotalVatBaseAmountType { Value = dataProvider.TotalValueForVAT };
			DutyTaxFeeTotalVatExemptionBaseAmountType totalVATExemptionBaseAmount = dataProvider.TotalVATExemptionValue == 0 ? null : new DutyTaxFeeTotalVatExemptionBaseAmountType { Value = dataProvider.TotalVATExemptionValue };
			DeclarationDutyTaxFeePayment payment = dataProvider.TotalPayableAmount == 0 ? null :
				new DeclarationDutyTaxFeePayment
				{
					TaxAssessedAmount = new PaymentTaxAssessedAmountType { Value = dataProvider.TotalPayableAmount }
				};
			if (totalVATBaseAmount != null || totalVATExemptionBaseAmount != null || payment != null)
			{
				result.Add(new DeclarationDutyTaxFee
				{
					TypeCode = new DutyTaxFeeTypeCodeType { Value = EntryTaxTypeList.Codes._5CZ },
					TotalVatBaseAmount = totalVATBaseAmount,
					TotalVatExemptionBaseAmount = totalVATExemptionBaseAmount,
					Payment = payment
				});
			}
			return result;
		}

		void AddDeclarationDutyTaxFee(Collection<DeclarationDutyTaxFee> collection, ZString typeCode, ZDecimal amount)
		{
			if (amount != 0)
			{
				collection.Add(new DeclarationDutyTaxFee
				{
					TypeCode = new DutyTaxFeeTypeCodeType { Value = typeCode },
					Payment = new DeclarationDutyTaxFeePayment
					{
						TaxAssessedAmount = new PaymentTaxAssessedAmountType { Value = amount }
					}
				});
			}
		}

		DeclarationGoodsShipment PopulateGoodsShipment()
		{
			var transportContractDocuments = new Collection<DeclarationGoodsShipmentConsignmentTransportContractDocument>();
			if (!dataProvider.HouseBillNumber.IsEmpty)
			{
				transportContractDocuments.Add(new DeclarationGoodsShipmentConsignmentTransportContractDocument
				{
					TypeCode = new TransportContractDocumentTypeCodeType { Value = TransportContractDocumentTypeCodeList.Codes._714 },
					Id = new TransportContractDocumentIdentificationIdType { Value = dataProvider.HouseBillNumber }
				});
			}
			if (!dataProvider.MasterBillNumber.IsEmpty)
			{
				transportContractDocuments.Add(new DeclarationGoodsShipmentConsignmentTransportContractDocument
				{
					TypeCode = new TransportContractDocumentTypeCodeType { Value = TransportContractDocumentTypeCodeList.Codes._704 },
					Id = new TransportContractDocumentIdentificationIdType { Value = dataProvider.MasterBillNumber }
				});
			}
			var result = new DeclarationGoodsShipment
			{
				ExportationCountryCode = new GoodsShipmentExportationCountryCodeType { Value = dataProvider.DepartureCountryCode },
				Consignment = new DeclarationGoodsShipmentConsignment
				{
					Carrier = dataProvider.CarrierID.IsEmpty ? null : new DeclarationGoodsShipmentConsignmentCarrier
					{
						Id = new CarrierIdentificationIdType { Value = dataProvider.CarrierID },
					},
					Express = dataProvider.CourierCompanyID.IsEmpty ? null : new DeclarationGoodsShipmentConsignmentExpress
					{
						Id = new ExpressIdentificationIdType { Value = dataProvider.CourierCompanyID },
					},
					TransportContractDocument = transportContractDocuments.Count == 0 ? null : transportContractDocuments,
					TransportEquipment = new DeclarationGoodsShipmentConsignmentTransportEquipment
					{
						CharacteristicCode = new TransportEquipmentCharacteristicCodeType { Value = dataProvider.ContainerPackMode }
					},
					UnloadingLocation = dataProvider.ArrivalPort.IsEmpty ? null : new DeclarationGoodsShipmentConsignmentUnloadingLocation
					{
						Id = new UnloadingLocationIdentificationIdType { Value = dataProvider.ArrivalPort }
					}
				},
				CustomsValuation = PopulateCustomsValuation(),
				GovernmentAgencyGoodsItem = PopulateGovernmentAgencyGoodsItem(),
				Seller = PopulateSeller(),
				TradeTerms = new DeclarationGoodsShipmentTradeTerms
				{
					ConditionCode = dataProvider.Incoterm.IsEmpty ? null : new TradeTermsConditionCodeType { Value = dataProvider.Incoterm },
					SettlementConditionCode = new TradeTermsSettlementConditionCodeType { Value = dataProvider.InvoicePaymentTerm }
				},
				Ucr = new DeclarationGoodsShipmentUcr
				{
					CustomsAssignedReferenceId = new UcrCustomsAssignedReferenceIdType { Value = dataProvider.CargoManagementNo },
					TraderAssignedReferenceId = dataProvider.OwnerReferenceNumber.IsEmpty ? null : new UcrTraderAssignedReferenceIdType { Value = dataProvider.OwnerReferenceNumber },
				},
				Warehouse = new DeclarationGoodsShipmentWarehouse
				{
					ArrivalDateTime = !dataProvider.UnderbondMovementArrivalDate.IsValid ? null : dataProvider.UnderbondMovementArrivalDate.ToString(DateFormatType.Date),
					Id = new WarehouseIdentificationIdType { Value = dataProvider.BondedAreaCode },
					Address = dataProvider.LocationIDInBondedArea.IsEmpty ? null : new DeclarationGoodsShipmentWarehouseAddress
					{
						Line = new AddressLineTextType { Value = dataProvider.LocationIDInBondedArea },
					},
				}
			};

			var agentID = dataProvider.FreightForwarderID.IsEmpty ? null : new AgentIdentificationIdType { Value = dataProvider.FreightForwarderID };
			var agentName = dataProvider.FreightForwarderCompanyName.IsEmpty ? null : new AgentNameTextType { Value = dataProvider.FreightForwarderCompanyName };
			if (agentID != null || agentName != null)
			{
				result.Agent = new DeclarationGoodsShipmentAgent
				{
					Id = agentID,
					Name = agentName
				};
			}
			return result;
		}

		DeclarationGoodsShipmentCustomsValuation PopulateCustomsValuation()
		{
			CustomsValuationExitToEntryChargeAmountType exitToEntry = dataProvider.Insurance == 0 ? null : new CustomsValuationExitToEntryChargeAmountType
			{
				CurrencyId = Iso3AlphaCurrencyCodeContentType.Krw,
				Value = dataProvider.Insurance
			};
			CustomsValuationFreightChargeAmountType freight = dataProvider.Freight == 0 ? null : new CustomsValuationFreightChargeAmountType
			{
				CurrencyId = Iso3AlphaCurrencyCodeContentType.Krw,
				Value = dataProvider.Freight
			};
			var chargeDeduction = new Collection<DeclarationGoodsShipmentCustomsValuationChargeDeduction>();
			if (dataProvider.AdditionalAmount != 0)
			{
				chargeDeduction.Add(new DeclarationGoodsShipmentCustomsValuationChargeDeduction
				{
					ChargesTypeCode = new ChargeDeductionChargesTypeCodeType { Value = "01" },
					OtherChargeDeductionAmount = new ChargeDeductionOtherChargeDeductionAmountType { Value = dataProvider.AdditionalAmount }
				});
			}
			if (dataProvider.DeductedAmount != 0)
			{
				chargeDeduction.Add(new DeclarationGoodsShipmentCustomsValuationChargeDeduction
				{
					ChargesTypeCode = new ChargeDeductionChargesTypeCodeType { Value = "02" },
					OtherChargeDeductionAmount = new ChargeDeductionOtherChargeDeductionAmountType { Value = dataProvider.DeductedAmount }
				});
			}
			return exitToEntry == null && freight == null && chargeDeduction.Count == 0 ? null :
				new DeclarationGoodsShipmentCustomsValuation
				{
					ExitToEntryChargeAmount = exitToEntry,
					FreightChargeAmount = freight,
					ChargeDeduction = chargeDeduction,
				};
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem> PopulateGovernmentAgencyGoodsItem()
		{
			var entryLines = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
			foreach (var entryLine in dataProvider.EntryLines)
			{
				var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
				{
					SequenceNumeric = entryLine.EntryLineNo,
					AdditionalCode = entryLine.MightRequireInspectionIndicator.IsEmpty ? null :
					new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalCode
					{
						ExaminationIndicatorCode = new AdditionalCodeExaminationIndicatorCodeType { Value = entryLine.MightRequireInspectionIndicator },
					},
					AdditionalInformation = PopulateAdditionalInformation(entryLine),
					Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
					{
						SequenceNumeric = entryLine.MaterialLineNo == 0 ? null : entryLine.MaterialLineNo,
						CargoDescription = new CommodityCargoDescriptionTextType { Value = entryLine.ModelName },
						CharacteristicCode = entryLine.AdditionalTariffCode.IsEmpty ? null : new CommodityCharacteristicCodeType { Value = entryLine.AdditionalTariffCode },
						CountQuantity = entryLine.Quantity == 0 ? null : new CommodityCountQuantityType
						{
							KcsUnitCode = entryLine.QuantityUnit,
							Value = entryLine.Quantity
						},
						Description = new CommodityDescriptionTextType { Value = entryLine.HSDescription },
						IntendedUseCode = entryLine.ProductOrMaterialCode.IsEmpty ? null : new CommodityIntendedUseCodeType { Value = entryLine.ProductOrMaterialCode },
						Name = new CommodityNameTextType { Value = entryLine.BrandName },
						NameCode = new CommodityNameCodeType { Value = entryLine.BrandCode },
						DetailedCountQuantity = entryLine.QuantityUQToClaimRefund.IsEmpty && entryLine.QuantityToClaimRefund == 0 ? null :
						new CommodityDetailedCountQuantityType
						{
							KcsUnitCode = entryLine.QuantityUQToClaimRefund,
							Value = entryLine.QuantityToClaimRefund
						},
						AdditionalCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalCode
						{
							AttachmentIndicatorCode = new AdditionalCodeAttachmentIndicatorCodeType
							{
								Value = dataProvider.ApplicationForAgreedRate
																												? Constants.SupportingDocAttachedCode.AgreementTaxRateApplicationAllInputs
																												: Constants.SupportingDocAttachedCode.EnterAll
							},
							CriteriaCode = new AdditionalCodeCriteriaCodeType { Value = entryLine.DutyRateTypeCode },
							ExaminationIndicatorCode = entryLine.CourierCargoSelectivityIndicator.IsEmpty ? null : new AdditionalCodeExaminationIndicatorCodeType { Value = entryLine.CourierCargoSelectivityIndicator }
						},
						AdditionalDocument = entryLine.SpecificUseCodeDutyRatePermitNo.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocument
						{
							Id = new AdditionalDocumentIdentificationIdType { Value = entryLine.SpecificUseCodeDutyRatePermitNo }
						},
						CertificateOfOrigin = IsCertificateOfOriginDataNull(entryLine) ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCertificateOfOrigin
						{
							CriteriaCode = entryLine.CertificateOfOriginCriteriaCode.IsEmpty ? null : new CertificateOfOriginCriteriaCodeType { Value = entryLine.CertificateOfOriginCriteriaCode },
							IssueDateTime = !entryLine.CertificateOfOriginIssueDate.IsValid ? null : entryLine.CertificateOfOriginIssueDate.ToString(DateFormatType.Date),
							IssueId = entryLine.CertificateOfOriginNo.IsEmpty ? null : new CertificateOfOriginIssueIdType { Value = entryLine.CertificateOfOriginNo },
							IssueAgencyName = entryLine.CertificateOfOriginAgencyName.IsEmpty ? null : new CertificateOfOriginIssueAgencyNameTextType { Value = entryLine.CertificateOfOriginAgencyName },
							IssueLocationCode = entryLine.CertificateOfOriginIssuingCountry.IsEmpty ? null : new CertificateOfOriginIssueLocationCodeType { Value = entryLine.CertificateOfOriginIssuingCountry },
							IssueLocationName = entryLine.CertificateOfOriginAreaName.IsEmpty ? null : new CertificateOfOriginIssueLocationNameTextType { Value = entryLine.CertificateOfOriginAreaName },
							IssuerName = entryLine.CertificateOfOriginPersonName.IsEmpty ? null : new CertificateOfOriginIssuerNameTextType { Value = entryLine.CertificateOfOriginPersonName },
							SplitIndicatorCode = entryLine.CertificateOfOriginSplitIndicator.IsEmpty ? null : new CertificateOfOriginSplitIndicatorCodeType { Value = entryLine.CertificateOfOriginSplitIndicator },
						},
						Classification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification
						{
							Id = new ClassificationIdentificationIdType { Value = entryLine.HSCode }
						},
						DetailedCommodity = PopulateDetailedCommodity(entryLine),
						DutyTaxFee = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee>
						{
							new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee
							{
								AdValoremTaxBaseAmount = new Collection<DutyTaxFeeAdValoremTaxBaseAmountType>
								{
									new DutyTaxFeeAdValoremTaxBaseAmountType
									{
										CurrencyId = Iso3AlphaCurrencyCodeContentType.Krw,
										Value = entryLine.CustomsValueKRW
									},
									new DutyTaxFeeAdValoremTaxBaseAmountType
									{
										CurrencyId = Iso3AlphaCurrencyCodeContentType.Usd,
										Value = entryLine.CustomsValueUSD
									}
								},
								DeductAmount = entryLine.DutyReductionAmount == 0 ? null : new DutyTaxFeeDeductAmountType { Value = entryLine.DutyReductionAmount },
								DutyTaxDecuctTypeCode = entryLine.DutyReductionClassification.IsEmpty ? null : new DutyTaxFeeDutyTaxDecuctTypeCodeType { Value = entryLine.DutyReductionClassification },
								DutyTaxDeductionInstallmentId = entryLine.DutyReductionOrInstallmentCode.IsEmpty ? null : new DutyTaxFeeDutyTaxDeductionInstallmentIdType { Value = entryLine.DutyReductionOrInstallmentCode },
								DeductionRateNumeric = entryLine.DutyReductionRate == 0 ? null : (decimal?)entryLine.DutyReductionRate,
								DutyRegimeCode = entryLine.DutyRateCode.IsEmpty ? null : new DutyTaxFeeDutyRegimeCodeType { Value = entryLine.DutyRateCode },
								TaxRateNumeric = PopulateCUDTaxRateNumeric(entryLine),
								TypeCode = new DutyTaxFeeTypeCodeType { Value = EntryTaxTypeList.Codes.CUD },
								Payment = entryLine.DutyAmount == 0 ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeePayment
								{
									TaxAssessedAmount = new PaymentTaxAssessedAmountType { Value = entryLine.DutyAmount }
								}
							},
						},
						PreviousDocument = PopulatePreviousDocument(entryLine),
					},
					GoodsMeasure = entryLine.NetWeightInKG == 0 ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure
					{
						NetNetWeightMeasure = new GoodsMeasureNetNetWeightMeasureType
						{
							KcsUnitCode = Constants.DefaultWeightUnit,
							Value = entryLine.NetWeightInKG
						},
					},
					Origin = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin
					{
						CountryCode = new OriginCountryCodeType { Value = entryLine.CountryOfOrigin },
						RuleCode = new OriginRuleCodeType { Value = entryLine.CountryOfOriginDeterminationRule },
						OriginDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginOriginDescription
						{
							DisplayIndicatorCode = new OriginDescriptionDisplayIndicatorCodeType { Value = entryLine.CountryOfOriginLabelLocation },
							MarksNumbersId = entryLine.CountryOfOriginLabelType.IsEmpty ? null : new OriginDescriptionMarksNumbersIdType { Value = entryLine.CountryOfOriginLabelType },
							NonDescriptionReason = entryLine.CertificateOfOriginExemptionReason.IsEmpty ? null : new OriginDescriptionNonDescriptionReasonTextType { Value = entryLine.CertificateOfOriginExemptionReason },
						},
					},
				};

				var lcnPaymentDescriptionAmount = entryLine.DomesticTaxBaseQtyOrPrice == 0 ? null : new PaymentSpecialTaxDescriptionAmountType { Value = entryLine.DomesticTaxBaseQtyOrPrice };
				var lcnPaymentAssessedAmount = PopulatePaymentTaxAssessedAmountType(entryLine);
				var lcnPayment = lcnPaymentDescriptionAmount == null && lcnPaymentAssessedAmount == null ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeePayment
				{
					SpecialTaxDescriptionAmount = lcnPaymentDescriptionAmount,
					TaxAssessedAmount = lcnPaymentAssessedAmount
				};
				IZType[] lcnValues = { entryLine.DomesticTaxClassification, entryLine.DomesticTaxCode, entryLine.ExemptionCodeOfSpecialConsumptionTax, entryLine.DomesticTaxRate, entryLine.ExemptionCodeOfLiquorTax };
				if (lcnPayment != null || lcnValues.Any(l => !l.IsEmpty))
				{
					item.Commodity.DutyTaxFee.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee
					{
						DutyRegimeCode = entryLine.DomesticTaxClassification.IsEmpty ? null : new DutyTaxFeeDutyRegimeCodeType { Value = entryLine.DomesticTaxClassification },
						InternalTaxTypeCode = entryLine.DomesticTaxCode.IsEmpty ? null : new DutyTaxFeeInternalTaxTypeCodeType { Value = entryLine.DomesticTaxCode },
						SpecialConsumptionTaxDeductionCode = entryLine.ExemptionCodeOfSpecialConsumptionTax.IsEmpty ? null : new DutyTaxFeeSpecialConsumptionTaxDeductionCodeType { Value = entryLine.ExemptionCodeOfSpecialConsumptionTax },
						TaxRateNumeric = entryLine.DomesticTaxRate == 0 ? null : (ZDecimal?)entryLine.DomesticTaxRate,
						TypeCode = new DutyTaxFeeTypeCodeType { Value = EntryTaxTypeList.Codes.LCN },
						LiquorTaxDeduction = entryLine.ExemptionCodeOfLiquorTax.IsEmpty ? null : new DutyTaxFeeLiquorTaxDeductionCodeType { Value = entryLine.ExemptionCodeOfLiquorTax },
						Payment = lcnPayment
					});
				}
				var lower5ABDutyRegimeCode = entryLine.EducationTaxExemptIndicator.IsEmpty ? null : new DutyTaxFeeDutyRegimeCodeType { Value = entryLine.EducationTaxExemptIndicator };
				var lower5ABPayment = entryLine.EducationTaxAmount.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeePayment
				{
					TaxAssessedAmount = new PaymentTaxAssessedAmountType { Value = entryLine.EducationTaxAmount }
				};
				if (lower5ABPayment != null || lower5ABDutyRegimeCode != null)
				{
					item.Commodity.DutyTaxFee.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee
					{
						DutyRegimeCode = lower5ABDutyRegimeCode,
						TypeCode = new DutyTaxFeeTypeCodeType { Value = EntryTaxTypeList.Codes._5AB },
						Payment = lower5ABPayment
					});
				}
				var capDutyRegimeCode = entryLine.AgricultureTaxClassification.IsEmpty ? null : new DutyTaxFeeDutyRegimeCodeType { Value = entryLine.AgricultureTaxClassification };
				var capPayment = entryLine.AgricultureTax.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeePayment
				{
					TaxAssessedAmount = new PaymentTaxAssessedAmountType { Value = entryLine.AgricultureTax }
				};
				if (capPayment != null || capDutyRegimeCode != null)
				{
					item.Commodity.DutyTaxFee.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee
					{
						DutyRegimeCode = capDutyRegimeCode,
						TypeCode = new DutyTaxFeeTypeCodeType { Value = EntryTaxTypeList.Codes.CAP },
						Payment = capPayment
					});
				}
				IZType[] vatValues = { entryLine.VATReductionCode, entryLine.VATRateCode, entryLine.ValueForVAT, entryLine.ValueExemptForVAT, entryLine.VATAmount };
				if (vatValues.Any(v => !v.IsEmpty))
				{
					item.Commodity.DutyTaxFee.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee
					{
						DeductionId = entryLine.VATReductionCode.IsEmpty ? null : new DutyTaxFeeDeductionIdType { Value = entryLine.VATReductionCode },
						DutyRegimeCode = entryLine.VATRateCode.IsEmpty ? null : new DutyTaxFeeDutyRegimeCodeType { Value = entryLine.VATRateCode },
						TypeCode = new DutyTaxFeeTypeCodeType { Value = EntryTaxTypeList.Codes.VAT },
						VatBaseAmount = entryLine.ValueForVAT.IsEmpty ? null : new DutyTaxFeeVatBaseAmountType { Value = entryLine.ValueForVAT },
						VatExemptionBaseAmount = entryLine.ValueExemptForVAT.IsEmpty ? null : new DutyTaxFeeVatExemptionBaseAmountType { Value = entryLine.ValueExemptForVAT },
						Payment = entryLine.VATAmount.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeePayment
						{
							TaxAssessedAmount = new PaymentTaxAssessedAmountType { Value = entryLine.VATAmount }
						}
					});
				}
				var dutyRegimeCode = entryLine.AdditionalDutyCode.IsEmpty ? null : new DutyTaxFeeDutyRegimeCodeType { Value = entryLine.AdditionalDutyCode };
				var taxRateNumeric = entryLine.AdditionalDutyRate == 0 ? null : (ZDecimal?)entryLine.AdditionalDutyRate;
				if (dutyRegimeCode != null || taxRateNumeric != null)
				{
					item.Commodity.DutyTaxFee.Add(
								new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee
								{
									DutyRegimeCode = dutyRegimeCode,
									TaxRateNumeric = taxRateNumeric,
									TypeCode = new DutyTaxFeeTypeCodeType { Value = EntryTaxTypeList.Codes.FCU },
								});
				}
				var responsibleGovernmentAgency = new Collection<ResponsibleGovernmentAgencyIdentificationIdType>();
				if (!entryLine.PostClearanceProcedureAgency1.IsEmpty)
				{
					responsibleGovernmentAgency.Add(new ResponsibleGovernmentAgencyIdentificationIdType
					{
						Value = entryLine.PostClearanceProcedureAgency1
					});
				}
				if (!entryLine.PostClearanceProcedureAgency2.IsEmpty)
				{
					responsibleGovernmentAgency.Add(new ResponsibleGovernmentAgencyIdentificationIdType
					{
						Value = entryLine.PostClearanceProcedureAgency2
					});
				}
				if (!entryLine.PostClearanceProcedureAgency3.IsEmpty)
				{
					responsibleGovernmentAgency.Add(new ResponsibleGovernmentAgencyIdentificationIdType
					{
						Value = entryLine.PostClearanceProcedureAgency3
					});
				}
				item.Commodity.ResponsibleGovernmentAgency = responsibleGovernmentAgency.Count == 0 ? null : responsibleGovernmentAgency;
				entryLines.Add(item);
			}
			return entryLines;
		}

		bool IsCertificateOfOriginDataNull(IImportEntryLine entryLine)
		{
			IZType[] values = { entryLine.CertificateOfOriginCriteriaCode, entryLine.CertificateOfOriginNo,
				entryLine.CertificateOfOriginAgencyName, entryLine.CertificateOfOriginIssuingCountry, entryLine.CertificateOfOriginAreaName,
				entryLine.CertificateOfOriginPersonName, entryLine.CertificateOfOriginSplitIndicator };
			return !values.Any(x => !x.IsEmpty) && !entryLine.CertificateOfOriginIssueDate.IsValid;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation> PopulateAdditionalInformation(IImportEntryLine entryLine)
		{
			var nonGADetails = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation>();
			foreach (var nonGADetail in entryLine.NonGADetails)
			{
				var content = nonGADetail.Reason.IsEmpty ? null : new AdditionalInformationContentTextType { Value = nonGADetail.Reason };
				var reconciliationReasonCode = nonGADetail.ReasonType.IsEmpty ? null : new AdditionalInformationReconciliationReasonCodeType { Value = nonGADetail.ReasonType };
				var criteriaConformanceCode = nonGADetail.RegulationCategoryCode.IsEmpty ? null : new AdditionalInformationCriteriaConformanceCodeType { Value = nonGADetail.RegulationCategoryCode };
				var statementCode = nonGADetail.NonGAReasonType.IsEmpty ? null : new AdditionalInformationStatementCodeType { Value = nonGADetail.NonGAReasonType };

				nonGADetails.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation
				{
					Content = content,
					ReconciliationReasonCode = reconciliationReasonCode,
					CriteriaConformanceCode = criteriaConformanceCode,
					StatementCode = statementCode
				});
			}
			return nonGADetails.Count == 0 ? null : nonGADetails;
		}

		PaymentTaxAssessedAmountType PopulatePaymentTaxAssessedAmountType(IImportEntryLine entryLine)
		{
			var tax = ZDecimal.Zero;
			var domesticTaxClassification = entryLine.DomesticTaxClassification.SubstringSafe(0, 1);
			if (domesticTaxClassification == Constants.DomestictaxClassificationCode.LQT)
			{
				tax = entryLine.LiquorTax;
			}
			else if (domesticTaxClassification == Constants.DomestictaxClassificationCode.TRT)
			{
				tax = entryLine.TransportationTax;
			}
			else
			{
				tax = entryLine.SpecialConsumptionTax;
			}

			return tax == 0 ? null : new PaymentTaxAssessedAmountType { Value = tax };
		}

		decimal PopulateCUDTaxRateNumeric(IImportEntryLine entryLine)
		{
			var rate = entryLine.DutyRateTypeCode == Constants.DutyRateTypeCode.DutyAdValorem ? entryLine.AdValoremDutyRate : entryLine.SpecificDutyRate;
			return rate;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocument> PopulatePreviousDocument(IImportEntryLine entryLine)
		{
			var previousExpDecLines = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocument>();
			foreach (var previousExpDecLine in entryLine.PreviousExpDecLines)
			{
				var id = previousExpDecLine.DeclarationNumber.IsEmpty ? null : new PreviousDocumentIdentificationIdType { Value = previousExpDecLine.DeclarationNumber };
				var lineNumeric = previousExpDecLine.EntryLineNo == 0 ? null : (decimal?)previousExpDecLine.EntryLineNo;
				var sequenceNumeric = previousExpDecLine.InvoiceLineNo == 0 ? null : (decimal?)previousExpDecLine.InvoiceLineNo;
				var goodsMeasure = previousExpDecLine.UQ.IsEmpty && previousExpDecLine.UsedQty == 0 ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocumentGoodsMeasure
				{
					NetNetWeightMeasure = new GoodsMeasureNetNetWeightMeasureType
					{
						KcsUnitCode = previousExpDecLine.UQ,
						Value = previousExpDecLine.UsedQty
					}
				};
				previousExpDecLines.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocument
				{
					Id = id,
					LineNumeric = lineNumeric,
					SequenceNumeric = sequenceNumeric,
					GoodsMeasure = goodsMeasure
				});
			}
			return previousExpDecLines.Count == 0 ? null : previousExpDecLines;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodity> PopulateDetailedCommodity(IImportEntryLine entryLine)
		{
			var invoiceLines = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodity>();
			foreach (var invoiceLine in entryLine.InvoiceLines)
			{
				var countQuantity = invoiceLine.InvoiceQuantity == 0 && invoiceLine.InvoiceUnitOfQuantiy.IsEmpty ? null : new DetailedCommodityCountQuantityType
				{
					KcsUnitCode = invoiceLine.InvoiceUnitOfQuantiy,
					Value = invoiceLine.InvoiceQuantity
				};
				var description = invoiceLine.ItemDescription.IsEmpty ? null : new DetailedCommodityDescriptionTextType { Value = invoiceLine.ItemDescription };
				var lotNumberID = invoiceLine.PartNumber.IsEmpty ? null : new DetailedCommodityLotNumberIdType { Value = invoiceLine.PartNumber };
				var unitPriceAmount = invoiceLine.UnitPrice == 0 ? null : new DetailedCommodityUnitPriceAmountType { Value = invoiceLine.UnitPrice };
				var valueAmount = invoiceLine.Amount == 0 ? null : new CargoWise.Customs.KR.MessageDefinitions.KCSDS.DetailedCommodityValueAmountType { Value = invoiceLine.Amount };
				var additionalDocument = PopulateAdditionalDocument(invoiceLine);
				var constituent = invoiceLine.Ingredient.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodityConstituent
				{
					ElementName = new ConstituentElementNameTextType { Value = invoiceLine.Ingredient }
				};

				var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodity
				{
					CountQuantity = countQuantity,
					Description = description,
					LotNumberId = lotNumberID,
					UnitPriceAmount = unitPriceAmount,
					ValueAmount = valueAmount,
					AdditionalDocument = additionalDocument,
					Constituent = constituent
				};
				if (invoiceLine.InvoiceLineNo != 0)
				{
					item.SequenceNumeric = invoiceLine.InvoiceLineNo;
				}
				invoiceLines.Add(item);
			}
			return invoiceLines.Count == 0 ? null : invoiceLines;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodityAdditionalDocument> PopulateAdditionalDocument(IImportInvoiceLine invoiceLine)
		{
			var gAApprovalDocuments = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodityAdditionalDocument>();
			foreach (var gAApprovalDocument in invoiceLine.GAApprovalDocuments)
			{
				var criteriaConformanceCode = gAApprovalDocument.RegulationCategoryCode.IsEmpty ? null : new CargoWise.Customs.KR.MessageDefinitions.DS.AdditionalDocumentCriteriaConformanceCodeType { Value = gAApprovalDocument.RegulationCategoryCode };
				var id = gAApprovalDocument.RequirementApprovalNumber.IsEmpty ? null : new AdditionalDocumentIdentificationIdType { Value = gAApprovalDocument.RequirementApprovalNumber };
				var issueDateTime = !gAApprovalDocument.ApprovalDate.IsValid ? null : gAApprovalDocument.ApprovalDate.ToString(DateFormatType.Date);
				var typeCode = gAApprovalDocument.RequirementDocumentType.IsEmpty ? null : new AdditionalDocumentTypeCodeType { Value = gAApprovalDocument.RequirementDocumentType };
				var name = gAApprovalDocument.DocumentName.IsEmpty ? null : new AdditionalDocumentTypeTextType { Value = gAApprovalDocument.DocumentName };
				var intendedUseCode = gAApprovalDocument.UseCode.IsEmpty ? null : new AdditionalDocumentIntendedUseCodeType { Value = gAApprovalDocument.UseCode };
				var commercialCategorizationID = gAApprovalDocument.UniqueItemID.IsEmpty ? null : new AdditionalDocumentCommercialCategorizationIdType { Value = gAApprovalDocument.UniqueItemID };

				gAApprovalDocuments.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodityAdditionalDocument
				{
					CriteriaConformanceCode = criteriaConformanceCode,
					Id = id,
					IssueDateTime = issueDateTime,
					TypeCode = typeCode,
					Name = name,
					IntendedUseCode = intendedUseCode,
					CommercialCategorizationId = commercialCategorizationID
				});
			}
			return gAApprovalDocuments.Count == 0 ? null : gAApprovalDocuments;
		}

		DeclarationGoodsShipmentSeller PopulateSeller()
		{
			DeclarationGoodsShipmentSeller result = null;
			if (dataProvider.Supplier != null)
			{
				var id = dataProvider.Supplier.ForeignCompanyID.IsEmpty ? null : new SellerIdentificationIdType { Value = dataProvider.Supplier.ForeignCompanyID };
				var name = dataProvider.Supplier.CompanyName.IsEmpty ? null : new SellerNameTextType { Value = dataProvider.Supplier.CompanyName };
				var address = dataProvider.Supplier.CountryCode.IsEmpty ? null : new DeclarationGoodsShipmentSellerAddress
				{
					CountryCode = new AddressCountryCodeType { Value = dataProvider.Supplier.CountryCode }
				};
				if (id != null || name != null || address != null)
				{
					result = new DeclarationGoodsShipmentSeller
					{
						Id = id,
						Name = name,
						Address = address
					};
				}
			}
			return result;
		}

		DeclarationGovernmentProcedure PopulateGovernmentProcedure()
		{
			return dataProvider.DeclarationPlanCode.IsEmpty ? null : new DeclarationGovernmentProcedure
			{
				CurrentCode = new GovernmentProcedureCurrentCodeType { Value = dataProvider.DeclarationPlanCode }
			};
		}

		DeclarationImporter PopulateImporter()
		{
			DeclarationImporter result = null;
			if (dataProvider.Importer != null)
			{
				var importerId = dataProvider.Importer.UnipassIDForOrganization.IsEmpty ? null : new ImporterIdentificationIdType { Value = dataProvider.Importer.UnipassIDForOrganization };
				var importerName = dataProvider.Importer.CompanyName.IsEmpty ? null : new ImporterNameTextType { Value = dataProvider.Importer.CompanyName };
				var importerRoleCode = dataProvider.ImporterType.IsEmpty ? null : new CargoWise.Customs.KR.MessageDefinitions.DS.ImporterRoleCodeType { Value = dataProvider.ImporterType };

				result = importerId == null && importerName == null && importerRoleCode == null ? null : new DeclarationImporter
				{
					Id = importerId,
					Name = importerName,
					RoleCode = importerRoleCode
				};
			}
			return result;
		}

		DeclarationSouthNorthTrade PopulateSouthNorthTrade()
		{
			return dataProvider.SouthNorthTradeYN.IsEmpty ? null : new DeclarationSouthNorthTrade
			{
				TradeIndicatorCode = new SouthNorthTradeTradeIndicatorCodeType { Value = dataProvider.SouthNorthTradeYN }
			};
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			var result = new DeclarationSubmitter
			{
				Name = new SubmitterNameTextType { Value = dataProvider.Declarant.CompanyName },
				Communication = new Collection<DeclarationSubmitterCommunication>
				{
					new DeclarationSubmitterCommunication
					{
						TypeId = new CommunicationTypeIdType { Value = Communication.TelNo },
						Id = new CommunicationIdentificationIdType { Value = dataProvider.Declarant.MobileNumber }
					},
				}
			};
			if (!dataProvider.Declarant.ExtensionNumber.IsEmpty)
			{
				result.Communication.Add(new DeclarationSubmitterCommunication
				{
					TypeId = new CommunicationTypeIdType { Value = Communication.ExtensionTelNo },
					Id = new CommunicationIdentificationIdType { Value = dataProvider.Declarant.ExtensionNumber }
				});
			}
			result.Communication.Add(new DeclarationSubmitterCommunication
			{
				TypeId = new CommunicationTypeIdType { Value = Communication.Email },
				Id = new CommunicationIdentificationIdType { Value = dataProvider.Declarant.Email }
			});
			return result;
		}

		DeclarationPayer PopulatePayer()
		{
			DeclarationPayer result = null;
			IDNumberAndType matchedNumber = dataProvider.Payer?.GetBusinessOrIndividualRegistrationNumber();

			var payerIdentificationID = matchedNumber?.Number ?? ZString.Empty;
			var roleCode = matchedNumber?.Type ?? ZString.Empty;

			result = new DeclarationPayer
			{
				Id = new Collection<PayerIdentificationIdType>
				{
					new PayerIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Zzz,
						Value = payerIdentificationID
					}
				},
				Name = new PayerNameTextType { Value = dataProvider.Payer?.CompanyName ?? ZString.Empty },
				RoleCode = new PayerRoleCodeType { Value = roleCode },
				Address = new DeclarationPayerAddress
				{
					CountrySubDivisionId = (dataProvider.Payer?.RoadNameCode ?? ZString.Empty).IsEmpty ? null : new AddressCountrySubDivisionIdType { Value = dataProvider.Payer.RoadNameCode },
					PostcodeId = (dataProvider.Payer?.Postcode ?? ZString.Empty).IsEmpty ? null : new AddressPostcodeIdType { Value = dataProvider.Payer.Postcode },
					Line = (dataProvider.Payer?.AddressLine2 ?? ZString.Empty).IsEmpty ? null : new AddressLineTextType { Value = dataProvider.Payer.AddressLine2 },
					BuildingNumber = (dataProvider.Payer?.BuildingNumber ?? ZString.Empty).IsEmpty ? null : new AddressBuildingNumberTextType { Value = dataProvider.Payer.BuildingNumber },
					Description = new AddressDescriptionTextType { Value = dataProvider.Payer?.AddressLine1 ?? ZString.Empty }
				},
				Contact = new DeclarationPayerContact
				{
					Name = new ContactNameTextType { Value = dataProvider.Payer?.RepresentativeName ?? ZString.Empty }
				},
				Communication = new Collection<DeclarationPayerCommunication>
				{
					new DeclarationPayerCommunication
					{
						TypeId = new CommunicationTypeIdType { Value = Communication.TelNo },
						Id = new CommunicationIdentificationIdType { Value = dataProvider.Payer?.MobileNumber ?? ZString.Empty }
					}
				}
			};

			if (dataProvider.Payer != null)
			{
				if (!dataProvider.Payer.Email.IsEmpty)
				{
					result.Communication.Add(new DeclarationPayerCommunication
					{
						TypeId = new CommunicationTypeIdType { Value = Communication.Email },
						Id = new CommunicationIdentificationIdType { Value = dataProvider.Payer.Email }
					});
				}
				var unipassID = dataProvider.Payer.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization);
				var officeID = dataProvider.Payer.GetRegistrationNumber(IdentificationType.OfficeID);
				if (!unipassID.IsEmpty())
				{
					result.Id.Add(new PayerIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Item380,
						Value = dataProvider.Payer.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization)
					});
				}
				if (!officeID.IsEmpty())
				{
					result.Id.Add(new PayerIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Kts,
						Value = dataProvider.Payer.GetRegistrationNumber(IdentificationType.OfficeID)
					});
				}
			}

			return result;
		}

		DeclarationTransportContractDocument PopulateTransportContractDocument()
		{
			return new DeclarationTransportContractDocument
			{
				SplitDeclarationIndicator = dataProvider.HouseBillSplitDeclarationIndicator
			};
		}
	}
}
