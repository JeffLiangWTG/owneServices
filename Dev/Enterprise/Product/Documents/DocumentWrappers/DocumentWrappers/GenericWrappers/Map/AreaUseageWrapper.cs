using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	[DefaultField("Useage")]
	public class AreaUseageWrapper : Base.GenericWrapper
	{
		public AreaUseageWrapper(ValueProviderDocumenter areaDocumentation, BusinessObjectFactory factory)
			: base(null, factory)
		{
			AreaDocumentation = areaDocumentation ?? new ValueProviderDocumenter("", (NoResString)"");
		}
		readonly ValueProviderDocumenter AreaDocumentation;

		public ZString Useage
		{
			get { return AreaDocumentation.Useage; }
		}

		public ZString Explanation
		{
			get { return AreaDocumentation.Explanation; }
		}
	}
}
