using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class CodeDescriptionBoolRelatedItemRegistryControlForTest : CodeDescriptionBoolRelatedItemRegistryControl
	{
		internal ZGrid CodeDescriptionBoolRelatedItemGridExposed
		{
			get
			{
				return CodeDescriptionBoolRelatedItemGrid;
			}
		}
	}
}
