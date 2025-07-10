using System.Collections;
using CargoWise.Common;
using CargoWise.Common.Testing;

namespace CargoWise.Application
{
	/// <summary>
	/// Represents an object for getting a list of objects.
	/// </summary>
	public sealed class ListObject : ArrayList
	{
		[SuppressWeaklyTypedCollectionMessage]
		public IList SourceList
		{
			set
			{
				Argument.NotNull(value, nameof(value));
				Clear();
				AddRange(value);
			}
		}
	}
}
