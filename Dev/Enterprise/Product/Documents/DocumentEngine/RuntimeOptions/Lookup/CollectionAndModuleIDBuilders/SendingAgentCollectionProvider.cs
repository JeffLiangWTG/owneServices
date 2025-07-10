using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class SendingAgentCollectionProvider : CollectionProvider
	{
		public SendingAgentCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			//Need to set the relationship filter on your collection if you need a master detail relationship between findboxes...
			return new ForwarderCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Organisation;

		public override void AddValidationAndDefault(FilterField parentFilterField, ValidatorPack validatorPack)
		{
			base.AddValidationAndDefault(parentFilterField, validatorPack);
			if (!Env.Security.ReportsExternalAgentsFilter.IsAllowed)
			{
				parentFilterField.Validators.Add(validatorPack.AtLeastOneAgentFilterContainsOrgProxy);
				validatorPack.AtLeastOneAgentFilterContainsOrgProxy.Filters.Add(parentFilterField);
				FilterDefaultsHelper.AddDefaultValue(parentFilterField, GlbCompany.CurrentCompany.OrgProxy);
			}
		}
	}
}
