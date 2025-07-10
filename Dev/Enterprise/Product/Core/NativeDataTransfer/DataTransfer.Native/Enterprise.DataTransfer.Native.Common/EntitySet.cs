using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;

namespace Enterprise.DataTransfer.Native.Common
{
	public interface IEntitySet
	{
		string Name { get; }
		IEntity Root { get; }
		IEntitySetDefinition Definition { get; }
	}

	public class EntitySet : IEntitySet
	{
		public EntitySet(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
		public IEntity Root { get; set; }
		public IEntitySetDefinition Definition { get; set; }
	}
}
