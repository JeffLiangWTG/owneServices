using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	[CodeAlive("Added in WI00533151. For future use.")]
	public class NCTS5CommonRepresentativeAtDestinationWrapper : PartyIdWrapper
	{
		public static new NCTS5CommonRepresentativeAtDestinationWrapper New(OrgHeader orgHeader) => orgHeader == null ? null : new NCTS5CommonRepresentativeAtDestinationWrapper(orgHeader);

		public NCTS5CommonRepresentativeAtDestinationWrapper(OrgHeader orgH) : base(orgH)
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
