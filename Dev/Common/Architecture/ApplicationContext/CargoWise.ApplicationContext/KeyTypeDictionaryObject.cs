using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.Common.Testing;

namespace CargoWise.Application
{
	/// <summary>
	/// Represents an object for getting name->Type pairs using ObjectFactory.GetType() for the value.
	/// </summary>
	/// 
	public sealed class KeyTypeDictionaryObject : Hashtable
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

					Type type = ObjectFactory.GetTypeWithoutSecurityCheck(typeof(ObjectFactory.EmptyType), (string)realEntry.Value);
					Add(realEntry.Key, type);
				}
			}
		}
	}
}
