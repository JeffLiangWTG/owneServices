using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	partial class CusEntryLine : IDLMDetailLine
	{
		#region IDLMDetailLine Members

		ZString IDLMDetailLine.CountryOfOrigin
		{
			get
			{
				RefCountry country = RefCountry.LoadFromCountryCode(Factory, RandomLine.EffectiveCountryOfOrigin);
				return country != null ? country.RN_DescMultilingual : ZString.Empty;
			}
		}

		public ZString ProvinceOfOrigin
		{
			get
			{
				ZString province = ZString.Empty;
				if (RandomLine.EffectiveCountryOfOrigin == Core.Constants.CountryCodes.Canada)
				{
					province = new CanadianProvinceList().GetDescriptionFromCode(RandomLine.EffectiveProvinceOfOrigin) ?? ZString.Empty;
				}
				return province;
			}
		}

		ZString IDLMDetailLine.HarmonizedSystemCode
		{
			get { return CL_AdValoremTariff; }
		}

		ZString IDLMDetailLine.ProductDescription
		{
			get { return EffectiveDescription; }
		}

		public ZString ConveyanceIdentificationNumber
		{
			get { return RandomLine.CA_ConveyanceIdentificationNumber; }
		}

		ZDecimal IDLMDetailLine.Quantity
		{
			get { return CustomsQuantity; }
		}

		ZString IDLMDetailLine.UnitOfMeasure
		{
			get { return CustomsUnitQtyDescription; }
		}

		public ZDecimal ValueFOBPointOfExit
		{
			get { return CurrencyConverter.ConvertExact(FOB, Declaration.DeclaredCurrency).Amount; }
		}

		#endregion
	}
}
