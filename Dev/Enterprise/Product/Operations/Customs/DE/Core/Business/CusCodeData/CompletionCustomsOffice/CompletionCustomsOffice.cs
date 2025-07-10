using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business
{
	public class CompletionCustomsOffice : CusCodeData
	{
		public CompletionCustomsOffice(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public new const int CY_DataMaxLength = 8;
		}

		[MaxLength(Schema.CY_DataMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CompletionCustomsOfficeLookups.OfficeCodeList))]
		[ResourceStringData("A82622A3-3342-494D-A0A3-EAD8E60D8D76|CompletionCustomsOffice|CY_Data", Caption = "Office Code")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		[ResourceStringData("8E6E0CEF-91A1-45B9-8CE0-A3DF634ED6D5|CompletionCustomsOffice|CY_Description", Caption = "Description")]
		public ZString CY_OfficeDescription => Office?.ZZD_Description ?? ZString.Empty;

		ZZRefCusCodeListCombined Office => office ?? (office = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, CY_Data, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
		ZZRefCusCodeListCombined office;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(Declaration.CusEntryInstruction));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.CompletionCustomsOffice;
			CY_Code = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
		}

		public new CompletionCustomsOfficeLookups Lookups => (CompletionCustomsOfficeLookups)base.Lookups;

		protected override CusCodeDataLookups GetNewLookups() => new CompletionCustomsOfficeLookups(this);

		public new CompletionCustomsOfficeValidation Validation => (CompletionCustomsOfficeValidation)base.Validation;

		protected override CusCodeDataValidation GetNewValidation() => new CompletionCustomsOfficeValidation(this);

		protected override ZString HumanReadableNameCore => Res.GetString("666BCD13-C6A6-4372-AC4E-E1FFDFCF81BB", "Customs Office");
	}
}
