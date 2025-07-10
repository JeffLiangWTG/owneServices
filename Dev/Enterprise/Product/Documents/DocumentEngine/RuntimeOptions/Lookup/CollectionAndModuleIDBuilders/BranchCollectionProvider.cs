using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class GlbBranchCollectionProvider : CollectionProviderWithCodeSupport
	{
		public GlbBranchCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new GlbBranchCollection(BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.GlbBranch;

		public override void AddValidationAndDefault(FilterField parentFilterField, ValidatorPack validatorPack)
		{
			base.AddValidationAndDefault(parentFilterField, validatorPack);
			if (!Env.Security.ReportsExternalBranchFilter.IsAllowed)
			{
				parentFilterField.Validators.Add(validatorPack.RequiredBranchFilter);
				validatorPack.RequiredBranchFilter.Filters.Add(parentFilterField);
				FilterDefaultsHelper.AddDefaultValue(parentFilterField, GlbBranch.CurrentBranch);
			}
		}

		public override int MaxLength => GlbBranchSchema.GB_Code.MaxLength;
	}
}
