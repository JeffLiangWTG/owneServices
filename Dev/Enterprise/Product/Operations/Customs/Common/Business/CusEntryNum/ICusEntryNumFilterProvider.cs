using CargoWise.EntityFramework;

namespace Enterprise.Customs.Common
{
	public interface ICusEntryNumFilterProvider
	{
		ZQuery ValidCusEntryNumFilter { get; }
	}
}
