using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalCopy.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "IOC")]
	class StmUniversalCopyDeleter : DeleteChecker
	{
		public override void BeforeSuccessfulDelete(BusinessObject businessObject)
		{
			base.BeforeSuccessfulDelete(businessObject);
			if (businessObject is IUniversalCopySelectivelySupportable ucSupportable && !ucSupportable.SupportsUniversalCopy)
			{
				return;
			}
			foreach (var relatedCopyJob in FindRelatedCopyJobs(businessObject))
			{
				relatedCopyJob.Delete();
			}
		}

		StmUniversalCopy[] FindRelatedCopyJobs(BusinessObject businessObject)
		{
			StmUniversalCopy[] result;
			if (string.IsNullOrEmpty(businessObject.TablePrefix))
			{
				result = System.Array.Empty<StmUniversalCopy>();
			}
			else
			{
				ZQuery query = RelatedCopyJobsQuery(businessObject);
				query.FetchOnlyFromLocalCache = !businessObject.IsInDatabase;
				result = businessObject.Factory.Load<StmUniversalCopy>(query);
			}
			return result;
		}

		public override DeleteDetails DeleteDetails(BusinessObject businessObject)
		{
			return new DeleteDetails.Allow();
		}

		public override void AddFetchHint(BusinessObject businessObject)
		{
			if (businessObject is IUniversalCopySelectivelySupportable ucSupportable && !ucSupportable.SupportsUniversalCopy)
			{
				return;
			}
			if (businessObject.IsInDatabase)
			{
				businessObject.Factory.AddFetchHint(new DeleterFetchHint(businessObject));
			}
		}

		ZQuery RelatedCopyJobsQuery(BusinessObject businessObject)
		{
			var query = new ZQuery(StmUniversalCopySchema.SUC_CopyObjectTableCode, businessObject.TablePrefix);
			query.AddToFilter(StmUniversalCopySchema.SUC_CopyObjectId, businessObject.PK);
			return query;
		}

		class DeleterFetchHint : IFetchHint
		{
			public DeleterFetchHint(BusinessObject businessObject)
			{
				this.businessObjectTablePrefix = businessObject.TablePrefix;
				this.businessObjectPK = businessObject.PK;
			}

			readonly string businessObjectTablePrefix;
			readonly ZGuid businessObjectPK;

			#region IFetchHint Members

			string IFetchHint.BuilderKey
			{
				get { return "StmUniversalCopy:Deleter:" + businessObjectTablePrefix; }
			}

			void IFetchHint.GenerateQuery(QueryBuilder builder)
			{
				if (builder.IsEmpty)
				{
					builder.Init(new ZQuery(StmUniversalCopySchema.SUC_CopyObjectTableCode, businessObjectTablePrefix), StmUniversalCopySchema.SUC_CopyObjectId);
				}

				builder.AddValue(businessObjectPK);
			}

			IQueryHashKey IFetchHint.GetHashKeyObject()
			{
				return query.GetHashKey();
			}

			ZQuery IFetchHint.GetQuery()
			{
				if (query == null)
				{
					query = new ZQuery(StmUniversalCopySchema.SUC_CopyObjectTableCode, businessObjectTablePrefix);
					query.AddToFilter(StmUniversalCopySchema.SUC_CopyObjectId, businessObjectPK);
				}

				return query;
			}

			ZQuery query;

			bool IFetchHint.IsDataHintLoaded { get; set; }

			bool IFetchHint.IsNeeded(QueryHistoryProvider historyProvider)
			{
				return !historyProvider.IsQueryCached(((IFetchHint)this).TableName, query);
			}

			IEnumerable<CargoWise.Schema.SchemaColumn> IFetchHint.LoadWithBlobs
			{
				get { return query.LoadWithBlobs.Union(query.BlobFilters); }
			}

			string IFetchHint.TableName
			{
				get { return StmUniversalCopySchema.Constants.TableName; }
			}

			#endregion
		}
	}
}
