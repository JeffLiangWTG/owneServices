using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseAttachingObject : NonPersistentBusinessObject
	{
		public static class Schema
		{
			public const string ShouldAttach = "ShouldAttach";
			public const string ImportLicenseEntryDescription = "ImportLicenseEntryDescription";
			public const string ImportLicenseEntryMRN = "ImportLicenseEntryMRN";
			public const string ImportLicenseEntryRegistrationDate = "ImportLicenseEntryRegistrationDate";
			public const string ImportLicenseEntryStatusDescription = "ImportLicenseEntryStatusDescription";
			public const string FeeType = "FeeType";
		}

		public ImportLicenseAttachingObject(CusEntryInstruction instruction) : base(instruction.Factory)
		{
			EntryInstruction = Argument.NotNull(instruction, nameof(instruction));
		}

		public readonly CusEntryInstruction EntryInstruction;

		protected override ZString HumanReadableNameCore => Res.GetString("59d8ca2d-945a-4213-a1e8-60ac84b87c23", "License");

		[ResourceStringData("BR.ImportLicenseAttachingObject|ShouldAttach", Caption = "Attach?")]
		public ZBool ShouldAttach
		{
			get => shouldAttach;
			set
			{
				SetNonPersistentPropertyValue(ShouldAttachInfo, ref shouldAttach, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateShouldAttach();
				}
			}
		}
		ZBool shouldAttach;

		public ZPropertyInfo ShouldAttachInfo => GetZPropertyInfo(Schema.ShouldAttach);

		[ResourceStringData("BR.ImportLicenseAttachingObject|ImportLicenseEntryDescription", Caption = "Description")]
		public ZString ImportLicenseEntryDescription => EntryInstruction.CEI_Description;

		public ZPropertyInfo ImportLicenseEntryDescriptionInfo => GetZPropertyInfo(Schema.ImportLicenseEntryDescription);

		[ResourceStringData("BR.ImportLicenseAttachingObject|ImportLicenseEntryMRN", Caption = "Number")]
		public ZString ImportLicenseEntryMRN => EntryInstruction.EntryHeader?.MovementReferenceNumber ?? ZString.Empty;

		public ZPropertyInfo ImportLicenseEntryMRNInfo => GetZPropertyInfo(Schema.ImportLicenseEntryMRN);

		[ResourceStringData("BR.ImportLicenseAttachingObject|ImportLicenseEntryRegistrationDate", Caption = "Registration Date")]
		public ZDateTime ImportLicenseEntryRegistrationDate => EntryInstruction.EntryHeader?.MovementReferenceNumberIssueDate ?? ZDateTime.Empty;

		public ZPropertyInfo ImportLicenseEntryRegistrationDateInfo => GetZPropertyInfo(Schema.ImportLicenseEntryRegistrationDate);

		[ResourceStringData("BR.ImportLicenseAttachingObject|ImportLicenseEntryStatusDescription", Caption = "License Status")]
		public ZString ImportLicenseEntryStatusDescription => EntryInstruction.EntryHeader?.EntryHeaderStatusDescription ?? ZString.Empty;

		public ZPropertyInfo ImportLicenseEntryStatusDescriptionInfo => GetZPropertyInfo(Schema.ImportLicenseEntryStatusDescription);

		[List(nameof(Lookups) + "." + nameof(ImportLicenseAttachingObjectLookups.FeeTypeList))]
		[ResourceStringData("BR.ImportLicenseAttachingObject|FeeType", Caption = "Fee Type")]
		public ZString FeeType { get => feeType; set => SetNonPersistentPropertyValue(FeeTypeInfo, ref feeType, value); }
		ZString feeType;

		public ZPropertyInfo FeeTypeInfo => GetZPropertyInfo(Schema.FeeType);

		#region Validation

		public ImportLicenseAttachingObjectValidation Validation
		{
			get { return new ImportLicenseAttachingObjectValidation(this); }
		}

		#endregion

		#region Lookups

		public ImportLicenseAttachingObjectLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new ImportLicenseAttachingObjectLookups(this);
				}
				return fLookups;
			}
		}

		ImportLicenseAttachingObjectLookups fLookups;

		#endregion
	}
}
