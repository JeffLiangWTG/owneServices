using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;

namespace Enterprise.ZArchitecture.Business.UniversalCopy.Testing
{
	sealed class BusinessObjectToCopyTemplateReflectionHelperTest : NUnit.Framework.TestCase
	{
		public void TestGetProperties()
		{
			var properties = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(typeof(DummyComponent)).OrderBy(info => info.Name).ToArray();

			AssertEquals(13, properties.Length);
			AssertEquals("PropertyA", properties[0].Name);
			AssertEquals("PropertyAInfo", properties[1].Name);
			AssertEquals("PropertyB", properties[2].Name);
			AssertEquals("PropertyBInfo", properties[3].Name);
			AssertEquals("PropertyC", properties[4].Name);
			AssertEquals("PropertyCInfo", properties[5].Name);
			AssertEquals("PropertyD", properties[6].Name);
			AssertEquals("PropertyDInfo", properties[7].Name);
			AssertEquals("PropertyE", properties[8].Name);
			AssertEquals("PropertyEInfo", properties[9].Name);
			AssertEquals("PropertyF", properties[10].Name);
			AssertEquals("PropertyG", properties[11].Name);
			AssertEquals("PropertyGInfo", properties[12].Name);

			properties = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(typeof(DummyComponent), false).OrderBy(info => info.Name).ToArray();

			AssertEquals(11, properties.Length);
			AssertEquals("PropertyB", properties[0].Name);
			AssertEquals("PropertyBInfo", properties[1].Name);
			AssertEquals("PropertyC", properties[2].Name);
			AssertEquals("PropertyCInfo", properties[3].Name);
			AssertEquals("PropertyD", properties[4].Name);
			AssertEquals("PropertyDInfo", properties[5].Name);
			AssertEquals("PropertyE", properties[6].Name);
			AssertEquals("PropertyEInfo", properties[7].Name);
			AssertEquals("PropertyF", properties[8].Name);
			AssertEquals("PropertyG", properties[9].Name);
			AssertEquals("PropertyGInfo", properties[10].Name);

			properties = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(typeof(DummyComponent), false, true).OrderBy(info => info.Name).ToArray();

			AssertEquals(10, properties.Length);
			var expectedNames = new[] { "PropertyB", "PropertyBInfo", "PropertyC", "PropertyCInfo", "PropertyD", "PropertyDInfo", "PropertyE", "PropertyEInfo", "PropertyF", "PropertyGInfo" };
			AssertContainsExactElementsInAnyOrder(expectedNames, properties.Select(x => x.Name));
		}

		public void TestGetCollectionCopyTemplateNode()
		{
			AssertNull("Non-collection type", BusinessObjectToCopyTemplateReflectionHelper.GetCollectionCopyTemplateNode(typeof(CargoWise.EntityFramework.Testing.DummyBusinessObject), null, "Collection1", "Table1", "FK1", "ParentTableCode1", null));

			var collectionCopyTemplateNode = BusinessObjectToCopyTemplateReflectionHelper.GetCollectionCopyTemplateNode(typeof(CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection), null, "Collection2", "Table2", "FK2", "ParentTableCode2", null);
			AssertNotNull(collectionCopyTemplateNode);
			AssertEquals("Collection2", collectionCopyTemplateNode.Name);
			AssertEquals("Table2", collectionCopyTemplateNode.ItemsTableName);
			AssertEquals("FK2", collectionCopyTemplateNode.ItemPropertyName);
			AssertEquals("ParentTableCode2", collectionCopyTemplateNode.ItemParentTablePropertyName);
			Assert(collectionCopyTemplateNode.InnerNode is EntityCopyTemplateNode);
			AssertEquals("DummyBizo", collectionCopyTemplateNode.InnerNode.Name);
		}

		public void TestGetEntityCopyTemplateNode()
		{
			var copyTemplateNode = BusinessObjectToCopyTemplateReflectionHelper.GetEntityCopyTemplateNode(typeof(DummyComponent), null, new Dictionary<Type, EntityCopyTemplateNode>(), null, "PropertyC");
			AssertDummyComponentEntityNodeSkipC((EntityCopyTemplateNode)copyTemplateNode);

			var entityCopyTemplateNode = BusinessObjectToCopyTemplateReflectionHelper.GetEntityCopyTemplateNode(typeof(DummyComponent), null, new Dictionary<Type, EntityCopyTemplateNode>(), null, "*") as EntityCopyTemplateNode;
			AssertNotNull(entityCopyTemplateNode);
			AssertEquals("Should skip all properties", 0, entityCopyTemplateNode.Nodes.Count);
		}

		void AssertDummyComponentEntityNodeSkipC(EntityCopyTemplateNode entityCopyTemplateNode)
		{
			var nodes = entityCopyTemplateNode.Nodes.OrderBy(node => node.Name).Cast<PropertyCopyTemplateNode>().ToArray();

			AssertEquals(3, nodes.Length);
			AssertEquals("PropertyA", nodes[0].Name);
			AssertEquals(nameof(Boolean), nodes[0].PropertyType);
			AssertEquals("PropertyB", nodes[1].Name);
			AssertEquals(nameof(Int32), nodes[1].PropertyType);
			AssertEquals("PropertyD", nodes[2].Name);
			AssertEquals(nameof(DateTime), nodes[2].PropertyType);
		}

