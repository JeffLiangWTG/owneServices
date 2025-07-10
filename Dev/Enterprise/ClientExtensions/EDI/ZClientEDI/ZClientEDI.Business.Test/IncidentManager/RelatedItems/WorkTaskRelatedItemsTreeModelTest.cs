using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(WorkTaskRelatedItemsTreeModel))]
	public class WorkTaskRelatedItemsTreeModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestChildrenBuildTree()
		{
			var workItemParent = Factory.NewWithValidTestData<EDIWorkItem>();

			var project99 = workItemParent.RelatedItems.AddNew(typeof(EDIProject)) as EDIProject;
			project99.WKP_ProjectNumber = "PJ00009999";
			var project66 = workItemParent.RelatedItems.AddNew(typeof(EDIProject)) as EDIProject;
			project66.WKP_ProjectNumber = "PJ00006666";

			project99.RelatedItems.AddNew(typeof(EDIWorkItem));
			project99.RelatedItems.AddNew(typeof(EDIWorkItem));

			var project1 = project66.RelatedItems.AddNew(typeof(EDIProject)) as EDIProject;
			project1.WKP_ProjectNumber = "PJ00000001";
			var project11 = project1.RelatedItems.AddNew(typeof(EDIProject)) as EDIProject;
			project11.WKP_ProjectNumber = "PJ00000011";
			var project12 = project1.RelatedItems.AddNew(typeof(EDIProject)) as EDIProject;
			var project111 = project11.RelatedItems.AddNew(typeof(EDIProject)) as EDIProject;
			project111.WKP_ProjectNumber = "PJ00000111";
			project11.RelatedItems.AddNew(typeof(EDIProject));
			project11.RelatedItems.AddNew(typeof(EDIProject));
			project111.RelatedItems.AddNew(typeof(EDIProject));

			var model = new WorkTaskRelatedItemsTreeModel(workItemParent);
			var mainNode = model.RootNodes;
			AssertEquals(1, mainNode.Count);
			AssertEquals("First node should by the tree bizo itself", workItemParent.WKI_Summary, ((WorkTaskRelatedItemsTreeNode)mainNode[0]).Description);

			var rootNodes = mainNode[0].ChildNodes.ToList();
			AssertEquals(2, rootNodes.Count);

			var project1Node = rootNodes[0];
			AssertEquals("WKP PJ00009999", project1Node.BizObjForBinding.Group);
			AssertEquals(2, project1Node.ChildNodes.Count());

			var project2Node = rootNodes[1];
			AssertEquals("WKP PJ00006666", project2Node.BizObjForBinding.Group);
			AssertEquals(1, project2Node.ChildNodes.Count());

			var wi11 = project2Node.ChildNodes.First(a => a.BizObj.RelatedItem.Number == "PJ00000001");
			AssertEquals("Max level should be 2", 0, wi11.ChildNodes.Count());
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var projectParent = Factory.NewWithValidTestData<EDIProject>();
			return new WorkTaskRelatedItemsTreeModel(projectParent);
		}

		#endregion
	}
}
