using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class ExportAddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckJE_SpecificCircumstanceIndicator() => CombineAssertions(() =>
	{
		var message = "Cannot use Special Circumstances 'A20' when Security is '0'";

		declaration.ZG_TypeOfSecurity = ExportSecurityTypeList.Codes.NotUsed;
		declaration.ZG_SpecificCircumstanceIndicator = string.Empty;
		AssertNoMessageError(declaration.ZG_SpecificCircumstanceIndicatorInfo, message);
		declaration.ZG_SpecificCircumstanceIndicator = NLSpecificCircumstanceIndicatorList.Codes.A20;
		AssertHasMessageError(declaration.ZG_SpecificCircumstanceIndicatorInfo, message);

		declaration.ZG_TypeOfSecurity = ExportSecurityTypeList.Codes.EXS;
		declaration.ZG_SpecificCircumstanceIndicator = string.Empty;
		AssertNoMessageError(declaration.ZG_SpecificCircumstanceIndicatorInfo, message);
		declaration.ZG_SpecificCircumstanceIndicator = NLSpecificCircumstanceIndicatorList.Codes.A20;
		AssertNoMessageError(declaration.ZG_SpecificCircumstanceIndicatorInfo, message);
	});

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
	}

	JobDeclaration declaration;
}
