using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UserAccountDeactivationWrapper : DeactivationWrapperBase
	{
		public UserAccountDeactivationWrapper(EdiCustomerUserAccount userAccount, int referenceNumber) : base(userAccount.Factory)
		{
			UserAccount = userAccount;
			OriginalIsContactRelationshipActive = userAccount.EUA_IsContactRelationshipActive;
			ReferenceNumber = referenceNumber;
		}

		public EdiCustomerUserAccount UserAccount { get; }
		public ZBool OriginalIsContactRelationshipActive { get; }
		public override int ReferenceNumber { get; }

		public override ZString LicenceType => new DatabaseTypes().GetDescriptionFromCode(UserAccount.Database.LD_LicenceType);

		public override ZString Organisation => ZString.Empty;

		public override ZString SystemInfo => UserAccount.GetSystemText();
	}
}
