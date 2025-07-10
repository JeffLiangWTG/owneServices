using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DropEditBusinessObject))]
	sealed class DropEditBusinessObjectTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.Add(new CodeDescriptionPair("abcdefg", ""));
			return new DropEditBusinessObject(list);
		}

		public void TestList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.Add(new CodeDescriptionPair("abc", "def"));
			DropEditBusinessObject myBusinessObject = new DropEditBusinessObject(list);
			AssertEquals("List", list, myBusinessObject.List);
		}

		public void TestSetDefaultValueAfterEleminatedFromList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("LongCodeExample", "Long One");
			list.AddPair("SecondLong", "Second Long One");
			list.AddPair("Short", "Short One");

			DropEditBusinessObject myBusinessObject = new DropEditBusinessObject(list);
			myBusinessObject.Value = list.GetCodeFromDescription("Long One");

			AssertEquals("LongCodeExample", myBusinessObject.Value);
			list.RemoveCode("LongCodeExample");
			myBusinessObject.Value = myBusinessObject.Value;
			AssertEquals("LongCodeEx", myBusinessObject.Value);
		}
	}
}
