using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.UniversalCopy.Interfaces;
using Moq;

namespace CargoWise.UniversalCopy.Test
{
	public class UniversalCopyCustomRelatedEntityManagerTest : TestCaseWithFactory
	{
		public void TestInitializeRelatedEntityCopyTemplateTreeNodes_CallsInitializeRelatedEntityCopyTemplateTreeNodeOnEachEntityOfTypeName()
		{
			// Arrange
			var entityNode = new EntityCopyTemplateNode();
			var type = typeof(EntityCopyTemplateNode);
			var processedProperties = new Dictionary<string, object>();
			var initilizeSubEntityNode = (Type type) => new EntityCopyTemplateNode();

			var mockUniversalCopyCustomRelatedEntityExpected1 = new Mock<IUniversalCopyCustomRelatedEntity>();
			mockUniversalCopyCustomRelatedEntityExpected1.Setup(entity => entity.SourceTypeName).Returns(type.Name);
			mockUniversalCopyCustomRelatedEntityExpected1.Setup(entity => entity.RelatedPropertyName).Returns("Expected1");
			mockUniversalCopyCustomRelatedEntityExpected1.Setup(entity => entity.InitializeRelatedEntityCopyTemplateTreeNode(
				It.IsAny<EntityCopyTemplateNode>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<Func<Type, CopyTemplateNode>>()));

			var mockUniversalCopyCustomRelatedEntityExpected2 = new Mock<IUniversalCopyCustomRelatedEntity>();
			mockUniversalCopyCustomRelatedEntityExpected2.Setup(entity => entity.SourceTypeName).Returns(type.Name);
			mockUniversalCopyCustomRelatedEntityExpected2.Setup(entity => entity.RelatedPropertyName).Returns("Expected2");
			mockUniversalCopyCustomRelatedEntityExpected2.Setup(entity => entity.InitializeRelatedEntityCopyTemplateTreeNode(
				It.IsAny<EntityCopyTemplateNode>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<Func<Type, CopyTemplateNode>>()));

			var mockUniversalCopyCustomRelatedEntityOther = new Mock<IUniversalCopyCustomRelatedEntity>();
			mockUniversalCopyCustomRelatedEntityOther.Setup(entity => entity.SourceTypeName).Returns("Other");
			mockUniversalCopyCustomRelatedEntityOther.Setup(entity => entity.RelatedPropertyName).Returns("Other");
			mockUniversalCopyCustomRelatedEntityOther.Setup(entity => entity.InitializeRelatedEntityCopyTemplateTreeNode(
				It.IsAny<EntityCopyTemplateNode>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<Func<Type, CopyTemplateNode>>()));

			// Act
			using (ObjectFactory.Substitute("UniversalCopyCustomRelatedEntitiesList", new ListObject()
				{
					mockUniversalCopyCustomRelatedEntityExpected1.Object,
					mockUniversalCopyCustomRelatedEntityExpected2.Object,
					mockUniversalCopyCustomRelatedEntityOther.Object
				}))
			{
				var universalCopyCustomRelatedEntityManager = new UniversalCopyCustomRelatedEntityManager();

				universalCopyCustomRelatedEntityManager.InitializeRelatedEntityCopyTemplateTreeNodes(entityNode, type, processedProperties, initilizeSubEntityNode);
			}

			// Assert
			mockUniversalCopyCustomRelatedEntityExpected1.Verify(entity => entity.InitializeRelatedEntityCopyTemplateTreeNode(entityNode, processedProperties, initilizeSubEntityNode), Times.Once);
			mockUniversalCopyCustomRelatedEntityExpected2.Verify(entity => entity.InitializeRelatedEntityCopyTemplateTreeNode(entityNode, processedProperties, initilizeSubEntityNode), Times.Once);
			mockUniversalCopyCustomRelatedEntityOther.Verify(entity => entity.InitializeRelatedEntityCopyTemplateTreeNode(entityNode, processedProperties, initilizeSubEntityNode), Times.Never);

			Assert("Meaningless assert because analyzer does not recognize moq", true);
		}

		public void TestGetUniversalCopyCustomRelatedEntity_EntityInObjectFactory_ReturnsCorrectEntity()
		{
			// Arrange
			var mockUniversalCopyCustomRelatedEntity = new Mock<IUniversalCopyCustomRelatedEntity>();
			mockUniversalCopyCustomRelatedEntity.Setup(entity => entity.RelatedPropertyName).Returns("TestRelatedPropertyName");
			IUniversalCopyCustomRelatedEntity result;

			// Act
			using (ObjectFactory.Substitute("UniversalCopyCustomRelatedEntitiesList", new ListObject() { mockUniversalCopyCustomRelatedEntity.Object }))
			{
				var universalCopyCustomRelatedEntityManager = new UniversalCopyCustomRelatedEntityManager();

				result = universalCopyCustomRelatedEntityManager.GetUniversalCopyCustomRelatedEntity("TestRelatedPropertyName");
			}

			// Assert
			AssertEquals(mockUniversalCopyCustomRelatedEntity.Object, result);
		}

		public void TestGetUniversalCopyCustomRelatedEntity_EntityNotInObjectFactory_ReturnsNull()
		{
			// Arrange
			var mockUniversalCopyCustomRelatedEntity = new Mock<IUniversalCopyCustomRelatedEntity>();
			mockUniversalCopyCustomRelatedEntity.Setup(entity => entity.RelatedPropertyName).Returns("TestRelatedPropertyName");
			IUniversalCopyCustomRelatedEntity result;

			// Act
			using (ObjectFactory.Substitute("UniversalCopyCustomRelatedEntitiesList", new ListObject() { mockUniversalCopyCustomRelatedEntity.Object }))
			{
				var universalCopyCustomRelatedEntityManager = new UniversalCopyCustomRelatedEntityManager();

				result = universalCopyCustomRelatedEntityManager.GetUniversalCopyCustomRelatedEntity("Invalid");
			}

			// Assert
			AssertEquals(null, result);
		}
	}
}
