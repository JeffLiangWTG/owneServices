using System;

namespace Enterprise.DocumentEngineIntegration
{
	public interface IAssemblyDataHelper
	{
		Type GetBusinessObjectType(string storageMainType);
	}
}
