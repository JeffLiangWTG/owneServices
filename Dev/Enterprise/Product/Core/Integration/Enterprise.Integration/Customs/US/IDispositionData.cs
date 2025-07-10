using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public interface IDispositionData : ICusAddInfo
			{
				ZString US_Code { get; set; }
				ZDateTime US_DispositionDate { get; set; }
			}
		}
	}
}
