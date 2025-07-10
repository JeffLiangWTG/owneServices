namespace CargoWise.UniversalCopy
{
	public interface IUniversalCopyValidationStrategy
	{
		string ValidateUniversalCopyPreconditions(CopyTemplateTree configurationTree);
	}
}
