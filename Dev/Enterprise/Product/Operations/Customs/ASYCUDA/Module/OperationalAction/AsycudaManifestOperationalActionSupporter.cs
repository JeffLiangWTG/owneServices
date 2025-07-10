using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.ASYCUDA.Module
{
	sealed class AsycudaManifestOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType => typeof(Business.AsycudaManifestHeader);

		public override BusinessContext BusinessContext => BusinessContext.AsycudaManifest;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.AsycudaManifestReporting;

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.INManifest);
		}
	}
}
