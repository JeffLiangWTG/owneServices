namespace Enterprise.Customs.IN.Business.Testing;

abstract class JobDeclarationValidationAbstractTest : Customs.Business.Testing.BaseJobDeclarationValidationTest<JobDeclaration>
{
	protected override void SetUp()
	{
		base.SetUp();
		Declaration = Factory.New<JobDeclaration>();
		Declaration.JE_MessageType = MessageType;
	}
	protected JobDeclaration Declaration { get; set; }

	protected abstract string MessageType { get; }
}
