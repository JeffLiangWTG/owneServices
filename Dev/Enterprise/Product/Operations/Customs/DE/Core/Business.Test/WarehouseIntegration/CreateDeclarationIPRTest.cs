using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CreateDeclarationIPR))]
	sealed class CreateDeclarationIPRTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCPC()
		{
			var resourceStringDataAttribute = createDeclarationIPR.CPCInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Default Value", "40", createDeclarationIPR.CPC);
		}

		public void TestDeclarationType()
		{
			var resourceStringDataAttribute = createDeclarationIPR.DeclarationTypeInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Default Value", "AVABR", createDeclarationIPR.DeclarationType);
		}

		public void TestDeclarantsReference()
		{
			var resourceStringDataAttribute = createDeclarationIPR.DeclarantsReferenceInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("MaxLength", 22, createDeclarationIPR.DeclarantsReferenceInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CreateDeclarationIPR();
		}

		protected override void SetUp()
		{
			base.SetUp();

			createDeclarationIPR = new CreateDeclarationIPR();
		}

		CreateDeclarationIPR createDeclarationIPR;
	}
}
