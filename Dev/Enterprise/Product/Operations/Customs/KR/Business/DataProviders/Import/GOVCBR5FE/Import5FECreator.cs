using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class Import5FECreator
	{
		public Import5FEHeader Create(CusEntryHeader entry, JobDeclarationAmendmentMessageSendingObject sendingObject)
		{
			var entryHeaderData = new Import5FEHeader();
			var declaration = entry.Declaration;
			var entryNum = entry.CusEntryNumber;
			if (entryNum != null)
			{
				entryHeaderData.ImportDeclarationNumber = entryNum.CE_EntryNum;
				if (!entryNum.CE_IssueDate.IsEmpty)
				{
					entryHeaderData.DeclarationDate = entryNum.CE_IssueDate.ToDateTime();
				}
			}
			entryHeaderData.DeclarationCustomsOffice = declaration.JE_CustomsOffice;
			entryHeaderData.DeclarationCustomsDivision = declaration.JE_CustomsDivision;
			entryHeaderData.DeclarantType = declaration.IsSelfDeclaringOwner ? Constants.DeclarantType.GOVCBR5FE.Importer : Constants.DeclarantType.GOVCBR5FE.Broker;
			entryHeaderData.DutyPenaltyType = TaxPenaltyTypeCodeList.GetCustomsPenaltyTypeByCW1(sendingObject.DutyPenaltyCause);
			entryHeaderData.DomesticTaxPenaltyType = TaxPenaltyTypeCodeList.GetCustomsPenaltyTypeByCW1(sendingObject.TaxPenaltyCause);
			entryHeaderData.DutyPenaltyReducedYN = sendingObject.ApplyDutyPenaltyReduction;
			entryHeaderData.PenaltyExemptionIndicator = sendingObject.PenaltyExemptionIndicator;
			entryHeaderData.PenaltyExemptionReasonCode = sendingObject.PenaltyExemptionReasonCode;
			entryHeaderData.PenaltyExemptionReason = sendingObject.PenaltyExemptionReason;
			entryHeaderData.PenaltyExemptionReqSequence = sendingObject.PenaltyExemptionReqSequence;
			entryHeaderData.PenaltyExemptionAmount = sendingObject.PenaltyExemptionAmount;
			entryHeaderData.RefundRequestNumber = sendingObject.RefundRequestNumber;
			PopulateOrganisationData(entryHeaderData, declaration);

			entryHeaderData.TaxItems = PopulateTaxItemsData(entryHeaderData, sendingObject.AmendedDutyTaxItems);
			PopulateSnapShotData(entryHeaderData, entry);
			PopulateCurrentData(entryHeaderData, entry);

			entryHeaderData.DutyTaxDifference = entryHeaderData.AfterTotalDutyTaxAmount - entryHeaderData.BeforeTotalDutyTaxAmount;
			entryHeaderData.CustomsValueDifference = entryHeaderData.AfterCustomsValue - entryHeaderData.BeforeCustomsValue;
			entryHeaderData.AmendedItems = Get5FELines(sendingObject.AmendedItems);
			return entryHeaderData;
		}

		Import5FEItem[] Get5FELines(AmendedItemCollection amendedItems)
		{
			var result = new List<Import5FEItem>();
			foreach (AmendedItem amendedItem in amendedItems)
			{
				if (amendedItem.AmendType != EntityAmendType.NoChange)
				{
					result.Add(CreateImport5FELine(amendedItem));
				}
			}
			return result.OrderBy(x => x.EntryLineNo).ToArray();
		}

		Import5FEItem CreateImport5FELine(AmendedItem item)
		{
			var result = new Import5FEItem
			{
				AmendDataItemID = item.DataItemID,
				AmendType = IsEntryLineOrItsChildElement(item.EntityType) ? AmendTypeCodeList.AmendTypeForMessage(item.AmendType) : ZString.Empty,
				BeforeDescription = item.BeforeValue,
				AfterDescription = item.AfterValue
			};
			SetIDValue(result, item);
			return result;
		}

		public Import5FEHeader Create(CargoWise.Customs.KR.MessageDefinitions.GOVCBR5FE.Declaration declaration)
		{
			var entryHeaderData = new Import5FEHeader();
			entryHeaderData.ImportDeclarationNumber = declaration.Id.Value;
			entryHeaderData.DeclarationDate = DateTime.ParseExact(declaration.IssueDateTime, DateFormatType.Date, System.Globalization.CultureInfo.InvariantCulture)
			;
			if (declaration.DeclarationOfficeId.Value?.Length == 5)
			{
				entryHeaderData.DeclarationCustomsOffice = declaration.DeclarationOfficeId.Value.Substring(0, 3);
				entryHeaderData.DeclarationCustomsDivision = declaration.DeclarationOfficeId.Value.Substring(3, 2);
			}
			entryHeaderData.DeclarantType = declaration.Submitter.RoleCode.Value;
			entryHeaderData.DutyPenaltyType = declaration.GovernmentInformation?.TypeCode?.Value ?? ZString.Empty;
			entryHeaderData.DomesticTaxPenaltyType = declaration.GoodsShipment?.Consignment?.DutyTaxFee?.AdditionalTaxTypeCode?.Value ?? ZString.Empty;
			entryHeaderData.DutyPenaltyReducedYN = declaration.GoodsShipment?.Consignment?.DutyTaxFee?.AdditionalTaxReductionCode?.Value ?? ZString.Empty;
			entryHeaderData.PenaltyExemptionIndicator = declaration.AdditionalInformation.StatementCode.Value;
			entryHeaderData.PenaltyExemptionReasonCode = declaration.AdditionalInformation.StatementTypeCode?.Value ?? ZString.Empty;
			entryHeaderData.PenaltyExemptionReason = declaration.AdditionalInformation.Content?.Value ?? ZString.Empty;
			entryHeaderData.PenaltyExemptionReqSequence = ZInt.ParseSafe(declaration.AdditionalInformation.StatementDescription?.Value, 0);
			entryHeaderData.PenaltyExemptionAmount = declaration.AdditionalDocument?.AmountAmount?.Value ?? ZDecimal.Zero;
			entryHeaderData.RefundRequestNumber = declaration.AdditionalDocument?.Id?.Value ?? ZString.Empty;

			entryHeaderData.Declarant = new Organisation(RoleType.Declarant)
			{
				CompanyName = declaration.Submitter.Name.Value,
				RepresentativeName = declaration.Submitter.Contact.Name.Value
			};
			entryHeaderData.Payer = new Organisation(RoleType.Payer)
			{
				CompanyName = declaration.Payer.Name.Value,
				RepresentativeName = declaration.Payer.Contact.Name.Value
			};

			entryHeaderData.TotalAmendedItemCount = (int)declaration.GoodsItemQuantity.Value;
			entryHeaderData.TotalAmendedTaxCount = (int)(declaration.LoadingListQuantity?.Value ?? 0);

			var beforeValues = declaration.GoodsShipment?.DutyTaxFee?.FirstOrDefault(x => x.RequestOverrideCode?.Value == RequestOverrideCode.Before);
			var afterValues = declaration.GoodsShipment?.DutyTaxFee?.FirstOrDefault(x => x.RequestOverrideCode?.Value == RequestOverrideCode.After);
			entryHeaderData.BeforeTotalDutyTaxAmount = beforeValues?.Payment?.TaxAssessedAmount?.Value ?? 0;
			entryHeaderData.BeforeCustomsValue = beforeValues?.Payment?.AssessmentAmount?.Value ?? 0;
			entryHeaderData.AfterTotalDutyTaxAmount = afterValues?.Payment?.TaxAssessedAmount?.Value ?? 0;
			entryHeaderData.AfterCustomsValue = afterValues?.Payment?.AssessmentAmount?.Value ?? 0;
			entryHeaderData.DutyTaxDifference = declaration.GoodsShipment?.Consignment?.DutyTaxFee?.TotalTaxAmount?.Value ?? 0;
			entryHeaderData.CustomsValueDifference = declaration.GoodsShipment?.Consignment?.DutyTaxFee?.AdValoremTaxBaseAmount?.Value ?? 0;
			return entryHeaderData;
		}

		static void SetIDValue(Import5FEItem import5FEItem, AmendedItem amendedItem)
		{
			if (amendedItem.IDsInList != null)
			{
				foreach (var id in amendedItem.IDsInList)
				{
					var value = HasNumericID(id.IDType) ? ZInt.ParseEmptyAsZero(id.IDValue) : ZInt.Zero;
					switch (id.IDType)
					{
						case nameof(IImportEntryLine):
							import5FEItem.EntryLineNo = value;
							break;
						case nameof(IImportInvoiceLine):
							import5FEItem.InvoiceLineNo = value;
							break;
						case nameof(IImportNonGADetail):
							import5FEItem.NonGASequnceNo = value;
							break;
						case nameof(IImportContainer):
							import5FEItem.ContainerNo = value;
							break;
						case nameof(IImportImmediateDelivery):
							import5FEItem.ImmediateDeliveryNo = value;
							break;
						case nameof(IImportOnlineOrder):
							import5FEItem.OnlineOrderNo = value;
							break;
						case nameof(IImportPreviousExpDecLine):
							import5FEItem.ExportDeclarationNumber = id.IDValue;
							break;
							//These cases will be implemented in WI00696133 
							//	case "IImportPreviousExpDecLine.EntryLineNo":
							//		import5FEItem.ExportDeclarationEntryLineNo = value;
							//		break;
							//	case "IImportPreviousExpDecLine.InvoiceLineNo":
							//		import5FEItem.ExportDeclarationInvoiceLineNo = value;
							//		break;
							//	case nameof(IImportGAApprovalDocument):
							//		import5FEItem.GARequirementApprovalNumber = id.IDValue;
							//		break;
					}
				}
			}
		}

		static bool IsEntryLineOrItsChildElement(string entityType) =>
									entityType == nameof(IImportEntryLine)
									|| entityType == nameof(IImportInvoiceLine)
									|| entityType == nameof(IImportGAApprovalDocument)
									|| entityType == nameof(IImportNonGADetail)
									|| entityType == nameof(IImportImmediateDelivery)
									|| entityType == nameof(IImportPreviousExpDecLine);

		static bool HasNumericID(string typeName)
		{
			return typeName == nameof(IImportEntryLine)
				|| typeName == nameof(IImportInvoiceLine)
				|| typeName == nameof(IImportNonGADetail)
				|| typeName == nameof(IImportContainer)
				|| typeName == nameof(IImportImmediateDelivery)
				|| typeName == nameof(IImportOnlineOrder);
		}

		void PopulateOrganisationData(Import5FEHeader entryHeaderData, JobDeclaration declaration)
		{
			if (declaration.BrokerAddress != null)
			{
				entryHeaderData.Declarant = new Organisation(RoleType.Declarant)
				{
					CompanyName = declaration.BrokerAddress.CompanyName,
					RepresentativeName = declaration.BrokerAddress.Header.GetRepresentativeName()
				};
			}
			if (declaration.DutyPayer != null)
			{
				entryHeaderData.Payer = new Organisation(RoleType.Payer)
				{
					CompanyName = declaration.DutyPayer.OH_FullName,
					RepresentativeName = declaration.DutyPayer.GetRepresentativeName()
				};
			}
		}

		Import5FETaxItem[] PopulateTaxItemsData(Import5FEHeader entryHeaderData, AmendedItemCollection amendedDutyTaxItems)
		{
			var result = new List<Import5FETaxItem>();
			foreach (AmendedItem item in amendedDutyTaxItems)
			{
				result.Add(new Import5FETaxItem
				{
					DutyTaxType = item.DutyTaxType,
					BeforeAmount = item.BeforeAmount,
					AfterAmount = item.AfterAmount,
					AmountDifference = item.AmountDifference
				});
			}
			return result.ToArray();
		}

		void PopulateSnapShotData(Import5FEHeader entryHeaderData, CusEntryHeader entry)
		{
			var snapShot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			if (snapShot != null)
			{
				using (var textReader = snapShot.GetCES_SnapshotXmlReader())
				{
					var snapShotData = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryHeader>(textReader);
					entryHeaderData.BeforeTotalDutyTaxAmount = snapShotData.TotalPayableAmount;
					entryHeaderData.BeforeCustomsValue = snapShotData.TotalCustomsValueKRW;
				}
			}
		}

		void PopulateCurrentData(Import5FEHeader entryHeaderdata, CusEntryHeader entry)
		{
			entryHeaderdata.AfterTotalDutyTaxAmount = entry.TotalAmountPayable;
			entryHeaderdata.AfterCustomsValue = entry.CustomsValue;
		}
	}
}
