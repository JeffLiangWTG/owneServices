namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Implement this interface in your business object to provide the ability to
	/// reverse (swap, change direction) the business object.
	/// </summary>
	public interface ITemplateReversible
	{
		void Reverse();
	}
}
