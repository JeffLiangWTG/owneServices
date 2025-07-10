using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.UniversalCopy;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	public class CollectionNodeDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestReadOnlyDescription()
		{
			var template = Factory.New<UniversalCopyTemplate>();
			using (var manager = new UniversalCopyManagerTest.UniversalCopyManagerForTest(typeof(DummyBusinessObject), DummyModuleIDs.Dummy))
			using (var form = new UniversalCopyTemplateForm(template, manager))
			{
				CollectionCopyTemplateBizo coll = GetNewCopyTemplateNodeBizo();
				EntityCopyTemplateBizo ent = coll;

				using (var collectionNodeDetailsNoSplit = new CollectionNodeDetailsUserControl(manager))
				using (var collectionNodeDetailsSplit = new CollectionNodeDetailsUserControl(manager, true))
				{
					Assert(collectionNodeDetailsNoSplit.zTextBox1.ReadOnly);
					Assert(!collectionNodeDetailsSplit.zTextBox1.ReadOnly);
				}
			}
		}

		protected CollectionCopyTemplateBizo GetNewCopyTemplateNodeBizo()
		{
			return CreateNewCopyTemplateNodeBizo("This is the name", DummyBizoSchema.Constants.Z0_Guid, DummyBizoSchema.Constants.TableName, null);
		}

		CollectionCopyTemplateBizo CreateNewCopyTemplateNodeBizo(string collectionName, string itemPropertyName, string itemTableName, EntityCopyTemplateBizo parent)
		{
			return new CollectionCopyTemplateBizo(CreateNewNode(collectionName, itemPropertyName, itemTableName), null, parent);
		}

		CollectionCopyTemplateNode CreateNewNode(string collectionName, string itemPropertyName, string itemTableName)
		{
			return
				new CollectionCopyTemplateNode
				{
					Id = Guid.NewGuid().ToString(),
					Name = collectionName,
					ItemPropertyName = itemPropertyName,
					ItemsTableName = itemTableName
				};
		}
	}
}
