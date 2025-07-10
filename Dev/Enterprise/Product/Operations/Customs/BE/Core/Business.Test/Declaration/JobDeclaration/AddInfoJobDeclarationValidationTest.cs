using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(JobDeclaration))]
public class AddInfoJobDeclarationValidationTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		return Factory.New<JobDeclaration>();
	}

	public void TestCheckZG_VATDeferType()
	{
		var dec = Factory.New<JobDeclaration>();

		dec.ZG_VATDeferType = "A";
		dec.AddInfoValidation.ValidateZG_VATDeferType();
		AssertNoMessageErrorContaining(dec.ZG_VATDeferTypeInfo, ListValidation.InvalidCodeMessageError);

		dec.ZG_VATDeferType = "X";
		dec.AddInfoValidation.ValidateZG_VATDeferType();
		AssertHasMessageErrorContaining(dec.ZG_VATDeferTypeInfo, ListValidation.InvalidCodeMessageError);
	}
}
