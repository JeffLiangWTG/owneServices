using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IVisualizableDocumentCommandProvider
	{
		IVisualizableDocumentCommand GetCommand(BusinessObject bizObj, IStmMenuItem menuItem, ModuleIdentifier moduleID);
	}
}