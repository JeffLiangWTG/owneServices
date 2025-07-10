using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.UniversalCopy.Interfaces;
using Moq;
using NUnit.Framework;
using WTG.Glow.Data.Annotations;

namespace CargoWise.UniversalCopy.Test
{
	/// <summary>
	/// This is a test class for ConfigurationTreeTest and is intended to contain all ConfigurationTreeTest Unit Tests
	/// </summary>
	public class CopyTemplateTreeTest : WrappedCopyTemplateNodeTest<CopyTemplateTree>
	{
		/// <summary>
		/// A test for CopyTemplateTree Constructor
		/// </summary>
		public void TestConfigurationTreeConstructor()
		{
			CopyTemplateTree target = CreateConfigurationNode();
			AssertConfigurationTreeTowardsTestModel(target);
		}

		public void TestConfigurationTreeConstructor_CustomRelatedEntityInType_InitializesCustomRelatedEntity()
		{
			// Arrange
			var mockUniversalCopyCustomRelatedEntity = new Mock<IUniversalCopyCustomRelatedEntity>();
			mockUniversalCopyCustomRelatedEntity.Setup(entity => entity.SourceTypeName).Returns(nameof(ITestModel));
			mockUniversalCopyCustomRelatedEntity.Setup(entity => entity.RelatedPropertyName).Returns("Test");
			mockUniversalCopyCustomRelatedEntity.Setup(entity => entity.InitializeRelatedEntityCopyTemplateTreeNode(
				It.IsAny<EntityCopyTemplateNode>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<Func<Type, CopyTemplateNode>>()));

			// Act
			using (ObjectFactory.Substitute("UniversalCopyCustomRelatedEntitiesList", new ListObject() { mockUniversalCopyCustomRelatedEntity.Object }))
			{
				var testTemplateTree = new CopyTemplateTree(typeof(ITestModel));
			}

			// Assert
			mockUniversalCopyCustomRelatedEntity.Verify(entity => entity.InitializeRelatedEntityCopyTemplateTreeNode(
				It.IsAny<EntityCopyTemplateNode>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<Func<Type, CopyTemplateNode>>()), Times.Once);

			Assert("Meaningless assert because analyzer does not recognize moq", true);
		}

		public void TestConfigurationTreeConstructor_CustomRelatedEntityNotInType_DoesNotInitializesCustomRelatedEntity()
		{
			// Arrange
			var mockUniversalCopyCustomRelatedEntity = new Mock<IUniversalCopyCustomRelatedEntity>();
			mockUniversalCopyCustomRelatedEntity.Setup(entity => entity.SourceTypeName).Returns("AnotherType");
			mockUniversalCopyCustomRelatedEntity.Setup(entity => entity.RelatedPropertyName).Returns("Test");
			mockUniversalCopyCustomRelatedEntity.Setup(entity => entity.InitializeRelatedEntityCopyTemplateTreeNode(
				It.IsAny<EntityCopyTemplateNode>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<Func<Type, CopyTemplateNode>>()));

			// Act
			using (ObjectFactory.Substitute("UniversalCopyCustomRelatedEntitiesList", new ListObject() { mockUniversalCopyCustomRelatedEntity.Object }))
			{
				var testTemplateTree = new CopyTemplateTree(typeof(ITestModel));
			}

			// Assert
			mockUniversalCopyCustomRelatedEntity.Verify(entity => entity.InitializeRelatedEntityCopyTemplateTreeNode(
				It.IsAny<EntityCopyTemplateNode>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<Func<Type, CopyTemplateNode>>()), Times.Never);

			Assert("Meaningless assert because analyzer does not recognize moq", true);
		}

		/// <summary>
		/// A test for CopyTemplateTree serialization and deserialization
		/// </summary>
		public void TestSerialization()
		{
			CopyTemplateTree target;
			using (var stream = new MemoryStream())
			{
				CopyTemplateTree source = CreateConfigurationNode();
				source.Filter = new EntityFilter { FilterTypeId = "type1", OrderBy = "PK", FilterData = "xyz" };

				source.Serialize(stream);
				stream.Seek(0, SeekOrigin.Begin);
				target = CopyTemplateTree.Deserialize(stream);
			}

			AssertConfigurationTreeTowardsTestModel(target);

			AssertNotNull(target.Filter);
			AssertEquals("type1", target.Filter.FilterTypeId);
			AssertEquals("PK", target.Filter.OrderBy);
			AssertEquals("xyz", target.Filter.FilterData);
		}

		#region Compact And Extend

