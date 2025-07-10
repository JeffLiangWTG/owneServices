/**
 * USAGE TO ACCESS COUNTRYCOMPLIANCE:
 *		((IAccountingCountryComplianceGlobalFactory)new AccountingCountryComplianceGlobalFactory());
 * 
 * This gives us a new instance of AccountingCountryComplianceGlobalFactory of type IAccountingCountryComplianceGlobalFactory.
 * More information on how to use CountryCompliance project can be found here in Readme.md in the solution folder.
 */
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Implementation;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(Constants))]

namespace Enterprise.Accounting.CountryCompliance.GlobalCountryFactory
{
	public class AccountingCountryComplianceGlobalFactory : IAccountingCountryComplianceGlobalFactory
	{
		FeatureInterface IAccountingCountryComplianceGlobalFactory.GetFeatureInterface<FeatureInterface>(ZString countryCode)
		{
			// Keep the following switch statement cases ordered alphabetically.
			return (string)countryCode switch
			{
				Constants.CountryCodes.Argentina			=> CastToFeatureInterface(new ArgentinaCountryFactory()),
				Constants.CountryCodes.Brazil				=> CastToFeatureInterface(new BrazilCountryFactory()),
				Constants.CountryCodes.Chile				=> CastToFeatureInterface(new ChileCountryFactory()),
				Constants.CountryCodes.China				=> CastToFeatureInterface(new ChinaCountryFactory()),
				Constants.CountryCodes.CzechRepublic		=> CastToFeatureInterface(new CzechRepublicCountryFactory()),
				Constants.CountryCodes.DominicanRepublic	=> CastToFeatureInterface(new DominicanRepublicCountryFactory()),
				Constants.CountryCodes.Germany				=> CastToFeatureInterface(new GermanyCountryFactory()),
				Constants.CountryCodes.Israel				=> CastToFeatureInterface(new IsraelCountryFactory()),
				Constants.CountryCodes.KoreaSouth			=> CastToFeatureInterface(new KoreaSouthCountryFactory()),
				Constants.CountryCodes.Kosovo				=> CastToFeatureInterface(new KosovoCountryFactory()),
				Constants.CountryCodes.Malaysia				=> CastToFeatureInterface(new MalaysiaCountryFactory()),
				Constants.CountryCodes.Mexico				=> CastToFeatureInterface(new MexicoCountryFactory()),
				Constants.CountryCodes.Norway				=> CastToFeatureInterface(new NorwayCountryFactory()),
				Constants.CountryCodes.SaintMartin			=> CastToFeatureInterface(new SaintMartinCountryFactory()),
				Constants.CountryCodes.Singapore			=> CastToFeatureInterface(new SingaporeCountryFactory()),
				Constants.CountryCodes.Spain				=> CastToFeatureInterface(new SpainCountryFactory()),
				Constants.CountryCodes.Turkey				=> CastToFeatureInterface(new TurkeyCountryFactory()),
				Constants.CountryCodes.VietNam				=> CastToFeatureInterface(new VietnamCountryFactory()),
				_											=> null,
			};

			FeatureInterface CastToFeatureInterface<CountryFactory>(CountryFactory countryFactory) => (countryFactory as IInstanceProvider<FeatureInterface>)?.Get();
		}
	}
}
