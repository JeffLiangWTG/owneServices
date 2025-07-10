using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.OrgMatching;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPEOrganisationMatching
	{
		public UPEOrganisationMatching(BusinessObjectFactory factory, bool createUnMatchedOrganisations)
		{
			Factory = factory;
			CreateUnMatchedOrganisations = createUnMatchedOrganisations;
		}
		readonly BusinessObjectFactory Factory;
		readonly bool CreateUnMatchedOrganisations;

		public OrgHeader TryMatchOrganisation(ZString fullName, ZString closestPort, ZString city, ZString phone, ZString fax, ZString postCode, ZString state, ZString address1, ZString address2, ZString country, ZString accountNumber)
		{
			var key = string.Join("|", fullName, closestPort, city, phone, fax, postCode, state, address1, address2, country, accountNumber);
			if (!OrganisationKeys.ContainsKey(key))
			{
				var orgAddressForMatching = GetOrgAddressForMatching();
				var result = MatchOnAccountNumber(accountNumber, orgAddressForMatching);
				if (result == null)
				{
					var matcher = new OrganisationMatcher(Factory, IfUnmatched.TakeBehaviourFromOverallSetting) { ShouldUseNewEngine = true };
					result = matcher.GetMatchingAddress(orgAddressForMatching, null, false)?.Header;
				}

				if (CreateUnMatchedOrganisations && result == null)
				{
					result = CreateOrganisation(orgAddressForMatching);
				}
				OrganisationKeys[key] = result;
			}
			return OrganisationKeys[key];

			OrganizationAddressFormatted GetOrgAddressForMatching()
			{
				return new OrganizationAddressFormatted(new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance)
				{
					Address1 = address1,
					Address2 = address2,
					City = city,
					CompanyName = fullName,
					Country = new Country() { Code = country },
					Fax = fax,
					Phone = phone,
					Port = new UNLOCO() { Code = closestPort },
					Postcode = postCode,
					State = state
				});
			}
		}

		OrgHeader MatchOnAccountNumber(ZString accountNumber, OrganizationAddressFormatted orgAddressForMatching)
		{
			OrgHeader result = null;
			accountNumber = accountNumber.Trim();
			if (!accountNumber.IsEmpty)
			{
				var candidateMatches = Factory.Load<OrgCusCode>(GetOrgCusCodeFilter(accountNumber));
				if (candidateMatches.Length == 1)
				{
					var candidateOrgForMatch = Factory.Load<OrgHeader>(candidateMatches[0].OK_OH);
					if (candidateOrgForMatch.OH_IsActive &&
						(candidateOrgForMatch.OH_FullName == orgAddressForMatching.CompanyName.Value
						|| !candidateOrgForMatch.MainAddress.OA_Address1.IsEmpty && candidateOrgForMatch.MainAddress.OA_Address1 == orgAddressForMatching.Address1.Value
						|| !candidateOrgForMatch.MainAddress.OA_City.IsEmpty && candidateOrgForMatch.MainAddress.OA_City == orgAddressForMatching.City.Value
						|| !candidateOrgForMatch.MainAddress.OA_PostCode.IsEmpty && candidateOrgForMatch.MainAddress.OA_PostCode == orgAddressForMatching.Postcode.Value
						|| !candidateOrgForMatch.MainAddress.OA_Phone.IsEmpty && candidateOrgForMatch.MainAddress.OA_Phone == orgAddressForMatching.Phone.Value
						|| !candidateOrgForMatch.MainAddress.OA_Fax.IsEmpty && candidateOrgForMatch.MainAddress.OA_Fax == orgAddressForMatching.Fax.Value))
					{
						result = candidateOrgForMatch;
					}
				}
			}
			return result;
		}

		public static ZQuery GetOrgCusCodeFilter(ZString accountNumber)
		{
			var query = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, accountNumber);
			query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber);
			return query;
		}

		OrgHeader CreateOrganisation(OrganizationAddressFormatted orgAddressForMatching)
		{
			OrgHeader result = null;
			var fullName = orgAddressForMatching.CompanyName.Value;
			var closestPort = orgAddressForMatching.Port.Code.Value;
			var address1 = orgAddressForMatching.Address1.Value;
			if (!fullName.IsEmpty && !closestPort.IsEmpty && !address1.IsEmpty)
			{
				var dummyOrg = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(orgAddressForMatching, Factory);
				var finder = new UPELowThresholdOrgHeaderMatchEngineFinder(dummyOrg, false, true);
				var potentialMatches = finder.FindPotentialDuplicates(false);

				if (!potentialMatches.Any())
				{
					result = Factory.New<OrgHeader>();
					result.OH_FullName = fullName;
					result.OH_RL_NKClosestPort = closestPort;
					var mainAddress = result.MainAddress;
					mainAddress.OA_Address1 = address1;
					mainAddress.OA_Address2 = orgAddressForMatching.Address2.Value;
					mainAddress.OA_City = orgAddressForMatching.City.Value;
					mainAddress.OA_PostCode = orgAddressForMatching.Postcode.Value;
					mainAddress.OA_State = orgAddressForMatching.State.Value;
					mainAddress.OA_Phone = orgAddressForMatching.Phone.Value;
					mainAddress.OA_Fax = orgAddressForMatching.Fax.Value;
					mainAddress.OA_RL_NKRelatedPortCode = closestPort;
					mainAddress.OA_RN_NKCountryCode = orgAddressForMatching.Country.Code.Value.Left(OrgAddress.Schema.OA_RN_NKCountryCodeMaxLength);
				}
			}
			return result;
		}

		Dictionary<string, OrgHeader> OrganisationKeys => organisationKeys ?? (organisationKeys = new Dictionary<string, OrgHeader>());
		Dictionary<string, OrgHeader> organisationKeys;

		class UPELowThresholdOrgHeaderMatchEngineFinder : OrgHeaderMatchEngineFinder
		{
			public UPELowThresholdOrgHeaderMatchEngineFinder(DeduplicationOrgHeader header, bool shouldUseCache, bool useMaxRecords) : base(header, shouldUseCache, useMaxRecords)
			{
			}

			protected override double GetScoreThresholdFromConfidenceRating() => 0.25;
		}

		internal IDisposable TemporarilySetOrganisationForTest(string key, OrgHeader value)
		{
			OrganisationKeys[key] = value;
			return new DisposableAction(() => OrganisationKeys.Remove(key));
		}
	}
}
