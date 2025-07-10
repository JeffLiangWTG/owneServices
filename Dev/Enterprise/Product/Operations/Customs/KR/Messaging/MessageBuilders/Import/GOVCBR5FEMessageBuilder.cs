using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5FE;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5FE)]
	public class GOVCBR5FEMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport5FEHeader dataProvider;
		readonly IAmendmentDetails amendmentDetails;
		public GOVCBR5FEMessageBuilder(IImport5FEHeader dataProvider, IAmendmentDetails amendmentDetails)
		{
			this.dataProvider = dataProvider;
			this.amendmentDetails = amendmentDetails;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				GoodsItemQuantity = PopulateGoodsItemQuantity(),
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				JurisdictionDateTime = PopulateJurisdictionDateTime(),
				LoadingListQuantity = PopulateLoadingListQuantity(),
				TypeCode = PopulateTypeCode(),
				VersionId = PopulateVersionID(),
				TransactionNatureCode = PopulateTransactionNatureCode(),
				Reason = PopulateReason(),
				ReasonCode = PopulateReasonCode(),
				AdditionalCode = PopulateAdditionalCode(),
				AdditionalDocument = PopulateAdditionalDocument(),
				AdditionalInformation = PopulateAdditionalInformation(),
				Amendment = PopulateAmendment(),
				Consignment = PopulateConsignment(),
				DutyTaxFee = PopulateDutyTaxFee(),
				GoodsShipment = PopulateGoodsShipment(),
				GovernmentInformation = PopulateGovernmentInformation(),
				Submitter = PopulateSubmitter(),
				Payer = PopulatePayer(),
			};

			DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
			{
				return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision };
			}

			DeclarationGoodsItemQuantityType PopulateGoodsItemQuantity()
			{
				return new DeclarationGoodsItemQuantityType { Value = dataProvider.TotalAmendedItemCount };
			}

			DeclarationIdentificationIdType PopulateID()
			{
				return new DeclarationIdentificationIdType { Value = dataProvider.ImportDeclarationNumber };
			}

			ZString PopulateIssueDateTime()
			{
				return ZDate.Today.ToString(DateFormatType.Date);
			}

			ZString PopulateJurisdictionDateTime()
			{
				return !dataProvider.DeclarationDate.IsValid ? string.Empty : dataProvider.DeclarationDate.ToString(DateFormatType.Date);
			}

			DeclarationLoadingListQuantityType PopulateLoadingListQuantity()
			{
				return dataProvider.TotalAmendedTaxCount.IsEmpty ? null : new DeclarationLoadingListQuantityType { Value = dataProvider.TotalAmendedTaxCount };
			}

			DeclarationTypeCodeType PopulateTypeCode()
			{
				return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5FE };
			}

			DeclarationVersionIdType PopulateVersionID()
			{
				return new DeclarationVersionIdType { Value = amendmentDetails.AmendmentVersionNo.ToString() };
			}

			DeclarationTransactionNatureCodeType PopulateTransactionNatureCode()
			{
				return new DeclarationTransactionNatureCodeType { Value = amendmentDetails.AmendmentType };
			}

			DeclarationReasonTextType PopulateReason()
			{
				return amendmentDetails.FaultPartyOtherDescription.IsEmpty ? null : new DeclarationReasonTextType { Value = amendmentDetails.FaultPartyOtherDescription };
			}

			DeclarationReasonCodeType PopulateReasonCode()
			{
				return new DeclarationReasonCodeType { Value = amendmentDetails.FaultParty };
			}

			DeclarationAdditionalCode PopulateAdditionalCode()
			{
				return new DeclarationAdditionalCode
				{
					AttachmentSubmissionTypeCode = new AdditionalCodeAttachmentSubmissionTypeCodeType { Value = !dataProvider.RefundRequestNumber.IsEmpty ? "Y" : "N" }
				};
			}

			DeclarationAdditionalDocument PopulateAdditionalDocument()
			{
				return dataProvider.PenaltyExemptionAmount.IsEmpty && dataProvider.RefundRequestNumber.IsEmpty ? null : new DeclarationAdditionalDocument
				{
					AmountAmount = dataProvider.PenaltyExemptionAmount.IsEmpty ? null : new AdditionalDocumentAmountAmountType { Value = dataProvider.PenaltyExemptionAmount },
					Id = dataProvider.RefundRequestNumber.IsEmpty ? null : new AdditionalDocumentIdentificationIdType { Value = dataProvider.RefundRequestNumber }
				};
			}

			DeclarationAdditionalInformation PopulateAdditionalInformation()
			{
				return new DeclarationAdditionalInformation
				{
					Content = dataProvider.PenaltyExemptionReason.IsEmpty ? null : new AdditionalInformationContentTextType { Value = dataProvider.PenaltyExemptionReason },
					StatementCode = new AdditionalInformationStatementCodeType { Value = dataProvider.PenaltyExemptionIndicator },
					StatementTypeCode = dataProvider.PenaltyExemptionReasonCode.IsEmpty ? null : new AdditionalInformationStatementTypeCodeType { Value = dataProvider.PenaltyExemptionReasonCode },
					StatementDescription = dataProvider.DutyPenaltyExemption5UASequenceNumber.IsEmpty ? null : new AdditionalInformationStatementDescriptionTextType { Value = dataProvider.DutyPenaltyExemption5UASequenceNumber.ToString() },
					AdditionalPaymentCode = amendmentDetails.PenaltyPaymentReasonCode.IsEmpty ? null : new AdditionalInformationAdditionalPaymentCodeType { Value = amendmentDetails.PenaltyPaymentReasonCode }
				};
			}

			DeclarationAmendment PopulateAmendment()
			{
				return new DeclarationAmendment
				{
					ChangeReasonCode = new AmendmentChangeReasonCodeType { Value = amendmentDetails.ReasonCode },
					Content = amendmentDetails.AmendReasonDescription.IsEmpty ? null : new AmendmentContentTextType { Value = amendmentDetails.AmendReasonDescription }
				};
			}

			Collection<DeclarationConsignment> PopulateConsignment()
			{
				var consignments = new Collection<DeclarationConsignment>();
				foreach (var consignment in dataProvider.AmendedItems)
				{
					var item = new DeclarationConsignment
					{
						SequenceNumeric = new ZDecimal(consignment.EntryLineNo),
						AdditionalDocument = PopulateDeclarationConsignmentAdditionalDocument(consignment),
						Amendment = new DeclarationConsignmentAmendment
						{
							ChangeReasonCode = new AmendmentChangeReasonCodeType { Value = consignment.AmendType },
							Pointer = new DeclarationConsignmentAmendmentPointer
							{
								DocumentSectionCode = consignment.InvoiceLineNo.IsEmpty ? null : new PointerDocumentSectionCodeType { Value = consignment.InvoiceLineNo.ToString() },
								TagId = new PointerTagIdType { Value = consignment.AmendDataItemID },
							},
							StatementDescription = consignment.BeforeDescription.IsEmpty ? null : new AmendmentStatementDescriptionTextType { Value = consignment.BeforeDescription },
							AdjustmentDescription = consignment.AfterDescription.IsEmpty ? null : new AmendmentAdjustmentDescriptionTextType { Value = consignment.AfterDescription }
						},
						PreviousDocument = PopulateDeclarationConsignmentPreviousDocument(consignment)
					};
					consignments.Add(item);
				}
				return consignments;
			}

			DeclarationConsignmentAdditionalDocument PopulateDeclarationConsignmentAdditionalDocument(IImport5FEItem consignment)
			{
				if (consignment.ExportDeclarationNumber.IsEmpty && consignment.ExportDeclarationEntryLineNo.IsEmpty && consignment.ExportDeclarationInvoiceLineNo.IsEmpty)
				{
					return null;
				}
				var result = new DeclarationConsignmentAdditionalDocument()
				{
					Id = consignment.ExportDeclarationNumber.IsEmpty ? null : new AdditionalDocumentIdentificationIdType { Value = consignment.ExportDeclarationNumber }
				};
				if (!consignment.ExportDeclarationEntryLineNo.IsEmpty)
				{
					result.SequenceNumeric = consignment.ExportDeclarationEntryLineNo;
				}
				if (!consignment.ExportDeclarationInvoiceLineNo.IsEmpty)
				{
					result.LineNumeric = consignment.ExportDeclarationInvoiceLineNo;
				}
				return result;
			}

			DeclarationConsignmentPreviousDocument PopulateDeclarationConsignmentPreviousDocument(IImport5FEItem consignment)
			{
				if (consignment.GARequirementApprovalNumber.IsEmpty && consignment.NonGASequnceNo.IsEmpty)
				{
					return null;
				}
				var result = new DeclarationConsignmentPreviousDocument()
				{
					Id = consignment.GARequirementApprovalNumber.IsEmpty ? null : new PreviousDocumentIdentificationIdType { Value = consignment.GARequirementApprovalNumber }
				};
				if (!consignment.NonGASequnceNo.IsEmpty)
				{
					result.SequenceNumeric = consignment.NonGASequnceNo;
				}
				return result;
			}

			Collection<DeclarationDutyTaxFee> PopulateDutyTaxFee()
			{
				if (!dataProvider.TaxItems.Any())
				{
					return null;
				}

				var dutyTaxFees = new Collection<DeclarationDutyTaxFee>();
				foreach (var dutyTaxFee in dataProvider.TaxItems)
				{
					var item = new DeclarationDutyTaxFee
					{
						TypeCode = new DutyTaxFeeTypeCodeType { Value = dutyTaxFee.DutyTaxType },
						Payment = new DeclarationDutyTaxFeePayment
						{
							TaxAssessedAmount = new PaymentTaxAssessedAmountType { Value = dutyTaxFee.BeforeAmount },
							PaymentAmount = new PaymentPaymentAmountType { Value = dutyTaxFee.AfterAmount }
						},
						DifferenceAmount = new DutyTaxFeeDifferenceAmountType { Value = dutyTaxFee.AmountDifference }
					};
					dutyTaxFees.Add(item);
				}
				return dutyTaxFees;
			}

			DeclarationGoodsShipment PopulateGoodsShipment()
			{
				var isDutyTaxDataEmpty = dataProvider.DomesticTaxPenaltyType.IsEmpty && dataProvider.DutyPenaltyReducedYN.IsEmpty && dataProvider.DutyTaxDifference.IsEmpty && dataProvider.CustomsValueDifference.IsEmpty;
				var isDutyTaxAmountEmpty = dataProvider.BeforeTotalDutyTaxAmount.IsEmpty && dataProvider.BeforeCustomsValue.IsEmpty && dataProvider.AfterTotalDutyTaxAmount.IsEmpty && dataProvider.AfterCustomsValue.IsEmpty;

				return isDutyTaxDataEmpty && isDutyTaxAmountEmpty ? null : new DeclarationGoodsShipment
				{
					Consignment = isDutyTaxDataEmpty ? null : new DeclarationGoodsShipmentConsignment
					{
						DutyTaxFee = new DeclarationGoodsShipmentConsignmentDutyTaxFee
						{
							AdditionalTaxTypeCode = dataProvider.DomesticTaxPenaltyType.IsEmpty ? null : new DutyTaxFeeAdditionalTaxTypeCodeType { Value = dataProvider.DomesticTaxPenaltyType },
							AdditionalTaxReductionCode = dataProvider.DutyPenaltyReducedYN.IsEmpty ? null : new DutyTaxFeeAdditionalTaxReductionCodeType { Value = dataProvider.DutyPenaltyReducedYN },
							TotalTaxAmount = dataProvider.DutyTaxDifference.IsEmpty ? null : new DutyTaxFeeTotalTaxAmountType { Value = dataProvider.DutyTaxDifference },
							AdValoremTaxBaseAmount = dataProvider.CustomsValueDifference.IsEmpty ? null : new DutyTaxFeeAdValoremTaxBaseAmountType { Value = dataProvider.CustomsValueDifference }
						}
					},

					DutyTaxFee = isDutyTaxAmountEmpty ? null : PopulateGoodsShipmentDutyTaxFee()
				};
			}

			Collection<DeclarationGoodsShipmentDutyTaxFee> PopulateGoodsShipmentDutyTaxFee()
			{
				var result = new Collection<DeclarationGoodsShipmentDutyTaxFee>();

				if (!dataProvider.BeforeTotalDutyTaxAmount.IsEmpty || !dataProvider.BeforeCustomsValue.IsEmpty)
				{
					var dutyTaxFee = new DeclarationGoodsShipmentDutyTaxFee
					{
						RequestOverrideCode = new DutyTaxFeeRequestOverrideCodeType { Value = RequestOverrideCode.Before },
						Payment = new DeclarationGoodsShipmentDutyTaxFeePayment
						{
							TaxAssessedAmount = dataProvider.BeforeTotalDutyTaxAmount.IsEmpty ? null : new PaymentTaxAssessedAmountType { Value = dataProvider.BeforeTotalDutyTaxAmount },
							AssessmentAmount = dataProvider.BeforeCustomsValue.IsEmpty ? null : new PaymentAssessmentAmountType { Value = dataProvider.BeforeCustomsValue }
						}
					};
					result.Add(dutyTaxFee);
				}

				if (!dataProvider.AfterTotalDutyTaxAmount.IsEmpty || !dataProvider.AfterCustomsValue.IsEmpty)
				{
					var dutyTaxFee = new DeclarationGoodsShipmentDutyTaxFee
					{
						RequestOverrideCode = new DutyTaxFeeRequestOverrideCodeType { Value = RequestOverrideCode.After },
						Payment = new DeclarationGoodsShipmentDutyTaxFeePayment
						{
							TaxAssessedAmount = dataProvider.AfterTotalDutyTaxAmount.IsEmpty ? null : new PaymentTaxAssessedAmountType { Value = dataProvider.AfterTotalDutyTaxAmount },
							AssessmentAmount = dataProvider.AfterCustomsValue.IsEmpty ? null : new PaymentAssessmentAmountType { Value = dataProvider.AfterCustomsValue }
						}
					};
					result.Add(dutyTaxFee);
				}

				return result;
			}

			DeclarationGovernmentInformation PopulateGovernmentInformation()
			{
				return dataProvider.DutyPenaltyType.IsEmpty ? null : new DeclarationGovernmentInformation
				{
					TypeCode = new GovernmentInformationTypeCodeType { Value = dataProvider.DutyPenaltyType }
				};
			}

			DeclarationSubmitter PopulateSubmitter()
			{
				return new DeclarationSubmitter
				{
					Name = new SubmitterNameTextType { Value = dataProvider.Declarant?.CompanyName ?? ZString.Empty },
					RoleCode = new SubmitterRoleCodeType { Value = dataProvider.DeclarantType },
					Contact = new DeclarationSubmitterContact
					{
						Name = new ContactNameTextType { Value = dataProvider.Declarant?.RepresentativeName ?? ZString.Empty }
					}
				};
			}

			DeclarationPayer PopulatePayer()
			{
				return new DeclarationPayer
				{
					Name = new PayerNameTextType { Value = dataProvider.Payer?.CompanyName ?? ZString.Empty },
					Contact = new DeclarationPayerContact
					{
						Name = new ContactNameTextType { Value = dataProvider.Payer?.RepresentativeName ?? ZString.Empty }
					}
				};
			}
		}
	}
}
