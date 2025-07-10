using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ApprovedCcpmReleaseCache
	{
		internal static void CacheInFactory(BusinessObjectFactory factory, IEnumerable<ZGuid> workflowScope)
		{
			factory.GetCachedValue(nameof(ApprovedCcpmReleaseCache), () => new ApprovedCcpmReleaseCache(factory, workflowScope));
		}

		internal static bool IsCCPMReadyToRelease(BusinessObjectFactory factory, ZGuid workflowPK)
		{
			var cache = factory.GetCachedValue<ApprovedCcpmReleaseCache>(nameof(ApprovedCcpmReleaseCache), () => null);

			return cache != null && cache.IsCCPMReadyToRelease(workflowPK);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
#if DEBUG
		public
#endif
 ApprovedCcpmReleaseCache(BusinessObjectFactory factory, IEnumerable<ZGuid> workflowScope)
		{
			var query = TagQueryProvider.GetTagMagnitudeFilter(TagProvider.GetCCPMReadyToReleaseTagMagnitudePK(), false, BMSRegistry.Instance.ReleaseGateConsiderParentWorkflowForCcpmRtrTag.Value, new ZSqlParameterCollection(), factory);
			query.AddToFilter(new ZQuery { AllowTableValuedParameters = true }.AddToFilter(ProcessHeaderSchema.PK, workflowScope));

			var sql = "SELECT FH_PK FROM dbo.ProcessHeader WHERE " + query.ParameterisedText.ParameterisedQueryText;
			cache = new HashSet<ZGuid>();

			using (var command = Db.Connection.Command(sql)) // This is much faster and better on memory than loading all these business objects when we just need the PKs of matching items.
			{
				foreach (var param in query.Params)
				{
					command.AddParameter(param);
				}
				using (var reader = command.ExecuteReader()) // This is much faster and better on memory than loading all these business objects when we just need the PKs of matching items.
				{
					while (reader.Read())
					{
						cache.Add(reader.GetGuid(0));
					}
				}
			}
		}

		readonly HashSet<ZGuid> cache;

		public bool IsCCPMReadyToRelease(ZGuid workflowPK)
		{
			return cache.Contains(workflowPK);
		}
	}
}
