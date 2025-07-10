using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationDVDAuthorisationWrapper : IDeclarationDVDAuthorisation
	{
		public DeclarationDVDAuthorisationWrapper(CusAuthorizationUsage authorization)
		{
			this.authorization = Argument.NotNull(authorization, nameof(authorization));
		}
		readonly CusAuthorizationUsage authorization;

		public ZString Type => authorization.AGC_Code;

		public ZString OwnerId => OrgHeaderExtension.GetIDCode(authorization.Owner);
	}
}
