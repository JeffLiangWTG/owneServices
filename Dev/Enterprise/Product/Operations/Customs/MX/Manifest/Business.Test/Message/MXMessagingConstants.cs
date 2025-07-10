namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	public static class MXMessagingConstants
	{
		public const string MessageBuilderDirectory = @"Enterprise\Product\Operations\Customs\MX\Manifest\Business.Test\Message\";

		public const string TestFilesAirResponseDirectory = MessageBuilderDirectory + @"TestFiles\AirMode\Response";

		public const string TestFilesSeaResponseDirectory = MessageBuilderDirectory + @"TestFiles\SeaMode\Response";

		public const string TestFilesAirFirstResponseDirectory = TestFilesAirResponseDirectory + @"\FirstResponse";

		public const string TestFilesAirFinalResponseDirectory = TestFilesAirResponseDirectory + @"\FinalResponse";

		public const string TestFilesSeaFirstResponseDirectory = TestFilesSeaResponseDirectory + @"\FirstResponse";

		public const string TestFilesSeaFinalResponseDirectory = TestFilesSeaResponseDirectory + @"\FinalResponse";

		public const string AirAcceptedFirstResponse = TestFilesAirFirstResponseDirectory + @"\Accepted.xml";

		public const string AirFaultFirstResponse = TestFilesAirFirstResponseDirectory + @"\Fault.xml";

		public const string AirRejectedFirstResponse = TestFilesAirFirstResponseDirectory + @"\Rejected.xml";

		public const string AirRejectedWithEnvelopeFirstResponse = TestFilesAirFirstResponseDirectory + @"\RejectedWithEnvelope.xml";

		public const string AirAcceptedFinalResponse = TestFilesAirFinalResponseDirectory + @"\Accepted.xml";

		public const string AirRejectedFinalResponse = TestFilesAirFinalResponseDirectory + @"\Rejected.xml";

		public const string SeaAcceptedFirstResponse = TestFilesSeaFirstResponseDirectory + @"\Accepted.xml";

		public const string SeaRejectedFirstResponse = TestFilesSeaFirstResponseDirectory + @"\Rejected.xml";

		public const string SeaRejectedWithEnvelopeFirstResponse = TestFilesSeaFirstResponseDirectory + @"\RejectedWithEnvelope.xml";

		public const string SeaAcceptedFinalResponse = TestFilesSeaFinalResponseDirectory + @"\Accepted.xml";

		public const string SeaRejectedFinalResponse = TestFilesSeaFinalResponseDirectory + @"\Rejected.xml";

		public const string SeaRejectedFinalResponse2 = TestFilesSeaFinalResponseDirectory + @"\Rejected2.xml";

		public const string HeaderTextErrorNotification = TestFilesSeaResponseDirectory + @"\HeaderTextErrorNotification.txt";
	}
}
