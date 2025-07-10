using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Core.Modules
{
	public interface IMainForm
	{
		void UpdateToolBarDeleteButton(ZEmbeddedModule embeddedModule);

		INamedModule CurrentModule { get; }
		string CurrentModuleLicenceCheckPointName { get; }
	}
}
