using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.Management
{
	public static class UniversalJobLinkHelper
	{
		public static IEnumerable<IColumnIndexer> GetMatchingJobLinks(UniversalObjectFactory universalFactory, ZGuid parentID)
		{
			var query = new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, parentID);
			return universalFactory.RowFactory.Load(StmUniversalJobLinkSchema.Constants.TableName, query)
										.Select(DataObjectReader.GetColumnIndexerFromRow).ToArray();
		}

		public static IEnumerable<IColumnIndexer> GetMatchingJobLinks(UniversalObjectFactory universalFactory, ZString? sourceKey, DataContextType context, IOrgHeader recipientOrganisation, string parentTableCode)
		{
			if (!string.IsNullOrEmpty(sourceKey) && !string.IsNullOrEmpty(parentTableCode))
			{
				var query = new ZQuery(StmUniversalJobLinkSchema.UCL_SourceKey, sourceKey);
				query.AddToFilter(StmUniversalJobLinkSchema.UCL_OH_Owner, recipientOrganisation?.PK); // recipientOrganisation null means internal
				query.AddToFilter(StmUniversalJobLinkSchema.UCL_SourceType, context.ToString());
				query.AddToFilter(StmUniversalJobLinkSchema.UCL_ParentTableCode, parentTableCode);

				return universalFactory.RowFactory.Load(StmUniversalJobLinkSchema.Constants.TableName, query)
											.Select(DataObjectReader.GetColumnIndexerFromRow).ToArray();
			}

			return Enumerable.Empty<IColumnIndexer>();
		}

		public static IEnumerable<IUniversalJobLink> GetMatchingJobLinks(BusinessObject parentBO, DataContextType context, IOrgHeader recipientOrganisation)
		{
			if (parentBO != null)
			{
				var query = new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, parentBO.PK);
				query.AddToFilter(StmUniversalJobLinkSchema.UCL_OH_Owner, recipientOrganisation?.PK); // recipientOrganisation null means internal
				query.AddToFilter(StmUniversalJobLinkSchema.UCL_SourceType, context.ToString());

				var links = parentBO.Factory.Load<IStmUniversalJobLink>(query);
				if (links.Any())
				{
					return links.Select(l => new UniversalJobLink(l)).ToArray();
				}
			}

			return Enumerable.Empty<IUniversalJobLink>();
		}

		public static IEnumerable<IStmUniversalJobLink> GetMatchingJobLinkEntities(BusinessObject parentBO)
		{
			if (parentBO != null)
			{
				var query = new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, parentBO.PK);

				var links = parentBO.Factory.Load<IStmUniversalJobLink>(query);
				if (links.Any())
				{
					foreach (var link in links)
					{
						link.HasChanges = true;
					}
					return links;
				}
			}

			return Enumerable.Empty<IStmUniversalJobLink>();
		}

		public static IEnumerable<IUniversalJobLink> GetMatchingJobLinks(BusinessObject parentBO)
		{
			var linkEntities = GetMatchingJobLinkEntities(parentBO);
			if (linkEntities.Any())
			{
				return linkEntities.Select(l => new UniversalJobLink(l)).ToArray();
			}

			return Enumerable.Empty<IUniversalJobLink>();
		}
	}
}
