using CargoWise.Common;
namespace Enterprise.DocumentEngine
{
	public interface IReportUrlHandler
	{
		string Create(ReportCommand reportCommand);
		string Create(Report report);
		bool CanHandle(QueryString queryString);
		bool Handle(QueryString queryString);
	}
}