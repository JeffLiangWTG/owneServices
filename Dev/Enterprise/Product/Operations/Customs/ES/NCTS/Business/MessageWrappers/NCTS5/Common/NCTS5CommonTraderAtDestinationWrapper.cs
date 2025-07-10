using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonTraderAtDestinationWrapper : PartyIdWrapper
	{
		public static new NCTS5CommonTraderAtDestinationWrapper New(JobDocAddress jobDocAddress) => jobDocAddress?.Address?.Header == null ? null : new NCTS5CommonTraderAtDestinationWrapper(jobDocAddress?.Address?.Header);

		NCTS5CommonTraderAtDestinationWrapper(OrgHeader orgH) : base(orgH)
		{
		}

		protected override ZString IdCore
		{
			get
			{
				var (id, isNaturalPersonIndividual) = GetIdForNaturalPerson();
				return isNaturalPersonIndividual ? id : base.IdCore;
			}
		}
	}
}
