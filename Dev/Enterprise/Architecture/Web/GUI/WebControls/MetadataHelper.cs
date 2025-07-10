using System.ComponentModel;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	class MetadataHelper
	{
		public static string GetListMember<T>(T bindable, object dataSource) where T : IBindTo, IBindToList
		{
			PropertyDescriptor descriptor = ZPropertyAccessor.GetPropertyDescriptor(dataSource, bindable.BindTo);
			return MetadataAccessor.GetListMember(bindable.BindToList, descriptor, bindable.BindTo);
		}
	}
}
