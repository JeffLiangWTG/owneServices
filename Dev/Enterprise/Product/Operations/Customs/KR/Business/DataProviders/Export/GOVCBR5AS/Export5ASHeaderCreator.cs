using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class Export5ASHeaderCreator
	{
		public ExportAmendmentHeader Create(CusEntryHeader entry, AmendedItem[] amendedItems)
		{
			var result = new ExportAmendmentHeader();
			var declaration = entry.Declaration;

			result.ExportDeclarationNumber = entry.EntryNumber;
			result.DeclarationCustomsOffice = declaration.JE_CustomsOffice;
			result.DeclarationCustomsDivision = declaration.JE_CustomsDivision;

			var supplier = entry.Declaration.SupplierAddress;

			if (supplier != null)
			{
				result.Exporter = new Organisation(RoleType.Exporter)
				{
					AddressLine1 = supplier.Address1,
					AddressLine2 = supplier.Address2,
					CompanyName = supplier.CompanyName
				};

				var idNumberAndTypes = supplier.GetRegistrationIDNumbers(new string[] { IdentificationType.UnipassIDForOrganization });
				result.Exporter.SetRegistrationIDNumbers(idNumberAndTypes);
			}

			result.UnipassDeclarantID = declaration.UNIPASSDeclarantID;
			result.AmendmentItems = Get5ASLines(amendedItems);

			return result;
		}

		Export5ASItem[] Get5ASLines(AmendedItem[] amendedItems)
		{
			var result = new List<Export5ASItem>();
			foreach (AmendedItem amendedItem in amendedItems)
			{
				if (amendedItem.AmendType != EntityAmendType.NoChange)
				{
					result.Add(CreateExport5ASLine(amendedItem));
				}
			}
			return result.OrderBy(x => x.EntryLineNo).ToArray();
		}

		Export5ASItem CreateExport5ASLine(AmendedItem item)
		{
			var result = new Export5ASItem();
			result.EntryLineNo = EmptyEntryLineNo;
			result.LineDetailNo = EmptyInvoiceLineNo;
			result.AmendDataItemID = item.DataItemID;
			result.LineAmendType = IsEntryLineOrItsChildElement(item.EntityType) ? AmendTypeCodeList.AmendTypeForMessage(item.AmendType) : ZString.Empty;
			SetIDValue(result, item);
			result.BeforeDescription = TransformDescription(result, item.BeforeValue, YesNo.No);
			result.AfterDescription = TransformDescription(result, item.AfterValue, YesNo.Yes);
			return result;
		}

		const string EmptyInvoiceLineNo = "00";
		public const string EmptyEntryLineNo = "000";

		static ZString TransformDescription(Export5ASItem item, ZString value, ZString valueOnDelete)
		{
			return item.LineAmendType == AmendTypeCodeList.Codes._02 ? valueOnDelete : value;
		}

		static bool IsEntryLineOrItsChildElement(string entityType) =>
									entityType == nameof(IExportEntryLine)
									|| entityType == nameof(IExportInvoiceLine)
									|| entityType == nameof(IExportGAApprovalDocument)
									|| entityType == nameof(IExportVehicleNo);

		static void SetIDValue(Export5ASItem export5ASItem, AmendedItem amendedItem)
		{
			if (amendedItem.IDsInList != null)
			{
				foreach (var id in amendedItem.IDsInList)
				{
					var value = HasNumericID(id.IDType) ? ZInt.ParseEmptyAsZero(id.IDValue) : ZInt.Zero;
					switch (id.IDType)
					{
						case nameof(IExportEntryLine):
							export5ASItem.EntryLineNo = value.ToString(NumberFormatDigit.D3);
							break;
						case nameof(IExportInvoiceLine):
							export5ASItem.LineDetailNo = value.ToString(NumberFormatDigit.D2);
							break;
						case nameof(IExportContainer):
							export5ASItem.ContainerSequenceNo = value.ToString(NumberFormatDigit.D2);
							break;
						case nameof(IExportVehicleNo):
							export5ASItem.VINSequenceNo = value.ToString(NumberFormatDigit.D3);
							break;
						case nameof(IExportGAApprovalDocument):
							export5ASItem.RegulationCategorySequnceNo = value.ToString(NumberFormatDigit.D2);
							break;
					}
				}
			}
		}
		static bool HasNumericID(string typeName)
		{
			return typeName == nameof(IExportEntryLine)
				|| typeName == nameof(IExportInvoiceLine)
				|| typeName == nameof(IExportContainer)
				|| typeName == nameof(IExportVehicleNo)
				|| typeName == nameof(IExportGAApprovalDocument);
		}

		public ExportAmendmentHeader Create(CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AS.Declaration declaration)
		{
			var result = new ExportAmendmentHeader();
			result.ExportDeclarationNumber = declaration.Id.Value;
			if (declaration.DeclarationOfficeId.Value?.Length == 5)
			{
				result.DeclarationCustomsOffice = declaration.DeclarationOfficeId.Value.Substring(0, 3);
				result.DeclarationCustomsDivision = declaration.DeclarationOfficeId.Value.Substring(3, 2);
			}
			result.UnipassDeclarantID = declaration.Submitter.Id.Value;

			if (declaration.Consignment != null)
			{
				var exportAmendmentItems = new List<Export5ASItem>();
				foreach (var consignment in declaration.Consignment)
				{
					var exportAmendmentItem = new Export5ASItem();
					if (consignment.AdditionalDocument != null)
					{
						exportAmendmentItem.RegulationCategorySequnceNo = consignment.AdditionalDocument.CriteriaConformanceId?.Value;
					}
					exportAmendmentItem.LineAmendType = consignment.Amendment.ChangeReasonCode?.Value;
					exportAmendmentItem.BeforeDescription = consignment.Amendment.StatementDescription?.Value;
					exportAmendmentItem.AfterDescription = consignment.Amendment.AdjustmentDescription?.Value;
					exportAmendmentItem.EntryLineNo = consignment.Amendment.Pointer.SequenceNumeric.Value;
					exportAmendmentItem.LineDetailNo = consignment.Amendment.Pointer.DocumentSectionCode.Value;
					exportAmendmentItem.AmendDataItemID = consignment.Amendment.Pointer.TagId.Value;

					if (consignment.ConsignmentItem?.Commodity != null)
					{
						exportAmendmentItem.VINSequenceNo = consignment.ConsignmentItem.Commodity.Id?.Value;
					}
					if (consignment.TransportEquipment != null)
					{
						exportAmendmentItem.ContainerSequenceNo = consignment.TransportEquipment.Id?.Value;
					}
					exportAmendmentItems.Add(exportAmendmentItem);
				}
				result.AmendmentItems = exportAmendmentItems.ToArray();
			}

			if (declaration.Exporter != null)
			{
				var exporter = new Organisation(RoleType.Exporter);
				exporter.CompanyName = declaration.Exporter.Name.Value;

				if (!string.IsNullOrEmpty(declaration.Exporter.Id?.Value ?? string.Empty))
				{
					exporter.UnipassIDForOrganization = declaration.Exporter.Id.Value;
				}
				result.Exporter = exporter;
			}

			return result;
		}
	}
}

