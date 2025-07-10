//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiUserAgreementAcceptanceLogValidation
//
//    This class should be used for overriding validation in AutoEdiUserAgreementAcceptanceLogValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiUserAgreementAcceptanceLogValidation : AutoEdiUserAgreementAcceptanceLogValidation
	{
		public EdiUserAgreementAcceptanceLogValidation(AutoEdiUserAgreementAcceptanceLog parent) : base(parent)
		{
		}

		public bool IsAddedManually => (Parent as EdiUserAgreementAcceptanceLog)?.IsAddedManually ?? false;

		protected override void CheckEUL_EUA()
		{
			base.CheckEUL_EUA();

			if (Parent.EUL_EUA.IsEmpty)
			{
				if (Parent.EUL_LE.IsEmpty && Parent.EUL_LD.IsEmpty && Parent.EUL_OH.IsEmpty && Parent.EUL_GS.IsEmpty)
				{
					Parent.EUL_EUAInfo.AddError(EnterUserAccountOrStaffAndOrganisationMessage);
				}
			}
			else
			{
				if (!Parent.EUL_OH.IsEmpty || !Parent.EUL_GS.IsEmpty)
				{
					Parent.EUL_EUAInfo.AddError(OnlyUserAccountOrStaffAndOrganisationMessage);
				}
			}
		}

		protected override void CheckEUL_GS()
		{
			base.CheckEUL_GS();

			if (Parent.EUL_GS.IsEmpty)
			{
				if (Parent.EUL_LE.IsEmpty && Parent.EUL_LD.IsEmpty && Parent.EUL_EUA.IsEmpty)
				{
					Parent.EUL_GSInfo.AddError(EnterUserAccountOrStaffAndOrganisationMessage);
				}
			}
			else if (!Parent.EUL_EUA.IsEmpty)
			{
				Parent.EUL_GSInfo.AddError(OnlyUserAccountOrStaffAndOrganisationMessage);
			}
		}

		protected override void CheckEUL_OH()
		{
			base.CheckEUL_OH();

			if (Parent.EUL_OH.IsEmpty)
			{
				if (Parent.EUL_LE.IsEmpty && Parent.EUL_LD.IsEmpty && Parent.EUL_EUA.IsEmpty)
				{
					Parent.EUL_OHInfo.AddError(EnterUserAccountOrStaffAndOrganisationMessage);
				}
			}
			else if (!Parent.EUL_EUA.IsEmpty)
			{
				Parent.EUL_OHInfo.AddError(OnlyUserAccountOrStaffAndOrganisationMessage);
			}
		}

		protected override void CheckEUL_AcceptedByName()
		{
			base.CheckEUL_AcceptedByName();
			if (IsAddedManually)
			{
				MandatoryValidation.CheckEntered(Parent.EUL_AcceptedByNameInfo);
			}
		}

		protected override void CheckEUL_AcceptedByJobTitle()
		{
			base.CheckEUL_AcceptedByJobTitle();
			if (IsAddedManually)
			{
				MandatoryValidation.CheckEntered(Parent.EUL_AcceptedByJobTitleInfo);
			}
		}

		protected override void CheckEUL_AcceptedByEmail()
		{
			base.CheckEUL_AcceptedByEmail();
			EmailAddressValidation.ValidateEmailAddress(Parent.EUL_AcceptedByEmailInfo);
			if (IsAddedManually)
			{
				MandatoryValidation.CheckEntered(Parent.EUL_AcceptedByEmailInfo);
			}
		}

		ZString EnterUserAccountOrStaffAndOrganisationMessage => Res.GetString("d8aeeaea-15a5-4322-a24f-19fa1323949e", "Please enter a user account or a staff member or a organization and enterprise.");
		ZString OnlyUserAccountOrStaffAndOrganisationMessage => Res.GetString("7710c27b-26ed-4590-a606-5ca064fb1551", "Please choose only a user account or a staff + organization combination for the agreement.");
	}
}
