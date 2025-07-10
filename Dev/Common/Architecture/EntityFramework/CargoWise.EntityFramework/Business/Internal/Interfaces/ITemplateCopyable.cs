namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Implement this interface in your business object to provide the ability to create
	/// a copy of a bizo to be used as a template.
	/// </summary>
	public interface ITemplateCopyable
	{
		IBusiness TemplateCopy();
	}
}
