using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Integration.Management;

namespace Enterprise.BufferManagement.DataTransfer
{
	public class UniversalTaskSetWriter : IUniversalTaskSetWriter
	{
		public void PopulateTaskSets(IDataObjectWriterStrategy writerStrategy, BusinessObject source, IDataObject destination)
		{
			if (source is IWorkflowProvider provider && destination is ITaskSetCollectionParent dataObject)
			{
				var factory = source.Factory;

				if (ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled && ProcessJobHeaderProvider.SupportsPAVE(provider.WorkflowType, factory))
				{
					PopulateTaskSetsForBufferManagement(writerStrategy, provider, dataObject, factory);
				}
				else
				{
					PopulateTaskSetsForNonBufferManagement(writerStrategy, provider, dataObject);
				}
			}
		}

		void PopulateTaskSetsForBufferManagement(IDataObjectWriterStrategy writerStrategy, IWorkflowProvider provider, ITaskSetCollectionParent dataObject, BusinessObjectFactory factory)
		{
			dataObject.SetTaskSetCollection(() =>
			{
				var jobHeader = (ProcessJobHeader)ProcessJobHeaderProvider.GetForParent(provider, factory, addDefaultProcessHeaderIfNone: false);
				var jobTaskSetData = CreateTaskSetFromProcessHeader(writerStrategy, jobHeader);

				return new List<TaskSet> { jobTaskSetData };
			});
		}

		void PopulateTaskSetsForNonBufferManagement(IDataObjectWriterStrategy writerStrategy, IWorkflowProvider provider, ITaskSetCollectionParent dataObject)
		{
			var tasks = provider.WorkflowItems.Tasks.Cast<ProcessTask>().ToArray();

			if (tasks.Any())
			{
				dataObject.SetTaskSetCollection(() =>
				{
					var jobHeader = new TaskSet(writerStrategy)
					{
						Description = BMGlobalConstants.DefaultJobWorkflowCompletionStatement,
						Type = GetTaskSetType(ProcessHeaderTypeList.Codes.Job),
					};

					jobHeader.SetTaskSetCollection(() =>
					{
						var taskSet = new TaskSet(writerStrategy)
						{
							Description = BMGlobalConstants.DefaultWorkflowCompletionStatement,
							Type = GetTaskSetType(ProcessHeaderTypeList.Codes.Workflow),
						};

						PopulateTasks(tasks, taskSet);

						return new List<TaskSet> { taskSet };
					});

					return new List<TaskSet> { jobHeader };
				});
			}
		}

		static void PopulateTasks(ProcessTask[] tasks, TaskSet taskSet)
		{
			if (tasks.Any())
			{
				var taskWriter = ObjectFactory.Get<IUniversalTaskWriter>();
				taskWriter.PopulateTasks(tasks, taskSet);
			}
		}

		TaskSet CreateTaskSetFromProcessHeader(IDataObjectWriterStrategy writerStrategy, ProcessHeader header)
		{
			var taskSet = new TaskSet(writerStrategy)
			{
				Description = header.FH_CompletionStatement,
				Type = GetTaskSetType(header),
				CurrentStatus = header.CurrentStatus,
				IsActive = header.FH_IsActive,
				AgreedDeliveryDateUTC = new ZDateTimeOffset(header.FH_AgreedDeliveryDate, TimeSpan.Zero),
				EarliestStartDateUTC = new ZDateTimeOffset(header.FH_DoNotStartBeforeDate, TimeSpan.Zero),
				DateAcceptability = new CodeDescriptionPair { Code = header.ApplicableDateAcceptability, Description = header.Lookups.DateAcceptabilities.GetDescriptionFromCode(header.ApplicableDateAcceptability) },
				TaskSetStatus = new CodeDescriptionPair { Code = header.FH_Status, Description = header.FH_StatusDescription },
			};

			var group = header.ReleaseGroup;
			var notes = header.WorkflowNote;

			if (group != null)
			{
				taskSet.ReleaseGroup = Group.New(header.ReleaseGroup);
			}

			if (notes != null)
			{
				taskSet.Notes = header.WorkflowNote.ST_NoteDataAsText;
			}

			if (!(header is ProcessJobHeader))
			{
				var tasks = header.Tasks.ToArray();
				PopulateTasks(tasks, taskSet);
			}

			var childWorkflows = header.ChildHeaders.Where(x => x.FH_ParentId == header.FH_ParentId).Select(childHeader => CreateTaskSetFromProcessHeader(writerStrategy, childHeader)).ToList();

			if (childWorkflows.Any())
			{
				taskSet.SetTaskSetCollection(() => childWorkflows);
			}

			return taskSet;
		}

		CodeDescriptionPair GetTaskSetType(IProcessHeader header)
		{
			var code = header.IsWorkflow ? ProcessHeaderTypeList.Codes.Workflow : ProcessHeaderTypeList.Codes.Job;

			return GetTaskSetType(code);
		}

		CodeDescriptionPair GetTaskSetType(string code)
		{
			return new CodeDescriptionPair { Code = code, Description = typeList.GetDescriptionFromCode(code) };
		}

		readonly ProcessHeaderTypeList typeList = new ProcessHeaderTypeList();
	}
}
