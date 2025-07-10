using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class ContentInformationType : CusCodeData
	{
		public ContentInformationType(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public new class Schema : CusCodeData.Schema
		{
			public const string DegreePercentage = nameof(ContentInformationType.DegreePercentage);

			public new const int CY_CodeMaxLength = 2;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.ContentInformationType;
		}

		protected override CusCodeDataValidation GetNewValidation() => new ContentInformationTypeValidation(this);

		public new ContentInformationTypeValidation Validation => (ContentInformationTypeValidation)base.Validation;

		protected override CusCodeDataLookups GetNewLookups() => new ContentInformationTypeLookups(this);

		public new ContentInformationTypeLookups Lookups => (ContentInformationTypeLookups)base.Lookups;

		[List(nameof(Lookups) + "." + nameof(ContentInformationTypeLookups.CY_CodeList))]
		[MaxLength(Schema.CY_CodeMaxLength)]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}

		[ResourceStringData("2F88CFFD-533D-499E-9322-BDB4D1916AF3", Caption = "Degree Percentage")]
		public ZDecimal DegreePercentage
		{
			get => ZDecimal.ParseSafe(CY_Data, ZDecimal.Zero);
			set => CY_Data = value.ToString(2);
		}

		public ZPropertyInfo DegreePercentageInfo => GetWrappedZPropertyInfo(Schema.DegreePercentage, x => CY_DataInfo);
	}
}
