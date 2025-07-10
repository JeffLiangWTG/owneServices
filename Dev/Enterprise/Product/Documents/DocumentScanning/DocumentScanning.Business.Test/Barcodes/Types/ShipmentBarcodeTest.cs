using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(ShipmentBarcode))]
	public class ShipmentBarcodeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ShipmentBarcode(MasterFactory, "[ROHCIVSYDMELH12345678901234567890]");
		}

		public void TestRefType()
		{
			AssertEquals("Shipment barcode will never be anything except SHP", Core.Constants.DocManagerCodes.Shipment, Barcode.DocManagerCode);
		}

		public void TestRefPK()
		{
			AssertEquals("RefPK on the barcode should be the same PK as the shipment with the same housebill", MatchingShipment1.PK, Barcode.RefPK);

			Barcode = new ShipmentBarcode(MasterFactory, "[EDICIVSYDADL1111111111]");
			AssertEquals("RefPK on the barcode should be the same PK as the shipment with the same housebill", MatchingShipment2.PK, Barcode.RefPK);

			Barcode = new ShipmentBarcode(MasterFactory, "[EDICIVSYDMEL1111111111]");
			AssertEquals("RefPK on the barcode returns empty guid, no matching shipment found", ZGuid.Empty, Barcode.RefPK);
		}

		public void TestRefCode()
		{
			AssertEquals("RefCode on the barcode should be the same Code as the shipment with the same housebill", MatchingShipment1[JobShipmentSchema.JS_UniqueConsignRef], Barcode.RefCode);

			Barcode = new ShipmentBarcode(MasterFactory, "[EDICIVSYDADL1111111111]");
			AssertEquals("RefCode on the barcode should be the same Code as the shipment with the same housebill", MatchingShipment2[JobShipmentSchema.JS_UniqueConsignRef], Barcode.RefCode);

			Barcode = new ShipmentBarcode(MasterFactory, "[EDICIVSYDMEL1111111111]");
			AssertEquals("RefCode on the barcode returns empty string, no matching shipment found", ZString.Empty, Barcode.RefCode);
		}

		public void TestCompany()
		{
			AssertEquals("EDI", Barcode.Company);

			Barcode = new ShipmentBarcode(MasterFactory, "ABC123");
			AssertEquals("Not valid, so company is empty", ZString.Empty, Barcode.Company);

			Barcode = new ShipmentBarcode(MasterFactory, "[ABCCIVSYDMEL12345678]");
			AssertEquals("valid, should match", "ABC", Barcode.Company);
		}

		public void TestOrigin()
		{
			AssertEquals("SYD", Barcode.Origin);

			Barcode = new ShipmentBarcode(MasterFactory, "ABC123");
			AssertEquals("Not valid, so origin is empty", ZString.Empty, Barcode.Origin);

			Barcode = new ShipmentBarcode(MasterFactory, "[ABCCIVMELSYD12345678]");
			AssertEquals("valid, should match", "MEL", Barcode.Origin);
		}

		public void TestDestination()
		{
			AssertEquals("MEL", Barcode.Destination);

			Barcode = new ShipmentBarcode(MasterFactory, "ABC123");
			AssertEquals("Not valid, so Destination is empty", ZString.Empty, Barcode.Destination);

			Barcode = new ShipmentBarcode(MasterFactory, "[ABCCIVMELSYD12345678]");
			AssertEquals("valid, should match", "SYD", Barcode.Destination);
		}

		public void TestHouseBill()
		{
			AssertEquals("12345678901234567890", Barcode.HouseBill);

			Barcode = new ShipmentBarcode(MasterFactory, "ABC123");
			AssertEquals("Not valid, so HouseBill is empty", ZString.Empty, Barcode.HouseBill);

			Barcode = new ShipmentBarcode(MasterFactory, "[ABCCIVSYDMEL12345678]");
			AssertEquals("valid, should match", "12345678", Barcode.HouseBill);
		}

		public void TestDocType()
		{
			AssertEquals("CIV", Barcode.DocType);

			Barcode = new ShipmentBarcode(MasterFactory, "ABC123");
			AssertEquals("not valid, so doctype is empty", ZString.Empty, Barcode.DocType);

			Barcode = new ShipmentBarcode(MasterFactory, "[ABCAWBSYDMEL12345678]");
			AssertEquals("Valid barcode, doctype should be AWB", "AWB", Barcode.DocType);
		}

		public void TestIsValid()
		{
			Assert(Barcode.IsValid);

			Barcode = new ShipmentBarcode(MasterFactory, "ABC123");
			Assert("invalid.. no prefix/suffix & not long enough", !Barcode.IsValid);

			Barcode = new ShipmentBarcode(MasterFactory, "EDICIVSYDMEL1]");
			Assert("invalid.. not long enough", !Barcode.IsValid);

			Barcode = new ShipmentBarcode(MasterFactory, "[EDICIVSYDMEL123456");
			Assert("invalid.. missing suffix", !Barcode.IsValid);

			Barcode = new ShipmentBarcode(MasterFactory, "[EDICVSYDMEL123456839]");
			Assert("invalid.. not 12 characters at start for company, doc, origin, destination", !Barcode.IsValid);

			Barcode = new ShipmentBarcode(MasterFactory, "[EDICIVSYDMEL123456839]");
			Assert("should be valid", Barcode.IsValid);
		}

		public void TestFirst12LettersAreCharacters()
		{
			Assert(Barcode.First12CharactersAreLetters);

			Barcode = new ShipmentBarcode(MasterFactory, "[aaaaaaaaaaaa]");
			Assert("doesn't count the first char - so this should be right", Barcode.First12CharactersAreLetters);

			Barcode = new ShipmentBarcode(MasterFactory, "[aaaaaaaaaaa1]");
			Assert("only 11 chars, this is false", !Barcode.First12CharactersAreLetters);
		}

		public void TestShipmentUsingIATACodes()
		{
			AssertEquals("the shipment on the barcode should be the shipment with same housebill, origin & destination", MatchingShipment1, Barcode.Shipment);

			Barcode = new ShipmentBarcode(MasterFactory, "[EDICIVSYDADL1111111111]");
			AssertEquals("the shipment loaded on the second barcode should match the second shipment, same housebill, origin & destination", MatchingShipment2, Barcode.Shipment);

			Barcode = new ShipmentBarcode(MasterFactory, "[EDICIVSYDMEL1111111111]");
			AssertNull("No shipment found, because nothing matches with housebill, origin & destination", Barcode.Shipment);
		}

		public void TestShipmentUsingUNLOCOCodes()
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = "GBYRK"; // no IATA code
			shipment[JobShipmentSchema.JS_RL_NKDestination] = "GBWIG"; // no IATA code
			shipment[JobShipmentSchema.JS_HouseBill] = "22222";

			Barcode = new ShipmentBarcode(MasterFactory, "[EDICIVYRKWIG22222]");

			AssertEquals("should pick up the shipment correctly based on last 3 letters of UNLOCO codes", shipment, Barcode.Shipment);
		}

		public void TestShipmentUsesIATACodesBeforeUNLOCOCodes()
		{
			RefUNLOCO loco1WithIATA = MasterFactory.New<RefUNLOCO>();
			loco1WithIATA.RL_Code = "ZZABC";
			loco1WithIATA.RL_IATA = "ABC";

			RefUNLOCO loco2WithIATA = MasterFactory.New<RefUNLOCO>();
			loco2WithIATA.RL_Code = "ZZDEF";
			loco2WithIATA.RL_IATA = "DEF";

			RefUNLOCO loco1WithoutIATA = MasterFactory.New<RefUNLOCO>();
			loco1WithoutIATA.RL_Code = "YYABC";

			RefUNLOCO loco2WithoutIATA = MasterFactory.New<RefUNLOCO>();
			loco2WithoutIATA.RL_Code = "YYDEF";

			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = "ZZABC";
			shipment[JobShipmentSchema.JS_RL_NKDestination] = "ZZDEF";
			shipment[JobShipmentSchema.JS_HouseBill] = "22222";

			BusinessObject shipmentWithoutIATACodes = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipmentWithoutIATACodes[JobShipmentSchema.JS_RL_NKOrigin] = "YYABC";
			shipmentWithoutIATACodes[JobShipmentSchema.JS_RL_NKDestination] = "YYDEF";
			shipmentWithoutIATACodes[JobShipmentSchema.JS_HouseBill] = "22222";

			Barcode = new ShipmentBarcode(MasterFactory, "[EDICIVABCDEF22222]");

			AssertEquals("The shipment should match on the first shipment in the test case because it should try using the IATA codes to match", shipment, Barcode.Shipment);

			shipment.Delete();
			ShipmentBarcode anotherBarcode = new ShipmentBarcode(MasterFactory, "[EDICIVABCDEF22222]");
			AssertEquals("Once the first shipment is deleted, it should match on the second shipment as a fall back", shipmentWithoutIATACodes, anotherBarcode.Shipment);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			SetupShipmentObjects();
			Barcode = new ShipmentBarcode(MasterFactory, "[EDICIVSYDMEL12345678901234567890]");
		}

		void SetupShipmentObjects()
		{
			MatchingShipment1 = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			MatchingShipment1[JobShipmentSchema.JS_HouseBill] = "12345678901234567890";
			MatchingShipment1[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			MatchingShipment1[JobShipmentSchema.JS_RL_NKDestination] = "AUMEL";

			MatchingShipment2 = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			MatchingShipment2[JobShipmentSchema.JS_HouseBill] = "1111111111";
			MatchingShipment2[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			MatchingShipment2[JobShipmentSchema.JS_RL_NKDestination] = "AUADL";

			UnmatchingShipment1 = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			UnmatchingShipment1[JobShipmentSchema.JS_HouseBill] = "12345678901234567890";
			UnmatchingShipment1[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			UnmatchingShipment1[JobShipmentSchema.JS_RL_NKDestination] = "AUBNE";

			UnmatchingShipment2 = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			UnmatchingShipment2[JobShipmentSchema.JS_HouseBill] = "1111111111";
			UnmatchingShipment2[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			UnmatchingShipment2[JobShipmentSchema.JS_RL_NKDestination] = "AUBNE";

			MasterFactory.Save();
		}

		DocumentFactory MasterFactory;
		ShipmentBarcode Barcode;
		BusinessObject MatchingShipment1;
		BusinessObject UnmatchingShipment1;
		BusinessObject MatchingShipment2;
		BusinessObject UnmatchingShipment2;
	}
}
