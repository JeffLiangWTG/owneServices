using System;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Business.StageDescriptors.PDO;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.PDO
{
	abstract class PDOWithoutJobsBaseDescriptorTest : PDOPurgeStageDescriptorTest
	{
		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
		{
			get
			{
				var query = new ZDBOnlyQuery(ExpectedTypeToArchive);
				_ = query.AddToFilter(ExpectedMainArchiveDateFilterColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, TestDate);
				var subquery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, true);
				query.AddSubQuery(subquery, JoinCondition.And);
				query.OrderBy = $"{ExpectedMainArchiveDateFilterColumn.Name}, {ExpectedMainArchiveNKColumn.Name}, {ExpectedMainArchivePKColumn.Name}";
				return query;
			}
		}

		protected override ZQuery ExpectedMainArchiveFilterWithDeclarations
			=> ExpectedMainArchiveFilterWithoutDeclarations;

		public abstract Type ExpectedTypeToArchive { get; }

		public void TestExpectedTypeToArchive()
		{
			var pdoWithoutJobsDescriptor = StageDescriptor as PDOWithoutJobsBaseDescriptor;
			AssertNotNull(pdoWithoutJobsDescriptor);
			AssertEquals(ExpectedTypeToArchive, pdoWithoutJobsDescriptor.TypeToArchive);
		}
	}
}
