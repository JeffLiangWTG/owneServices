namespace Enterprise.ZArchitecture.Modules
{
	public class ControllerOverrides : RegistrationList<ControllerID, ControllerInfo>
	{
		public void AddControllerOverride(ClientOverrideControllerInfo controllerInfo)
		{
			Add(controllerInfo);
		}
	}
}
