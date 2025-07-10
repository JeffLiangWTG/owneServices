using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(JobDeclaration))]
class AddInfoJobDeclarationTest : NonPersistentBusinessObjectTestCase
{
	public void TestZG_SpecificCircumstanceIndicatorMaxLength()
	{
		AssertEquals(3, declaration.ZG_SpecificCircumstanceIndicatorInfo.MaxLength);
	}

	protected override BusinessObject GetNewBusinessObject() => declaration;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
}
