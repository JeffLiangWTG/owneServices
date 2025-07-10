using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPECusHAWBConsignorMatchApproval))]
	public class UPECusHAWBConsignorMatchApprovalTest : UPEConsigneeConsignorMatchApprovalBaseTest
	{
		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPECusHAWBConsignorMatchApproval>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestOnMatchApproved()
		{
			CusHAWBConsignorMatchApproval matchApproval = (CusHAWBConsignorMatchApproval)GetNewBusinessObject();
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			AirCargo = (UPECusHAWB)matchApproval.Parent;
			Level1Record level1Record = new Level1Record();
			level1Record.AddRecordLine("US4196AU9639000626              DA15V04FXKC730000007201004A15V04    AVM SOFTWARE                       213WEST 35TH STREET,               402                                NEW YORK                                               NY10001    US 12125649997   12125630422                                                                                                    NALENI MCGA");
			AirCargo.Level1Record = level1Record;
			AirCargo.RequiresConsigneeMatch = true;
			AirCargo.UPEConsigneeAddress.P3_OH_MatchOrg = organisation.PK;
			AirCargo.CS_OA_ConsignorAddress = organisation.MainAddress.PK;
			Factory.Save();
			AssertEquals("12125649997", organisation.MainAddress.OA_Phone);
			AssertEquals("12125630422", organisation.MainAddress.OA_Fax);
		}

		#region Base Test Overrides
		protected override ZPropertyInfo ExpectedParentUNLOCOInfo
		{
			get
			{
				return AirCargo.CS_RL_NKOriginInfo;
			}
		}

		protected override void SetLevel1RecordOrganisation(Level1Record level1Record)
		{
			level1Record._300000 = new _300000Line("US4196AU9639000626              DA15V04FXKC730000007201004A15V04    AVM SOFTWARE                       213WEST 35TH STREET,               402                                NEW YORK                                               NY10001    US 12125649997   12125630422                                                                                                    NALENI MCGA");
		}

		protected override OrgMatchApproval GetNewOrgMatchApproval()
		{
			return Loader.LoadOrCreate(AirCargo.PK, OrgMatchApprovalType.AirCargoConsignor);
		}
		#endregion
	}
}
