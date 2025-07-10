using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPECusHAWBConsigneeMatchApproval))]
	public class UPECusHAWBConsigneeMatchApprovalTest : UPEConsigneeConsignorMatchApprovalBaseTest
	{
		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPECusHAWBConsigneeMatchApproval>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestOnMatchApproved()
		{
			CusHAWBConsigneeConsignorMatchApproval matchApproval = (CusHAWBConsigneeConsignorMatchApproval)GetNewBusinessObject();
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			AirCargo = (UPECusHAWB)matchApproval.Parent;
			Level1Record level1Record = new Level1Record();
			SetLevel1RecordOrganisation(level1Record);
			AirCargo.Level1Record = level1Record;
			AirCargo.CS_OH_Consignor = organisation.PK;
			AirCargo.RequiresConsigneeMatch = true;
			AirCargo.UPEConsigneeAddress.P3_OH_MatchOrg = organisation.PK;
			AssertEquals("0738831842", organisation.MainAddress.OA_Phone);
			AssertEquals("12125630422", organisation.MainAddress.OA_Fax);
		}

		public void TestDontSetCusHAWBForeignKeyOnMatchApproved()
		{
			CusHAWB hAWB = Factory.New<CusHAWB>();
			OrgMatchApproval matchApproval = new OrgMatchApproval.Loader(Factory).LoadOrCreate(hAWB.PK, OrgMatchApprovalType.AirCargoConsignee);
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			matchApproval.ApproveMatchBySupervisor(consignee);
			AssertEquals("CS_OH_Consignee should remain empty, the Consignee details on the CusHAWB must store the delivery details", ZGuid.Empty, hAWB.CS_OH_Consignee);
			AssertEquals("Matched organisation should be stored in OrgPatternMatchAddress.P3_OH_MatchOrg", consignee.PK, matchApproval.AddressToBeMatched.P3_OH_MatchOrg);
		}

		#region Base Test Overrides
		protected override ZPropertyInfo ExpectedParentUNLOCOInfo
		{
			get
			{
				return AirCargo.CS_RL_NKDestinationInfo;
			}
		}

		protected override void SetLevel1RecordOrganisation(Level1Record level1Record)
		{
			level1Record._400000 = new _400000Line("US4196AU9639000626              DA15V04FXKC7400000        0000A15V04DAVID TASKER                       DAVID TASKER             18 WENDY CRESENT                   XYZ                                QUEENSLAND                                             VI4019     AU 0738831842    12125630422                                                                        000           ");
		}

		protected override OrgMatchApproval GetNewOrgMatchApproval()
		{
			return Loader.LoadOrCreate(AirCargo.PK, OrgMatchApprovalType.AirCargoConsignee);
		}
		#endregion
	}
}
