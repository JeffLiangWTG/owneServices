using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class CASSChargeCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCASSTypeList()
		{
			AssertEquals("CASSTypeList.Count", 3, BizObj.CASSTypesList.Count);
			AssertEquals("CASSTypeList should contain 'ALL'", true, BizObj.CASSTypesList.ContainsCode("ALL"));
			AssertEquals("CASSTypeList should contain 'CASS Export'", true, BizObj.CASSTypesList.ContainsCode(CASSChargeCodeLookups.CASSTypes.Export));
			AssertEquals("CASSTypeList should contain 'CASS Import'", true, BizObj.CASSTypesList.ContainsCode(CASSChargeCodeLookups.CASSTypes.Import));
		}

		public void TestCASSLineComponentList()
		{
			AssertEquals("CASSLineComponentList.Count", 1, BizObj.CASSLineComponentList.Count);
			AssertEquals("CASSLineComponentList should contain 'ALL'", true, BizObj.CASSLineComponentList.ContainsCode("ALL"));

			CASSCode.CASSType = CASSChargeCodeLookups.CASSTypes.Export.Code;
			AssertEquals("CASSLineComponentList.Count", 7, BizObj.CASSLineComponentList.Count);
			AssertEquals("CASSLineComponentList should contain 'ALL'", true, BizObj.CASSLineComponentList.ContainsCode("ALL"));
			AssertEquals("CASSLineComponentList should contain 'PWC'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge));
			AssertEquals("CASSLineComponentList should contain 'PVC'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge));
			AssertEquals("CASSLineComponentList should contain 'PCC'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier));
			AssertEquals("CASSLineComponentList should contain 'COA'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport));
			AssertEquals("CASSLineComponentList should contain 'COM'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.Commission));
			AssertEquals("CASSLineComponentList should contain 'DOM'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.Discount));

			CASSCode.CASSType = CASSChargeCodeLookups.CASSTypes.Import.Code;
			AssertEquals("CASSLineComponentList.Count", 10, BizObj.CASSLineComponentList.Count);
			AssertEquals("CASSLineComponentList should contain 'ALL'", true, BizObj.CASSLineComponentList.ContainsCode("ALL"));
			AssertEquals("CASSLineComponentList should contain 'WVC'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.WeightOrValuationCharge));
			AssertEquals("CASSLineComponentList should contain 'CCA'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentImport));
			AssertEquals("CASSLineComponentList should contain 'CCC'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheCarrier));
			AssertEquals("CASSLineComponentList should contain 'COF'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.CollectFee));
			AssertEquals("CASSLineComponentList should contain 'HDC'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.HandlingCharges));
			AssertEquals("CASSLineComponentList should contain 'STC'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.StorageCharges));
			AssertEquals("CASSLineComponentList should contain 'OC1'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.OtherCharge1));
			AssertEquals("CASSLineComponentList should contain 'OC2'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.OtherCharge2));
			AssertEquals("CASSLineComponentList should contain 'MCA'", true, BizObj.CASSLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.MiscellaneousChargesAmount));
		}

		public void TestCASSExportComponentCodeList()
		{
			AssertEquals("CASSExportLineComponentList.Count", 6, CASSChargeCodeLookups.CASSExportLineComponentList.Count);
			AssertEquals("CASSExportLineComponentList should contain 'PWC'", true, CASSChargeCodeLookups.CASSExportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge));
			AssertEquals("CASSExportLineComponentList should contain 'PVC'", true, CASSChargeCodeLookups.CASSExportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge));
			AssertEquals("CASSExportLineComponentList should contain 'PCC'", true, CASSChargeCodeLookups.CASSExportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier));
			AssertEquals("CASSExportLineComponentList should contain 'COA'", true, CASSChargeCodeLookups.CASSExportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport));
			AssertEquals("CASSExportLineComponentList should contain 'COM'", true, CASSChargeCodeLookups.CASSExportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.Commission));
			AssertEquals("CASSExportLineComponentList should contain 'DOM'", true, CASSChargeCodeLookups.CASSExportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.Discount));
		}

		public void TestCASSImportComponentCodeList()
		{
			AssertEquals("CASSImportLineComponentList.Count", 9, CASSChargeCodeLookups.CASSImportLineComponentList.Count);
			AssertEquals("CASSImportLineComponentList should contain 'WVC'", true, CASSChargeCodeLookups.CASSImportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.WeightOrValuationCharge));
			AssertEquals("CASSImportLineComponentList should contain 'CCA'", true, CASSChargeCodeLookups.CASSImportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentImport));
			AssertEquals("CASSImportLineComponentList should contain 'CCC'", true, CASSChargeCodeLookups.CASSImportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheCarrier));
			AssertEquals("CASSImportLineComponentList should contain 'COF'", true, CASSChargeCodeLookups.CASSImportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.CollectFee));
			AssertEquals("CASSImportLineComponentList should contain 'HDC'", true, CASSChargeCodeLookups.CASSImportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.HandlingCharges));
			AssertEquals("CASSImportLineComponentList should contain 'STC'", true, CASSChargeCodeLookups.CASSImportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.StorageCharges));
			AssertEquals("CASSImportLineComponentList should contain 'OC1'", true, CASSChargeCodeLookups.CASSImportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.OtherCharge1));
			AssertEquals("CASSImportLineComponentList should contain 'OC2'", true, CASSChargeCodeLookups.CASSImportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.OtherCharge2));
			AssertEquals("CASSImportLineComponentList should contain 'MCA'", true, CASSChargeCodeLookups.CASSImportLineComponentList.ContainsCode(CASSChargeCodeLookups.CASSComponents.MiscellaneousChargesAmount));
		}

		CASSChargeCodeLookups BizObj
		{
			get
			{
				if (bizObj == null)
				{
					bizObj = new CASSChargeCodeLookups(CASSCode);
				}
				return bizObj;
			}
		}
		CASSChargeCodeLookups bizObj;

		CASSChargeCode CASSCode
		{
			get
			{
				if (cassCode == null)
				{
					cassCode = new CASSChargeCode();
				}
				return cassCode;
			}
		}
		CASSChargeCode cassCode;
	}
}
