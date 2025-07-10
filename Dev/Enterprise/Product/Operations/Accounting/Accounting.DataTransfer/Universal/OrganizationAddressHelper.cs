using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCountry = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;
using UniversalOrgAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalRegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalRegistrationNumberType = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumberType;
using UniversalUNLOCO = Enterprise.UniversalDataBuss.DataObjects.Universal.UNLOCO;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0004: Cast is redundant", Justification = "Failed unit tests: without ZString? casting, its value will be string.Empty instead of null")]
	internal static class OrganizationAddressHelper
	{
		internal static UniversalOrgAddress CreateOrgAddress(IDataObjectWriterStrategy strategy, OrgHeader orgHeader, AccComplianceReport report)
		{
			var result = new UniversalOrgAddress(strategy);

			if (orgHeader.OH_Code == report.Company.OrgProxy.OH_Code)
			{
				result.AddressType = TransactionBatchOrganizationType.SendingCompany;
				result.GovRegNum = report.Company.GC_BusinessRegNo;
			}
			else if (report.Branch != null && report.Branch.OrgProxy != null && orgHeader.OH_Code == report.Branch.OrgProxy.OH_Code)
			{
				result.AddressType = TransactionBatchOrganizationType.SendingBranch;
			}
			else
			{
				result.AddressType = OrgAddressType.Office.ToString();
			}

			result.AddressOverride = ZBool.False;
			result.OrganizationCode = orgHeader.OH_Code;
			CopyOfficeAddressDetails(result, orgHeader.Addresses.DefaultAddressOfType(OrgAddressType.Office));

			if (orgHeader.CustomsCodes.Any())
			{
				if (result.SetRegistrationNumberCollection(() => new List<UniversalRegistrationNumber>()))
				{
					foreach (OrgCusCode regNo in orgHeader.CustomsCodes)
					{
						result.RegistrationNumberCollection.Add(CreateRegistrationNumber(regNo));
					}
				}
			}

			return result;
		}

		static void CopyOfficeAddressDetails(UniversalOrgAddress orgAddress, OrgAddress officeAddress)
		{
			if (officeAddress == null)
			{
				throw new ArgumentNullException(nameof(officeAddress));
			}

			orgAddress.Port = officeAddress.OA_RL_NKRelatedPortCode.IsEmpty ? null : new UniversalUNLOCO()
			{
				Code = officeAddress.OA_RL_NKRelatedPortCode,
				Name = officeAddress.PortName
			};

			ZString? companyNameOverride = officeAddress.OA_CompanyNameOverride.IsEmpty ? null : (ZString?)officeAddress.OA_CompanyNameOverride;
			orgAddress.CompanyName = new ZString((companyNameOverride.HasValue && !companyNameOverride.Value.IsEmpty) ? companyNameOverride : officeAddress.Header.OH_FullName).Left(50); //orgAddress.GetType().GetField("CompanyName").GetCustomAttribute<MaxLengthAttribute>().MaxLength);
			orgAddress.Country = officeAddress.OA_RN_NKCountryCode.IsEmpty ? null : new UniversalCountry()
			{
				Code = officeAddress.OA_RN_NKCountryCode,
				Name = officeAddress.Country != null ? officeAddress.Country.RN_DescMultilingual : null
			};

			orgAddress.ScreeningStatus = officeAddress.Header.OH_ScreeningStatus.IsEmpty ? null : new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair()
			{
				Code = officeAddress.Header.OH_ScreeningStatus,
				Description = new ScreeningStatusesList().GetDescriptionFromCode(officeAddress.Header.OH_ScreeningStatus)
			};

			orgAddress.AddressShortCode = officeAddress.OA_Code;
			orgAddress.Address1 = officeAddress.OA_Address1.IsEmpty ? null : (ZString?)officeAddress.OA_Address1;
			orgAddress.Address2 = officeAddress.OA_Address2.IsEmpty ? null : (ZString?)officeAddress.OA_Address2;
			orgAddress.City = officeAddress.OA_City.IsEmpty ? null : (ZString?)officeAddress.OA_City;
			orgAddress.Postcode = officeAddress.OA_PostCode.IsEmpty ? null : (ZString?)officeAddress.OA_PostCode;
			orgAddress.State = officeAddress.OA_State.IsEmpty ? null : (ZString?)officeAddress.OA_State;

			orgAddress.Email = officeAddress.OA_Email.IsEmpty ? null : (ZString?)officeAddress.OA_Email;
			orgAddress.Fax = officeAddress.OA_Fax.IsEmpty ? null : (ZString?)officeAddress.OA_Fax;
			orgAddress.Phone = officeAddress.OA_Phone.IsEmpty ? null : (ZString?)officeAddress.OA_Phone;
		}

		internal static UniversalRegistrationNumber CreateRegistrationNumber(OrgCusCode regNo)
		{
			UniversalCountry country = null;
			if (!regNo.OK_RN_NKCodeCountry.IsEmpty)
			{
				country = new UniversalCountry();
				country.Code = regNo.OK_RN_NKCodeCountry.IsEmpty ? null : regNo.OK_RN_NKCodeCountry;
				country.Name = regNo.CodeCountry.RN_DescMultilingual.IsEmpty ? null : regNo.CodeCountry.RN_DescMultilingual;
			}

			UniversalRegistrationNumberType type = null;
			if (!regNo.OK_CodeType.IsEmpty)
			{
				type = new UniversalRegistrationNumberType();
				type.Code = regNo.OK_CodeType.IsEmpty ? null : regNo.OK_CodeType;
				type.Description = regNo.CustomsRegNoFieldType.IsEmpty ? null : regNo.CustomsRegNoFieldType;
			}

			var value = regNo.OK_CustomsRegNo.IsEmpty ? null : (ZString?)regNo.OK_CustomsRegNo;

			return new UniversalRegistrationNumber() { CountryOfIssue = country, Type = type, Value = value };
		}
	}
}
