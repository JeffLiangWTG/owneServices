namespace Enterprise.ZArchitecture.Business
{
	public interface ICustomFieldProvider
	{
		CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false);
	}
}
