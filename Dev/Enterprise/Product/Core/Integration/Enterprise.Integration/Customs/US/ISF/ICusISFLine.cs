using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class ISF
			{
				public interface ICusISFLine
				{
					ZString BL_HarmonisedNum { get; }
					ZString BL_RN_NKGoodsOrigin { get; }
					ZString BL_TextProductCode { get; }
					IUSISFDocAddress ManufacturerDocAddress { get; }
				}
			}
		}
	}
}
