//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiOrgMembershipValidation
//
//    This class should be used for overriding validation in AutoEdiOrgMembershipValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiOrgMembershipValidation : AutoEdiOrgMembershipValidation
	{
		public EdiOrgMembershipValidation(AutoEdiOrgMembership parent) : base(parent)
		{
		}

		protected override void CheckEOR_MembershipType()
		{
			var info = Parent.EOR_MembershipTypeInfo;
			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info);
		}

		protected override void CheckEOR_OH_Organisation()
		{
			ZGuid ohOrg = Parent.EOR_OH_Organisation;
			string latestMemershipType = ((EdiOrgMembership)Parent).LatestMembershipType;
			bool isOrganisationRequired = EDIDataRegistry.Instance.OrgMembershipTypes.Value.GetActiveCodeDescriptionPairList().GetAllCodes().Contains(latestMemershipType);
			if (isOrganisationRequired && ohOrg.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.EOR_OH_OrganisationInfo);
			}
		}

		protected override void CheckEOR_ValidFromIsValidZDateRange()
		{
		}

		protected override void CheckEOR_ValidFrom()
		{
			var info = Parent.EOR_ValidFromInfo;
			MandatoryValidation.CheckEntered(info);
		}

		protected override void CheckEOR_ValidToIsValidZDateRange()
		{
		}

		protected override void CheckEOR_ValidTo()
		{
			var parent = (EdiOrgMembership)Parent;
			var value = parent.EOR_ValidTo;
			if (!value.IsEmpty && value.IsValid && parent.EOR_ValidFrom.IsValid)
			{
				CompareValidation.CheckDateIsAfterAnotherDate(Parent.EOR_ValidToInfo, parent.EOR_ValidFrom);
			}

			if (parent.EOR_ValidFrom.IsValid &&
				!parent.EOR_ValidFrom.IsEmpty &&
				!parent.EOR_MembershipType.IsEmpty)
			{
				foreach (var other in ((EDIOrgHeader)Parent.Header).Memberships)
				{
					if (other.PK != Parent.PK &&
						other.EOR_MembershipType == parent.EOR_MembershipType &&
						DateRange.HasOverlap(parent.EOR_ValidFrom, parent.EOR_ValidTo, other.EOR_ValidFrom, other.EOR_ValidTo))
					{
						Parent.EOR_ValidToInfo.AddError(Res.GetString("A95E9215-B6D0-41EA-BF7E-8266F6D30962", "Dates overlap with another membership entry of this type."));
						break;
					}
				}
			}
		}

		protected override void CheckEOR_AgreementVersion()
		{
			var parent = (EdiOrgMembership)Parent;
			var value = parent.EOR_AgreementVersion;
			var validTo = parent.EOR_ValidTo;
			var validFrom = parent.EOR_ValidFrom;
			if (!value.IsEmpty)
			{
				if (validFrom.IsEmpty)
				{
					Parent.EOR_AgreementVersionInfo.AddError(Res.GetString("84C891FF-B7F3-4D5A-82CD-438DB345309F", "The Valid-From date must be set before the agreement version"));
				}
				else if (value < parent.EOR_ValidFrom && validTo.IsEmpty)
				{
					Parent.EOR_AgreementVersionInfo.AddError(Res.GetString("D0FFED0C-6130-456F-BB33-F18D735E4418", "Agreement version date cannot be earlier than the Valid-From date"));
				}
				else if (value < parent.EOR_ValidFrom || value > parent.EOR_ValidTo)
				{
					Parent.EOR_AgreementVersionInfo.AddError(Res.GetString("93404E08-3140-419D-8D01-DBA5DE83E5D4", "Agreement version date must be between the Valid-From and Valid-To dates"));
				}
			}
		}
	}
}

