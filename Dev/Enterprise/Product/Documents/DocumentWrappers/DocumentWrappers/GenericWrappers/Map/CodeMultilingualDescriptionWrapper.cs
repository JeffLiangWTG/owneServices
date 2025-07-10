using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	[DefaultField("Useage")]
	public class CodeMultilingualDescriptionWrapper : GenericWrapper
	{
		public CodeMultilingualDescriptionWrapper(ZString code, ZString description, BusinessObjectFactory factory)
			: base(null, factory)
		{
			useage = code;
			explanation = description;
		}

		readonly ZString useage;
		readonly ZString explanation;

		public ZString Useage
		{
			get { return useage; }
		}

		public ZString Explanation
		{
			get { return explanation; }
		}
	}
}
