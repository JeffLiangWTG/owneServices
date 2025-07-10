using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using AttachmentTypeList = Enterprise.BufferManagement.NetworkVisualisation.Business.AttachmentTypeList;

[assembly: HostedService(
	BufferPenetrationUpdaterServiceTask.Code,
	BufferPenetrationUpdaterServiceTask.Description,
	BMSServiceTaskBase.Category,
	typeof(BufferPenetrationUpdaterServiceTask),
	MinimumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour",
	DefaultScheduleStartAtUtc = "4hours",
	ActiveByDefault = true
	)]

namespace Enterprise.BufferManagement.NetworkVisualisation.ServiceTasks
{
	public class BufferPenetrationUpdaterServiceTask : BMSServiceTaskBase
	{
		public const string Code = BMConstants.BufferPenetrationUpdaterServiceTaskCode;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Buffer Penetration Updater";

		protected override string TaskDescription
		{
			get { return Description; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "I can if I want to though.")]

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task logging")]
		protected override void RunTaskCore(CancellationToken token)
		{
			var builder = new SqlBuilder()
				.Append("select BNS_PK from dbo.BMNCNShape where ");

			GetQuery().AddFilterString(builder);

			var pks = new List<ZGuid>();
			using (var cmd = Db.Connection.Command(builder.ToString()))
			{
				foreach (var param in builder.Parameters)
				{
					cmd.AddParameter(param.ParameterName, param.SchemaColumn.SqlDbType, param.ValueForSql);
				}

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						pks.Add(reader.GetGuid(0));
					}
				}
			}

			var batches = 0L;
			var recordsLoaded = pks.Count;
			var recordsUpdated = 0L;

			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				foreach (var superBatch in pks.Batch(500))
				{
					batches++;
					token.ThrowIfCancellationRequested();
					var superFactory = new BusinessObjectFactory { NameForDebugging = "BMB Super Factory", RefreshEnabled = false };
					var shapes = superFactory.Load<BMNCNShape>(new ZQuery(BMNCNShapeSchema.PK, superBatch));

					foreach (var group in shapes.GroupBy(s => WorkingTimeContext.CreateForServiceTask(s)))
					{
						if (group.Key == null)
						{
							ServiceLogger.Log(LogType.Information, FormattableString.Invariant($"Missing valid user context following shapes: {string.Join(", ", group)}"));
						}
						else
						{
							ZExceptionReporting.ProcessWithSaveExceptionHandling(() =>
							{
								var factory = new BusinessObjectFactory { NameForDebugging = "BMB Factory", RefreshEnabled = Globals.IsTest };
								var shapesInIsolatedFactory = factory.Load<BMNCNShape>(new ZQuery(BMNCNShapeSchema.PK, group.Select(g => g.PK).ToArray()));
								using (Env.SetTemporaryUserContext(Env.CurrentUserPK, group.Key.Branch.PK.ToGuid(), group.Key.Department.PK.ToGuid()))
								{
									foreach (var shape in shapesInIsolatedFactory)
									{
										if (shape.UpdateBufferPenetration(group.Key))
										{
											recordsUpdated++;
										}
									}
									factory.Save();
								}
							}, () => { });
						}
					}
				}
				var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Processed {0} batches, {1} records in total, {2} records updated", batches, recordsLoaded, recordsUpdated);
				ServiceLogger.Log(LogType.Information, message);
			}
		}

		static ZDBOnlyQuery GetQuery()
		{
			var query = new ZDBOnlyQuery(typeof(BMNCNShape));
			query.IncludeBlob(BMNCNShapeSchema.BNS_LayoutData);

			var workflowQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), BMNCNShapeSchema.BNS_RelatedEntityID);
			workflowQuery.AddToFilter(ProcessHeaderSchema.FH_Status, ProcessHeader.GetOpenStatuses());

			var attachmentQuery = new ZDBOnlySubQuery(typeof(BMNCNAttachment), BMNCNAttachmentSchema.BNA_BNS_ToShape);
			attachmentQuery.AddToFilter(BMNCNAttachmentSchema.BNA_Type, AttachmentTypeList.Codes.RelatedBuffer);

			var bufferQuery = new ZDBOnlySubQuery(typeof(BMNCNShape), BMNCNShapeSchema.PK);
			bufferQuery.AddToFilter(BMNCNShapeSchema.BNS_ShapeType, ShapeTypeList.Codes.Buffer);

			attachmentQuery.AddSubQuery(BMNCNAttachmentSchema.BNA_BNS_Owner, bufferQuery, JoinCondition.And);
			query.AddSubQuery(BMNCNShapeSchema.PK, attachmentQuery, JoinCondition.And);
			query.AddSubQuery(workflowQuery, JoinCondition.And);

			query.AddAsUnionQuery(GetBufferQuery(), addAsUnionAll: true);

			return query;
		}

		static ZDBOnlySubQuery GetBufferQuery()
		{
			var bufferShapeQuery = new ZDBOnlySubQuery(typeof(BMNCNShape), BMNCNShapeSchema.PK);
			bufferShapeQuery.AddToFilter(BMNCNShapeSchema.BNS_ShapeType, ShapeTypeList.Codes.Buffer);

			var relatedBufferAttachmentQuery = new ZDBOnlySubQuery(typeof(BMNCNAttachment), BMNCNAttachmentSchema.BNA_BNS_Owner);
			relatedBufferAttachmentQuery.AddToFilter(BMNCNAttachmentSchema.BNA_Type, AttachmentTypeList.Codes.RelatedBuffer);

			var shapeQuery = new ZDBOnlySubQuery(typeof(BMNCNShape), BMNCNShapeSchema.PK);
			var processHeaderQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			processHeaderQuery.AddToFilter(ProcessHeaderSchema.FH_Status, ProcessHeader.GetOpenStatuses());

			shapeQuery.AddSubQuery(BMNCNShapeSchema.BNS_RelatedEntityID, processHeaderQuery, JoinCondition.And);
			relatedBufferAttachmentQuery.AddSubQuery(BMNCNAttachmentSchema.BNA_BNS_ToShape, shapeQuery, JoinCondition.And);
			bufferShapeQuery.AddSubQuery(relatedBufferAttachmentQuery, JoinCondition.And);

			return bufferShapeQuery;
		}

		[HostedServiceRequirement]
		public static string CheckSufficientWorkflowModeEnabled()
		{
			return CheckPlanningManagementEnabled();
		}
	}
}
