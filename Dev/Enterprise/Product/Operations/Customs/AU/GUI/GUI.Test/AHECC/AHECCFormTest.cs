using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AHECCFormTest : TestCase
	{
		public void TestTopNode()
		{
			using (var testForm = new AHECCFormForTest())
			{
				testForm.Show();
				var topNode = testForm.TreeView.TopNode;
				AssertEquals("Top Node Text", "0 " + AHECCFormForTest.SectionDescription, testForm.TreeView.TopNode.Text);
				AssertEquals("Placeholder count", 1, topNode.Nodes.Count); // Child level
				AssertEquals("Top Node Placeholder", "Please wait - Loading", topNode.Nodes[0].Text);
			}
		}

		public void TestChildNodeAfterExpansion()
		{
			using (var testForm = new AHECCFormForTest())
			{
				testForm.Show();
				var topNode = testForm.TreeView.TopNode;
				testForm.TreeView.TopNode.Expand();
				AssertEquals("Child Node count", 1, topNode.Nodes.Count); // Child level
				AssertEquals("Child Node Text", " " + AHECCFormForTest.ChapterDescription, topNode.Nodes[0].Text);
			}
		}

		sealed class AHECCFormForTest : AHECCForm
		{
			public const string SectionDescription = "Section 1";
			public const string ChapterDescription = "Chapter 1";
			public const string AHECC1 = "0101";
			public const string Desc1 = "Horse";
			public const string AHECC2 = "0101.01";
			public const string Desc2 = "Almost fully described Horse";
			public const string AHECC3 = "0101.01.01";
			public const string Desc3 = "Fully described Horse";

			protected override BusinessObjectCollection GetTopLevelCollection(BusinessObjectFactory factory)
			{
				var sectionCollection = new AUCSectionCollection(factory);
				var section = sectionCollection.AddNew();
				section.UG_Description = SectionDescription;
				var chapter = section.AUCChapters.AddNew();
				chapter.UH_Description = ChapterDescription;
				var bizObj = ((AUCChapterAHECCCollection)chapter.ChildCollection).AddNew();
				bizObj.UA_AHECC = AHECC1;
				bizObj.UA_ShortDescription = Desc1;
				var child = bizObj.AUCAHECCs.AddNew();
				child.UA_AHECC = AHECC2;
				child.UA_ShortDescription = Desc2;
				var grandChild = child.AUCAHECCs.AddNew();
				grandChild.UA_AHECC = AHECC3;
				grandChild.UA_ShortDescription = Desc3;
				return sectionCollection;
			}
		}
	}
}
