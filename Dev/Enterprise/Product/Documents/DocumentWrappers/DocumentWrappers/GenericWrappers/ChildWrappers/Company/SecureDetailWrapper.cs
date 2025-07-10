using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Public"), WrapperTypeName("SecureDetail")]
	public class SecureDetailWrapper : GenericWrapper
	{
		public SecureDetailWrapper(ZString value, ZBool isPublished, BusinessObjectFactory factory)
			: base(null, factory)
		{
			this.value = value;
			this.isPublished = isPublished;
		}

		public ZString Internal
		{
			get { return value; }
		}

		public ZString Public
		{
			get { return isPublished ? value : ZString.Empty; }
		}

		public ZBool IsPublished
		{
			get { return isPublished; }
		}

		readonly ZString value;
		readonly ZBool isPublished;
	}
}
