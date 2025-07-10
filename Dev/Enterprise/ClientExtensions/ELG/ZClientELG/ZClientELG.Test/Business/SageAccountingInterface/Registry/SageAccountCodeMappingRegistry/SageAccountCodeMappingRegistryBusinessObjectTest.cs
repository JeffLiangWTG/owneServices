using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.ELG.Testing
{
	[TestedType(typeof(SageAccountCodeMappingRegistryBusinessObject))]
	public class SageAccountCodeMappingRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase<SageAccountCodeMappingRegistryBusinessObject>
	{
		public void TestProperties()
		{
			AssertNotNull(BizObj.Organisations);
			AssertEquals(0, BizObj.Organisations.Count);
			AssertNotNull(BizObj.Currencies);
			Assert(BizObj.Currencies.Count > 0);
			OrgHeader orgHeader = BizObj.Organisations.AddNew();
			orgHeader.OH_Code = "DAH";
			AssertEquals(1, BizObj.Organisations.Count);
			AssertEquals("DAH", BizObj.Organisations[0].OH_Code);
			SageAccountCodeMappingRegistryBusinessObject bizObj = (SageAccountCodeMappingRegistryBusinessObject)GetNewBusinessObject();
			bizObj.OrgHeaderPK = ZGuid.Empty;
			bizObj.RefCurrencyPK = ZGuid.Empty;
			bizObj.LedgerType = ZString.Empty;
			bizObj.SageAccountCode = ZString.Empty;
			AssertEquals("Organisation Header PK should have errors", true, bizObj.OrgHeaderPKInfo.HasErrors());
			AssertEquals("Reference Currency PK should have errors", true, bizObj.RefCurrencyPKInfo.HasErrors());
			AssertEquals("Ledger Type should have errors", true, bizObj.LedgerTypeInfo.HasErrors());
			AssertEquals("Sage Account Code should have errors", true, bizObj.SageAccountCodeInfo.HasErrors());
		}

		#region Implementation
		protected override SageAccountCodeMappingRegistryBusinessObject GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override SageAccountCodeMappingRegistryBusinessObject GetBusinessObjectToSerialise()
		{
			BizObj.RefCurrencyPK = TestHelper.FindCurrency(Core.Constants.CurrencyCodes.Australia).PK;
			BizObj.OrgHeaderPK = TestHelper.FindOrCreateCharge("FRT").PK;
			BizObj.LedgerType = "AR";
			BizObj.SageAccountCode = "BAZ";
			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get
			{
				return true;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SageAccountCodeMappingRegistryBusinessObject(Factory);
		}

		#endregion
		ELGTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new ELGTestHelper(Factory));
			}
		}

		ELGTestHelper testHelper;
	}
}
