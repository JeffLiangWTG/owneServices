namespace ServiceManager.Common.Abstractions
{
	public record CommandLineParseError
	{
		public CommandLineParseError(string name, string message) {
			Name = name;
			Message = message;
		}

		public string Name { get; }
		public string Message { get; }
	}
}
