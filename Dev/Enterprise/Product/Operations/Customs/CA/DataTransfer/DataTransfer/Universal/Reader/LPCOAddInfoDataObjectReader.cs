using System;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class LPCOAddInfoDataObjectReader : DataObjectReader<UniversalDataBuss.DataObjects.Universal.Customs.AddInfoGroup>, IAddInfoGroupToOtherTableDataObjectReader
	{
		public LPCOAddInfoDataObjectReader(UniversalDataBuss.DataObjects.Universal.Customs.AddInfoGroup addInfoGroup, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(addInfoGroup, logger, helper.Factory)
		{
		}

		public CargoWise.EntityFramework.IColumnIndexer ReadIntoRow(ZGuid parentPK, ZString parentTableCode)
		{
			var result = base.CreateNewColumnIndexer(CusCALPCOSchema.PK, typeof(CusCALPCO));
			if (result != null)
			{
				SetValue(result, CusCALPCOSchema.CLP_ParentID, parentPK);
				SetValue(result, CusCALPCOSchema.CLP_ParentTableCode, parentTableCode);

				DateTime dateTime;
				ZString? isApplicantOverriddenAddInfoValue = null;
				ZString? isHolderOverriddenAddInfoValue = null;
				ZString? lpcoHolderNameValue = null;
				ZString? authorizedPartyContactEmailValue = null;
				ZString? authorizedPartyContactNameValue = null;
				ZString? authorizedPartyContactPhoneValue = null;
				ZString? lpcoApplicantNameValue = null;
				ZString? applicantContactEmailValue = null;
				ZString? applicantContactNameValue = null;
				ZString? applicantContactPhoneValue = null;
				ZString? meltAndPourCountryCode = null;

				foreach (var addInfo in dataObject.AddInfoCollection)
				{
					switch (addInfo.Key)
					{
						case Constants.AddInfoKeys.CusCALPCO.Type:
							SetValue(result, CusCALPCOSchema.CLP_Type, addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.RefNo:
							SetValue(result, CusCALPCOSchema.CLP_RefNo, addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.SecondaryRefNo:
							SetValue(result, CusCALPCOSchema.CLP_SecondaryRefNo, addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.DIFRefNumberOrLocation:
							SetValue(result, CusCALPCOSchema.CLP_DIFRefNumberOrLocation, addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOStartDate:
							if (DateTime.TryParse(addInfo.Value, out dateTime))
							{
								SetValue(result, CusCALPCOSchema.CLP_StartDate, dateTime);
							}
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOEndDate:
							if (DateTime.TryParse(addInfo.Value, out dateTime))
							{
								SetValue(result, CusCALPCOSchema.CLP_EndDate, dateTime);
							}
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOIssueDate:
							if (DateTime.TryParse(addInfo.Value, out dateTime))
							{
								SetValue(result, CusCALPCOSchema.CLP_IssueDate, dateTime);
							}
							break;
						case Constants.AddInfoKeys.CusCALPCO.CountryOfIssuance:
							SetValue(result, CusCALPCOSchema.CLP_RN_NKIssuanceCountryCode, addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.CountryOfOrigin:
							SetValue(result, CusCALPCOSchema.CLP_RN_NKOriginCountryCode, addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.AuthorizationCountry:
							SetValue(result, CusCALPCOSchema.CLP_RN_NKAuthorizationCountry, addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.IsMixedCountryOfOrigin:
							if (ZBool.TryParse(addInfo.Value, out ZBool boolValue))
							{
								SetValue(result, CusCALPCOSchema.CLP_IsMixedCountryOfOrigin, boolValue);
							}
							break;
						case Constants.AddInfoKeys.CusCALPCO.CommodityTypeCode:
							SetValue(result, CusCALPCOSchema.CLP_CommodityTypeCode, addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.Qty:
							if (ZDecimal.TryParse(addInfo.Value, out ZDecimal decimalValue))
							{
								SetValue(result, CusCALPCOSchema.CLP_AlternativeQuotaQuantity, decimalValue);
							}
							break;
						case Constants.AddInfoKeys.CusCALPCO.UQ:
							SetValue(result, CusCALPCOSchema.CLP_AlternativeQuotaUQ, addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOHolderType:
							SetValue(result, CusCALPCOSchema.CLP_HolderType, addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOApplicant:
							SetValue(result, CusCALPCOSchema.CLP_ApplicantType, addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.IsApplicantOverridden:
							isApplicantOverriddenAddInfoValue = addInfo.Value;
							break;
						case Constants.AddInfoKeys.CusCALPCO.IsHolderOverridden:
							isHolderOverriddenAddInfoValue = addInfo.Value;
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOHolderName:
							lpcoHolderNameValue = addInfo.Value;
							break;
						case Constants.AddInfoKeys.CusCALPCO.AuthorizedPartyContactEmail:
							authorizedPartyContactEmailValue = addInfo.Value;
							break;
						case Constants.AddInfoKeys.CusCALPCO.AuthorizedPartyContactName:
							authorizedPartyContactNameValue = addInfo.Value;
							break;
						case Constants.AddInfoKeys.CusCALPCO.AuthorizedPartyContactPhone:
							authorizedPartyContactPhoneValue = addInfo.Value;
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOApplicantName:
							lpcoApplicantNameValue = addInfo.Value;
							break;
						case Constants.AddInfoKeys.CusCALPCO.ApplicantContactEmail:
							applicantContactEmailValue = addInfo.Value;
							break;
						case Constants.AddInfoKeys.CusCALPCO.ApplicantContactName:
							applicantContactNameValue = addInfo.Value;
							break;
						case Constants.AddInfoKeys.CusCALPCO.ApplicantContactPhone:
							applicantContactPhoneValue = addInfo.Value;
							break;
						case Constants.AddInfoKeys.CusCALPCO.RN_NKSmeltAndPourCountryCode:
							meltAndPourCountryCode = addInfo.Value;
							break;
					}
				}

				ZBool? isApplicantOverriddenValue = null;
				if (isApplicantOverriddenAddInfoValue.HasValue)
				{
					isApplicantOverriddenValue = ZBool.TryParse(isApplicantOverriddenAddInfoValue.Value, out ZBool isApplicantOverridden) && isApplicantOverridden;
				}
				ZBool? isHolderOverriddenValue = null;
				if (isHolderOverriddenAddInfoValue.HasValue)
				{
					isHolderOverriddenValue = ZBool.TryParse(isHolderOverriddenAddInfoValue.Value, out ZBool isHolderOverridden) && isHolderOverridden;
				}
				var cusCALPCO = (CusCALPCO)result;
				var doesLPCOApplicantNotReferenceDecOrgs = !cusCALPCO.DoesLPCOApplicantReferenceDecOrgs;
				var needLPCOApplicantOrganization = !isApplicantOverriddenValue.HasValue || doesLPCOApplicantNotReferenceDecOrgs;
				var doesLPCOHolderNotReferenceDecOrgs = !cusCALPCO.DoesLPCOHolderReferenceDecOrgs;
				var needLPCOHolderOrganization = !isHolderOverriddenValue.HasValue || doesLPCOHolderNotReferenceDecOrgs;
				OrganizationAddress applicantOrganizationAddress = null;
				OrganizationAddress holderOrganizationAddress = null;
				var organizationAddressCollection = dataObject.OrganizationAddressCollection;
				if (organizationAddressCollection != null && (needLPCOApplicantOrganization || needLPCOHolderOrganization))
				{
					foreach (var organizationAddress in organizationAddressCollection)
					{
						if (needLPCOApplicantOrganization && applicantOrganizationAddress == null && organizationAddress.AddressType.GetValueOrDefault().EqualsIgnoringCase(Constants.AddressType.LPCOApplicant))
						{
							applicantOrganizationAddress = organizationAddress;
						}
						if (needLPCOHolderOrganization && holderOrganizationAddress == null && organizationAddress.AddressType.GetValueOrDefault().EqualsIgnoringCase(Constants.AddressType.LPCOHolder))
						{
							holderOrganizationAddress = organizationAddress;
						}
						if ((!needLPCOApplicantOrganization || applicantOrganizationAddress != null) && (!needLPCOHolderOrganization || holderOrganizationAddress != null))
						{
							break;
						}
					}
				}

				if (!isApplicantOverriddenValue.HasValue && applicantOrganizationAddress != null)
				{
					isApplicantOverriddenValue = applicantOrganizationAddress.AddressOverride;
				}
				SetValue(result, CusCALPCOSchema.CLP_IsApplicantOverridden, isApplicantOverriddenValue);

				if (!isHolderOverriddenValue.HasValue && holderOrganizationAddress != null)
				{
					isHolderOverriddenValue = holderOrganizationAddress.AddressOverride;
				}
				SetValue(result, CusCALPCOSchema.CLP_IsHolderOverridden, isHolderOverriddenValue);

				if (doesLPCOApplicantNotReferenceDecOrgs && applicantOrganizationAddress != null)
				{
					FillJobDocAddress(cusCALPCO, applicantOrganizationAddress, CusCALPCOSchema.CLP_OA_Applicant);
				}
				if (doesLPCOHolderNotReferenceDecOrgs && holderOrganizationAddress != null)
				{
					FillJobDocAddress(cusCALPCO, holderOrganizationAddress, CusCALPCOSchema.CLP_OA_Holder);
				}
				SetValue(result, CusCALPCOSchema.CLP_HolderName, lpcoHolderNameValue);
				SetValue(result, CusCALPCOSchema.CLP_HolderContactEmail, authorizedPartyContactEmailValue);
				SetValue(result, CusCALPCOSchema.CLP_HolderContactName, authorizedPartyContactNameValue);
				SetValue(result, CusCALPCOSchema.CLP_HolderContactPhone, authorizedPartyContactPhoneValue);
				SetValue(result, CusCALPCOSchema.CLP_ApplicantName, lpcoApplicantNameValue);
				SetValue(result, CusCALPCOSchema.CLP_ApplicantContactEmail, applicantContactEmailValue);
				SetValue(result, CusCALPCOSchema.CLP_ApplicantContactName, applicantContactNameValue);
				SetValue(result, CusCALPCOSchema.CLP_ApplicantContactPhone, applicantContactPhoneValue);
				SetValue(result, CusCALPCOSchema.CLP_RN_NKSmeltAndPourCountryCode, meltAndPourCountryCode);

				if (result.GetValue(CusCALPCOSchema.CLP_AlternativeQuotaUQ).IsEmpty)
				{
					SetValue(result, CusCALPCOSchema.CLP_AlternativeQuotaQuantity, 0);
				}
			}
			return result;
		}

		void FillJobDocAddress(CusCALPCO cusCALPCO, OrganizationAddress address, SchemaGuidColumn column)
		{
			if (address != null)
			{
				var addressBO = new OrganisationDataObjectReader(address, logger, factory).GetMatched();
				if (addressBO != null)
				{
					SetValue(cusCALPCO, column, addressBO.PK);
				}
			}
		}
	}
}
