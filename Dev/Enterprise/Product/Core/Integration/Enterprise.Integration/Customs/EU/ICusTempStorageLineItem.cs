using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface ICusTempStorageLineItem
			{
				ZGuid PK { get; }

				ZGuid TSI_TSL { get; set; }
			}
		}
	}
}
