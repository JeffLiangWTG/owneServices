using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusOutturnHeader))]
	sealed class CusOutturnHeaderTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		public void TestSeaCargoOutturnMessages()
		{
			var cusOutturnHeader = Factory.New<CusOutturnHeader>();
			var ediMessage = Factory.New<EDIMessageForTest>();
			ediMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
			ediMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			ediMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			ediMessage.EM_ReceiveTransmit = "TRX";
			cusOutturnHeader.Messages.Add(ediMessage);

			AssertCollectionContains(ediMessage, cusOutturnHeader.Messages);
			AssertCollectionNotContains("XUS message should NOT be in the Outturn Message collection", ediMessage, cusOutturnHeader.SeaCargoOutturnMessages);
		}

		public void TestSetterSuspender()
		{
			var aplVessel = Factory.New<RefVessel>();
			aplVessel.RV_Code = "VESSEL";
			aplVessel.RV_LloydsNumber = "9832343";
			aplVessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Australia;
			aplVessel.RV_RadioCallSign = "CALLME";

			var primiseAddress = MasterFiles.DataTransfer.Universal.Testing.OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory);
			primiseAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "CP123", GlbCompany.CurrentCompany.Country.Code);
			Factory.Save();

			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_LloydsIMO = "9832343";
			outturnHeader.C6_VesselName = "VESSEL";
			outturnHeader.C6_OutturningPremiseID = "CP123";
			outturnHeader.C6_OA_OutturningPremise = primiseAddress.MainAddress.PK;
			Factory.Save();

			outturnHeader.SetterSuspender.SuspendSetting(AutoCusOutturnHeader.Schema.C6_OutturningPremiseID,
														AutoCusOutturnHeader.Schema.C6_OA_OutturningPremise,
														AutoCusOutturnHeader.Schema.C6_VesselName,
														AutoCusOutturnHeader.Schema.C6_LloydsIMO);

			outturnHeader.C6_LloydsIMO = "ZZZ";
			outturnHeader.C6_VesselName = "ZZZ";
			outturnHeader.C6_OutturningPremiseID = "ZZZ";
			outturnHeader.C6_OA_OutturningPremise = ZGuid.Empty;
			Factory.Save();

			AssertEquals("C6_LloydsIMO:setter suspended", true, outturnHeader.SetterSuspender.IsSetterSuspended(AutoCusOutturnHeader.Schema.C6_LloydsIMO));
			AssertEquals("C6_LloydsIMO:not set new value", "9832343", outturnHeader.C6_LloydsIMO);
			AssertEquals("C6_VesselName:setter suspended", true, outturnHeader.SetterSuspender.IsSetterSuspended(AutoCusOutturnHeader.Schema.C6_VesselName));
			AssertEquals("C6_VesselName:not set new value", "VESSEL", outturnHeader.C6_VesselName);
			AssertEquals("C6_OA_OutturningPremise:setter suspended", true, outturnHeader.SetterSuspender.IsSetterSuspended(AutoCusOutturnHeader.Schema.C6_OA_OutturningPremise));
			AssertEquals("C6_OA_OutturningPremise:not set new value", primiseAddress.MainAddress.PK, outturnHeader.C6_OA_OutturningPremise);
			AssertEquals("C6_OutturningPremiseID:setter suspended", true, outturnHeader.SetterSuspender.IsSetterSuspended(AutoCusOutturnHeader.Schema.C6_OutturningPremiseID));
			AssertEquals("C6_OutturningPremiseID:not set new value", "CP123", outturnHeader.C6_OutturningPremiseID);

			outturnHeader.SetterSuspender.ResumeSetting(AutoCusOutturnHeader.Schema.C6_OutturningPremiseID,
											AutoCusOutturnHeader.Schema.C6_OA_OutturningPremise,
											AutoCusOutturnHeader.Schema.C6_VesselName,
											AutoCusOutturnHeader.Schema.C6_LloydsIMO);

			outturnHeader.C6_LloydsIMO = "ZZZ";
			outturnHeader.C6_VesselName = "ZZZ";
			outturnHeader.C6_OutturningPremiseID = "ZZZ";
			outturnHeader.C6_OA_OutturningPremise = ZGuid.Empty;
			Factory.Save();

			AssertEquals("C6_LloydsIMO:setter suspended", false, outturnHeader.SetterSuspender.IsSetterSuspended(AutoCusOutturnHeader.Schema.C6_LloydsIMO));
			AssertEquals("C6_LloydsIMO:not set new value", "ZZZ", outturnHeader.C6_LloydsIMO);
			AssertEquals("C6_VesselName:setter suspended", false, outturnHeader.SetterSuspender.IsSetterSuspended(AutoCusOutturnHeader.Schema.C6_VesselName));
			AssertEquals("C6_VesselName:not set new value", "ZZZ", outturnHeader.C6_VesselName);
			AssertEquals("C6_OA_OutturningPremise:setter suspended", false, outturnHeader.SetterSuspender.IsSetterSuspended(AutoCusOutturnHeader.Schema.C6_OA_OutturningPremise));
			AssertEquals("C6_OA_OutturningPremise:not set new value", ZGuid.Empty, outturnHeader.C6_OA_OutturningPremise);
			AssertEquals("C6_OutturningPremiseID:setter suspended", false, outturnHeader.SetterSuspender.IsSetterSuspended(AutoCusOutturnHeader.Schema.C6_OutturningPremiseID));
			AssertEquals("C6_OutturningPremiseID:not set new value", "ZZZ", outturnHeader.C6_OutturningPremiseID);
		}

		public void TestHumanReadableName()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_SendersMessageReference = "O00000340";
			AssertEquals("Sea Cargo Outturn O00000340", outturnHeader.HumanReadableName);
		}

		public void TestOnRescindMessageReceived()
		{
			CusOutturnHeaderForTest outturnHeader = Factory.New<CusOutturnHeaderForTest>();

			AssertEquals("", outturnHeader.MessageText);

			outturnHeader.ShowPopupRescindMessageReceived("Some Text");
			AssertEquals("OnRescindMessageReceived method should be called", "Some Text", outturnHeader.MessageText);
		}

		public void TestNilOutturn()
		{
			DepotCusOutturn outturn1 = OutturnHeader.Outturns.AddNew();
			DepotCusOutturn outturn2 = OutturnHeader.Outturns.AddNew();
			DepotCusOutturn outturn3 = OutturnHeader.Outturns.AddNew();
			DepotCusOutturn outturn4 = OutturnHeader.Outturns.AddNew();
			DepotCusOutturn outturn5 = OutturnHeader.Outturns.AddNew();

			outturn1.C5_ContainerNumber = "AAAA1111113";
			outturn1.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			outturn1.C5_OuterPacks = 1;
			outturn1.C5_OuterPackUnits = CMRPackageTypes.Codes.UnpackedOrPacked;

			outturn2.C5_ContainerNumber = "AAAA1111113";
			outturn2.C5_HouseBill = "HOUSE100";
			outturn2.C5_MasterBill = "OBL100";
			outturn2.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			outturn2.C5_OuterPacks = 50;
			outturn2.C5_OuterPackUnits = CMRPackageTypes.Codes.BeerCrate;

			outturn3.C5_ContainerNumber = "AAAA1111113";
			outturn3.C5_HouseBill = "HOUSE200";
			outturn3.C5_MasterBill = "OBL100";
			outturn3.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			outturn3.C5_OuterPacks = 40;
			outturn3.C5_OuterPackUnits = CMRPackageTypes.Codes.Crate;
			outturn3.C5_SealIntactIndicator = true;
			outturn3.C5_PillageIndicator = true;
			outturn3.C5_DamageIndicator = true;
			outturn3.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;

			outturn4.C5_MasterBill = "OBL200";
			outturn4.C5_CargoType = CMRImportCargoTypes.Codes.BreakBulk;
			outturn4.C5_OuterPacks = 5;
			outturn4.C5_OuterPackUnits = CMRPackageTypes.Codes.Drum;

			outturn5.C5_MasterBill = "OBL300";
			outturn5.C5_CargoType = CMRImportCargoTypes.Codes.Bulk;
			outturn5.C5_OuterPacks = 7;
			outturn5.C5_OuterPackUnits = CMRQuantityUnits.Codes.Skid;

			OutturnHeader.NilOutturn();

			AssertEquals(1, outturn1.C5_PackagesOutturned);
			AssertEquals(CMRPackageTypes.Codes.UnpackedOrPacked, outturn1.C5_PackagesUnits);
			AssertEquals(CMROutturnResultType.Codes.NilDiscrepancy, outturn1.C5_OutturnResultType);
			AssertEquals(false, outturn1.C5_PillageIndicator);
			AssertEquals(false, outturn1.C5_DamageIndicator);
			AssertEquals(true, outturn1.C5_SealIntactIndicator);

			AssertEquals(50, outturn2.C5_PackagesOutturned);
			AssertEquals(CMRPackageTypes.Codes.BeerCrate, outturn2.C5_PackagesUnits);
			AssertEquals(CMROutturnResultType.Codes.NilDiscrepancy, outturn2.C5_OutturnResultType);
			AssertEquals(false, outturn2.C5_PillageIndicator);
			AssertEquals(false, outturn2.C5_DamageIndicator);
			AssertEquals(false, outturn2.C5_SealIntactIndicator);

			AssertEquals(40, outturn3.C5_PackagesOutturned);
			AssertEquals(CMRPackageTypes.Codes.Crate, outturn3.C5_PackagesUnits);
			AssertEquals(CMROutturnResultType.Codes.NilDiscrepancy, outturn3.C5_OutturnResultType);
			AssertEquals(false, outturn3.C5_PillageIndicator);
			AssertEquals(false, outturn3.C5_DamageIndicator);
			AssertEquals(false, outturn3.C5_SealIntactIndicator);

			AssertEquals(5, outturn4.C5_PackagesOutturned);
			AssertEquals(CMRPackageTypes.Codes.Drum, outturn4.C5_PackagesUnits);
			AssertEquals(CMROutturnResultType.Codes.NilDiscrepancy, outturn4.C5_OutturnResultType);
			AssertEquals(false, outturn4.C5_PillageIndicator);
			AssertEquals(false, outturn4.C5_DamageIndicator);
			AssertEquals(false, outturn4.C5_SealIntactIndicator);

			AssertEquals(7, outturn5.C5_PackagesOutturned);
			AssertEquals(CMRQuantityUnits.Codes.Skid, outturn5.C5_PackagesUnits);
			AssertEquals(CMROutturnResultType.Codes.NilDiscrepancy, outturn5.C5_OutturnResultType);
			AssertEquals(false, outturn5.C5_PillageIndicator);
			AssertEquals(false, outturn5.C5_DamageIndicator);
			AssertEquals(false, outturn5.C5_SealIntactIndicator);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("Precondition: abn is not empty", false, GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number.IsEmpty);
			AssertEquals("Responsible party ID is set to current company abn", GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number, OutturnHeader.C6_ResponsiblePartyID);
		}

		public void TestVesselName()
		{
			RefVessel vessel = RefVessel.New(Factory);
			vessel.RV_Code = "FOO";
			vessel.RV_LloydsNumber = "BAR";

			Factory.Save();

			OutturnHeader.C6_VesselName = "FOO";
			AssertEquals("Our vessel", vessel, OutturnHeader.VesselName);
			AssertEquals("Lloyds has been set as well", "BAR", OutturnHeader.C6_LloydsIMO);

			OutturnHeader.C6_VesselName = ZString.Empty;
			OutturnHeader.C6_LloydsIMO = ZString.Empty;

			OutturnHeader.C6_LloydsIMO = "BAR";
			AssertEquals("Vessel name has been set as well", "FOO", OutturnHeader.C6_VesselName);

			OutturnHeader.C6_LloydsIMO = "SPAM";
			AssertEquals("Vessel is cleared if lloyds not recognised", "FOO", OutturnHeader.C6_VesselName);

			OutturnHeader.C6_VesselName = "EGGS";
			AssertEquals("Lloyds is not cleared if vessel is invalid.", "SPAM", OutturnHeader.C6_LloydsIMO);

			OutturnHeader.C6_LloydsIMO = "BAR";
			AssertEquals("Vessel name has been set as well", "FOO", OutturnHeader.C6_VesselName);

			var vessel2 = RefVessel.New(Factory);
			vessel2.RV_Code = "FOO2";
			vessel2.RV_LloydsNumber = "BAR";

			Factory.Save();

			OutturnHeader.C6_VesselName = "EGGS";
			OutturnHeader.C6_LloydsIMO = "BAR";
			AssertEquals("Vessel name should not be set", "EGGS", OutturnHeader.C6_VesselName);
		}

		public void TestOutturningPremiseID()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.MainAddress.LocalControlledPremisesID = "54321";
			OutturnHeader.C6_OutturningPremiseID = "54321";
			AssertEquals("54321", OutturnHeader.C6_OutturningPremiseID);
			AssertEquals("Outturning premise is set too", org.MainAddress.PK, OutturnHeader.C6_OA_OutturningPremise);
			AssertEquals(true, OutturnHeader.C6_OutturningPremiseIDInfo.ReadOnly);

			OutturnHeader.C6_OutturningPremiseID = ZString.Empty;
			OutturnHeader.C6_OA_OutturningPremise = ZGuid.Empty;

			OutturnHeader.C6_OA_OutturningPremise = org.MainAddress.PK;
			AssertEquals("Outturning premise is ours", org.MainAddress, OutturnHeader.OutturningPremise);
			AssertEquals("54321", OutturnHeader.C6_OutturningPremiseID);
			AssertEquals(true, OutturnHeader.C6_OutturningPremiseIDInfo.ReadOnly);

			OutturnHeader.C6_OA_OutturningPremise = ZGuid.Invalid;
			AssertEquals("54321", OutturnHeader.C6_OutturningPremiseID);
			AssertEquals(false, OutturnHeader.C6_OutturningPremiseIDInfo.ReadOnly);

			OutturnHeader.C6_OutturningPremiseID = "54321";
			AssertEquals("outturning premise set when not empty", org.MainAddress.PK, OutturnHeader.C6_OA_OutturningPremise);

			OutturnHeader.C6_OutturningPremiseID = "SPAM";
			OutturnHeader.C6_OA_OutturningPremise = org.MainAddress.PK;
			AssertEquals("outturning id set when not empty", "54321", OutturnHeader.C6_OutturningPremiseID);
		}

		public void TestOutturnStatus()
		{
			OutturnHeader.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;

			AssertEquals("OutturnStatus.StatusCode should be same as CMRAcceptedRejectedList.Codes.Accepted", CMRBaseStatuses.Codes.OriginalAccepted, OutturnHeader.OutturnStatus.Code);
			AssertEquals("descrption matches", CMRBaseStatuses.Descriptions.OriginalAccepted, OutturnHeader.OutturnStatus.Description);
		}

		public void TestOutturnStatusStartsAsNotSent()
		{
			AssertEquals("Status Code", CMRBaseStatuses.Codes.NotSent, OutturnHeader.OutturnStatus.Code);
		}

		public void TestICMRMessageRespondeeDetails()
		{
			OutturnHeader.C6_VoyageNum = "12345";
			OutturnHeader.C6_VesselName = "ADMIRALENGRACHT";
			AssertEquals("Details", "Vessel: ADMIRALENGRACHT\r\nVoyage: 12345\r\n", ((ICMRMessageRespondee)OutturnHeader).Details);
		}

		public void TestICMRMessageRespondeeShortDescription()
		{
			OutturnHeader.C6_VoyageNum = "12345";
			AssertEquals("ShortDescription", "Voyage: 12345", ((ICMRMessageRespondee)OutturnHeader).ShortDescription);
		}

		public void TestCalculator()
		{
			AssertNotNull("Null check", OutturnHeader.Calculator);
			AssertEquals("Type", typeof(CusOutturnHeaderStatusCalculator), OutturnHeader.Calculator.GetType());
		}

		public void TestLloydsReadOnly()
		{
			AssertEquals("by default", false, OutturnHeader.C6_LloydsIMOInfo.ReadOnly);
			OutturnHeader.C6_VesselName = "foo";
			AssertEquals("when vessel name set to invalid", false, OutturnHeader.C6_LloydsIMOInfo.ReadOnly);

			OutturnHeader.C6_VesselName = "ADMIRALENGRACHT";
			AssertEquals("when vessel name set to invalid", false, OutturnHeader.C6_LloydsIMOInfo.ReadOnly);

			OutturnHeader.C6_VesselName = ZString.Empty;
			AssertEquals("when vessel name empty", false, OutturnHeader.C6_LloydsIMOInfo.ReadOnly);

			OutturnHeader.C6_VesselName = "ADMIRALENGRACHT";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusOutturnHeader loadedHeader = factory2.Load<CusOutturnHeader>(OutturnHeader.PK);
			AssertEquals("after loaded", false, loadedHeader.C6_LloydsIMOInfo.ReadOnly);
		}

		public void TestOutturns()
		{
			AssertNotNull("nullness", OutturnHeader.Outturns);
			AssertEquals("type", typeof(CusOutturnHeaderDepotCusOutturnCollection), OutturnHeader.Outturns.GetType());
		}

		public void TestLookups()
		{
			AssertNotNull("nullness", OutturnHeader.Lookups);
			AssertEquals("type", typeof(CusOutturnHeaderLookups), OutturnHeader.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertNotNull("nullness", OutturnHeader.Lookups);
			AssertEquals("type", typeof(CusOutturnHeaderValidation), OutturnHeader.Validation.GetType());
		}

		public void TestSetDefaltValues()
		{
			AssertEquals("comes from currenty company abn", ABN, OutturnHeader.C6_ResponsiblePartyID);
		}

		public void TestABNSpacesStripped()
		{
			OutturnHeader.C6_ResponsiblePartyID = "75 006 687 958";
			AssertEquals("Responsible Party ID", "75006687958", OutturnHeader.C6_ResponsiblePartyID);
		}

		public void TestUnderbonds()
		{
			AssertNotNull(OutturnHeader.Underbonds);
			AssertEquals(typeof(DepotCusUnderbondCusOutturnHeaderCollection), OutturnHeader.Underbonds.GetType());
			AssertEquals("ChildEditable is not required on this collection. Underbonds are not to be validated.", false, OutturnHeader.IsRegisteredEditableChildObject(OutturnHeader.Underbonds));

			OutturnHeader.C6_LloydsIMO = "9044748";
			OutturnHeader.C6_OutturningPremiseID = "9914N";
			OutturnHeader.C6_VoyageNum = "304W";
			OutturnHeader.RunPreSaveValidation();
			AssertEquals("Precondition", 0, OutturnHeader.Notifications.Count());

			CusUnderbond underbond = OutturnHeader.Underbonds.AddNew();
			AssertEquals(OutturnHeader, underbond.Header);
			OutturnHeader.RunPreSaveValidation();
			AssertEquals("Underbond is not validated.", 0, OutturnHeader.Notifications.Count());
			OutturnHeader.Delete();
		}

		public void TestNewUnderbondsAppearOnDepotParent()
		{
			var underbond = OutturnHeader.Underbonds.AddNew();
			var mawb = Factory.New<CusMAWB>();
			underbond.LinkedObject = mawb;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var mawbIOF = otherFactory.Load<CusMAWB>(mawb.PK);
			var underbondIOF = mawbIOF.Underbonds.FirstOrDefault(u => u.PK == underbond.PK) as CusUnderbond;
			AssertNotNull("New Underbond is attached to Depot Parent", underbondIOF);
			AssertEquals("OutturnHeader is attached to Underbond", OutturnHeader.PK, underbondIOF.Header.PK);
		}

		public void TestHasSplitMessageOriginalRejectedLog()
		{
			AssertEquals("Has no outstanding amendments yet", false, OutturnHeader.HasSplitMessageOriginalRejectedLog);
			AssertEquals("Has no outstanding amendments yet", false, testManager.HasSplitMessageOriginalRejectedLog);

			StmALog newLog = OutturnHeader.Logs.AddNew(Events.UnderbondSplitOutturnOriginalRejected, "Test");
			AssertEquals("Has Outturn SplitMessageOriginalRejectedLog", true, OutturnHeader.HasSplitMessageOriginalRejectedLog);
			AssertEquals("Has Outturn SplitMessageOriginalRejectedLog", true, testManager.HasSplitMessageOriginalRejectedLog);

			newLog.Cancel();
			AssertEquals("Has no Outturn SplitMessageOriginalRejectedLog", 0, testManager.SplitMessageOriginalRejectedLog.Count);
			AssertEquals("Has no Outturn SplitMessageOriginalRejectedLog", false, OutturnHeader.HasSplitMessageOriginalRejectedLog);
		}

		public void TestHasOutturnSplitMessageFailedLog()
		{
			AssertEquals("Has no outstanding amendments yet", false, OutturnHeader.HasSplitMessageFailedLog);
			AssertEquals("Has no outstanding amendments yet", false, testManager.HasSplitMessageFailedLog);

			StmALog newLog = OutturnHeader.Logs.AddNew(Events.Cancelled, "Test");
			AssertEquals("Has Outturn SplitMessageFailedLog", true, OutturnHeader.HasSplitMessageFailedLog);
			AssertEquals("Has Outturn SplitMessageFailedLog", true, testManager.HasSplitMessageFailedLog);

			newLog.Cancel();
			AssertEquals("Has no Outturn SplitMessageFailedLog", 0, testManager.SplitMessageFailedLog.Count);
			AssertEquals("Has no Outturn SplitMessageFailedLog", false, testManager.HasSplitMessageFailedLog);
		}

		public void TestHasNonExistantLineAtCustoms()
		{
			AssertEquals("Has no outturn HasNonExistantLineAtCustoms", false, OutturnHeader.HasNonExistantLineAtCustomsLog);
			OutturnHeader.Logs.AddNew(Events.UnderbondOutturnRejected, "Partial Amendment Received");
			AssertEquals("Has HasNonExistantLineAtCustoms", true, testManager.HasNonExistantLineAtCustoms);
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = ABN;
			base.SetUp();

			testManager = new CusUnderbondOutturnLogManager(OutturnHeader);
		}

		CusUnderbondOutturnLogManager testManager;

		CusOutturnHeader OutturnHeader => outturnHeader ?? (outturnHeader = Factory.New<CusOutturnHeader>());
		CusOutturnHeader outturnHeader;

		const string ABN = "36103224237";

		class CusOutturnHeaderForTest : CusOutturnHeader
		{
			public CusOutturnHeaderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void OnRescindMessageReceived(RescindMessageEventArgs e)
			{
				MessageText = e.MessageText;
			}

			public ZString MessageText { get; private set; }

			public void AddDeferredScheduledMessagesEventExposed()
			{
				AddDeferredScheduledMessagesEvent();
			}

			public void SetDefaultValuesExposed()
			{
				base.SetDefaultValues();
			}
		}

		sealed class EDIMessageForTest : EDIMessage
		{
			public EDIMessageForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber() => "000000000000194233";
		}
	}
}
