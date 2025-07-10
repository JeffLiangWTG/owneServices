using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5SM;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5SM)]
	public class GOVCBR5SMMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport5SMHeader dataProvider;
		public GOVCBR5SMMessageBuilder(IImport5SMHeader dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				TypeCode = PopulateTypeCode(),
				TransactionNatureCode = PopulateTransactionNatureCode(),
				AdditionalInformation = PopulateAdditionalInformation(),
				GoodsShipment = PopulateGoodsShipment(),
				PreviousDocument = PopulatePreviousDocument(),
				Submitter = PopulateSubmitter(),
				Payer = PopulatePayer(),
				Requirement = PopulateRequirement(),
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision };
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.ValueDeclarationTemplateNumber };
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5SM };
		}

		DeclarationTransactionNatureCodeType PopulateTransactionNatureCode()
		{
			return new DeclarationTransactionNatureCodeType { Value = dataProvider.ValuationMethod };
		}

		Collection<DeclarationAdditionalInformation> PopulateAdditionalInformation()
		{
			Collection<DeclarationAdditionalInformation> declarationAdditionalInformation = null;
			if (dataProvider.FormDData != null)
			{
				if (!dataProvider.FormDData.ValuationSupportingDocument1.IsEmpty || !dataProvider.FormDData.ValuationSupportingDocument2.IsEmpty)
				{
					declarationAdditionalInformation = new Collection<DeclarationAdditionalInformation>();
					SetAdditionalInformation(declarationAdditionalInformation, dataProvider.FormDData.ValuationSupportingDocument1, PriceStatementCodeType.Codes._1);
					SetAdditionalInformation(declarationAdditionalInformation, dataProvider.FormDData.ValuationSupportingDocument2, PriceStatementCodeType.Codes._2);
				}
			}
			return declarationAdditionalInformation;
		}
		void SetAdditionalInformation(Collection<DeclarationAdditionalInformation> declarationAdditionalInformation, ZString statementDesc, ZString statementCode)
		{
			if (!statementDesc.IsEmpty)
			{
				var commodityItem = new DeclarationAdditionalInformation
				{
					StatementDescription = new AdditionalInformationStatementDescriptionTextType { Value = statementDesc },
					StatementCode = new AdditionalInformationStatementCodeType { Value = statementCode }
				};
				declarationAdditionalInformation.Add(commodityItem);
			}
		}

		DeclarationGoodsShipment PopulateGoodsShipment()
		{
			var importerCompanyName = dataProvider.Importer?.CompanyName ?? ZString.Empty;
			var importerRepresentativeName = dataProvider.Importer?.RepresentativeName ?? ZString.Empty;
			var importerAddress1 = dataProvider.Importer?.AddressLine1 ?? ZString.Empty;
			var importerAddress2 = dataProvider.Importer?.AddressLine2 ?? ZString.Empty;

			var supplierCompanyName = dataProvider.Supplier?.CompanyName ?? ZString.Empty;
			var supplierRepresentativeName = dataProvider.Supplier?.RepresentativeName ?? ZString.Empty;
			var supplierAddress1 = dataProvider.Supplier?.AddressLine1 ?? ZString.Empty;
			var supplierAddress2 = dataProvider.Supplier?.AddressLine2 ?? ZString.Empty;
			var supplierCountryCode = dataProvider.Supplier?.CountryCode ?? ZString.Empty;

			return new DeclarationGoodsShipment
			{
				Buyer = new DeclarationGoodsShipmentBuyer { Name = new BuyerNameTextType { Value = string.Join(", ", new[] { importerCompanyName, importerRepresentativeName, importerAddress1, importerAddress2 }.Where(x => !x.IsEmpty)) } },
				AdditionalInformation = PopulateGoodsShipmentAdditionalInformation(),
				DutyTaxFee = PopulateDeclarationGoodsShipmentDutyTaxFee(dataProvider.FormDData?.ValuationMethod ?? ZString.Empty),
				GovernmentAgencyGoodsItem = PopulateGoodsShipmentGovernmentAgencyGoodsItem(),
				Seller = new DeclarationGoodsShipmentSeller { Name = new SellerNameTextType { Value = string.Join(", ", new[] { supplierCompanyName, supplierRepresentativeName, supplierAddress1, supplierAddress2, supplierCountryCode }.Where(x => !x.IsEmpty)) } }
			};
		}
		Collection<DeclarationGoodsShipmentAdditionalInformation> PopulateGoodsShipmentAdditionalInformation()
		{
			var declarationGoodsShipmentAdditionalInformation = new Collection<DeclarationGoodsShipmentAdditionalInformation>();
			if (dataProvider.FormDData != null)
			{
				if (dataProvider.FormDData.UseCodes != null)
				{
					foreach (var useCodesLine in dataProvider.FormDData.UseCodes)
					{
						if (!useCodesLine.Code.IsEmpty || !useCodesLine.CodeOtherDescription.IsEmpty)
						{
							var item = new DeclarationGoodsShipmentAdditionalInformation
							{
								StatementCode = useCodesLine.Code.IsEmpty ? null : new AdditionalInformationStatementCodeType { Value = useCodesLine.Code },
								StatementTypeCode = useCodesLine.CodeOtherDescription.IsEmpty ? null : new AdditionalInformationStatementTypeCodeType { Value = useCodesLine.CodeOtherDescription }
							};
							declarationGoodsShipmentAdditionalInformation.Add(item);
						}
					}
				}
				if (dataProvider.FormDData.GoodsPricingBasis != null)
				{
					foreach (var goodsPricingBasisLine in dataProvider.FormDData.GoodsPricingBasis)
					{
						if (!goodsPricingBasisLine.Code.IsEmpty || !goodsPricingBasisLine.CodeOtherDescription.IsEmpty)
						{
							var item = new DeclarationGoodsShipmentAdditionalInformation
							{
								StatementCode = goodsPricingBasisLine.Code.IsEmpty ? null : new AdditionalInformationStatementCodeType { Value = goodsPricingBasisLine.Code },
								StatementTypeCode = goodsPricingBasisLine.CodeOtherDescription.IsEmpty ? null : new AdditionalInformationStatementTypeCodeType { Value = goodsPricingBasisLine.CodeOtherDescription }
							};
							declarationGoodsShipmentAdditionalInformation.Add(item);
						}
					}
				}
			}
			return declarationGoodsShipmentAdditionalInformation.Count == 0 ? null : declarationGoodsShipmentAdditionalInformation;
		}
		DeclarationGoodsShipmentDutyTaxFee PopulateDeclarationGoodsShipmentDutyTaxFee(ZString valuationMethod)
		{
			DeclarationGoodsShipmentDutyTaxFee dutyTaxfee = null;
			if (!valuationMethod.IsEmpty)
			{
				dutyTaxfee = new DeclarationGoodsShipmentDutyTaxFee
				{
					DutyRegimeCode = new DutyTaxFeeDutyRegimeCodeType { Value = valuationMethod }
				};
			}
			return dutyTaxfee;
		}
		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem> PopulateGoodsShipmentGovernmentAgencyGoodsItem()
		{
			var declarationGoodsShipmentGovernmentAgencyGoodsItem = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
			foreach (var entryLine in dataProvider.EntryLines)
			{
				var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
				{
					SequenceNumeric = entryLine.EntryLineNo,
					Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
					{
						CargoDescription = entryLine.InvoiceDescription.IsEmpty ? null : new CommodityCargoDescriptionTextType { Value = entryLine.InvoiceDescription },
						Description = new CommodityDescriptionTextType { Value = entryLine.HSDescription },
						Name = entryLine.BrandName.IsEmpty ? null : new CommodityNameTextType { Value = entryLine.BrandName },
						Classification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification
						{
							Id = new ClassificationIdentificationIdType
							{
								Value = entryLine.HSCode
							}
						},
						Constituent = entryLine.Ingredient.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent
						{
							ElementName = new ConstituentElementNameTextType
							{
								Value = entryLine.Ingredient
							}
						},
						DetailedCommodity = entryLine.ItemDescription.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodity
						{
							Description = new DetailedCommodityDescriptionTextType
							{
								Value = entryLine.ItemDescription
							}
						}
					}
				};
				declarationGoodsShipmentGovernmentAgencyGoodsItem.Add(item);
			}
			return declarationGoodsShipmentGovernmentAgencyGoodsItem;
		}

		DeclarationPreviousDocument PopulatePreviousDocument()
		{
			DeclarationPreviousDocument declarationPreviousDocument = null;
			if (!dataProvider.PurchaseOrderNo.IsEmpty || dataProvider.PurchaseOrderDate.IsValid)
			{
				declarationPreviousDocument = new DeclarationPreviousDocument
				{
					Id = dataProvider.PurchaseOrderNo.IsEmpty ? null : new PreviousDocumentIdentificationIdType { Value = dataProvider.PurchaseOrderNo },
					IssueDateTime = !dataProvider.PurchaseOrderDate.IsValid ? null : dataProvider.PurchaseOrderDate.ToString(DateFormatType.Date)
				};
			}
			return declarationPreviousDocument;
		}

		DeclarationSubmitter PopulateSubmitter()
		{
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
					}
				},
				Communication = new DeclarationSubmitterCommunication
				{
					Id = new CommunicationIdentificationIdType { Value = dataProvider.Author?.TelephoneNumber ?? ZString.Empty }
				}
			};
		}

		DeclarationPayer PopulatePayer()
		{
			var matchedNumber = dataProvider.Payer?.GetBusinessOrIndividualRegistrationNumber()
				?? dataProvider.Payer?.GetRegistrationTypeAndNumber(IdentificationType.ForeignCompanyID);
			return new DeclarationPayer
			{
				Name = new PayerNameTextType { Value = dataProvider.Payer?.CompanyName ?? ZString.Empty },
				Id = new PayerIdentificationIdType { Value = matchedNumber == null ? ZString.Empty : matchedNumber.Number },
				RoleCode = new PayerRoleCodeType { Value = matchedNumber == null ? ZString.Empty : matchedNumber.Type }
			};
		}

		Collection<DeclarationRequirementCheckList> PopulateRequirement()
		{
			var declarationRequirementCheckList = new Collection<DeclarationRequirementCheckList>();
			if (dataProvider.FormCData?.Questions != null)
			{
				foreach (var questions in dataProvider.FormCData.Questions)
				{
					if (!questions.QuestionCode.IsEmpty || !questions.AnswerCode.IsEmpty)
					{
						var item = new DeclarationRequirementCheckList
						{
							ItemCode = questions.QuestionCode.IsEmpty ? null : new CheckListItemCodeType { Value = questions.QuestionCode },
							Details = PopulateDeclarationRequirementCheckListDetails(questions.QuestionCode, questions.AnswerCode, questions.AnswerOtherDescription)
						};
						declarationRequirementCheckList.Add(item);
					}
				}
			}
			return declarationRequirementCheckList.Count == 0 ? null : declarationRequirementCheckList;
		}

		DeclarationRequirementCheckListDetails PopulateDeclarationRequirementCheckListDetails(ZString questionCode, ZString answerCode, ZString answerOtherDescription)
		{
			DeclarationRequirementCheckListDetails result = null;
			if (!answerCode.IsEmpty || (questionCode == PriceQuestionCodeList.Codes._5E && !answerOtherDescription.IsEmpty))
			{
				result = new DeclarationRequirementCheckListDetails()
				{
					StatusCode = new DetailsStatusCodeType { Value = answerCode },
					Description = questionCode == PriceQuestionCodeList.Codes._5E && !answerOtherDescription.IsEmpty ?
					new DetailsDescriptionTextType { Value = answerOtherDescription } : null
				};
			}
			return result;
		}
	}
}
