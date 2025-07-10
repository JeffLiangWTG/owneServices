using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CTOMessageBuilderTest : CMRMessageBuilderAbstractTest
	{
		public void TestUNHMessageReferenceNumber()
		{
			AssertLine("UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'", 0);
		}

		public void TestBGMSendersReference()
		{
			AssertLine("BGM+" + MessageCode + ":::" + MessageName + "+" + GetReference(0) + ":1'", 1);
		}

		public void TestBGMSendersReferenceOriginal()
		{
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertLine("BGM+" + MessageCode + ":::" + MessageName + "+" + GetReference(1) + ":1+9'", 1);
		}

		public void TestLOCCTOEstablishmentID()
		{
			line.CTOEstablishmentID = "foo";
			AssertLine("LOC+202+FOO::95'", 2);
		}

		public void TestTDTModeOfTransportAir()
		{
			line.IsAir = true;
			AssertLine("TDT+20+++6'", 2);
		}

		public void TestTDTModeOfTransportSea()
		{
			line.IsAir = false;
			AssertLine("TDT+20+++11'", 2);
		}

		public void TestCNILine()
		{
			AssertLine("CNI+1+:::I'", 3);
		}

		public void TestRFFContainerNumber()
		{
			line.IsSea = true;
			line.ContainerNumber = "AAAA1111113";
			AssertLine("RFF+AAQ:AAAA1111113'", 4);
		}

		public void TestRFFContingencyCAN()
		{
			line.CustomsContingencyAuthorityNumber = "PANTS";
			AssertLine("RFF+AHV:PANTS'", 4);
		}

		public void TestRFFNonContainerisedIdentifier()
		{
			line.IsSea = true;
			line.NonContainerisedIdentifier = "MONKEYS";
			AssertLine("RFF+AKR:MONKEYS'", 4);
		}

		public void TestRFFAirWaybill()
		{
			line.IsAir = true;
			line.AirWaybill = "SPAM";
			AssertLine("RFF+AWB:SPAM'", 4);
		}

		public void TestRFFExportDeclarationExemptionCode()
		{
			line.ExportDeclarationExceptionCode = "EGGS";
			AssertLine("RFF+TL:EGGS'", 4);
		}

		public void TestRFFCustomsAuthorityNumber()
		{
			line.CustomsAuthorityNumber = "BLAH";
			AssertLine("RFF+TN:BLAH'", 4);
		}

		protected abstract string MessageName { get; }

		protected abstract string MessageCode { get; }

		protected void AssertLine(string expected, int lineNumber)
		{
			var messageText = builder.MessageText;
			var lines = messageText.Split('\'');

			if (lineNumber >= lines.Length)
			{
				Fail("No such line found.");
			}

			AssertEquals(expected, lines[lineNumber] + '\'');
		}

		protected void AssertNotContains(string expectedToBeMissing)
		{
			AssertEquals(false, builder.MessageText.Contains(expectedToBeMissing));
		}

		protected string GetReference(int numberOfOriginals) => EDIMessage.SendersReferencePlaceHolder + "/" + Env.Registry.PhysicalServerID + numberOfOriginals;

		protected override void SetUp()
		{
			base.SetUp();

			builder = GetMessageBuilderToTest();
			builder.Messages = new EDIMessageCollection(Factory.New<DummyBusinessObject>());
		}

		protected CMRMessageBuilder builder;

		protected DummyLine line = new DummyLine();

		protected class DummyLine : ICTOMessageLine
		{
			public ZString ExportDeclarationExceptionCode;
			public ZString CustomsAuthorityNumber;
			public ZString CarrierPartyID;
			public ZDate ProposedDateOfDeparture;
			public ZBool OffloadIndicator;
			public ZString VesselID;
			public ZString VoyageNumber;
			public ZString GoodsOwnerPartyID;
			public ZString OwnerName;
			public ZString GoodsDescription;
			public ZString CountryOfDestination;
			public ZString AirWaybill;
			public ZString ContainerNumber;
			public ZString NonContainerisedIdentifier;
			public ZString CustomsContingencyAuthorityNumber;
			public ZBool IsAir;
			public ZBool IsSea;
			public ZString CTOEstablishmentID;

			ZString ICTOMessageLine.ExportDeclarationExemptionCode => ExportDeclarationExceptionCode;

			ZString ICTOMessageLine.CustomsAuthorityNumber => CustomsAuthorityNumber;

			ZString ICTOMessageLine.CarrierPartyID => CarrierPartyID;

			ZDate ICTOMessageLine.ProposedDateOfDeparture => ProposedDateOfDeparture;

			ZBool ICTOMessageLine.OffloadIndicator => OffloadIndicator;

			ZString ICTOMessageLine.VesselID => VesselID;

			ZString ICTOMessageLine.VoyageNumber => VoyageNumber;

			ZString ICTOMessageLine.GoodsOwnerPartyID => GoodsOwnerPartyID;

			ZString ICTOMessageLine.OwnerName => OwnerName;

			ZString ICTOMessageLine.GoodsDescription => GoodsDescription;

			ZString ICTOMessageLine.CountryOfDestination => CountryOfDestination;

			ZString ICTOMessageLine.AirWaybill => AirWaybill;

			ZString ICTOMessageLine.ContainerNumber => ContainerNumber;

			ZString ICTOMessageLine.NonContainerisedIdentifier => NonContainerisedIdentifier;

			ZBool ICTOMessageLine.IsAir => IsAir;

			ZBool ICTOMessageLine.IsSea => IsSea;

			ZString ICTOMessageLine.CTOEstablishmentID => CTOEstablishmentID;

			EDIMessageCollection ICTOMessageLine.Messages => null;

			ZString ICTOMessageLine.CustomsContingencyAuthorityNumber => CustomsContingencyAuthorityNumber;
		}
	}
}
