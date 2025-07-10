using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class OrgCusCodeValidation : MasterFiles.Business.OrgCusCodeValidation, IOrgCusCodeValidation
	{
		public OrgCusCodeValidation(AutoOrgCusCode parent) : base(parent)
		{
			this.parent = (OrgCusCode)parent;
		}

		protected override void CheckOK_CodeType()
		{
			base.CheckOK_CodeType();
			if (parent.CountryIs(Core.Constants.CountryCodes.Australia)
				&& parent.OK_CodeType == OrgCusCode.AustraliaCodeTypes.ARN
				&& !parent.Organisation.OH_IsConsignor)
			{
				parent.OK_CodeTypeInfo.AddWarning("The organisation does not have a Consignor role.");
			}
		}

		protected void ValidateABN()
		{
			if (!ABNValidation.CheckValidABN(parent.OK_CustomsRegNo))
			{
				parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("60cde44e-77d4-40f6-ae1c-f97bb7cb846c", "The entered ABN is not valid.\r\nAn ABN must be 11 or 14 digits with a valid check-digit."), OrganisationRegistry.RegistrationNumberFormatFields.AUABN);
			}
		}

		protected void ValidateARN()
		{
			if (!parent.OK_CustomsRegNo.IsNumbersOnlyOrEmpty || parent.OK_CustomsRegNo.Length != 12)
			{
				parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("755077c7-a4ee-4258-8be6-72ef02b7282c", "ARN number should be 12 digits."), OrganisationRegistry.RegistrationNumberFormatFields.AUARN);
			}
		}

		protected void ValidateApprovedArrangementNumber()
		{
			if (parent.OK_CustomsRegNo.Length > 5)
			{
				parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("4FC91525-5A2A-4277-8A40-A2C95092C96A", "AAN (Approved Arrangement Number), cannot be greater than 5 characters."), OrgCusCode.AustraliaCodeTypes.ApprovedArrangementNumber);
			}
		}

		protected void ValidateCustomsClientCode()
		{
			CCDAndCSCValidation cCDChecker = new CCDAndCSCValidation(parent.OK_CustomsRegNo, true);
			ZString result = cCDChecker.CheckValid();
			if (!result.IsEmpty)
			{
				parent.OK_CustomsRegNoInfo.AddMessageError(result);
			}
		}

		protected void ValidateSupplierCode()
		{
			CCDAndCSCValidation cSCChecker = new CCDAndCSCValidation(parent.OK_CustomsRegNo);
			ZString result = cSCChecker.CheckValid();
			if (!result.IsEmpty)
			{
				parent.OK_CustomsRegNoInfo.AddMessageError(result);
			}
		}

		protected new void ValidateControlledPremisesID()
		{
			new AUCCPValidator(parent.OK_CustomsRegNoInfo).ValidateCCPAddError();
		}

		protected void ValidateEXDOCExporterNumber()
		{
			if (parent.OK_CustomsRegNo.Length > 5)
			{
				parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("56782e1f-2203-4f27-9d2a-0a1e4d076f6b", "Exporter number cannot be greater than 5 characters in length."), OrganisationRegistry.RegistrationNumberFormatFields.AUEEN);
			}
		}

		protected void ValidateNEXDOCSExportNumber()
		{
			if (!Regex.IsMatch(parent.OK_CustomsRegNo, @"^[A-Z|a-z]{2}[0-9]{4,5}$"))
			{
				parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("77D3B44C-387E-48E6-9CF6-816B16F5A028", "Export number should be two alpha followed by 4 or 5 digits."), OrganisationRegistry.RegistrationNumberFormatFields.AUNEN);
			}
		}

		protected void ValidateEXDOCAMLCPerformanceExporterNumber()
		{
			if (parent.OK_CustomsRegNo.Length > 5)
			{
				parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("c0188c8c-4f84-46eb-acc8-a60ecaec19cd", "AMLC performance exporter number cannot be greater than 5 characters in length."), OrganisationRegistry.RegistrationNumberFormatFields.AUEAP);
			}
		}

		protected void ValidateEXDOCEstablishmentNumber()
		{
			if (parent.OK_CustomsRegNo.Length > 6)
			{
				parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("6382b6ee-9ea6-45ff-a344-9f71e11792cb", "Establishment number cannot be greater than 6 characters in length."), OrganisationRegistry.RegistrationNumberFormatFields.AUESN);
			}
		}

		protected void ValidateEXDOCEDIUser()
		{
			if (parent.OK_CustomsRegNo.Length > 7)
			{
				parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("bd7308df-8f2c-4ab7-a8f3-c6bc58e79a76", "EDI user cannot be greater than 7 characters in length."), OrganisationRegistry.RegistrationNumberFormatFields.AUEEU);
			}
		}

		protected void ValidateCAG()
		{
			MandatoryValidation.CheckEntered(parent.OK_CustomsRegNoInfo);
			if (!CAGCodeIssuer.GetIsCAGCodeValid(parent) && !parent.OK_CodeTypeInfo.HasErrors() && !parent.OK_RN_NKCodeCountryInfo.HasErrors())
			{
				parent.OK_CustomsRegNoInfo.AddError(Res.GetString("D0364A50-B1F4-4AF7-9F38-6D859AA56F59", "Commercial And Government Entity Code is not valid."));
			}
		}

		protected void ValidateAEO()
		{
			if (!Regex.IsMatch(parent.OK_CustomsRegNo, "^[0-9]{11}$", RegexOptions.IgnoreCase))
			{
				parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("d3e69d52-8365-4b0f-ab48-6fbeb7f4ffda", "AU AEO number should consist of 11 numeric characters."));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502")]
		protected override void CheckOK_CustomsRegNo()
		{
			base.CheckOK_CustomsRegNo();

			if (!parent.OK_CustomsRegNoInfo.HasErrors() && parent.CountryIs(Core.Constants.CountryCodes.Australia))
			{
				switch (parent.OK_CodeType)
				{
					case OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber:
						ValidateABN();
						break;
					case OrgCusCode.AustraliaCodeTypes.ARN:
						ValidateARN();
						break;
					case OrgCusCode.AustraliaCodeTypes.ApprovedArrangementNumber:
						ValidateApprovedArrangementNumber();
						break;
					case OrgCusCode.CodeTypes.CustomsClientCode:
						ValidateCustomsClientCode();
						break;
					case OrgCusCode.CodeTypes.SupplierCode:
						ValidateSupplierCode();
						break;
					case OrgCusCode.CodeTypes.ControlledPremisesID:
						ValidateControlledPremisesID();
						break;
					case OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber:
						ValidateEXDOCExporterNumber();
						break;
					case OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber:
						ValidateNEXDOCSExportNumber();
						break;
					case OrgCusCode.AUQuarantineCodeTypes.EXDOCAMLCPerformanceExporterNumber:
						ValidateEXDOCAMLCPerformanceExporterNumber();
						break;
					case OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber:
						ValidateEXDOCEstablishmentNumber();
						break;
					case OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser:
						ValidateEXDOCEDIUser();
						break;
					case OrgCusCode.CodeTypes.CommercialAndGovernmentEntity:
						ValidateCAG();
						break;
					case OrgCusCode.AustraliaCodeTypes.AEO:
						ValidateAEO();
						break;
				}
			}
		}

		readonly OrgCusCode parent;
	}
}
