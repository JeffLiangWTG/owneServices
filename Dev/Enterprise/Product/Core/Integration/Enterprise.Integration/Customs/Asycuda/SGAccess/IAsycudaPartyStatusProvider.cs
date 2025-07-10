using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public static partial class SGAccess
			{
				public interface IAsycudaPartyStatusProvider
				{
					ICodeDescriptionPairList GetPartyStatusCodeList(BusinessObjectFactory factory);
				}
			}
		}
	}
}
