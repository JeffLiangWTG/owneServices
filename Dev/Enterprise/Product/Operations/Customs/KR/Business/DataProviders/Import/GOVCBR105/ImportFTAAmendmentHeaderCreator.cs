using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class ImportFTAAmendmentHeaderCreator : ImportFTACoreHeaderCreator<ImportFTAAmendmentHeader, ImportFTALine>
	{
		public ImportFTAAmendmentHeader Create(CusEntryHeader entry, AmendedItem[] amendedItems)
		{
			var result = base.Create(entry);
			var declaration = entry.Declaration;
			if (!entry.CH_EntryReleaseDate.IsEmpty && entry.CH_EntryReleaseDate.IsValid)
			{
				result.EntryReleaseDate = entry.CH_EntryReleaseDate.ToDateTime();
			}
			result.DeclarationCustomsOffice = declaration.JE_CustomsOffice;
			result.DeclarationCustomsDivision = declaration.JE_CustomsDivision;
			var brokerAddress = declaration.BrokerAddress;
			if (brokerAddress != null)
			{
				result.Declarant = new Organisation(RoleType.Declarant)
				{
					CompanyName = brokerAddress.CompanyName,
					RepresentativeName = brokerAddress.Header.GetRepresentativeName()
				};
			}
			result.Items = GetImportFTAAmendmentLines(amendedItems);

			return result;
		}

		ImportFTAAmendmentItem[] GetImportFTAAmendmentLines(AmendedItem[] amendedItems)
		{
			var lines = new List<ImportFTAAmendmentItem>();
			foreach (AmendedItem amendedItem in amendedItems)
			{
				if (amendedItem.AmendType != EntityAmendType.NoChange)
				{
					var result = new ImportFTAAmendmentItem
					{
						DataItemID = amendedItem.DataItemID,
						AmendType = amendedItem.EntityType == nameof(IImportFTALine)
										? AmendTypeCodeList.ImportFTAAmendTypeForItem(amendedItem.AmendType) : ZString.Empty,
						BeforeDescription = amendedItem.BeforeValue,
						AfterDescription = amendedItem.AfterValue
					};
					SetIDValue(result, amendedItem);
					lines.Add(result);
				}
			}
			return lines.OrderBy(x => x.EntryLineNo).ToArray();
		}

		static void SetIDValue(ImportFTAAmendmentItem ftaItem, AmendedItem amendedItem)
		{
			if (amendedItem.IDsInList != null)
			{
				foreach (var id in amendedItem.IDsInList)
				{
					if (id.IDType == nameof(IImportFTALine))
					{
						ftaItem.EntryLineNo = ZInt.ParseEmptyAsZero(id.IDValue);
					}
				}
			}
		}

		public ImportFTAAmendmentHeader Create(CargoWise.Customs.KR.MessageDefinitions.GOVCBR105.Declaration declaration)
		{
			var result = new ImportFTAAmendmentHeader();
			
			if (declaration.DeclarationOfficeId.Value?.Length == 5)
			{
				result.DeclarationCustomsOffice = declaration.DeclarationOfficeId.Value.Substring(0, 3);
				result.DeclarationCustomsDivision = declaration.DeclarationOfficeId.Value.Substring(3, 2);
			}
			result.ImportDeclarationNumber = declaration.Id.Value;
			if (declaration.AuthenticationDateTime != null)
			{
				if (DateTime.TryParseExact(declaration.AuthenticationDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
				{
					result.EntryReleaseDate = dt;
				}
			}
			if(declaration.AdditionalDocument != null)
			{
				result.LawCode = declaration.AdditionalDocument.TypeCode?.Value ?? ZString.Empty;
				result.StatementNumber5WN = declaration.AdditionalDocument.Id?.Value ?? ZString.Empty;
			}

			if (declaration.Consignment != null)
			{
				var importFTAAmendmentItems = new List<ImportFTAAmendmentItem>();
				foreach (var consignment in declaration.Consignment)
				{
					var importFTAAmendmentItem = new ImportFTAAmendmentItem();
					importFTAAmendmentItem.EntryLineNo = consignment.SequenceNumeric.HasValue ? (int)consignment.SequenceNumeric.Value : 0;
					importFTAAmendmentItem.SequenceNo = (int)consignment.AdditionalDocument.SequenceNumeric;
					importFTAAmendmentItem.AmendType = consignment.AdditionalInformation?.StatementCode.Value ?? string.Empty;
					importFTAAmendmentItem.BeforeDescription = consignment.Amendment?.StatementDescription?.Value ?? string.Empty;
					importFTAAmendmentItem.AfterDescription = consignment.Amendment?.AdjustmentDescription?.Value ?? string.Empty;
					importFTAAmendmentItem.DataItemID = consignment.Amendment?.Pointer?.TagId?.Value ?? string.Empty;
					importFTAAmendmentItems.Add(importFTAAmendmentItem);
				}
				result.Items = importFTAAmendmentItems.ToArray();
			}

			var importer = new Organisation(RoleType.Importer);
			var roleCode = declaration.Importer.RoleCode.Value;
			switch (roleCode)
			{
				case Constants.IdentificationType.KoreanRegNoForResident:
					importer.KoreanRegNoForResident = declaration.Importer.Id[0].Value;
					break;
				case Constants.IdentificationType.PassportNo:
					importer.PassportNo = declaration.Importer.Id[0].Value;
					break;
				case Constants.IdentificationType.KoreanRegNoForForeigner:
					importer.KoreanRegNoForForeigner = declaration.Importer.Id[0].Value;
					break;
				case Constants.IdentificationType.UnipassIDForIndividual:
					importer.UnipassIDForIndividual = declaration.Importer.Id[0].Value;
					break;
				case Constants.IdentificationType.BusinessRegNo:
					importer.BusinessRegNo = declaration.Importer.Id[0].Value;
					break;
			}
			importer.UnipassIDForOrganization = declaration.Importer.Id[1].Value;
			importer.CompanyName = declaration.Importer.Name.Value;
			importer.RoadNameCode = declaration.Importer.Address.CountrySubDivisionId?.Value ?? ZString.Empty;
			importer.AddressLine2 = declaration.Importer.Address.Line?.Value ?? ZString.Empty;
			importer.Postcode = declaration.Importer.Address.PostcodeId.Value;
			importer.BuildingNumber = declaration.Importer.Address.BuildingNumber?.Value ?? ZString.Empty;
			importer.AddressLine1 = declaration.Importer.Address.Description.Value;
			importer.RepresentativeName = declaration.Importer.Contact.Name.Value;
			importer.PhoneNumber = declaration.Importer.Communication[0].Id.Value;
			importer.FaxNumber = declaration.Importer.Communication[1].Id.Value;
			importer.Email = declaration.Importer.Communication[2].Id.Value;
			result.Importer = importer;

			var declarant = new Organisation(RoleType.Declarant);
			declarant.CompanyName = declaration.Submitter.Name.Value;
			declarant.RepresentativeName = declaration.Submitter.Contact.Name.Value;
			result.Declarant = declarant;

			return result;
		}

		public ImportFTAAmendmentHeader Create(CargoWise.Customs.KR.MessageDefinitions.GOVCBRDHS.Declaration declaration)
		{
			var result = new ImportFTAAmendmentHeader();

			if (declaration.DeclarationOfficeId.Value?.Length == 5)
			{
				result.DeclarationCustomsOffice = declaration.DeclarationOfficeId.Value.Substring(0, 3);
				result.DeclarationCustomsDivision = declaration.DeclarationOfficeId.Value.Substring(3, 2);
			}
			result.ImportDeclarationNumber = declaration.Id.Value;

			if (declaration.AuthenticationDateTime != null)
			{
				if (DateTime.TryParseExact(declaration.AuthenticationDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
				{
					result.EntryReleaseDate = dt;
				}
			}
			result.LawCode = declaration.AdditionalDocument?.TypeCode?.Value ?? ZString.Empty;
			result.StatementNumber5WN = declaration.AdditionalDocument?.Id?.Value ?? ZString.Empty;

			if (declaration.Consignment != null)
			{
				var importFTAAmendmentItems = new List<ImportFTAAmendmentItem>();
				foreach (var consignment in declaration.Consignment)
				{
					var importFTAAmendmentItem = new ImportFTAAmendmentItem();
					importFTAAmendmentItem.SequenceNo = (int)consignment.SequenceNumeric;
					importFTAAmendmentItem.AmendType = consignment.AdditionalInformation?.StatementCode.Value ?? ZString.Empty;
					importFTAAmendmentItem.BeforeDescription = consignment.Amendment?.StatementDescription?.Value ?? ZString.Empty;
					importFTAAmendmentItem.AfterDescription = consignment.Amendment?.AdjustmentDescription?.Value ?? ZString.Empty;
					importFTAAmendmentItem.DataItemID = consignment.Amendment?.Pointer?.TagId?.Value ?? ZString.Empty;
					importFTAAmendmentItem.InvoiceLineNo = ZInt.ParseSafe(consignment.ConsignmentItem?.Commodity?.IdentityQualifierCode?.Value ?? ZString.Empty, 0);
					importFTAAmendmentItem.EntryLineNo = ZInt.ParseSafe(consignment.ConsignmentItem?.Commodity?.SequenceId?.Value ?? ZString.Empty, 0);
					importFTAAmendmentItems.Add(importFTAAmendmentItem);
				}
				result.Items = importFTAAmendmentItems.ToArray();
			}

			var importer = new Organisation(RoleType.Importer);
			var roleCode = declaration.Importer.RoleCode.Value;
			switch (roleCode)
			{
				case Constants.IdentificationType.KoreanRegNoForResident:
					importer.KoreanRegNoForResident = declaration.Importer.Id[0].Value;
					break;
				case Constants.IdentificationType.PassportNo:
					importer.PassportNo = declaration.Importer.Id[0].Value;
					break;
				case Constants.IdentificationType.KoreanRegNoForForeigner:
					importer.KoreanRegNoForForeigner = declaration.Importer.Id[0].Value;
					break;
				case Constants.IdentificationType.UnipassIDForIndividual:
					importer.UnipassIDForIndividual = declaration.Importer.Id[0].Value;
					break;
				case Constants.IdentificationType.BusinessRegNo:
					importer.BusinessRegNo = declaration.Importer.Id[0].Value;
					break;
			}
			if (declaration.Importer.Id.Count == 2)
			{
				importer.UnipassIDForOrganization = declaration.Importer.Id[1].Value;
			}

			importer.CompanyName = declaration.Importer.Name.Value;
			importer.RoadNameCode = declaration.Importer.Address.CountrySubDivisionId?.Value ?? ZString.Empty;
			importer.AddressLine2 = declaration.Importer.Address.Line?.Value ?? ZString.Empty;
			importer.Postcode = declaration.Importer.Address.PostcodeId.Value;
			importer.BuildingNumber = declaration.Importer.Address.BuildingNumber?.Value ?? ZString.Empty;
			importer.AddressLine1 = declaration.Importer.Address.Description.Value;
			importer.RepresentativeName = declaration.Importer.Contact.Name.Value;
			importer.PhoneNumber = declaration.Importer.Communication[0].Id.Value;
			importer.FaxNumber = declaration.Importer.Communication[1].Id.Value;
			importer.Email = declaration.Importer.Communication[2].Id.Value;
			result.Importer = importer;

			var declarant = new Organisation(RoleType.Declarant);
			declarant.CompanyName = declaration.Submitter.Name.Value;
			declarant.RepresentativeName = declaration.Submitter.Contact.Name.Value;
			result.Declarant = declarant;

			return result;
		}
	}
}
