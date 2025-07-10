namespace Enterprise.ZArchitecture.Business
{
	public interface IUniversalCopySelectivelySupportable
	{
		bool SupportsUniversalCopy { get; }
		string ReasonForNotSupportingUniversalCopy { get; }
	}
}
