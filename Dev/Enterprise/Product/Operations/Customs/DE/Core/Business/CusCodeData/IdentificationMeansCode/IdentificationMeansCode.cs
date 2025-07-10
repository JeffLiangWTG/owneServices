using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.Business
{
	public class IdentificationMeansCode : CusCodeData
	{
		public IdentificationMeansCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public new const int CY_CodeMaxLength = 1;
			public new const int CY_DataMaxLength = 255;
		}

		[MaxLength(Schema.CY_CodeMaxLength)]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}

		[MaxLength(Schema.CY_DataMaxLength)]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		public override bool CY_DataAllowWesternEuropeanCharactersOnly => false;

		public new IdentificationMeansCodeValidation Validation => (IdentificationMeansCodeValidation)base.Validation;

		public new IdentificationMeansCodeLookups Lookups => (IdentificationMeansCodeLookups)base.Lookups;

		protected override ZString HumanReadableNameCore => Res.GetString("5db40a87-dd2e-4bd5-9bb1-baa48d55c266", "Identification Means");

		protected override CusCodeDataValidation GetNewValidation() => new IdentificationMeansCodeValidation(this);

		protected override CusCodeDataLookups GetNewLookups() => new IdentificationMeansCodeLookups(this);

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusEntryInstruction));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.IdentificationMeansCode;
		}
	}
}
