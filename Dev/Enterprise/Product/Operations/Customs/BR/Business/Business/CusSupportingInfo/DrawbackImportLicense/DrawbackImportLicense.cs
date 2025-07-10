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
	public class DrawbackImportLicense : SingleCusSupportingInfo
	{
		public DrawbackImportLicense(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;

		public override bool SupportsNotes => false;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.Drawback;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(DrawbackImportLicenseLookups.DrawbackModalityList))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (!IsCopying && oldValue != CSI_Code)
				{
					if (CSI_ReferenceNumber_ReadOnly)
					{
						CSI_ReferenceNumber = ZString.Empty;
					}
					if (CSI_ItemNumber_ReadOnly)
					{
						CSI_ItemNumber = ZShort.Zero;
					}
				}
			}
		}

		[MaxLength(13)]
		[ReadOnlyMember(nameof(CSI_ReferenceNumber_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.DrawbackImportLicense|CSI_ReferenceNumber", Caption = "CA Number")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		public bool CSI_ReferenceNumber_ReadOnly => CSI_Code == DrawbackModalityList.Codes.NoDrawback;

		[ReadOnlyMember(nameof(CSI_ItemNumber_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.DrawbackImportLicense|DrawbackItemNumber", Caption = "Item Number")]
		public override ZInt CSI_ItemNumber
		{
			get => base.CSI_ItemNumber;
			set => base.CSI_ItemNumber = value;
		}

		public bool CSI_ItemNumber_ReadOnly => CSI_Code == DrawbackModalityList.Codes.NoDrawback || CSI_Code == DrawbackModalityList.Codes.ExemptionPaper;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new DrawbackImportLicenseValidation(this);
		}

		public new DrawbackImportLicenseLookups Lookups => (DrawbackImportLicenseLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new DrawbackImportLicenseLookups(this);

		public override IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return CSI_CodeInfo;
			yield return CSI_ReferenceNumberInfo;
			yield return CSI_ItemNumberInfo;
		}
	}
}
