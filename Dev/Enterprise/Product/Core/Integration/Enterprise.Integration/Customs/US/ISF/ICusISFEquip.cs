using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class ISF
			{
				public interface ICusISFEquip
				{
					ZString BE_ContainerISO { get; }
					ZString BE_ContainerNum { get; }
					ZString BE_EquipCode { get; }
				}
			}
		}
	}
}
