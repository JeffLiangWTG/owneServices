using System;
using System.Collections.Generic;

namespace CargoWise.Integration
{
	public interface ITypeDeciderDictionary : IEnumerable<KeyValuePair<Type, ITypeDecider>>
	{
		ITypeDecider this[Type type] { get; }
		bool ContainsKey(Type type);
	}
}
