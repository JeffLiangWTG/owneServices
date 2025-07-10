using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EUExitControl
		{
			public interface ICusExitConsignmentItem
			{
				ZGuid PK { get; }

				ZShort CCI_LineNumber { get; set; }
			}
		}
	}
}
