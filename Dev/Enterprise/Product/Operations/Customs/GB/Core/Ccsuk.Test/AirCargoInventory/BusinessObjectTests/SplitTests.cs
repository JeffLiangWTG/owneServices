using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(SplitHouse))]
	class SplitHouseTests : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestSaveDoesntSetStatus1DateWithoutPieces()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "CUKAIR98LHRBAC";
			mawb.AgentBadge = "LXA";
			mawb.NumberOfPiecesExpected = 0;
			var hawb = mawb.ChildBills.AddNew();
			var split = hawb.Splits.AddNew();
			split.SplitReference = "01";
			Factory.Save();
			AssertEquals(ZDateTime.Empty, split.Status1Date);
			AssertEquals(ZDateTime.Empty, hawb.Status1Date);
			AssertEquals(ZDateTime.Empty, mawb.Status1Date);
		}

		public void TestHasEntryWithLodgedOrPrelodgedWithCustoms_CDS()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "CUKFFW98000CAR";
			var hawb = mawb.ChildBills.AddNew();
			var split = hawb.Splits.AddNew();
			ICcsukCusAwb awb = split;
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			var dec = awb.CreateNewStandaloneCDSDeclaration();
			AssertEquals("CDS declaration", "CDS", dec.JE_ApplicationCode);
			AssertEquals("Default Declaration Type", "H1", dec.JE_DeclarationType);
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			var entry = dec.CustomsEntryHeaders.AddNew();
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);

			entry.EntryNumber = "123";
			entry.CH_EntryStatus = ZString.Empty;
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);

			entry.CH_EntryStatus = "REJ";
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);

			new List<string> { "ACC", "RCV", "CLR", "CAN" }.ForEach(x =>
			{
				entry.CH_EntryStatus = x;
				AssertEquals(true, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			});
		}

		public void TestAgentBadge()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.AgentBadge = "DVG";
			var split = hawb.Splits.AddNew();
			AssertEquals("Split's badge is that of parent when not explicitly set on split", "DVG", split.AgentBadge);
			split.AgentBadge = "ZPE";
			AssertEquals("Split's badge is now its own", "ZPE", split.AgentBadge);
			AssertEquals("Split's badge does not clobber parent's", "DVG", hawb.AgentBadge);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			AssertEquals("Split's badge saved OK", "ZPE", newFactory.Load<SplitHouse>(split.PK).AgentBadge);
			AssertEquals("Split's badge doesn't clobber parent's", "DVG", newFactory.Load<CusHAWB>(hawb.PK).AgentBadge);
		}

		public void TestAgentBadgeReadOnly()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.Profile = "CUKFFW98000LXA";
			var split = Factory.New<SplitHouseWithPublicReadOnlyBadgeForTest>();
			hawb.Splits.Add(split);
			AssertEquals("Split's badge is locked for agent", true, split.BadgeReadOnlyForTest_ThisIsOnlyHereBecauseZPropertyInfoIsABitSucky);
			hawb.Profile = "CUKAIR98LHRBAC";
			AssertEquals("Split's badge still locked - must inherit from parent", true, split.BadgeReadOnlyForTest_ThisIsOnlyHereBecauseZPropertyInfoIsABitSucky);
		}

		public void TestSetEcStatusRelease()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var split = hawb.Splits.AddNew();
			split.SplitReference = "02";
			var ot = hawb.OutTurns.AddNew();
			ot.SplitReferenceToWhichThisPertains = split.SplitReference;
			CusHawbTests.RunSetEcStatusReleaseTest(split, null, ot);
		}

		public void TestUnsetEcStatusWipesOldEdocs()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "11122222222";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "33333333";
			var split = hawb.Splits.AddNew();
			split.SplitReference = "44";
			var eDocEcRra = split.DocManagerInfo.AddFileOrDocument(ZBlob.FromAscii("Daniel"), "RRA.txt", "RRA");
			var eDocIrrelevant = split.DocManagerInfo.AddFileOrDocument(ZBlob.FromAscii("Clarke"), "Foo.txt", "GRA");
			eDocEcRra.Description = "Company - Branch - Release/Removal Authority for 111-22222222-33333333/44 reprint blah";
			eDocIrrelevant.Description = "Company - Branch - Whatever";
			Factory.Save();
			((ICcsukCusAwb)split).SetEcStatusRelease(false);
			Factory.Save();
			var splitReloaded = new BusinessObjectFactory().Load<SplitHouse>(split.PK);
			AssertEquals(1, split.DocManagerInfo.AllEDocs.Count);
		}

		[TestDate(1986, 3, 12, 4, 0, 1)]
		public void TestLocalCreationTime()
		{
			var awb = (ICcsukCusAwb)GetNewBusinessObject();
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 0, 1), awb.LocalCreationDate);
		}

		public void TestUpdateStatusToCacIfAllowed()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var split1 = (SplitHouse)hawb.Splits.AddNew();
			var split2 = (SplitHouse)hawb.Splits.AddNew();
			var split3 = (SplitHouse)hawb.Splits.AddNew();
			CusHawbTests.RunUpdateStatusToCacIfAllowedTest(split1, CustomsStatusCodes.Codes.EntryOrRequestAccepted);
			CusHawbTests.RunUpdateStatusToCacIfAllowedTest(split2, CustomsStatusCodes.Codes.EntryOrRequestCancelled);
			CusHawbTests.RunUpdateStatusToCacIfAllowedTest(split3, CustomsStatusCodes.Codes.CustomsQueriedDetained);
		}

		public void TestIsPrearrival()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var hawb = cusMawb.ChildBills.AddNew();
			var split = hawb.Splits.AddNew();
			AssertEquals(true, split.IsPrearrival);
			cusMawb.NumberOfPiecesReceived = 69;
			AssertEquals(false, split.IsPrearrival);
			cusMawb.NumberOfPiecesReceived = 0;
			cusMawb.CM_ArrivalDate = ZDateTime.Now;
			AssertEquals(false, split.IsPrearrival);
			cusMawb.NumberOfPiecesReceived = 0;
			cusMawb.CM_ArrivalDate = ZDateTime.Empty;
			AssertEquals(true, split.IsPrearrival);
			hawb.CS_PiecesLanded = 6;
			AssertEquals(false, split.IsPrearrival);
			hawb.CS_PiecesLanded = 0;
			split.NumberOfPiecesReceived = 6;
			AssertEquals(false, split.IsPrearrival);
		}

		public void TestCompleteOnCcsukAndIsCompleteOnCcsuk()
		{
			var mawb = Factory.New<CusMAWB>();
			var house = mawb.ChildBills.AddNew();
			ICcsukCusAwb split1 = house.Splits.AddNew();
			ICcsukCusAwb split2 = house.Splits.AddNew();
			split1.SetCustomsActionCode("DC", ZDateTime.BrettsBirthday);
			Assert(!split1.IsCompleteOnCcsuk);
			split1.CompleteOnCcsuk();
			Assert(split1.IsCompleteOnCcsuk);
			Assert(!((ICcsukCusAwb)house).IsCompleteOnCcsuk);
			Assert(!((ICcsukCusAwb)mawb).IsCompleteOnCcsuk);
			CusHawbTests.AssertHasStatus3Logs(split1, "DC");
			split2.CompleteOnCcsuk();
			Assert(split2.IsCompleteOnCcsuk);
			Assert(((ICcsukCusAwb)house).IsCompleteOnCcsuk);
			Assert(((ICcsukCusAwb)mawb).IsCompleteOnCcsuk);
			split1.UncompleteOnCcsuk();
			Assert(!split1.IsCompleteOnCcsuk);
			Assert(split2.IsCompleteOnCcsuk);
			Assert(!((ICcsukCusAwb)house).IsCompleteOnCcsuk);
			Assert(!((ICcsukCusAwb)mawb).IsCompleteOnCcsuk);

			SplitBasicTests.RunSetStatus1AndStatus3TestForCompleteness(split1);
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestStatus1()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "CUKFFW98000ABC";
			var hawb = mawb.ChildBills.AddNew();
			var split = (SplitHouse)hawb.Splits.AddNew();
			CusHawbTests.RunStatus1Test(split);
		}

		public void TestArchiveOnCcsukAndIsArchivedOnCcsuk()
		{
			var mawb = Factory.New<CusMAWB>();
			var house = mawb.ChildBills.AddNew();
			ICcsukCusAwb split1 = house.Splits.AddNew();
			ICcsukCusAwb split2 = house.Splits.AddNew();
			Assert(!split1.IsArchivedOnCcsuk);
			split1.ArchiveOnCcsuk(ReasonForArchiving.NprFewerThanNpxAndFinalCustomsActionDateOlderThan180Days);
			Assert(split1.IsArchivedOnCcsuk);
			Assert(!((ICcsukCusAwb)house).IsArchivedOnCcsuk);
			Assert(!((ICcsukCusAwb)mawb).IsArchivedOnCcsuk);
			split2.ArchiveOnCcsuk(ReasonForArchiving.NprFewerThanNpxAndFinalCustomsActionDateOlderThan180Days);
			Assert(split2.IsArchivedOnCcsuk);
			Assert(((ICcsukCusAwb)house).IsArchivedOnCcsuk);
			Assert(((ICcsukCusAwb)mawb).IsArchivedOnCcsuk);
		}

		public void TestIsEntryCancelled_CDS()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.Profile = "CUKFFW98000CAR";

			var hawb = cusMawb.ChildBills.AddNew();
			var split = hawb.Splits.AddNew();

			var awb = (ICcsukCusAwb)split;

			AssertEquals(false, awb.IsEntryCancelled);

			split.CG_CustomsStatus = CustomsStatusCodes.Codes.EntryOrRequestCancelled;
			AssertEquals(false, awb.IsEntryCancelled);

			var declaration = awb.CreateNewStandaloneCDSDeclaration();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(false, awb.IsEntryCancelled);

			split.CG_CustomsStatus = CustomsStatusCodes.Codes.ClearedByCustoms;
			AssertEquals(false, awb.IsEntryCancelled);

			split.CG_CustomsStatus = CustomsStatusCodes.Codes.EntryOrRequestCancelled;
			new List<string> { "ACC", "CLR", "RCV", "REJ" }.ForEach(x =>
			{
				entry.CH_EntryStatus = x;
				AssertEquals(false, awb.IsEntryCancelled);
			});

			entry.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
			AssertEquals(true, awb.IsEntryCancelled);
		}

		public void TestDeclarationFunctionality()
		{
			var split1 = (SplitHouse)GetNewBusinessObject();
			split1.HAWB.MAWB.CM_ArrivalDate = new ZDateTime(1986, 3, 12);
			split1.HAWB.Profile = "CUKFFW98000XXX";
			split1.HAWB.MAWB.CM_MAWB = "11122222222";
			split1.HAWB.MAWB.CM_FlightNo = "BA123";
			split1.HAWB.CS_HAWB = "33333333";
			split1.HAWB.CS_GoodsDescription = "Stuff";
			split1.NumberOfPiecesExpected = 9;
			AssertNull(split1.OwnDeclaration);
			Assert(!split1.HasOwnDeclaration);
			AssertEquals("", split1.ChiefDeclarationUCR);
			split1.HAWB.CS_JS = Factory.New<Freight.Forwarding.Business.ForwardingShipment>().PK;
			var dec1 = ((ICcsukCusAwb)split1).CreateNewStandaloneCDSDeclaration();
			AssertNull(dec1);
			split1.HAWB.CS_JS = ZGuid.Empty;
			RunSecondHalfOfSplitDeclarationTests(split1, out dec1);
			AssertEquals("33333333", dec1.JE_HouseBill);
		}

		internal static void RunSecondHalfOfSplitDeclarationTests(SplitConsignment split1, out JobDeclaration dec1)
		{
			dec1 = ((ICcsukCusAwb)split1).CreateNewStandaloneCDSDeclaration();
			AssertNotNull(dec1);
			AssertEquals(dec1, split1.OwnDeclaration);
			Assert(split1.HasOwnDeclaration);
			AssertEquals("IMP", dec1.JE_MessageType);
			AssertEquals("AIR", dec1.JE_TransportMode);
			AssertEquals("11122222222", dec1.JE_MasterBill);
			AssertEquals("03", dec1.ZG_HouseSplitReference);
			AssertEquals("A", dec1.JE_EntrySubStyle);
			AssertEquals(9, dec1.JE_TotalNoOfPacks);
			AssertEquals("STUFF", dec1.JE_GoodsDescription);
			AssertEquals(new ZDateTime(1986, 3, 12), dec1.JE_DateOfArrival);
			AssertEquals("Don't make a second dec if one already exists", null, ((ICcsukCusAwb)split1).CreateNewStandaloneCDSDeclaration());
			dec1.JE_UCR = "Test DUCR";
			AssertEquals("Test DUCR", split1.ChiefDeclarationUCR);
			dec1.JE_OwnerRef = "XYZ";
			var split2 = split1.AWB.Splits.AddNew();
			split2.SplitReference = "04";
			var dec2 = ((ICcsukCusAwb)split2).CreateNewStandaloneCDSDeclaration();
			AssertEquals(dec2, split2.OwnDeclaration);
			Assert("Should have been saved to DB automatically", dec2.IsInDatabase);
			AssertEquals("Asserts that dec2 is a clone of dec1", "XYZ", dec2.JE_OwnerRef);
			Assert(dec1.RelatedDeclarations.Contains(dec2));
			dec1.Factory.Save();
			var splitReloaded = new BusinessObjectFactory().Load<SplitHouse>(split1.PK);
			AssertEquals(dec1.PK, splitReloaded.OwnDeclaration.PK);
		}

		public void TestReferenceNumbersAndHumanReadableName()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			mawb.CM_MAWB = "12387654321";
			hawb.CargoTerminalOperatorAirport = "LHR";
			hawb.CargoTerminalOperator = "CAX";
			hawb.CS_HAWB = "12345678";
			var split = hawb.Splits.AddNew();
			split.SplitReference = "03";
			split.NumberOfPiecesExpected = 69;
			AssertEquals("123-87654321-12345678/03", split.ReferenceNumber);
			AssertEquals("Split House 123-87654321-12345678/03 (69 pieces)", split.HumanReadableNameForFormCaption);
			AssertEquals("LHRCAX-123-87654321-12345678/03", split.ReferenceNumberWithShed);
			AssertEquals("123876543211234567803", ((ICcsukCusAwb)split).ChiefMasterUCRReferenceSuffix);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var mawb = Factory.New<CusMAWB>();
			var house = mawb.ChildBills.AddNew();
			var split = (SplitHouse)house.Splits.AddNew();
			split.SplitReference = "03";
			return split;
		}

		public void TestDocManagerInfoAndDocumentSupporter()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var house = cusMawb.ChildBills.AddNew();
			var splitHouse = house.Splits.AddNew();
			AssertType(typeof(CcsukDocumentSupporter), splitHouse.DocumentSupporter);
			AssertType(typeof(SplitConsignmentDocManagerInfo), ((IDocManagerSupport)splitHouse).DocManagerInfo);
		}

		public void TestPresenceOnNetworkStatus()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.PresenceOnNetworkStatus = "Y";
			var split = (SplitHouse)hawb.Splits.AddNew();
			split.PresenceOnNetworkStatus = "X";
			AssertEquals("X", split.PresenceOnNetworkStatus);
			Factory.Save();
			var splitReloaded = Factory.Load<SplitHouse>(split.PK);
			AssertEquals("X", splitReloaded.PresenceOnNetworkStatus);
		}

		[TestDate(2015, 8, 22, 14, 00, 00)]
		public void TestNumberOfPiecesReceived()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.Profile = "CUKAIR98LHRBAC";
			hawb.CS_PiecesManifested = 9;
			var split = (SplitHouse)hawb.Splits.AddNew();
			split.SplitReference = "01";
			split.NumberOfPiecesExpected = 9;
			AssertEquals((ZShort)0, split.NumberOfPiecesReceived);
			var ot1 = hawb.OutTurns.AddNew();
			ot1.C5_PackagesOutturned = 4;
			AssertEquals((ZShort)0, split.NumberOfPiecesReceived);
			ot1.SplitReferenceToWhichThisPertains = "01";
			Factory.Save();
			AssertEquals((ZShort)4, hawb.CS_PiecesLanded);
			AssertEquals((ZShort)4, split.NumberOfPiecesReceived);
			var ot2 = hawb.OutTurns.AddNew();
			ot2.C5_PackagesOutturned = 5;
			Factory.Save();
			AssertEquals((ZShort)9, hawb.CS_PiecesLanded);
			AssertEquals((ZShort)4, split.NumberOfPiecesReceived);
			ot2.SplitReferenceToWhichThisPertains = "01";
			Factory.Save();
			AssertEquals((ZShort)9, split.NumberOfPiecesReceived);
			AssertEquals((ZShort)9, hawb.CS_PiecesLanded);
			AssertEquals(new ZDateTime(2015, 8, 22, 14, 00, 00), split.Status1Date);
			AssertEquals(new ZDateTime(2015, 8, 22, 14, 00, 00), hawb.Status1Date);
		}

		public void TestNprReadOnly()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.Profile = "CUKAIR98LHRBAC";
			var split = (SplitHouse)hawb.Splits.AddNew();
			AssertEquals(false, split.NumberOfPiecesReceivedReadOnly);
			hawb.Profile = "CUKFFW98000LXA";
			AssertEquals(true, split.NumberOfPiecesReceivedReadOnly);
			hawb.Profile = "";
			AssertEquals(true, split.NumberOfPiecesReceivedReadOnly);
		}

		public void TestOutturns()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var split1 = hawb.Splits.AddNew();
			split1.SplitReference = "01";
			var split2 = hawb.Splits.AddNew();
			split2.SplitReference = "02";
			var outturn1 = hawb.OutTurns.AddNew();
			var outturn2 = hawb.OutTurns.AddNew();
			outturn1.SplitReferenceToWhichThisPertains = "01";
			outturn2.SplitReferenceToWhichThisPertains = "02";
			AssertEquals(1, ((ICcsukCusAwb)split1).OutTurns.Count);
			AssertEquals(outturn1.PK, ((ICcsukCusAwb)split1).OutTurns[0].PK);
			AssertEquals(1, ((ICcsukCusAwb)split2).OutTurns.Count);
			AssertEquals(outturn2.PK, ((ICcsukCusAwb)split2).OutTurns[0].PK);
		}

		public void TestOutturnsWithUnderbondsProxied()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var split1 = hawb.Splits.AddNew();
			var ub = hawb.TSRs.AddNew();
			split1.SplitReference = "01";
			ub.SplitReferenceToWhichThisRemovalPertains = "01";
			Factory.Save(); //?
			var outturn1 = hawb.OutTurns.AddNew();
			outturn1.C5_C4_Underbond = ub.PK;
			AssertEquals(1, ((ICcsukCusAwb)split1).OutTurns.Count);
			AssertEquals(outturn1.PK, ((ICcsukCusAwb)split1).OutTurns[0].PK);
		}

		public void TestProperties()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "99912345678";
			var house = mawb.ChildBills.AddNew();
			house.CargoTerminalOperator = "BAC";
			house.CargoTerminalOperatorAirport = "BAC";
			house.AgentBadge = "DAN";
			var split = (SplitHouse)house.Splits.AddNew();
			split.SplitReference = "03";
			split.Weight = 123m;
			split.WeightCode = "KG";
			split.SetCustomsActionCode("CT", ZDateTime.BrettsBirthday);
			split.LatestCustomsActionText = "You suck";
			split.HandlingInformation = "Tuftykins";
			var mocker = Factory.NewMoq<EDIMessageDummyForTest_2343233>();
			mocker.Protected().Setup<string>("GetMessageReferenceNumber").Returns("2343233");
			var message = mocker.Object;
			house.Messages.Add(message);
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;

			AssertEquals("03", split.SplitReference);
			AssertEquals("KG", split.WeightCode);
			AssertEquals(123m, split.Weight);
			AssertEquals(house.PK, split.AWB.PK);
			AssertEquals("999-12345678", split.MasterBill);
			AssertEquals("BAC", split.CargoTerminalOperator);
			AssertEquals("BAC", split.CargoTerminalOperatorAirport);
			AssertEquals("DAN", split.AgentBadge);
			AssertEquals("CT", split.CustomsActionCode);
			AssertEquals("You suck", split.LatestCustomsActionText);
			AssertEquals("Tuftykins", split.HandlingInformation);
			AssertType(typeof(SplitToFsrProvider), split.GetAwbToFsrProvider(new CcsukTransmissionMessageFunction.CUKFSR.FSN()));

			Factory.Save();
			split = new BusinessObjectFactory().Load<SplitHouse>(split.PK);
			AssertEquals("03", split.SplitReference);
			AssertEquals("KG", split.WeightCode);
			AssertEquals(123m, split.Weight);
			AssertEquals(house.PK, split.AWB.PK);
			AssertEquals("999-12345678", split.MasterBill);
			AssertEquals("BAC", split.CargoTerminalOperator);
			AssertEquals("BAC", split.CargoTerminalOperatorAirport);
			AssertEquals("DAN", split.AgentBadge);
			AssertEquals("CT", split.CustomsActionCode);
			AssertEquals("You suck", split.LatestCustomsActionText);
			AssertEquals("Tuftykins", split.HandlingInformation);
			mocker.VerifyAll();
		}

		public void TestHandlingInformationForBinding()
		{
			var mawb = Factory.New<CusMAWB>();
			var house = mawb.ChildBills.AddNew();
			var split = house.Splits.AddNew();
			RunHandlingInformationForBindingTest(split, house.OutTurns);
		}

		internal static void RunHandlingInformationForBindingTest(SplitConsignment split, CusOutTurnCollection outTurns)
		{
			IWhsLocation locationBac1;
			IWhsLocation locationBac2;
			IWhsLocation locationCax;
			CusOutTurnTest.CreateWarehouseAreasForTest(split.Factory, out locationBac1, out locationBac2, out locationCax);

			split.HandlingInformation = "Red";
			AssertEquals("Red", split.HandlingInformation);
			AssertEquals("Red", split.HandlingInformationForBinding);
			split.SplitReference = "01";
			var ot1 = outTurns.AddNew();
			ot1.SplitReferenceToWhichThisPertains = "01";
			ot1.C5_PackagesOutturned = 1;
			ot1.C5_PackagesUnits = "CT";
			ot1.WarehouseLocationID = locationBac1.PK;
			AssertEquals("1CT marked Red in " + CusOutTurnTest.rowName1, split.HandlingInformationForBinding);
			ot1.C5_MarksAndNumbers = "Blue";
			AssertEquals("1CT marked Blue in " + CusOutTurnTest.rowName1, split.HandlingInformationForBinding);
			ot1.WarehouseLocationID = ZGuid.Empty;
			ot1.C5_MarksAndNumbers = "Blue";
			AssertEquals("1CT marked Blue", split.HandlingInformationForBinding);
			ot1.WarehouseLocationID = locationBac2.PK;
			ot1.C5_MarksAndNumbers = "";
			AssertEquals("1CT in " + CusOutTurnTest.rowName2, split.HandlingInformationForBinding);
			var ot2 = outTurns.AddNew();
			ot2.SplitReferenceToWhichThisPertains = "01";
			ot2.C5_PackagesOutturned = 2;
			ot2.C5_PackagesUnits = "BX";
			AssertEquals(FormattableString.Invariant($"1CT in {CusOutTurnTest.rowName2}...\r\n2BX marked Red"), split.HandlingInformationForBinding);
			var ot3 = outTurns.AddNew();
			ot3.SplitReferenceToWhichThisPertains = "33";
			ot3.C5_PackagesOutturned = 3;
			ot3.C5_PackagesUnits = "BX";
			AssertEquals(FormattableString.Invariant($"1CT in {CusOutTurnTest.rowName2}...\r\n2BX marked Red"), split.HandlingInformationForBinding);
		}

		public void TestCustomsActionsSetBlankOnSplitDoesNotUpdateParent()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var split = hawb.Splits.AddNew();
			split.SetCustomsActionCode("", ZDateTime.Now);
			AssertEquals("", hawb.CustomsActionCode);
		}

		public void TestCustomsActions()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var split = hawb.Splits.AddNew();
			split.SetCustomsActionCode("CT", ZDateTime.BrettsBirthday);
			split.LatestCustomsActionText = "You suck";
			AssertEquals("CT", split.CustomsActionCode);
			AssertEquals("You suck", split.LatestCustomsActionText);
			var filter = new ZQuery(StmALogSchema.SL_Reference, "CT");
			filter.AddToFilter(StmALogSchema.SL_Parent, split.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, "CES");
			var stmLog = Factory.Load<StmALog>(filter);
			AssertNotNull(stmLog);
			Factory.Save();
			split = new BusinessObjectFactory().Load<SplitHouse>(split.PK);
			AssertEquals("CAC visible upon reload", "CT", split.CustomsActionCode);
			AssertEquals("CAT visible on reload", "You suck", split.LatestCustomsActionText);
			AssertEquals("CAC date recorded", ZDateTime.BrettsBirthday, split.CustomsActionDate);
			split.SetCustomsActionCode("CT", ZDateTime.BrettsBirthday.AddDays(1));
			AssertEquals("CAC date updated when CAC set again to same value (allows CAC to go from blank->CT->CX->CT again)", ZDateTime.BrettsBirthday.AddDays(1), split.CustomsActionDate);
			AssertEquals("CAC visible upon reload", "CT", split.CustomsActionCode);
			split.SetCustomsActionCode("BB", ZDateTime.BrettsBirthday.AddYears(1));
			split.LatestCustomsActionText = "You still suck";
			AssertEquals("BB", split.CustomsActionCode);
			AssertEquals("You still suck", split.LatestCustomsActionText);
			AssertEquals("CAC date now updated because code also changed", ZDateTime.BrettsBirthday.AddYears(1), split.CustomsActionDate);
		}

		public void TestModuleControllerId()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			var split = cusHawb.Splits.AddNew();
			AssertEquals(ControllerIDs.Customs.GB.CcsukSplitHouseController, split.ModuleControllerId);
		}

		public void TestPiecesReleased()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var split = hawb.Splits.AddNew();
			split.SplitReference = "01";
			CusHawbTests.PiecesReleasedTestRunner<SplitHouse>(split, delegate (string p)
			{ hawb.Profile = p; });
		}

		public void TestValidateBadge()
		{
			AirCargoInventory.Testing.LicencingAndShedRestrictionsTests.EnsureAgentLxa();
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var split = hawb.Splits.AddNew();
			split.SplitReference = "01";
			split.AgentBadge = "XXX";
			AssertHasMessageErrorContaining(split.AgentBadgeInfo, "list");
			split.AgentBadge = "";
			AssertHasMessageErrorContaining(split.AgentBadgeInfo, "badge");
			split.AgentBadge = "LXA";
			AssertNoMessageErrorContaining(split.AgentBadgeInfo, "badge");
			AssertNoMessageErrorContaining(split.AgentBadgeInfo, "list");
		}

		public void TestDelete()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "CUKFFW98000CAR";
			var hawb = mawb.ChildBills.AddNew();
			var split = hawb.Splits.AddNew();
			var declaration = ((ICcsukCusAwb)split).CreateNewStandaloneCDSDeclaration();
			split.SplitReference = "01";
			split.TemporaryStorageEndDate = DateTime.Now;
			split.HandlingInformation = "ABC";
			var handlingCodes = ((ICcsukCusAwb)split).CommunityHandlingCodes;
			var handlingCode1 = handlingCodes.AddNew();
			handlingCode1.Data.C4_SplitReferenceToWhichThisPertains = split.SplitReference;
			handlingCode1.Data.C4_CommunityHandlingCode = "DEF";
			Factory.Save();

			split.Delete();
			Factory.Save();

			AssertNotNull(Factory.Load<JobDeclaration>(declaration.PK));
			AssertNull(Factory.LoadTop1<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1ID, split.PK)));
			AssertNull(Factory.LoadTop1<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_ParentID, split.PK)));
			AssertNull(Factory.LoadTop1<CusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, split.PK)));
		}

		class SplitHouseWithPublicReadOnlyBadgeForTest : SplitHouse
		{
			public SplitHouseWithPublicReadOnlyBadgeForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{ }

			public bool BadgeReadOnlyForTest_ThisIsOnlyHereBecauseZPropertyInfoIsABitSucky
			{
				get { return AgentBadgeReadOnly; }
			}
		}
	}

	[TestedType(typeof(SplitBasic))]
	class SplitBasicTests : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestSaveDoesntSetStatus1DateWithoutPieces()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			basic.AgentBadge = "LXA";
			basic.NumberOfPiecesExpected = 0;
			var split = basic.Splits.AddNew();
			split.SplitReference = "01";
			Factory.Save();
			AssertEquals(ZDateTime.Empty, split.Status1Date);
			AssertEquals(ZDateTime.Empty, basic.Status1Date);
		}

		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestSaveDoesntResetStatus1Date()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			basic.AgentBadge = "LXA";
			basic.NumberOfPiecesExpected = 10;
			Factory.Save();

			var split = basic.Splits.AddNew();
			split.SplitReference = "01";
			split.NumberOfPiecesExpected = 10;

			var ot = basic.OutTurns.AddNew();
			ot.C5_PackagesOutturned = 10;
			ot.SplitReferenceToWhichThisPertains = "01";
			Factory.Save();
			AssertEquals(new ZDateTime(2015, 8, 22, 14, 0, 0), split.Status1Date);

			var djc = new ZDateTime(1979, 8, 9, 9, 56, 0);
			split.Status1Date = djc;
			AssertEquals(djc, split.Status1Date);
			Factory.Save();
			AssertEquals(djc, split.Status1Date);
		}

		public void TestHasEntryWithLodgedOrPrelodgedWithCustoms_CDS()
		{
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000CAR";
			var split = basic.Splits.AddNew();
			ICcsukCusAwb awb = split;
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			var dec = awb.CreateNewStandaloneCDSDeclaration();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			var entry = dec.CustomsEntryHeaders.AddNew();
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);

			entry.EntryNumber = "123";
			entry.CH_EntryStatus = ZString.Empty;
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);

			entry.CH_EntryStatus = "REJ";
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);

			new List<string> { "ACC", "RCV", "CLR", "CAN" }.ForEach(x =>
			{
				entry.CH_EntryStatus = x;
				AssertEquals(true, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			});
		}

		[ExpectNoExceptions]
		public void TestHandlingInformationViaGenAddOnDoesNotExplode()
		{
			var basic = Factory.New<CusMAWB>();
			basic.AgentBadge = "DVG";
			var split = basic.Splits.AddNew();
			var throwAway = split.HandlingInformation;
			split.HandlingInformation = ZString.Empty;
			AssertEquals(ZString.Empty, split.HandlingInformation);
			split.HandlingInformation = "ABC";
			AssertEquals("ABC", split.HandlingInformation);
			split.HandlingInformation = ZString.Empty;
			AssertEquals(ZString.Empty, split.HandlingInformation);
			split.HandlingInformation = "XYZ";
			AssertEquals("XYZ", split.HandlingInformation);
			Factory.Save();
			AssertEquals("XYZ", split.HandlingInformation);
			var splitReloaded = new BusinessObjectFactory().Load<SplitBasic>(split.PK);
			AssertEquals("XYZ", splitReloaded.HandlingInformation);
		}

		public void TestHandlingInformationForBinding()
		{
			var basic = Factory.New<CusMAWB>();
			var split = basic.Splits.AddNew();
			SplitHouseTests.RunHandlingInformationForBindingTest(split, basic.OutTurns);
		}

		public void TestAgentBadge()
		{
			var basic = Factory.New<CusMAWB>();
			basic.AgentBadge = "DVG";
			var split = basic.Splits.AddNew();
			AssertEquals("Split's badge is automatically set from parent", "DVG", split.AgentBadge);
			split.AgentBadge = "ZPE";
			AssertEquals("Split's badge is now its own", "ZPE", split.AgentBadge);
			AssertEquals("Split's badge does not clobber parent's", "DVG", basic.AgentBadge);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			AssertEquals("Split's badge saved OK", "ZPE", newFactory.Load<SplitBasic>(split.PK).AgentBadge);
			AssertEquals("Split's badge doesn't clobber parent's", "DVG", newFactory.Load<CusMAWB>(basic.PK).AgentBadge);
		}

		public void TestAgentBadgeReadOnly()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000LXA";
			var split = Factory.New<SplitBasicWithPublicReadOnlyBadgeForTest>();
			basic.Splits.Add(split);
			AssertEquals("Split's badge is locked for agent", true, split.BadgeReadOnlyForTest_ThisIsOnlyHereBecauseZPropertyInfoIsABitSucky);
			basic.Profile = "CUKAIR98LHRBAC";
			AssertEquals("Split's badge still locked - must inherit from parent", true, split.BadgeReadOnlyForTest_ThisIsOnlyHereBecauseZPropertyInfoIsABitSucky);
		}

		public void TestSetEcStatusRelease()
		{
			var basic = Factory.New<CusMAWB>();
			var split = basic.Splits.AddNew();
			split.SplitReference = "01";
			var ot = basic.OutTurns.AddNew();
			ot.SplitReferenceToWhichThisPertains = split.SplitReference;

			var splitIrrelevant = basic.Splits.AddNew();
			splitIrrelevant.SplitReference = "02";
			var otSplit2 = basic.OutTurns.AddNew();
			otSplit2.SplitReferenceToWhichThisPertains = splitIrrelevant.SplitReference;
			otSplit2.IsReleasedAlready = true;
			CusHawbTests.RunSetEcStatusReleaseTest(split, null, ot);
			AssertEquals("Irrelevant split is not un-released", true, otSplit2.IsReleasedAlready);
		}

		public void TestUnsetEcStatusWipesOldEdocs()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "11122222222";
			var split = basic.Splits.AddNew();
			split.SplitReference = "44";
			var eDocEcRra = split.DocManagerInfo.AddFileOrDocument(ZBlob.FromAscii("Daniel"), "RRA.txt", "RRA");
			var eDocIrrelevant = split.DocManagerInfo.AddFileOrDocument(ZBlob.FromAscii("Clarke"), "Foo.txt", "GRA");
			eDocEcRra.Description = "Company - Branch - Release/Removal Authority for 111-22222222/44 reprint blah";
			eDocIrrelevant.Description = "Company - Branch - Whatever";
			Factory.Save();
			((ICcsukCusAwb)split).SetEcStatusRelease(false);
			Factory.Save();
			var splitReloaded = new BusinessObjectFactory().Load<SplitBasic>(split.PK);
			AssertEquals(1, split.DocManagerInfo.AllEDocs.Count);
		}

		[TestDate(1986, 3, 12, 4, 0, 1)]
		public void TestLocalCreationTime()
		{
			var split = (ICcsukCusAwb)GetNewBusinessObject();
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 0, 1), split.LocalCreationDate);
		}

		public void TestUpdateStatusToCacIfAllowed()
		{
			var basic = Factory.New<CusMAWB>();
			var split1 = (SplitBasic)basic.Splits.AddNew();
			var split2 = (SplitBasic)basic.Splits.AddNew();
			var split3 = (SplitBasic)basic.Splits.AddNew();
			CusHawbTests.RunUpdateStatusToCacIfAllowedTest(split1, CustomsStatusCodes.Codes.EntryOrRequestAccepted);
			CusHawbTests.RunUpdateStatusToCacIfAllowedTest(split2, CustomsStatusCodes.Codes.EntryOrRequestCancelled);
			CusHawbTests.RunUpdateStatusToCacIfAllowedTest(split3, CustomsStatusCodes.Codes.CustomsQueriedDetained);
		}

		public void TestIsPrearrival()
		{
			var basic = Factory.New<CusMAWB>();
			var split = basic.Splits.AddNew();
			AssertEquals(true, split.IsPrearrival);
			basic.NumberOfPiecesReceived = 69;
			AssertEquals(false, split.IsPrearrival);
			basic.NumberOfPiecesReceived = 0;
			basic.CM_ArrivalDate = ZDateTime.Now;
			AssertEquals(false, split.IsPrearrival);
			basic.NumberOfPiecesReceived = 0;
			basic.CM_ArrivalDate = ZDateTime.Empty;
			AssertEquals(true, split.IsPrearrival);
			basic.NumberOfPiecesReceived = 6;
			AssertEquals(false, split.IsPrearrival);
			basic.NumberOfPiecesReceived = 0;
			split.NumberOfPiecesReceived = 6;
			AssertEquals(false, split.IsPrearrival);
		}

		public void TestCompleteOnCcsukAndIsCompleteOnCcsuk()
		{
			var basic = Factory.New<CusMAWB>();
			ICcsukCusAwb split1 = basic.Splits.AddNew();
			ICcsukCusAwb split2 = basic.Splits.AddNew();
			split1.SetCustomsActionCode(CustomsStatusCodes.Codes.ClearedByCustoms, ZDateTime.BrettsBirthday);
			Assert(!split1.IsCompleteOnCcsuk);
			split1.CompleteOnCcsuk();
			Assert(split1.IsCompleteOnCcsuk);
			Assert(!((ICcsukCusAwb)basic).IsCompleteOnCcsuk);
			CusHawbTests.AssertHasStatus3Logs(split1, CustomsStatusCodes.Codes.ClearedByCustoms);
			split2.CompleteOnCcsuk();
			Assert(split2.IsCompleteOnCcsuk);
			Assert(((ICcsukCusAwb)basic).IsCompleteOnCcsuk);
			split2.UncompleteOnCcsuk();
			Assert(!split2.IsCompleteOnCcsuk);
			Assert(split1.IsCompleteOnCcsuk);
			Assert(!((ICcsukCusAwb)basic).IsCompleteOnCcsuk);

			// Now check that setting st1 with st3, and st3 with st1, marks as complete
			RunSetStatus1AndStatus3TestForCompleteness(split2);
		}

		public void TestCompleteOnCcsukAndIsCompleteOnCcsuk_BasicSplitWithHawb()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.ChildBills.AddNew();
			var split = Factory.New<SplitBasic>();
			split.AWB = mawb;
			Factory.Save();
			((ICcsukCusAwb)split).CompleteOnCcsuk();
			Assert(((ICcsukCusAwb)split).IsCompleteOnCcsuk);
		}

		internal static void RunSetStatus1AndStatus3TestForCompleteness(ICcsukCusAwb awb)
		{
			awb.NumberOfPiecesExpected = 10;
			awb.SetCustomsActionCode(CustomsStatusCodes.Codes.ClearedByCustoms, ZDateTime.Now);
			Assert(!awb.IsCompleteOnCcsuk);
			awb.NumberOfPiecesReceived = 10;
			Assert(!awb.IsCompleteOnCcsuk);
			awb.Status1Date = ZDateTime.BrettsBirthday;
			Assert(awb.IsCompleteOnCcsuk);
			awb.Status1Date = ZDateTime.Empty;
			Assert(!awb.IsCompleteOnCcsuk);
			awb.SetCustomsActionCode(CustomsStatusCodes.Codes.EntryOrRequestCancelled, ZDateTime.Now);
			Assert(!awb.IsCompleteOnCcsuk);
			awb.Status1Date = ZDateTime.BrettsBirthday;
			awb.SetCustomsActionCode(CustomsStatusCodes.Codes.ClearedByCustoms, ZDateTime.Now);
			Assert(awb.IsCompleteOnCcsuk);
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestStatus1()
		{
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000ABC";
			var split = (SplitBasic)basic.Splits.AddNew();
			CusHawbTests.RunStatus1Test(split);
		}

		public void TestReferenceNumbersAndHumanReadableName()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "12387654321";
			basic.CargoTerminalOperatorAirport = "LHR";
			basic.CargoTerminalOperator = "CAX";
			var split = basic.Splits.AddNew();
			split.SplitReference = "03";
			split.NumberOfPiecesExpected = 69;
			AssertEquals("123-87654321/03", split.ReferenceNumber);
			AssertEquals("Split Basic 123-87654321/03 (69 pieces)", split.HumanReadableNameForFormCaption);
			AssertEquals("12387654321        03", ((ICcsukCusAwb)split).ChiefMasterUCRReferenceSuffix);
			AssertEquals("LHRCAX-123-87654321/03", split.ReferenceNumberWithShed);
		}

		public void TestArchiveOnCcsukAndIsArchivedOnCcsuk()
		{
			var basic = Factory.New<CusMAWB>();
			ICcsukCusAwb split1 = basic.Splits.AddNew();
			ICcsukCusAwb split2 = basic.Splits.AddNew();
			Assert(!split1.IsArchivedOnCcsuk);
			split1.ArchiveOnCcsuk(ReasonForArchiving.NprFewerThanNpxAndFinalCustomsActionDateOlderThan180Days);
			Assert(split1.IsArchivedOnCcsuk);
			Assert(!((ICcsukCusAwb)basic).IsArchivedOnCcsuk);
			split2.ArchiveOnCcsuk(ReasonForArchiving.Status1DateAndCacDatesBothOlderThanOneWeek);
			Assert(split2.IsArchivedOnCcsuk);
			Assert(((ICcsukCusAwb)basic).IsArchivedOnCcsuk);
		}

		public void TestArchiveOnCcsukAndIsArchivedOnCcsuk_BasicSplitWithHawb()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.ChildBills.AddNew();
			var split = Factory.New<SplitBasic>();
			split.AWB = mawb;
			Factory.Save();
			((ICcsukCusAwb)split).ArchiveOnCcsuk(ReasonForArchiving.NprFewerThanNpxAndFinalCustomsActionDateOlderThan180Days);
			Assert(((ICcsukCusAwb)split).IsArchivedOnCcsuk);
		}

		public void TestDeclarationFunctionality()
		{
			var split1 = (SplitBasic)GetNewBusinessObject();
			split1.Basic.Profile = "CUKFFW98000XXX";
			split1.Basic.CM_MAWB = "11122222222";
			split1.Basic.CM_ArrivalDate = new ZDateTime(1986, 3, 12);
			split1.Basic.CM_FlightNo = "BA123";
			split1.Basic.DescriptionOfGoods = "Stuff";
			split1.NumberOfPiecesExpected = 9;
			AssertNull(split1.OwnDeclaration);
			Assert(!split1.HasOwnDeclaration);
			AssertEquals("", split1.ChiefDeclarationUCR);
			split1.Basic.CM_JK = Factory.New<Freight.Forwarding.Business.ForwardingConsol>().PK;
			var dec1 = ((ICcsukCusAwb)split1).CreateNewStandaloneCDSDeclaration();
			split1.Basic.CM_JK = ZGuid.Empty;
			SplitHouseTests.RunSecondHalfOfSplitDeclarationTests(split1, out dec1);
		}

		public void TestPresenceOnNetworkStatus()
		{
			var basic = Factory.New<CusMAWB>();
			basic.PresenceOnNetworkStatus = "Y";
			var split = (SplitBasic)basic.Splits.AddNew();
			split.PresenceOnNetworkStatus = "X";
			AssertEquals("X", split.PresenceOnNetworkStatus);
			Factory.Save();
			var splitReloaded = Factory.Load<SplitBasic>(split.PK);
			AssertEquals("X", splitReloaded.PresenceOnNetworkStatus);
		}

		public void TestPiecesReleased()
		{
			var basic = Factory.New<CusMAWB>();
			var split = basic.Splits.AddNew();
			split.SplitReference = "01";
			CusHawbTests.PiecesReleasedTestRunner<SplitBasic>(split, delegate (string p)
			{ basic.Profile = p; });
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var mawb = Factory.New<CusMAWB>();
			var split = (SplitBasic)mawb.Splits.AddNew();
			split.SplitReference = "03";
			return split;
		}

		public void TestDocManagerInfoAndDocumentSupporter()
		{
			var basic = Factory.New<CusMAWB>();
			var splitBasic = basic.Splits.AddNew();
			AssertType(typeof(CcsukDocumentSupporter), splitBasic.DocumentSupporter);
			AssertType(typeof(SplitConsignmentDocManagerInfo), ((IDocManagerSupport)splitBasic).DocManagerInfo);
		}

		public void TestOutturns()
		{
			var basic = Factory.New<CusMAWB>();
			var split1 = basic.Splits.AddNew();
			var split2 = basic.Splits.AddNew();
			split1.SplitReference = "01";
			split2.SplitReference = "02";
			var outturn1 = basic.OutTurns.AddNew();
			var outturn2 = basic.OutTurns.AddNew();
			outturn1.SplitReferenceToWhichThisPertains = "01";
			outturn2.SplitReferenceToWhichThisPertains = "02";

			AssertEquals(1, ((ICcsukCusAwb)split1).OutTurns.Count);
			AssertEquals(outturn1.PK, ((ICcsukCusAwb)split1).OutTurns[0].PK);
			AssertEquals(1, ((ICcsukCusAwb)split2).OutTurns.Count);
			AssertEquals(outturn2.PK, ((ICcsukCusAwb)split2).OutTurns[0].PK);
		}

		public void TestProperties()
		{
			var basic = Factory.New<CusMAWB>();
			basic.AgentBadge = "DAN";
			var split = (SplitBasic)basic.Splits.AddNew();
			split.SplitReference = "03";
			split.Weight = 123m;
			split.WeightCode = "KG";
			var mocker = Factory.NewMoq<EDIMessageDummyForTest_2343233>();
			mocker.Protected().Setup<string>("GetMessageReferenceNumber").Returns("2343233");
			var message = mocker.Object;
			basic.Messages.Add(message);
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			basic.CargoTerminalOperator = "BAC";
			basic.CargoTerminalOperatorAirport = "BAC";
			basic.CM_MAWB = "12512345678";

			AssertEquals("03", split.SplitReference);
			AssertEquals("KG", split.WeightCode);
			AssertEquals(123m, split.Weight);
			AssertEquals(basic.PK, split.AWB.PK);
			AssertEquals("125-12345678", split.MasterBill);
			AssertEquals("BAC", split.CargoTerminalOperator);
			AssertEquals("BAC", split.CargoTerminalOperatorAirport);
			AssertEquals("DAN", split.AgentBadge);
			AssertType(typeof(SplitToFsrProvider), split.GetAwbToFsrProvider(new CcsukTransmissionMessageFunction.CUKFSR.FSN()));

			Factory.Save();
			split = new BusinessObjectFactory().Load<SplitBasic>(split.PK);
			AssertEquals("03", split.SplitReference);
			AssertEquals("KG", split.WeightCode);
			AssertEquals(123m, split.Weight);
			AssertEquals(basic.PK, split.AWB.PK);
			AssertEquals("125-12345678", split.MasterBill);
			AssertEquals("BAC", split.CargoTerminalOperator);
			AssertEquals("BAC", split.CargoTerminalOperatorAirport);
			AssertEquals("DAN", split.AgentBadge);
			mocker.VerifyAll();
		}

		public void TestSettingPiecesDoesntUpdateParent()
		{
			var basic = Factory.New<CusMAWB>();
			var split1 = basic.Splits.AddNew();
			split1.NumberOfPiecesReceived = 10;
			AssertEquals(0, (int)basic.NumberOfPiecesReceived);
			split1.NumberOfPiecesExpected = 11;
			AssertEquals(0, (int)basic.NumberOfPiecesExpected);
		}

		public void TestModuleControllerId()
		{
			var basic = Factory.New<CusMAWB>();
			var split = basic.Splits.AddNew();
			AssertEquals(ControllerIDs.Customs.GB.CcsukSplitBasicController, split.ModuleControllerId);
		}

		public void TestDelete()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "CUKFFW98000CAR";
			var split = mawb.Splits.AddNew();
			var declaration = ((ICcsukCusAwb)split).CreateNewStandaloneCDSDeclaration();
			split.SplitReference = "01";
			split.TemporaryStorageEndDate = DateTime.Now;
			split.HandlingInformation = "ABC";
			var handlingCodes = ((ICcsukCusAwb)split).CommunityHandlingCodes;
			var handlingCode1 = handlingCodes.AddNew();
			handlingCode1.Data.C4_SplitReferenceToWhichThisPertains = split.SplitReference;
			handlingCode1.Data.C4_CommunityHandlingCode = "DEF";
			Factory.Save();

			split.Delete();
			Factory.Save();

			AssertNotNull(Factory.Load<JobDeclaration>(declaration.PK));
			AssertNull(Factory.LoadTop1<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1ID, split.PK)));
			AssertNull(Factory.LoadTop1<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_ParentID, split.PK)));
			AssertNull(Factory.LoadTop1<CusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, split.PK)));
		}

		class SplitBasicWithPublicReadOnlyBadgeForTest : SplitBasic
		{
			public SplitBasicWithPublicReadOnlyBadgeForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{ }
			public bool BadgeReadOnlyForTest_ThisIsOnlyHereBecauseZPropertyInfoIsABitSucky
			{
				get { return AgentBadgeReadOnly; }
			}
		}
	}

	[TestedType(typeof(SplitCollection))]
	class SplitCollectionTests : BusinessObjectCollectionTestCase
	{
		public void TestListOfSplits()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "123-12345678";
			var split1 = basic.Splits.AddNew();
			var split2 = basic.Splits.AddNew();
			split1.SplitReference = "01";
			split1.NumberOfPiecesExpected = 69;
			split2.SplitReference = "02";
			split2.NumberOfPiecesExpected = 68;
			var tsr = basic.TSRs.AddNew();
			var cdpl = new CodeDescriptionPairList();
			cdpl.AddRange(basic.Splits);
			AssertEquals("123-12345678/01 (69 pieces)", cdpl.GetDescriptionFromCode("01"));
			AssertEquals("123-12345678/02 (68 pieces)", cdpl.GetDescriptionFromCode("02"));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			return new SplitCollection(hawb);
		}

		public void TestAddNewMethod()
		{
			var basic = Factory.New<CusMAWB>();
			var splitBasic = basic.Splits.AddNew();
			AssertEquals(ZGuid.Empty, splitBasic.CG_CS);
			AssertEquals(basic.PK, splitBasic.CG_CM_LinkToPartMaster);
			AssertCollectionContains(splitBasic, basic.Splits);
			AssertType(typeof(SplitBasic), splitBasic);

			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var splitHouse = hawb.Splits.AddNew();
			AssertEquals(ZGuid.Empty, splitHouse.CG_CM_LinkToPartMaster);
			AssertEquals(hawb.PK, splitHouse.CG_CS);
			AssertCollectionContains(splitHouse, hawb.Splits);
			AssertType(typeof(SplitHouse), splitHouse);
		}

		public void TestAddMethod()
		{
			var basic = Factory.New<CusMAWB>();
			var splitBasic = Factory.New<SplitBasic>();
			basic.Splits.Add(splitBasic);
			AssertEquals(basic.PK, splitBasic.CG_CM_LinkToPartMaster);
			AssertEquals(ZGuid.Empty, splitBasic.CG_CS);

			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var splitHouse = Factory.New<SplitHouse>();
			hawb.Splits.Add(splitHouse);
			AssertEquals(hawb.PK, splitHouse.CG_CS);
			AssertEquals(ZGuid.Empty, splitHouse.CG_CM_LinkToPartMaster);

			AssertExceptionThrown(typeof(NotSupportedException), delegate
			{ hawb.Splits.Add(splitBasic); });
			AssertExceptionThrown(typeof(NotSupportedException), delegate
			{ basic.Splits.Add(splitHouse); });
		}

		public void TestSplitReferenceIndexer()
		{
			var split1 = Factory.New<SplitHouse>();
			var split2 = Factory.New<SplitHouse>();
			var coll = new SplitCollection(Factory);
			coll.Add(split1);
			coll.Add(split2);
			split1.SplitReference = "01";
			split2.SplitReference = "02";
			AssertEquals(split1.PK, coll["01"].PK);
			AssertEquals(split2.PK, coll["02"].PK);
		}
	}

	public class EDIMessageDummyForTest_2343233 : EDIMessage
	{
		public EDIMessageDummyForTest_2343233(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "2343233";
		}
	}
}