		public void TestCompactAndExtend()
		{
			CopyTemplateTree template = new CopyTemplateTree(typeof(TestObject));
			FindCopyTemplateNodeByName<PropertyCopyTemplateNode>(template.InnerNode, "Property1").CopyMethod = CopyMethod.Copy;
			FindCopyTemplateNodeByName<RelatedEntityCopyTemplateNode>(template.InnerNode, "RelatedObject1").CopyMethod = RelatedEntityCopyMethod.Copy;

			RelatedEntityCopyTemplateNode relatedEntity3Node = FindCopyTemplateNodeByName<RelatedEntityCopyTemplateNode>(template.InnerNode, "RelatedObject3");
			relatedEntity3Node.CopyMethod = RelatedEntityCopyMethod.Copy;
			TemplateCopyTemplateNode innerTemplateNode = (TemplateCopyTemplateNode)relatedEntity3Node.InnerNode;
			innerTemplateNode.FindTemplateAndInitializeInnerNode(template);
			FindCopyTemplateNodeByName<PropertyCopyTemplateNode>(innerTemplateNode.InnerNode, "Property3").CopyMethod = CopyMethod.Copy;

			AssertTemplateForCompactAndExtend(template, true, true);
			int templateNodesCount = GetNodesCount(template);

			CopyTemplateTree compactedAndExtendedTemplate = template.GetCompactCopy();
			AssertTemplateForCompactAndExtend(compactedAndExtendedTemplate, false, false);
			Assert(GetNodesCount(compactedAndExtendedTemplate) < templateNodesCount);

			compactedAndExtendedTemplate.Extend(new CopyTemplateTree(typeof(TestObject)));
			AssertTemplateForCompactAndExtend(compactedAndExtendedTemplate, true, true);
			AssertEquals(templateNodesCount, GetNodesCount(compactedAndExtendedTemplate));

			compactedAndExtendedTemplate = template.GetCompactCopy();
			((EntityCopyTemplateNode)compactedAndExtendedTemplate.InnerNode).Nodes.Add(new PropertyCopyTemplateNode { Name = "FK2" });
			compactedAndExtendedTemplate.Extend(new CopyTemplateTree(typeof(TestObject)));
			AssertTemplateForCompactAndExtend(compactedAndExtendedTemplate, true, false);
			Assert(GetNodesCount(compactedAndExtendedTemplate) < templateNodesCount);
		}

		public void TestExtendNodeCopiesTransientProperties()
		{
			var sourcePropertyNode1 = new PropertyCopyTemplateNode { Name = "Property1", Description = "Property One", IsMandatory = true };
			var sourcePropertyNode2 = new PropertyCopyTemplateNode { Name = "Property2", Description = "Property Two", IsMandatory = true };
			var sourcePropertyNode3 = new PropertyCopyTemplateNode { Name = "Property3", Description = "Property Three", IsMandatory = true };
			var sourceEntityNode = new EntityCopyTemplateNode { Name = "Entity" };
			sourceEntityNode.Nodes.AddRange(new[] { sourcePropertyNode1, sourcePropertyNode2, sourcePropertyNode3 });
			var sourceTemplate = new CopyTemplateTree { InnerNode = sourceEntityNode };

			var targetPropertyNode1 = new PropertyCopyTemplateNode { Name = "Property1", Description = "Property One", IsMandatory = false };
			var targetPropertyNode2 = new PropertyCopyTemplateNode { Name = "Property2", Description = "Other description 2", IsMandatory = false };
			var targetPropertyNode3 = new PropertyCopyTemplateNode { Name = "Property3", Description = "", IsMandatory = false };
			var targetEntityNode = new EntityCopyTemplateNode { Name = "Entity" };
			targetEntityNode.Nodes.AddRange(new[] { targetPropertyNode1, targetPropertyNode2, targetPropertyNode3 });
			var targetTemplate = new CopyTemplateTree { InnerNode = targetEntityNode };

			targetTemplate.Extend(sourceTemplate);

			AssertEquals("Property One", targetPropertyNode1.Description);
			Assert(targetPropertyNode1.IsMandatory);
			AssertEquals("Keeps non-empty description", "Other description 2", targetPropertyNode2.Description);
			Assert("Still overrides mandatory flag", targetPropertyNode2.IsMandatory);
			AssertEquals("Property Three", targetPropertyNode3.Description);
			Assert(targetPropertyNode3.IsMandatory);
		}

		public void TestCompactAndExtendWithTemplateNode()
		{
			var compactTree = new CopyTemplateTree();
			var compactEntityNode = new EntityCopyTemplateNode { Id = "100" };
			compactTree.InnerNode = compactEntityNode;
			compactEntityNode.Nodes.Add(new TemplateCopyTemplateNode { Id = "102", TemplateNodeId = "101", InnerNode = new EntityCopyTemplateNode { Id = "103" } });

			var templateTree = new CopyTemplateTree();
			var templateEntityNode = new EntityCopyTemplateNode { Id = "200" };
			templateTree.InnerNode = templateEntityNode;
			templateEntityNode.Nodes.Add(new EntityCopyTemplateNode { Id = "201" });
			templateEntityNode.Nodes.Add(new TemplateCopyTemplateNode { Id = "202", TemplateNodeId = "201" });

			AssertEquals("Precondition", 1, compactEntityNode.Nodes.Count);

			compactTree.Extend(templateTree);

			AssertEquals("Id should be changed", "200", compactEntityNode.Id);
			AssertEquals(2, compactEntityNode.Nodes.Count);
			AssertEquals("Id should be changed", "202", compactEntityNode.Nodes[0].Id);
			AssertEquals("201", compactEntityNode.Nodes[1].Id);
			AssertEquals("Template Id should be changed", "201", ((TemplateCopyTemplateNode)compactEntityNode.Nodes[0]).TemplateNodeId);
			AssertEquals("Ids after template node should not be changed", "103", ((TemplateCopyTemplateNode)compactEntityNode.Nodes[0]).InnerNode.Id);
		}

		[ExpectNoExceptions]
		public void TestCompactAndExtendWithTemplateNode2()
		{
			var compactTree = new CopyTemplateTree();
			var compactTreeInnerNode = new TemplateCopyTemplateNode { Id = "1202", TemplateNodeId = "1201" };
			compactTree.InnerNode = compactTreeInnerNode;

			var templateTree = new CopyTemplateTree();
			var templateEntityNode = new EntityCopyTemplateNode { Id = "200" };
			templateTree.InnerNode = templateEntityNode;

			compactTree.Extend(templateTree);
		}

