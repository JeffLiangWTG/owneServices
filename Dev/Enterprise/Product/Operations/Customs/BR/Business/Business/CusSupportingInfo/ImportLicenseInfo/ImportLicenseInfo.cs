using System.Collections.Generic;
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
	public class ImportLicenseInfo : SingleCusSupportingInfo
	{
		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public new const int CSI_CodeMaxLength = 1;
			public new const int CSI_ReferenceNumberMaxLength = 10;
			public new const int CSI_SubTypeMaxLength = 4;
		}

		public ImportLicenseInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.ImportLicense;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;

		public new ImportLicenseInfoLookups Lookups => (ImportLicenseInfoLookups)base.Lookups;

		public new ImportLicenseInfoValidation Validation => (ImportLicenseInfoValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new ImportLicenseInfoValidation(this);

		protected override CusSupportingInfoLookups GetNewLookups() => new ImportLicenseInfoLookups(this);

		public override bool SupportsNotes => false;

		[MaxLength(Schema.CSI_CodeMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.ImportLicense|CSI_Code", ShortCaption = "Type", Caption = "Import License Type")]
		[ReadOnlyMember(nameof(ImportLicenseDetails_ReadOnly))]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.ImportLicense|CSI_DateOfIssue", ShortCaption = "Concession Date", Caption = "Import License Concession Date")]
		[ReadOnlyMember(nameof(ImportLicenseDetails_ReadOnly))]
		public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

		[MaxLength(Schema.CSI_SubTypeMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ImportLicenseFeeType", Caption = "Import License Fee Type", ShortCaption = "Fee Type", FullDescription = "The Import License Fee Type.")]
		[List(nameof(Lookups) + "." + nameof(ImportLicenseInfoLookups.FeeTypeList))]
		[ReadOnlyMember(nameof(ImportLicenseDetails_ReadOnly))]
		public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

		[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.ImportLicense|CSI_ReferenceNumber", ShortCaption = "Number", Caption = "Import License Number")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set
			{
				var oldValue = CSI_ReferenceNumber;
				base.CSI_ReferenceNumber = value;
				if (!IsCopying && oldValue != CSI_ReferenceNumber)
				{
					if (CSI_ReferenceNumber.IsEmpty)
					{
						CSI_Code = ZString.Empty;
						CSI_DateOfIssue = ZDateTime.Empty;
						CSI_SubType = ZString.Empty;
					}

					Parent?.InvoiceHeader?.MarkAsNeedingValidation();
					Parent?.InvoiceHeader?.InvoiceLines?.MarkAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateCSI_Code();
						Validation.ValidateCSI_DateOfIssue();
						Validation.ValidateCSI_SubType();
					}
				}
			}
		}

		ZBool ImportLicenseDetails_ReadOnly => CSI_ReferenceNumber.IsEmpty;

		public override IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return CSI_CodeInfo;
			yield return CSI_DateOfIssueInfo;
			yield return CSI_ReferenceNumberInfo;
			yield return CSI_SubTypeInfo;
		}
	}
}
