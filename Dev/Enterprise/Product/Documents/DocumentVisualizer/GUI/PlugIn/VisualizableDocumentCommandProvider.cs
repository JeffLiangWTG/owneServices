using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentVisualizer.GUI
{
	public sealed class VisualizableDocumentCommandProvider : IVisualizableDocumentCommandProvider
	{
		public IVisualizableDocumentCommand GetCommand(BusinessObject bizObj, IStmMenuItem menuItem, ModuleIdentifier moduleID)
		{
			if (bizObj == null
				|| menuItem == null)
			{
				return null;
			}

			return bizObj.GetVisualizableDocumentCommand(menuItem, moduleID);
		}
	}
}