using System;
using System.Reflection;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(ProjectCollectionProvider))]
	sealed class ProjectCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => Assembly.Load("Enterprise.ProcessManagement.Business").GetType("Enterprise.ProcessManagement.Business.ProjectCollection");

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Project;

		protected override int ExpectedMaxLength => WorkProjectSchema.WKP_ProjectNumber.MaxLength;
	}
}
