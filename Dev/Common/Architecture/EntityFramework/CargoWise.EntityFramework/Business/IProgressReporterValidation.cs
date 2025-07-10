using System;

namespace CargoWise.EntityFramework.Business
{
	public interface IProgressReporterValidation
	{
		IDisposable ReportProgress(Action<string> progress);
	}
}
