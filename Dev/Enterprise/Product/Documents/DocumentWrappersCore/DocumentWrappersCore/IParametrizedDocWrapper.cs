using System.Collections.Generic;

namespace Enterprise.DocumentWrappersCore
{
	public interface IParametrizedDocWrapper
	{
		Dictionary<string, object> Parameters { get; set; }
	}
}
