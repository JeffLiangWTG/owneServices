using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(LicenceEnterpriseCollectionProvider))]
	public class LicenceEnterpriseCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(LicenceEnterpriseCollection);

		protected override ModuleIdentifier ExpectedModuleID => Modules.ClientModuleRegistration.LicenceEnterprise;

		protected override int ExpectedMaxLength => LicenceEnterpriseSchema.LE_EnterpriseCode.MaxLength;
	}
}
