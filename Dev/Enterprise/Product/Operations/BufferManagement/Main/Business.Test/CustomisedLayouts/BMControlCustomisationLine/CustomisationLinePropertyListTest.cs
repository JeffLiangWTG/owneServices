using System;
using System.Linq;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	class CustomisationLinePropertyListTest : BMSTestCaseWithFactory
	{
		public void TestList_ProcessTask()
		{
			var list = new CustomisationLinePropertyList(PropertySourceList.Codes.ProcessTask);
			Assert("Should have a bunch of properties", list.Count > 50);

			AssertCollectionContains(list.Cast<CodeDescriptionPair>(), c => c.Code == ProcessTasksSchema.P9_Sequence.Name);
			AssertCollectionContains(list.Cast<CodeDescriptionPair>(), c => c.Code == ProcessTasksSchema.P9_Description.Name);
		}

		public void TestList_DescriptionShouldBeUnique_WhenProcessTask()
		{
			AssertIfDescriptionIsUnique(PropertySourceList.Codes.ProcessTask);
		}

		public void TestList_DescriptionShouldBeUnique_WhenProcessHeader()
		{
			AssertIfDescriptionIsUnique(PropertySourceList.Codes.Workflow);
		}

		static void AssertIfDescriptionIsUnique(string propertySource)
		{
			var list = new CustomisationLinePropertyList(propertySource);
			var itemsWithSameDescriptions = list.ToArray()
				.GroupBy(item => item.Description, StringComparer.OrdinalIgnoreCase)
				.Where(group => group.Count() > 1);
			string message = null;

			foreach (var item in itemsWithSameDescriptions)
			{
				var properties = string.Join(", ", item.Select(i => i.Code)).Trim();
				message += $"{item.Key} is set in: {properties} Properties\r\n";
			}

			Assert($"Description should be unique:\r\n{message}", !itemsWithSameDescriptions.Any());
		}
	}
}