		public void TestExtendEntityNodeFromTemplateNode()
		{
			var compactTree = new CopyTemplateTree();
			var compactRootNode = new EntityCopyTemplateNode();
			compactTree.InnerNode = compactRootNode;
			var compactEntityNode = new EntityCopyTemplateNode
			{
				Name = "Entity"
			};
			var compactRelatedNode = new RelatedEntityCopyTemplateNode
			{
				Name = "Related Entity",
				InnerNode = compactEntityNode
			};
			compactRootNode.Nodes.Add(compactRelatedNode);

			var templateTree = new CopyTemplateTree();
			var templateRootNode = new EntityCopyTemplateNode { Id = "200" };
			templateTree.InnerNode = templateRootNode;
			var templateEntityNode = new EntityCopyTemplateNode
			{
				Id = "201",
				Name = "Entity",
				Nodes =
				{
					new PropertyCopyTemplateNode
					{
						Id = "202",
						Name = "Property 1",
						PropertyType = "Type1"
					}
				}
			};
			var templateRelatedNode1 = new RelatedEntityCopyTemplateNode
			{
				Id = "203",
				Name = "Other Related Entity",
				InnerNode = templateEntityNode
			};
			templateRootNode.Nodes.Add(templateRelatedNode1);
			var templateRelatedNode2 = new RelatedEntityCopyTemplateNode
			{
				Id = "204",
				Name = "Related Entity",
				InnerNode = new TemplateCopyTemplateNode(templateEntityNode)
				{
					Id = "205",
					TemplateNodeId = "204"
				}
			};
			templateRootNode.Nodes.Add(templateRelatedNode2);

			compactTree.Extend(templateTree);

			AssertEquals("Id should be copied", templateRelatedNode2.Id, compactRelatedNode.Id);
			AssertEquals("Id should be copied", templateEntityNode.Id, compactEntityNode.Id);
			AssertEquals("There should be 1 child added to target node", 1, compactEntityNode.Nodes.Count);
			AssertEquals(typeof(PropertyCopyTemplateNode), compactEntityNode.Nodes[0].GetType());
			AssertEquals(templateEntityNode.Nodes[0].Id, compactEntityNode.Nodes[0].Id);
			AssertEquals("Property 1", compactEntityNode.Nodes[0].Name);
			AssertEquals("Type1", ((PropertyCopyTemplateNode)compactEntityNode.Nodes[0]).PropertyType);
		}

		void AssertTemplateForCompactAndExtend(CopyTemplateTree template, bool expectProperty2, bool expectRelatedObject2)
		{
			AssertEquals(CopyMethod.Copy, FindCopyTemplateNodeByName<PropertyCopyTemplateNode>(template.InnerNode, "Property1").CopyMethod);
			AssertEquals(RelatedEntityCopyMethod.Copy, FindCopyTemplateNodeByName<RelatedEntityCopyTemplateNode>(template.InnerNode, "RelatedObject1").CopyMethod);

			RelatedEntityCopyTemplateNode relatedEntity3Node = FindCopyTemplateNodeByName<RelatedEntityCopyTemplateNode>(template.InnerNode, "RelatedObject3");
			TemplateCopyTemplateNode innerTemplateNode = (TemplateCopyTemplateNode)relatedEntity3Node.InnerNode;
			AssertEquals(CopyMethod.Copy, FindCopyTemplateNodeByName<PropertyCopyTemplateNode>(innerTemplateNode.InnerNode, "Property3").CopyMethod);

			AssertEquals(expectProperty2, FindCopyTemplateNodeByName<CopyTemplateNode>(template.InnerNode, "Property2") != null);
			AssertEquals(expectRelatedObject2, FindCopyTemplateNodeByName<CopyTemplateNode>(template.InnerNode, "RelatedObject2") != null);
		}

		int GetNodesCount(CopyTemplateNode parentNode)
		{
			if (parentNode != null)
			{
				WrappedCopyTemplateNode wrappedNode = parentNode as WrappedCopyTemplateNode;
				if (wrappedNode != null)
				{
					return 1 + GetNodesCount(wrappedNode.InnerNode);
				}

				EntityCopyTemplateNode entityNode = parentNode as EntityCopyTemplateNode;
				if (entityNode != null)
				{
					return 1 + entityNode.Nodes.Sum(node => GetNodesCount(node));
				}

				return 1;
			}

			return 0;
		}

		[TableNameProvider("TestObject")]
		interface ITestObject { }

		class TestObject : CopyManagerTest.TestEntity, ITestObject
		{
			public string Property1 { get; set; }
			public string Property2 { get; set; }

			public Guid FK1 { get; set; }
			[RelationProperty(nameof(FK1))]
			public TestObject2 RelatedObject1 { get; set; }

			public Guid FK2 { get; set; }
			[RelationProperty(nameof(FK2))]
			public TestObject2 RelatedObject2 { get; set; }

			public Guid FK3 { get; set; }
			[RelationProperty(nameof(FK3))]
			public TestObject2 RelatedObject3 { get; set; }
		}

		[TableNameProvider("TestObject2")]
		interface ITestObject2 { }

		class TestObject2 : CopyManagerTest.TestEntity, ITestObject2
		{
			public string Property3 { get; set; }
		}

		#endregion

		#region Extend with duplicates and templates

