using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface ICusExitDetail
			{
				ZGuid CED_CEH { get; set; }
			}
		}
	}
}
