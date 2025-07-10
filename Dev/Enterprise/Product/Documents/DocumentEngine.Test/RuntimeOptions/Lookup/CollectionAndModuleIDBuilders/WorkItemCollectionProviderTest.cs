using System;
using System.Reflection;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(WorkItemCollectionProvider))]
	sealed class WorkItemCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => Assembly.Load("Enterprise.ProcessManagement.Business").GetType("Enterprise.ProcessManagement.Business.WorkItemCollection");

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WorkItem;

		protected override int ExpectedMaxLength => WorkItemSchema.WKI_WorkItemNumber.MaxLength;
	}
}
