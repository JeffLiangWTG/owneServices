using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Integration;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class TypeDeciderDictionaryTest : TestCase
	{
		public void TestContainsKey()
		{
			Dictionary<Type, ITypeDecider> dictionary = new Dictionary<Type, ITypeDecider>();
			dictionary[typeof(TestBusinessObject)] = new TypeDecider();

			TypeDeciderDictionary typeDeciderDictionary = new TypeDeciderDictionary(dictionary);
			AssertEquals(false, typeDeciderDictionary.ContainsKey(typeof(int)));
			AssertEquals(true, typeDeciderDictionary.ContainsKey(typeof(TestBusinessObject)));
		}

		class TestBusinessObject
		{
		}

		class TypeDecider : ITypeDecider
		{
			public Type GetTypeForBinding()
			{
				return typeof(object);
			}

			public Type GetTypeForLoad(DataRow row, object factory)
			{
				return typeof(object);
			}

			public Type GetTypeForNew()
			{
				return typeof(object);
			}
		}
	}
}
