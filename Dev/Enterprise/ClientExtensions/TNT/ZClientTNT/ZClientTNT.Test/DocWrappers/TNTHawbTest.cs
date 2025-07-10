using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.TNT.DocWrappers.Testing
{
	[TestedType(typeof(TNTHawb))]
	public class TNTHawbTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			CreateTNTAWB();
			return new DocumentWrapper[] { Doc };
		}

		public void TestAccountOrganisationNameAndNumber()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.DeliveredDutyPaid;
			AssertEquals("AccountOrganisationName should be 'LocalCharges', Shipment is Prepaid", "A/C: LocalCharges", Doc.AccountOrganisationName);
			AssertEquals("AccountNumber should be ' 1 2 3 4 5 6 7 8', Shipment is Prepaid", "1 2 3 4 5 6 7 8", Doc.AccountNumber);
			Header.LocalChargesPK = ZGuid.Empty;
			AssertEquals("AccountOrganisationName should be empty, LocalCharges is null", "", Doc.AccountOrganisationName);
			AssertEquals("AccountNumber should be empty, LocalCharges is null", "", Doc.AccountNumber);
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("AccountOrganisationName should be 'AgentCollect', Shipment is Collect", "A/C: AgentCollect", Doc.AccountOrganisationName);
			AssertEquals("AccountOrganisationName should be '8 7 6 5 4 3 2 1', Shipment is Collect", "8 7 6 5 4 3 2 1", Doc.AccountNumber);
			Header.AgentCollectPK = ZGuid.Empty;
			AssertEquals("AccountOrganisationName should be empty, AgentCollect is null", "", Doc.AccountOrganisationName);
			AssertEquals("AccountNumber should be empty, AgentCollect is null", "", Doc.AccountNumber);
		}

		public void TestHouseBill()
		{
			AssertEquals("HouseBIll should be 'G947063087'", "G947063087", Doc.HouseBill);
		}

		public void TestTNTHawbWhenJobHeaderIsNull()
		{
			Header.Delete();
			Factory.Save();
			AssertEquals("AccountNumber should be empty", ZString.Empty, Doc.AccountNumber);
		}

		ForwardingShipment Shipment;
		JobHeader Header;
		ExportAWBHeader AWB;
		TNTHawb Doc;
		void CreateTNTAWB()
		{
			Shipment = Factory.NewWithValidTestData<ForwardingShipment>(TestBusinessObjectKind.MinimumRequiredToSave);
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Shipment.JS_HouseBill = "G947063087";
			OrgHeader localCharges = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			localCharges.OH_FullName = "LocalCharges";
			OrgCusCode cusCode = localCharges.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			cusCode.OK_CustomsRegNo = "12345678";
			OrgHeader agentCollect = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			agentCollect.OH_FullName = "AgentCollect";
			cusCode = agentCollect.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			cusCode.OK_CustomsRegNo = "87654321";
			Header = Factory.NewJobForTesting<JobHeader>();
			Header.LocalChargesPK = localCharges.PK;
			Header.AgentCollectPK = agentCollect.PK;
			Header.JH_ParentID = Shipment.PK;
			Header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AWB = Shipment.AWBHeader;
			Doc = TNTHawb.New(AWB, Factory);
		}
	}
}
