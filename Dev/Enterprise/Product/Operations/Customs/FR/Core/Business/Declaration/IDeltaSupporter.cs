using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public interface IDeltaSupporter
	{
		ZBool IsDeltaDStepOneSentOK { get; }

		ZBool IsDeltaDStepTwoSentOK { get; }

		ZString DeltaMode { get; }
	}
}