		public void TestExtendWithDuplicates()
		{
			var sourceMainEntity = new EntityCopyTemplateNode { Name = "MainEntity", Id = "11" };

			var sourceSubEntity1 = new EntityCopyTemplateNode { Name = "SubEntity", Description = "Duplicate 1", Id = "12" };
			sourceMainEntity.Nodes.Add(sourceSubEntity1);
			sourceSubEntity1.Nodes.Add(new PropertyCopyTemplateNode { Name = "Property1", Id = "13" });
			sourceSubEntity1.Nodes.Add(new PropertyCopyTemplateNode { Name = "Property2", Id = "14" });

			var sourceSubEntity2 = new EntityCopyTemplateNode { Name = "SubEntity", Description = "", Id = "15" };
			sourceMainEntity.Nodes.Add(sourceSubEntity2);
			sourceSubEntity2.Nodes.Add(new PropertyCopyTemplateNode { Name = "Property3", Id = "16" });
			sourceSubEntity2.Nodes.Add(new PropertyCopyTemplateNode { Name = "Property4", Id = "17" });

			var targetMainEntity = new EntityCopyTemplateNode { Name = "MainEntity", Id = "21" };
			targetMainEntity.Nodes.Add(new EntityCopyTemplateNode { Name = "SubEntity", Description = "Duplicate 2", Id = "22" });
			targetMainEntity.Nodes.Add(new EntityCopyTemplateNode { Name = "SubEntity", Description = "Duplicate 3", Id = "23" });

			AssertEquals("21", targetMainEntity.Id);
			AssertEquals(2, targetMainEntity.Nodes.Count);

			new CopyTemplateTree { InnerNode = targetMainEntity }.Extend(new CopyTemplateTree { InnerNode = sourceMainEntity });

			AssertEquals("11", targetMainEntity.Id);
			AssertEquals(4, targetMainEntity.Nodes.Count);

			var resultSubEntity1 = targetMainEntity.Nodes.FirstOrDefault(node => node.Name == "SubEntity" && string.IsNullOrEmpty(node.Description)) as EntityCopyTemplateNode;
			AssertEntityNodeAndChildNodes(resultSubEntity1, "15", true, "Property3", "Property4");

			var resultSubEntity2 = targetMainEntity.Nodes.FirstOrDefault(node => node.Name == "SubEntity" && node.Description == "Duplicate 1") as EntityCopyTemplateNode;
			AssertEntityNodeAndChildNodes(resultSubEntity2, "12", true, "Property1", "Property2");

			var resultSubEntity3 = targetMainEntity.Nodes.FirstOrDefault(node => node.Name == "SubEntity" && node.Description == "Duplicate 2") as EntityCopyTemplateNode;
			AssertEntityNodeAndChildNodes(resultSubEntity3, "15", false, "Property3", "Property4");

			var resultSubEntity4 = targetMainEntity.Nodes.FirstOrDefault(node => node.Name == "SubEntity" && node.Description == "Duplicate 3") as EntityCopyTemplateNode;
			AssertEntityNodeAndChildNodes(resultSubEntity4, "15", false, "Property3", "Property4");

			AssertNoDoubleUseOfNodes(targetMainEntity);
		}

		public void TestExtendWithDuplicatesAndWrappedNodes()
		{
			var sourceMainEntity = new EntityCopyTemplateNode { Name = "MainEntity", Id = "11" };

			var sourceRelatedEntity = new RelatedEntityCopyTemplateNode { Name = "RelatedEntity", Id = "12" };
			sourceMainEntity.Nodes.Add(sourceRelatedEntity);

			var sourceSubEntity = new EntityCopyTemplateNode { Name = "SubEntity", Description = "", Id = "13" };
			sourceRelatedEntity.InnerNode = sourceSubEntity;
			sourceSubEntity.Nodes.Add(new PropertyCopyTemplateNode { Name = "Property", Id = "14" });

			var targetMainEntity = new EntityCopyTemplateNode { Name = "MainEntity", Id = "21" };
			targetMainEntity.Nodes.Add(new RelatedEntityCopyTemplateNode { Name = "RelatedEntity", Description = "Duplicate", Id = "22" });

			new CopyTemplateTree { InnerNode = targetMainEntity }.Extend(new CopyTemplateTree { InnerNode = sourceMainEntity });

			AssertEquals(2, targetMainEntity.Nodes.Count);

			var resultRelatedEntity1 = targetMainEntity.Nodes[0] as RelatedEntityCopyTemplateNode;
			AssertNotNull(resultRelatedEntity1);
			AssertEquals("RelatedEntity", resultRelatedEntity1.Name);
			AssertEquals("Duplicate", resultRelatedEntity1.Description);
			AssertNotEquals("Should not copy id", "12", resultRelatedEntity1.Id);
			AssertNotEquals("New id should be generated", "22", resultRelatedEntity1.Id);
			AssertNotNull("Should initialize inner node", resultRelatedEntity1.InnerNode);
			AssertNotEquals("Should create new node", sourceSubEntity.Id, resultRelatedEntity1.InnerNode.Id);
			AssertEquals(sourceSubEntity.GetType(), resultRelatedEntity1.InnerNode.GetType());
			AssertEquals(sourceSubEntity.Name, resultRelatedEntity1.InnerNode.Name);
			AssertEntityNodeAndChildNodes((EntityCopyTemplateNode)resultRelatedEntity1.InnerNode, "13", false, "Property");

			var resultRelatedEntity2 = targetMainEntity.Nodes[1] as RelatedEntityCopyTemplateNode;
			AssertNotNull(resultRelatedEntity2);
			AssertSame(sourceRelatedEntity, resultRelatedEntity2);

			AssertNoDoubleUseOfNodes(targetMainEntity);
		}

