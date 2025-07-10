using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.JAS.Registry.Business
{
	public class CognosModeMappingRegistryItem : StronglyTypedRegistryItem<CognosModeMapping>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public CognosModeMappingRegistryItem(string category)
			: base(new RegistryItemImpl("CognosModeMapping", (NoResString)category, (NoResString)"Cognos Mode Mapping", (NoResString)"Please Map COGNOS modes to CargoWise One Departments", new CognosModeMappingRegistryDataType(), RegistryStorageFlags.System))
		{
		}
	}
}
