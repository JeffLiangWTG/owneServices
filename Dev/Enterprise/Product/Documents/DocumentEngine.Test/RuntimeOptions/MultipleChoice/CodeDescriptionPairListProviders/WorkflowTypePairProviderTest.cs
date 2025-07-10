using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class WorkflowTypePairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new WorkflowTypePairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			CodeDescriptionPairList workflowTypeList = new CodeDescriptionPairList();

			foreach (WorkflowDescriptor module in WorkflowDescriptors.Instance.Values)
			{
				if (module.SupportsEventTracking)
				{
					workflowTypeList.AddPair(module.Code, module.Description.ToString());
				}
			}

			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), workflowTypeList);
		}

		public void TestEachWorkflowDescriptorIsCoveredInWorkflowExceptionsReport()
		{
			foreach (WorkflowDescriptor module in WorkflowDescriptors.Instance.Values)
			{
				if (module.SupportsEventTracking)
				{
					switch (module.Code)
					{
						case "CAE":
						case "ACI":
						case "CON":
						case "SHP":
						case "BRK":
						case "CIV":
						case "STM":
						case "TRN":
						case "LTL":
						case "ORD":
						case "ORL":
						case "SPA":
						case "CNT":
						case "WIN":
						case "WOU":
						case "WWO":
						case "WDO":
						case "WCR":
						case "WAJ":
						case "BOL":
						case "BKN":
						case "QTN":
						case "OPP":
						case "CAM":
						case "HRC":
						case "ISF":
						case "QBK":
						case "TBM":
						case "TBC":
						case "TBI":
						case "TBW":
						case "ORG":
						case "RNV":
						case "PNV":
						case "CSM":
						case "MAN":
						case "SCH":
						case "CNS":
						case "ACR":
						case "HAC":
						case "INQ":
						case "HLB":
						case "HHB":
						case "HVH":
						case "HVC":
						case "HVL":
						case "HVO":
						case "ELV":
						case "HRA":
						case "CSQ":
						case "TCW":
						case "SCR":
						case "COM":
						case "WKI":
						case "JPA":
						case "INB":
						case "PRD":
						case "WKP":
						case "CSH":
						case "CLL":
						case "PHW": // This type (ProcessHeader) should never be supported.
						case "STA": // This type (Tasks) should never be supported.
						case "PTW": // This type (Tasks) should never be supported.
						case "WTF":
						case "SAL":
						case "WSC":
						case "WPU":
						case "REC":
						case "PRO":
						case "DRW":
						case "SIM":
						case "WVO":
						case "CBW":
						case "COW":
						case "TRC":
						case "TRA":
						case "TRU":
						case "TDC":
						case "TDU":
						case "TLL":
						case "REF":
						case "POD":
						case "CTR":
						case "PCD":
						case "RCD":
						case "NCT":
						case "TLM":
						case "TRS":
						case "GLB":
						case "AMS":
						case "LTC":
						case "PKG":
						case "CEH": // This type is intended to be used in line trigger only. CusEntryHeader delegates IWorkflowProvider to BaseJobDeclaration
						case "AMW":
						case "CTJ":
						case "SAR":
						case "CST":
						case "ACA":
						case "DNC":
						case "CYD":
						case "CUL":
						case "SCU":
						case "AOG":
						case "GRP":
						case "PER": // What is the point in this test? It doesn't check the report at all.  It just checks this switch statement.
						case "SCO":
						case "GTF":
						case "UBR":
						case "PPA":
						case "PPT":
						case "PRC":
						case "RPA":
						case "RPN":
						case "RPT":
						case "RRC":
						case "SBK":
						case "GMM":
						case "CLH":
						case "CLP":
						case "YTU":
						case "YRA":
						case "YUS":
						case "YDH":
						case "YDL":
						case "YPH":
						case "YPL":
						case "HRQ":
						case "SHO":
						case "HRO":
						case "TSH":
						case "CRD":
						case "GCR":
						case "CXH":
						case "RPR":
						case "YRE":
						case "OCC":
						case "OCS":
						case "SRV":
						case "CXR": // This type is intended to be used in line trigger only. CusExitReport delegates IWorkflowProvider to CusExitHeader
						case "LPC":
						case "CGC":
						case "GBK":
						case "GBM":
						case "GGM":
						case "GVM":
						case "BFR":
						case "TPS":
						case "MWO":
						case "YAO":
						case "PDT":
						case "WLO":
						case "VOY":
						case "PRT":
						case "EMM":
						case "WCW":
							//Report_WorkflowExceptions already supports these types
							Assert("This is not an empty test...", true);
							break;
						default:
							Assert("New WorkflowDescriptor which SupportsEventTracking. Make sure the SQL function Report_WorkflowExceptions works for type: " + module.Code + ":" + module.Description, false);
							break;
					}
				}
			}
		}
	}
}
