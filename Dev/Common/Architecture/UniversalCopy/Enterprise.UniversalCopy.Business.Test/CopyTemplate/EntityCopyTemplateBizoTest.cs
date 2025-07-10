using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.UniversalCopy;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.Business.Testing
{
	abstract class EntityCopyTemplateBizoTest<T> : ConfigurationNodeBizoTest<T> where T : EntityCopyTemplateBizo
	{
		public void TestChildNodes()
		{
			var entityCopyBizo = GetNewCopyTemplateNodeBizo();
			var childNodes = entityCopyBizo.ChildNodes;
			AssertNotNull(childNodes);
			AssertEquals(0, childNodes.Count);

			entityCopyBizo = CreateNewEntityCopyTemplateBizo();
			childNodes = entityCopyBizo.ChildNodes;

			AssertNotNull(childNodes);
			AssertSame("Should be cached and do not create new instances", childNodes, entityCopyBizo.ChildNodes);
			Assert("Should be registered as editable child", entityCopyBizo.IsRegisteredEditableChildObject(childNodes));
			AssertEquals(3, childNodes.Count);
			AssertEquals("Collection1", childNodes[0].Name);
			AssertEquals("Collection2", childNodes[1].Name);
			AssertEquals("Related2", childNodes[2].Name);
			Assert(!childNodes.AllowNew);
			Assert(!childNodes.AllowRemove);
		}

		public void TestPropertyNodes()
		{
			var entityCopyBizo = GetNewCopyTemplateNodeBizo();
			var propertyNodes = entityCopyBizo.PropertyNodes;
			AssertNotNull(propertyNodes);
			AssertEquals(0, propertyNodes.Count);

			entityCopyBizo = CreateNewEntityCopyTemplateBizo();
			propertyNodes = entityCopyBizo.PropertyNodes;

			AssertNotNull(propertyNodes);
			AssertSame("Should be cached and do not create new instances", propertyNodes, entityCopyBizo.PropertyNodes);
			Assert("Should be registered as editable child", entityCopyBizo.IsRegisteredEditableChildObject(propertyNodes));
			AssertEquals(3, propertyNodes.Count);
			AssertEquals("Property1", propertyNodes[0].Name);
			AssertEquals("Property2", propertyNodes[1].Name);
			AssertEquals("Property3", propertyNodes[2].Name);
			Assert(!propertyNodes.AllowNew);
			Assert(!propertyNodes.AllowRemove);
		}

		public void TestFilterStripBizo()
		{
			var entityCopyBizo = GetNewCopyTemplateNodeBizo();
			AssertNull(entityCopyBizo.FilterStripBizo);

			var filter1 = new FilterBusinessObjectForTest();
			var filter2 = new FilterBusinessObjectForTest();

			entityCopyBizo.FilterStripBizo = filter1;
			AssertEquals(filter1, entityCopyBizo.FilterStripBizo);
			Assert(entityCopyBizo.IsRegisteredEditableChildObject(filter1));
			Assert(!entityCopyBizo.IsRegisteredEditableChildObject(filter2));

			entityCopyBizo.FilterStripBizo = filter2;
			AssertEquals(filter2, entityCopyBizo.FilterStripBizo);
			Assert(!entityCopyBizo.IsRegisteredEditableChildObject(filter1));
			Assert(entityCopyBizo.IsRegisteredEditableChildObject(filter2));
		}

		public virtual void TestRunPreSaveValidationCoreLoadsChildCollections()
		{
			var entityCopyBizo = CreateNewEntityCopyTemplateBizo();
			AssertNull("Child entities collection is not loaded yet", entityCopyBizo.ChildNodesNoCreateForTest);
			AssertNull("Property nodes collection is not loaded yet", entityCopyBizo.PropertyNodesNoCreateForTest);

			Assert("Precondition", !entityCopyBizo.CopyTemplateNode.HasData());
			entityCopyBizo.RunPreSaveValidation();
			AssertNull("Child entities collection is not loaded as parent entity doesn't has data", entityCopyBizo.ChildNodesNoCreateForTest);
			AssertNull("Property nodes collection is not loaded as parent entity doesn't has data", entityCopyBizo.PropertyNodesNoCreateForTest);

			SetEntityCopyTemplateBizoToHaveData(entityCopyBizo);
			Assert("Should have data", entityCopyBizo.CopyTemplateNode.HasData());
			entityCopyBizo.RunPreSaveValidation();
			AssertNotNull("Child entities collection should be loaded", entityCopyBizo.ChildNodesNoCreateForTest);
			AssertNotNull("Property nodes collection should be loaded", entityCopyBizo.PropertyNodesNoCreateForTest);
		}

		#region Implementation

		class FilterBusinessObjectForTest : FilterBusinessObject
		{
			public FilterBusinessObjectForTest()
				: base(new BusinessObjectFactory(), new DataTable().NewRow())
			{ }

			public override ZQuery Filter
			{
				get { return new ZQuery(); }
			}

			protected sealed override void SetPKAndDefaults()
			{ }
		}

		T CreateNewEntityCopyTemplateBizo()
		{
			var entityCopyTemplateBizo = GetNewCopyTemplateNodeBizo();
			entityCopyTemplateBizo.ParentPropertyName = "X";

			var innerEntityNode = new EntityCopyTemplateNode { Id = Guid.NewGuid().ToString() };
			innerEntityNode.Nodes.Add(new RelatedEntityCopyTemplateNode { Name = "Related2", RelatedPropertyName = "Y" });
			innerEntityNode.Nodes.Add(new CollectionCopyTemplateNode { Name = "Collection1" });
			innerEntityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "Property2" });
			innerEntityNode.Nodes.Add(new RelatedEntityCopyTemplateNode { Name = "Related1", RelatedPropertyName = "X" }); // Will be skipped
			innerEntityNode.Nodes.Add(new CollectionCopyTemplateNode { Name = "Collection2" });
			innerEntityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "Property3" });
			innerEntityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "Property1" });

			entityCopyTemplateBizo.CopyTemplateNode.InnerNode = new TemplateCopyTemplateNode(innerEntityNode) { InnerNode = innerEntityNode };

			return entityCopyTemplateBizo;
		}

		protected abstract void SetEntityCopyTemplateBizoToHaveData(T entityCopyTemplateBizo);

		#endregion
	}

	public abstract class CopyTemplateNodeBizoCollectionTest<T, U> : NonPersistentBusinessObjectCollectionTestCase<T>
		where T : CopyTemplateNodeBizoCollection<U> where U : CopyTemplateNodeBizo
	{
		[ExpectNoExceptions]
		public void TestLoadNodesNoRecursion()
		{
			var templateTree = new CopyTemplateTree { Name = "Tree" };

			var entityNode = new EntityCopyTemplateNode { Name = "Entity" };
			templateTree.InnerNode = entityNode;

			var relatedEntityNode = new RelatedEntityCopyTemplateNode { Id = Guid.NewGuid().ToString(), Name = "Related" };
			entityNode.Nodes.Add(relatedEntityNode);

			var templateNode = new TemplateCopyTemplateNode { Id = Guid.NewGuid().ToString(), Name = "Template", TemplateNodeId = relatedEntityNode.Id };
			relatedEntityNode.InnerNode = templateNode;

			var collection = (CopyTemplateNodeBizoCollection<U>)GetCollectionToTest();
			collection.LoadNodes(relatedEntityNode, templateTree, "");

			AssertEquals(0, collection.Count);

			AssertEquals("InvalidOperationException_LoadNodes", ErrorReporter.LastKeyReported);
			AssertEquals(
				"Error populating TemplateCopyTemplateNode with name Template while loading nodes into " + collection.GetType().Name + ", located at:\r\n" +
				"CopyTemplateTree Tree\\EntityCopyTemplateNode Entity\\RelatedEntityCopyTemplateNode Related\\TemplateCopyTemplateNode Template",
				ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}
	}

	[TestedType(typeof(EntityCopyTemplateBizoCollection))]
	public class EntityCopyTemplateBizoCollectionTest : CopyTemplateNodeBizoCollectionTest<EntityCopyTemplateBizoCollection, EntityCopyTemplateBizo>
	{
		protected override EntityCopyTemplateBizoCollection GetCollectionToTest()
		{
			return new EntityCopyTemplateBizoCollection(null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CollectionCopyTemplateBizo(null, null, null);
		}
	}

	[TestedType(typeof(PropertyCopyTemplateBizoCollection))]
	public class PropertyCopyTemplateBizoCollectionTest : CopyTemplateNodeBizoCollectionTest<PropertyCopyTemplateBizoCollection, PropertyCopyTemplateBizo>
	{
		protected override PropertyCopyTemplateBizoCollection GetCollectionToTest()
		{
			return new PropertyCopyTemplateBizoCollection(null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PropertyCopyTemplateBizo(null, null);
		}
	}

	[TestedType(typeof(ValueOnlyPropertyCopyTemplateBizoCollection))]
	public class ValueOnlyPropertyCopyTemplateBizoCollectionTest : CopyTemplateNodeBizoCollectionTest<ValueOnlyPropertyCopyTemplateBizoCollection, PropertyCopyTemplateBizo>
	{
		protected override ValueOnlyPropertyCopyTemplateBizoCollection GetCollectionToTest()
		{
			return new ValueOnlyPropertyCopyTemplateBizoCollection(null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PropertyCopyTemplateBizo(null, null);
		}
	}
}
