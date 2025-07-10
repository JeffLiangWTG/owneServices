namespace CargoWise.BuildTools
{
	public interface ISourceControlFactory
	{
		ISourceControl GetSourceControl();
		ISourceControl GetSourceControl(string localPath);
		ISourceControl WithAdditionalRepository(ISourceControl sourceControl, string localPath);
	}
}
