using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface ICodeDataPair
			{
				ZString ZO_Code { get; set; }
				ZString ZO_Data { get; set; }
				ZString ZO_Description { get; }
			}
		}
	}
}
