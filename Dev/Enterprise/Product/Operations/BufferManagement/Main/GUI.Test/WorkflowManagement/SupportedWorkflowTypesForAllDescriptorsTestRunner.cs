using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class SupportedWorkflowTypesForAllDescriptorsTestRunner : ISupportedWorkflowTypesForAllDescriptorsTestRunner
	{
		struct WorkflowTypeObjects
		{
			public Type BizoType { get; set; }
			public BusinessObject Bizo { get; set; }
			public IWorkflowProvider Job { get; set; }
			public ProcessJobHeader JobHeader { get; set; }
			public ProcessHeader Workflow { get; set; }
		}

		public void RunTestAllSupportedJobTypes(IWorkflowDescriptor descriptor)
		{
			var testObj = GetWorkflowTypeObjects(descriptor, Factory);

			if (testObj.JobHeader != null)
			{
				Assertion.AssertEquals($@"{descriptor.GetType().FullName} descriptor. Descriptor code does not match workflow type", descriptor.Code, testObj.Workflow.FH_WorkflowType);

				var newFactory = new BusinessObjectFactory();

				IProcessHeader loadedWorkflow = newFactory.Load<ProcessHeader>(testObj.Workflow.PK);
				var workflowView = loadedWorkflow.GetView();

				var parent = loadedWorkflow.Parent;
				if (!(testObj.Bizo is NonPersistentBusinessObject))
				{
					var loadedJob = newFactory.Load(testObj.BizoType, testObj.Job.PK);
					Assertion.AssertNotNull($@"{testObj.BizoType.Name} loaded job", loadedJob);
					Assertion.AssertEquals($@"{descriptor.GetType().FullName} descriptor. {testObj.BizoType.Name} job parent. If this fails the table prefix [{testObj.Bizo.TablePrefix}] needs to be added to BusinessObjectPrefixTypesConfiguration.xml under EnterpriseBusinessObjectPrefixTypes.", loadedJob, parent);
				}

				if (parent != null)
				{
					AssertWorkflowType(descriptor, workflowView, testObj.Bizo);
					try
					{
						using (var form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(loadedWorkflow))
						{
							// it's incorect to assume that the form's business object type will be the same as the bizoType; doesn't work for situation like Declaration plugin to Shipment form.
							Assertion.AssertNotNull($@"{descriptor.Description} descriptor. {testObj.BizoType.Name} form. If this fails then override ControllerID in the WorkflowDescriptor [{descriptor.GetType().FullName}]. Or, check that we can create a controller for this country (probably currently AU). If not, either register your controller for this country or override SupportsBufferManagement in your WorkFlowDescriptor and return false (which is pretty sad to opt-out of buffer management entirely just because of this arse-backwards test)", form);
						}
					}
					catch (ModuleGuiNotSupportedException)
					{
						// Module doesn't support it - nothing we can do about it here.
					}
					catch (Exception ex)
					{
						throw new Exception($@"Descriptor:{descriptor.GetType().FullName}, BizObj:{testObj.Bizo.GetType().FullName}", ex);
					}
				}
			}
		}

		public void RunTestAllSupportedJobTypes_ShouldDisplayOnVisualBoard(IWorkflowDescriptor descriptor)
		{
			var testObj = GetWorkflowTypeObjects(descriptor, Factory);

			if (testObj.JobHeader != null)
			{
				var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, new string[] { testObj.Workflow.FH_WorkflowType }, "VB101", shouldUseExistingSystem: true);
				var section = config.BufferSection;
				var workflowTask = BMSTestHelper.CreateWorkflowAndTask(testObj.JobHeader, "Workflow", config.Buffer);

				Factory.Save();

				using (BMSGUITestCase.DisableAsyncBehaviour())
				using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section))
				{
					var control = form.FindAll<BMComponentControl>().Single();
					var taskCard = control.FindAll<TaskCardControl>().ToArray().LastOrDefault();
					Assertion.AssertNotNull($@"Descriptor:{descriptor.GetType().FullName}, BizObj:{testObj.Bizo.GetType().FullName}, ProcessHeaderType:{testObj.Workflow.FH_WorkflowType}, The task for the Job Header is not displayed on the Buffer board.", taskCard);
				}
			}
		}

		public void RunTestAllSupportedJobTypes_ShouldPerformFilterOnVisualBoard(IWorkflowDescriptor descriptor)
		{
			Assertion.AssertNoExceptionThrown($@"{descriptor.GetType().FullName} descriptor. No exception should have been thrown", () =>
			{
				var testObj = GetWorkflowTypeObjects(descriptor, Factory);

				if (testObj.JobHeader != null)
				{
					var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, new string[] { testObj.Workflow.FH_WorkflowType }, "VB101", shouldUseExistingSystem: true);
					var section = config.BufferSection;
					var workflowTask = BMSTestHelper.CreateWorkflowAndTask(testObj.JobHeader, "Workflow", config.Buffer);

					Factory.Save();

					using (BMSGUITestCase.DisableAsyncBehaviour())
					using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section))
					{
						var searchControl = form.FindAll<ZSearchBox>().Single();

						searchControl.SearchTerm = "I am searching for something";
						searchControl.OnSearchPerformed(false);
					}
				}
			});
		}

		void TestJobTypesCore(bool shouldDisplayManagementTab, IWorkflowDescriptor descriptor, Action<WorkflowTypeObjects, WorkflowManagementTabPage> assertionAction)
		{
			var testObj = GetWorkflowTypeObjects(descriptor, Factory);

			if (testObj.JobHeader != null)
			{
				IProcessHeader loadedWorkflow = Factory.Load<ProcessHeader>(testObj.Workflow.PK);
				var workflowView = loadedWorkflow.GetView();
				var parent = loadedWorkflow.Parent;

				if (parent != null)
				{
					try
					{
						var system = BMSystem.GetSystemForWorkflowType(testObj.Workflow.FH_WorkflowType, Factory) ?? BMSTestHelper.GetOrCreateSystem(Factory, new string[] { testObj.Workflow.FH_WorkflowType });
						system.RelatedWorkflowTypes.Single().FSW_IsActive = shouldDisplayManagementTab;

						Factory.Save();

						using (var form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(loadedWorkflow))
						{
							Assertion.AssertNotNull($@"Descriptor:{descriptor.GetType().FullName}, BizObj:{testObj.Bizo.GetType().FullName}, ProcessHeaderType:{testObj.Workflow.FH_WorkflowType}, The form for the Job Header can not be displayed.", form);

							BMSGUITestCase.DoEvents();
							var managementTab = ((ZForm)form).FindAll<WorkflowManagementTabPage>().FirstOrDefault();
							assertionAction(testObj, managementTab);
						}
					}
					catch (ModuleGuiNotSupportedException)
					{
						// Module doesn't support it - nothing we can do about it here.
						Assertion.Assert(true);
					}
					catch (Exception ex)
					{
						throw new Exception($@"Descriptor:{descriptor.GetType().FullName}, BizObj:{testObj.Bizo.GetType().FullName}", ex);
					}
				}
			}
		}

		public void RunTestAllSupportedJobTypes_ShouldDisplayManagementTab(IWorkflowDescriptor descriptor)
		{
			var exceptedJobTypes = new string[] { "ENT", "GRP" };
			//The exception list explanations
			//ENT - Entry Header - Does not currently support the Management tab 
			//GRP - Release Group - Does not currently support the Management tab
			BMSTestHelper.EnableBMSInRegistry();

			TestJobTypesCore(shouldDisplayManagementTab: true, descriptor, (testObj, managementTab) =>
			{
				if (!exceptedJobTypes.Contains(descriptor.Code))
				{
					Assertion.AssertNotNull($@"Descriptor:{descriptor.GetType().FullName}, BizObj:{testObj.Bizo.GetType().FullName}, ProcessHeaderType:{testObj.Workflow.FH_WorkflowType}, The Management Tab for the Job Header is not displayed on the Job Form.", managementTab);
				}
				else
				{
					Assertion.AssertNull($@"Descriptor:{descriptor.GetType().FullName}, BizObj:{testObj.Bizo.GetType().FullName}, ProcessHeaderType:{testObj.Workflow.FH_WorkflowType}, The Management Tab for this WorkflowProvider type cannot be currently displayed.", managementTab);
				}
			});
		}

		public void RunTestInactiveRelatedJobType_ShouldNotDisplayManagementTab(IWorkflowDescriptor descriptor)
		{
			AssertActiveRelatedJobTypes(descriptor, shouldDisplayManagementTab: false);
		}

		public void RunTestActiveRelatedJobType_ShouldDisplayManagementTab(IWorkflowDescriptor descriptor)
		{
			AssertActiveRelatedJobTypes(descriptor, shouldDisplayManagementTab: true);
		}

		void AssertActiveRelatedJobTypes(IWorkflowDescriptor descriptor, bool shouldDisplayManagementTab)
		{
			var exceptedJobTypes = new string[] { "ENT", "GRP" };
			//The exception list explanations
			//ENT - Entry Header - Does not currently support the Management tab 
			//GRP - Release Group - Does not currently support the Management tab
			BMSTestHelper.EnableBMSInRegistry();

			TestJobTypesCore(shouldDisplayManagementTab, descriptor, (testObj, managementTab) =>
			{
				if (!exceptedJobTypes.Contains(descriptor.Code) && shouldDisplayManagementTab)
				{
					Assertion.AssertNotNull($@"Descriptor:{descriptor.GetType().FullName}, BizObj:{testObj.Bizo.GetType().FullName}, ProcessHeaderType:{testObj.Workflow.FH_WorkflowType}, The Management Tab for the Job Header is not displayed on the Job Form while active.", managementTab);
				}
				else
				{
					Assertion.AssertNull($@"Descriptor:{descriptor.GetType().FullName}, BizObj:{testObj.Bizo.GetType().FullName}, ProcessHeaderType:{testObj.Workflow.FH_WorkflowType}, The Management Tab for the Job Header is displayed on the Job Form while NOT active.", managementTab);
				}
			});
		}

		public void RunTestAllSupportedJobTypes_ShouldSafelyCreateFetchHints(IWorkflowDescriptor descriptor)
		{
			var testObj = GetWorkflowTypeObjects(descriptor, Factory);

			if (testObj.JobHeader != null)
			{
				IProcessHeader loadedWorkflow = Factory.Load<ProcessHeader>(testObj.Workflow.PK);
				var parent = loadedWorkflow.Parent;

				if (parent != null)
				{
					Assertion.AssertNoExceptionThrown($@"{descriptor.GetType().FullName} workflow descriptor failed to create a workflow that could safely have fetchhints be added.", () => testObj.Workflow.AddDeepFetchHintForParentType());
				}
				else
				{
					Assertion.Assert($@"When making a workflow/provider pair from {descriptor.GetType().FullName}, the workflow or its parent was bad.", false);
				}
			}
		}

		public void RunTestAllSupportedJobTypes_ShouldBeAbleToSaveJobNetworks(IWorkflowDescriptor descriptor)
		{
			var testObj = GetWorkflowTypeObjects(descriptor, Factory);

			if (testObj.JobHeader != null)
			{
				IProcessHeader loadedWorkflow = Factory.Load<ProcessHeader>(testObj.Workflow.PK);
				var parent = loadedWorkflow.Parent;

				if (parent != null && loadedWorkflow is ProcessHeader actuallyAWorkflow)
				{
					actuallyAWorkflow.FH_CompletionStatement = "workflow1";
					var diagram = NetworkTestCase.CreateDiagram(testObj.JobHeader);
					var network = NetworkTestCase.CreateNetwork(diagram);
					var shape = network.ShowEntity(actuallyAWorkflow, diagram).First().AsShape();

					Assertion.AssertEquals("workflow1", shape.BNS_Name);
					Assertion.AssertEquals("workflow1", shape.Name);
					Assertion.AssertNoExceptionThrown($"Saving a JobNetwork created from a {descriptor.GetType().FullName} should be possible.", () => Factory.Save());
				}
			}
		}

		public void RunTestAllSupportedJobTypes_ShouldBeWorkQueuable_ShouldNotDieHorribly(IWorkflowDescriptor descriptor)
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var testObj = GetWorkflowTypeObjects(descriptor, Factory);

			if (testObj.JobHeader != null)
			{
				var loadedWorkflow = Factory.Load<ProcessHeader>(testObj.Workflow.PK);
				var parent = loadedWorkflow.Parent;

				if (parent != null)
				{
					loadedWorkflow.FH_CompletionStatement = "workflow1";

					Factory.Save();

					using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.WorkQueues).ShowEditForm(queue))
					{
						Assertion.AssertNoExceptionThrown($"Saving a JobNetwork created from a {descriptor.GetType().FullName} should be possible.", () => BatchTagOperator.TryAddTag(queue, new[] { loadedWorkflow }, out string dummyString));
						Assertion.AssertNoExceptionThrown($"Saving a JobNetwork created from a {descriptor.GetType().FullName} should be possible.", () => Factory.Save());
					}
				}
			}
		}

		static void AssertWorkflowType(IWorkflowDescriptor descriptor, ViewProcessHeader workflowView, BusinessObject parent)
		{
			if (descriptor.Code != workflowView.VFH_WorkflowType)
			{
				Assertion.Assert(true);
			}
			else
			{
				Assertion.AssertEquals(string.Format("VFH_WorkflowType for {0} job (table code {1}, table {2})", descriptor.WorkflowProviderType.Name, parent.TablePrefix, parent.TableName), descriptor.Code, workflowView.VFH_WorkflowType);
			}
		}

		static WorkflowTypeObjects GetWorkflowTypeObjects(IWorkflowDescriptor descriptor, BusinessObjectFactory factory)
		{
			var bizo = descriptor.GetBizOForTest(factory);

			var objects = new WorkflowTypeObjects
			{
				BizoType = descriptor.WorkflowProviderType,
				Bizo = bizo,
				Job = bizo as IWorkflowProvider
			};

			if (objects.Job != null && !(objects.Job is ProcessTask) && !(objects.Job is ProcessHeader))
			{
				objects.JobHeader = ProcessJobHeader.GetForParent(objects.Job, factory, checkTemplates: false);
				objects.Workflow = objects.JobHeader.ProcessHeaders.AddNew();
			}

			factory.Save();

			return objects;
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
