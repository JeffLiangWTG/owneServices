using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderProcessTaskCollectionView : ProcessTaskCollectionView, IUseParentGridContext
	{
		public ProcessHeaderProcessTaskCollectionView(ProcessHeader workflow, bool allowCompletionStatements = false)
			: base(workflow.Parent.WorkflowItems)
		{
			this.processHeader = workflow;
			this.allowCompletionStatements = allowCompletionStatements;

			Rebuild();
		}

		readonly ProcessHeader processHeader;
		readonly bool allowCompletionStatements;

		public ProcessHeader ProcessHeader
		{
			get { return processHeader; }
		}

		protected override void AddNewTaskCore(ProcessTask task)
		{
			base.AddNewTaskCore(task);

			if (!(processHeader is ProcessJobHeader))
			{
				using (task.GetValidationSuspender())
				{
					task.P9_FH_ProcessHeader = processHeader.PK;
				}
			}
		}

		protected override void SetDefaultProcessHeader(ProcessTask task)
		{
			if (!(processHeader is ProcessJobHeader))
			{
				task.P9_FH_ProcessHeader = processHeader.PK;
			}
			else
			{
				base.SetDefaultProcessHeader(task);
			}
		}

		protected sealed override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return base.IsThisPartOfTheCollection(element)
				&& BelongsToHeader(element);
		}

		bool BelongsToHeader(BusinessObject element)
		{
			if (processHeader == null)
			{
				return false;
			}

			var task = (ProcessTask)element;
			return BelongsToParent(task) && IsTaskPartOfCollection(task);
		}

		protected virtual bool IsTaskPartOfCollection(ProcessTask task)
		{
			return allowCompletionStatements || !task.IsCompletionStatement;
		}

		protected virtual bool BelongsToParent(ProcessTask task)
		{
			var belongsToParent = task.P9_FH_ProcessHeader == processHeader.PK;
			var processJobHeader = processHeader as ProcessJobHeader;

			if (processJobHeader != null && !processJobHeader.IsDeleted)
			{
				belongsToParent = processJobHeader.IsInSameJob(task);
			}

			return belongsToParent;
		}

		public override void Add(BusinessObject businessObject)
		{
			if (!(processHeader is ProcessJobHeader))
			{
				var task = businessObject as ProcessTask;
				if (task != null && task.P9_FH_ProcessHeader.IsEmpty)
				{
					task.P9_FH_ProcessHeader = processHeader.PK;
				}
			}
			base.Add(businessObject);
		}

		#region IUseParentGridContext Members

		Type IUseParentGridContext.ParentType
		{
			get { return BusinessObjectCollection.GetElementTypeFromCollectionType(processHeader.Parent.WorkflowItems.GetType()); }
		}

		#endregion
	}
}
