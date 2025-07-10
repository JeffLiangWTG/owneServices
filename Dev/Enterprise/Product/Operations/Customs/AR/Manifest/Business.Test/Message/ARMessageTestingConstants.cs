namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	public static class ARMessageTestingConstants
	{
		public const string MessageDirectory = @"Enterprise\Product\Operations\Customs\AR\Manifest\Business.Test\Message\";

		public const string TestFilesAirDirectory = MessageDirectory + @"TestFiles\AirMode";

		public const string TestFilesSeaDirectory = MessageDirectory + @"TestFiles\SeaMode";

		public const string TestFilesAirResponseDirectory = TestFilesAirDirectory + @"\Response";

		public const string TestFilesSeaResponseDirectory = TestFilesSeaDirectory + @"\Response";

		public const string AirAcceptedResponse = TestFilesAirResponseDirectory + @"\Accepted.xml";

		public const string AirRejectedResponse = TestFilesAirResponseDirectory + @"\Rejected.xml";

		public const string SeaAcceptedResponse = TestFilesSeaResponseDirectory + @"\Accepted.xml";

		public const string SeaRejectedResponse = TestFilesSeaResponseDirectory + @"\Rejected.xml";

		public const string XTAcceptedResponse = @"XTConfigurationResponseMessageSuccess.xml";

		public const string XTRejectedResponse = @"XTConfigurationResponseMessageFailure.xml";

		public const string XTOutgoingInterchange = @"OutgoingInterchange.xml";
	}
}
