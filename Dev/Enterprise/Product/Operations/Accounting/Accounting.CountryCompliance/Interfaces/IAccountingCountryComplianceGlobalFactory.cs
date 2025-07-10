/**
 * USAGE TO ACCESS COUNTRYCOMPLIANCE:
 *		ObjectFactory.Get<IGlobalFactory>();
 * 
 * This gives us a new instance of GlobalFactory of type IGlobalFactory.
 * More information on how to use CountryCompliance project can be found here in Readme.md in the solution folder.
 */
using CargoWise.Types;

namespace Enterprise.Accounting.CountryCompliance.Interfaces
{
	public interface IAccountingCountryComplianceGlobalFactory
	{
		public FeatureInterface GetFeatureInterface<FeatureInterface>(ZString countryCode) where FeatureInterface : class;
	}
}
