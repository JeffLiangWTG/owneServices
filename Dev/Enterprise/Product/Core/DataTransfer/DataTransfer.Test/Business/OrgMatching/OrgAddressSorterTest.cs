using System.Collections;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class OrgAddressSorterTest : TestCase
	{
		public void TestOrgAddressSorter()
		{
			var sorter = new OrgAddressSorter(GetDocAddressCollectionForOrgAddressSorter());

			AssertReferences(sorter, "XXX1", "1", "5");
			AssertReferences(sorter, "XXX2", "3", "1", "8");
			AssertReferences(sorter, "XXX3");
		}

		void AssertReferences(IOrgAddressSorter sorter, string ediCode, params string[] expectedReferences)
		{
			IList references = sorter.GetReferences(ediCode);

			if (expectedReferences == null || expectedReferences.Length == 0)
			{
				AssertNull(references);
			}
			else
			{
				AssertNotNull(references);
				AssertEquals(expectedReferences.Length, references.Count);
				for (int i = 0; i < expectedReferences.Length; i++)
				{
					AssertEquals(expectedReferences[i], references[i]);
				}
			}
		}

		static DocAddressCollection GetDocAddressCollectionForOrgAddressSorter()
		{
			DocAddressCollection collection = new DocAddressCollection { IsSpecified = true };
			collection.Add(GetDocAddressForOrgAddressSorter(1, "XXX1"));
			collection.Add(GetDocAddressForOrgAddressSorter(5, "XXX1"));
			collection.Add(GetDocAddressForOrgAddressSorter(3, "XXX2"));
			collection.Add(GetDocAddressForOrgAddressSorter(1, "XXX2"));
			collection.Add(GetDocAddressForOrgAddressSorter(8, "XXX2"));
			return collection;
		}

		internal static DocAddress GetDocAddressForOrgAddressSorter(int reference, string ediCode)
		{
			return
				new DocAddress
				{
					AddressReference =
						{
							AddressSequenceRef = reference,
							Organisation =
								{
									EDICode = ediCode,
									EDICodeSpecified = true,
									IsSpecified = true
								},
							IsSpecified = true
						},
					IsSpecified = true
				};
		}
	}
}
