using System.Collections.Generic;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.VisualBoards.Business
{
	public interface IBoardFactoryServiceProvider
	{
		IEnumerable<IBoardFactoryService> GetFactoryServices(IVisualBoardProvider source);
	}
}
