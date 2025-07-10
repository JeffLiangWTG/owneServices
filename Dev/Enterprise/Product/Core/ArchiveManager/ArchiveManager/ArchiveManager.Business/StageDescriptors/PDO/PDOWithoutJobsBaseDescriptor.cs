using System;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.StageDescriptors.PDO
{
	public abstract class PDOWithoutJobsBaseDescriptor : PDOPurgeStageDescriptor
	{
		public virtual Type TypeToArchive { get; }

		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			var query = new ZDBOnlyQuery(TypeToArchive);
			_ = query.AddToFilter(MainDateFilterColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, config.ArchiveJobsOnOrBeforeThisDate);
			var subquery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, true);
			query.AddSubQuery(subquery, JoinCondition.And);
			query.OrderBy = $"{MainDateFilterColumn.Name}, {MainArchiveNKColumn.Name}, {MainArchivePKColumn.Name}";
			return query;
		}
	}
}
