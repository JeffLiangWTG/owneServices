using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaTax))]
	public class AsycudaTaxTest : EnterpriseBusinessObjectTestCase
	{
		[TestedType(typeof(AsycudaTax))]
		public class AsycudaTaxBaseTest : CargoWise.EntityFramework.Testing.BusinessObjectBaseTestCase
		{
			protected override BusinessObject GetNewBusinessObject() => Factory.New<AsycudaTax>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();
			return asycudaTax;
		}

		public void TestLookups()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();
			AssertType<AsycudaTaxLookups>(asycudaTax.Lookups);
		}

		public void TestDecimalPlaces()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();
			AssertHasCustomAttribute<DecimalPlacesAttribute>(asycudaTax.GetType(), "AET_ChargeAmount", false, attr => attr.DecimalPlaces == 2);
		}

		public void TestAET_TypeDescription()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();
			AssertEquals("AET_TypeDescription", ZString.Empty, asycudaTax.AET_TypeDescription);

			asycudaTax.AET_ChargeType = ChargeCodeList.Codes._001;
			AssertEquals("AET_TypeDescription", ChargeCodeList.Descriptions._001, asycudaTax.AET_TypeDescription);
		}

		public void TestSetDefaultValues()
		{
			var tax = (AsycudaTax)GetNewBusinessObjectForDeleteTest(Factory);
			AssertEquals("AET_MethodOfCalculation default value", tax.AET_MethodOfCalculation, "%");
			AssertEquals("AET_IsCharge default value", tax.AET_IsCharge, true);
		}

		AsycudaTax SetupTestEnvironmentForAsycudaTax()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.AsycudaTaxes.AddNew();
		}
	}
}
