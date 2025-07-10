using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business;

public abstract class RefCountryValidationRule : ValidationRule
{
	public sealed override ValidationResult Validate(object value) => ValidateCore((RefCountry)value);

	protected abstract ValidationResult ValidateCore(RefCountry country);
}
