using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BB;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5BB)]
	public class GOVCBR5BBMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport5BBHeader dataProvider;
		readonly IAmendmentDetails amendmentProvider;
		public GOVCBR5BBMessageBuilder(IImport5BBHeader dataProvider, IAmendmentDetails amendmentProvider)
		{
			this.dataProvider = dataProvider;
			this.amendmentProvider = amendmentProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				TypeCode = PopulateTypeCode(),
				VersionId = PopulateVersionID(),
				TransactionNatureCode = PopulateTransactionNatureCode(),
				Reason = PopulateReason(),
				Consignment = PopulateConsignment(),
				Submitter = PopulateSubmitter()
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

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5BB };
		}

		DeclarationVersionIdType PopulateVersionID()
		{
			return new DeclarationVersionIdType { Value = amendmentProvider.AmendmentVersionNo.ToString() };
		}

		DeclarationTransactionNatureCodeType PopulateTransactionNatureCode()
		{
			return new DeclarationTransactionNatureCodeType { Value = amendmentProvider.AmendmentType };
		}

		DeclarationReasonTextType PopulateReason()
		{
			return new DeclarationReasonTextType { Value = amendmentProvider.AmendReasonDescription };
		}

		Collection<DeclarationConsignment> PopulateConsignment()
		{
			var entryLines = dataProvider.EntryLines;

			var amendmentType = amendmentProvider.AmendmentType;
			var consignmentList = new Collection<DeclarationConsignment>();
			if (entryLines.Any())
			{
				foreach (var lineNo in entryLines.Select(x => x.EntryLineNo).Distinct())
				{
					var consignment = new DeclarationConsignment { SequenceNumeric = lineNo };
					var elementsWithSameLineNo = entryLines.Where(x => x.EntryLineNo == lineNo);

					if (amendmentType == _5BBAmendmentType.Codes.Update)
					{
						consignment.Amendment = PopulateAmendment(elementsWithSameLineNo);
					}
					else if (amendmentType == _5BBAmendmentType.Codes.Add)
					{
						consignment.ConsignmentItem = PopulateConsignmentItem(elementsWithSameLineNo);
					}
					consignmentList.Add(consignment);
				}
			}
			else
			{
				var consignment = new DeclarationConsignment { SequenceNumeric = ZDecimal.Zero };
				consignmentList.Add(consignment);
			}
			return consignmentList;
		}

		Collection<DeclarationConsignmentAmendment> PopulateAmendment(IEnumerable<IImport5BBLine> entryLines)
		{
			var amendmentList = new Collection<DeclarationConsignmentAmendment>();
			foreach (var line in entryLines)
			{
				var amendment = new DeclarationConsignmentAmendment
				{
					StatementDescription = new AmendmentStatementDescriptionTextType { Value = line.BeforeDescription },
					AdjustmentDescription = new AmendmentAdjustmentDescriptionTextType { Value = line.AfterDescription },
					Pointer = new DeclarationConsignmentAmendmentPointer
					{
						TagId = new PointerTagIdType { Value = line.AmendDataItemID }
					}
				};
				amendmentList.Add(amendment);
			}
			return amendmentList;
		}

		Collection<DeclarationConsignmentConsignmentItem> PopulateConsignmentItem(IEnumerable<IImport5BBLine> entryLines)
		{
			var consignmentItemList = new Collection<DeclarationConsignmentConsignmentItem>();
			foreach (var line in entryLines)
			{
				var consignmentItem = new DeclarationConsignmentConsignmentItem
				{
					Commodity = new DeclarationConsignmentConsignmentItemCommodity
					{
						CargoDescription = new CommodityCargoDescriptionTextType { Value = line.InvoiceDescription },
						Description = new CommodityDescriptionTextType { Value = line.HSDescription },
						Classification = new DeclarationConsignmentConsignmentItemCommodityClassification
						{
							Id = new ClassificationIdentificationIdType { Value = line.HSCode },
						},
						DutyTaxFee = new DeclarationConsignmentConsignmentItemCommodityDutyTaxFee
						{
							TaxRateNumeric = dataProvider.TariffRate,
							TypeCode = new DutyTaxFeeTypeCodeType { Value = dataProvider.TariffRateClassification }
						}
					}
				};
				consignmentItemList.Add(consignmentItem);
			}
			return consignmentItemList;
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			return new DeclarationSubmitter
			{
				Name = new SubmitterNameTextType { Value = dataProvider.Declarant?.CompanyName ?? ZString.Empty }
			};
		}
	}
}
