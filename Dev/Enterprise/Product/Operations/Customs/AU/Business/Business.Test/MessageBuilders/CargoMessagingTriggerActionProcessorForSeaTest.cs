using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CargoMessagingTriggerActionProcessorForSeaTest : TestCaseWithFactory
	{
		public void TestSendMessagesWithoutErrors()
		{
			var oceanBill = GetNoMessageErrorsOceanBill();
			var house = oceanBill.HouseBills[0];

			var notify = new NotificationBuffer();
			((ITriggerActionMessagingSupporter)oceanBill).SendMessage(notify, "TV", "");

			AssertEquals(1, house.Messages.Count);
			AssertMultilineASCIIEquals("Log",
@"Generating Cargo messages for Job OBL3337
1 messages sucessfully created.", notify.AsString);

			Assert("Should not save the factory", !house.Messages[0].IsInDatabase);
		}

		public void TestSendMessagesWithErrors()
		{
			var postMaster = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var staff = postMaster.Staff.AddNew();
			staff.GS_EmailAddress = "tv@cargowise.com";

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var oceanBill = GetNoMessageErrorsOceanBill();
			var house = oceanBill.HouseBills[0];
			house.CA_RN_NKGoodsOrigin = string.Empty;
			house.Pivot[0].CV_PackageCount = 0;
			house.Pivot[0].Container.CN_ContainerMode = "XX";

			var notify = new NotificationBuffer();
			((ITriggerActionMessagingSupporter)oceanBill).SendMessage(notify, "TV", "");

			AssertEquals(0, house.Messages.Count);
			AssertMultilineASCIIEquals("Log",
@"Generating Cargo messages for Job OBL3337
0 messages sucessfully created, but errors or warnings occured on others:
 Error: H33337B/OBL3337:
Container Mode: The code you have selected is not in the list.
Package Count: Please enter a Package Count.
Goods Origin: Goods origin is required on house bill: H33337B", notify.AsString);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals("Errors or warnings have occurred when sending Cargo messages for Job OBL3337, please see the Event Log Walker Service Task log.", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
		}

		public void TestProcessNotAcceptableJob()
		{
			var oceanBill = GetNoMessageErrorsOceanBill();
			oceanBill.CB_GB = ZGuid.Empty;

			var processorJob = new SeaCargoProcessorJob(oceanBill);
			var notify = new NotificationBuffer();
			var processor = new HouseBillsCargoMessageProcessor(notify);
			processor.Process(processorJob);
			AssertMultilineASCIIEquals("Log",
@"Error: Error - CB_GB: Valid branch is required.", notify.AsString);
		}

		public void TestProcessWithoutErrors()
		{
			var oceanBill = GetNoMessageErrorsOceanBill();
			var house = oceanBill.HouseBills[0];

			var processorJob = new SeaCargoProcessorJob(oceanBill);
			var notify = new NotificationBuffer();
			var processor = new HouseBillsCargoMessageProcessor(notify);
			processor.Process(processorJob);
			AssertEquals(1, house.Messages.Count);
			AssertMultilineASCIIEquals("Log",
@"H33337B/OBL3337 created and message prepared for sending to customs.
Messages successfully sent.", notify.AsString);
		}

		public void TestProcessWithMessageErrors()
		{
			var oceanBill = GetNoMessageErrorsOceanBill();
			var house = oceanBill.HouseBills[0];
			house.CA_RN_NKGoodsOrigin = ZString.Empty;

			var processorJob = new SeaCargoProcessorJob(oceanBill);
			var notify = new NotificationBuffer();
			var processor = new HouseBillsCargoMessageProcessor(notify);
			processor.Process(processorJob);
			AssertEquals(0, house.Messages.Count);
			AssertMultilineASCIIEquals("Log",
@"Error: H33337B/OBL3337:
Goods Origin: Goods origin is required on house bill: H33337B", notify.AsString);
		}

		CusSCAOceanBill GetNoMessageErrorsOceanBill()
		{
			var result = Factory.New<CusSCAOceanBill>();
			result.CB_LloydsIMO = "7619410";
			result.CB_Voyage = "3337";
			result.CB_PrincipalID = "41083962136";
			result.CB_ResponsiblePartyID = "41065894724";
			result.CB_RL_NKPortOfLoading = "GBLON";
			result.CB_RL_NKPortOfDischarge = "AUSYD";
			result.CB_OceanBill = "OBL3337";

			var container = result.Containers.AddNew();
			container.CN_ContainerNumber = "OCLU1233401";
			container.CN_SealNumber = "2345";
			container.CN_ContainerMode = "LCL";
			container.CN_RC_NKContainerType = "20GP";

			var house = result.HouseBills.AddNew();
			house.CA_HouseBill = "H33337B";
			house.CA_RL_NK_PortOfOrigin = "GBLON";
			house.CA_RL_NK_PortOfDestination = "AUSYD";
			house.CA_RN_NKGoodsOrigin = "GB";
			house.CA_PrepaidCollectOther = "CC";
			house.CA_ConsigneeName = "TREETOYS PTY LTD";
			house.CA_ConsigneeAddress1 = "105 WOMBAT DRIVE";
			house.CA_ConsigneeSuburb = "KATOOMBA";
			house.CA_ConsigneePostcode = "2780";
			house.CA_RN_NKConsigneeCountryCode = "AU";
			house.CA_ConsigneePhone = "+61290251100";
			house.CA_ConsigneeFax = "+61290251199";
			house.CA_ConsignorName = "MICROLOG GMBH";
			house.CA_ConsignorAddress1 = "STUTTGARTER STRASSE 45-51";
			house.CA_ConsignorSuburb = "NECKARTENZLINGEN";
			house.CA_ConsignorPostcode = "72654";
			house.CA_RN_NKConsignorCountryCode = "DE";

			var packing = container.Pivots.AddNew();
			packing.CV_CA = house.PK;
			packing.CV_PackageCount = 1;
			packing.CV_PackageType = "PF";
			packing.CV_NetWeight = 1m;
			packing.CV_Weight = 1;
			packing.CV_WeightUQ = "KG";
			packing.CV_Volume = 1;
			packing.CV_GoodsDescription = "STUFF";
			packing.CV_MarksAndNumbers = "XXXXXXXXXXXXX";

			return result;
		}
	}
}
