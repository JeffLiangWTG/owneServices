using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class AdditionalIdentification : CusCodeData, Integration.Customs.BR.IAdditionalIdentification
	{
		public AdditionalIdentification(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const int AdditionalIdentificationDataMaxLength = 35;
			public const int AdditionalIdentificationCodeMaxLength = 3;
		}

		public override bool SupportsNotes => false;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(OrgHeader));

		public new AdditionalIdentificationLookups Lookups => (AdditionalIdentificationLookups)base.Lookups;

		public new AdditionalIdentificationValidation Validation => (AdditionalIdentificationValidation)base.Validation;

		protected override CusCodeDataLookups GetNewLookups() => new AdditionalIdentificationLookups(this);

		protected override CusCodeDataValidation GetNewValidation() => new AdditionalIdentificationValidation(this);

		[ResourceStringData("Enterprise.Customs.BR.Business.AdditionalIdentificationCusCodeData|CY_Data", Caption = "Number")]
		[MaxLength(Schema.AdditionalIdentificationDataMaxLength)]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		[List(nameof(Lookups) + "." + nameof(AdditionalIdentificationLookups.IssuingAgencyList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.AdditionalIdentificationCusCodeData|CY_Code", Caption = "Issuing Agency")]
		[MaxLength(Schema.AdditionalIdentificationCodeMaxLength)]
		public override ZString CY_Code { get => base.CY_Code; set => base.CY_Code = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			CY_Type = CusCodeDataTypeList.Codes.AdditionalIdentification;
		}
	}
}
