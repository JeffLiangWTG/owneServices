using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRiskBusinessObject : NonPersistentBusinessObject
	{
		public ComplianceRiskBusinessObject(IBusiness hostBusinessEntity)
			: this(hostBusinessEntity, null)
		{
		}

		public ComplianceRiskBusinessObject(IBusiness hostBusinessEntity, ComplianceRiskStatus complianceRiskStatus)
		{
			ComplianceItemRiskStatusProvider = hostBusinessEntity as IComplianceItemRiskStatusProvider ?? throw new ArgumentException("Host Business Entity is null", nameof(hostBusinessEntity));
			CentralizedValidator.ValidateAll(ComplianceItemRiskStatusProvider);

			HostBusinessEntity = hostBusinessEntity;
			ComplianceRiskStatus = complianceRiskStatus ?? GetComplianceRiskStatus();

			(HostBusinessEntity as BusinessObject)?.RegisterEditableChildObject(ComplianceRiskStatus);

			ComplianceRiskStatus.ParentJob = this;
		}

		internal virtual void ReplaceComplianceRiskStatus(ComplianceRiskStatus riskStatus)
		{
			(HostBusinessEntity as BusinessObject)?.UnRegisterEditableChildObject(ComplianceRiskStatus);
			ComplianceRiskStatus = riskStatus;
			(HostBusinessEntity as BusinessObject)?.RegisterEditableChildObject(ComplianceRiskStatus);
			ComplianceRiskStatus.ParentJob = this;
		}

		bool IsCurrent => (HostBusinessEntity as IComplianceItemRiskStatusProvider)?.JobTime.IsCurrent ?? false;

		ComplianceRiskStatus GetComplianceRiskStatus()
		{
			var query = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, ComplianceItemRiskStatusProvider.ParentID);
			return ComplianceItemRiskStatusProvider.Factory.LoadTop1<ComplianceRiskStatus>(query) ?? CreateNewComplianceRiskStatus(ComplianceItemRiskStatusProvider, !IsCurrent);
		}

		public void RefreshData()
		{
			ComplianceRiskStatus.CommodityDetailCollection.LoadWithSuspendChanges();

			Parties.RemoveAll();
			var parties = CompliancePartyRiskStatusProvider?.Parties.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson));
			if (parties?.Any() ?? false)
			{
				DeniedPartyScreenerAsync.MergeDuplicateParties(parties.ToArray()).ForEach(party =>
				{
					if (party.ScreeningEntity != null && party.Country == null)
					{
						Parties.Add(new ComplianceRiskPartyWrapper(party));
					}
				});
			}

			Locations.RemoveAll();
			var locations = ComplianceLocationRiskStatusProvider?.Locations?.ToArray();

			if (locations?.Length > 0)
			{
				MergeDuplicateLocations(locations).ForEach(location =>
				{
					if (location.Value.Country != null)
					{
						Locations.Add(new ComplianceRiskLocationWrapper(location.Value));
					}
				});
			}
		}

		static Dictionary<ZGuid, IComplianceLocation> MergeDuplicateLocations(IComplianceLocation[] locations)
		{
			var uniqueLocations = new Dictionary<ZGuid, IComplianceLocation>();
			foreach (var location in locations)
			{
				if (!uniqueLocations.TryGetValue(location.Key, out var existingLocation))
				{
					uniqueLocations.Add(location.Key, location);
				}
				else
				{
					if (!existingLocation.Parents.Contains(location.Parent))
					{
						existingLocation.Parents.Add(location.Parent);
						if (existingLocation.ParentsDescription.Length > 0)
						{
							existingLocation.ParentsDescription += "; ";
						}

						existingLocation.ParentsDescription += location.ParentsDescription;
					}
				}
			}

			return uniqueLocations;
		}

		internal static ComplianceRiskStatus CreateNewComplianceRiskStatus(IComplianceItemRiskStatusProvider statusProvider, bool useNewFactory = false)
		{
			if (useNewFactory)
			{
				var newFactory = new BusinessObjectFactory();
				var complianceRiskStatus = newFactory.New<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentTableCode = statusProvider.ParentTableCode;
				complianceRiskStatus.COR_ParentID = statusProvider.ParentID;
				newFactory.Save();

				var query = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, statusProvider.ParentID);
				return statusProvider.Factory.LoadTop1<ComplianceRiskStatus>(query) ?? CreateNewComplianceRiskStatus(statusProvider);
			}
			else
			{
				var complianceRiskStatus = statusProvider.Factory.New<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentTableCode = statusProvider.ParentTableCode;
				complianceRiskStatus.COR_ParentID = statusProvider.ParentID;

				return complianceRiskStatus;
			}
		}

		public ComplianceRiskStatus ComplianceRiskStatus { get; private set; }

		public ComplianceRiskPartyWrapperCollection Parties { get; } = [];

		public ComplianceRiskLocationWrapperCollection Locations { get; } = [];

		public IComplianceItemRiskStatusProvider ComplianceItemRiskStatusProvider { get; }

		public ICompliancePartyRiskStatusProvider CompliancePartyRiskStatusProvider => ComplianceItemRiskStatusProvider as ICompliancePartyRiskStatusProvider;

		public IComplianceLocationRiskStatusProvider ComplianceLocationRiskStatusProvider => ComplianceItemRiskStatusProvider as IComplianceLocationRiskStatusProvider;

		public IComplianceCommodityRiskStatusProvider ComplianceCommodityRiskStatusProvider => ComplianceItemRiskStatusProvider as IComplianceCommodityRiskStatusProvider;

		public ISupportCheckCommodityRiskStatus CommodityRiskStatusChecker { get; set; }

		public IBusiness HostBusinessEntity { get; }

		public ComplianceCheckRequestModel ComplianceMaterialChangesSnapshot { get; set; }

		/// <summary>
		/// Gets the collection of <see cref="ComplianceJobEntityCache"/> objects associated with the current <see cref="ComplianceRiskStatus"/>.
		/// The collection is lazily initialized on first access and provides access to cached job entity compliance data for the business object.
		/// </summary>
		[BusinessObjectTestExclude]
		public ComplianceJobEntityCacheCollection JobEntityCacheCollection => jobEntityCacheCollection ??= new ComplianceJobEntityCacheCollection(ComplianceRiskStatus);
		ComplianceJobEntityCacheCollection jobEntityCacheCollection;

		/// <summary>
		/// Reloads the data in the database in an existing BusinessObject. Does not throw a save concurrency exception.
		/// </summary>
		public void ReloadSafeComplianceRiskStatusWithRelatedParties()
		{
			var complianceSnapshot = (ComplianceRiskStatus?.COR_OverallRisk,
				ComplianceRiskStatus?.COR_PartyRisk,
				ComplianceRiskStatus?.COR_LocationRisk,
				ComplianceRiskStatus?.COR_CommodityRisk);

			(bool Overall, bool Party, bool Location, bool Commodity) complianceHasChangesTo = ((bool)ComplianceRiskStatus?.COR_OverallRiskInfo.HasChanges,
				(bool)ComplianceRiskStatus?.COR_PartyRiskInfo.HasChanges,
				(bool)ComplianceRiskStatus?.COR_LocationRiskInfo.HasChanges,
				(bool)ComplianceRiskStatus?.COR_CommodityRiskInfo.HasChanges);

			ComplianceRiskStatus?.ReloadSafe();

			if (complianceHasChangesTo.Overall)
			{
				ComplianceRiskStatus.COR_OverallRisk = (ZString)complianceSnapshot.COR_OverallRisk;
			}

			if (complianceHasChangesTo.Party)
			{
				ComplianceRiskStatus.COR_PartyRisk = (ZString)complianceSnapshot.COR_PartyRisk;
			}

			if (complianceHasChangesTo.Location)
			{
				ComplianceRiskStatus.COR_LocationRisk = (ZString)complianceSnapshot.COR_LocationRisk;
			}

			if (complianceHasChangesTo.Commodity)
			{
				ComplianceRiskStatus.COR_CommodityRisk = (ZString)complianceSnapshot.COR_CommodityRisk;
			}

			var orgParties = new List<OrgHeader>();
			var vesselParties = new List<RefVessel>();
			var jobDocAddressParties = new List<JobDocAddress>();
			var transportParties = new List<ITransport>();
			var locations = new List<RefCountry>();

			(CompliancePartyRiskStatusProvider?.Parties?.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson)))?.ToArray().ForEach(party =>
			{
				if (party?.ScreeningEntity != null && !party.ScreeningEntity.HasChanges && party.ScreeningEntity is not NonPersistentBusinessObject)
				{
					if (party.ScreeningEntity is OrgHeader orgParty)
					{
						orgParties.Add(orgParty);
					}
					else if (party.ScreeningEntity is RefVessel vesselParty)
					{
						vesselParties.Add(vesselParty);
					}
					else if (party.ScreeningEntity is JobDocAddress jobDocAddressParty)
					{
						jobDocAddressParties.Add(jobDocAddressParty);
					}
					else if (party.ScreeningEntity is ITransport transportParty)
					{
						transportParties.Add(transportParty);
					}
				}
			});

			(ComplianceLocationRiskStatusProvider?.Locations)?.ToArray().ForEach(location =>
			{
				if (location?.Country != null && !location.Country.HasChanges && location.Country is RefCountry country)
				{
					locations.Add(country);
				}
			});

			if (orgParties.Count > 0)
			{
				var changedOrgPartiesQuery = new ZDBOnlyQuery(typeof(OrgHeader))
				{
					ReLoadExistingRows = true,
					IgnoreDbQueryCache = true,
				};

				var groupedOrgParties = orgParties
					.GroupBy(u => u.PK)
					.Select(u => u.First())
					.GroupBy(item => item.OH_ScreeningStatus)
					.Select(group => new ZQuery() { AllowTableValuedParameters = true }.AddToFilter(OrgHeaderSchema.PK, group.Select(u => u.PK))
					.AddToFilter(OrgHeaderSchema.OH_ScreeningStatus, SQLComparisonOperator.NotEqual, group.Key));

				var combinedQuery = groupedOrgParties.Aggregate((current, next) => current.AddToFilter(next, JoinCondition.Or));
				changedOrgPartiesQuery.AddToFilter(combinedQuery);

				ComplianceRiskStatus.Factory.Load<OrgHeader>(changedOrgPartiesQuery);
			}

			if (vesselParties.Count > 0)
			{
				var changedVesselsQuery = new ZDBOnlyQuery(typeof(RefVessel))
				{
					ReLoadExistingRows = true,
					IgnoreDbQueryCache = true,
				};

				var groupedVesselParties = vesselParties
					.GroupBy(u => u.PK)
					.Select(u => u.First())
					.GroupBy(item => item.RV_ScreeningStatus)
					.Select(group => new ZQuery() { AllowTableValuedParameters = true }.AddToFilter(RefVesselSchema.PK, group.Select(u => u.PK))
					.AddToFilter(RefVesselSchema.RV_ScreeningStatus, SQLComparisonOperator.NotEqual, group.Key));

				var combinedQuery = groupedVesselParties.Aggregate((current, next) => current.AddToFilter(next, JoinCondition.Or));
				changedVesselsQuery.AddToFilter(combinedQuery);
				ComplianceRiskStatus.Factory.Load<RefVessel>(changedVesselsQuery);
			}

			if (jobDocAddressParties.Count > 0)
			{
				var changedJobDocAddressQuery = new ZDBOnlyQuery(typeof(JobDocAddress))
				{
					ReLoadExistingRows = true,
					IgnoreDbQueryCache = true,
				};

				var groupedJobDocAddressParties = jobDocAddressParties
					.GroupBy(u => u.PK)
					.Select(u => u.First())
					.GroupBy(item => item.E2_ScreeningStatus)
					.Select(group => new ZQuery() { AllowTableValuedParameters = true }.AddToFilter(JobDocAddressSchema.PK, group.Select(u => u.PK))
					.AddToFilter(JobDocAddressSchema.E2_ScreeningStatus, SQLComparisonOperator.NotEqual, group.Key));

				var combinedQuery = groupedJobDocAddressParties.Aggregate((current, next) => current.AddToFilter(next, JoinCondition.Or));
				changedJobDocAddressQuery.AddToFilter(combinedQuery);
				ComplianceRiskStatus.Factory.Load<JobDocAddress>(changedJobDocAddressQuery);
			}

			if (transportParties.Count > 0)
			{
				var changedTransportPartiesQuery = new ZDBOnlyQuery(transportParties[0].GetType())
				{
					ReLoadExistingRows = true,
					IgnoreDbQueryCache = true,
				};

				var groupedTransportParties = transportParties
					.GroupBy(u => (u as BusinessObject).PK)
					.Select(u => u.First())
					.GroupBy(item => item.JW_VesselScreeningStatus)
					.Select(group => new ZQuery() { AllowTableValuedParameters = true }.AddToFilter(JobConsolTransportSchema.PK, group.Select(u => (u as BusinessObject).PK))
					.AddToFilter(JobConsolTransportSchema.JW_VesselScreeningStatus, SQLComparisonOperator.NotEqual, group.Key));

				var combinedQuery = groupedTransportParties.Aggregate((current, next) => current.AddToFilter(next, JoinCondition.Or));
				changedTransportPartiesQuery.AddToFilter(combinedQuery);
				ComplianceRiskStatus.Factory.Load<ITransport>(changedTransportPartiesQuery);
			}

			if (locations.Count > 0)
			{
				var changedLocationsQuery = new ZDBOnlyQuery(typeof(RefCountry))
				{
					ReLoadExistingRows = true,
					IgnoreDbQueryCache = true,
				};

				var groupedLocations = locations
					.GroupBy(u => u.PK)
					.Select(u => u.First())
					.GroupBy(item => item.RN_IsSanctioned)
					.Select(group => new ZQuery() { AllowTableValuedParameters = true }.AddToFilter(RefCountrySchema.PK, group.Select(u => u.PK))
					.AddToFilter(RefCountrySchema.RN_IsSanctioned, SQLComparisonOperator.NotEqual, group.Key));

				var combinedQuery = groupedLocations.Aggregate((current, next) => current.AddToFilter(next, JoinCondition.Or));
				changedLocationsQuery.AddToFilter(combinedQuery);
				ComplianceRiskStatus.Factory.Load<RefCountry>(changedLocationsQuery);
			}
		}
	}
}
