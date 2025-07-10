using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	public class BMQueryOptimisationTest : BMSTestCaseWithFactory
	{
		public void TestProcessTasksIndexUsage_ShouldUseRelevantTimeColumns_ShouldPreventExcessiveRIDLookups()
		{
			#region QueryString_ExistingIndex
			var query_usesExistingIndex = @"
P9_FH_ProcessHeader in (
	Select P9_FH_ProcessHeader
	From dbo.ProcessTasks
	Where
		P9_Status = 'CLS' 
		AND P9_ActualDuration is not null
		AND P9_EstDuration is not null
		AND P9_GS_NKAssignedStaffMember <> ''
		AND P9_FH_ProcessHeader is not null
		AND P9_Type <> 'TRG'
		AND P9_Type <> 'MIL'
		AND P9_Type <> 'EXC')
And P9_Type in (
	Select P9_Type
	From dbo.ProcessTasks
	Where
		P9_Status = 'CLS' 
		AND P9_ActualDuration is not null
		AND P9_EstDuration is not null
		AND P9_GS_NKAssignedStaffMember <> ''
		AND P9_FH_ProcessHeader is not null
		AND P9_Type <> 'TRG'
		AND P9_Type <> 'MIL'
		AND P9_Type <> 'EXC')
And P9_Status in (
	Select P9_Status
	From dbo.ProcessTasks
	Where
		P9_Status = 'CLS' 
		AND P9_ActualDuration is not null
		AND P9_EstDuration is not null
		AND P9_GS_NKAssignedStaffMember <> ''
		AND P9_FH_ProcessHeader is not null
		AND P9_Type <> 'TRG'
		AND P9_Type <> 'MIL'
		AND P9_Type <> 'EXC')";
			#endregion

			#region QueryString_NewIndex
			var query_queryButBetter = @"
P9_FH_ProcessHeader in (
	Select P9_FH_ProcessHeader
	From dbo.ProcessTasks
	Where
		P9_FH_ProcessHeader is not null
		AND P9_Type <> 'TRG'
		AND P9_Type <> 'MIL'
		AND P9_Type <> 'EXC'
		AND P9_Status = 'ASN' 
		AND P9_ActualDuration is not null
		AND P9_EstDuration is not null
		AND P9_EstimateVariationFactor is not null
		AND P9_EstimatedTimeToComplete is not null)
And P9_Type in (
	Select P9_Type
	From dbo.ProcessTasks
	Where
		P9_FH_ProcessHeader is not null
		AND P9_Type <> 'TRG'
		AND P9_Type <> 'MIL'
		AND P9_Type <> 'EXC'
		AND P9_Status = 'ASN' 
		AND P9_ActualDuration is not null
		AND P9_EstDuration is not null
		AND P9_EstimateVariationFactor is not null
		AND P9_EstimatedTimeToComplete is not null)
And P9_Status in (
	Select P9_Status
	From dbo.ProcessTasks
	Where
		P9_FH_ProcessHeader is not null
		AND P9_Type <> 'TRG'
		AND P9_Type <> 'MIL'
		AND P9_Type <> 'EXC'
		AND P9_Status = 'ASN' 
		AND P9_ActualDuration is not null
		AND P9_EstDuration is not null
		AND P9_EstimateVariationFactor is not null
		AND P9_EstimatedTimeToComplete is not null)";
			#endregion

			var queryTheFirst = new ZDBOnlyQuery(typeof(ProcessTask));
			queryTheFirst.AddFilterAndZSQLParameterCollection(query_usesExistingIndex, new ZSqlParameterCollection());

			var queryTheSecond = new ZDBOnlyQuery(typeof(ProcessTask));
			queryTheSecond.AddFilterAndZSQLParameterCollection(query_queryButBetter, new ZSqlParameterCollection());

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var relevantTasks = Factory.Load<ProcessTask>(queryTheFirst);
				var relevantTasks2 = Factory.Load<ProcessTask>(queryTheSecond);

				var queryPlans_IndexAlwaysExpected = TestConnection.ExecutedCommandsAndQueryPlans?.FirstOrDefault(t => t.Item1.Contains("'CLS'"));
				var queryPlans_IndexInDoubt = TestConnection.ExecutedCommandsAndQueryPlans?.FirstOrDefault(t => t.Item1.Contains("'ASN'"));

				var planalyzer_ExpectedIndexUsed = new QueryPlanalyzer(queryPlans_IndexAlwaysExpected.Item2.Last());
				var planalyzer_HopingIndexUsed = new QueryPlanalyzer(queryPlans_IndexInDoubt.Item2.Last());

				AssertCollectionContains(
					"PRE: This query should execute normally, using our modified Index, and yet...",
					"NR_RX__P9_FH_ProcessHeader_P9_Type_P9_Status",
					planalyzer_ExpectedIndexUsed.IndexScans.Select(x => x.IndexName));

				AssertCollectionContains(
					"This new query should execute normally, using our modified Index, and yet...",
					"NR_RX__P9_FH_ProcessHeader_P9_Type_P9_Status",
					planalyzer_HopingIndexUsed.IndexScans.Select(x => x.IndexName));

				QueryPlanalyzer.AssertNoRIDLookups(planalyzer_ExpectedIndexUsed);
				QueryPlanalyzer.AssertNoRIDLookups(planalyzer_HopingIndexUsed);
			}
		}

		public void TestGlbGroupLinkIndex_ShouldUseReleaseGroupColumn_ShouldPreventExcessiveRIDLookups()
		{
			#region QueryString_ExistingIndex
			var query_usesExistingIndex = @"
P9_GS_NKAssignedStaffMember in (
	Select GS_Code
	From dbo.GlbStaff
	Join dbo.GlbGroupLink on GS_PK = GK_GS
	Join dbo.ProcessHeader on P9_GS_NKAssignedStaffMember = GS_Code) -- Hi This Comment is IN THIS QUERY!";
			#endregion

			#region QueryString_NewIndex
			var query_queryButBetter = @"
P9_GS_NKAssignedStaffMember in (
	Select GG_Code
	From dbo.GlbGroup
	Join dbo.GlbGroupLink on GK_GG = GG_PK
	Join dbo.GlbStaff on GS_PK = GK_GS
	Join dbo.ProcessHeader on P9_GS_NKAssignedStaffMember = GS_Code
	Where GK_GS in
	(
		Select GS_PK
			From dbo.GlbStaff
			Join dbo.GlbGroupLink on GS_PK = GK_GS
	)) -- Hi I'm Another Comment IN THAT QUERY!";
			#endregion

			var queryTheFirst = new ZDBOnlyQuery(typeof(ProcessTask));
			queryTheFirst.AddFilterAndZSQLParameterCollection(query_usesExistingIndex, new ZSqlParameterCollection());

			var queryTheSecond = new ZDBOnlyQuery(typeof(ProcessTask));
			queryTheSecond.AddFilterAndZSQLParameterCollection(query_queryButBetter, new ZSqlParameterCollection());

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var relevantTasks = Factory.Load<ProcessTask>(queryTheFirst);
				var relevantTasks2 = Factory.Load<ProcessTask>(queryTheSecond);

				var queryPlans_IndexAlwaysExpected = TestConnection.ExecutedCommandsAndQueryPlans?.FirstOrDefault(t => t.Item1.Contains("-- Hi This Comment is IN THIS QUERY!"));
				var queryPlans_IndexInDoubt = TestConnection.ExecutedCommandsAndQueryPlans?.FirstOrDefault(t => t.Item1.Contains("-- Hi I'm Another Comment IN THAT QUERY!"));

				var planalyzer_ExpectedIndexUsed = new QueryPlanalyzer(queryPlans_IndexAlwaysExpected.Item2.Last());
				var planalyzer_HopingIndexUsed = new QueryPlanalyzer(queryPlans_IndexInDoubt.Item2.Last());

				AssertCollectionContains(
					"PRE: This query should execute normally, using our modified Index, and yet...",
					"FK_RC__GK_GS",
					planalyzer_ExpectedIndexUsed.IndexSeeks.Select(x => x.IndexName));

				AssertCollectionContains(
					"This new query should execute normally, using our modified Index, and yet...",
					"FK_RC__GK_GS",
					planalyzer_HopingIndexUsed.IndexSeeks.Select(x => x.IndexName));

				QueryPlanalyzer.AssertNoRIDLookups(planalyzer_ExpectedIndexUsed);
				QueryPlanalyzer.AssertNoRIDLookups(planalyzer_HopingIndexUsed);
			}
		}

		public void TestTagMagnitudeIndex_ShouldNotUseRIDLookups()
		{
			#region Query SQL
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			query.AddFilterAndZSQLParameterCollection(@"
  FH_WorkflowType = 'INC' 
  AND
  (
   FH_PK in
   (
    SELECT FH_PK
         FROM dbo.ProcessHeader
         JOIN dbo.TagLink jobLink 
          ON jobLink.TGL_ParentID = FH_FH_ParentHeader
    AND
    jobLink.TGL_TGM_Magnitude = 'F2CABBDC-879C-4280-99BD-722BB4F19B03'
         WHERE FH_FH_ParentHeader is not null
         UNION ALL
         SELECT TGL_ParentID FROM dbo.TagLink
         WHERE TGL_TGM_Magnitude = 'F2CABBDC-879C-4280-99BD-722BB4F19B03'
   )
  )
 OR
 (
  (
       FH_PK in
   (
         SELECT FH_PK
         FROM dbo.ProcessHeader
         JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId
         JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK
         WHERE TGM_TGD_Tag = 'A9858600-73AA-4CFC-823B-DE2596822DBB'
         UNION ALL
         SELECT TGL_ParentID
         FROM dbo.TagLink
         JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK
         WHERE TGM_TGD_Tag = 'A9858600-73AA-4CFC-823B-DE2596822DBB'
   )
  )
  AND
  (
   (
       FH_PK in
       (
          SELECT FH_PK
          FROM dbo.ProcessHeader
          JOIN dbo.TagLink jobLink 
           ON jobLink.TGL_ParentID = FH_FH_ParentHeader
     AND
     jobLink.TGL_TGM_Magnitude = 'A80D9D43-A15B-40A1-8D4C-624FD065DF86'
          JOIN dbo.TagMagnitude mag ON mag.TGM_PK = jobLink.TGL_TGM_Magnitude
          WHERE 1=1
     AND
     FH_FH_ParentHeader is not null
     AND
     NOT EXISTS
     (
             SELECT null
             FROM dbo.TagMagnitude workflowMag
             JOIN dbo.TagLink workflowLink on workflowLink.TGL_TGM_Magnitude = workflowMag.TGM_PK
             WHERE 1=1
      AND
      workflowLink.TGL_ParentID = FH_PK
      AND
      workflowMag.TGM_TGD_Tag = mag.TGM_TGD_Tag
      AND
      workflowMag.TGM_PK <> mag.TGM_PK
     )
          UNION ALL
          SELECT TGL_ParentID FROM dbo.TagLink
          WHERE TGL_TGM_Magnitude = 'A80D9D43-A15B-40A1-8D4C-624FD065DF86a'
    )
   )
   AND
   (
        FH_PK in
    (
          SELECT FH_PK
          FROM dbo.ProcessHeader
          JOIN dbo.TagLink jobLink 
          ON jobLink.TGL_ParentID = FH_FH_ParentHeader
     AND
     jobLink.TGL_TGM_Magnitude = '2EF666CC-0583-4290-B08A-D849EB5B16E0'
          JOIN dbo.TagMagnitude mag ON mag.TGM_PK = jobLink.TGL_TGM_Magnitude
          WHERE 1=1
     AND
     FH_FH_ParentHeader is not null
     AND
     NOT EXISTS
     (
             SELECT null
             FROM dbo.TagMagnitude workflowMag
             JOIN dbo.TagLink workflowLink on workflowLink.TGL_TGM_Magnitude = workflowMag.TGM_PK
             WHERE 1=1
      AND
      workflowLink.TGL_ParentID = FH_PK
      AND
      workflowMag.TGM_TGD_Tag = mag.TGM_TGD_Tag
      AND
      workflowMag.TGM_PK <> mag.TGM_PK
     )
          UNION ALL
          SELECT TGL_ParentID FROM dbo.TagLink
          WHERE TGL_TGM_Magnitude = '2EF666CC-0583-4290-B08A-D849EB5B16E0'
    )
   )
  )
 )
 OR
 (
  (
       FH_PK in
   (
         SELECT FH_PK
         FROM dbo.ProcessHeader
         JOIN dbo.TagLink jobLink 
          ON jobLink.TGL_ParentID = FH_FH_ParentHeader
    AND
    jobLink.TGL_TGM_Magnitude = '535B7FCE-62E8-48C0-A6FA-F9FE0CCBAE28'
         WHERE FH_FH_ParentHeader is not null
         UNION ALL
         SELECT TGL_ParentID FROM dbo.TagLink
         WHERE TGL_TGM_Magnitude = '535B7FCE-62E8-48C0-A6FA-F9FE0CCBAE28'
   )
  )
  AND
  (
       FH_PK in
   (
         SELECT FH_PK
         FROM dbo.ProcessHeader
         JOIN dbo.TagLink jobLink 
          ON jobLink.TGL_ParentID = FH_FH_ParentHeader
    AND
    jobLink.TGL_TGM_Magnitude = '535B7FCE-62E8-48C0-A6FA-F9FE0CCBAE283'
         WHERE FH_FH_ParentHeader is not null
         UNION ALL
         SELECT TGL_ParentID FROM dbo.TagLink
         WHERE TGL_TGM_Magnitude = '535B7FCE-62E8-48C0-A6FA-F9FE0CCBAE28'
   )
  )
 )
 OR
 (
  (
       FH_PK in
   (
         SELECT FH_PK
         FROM dbo.ProcessHeader
         JOIN dbo.TagLink jobLink 
          ON jobLink.TGL_ParentID = FH_FH_ParentHeader
    AND
    jobLink.TGL_TGM_Magnitude = 'F2CABBDC-879C-4280-99BD-722BB4F19B03'
         WHERE FH_FH_ParentHeader is not null
         UNION ALL
         SELECT TGL_ParentID FROM dbo.TagLink
         WHERE TGL_TGM_Magnitude = 'F2CABBDC-879C-4280-99BD-722BB4F19B03'
   )
  )
  AND
  (
       FH_PK in
   (
         SELECT FH_PK
         FROM dbo.ProcessHeader
         JOIN dbo.TagLink jobLink 
          ON jobLink.TGL_ParentID = FH_FH_ParentHeader
    AND
    jobLink.TGL_TGM_Magnitude = 'F2CABBDC-879C-4280-99BD-722BB4F19B03'
         WHERE FH_FH_ParentHeader is not null
         UNION ALL
         SELECT TGL_ParentID FROM dbo.TagLink
         WHERE TGL_TGM_Magnitude = 'F2CABBDC-879C-4280-99BD-722BB4F19B03'
   )
  )
 )

AND
(
 FH_PK IN 
 (
  SELECT FH_PK FROM dbo.ProcessHeader WHERE FH_PK IN 
  (
   SELECT FH_FH_ParentHeader FROM dbo.ProcessHeader WHERE FH_FH_ParentHeader IS NOT NULL 
   AND
   FH_FC_CurrentComponent = '925A735B-18E1-4A0A-900E-6C1E6489BA23'
  )
   UNION ALL SELECT FH_PK FROM dbo.ProcessHeader WHERE FH_FC_CurrentComponent = '925A735B-18E1-4A0A-900E-6C1E6489BA23'
 )
 AND
 FH_P0_Template is NULL 
 AND
 FH_IsActive = 1
) -- this is a query", new ZSqlParameterCollection());
			#endregion

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var newFactory = Factory.CreateNewFactory();
				newFactory.Load<ProcessHeader>(query);

				var queryPlans = TestConnection.ExecutedCommandsAndQueryPlans?.FirstOrDefault(t => t.Item1.Contains("-- this is a query"));
				var planalyser = new QueryPlanalyzer(queryPlans?.Item2.Last());

				CombineAssertions(() =>
				{
					var expectedIndexes = new[] { "PK_UX__TGM_PK", "FK_UC__TGM_TGD_Tag_TGM_Code" };
					Assert(
						"Index seeks on PK_UX__TGM_PK or FK_UC__TGM_TGD_Tag_TGM_Code",
						planalyser
							.IndexSeeks
							.Select(indexSeek => indexSeek.IndexName)
							.Any(indexName => expectedIndexes.Contains(indexName)));

					AssertContainsExactElementsInAnyOrder(
						"We expected no RID lookups, and yet...",
						Array.Empty<Tuple<string, string>>(),
						planalyser.RowIDLookups.SelectMany(x => x.OutputList).Select(c => Tuple.Create(c.TableName, c.ColumnName)));
				});
			}
		}
	}
}
