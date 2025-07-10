namespace CargoWise.EntityFramework
{
	public interface IRelatedBusinessObjectProvider
	{
		BusinessObject[] GetRelatedBusinessObjects(BusinessObject bizo);
	}
}
