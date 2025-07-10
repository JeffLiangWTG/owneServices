using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Visualisation
{
	public interface IVisualizerMenuCustomisationFormCreator
	{
		IMenuCustomisationForm CreateVisualizerMenuCustomisationForm(BusinessObject bizObj);
		ISecurityCheckpoint GetCustomizeFormCheckpoint(BusinessObject bizObj);
		ZGuid[] GetExcludedClientTemplatePKs();
	}
}
