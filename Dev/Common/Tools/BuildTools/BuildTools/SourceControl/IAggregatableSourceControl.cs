namespace CargoWise.BuildTools
{
	public interface IAggregatableSourceControl : ISourceControl
	{
		ISourceControl WithAdditionalRepository(ISourceControl sourceControl, string rootPath);
	}
}
