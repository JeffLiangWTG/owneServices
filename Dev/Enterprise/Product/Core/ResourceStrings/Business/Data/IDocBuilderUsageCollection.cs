using CargoWise.EntityFramework;

namespace Enterprise.ResourceStrings.Business
{
	public interface IDocBuilderUsageCollection : IBusinessObjectCollection
	{
		new IDocBuilderUsage this[int i] { get; }
	}
}
