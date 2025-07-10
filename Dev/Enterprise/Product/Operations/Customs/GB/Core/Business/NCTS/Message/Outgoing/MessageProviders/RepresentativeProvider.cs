using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class RepresentativeProvider : PartyProvider, INCTSRepresentative
	{
		public static RepresentativeProvider New(JobDocAddress jobDocAddress, bool isInPhase5TransitionPeriod) => jobDocAddress.IsValidAddress ? new RepresentativeProvider(jobDocAddress, isInPhase5TransitionPeriod) : null;

		RepresentativeProvider(JobDocAddress jobDocAddress, bool isInPhase5TransitionPeriod) : base(jobDocAddress, isInPhase5TransitionPeriod)
		{
		}

		public int Status => 2;

		protected override ZBool IncludeAddress => false;
	}
}
