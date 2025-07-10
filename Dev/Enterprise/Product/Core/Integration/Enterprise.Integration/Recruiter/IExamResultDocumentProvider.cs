using CargoWise.Types;

namespace Enterprise.Integration.Recruiter
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Will need further investigation before removal")]
	public interface IExamResultDocumentProvider
	{
		ZBlob GetExamResultDocumentAsBlob();
	}
}
