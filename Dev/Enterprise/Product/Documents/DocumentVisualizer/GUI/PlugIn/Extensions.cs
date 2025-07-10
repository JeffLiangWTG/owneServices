using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentVisualizer.GUI
{
	public static class Extensions
	{
		public static IVisualizableDocumentCommand GetVisualizableDocumentCommand(this BusinessObject bizObj, IStmMenuItem menuItem, ModuleIdentifier moduleID)
		{
			Argument.NotNull(bizObj, nameof(bizObj));
			Argument.NotNull(bizObj is IDocumentSupportable, nameof(bizObj));
			Argument.NotNull(menuItem, nameof(menuItem));

			return new VisualizableDocumentCommand(bizObj, menuItem, moduleID);
		}
	}
}
