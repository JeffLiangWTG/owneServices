using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.DocumentEngine
{
	public interface ITextMacroProcessingCachingService : IService
	{
		void CacheMacroValue(ZString macro, object value);

		bool TryGetMacroValue(ZString macro, out object value);

		IDisposable WithNoCaching();
	}
}
