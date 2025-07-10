using System.Collections.Generic;

namespace CargoWise.Integration
{
	public interface IChangedTableNames : ICollection<string>
	{
		bool ShouldChangeAll { get; }
	}
}
