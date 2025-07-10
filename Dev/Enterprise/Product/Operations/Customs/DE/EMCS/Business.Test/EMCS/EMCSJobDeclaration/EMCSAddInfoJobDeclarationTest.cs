using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSAddInfoJobDeclaration))]
	public class EMCSAddInfoJobDeclarationTest : NonPersistentBusinessObjectTestCase
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

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			return new EMCSAddInfoJobDeclaration(declaration);
		}
	}
}
