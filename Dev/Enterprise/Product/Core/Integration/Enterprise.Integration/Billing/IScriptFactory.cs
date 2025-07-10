using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Integration.Billing
{
	public interface IScriptFactory
	{
		IEnumerable<IStlItem> CreateScripts(BusinessObjectFactory factory);
	}
}
