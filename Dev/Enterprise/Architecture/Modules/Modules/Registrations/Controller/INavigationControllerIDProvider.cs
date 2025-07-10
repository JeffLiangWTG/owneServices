namespace Enterprise.ZArchitecture.Modules
{
	public interface INavigationControllerIDProvider
	{
		ControllerID GetValidControllerID(object dataSource);

		bool ShouldLoadBusinessObject { get; }
	}
}
