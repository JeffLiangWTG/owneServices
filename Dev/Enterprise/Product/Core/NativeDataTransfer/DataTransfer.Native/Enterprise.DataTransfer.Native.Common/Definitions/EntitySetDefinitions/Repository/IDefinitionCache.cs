namespace Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository
{
	public interface IDefinitionCache
	{
		bool HasDefinition(string definitionName);
		EntitySetDefinition Find(string definitionName);
		void Reset();
	}
}