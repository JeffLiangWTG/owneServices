using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class TaxConfigurationCodeDescriptionPairListProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			var query = new ZQuery(AccTaxConfigurationSchema.ETC_IsActive, true);
			AddAdditionalFilter(query);
			var configurations = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetCompanyTaxConfigurations(new ReadOnlyBusinessObjectFactory(), GlbCompany.CurrentCompany, query);
			foreach (var configuration in configurations)
			{
				result.AddPair(configuration.ETC_Code, configuration.ETC_Description);
			}

			return result;
		}

		virtual protected void AddAdditionalFilter(ZQuery query)
		{
		}
	}
}
