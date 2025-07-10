using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BE.NCTS.Business
{
	static class NCTSMessageProcessorHelper
	{
		public static ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject) => linkedObject is NctsHeader header ? header.RegistryBranchPK : ZGuid.Empty;

		public static  ZString NoteForUnableToFindALinkedBusinessObject => Res.GetString("847CC8BA-0AE7-4725-B56E-371138C3054A", "The processing of the message with interchange failed because the message could not be linked to a NCTS declaration.");
	}
}
