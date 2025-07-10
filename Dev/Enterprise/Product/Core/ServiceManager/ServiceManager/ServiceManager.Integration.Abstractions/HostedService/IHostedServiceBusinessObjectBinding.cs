namespace ServiceManager.Integration.Abstractions
{
	public interface IHostedServiceBusinessObjectBinding
	{
		string Table { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This is a attribute parameter, cannot be complex")]
		string[] Predicates { get; }

		string ServiceTaskCode { get; }

		string QueueName { get; }
	}
}
