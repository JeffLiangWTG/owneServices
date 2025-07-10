using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public interface ICodeDescriptionBoolTreeNodeGridExtension
	{
		CodeDescriptionBoolControl ParentGrid { get; }
		CodeDescriptionBoolControl ChildGrid { get; }

		ZGrid InnerGrid { get; }

		void SetLevel(CodeDescriptionBoolControl parent, CodeDescriptionBoolControl child);
	}
}
