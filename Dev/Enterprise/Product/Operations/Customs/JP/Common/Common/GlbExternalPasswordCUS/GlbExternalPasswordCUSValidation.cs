using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Common
{
	public class GlbExternalPasswordCUSValidation : GlbExternalPasswordWithPasswordTypeValidation
	{
		public GlbExternalPasswordCUSValidation(GlbExternalPasswordCUS parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateGP_Transport();
		}

		public void ValidateGP_Transport()
		{
			ValidateCalculatedProperty(Parent.GP_TransportInfo);
		}

		public override void ValidateDuplicateConstraint()
		{
			if (!Parent.IsInDatabase ||
				Parent.GP_GSInfo.HasChanges ||
				Parent.GP_PasswordTypeInfo.HasChanges ||
				Parent.GP_UserIDInfo.HasChanges ||
				Parent.GP_MailBoxIDInfo.HasChanges)
			{
				var filter = new ZQuery(GlbExternalPasswordSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				filter.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, Parent.GP_PasswordType);
				filter.AddToFilter(GlbExternalPasswordSchema.GP_UserID, Parent.GP_UserID);
				filter.AddToFilter(GlbExternalPasswordSchema.GP_MailBoxID, Parent.GP_MailBoxID);

				if (Parent.Factory.LoadTop1<GlbExternalPassword>(filter) != null)
				{
					Parent.AddRowError(Res.GetString("7EC157FE-B832-49D4-804A-EF88B715F0E9", "Password Type, User Code and User ID must be unique. {0} + {1} + {2} already exists. ", Parent.GP_PasswordType, Parent.GP_MailBoxID, Parent.GP_UserID));
				}
			}
		}

		protected override void CheckGP_UserID()
		{
			base.CheckGP_UserID();
			MandatoryValidation.CheckEntered(Parent.GP_UserIDInfo);
			if (Parent.GP_UserID.Length > 0 && Parent.GP_UserID.Length != 3)
			{
				Parent.GP_UserIDInfo.AddError(Res.GetString("491C7D9A-B6A9-45F9-8D10-A7FA876FAC56", "{0} must be exactly 3 characters long.", Parent.GP_UserIDInfo.HumanReadableName));
			}
			else if (Parent.GP_UserID.Length > 0 && !Regex.IsMatch(Parent.GP_UserID, ".*[0-9]$"))
			{
				Parent.GP_UserIDInfo.AddError(Res.GetString("7B2A2E01-BDF1-4443-9939-D00E0E7F376F", "{0} must end with a digit", Parent.GP_UserIDInfo.HumanReadableName));
			}
		}

		protected override void CheckCurrentDecryptedPassword()
		{
			base.CheckCurrentDecryptedPassword();
			MandatoryValidation.CheckEntered(Parent.CurrentDecryptedPasswordInfo);
			if (Parent.CurrentDecryptedPassword.Length > 0 && Parent.CurrentDecryptedPassword.Length != 8)
			{
				Parent.CurrentDecryptedPasswordInfo.AddError(Res.GetString("9D7696C8-49B3-4EC2-A296-2AA99C09EC2F", "Password must be exactly 8 characters long."));
			}
		}

		protected void CheckGP_Transport()
		{
			MandatoryValidation.CheckEntered(Parent.GP_TransportInfo);
			ListValidation.ErrorIfInvalidCode(Parent.GP_TransportInfo);
		}

		protected override void CheckGP_MailBoxID()
		{
			MandatoryValidation.CheckEntered(Parent.GP_MailBoxIDInfo);
			if (Parent.GP_MailBoxID.Length > 0 && Parent.GP_MailBoxID.Length != 5)
			{
				Parent.GP_MailBoxIDInfo.AddError(Res.GetString("2FFE28DF-884A-45DD-AD87-67540D62E21D", "{0} must be exactly 5 characters long.", Parent.GP_MailBoxIDInfo.HumanReadableName));
			}
		}

		protected override void CheckGP_PasswordType()
		{
			MandatoryValidation.CheckEntered(Parent.GP_PasswordTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.GP_PasswordTypeInfo);
		}

		protected new GlbExternalPasswordCUS Parent => (GlbExternalPasswordCUS)base.Parent;
	}
}
