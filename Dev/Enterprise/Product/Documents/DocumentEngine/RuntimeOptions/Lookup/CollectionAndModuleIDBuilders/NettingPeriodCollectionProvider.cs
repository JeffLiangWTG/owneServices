using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class NettingPeriodCollectionProvider : CollectionProviderWithCodeSupport
	{
		public NettingPeriodCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new NettingSystemPeriodCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(NettingSystemPeriod)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new NettingSystemPeriodCollection(BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.NettingPeriod;

		public override int MaxLength => NettingSystemPeriodSchema.NSP_Period.MaxLength;

		public override void AddValidationAndDefault(FilterField parentFilterField, ValidatorPack validatorPack)
		{
			base.AddValidationAndDefault(parentFilterField, validatorPack);

			FilterDefaultsHelper.AddDefaultValue(parentFilterField, NettingPeriodHelper.GetFirstOpenNettingPeriod(GlbCompany.CurrentCompany.PK.ToGuid(), BusinessObjectFactory));
		}
	}
}
