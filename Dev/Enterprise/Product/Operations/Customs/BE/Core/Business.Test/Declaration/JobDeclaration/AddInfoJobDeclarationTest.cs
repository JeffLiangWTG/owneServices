using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(JobDeclaration))]
sealed class AddInfoJobDeclarationTest : NonPersistentBusinessObjectTestCase
{
	public void TestValidationType()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertType<ImportJobDeclarationValidation>("IMP", declaration.AddInfoValidation);
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationValidation>("EXP", declaration.AddInfoValidation);
		});
	}

	public void TestZG_RegionOfDestination()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(1, declaration.ZG_RegionOfDestinationInfo.MaxLength);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return Factory.New<JobDeclaration>();
	}
}