		public void TestExtendWithDuplicatesAndTemplates()
		{
			var sourceMainEntity = new EntityCopyTemplateNode { Name = "MainEntity", Id = "11" };
			var sourceMiddleEntity = new EntityCopyTemplateNode { Name = "MiddleEntity", Id = "12" };
			sourceMainEntity.Nodes.Add(sourceMiddleEntity);

			var sourceSubEntity1 = new EntityCopyTemplateNode { Name = "SubEntity1", Id = "13" };
			sourceMiddleEntity.Nodes.Add(sourceSubEntity1);
			sourceSubEntity1.Nodes.Add(new PropertyCopyTemplateNode { Name = "Property", Id = "14" });

			var sourceSubEntity2 = new TemplateCopyTemplateNode(sourceSubEntity1) { Id = "15" };
			sourceMiddleEntity.Nodes.Add(sourceSubEntity2);

			var targetMainEntity = new EntityCopyTemplateNode { Name = "MainEntity", Id = "21" };
			var targetMiddleEntity = new EntityCopyTemplateNode { Name = "MiddleEntity", Description = "Duplicate", Id = "22" };
			targetMainEntity.Nodes.Add(targetMiddleEntity);

			var targetSubEntity1 = new EntityCopyTemplateNode { Name = "SubEntity1", Id = "23" };
			targetMiddleEntity.Nodes.Add(targetSubEntity1);

			var targetSubEntity2 = new TemplateCopyTemplateNode(targetSubEntity1) { Id = "24" };
			targetMiddleEntity.Nodes.Add(targetSubEntity2);
			AssertEquals("Precondition", "23", targetSubEntity2.TemplateNodeId);

			AssertEquals(1, targetMainEntity.Nodes.Count);
			new CopyTemplateTree { InnerNode = targetMainEntity }.Extend(new CopyTemplateTree { InnerNode = sourceMainEntity });
			AssertEquals(2, targetMainEntity.Nodes.Count);

			AssertNotEquals("Should be changed", "23", targetSubEntity1.Id);
			AssertNotEquals("Should not be copied from source as it is in duplicated node", sourceSubEntity1.Id, targetSubEntity1.Id);

			AssertEquals("Should reference new template node id", sourceSubEntity1.Id, targetSubEntity2.TemplateNodeId);
			AssertNotNull("New template node id should reference a node in the tree", targetMainEntity.FindNode(targetSubEntity2.TemplateNodeId));

			AssertNoDoubleUseOfNodes(targetMainEntity);
		}

		void AssertEntityNodeAndChildNodes(EntityCopyTemplateNode entityNode, string id, bool expectId, params string[] nodeNames)
		{
			AssertNotNull(entityNode);
			AssertEquals(expectId, entityNode.Id.Equals(id));
			AssertContainsExactElementsInAnyOrder(nodeNames, entityNode.Nodes.Select(node => node.Name));
		}

		static void AssertNoDoubleUseOfNodes(CopyTemplateNode node)
		{
			AssertNoDoubleUseOfNodes(node, new Dictionary<object, object>());
		}

		static void AssertNoDoubleUseOfNodes(CopyTemplateNode node, Dictionary<object, object> nodeReferences)
		{
			if (node == null)
			{
				return;
			}

			if (nodeReferences.ContainsKey(node))
			{
				Fail("EntityNode " + node.Name + " with ID " + node.Id + " is referenced multiple times.");
			}
			if (nodeReferences.ContainsKey(node.Id))
			{
				Fail("ID " + node.Id + " is used in multiple nodes.");
			}

			nodeReferences.Add(node, null);
			nodeReferences.Add(node.Id, null);

			var wrappedNode = node as WrappedCopyTemplateNode;
			if (wrappedNode != null)
			{
				AssertNoDoubleUseOfNodes(wrappedNode.InnerNode, nodeReferences);
			}

			var entityNode = node as EntityCopyTemplateNode;
			if (entityNode != null)
			{
				foreach (var childNode in entityNode.Nodes)
				{
					AssertNoDoubleUseOfNodes(childNode, nodeReferences);
				}
			}
		}

		#endregion

		#region GetProperties

		public void TestGetProperties()
		{
			AssertEquals(2, CopyTemplateTree.GetPropertiesWithUniqueName(typeof(TestClassWithOverrideProperty)).Count());
		}

		class TestClassWithOverrideProperty : TestAbstractBaseClass, ITestInterface
		{
			public override string TestProperty { get; }

			string ITestInterface.InterfaceProperty => throw new NotImplementedException();
		}

		abstract class TestAbstractBaseClass : ITestInterface
		{
			public abstract string TestProperty { get; }

			string ITestInterface.InterfaceProperty => throw new NotImplementedException();
		}

		interface ITestInterface
		{
			string InterfaceProperty { get; }
		}

		#endregion GetProperties

		#region JobHeaderExclusion

		public void TestCreateTemplateWithJobHeaderAndJobChargeEntities()
		{
			var tree = new CopyTemplateTree(typeof(IJobHeaderRoot), copyTreeConfiguration: new CopyTreeConfigurationForTest());

			using (var stream = new MemoryStream())
			{
				tree.Serialize(stream);
				stream.Seek(0, SeekOrigin.Begin);
				using (var reader = new StreamReader(stream))
				{
					var templateTreeXml = reader.ReadToEnd().Replace("><", ">\n<");
					var expectedXml = ExpectedTemplateTreeWithNoJobHeaderXml.Replace("\t", "").Replace("\r\n", "\n");

					AssertXMLEquals("Should generate correct copy template tree", expectedXml, templateTreeXml);
				}
			}
		}

