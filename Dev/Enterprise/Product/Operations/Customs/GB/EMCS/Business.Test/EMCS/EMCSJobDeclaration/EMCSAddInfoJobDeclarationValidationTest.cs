using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSAddInfoJobDeclaration))]
	sealed class EMCSAddInfoJobDeclarationValidationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTypeOfValidation()
		{
			var infoDeclaration = (EMCSAddInfoJobDeclaration)GetNewBusinessObject();
			AssertType<EMCSAddInfoJobDeclarationValidation>(infoDeclaration.Validation);
		}

		public void TestCheckZG_DispatchReference()
		{
			declaration.AddInfoValidation.ValidateZG_DispatchReference();
			AssertNoMessageErrors("No message errors", declaration.ZG_DispatchReferenceInfo);

			declaration.ZG_DispatchReference = "REFERENCE";
			AssertNoMessageErrors("No message errors", declaration.ZG_DispatchReferenceInfo);
		}

		protected override BusinessObject GetNewBusinessObject() => new EMCSAddInfoJobDeclaration(declaration);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
	}
}
