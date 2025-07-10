using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Module
{
	public class AirCTOCusUnderbondPluginController : AirCTOImportController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.AirCTOCusUnderbondPluginController; }
		}

		internal ZPlugIn GetPlugInInternal(IBusiness businessEntity) => GetPlugIn(businessEntity);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new GUI.CMRCusUnderbondPlugin((IAUCusUnderbondUnionCollectionParent)businessEntity, ZString.Empty);
		}
	}
}
