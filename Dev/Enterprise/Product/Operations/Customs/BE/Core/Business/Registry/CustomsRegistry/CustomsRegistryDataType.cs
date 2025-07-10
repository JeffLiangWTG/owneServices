using Enterprise.Registry.Business;

namespace Enterprise.Customs.BE.Business;

[RegistryEditor("Enterprise.Customs.BE.GUI.Registry.CustomsRegistryItemEditor, Enterprise.Customs.BE.GUI")]
public class CustomsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CustomsRegistryCollection>
{
}
