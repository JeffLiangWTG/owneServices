using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCLREGInfoProviderAddInfoValidation : AutoAUCLREGInfoProviderAddInfoValidation
	{
		public AUCLREGInfoProviderAddInfoValidation(AutoAUCLREGInfoProviderAddInfo parent) : base(parent)
		{
		}

		public static class Constants
		{
			public const string InvalidPostCode = "Australian Post Code can only be 4 characters long.";
		}

		new CLREGInfoProviderAddInfo Parent
		{
			get { return (CLREGInfoProviderAddInfo)base.Parent; }
		}

		protected override void CheckZA_ABN()
		{
			base.CheckZA_ABN();
			if (!Parent.ZA_ABN.IsEmpty)
			{
				if (Parent.ZA_ABN.Trim().Length != 11)
				{
					Parent.ZA_ABNInfo.AddMessageError(ABNLength);
				}

				ValidateRoll(Parent.ZA_ABNInfo);
			}
			else if (!Parent.ZA_IsOrg && !Parent.ZA_IsIndiv)
			{
				Parent.ZA_ABNInfo.AddMessageError(MessagingModeShouldBeSelected);
			}

			ValidateZA_CAC();
			ValidateZA_CACType();
			ValidateZA_IsIndiv();
			ValidateZA_IsOrg();
		}
		internal const string ABNLength = "ABN must be 11 characters long.";
		internal const string RollRequired = "At least one Organisation Role required for Client Reqistration message.";
		internal const string MessagingModeShouldBeSelected = "Messaging Mode should be specified. Please enter ABN or select Organisation or Individual mode.";

		protected override void CheckZA_ABNInd()
		{
			base.CheckZA_ABNInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_ABNIndInfo, Parent.Lookups.ABNNominatedClientTypeList);
		}

		void ValidateRoll(ZPropertyInfo propertyInfo)
		{
			if (Parent.CLREGInfoProvider.Rolls.Count == 0)
			{
				propertyInfo.AddMessageError(RollRequired);
			}
		}

		protected override void CheckZA_DateofBirthIsValidZDateTimeRange()
		{
		}

		protected override void CheckZA_DateofBirth()
		{
			base.CheckZA_DateofBirth();

			if (Parent.ZA_DateofBirth.IsInTheFutureDatePartOnly)
			{
				Parent.ZA_DateofBirthInfo.AddMessageError(DOBFutureDate);
			}
		}
		internal const string DOBFutureDate = "Date Of Birth cannot be in the future.";

		protected override void CheckZA_CAC()
		{
			base.CheckZA_CAC();

			if (!Parent.ZA_CAC.IsEmpty)
			{
				if (Parent.ZA_ABN.IsEmpty)
				{
					Parent.ZA_CACInfo.AddMessageError(CACShouldBeBlank);
				}
				else if (Parent.ZA_CAC.Trim().Length != 3)
				{
					Parent.ZA_CACInfo.AddMessageError(CACLength);
				}
			}
			ValidateZA_CACType();
		}
		internal const string CACShouldBeBlank = "CAC should be blank if ABN is blank.";
		internal const string CACLength = "CAC must be 3 characters long.";

		protected override void CheckZA_CACType()
		{
			base.CheckZA_CACType();

			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_CACTypeInfo, Parent.Lookups.CACTypeList);
			if (!Parent.ZA_CACType.IsEmpty)
			{
				if (Parent.ZA_ABN.IsEmpty || Parent.ZA_CAC.IsEmpty)
				{
					Parent.ZA_CACTypeInfo.AddMessageError(CACTypeShouldBeBlank);
				}
			}
		}
		internal const string CACTypeShouldBeBlank = "CAC Type should be blank if ABN or CAC blank.";

		protected override void CheckZA_IsIndiv()
		{
			base.CheckZA_IsIndiv();

			if (Parent.ZA_IsIndiv)
			{
				if (!Parent.ZA_ABN.IsEmpty || Parent.ZA_IsOrg)
				{
					Parent.ZA_IsIndivInfo.AddError(IsIndividualOrOrganization);
				}

				ValidateRoll(Parent.ZA_IsIndivInfo);
			}
			else if (!Parent.ZA_IsOrg && Parent.ZA_ABN.IsEmpty)
			{
				Parent.ZA_IsIndivInfo.AddMessageError(MessagingModeShouldBeSelected);
			}

			ValidateZA_IsOrg();
			ValidateZA_ABN();
		}
		internal const string IsIndividualOrOrganization = "You can select 'ABN' or 'Individual' or 'Organization' messaging mode.";

		protected override void CheckZA_IsOrg()
		{
			base.CheckZA_IsOrg();

			if (Parent.ZA_IsOrg)
			{
				if (!Parent.ZA_ABN.IsEmpty || Parent.ZA_IsIndiv)
				{
					Parent.ZA_IsOrgInfo.AddError(IsIndividualOrOrganization);
				}

				ValidateRoll(Parent.ZA_IsOrgInfo);
			}
			else if (!Parent.ZA_IsIndiv && Parent.ZA_ABN.IsEmpty)
			{
				Parent.ZA_IsOrgInfo.AddMessageError(MessagingModeShouldBeSelected);
			}

			ValidateZA_IsIndiv();
			ValidateZA_ABN();
		}

		protected override void CheckZA_BusinessName()
		{
			base.CheckZA_BusinessName();

			if (Parent.ZA_IsOrg && Parent.ZA_BusinessName.IsEmpty)
			{
				Parent.ZA_BusinessNameInfo.AddMessageError(BusinessNameRequired);
			}
		}
		internal const string BusinessNameRequired = "Business Name required if is Organisation messaging mode.";

		protected override void CheckZA_FamilyName()
		{
			base.CheckZA_FamilyName();
			if (Parent.ZA_IsIndiv && Parent.ZA_FamilyName.IsEmpty)
			{
				Parent.ZA_FamilyNameInfo.AddMessageError(FamilyNameRequired);
			}
		}
		internal const string FamilyNameRequired = "Family Name required if is Individual messaging mode.";

		protected override void CheckZA_Gender()
		{
			base.CheckZA_Gender();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_GenderInfo, Parent.Lookups.GenderList);

			if (Parent.ZA_IsIndiv)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZA_GenderInfo);
			}
		}

		protected override void CheckZA_Bsn1()
		{
			base.CheckZA_Bsn1();

			if (Parent.ZA_IsOrg || Parent.ZA_IsIndiv)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZA_Bsn1Info);
			}

			if (!Parent.ZA_ABN.IsEmpty && !Parent.ZA_Bsn1.IsEmpty)
			{
				Parent.ZA_Bsn1Info.AddMessageError(BusinessAddressNotRequired);
			}
		}
		internal const string BusinessAddressNotRequired = "Business Address Details is not required if ABN is entered.";

		protected override void CheckZA_Bsn2()
		{
			base.CheckZA_Bsn2();

			if (!Parent.ZA_ABN.IsEmpty && !Parent.ZA_Bsn2.IsEmpty)
			{
				Parent.ZA_Bsn2Info.AddMessageError(BusinessAddressNotRequired);
			}
		}

		protected override void CheckZA_BsnCity()
		{
			base.CheckZA_BsnCity();

			if (Parent.ZA_IsOrg || Parent.ZA_IsIndiv)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZA_BsnCityInfo);
			}

			if (!Parent.ZA_ABN.IsEmpty && !Parent.ZA_BsnCity.IsEmpty)
			{
				Parent.ZA_BsnCityInfo.AddMessageError(BusinessAddressNotRequired);
			}
		}

		protected override void CheckZA_BsnPostCode()
		{
			base.CheckZA_BsnPostCode();

			if (Parent.ZA_IsOrg || Parent.ZA_IsIndiv)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZA_BsnPostCodeInfo);
			}

			if (!Parent.ZA_ABN.IsEmpty && !Parent.ZA_BsnPostCode.IsEmpty)
			{
				Parent.ZA_BsnPostCodeInfo.AddMessageError(BusinessAddressNotRequired);
			}

			if (Parent.ZA_BsnPort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Australia &&
				Parent.ZA_BsnPostCode.Length > 4)
			{
				Parent.ZA_BsnPostCodeInfo.AddMessageError(Constants.InvalidPostCode);
			}
		}

		protected override void CheckZA_BsnPort()
		{
			base.CheckZA_BsnPort();

			if (Parent.ZA_IsOrg || Parent.ZA_IsIndiv)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZA_BsnPortInfo, Parent.Lookups.RefUNLOCOs);
			}

			if (!Parent.ZA_ABN.IsEmpty && !Parent.ZA_BsnPort.IsEmpty)
			{
				Parent.ZA_BsnPortInfo.AddMessageError(BusinessAddressNotRequired);
			}
		}

		protected override void CheckZA_BsnState()
		{
			base.CheckZA_BsnState();

			if (Parent.ZA_IsOrg || Parent.ZA_IsIndiv)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZA_BsnStateInfo, Parent.Lookups.OA_StateListForBusinessAddress);
			}

			if (!Parent.ZA_ABN.IsEmpty && !Parent.ZA_BsnState.IsEmpty)
			{
				Parent.ZA_BsnStateInfo.AddMessageError(BusinessAddressNotRequired);
			}
		}

		protected override void CheckZA_PostPort()
		{
			base.CheckZA_PostPort();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_PostPortInfo, Parent.Lookups.RefUNLOCOs);
		}

		protected override void CheckZA_PostPostCode()
		{
			base.CheckZA_PostPostCode();

			if (Parent.ZA_PostPort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Australia &&
					Parent.ZA_PostPostCode.Length > 4)
			{
				Parent.ZA_PostPostCodeInfo.AddMessageError(Constants.InvalidPostCode);
			}
		}

		protected override void CheckZA_PostState()
		{
			base.CheckZA_PostState();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_PostStateInfo, Parent.Lookups.OA_StateListForPostalAddress);
		}

		protected override void CheckZA_IsExDocsUser()
		{
			base.CheckZA_IsExDocsUser();

			if (Parent.ZA_IsExDocsUser && !Parent.CLREGInfoProvider.Rolls.HasExporterRoll)
			{
				Parent.ZA_IsExDocsUserInfo.AddMessageError(ExDocsUser);
			}
		}
		internal const string ExDocsUser = "The client is a Quarantine Exdoc user can only be nominated if the role of 'Exporter' is supplied.";

		protected override void CheckZA_Title()
		{
			base.CheckZA_Title();

			if (Parent.ZA_IsIndiv && Parent.ZA_Title.IsEmpty)
			{
				Parent.ZA_TitleInfo.AddMessageError(TitleIsMandatory);
			}
		}
		internal const string TitleIsMandatory = "Title is mandatory field for Individual message mode and should be entered. In other case message will be rejected by Customs.";
	}
}
