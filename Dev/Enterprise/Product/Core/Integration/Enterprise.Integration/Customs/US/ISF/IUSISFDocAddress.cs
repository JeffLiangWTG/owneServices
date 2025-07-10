using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class ISF
			{
				public interface IUSISFDocAddress
				{
					ZString E2_AddressType { get; }
					ZString E2_GovRegNum { get; }
					ZGuid PK { get; }
					ZString E2_GovRegNumType { get; }
					ZByte E2_AddressSequence { get; }
					ZString E2_CompanyName { get; }
					ZString AddressDescription { get; }
					ZString RegTypeAndNumForDocument { get; }
				}
			}
		}
	}
}
