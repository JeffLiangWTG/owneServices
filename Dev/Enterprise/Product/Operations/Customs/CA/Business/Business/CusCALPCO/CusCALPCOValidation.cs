using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCALPCOValidation : AutoCusCALPCOValidation
	{
		public CusCALPCOValidation(AutoCusCALPCO parent)
			: base(parent)
		{
		}

		protected new CusCALPCO Parent
		{
			get { return (CusCALPCO)base.Parent; }
		}

		protected IPGAProgramRequirementProvider PGAHeader => Parent?.Parent as IPGAProgramRequirementProvider;

		protected override void CheckCLP_Type()
		{
			base.CheckCLP_Type();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CLP_TypeInfo);
		}

		protected override void CheckCLP_RefNo()
		{
			base.CheckCLP_RefNo();

			var lpco = Parent;
			var pgaHeader = PGAHeader;
			if (pgaHeader != null && pgaHeader.GovAgencyIDCode == PGACodes.Codes.CFIA)
			{
				if (!lpco.CLP_Type.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(lpco.CLP_RefNoInfo);
					if (!lpco.CLP_RefNo.IsEmpty)
					{
						RegistrationNumberHelper.ValidateCFIARegNum(lpco);
					}
				}
			}
			else
			{
				if (pgaHeader != null)
				{
					var attributes = Parent.DocumentType?.GetAttributesValues(RefCusCodeListAttributeTypes.Codes.CADocumentTypeValuesAllowed).ToArray();
					if (attributes?.Length == 1 && Parent.CLP_RefNo != attributes[0])
					{
						lpco.CLP_RefNoInfo.AddMessageError(Res.GetString("a2976215-1d3e-486b-8a0a-679f90b9ee16", "Ref No. should remain : {0}.", attributes[0]));
					}
				}

				var declaration = lpco?.Declaration;

				var isRefNumberNotRequired = declaration != null
					&& declaration.CA_PermitApplication
					&& pgaHeader == null
					&& (lpco.CLP_Type == LPCODocumentTypeQualifier.Codes._2001 || lpco.CLP_Type == LPCODocumentTypeQualifier.Codes._2003);

				if (!isRefNumberNotRequired)
				{
					MandatoryValidation.MessageErrorIfNotEntered(lpco.CLP_RefNoInfo);
					if (lpco.IsTypeSafeFoodForCanadiansLicence)
					{
						ListValidation.WarnIfInvalidCode(lpco.CLP_RefNoInfo, RegistrationNumberHelper.SafeFoodLicenseNotListed);
					}
				}
			}
		}

		protected override void CheckCLP_HolderType()
		{
			base.CheckCLP_HolderType();

			var documentTypes = new ZString[]
			{
				LPCODocumentTypeQualifier.Codes._2001,
				LPCODocumentTypeQualifier.Codes._2003,
				LPCODocumentTypeQualifier.Codes._7000,
				LPCODocumentTypeQualifier.Codes._8020,
				LPCODocumentTypeQualifier.Codes._8021,
				LPCODocumentTypeQualifier.Codes._8022,
				LPCODocumentTypeQualifier.Codes._8023
			};

			if (documentTypes.Contains(Parent.CLP_Type))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_HolderTypeInfo);
			}

			CheckHolderPartyAndBusinessNumber(Parent.CLP_HolderTypeInfo, Parent.Lookups.HolderPartyTypeCodes);
		}

		protected override void CheckCLP_SecondaryRefNo()
		{
			base.CheckCLP_SecondaryRefNo();

			if (Parent.CLP_Type == LPCODocumentTypeQualifier.Codes._2007)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_SecondaryRefNoInfo);
			}
		}

		protected override void CheckCLP_IssueDate()
		{
			base.CheckCLP_IssueDate();

			var documentTypes = new ZString[]
			{
				LPCODocumentTypeQualifier.Codes._3004,
				LPCODocumentTypeQualifier.Codes._2007,
				LPCODocumentTypeQualifier.Codes._8020,
				LPCODocumentTypeQualifier.Codes._8021,
				LPCODocumentTypeQualifier.Codes._8022,
				LPCODocumentTypeQualifier.Codes._8023
			};

			if (documentTypes.Contains(Parent.CLP_Type))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_IssueDateInfo);
			}
		}

		protected override void CheckCLP_RN_NKIssuanceCountryCode()
		{
			base.CheckCLP_RN_NKIssuanceCountryCode();

			if (Parent.CLP_Type == LPCODocumentTypeQualifier.Codes._3004)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_RN_NKIssuanceCountryCodeInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CLP_RN_NKIssuanceCountryCodeInfo, Parent.Lookups.IssuanceCountryCodes);
		}

		protected override void CheckCLP_RN_NKOriginCountryCode()
		{
			base.CheckCLP_RN_NKOriginCountryCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CLP_RN_NKOriginCountryCodeInfo, Parent.Lookups.OriginCountryCodes);
		}

		protected override void CheckCLP_RN_NKAuthorizationCountry()
		{
			base.CheckCLP_RN_NKAuthorizationCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.CLP_RN_NKAuthorizationCountryInfo, Parent.Lookups.AuthorizationCountries);
		}

		protected override void CheckCLP_RN_NKSmeltAndPourCountryCode()
		{
			base.CheckCLP_RN_NKSmeltAndPourCountryCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CLP_RN_NKSmeltAndPourCountryCodeInfo, Parent.Lookups.SmeltAndPourCountryCodes);
		}

		protected override void CheckCLP_EndDate()
		{
			base.CheckCLP_EndDate();

			var documentTypes = new ZString[]
			{
				LPCODocumentTypeQualifier.Codes._3004,
				LPCODocumentTypeQualifier.Codes._8020,
				LPCODocumentTypeQualifier.Codes._8021,
				LPCODocumentTypeQualifier.Codes._8022,
				LPCODocumentTypeQualifier.Codes._8023
			};

			if (documentTypes.Contains(Parent.CLP_Type))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_EndDateInfo);
			}
		}

		protected override void CheckCLP_ApplicantType()
		{
			base.CheckCLP_ApplicantType();

			var documentTypes = new ZString[]
			{
				LPCODocumentTypeQualifier.Codes._2001,
				LPCODocumentTypeQualifier.Codes._2003,
				LPCODocumentTypeQualifier.Codes._3001,
				LPCODocumentTypeQualifier.Codes._3002,
				LPCODocumentTypeQualifier.Codes._3003,
			};

			if (documentTypes.Contains(Parent.CLP_Type))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_ApplicantTypeInfo);
			}

			CheckHolderPartyAndBusinessNumber(Parent.CLP_ApplicantTypeInfo, Parent.Lookups.LPCOApplicantCodes);
		}

		void CheckHolderPartyAndBusinessNumber(ZPropertyInfo partyTypeInfo, CodeDescriptionPairList codeDescriptionPairList)
		{
			ListValidation.MessageErrorIfInvalidCode(partyTypeInfo, codeDescriptionPairList);

			var partyType = (ZString)partyTypeInfo.Value;
			var header = Parent?.Parent as IPGAHeader;
			var line = header?.Parent as JobComInvoiceLine;

			if (codeDescriptionPairList.ContainsCode(partyTypeInfo.Value) && partyType != LPCOHolderPartyTypeCodes.Codes.Other && line != null)
			{
				var party = Parent?.GetHolderParty(partyType, () => null);//does not matter if null is returned as this is not invoked for OTH

				if (party == null)
				{
					var partyName = codeDescriptionPairList.GetDescriptionFromCode(partyTypeInfo.Value.ToString());
					partyTypeInfo.AddMessageError(Res.GetString("355e1785-9bb7-4a7b-8411-dd0806e3ccec", "{0} doesn't exist for this invoice line.", partyName));
				}
				else
				{
					CheckBusinessNumber(partyTypeInfo, party);
				}
			}
		}

		void CheckOtherPartyAddress(ZPropertyInfo addressInfo, OrgHeader party)
		{
			MandatoryValidation.MessageErrorIfNotEntered(addressInfo);
			CheckBusinessNumber(addressInfo, party);
		}

		public static void CheckBusinessNumber(ZPropertyInfo info, OrgHeader party)
		{
			if (party != null)
			{
				var businessNumber = party.GetCABrokerBusinessNumber();

				if (string.IsNullOrWhiteSpace(businessNumber))
				{
					var message = Res.GetString("3115abb9-1326-4aea-bc77-ab5410ea4fc5",
						"The {0} does not specify the Business Number Customs Broker(BRB) or Business Number For Import Export(BRM), please see Organization -> Details -> Config -> Registration Numbers / Codes.",
						party.HumanReadableShortcutName);

					info.AddMessageError(message);
				}
			}
		}

		protected override void CheckCLP_AlternativeQuotaQuantity()
		{
			base.CheckCLP_AlternativeQuotaQuantity();

			if (Parent.CLP_Type == LPCODocumentTypeQualifier.Codes._2005)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.CLP_AlternativeQuotaQuantityInfo);
			}
		}

		protected override void CheckCLP_AlternativeQuotaUQ()
		{
			base.CheckCLP_AlternativeQuotaUQ();
			if (Parent.CLP_AlternativeQuotaQuantity > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_AlternativeQuotaUQInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CLP_AlternativeQuotaUQInfo, Parent.Lookups.UQList);
		}

		protected override void CheckCLP_DIFRefNumberOrLocation()
		{
			base.CheckCLP_DIFRefNumberOrLocation();

			var pgaHeader = PGAHeader;
			if (pgaHeader != null && pgaHeader.GovAgencyIDCode == PGACodes.Codes.CFIA)
			{
				RegistrationNumberHelper.ValidateCFIADIFURN(Parent);
			}
			else if (URNMandatoryDocumentTypes.Contains(Parent.CLP_Type))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_DIFRefNumberOrLocationInfo);
			}

			if (!Parent.CLP_DIFRefNumberOrLocation.IsEmpty && !Regex.IsMatch(Parent.CLP_DIFRefNumberOrLocation, @"^[0-9]{14}$"))
			{
				Parent.CLP_DIFRefNumberOrLocationInfo.AddMessageError(Res.GetString("317fa368-155a-4ca3-a3b4-15f676f164a6", "DIF URN must be numeric and should be 14 digits"));
			}
		}

		protected List<string> URNMandatoryDocumentTypes => PGAHeader?.GetURNMandatoryDocumentTypes() ?? new List<string>();

		protected override void CheckCLP_OA_Holder()
		{
			base.CheckCLP_OA_Holder();
			var parent = Parent;
			if (parent.CLP_HolderType == LPCOHolderPartyTypeCodes.Codes.Other)
			{
				CheckOtherPartyAddress(parent.CLP_OA_HolderInfo, parent.OthLPCOHolder);
			}
		}

		protected override void CheckCLP_OA_Applicant()
		{
			base.CheckCLP_OA_Applicant();

			var parent = Parent;
			if (parent.CLP_ApplicantType == LPCOHolderPartyTypeCodes.Codes.Other)
			{
				CheckOtherPartyAddress(parent.CLP_OA_ApplicantInfo, parent.OthLPCOApplicant);
			}
		}

		protected override void CheckCLP_HolderName()
		{
			base.CheckCLP_HolderName();

			var parent = Parent;
			if (parent.CLP_HolderType == LPCOHolderPartyTypeCodes.Codes.Other)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CLP_HolderNameInfo);
			}
		}

		protected override void CheckCLP_ApplicantName()
		{
			base.CheckCLP_ApplicantName();

			var parent = Parent;
			if (parent.CLP_ApplicantType == LPCOHolderPartyTypeCodes.Codes.Other)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CLP_ApplicantNameInfo);
			}
		}

		protected override void CheckCLP_HolderContactEmail()
		{
			base.CheckCLP_HolderContactEmail();

			var parent = Parent;
			if (!EmailAddressValidation.IsEmailAddressValid(parent.CLP_HolderContactEmail))
			{
				parent.CLP_HolderContactEmailInfo.AddMessageError(Res.GetString("DE57A91A-ADC7-4FD0-ADEE-C73B01A50332", "Holder Contact Email Address is not valid."));
			}
		}

		protected override void CheckCLP_ApplicantContactEmail()
		{
			base.CheckCLP_ApplicantContactEmail();

			var parent = Parent;
			if (!EmailAddressValidation.IsEmailAddressValid(parent.CLP_ApplicantContactEmail))
			{
				parent.CLP_ApplicantContactEmailInfo.AddMessageError(Res.GetString("D57BB2EE-3CA9-412E-BA15-CBF76FC7AAA2", "Applicant Contact Email Address is not valid."));
			}
		}
	}
}
