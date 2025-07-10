using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(HBLPackLinesDisplayOrderDropEditBussinessObject))]
	sealed class HBLPackLinesDisplayOrderDropEditBussinessObjectTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var list = new CodeDescriptionPairList();
			list.Add(new CodeDescriptionPair("abcdefg", ""));
			return new HBLPackLinesDisplayOrderDropEditBussinessObject(list);
		}

		public void TestList()
		{
			var list = new CodeDescriptionPairList();
			list.Add(new CodeDescriptionPair("abc", "def"));
			var myBusinessObject = new HBLPackLinesDisplayOrderDropEditBussinessObject(list);
			AssertEquals("List", list, myBusinessObject.List);
		}

		public void TestSetDefaultValueAfterEleminatedFromList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("LongCodeExample", "Long One");
			list.AddPair("SecondLong", "Second Long One");
			list.AddPair("Short", "Short One");

			var myBusinessObject = new HBLPackLinesDisplayOrderDropEditBussinessObject(list);
			myBusinessObject.Value = list.GetCodeFromDescription("Long One");

			AssertEquals("LongCodeExample", myBusinessObject.Value);
			list.RemoveCode("LongCodeExample");
			myBusinessObject.Value = myBusinessObject.Value;
			AssertEquals("LongCodeEx", myBusinessObject.Value);
		}

		public override void TestBizObjectFields()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("LongCodeExample", "Long One");
			list.AddPair("SecondLong", "Second Long One");
			list.AddPair("Short", "Short One");

			var myBusinessObject = new HBLPackLinesDisplayOrderDropEditBussinessObject(list);

			myBusinessObject.Description = "Long One";
			AssertEquals("LongCodeExample", myBusinessObject.Value);

			myBusinessObject.Description = "Second Long One";
			AssertEquals("SecondLong", myBusinessObject.Value);

			myBusinessObject.Description = "Short One";
			AssertEquals("Short", myBusinessObject.Value);
		}
	}
}
