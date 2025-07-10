using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class DatabaseResourceStringSourceTest : TransactionedTestCase
	{
		public void TestReadWriteDelete()
		{
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ResourceStringData>(), new DatabaseResourceStringSource(Core.SharedConstants.Languages.French).ReadAll());
			new DatabaseResourceStringSource(Core.SharedConstants.Languages.French).WriteAll(new[] { new ResourceStringData("1", "One", "", "", ""), new ResourceStringData("2", "", "Two", "", ""), new ResourceStringData("3", "", "", "Three", ""), new ResourceStringData("4", "", "", "", "Four") });
			AssertContainsExactElementsInAnyOrder(new[] { new ResourceStringData("1", "One", "", "", ""), new ResourceStringData("2", "", "Two", "", ""), new ResourceStringData("3", "", "", "Three", ""), new ResourceStringData("4", "", "", "", "Four") }, new DatabaseResourceStringSource(Core.SharedConstants.Languages.French).ReadAll());
			new DatabaseResourceStringSource(Core.SharedConstants.Languages.French).WriteAll(new[] { new ResourceStringData("5", "Five") });
			AssertContainsExactElementsInAnyOrder(new[] { new ResourceStringData("5", "", "", "Five", "") }, new DatabaseResourceStringSource(Core.SharedConstants.Languages.French).ReadAll());
			new DatabaseResourceStringSource(Core.SharedConstants.Languages.French).WriteAll(System.Array.Empty<ResourceStringData>());
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ResourceStringData>(), new DatabaseResourceStringSource(Core.SharedConstants.Languages.French).ReadAll());
		}

		public void TestLanguageSpecific()
		{
			new DatabaseResourceStringSource(Core.SharedConstants.Languages.French).WriteAll(new[] { new ResourceStringData("k1", "French") });
			new DatabaseResourceStringSource(Core.SharedConstants.Languages.German).WriteAll(new[] { new ResourceStringData("k2", "German") });
			new DatabaseResourceStringSource(Core.SharedConstants.Languages.Spanish).WriteAll(new[] { new ResourceStringData("k3", "Spanish") });
			AssertContainsExactElementsInAnyOrder(new[] { new ResourceStringData("k1", "", "", "French", "") }, new DatabaseResourceStringSource(Core.SharedConstants.Languages.French).ReadAll());
			AssertContainsExactElementsInAnyOrder(new[] { new ResourceStringData("k2", "", "", "German", "") }, new DatabaseResourceStringSource(Core.SharedConstants.Languages.German).ReadAll());
			AssertContainsExactElementsInAnyOrder(new[] { new ResourceStringData("k3", "", "", "Spanish", "") }, new DatabaseResourceStringSource(Core.SharedConstants.Languages.Spanish).ReadAll());
		}

		public void TestCreateAll()
		{
			new DatabaseResourceStringSource(Core.SharedConstants.Languages.French).WriteAll(new[] { new ResourceStringData("k1", "French") });
			new DatabaseResourceStringSource(Core.SharedConstants.Languages.German).WriteAll(new[] { new ResourceStringData("k2", "German") });
			new DatabaseResourceStringSource(Core.SharedConstants.Languages.Spanish).WriteAll(new[] { new ResourceStringData("k3", "Spanish") });
			var res = DatabaseResourceStringSource.CreateAll(new string[] { Core.SharedConstants.Languages.French, Core.SharedConstants.Languages.German, Core.SharedConstants.Languages.Spanish });
			AssertEquals(3, res.Count);
			AssertEquals(Core.SharedConstants.Languages.French, res[Core.SharedConstants.Languages.French].Source.Language);
			AssertContainsExactElementsInAnyOrder(new[] { new ResourceStringData("k1", "", "", "French", "") }, res[Core.SharedConstants.Languages.French].Data);
			AssertEquals(Core.SharedConstants.Languages.German, res[Core.SharedConstants.Languages.German].Source.Language);
			AssertContainsExactElementsInAnyOrder(new[] { new ResourceStringData("k2", "", "", "German", "") }, res[Core.SharedConstants.Languages.German].Data);
			AssertEquals(Core.SharedConstants.Languages.Spanish, res[Core.SharedConstants.Languages.Spanish].Source.Language);
			AssertContainsExactElementsInAnyOrder(new[] { new ResourceStringData("k3", "", "", "Spanish", "") }, res[Core.SharedConstants.Languages.Spanish].Data);
		}
	}
}
