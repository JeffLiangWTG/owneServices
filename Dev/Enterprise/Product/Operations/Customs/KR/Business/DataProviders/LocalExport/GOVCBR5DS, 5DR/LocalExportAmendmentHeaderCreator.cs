using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportAmendmentHeaderCreator
	{
		public LocalExportAmendEntryHeader Create(CusEntryHeader entry, AmendedItem[] amendedItems)
		{
			var result = new LocalExportAmendEntryHeader();
			var declaration = entry.Declaration;
			result.DeclarationCustomsOfficeAndDivision = declaration.JE_CustomsOffice + declaration.JE_CustomsDivision;
			result.CustomsReceiptNumber = entry.CH_BGMReference;
			result.DeclarationNumber = EDIMessage.EntryNumberPlaceHolder;

			if (entry.Declaration.Supplier != null)
			{
				result.Supplier = new Organisation(RoleType.Supplier);
				var idNumbers = entry.Declaration.Supplier.GetRegistrationIDNumbers(new string[] { Constants.IdentificationType.BusinessRegNo, Constants.IdentificationType.UnipassIDForOrganization });
				result.Supplier.SetRegistrationIDNumbers(idNumbers);
			}

			result.AmendedItems = GetLocalExportLines(amendedItems);
			return result;
		}

		public LocalExportAmendEntryHeader Create(CargoWise.Customs.KR.MessageDefinitions.GOVCBR5DR.Declaration declaration5DR)
		{
			var result = new LocalExportAmendEntryHeader();
			result.DeclarationCustomsOfficeAndDivision = declaration5DR.DeclarationOfficeId.Value;
			result.CustomsReceiptNumber = declaration5DR.AdditionalDocument.Id.Value;
			result.DeclarationNumber = declaration5DR.Id.Value;
			var regNo = declaration5DR.Submitter?.SingleOrDefault(x => x.SchemeAgencyId == AgencyIdentificationCodeContentType.Ktx)?.Value ?? ZString.Empty;
			var unipassID = declaration5DR.Submitter?.SingleOrDefault(x => x.SchemeAgencyId == AgencyIdentificationCodeContentType.Item380)?.Value ?? ZString.Empty;
			result.Supplier = GetSupplier(regNo, unipassID);
			var amendItems = new List<LocalExportAmendItem>();
			if (declaration5DR.AmendmentSpecified)
			{
				foreach (var amendment in declaration5DR.Amendment)
				{
					amendItems.Add(CreateLocalExportLine
					(
						amendment.ChangeReasonCode.Value,
						(int)amendment.Pointer.SequenceNumeric,
						amendment.Pointer.TagId?.Value ?? ZString.Empty,
						amendment.StatementDescription?.Value ?? ZString.Empty,
						amendment.AdjustmentDescription?.Value ?? ZString.Empty
					));
				}
			}
			result.AmendedItems = amendItems.ToArray();
			return result;
		}
		public LocalExportAmendEntryHeader Create(CargoWise.Customs.KR.MessageDefinitions.GOVCBR5DS.Declaration declaration5DS)
		{
			var result = new LocalExportAmendEntryHeader();
			result.DeclarationCustomsOfficeAndDivision = declaration5DS.DeclarationOfficeId.Value;
			result.CustomsReceiptNumber = declaration5DS.AdditionalDocument.Id.Value;
			result.DeclarationNumber = declaration5DS.Id.Value;
			var regNo = declaration5DS.Submitter?.SingleOrDefault(x => x.SchemeAgencyId == AgencyIdentificationCodeContentType.Ktx)?.Value ?? ZString.Empty;
			var unipassID = declaration5DS.Submitter?.SingleOrDefault(x => x.SchemeAgencyId == AgencyIdentificationCodeContentType.Item380)?.Value ?? ZString.Empty;
			result.Supplier = GetSupplier(regNo, unipassID);
			var amendItems = new List<LocalExportAmendItem>();
			if (declaration5DS.AmendmentSpecified)
			{
				foreach (var amendment in declaration5DS.Amendment)
				{
					amendItems.Add(CreateLocalExportLine
					(
						amendment.ChangeReasonCode.Value,
						(int)amendment.Pointer.SequenceNumeric,
						amendment.Pointer.TagId?.Value ?? ZString.Empty,
						amendment.StatementDescription?.Value ?? ZString.Empty,
						amendment.AdjustmentDescription?.Value ?? ZString.Empty
					));
				}
			}
			result.AmendedItems = amendItems.ToArray();
			return result;
		}

		LocalExportAmendItem[] GetLocalExportLines(AmendedItem[] amendedItems)
		{
			var result = new List<LocalExportAmendItem>();
			foreach (AmendedItem amended in amendedItems)
			{
				result.Add(CreateLocalExportLine(amended));
			}
			return result.Count > 0 ? result.OrderBy(x => x.ItemSequenceNumber).ToArray() : null;
		}

		LocalExportAmendItem CreateLocalExportLine(AmendedItem item)
		{
			var result = new LocalExportAmendItem();
			result.DataItemNo = item.DataItemID;
			result.AmendType = AmendTypeCodeList.LocalExportAmendTypeForMessage(item.AmendType);
			SetIDValue(result, item);
			result.BeforeValue = item.BeforeValue;
			result.AfterValue = item.AfterValue;

			return result;
		}
		LocalExportAmendItem CreateLocalExportLine(string amendType, int itemSequenceNum, string dataItemNo, string beforeValue, string afterValue)
		{
			return new LocalExportAmendItem
			{
				AmendType = amendType,
				ItemSequenceNumber = itemSequenceNum,
				DataItemNo = dataItemNo,
				BeforeValue = beforeValue,
				AfterValue = afterValue
			};
		}

		Organisation GetSupplier(string regNo, string unipassID)
		{
			var result = new Organisation(RoleType.Supplier);
			result.BusinessRegNo = regNo;
			result.UnipassIDForOrganization = unipassID;
			return result;
		}

		void SetIDValue(LocalExportAmendItem localExportAmendItem, AmendedItem amendedItem)
		{
			if (amendedItem.IDsInList != null)
			{
				foreach (var id in amendedItem.IDsInList)
				{
					if (id.IDType == nameof(ILocalExportEntryLine) || id.IDType == nameof(ILocalExportOtherTransportMeans))
					{
						localExportAmendItem.ItemSequenceNumber = ZInt.ParseEmptyAsZero(id.IDValue);
					}
				}
			}
		}
	}
}

