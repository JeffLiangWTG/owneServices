using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOREMMessageBuilderTest : CTOMessageBuilderTest
	{
		public void TestAir()
		{
			line.IsAir = true;
			line.CTOEstablishmentID = "CTOEST";
			line.CustomsAuthorityNumber = "CANCAN";
			line.AirWaybill = "12345";

			string message = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+111:::CTOREM+" + GetReference(0) + @":1'
LOC+202+CTOEST::95'
TDT+20+++6'
CNI+1+:::I'
RFF+AWB:12345'
GID+1'
RFF+TN:CANCAN'
GID+1'
UNT+10+<<MSGNO PLACEHOLDER>>'";

			AssertMultilineASCIIEquals("message", message, builder.MessageText.Replace("'", "'\r\n"));
		}

		protected override CMRMessageBuilder GetMessageBuilderToTest() => new CTOREMMessageBuilder(line);

		protected override string MessageName => "CTOREM";

		protected override string MessageCode => DocumentNameCodeList.TransportMovementGateOutReport;
	}
}
