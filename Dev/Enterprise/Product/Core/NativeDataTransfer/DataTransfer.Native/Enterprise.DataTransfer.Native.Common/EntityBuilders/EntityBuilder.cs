namespace Enterprise.DataTransfer.Native.Common.EntityBuilders
{
	public abstract class EntityBuilder
	{
		public static Entity Construct(EntityBuilder entityBuilder)
		{
			entityBuilder.BuildAction();
			entityBuilder.BuildInternalPK();
			entityBuilder.BuildProperties();
			return entityBuilder.Entity;
		}

		protected Entity Entity { get; set; }

		public abstract void BuildInternalPK();
		public abstract void BuildAction();
		public abstract void BuildProperties();

		public EntityAction Action => Entity.Action;
	}
}