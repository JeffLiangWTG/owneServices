using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(TransportAndChargeCodeMappingRegistryBusinessObject))]
	public class TransportAndChargeCodeMappingRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase<TransportAndChargeCodeMappingRegistryBusinessObject>
	{
		public void TestProperties()
		{
			AssertNotNull(BizObj.TransportModes);
			Assert(BizObj.TransportModes.Count > 0);
			AssertNotNull(BizObj.ChargeCodes);
			AssertEquals(0, BizObj.ChargeCodes.Count);

			AccChargeCode chargeCode = BizObj.ChargeCodes.AddNew();
			chargeCode.AC_Code = "DAH";

			AssertEquals(1, BizObj.ChargeCodes.Count);
			AssertEquals("DAH", BizObj.ChargeCodes[0].AC_Code);

			TransportAndChargeCodeMappingRegistryBusinessObject bizObj = (TransportAndChargeCodeMappingRegistryBusinessObject)GetNewBusinessObject();
			bizObj.TransportModeCode = ZString.Empty;
			bizObj.ChargeCodePK = ZGuid.Empty;
			bizObj.NominalCostCode = ZString.Empty;
			bizObj.NominalRevenueCode = ZString.Empty;

			AssertEquals("Transport Mode Code should have errors", true, bizObj.TransportModeCodeInfo.HasErrors());
			AssertEquals("Charge Code PK should have errors", true, bizObj.ChargeCodePKInfo.HasErrors());
			AssertEquals("Nominal Cost Code should have errors", true, bizObj.NominalCostCodeInfo.HasErrors());
			AssertEquals("Nominal Revenue Code should have errors", true, bizObj.NominalRevenueCodeInfo.HasErrors());
		}

		#region Implementation
		protected override TransportAndChargeCodeMappingRegistryBusinessObject GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override TransportAndChargeCodeMappingRegistryBusinessObject GetBusinessObjectToSerialise()
		{
			BizObj.TransportModeCode = "SEA";
			BizObj.ChargeCodePK = TestHelper.FindOrCreateCharge("FRT").PK;
			BizObj.NominalCostCode = "BOB";
			BizObj.NominalRevenueCode = "BAZ";
			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TransportAndChargeCodeMappingRegistryBusinessObject(Factory);
		}
		#endregion

		SharedTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new SharedTestHelper(Factory)); }
		}
		SharedTestHelper testHelper;
	}
}
