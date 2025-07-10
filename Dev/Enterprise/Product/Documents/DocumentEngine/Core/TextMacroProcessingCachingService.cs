using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.DocumentEngine
{
	public class TextMacroProcessingCachingService : ITextMacroProcessingCachingService
	{
		readonly Dictionary<ZString, object> macroValues = new Dictionary<ZString, object>();
		bool cachingDisabled;

		public void CacheMacroValue(ZString macro, object value)
		{
			if (cachingDisabled)
			{
				return;
			}

			macroValues[macro] = value;
		}

		public bool TryGetMacroValue(ZString macro, out object value)
		{
			if (cachingDisabled)
			{
				value = null;
				return false;
			}

			return macroValues.TryGetValue(macro, out value);
		}

		public IDisposable WithNoCaching()
		{
			if (cachingDisabled)
			{
				return null;
			}

			cachingDisabled = true;
			return new DisposableAction(() => cachingDisabled = false);
		}
	}
}
