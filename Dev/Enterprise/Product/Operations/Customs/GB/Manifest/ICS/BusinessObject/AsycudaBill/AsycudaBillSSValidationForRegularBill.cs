using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class AsycudaBillSSValidationForRegularBill : EU.Manifest.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillSSValidationForRegularBill(AsycudaBillSS parent)
			: base(parent)
		{
		}

		protected new AsycudaBillSS Parent => (AsycudaBillSS)base.Parent;

		protected override ZBool NeedsToShowABL_ManifestUQNotMappedMessageError => false;

		protected override void CheckABL_RL_NKOrigin()
		{
			if (!Parent.ABL_RL_NKOrigin.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ABL_RL_NKOriginInfo);
			}
		}

		protected override void CheckABL_RL_NKFinalDestination()
		{
			if (!Parent.ABL_RL_NKFinalDestination.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ABL_RL_NKFinalDestinationInfo);
			}
		}

		void AddMessageErrorIfNotifyPartyDeclared(ZPropertyInfo propertyInfo)
		{
			if (Parent.NotifyPartyDeclared)
			{
				propertyInfo.AddMessageError(HavingBothConsigneeAndNotifyPartyWillBeRejectedMessage);
			}
		}

		void AddMessageErrorIfConsigneeDeclared(ZPropertyInfo propertyInfo)
		{
			if (Parent.ConsigneeDeclared)
			{
				propertyInfo.AddMessageError(HavingBothConsigneeAndNotifyPartyWillBeRejectedMessage);
			}
		}

		protected override void CheckABL_ConsigneeName()
		{
			if (!Parent.ConsigneeUseRealOrg && !Parent.ABL_ConsigneeName.IsEmpty)
			{
				AddMessageErrorIfNotifyPartyDeclared(Parent.ABL_ConsigneeNameInfo);
			}
			else if (!Parent.NotifyPartyDeclared)
			{
				base.CheckABL_ConsigneeName();
			}
		}

		protected override void CheckABL_ConsigneeStreet1()
		{
			if (!Parent.ConsigneeUseRealOrg && !Parent.ABL_ConsigneeStreet1.IsEmpty)
			{
				AddMessageErrorIfNotifyPartyDeclared(Parent.ABL_ConsigneeStreet1Info);
			}
			else if (!Parent.NotifyPartyDeclared)
			{
				base.CheckABL_ConsigneeStreet1();
			}
		}

		protected override void CheckABL_ConsigneeCity()
		{
			if (!Parent.ConsigneeUseRealOrg && !Parent.ABL_ConsigneeCity.IsEmpty)
			{
				AddMessageErrorIfNotifyPartyDeclared(Parent.ABL_ConsigneeCityInfo);
			}
			else if (!Parent.NotifyPartyDeclared)
			{
				base.CheckABL_ConsigneeCity();
			}
		}

		protected override void CheckABL_RN_NKConsigneeCountry()
		{
			if (!Parent.ConsigneeUseRealOrg && !Parent.ABL_RN_NKConsigneeCountry.IsEmpty)
			{
				AddMessageErrorIfNotifyPartyDeclared(Parent.ABL_RN_NKConsigneeCountryInfo);
			}
			else if (!Parent.NotifyPartyDeclared)
			{
				base.CheckABL_RN_NKConsigneeCountry();
			}
		}

		protected override void CheckABL_ConsigneePostcode()
		{
			if (!Parent.ConsigneeUseRealOrg && !Parent.ABL_ConsigneePostcode.IsEmpty)
			{
				AddMessageErrorIfNotifyPartyDeclared(Parent.ABL_ConsigneePostcodeInfo);
			}
			else if (!Parent.NotifyPartyDeclared)
			{
				base.CheckABL_ConsigneePostcode();
			}
		}

		protected override void CheckABL_NotifyPartyName()
		{
			base.CheckABL_NotifyPartyName();
			if (!Parent.NotifyPartyUseRealOrg && !Parent.ABL_NotifyPartyName.IsEmpty)
			{
				AddMessageErrorIfConsigneeDeclared(Parent.ABL_NotifyPartyNameInfo);
			}
			if (NonOrganizationNotifyPartyUsed && Parent.ABL_NotifyPartyName.IsEmpty)
			{
				Parent.ABL_NotifyPartyNameInfo.AddMessageError(GetErrorForMissingNotifyPartyProperty(Parent.ABL_NotifyPartyNameInfo));
			}
		}

		protected override void CheckABL_NotifyPartyStreet1()
		{
			base.CheckABL_NotifyPartyStreet1();
			if (!Parent.NotifyPartyUseRealOrg && !Parent.ABL_NotifyPartyStreet1.IsEmpty)
			{
				AddMessageErrorIfConsigneeDeclared(Parent.ABL_NotifyPartyStreet1Info);
			}
			if (NonOrganizationNotifyPartyUsed && Parent.ABL_NotifyPartyStreet1.IsEmpty)
			{
				Parent.ABL_NotifyPartyStreet1Info.AddMessageError(GetErrorForMissingNotifyPartyProperty(Parent.ABL_NotifyPartyStreet1Info));
			}
		}

		protected override void CheckABL_NotifyPartyCity()
		{
			base.CheckABL_NotifyPartyCity();
			if (!Parent.NotifyPartyUseRealOrg && !Parent.ABL_NotifyPartyCity.IsEmpty)
			{
				AddMessageErrorIfConsigneeDeclared(Parent.ABL_NotifyPartyCityInfo);
			}
			if (NonOrganizationNotifyPartyUsed && Parent.ABL_NotifyPartyCity.IsEmpty)
			{
				Parent.ABL_NotifyPartyCityInfo.AddMessageError(GetErrorForMissingNotifyPartyProperty(Parent.ABL_NotifyPartyCityInfo));
			}
		}

		protected override void CheckABL_RN_NKNotifyPartyCountry()
		{
			base.CheckABL_RN_NKNotifyPartyCountry();
			if (!Parent.NotifyPartyUseRealOrg && !Parent.ABL_RN_NKNotifyPartyCountry.IsEmpty)
			{
				AddMessageErrorIfConsigneeDeclared(Parent.ABL_RN_NKNotifyPartyCountryInfo);
			}
			if (NonOrganizationNotifyPartyUsed && Parent.ABL_RN_NKNotifyPartyCountry.IsEmpty)
			{
				Parent.ABL_RN_NKNotifyPartyCountryInfo.AddMessageError(GetErrorForMissingNotifyPartyProperty(Parent.ABL_RN_NKNotifyPartyCountryInfo));
			}
		}

		protected override void CheckABL_NotifyPartyPostcode()
		{
			base.CheckABL_NotifyPartyPostcode();
			if (!Parent.NotifyPartyUseRealOrg && !Parent.ABL_NotifyPartyPostcode.IsEmpty)
			{
				AddMessageErrorIfConsigneeDeclared(Parent.ABL_NotifyPartyPostcodeInfo);
			}
			if (NonOrganizationNotifyPartyUsed && Parent.ABL_NotifyPartyPostcode.IsEmpty)
			{
				Parent.ABL_NotifyPartyPostcodeInfo.AddMessageError(GetErrorForMissingNotifyPartyProperty(Parent.ABL_NotifyPartyPostcodeInfo));
			}
		}

		protected override void CheckABL_OA_Consignee()
		{
			if (Parent.NotifyPartyDeclared && Parent.ConsigneeUseRealOrg)
			{
				Parent.ABL_OA_ConsigneeInfo.AddMessageError(HavingBothConsigneeAndNotifyPartyWillBeRejectedMessage);
			}
			else
			{
				base.CheckABL_OA_Consignee();
			}
		}

		protected override void CheckMandatoryABL_OA_Consignee()
		{
			if (!Parent.NotifyPartyDeclared)
			{
				base.CheckMandatoryABL_OA_Consignee();
			}
		}

		protected override void CheckABL_OA_NotifyParty()
		{
			if (Parent.ConsigneeDeclared && Parent.NotifyPartyUseRealOrg)
			{
				Parent.ABL_OA_NotifyPartyInfo.AddMessageError(HavingBothConsigneeAndNotifyPartyWillBeRejectedMessage);
			}
			else
			{
				base.CheckABL_OA_NotifyParty();
			}
		}

		ZString GetErrorForMissingNotifyPartyProperty(ZPropertyInfo propertyInfo)
		{
			return $"The field {propertyInfo.Description} is required for a valid Notify Party submission";
		}

		bool NonOrganizationNotifyPartyUsed => !Parent.NotifyPartyUseRealOrg &&
			(!Parent.ABL_NotifyPartyName.IsEmpty ||
			!Parent.ABL_NotifyPartyStreet1.IsEmpty ||
			!Parent.ABL_NotifyPartyStreet2.IsEmpty ||
			!Parent.ABL_NotifyPartyCity.IsEmpty ||
			!Parent.ABL_RN_NKNotifyPartyCountry.IsEmpty ||
			!Parent.ABL_NotifyPartyState.IsEmpty ||
			!Parent.ABL_NotifyPartyPostcode.IsEmpty ||
			!Parent.ABL_NotifyPartyPhone.IsEmpty ||
			!Parent.ABL_NotifyPartyRegNo.IsEmpty);

		readonly static string HavingBothConsigneeAndNotifyPartyWillBeRejectedMessage = "The manifest will be rejected if both a consignee and notify party are present";
	}
}
