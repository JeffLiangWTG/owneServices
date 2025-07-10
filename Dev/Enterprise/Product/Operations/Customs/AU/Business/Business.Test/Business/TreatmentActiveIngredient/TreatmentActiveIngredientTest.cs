using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(TreatmentActiveIngredient))]
	class TreatmentActiveIngredientTest : CusCodeDataTest<TreatmentActiveIngredient>
	{
		public void TestSetDefaultValues()
		{
			var customsManifestLineSequence = Factory.New<TreatmentActiveIngredient>();
			AssertEquals(CusCodeDataTypeList.Codes.EXDOCTreatmentActiveIngredient, customsManifestLineSequence.CY_Type);
		}

		public void TestValidationType()
		{
			AssertType<TreatmentActiveIngredientValidation>(((TreatmentActiveIngredient)GetNewBusinessObject()).Validation);
		}

		public void TestLookupsType()
		{
			AssertType<TreatmentActiveIngredientLookups>(((TreatmentActiveIngredient)GetNewBusinessObject()).Lookups);
		}

		public void TestCY_CodeMaxLength()
		{
			var treatmentActiveIngredient = Factory.New<TreatmentActiveIngredient>();
			AssertEquals(4, treatmentActiveIngredient.CY_CodeInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var process = factory.NewWithValidTestData<QuarantineExDocEstablishmentAndTime>();
			var collection = process.Ingredients;
			return collection.AddNew();
		}
	}
}
