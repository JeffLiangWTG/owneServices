using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public class GlbMauExternalPasswordValidation : GlbExternalPasswordValidation
{
	public GlbMauExternalPasswordValidation(GlbMauExternalPassword parent) : base(parent)
	{
	}

	protected new GlbMauExternalPassword Parent => (GlbMauExternalPassword)base.Parent;

	protected override void CheckGP_UserID()
	{
		base.CheckGP_UserID();

		var targetPropertyInfo = Parent.GP_UserIDInfo;
		var userID = Parent.GP_UserID;

		new InternalCodeValidation()
			.CheckInternalCode(targetPropertyInfo, userID);

		CheckInternalCodeMustBeUnique(targetPropertyInfo, userID);
	}

	protected override void CheckGP_Name()
	{
		base.CheckGP_Name();

		var targetPropertyInfo = Parent.GP_NameInfo;
		MandatoryValidation.CheckEntered(targetPropertyInfo);
		ListValidation.ErrorIfInvalidCode(targetPropertyInfo);
	}

	protected override void CheckGP_MailBoxID()
	{
		base.CheckGP_MailBoxID();

		var targetPropertyInfo = Parent.GP_MailBoxIDInfo;
		MandatoryValidation.CheckEntered(targetPropertyInfo);
		CheckAuthorizedUserFormat(targetPropertyInfo, Parent.GP_MailBoxID);
	}

	#region Implementation

	void CheckInternalCodeMustBeUnique(ZPropertyInfo targetPropertyInfo, ZString userID)
	{
		if (userID.IsEmpty)
		{
			return;
		}

		var parentCollection = GlbCompanyWrapper.Get(Parent.Company)
			?.PasswordCollection
			?.Cast<GlbMauExternalPassword>()
			?? Enumerable.Empty<GlbMauExternalPassword>();

		if (parentCollection.Any(x => x.GP_UserID == userID && x.PK != Parent.PK))
		{
			targetPropertyInfo.AddError(ValidationCaptions.GlbMauExternalPassword.InternalCodeMustBeUnique);
		}
	}

	void CheckAuthorizedUserFormat(ZPropertyInfo targetPropertyInfo, ZString authorizedUser)
	{
		if (authorizedUser.IsEmpty)
		{
			return;
		}

		var cusCodeValidator = ITCusCodeValidationSelector.GetCODCodeValidator(authorizedUser);
		var validationResult = cusCodeValidator.Validate(authorizedUser);
		if (validationResult == ITCusCodeValidationResult.InvalidLength || validationResult == ITCusCodeValidationResult.InvalidPattern)
		{
			targetPropertyInfo.AddMessageError(ValidationCaptions.GlbMauExternalPassword.AuthorizedUserFormatMustBeValid);
		}
	}

	#endregion
}
