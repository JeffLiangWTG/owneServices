using System;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocWorkFlow))]
	sealed class DocWorkFlowTest : DocumentWrapperTestCase
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

		public void TestDocWorkFlow()
		{
			DocWorkFlow result = DocWorkFlow.New(processTask, Factory);
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

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
				{
					DocWorkFlow.New(processTask, Factory),
				};
		}
	}
}
