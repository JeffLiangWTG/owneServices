using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.Business
{
	public class ResponseNotificationHelper
	{
		public ResponseNotificationHelper(EDIReleaseMessage message)
		{
			fNotificationEmailGroups = GetWarehouseRNSNotificationEmailGroups(message.Factory, message);
		}

		public ResponseNotificationHelper(BusinessObjectFactory factory, ManifestForwardHouseBillWrapper wrapper)
		{
			fNotificationEmailGroups = GetForwardedManifestsNotificationEmailGroups(factory, wrapper);
		}

		public IEnumerable<ZGuid> NotificationEmailGroups
		{
			get { return fNotificationEmailGroups; }
		}

		readonly IEnumerable<ZGuid> fNotificationEmailGroups;

		public GlbBranch NotificationBranch
		{
			get { return fNotificationBranch; }
		}
		GlbBranch fNotificationBranch;

		IEnumerable<ZGuid> GetWarehouseRNSNotificationEmailGroups(BusinessObjectFactory factory, EDIReleaseMessage message)
		{
			return GetEmailGroups(factory, message.SubLocation, OrgCusCode.CodeTypes.ControlledPremisesID, CACustomsDataRegistry.Instance.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries, false);
		}

		IEnumerable<ZGuid> GetForwardedManifestsNotificationEmailGroups(BusinessObjectFactory factory, ManifestForwardHouseBillWrapper wrapper)
		{
			switch (wrapper.SNPType)
			{
				case SecondaryNotifyPartyTypeList.Codes.Warehouse:
					return GetEmailGroups(factory, wrapper.SnpIdentifier, OrgCusCode.CodeTypes.ControlledPremisesID, CACustomsDataRegistry.Instance.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries, true);
				case SecondaryNotifyPartyTypeList.Codes.Carrier:
					return GetEmailGroups(factory, wrapper.SnpIdentifier, OrgCusCode.CodeTypes.CarrierCode, CACustomsDataRegistry.Instance.SendCarrierSNPMessageDetailsToGroupAppliesAllCountries, true);
				case SecondaryNotifyPartyTypeList.Codes.Forwarder:
					return GetEmailGroups(factory, wrapper.SnpIdentifier, OrgCusCode.CodeTypes.CarrierCode, CACustomsDataRegistry.Instance.SendForwarderSNPMessageDetailsToGroupAppliesAllCountries, true);
				default:
					return GetEmailGroups(factory, wrapper.SnpIdentifier, null,
						delegate(GlbCompany company)
						{ return CACustomsDataRegistry.Instance.AccountSecurityNo.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty) == wrapper.SnpIdentifier; },
						CACustomsDataRegistry.Instance.SendBrokerSNPMessageDetailsToGroupAppliesAllCountries, true);
			}
		}

		#region GetEmailGroups

		delegate bool IsBranchMatchCusCodeDelegate(GlbBranch branch);
		delegate bool IsCompanyMatchCusCodeDelegate(GlbCompany company);

		IEnumerable<ZGuid> GetEmailGroups(BusinessObjectFactory factory, ZString codeInMessage, ZString orgCusCodeType, GuidRegistryItem registryItem, bool fallBackToSystemLevel)
		{
			return GetEmailGroups(factory, codeInMessage,
				delegate(GlbBranch branch)
				{
					var orgProxy = branch.OrgProxy;
					return (orgProxy != null && orgProxy.CustomsCodes.GetOrgCusCodesForCodeAndCountry(orgCusCodeType, Core.Constants.CountryCodes.Canada).Any(c => c.OK_CustomsRegNo == codeInMessage));
				},
				delegate(GlbCompany company)
				{
					var orgProxy = company.OrgProxy;
					return (orgProxy != null && orgProxy.CustomsCodes.GetOrgCusCodesForCodeAndCountry(orgCusCodeType, Core.Constants.CountryCodes.Canada).Any(c => c.OK_CustomsRegNo == codeInMessage));
				}, registryItem, fallBackToSystemLevel);
		}

		IEnumerable<ZGuid> GetEmailGroups(BusinessObjectFactory factory, ZString codeInMessage, IsBranchMatchCusCodeDelegate isBranchMatchCusCodeDelegate, IsCompanyMatchCusCodeDelegate isCompanyMatchCusCodeDelegate, GuidRegistryItem registryItem, bool fallBackToSystemLevel)
		{
			var emailGroups = new List<ZGuid>();
			if (!codeInMessage.IsEmpty)
			{
				if (isBranchMatchCusCodeDelegate != null)
				{
					foreach (var branch in new GlbBranch.Loader(factory).LoadAllBranchesInThisCountryActiveOnly(Core.Constants.CountryCodes.Canada))
					{
						if (isBranchMatchCusCodeDelegate(branch))
						{
							if (fNotificationBranch == null)
							{
								fNotificationBranch = branch;
							}

							var emailGroup = new ZGuid(registryItem.GetFallBackValueAtAllLevels(branch.Company.PK.ToGuid(), branch.PK.ToGuid(), Guid.Empty));
							if (emailGroup.IsValid && !emailGroups.Contains(emailGroup))
							{
								emailGroups.Add(emailGroup);
							}
						}
					}
				}

				if (emailGroups.Count == 0 && isCompanyMatchCusCodeDelegate != null)
				{
					foreach (var company in GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.Canada, factory))
					{
						if (isCompanyMatchCusCodeDelegate(company))
						{
							var emailGroup = new ZGuid(registryItem.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
							if (emailGroup.IsValid && !emailGroups.Contains(emailGroup))
							{
								emailGroups.Add(emailGroup);
							}
						}
					}
				}
			}

			if (emailGroups.Count == 0 && fallBackToSystemLevel)
			{
				var emailGroup = new ZGuid(registryItem.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
				if (emailGroup.IsValid)
				{
					emailGroups.Add(emailGroup);
				}
			}

			return emailGroups;
		}

		#endregion
	}
}
