using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos
{
	public class SupportingDocumentValueSetStrategy : IValueSetStrategy
	{
		public SupportingDocumentValueSetStrategy(SupportingDocument supportingDocument)
		{
			this.supportingDocument = supportingDocument;
		}

		void IValueSetStrategy.ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case SupportingDocument.Schema.CSI_Code:
					HandleSettingOfCSI_Code(valueThatHasChanged);
					break;
			}
		}

		void HandleSettingOfCSI_Code(ZPropertyInfo valueThatHasChanged)
		{
			var itemDefaults = GBCustomsDataRegistry.Instance.ItemDefaults.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			if (supportingDocument.Parent != null)
			{
				foreach (var supportingDocumentDefaults in (from ItemDefaulterSetting ids in itemDefaults
															where ids.SourceType == SourceTypesList.Codes.SupportingDocumentBox44 && ids.SourceValue == valueThatHasChanged.Value.ToString()
															select ids))
				{
					switch (supportingDocumentDefaults.TargetType)
					{
						case TargetTypesList.Codes.SupportingDocumentBox44:
							AddSupportingDocument(supportingDocumentDefaults.TargetCode);
							break;
						case TargetTypesList.Codes.SetReferenceFromInvoiceNumber:
							SetSupportingDocumentReferenceNumber();
							break;
						case TargetTypesList.Codes.RegistrationNumberFromImporter:
							SetSupportingDocumentReferenceNumberFromImporter(supportingDocumentDefaults.TargetCode);
							break;
						case TargetTypesList.Codes.RegistrationNumberFromExporter:
							SetSupportingDocumentReferenceNumberFromSupplier(supportingDocumentDefaults.TargetCode);
							break;
					}
				}
			}
		}

		void AddSupportingDocument(string targetCode)
		{
			if (!supportingDocument.IsBeingCreatedByItemDefaulter)
			{
				var sDoc = supportingDocument.ParentIsJobComInvoiceHeader ? (supportingDocument.Parent as JobComInvoiceHeader).SupportingDocuments.AddNew()
					: (supportingDocument.ParentIsJobComInvoiceLine) ? (supportingDocument.Parent as JobComInvoiceLine).SupportingDocuments.AddNew() : null;

				if (sDoc != null)
				{
					sDoc.IsBeingCreatedByItemDefaulter = true;
					sDoc.CSI_Code = targetCode;
				}
			}
		}

		void SetSupportingDocumentReferenceNumber()
		{
			if (supportingDocument.ParentIsJobComInvoiceHeader)
			{
				supportingDocument.CSI_ReferenceNumber = (supportingDocument.Parent as JobComInvoiceHeader).JZ_InvoiceNumber;
			}
			else if (supportingDocument.ParentIsJobComInvoiceLine)
			{
				supportingDocument.CSI_ReferenceNumber =  (supportingDocument.Parent as JobComInvoiceLine).InvoiceHeader.JZ_InvoiceNumber;
			}
		}

		void SetSupportingDocumentReferenceNumberFromImporter(string code)
		{
			var invoiceHeader = supportingDocument.Parent as JobComInvoiceHeader;
			var invoiceLine = supportingDocument.Parent as JobComInvoiceLine;
			var importer = supportingDocument.ParentIsJobComInvoiceHeader ? invoiceHeader.Importer_Effective : supportingDocument.ParentIsJobComInvoiceLine ? invoiceLine.Importer : null;
			var isImport = supportingDocument.ParentIsJobComInvoiceHeader ? invoiceHeader.IsImport : supportingDocument.ParentIsJobComInvoiceLine ? invoiceLine.IsImport : ZBool.False;

			if (importer != null && isImport)
			{
				supportingDocument.CSI_ReferenceNumber = importer.CustomsCodes.GetCustomsRegNo(code, Core.Constants.CountryCodes.UnitedKingdom);
			}
		}

		void SetSupportingDocumentReferenceNumberFromSupplier(string code)
		{
			var invoiceHeader = supportingDocument.Parent as JobComInvoiceHeader;
			var invoiceLine = supportingDocument.Parent as JobComInvoiceLine;
			var supplier = supportingDocument.ParentIsJobComInvoiceHeader ? invoiceHeader.Supplier : supportingDocument.ParentIsJobComInvoiceLine ? invoiceLine.Supplier : null;
			var isExport = supportingDocument.ParentIsJobComInvoiceHeader ? invoiceHeader.IsExport : supportingDocument.ParentIsJobComInvoiceLine ? invoiceLine.IsExport : ZBool.False;

			if (supplier != null && isExport)
			{
				supportingDocument.CSI_ReferenceNumber = supplier.CustomsCodes.GetCustomsRegNo(code, Core.Constants.CountryCodes.UnitedKingdom);
			}
		}

		readonly SupportingDocument supportingDocument;
	}
}
