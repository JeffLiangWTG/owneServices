using CargoWise.Integration;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.UserManagement.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class UserAgreementRequestValidationHelper
	{
		public static bool IsValidAgreementType(string type) => new EdiUserAgreementTypes().ContainsCode(type);

		public static bool CanBypassUserAccountCollectionAgreement(string product)
		{
			return ((ICodeDescriptionPairList)EDIDataRegistry.Instance.TrustedMessagingUserAgreementCheckBypassProducts.Value).ContainsCode(product.ToUpperInvariant());
		}
	}
}
