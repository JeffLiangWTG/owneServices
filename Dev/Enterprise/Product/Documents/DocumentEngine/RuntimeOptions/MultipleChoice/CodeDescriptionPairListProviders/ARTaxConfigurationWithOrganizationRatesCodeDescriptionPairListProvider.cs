using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ARTaxConfigurationWithOrganizationRatesCodeDescriptionPairListProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var result = new CodeDescriptionPairList();

			var configurations = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().
							GetTaxConfigurationThatSupportsOrganisationRates(new ReadOnlyBusinessObjectFactory(), GlbCompany.CurrentCompany)
							.Where(x => x.ETC_Ledger.Equals(TaxConfigurationLedgers.AccountsReceivable.Code));

			foreach (var configuration in configurations)
			{
				result.AddPair(configuration.ETC_Code, configuration.ETC_Description);
			}

			return result;
		}
	}
}
