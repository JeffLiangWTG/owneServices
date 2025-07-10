using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(MessageVersionRegistry))]
sealed class MessageVersionRegistryTest : RegistryBusinessObjectTemplateTestCase<MessageVersionRegistry>
{
	public void TestValidNCTSTarget()
	{
		AssertEquals(MessageVersionRegistry.NCTSP5DefaultTarget, "NCTS.NL");
	}

	public void TestSystemCodeReadOnly()
	{
		Assert(new MessageVersionRegistry().DomainCodeInfo.ReadOnly);
	}

	public void TestNCTSVersionNumber()
	{
		AssertEquals(MessageVersionRegistry.NCTSP5DomainCode, "NCTSP5");
	}

	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => true;

	protected override MessageVersionRegistry GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

	protected override MessageVersionRegistry GetBusinessObjectToSerialise()
	{
		BizObj.DomainCode = MessageVersionRegistry.NCTSP5DomainCode;
		BizObj.TargetSystemName = MessageVersionRegistry.NCTSP5DefaultTarget;
		return BizObj;
	}
}
