using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSAddInfoJobDeclaration))]
	class EMCSAddInfoJobDeclarationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTypeOfValidation()
		{
			var infoDeclaration = (EMCSAddInfoJobDeclaration)GetNewBusinessObject();
			AssertType<EMCSAddInfoJobDeclarationValidation>(infoDeclaration.Validation);
		}

		public void TestTypeOfLookups()
		{
			var infoDeclaration = (EMCSAddInfoJobDeclaration)GetNewBusinessObject();
			AssertType<EMCSAddInfoJobDeclarationLookups>(infoDeclaration.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => new EMCSAddInfoJobDeclaration(Factory.New<EMCSJobDeclaration>());
	}
}
