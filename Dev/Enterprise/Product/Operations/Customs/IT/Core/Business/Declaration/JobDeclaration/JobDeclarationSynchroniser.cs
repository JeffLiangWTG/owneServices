using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobDeclarationSynchroniser : EU.Business.Declaration.JobDeclarationSynchroniser
{
	public JobDeclarationSynchroniser(JobDeclaration destination)
		: base(destination)
	{
	}

	protected override void HookWeight()
	{
		Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalWeightInfo, Source.JS_ActualWeightInfo));
		Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalWeightUnitInfo, Source.JS_UnitOfWeightInfo));
	}

	protected override void HookVolume()
	{
		Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalVolumeInfo, Source.JS_ActualVolumeInfo));
		Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalVolumeUnitInfo, Source.JS_UnitOfVolumeInfo));
	}

	protected override void AddPacksSynchroniser()
	{
		Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalNoOfPacksInfo, Source.JS_OuterPacksInfo));
		Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalNoOfPacksPackTypeInfo, Source.JS_F3_NKPackTypeInfo));
	}

	protected override IZType GetCTStatusID()
	{
		var sourceCTStatus = base.GetCTStatusID();
		if (Destination is JobDeclaration declaration && declaration.AddInfoLookups.CommunityTransitStatusIDList.ContainsCode(sourceCTStatus))
		{
			return sourceCTStatus;
		}
		return ZString.Empty;
	}

	protected override Customs.Business.PackingSynchroniser GetPackingSynchroniser()
	{
		return new PackingSynchroniser(this, Destination);
	}
}
