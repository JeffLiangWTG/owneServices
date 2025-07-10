using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidationType()
		{
			AssertType<CusGoodsLocationValidation>(cusGoodsLocation.Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => cusGoodsLocation;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.Parent = entryInstruction;
			cusGoodsLocation.CGL_LocationUse = "DEP";
		}

		CusGoodsLocation cusGoodsLocation;
	}
}
