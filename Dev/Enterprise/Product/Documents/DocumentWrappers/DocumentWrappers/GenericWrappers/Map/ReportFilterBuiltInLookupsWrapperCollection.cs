using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	public class ReportFilterBuiltInLookupsWrapperCollection : GenericWrapperCollection<CodeMultilingualDescriptionWrapper>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ReportFilterBuiltInLookupsWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (CodeDescriptionPair item in new CollectionProviderTypeCodeDescriptionList())
			{
				Add(new CodeMultilingualDescriptionWrapper(item.Code, item.Description, Factory));
			}
		}
	}
}
