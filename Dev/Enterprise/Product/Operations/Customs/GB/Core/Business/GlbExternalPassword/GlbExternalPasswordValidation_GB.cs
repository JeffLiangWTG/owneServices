using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business
{
	public class GlbExternalPasswordValidation_GB : GlbExternalPasswordValidation
	{
		public GlbExternalPasswordValidation_GB(GlbExternalPassword_GB parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateEORI();
			ValidateBadge();
		}

		public void ValidateBadge()
		{
			ValidateCalculatedProperty(Parent.BadgeInfo);
		}

		public void ValidateEORI()
		{
			ValidateCalculatedProperty(Parent.EORIInfo);
		}

		protected new GlbExternalPassword_GB Parent
		{
			get { return (GlbExternalPassword_GB)base.Parent; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ZPropertyInfo validation method call via reflection. Origin call is ValidateBadge")]
		void CheckBadge()
		{
			var info = Parent.BadgeInfo;
			if (info.Value.IsEmpty)
			{
				info.AddWarning(MandatoryValidation.YouHaveNotEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(info)));
			}
			else
			{
				ListValidation.WarnIfInvalidCode(info, ((GlbExternalPasswordLookups_GB)Parent.Lookups).BadgeCodes);
			}

			CheckForDuplicatePassword(info);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ZPropertyInfo validation method call via reflection. Origin call is ValidateEORI")]
		void CheckEORI()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.EORIInfo, ((GlbExternalPasswordLookups_GB)Parent.Lookups).EORIs);

			CheckForDuplicatePassword(Parent.EORIInfo);
		}

		void CheckForDuplicatePassword(ZPropertyInfo info)
		{
			var password = GetPassword();

			if (IsDuplicatePassword(password))
			{
				var error = GetDuplicatePasswordValidationError((ZString)GetCompany(password.GP_GC)?.GC_Code);
				if (GlbStaff.CurrentUser.IsSupportUser || GlbStaff.CurrentUser.GS_IsController)
				{
					info.AddMessageError(error);
				}
				else
				{
					info.AddError(error);
				}
			}
		}

		bool IsDuplicatePassword(GlbExternalPassword password) => password != null && !password.PK.Equals(Parent.PK);

		GlbExternalPassword GetPassword()
		{
			GlbExternalPassword result = null;

			if (!string.IsNullOrEmpty(Parent.GP_UserID))
			{
				var query = new ZDBOnlyQuery(typeof(GlbExternalPassword));
				query.AddToFilter(GlbExternalPasswordSchema.GP_UserID, Parent.GP_UserID);   // eori and badge here
				var matches = Parent.Factory.Load<GlbExternalPassword>(query);
				if (matches.Any())
				{
					result = matches.FirstOrDefault();
				}
			}

			return result;
		}

		GlbCompany GetCompany(ZGuid guid)
		{
			return Parent.Factory.Load<GlbCompany>(guid);
		}

		ZString GetDuplicatePasswordValidationError(ZString companyCode) => string.Format(CultureInfo.CurrentCulture, SupervisingOfficeDoesNotExist_ErrorMessage, companyCode);

		const string SupervisingOfficeDoesNotExist_ErrorMessage = "The EORI and Badge combination used is not unique. This has already been used for company {0}";
	}
}
