using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(CustomMapPairList))]
	sealed class CustomMapPairListTest : NonPersistentBusinessObjectCollectionTestCase<CustomMapPairList>
	{
		public void TestICodeDescriptionPairList()
		{
			CustomMapPairList list = new CustomMapPairList();
			CustomMapPair p = list.AddNew();
			p.Input = "AAA";
			p.Output = "123";
			p = list.AddNew();
			p.Input = "BBB";
			p.Output = "456";
			p = list.AddNew();
			p.Input = "CCC";
			p.Output = "789";

			ICodeDescriptionPairList codeDescriptionPairList = list;
			Assert(codeDescriptionPairList.ContainsCode("AAA"));
			Assert(!codeDescriptionPairList.ContainsCode("DDD"));
			Assert(codeDescriptionPairList.ContainsCode("CCC"));
			Assert(codeDescriptionPairList.ContainsCode(" CCC"));
			Assert(codeDescriptionPairList.ContainsCode("CCC\t "));
			AssertEquals("123", codeDescriptionPairList.GetDescriptionFromCode("AAA"));
			AssertEquals(null, codeDescriptionPairList.GetDescriptionFromCode("DDD"));
			AssertEquals("789", codeDescriptionPairList.GetDescriptionFromCode("CCC"));
			AssertEquals("789", codeDescriptionPairList.GetDescriptionFromCode(" CCC"));
			AssertEquals("789", codeDescriptionPairList.GetDescriptionFromCode("CCC   \t  "));
		}

		#region Implementation

		protected override CustomMapPairList GetCollectionToTest()
		{
			return new CustomMapPairList();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CustomMapPair();
		}

		#endregion
	}
}
