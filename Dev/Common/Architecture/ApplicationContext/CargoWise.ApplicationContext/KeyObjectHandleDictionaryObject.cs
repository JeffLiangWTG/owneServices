using System.Collections;
using CargoWise.Common;
using CargoWise.Common.Testing;

namespace CargoWise.Application
{
	/// <summary>
	/// Represents an object for getting name->TypeName pairs using ObjectFactory.GetType() for the value.
	/// </summary>
	public sealed class KeyObjectHandleDictionaryObject : Hashtable
	{
		[SuppressWeaklyTypedCollectionMessage]
		public IDictionary SourceDictionary
		{
			set
			{
				Argument.NotNull(value, nameof(value));
				Clear();
				foreach (var entry in value)
				{
					var realEntry = (DictionaryEntry)entry;

					Add(realEntry.Key, new ObjectHandle((string)realEntry.Value));
				}
			}
		}
	}
}
