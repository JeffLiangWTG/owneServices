using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5UL;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5UL)]
	public class GOVCBR5ULMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport5ULHeader dataProvider;
		public GOVCBR5ULMessageBuilder(IImport5ULHeader dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				Id = PopulateID(),
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				FunctionCode = PopulateFunctionCode(),
				InvoiceAmount = PopulateInvoiceAmount(),
				TypeCode = PopulateTypeCode(),
				TransactionNatureCode = PopulateTransactionNatureCode(),
				ReasonCode = PopulateReasonCode(),
				AdditionalInformation = PopulateAdditionalInformation(),
				Agent = PopulateAgent(),
				BankAccount = PopulateBankAccount(),
				GoodsShipment = PopulateGoodsShipment(),
				Submitter = PopulateSubmitter()
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision };
		}

		DeclarationFunctionCodeType PopulateFunctionCode()
		{
			return dataProvider.TaxOfficeCode.IsEmpty ? null : new DeclarationFunctionCodeType { Value = dataProvider.TaxOfficeCode };
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.RefundDeclarationNumber };
		}

		DeclarationInvoiceAmountType PopulateInvoiceAmount()
		{
			return new DeclarationInvoiceAmountType { Value = dataProvider.TotalRefundAmount };
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5UL };
		}

		DeclarationTransactionNatureCodeType PopulateTransactionNatureCode()
		{
			return new DeclarationTransactionNatureCodeType { Value = dataProvider.RefundType };
		}

		DeclarationReasonCodeType PopulateReasonCode()
		{
			return dataProvider.RefundCauseCode.IsEmpty ? null : new DeclarationReasonCodeType { Value = dataProvider.RefundCauseCode };
		}

		DeclarationAdditionalInformation PopulateAdditionalInformation()
		{
			return new DeclarationAdditionalInformation
			{
				StatementCode = new AdditionalInformationStatementCodeType { Value = dataProvider.Are5FE_5ULToBeSentTogether ? YesNo.Yes : YesNo.No },
				Pointer = dataProvider.RefundReasonCode.IsEmpty ? null : new DeclarationAdditionalInformationPointer
				{
					DocumentSectionCode = new PointerDocumentSectionCodeType { Value = dataProvider.RefundReasonCode }
				}
			};
		}

		Collection<AgentIdentificationIdType> PopulateAgent()
		{
			var result = new Collection<AgentIdentificationIdType>();

			if (dataProvider.Payer != null)
			{
				if (!dataProvider.Payer.IsIndividual)
				{
					result.Add(new AgentIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Ktx,
						Value = dataProvider.Payer.BusinessRegNo
					});
				}

				if (!string.IsNullOrEmpty(dataProvider.Payer.KoreanRegNoForResident))
				{
					result.Add(new AgentIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Kas,
						Value = dataProvider.Payer.KoreanRegNoForResident
					});
				}
			}

			return result;
		}

		DeclarationBankAccount PopulateBankAccount()
		{
			return new DeclarationBankAccount
			{
				Id = new CargoWise.Customs.KR.MessageDefinitions.DS.BankAccountIdentificationIdType { Value = dataProvider.BankAccountNumber },
				ReferenceId = new BankAccountReferenceIdType { Value = dataProvider.BankCode },
				Name = PopulateName()
			};

			BankAccountNameTextType PopulateName()
			{
				var description = new BankTypeList().GetDescriptionFromCode(dataProvider.BankCode);
				return description == null ? null : new BankAccountNameTextType { Value = description };
			}
		}

		Collection<DeclarationGoodsShipment> PopulateGoodsShipment()
		{
			var result = new Collection<DeclarationGoodsShipment>();
			foreach (var entryLine in dataProvider.EntryLines)
			{
				var item = new DeclarationGoodsShipment
				{
					SequenceNumeric = entryLine.RefundLineNo,
					AdditionalDocument = PopulateAdditionalDocument(entryLine),
					DutyTaxFee = PopulateDutyTaxFee(entryLine),
					GovernmentAgencyGoodsItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
					{
						SequenceNumeric = entryLine.VersionNumber5WN,
						AdditionalDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument
						{
							AmountAmount = new AdditionalDocumentAmountAmountType { Value = entryLine.TotalOtherTaxItemAmount },
							Id = new AdditionalDocumentIdentificationIdType { Value = entryLine.SoABillNumber },
							IssueDateTime = entryLine.VATDecisionDate.IsValid ? entryLine.VATDecisionDate.ToString(DateFormatType.Date) : null
						},
						AdditionalInformation = PopulateAdditionalInformation(entryLine),
						Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
						{
							Description = entryLine.ResidualSubstanceDescription.IsEmpty ? null : new CommodityDescriptionTextType { Value = entryLine.ResidualSubstanceDescription },
							DetailedCommodity = PopulateDetailedCommodity(entryLine),
							DutyTaxFee = PopulateDutyOtherTaxFee(entryLine)
						},
						PreviousDocument = entryLine.DisposalNumber.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocument
						{
							Id = new PreviousDocumentIdentificationIdType { Value = entryLine.DisposalNumber }
						}
					},
					Warehouse = PopulateWarehouse(entryLine)
				};
				result.Add(item);
			}
			return result;

			Collection<DeclarationGoodsShipmentAdditionalDocument> PopulateAdditionalDocument(IImport5ULEntryLine entryLine)
			{
				Collection<DeclarationGoodsShipmentAdditionalDocument> additionalDocuments = null;
				if (!entryLine.ImportDeclarationNumber.IsEmpty && !entryLine.ImportEntryLineNo.IsEmpty)
				{
					additionalDocuments = new Collection<DeclarationGoodsShipmentAdditionalDocument>()
					{
						new DeclarationGoodsShipmentAdditionalDocument
						{
							Id = new AdditionalDocumentIdentificationIdType { Value = entryLine.ImportDeclarationNumber },
							SequenceNumeric = entryLine.ImportEntryLineNo,
							TypeCode = new AdditionalDocumentTypeCodeType { Value = ElectronicDocumentTypeList.Codes._929 }
						}
					};
				}

				if (!entryLine.ExportDeclarationNumber.IsEmpty && !entryLine.ExportEntryLineNo.IsEmpty)
				{
					(additionalDocuments = additionalDocuments ?? new Collection<DeclarationGoodsShipmentAdditionalDocument>())
						.Add(new DeclarationGoodsShipmentAdditionalDocument
						{
							Id = new AdditionalDocumentIdentificationIdType { Value = entryLine.ExportDeclarationNumber },
							SequenceNumeric = entryLine.ExportEntryLineNo,
							TypeCode = new AdditionalDocumentTypeCodeType { Value = ElectronicDocumentTypeList.Codes._830 }
						});
				}

				return additionalDocuments;
			}

			DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation PopulateAdditionalInformation(IImport5ULEntryLine entryLine)
			{
				var content = entryLine.DamageSituation.IsEmpty ? null : new AdditionalInformationContentTextType { Value = entryLine.DamageSituation };
				var statementCode = entryLine.CancelReasonCode.IsEmpty ? null : new AdditionalInformationStatementCodeType { Value = entryLine.CancelReasonCode };

				if (content == null && statementCode == null)
				{
					return null;
				}
				return new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation
				{
					Content = content,
					StatementCode = statementCode,
				};
			}

			DeclarationGoodsShipmentWarehouse PopulateWarehouse(IImport5ULEntryLine entryLine)
			{
				var arrivalDateTime = entryLine.DisposalDate.IsValid ? entryLine.DisposalDate.ToString(DateFormatType.Date) : null;
				var name = entryLine.GoodsLocationDescription.IsEmpty ? null : new WarehouseNameTextType { Value = entryLine.GoodsLocationDescription };

				if (arrivalDateTime == null && name == null)
				{
					return null;
				}

				return new DeclarationGoodsShipmentWarehouse
				{
					Name = name,
					ArrivalDateTime = arrivalDateTime
				};
			}
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			return new DeclarationSubmitter
			{
				Id = PopulateID(),
				Name = new SubmitterNameTextType { Value = dataProvider.Payer?.CompanyName ?? ZString.Empty },
				RoleCode = new SubmitterRoleCodeType { Value = !(dataProvider.Payer?.IsIndividual ?? false) ? "01" : "02" },
				Address = new DeclarationSubmitterAddress
				{
					CountrySubDivisionId = dataProvider.Payer?.RoadNameCode.IsEmpty ?? true ? null : new AddressCountrySubDivisionIdType { Value = dataProvider.Payer?.RoadNameCode },
					Line = dataProvider.Payer?.AddressLine2.IsEmpty ?? true ? null : new AddressLineTextType { Value = dataProvider.Payer?.AddressLine2 },
					PostcodeId = dataProvider.Payer?.Postcode.IsEmpty ?? true ? null : new AddressPostcodeIdType { Value = dataProvider.Payer?.Postcode },
					BuildingNumber = dataProvider.Payer?.BuildingNumber.IsEmpty ?? true ? null : new AddressBuildingNumberTextType { Value = dataProvider.Payer?.BuildingNumber },
					Description = new AddressDescriptionTextType { Value = dataProvider.Payer?.AddressLine1 ?? ZString.Empty }
				},
				Contact = dataProvider.Payer?.RepresentativeName.IsEmpty ?? true ? null : new DeclarationSubmitterContact
				{
					RepresentativeName = new ContactRepresentativeNameTextType { Value = dataProvider.Payer?.RepresentativeName },
				},
				Agent = PopulateAgent()
			};

			SubmitterIdentificationIdType PopulateID()
			{
				if (dataProvider.Payer?.IsIndividual ?? true)
				{
					return null;
				}

				var registrationNumber = dataProvider.Payer?.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization);
				return string.IsNullOrEmpty(registrationNumber) ? null : new SubmitterIdentificationIdType
				{
					Value = registrationNumber
				};
			}

			DeclarationSubmitterAgent PopulateAgent()
			{
				if (!(dataProvider.Payer?.IsIndividual ?? false))
				{
					return null;
				}

				var registrationNumber = dataProvider.Payer?.GetRegistrationNumber(IdentificationType.UnipassIDForIndividual);
				return string.IsNullOrEmpty(registrationNumber) ? null : new DeclarationSubmitterAgent
				{
					Id = new AgentIdentificationIdType
					{
						Value = registrationNumber
					}
				};
			}
		}

		Collection<DeclarationGoodsShipmentDutyTaxFee> PopulateDutyTaxFee(IImport5ULEntryLine entryLine)
		{
			var result = new Collection<DeclarationGoodsShipmentDutyTaxFee>();

			foreach (var itemNumber in entryLine.TaxItems)
			{
				var item = new DeclarationGoodsShipmentDutyTaxFee
				{
					TypeCode = new DutyTaxFeeTypeCodeType { Value = itemNumber.TaxItem },
					SurTaxAmount = new DutyTaxFeeSurTaxAmountType { Value = itemNumber.PenaltyAmount },
					Payment = new DeclarationGoodsShipmentDutyTaxFeePayment
					{
						TaxAssessedAmount = new PaymentTaxAssessedAmountType { Value = itemNumber.Tax }
					}
				};
				result.Add(item);
			}
			return result;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodity> PopulateDetailedCommodity(IImport5ULEntryLine entryLine)
		{
			if (!(entryLine.InvoiceLines?.Any() ?? false))
			{
				return null;
			}
			var result = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodity>();
			foreach (var invoiceLine in entryLine.InvoiceLines)
			{
				var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodity
				{
					SequenceNumeric = invoiceLine.InvoiceLineNo,
					CargoDescription = invoiceLine.HSDescription.IsEmpty ? null : new DetailedCommodityCargoDescriptionTextType { Value = invoiceLine.HSDescription },
					CountQuantity = invoiceLine.InvoiceQuantity.IsEmpty ? null : new DetailedCommodityCountQuantityType { Value = invoiceLine.InvoiceQuantity },
					Description = invoiceLine.ItemDescription.IsEmpty ? null : new DetailedCommodityDescriptionTextType { Value = invoiceLine.ItemDescription },
					UnitPriceAmount = invoiceLine.UnitPrice.IsEmpty ? null : new DetailedCommodityUnitPriceAmountType { Value = invoiceLine.UnitPrice },
					DetailedCountQuantity = invoiceLine.RefundQuantity.IsEmpty ? null : new DetailedCommodityDetailedCountQuantityType { Value = invoiceLine.RefundQuantity }
				};
				result.Add(item);
			}
			return result;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee> PopulateDutyOtherTaxFee(IImport5ULEntryLine entryLine)
		{
			var result = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee>();
			foreach (var itemNumber in entryLine.OtherTaxItems)
			{
				var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee
				{
					TypeCode = new DutyTaxFeeTypeCodeType { Value = itemNumber.TaxItem },
					Payment = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeePayment
					{
						TaxAssessedAmount = new PaymentTaxAssessedAmountType { Value = itemNumber.Tax }
					}
				};
				result.Add(item);
			}
			return result;
		}
	}
}
