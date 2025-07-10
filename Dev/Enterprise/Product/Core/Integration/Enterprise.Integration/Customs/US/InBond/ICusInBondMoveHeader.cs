using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class InBond
			{
				public interface ICusInBondMoveHeader : Customs.ICusInBondMoveHeader
				{
					ZString InBondNumber { get; set; }
				}
			}
		}
	}
}