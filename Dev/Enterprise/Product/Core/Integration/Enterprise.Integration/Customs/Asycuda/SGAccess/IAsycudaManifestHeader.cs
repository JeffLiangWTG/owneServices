using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public static partial class SGAccess
			{
				public partial interface IAsycudaManifestHeader
				{
					ZDateTime ValuationDate { get; }
				}
			}
		}
	}
}