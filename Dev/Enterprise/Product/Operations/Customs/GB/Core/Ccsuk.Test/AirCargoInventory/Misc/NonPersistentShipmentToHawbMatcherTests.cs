using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(NonPersistentShipmentToHawbMatcherLineCollection))]
	class NonPersistentShipmentToHawbMatcherLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NonPersistentShipmentToHawbMatcherLineCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(NonPersistentShipmentToHawbMatcherLineCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NonPersistentShipmentToHawbMatcherLine(Header, Factory);
		}

		protected override NonPersistentShipmentToHawbMatcherLineCollection GetCollectionToTest()
		{
			return Header.Pivots;
		}

		NonPersistentShipmentToHawbMatcherHeader header;
		NonPersistentShipmentToHawbMatcherHeader Header
		{
			get
			{
				if (header == null)
				{
					header = ShipmentToHawbMatcherManagerTest.CreateShipmentsAndHawbsForTest(Factory);
				}
				return header;
			}
		}
	}

	[TestedType(typeof(NonPersistentShipmentToHawbMatcherHeader))]
	public class NonPersistentShipmentToHawbMatcherHeaderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return ShipmentToHawbMatcherManagerTest.CreateShipmentsAndHawbsForTest(Factory);
		}

		public void TestCheckIfCanProceedToMakeOrMatchAll()
		{
			#region Scenario where we are not allowed to proceed:

			var header = PrepareHeaderObjectThatWillCauseValidationErrors(Factory);
			var line = (NonPersistentShipmentToHawbMatcherLine)(header.Pivots.ToArray()[0]);

			Assert("Prerequisite: Line.CreateNewHawb is true.", line.CreateNewHawb);
			Assert("Prerequisite: Header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR is false.", !header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR);
			Assert("Prerequisite: The basic CusMAWB has splits.", header.SelectedMAWB.Splits.Count > 0);

			var checkResult = header.CheckIfCanProceedToMakeOrMatchAll();

			Assert("Property 'CanProceed' must be false.", !checkResult.CanProceed);
			AssertEquals("Property 'ErrorMessage' must contain the error message.", "Please fix the validation errors.", checkResult.ErrorMessage);

			#endregion

			#region Scenario 01 where we are allowed to proceed, because not wanting to create new HAWBs:

			header = PrepareHeaderObjectThatWillCauseValidationErrors(Factory);

			line = (NonPersistentShipmentToHawbMatcherLine)(header.Pivots.ToArray()[0]);
			line.CreateNewHawb = false;

			checkResult = header.CheckIfCanProceedToMakeOrMatchAll();
			ConfirmCanProceedSuccessCondition("Scenario 01", checkResult);

			#endregion

			#region Scenario 02 where we are allowed to proceed, because existing splits can/will be deleted:

			header = PrepareHeaderObjectThatWillCauseValidationErrors(Factory);
			header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR = true;

			checkResult = header.CheckIfCanProceedToMakeOrMatchAll();
			ConfirmCanProceedSuccessCondition("Scenario 02", checkResult);

			#endregion

			#region Scenario 03 where we are allowed to proceed, because there are no existing splits:

			header = PrepareHeaderObjectThatWillCauseValidationErrors(Factory);
			header.SelectedMAWB.Splits.RemoveAndDeleteAll();

			checkResult = header.CheckIfCanProceedToMakeOrMatchAll();
			ConfirmCanProceedSuccessCondition("Scenario 03", checkResult);

			#endregion
		}

		void ConfirmCanProceedSuccessCondition(string label, CanProceedCheckResult checkResult)
		{
			Assert(label + ": Property 'CanProceed' must be true.", checkResult.CanProceed);
			Assert(label + ": Property 'ErrorMessage' must be empty.", checkResult.ErrorMessage.IsEmpty);
		}

		public static NonPersistentShipmentToHawbMatcherHeader PrepareHeaderObjectThatWillCauseValidationErrors(BusinessObjectFactory factory)
		{
			var consol = factory.New<ForwardingConsol>();
			var shipment = factory.New<ForwardingShipment>();

			consol.Shipments.Add(shipment);
			var shipments = consol.Shipments.OfType<ForwardingShipment>();

			var mawb = factory.New<CusMAWB>();
			mawb.Splits.AddNew();

			var header = new NonPersistentShipmentToHawbMatcherHeader(consol, new CusMAWB[] { mawb }, shipments);
			header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR = false;

			var line = header.Pivots.AddNew();
			line.CreateNewHawb = true;
			line.HawbNumber = "Hawb-1";

			return header;
		}
	}

	[TestedType(typeof(NonPersistentShipmentToHawbMatcherLine))]
	public class NonPersistentShipmentToHawbMatcherLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return ShipmentToHawbMatcherManagerTest.CreateShipmentsAndHawbsForTest(Factory).Pivots.First();
		}

		public void TestValidationOnThe_CreateNewHawb_FieldWhenTheMawbHasSplits()
		{
			var header = NonPersistentShipmentToHawbMatcherHeaderTest.PrepareHeaderObjectThatWillCauseValidationErrors(Factory);
			var line = (NonPersistentShipmentToHawbMatcherLine)(header.Pivots.ToArray()[0]);

			Assert("Prerequisite: Line.CreateNewHawb is true.", line.CreateNewHawb);
			Assert("Prerequisite: Header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR is false.", !header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR);
			Assert("Prerequisite: The basic CusMAWB has splits.", header.SelectedMAWB.Splits.Count > 0);

			line.Validation.ValidateCreateNewHawb();

			var expectedErrorText = "You cannot create new HAWBs when splits exist. First tick the box to 'Remove all splits from basic'";
			AssertHasError("Check for a specific, expected validation error.", line.CreateNewHawbInfo, expectedErrorText);
		}

		public void TestHawbNumberMaxLength()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = new NonPersistentShipmentToHawbMatcherHeader(consol, Array.Empty<CusMAWB>(), consol.Shipments.OfType<ForwardingShipment>());

			var mawb = Factory.New<CusMAWB>();
			var m1 = mawb.ChildBills.AddNew();
			m1.CS_HAWB = "AR100001";
			var m2 = mawb.ChildBills.AddNew();
			m2.CS_HAWB = "AR10000200";
			var m3 = mawb.ChildBills.AddNew();
			m3.CS_HAWB = "AR10003";
			Factory.Save();

			CombineAssertions(() =>
			{
				var line = new NonPersistentShipmentToHawbMatcherLine(header, Factory);
				line.CS = m1.PK;
				AssertEquals("AR100001", line.HawbNumber);
				line.CS = m2.PK;
				AssertEquals("AR100002", line.HawbNumber);
				line.CS = m3.PK;
				AssertEquals("0AR10003", line.HawbNumber);
			});
		}
	}

	public class ShipmentToHawbMatcherManagerTest : TestCaseWithFactory
	{
		public void TestLoadPivotsFromConsol()
		{
			var header = CreateShipmentsAndHawbsForTest(Factory);
			AssertEquals("Three hawbs and four shipments but only three pivots bewcvause one shipment is not GB and another already has a hawb", 3, header.Pivots.Count);
			AssertEquals("Lookups list for HAWBs should show only two because one HAWB is already attached to a shipment ", 2, header.Pivots[0].HawbsList.Count);
		}

		public static NonPersistentShipmentToHawbMatcherHeader CreateShipmentsAndHawbsForTest(BusinessObjectFactory factory, bool multipleMawbs = false)
		{
			ForwardingConsol consol = null;
			CusMAWB mawb = null;

			consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "USATL";
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.JK_MasterBillNum = "12512345678";
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKDestination = "GBLHR";
			shipment1.JS_RL_NKOrigin = "USATL";
			shipment1.JS_HouseBill = "SATL0001";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_RL_NKDestination = "GBLHR";
			shipment2.JS_RL_NKOrigin = "USATL";
			shipment2.JS_HouseBill = "SATL0002";
			var shipment3AlreadyhasHawb = consol.Shipments.AddNew();
			shipment3AlreadyhasHawb.JS_RL_NKDestination = "GBLHR";
			shipment3AlreadyhasHawb.JS_RL_NKOrigin = "USATL";
			shipment3AlreadyhasHawb.JS_HouseBill = "SATL0003";
			var shipment4NotRelevant = consol.Shipments.AddNew();
			shipment4NotRelevant.JS_RL_NKDestination = "FRPAR";
			shipment4NotRelevant.JS_RL_NKOrigin = "USATL";
			shipment4NotRelevant.JS_HouseBill = "SATL0004";
			var shipment5NotRelevant = consol.Shipments.AddNew();
			shipment5NotRelevant.JS_RL_NKDestination = "GBLHR";
			shipment5NotRelevant.JS_RL_NKOrigin = "USATL";
			shipment5NotRelevant.JS_HouseBill = "SATL0006";

			mawb = factory.New<CusMAWB>();
			mawb.CM_MAWB = "12512345678";
			mawb.AirportOfArrival = "LHR";
			var h1 = mawb.ChildBills.AddNew();
			h1.CS_HAWB = "00000001";
			var h2 = mawb.ChildBills.AddNew();
			h2.CS_HAWB = "ATL00002";
			var hawb3AlreadyHasShipment = mawb.ChildBills.AddNew();
			hawb3AlreadyHasShipment.CS_HAWB = "SATL0003";
			hawb3AlreadyHasShipment.CS_JS = shipment3AlreadyhasHawb.PK;

			factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var mawbInAnotherCountry = anotherFactory.New<CusMAWB>();
			mawbInAnotherCountry.CM_MAWB = "12512345678";
			var hawbInAnotherCountry = mawbInAnotherCountry.ChildBills.AddNew();
			hawbInAnotherCountry.CS_HAWB = "SATL0005";
			hawbInAnotherCountry.CS_JS = shipment5NotRelevant.PK;
			mawbInAnotherCountry.CM_ApplicationCode = "CMR";
			hawbInAnotherCountry.CS_ApplicationCode = "";

			anotherFactory.Save();

			if (multipleMawbs)
			{
				var shipment6AlreadyhasHawb = consol.Shipments.AddNew();
				shipment6AlreadyhasHawb.JS_RL_NKDestination = "GBLHR";
				shipment6AlreadyhasHawb.JS_RL_NKOrigin = "USATL";
				shipment6AlreadyhasHawb.JS_HouseBill = "SATL0007";
				CusMAWB mawb2 = factory.New<CusMAWB>();
				mawb2.CM_MAWB = "12512345678";
				mawb2.AirportOfArrival = "LHR";
				var h3 = mawb2.ChildBills.AddNew();
				h3.CS_HAWB = "00000003";
				var h4 = mawb.ChildBills.AddNew();
				h4.CS_HAWB = "ATL00004";
				var haw5AlreadyHasShipment = mawb.ChildBills.AddNew();
				haw5AlreadyHasShipment.CS_HAWB = "SATL0005";
				haw5AlreadyHasShipment.CS_JS = shipment6AlreadyhasHawb.PK;
				mawb2.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;

				return new NonPersistentShipmentToHawbMatcherHeader(consol, new CusMAWB[] { mawb, mawb2 }, consol.Shipments.OfType<ForwardingShipment>());
			}
			else
			{
				return new NonPersistentShipmentToHawbMatcherHeader(consol, new CusMAWB[] { mawb }, consol.Shipments.OfType<ForwardingShipment>());
			}
		}
	}

	public class ExistingSplitsOnBasicMawbDeleteOperationTest : TestCaseWithFactory
	{
		public void TestDeleteOfExistingSplitsOnBasicMawb()
		{
			var header = CreateScenarioToTestDeleteOfExistingSplitsOnBasicMawb(Factory);
			Factory.Save();

			#region Check Setup, Pre Matching Operation:

			var preFactory = Factory.CreateNewFactory();
			var preMAWB = preFactory.Load<CusMAWB>(header.SelectedMAWB.PK);

			AssertEquals("Prerequisite: There must be splits.", 2, preMAWB.Splits.Count);

			var splits = preMAWB.Splits.Cast<SplitBasic>().OrderBy(x => x.CG_MessageReference);
			var split_1 = splits.FirstOrDefault();
			var split_2 = splits.LastOrDefault();

			AssertEquals("Prerequisite: Checking split 01 details.", "SplitRef-01", split_1.CG_MessageReference);
			AssertEquals("Prerequisite: Checking split 02 details.", "SplitRef-02", split_2.CG_MessageReference);

			var query = new ZQuery();
			query.AddToFilter(CusHAWBSchema.CS_HAWB, "Hawb-1");
			var pre_Hawb_1 = preFactory.LoadTop1<CusHAWB>(query);

			query.Clear();
			query.AddToFilter(CusHAWBSchema.CS_HAWB, "Hawb-2");
			var pre_Hawb_2 = preFactory.LoadTop1<CusHAWB>(query);

			AssertNull("Prerequisite: HAWB 01 does not exist yet.", pre_Hawb_1);
			AssertNull("Prerequisite: HAWB 02 does not exist yet.", pre_Hawb_2);

			Assert("Prerequisite: Header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR is true.", header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR);
			AssertEquals("Prerequisite: 2 match lines expected.", 2, header.Pivots.Count);

			var line_1 = (NonPersistentShipmentToHawbMatcherLine)(header.Pivots.ToArray()[0]);
			var line_2 = (NonPersistentShipmentToHawbMatcherLine)(header.Pivots.ToArray()[1]);

			Assert("Prerequisite: For Line 01: Line.CreateNewHawb is true.", line_1.CreateNewHawb);
			Assert("Prerequisite: For Line 02: Line.CreateNewHawb is true.", line_2.CreateNewHawb);

			AssertEquals("Prerequisite: For Line 01: Line.HawbNumber must be specified.", "Hawb-1", line_1.HawbNumber);
			AssertEquals("Prerequisite: For Line 02: Line.HawbNumber must be specified.", "Hawb-2", line_2.HawbNumber);

			#endregion

			header.MakeOrMatchAll();
			Factory.Save();

			#region Check Status, Post Matching Operation:

			var postFactory = Factory.CreateNewFactory();
			var postMAWB = postFactory.Load<CusMAWB>(header.SelectedMAWB.PK);

			Assert("Splits must be deleted.", !postMAWB.HasSplits);

			query.Clear();
			query.AddToFilter(CusHAWBSchema.CS_HAWB, "00HAWB-1");
			var hawb_1 = postFactory.LoadTop1<CusHAWB>(query);

			query.Clear();
			query.AddToFilter(CusHAWBSchema.CS_HAWB, "00HAWB-2");
			var hawb_2 = postFactory.LoadTop1<CusHAWB>(query);

			AssertNotNull("HAWB 01 was created.", hawb_1);
			AssertNotNull("HAWB 02 was created.", hawb_2);

			var consol = header.Consol;
			var shipments = consol.Shipments.Cast<ForwardingShipment>().OrderBy(x => x.JS_GoodsDescription);
			var shipment_1 = shipments.FirstOrDefault();
			var shipment_2 = shipments.LastOrDefault();

			AssertEquals("HAWB 01 must be linked to Shipment 01.", shipment_1.PK, hawb_1.CS_JS);
			AssertEquals("HAWB 02 must be linked to Shipment 02.", shipment_2.PK, hawb_2.CS_JS);

			#endregion
		}

		static NonPersistentShipmentToHawbMatcherHeader CreateScenarioToTestDeleteOfExistingSplitsOnBasicMawb(BusinessObjectFactory factory)
		{
			const string AIR = "AIR";
			const string PortFrom = "USATL";
			const string PortTo = "GBLHR";

			var consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = AIR;
			consol.JK_RL_NKLoadPort = PortFrom;
			consol.JK_RL_NKDischargePort = PortTo;

			var shipment_1 = factory.New<ForwardingShipment>();
			shipment_1.JS_TransportMode = AIR;
			shipment_1.JS_RL_NKOrigin = PortFrom;
			shipment_1.JS_RL_NKDestination = PortTo;
			shipment_1.JS_GoodsDescription = "Goods1";

			var shipment_2 = factory.New<ForwardingShipment>();
			shipment_2.JS_TransportMode = AIR;
			shipment_2.JS_RL_NKOrigin = PortFrom;
			shipment_2.JS_RL_NKDestination = PortTo;
			shipment_2.JS_GoodsDescription = "Goods2";

			consol.Shipments.Add(shipment_1);
			consol.Shipments.Add(shipment_2);
			var shipments = consol.Shipments.OfType<ForwardingShipment>();

			var mawb = factory.New<CusMAWB>();
			mawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;

			var split_1 = mawb.Splits.AddNew();
			split_1.CG_MessageReference = "SplitRef-01";
			split_1.CG_PiecesManifested = 19;
			split_1.CG_PiecesLanded = 18;

			var split_2 = mawb.Splits.AddNew();
			split_2.CG_MessageReference = "SplitRef-02";
			split_2.CG_PiecesManifested = 19;
			split_2.CG_PiecesLanded = 17;

			var header = new NonPersistentShipmentToHawbMatcherHeader(consol, new CusMAWB[] { mawb }, shipments);
			header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR = true;

			var line_1 = header.Pivots.AddNew();
			line_1.JS = shipment_1.PK;
			line_1.CreateNewHawb = true;
			line_1.HawbNumber = "Hawb-1";
			line_1.JS = shipment_1.PK;

			var line_2 = header.Pivots.AddNew();
			line_2.JS = shipment_2.PK;
			line_2.CreateNewHawb = true;
			line_2.HawbNumber = "Hawb-2";
			line_2.JS = shipment_2.PK;

			return header;
		}
	}

	public class NonPersistentShipmentToHawbMatcherLineWithCusMAWBChildBillsTest : TestCaseWithFactory
	{
		public void TestCreateNewHawb()
		{
			const string AIR = "AIR";
			const string PortFrom = "USATL";
			const string PortTo = "GBLHR";
			var factory = Factory;

			var consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = AIR;
			consol.JK_RL_NKLoadPort = PortFrom;
			consol.JK_RL_NKDischargePort = PortTo;

			var shipment_1 = factory.New<ForwardingShipment>();
			shipment_1.JS_TransportMode = AIR;
			shipment_1.JS_RL_NKOrigin = PortFrom;
			shipment_1.JS_RL_NKDestination = PortTo;

			var shipment_2 = factory.New<ForwardingShipment>();
			shipment_2.JS_TransportMode = AIR;
			shipment_2.JS_RL_NKOrigin = PortFrom;
			shipment_2.JS_RL_NKDestination = PortTo;

			consol.Shipments.Add(shipment_1);
			consol.Shipments.Add(shipment_2);
			var shipments = consol.Shipments.OfType<ForwardingShipment>();

			var mawb = factory.New<CusMAWB>();
			var h1 = mawb.ChildBills.AddNew();
			h1.CS_HAWB = "00000001";
			var h2 = mawb.ChildBills.AddNew();
			h2.CS_HAWB = "ATL00002";
			mawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;

			var header = new NonPersistentShipmentToHawbMatcherHeader(consol, new CusMAWB[] { mawb }, shipments);
			header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR = false;

			var line_1 = header.Pivots.AddNew();
			line_1.JS = shipment_1.PK;
			line_1.HawbNumber = "Hawb-1";
			line_1.JS = shipment_1.PK;
			AssertNoExceptionThrown(() => line_1.CreateNewHawb = true);
			AssertNoExceptionThrown(() => line_1.CreateNewHawb = false);
			AssertHasNotifications("You cannot create new HAWBs when splits exist. First tick the box to 'Remove all splits from basic'", line_1.CreateNewHawbInfo);
		}
	}

	public class NonPersistentShipmentToHawbMatcherHeaderValidationTests : TestCaseWithFactory
	{
		public void TestCheckSelectedMawbWrapperGUID()
		{
			var shipmentToHawbMatcher = ShipmentToHawbMatcherManagerTest.CreateShipmentsAndHawbsForTest(Factory, true);
			var mawb1 = shipmentToHawbMatcher.AvailableMawbs.OfType<CusMawbWrapper>().FirstOrDefault();
			var mawb2 = shipmentToHawbMatcher.AvailableMawbs.OfType<CusMawbWrapper>().LastOrDefault();

			shipmentToHawbMatcher.SelectedMawbWrapperGUID = ZGuid.Empty;
			AssertHasError(shipmentToHawbMatcher.SelectedMawbWrapperGUIDInfo, "You must select a MAWB from the list.");

			shipmentToHawbMatcher.SelectedMawbWrapperGUID = mawb2?.PK ?? ZGuid.Empty;
			AssertNoError(shipmentToHawbMatcher.SelectedMawbWrapperGUIDInfo, "You must select a MAWB from the list.");

			mawb1.MAWB.SetCustomsActionCode(CustomsStatusCodes.Codes.EntryOrRequestAccepted, ZDateTime.Now);
			shipmentToHawbMatcher.SelectedMawbWrapperGUID = mawb1?.PK ?? ZGuid.Empty;
			AssertHasMessageError(shipmentToHawbMatcher.SelectedMawbWrapperGUIDInfo, "This basic already has a customs action status and should not be selected.");

			mawb1.MAWB.SetCustomsActionCode(CustomsStatusCodes.Codes.EntryOrRequestCancelled, ZDateTime.Now);
			shipmentToHawbMatcher.Validation.ValidateSelectedMawbWrapperGUID();
			AssertNoMessageError(shipmentToHawbMatcher.SelectedMawbWrapperGUIDInfo, "This basic already has a customs action status and should not be selected.");
		}
	}
}
