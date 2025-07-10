using System;
using System.Collections.Generic;
using System.Globalization;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class EDIWorkflowDescriptorsTest : TestCase
	{
		public void TestThisSubTypeOverride()
		{
			AssertEquals(typeof(EDIWorkflowDescriptors), WorkflowDescriptors.Instance.GetType());
		}

		public void TestEDIWorkflowDescriptors()
		{
			WorkflowDescriptors descriptors = WorkflowDescriptors.Instance;
			WorkflowDescriptor[] descriptorsArray = new List<WorkflowDescriptor>(descriptors.Values).ToArray();
			AssertContainDescriptorType(descriptorsArray, new WorkItemWorkflowDescriptor().GetType());
			AssertContainDescriptorCode(descriptorsArray, EDIJobInvoicingConsumerTypes.Incident.Code);
			AssertContainDescriptorCode(descriptorsArray, EDIJobInvoicingConsumerTypes.PSQuote.Code);
			AssertContainDescriptorType(descriptorsArray, new IncidentManagementGroupWorkflowDescriptor().GetType());
			AssertContainDescriptorType(descriptorsArray, new ProjectWorkflowDescriptor().GetType());
			AssertContainDescriptorType(descriptorsArray, new EDIGlbStaffWorkflowDescriptor().GetType());
		}

		void AssertContainDescriptorCode(WorkflowDescriptor[] descriptors, string descriptorCode)
		{
			bool descriptorFound = Array.Exists(descriptors, delegate(WorkflowDescriptor descriptor)
			{
				return descriptor.Code == descriptorCode;
			});

			Assert(string.Format(CultureInfo.CurrentCulture, "WorkflowDescriptor '{0}' not found", descriptorCode), descriptorFound);
		}

		void AssertContainDescriptorType(WorkflowDescriptor[] descriptors, Type descriptorType)
		{
			bool descriptorFound = Array.Exists(descriptors, delegate(WorkflowDescriptor descriptor)
			{
				return descriptor.GetType() == descriptorType;
			});

			Assert(string.Format(CultureInfo.CurrentCulture, "'{0}' not found", descriptorType), descriptorFound);
		}
	}
}
