using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Utils.Models;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.WorkflowTemplates
{
	public class WorkflowTemplateInterceptor : BaseInterceptor
	{
		public WorkflowTemplateInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices) : base(setting, sessionServices)
		{
			factory = setting.Context.ObjectFactory;
		}
		readonly BusinessObjectFactory factory;

		public override void Invoke(IEntitySet entitySet)
		{
			entitySet.Root.DepthFirstTraversal((entity, relative) => { Intercept(entity); });

			Function(entitySet);
		}

		void Intercept(IEntity entity)
		{
			SetCondition2Value(entity);
		}

		void SetCondition2Value(IEntity entity)
		{
			var isUpdatingUdfCondition = entity.TableName.Equals(ProcessTasksSchema.Constants.TableName)
				&& (entity.Action == EntityAction.INSERT || entity.Action == EntityAction.MERGE || entity.Action == EntityAction.UPDATE)
				&& entity.GetPropertyOrBlankString("Condition2").Equals("UDF");

			if (isUpdatingUdfCondition)
			{
				var condition2Value = entity.GetPropertyOrBlankString("Condition2Value");

				if (!string.IsNullOrEmpty(condition2Value))
				{
					UpdateOrCreateUdfNote(entity, condition2Value);
					entity["Condition2Value"] = "";
				}
			}
		}

		void UpdateOrCreateUdfNote(IEntity taskEntity, string udfCondition)
		{
			var entity = taskEntity.ChildrenCollection.FirstOrDefault(x => x.EntityName == NoteEntityName);

			if (entity == null)
			{
				var definition = taskEntity.Definition.EntitySetDefinition.Entities.FindDefinition(CondtionNoteDefinitionPath);

				if (TryFindExistingNote(taskEntity.InternalPK, out StmNote note))
				{
					entity = new EntitySetBuilder().BuildEntitySet(((INeedRow)note).Row, definition, sessionServices);
				}
				else
				{
					entity = new Entity(definition, sessionServices);
					entity["NoteType"] = nameof(StmNoteVisibility.DOC);
					entity["Description"] = NoteDescription;
					entity["NoteContext"] = NoteContext;
				}
				((Entity)entity).Parent = taskEntity;
				taskEntity.ChildrenCollection.Add(entity);
				entity.Action = taskEntity.Action;
			}

			entity["NoteText"] = udfCondition;
		}

		bool TryFindExistingNote(Guid parentPK, out StmNote note)
		{
			var filter = new ZQuery();
			filter.AddToFilter(StmNoteSchema.ST_ParentID, parentPK);
			filter.AddToFilter(StmNoteSchema.ST_Table, ProcessTasksSchema.Constants.TableName);
			filter.AddToFilter(StmNoteSchema.ST_Description, NoteDescription);
			filter.AddToFilter(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.DOC));
			note = factory.LoadTop1<StmNote>(filter);
			return note != null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		const string NoteDescription = "User Defined Condition";

		const string NoteContext = "AAA";

		const string CondtionNoteDefinitionPath = "ProcessTaskTemplate.ProcessTasks.TemplateConditionNote";

		const string NoteEntityName = "TemplateConditionNote";
	}
}
