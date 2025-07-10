using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public static partial class ACEManifest
			{
				public partial interface IAsycudaBill
				{
					ZBool FDAIndicator { get; set; }
				}
			}
		}
	}
}