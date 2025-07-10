using CargoWise.Types;

namespace Enterprise.Integration;

public static partial class Customs
{
	public static partial class MY
	{
		public interface IPortOperator
		{
			ZString PortOperatorAndSCN { get; }
		}
	}
}
