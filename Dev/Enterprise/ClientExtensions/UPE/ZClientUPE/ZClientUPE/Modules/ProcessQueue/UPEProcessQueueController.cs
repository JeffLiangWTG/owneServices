using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.UPE.GUI;

using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.UPE.Module
{
	public class UPEProcessQueueController : ProcessQueueController
	{
		public UPEProcessQueueController()
		{
		}

		public override ResourceStringData PluginTabPageCaption => Res.GetData("98f85b5b-6964-454a-b3dc-23ee207ffa01", "Process Queue");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new UPEProcessQueuePlugin(businessEntity as IProcessQueueParent);
		}
	}
}
