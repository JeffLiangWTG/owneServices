using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.DocumentWrappers.Freight.Testing
{
	public class DeclarationWrapperTest : TestCaseWithFactory
	{
		[TestDate(1986, 3, 12)]
		public void TestEverythingWithShipment()
		{
			var consol = DeclarationWrapperCollectionTest.SetupConsolAndDeclarations(Factory);
			Factory.Save();

			var consolWrapper = DocForwardingConsolGB.New(consol, Factory);
			var wrapper1SingleDecShipment = consolWrapper.Declarations[0];
			var wrapper2MultiEntryShipment = consolWrapper.Declarations[1];
			var wrapper3CStatus = consolWrapper.Declarations[2];
			var wrapper4ExternalBrokerage = consolWrapper.Declarations[3];

			AssertContains("6-S00001000\r\n", wrapper1SingleDecShipment.AdsParticipant.DeclarationUCRs);
			AssertEquals("6-S00001001\r\n6-S00001001/1\r\n", wrapper2MultiEntryShipment.AdsParticipant.DeclarationUCRs);
			AssertEquals("C STATUS GOODS", wrapper3CStatus.AdsParticipant.DeclarationUCRs);
			AssertEquals("7GB1234567-987654\r\n7GB1234567-987655/2", wrapper4ExternalBrokerage.AdsParticipant.DeclarationUCRs);

			AssertEquals("120-12345-12/03/1986\r\n", wrapper1SingleDecShipment.AdsParticipant.ChiefEntryReferences);
			AssertEquals("multi-entry dec has many entry numbers", "333-33333-19/02/1986\r\n444-55555-18/02/1986\r\n", wrapper2MultiEntryShipment.AdsParticipant.ChiefEntryReferences);
			AssertEquals("C-ship has no entry number", "", wrapper3CStatus.AdsParticipant.ChiefEntryReferences);
			AssertEquals("CT status directly from shipment", "C", wrapper3CStatus.AdsParticipant.CtStatus);
			AssertEquals("CT status directly from shipment", "X", wrapper4ExternalBrokerage.AdsParticipant.CtStatus);

			AssertEquals("Number of HAWBs/Shipments detailed on the ADS", 4, consolWrapper.Declarations.Count);
			AssertEquals("Total number of DUCRs listed - all internal entries, all external entries, exluding C-shipments. One on w1, 2 on w2, none on w3, 2 externals on w4.", 5, consolWrapper.EntriesCount);
			AssertEquals(100, wrapper1SingleDecShipment.AdsParticipant.TotalNoOfPacks);
			AssertEquals(1000m, wrapper1SingleDecShipment.AdsParticipant.TotalWeightAds);
			AssertEquals("LHR", wrapper1SingleDecShipment.AdsParticipant.Origin);
			AssertEquals("MEL", wrapper1SingleDecShipment.AdsParticipant.FinalDestination);
			AssertEquals("Stuff", wrapper1SingleDecShipment.AdsParticipant.GoodsDescription);
			AssertEquals("12345678", wrapper1SingleDecShipment.AdsParticipant.HouseBill);
			AssertEquals("X", wrapper1SingleDecShipment.AdsParticipant.CtStatus);
		}
	}

	[TestedType(typeof(DeclarationWrapperCollection))]
	public class DeclarationWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DeclarationWrapperCollection>
	{
		protected override System.Type GetExpectedCollectionType()
		{
			return typeof(DeclarationWrapperCollection);
		}

		protected override DeclarationWrapperCollection GetCollectionToTest()
		{
			return new DeclarationWrapperCollection(SetupConsolAndDeclarations(Factory), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var consol = SetupConsolAndDeclarations(Factory);
			var consolWrapper = DocForwardingConsolGB.New(consol, Factory);
			return consolWrapper.Declarations[3];
		}

		internal static ForwardingConsol SetupConsolAndDeclarations(BusinessObjectFactory factory)
		{
			var consol = factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "GBLON";
			var declaration1InternalEntry = factory.New<JobDeclaration>();
			declaration1InternalEntry.JE_RL_NKOrigin = "GBLHR";
			declaration1InternalEntry.JE_RL_NKFinalDestination = "AUMEL";
			declaration1InternalEntry.JE_HouseBill = "12345678";
			declaration1InternalEntry.JE_TransportMode = "AIR";
			declaration1InternalEntry.JE_MessageType = "EXP";
			declaration1InternalEntry.JE_GoodsDescription = "Stuff";
			declaration1InternalEntry.JE_JS = shipment1.PK;
			declaration1InternalEntry.JE_TotalNoOfPacks = 100;
			declaration1InternalEntry.JE_TotalWeight = 1000;
			declaration1InternalEntry.JE_TotalWeightUnit = "KG";
			var ceh1 = declaration1InternalEntry.CustomsEntryHeaders.AddNew();
			ceh1.EntryNumber = "120-12345";
			ceh1.CusEntryNumber.CE_IssueDate = ZDateTime.Now;

			var shipment2MultipleInternalEntries = consol.Shipments.AddNew();
			shipment2MultipleInternalEntries.JS_RL_NKOrigin = "GBMAN";
			var multiEntryDeclaration2 = factory.New<JobDeclaration>();
			multiEntryDeclaration2.JE_JS = shipment2MultipleInternalEntries.PK;
			multiEntryDeclaration2.JE_TransportMode = "AIR";
			multiEntryDeclaration2.JE_MessageType = "EXP";
			multiEntryDeclaration2.JE_TotalNoOfPacks = 200;
			multiEntryDeclaration2.JE_JS = shipment2MultipleInternalEntries.PK;
			multiEntryDeclaration2.JE_TotalWeight = 2000;
			multiEntryDeclaration2.JE_TotalWeightUnit = "KG";
			var ceh21 = multiEntryDeclaration2.CustomsEntryHeaders.AddNew();
			var ceh22 = multiEntryDeclaration2.CustomsEntryHeaders.AddNew();
			ceh21.EntryNumber = "333-33333";
			ceh21.CusEntryNumber.CE_IssueDate = ZDateTime.Now.AddDays(-21);
			ceh22.EntryNumber = "444-55555";
			ceh22.CusEntryNumber.CE_IssueDate = ZDateTime.Now.AddDays(-22);

			var shipment3CStatus = consol.Shipments.AddNew();
			shipment3CStatus.JS_TransportMode = "AIR";
			shipment3CStatus.JS_RL_NKOrigin = "GBEDI";
			shipment3CStatus.JS_OuterPacks = 300;
			shipment3CStatus.JS_ActualWeight = 3000;
			shipment3CStatus.JS_CommunityTransitStatus = ExportCommunityTransitStatusList.Codes.C;

			var shipment4WithoutDeclarationButNotExGB = consol.Shipments.AddNew();
			shipment4WithoutDeclarationButNotExGB.JS_CommunityTransitStatus = ExportCommunityTransitStatusList.Codes.C;
			shipment4WithoutDeclarationButNotExGB.JS_OuterPacks = 100;
			shipment4WithoutDeclarationButNotExGB.JS_ActualWeight = 50;
			shipment4WithoutDeclarationButNotExGB.JS_RL_NKOrigin = "FRPAR";
			shipment4WithoutDeclarationButNotExGB.JS_RL_NKDestination = "AUSYD";

			var shipment5WithExternalDucrs = consol.Shipments.AddNew();
			shipment5WithExternalDucrs.JS_CommunityTransitStatus = ExportCommunityTransitStatusList.Codes.X;
			shipment5WithExternalDucrs.JS_OuterPacks = 1000;
			shipment5WithExternalDucrs.JS_ActualWeight = 500;
			shipment5WithExternalDucrs.JS_RL_NKOrigin = "GBLHR";
			shipment5WithExternalDucrs.JS_RL_NKDestination = "AUSYD";
			AddNewExternalDucr(shipment5WithExternalDucrs, "7GB1234567-987654");
			AddNewExternalDucr(shipment5WithExternalDucrs, "7GB1234567-987655/2");

			return consol;
		}

		static void AddNewExternalDucr(ForwardingShipment shipmentWithExternalDucrs, string ducrAndpart)
		{
			var number = shipmentWithExternalDucrs.Numbers.AddNew();
			number.CE_EntryNum = ducrAndpart;
			number.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
		}
	}
}
