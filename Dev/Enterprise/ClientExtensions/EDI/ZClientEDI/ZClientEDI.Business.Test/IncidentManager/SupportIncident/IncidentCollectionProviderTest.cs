using System;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.BusinessTesting
{
	[TestedType(typeof(IncidentCollectionProvider))]
	public class IncidentCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(SupportIncidentCollection);

		protected override ModuleIdentifier ExpectedModuleID => Modules.ClientModuleRegistration.SupportIncident;

		protected override int ExpectedMaxLength => IncidentMainSchema.IM_IncidentNumber.MaxLength;
	}
}
