namespace CargoWise.EntityFramework
{
	public interface IWrappedPropertyInfo
	{
		ZPropertyInfo InnerInfo { get; }
	}
}
