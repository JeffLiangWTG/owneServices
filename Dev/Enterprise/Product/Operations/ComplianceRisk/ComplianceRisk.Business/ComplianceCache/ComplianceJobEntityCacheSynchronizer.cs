using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.Business
{
	public static class ComplianceJobEntityCacheSynchronizer
	{
		/// <summary>
		/// Synchronizes the <see cref="ComplianceJobEntityCacheCollection"/> of the specified <see cref="ComplianceRiskBusinessObject"/> with the current in-memory party data.
		/// If <paramref name="forceResynchronization"/> is <c>true</c>, or if job entity caching is enabled, the host business entity has changes, or the cache is out of sync,
		/// this method updates the cache to accurately reflect the current set of related parties. Otherwise, removes all entries from the entity cache.
		/// </summary>
		/// <param name="complianceRiskBizO">The compliance risk business object whose entity cache should be synchronized.</param>
		/// <param name="forceResynchronization">If <c>true</c>, forces the cache to be resynchronized regardless of current state.</param>
		public static void JobEntityCacheSyncCore(this ComplianceRiskBusinessObject complianceRiskBizO, bool forceResynchronization = false)
		{
			if ((ComplianceRiskHelper.IsJobEntitiesCachingEnabled && IsJobPartyProviderWithChanges(complianceRiskBizO)) || forceResynchronization)
			{
				var jobEndDate = complianceRiskBizO.ComplianceItemRiskStatusProvider.JobTime.JobEndDate.UtcToDateTimeOffset();
				if (jobEndDate > ZDateTime.UtcNow.UtcToDateTimeOffset()
					&& IsJobEntityCacheOutOfSync(complianceRiskBizO))
				{
					ExecuteJobEntityCacheReconciliation(complianceRiskBizO);
				}
				else
				{
					complianceRiskBizO.JobEntityCacheCollection.RemoveAndDeleteAll();
				}
			}
		}

		#region validation rule

		/// <summary>
		/// Determines whether the specified <see cref="ComplianceRiskBusinessObject"/> has a host business entity
		/// that implements <see cref="ICompliancePartyRiskStatusProvider"/> and has unsaved changes.
		/// </summary>
		/// <param name="complianceRiskBizO">The compliance risk business object to evaluate.</param>
		/// <returns>
		/// <c>true</c> if the host business entity is a party risk provider and has changes; otherwise, <c>false</c>.
		/// </returns>
		static bool IsJobPartyProviderWithChanges(ComplianceRiskBusinessObject complianceRiskBizO)
		{
			return complianceRiskBizO?.HostBusinessEntity is ICompliancePartyRiskStatusProvider
				&& complianceRiskBizO.HostBusinessEntity.HasChanges;
		}

		/// <summary>
		/// Determines whether the <see cref="ComplianceJobEntityCacheCollection"/> for the specified <see cref="ComplianceRiskBusinessObject"/>
		/// is out of sync with the current in-memory set of related parties. Compares the set of (EntityID, TablePrefix) pairs
		/// in the cache with those in memory and returns <c>true</c> if they do not match; otherwise, <c>false</c>.
		/// </summary>
		/// <param name="complianceRiskBizO">The compliance risk business object to check for cache synchronization.</param>
		/// <returns><c>true</c> if the entity cache and in-memory parties are not synchronized; otherwise, <c>false</c>.</returns>
		static bool IsJobEntityCacheOutOfSync(ComplianceRiskBusinessObject complianceRiskBizO)
		{
			complianceRiskBizO.JobEntityCacheCollection.Load();

			var entityCache = complianceRiskBizO.JobEntityCacheCollection
				.Select(c => (c.CJE_EntityID, c.CJE_EntityTableCode))
				.ToHashSet();

			var memoryCache = GetMemoryCache((BusinessObject)complianceRiskBizO.HostBusinessEntity)
				.Select(c => (c.Key, (ZString)c.ScreeningEntity.TablePrefix))
				.ToHashSet();

			return !entityCache.SetEquals(memoryCache);
		}

		#endregion

		/// <summary>
		/// Reconciles the <see cref="ComplianceJobEntityCacheCollection"/> of the specified <see cref="ComplianceRiskBusinessObject"/>
		/// with the current in-memory set of related parties. This method ensures the cache accurately reflects the current state by:
		/// <list type="bullet">
		/// <item>Adding new cache entries for parties present in memory but missing from the cache.</item>
		/// <item>Removing obsolete cache entries that are no longer present in memory.</item>
		/// </list>
		/// </summary>
		/// <param name="complianceRiskBizO">The compliance risk business object whose entity cache should be reconciled with memory.</param>
		static void ExecuteJobEntityCacheReconciliation(ComplianceRiskBusinessObject complianceRiskBizO)
		{
			// Retrieves a lookup of ScreeningParty instances grouped by (Key, TablePrefix) for the specified host business entity.
			// This lookup represents the current set of parties in memory that are relevant for job entity cache synchronization.
			var memoryCache = GetMemoryCache((BusinessObject)complianceRiskBizO.HostBusinessEntity)
				.ToLookup(c => (c.Key, (ZString)c.ScreeningEntity.TablePrefix));

			var entityCache = complianceRiskBizO.JobEntityCacheCollection
				.Cast<ComplianceJobEntityCache>()
				.ToLookup(c => (c.CJE_EntityID, c.CJE_EntityTableCode));

			// Add missing entries
			foreach (var memorykey in memoryCache.Where((mkey) => !entityCache.Contains(mkey.Key)))
			{
				complianceRiskBizO.JobEntityCacheCollection.Add(AddToCache(complianceRiskBizO.ComplianceRiskStatus, memorykey.First()));
			}

			// Remove obsolete entries
			foreach (var entityKey in entityCache.Where((eKey) => !memoryCache.Contains(eKey.Key)))
			{
				complianceRiskBizO.JobEntityCacheCollection.RemoveAndDelete(entityKey.First());
			}
		}

		/// <summary>
		/// Returns a filtered collection of <see cref="ScreeningParty"/> instances associated with the specified job.
		/// Only parties with a non-empty key and whose parent or associated job primary key matches the current or related job(s) are included.
		/// </summary>
		/// <param name="hostJob">The business object representing the job to evaluate.</param>
		/// <returns>
		/// An <see cref="IEnumerable{ScreeningParty}"/> containing the relevant screening parties for the job.
		/// </returns>
		static IEnumerable<ScreeningParty> GetMemoryCache(BusinessObject hostJob)
		{
			var associatedJobPKs = GetAssociatedJobPK(hostJob);
			var provider = (ICompliancePartyRiskStatusProvider)hostJob;
			return provider.Parties
				.Cast<ScreeningParty>()
				.Where(c =>
					!c.Key.IsEmpty
					&& (associatedJobPKs.Contains(c.Parent.PK) || c.AssociatedJobPK == hostJob.PK));
		}

		/// <summary>
		/// Returns a set of relevant job primary keys for the specified business object.
		/// If the job is a quoted booking with a forwarding shipment that is a booking and not yet forward registered,
		/// the set includes both the host job's PK and the related shipment's PK; otherwise, it contains only the host job's PK.
		/// </summary>
		/// <param name="hostJob">The business object to evaluate for associated job primary keys.</param>
		/// <returns>A <see cref="HashSet{ZGuid}"/> containing the relevant job primary key(s).</returns>
		static HashSet<ZGuid> GetAssociatedJobPK(BusinessObject hostJob)
		{
			if (hostJob is IQuotedBooking booking
				&& booking.ForwardingShipment is IForwardingShipment shipment
				&& shipment.JS_IsBooking
				&& !shipment.JS_IsForwardRegistered)
			{
				return [hostJob.PK, shipment.PK];
			}
			return [hostJob.PK];
		}

		/// <summary>
		/// Creates and initializes a new <see cref="ComplianceJobEntityCache"/> instance for the specified <see cref="ScreeningParty"/>.
		/// The new cache entry is associated with the given <see cref="ComplianceRiskStatus"/> and is populated with the party's key and table prefix.
		/// </summary>
		/// <param name="complianceRiskStatus">The compliance risk status to associate with the new cache entry.</param>
		/// <param name="screeningParty">The screening party for which the cache entry is created.</param>
		/// <returns>A new <see cref="ComplianceJobEntityCache"/> instance initialized with the provided data.</returns>
		static ComplianceJobEntityCache AddToCache(ComplianceRiskStatus complianceRiskStatus, ScreeningParty screeningParty)
		{
			var cache = complianceRiskStatus.Factory.New<ComplianceJobEntityCache>();
			cache.CJE_COR_ComplianceRisk = complianceRiskStatus.PK;
			cache.CJE_EntityID = screeningParty.Key;
			cache.CJE_EntityTableCode = screeningParty.ScreeningEntity.TablePrefix;
			return cache;
		}
	}
}
