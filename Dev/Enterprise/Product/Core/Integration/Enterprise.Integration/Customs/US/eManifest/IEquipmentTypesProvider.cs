namespace Enterprise.Integration
{
	using CargoWise.Integration;

	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class eManifest
			{
				public interface IEquipmentTypesProvider
				{
					ICodeDescriptionPairList GetEquipmentTypes();
				}
			}
		}
	}
}