using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.WorkItem;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WTG.DevTools.Configuration;
using WTG.DevTools.ServiceClient.Assess;
using WorkItemProcessTask = Enterprise.Client.EDI.IncidentManager.Business.WorkItemProcessTask;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/WorkItem")]
	public class WorkItemController : BusinessObjectController
	{
		readonly IAssessServiceClient assessServiceClient;

		public WorkItemController()
			: this(null)
		{
		}

		public WorkItemController(IAssessServiceClient assessServiceClient)
		{
			this.assessServiceClient = assessServiceClient ?? ObjectFactory.Get<IAssessServiceClient>();
		}

		[HttpPost]
		[Route("GetStatuses")]
		public HttpResponseMessage GetStatuses([FromBody] WorkItemStatusRequestData requestData)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = new BusinessObjectFactory() { RefreshEnabled = false };
				var workItems = factory.Load<WorkItem>(new ZQuery(WorkItemSchema.WKI_WorkItemNumber, requestData.WorkItemNumbers));
				var result = new WorkItemStatusResponseData
				{
					Statuses = workItems.Select(x => new WorkItemStatus { WorkItemPk = x.PK.ToGuid(), WorkItemNumber = x.WKI_WorkItemNumber, Status = x.WKI_Status })
				};
				return Request.CreateResponse(result);
			}
		}

		[HttpPost]
		[Route("CancelProcessTask")]
		public HttpResponseMessage CancelProcessTask([FromBody] JObject jsonObject)
		{
			using (Db.DisposableActionForDbConnection())
			{
				return UpdateBusinessObject<WorkItemProcessTask>(jsonObject, "CancelProcessTask", (processTask) =>
				{
					using (processTask.BeginShelfUpdateSuspender())
					{
						Type notesType = typeof(byte[]);
						var notes = Convert.ChangeType(jsonObject["Notes"], notesType, CultureInfo.InvariantCulture);

						processTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
						processTask.P9_Notes = (byte[])notes;
						Factory.Save();
					}
				});
			}
		}

		[HttpPost]
		[Route("UpdateProcessTaskWithNewShelfInformation")]
		public HttpResponseMessage UpdateProcessTaskWithNewShelfInformation([FromBody] JObject jsonObject)
		{
			using (Db.DisposableActionForDbConnection())
			{
				return UpdateBusinessObject<WorkItemProcessTask>(jsonObject, "UpdateProcessTaskWithNewShelfInformation", processTask =>
				{
					using (processTask.BeginShelfUpdateSuspender())
					{
						processTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
						processTask.P9_Description = jsonObject["ShelfName"].ToString();
						processTask.P9_GS_NKAssignedStaffMember = jsonObject["StaffCode"].ToString();
						processTask.P9_Type = jsonObject["CheckInTaskType"].ToString();
						var notes = jsonObject["Notes"]?.ToString();
						if (!string.IsNullOrEmpty(notes))
						{
							processTask.P9_NotesAsString = notes;
						}
						Factory.Save();
					}
				});
			}
		}

		[HttpPost]
		[Route("AssignProcessTaskBackToUser")]
		public HttpResponseMessage AssignProcessTaskBackToUser([FromBody] JObject jsonObject)
		{
			using (Db.DisposableActionForDbConnection())
			{
				return UpdateBusinessObject<WorkItemProcessTask>(jsonObject, "AssignProcessTaskBackToUser", processTask =>
				{
					using (processTask.BeginShelfUpdateSuspender())
					{
						var iterationType = jsonObject["IterationType"]?.ToString();
						var iterationReasonCode = jsonObject["ReasonCode"]?.ToString();
						var staffCode = jsonObject["StaffCode"].ToString();

						if (iterationType == null || iterationReasonCode == null)
						{
							SuspendTaskAndReassignStaffMember(processTask, staffCode);
						}
						else
						{
							var qualityIterationHelper = new QualityIterationHelper(new QualityIterationInfo(processTask, iterationType, iterationReasonCode, staffCode), Factory, null); // No logging here

							IEnumerable<QualityIterationTaskDescriptor> CustomQualityIterationTasks()
							{
								yield return new QualityIterationTaskDescriptor
								{
									Type = "COD",
									Description = iterationType,
									AssignedStaffCode = staffCode,
									EstDuration = new ZInt(LowEstimatedDurationForManualPatchTasksInMinutes).GetDateTimeFromMinutes(),
								};

								yield return new QualityIterationTaskDescriptor
								{
									Type = WorkItemProcessTask.CodeReviewTaskType,
									Description = "Code Review",
									AssignedStaffCode = ZString.Empty,
									EstDuration = new ZInt(LowEstimatedDurationForManualPatchCodeReviewTasksInMinutes).GetDateTimeFromMinutes(),
								};
							}

							void AdjustTasksAfterCopyTasksDelegate(IProcessHeader iterationWorkflow)
							{
								var lastTask = (ProcessTask)((IWorkflowProvider)iterationWorkflow.Parent).WorkflowItems.Tasks.Last();
								lastTask.P9_GS_NKAssignedStaffMember = staffCode;

								var codeReviewTasks = lastTask.Parent.WorkflowItems.Tasks
									.Where(x => x.P9_Type == WorkItemProcessTask.CodeReviewTaskType)
									.OrderBy(x => x.P9_ParentTemplateID.IsEmpty)
									.ThenBy(x => x.P9_Sequence)
									.ToList();

								var existingCodeReviewTask = codeReviewTasks.FirstOrDefault();
								var codeReviewTask = codeReviewTasks.Last();

								codeReviewTask.P9_G4_RequiredCapability = existingCodeReviewTask?.P9_G4_RequiredCapability ?? ZGuid.Empty;

								if (codeReviewTask.P9_G4_RequiredCapability.IsEmpty && existingCodeReviewTask != null)
								{
									// If the code review task doesn't have a capability, better to assign it to the person who did the original review than leave it OPN and showing up in no one's channel.
									codeReviewTask.P9_GS_NKAssignedStaffMember = existingCodeReviewTask.P9_GS_NKAssignedStaffMember;
								}
							}

							if (!qualityIterationHelper.CreateQualityIteration(true, AdjustTasksAfterCopyTasksDelegate, CustomQualityIterationTasks()))
							{
								SuspendTaskAndReassignStaffMember(processTask, staffCode);
							}
						}

						Factory.Save();
					}
				});
			}
		}

		const int LowEstimatedDurationForManualPatchTasksInMinutes = 20;
		const int LowEstimatedDurationForManualPatchCodeReviewTasksInMinutes = 10;

		[HttpPost]
		[Route("EnsureRequiredReviewsExist")]
		public HttpResponseMessage EnsureRequiredReviewsExist([FromBody] JObject jsonObject)
		{
			if (Request == null)
			{
				return new HttpResponseMessage(HttpStatusCode.Forbidden);
			}

			if (jsonObject["ShelfTaskPK"] == null || jsonObject["ReviewTaskCapability"] == null || jsonObject["ReviewName"] == null || jsonObject["TaskNotes"] == null)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Request, Required JSON keys are ShelfTaskPK, ReviewTaskCapability, ReviewName and TaskNotes");
			}

			if (!ZGuid.TryParse(jsonObject["ShelfTaskPK"].ToString(), out var shelfTaskPK))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to parse GUID for ShelfTaskPK: " + jsonObject["ShelfTaskPK"]);
			}
			using (Db.DisposableActionForDbConnection())
			{
				var shelfTask = Factory.Load<WorkItemProcessTask>(shelfTaskPK);
				if (shelfTask == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Shelf Task not found with PK: " + shelfTaskPK);
				}

				if (shelfTask.IsClosedOrCancelled)
				{
					return Request.CreateResponse(HttpStatusCode.OK);
				}

				var taskNotes = jsonObject["TaskNotes"].ToString();
				var reviewName = jsonObject["ReviewName"].ToString();
				var capabilityCode = jsonObject["ReviewTaskCapability"].ToString();

				if (!shelfTask.EnsureRequiredAspectReviewExists(capabilityCode, reviewName, taskNotes))
				{
					return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "All tasks appear to be closed in work item: " + shelfTask.Parent.WKI_WorkItemNumber);
				}

				try
				{
					Factory.Save();
				}
				catch (ZSaveException ex)
				{
					return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error encountered when saving", ex);
				}

				return Request.CreateResponse(HttpStatusCode.OK);
			}
		}

		[HttpPost]
		[Route("DoesReviewExistWithCapability")]
		public HttpResponseMessage DoesReviewExistWithCapability([FromBody] JObject jsonObject)
		{
			if (Request == null)
			{
				return new HttpResponseMessage(HttpStatusCode.Forbidden);
			}

			if (jsonObject["ShelfTaskPK"] == null || jsonObject["ReviewTaskCapability"] == null)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Request, Required JSON keys are ShelfTaskPK and ReviewTaskCapability");
			}

			if (!ZGuid.TryParse(jsonObject["ShelfTaskPK"].ToString(), out var shelfTaskPK))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to parse GUID for ShelfTaskPK: " + jsonObject["ShelfTaskPK"]);
			}

			using (Db.DisposableActionForDbConnection())
			{
				var capability = Factory.Load<GlbCapability>(new ZQuery(GlbCapabilitySchema.G4_Code, jsonObject["ReviewTaskCapability"].ToString())).FirstOrDefault();

				if (capability == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to find Capability, ReviewTaskCapability: " + jsonObject["ReviewTaskCapability"]);
				}

				var shelfTask = Factory.Load<WorkItemProcessTask>(shelfTaskPK);
				if (shelfTask == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Shelf Task not found with PK: " + shelfTaskPK);
				}

				return Request.CreateResponse(HttpStatusCode.OK, DoesReviewExistWithCapability(capability, shelfTask));
			}
		}

		[HttpPost]
		[Route("EnsureAppropriateReviewExists")]
		public HttpResponseMessage EnsureAppropriateReviewExists([FromBody] JObject jsonObject)
		{
			if (Request == null)
			{
				return new HttpResponseMessage(HttpStatusCode.Forbidden);
			}

			EnsureAppropriateReviewExistsRequest request;
			try
			{
				request = JsonConvert.DeserializeObject<EnsureAppropriateReviewExistsRequest>(jsonObject.ToString(Formatting.None));
				request.AspectPKs ??= [];
			}
			catch (JsonSerializationException ex)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to parse JSON request: " + ex.Message);
			}

			using (Db.DisposableActionForDbConnection())
			{
				var submissionTask = Factory.Load<WorkItemProcessTask>(request.TaskPK);
				if (submissionTask == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Shelf Task not found with PK " + request.TaskPK);
				}

				EnsureAppropriateReviewExistsResult result;
				try
				{
					result = EnsureAppropriateReviewExists(request, submissionTask);
				}
				catch (ZSaveException ex)
				{
					return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error encountered when saving", ex);
				}

				return Request.CreateResponse(HttpStatusCode.OK, result);
			}
		}

		EnsureAppropriateReviewExistsResult EnsureAppropriateReviewExists(EnsureAppropriateReviewExistsRequest request, WorkItemProcessTask submissionTask)
		{
			if (request.AspectPKs.Count == 0)
			{
				return EnsureAppropriateReviewExistsResult.AppropriateReviewDoesExist;
			}

			var assessRequirements = WorkItemTaskHelper.GetAssessRequirementsCompletionState(submissionTask, request, assessServiceClient);
			if (assessRequirements.UnmetRequirements.IsEmpty)
			{
				return EnsureAppropriateReviewExistsResult.AppropriateReviewDoesExist; 
			}

			submissionTask.EnsureAssessReviewExists(assessRequirements, assessServiceClient);
			Factory.Save();

			var wtaCourseDetails = (
				from requirement in assessRequirements.UnmetRequirements.CompetencyRequirements
				let urlResult = assessServiceClient.GetLearningUnitUrlAsync(requirement.AspectPK).GetAwaiter().GetResult()
				where urlResult.IsSuccess
				let detail = new { text = WorkItemTaskHelper.GetHumanReadableText(requirement, assessServiceClient), url = urlResult.Content }
				orderby detail.text
				select detail).ToArray();

			return new EnsureAppropriateReviewExistsResult
			{
				AppropriateReviewExists = false,
				MissingCompetencies = Array.Empty<string>(),
				MissingWiseTechAcademySubjects = wtaCourseDetails.Select(d => d.text).ToArray(),
				MissingWiseTechAcademyCourseUrls = wtaCourseDetails.Select(d => d.url).ToArray(),
			};
		}

		bool DoesReviewExistWithCapability(GlbCapability capability, WorkItemProcessTask submissionTask)
		{
			var workflows = WorkItemTaskHelper.GetWorkflowsWithinJobThatMayContainReviewTask(submissionTask).ToArray();
			var reviewTasks = (
				from workflow in workflows
				from ProcessTask task in workflow.Tasks
				where task.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled
				where task.P9_Type == WorkItemProcessTask.CodeReviewTaskType
				select task
				).ToArray();

			return reviewTasks.Any(t => t.P9_G4_RequiredCapability == capability.PK)
				|| reviewTasks.Any(t => t.AssignedStaffMember != null && t.AssignedStaffMember.Capabilities.Contains(capability));
		}

		void SuspendTaskAndReassignStaffMember(WorkItemProcessTask task, string staff)
		{
			task.P9_GS_NKAssignedStaffMember = staff;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
		}

		[HttpGet]
		[Route("Hyperlink")]
		public string GetWorkItemHyperlink(string workItemNumber)
		{
			var workItem = Factory.LoadTop1<WorkItem>(new ZQuery(WorkItemSchema.WKI_WorkItemNumber, workItemNumber));

			if (workItem != null)
			{
				return ShowEditFormUrlHandler.Instance.Create(ControllerIDs.WorkItem, workItem.PK);
			}

			return null;
		}

		[HttpPost]
		[Route("ProductDetails")]
		public IEnumerable<ProductDetails> GetProductDetails([FromBody] Guid[] workItemPKs)
		{
			if (workItemPKs?.Length > 0)
			{
				var query = new ZQuery(WorkItemSchema.PK, workItemPKs);
				var workItems = Factory.Load<WorkItem>(query);

				return workItems.Select(wi => new ProductDetails(wi.WKI_WorkItemType, wi.WKI_WorkItemArea, wi.WKI_ActivityType)).ToArray();
			}

			return Array.Empty<ProductDetails>();
		}
	}
}
