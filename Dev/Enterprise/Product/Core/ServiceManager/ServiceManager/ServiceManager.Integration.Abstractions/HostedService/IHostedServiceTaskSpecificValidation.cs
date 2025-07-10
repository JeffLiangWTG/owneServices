namespace ServiceManager.Integration.Abstractions;

public interface IHostedServiceTaskSpecificValidation
{
	string TaskSpecificValidationTypeName { get; }
	string TaskSpecificValidationTypeAssemblyName { get; }
}
