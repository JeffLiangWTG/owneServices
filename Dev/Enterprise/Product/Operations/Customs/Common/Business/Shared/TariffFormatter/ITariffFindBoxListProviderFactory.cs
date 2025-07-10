
namespace Enterprise.Customs.Common
{
	using CargoWise.EntityFramework;

	/// <summary>
	///		Defines methods for creating the <see cref="IFindBoxListProvider"/> instance.
	/// </summary>
	public interface IFindBoxListProviderFactory
	{
		IFindBoxListProvider Create(IBusinessObjectCollection list);
	}
}
