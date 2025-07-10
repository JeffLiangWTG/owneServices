using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Forwarding
	{
		public interface IDirectionIsDomesticFreight
		{
			ZBool IsDomesticFreight { get; set; }
		}
	}
}
