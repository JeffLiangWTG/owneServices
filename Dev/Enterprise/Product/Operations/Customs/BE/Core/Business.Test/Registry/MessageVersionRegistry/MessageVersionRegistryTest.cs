using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(MessageVersionRegistry))]
sealed class MessageVersionRegistryTest : RegistryBusinessObjectTemplateTestCase<MessageVersionRegistry>
{
	public void TestValidNCTSTarget()
	{
		AssertEquals(MessageVersionRegistry.NCTSP5DefaultTarget, "NCTS.BE");
	}

	public void TestValidAESTarget()
	{
		AssertEquals(MessageVersionRegistry.AESDefaultTarget, "AES.BE");
	}

	public void TestValidIDMSTarget()
	{
		AssertEquals(MessageVersionRegistry.IDMSDefaultTarget, "IDMS.BE");
	}

	public void TestSystemCodeReadOnly()
	{
		Assert(new MessageVersionRegistry().DomainCodeInfo.ReadOnly);
	}

	public void TestAtlasVersionNumber()
	{
		AssertEquals(MessageVersionRegistry.NCTSP5DomainCode, "NCTSP5");
	}

	public void TestAESVersionNumber()
	{
		AssertEquals(MessageVersionRegistry.AESDomainCode, "AES");
	}

	public void TestEMCSVersionNumber()
	{
		AssertEquals(MessageVersionRegistry.IDMSDomainCode, "IDMS");
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
