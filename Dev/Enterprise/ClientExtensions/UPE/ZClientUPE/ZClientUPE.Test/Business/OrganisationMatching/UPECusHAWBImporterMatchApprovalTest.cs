using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPECusHAWBImporterMatchApproval))]
	public class UPECusHAWBImporterMatchApprovalTest : UPEConsigneeConsignorMatchApprovalBaseTest
	{
		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPECusHAWBImporterMatchApproval>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestOnMatchApproved()
		{
			CusHAWBImporterMatchApproval matchApproval = (CusHAWBImporterMatchApproval)GetNewBusinessObject();
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			AirCargo = (UPECusHAWB)matchApproval.Parent;
			Level1Record level1Record = new Level1Record();
			level1Record.AddRecordLine("US2795AU9639040422              DA4W82133K3G401000        8AU0596227WENDY PATERSON                     ContactName              GPO BOX 1609                       ABC                                SYDNEY                                                 NS2001     AU 123456789        987654321  09576XP334CAU09639169   173    LBS466                                237           ");
			AirCargo.Level1Record = level1Record;
			AirCargo.RequiresConsigneeMatch = true;
			AirCargo.UPEConsigneeAddress.P3_OH_MatchOrg = organisation.PK;
			AirCargo.CS_OH_Consignor = organisation.PK;
			matchApproval.ApproveMatchBySupervisor(organisation);
			AssertEquals("123456789", organisation.MainAddress.OA_Phone);
			AssertEquals("987654321", organisation.MainAddress.OA_Fax);
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
			level1Record._401000 = new _401000Line("US2795AU9639040422              DA4W82133K3G401000        A15V04    WENDY PATERSON                     ContactName              GPO BOX 1609                       ABC                                SYDNEY                                                 NS2001     AU 123456789        1212563042209576XP334CAU09639169   173    LBS466                                237           ");
		}

		protected override OrgMatchApproval GetNewOrgMatchApproval()
		{
			return Loader.LoadOrCreate(AirCargo.PK, OrgMatchApprovalType.AirCargoImporter);
		}
		#endregion
	}
}
