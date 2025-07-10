using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.ScimApiTokenAuthenticationRegistryItemEditor, Enterprise.Registry.GUI")]
	public class ScimApiTokenAuthenticationDataType : StringRegistryDataType
	{
		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return null;
		}

		protected override bool HasDefaultEditorInfoCore => false;
	}
}
