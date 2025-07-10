using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusInBondMoveLineItem
		{
			ZGuid PK { get; }
			ZGuid BI_B9 { get; set; }
		}
	}
}
