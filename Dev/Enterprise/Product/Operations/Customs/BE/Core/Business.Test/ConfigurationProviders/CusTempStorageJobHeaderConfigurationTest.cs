using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CusTempStorageJobHeaderConfiguration))]
sealed class CusTempStorageJobHeaderConfigurationTest : EU.Business.Testing.CusTempStorageJobHeaderConfigurationAbstractTest<CusTempStorageJobHeaderConfiguration>
{
	protected override bool IsUCC6_Expected => true;
}
