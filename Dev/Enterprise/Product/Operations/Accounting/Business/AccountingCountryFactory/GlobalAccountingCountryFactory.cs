using CargoWise.Types;
using Enterprise.Integration.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	/// <summary>
	///-------------------------------------------------------------------
	/// 🚩🚩🚩 IMPORTANT: THIS FACTORY MUST NOT BE USED FOR NEW FEATURES.
	///-------------------------------------------------------------------
	///
	/// It just keeps existing features that we did not move to a new Accounting.CountryCompliance solution.
	/// New features must be implemented in new Accounting.CountryCompliance solution.
	/// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/11765/Accounting.CountryCompliance-solution
	/// </summary>
	class GlobalAccountingCountryFactory : IGlobalAccountingCountryFactory
	{
		IAccountingCountryFactory IGlobalAccountingCountryFactory.GetCountryFactory(ZString countryCode)
		{
			//Keep the following switch statement cases ordered alphabetically
			return (string)countryCode switch
			{
				CountryCodes.Argentina => new ArgentinaAccountingCountryFactory(),
				CountryCodes.Australia => new AustraliaAccountingCountryFactory(),
				CountryCodes.Belize => new BelizeAccountingCountryFactory(),
				CountryCodes.BonaireSintEustatiusAndSaba => new BonaireSintEustatiusAndSabaAccountingCountryFactory(),
				CountryCodes.Brazil => new BrazilAccountingCountryFactory(),
				CountryCodes.BurkinaFaso => new BurkinaFasoAccountingCountryFactory(),
				CountryCodes.CapeVerde => new CapeVerdeAccountingCountryFactory(),
				CountryCodes.Chile => new ChileAccountingCountryFactory(),
				CountryCodes.China => new ChinaAccountingCountryFactory(),
				CountryCodes.Colombia => new ColombiaAccountingCountryFactory(),
				CountryCodes.CookIslands => new CookIslandsAccountingCountryFactory(),
				CountryCodes.CostaRica => new CostaRicaAccountingCountryFactory(),
				CountryCodes.DominicanRepublic => new DominicanRepublicAccountingCountryFactory(),
				CountryCodes.Egypt => new EgyptAccountingCountryFactory(),
				CountryCodes.FaeroeIslands => new FaeroeIslandsAccountingCountryFactory(),
				CountryCodes.Fiji => new FijiAccountingCountryFactory(),
				CountryCodes.FrenchGuyana => new FrenchGuianaAccountingCountryFactory(),
				CountryCodes.Gambia => new GambiaAccountingCountryFactory(),
				CountryCodes.Germany => new GermanyAccountingCountryFactory(),
				CountryCodes.Ghana => new GhanaAccountingCountryFactory(),
				CountryCodes.India => new IndiaAccountingCountryFactory(),
				CountryCodes.Indonesia => new IndonesiaAccountingCountryFactory(),
				CountryCodes.Israel => new IsraelAccountingCountryFactory(),
				CountryCodes.Italy => new ItalyAccountingCountryFactory(),
				CountryCodes.Jordan => new JordanAccountingCountryFactory(),
				CountryCodes.KoreaSouth => new KoreaSouthAccountingCountryFactory(),
				CountryCodes.Kyrgyzstan => new KyrgyzstanAccountingCountryFactory(),
				CountryCodes.Latvia => new LatviaAccountingCountryFactory(),
				CountryCodes.Malaysia => new MalaysiaAccountingCountryFactory(),
				CountryCodes.Mauritius => new MauritiusAccountingCountryFactory(),
				CountryCodes.Mayotte => new MayotteAccountingCountryFactory(),
				CountryCodes.Mexico => new MexicoAccountingCountryFactory(),
				CountryCodes.Norway => new NorwayAccountingCountryFactory(),
				CountryCodes.Panama => new PanamaAccountingCountryFactory(),
				CountryCodes.Philippines => new PhilippinesAccountingCountryFactory(),
				CountryCodes.Poland => new PolandAccountingCountryFactory(),
				CountryCodes.Portugal => new PortugalAccountingCountryFactory(),
				CountryCodes.Romania => new RomaniaAccountingCountryFactory(),
				CountryCodes.SaintKittsAndNevis => new SaintKittsAndNevisAccountingCountryFactory(),
				CountryCodes.SaudiArabia => new SaudiArabiaAccountingCountryFactory(),
				CountryCodes.Serbia => new SerbiaAccountingCountryFactory(),
				CountryCodes.Spain => new SpainAccountingCountryFactory(),
				CountryCodes.Suriname => new SurinameAccountingCountryFactory(),
				CountryCodes.Swaziland => new SwazilandAccountingCountryFactory(),
				CountryCodes.Taiwan => new TaiwanAccountingCountryFactory(),
				CountryCodes.Turkey => new TurkeyAccountingCountryFactory(),
				CountryCodes.Turkmenistan => new TurkmenistanAccountingCountryFactory(),
				CountryCodes.Tuvalu => new TuvaluAccountingCountryFactory(),
				CountryCodes.UnitedKingdom => new UnitedKingdomAccountingCountryFactory(),
				CountryCodes.Uruguay => new UruguayAccountingCountryFactory(),
				CountryCodes.VietNam => new VietnamAccountingCountryFactory(),
				CountryCodes.Zimbabwe => new ZimbabweAccountingCountryFactory(),
				_ => null,
			};
		}
	}
}
