namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	public static class CLMessagingConstants
	{
		public const string MessageBuilderDirectory = @"Enterprise\Product\Operations\Customs\CL\Manifest\Business.Test\Message\";

		public const string TestFilesAirResponseDirectory = MessageBuilderDirectory + @"TestFiles\AirMode\Response";

		public const string TestFilesSeaResponseDirectory = MessageBuilderDirectory + @"TestFiles\SeaMode\Response";

		public const string AirAcceptedResponse = TestFilesAirResponseDirectory + @"\Accepted.xml";

		public const string AirRejectedResponse = TestFilesAirResponseDirectory + @"\Rejected.xml";

		public const string AirCancelAcceptedResponse = TestFilesAirResponseDirectory + @"\CancellationAccepted.xml";

		public const string AirCancelRejectedResponse = TestFilesAirResponseDirectory + @"\CancellationRejected.xml";

		public const string SeaAcceptedResponse = TestFilesSeaResponseDirectory + @"\Accepted.xml";

		public const string SeaRejectedResponse = TestFilesSeaResponseDirectory + @"\Rejected.xml";

		public const string SeaInvalidResponse = TestFilesSeaResponseDirectory + @"\InvalidResponse.xml";

		public const string SeaCancelAcceptedResponse = TestFilesSeaResponseDirectory + @"\CancellationAccepted.xml";

		public const string SeaCancelRejectedResponse = TestFilesSeaResponseDirectory + @"\CancellationRejected.xml";
	}
}
