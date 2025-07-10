namespace CargoWise.EntityFramework
{
	public interface IKnowPropertyInformation
	{
		bool IsNullable(string propertyName);
		object GetDefaultValue(string propertyName);
		int GetMaxLength(string propertyName);
	}
}
