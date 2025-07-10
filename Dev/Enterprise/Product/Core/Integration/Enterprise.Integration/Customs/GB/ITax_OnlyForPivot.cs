using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class GB
		{
			public interface ITax_OnlyForPivot
			{
				ZString G4_Type { get; set; }
			}
		}
	}
}
