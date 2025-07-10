using Enterprise.Integration;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class MockLoggerStream : LoggerStream
	{
		public bool writeCalled { get; set; }
		public MockLoggerStream(ILogger logger) : base(logger)
		{
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			writeCalled = true;
		}
	}
}