		/*
			JobHeaderRoot
				Dummies[] // Collection, processed
					Dummy1Info
						Dummy1
					IJobHeader // RelationProperty, NOT processed
				JobHeaders[] // CollectionRelationProperty, NOT processed
					IJobChargeInfo
						JobCharge
				Dummy2 // Dummy template node, processed
					JobCharge // RelationProperty, NOT processed
		*/
		const string ExpectedTemplateTreeWithNoJobHeaderXml =
@"<?xml version=""1.0"" encoding=""utf-16""?>
	<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""JobHeaderRoot"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" TableName=""JobHeaderRoot"" Active=""true"">
		<E N=""JobHeaderRoot"">
			<C N=""Dummies"" ItemPropertyName=""DM_RT"" ItemTableName=""Dummy"" Do=""None"" IsSplitCollection=""false"" Order=""0"">
				<E N=""Dummy"">
					<P N=""DM_D1"" Do=""None"" />
					<P N=""DM_RT"" Do=""None"" />
					<P N=""DM_JH"" Do=""None"" />
				</E>
			</C>
			<R N=""Dummy2"" RelatedPropertyName=""RT_D2"" RelatedEntityTableName=""Dummy2"" Do=""None"">
				<E N=""Dummy2"">
					<P N=""D2_D5"" Do=""None"" />
					<P N=""DM_JC"" Do=""None"" />
				</E>
			</R>
		</E>
	</CopyTemplateTree>";

		[CollectionRelationProperty("Dummies", "DM_RT", "Dummy")]
		[CollectionRelationProperty("JobHeaders", "JH_RT", "JobHeader")]
		[TableNameProvider("JobHeaderRoot")]
		[TableCode("RT")]
		interface IJobHeaderRoot
		{
			Guid RT_D2 { get; set; }

			[RelationProperty(nameof(RT_D2))]
			IDummy2 Dummy2 { get; set; }

			ICollection<IDummy> Dummies { get; }

			ICollection<JobHeader> JobHeaders { get; }
		}

		[TableNameProvider("Dummy")]
		[TableCode("DM")]
		interface IDummy
		{
			Guid DM_D1 { get; set; }

			Guid DM_RT { get; set; }

			Guid DM_JH { get; set; }

			[RelationProperty(nameof(DM_D1))]
			IDummy1Info Dummy1 { get; set; }

			[RelationProperty(nameof(DM_JH))]
			IJobHeader JobHeader { get; set; }
		}

		[TableNameProvider("Dummy1")]
		[TableCode("D1")]
		[InfoType(typeof(IDummy1Info))]
		interface IDummy1
		{
			Guid D1_D3 { get; set; }

			Guid D1_D4 { get; set; }
		}

		[TableNameProvider("Dummy1")]
		[TableCode("D1")]
		[InfoTypeFor(typeof(IDummy1))]
		interface IDummy1Info
		{
			Guid D1_D3 { get; set; }

			Guid D1_D4 { get; set; }
		}

		[TableNameProvider("Dummy2")]
		[TableCode("D2")]
		interface IDummy2
		{
			Guid D2_D5 { get; set; }

			Guid DM_JC { get; set; }

			[RelationProperty(nameof(DM_JC))]
			IJobCharge JobCharge { get; set; }
		}

		[TableNameProvider("JobHeader")]
		[TableCode("JH")]
		public interface IJobHeader
		{
			Guid JH_JC { get; set; }

			Guid JH_RT { get; set; }

			IJobChargeInfo JobCharge { get; set; }
		}

		[UniversalCopyIgnoreBusinessObject]
		public class JobHeader : IJobHeader
		{
			public Guid JH_JC { get; set; }

			public Guid JH_RT { get; set; }

			[RelationProperty(nameof(JH_JC))]
			public IJobChargeInfo JobCharge { get; set; }
		}

		[TableNameProvider("JobCharge")]
		[TableCode("JC")]
		[InfoType(typeof(IJobChargeInfo))]
		public interface IJobCharge
		{
			Guid JC_DM { get; set; }
		}

		[TableNameProvider("JobCharge")]
		[TableCode("JC")]
		[InfoTypeFor(typeof(IJobCharge))]
		public interface IJobChargeInfo
		{
			Guid JC_DM { get; set; }
		}

		#endregion

		#region CreateTemplateWithDuplicatesInLinkableRelateds

		public void TestCreateTemplateWithDuplicatesInLinkableRelateds()
		{
			var tree = new CopyTemplateTree(typeof(IRoot), copyTreeConfiguration: new CopyTreeConfigurationForTest());

			using (var stream = new MemoryStream())
			{
				tree.Serialize(stream);
				stream.Seek(0, SeekOrigin.Begin);
				using (var reader = new StreamReader(stream))
				{
					var templateTreeXml = reader.ReadToEnd().Replace("><", ">\n<");
					var expectedXml = ExpectedTemplateTreeXml.Replace("\t", "").Replace("\r\n", "\n");

					AssertXMLEquals("Should generate correct copy template tree", expectedXml, templateTreeXml);
				}
			}
		}

		#region Test model

		/*
			Root
				OC[] // Collection, procesed before related entities
					O1Info // Linkable only, will be converted to property
						O2
							O3
						O4
							O2
				O4 // Template node
					O2 // Template node in template node
						O3 // Test object to check
		//*/

		const string ExpectedTemplateTreeXml =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<CopyTemplateTree xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" N=""Root"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" TableName=""Root"" Active=""true"">
	<E N=""Root"">
		<C N=""OCs"" ItemPropertyName=""OC_RT"" ItemTableName=""OC"" Do=""None"" IsSplitCollection=""false"" Order=""0"">
			<E N=""IOC"">
				<P N=""OC_O1"" Do=""None"" />
				<P N=""OC_RT"" Do=""None"" />
			</E>
		</C>
		<R N=""O4"" RelatedPropertyName=""RT_O4"" RelatedEntityTableName=""O4"" Do=""None"">
			<E N=""IO4"">
				<R N=""O2"" RelatedPropertyName=""O4_O2"" RelatedEntityTableName=""O2"" Do=""None"">
					<E N=""IO2"">
						<R N=""O3"" RelatedPropertyName=""O2_O3"" RelatedEntityTableName=""O3"" Do=""None"">
							<E N=""IO3"">
								<P N=""Name"" Do=""None"" />
							</E>
						</R>
					</E>
				</R>
			</E>
		</R>
	</E>
</CopyTemplateTree>";