		public void TestGetRelatedEntityCopyTemplateNode()
		{
			var relatedEntityCopyTemplateNode = BusinessObjectToCopyTemplateReflectionHelper.GetRelatedEntityCopyTemplateNode(typeof(DummyComponent), new Dictionary<Type, EntityCopyTemplateNode>(), "Buzz", null, "PropertyC");
			AssertNotNull(relatedEntityCopyTemplateNode);
			AssertEquals("Buzz", relatedEntityCopyTemplateNode.Name);

			var entityCopyTemplateNode = relatedEntityCopyTemplateNode.InnerNode as EntityCopyTemplateNode;
			AssertDummyComponentEntityNodeSkipC(entityCopyTemplateNode);
		}

		public void TestGetSystemTypeFromZType()
		{
			AssertEquals(typeof(string), BusinessObjectToCopyTemplateReflectionHelper.GetSystemTypeFromZType(typeof(ZString)));
			AssertEquals(typeof(int), BusinessObjectToCopyTemplateReflectionHelper.GetSystemTypeFromZType(typeof(ZInt)));
			AssertEquals(typeof(decimal), BusinessObjectToCopyTemplateReflectionHelper.GetSystemTypeFromZType(typeof(ZDecimal)));
			AssertEquals(typeof(bool), BusinessObjectToCopyTemplateReflectionHelper.GetSystemTypeFromZType(typeof(ZBool)));
			AssertEquals(typeof(DateTime), BusinessObjectToCopyTemplateReflectionHelper.GetSystemTypeFromZType(typeof(ZDateTime)));
			AssertEquals(typeof(Guid), BusinessObjectToCopyTemplateReflectionHelper.GetSystemTypeFromZType(typeof(ZGuid)));
		}

		public void TestGetEntityCopyTemplateNode_DeepScan()
		{
			var entityCopyTemplateNode = (EntityCopyTemplateNode)BusinessObjectToCopyTemplateReflectionHelper.GetEntityCopyTemplateNode(typeof(DeepDummy1), null, new Dictionary<Type, EntityCopyTemplateNode>(), null);
			AssertEquals("DeepDummy1", entityCopyTemplateNode.Name);
			AssertEquals(1, entityCopyTemplateNode.Nodes.Count);

			var childNode = entityCopyTemplateNode.Nodes[0];
			AssertEquals("Child", childNode.Name);

			RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode = childNode as RelatedEntityCopyTemplateNode;
			AssertNotNull(relatedEntityCopyTemplateNode);
			AssertNotNull(relatedEntityCopyTemplateNode.InnerNode);
			AssertEquals("DeepDummy2", relatedEntityCopyTemplateNode.InnerNode.Name);

			var childEntityNode = relatedEntityCopyTemplateNode.InnerNode as EntityCopyTemplateNode;
			AssertNotNull(childEntityNode);
			AssertEquals(2, childEntityNode.Nodes.Count);

			var grandchildNodes = childEntityNode.Nodes.OrderBy(node => node.Name).ToArray();

			AssertEquals("Property", grandchildNodes[0].Name);
			AssertEquals(typeof(PropertyCopyTemplateNode), grandchildNodes[0].GetType());

			AssertEquals("RecursiveChild", grandchildNodes[1].Name);
			AssertEquals(typeof(RelatedEntityCopyTemplateNode), grandchildNodes[1].GetType());
			var grandchildInnerNode = ((RelatedEntityCopyTemplateNode)grandchildNodes[1]).InnerNode;
			AssertEquals("DeepDummy2", grandchildInnerNode.Name);
			AssertEquals(typeof(TemplateCopyTemplateNode), grandchildInnerNode.GetType());
			AssertEquals(childEntityNode.Id, ((TemplateCopyTemplateNode)grandchildInnerNode).TemplateNodeId);
		}

		#region Test classes

		interface IDummy
		{
			ZBool PropertyA { get; set; }
			ZPropertyInfo PropertyAInfo { get; }
		}

		class DummyBase
		{
			public ZInt PropertyB { get; set; }
			public ZPropertyInfo PropertyBInfo { get { return null; } }

			public ZString PropertyC { get; set; }
			public ZPropertyInfo PropertyCInfo { get { return null; } }
		}

		[UniversalCopyIgnoreElement("PropertyG")]
		class DummyComponent : DummyBase, IDummy
		{
			ZBool IDummy.PropertyA { get; set; }
			ZPropertyInfo IDummy.PropertyAInfo { get { return null; } }

			public ZDateTime PropertyD { get; set; }
			public ZPropertyInfo PropertyDInfo { get { return null; } }

			public ZGuid PropertyE { get { return ZGuid.Empty; } } // Readonly
			public ZPropertyInfo PropertyEInfo { get { return null; } }

			public ZString PropertyF { get; set; } // No property info

			public string PropertyG { get; set; } // Not IZType
			public ZPropertyInfo PropertyGInfo { get { return null; } }
		}

		[UniversalCopyWithExtendedEntities]
		class DeepDummy1
		{
			[UniversalCopyRelatedEntity]
			public DeepDummy2 Child { get; set; }
		}

		[UniversalCopyWithExtendedEntities]
		class DeepDummy2
		{
			public ZString Property { get; set; }
			public ZPropertyInfo PropertyInfo { get { return null; } }

			[UniversalCopyRelatedEntity]
			public DeepDummy2 RecursiveChild { get; set; }
		}

		#endregion
	}
}
