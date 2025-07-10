using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICusEquipment
			{
				ZGuid PK { get; }

				ZString CEQ_IdentificationNumber { get; set; }

				ZGuid CEQ_JE_Declaration { get; set; }
			}
		}
	}
}
