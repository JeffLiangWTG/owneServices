using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class MercosulForeignDeclaration : CusSupportingInfo
	{
		public MercosulForeignDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : CusSupportingInfo.Schema
		{
			public const int DescriptionMaxLength = 70;
		}

		public JobComInvoiceLine InvoiceLine => Parent as JobComInvoiceLine;

		public CusEntryInstruction EntryInstruction => Parent as CusEntryInstruction;

		public override bool SupportsNotes => false;

		[MaxLength(nameof(DescriptionMaxLength))]
		[ResourceStringData("Enterprise.Customs.BR.Business.MercosulForeignDeclaration|CSI_Description", Caption = "Declaration No.", FullDescription = "The identification of the foreign Export Declaration originating from the MERCOSUR member countries.")]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		int DescriptionMaxLength => IsImportOnly ? Schema.DescriptionMaxLength : 16;

		[MaxLength(nameof(ReferenceNumberMaxLength))]
		[ResourceStringData("Enterprise.Customs.BR.Business.MercosulForeignDeclaration|CSI_ReferenceNumber", Caption = "Initial Range No.", FullDescription = "The number that identifies the Initial Range of the items that composes the foreign Export Declaration.")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		[MaxLength(nameof(ReferenceNumberMaxLength))]
		[ResourceStringData("Enterprise.Customs.BR.Business.MercosulForeignDeclaration|CSI_ReferenceNumber2", Caption = "Final Range No.", FullDescription = "The number that identifies the Final Range of the items that composes the foreign Export Declaration.")]
		public override ZString CSI_ReferenceNumber2
		{
			get => base.CSI_ReferenceNumber2;
			set => base.CSI_ReferenceNumber2 = value;
		}

		int ReferenceNumberMaxLength => IsImportOnly ? 7 : 4;

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(MercosulForeignDeclarationLookups.MercosulCountriesList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.MercosulForeignDeclaration|CSI_RN_NKCountryCode", Caption = "Issuing Country", FullDescription = "The Issuing Country.")]
		public override ZString CSI_RN_NKCountryCode
		{
			get => base.CSI_RN_NKCountryCode;
			set => base.CSI_RN_NKCountryCode = value;
		}

		[MaxLength(16)]
		[ResourceStringData("Enterprise.Customs.BR.Business.MercosulForeignDeclaration|CSI_Code", Caption = "Certificate No.", FullDescription = "The Certificate Number.")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		[MaxLength(4)]
		[ResourceStringData("Enterprise.Customs.BR.Business.MercosulForeignDeclaration|CSI_ItemNumber", Caption = "Item No.", FullDescription = "The Item Number.")]
		public override ZInt CSI_ItemNumber
		{
			get => base.CSI_ItemNumber;
			set => base.CSI_ItemNumber = value;
		}

		[MaxLength(19)]
		[DecimalPlaces(5)]
		[ResourceStringData("Enterprise.Customs.BR.Business.MercosulForeignDeclaration|CSI_Quantity3", Caption = "Item Qty.", FullDescription = "The Item Quantity measured in the Statistical Unit of Quantity.")]
		public override ZDecimal CSI_Quantity3
		{
			get => base.CSI_Quantity3;
			set => base.CSI_Quantity3 = value;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.MercosulForeignDeclaration;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return IsImportExcludingLicense ? new MercosulForeignDeclarationValidation(this) : base.GetNewValidation();
		}

		public new MercosulForeignDeclarationLookups Lookups => (MercosulForeignDeclarationLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new MercosulForeignDeclarationLookups(this);

		bool IsImportOnly => InvoiceLine?.IsImportOnly ?? EntryInstruction?.IsImportOnly ?? false;

		bool IsImportExcludingLicense => InvoiceLine?.IsImportExcludingLicense ?? EntryInstruction?.IsImportExcludingLicense ?? false;
	}
}
