using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AccChargeCodeListCollectionWrapper))]
	sealed class AccChargeCodeListCollectionWrapperTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestAccChargeCodeListCollectionWrapper()
		{
			var cc1 = Factory.NewWithValidTestData<AccChargeCode>();
			cc1.AC_Desc = "CC1";
			var cc2 = Factory.NewWithValidTestData<AccChargeCode>();
			cc1.AC_Desc = "CC1";
			var cc3 = Factory.NewWithValidTestData<AccChargeCode>();
			cc1.AC_Desc = "CC1";
			var cc4 = Factory.NewWithValidTestData<AccChargeCode>();
			cc1.AC_Desc = "CC1";
			Factory.Save();

			string value1 = cc1.PK + "," + cc2.PK + "," + cc3.PK;
			string value2 = cc1.PK + "," + cc3.PK + "," + cc4.PK;

			AccChargeCodeListCollectionWrapper bizO = new AccChargeCodeListCollectionWrapper(value1, RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
			AssertEquals("ChargeCode[0]", cc1.PK, bizO.AccChargeCodeList[0].ChargeCode);
			AssertEquals("ChargeCode[1]", cc2.PK, bizO.AccChargeCodeList[1].ChargeCode);
			AssertEquals("ChargeCode[2]", cc3.PK, bizO.AccChargeCodeList[2].ChargeCode);

			bizO.AccChargeCodeList.Remove(bizO.AccChargeCodeList[1]);
			AccChargeCodeListElement elem = bizO.AccChargeCodeList.AddNew();
			elem.ChargeCode = cc4.PK;
			AssertEquals("Value", value2, bizO.AccChargeCodeList.ToString());
		}

		public void TestAccChargeCodeListFilter()
		{
			AccChargeCodeListCollectionWrapper bizO = new AccChargeCodeListCollectionWrapper("", RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
			AssertEquals("AccChargeCodeList.Filter", RegistryFindBoxFilter.None, bizO.AccChargeCodeList.Filter);

			bizO = new AccChargeCodeListCollectionWrapper("", RegistryFindBoxFilter.FreightChargeCode, Factory, Env.CurrentCompany.PK);
			AssertEquals("AccChargeCodeList.Filter", RegistryFindBoxFilter.FreightChargeCode, bizO.AccChargeCodeList.Filter);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AccChargeCodeListCollectionWrapper("", RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
		}

		#endregion
	}
}
