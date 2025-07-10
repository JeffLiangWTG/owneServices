using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.Business.MessagesWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaGJobDeclarationValidation : JobDeclarationValidation
	{
		public DeltaGJobDeclarationValidation(JobDeclaration parent) : base(parent)
		{
		}

		protected override void CheckJE_LocationOfGoods()
		{
			base.CheckJE_LocationOfGoods();

			var parent = Parent;

			if(parent.JE_DeltaMode == OrgCusAccountDeltaGTypeList.Codes.G2 && parent.JE_LocationOfGoods.IsEmpty)
			{
				parent.JE_LocationOfGoodsInfo.AddMessageError(Res.GetString("b71f8755-ee74-4511-a930-f7c879d527af", "An authorized location is mandatory when doing G2."));
			}
		}

		protected void CheckJE_DeltaMode()
		{
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.JE_DeltaModeInfo);

			if (parent.JE_DeltaMode == OrgCusAccountDeltaGTypeList.Codes.G1)
			{
				if (CheckAuthorizationOwnerWithoutZO_DeltaG1SubProcedure(parent.DeltaAccountOrgHeader))
				{
					parent.JE_DeltaModeInfo.AddMessageError(Res.GetString("A672DAC3-AA9D-4EF3-A0BD-124A0F8A1745", "The agreement owner Delta G1 sub procedure has not been set yet."));
				}
			}
		}

		bool CheckAuthorizationOwnerWithoutZO_DeltaG1SubProcedure(OrgHeader header)
		{
			var result = false;

			if (header != null)
			{
				var orgImpAddInfo = FROrgImpAddInfo.Get(header);
				var deltaAgreementNumberCollection = header.DeltaAgreementNumberCollection;
				if (orgImpAddInfo.ZO_DeltaG1SubProcedure.IsEmpty && deltaAgreementNumberCollection != null && deltaAgreementNumberCollection.Cast<OrgCusAccount>().Any(account => account.CZ_Type == OrgCusAccountDeltaGTypeList.Codes.G1))
				{
					result = true;
				}
			}

			return result;
		}

		protected override string AdditionalSpecificErrorMessageForNeededACODGuarantee => Res.GetString("3CCB6181-BD1C-483C-B4C6-8B6267DCF0FC", ", matching Delta mode {0} for use at the selected address", Parent.JE_DeltaMode);

		protected override void CheckJE_OA_DeclarantAddress()
		{
			base.CheckJE_OA_DeclarantAddress();

			var registrationNumber = ZString.Empty;
			var parent = Parent;
			var header = parent.DeclarantAddress?.Header;
			if (header.GetEORI().IsEmpty)
			{
				Parent.JE_OA_DeclarantAddressInfo.AddMessageError(ErrorCollectorHelper.DeclarantEoriNotConfigured);
			}
		}

		protected override void CheckJE_CustomsProfile()
		{
			base.CheckJE_CustomsProfile();

			var parent = Parent;
			var declarantAgreements = parent.Declarant?.Header?.DeltaAgreementNumberCollection;
			var importerAgreements = parent.Importer?.DeltaAgreementNumberCollection;
			if (declarantAgreements.IsNullOrEmpty() && importerAgreements.IsNullOrEmpty())
			{
				parent.JE_CustomsProfileInfo.AddMessageError(Res.GetString("7FE772C3-4413-4759-92AE-692FA79FB15C", "Importer or Declarant must have an agreement number."));
			}

			if (!parent.CustomsProfileAndDeltaModeMatch)
			{
				var isSelectedProfileValid = !parent.JE_CustomsProfile.IsEmpty && parent.Lookups.ProfileList.ContainsCode(parent.JE_CustomsProfile);
				var isSelectedDeltaModeValid = parent.JE_DeltaMode != ZString.Empty && parent.Lookups.DeltaModeList.ContainsCode(parent.JE_DeltaMode);

				var errorMessage = Res.GetString("7EA10CB7-8B3B-49B0-85DB-6224DFC16E73", "Delta mode and profile don't match. To resolve this, please ");
				var selectProfileErrorMessage = Res.GetString("752FBE90-AD2B-44CB-9708-67D0209039FB", "select a profile that supports Delta mode {0}", parent.JE_DeltaMode);

				if (isSelectedProfileValid)
				{
					var selectedProfileAccounts = parent.DeltaAccounts.Where(x => x.CZ_Account == parent.JE_CustomsProfile);
					if (selectedProfileAccounts.Count() == 1)
					{
						errorMessage += Res.GetString("1097C4EF-8499-403E-B034-BCDE93865BFF", "select Delta mode {0}", selectedProfileAccounts.First().CZ_Type);
					}
					else
					{
						errorMessage += Res.GetString("A30180E5-0AB7-45FA-816A-1F478DCC4973", "select a Delta mode matching the profile", selectedProfileAccounts.FirstOrDefault()?.CZ_Type);
					}

					if (isSelectedDeltaModeValid)
					{
						errorMessage += Res.GetString("3EAC1EB0-4C2C-4AD9-81F2-19C5E63219FA", " or ");
						errorMessage += selectProfileErrorMessage;
					}
				}
				else
				{
					if (isSelectedDeltaModeValid)
					{
						errorMessage += selectProfileErrorMessage;
					}
					else
					{
						errorMessage += Res.GetString("27AF62D4-04B1-4ACA-9D08-E8FA964E0C14", "select a valid profile and a Delta mode matching the profile.");
					}
				}

				parent.JE_CustomsProfileInfo.AddMessageError(errorMessage);
			}
		}
	}
}
