using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Customs.CN.Business
{
	public class CNJobDocAddressValidation : JobDocAddressValidation
	{
		public CNJobDocAddressValidation(AutoJobDocAddress parent) : base(parent)
		{
		}

		protected new CNJobDocAddress Parent => base.Parent as CNJobDocAddress;

		JobDeclaration Declaration => Parent.Parent as JobDeclaration;

		internal IValidationModeProvider ValidationModeProvider => Declaration;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateChineseCompanyName();
			ValidateSocialCreditCode();
			ValidateCustomsCode();
			ValidateCIQCode();
			ValidateOverseasPartyCodeType();
			ValidateOverseasPartyCode();
		}

		#region OrganisationPK

		void CheckMandatory(ZPropertyInfo targetInfo)
		{
			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.ImporterDocumentaryAddress:
				case DocAddressTypes.Codes.SupplierDocumentaryAddress:
					targetInfo.AddNotificationIfNotEntered(ValidationModeProvider);
					break;
			}
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (!Parent.E2_AddressOverride)
			{
				CheckMandatory(Parent.OrganisationPKInfo);
			}
		}

		#endregion

		#region ChineseCompanyName

		public void ValidateChineseCompanyName()
		{
			ValidateCalculatedProperty(Parent.ChineseCompanyNameInfo);
		}

		protected void CheckChineseCompanyName()
		{
			Parent.Validation.ValidateE2_CompanyName();
			Parent.ChineseCompanyNameInfo.AddAllNotificationsFrom(Parent.E2_CompanyNameInfo);
		}

		#endregion

		#region Codes

		void CheckAtLeastOneChinaRegNumberEntered(ZPropertyInfo propertyInfo)
		{
			if (Parent.E2_AddressOverride && Parent.RequiresDomesticOrg && Parent.SocialCreditCode.IsEmpty && Parent.CustomsCode.IsEmpty && Parent.CIQCode.IsEmpty)
			{
				propertyInfo.AddNotification(Res.GetString("C9F8FD77-7757-44A9-B469-F35DB1E7396E", "Please enter USC or CCD or CIQ code."), ValidationModeProvider);
			}
		}

		public void ValidateSocialCreditCode()
		{
			ValidateCalculatedProperty(Parent.SocialCreditCodeInfo);
		}

		protected void CheckSocialCreditCode()
		{
			if (!Parent.ChineseCompanyName.IsEmpty)
			{
				CheckAtLeastOneChinaRegNumberEntered(Parent.SocialCreditCodeInfo);
				CheckSocialCreditCodeIfNeeded(Parent.SocialCreditCodeInfo);
			}
		}

		void CheckSocialCreditCodeIfNeeded(ZPropertyInfo targetInfo)
		{
			var notificationType = Parent.RequiresTradeOrg ? ValidationModeProvider.GetNotificationType() : (Parent.RequiresOwnerOrg ? NotificationType.Warning : null);
			if (notificationType != null)
			{
				var code = Parent.SocialCreditCode;
				if (!Parent.E2_AddressOverride || !code.IsEmpty)
				{
					ValidationHelper.CheckRegNumbers(Parent.SocialCreditCode, OrgCusCode.ChinaCodeTypes.USC, targetInfo, notificationType, !Parent.E2_AddressOverride);
				}
			}
		}

		public void ValidateCustomsCode()
		{
			ValidateCalculatedProperty(Parent.CustomsCodeInfo);
		}

		protected void CheckCustomsCode()
		{
			if (!Parent.ChineseCompanyName.IsEmpty)
			{
				CheckAtLeastOneChinaRegNumberEntered(Parent.CustomsCodeInfo);
				CheckCustomsCodeIfNeeded(Parent.CustomsCodeInfo);
			}
		}

		void CheckCustomsCodeIfNeeded(ZPropertyInfo targetInfo)
		{
			if (Parent.RequiresTradeOrg)
			{
				var code = Parent.CustomsCode;
				if (!Parent.E2_AddressOverride || !code.IsEmpty)
				{
					ValidationHelper.CheckRegNumbers(code, OrgCusCode.CodeTypes.CustomsClientCode, targetInfo, ValidationModeProvider.GetNotificationType(), !Parent.E2_AddressOverride);
				}
			}
		}

		public void ValidateCIQCode()
		{
			ValidateCalculatedProperty(Parent.CIQCodeInfo);
		}

		protected void CheckCIQCode()
		{
			if (!Parent.ChineseCompanyName.IsEmpty)
			{
				CheckAtLeastOneChinaRegNumberEntered(Parent.CIQCodeInfo);
				CheckCIQCodeIfNeeded(Parent.CIQCodeInfo);
			}
		}

		void CheckCIQCodeIfNeeded(ZPropertyInfo targetInfo)
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				var ciqRequires = declaration.CIQRequires;
				if (Parent.RequiresTradeOrg && (ciqRequires || declaration.IsImport) || Parent.RequiresOwnerOrg && ciqRequires)
				{
					var code = Parent.CIQCode;
					if (!Parent.E2_AddressOverride || !code.IsEmpty)
					{
						ValidationHelper.CheckRegNumbers(code, OrgCusCode.ChinaCodeTypes.CIQ, targetInfo, ValidationModeProvider.GetNotificationType(), !Parent.E2_AddressOverride);
					}
				}
			}
		}
		public void ValidateOverseasPartyCode()
		{
			ValidateCalculatedProperty(Parent.OverseasPartyCodeInfo);
		}

		protected void CheckOverseasPartyCode()
		{
			if (Parent.E2_AddressOverride && Parent.RequiresOverseasOrg)
			{
				var targetInfo = Parent.OverseasPartyCodeInfo;
				var code = Parent.OverseasPartyCode;
				if (!code.IsEmpty && Parent.IsAeo(Parent.OverseasPartyCodeType))
				{
					if (code.Length < 3)
					{
						targetInfo.AddError(Res.GetString("F4BE97CC-F830-415E-8D4A-E204093EEA75", "AEO number should have at least 3 characters."));
					}
					else
					{
						var first2Characters = code.SubstringSafe(0, 2);
						if (!(AEONumberHelper.IsAEOMutualRecognitionCountry(Parent.Factory, first2Characters, ZDateTime.Today) || AEONumberHelper.IsInEuropeanCustomsUnion(first2Characters)))
						{
							targetInfo.AddNotification(Res.GetString("6EFB9512-36EC-4E7C-93CD-CF9349086386", "The code should start with 2-letter country code of an AEO mutual recognition country."), ValidationModeProvider);
						}
					}
				}
			}
		}

		public void ValidateOverseasPartyCodeType()
		{
			ValidateCalculatedProperty(Parent.OverseasPartyCodeTypeInfo);
		}

		protected void CheckOverseasPartyCodeType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.OverseasPartyCodeTypeInfo);
		}

		#endregion

		#region Contact

		protected override void CheckE2_Phone_Formatted()
		{
			base.CheckE2_Phone_Formatted();

			var parent = Parent;
			if (parent.E2_Phone_Formatted.IsEmpty && ShouldCheckContact())
			{
				if (!parent.E2_Contact.IsEmpty)
				{
					MandatoryValidation.WarnIfNotEntered(parent.E2_Phone_FormattedInfo);
				}
				else if (!parent.E2_AddressOverride && parent.Contact != null)
				{
					parent.E2_Phone_FormattedInfo.AddWarning(Res.GetString("8363DAC4-9927-49F1-A296-F6D2259FA44E", "The contact you selected does not have a Work Number."));
				}
			}
		}

		bool ShouldCheckContact()
		{
			var parent = Parent;
			var declaration = Declaration;
			return declaration != null
				&& (parent.E2_AddressType == DocAddressTypes.Codes.ImporterDocumentaryAddress && declaration.WillGenerateEnteringEntry
					|| parent.E2_AddressType == DocAddressTypes.Codes.SupplierDocumentaryAddress && declaration.WillGenerateExitingEntry
					|| parent.E2_AddressType == DocAddressTypes.Codes.BuyerDocumentaryAddress && declaration.WillGenerateEnteringEntry);
		}

		#endregion

		#region E2_OA_Address

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (Parent.HasRealAddress)
			{
				ZString requiredCodeType = ZString.Empty;
				if (Parent.E2_AddressType == DocAddressTypes.Codes.CustomsDepotAddress)
				{
					requiredCodeType = OrgCusCode.CodeTypes.DepotControlledPremisesID;
				}
				else if (Parent.E2_AddressType == DocAddressTypes.Codes.CustomsWarehouseAddress)
				{
					requiredCodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
				}
				if (!requiredCodeType.IsEmpty && Parent.Address.CustomsCodes.GetCustomsRegNo(requiredCodeType, Core.Constants.CountryCodes.China).IsEmpty)
				{
					Parent.E2_OA_AddressInfo.AddWarning(Res.GetString(
						"4357ECF0-AA72-44BD-88BE-C36B430F1849",
						"{0} Registration Number for CN is required for {1}. Please press F3, go to Details -> Config -> Registration Numbers/Codes and enter the Registration Number.",
						requiredCodeType,
						AddressDescription
					));
				}
			}
		}

		ZString AddressDescription => Parent.Factory.GetCachedValue<DocAddressTypes>().GetDescriptionFromCode(Parent.E2_AddressType);

		#endregion

		public static void ValidateAddress1(JobDocAddressValidation validation)
		{
			MandatoryValidation.WarnIfNotEntered(validation.Parent.E2_Address1Info);
		}

		public static void ValidateCity(JobDocAddressValidation validation)
		{
			MandatoryValidation.WarnIfNotEntered(validation.Parent.E2_CityInfo);
		}

		public static void ValidateCountry(JobDocAddressValidation validation)
		{
			MandatoryValidation.WarnIfNotEntered(validation.Parent.E2_RN_NKCountryCodeInfo);
		}
		public static void NoValidation(JobDocAddressValidation validation)
		{
		}
	}
}
