using CargoWise.EntityFramework;

namespace Enterprise.VisualBoards.Business
{
	public interface IBoardFactoryService : IService
	{
		BoardServiceStalenessPolicy StalenessPolicy { get; }

		void ClearCache();
	}
}
