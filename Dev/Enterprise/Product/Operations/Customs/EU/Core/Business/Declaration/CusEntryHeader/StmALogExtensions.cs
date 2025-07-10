using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business.Declaration;

public static class StmALogExtensions
{
	public static StmALogWithRecipients WithRecipients(this StmALog eventBO, RecipientRoleType[] recipientRoleTypes)
	{
		return new StmALogWithRecipients(recipientRoleTypes, eventBO);
	}
}
