using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentVisualizer.GUI
{
	public sealed class VisualizerMenuCustomisationFormCreator : IVisualizerMenuCustomisationFormCreator
	{
		public IMenuCustomisationForm CreateVisualizerMenuCustomisationForm(BusinessObject bizObj)
		{
			if (bizObj == null)
			{
				return null;
			}

			var factory = new BusinessObjectFactory() { NameForDebugging = "VisualizerMenuCustomisation" };
			var businessContext = new DocumentEngineHelper().GetBusinessContext(bizObj).ToString();
			var menuCustomisation = new VisualizerMenuCustomisation(bizObj, factory, businessContext);

			return new VisualizerMenuCustomisationForm(menuCustomisation);
		}

		public ISecurityCheckpoint GetCustomizeFormCheckpoint(BusinessObject bizObj)
		{
			var supporter = bizObj.GetSupporter();
			return supporter != null ? supporter.CustomizeFormCheckpoint : Env.Security.None;
		}

		public ZGuid[] GetExcludedClientTemplatePKs()
		{
			return ClientMenuCustomisationHelper.GetExcludedTemplatePKs();
		}
	}
}
