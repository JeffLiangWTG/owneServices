using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public interface IDispositionCodeListLoader
			{
				IBusinessObjectCollection GetAMSDispositionCollection(BusinessObjectFactory factory);
			}
		}
	}
}
