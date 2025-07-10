using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public interface IAdditionalDebuggingDetails
	{
		IEnumerable<string> AdditionalDetails { get; }
	}
}