		[CollectionRelationProperty("OCs", "OC_RT", "OC")]
		[TableNameProvider("Root")]
		[TableCode("RT")]
		interface IRoot
		{
			Guid RT_O4 { get; set; }

			[RelationProperty(nameof(RT_O4))]
			IO4 O4 { get; set; }

			ICollection<IOC> OCs { get; }
		}

		[TableNameProvider("OC")]
		[TableCode("OC")]
		interface IOC
		{
			Guid OC_O1 { get; set; }

			Guid OC_RT { get; set; }

			[RelationProperty(nameof(OC_O1))]
			IO1Info O1 { get; set; }
		}

		[TableNameProvider("O1")]
		[TableCode("O1")]
		[InfoType(typeof(IO1Info))]
		interface IO1
		{
			Guid O1_O2 { get; set; }

			Guid O1_O4 { get; set; }

			[RelationProperty(nameof(O1_O2))]
			IO2 O2 { get; set; }

			[RelationProperty(nameof(O1_O4))]
			IO4 O4 { get; set; }
		}

		[TableNameProvider("O1")]
		[TableCode("O1")]
		[InfoTypeFor(typeof(IO1))]
		interface IO1Info
		{
			Guid O1_O2 { get; set; }

			Guid O1_O4 { get; set; }

			[RelationProperty(nameof(O1_O2))]
			IO2 O2 { get; }

			[RelationProperty(nameof(O1_O4))]
			IO4 O4 { get; }
		}

		[TableNameProvider("O2")]
		[TableCode("O2")]
		interface IO2
		{
			Guid O2_O3 { get; set; }

			[RelationProperty(nameof(O2_O3))]
			IO3 O3 { get; set; }
		}

		[TableNameProvider("O3")]
		[TableCode("O3")]
		interface IO3
		{
			string Name { get; set; }
		}

		[TableNameProvider("O4")]
		[TableCode("O4")]
		interface IO4
		{
			Guid O4_O2 { get; set; }

			[RelationProperty(nameof(O4_O2))]
			IO2 O2 { get; set; }
		}

		#endregion

		#region Test configuration

		class CopyTreeConfigurationForTest : ICopyTreeConfiguration
		{
			public void PreInitializeEntity(EntityCopyTemplateNode copyTemplateNode, Type type, Type componentType)
			{
			}

			public void AdditionalEntityInitialization(EntityCopyTemplateNode copyTemplateNode, Type type, Type componentType, IDictionary<Type, EntityCopyTemplateNode> preprocessedTypes, EventHandler<CopyTemplateTree.ExceptionThrownEventArgs> exceptionThrownHandle)
			{
			}

			public string GetAssociateCollectionPropertyName(Type componentType, string collectionPropertyName)
			{
				throw new NotImplementedException();
			}

			public Type GetCollectionElementTypeFromCollectionType(Type collectionType)
			{
				throw new NotImplementedException();
			}

			public Type GetEntityTypeFromTableName(string tableName)
			{
				return null;
			}

			public IEnumerable<string> GetExcludedElements(Type type, Type componentType)
			{
				return Enumerable.Empty<string>();
			}

			public bool ShouldMoveLinkableOnlyEntityToProperties()
			{
				return true; // To test conversion of linkable entity with template sub-nodes into a property node
			}

			public IEnumerable<PropertyInfo> GetAddInfoProperties(Type componentType)
			{
				return Enumerable.Empty<PropertyInfo>();
			}

			public IEnumerable<PropertyInfo> GetValueOnlyProperties(Type componentType)
			{
				return Enumerable.Empty<PropertyInfo>();
			}

			public Type GetPropertyTypeSubstitute(Type type)
			{
				return null;
			}
		}

		#endregion

		#endregion

		#region Implementation

