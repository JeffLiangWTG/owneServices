using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AccChargeCodeListElement))]
	sealed class AccChargeCodeListElementTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestAccChargeCodeCollection()
		{
			var anotherCompany = Factory.New<GlbCompany>();

			var chargeCode1 = Factory.New<AccChargeCode>();
			var chargeCode2 = Factory.New<AccChargeCode>();
			var chargeCode3 = Factory.New<AccChargeCode>();

			chargeCode1.AC_GC = Env.CurrentCompany.PK;
			chargeCode2.AC_GC = Env.CurrentCompany.PK;
			chargeCode3.AC_GC = anotherCompany.PK;

			chargeCode1.AC_ChargeGroup = "FRT";
			chargeCode2.AC_ChargeGroup = "ABC";
			chargeCode3.AC_ChargeGroup = "XYZ";

			var wrapper = new AccChargeCodeListCollectionWrapper("", RegistryFindBoxFilter.FreightChargeCode, Factory, Env.CurrentCompany.PK);
			var element = wrapper.AccChargeCodeList.AddNew();

			((AccChargeCodeCollection)element.AccChargeCodeCollection).Load();
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode1)", true, element.AccChargeCodeCollection.Contains(chargeCode1));
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode2)", false, element.AccChargeCodeCollection.Contains(chargeCode2));
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode3)", false, element.AccChargeCodeCollection.Contains(chargeCode3));
		}

		public void TestAccChargeCodeListElement()
		{
			var charge = Factory.NewWithValidTestData<AccChargeCode>();
			charge.AC_Desc = "ChargeDescription";

			var elem = (AccChargeCodeListElement)GetNewBusinessObject();
			elem.ChargeCode = charge.PK;

			AssertEquals("ChargeCode", charge.PK, elem.ChargeCode);
			AssertEquals("ChargeDescription", "ChargeDescription", elem.ChargeDescription);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			AccChargeCodeListCollection collection = new AccChargeCodeListCollection("", RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
			AccChargeCodeListElement bizO = new AccChargeCodeListElement(ZGuid.Empty, collection);
			return bizO;
		}

		#endregion
	}
}
