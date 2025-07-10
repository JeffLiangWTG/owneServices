using System;
using CargoWise.PAVE.Common.DTO;

namespace Enterprise.BufferManagement.Service.Shared
{
	public interface IBoardService
	{
		BoardConfigurationDTO GetConfiguration(Guid boardPK);
	}
}
