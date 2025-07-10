using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class NctsValidationHelper
	{
		public static bool HasCL112OfficeOfDepartureCountry(this NctsEuOfficeCodeCollection offices, BusinessObjectFactory factory)
		{
			return offices.Cast<NctsEuOfficeCode>()
				.Any(x => x.IsOfficeDeparture && x.IsInCL112CountryList);
		}

		public static bool HasCL178PreviousDocument(this IEnumerable<PreviousDocument> previousDocuments, BusinessObjectFactory factory)
		{
			var cl178Codes = UniversalLookupsHelper.GetNctsPreviousDocumentUnionGoodsCode(factory);
			return previousDocuments.Any(x => cl178Codes.ContainsCode(x.CSI_Code));
		}

		public static bool HasN952PreviousDocument(this IEnumerable<PreviousDocument> previousDocuments)
		{
			return previousDocuments.Any(x => NctsConstants.NctsTypeOfPreviousDocument.Codes.N952 == x.CSI_Code);
		}

		public static bool IsEntryTypeT2OrT2F(ZString entryType) => entryType == NctsPhase5DeclarationTypeList.Codes.T2 || entryType == NctsPhase5DeclarationTypeList.Codes.T2F;

		public static void CheckPermitRuleNotFound(NctsGuarantee guarantee)
		{
			if (guarantee.CusGuaranteeWithoutPermitHolder != null && !guarantee.HasMatchingLapPermitRule)
			{
				guarantee.PW_BondAmountInfo.AddWarning(Res.GetString("407E4460-83FC-4BA1-AF25-8CD48137CABA", "A Liability Applicable Percentage is not found for the Guarantee selected in its Rules Tab, so the full amount has been calculated"));
			}
		}

		public static void CheckContainerNumberIsUnique(this BaseCusInBondContainer container, IEnumerable<BaseCusInBondContainer> allContainers, ZString errorPrefix)
		{
			var containerNum = container.BC_ContainerNum;
			if (!containerNum.IsEmpty && allContainers.Where(x => x.BC_ContainerNum == containerNum).Skip(1).Any())
			{
				container.BC_ContainerNumInfo.AddMessageError(errorPrefix + Res.GetString("23B9CFC3-B9FF-4BA9-B732-FC5F0A1866A9", "Duplicate Container Number is entered."));
			}
		}

		public static void CheckTR0046AllSameMode(this BaseCusInBondContainer container, IEnumerable<BaseCusInBondContainer> allContainers)
		{
			if (!allContainers.Where(x => !x.BC_Mode.IsEmpty).AllSame(x => x.BC_Mode))
			{
				container.BC_ModeInfo.AddWarning(Res.GetString("01882d8a-2428-4062-a942-41034b1c3399", "[TR0046] You have selected Both Mode 'CNT' and 'NCT'"));
			}
		}

		public static ZString CheckMRNFormat(ZString mrn, BusinessObjectFactory factory, ZString preRequisteMsg)
		{
			return MRNFormatValidator.CheckMRNFormat(mrn, factory, preRequisteMsg);
		}

		public static void CheckRuleTR0062(AdditionalInfo additionalDocument)
		{
			var parent = additionalDocument.Parent;
			var nctsHeader = parent as NctsHeader ?? (parent as NctsBill)?.Header;
			if (nctsHeader?.IsDepartureMovement ?? false)
			{
				var referenceNumber = additionalDocument.CSI_ReferenceNumber;
				if (!referenceNumber.IsEmpty && additionalDocument.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument && additionalDocument.CSI_Code == AdditionalDocumentTypes.MAWB)
				{
					if (referenceNumber.Length != 11 || !ZInt.TryParse(referenceNumber.Substring(3, 8), out var serialNumber))
					{
						additionalDocument.CSI_ReferenceNumberInfo.AddMessageError(nctsHeader.Configuration.ValidationRuleConfiguration.Messages.TR0062aMessage);
					}
					else
					{
						var validCheckdigit = Math.DivRem(serialNumber, 10, out var checkDigit) % 7;
						if (checkDigit != validCheckdigit)
						{
							additionalDocument.CSI_ReferenceNumberInfo.AddMessageError(nctsHeader.Configuration.ValidationRuleConfiguration.Messages.TR0062cMessage(validCheckdigit));
						}

						if (RefAirline.LoadFromAirlinePrefix(additionalDocument.Factory, referenceNumber.Left(3)) == null)
						{
							additionalDocument.CSI_ReferenceNumberInfo.AddMessageError(nctsHeader.Configuration.ValidationRuleConfiguration.Messages.TR0062bMessage);
						}
					}
				}
			}
		}

		public static void CheckBY_Supplements_HasChildValidationNotification(NctsCommonCargoDesc nctsCommonCargoDesc)
		{
			var additionalSupplementaryCodes = nctsCommonCargoDesc.AdditionalSupplementaryCodes;
			if (additionalSupplementaryCodes.Count > 0)
			{
				nctsCommonCargoDesc.BY_SupplementsInfo.AddNotificationBasedOnChildValidationStatus(
					Res.GetString("B8812135-573C-4934-AD2C-204CAE3511A3", "There are errors within 'Additional Codes', please click on 'Additional codes...' to view the error information"),
					() => additionalSupplementaryCodes.OfType<SupplementaryCode>().ForEach(x => x.Validation.ValidateAll()),
					additionalSupplementaryCodes);
			}
		}

		public static void CheckBY_RN_NKCountryOfOriginIsValid(NctsCommonCargoDesc nctsCommonCargoDesc)
		{
			if (nctsCommonCargoDesc.Header != null)
			{
				ListValidation.MessageErrorIfInvalidCode(nctsCommonCargoDesc.BY_RN_NKCountryOfOriginInfo, nctsCommonCargoDesc.Lookups.CountryOfOriginList);
			}
		}

		public static void CheckBY_RN_NKCountryOfOrigin_RuleNR0058(NctsCommonCargoDesc nctsCommonCargoDesc, INctsCargoDescValidationDecider validationDecider, ValidationRuleConfiguration validationRuleConfiguration)
		{
			if ((validationDecider?.IsRuleNR0058Active ?? false) && nctsCommonCargoDesc.BY_RN_NKCountryOfOrigin.IsEmpty)
			{
				nctsCommonCargoDesc.BY_RN_NKCountryOfOriginInfo.AddWarning(validationRuleConfiguration.Messages.NR0058Message);
			}
		}

		public static void CheckBY_MonetaryValue_RuleNR0059(NctsCommonCargoDesc nctsCommonCargoDesc, INctsCargoDescValidationDecider validationDecider, ValidationRuleConfiguration validationRuleConfiguration)
		{
			if ((validationDecider?.IsRuleNR0059Active ?? false) && nctsCommonCargoDesc.BY_MonetaryValue.IsEmpty)
			{
				nctsCommonCargoDesc.BY_MonetaryValueInfo.AddWarning(validationRuleConfiguration.Messages.NR0059Message);
			}
		}

		public static void CheckBY_Supplements_RuleNR0060(NctsCommonCargoDesc nctsCommonCargoDesc, INctsCargoDescValidationDecider validationDecider, ValidationRuleConfiguration validationRuleConfiguration)
		{
			if ((validationDecider?.IsRuleNR0060Active ?? false) && SupplementaryCodeHelper.GetCodeList(nctsCommonCargoDesc).Count > 0 && nctsCommonCargoDesc.BY_Supplements.IsEmpty)
			{
				nctsCommonCargoDesc.BY_SupplementsInfo.AddMessageError(validationRuleConfiguration.Messages.NR0060Message);
			}
		}

		public static void CheckBY_CustomsSecondUnitQtyIsValid(NctsCommonCargoDesc nctsCommonCargoDesc)
		{
			var header = nctsCommonCargoDesc.Header;
			if (header != null)
			{
				ListValidation.MessageErrorIfInvalidCode(nctsCommonCargoDesc.BY_CustomsSecondUnitQtyInfo, nctsCommonCargoDesc.Lookups.CustomsUnitOfQuantityList);
			}
		}

		public static void CheckRuleTR0084(NctsHeader nctsHeader, ZPropertyInfo infoQty, ZString unitQty, ZDecimal quantity, INotificationType notificationType)
		{
			if (nctsHeader != null && nctsHeader.IsPhase5)
			{
				var validationConfiguration = nctsHeader.Configuration.ValidationRuleConfiguration;
				if (validationConfiguration.IsRuleTR0084Active
					&& !unitQty.IsEmpty
					&& quantity.IsEmpty)
				{
					infoQty.Add(notificationType, validationConfiguration.Messages.TR0084Message(infoQty.HumanReadableName));
				}
			}
		}
	}
}
