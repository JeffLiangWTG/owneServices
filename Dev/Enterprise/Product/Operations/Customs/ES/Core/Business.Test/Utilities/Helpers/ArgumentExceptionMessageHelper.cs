namespace Enterprise.Customs.ES.Business.Testing
{
	public static class ArgumentExceptionMessageHelper
	{
		public static string GetArgumentExceptionMessage(string message, string parameterName)
		{
#if NETFRAMEWORK
			return string.Format(message + "\r\nParameter name: {0}", parameterName);
#else
			return string.Format(message + " (Parameter '{0}')", parameterName);
#endif
		}
	}
}
