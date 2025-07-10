using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AccTaxRateListCollection))]
	sealed class AccTaxRateListCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AccTaxRateListCollection>
	{
		public void TestFilter()
		{
			AccTaxRate taxRate1 = Factory.New<AccTaxRate>();
			AccTaxRate taxRate2 = Factory.New<AccTaxRate>();

			taxRate1.AT_IsActive = true;
			taxRate2.AT_IsActive = false;
			taxRate1.AT_RN_NKCountry = Env.CurrentCompany.Country.Code;
			taxRate2.AT_RN_NKCountry = Env.CurrentCompany.Country.Code;

			AccTaxRateListCollection collection = new AccTaxRateListCollection("", RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
			collection.AccTaxRateCollection.Load();
			AssertEquals("AccTaxRateCollection.Contains(TaxRate1)", true, collection.AccTaxRateCollection.Contains(taxRate1));
			AssertEquals("AccTaxRateCollection.Contains(TaxRate2)", false, collection.AccTaxRateCollection.Contains(taxRate2));
		}

		#region Implementation

		protected override AccTaxRateListCollection GetCollectionToTest()
		{
			return new AccTaxRateListCollection("", RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AccTaxRateListElement(ZGuid.NewZGuid(), Collection);
		}

		#endregion
	}
}
