#if DEBUG
using System;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IClientSpecificArchiveManagerHelper
	{
		void CreateClientSpecificConfiguration();
		void CreateClientSpecificData(ZGuid shipmetPK);
		void DropClientSpecificConfiguration();
		void AssertNumberOfRecords(string status, Action<bool> assert);
	}
}

#endif
