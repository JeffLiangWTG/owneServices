using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AESCommonExporterWrapper : PartyIdWrapper
	{
		public static new AESCommonExporterWrapper New(OrgHeader orgHeader) => orgHeader == null ? null : new AESCommonExporterWrapper(orgHeader);

		protected AESCommonExporterWrapper(OrgHeader orgH) : base(orgH)
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
