using System;
using CargoWise.Integration;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ReadOnlyCodeDescriptionPairListPKIndexerTest : TestCase
	{
		public void TestPKIndexer()
		{
			ZGuid pairPK = ZGuid.NewZGuid();
			ICodeDescription pair = new CodeElement(pairPK, "Code1", "Description1");
			List.Add(pair);

			AssertEquals("Retrieved pair from list", pair, List[pairPK]);
		}

		public void TestPKIndexerReturnsNullForValidButMissingElements()
		{
			ZGuid guid = ZGuid.NewZGuid();
			Assert(guid.IsValid);

			AssertNull("No matching element", List[guid]);
		}

		public void TestPKIndexerMultipleResults()
		{
			ZGuid pair1PK = ZGuid.NewZGuid();
			ICodeDescription pair1 = new CodeElement(pair1PK, "Code1", "Description1");
			List.Add(pair1);

			ZGuid pair2PK = ZGuid.NewZGuid();
			ICodeDescription pair2 = new CodeElement(pair2PK, "Code2", "Description2");
			List.Add(pair2);

			AssertEquals("Retrieved pair from list", pair1, List[pair1PK]);
			AssertEquals("Retrieved pair from list", pair2, List[pair2PK]);
		}

		public void TestPKIndexerWithInvalidValues()
		{
			ICodeDescription pair1 = new CodeElement(ZGuid.Empty, "Code1", "Description1");
			List.Add(pair1);
			ICodeDescription pair2 = new CodeElement(ZGuid.Invalid, "Code2", "Description2");
			List.Add(pair2);

			AssertNull("Should not find element with empty ZGuid", List[ZGuid.Empty]);
			AssertNull("Should not find element with invalid ZGuid", List[ZGuid.Invalid]);
		}

		public void TestPKIndexerOnNonICodeDescriptionItems()
		{
			List.Add(new CodeDescriptionPair("Code1", "Description1"));
			AssertNull(List[ZGuid.NewZGuid()]);
		}

		public void TestPKIndexerWithICodeDescriptionAndCodeDescriptionPairs()
		{
			List.Add(new CodeDescriptionPair("Code1", "Description1"));
			List.Add(new CodeDescriptionPair("Code2", "Description2"));

			var pk = ZGuid.NewZGuid();
			var codeElement = new CodeElement(pk, "Code2", "Description2");
			List.Add(codeElement);

			AssertEquals(codeElement, List[pk]);
		}

		public void TestAddNullCode()
		{
			AssertExceptionThrown<ArgumentException>(() => List.Add(new CodeDescriptionPairForTest(null, "test")));
		}

		class CodeDescriptionPairForTest : ICodeDescription
		{
			public CodeDescriptionPairForTest(string code, string description)
			{
				PK = Guid.NewGuid();
				Code = code;
				Description = description;
			}
			public object PK { get; }

			public string Code { get; }

			public string Description { get; }
		}
		#region Implementation

		protected override void SetUp()
		{
			List = new CodeDescriptionPairListForTest();
		}

		CodeDescriptionPairListForTest List;

		class CodeDescriptionPairListForTest : ReadOnlyCodeDescriptionPairList
		{
			internal void Add(ICodeDescription item)
			{
				Elements.Add(item);
			}
		}

		#endregion
	}
}
