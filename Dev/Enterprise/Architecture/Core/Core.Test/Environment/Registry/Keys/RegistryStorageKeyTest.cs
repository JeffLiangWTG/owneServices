using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RegistryStorageKeyTest : TestCase
	{
		public void TestKey()
		{
			Guid guid1 = new Guid("03052ED3-2C64-49AC-97D8-C6079D5015B5");
			Guid guid2 = new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");
			Guid guid3 = Guid.Empty;

			AssertEquals("Key", "0y4FA2QsrEmX2MYHnVAVtQ==ynqNh8P//EmXEJacoMDyrA==", new RegistryStorageKey(guid1, guid2).Key);
			AssertEquals("Key", "0y4FA2QsrEmX2MYHnVAVtQ==ynqNh8P//EmXEJacoMDyrA==", new RegistryStorageKey(guid1, guid3, guid2).Key);
			AssertEquals("Key", "0y4FA2QsrEmX2MYHnVAVtQ==ynqNh8P//EmXEJacoMDyrA==", new RegistryStorageKey(guid3, guid1, guid2).Key);
			AssertEquals("Key", "ynqNh8P//EmXEJacoMDyrA==0y4FA2QsrEmX2MYHnVAVtQ==", new RegistryStorageKey(guid2, guid1).Key);
			AssertEquals("Key", "0y4FA2QsrEmX2MYHnVAVtQ==AAAAAAAAAAAAAAAAAAAAAA==", new RegistryStorageKey(guid1, guid3).Key);
			AssertEquals("Key", "AAAAAAAAAAAAAAAAAAAAAA==0y4FA2QsrEmX2MYHnVAVtQ==", new RegistryStorageKey(guid3, guid1).Key);
		}
	}
}
