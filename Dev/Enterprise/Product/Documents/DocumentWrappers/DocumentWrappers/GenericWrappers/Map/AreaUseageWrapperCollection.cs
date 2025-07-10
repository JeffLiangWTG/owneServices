using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Areas;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	public class AreaUseageWrapperCollection : Base.GenericWrapperCollection<AreaUseageWrapper>
	{
		public AreaUseageWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (ValueProviderDocumenter areaDocumentation in new AreaListManager().GetDocumenters())
			{
				Add(new AreaUseageWrapper(areaDocumentation, factory));
			}
		}
	}
}