		void AssertConfigurationTreeTowardsTestModel(CopyTemplateTree target)
		{
			AssertConfigurationNode(target, typeof(CopyTemplateTree), "TestModel");
			AssertConfigurationNode(target.InnerNode, typeof(EntityCopyTemplateNode), "TestModel");

			var nodes = ((EntityCopyTemplateNode)target.InnerNode).Nodes.OrderBy(node => node.Name).ToArray();

			AssertEquals(10, nodes.Length);
			AssertConfigurationNode(nodes[0], typeof(CollectionCopyTemplateNode), "RecursiveElements");
			AssertConfigurationNode(nodes[1], typeof(RelatedEntityCopyTemplateNode), "RelatedElementA");
			AssertConfigurationNode(nodes[2], typeof(RelatedEntityCopyTemplateNode), "RelatedElementB");
			AssertConfigurationNode(nodes[3], typeof(CollectionCopyTemplateNode), "SimpleElements");
			AssertConfigurationNode(nodes[4], typeof(PropertyCopyTemplateNode), "TM_Bitmap");
			AssertConfigurationNode(nodes[5], typeof(PropertyCopyTemplateNode), "TM_Bool");
			AssertConfigurationNode(nodes[6], typeof(PropertyCopyTemplateNode), "TM_Code");
			AssertConfigurationNode(nodes[7], typeof(PropertyCopyTemplateNode), "TM_Decimal");
			AssertConfigurationNode(nodes[8], typeof(PropertyCopyTemplateNode), "TM_Description");
			AssertConfigurationNode(nodes[9], typeof(PropertyCopyTemplateNode), "TM_Number");

			AssertConfigurationNode(((CollectionCopyTemplateNode)nodes[0]).InnerNode, typeof(EntityCopyTemplateNode), "RecursiveElement");
			var innerNodes = ((EntityCopyTemplateNode)((CollectionCopyTemplateNode)nodes[0]).InnerNode).Nodes.OrderBy(node => node.Name).ToArray();
			AssertEquals(3, innerNodes.Length);
			AssertConfigurationNode(innerNodes[0], typeof(CollectionCopyTemplateNode), "Children");
			AssertConfigurationNode(((CollectionCopyTemplateNode)innerNodes[0]).InnerNode, typeof(TemplateCopyTemplateNode), "RecursiveElement");
			AssertEquals(((CollectionCopyTemplateNode)nodes[0]).InnerNode.Id, ((TemplateCopyTemplateNode)((CollectionCopyTemplateNode)innerNodes[0]).InnerNode).TemplateNodeId);
			AssertConfigurationNode(innerNodes[1], typeof(PropertyCopyTemplateNode), "E2_Code");
			AssertConfigurationNode(innerNodes[2], typeof(PropertyCopyTemplateNode), "E2_Parent");

			AssertConfigurationNode(((RelatedEntityCopyTemplateNode)nodes[1]).InnerNode, typeof(EntityCopyTemplateNode), "RelatedElement");
			innerNodes = ((EntityCopyTemplateNode)((RelatedEntityCopyTemplateNode)nodes[1]).InnerNode).Nodes.OrderBy(node => node.Name).ToArray();
			AssertEquals(3, innerNodes.Length);
			AssertConfigurationNode(innerNodes[0], typeof(PropertyCopyTemplateNode), "E3_Code");
			AssertConfigurationNode(innerNodes[1], typeof(RelatedEntityCopyTemplateNode), "SimpleElement");
			AssertConfigurationNode(((RelatedEntityCopyTemplateNode)innerNodes[1]).InnerNode, typeof(TemplateCopyTemplateNode), "SimpleElement");
			AssertEquals(((CollectionCopyTemplateNode)nodes[3]).InnerNode.Id, ((TemplateCopyTemplateNode)((RelatedEntityCopyTemplateNode)innerNodes[1]).InnerNode).TemplateNodeId);
			AssertConfigurationNode(innerNodes[2], typeof(RelatedEntityCopyTemplateNode), "SubRelatedElement");
			AssertConfigurationNode(((RelatedEntityCopyTemplateNode)innerNodes[2]).InnerNode, typeof(TemplateCopyTemplateNode), "RelatedElement");
			AssertEquals(((RelatedEntityCopyTemplateNode)nodes[1]).InnerNode.Id, ((TemplateCopyTemplateNode)((RelatedEntityCopyTemplateNode)innerNodes[2]).InnerNode).TemplateNodeId);

			AssertConfigurationNode(((RelatedEntityCopyTemplateNode)nodes[2]).InnerNode, typeof(TemplateCopyTemplateNode), "RelatedElement");
			AssertNull(((TemplateCopyTemplateNode)((RelatedEntityCopyTemplateNode)nodes[2]).InnerNode).InnerNode);
			AssertEquals(((RelatedEntityCopyTemplateNode)nodes[1]).InnerNode.Id, ((TemplateCopyTemplateNode)((RelatedEntityCopyTemplateNode)nodes[2]).InnerNode).TemplateNodeId);

			AssertEquals("E1_TM", ((CollectionCopyTemplateNode)nodes[3]).ItemPropertyName);
			AssertEquals("SimpleElement", ((CollectionCopyTemplateNode)nodes[3]).ItemsTableName);
			AssertEquals("E1_ParentTable", ((CollectionCopyTemplateNode)nodes[3]).ItemParentTablePropertyName);
			AssertConfigurationNode(((CollectionCopyTemplateNode)nodes[3]).InnerNode, typeof(EntityCopyTemplateNode), "SimpleElement");
			innerNodes = ((EntityCopyTemplateNode)((CollectionCopyTemplateNode)nodes[3]).InnerNode).Nodes.OrderBy(node => node.Name).ToArray();
			AssertEquals(2, innerNodes.Length);
			AssertConfigurationNode(innerNodes[0], typeof(PropertyCopyTemplateNode), "E1_Code");
			AssertConfigurationNode(innerNodes[1], typeof(PropertyCopyTemplateNode), "E1_TM");
		}

		void AssertConfigurationNode(CopyTemplateNode node, Type expectedNodeType, string expectedCaption)
		{
			AssertEquals(expectedNodeType, node.GetType());
			AssertEquals(expectedCaption, node.Name);
		}

		protected override CopyTemplateTree CreateConfigurationNode()
		{
			return new CopyTemplateTree(typeof(ITestModel));
		}

		protected override string ExpectedName
		{
			get { return "TestModel"; }
		}

		protected override IEnumerable<KeyValuePair<string, CopyTemplateNode>> GetExpectedFindNodePairs(CopyTemplateNode root)
		{
			yield return new KeyValuePair<string, CopyTemplateNode>(root.Id, root);

			WrappedCopyTemplateNode wrappedNode = root as WrappedCopyTemplateNode;
			if (wrappedNode != null && wrappedNode.InnerNode != null)
			{
				foreach (var pair in GetExpectedFindNodePairs(wrappedNode.InnerNode))
				{
					yield return pair;
				}
			}

			EntityCopyTemplateNode entity = root as EntityCopyTemplateNode;
			if (entity != null)
			{
				foreach (var pair in entity.Nodes.SelectMany(GetExpectedFindNodePairs))
				{
					yield return pair;
				}
			}
		}

		#endregion
	}
}
