using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface INZSupplierProvider
			{
				BusinessObjectCollection GetSupplierList(BusinessObjectFactory factory, IBusiness parent);
			}
		}
	}
}
