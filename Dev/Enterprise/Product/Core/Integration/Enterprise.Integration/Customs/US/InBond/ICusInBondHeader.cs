using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class InBond
			{
				public interface ICusInBondHeader : Customs.ICusInBondHeader
				{
					ZString InBondClosedDate { get; }
					ZString InBondEntryTypes { get; }
				}
			}
		}
	}
}