namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public interface IJobNetworkValidator
	{
		void Validate(IJobNetwork network);
		void ValidateLoops(IJobNetwork network, string progressReporterCaption);
	}
}
