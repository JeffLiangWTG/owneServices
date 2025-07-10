using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(ShipsAgencyPrincipalProvider))]
	sealed class ShipsAgencyPrincipalProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(ShipsAgencyPrincipalCollectionWithSecurityCheck);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Organisation;
	}
}
