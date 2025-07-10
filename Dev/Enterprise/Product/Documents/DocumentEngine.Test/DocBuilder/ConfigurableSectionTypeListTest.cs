using System.Reflection;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class ConfigurableSectionTypeListTest : TestCase
	{
		public void TestListIsInOrder()
		{
			Assert("EndOfReport > ConfigSection", ConfigurableSectionTypeList.ListInOrder.IndexOf(ConfigurableSectionTypeList.Codes.EndOfReport) > ConfigurableSectionTypeList.ListInOrder.IndexOf(ConfigurableSectionTypeList.Codes.ConfigSection));
		}

		public void TestListHasAllElementsInIt()
		{
			FieldInfo[] infos = typeof(ConfigurableSectionTypeList.Codes).GetFields(BindingFlags.Public | BindingFlags.Static);
			AssertEquals("ConfigurableSectionTypeList.ListInOrder.Count", infos.Length, ConfigurableSectionTypeList.ListInOrder.Count);
			foreach (FieldInfo info in infos)
			{
				string value = (string)info.GetValue(null);
				Assert("ConfigurableSectionTypeList.ListInOrder.Contains(\"" + value + "\")", ConfigurableSectionTypeList.ListInOrder.Contains(value));
			}
		}
	}
}
