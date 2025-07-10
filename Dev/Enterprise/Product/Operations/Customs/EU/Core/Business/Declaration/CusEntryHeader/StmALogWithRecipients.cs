using System.Collections.Generic;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class StmALogWithRecipients
	{
		public StmALogWithRecipients(RecipientRoleType[] recipientRoleTypes, StmALog eventBO)
		{
			RecipientRoleTypes = recipientRoleTypes;
			EventBO = eventBO;
		}

		public IReadOnlyList<RecipientRoleType> RecipientRoleTypes { get; }

		public StmALog EventBO { get; }
	}
}
