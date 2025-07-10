using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class SupportingDocument : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument
	{
		public SupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument.Schema
		{
			public new const int CSI_CodeMaxLength = 4;
			public new const int CSI_ReferenceNumberMaxLength = 70;
			public new const int CSI_AdditionalDescriptionMaxLength = 70;
			public new const int CSI_ReferenceNumber2MaxLength = 70;
		}

		public const int CSI_ReferenceNumberMaxLength_AISUCC5 = 35;

		#region Properties

		[MaxLength(Schema.CSI_CodeMaxLength)]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		[ResourceStringData("IE.SupportingDocument.CSI_ReferenceNumber2", Caption = "Issuing Authority Name Role Code", ShortCaption = "Issuing Auth. Role Code")]
		[MaxLength(Schema.CSI_ReferenceNumber2MaxLength)]
		public override ZString CSI_ReferenceNumber2
		{
			get => base.CSI_ReferenceNumber2;
			set => base.CSI_ReferenceNumber2 = value;
		}

		[ResourceStringData("IE.SupportingDocument.CSI_AdditionalDescription", Caption = "Issuing Authority Name", ShortCaption = "Authority Name")]
		[MaxLength(Schema.CSI_AdditionalDescriptionMaxLength)]
		public override ZString CSI_AdditionalDescription
		{
			get => base.CSI_AdditionalDescription;
			set => base.CSI_AdditionalDescription = value;
		}

		[ResourceStringData("IE.SupportingDocument.CSI_DateOfExpiry", Caption = "Date of Validity")]
		public override ZDateTime CSI_DateOfExpiry
		{
			get => base.CSI_DateOfExpiry;
			set => base.CSI_DateOfExpiry = value;
		}

		[ResourceStringData("IE.SupportingDocument.CSI_Value", Caption = "Amount")]
		public override ZDecimal CSI_Value
		{
			get => base.CSI_Value;
			set => base.CSI_Value = value;
		}

		public override ZDecimal CSI_Quantity
		{
			get => base.CSI_Quantity;
			set
			{
				var oldValue = CSI_Quantity;
				base.CSI_Quantity = value;

				if (!IsCopying && oldValue != CSI_Quantity && !IsValidationSuspended)
				{
					Validation.ValidateCSI_UnitOfQuantity();
				}
			}
		}

		public override ZString UnitOfQuantityFieldType => nameof(FieldType.TextDropEdit);

		public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new SupportingDocumentLookups(this);

		public new CommonSupportingDocumentValidation Validation => (CommonSupportingDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			CusSupportingInfoValidation result;

			if (ImportExportParent is ICanBeImportOrExport importExportParent)
			{
				if (importExportParent.IsExport)
				{
					result = ParentIsJobComInvoiceLine ? new InvoiceLineExportSupportingDocumentValidation(this) : new ExportSupportingDocumentValidation(this);
				}
				else if (importExportParent.IsImport)
				{
					result = ParentIsJobComInvoiceLine ? new InvoiceLineImportSupportingDocumentValidation(this) : new ImportSupportingDocumentValidation(this);
				}
				else
				{
					result = new CommonSupportingDocumentValidation(this);
				}
			}
			else
			{
				result = new CommonSupportingDocumentValidation(this);
			}

			return result;
		}

		#endregion
	}
}
