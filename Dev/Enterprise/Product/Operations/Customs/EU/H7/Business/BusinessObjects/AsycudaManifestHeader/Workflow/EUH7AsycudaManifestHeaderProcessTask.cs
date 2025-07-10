using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.H7.Business
{
	public class EUH7AsycudaManifestHeaderProcessTask : ProcessTask
	{
		public EUH7AsycudaManifestHeaderProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type ParentType => typeof(AsycudaManifestHeader);
		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;
		public override ControllerID ParentControllerID => ControllerIDs.Customs.EU.EUH7;
	}
}
