using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.WorkflowTemplates
{
	public class WorkflowTemplateInterceptorTest : TransactionedTestCase
	{
		IEntityDefinition TemplateDefintion => TestUtil.FindEntityDefinition("WorkflowTemplate", "ProcessTaskTemplate");
		IEntityDefinition TaskDefintion => TestUtil.FindEntityDefinition("WorkflowTemplate", "ProcessTaskTemplate.ProcessTasks");

		EntitySet CreateTemplateWithUdfTask()
		{
			var entitySet = new EntitySet("WorkflowTemplate");
			var template = new Entity(TemplateDefintion, sessionServices);
			template["Name"] = "TEMPLATE";
			template.Action = EntityAction.MERGE;
			template.InternalPK = Guid.NewGuid();
			entitySet.Root = template;

			var task = new Entity(TaskDefintion, sessionServices);
			task["Description"] = "TASK FOR TEST";
			task["Condition2"] = "UDF";
			task["Condition2Value"] = "\"1\"==\"1\"";
			task.Action = EntityAction.MERGE;
			task.InternalPK = Guid.NewGuid();

			template.ChildrenCollection.Add(task);

			return entitySet;
		}

		public void TestInterceptorSettings()
		{
			var settings = new WorkflowTemplateSetting();
			AssertEquals(0, settings.DisableList.Count());
			AssertContainsExactElementsInAnyOrder(new string[] { "WorkflowTemplate" }, settings.EnableList);
		}

		public void Test_UsingCondition2ValueForUdfCondition()
		{
			var template = CreateTemplateWithUdfTask();
			context.Update(template);

			AssertTaskHasUdfCondition("\"1\"==\"1\"");
		}

		public void Test_UpdateExistingUdfCondition()
		{
			var template = CreateTemplateWithUdfTask();
			context.Update(template);
			AssertTaskHasUdfCondition("\"1\"==\"1\"");

			var builder = new EntitySetBuilder();
			var templateBizo = factory.LoadTop1<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_Name, "TEMPLATE"));
			var taskBizo = factory.LoadTop1<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_Description, "TASK FOR TEST"));
			var templateEntity = builder.BuildEntitySet(((INeedRow)templateBizo).Row, TemplateDefintion, sessionServices);
			var taskEntity = builder.BuildEntitySet(((INeedRow)taskBizo).Row, TaskDefintion, sessionServices);
			taskEntity["Condition2Value"] = "\"2\"==\"2\"";
			templateEntity.ChildrenCollection.Add(taskEntity);
			templateEntity.Action = EntityAction.MERGE;
			taskEntity.Action = EntityAction.MERGE;
			AssertEquals(taskEntity.ChildrenCollection.Count(), 1);
			foreach (var child in taskEntity.ChildrenCollection)
			{
				child.Action = EntityAction.MERGE;
			}
			template.Root = templateEntity;

			context.Update(template);

			AssertTaskHasUdfCondition("\"2\"==\"2\"");
		}

		public void Test_UpdateExistingUdfCondition_OnlyInDB()
		{
			var template = CreateTemplateWithUdfTask();
			context.Update(template);
			AssertTaskHasUdfCondition("\"1\"==\"1\"");

			var builder = new EntitySetBuilder();
			var templateBizo = factory.LoadTop1<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_Name, "TEMPLATE"));
			var taskBizo = factory.LoadTop1<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_Description, "TASK FOR TEST"));
			var templateEntity = builder.BuildEntitySet(((INeedRow)templateBizo).Row, TemplateDefintion, sessionServices);
			var taskEntity = builder.BuildEntitySet(((INeedRow)taskBizo).Row, TaskDefintion, sessionServices);
			taskEntity["Condition2Value"] = "\"2\"==\"2\"";
			templateEntity.ChildrenCollection.Add(taskEntity);
			templateEntity.Action = EntityAction.MERGE;
			taskEntity.Action = EntityAction.MERGE;
			AssertEquals(taskEntity.ChildrenCollection.Count(), 1);
			taskEntity.ChildrenCollection.RemoveAll();//remove StmNote entity 
			template.Root = templateEntity;

			context.Update(template);

			AssertTaskHasUdfCondition("\"2\"==\"2\"");
		}

		void AssertTaskHasUdfCondition(string condition)
		{
			var task = new BusinessObjectFactory().LoadTop1<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_Description, "TASK FOR TEST"));
			AssertEquals("This getter should get the udf condition from the StmNote table", condition, task.TemplateConditions.TemplateCondition2Value);
		}

		protected override void SetUp()
		{
			base.SetUp();
			sessionServices = new AncillaryImportServices();
			factory = new BusinessObjectFactory();
			context = new UpdateContext(sessionServices, new FactoryProvider());

			var setting = new WorkflowTemplateSetting();
			setting.Enable = true;
			setting.Context = context;

			interceptor = new WorkflowTemplateInterceptor(setting, sessionServices);
			setting.Interceptor = interceptor;

			context.InterceptorSettings.Add(setting);
		}

		AncillaryImportServices sessionServices;
		WorkflowTemplateInterceptor interceptor;
		BusinessObjectFactory factory;
		IUpdateContext context;
	}
}
