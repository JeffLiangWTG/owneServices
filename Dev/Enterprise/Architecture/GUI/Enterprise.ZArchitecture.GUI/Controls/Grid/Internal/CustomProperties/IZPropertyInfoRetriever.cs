using System.Windows.Forms;

using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IZPropertyInfoRetriever
	{
		ZPropertyInfo GetZPropertyInfo(BusinessObject businessObject);
	}

	static class ZPropertyInfoRetriever
	{
		public static ZPropertyInfo GetZPropertyInfo(DataGridColumnStyle columnStyle, BusinessObject businessObject)
		{
			return businessObject.ZPropertyInfoHash.GetPropertySafe(columnStyle.MappingName) ??
				GetZPropertyInfo(columnStyle.PropertyDescriptor as IZPropertyInfoRetriever, businessObject);
		}

		public static ZPropertyInfo GetZPropertyInfo(IZPropertyInfoRetriever retriever, BusinessObject businessObject)
		{
			return retriever != null ? retriever.GetZPropertyInfo(businessObject) : null;
		}
	}
}
