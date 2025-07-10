using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR934;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._934)]
	public class GOVCBR934MessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport934Header dataProvider;
		readonly MessageFunctions.MessageFunctionCode messageSendTypeCode;
		public GOVCBR934MessageBuilder(IImport934Header dataProvider, MessageFunctions.MessageFunctionCode sendTypeCode)
		{
			this.dataProvider = dataProvider;
			messageSendTypeCode = sendTypeCode;
		}
		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				FunctionCode = PopulateFunctionCode(messageSendTypeCode),
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				TypeCode = PopulateTypeCode(),
				TransactionNatureCode = PopulateTransactionNatureCode(),
				Reason = PopulateReason(),
				ReasonCode = PopulateReasonCode(),
				AdditionalDocument = PopulateAdditionalDocument(),
				AdditionalInformation = PopulateAdditionalInformation(),
				Agent = null,
				CurrencyExchange = PopulateCurrencyExchange(),
				Consignment = PopulateConsignment(),
				DutyTaxFee = PopulateDutyTaxFee(),
				GoodsShipment = PopulateGoodsShipment(),
				PreviousDocument = PopulatePreviousDocument(),
				Submitter = PopulateSubmitter(),
				Payer = PopulatePayer(),
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision };
		}

		DeclarationFunctionCodeType PopulateFunctionCode(MessageFunctions.MessageFunctionCode messageSendTypeCode)
		{
			ZString msgSendTypeCode = string.Empty;
			switch (messageSendTypeCode)
			{
				case MessageFunctions.MessageFunctionCode.Original:
					msgSendTypeCode = FunctionCode.Original;
					break;
				case MessageFunctions.MessageFunctionCode.Addition:
					msgSendTypeCode = FunctionCode.Addition;
					break;
			}
			return new DeclarationFunctionCodeType
			{
				Value = msgSendTypeCode
			};
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.ImportDeclarationNumber };
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._934 };
		}

		DeclarationTransactionNatureCodeType PopulateTransactionNatureCode()
		{
			return new DeclarationTransactionNatureCodeType { Value = dataProvider.ValuationMethod };
		}

		DeclarationReasonTextType PopulateReason()
		{
			if (dataProvider.ProvisionalPricingReasons == null)
			{
				return null;
			}

			DeclarationReasonTextType reason = null;
			foreach (var provisionalPricingReason in dataProvider.ProvisionalPricingReasons)
			{
				if (provisionalPricingReason.Code == PriceDeclarationItemCodeList.Codes._119)
				{
					reason = new DeclarationReasonTextType { Value = provisionalPricingReason.CodeOtherDescription };
					break;
				}
			}
			return reason;
		}

		Collection<DeclarationReasonCodeType> PopulateReasonCode()
		{
			if (!(dataProvider.ProvisionalPricingReasons?.Any() ?? false))
			{
				return null;
			}

			var provisionalPricingReasons = new Collection<DeclarationReasonCodeType>();
			foreach (var provisionalPricingReason in dataProvider.ProvisionalPricingReasons)
			{
				var item = new DeclarationReasonCodeType { Value = provisionalPricingReason.Code };
				provisionalPricingReasons.Add(item);
			}
			return provisionalPricingReasons;
		}

		DeclarationAdditionalDocument PopulateAdditionalDocument()
		{
			if (!dataProvider.ProvisionalPricingYN || dataProvider.DeclarationCustomsOffice.IsEmpty || dataProvider.DeclarationCustomsDivision.IsEmpty)
			{
				return null;
			}

			return new DeclarationAdditionalDocument
			{
				Id = new AdditionalDocumentIdentificationIdType
				{
					Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision + ZDate.Today.ToString("yy")
				}
			};
		}

		Collection<DeclarationAdditionalInformation> PopulateAdditionalInformation()
		{
			if ((dataProvider.FormAData?.Questions) == null)
			{
				return null;
			}

			var additionalInformationLines = new Collection<DeclarationAdditionalInformation>();
			foreach (var questionsLine in dataProvider.FormAData.Questions)
			{
				if (!questionsLine.AnswerCode.IsEmpty)
				{
					var item = addDeclarationCheckList(questionsLine.QuestionCode, questionsLine.AnswerCode, questionsLine.AnswerOtherDescription);
					additionalInformationLines.Add(item);
				}
			}
			return additionalInformationLines;
		}

		DeclarationAdditionalInformation addDeclarationCheckList(ZString itemCode, ZString statusCode, ZString description)
		{
			var item = new DeclarationAdditionalInformation
			{
				StatementCode = itemCode.IsEmpty ? null : new AdditionalInformationStatementCodeType { Value = itemCode }
			};

			if (itemCode == PriceQuestionCodeList.Codes._7B || itemCode == PriceQuestionCodeList.Codes._7E)
			{
				item.StatementDescription = statusCode.IsEmpty ? null : new AdditionalInformationStatementDescriptionTextType { Value = statusCode };
			}
			else
			{
				item.StatementTypeCode = statusCode.IsEmpty ? null : new AdditionalInformationStatementTypeCodeType { Value = statusCode };
			}

			if (itemCode == PriceQuestionCodeList.Codes._7E)
			{
				item.Content = description.IsEmpty ? null : new AdditionalInformationContentTextType { Value = description };
			}
			return item;
		}

		Collection<DeclarationCurrencyExchange> PopulateCurrencyExchange()
		{
			Collection<DeclarationCurrencyExchange> currencyExchangeLines = null;
			if (dataProvider.FormAData?.Method1ValuationData != null)
			{
				TryAddExchangeLineItem(dataProvider.FormAData.Method1ValuationData, PriceCurrencyTypeCode.Codes.A);
			}

			if (dataProvider.FormBData != null)
			{
				TryAddExchangeLineItem(dataProvider.FormBData.Method2_3ValuationData, PriceCurrencyTypeCode.Codes.D);
				TryAddExchangeLineItem(dataProvider.FormBData.Method4ValuationData, PriceCurrencyTypeCode.Codes.G);
			}
			return currencyExchangeLines;

			void TryAddExchangeLineItem(IValuationMethodData methodData, string code)
			{
				currencyExchangeLines = currencyExchangeLines ?? new Collection<DeclarationCurrencyExchange>();
				if (methodData != null)
				{
					var item = new DeclarationCurrencyExchange
					{
						RateTypeCode = new CurrencyExchangeRateTypeCodeType { Value = code },
						RateNumeric = methodData.ExchangeRate,
					};
					currencyExchangeLines.Add(item);
				}
			}
		}

		DeclarationConsignment PopulateConsignment()
		{
			var id = dataProvider.InvoiceNo.IsEmpty ? null : new AdditionalDocumentIdentificationIdType { Value = dataProvider.InvoiceNo };
			var issueDateTime = dataProvider.InvoiceDate.IsValid ? dataProvider.InvoiceDate.ToString(DateFormatType.Date) : null;

			var limitDateTime = dataProvider.EstimatedDateOfFinalPrice.IsValid ? dataProvider.EstimatedDateOfFinalPrice.ToString(DateFormatType.Date) : null;
			var periodDateTime = dataProvider.ContractExpirationDate.IsValid ? dataProvider.ContractExpirationDate.ToString(DateFormatType.Date) : null;

			return new DeclarationConsignment
			{
				AdditionalDocument = id == null && issueDateTime == null ? null : new DeclarationConsignmentAdditionalDocument
				{
					Id = id,
					IssueDateTime = issueDateTime
				},
				AdditionalInformation = limitDateTime == null && periodDateTime == null ? null : new DeclarationConsignmentAdditionalInformation
				{
					LimitDateTime = limitDateTime,
					PeriodDateTime = periodDateTime,
				},
				ConsignmentItem = new DeclarationConsignmentConsignmentItem
				{
					Commodity = new DeclarationConsignmentConsignmentItemCommodity
					{
						DutyTaxFee = new DeclarationConsignmentConsignmentItemCommodityDutyTaxFee
						{
							TaxRateNumeric = dataProvider.ProvisionalAdditionRate.IsEmpty ? null : dataProvider.ProvisionalAdditionRate,
							AdValoremTaxBaseAmount = new DutyTaxFeeAdValoremTaxBaseAmountType { Value = dataProvider.TotalCustomsValueKRW },

							Payment = dataProvider.ProvisionalAdditionalAmount.IsEmpty ? null : new DeclarationConsignmentConsignmentItemCommodityDutyTaxFeePayment
							{
								PaymentAmount = new PaymentPaymentAmountType { Value = dataProvider.ProvisionalAdditionalAmount }
							}
						}
					}
				}
			};
		}

		Collection<DeclarationDutyTaxFee> PopulateDutyTaxFee()
		{
			Collection<DeclarationDutyTaxFee> dutyTaxFeeLines = null;

			if (dataProvider.FormAData?.Method1ValuationData != null)
			{
				(dutyTaxFeeLines = dutyTaxFeeLines ?? new Collection<DeclarationDutyTaxFee>()).Add(PopulateDutyTaxFeeAddTypeCode(PriceDutyTaxFeeTypeCode.Codes.A101, dataProvider.FormAData.Method1ValuationData.BaseAmount, dataProvider.FormAData.Method1ValuationData.BaseAmountCurrency));

				if (!dataProvider.FormAData.Method1ValuationData.IndirectPaymentAmount.IsEmpty)
				{
					dutyTaxFeeLines.Add(PopulateDutyTaxFeeAddTypeCode(PriceDutyTaxFeeTypeCode.Codes.A102, dataProvider.FormAData.Method1ValuationData.IndirectPaymentAmount, ZString.Empty));
				}
				if (dataProvider.FormAData.Method1ValuationData.Additions != null)
				{
					var totalAdditionalAmount = ZDecimal.Zero;
					foreach (var additionCharge in dataProvider.FormAData.Method1ValuationData.Additions)
					{
						if (!additionCharge.Amount.IsEmpty)
						{
							var dutyTaxFee = PopulateDutyTaxFeeAddTypeCode(additionCharge.Type, additionCharge.Amount, ZString.Empty);
							dutyTaxFeeLines.Add(dutyTaxFee);
							totalAdditionalAmount += additionCharge.Amount;
						}
					}
					if (!totalAdditionalAmount.IsEmpty)
					{
						dutyTaxFeeLines.Add(PopulateDutyTaxFeeAddTypeCode(PriceDutyTaxFeeTypeCode.Codes.A117, totalAdditionalAmount, ZString.Empty));
					}
				}
				if (dataProvider.FormAData.Method1ValuationData.Deductions != null)
				{
					var totalDeductionAmount = ZDecimal.Zero;
					foreach (var deductionCharge in dataProvider.FormAData.Method1ValuationData.Deductions)
					{
						if (!deductionCharge.Amount.IsEmpty)
						{
							var dutyTaxFee = PopulateDutyTaxFeeAddTypeCode(deductionCharge.Type, deductionCharge.Amount, ZString.Empty);
							dutyTaxFeeLines.Add(dutyTaxFee);
							totalDeductionAmount += deductionCharge.Amount;
						}
					}
					if (!totalDeductionAmount.IsEmpty)
					{
						dutyTaxFeeLines.Add(PopulateDutyTaxFeeAddTypeCode(PriceDutyTaxFeeTypeCode.Codes.A122, totalDeductionAmount, ZString.Empty));
					}
				}
			}

			if (dataProvider.FormBData?.Method2_3ValuationData != null)
			{
				(dutyTaxFeeLines = dutyTaxFeeLines ?? new Collection<DeclarationDutyTaxFee>()).Add(PopulateDutyTaxFeeAddTypeCode(PriceDutyTaxFeeTypeCode.Codes.B301, dataProvider.FormBData.Method2_3ValuationData.BaseAmount, dataProvider.FormBData.Method2_3ValuationData.BaseAmountCurrency));
				if (!dataProvider.FormBData.Method2_3ValuationData.AmountInKRW.IsEmpty)
				{
					dutyTaxFeeLines.Add(PopulateDutyTaxFeeAddTypeCode(PriceDutyTaxFeeTypeCode.Codes.B302, dataProvider.FormBData.Method2_3ValuationData.AmountInKRW, ZString.Empty));
				}

				if (dataProvider.FormBData.Method2_3ValuationData.Deductions != null)
				{
					var totalDeductionAmount = ZDecimal.Zero;
					foreach (var deductionCharge in dataProvider.FormBData.Method2_3ValuationData.Deductions)
					{
						dutyTaxFeeLines.Add(PopulateDutyTaxFeeAddTypeCode(deductionCharge.Type, deductionCharge.Amount, ZString.Empty));
						totalDeductionAmount += deductionCharge.Amount;
					}
					if (!totalDeductionAmount.IsEmpty)
					{
						dutyTaxFeeLines.Add(PopulateDutyTaxFeeAddTypeCode(PriceDutyTaxFeeTypeCode.Codes.B308, totalDeductionAmount, ZString.Empty));
					}
				}
				if (dataProvider.FormBData.Method2_3ValuationData.Additions != null)
				{
					var totalAdditionalAmount = ZDecimal.Zero;
					foreach (var additionCharge in dataProvider.FormBData.Method2_3ValuationData.Additions)
					{
						dutyTaxFeeLines.Add(PopulateDutyTaxFeeAddTypeCode(additionCharge.Type, additionCharge.Amount, ZString.Empty));
						totalAdditionalAmount += additionCharge.Amount;
					}
					if (!totalAdditionalAmount.IsEmpty)
					{
						dutyTaxFeeLines.Add(PopulateDutyTaxFeeAddTypeCode(PriceDutyTaxFeeTypeCode.Codes.B314, totalAdditionalAmount, ZString.Empty));
					}
				}
			}
			if (dataProvider.FormBData?.Method4ValuationData != null)
			{
				(dutyTaxFeeLines = dutyTaxFeeLines ?? new Collection<DeclarationDutyTaxFee>()).Add(PopulateDutyTaxFeeAddTypeCode(PriceDutyTaxFeeTypeCode.Codes.B401, dataProvider.FormBData.Method4ValuationData.BaseAmount, dataProvider.FormBData.Method4ValuationData.BaseAmountCurrency));
				if (!dataProvider.FormBData.Method4ValuationData.AmountInKRW.IsEmpty)
				{
					dutyTaxFeeLines.Add(PopulateDutyTaxFeeAddTypeCode(PriceDutyTaxFeeTypeCode.Codes.B403, dataProvider.FormBData.Method4ValuationData.AmountInKRW, ZString.Empty));
				}

				DeclarationDutyTaxFee item = null;
				if (dataProvider.FormBData.Method4ValuationData.Deductions != null)
				{
					var totalDeductionAmount = ZDecimal.Zero;
					foreach (var deductionCharge in dataProvider.FormBData.Method4ValuationData.Deductions)
					{
						if (deductionCharge.Type == PriceDutyTaxFeeTypeCode.Codes.B405)
						{
							item = new DeclarationDutyTaxFee
							{
								TypeCode = new DutyTaxFeeTypeCodeType { Value = PriceDutyTaxFeeTypeCode.Codes.B405 },
								CostRateNumeric = dataProvider.FormBData.Method4ValuationData.DeductionGeneralCostPercentage,
								RateTypeCode = new DutyTaxFeeRateTypeCodeType { Value = dataProvider.FormBData.Method4ValuationData.DeductionGeneralCostPercentageType },
								Payment = new DeclarationDutyTaxFeePayment
								{
									PaymentAmount = new PaymentPaymentAmountType { Value = deductionCharge.Amount }
								}
							};
						}
						else
						{
							item = PopulateDutyTaxFeeAddTypeCode(deductionCharge.Type, deductionCharge.Amount, ZString.Empty);
						}
						dutyTaxFeeLines.Add(item);
						totalDeductionAmount += deductionCharge.Amount;
					}
					if (!totalDeductionAmount.IsEmpty)
					{
						item = new DeclarationDutyTaxFee
						{
							TypeCode = new DutyTaxFeeTypeCodeType { Value = PriceDutyTaxFeeTypeCode.Codes.B412 },
							Payment = new DeclarationDutyTaxFeePayment
							{
								PaymentAmount = new PaymentPaymentAmountType { Value = totalDeductionAmount },
								ReferenceId = new PaymentReferenceIdType { Value = dataProvider.FormBData.Method4ValuationData.DeductionCustomsReferenceNo }
							}
						};
						dutyTaxFeeLines.Add(item);
					}
				}
			}
			if (dataProvider.FormBData?.Method5_6ValuationData != null)
			{
				(dutyTaxFeeLines = dutyTaxFeeLines ?? new Collection<DeclarationDutyTaxFee>()).Add(PopulateDutyTaxFeeAddTypeCode(PriceDutyTaxFeeTypeCode.Codes.B505, dataProvider.FormBData.Method5_6ValuationData.BaseAmount, ZString.Empty));

				if (dataProvider.FormBData.Method5_6ValuationData.Additions != null)
				{
					var totalAdditionalAmount = ZDecimal.Zero;
					foreach (var additionCharge in dataProvider.FormBData.Method5_6ValuationData.Additions)
					{
						dutyTaxFeeLines.Add(PopulateDutyTaxFeeAddTypeCode(additionCharge.Type, additionCharge.Amount, ZString.Empty));
						totalAdditionalAmount += additionCharge.Amount;
					}
					if (!totalAdditionalAmount.IsEmpty)
					{
						dutyTaxFeeLines.Add(PopulateDutyTaxFeeAddTypeCode(PriceDutyTaxFeeTypeCode.Codes.B504, totalAdditionalAmount, ZString.Empty));
					}
				}
			}
			return dutyTaxFeeLines;
		}

		DeclarationDutyTaxFee PopulateDutyTaxFeeAddTypeCode(ZString amountType, ZDecimal amountValue, ZString amountCurrency)
		{
			var item = new DeclarationDutyTaxFee
			{
				TypeCode = new DutyTaxFeeTypeCodeType { Value = amountType },
				Payment = new DeclarationDutyTaxFeePayment
				{
					PaymentAmount = new PaymentPaymentAmountType
					{
						Value = amountValue
					}
				}
			};

			if (!amountCurrency.IsEmpty)
			{
				item.Payment.PaymentAmount.CurrencyId = Enum.TryParse(amountCurrency, true, out Iso3AlphaCurrencyCodeContentType result) ? result : new Iso3AlphaCurrencyCodeContentType?();
			}

			return item;
		}

		DeclarationGoodsShipment PopulateGoodsShipment()
		{
			if (dataProvider.FormBData == null)
			{
				return null;
			}

			Collection<DeclarationGoodsShipmentAdditionalInformation> useCodesLines = null;
			if (dataProvider.FormBData.UseCodes?.Any() ?? false)
			{
				useCodesLines = new Collection<DeclarationGoodsShipmentAdditionalInformation>();
				foreach (var useCodesLine in dataProvider.FormBData.UseCodes)
				{
					var statementCode = useCodesLine.Code.IsEmpty ? null : new AdditionalInformationStatementCodeType { Value = useCodesLine.Code };
					var statementDescription = useCodesLine.CodeOtherDescription.IsEmpty ? null : new AdditionalInformationStatementDescriptionTextType { Value = useCodesLine.CodeOtherDescription };

					if (statementCode == null && statementDescription == null)
					{
						continue;
					}

					useCodesLines.Add(new DeclarationGoodsShipmentAdditionalInformation
					{
						StatementCode = statementCode,
						StatementDescription = statementDescription
					});
				}
			}

			Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation> goodsPricingBasisLines = null;
			if (dataProvider.FormBData.GoodsPricingBasis?.Any() ?? false)
			{
				goodsPricingBasisLines = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation>();
				foreach (var goodsPricingBasisLine in dataProvider.FormBData.GoodsPricingBasis)
				{
					var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation
					{
						StatementCode = new AdditionalInformationStatementCodeType { Value = goodsPricingBasisLine.Code },
						StatementDescription = new AdditionalInformationStatementDescriptionTextType { Value = goodsPricingBasisLine.CodeOtherDescription }
					};
					goodsPricingBasisLines.Add(item);
				}
			}

			Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalInformation> commodityAdditionalInformationLines = null;
			if (!dataProvider.FormBData.ValuationSupportingDocument1.IsEmpty)
			{
				commodityAdditionalInformationLines = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalInformation>();
				var commodityItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalInformation
				{
					Content = new AdditionalInformationContentTextType { Value = dataProvider.FormBData.ValuationSupportingDocument1 },
					StatementCode = new AdditionalInformationStatementCodeType { Value = PriceStatementCodeType.Codes._1 }
				};
				commodityAdditionalInformationLines.Add(commodityItem);
			}
			if (!dataProvider.FormBData.ValuationSupportingDocument2.IsEmpty)
			{
				commodityAdditionalInformationLines = commodityAdditionalInformationLines ?? new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalInformation>();
				var commodityItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalInformation
				{
					Content = new AdditionalInformationContentTextType { Value = dataProvider.FormBData.ValuationSupportingDocument2 },
					StatementCode = new AdditionalInformationStatementCodeType { Value = PriceStatementCodeType.Codes._2 }
				};
				commodityAdditionalInformationLines.Add(commodityItem);
			}

			var consignment = dataProvider.FormBData.ValuationMethod.IsEmpty ? null : new DeclarationGoodsShipmentConsignment
			{
				DutyTaxFee = new DeclarationGoodsShipmentConsignmentDutyTaxFee
				{
					DutyRegimeCode = new DutyTaxFeeDutyRegimeCodeType { Value = dataProvider.FormBData.ValuationMethod }
				}
			};
			var dutyTaxFee = dataProvider.FormBData.ExpectedCustomsValue.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee
			{
				AdValoremTaxBaseAmount = new DutyTaxFeeAdValoremTaxBaseAmountType { Value = dataProvider.FormBData.ExpectedCustomsValue }
			};

			if ((useCodesLines ?? consignment ?? goodsPricingBasisLines ?? commodityAdditionalInformationLines ?? (object)dutyTaxFee) == null)
			{
				return null;
			}

			var commodity = ((dutyTaxFee ?? (object)commodityAdditionalInformationLines) == null) ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
			{
				AdditionalInformation = commodityAdditionalInformationLines,
				DutyTaxFee = dutyTaxFee
			};

			var governmentAgencyGoodsItem = (goodsPricingBasisLines ?? (object)commodity) == null ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
			{
				AdditionalInformation = goodsPricingBasisLines,
				Commodity = commodity,
			};

			return new DeclarationGoodsShipment
			{
				AdditionalInformation = useCodesLines,
				Consignment = consignment,
				GovernmentAgencyGoodsItem = governmentAgencyGoodsItem,
			};
		}

		Collection<DeclarationPreviousDocument> PopulatePreviousDocument()
		{
			Collection<DeclarationPreviousDocument> previousDocumentLines = null;

			var id = dataProvider.PurchaseOrderNo.IsEmpty ? null : new PreviousDocumentIdentificationIdType { Value = dataProvider.PurchaseOrderNo };
			var issueDateTime = dataProvider.PurchaseOrderDate.IsValid ? dataProvider.PurchaseOrderDate.ToString(DateFormatType.Date) : null;

			if (id != null || issueDateTime != null)
			{
				previousDocumentLines = new Collection<DeclarationPreviousDocument>()
				{
					new DeclarationPreviousDocument
					{
						Id = id,
						IssueDateTime = issueDateTime,
						TypeCode = new PreviousDocumentTypeCodeType { Value = PricePreviousDocumentTypeCode.Codes._105 }
					}
				};
			}

			id = dataProvider.ContractNo.IsEmpty ? null : new PreviousDocumentIdentificationIdType { Value = dataProvider.ContractNo };
			issueDateTime = dataProvider.ContractDate.IsValid ? dataProvider.ContractDate.ToString(DateFormatType.Date) : null;

			if (id != null || issueDateTime != null)
			{
				(previousDocumentLines = previousDocumentLines ?? new Collection<DeclarationPreviousDocument>()).Add(new DeclarationPreviousDocument
				{
					Id = id,
					IssueDateTime = issueDateTime,
					TypeCode = new PreviousDocumentTypeCodeType { Value = PricePreviousDocumentTypeCode.Codes._315 }
				});
			}

			return previousDocumentLines;
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			if (dataProvider.Author == null && dataProvider.ResponsiblePerson == null)
			{
				return null;
			}

			return new DeclarationSubmitter
			{
				Name = new SubmitterNameTextType { Value = dataProvider.Author?.Name ?? ZString.Empty },
				Contact = new DeclarationSubmitterContact
				{
					JobTitle = new ContactJobTitleTextType { Value = dataProvider.Author?.DepartmentAndPosition ?? ZString.Empty },
					DepartmentName = new ContactDepartmentNameTextType { Value = dataProvider.ResponsiblePerson?.DepartmentAndPosition ?? ZString.Empty },
					RepresentativeName = new ContactRepresentativeNameTextType { Value = dataProvider.ResponsiblePerson?.Name ?? ZString.Empty },
					Communication = new DeclarationSubmitterContactCommunication
					{
						Id = new CommunicationIdentificationIdType { Value = dataProvider.ResponsiblePerson?.TelephoneNumber ?? ZString.Empty }
					},
				},
				Communication = new DeclarationSubmitterCommunication
				{
					Id = new CommunicationIdentificationIdType { Value = dataProvider.Author?.TelephoneNumber ?? ZString.Empty }
				},
			};
		}

		DeclarationPayer PopulatePayer()
		{
			if (dataProvider.Payer == null)
			{
				return null;
			}

			var matchedNumber = dataProvider.Payer.GetBusinessOrIndividualRegistrationNumber()
				?? dataProvider.Payer.GetRegistrationTypeAndNumber(IdentificationType.ForeignCompanyID);

			var payerIdentificationID = matchedNumber?.Number ?? ZString.Empty;
			var payerRoleCode = matchedNumber?.Type ?? ZString.Empty;

			return new DeclarationPayer
			{
				Id = new PayerIdentificationIdType { Value = payerIdentificationID },
				Name = new PayerNameTextType { Value = dataProvider.Payer.CompanyName },
				RoleCode = new PayerRoleCodeType { Value = payerRoleCode }
			};
		}
	}
}
