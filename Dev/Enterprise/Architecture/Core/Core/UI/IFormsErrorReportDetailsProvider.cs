namespace Enterprise.ZArchitecture.Core
{
	public interface IFormsErrorReportDetailsProvider
	{
		string LastActiveFormInfo { get; }
		string GetDetails();
		string GetSystemResourcesUsageElements();
		string GetSystemResourcesUsageText();
	}
}
