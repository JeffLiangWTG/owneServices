using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	public class DocumentUDFFieldPropertiesWrapperCollection : GenericWrapperCollection<CodeMultilingualDescriptionWrapper>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public DocumentUDFFieldPropertiesWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (CodeDescriptionPair item in new FieldBuilderPropertyCodeDescriptionList())
			{
				Add(new CodeMultilingualDescriptionWrapper(item.Code, item.Description, Factory));
			}
		}
	}
}
