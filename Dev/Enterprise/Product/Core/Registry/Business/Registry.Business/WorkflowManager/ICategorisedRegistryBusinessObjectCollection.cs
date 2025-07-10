namespace Enterprise.Registry.Business
{
	public interface ICategorisedRegistryBusinessObjectCollection
	{
		RegistryBusinessObjectCollection InnerCollection { get; }
	}
}
