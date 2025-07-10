using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

static class DeclarationMessageProcessorHelper
{
	public static ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject) => linkedObject is CusEntryHeader entry ? entry.RegistryBranchPK : ZGuid.Empty;

	public static ZString NoteForUnableToFindALinkedBusinessObject => Res.GetString("3bb04e4d-a5c2-4562-9633-800723c27fb6", "The processing of the message with interchange failed because the message could not be linked to a declaration.");
}
