using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class AdditionalInfo : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
	{
		public new class Schema : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo.Schema
		{
			public new const int DescriptionMaxLength = 512;
		}
		public AdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AdditionalInfoValidation Validation => (AdditionalInfoValidation)base.Validation;

		public new AdditionalInfoLookups Lookups => (AdditionalInfoLookups)base.Lookups;

		public bool ParentIsInvoiceHeader => CSI_ParentTableCode == JobComInvoiceHeaderSchema.Constants.Prefix;

		public bool ParentIsInvoiceLine => CSI_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix;

		public bool ParentIsCusEntryInstruction => CSI_ParentTableCode == CusEntryInstructionSchema.Constants.Prefix;

		public bool ParentIsCusClassPartPivot => CSI_ParentTableCode == CusClassPartPivotSchema.Constants.Prefix;

		public JobComInvoiceLine InvoiceLine => Parent is JobComInvoiceLine invoiceLine ? invoiceLine : null;

		public JobComInvoiceHeader InvoiceHeader => Parent is JobComInvoiceHeader invoiceHeader ? invoiceHeader : null;

		public CusEntryInstruction EntryInstruction => Parent is CusEntryInstruction entryInstruction ? entryInstruction : null;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			if (Parent is ICanBeImportOrExport parent)
			{
				if (parent.IsExport)
				{
					return new ExportAdditionalInfoValidation(this);
				}

				if (parent.IsImport)
				{
					return new ImportAdditionalInfoValidation(this);
				}
			}

			return new AdditionalInfoValidation(this);
		}

		protected override CusSupportingInfoLookups GetNewLookups() => new AdditionalInfoLookups(this);

		[ReadOnlyMember(nameof(CSI_SubType_ReadOnly))]
		[MaxLength(3)]
		public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

		public bool CSI_SubType_ReadOnly => IsUCC5AndIsImport;

		[ReadOnlyMember(nameof(CSI_ReferenceNumber_ReadOnly))]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		public bool CSI_ReferenceNumber_ReadOnly => IsAnAdditionalInformation;

		[ReadOnlyMember(nameof(CSI_Description_ReadOnly))]
		[MaxLength(Schema.DescriptionMaxLength)]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

		public bool CSI_Description_ReadOnly => IsATransportDocument || IsAnAdditionalReference;

		internal bool IsAdditionalReferenceOnDeclaration => IsAnAdditionalReference && (ParentIsJobComInvoiceLine || ParentIsCusEntryInstruction || ParentIsInvoiceHeader);
		internal bool IsRoroRoroUnaccompaniedTrailer => CSI_Code.EqualsIgnoringCase(Constants.AdditionalReferenceCodes.RoRoUnaccompaniedTrailerRegistrationNumber);
		internal bool IsRoRoShipID => CSI_Code.EqualsIgnoringCase(Constants.AdditionalReferenceCodes.RoRoShipID);
		internal bool IsUCC5AndIsImport => Declaration is JobDeclaration declaration && declaration.IsUCC5AndIsImport;
	}
}
