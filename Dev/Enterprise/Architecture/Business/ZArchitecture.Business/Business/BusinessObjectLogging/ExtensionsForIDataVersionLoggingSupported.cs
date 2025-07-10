namespace Enterprise.ZArchitecture.Business
{
	public static class ExtensionsForIDataVersionLoggingSupported
	{
		public static DataVersionLogValueFormatter GetDefaultDataVersionLogFormatter(this IDataVersionLoggingSupported bizo) => new DataVersionLogValueFormatter();
	}
}
