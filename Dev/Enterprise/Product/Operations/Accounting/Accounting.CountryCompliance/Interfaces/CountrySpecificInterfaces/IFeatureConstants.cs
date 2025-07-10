namespace Enterprise.Accounting.CountryCompliance.Interfaces
{
	public interface IFeatureConstants
	{
		T GetFeatureContants<T>() where T : class;
	}
}
