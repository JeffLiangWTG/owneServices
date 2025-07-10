using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusOutturnHeaderValidationTest : Customs.Business.Testing.CusOutturnHeaderValidationTest
	{
		public void TestVesselName()
		{
			header.C6_LloydsIMO = "foo";
			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Code = "Code1";
			refVessel.RV_LloydsNumber = "foo";
			Factory.Save();

			header.Validation.ValidateC6_VesselName();
			AssertNoWarning(header.C6_VesselNameInfo, "Vessel is not on file.");

			header.C6_VesselName = "Code1";
			AssertNoWarning(header.C6_VesselNameInfo, "Vessel is not on file.");

			header.C6_LloydsIMO = "";
			AssertNoWarning(header.C6_VesselNameInfo, "Vessel is not on file.");

			header.C6_LloydsIMO = "foo2";
			AssertHasWarning(header.C6_VesselNameInfo, "Vessel is not on file.");

			header.C6_LloydsIMO = "foo";
			header.C6_VesselName = "Code2";
			AssertHasWarning(header.C6_VesselNameInfo, "Vessel is not on file.");
		}

		public void TestLloyds()
		{
			header.C6_LloydsIMO = ZString.Empty;
			AssertHasErrors("by default", header.C6_LloydsIMOInfo);

			header.C6_LloydsIMO = "foo";
			AssertNoNotifications("when set", header.C6_LloydsIMOInfo);
		}

		public void TestPremiseID()
		{
			header.C6_OutturningPremiseID = ZString.Empty;
			AssertHasErrors("by default", header.C6_OutturningPremiseIDInfo);

			header.C6_OutturningPremiseID = "foo";
			AssertNoNotifications("when set", header.C6_OutturningPremiseIDInfo);
		}

		public void TestVoyageNum()
		{
			header.C6_VoyageNum = ZString.Empty;
			AssertHasErrors("by default", header.C6_VoyageNumInfo);

			header.C6_VoyageNum = "foo";
			AssertNoNotifications("when set", header.C6_VoyageNumInfo);
		}

		public void TestCheckC6_OutturningPremiseID()
		{
			ZString oldRegNum = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
			try
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "12345";
				EDIInterchange interchange = Factory.New<EDIInterchange>();
				interchange.EI_InterchangeNum = "555";
				interchange.EI_From = "12345";
				CMRSEAOUTMessage outturnMessage = Factory.New<CMRSEAOUTMessage>();
				outturnMessage.EM_Status = EDIMessage.Status.Sent;
				outturnMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
				outturnMessage.EM_EI = interchange.PK;
				header.Messages.Add(outturnMessage);
				header.Validation.ValidateC6_OutturningPremiseID();
				AssertNoMessageErrorContaining(header.C6_OutturningPremiseIDInfo, "This outturn was originally created under a different Customs Site Id to that specified in the current company's Customs Reg No field.");
				interchange.EI_From = "12345X";
				header.Validation.ValidateC6_OutturningPremiseID();
				AssertHasMessageErrorContaining(header.C6_OutturningPremiseIDInfo, "This outturn was originally created under a different Customs Site Id to that specified in the current company's Customs Reg No field.");
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = oldRegNum;
			}
		}

		public void TestUniqueness()
		{
			header.C6_VoyageNum = "123";
			header.C6_LloydsIMO = "54321";
			header.C6_OutturningPremiseID = "9914N";

			header.Validation.ValidateAll();

			AssertNoNotifications("when set", header.C6_LloydsIMOInfo);
			AssertNoNotifications("when set", header.C6_OutturningPremiseIDInfo);
			AssertNoNotifications("when set", header.C6_VoyageNumInfo);

			CusOutturnHeader header2 = CusOutturnHeader.New(Factory);
			header2.C6_VoyageNum = "123";

			AssertNoNotifications("when set", header.C6_VoyageNumInfo);
			AssertNoNotifications("when set", header2.C6_VoyageNumInfo);

			header2.C6_LloydsIMO = "54321";

			AssertNoNotifications("when set", header.C6_LloydsIMOInfo);
			AssertNoNotifications("when set", header2.C6_LloydsIMOInfo);

			header2.C6_OutturningPremiseID = "9914N";

			header.Validation.ValidateAll();
			header2.Validation.ValidateAll();

			AssertHasErrors("when duplicate unique details", header.C6_LloydsIMOInfo);
			AssertHasErrors("when duplicate unique details", header.C6_OutturningPremiseIDInfo);
			AssertHasErrors("when duplicate unique details", header.C6_VoyageNumInfo);
			AssertHasErrors("when duplicate unique details", header2.C6_LloydsIMOInfo);
			AssertHasErrors("when duplicate unique details", header2.C6_OutturningPremiseIDInfo);
			AssertHasErrors("when duplicate unique details", header2.C6_VoyageNumInfo);
		}

		public void TestChildUnique()
		{
			var outturn = header.Outturns.AddNew();

			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			outturn.C5_ContainerNumber = "OCLU8911239";
			outturn.C5_MasterBill = ZString.Empty;
			outturn.C5_HouseBill = ZString.Empty;

			var otherOutturn = header.Outturns.AddNew();

			header.Validation.ValidateChildUnique();

			AssertNoRowErrors(otherOutturn);
			AssertNoRowErrors(outturn);

			otherOutturn.C5_CargoType = outturn.C5_CargoType;
			otherOutturn.C5_MasterBill = outturn.C5_MasterBill;
			otherOutturn.C5_HouseBill = outturn.C5_HouseBill;
			otherOutturn.C5_ContainerNumber = outturn.C5_ContainerNumber;

			header.Validation.ValidateChildUnique();

			AssertHasRowError(otherOutturn, CusOutturnHeaderValidation.messageError2);
			AssertHasRowError(outturn, CusOutturnHeaderValidation.messageError2);

			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills;

			header.Validation.ValidateChildUnique();

			AssertNoRowErrors(otherOutturn);
			AssertNoRowErrors(outturn);

			var otherFCXOutturn1 = header.Outturns.AddNew();
			otherFCXOutturn1.C5_CargoType = outturn.C5_CargoType;
			otherFCXOutturn1.C5_MasterBill = outturn.C5_MasterBill;
			otherFCXOutturn1.C5_HouseBill = outturn.C5_HouseBill;
			otherFCXOutturn1.C5_ContainerNumber = outturn.C5_ContainerNumber;

			header.Validation.ValidateChildUnique();
			AssertHasRowError(otherFCXOutturn1, CusOutturnHeaderValidation.messageError2);

			header.Validation.ValidateChildUnique();
			AssertHasRowError(outturn, CusOutturnHeaderValidation.messageError2);

			otherFCXOutturn1.C5_HouseBill = "11111";

			header.Validation.ValidateChildUnique();
			AssertNoRowErrors(otherFCXOutturn1);

			var otherFCXOutturn2 = header.Outturns.AddNew();
			otherFCXOutturn2.C5_CargoType = otherFCXOutturn1.C5_CargoType;
			otherFCXOutturn2.C5_MasterBill = otherFCXOutturn1.C5_MasterBill;
			otherFCXOutturn2.C5_HouseBill = otherFCXOutturn1.C5_HouseBill;
			otherFCXOutturn2.C5_ContainerNumber = otherFCXOutturn1.C5_ContainerNumber;

			header.Validation.ValidateChildUnique();
			AssertHasRowError(otherFCXOutturn2, CusOutturnHeaderValidation.messageError1);

			header.Validation.ValidateChildUnique();
			AssertHasRowError(otherFCXOutturn1, CusOutturnHeaderValidation.messageError1);

			otherFCXOutturn1.Delete();
			otherFCXOutturn2.Delete();

			outturn.C5_HouseBill = "AAA";
			otherOutturn.C5_HouseBill = "AAA";

			header.Validation.ValidateChildUnique();
			AssertNoRowErrors(otherOutturn);
			AssertNoRowErrors(outturn);

			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;

			header.Validation.ValidateChildUnique();
			AssertHasRowError(otherOutturn, CusOutturnHeaderValidation.messageError1);
			AssertHasRowError(outturn, CusOutturnHeaderValidation.messageError1);

			outturn.C5_MessageStatus = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived;

			header.Validation.ValidateChildUnique();
			AssertHasRowError(otherOutturn, CusOutturnHeaderValidation.messageError1);
			AssertHasRowError(outturn, CusOutturnHeaderValidation.messageError1);

			Assert(!outturn.ReadOnly);
			Assert(!otherOutturn.ReadOnly);

			outturn.C5_MessageStatus = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived;

			header.Validation.ValidateChildUnique();
			AssertNoRowError(otherOutturn, CusOutturnHeaderValidation.messageError1);
			AssertNoRowError(outturn, CusOutturnHeaderValidation.messageError1);

			Assert(outturn.ReadOnly);
			Assert(!otherOutturn.ReadOnly);
		}

		public void TestChildUnique_ConcurrentSaving()
		{
			Factory.Save();
			var outturn = header.Outturns.AddNew();

			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			outturn.C5_ContainerNumber = "OCLU8911239";
			outturn.C5_MasterBill = ZString.Empty;
			outturn.C5_HouseBill = ZString.Empty;

			Factory.Save();

			CombineAssertions(() =>
			{
				header.Validation.ValidateChildUnique();
				AssertNoRowErrors("No error for non-duplicate row", outturn);

				var otherFactory = new BusinessObjectFactory();
				otherFactory.RefreshEnabled = false;
				var otherOutturn = otherFactory.New<CusOutturn>();
				otherOutturn.C5_C6 = outturn.C5_C6;
				otherOutturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
				otherOutturn.C5_ContainerNumber = "OCLU8911239";
				otherOutturn.C5_MasterBill = ZString.Empty;
				otherOutturn.C5_HouseBill = ZString.Empty;

				var irrelevantOutturn = otherFactory.New<CusOutturn>();
				irrelevantOutturn.C5_C6 = outturn.C5_C6;

				otherFactory.Save();

				header.Validation.ValidateChildUnique();
				var loadedOutturn = header.Outturns.SingleOrDefault(_ => _.PK == otherOutturn.PK);
				AssertNotNull("Newly added row with conflict is loaded", loadedOutturn);
				AssertNotNull("Newly added row with no conflict is loaded", header.Outturns.SingleOrDefault(_ => _.PK == irrelevantOutturn.PK));
				AssertHasRowError("Error should be on the added row loaded out of DB", loadedOutturn, "This duplicate row was added by others. Please confirm and remove any duplicates.");

				header.Validation.ValidateChildUnique();
				AssertHasRowError("Validate again and the loaded row has normal error", loadedOutturn, CusOutturnHeaderValidation.messageError2);
				AssertHasRowError("Validate again and the edited row has normal error", outturn, CusOutturnHeaderValidation.messageError2);

				Db.Connection.ExecuteNonQuery($@"INSERT INTO dbo.CusOutturn
(C5_PK, C5_C6, C5_CargoType, C5_ContainerNumber, C5_SystemCreateTimeUTC, C5_SystemCreateUser, C5_SystemLastEditTimeUTC, C5_SystemLastEditUser)
VALUES
('9b28876e-85ab-4913-9a17-91dca4cec281', '{outturn.C5_C6}', 'FCL', 'OCLU8911239', CAST('2079-06-06 23:59:00' AS smalldatetime), '~BP', CAST('2079-06-06 23:59:00' AS smalldatetime), '~BP')");

				header.Validation.ValidateChildUnique();
				loadedOutturn = header.Outturns.SingleOrDefault(_ => _.PK == new ZGuid("9b28876e-85ab-4913-9a17-91dca4cec281"));
				AssertHasRowError("Error should be on the added row regardless of factory cache", loadedOutturn, "This duplicate row was added by others. Please confirm and remove any duplicates.");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = CusOutturnHeader.New(Factory);
		}

		CusOutturnHeader header;
	}
}
