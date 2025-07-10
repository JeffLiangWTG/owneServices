using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(OtherPackage))]
	class OtherPackageTest : Customs.Business.Testing.CusCodeDataTest<OtherPackage>
	{
		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, ((OtherPackage)BusinessObject).SupportsNotes);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			return declaration.CustomsEntryInstructions.AddNew().OtherPackages.AddNew();
		}

		public void TestDefaultValue()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var otherPackage = instruction.OtherPackages.AddNew();
			AssertEquals(Constants.CusCodeDataTypes.Codes.Package, otherPackage.CY_Type);
		}
	}
}
