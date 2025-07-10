using System;
using System.ComponentModel;

using CargoWise.ComponentModel.Design;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	[Serializable]
	public class ZCompileTimeCheckBindingMember : CompileTimeCheckBindingMember
	{
		public ZCompileTimeCheckBindingMember(Type dataSourceType, Type controlPropertyType, string bindingMember)
			: base(dataSourceType, controlPropertyType, bindingMember)
		{
		}

		public override PropertyDescriptorCollection GetProperties(Type type)
		{
			return ZCustomTypeDescriptor.GetProperties(type);
		}
	}
}
