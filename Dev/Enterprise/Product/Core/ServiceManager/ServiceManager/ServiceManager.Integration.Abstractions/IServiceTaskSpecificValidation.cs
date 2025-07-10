using System.Collections.Generic;

namespace ServiceManager.Integration.Abstractions;

public interface IServiceTaskSpecificValidation
{
	ValidationResult Validate();
}

public record ValidationResult
{
	public IEnumerable<string> Errors { get; set; } = [];
	public IEnumerable<KeyValuePair<string, string>> PropertySpecificWarnings { get; set; } = [];
}
