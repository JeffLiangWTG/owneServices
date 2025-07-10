using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ChargeCodeMappingRegistryBusinessObject))]
	public class ChargeCodeMappingRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase<ChargeCodeMappingRegistryBusinessObject>
	{
		public void TestProperties()
		{
			AssertNotNull(BizObj.ChargeCodes);
			AssertEquals(0, BizObj.ChargeCodes.Count);

			AccChargeCode chargeCode = BizObj.ChargeCodes.AddNew();
			chargeCode.AC_Code = "BLA";

			AssertEquals(1, BizObj.ChargeCodes.Count);
			AssertEquals("BLA", BizObj.ChargeCodes[0].AC_Code);

			ChargeCodeMappingRegistryBusinessObject bizObj = (ChargeCodeMappingRegistryBusinessObject)GetNewBusinessObject();
			bizObj.CodePK = ZGuid.Empty;
			bizObj.ExternalCode = ZString.Empty;

			AssertEquals("Debtor should have errors", true, bizObj.CodePKInfo.HasErrors());
			AssertEquals("ExternalCode should have errors", true, bizObj.ExternalCodeInfo.HasErrors());
		}

		#region Implementation
		protected override ChargeCodeMappingRegistryBusinessObject GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override ChargeCodeMappingRegistryBusinessObject GetBusinessObjectToSerialise()
		{
			BizObj.CodePK = GetChargeCodeToTest().PK;
			BizObj.ExternalCode = "BOB";
			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return true;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ChargeCodeMappingRegistryBusinessObject(Factory);
		}

		AccChargeCode GetChargeCodeToTest()
		{
			return Factory.Load<AccChargeCode>(new ZGuid("8319278C-E149-4895-BC52-114E69E069D9"));  // AC_Code = "FRT"
		}
		#endregion
	}
}
