using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface ICusTempStorageLinePivot
			{
				ZGuid PK { get; }

				ZGuid SLR_TSL_FromLine { get; set; }

				ZGuid SLR_TSL_ToLine { get; set; }
			}
		}
	}
}
