
using Enterprise.MasterFiles.Integration;
namespace Enterprise.DocumentEngineCore.DocumentSupport
{
	public interface IDocumentDataStateManager
	{
		DocumentDataStateManagerResult Evaluate(IStmMenuItem commandAboutToBeRun);
		bool IsDataStateValid { get; }
		string DataStateErrorMessage { get; }
	}
}
