using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AccTaxRateListElement))]
	sealed class AccTaxRateListElementTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestAccTaxRateCollection()
		{
			ZString anotherCountry = Env.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Australia ? Core.Constants.CountryCodes.UnitedKingdom : Core.Constants.CountryCodes.Australia;

			AccTaxRate taxRate1 = Factory.New<AccTaxRate>();
			AccTaxRate taxRate2 = Factory.New<AccTaxRate>();
			AccTaxRate taxRate3 = Factory.New<AccTaxRate>();

			taxRate1.AT_RN_NKCountry = Env.CurrentCompany.Country.Code;
			taxRate2.AT_RN_NKCountry = Env.CurrentCompany.Country.Code;
			taxRate3.AT_RN_NKCountry = anotherCountry;

			AccTaxRateListCollectionWrapper wrapper = new AccTaxRateListCollectionWrapper("", RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
			AccTaxRateListElement element = wrapper.AccTaxRateList.AddNew();

			element.AccTaxRateCollection.Load();
			AssertEquals("AccTaxRateCollection.Contains(TaxRate1)", true, element.AccTaxRateCollection.Contains(taxRate1));
			AssertEquals("AccTaxRateCollection.Contains(TaxRate2)", true, element.AccTaxRateCollection.Contains(taxRate2));
			AssertEquals("AccTaxRateCollection.Contains(TaxRate3)", false, element.AccTaxRateCollection.Contains(taxRate3));
		}

		public void TestAccTaxRateListElement()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Description = "TaxRateDescription";

			var elem = (AccTaxRateListElement)GetNewBusinessObject();
			elem.TaxRate = taxRate.PK;

			AssertEquals("TaxRate", taxRate.PK, elem.TaxRate);
			AssertEquals("TaxRateDescription", "TaxRateDescription", elem.TaxRateDescription);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			AccTaxRateListCollection collection = new AccTaxRateListCollection("", RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
			AccTaxRateListElement bizO = new AccTaxRateListElement(ZGuid.Empty, collection);
			return bizO;
		}

		#endregion
	}
}
