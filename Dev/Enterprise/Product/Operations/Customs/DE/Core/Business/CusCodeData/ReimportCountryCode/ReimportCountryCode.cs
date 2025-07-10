using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.Business
{
	public class ReimportCountryCode : CusCodeData
	{
		public ReimportCountryCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public new class Schema : CusCodeData.Schema
		{
			public const string CountryDescription = "CountryDescription";
			public new const int CY_CodeMaxLength = 2;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("a6325a89-3dea-4308-a904-72ad156aab7d", "Reimport Country/Region");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.ReimportCountryCode;
		}

		protected override CusCodeDataValidation GetNewValidation() => new ReimportCountryCodeValidation(this);

		public new ReimportCountryCodeValidation Validation => (ReimportCountryCodeValidation)base.Validation;

		protected override CusCodeDataLookups GetNewLookups() => new ReimportCountryCodeLookups(this);

		public new ReimportCountryCodeLookups Lookups => (ReimportCountryCodeLookups)base.Lookups;

		[List(nameof(Lookups) + "." + nameof(ReimportCountryCodeLookups.CY_CodeList))]
		[MaxLength(Schema.CY_CodeMaxLength)]
		[ResourceStringData("4932350C-3E35-4AD7-8754-52AF5AD9B789", ShortCaption = "Ctry./Rgn.", Caption = "Country/Region")]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}

		public ZString CountryDescription
		{
			get => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory
						, CY_Code
						, Core.Constants.CountryCodes.Germany
						, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DEReimportCountry
						, ZDate.Today)?.ZZD_Description ?? ZString.Empty;
		}

		public ZPropertyInfo CountryDescriptionInfo => GetWrappedZPropertyInfo(Schema.CountryDescription, x => CY_DataInfo);

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusEntryInstruction));
	}
}
