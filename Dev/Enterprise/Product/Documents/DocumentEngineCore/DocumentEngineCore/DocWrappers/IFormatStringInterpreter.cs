using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	public interface IFormatStringInterpreter : IDisposable
	{
		Func<IList> GetCollectionScopeStrategy { get; set; }
		ZString Format(IBODocDataProvider docDataProvider, ZString formatString);
		ZString Format(BusinessObject parent, ZString formatString);
	}
}
