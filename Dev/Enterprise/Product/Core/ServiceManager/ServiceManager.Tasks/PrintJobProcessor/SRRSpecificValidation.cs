using Enterprise.Registry.Business;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor;

public class SRRSpecificValidation : IServiceTaskSpecificValidation
{
	public SRRSpecificValidation(int secondaryProcessesMaxCount)
	{
		this.secondaryProcessesMaxCount = secondaryProcessesMaxCount;
	}

	public ValidationResult Validate()
	{
		var maximumConcurrentRunningReports = SystemDataRegistry.Instance.ReportMaxConnections.Value;
		var error = (string)null;
		if (maximumConcurrentRunningReports != 0 && secondaryProcessesMaxCount >= maximumConcurrentRunningReports)
		{
			error = Res.GetString(
				"bef3ae2f-b6dc-402c-9588-a6a33540a146",
				"Maximum count of secondary processes must be less than \"{0}\" value in Registry which is set to {1}.",
				SystemDataRegistry.Instance.ReportMaxConnections.Caption,
				SystemDataRegistry.Instance.ReportMaxConnections.Value);
		}

		return new ValidationResult() { Errors = string.IsNullOrEmpty(error) ? [] : [error] };
	}

	readonly int secondaryProcessesMaxCount;
}
