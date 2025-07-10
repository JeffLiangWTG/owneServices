using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	class ProcessHeaderCollectionProvider : IProcessHeaderCollectionProvider
	{
		#region IProcessHeaderCollectionProvider Members

		IProcessHeaderCollection IProcessHeaderCollectionProvider.GetForTemplate(IProcessTaskTemplate template)
		{
			return GetForTemplate(((ProcessTaskTemplate)template));
		}

		IProcessHeaderLinkCollection IProcessHeaderCollectionProvider.GetLinkCollection(IProcessTaskTemplate template)
		{
			return new ProcessHeaderLinkCollection((ProcessTaskTemplate)template);
		}

		IProcessHeaderCollection IProcessHeaderCollectionProvider.GetForTask(IProcessTask processTask)
		{
			var concreteTask = (ProcessTask)processTask;
			var templateTask = concreteTask as TemplateProcessTask;

			if (templateTask != null)
			{
				return GetForTemplate(templateTask.Parent, new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, SQLComparisonOperator.NotEqual, null));
			}
			else if (!concreteTask.IsDeleted && concreteTask.Parent != null && !(concreteTask.Parent is ProcessTask))
			{
				var jobHeader = ProcessJobHeaderProvider.GetForParent(concreteTask.Parent, concreteTask.Factory);
				if (jobHeader == null)
				{
					return new ProcessHeaderCollection(concreteTask.Factory, ZQuery.NoResultQuery);
				}
				else
				{
					return jobHeader.ProcessHeaders;
				}
			}

			return new ProcessHeaderCollection(concreteTask.Factory, ZQuery.NoResultQuery);
		}

		IBusinessObjectCollection IProcessHeaderCollectionProvider.GetCollectionWithAdhocRelationship(BusinessObjectFactory factory)
		{
			return new ActiveBusinessObjectCollection<ProcessHeader>(factory, new AdhocCollectionRelationship(typeof(ProcessHeader)));
		}

		IBusinessObjectCollection IProcessHeaderCollectionProvider.GetCollection(BusinessObjectFactory factory)
		{
			return new ActiveBusinessObjectCollection<ProcessHeader>(factory);
		}

		void IProcessHeaderCollectionProvider.EnsureJobHeaderPresentWhenRequired(IProcessTaskTemplate template)
		{
			EnsureJobHeaderPresent((ProcessTaskTemplate)template);
		}

		#endregion

		#region Implementation

		static IProcessHeaderCollection GetForTemplate(ProcessTaskTemplate template, ZQuery additionalFilter = null)
		{
			return new ProcessHeaderCollection(template, additionalFilter);
		}

		static void EnsureJobHeaderPresent(ProcessTaskTemplate template)
		{
			if (template.ProcessHeaders.Count <= 1)
			{
				var system = BMSystem.GetForTemplate(template);

				if (template.ProcessHeaders.Count == 0 && system != null)
				{
					var jobHeader = template.Factory.New<ProcessJobHeader>();
					jobHeader.FH_CompletionStatement = BMGlobalConstants.DefaultJobWorkflowCompletionStatement;
					jobHeader.FH_P0_Template = template.PK;
					jobHeader.FH_WorkflowType = ProcessTaskTemplateSchema.Constants.Prefix;

					template.ProcessHeaders.Add(jobHeader);
				}
				else if (template.ProcessHeaders.Count == 1 && system == null)
				{
					var jobHeader = template.GetJobHeader();

					if (jobHeader != null && !jobHeader.IsInDatabase)
					{
						jobHeader.Delete();
					}
				}
			}
		}

		#endregion
	}
}
