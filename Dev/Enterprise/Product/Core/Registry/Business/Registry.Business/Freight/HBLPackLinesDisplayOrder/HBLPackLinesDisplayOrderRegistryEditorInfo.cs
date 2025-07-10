using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.HBLPackLinesDisplayOrderRegistryItemEditor, Enterprise.Registry.GUI")]
	public class HBLPackLinesDisplayOrderRegistryEditorInfo : ComboBoxRegistryEditorInfo
	{
		public HBLPackLinesDisplayOrderRegistryEditorInfo(ICodeDescriptionPairListProvider lookUpList)
			: base(lookUpList)
		{
		}
	}
}
