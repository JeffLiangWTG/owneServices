using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(ContactCollectionProvider))]
	sealed class ContactCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(OrgContactCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.OrgContacts;
	}
}
