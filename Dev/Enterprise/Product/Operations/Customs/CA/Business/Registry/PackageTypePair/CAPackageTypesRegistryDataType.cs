using Enterprise.Registry.Business;

namespace Enterprise.Customs.CA.Registry;

[RegistryEditor("Enterprise.Customs.CA.GUI.CAPackageTypePairsRegistryItemEditor, Enterprise.Customs.CA.GUI")]
public class CAPackageTypesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CAPackageTypePairCollection>
{
}
