using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AccTaxRateListCollectionWrapper))]
	sealed class AccTaxRateListCollectionWrapperTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestAccTaxRateListCollectionWrapper()
		{
			ZGuid guid1 = ZGuid.NewZGuid();
			ZGuid guid2 = ZGuid.NewZGuid();
			ZGuid guid3 = ZGuid.NewZGuid();
			ZGuid guid4 = ZGuid.NewZGuid();
			string value1 = guid1 + "," + guid2 + "," + guid3;
			string value2 = guid1 + "," + guid3 + "," + guid4;

			AccTaxRateListCollectionWrapper bizO = new AccTaxRateListCollectionWrapper(value1, RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
			AssertEquals("TaxRate[0]", guid1, bizO.AccTaxRateList[0].TaxRate);
			AssertEquals("TaxRate[1]", guid2, bizO.AccTaxRateList[1].TaxRate);
			AssertEquals("TaxRate[2]", guid3, bizO.AccTaxRateList[2].TaxRate);

			bizO.AccTaxRateList.Remove(bizO.AccTaxRateList[1]);
			AccTaxRateListElement elem = bizO.AccTaxRateList.AddNew();
			elem.TaxRate = guid4;
			AssertEquals("Value", value2, bizO.AccTaxRateList.ToString());
		}

		public void TestAccTaxRateListFilter()
		{
			AccTaxRateListCollectionWrapper bizO = new AccTaxRateListCollectionWrapper("", RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
			AssertEquals("AccTaxRateList.Filter", RegistryFindBoxFilter.None, bizO.AccTaxRateList.Filter);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AccTaxRateListCollectionWrapper("", RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
		}

		#endregion
	}
}
