namespace Enterprise.DataTransfer.Native.Common
{
	public interface IPropertyDefinitionFinder
	{
		IPropertyDef Find(string propertyName);
		bool HasDefinition(string propertyName);
	}
}
