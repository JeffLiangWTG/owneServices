using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class AccGLAccountDescriptorCHSCollectionProvider : CollectionProvider
	{
		public AccGLAccountDescriptorCHSCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			var query = new ZQuery(AccGLAccountDescriptorSchema.AJ_Language, SQLComparisonOperator.Equal, "ZH-CN");
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, "CN");
			Filter.AddToFilter(query);
			return new AccGLAccountDescriptorCollection(BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.AccGLAccountDescriptor;
	}
}
