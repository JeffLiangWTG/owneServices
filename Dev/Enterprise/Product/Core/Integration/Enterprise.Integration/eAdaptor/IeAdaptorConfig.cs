namespace Enterprise.Integration
{
	public interface IeAdaptorConfig : IHttpXmlProcessingConfig
	{
		string Name { get; }

		IeAdaptorContextSetter ContextSetter { get; }

		IWebConfig WebConfig { get; }

		IeAdaptorHttpResponseWriter ResponseWriter { get; }

		bool IsActive { get; }

		bool ThrowIfNotActive { get; }
	}
}
