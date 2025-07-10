using Enterprise.Registry.Business;
namespace Enterprise.Customs.GB.Registry
{
	[RegistryEditor("Enterprise.Customs.GB.GUI.Registry.BadgeCodeRegistryItemEditor, Enterprise.Customs.GB.GUI")]
	public class BadgeCodeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BadgeCodeSettingCollection>
	{
		public BadgeCodeRegistryDataType()
		{
		}
	}
}
