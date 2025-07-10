using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class AccChargeCodeCollectionProvider : CollectionProviderWithCodeSupport
	{
		public AccChargeCodeCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new AccChargeCodeCollection(BusinessObjectFactory, Filter, Env.CurrentCompany.PK);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.AccChargeCode;

		public override int MaxLength => AccChargeCodeSchema.AC_Code.MaxLength;
	}
}
