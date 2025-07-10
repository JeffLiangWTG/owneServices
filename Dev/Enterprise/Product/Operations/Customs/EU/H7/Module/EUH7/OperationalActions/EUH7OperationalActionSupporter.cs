using System;
using CargoWise.Definitions;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.EU.H7.Module;

public class EUH7OperationalActionSupporter : OperationalActionSupporter
{
	public override string SingularElementNoun => Res.GetString("EUH7OperationalActionSupporter|SingularElementNoun", "job");

	public override string PluralElementNoun => Res.GetString("EUH7OperationalActionSupporter|PluralElementNoun", "jobs");

	public override Type RootType => typeof(AsycudaManifestHeader);

	public override BusinessContext BusinessContext => BusinessContext.AsycudaManifest;

	public override SecurityCheckpoint BaseCheckpoint => Env.Security.EuH7;

	protected override void PopulateMethods(OperationalActionMethodList list)
	{
		base.PopulateMethods(list);
		list.Add(ActionMethodProviderIDs.EUH7);
	}
}
