using System;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WorkflowItemWrapper))]
	sealed class WorkflowItemWrapperTest : Base.Testing.GenericWrapperTest
	{
		protected override void SetUp()
		{
			var forwardingShipment = Factory.New<ForwardingShipment>();
			processTask = forwardingShipment.WorkflowItems.AddNew();
			processTask.P9_Description = "desc";
			processTask.P9_Type = "TYP";
			processTask.TaskProperties.ActualDate = new ZDateTimeOffset(new ZDateTime(2000, 2, 20, 20, 20, 20));
			processTask.TaskProperties.ScheduledDate = new ZDateTimeOffset(new ZDateTime(1900, 10, 10, 10, 10, 10));
			base.SetUp();
		}
		ProcessTask processTask;

		public override void TestWrapperMappingsEmpty()
		{
			WorkflowItemWrapper wrapperEmpty = (WorkflowItemWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.Description", ZString.Empty, wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Type", ZString.Empty, wrapperEmpty.Type);
			AssertEquals("wrapperEmpty.TypeDescription", ZString.Empty, wrapperEmpty.TypeDescription);
		}

		public void TestDocWorkFlow()
		{
			WorkflowItemWrapper result = WorkflowItemWrapper.New(processTask, Factory);
			AssertEquals(result.Description, processTask.P9_Description);
			AssertEquals(result.Type, processTask.P9_Type);
			AssertEquals(result.ActualDate, processTask.P9_ActualDate);
			AssertEquals(result.ScheduledDate, processTask.P9_ScheduledDate);
			AssertEquals(result.DateTime, processTask.P9_ActualDate);
			processTask.TaskProperties.ActualDate = ZDateTimeOffset.Empty;
			DocumentsDataRegistry.Instance.IncludeEstimatedMilesonesOnPODDocument.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(result.DateTime, ZDateTime.Empty);
			DocumentsDataRegistry.Instance.IncludeEstimatedMilesonesOnPODDocument.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(result.DateTime, processTask.P9_ScheduledDate);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return WorkflowItemWrapper.New(Factory.New<ProcessTask>(), Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Milestone                                 (Default Field: Description)
======================================================================
Name                                    Type
----------------------------------------------------------------------
ActualDate                              DateTime
DateTime                                DateTime
Description                             String
EstimatedIsShownFlag                    String
EstimatedToBeShown                      Bool
ScheduledDate                           DateTime
Type                                    String
TypeDescription                         String

";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			ProcessTask processTask = Factory.New<ProcessTask>();
			return WorkflowItemWrapper.New(processTask, Factory);
		}
	}
}
