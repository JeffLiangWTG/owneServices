using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Integration;

namespace CargoWise.EntityFramework
{
	public class ChangedTableNames : Collection<string>, IChangedTableNames
	{
		public static ChangedTableNames Empty => new ChangedTableNames();
		public static ChangedTableNames All => new ChangedTableNames { ShouldChangeAll = true };

		ChangedTableNames()
		{
		}

		public ChangedTableNames(IList<IChangedTableNames> changedTableNamesSet) : base(changedTableNamesSet.SelectMany(s => s).ToArray())
		{
			Argument.NotNull(changedTableNamesSet, nameof(changedTableNamesSet));

			ShouldChangeAll = changedTableNamesSet.Any(s => s.ShouldChangeAll);
		}

		public ChangedTableNames(IList<string> list) : base(list)
		{
			Argument.NotNull(list, nameof(list));
		}

		public bool ShouldChangeAll { get; private set; }
	}
}
