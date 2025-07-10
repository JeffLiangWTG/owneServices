using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.UniversalCopy;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Shape;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test.Shape
{
	public class BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntityTest : TestCaseWithFactory
	{
		public void TestSourceTypeName_ReturnsName()
		{
			// Arrange
			var customRelatedEntity = new BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity();

			// Act & Assert
			AssertEquals("IBMNCNShape", customRelatedEntity.SourceTypeName);
		}

		public void TestRelatedPropertyName_ReturnsName()
		{
			// Arrange
			var customRelatedEntity = new BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity();

			// Act & Assert
			AssertEquals("BNS_RelatedEntityID", customRelatedEntity.RelatedPropertyName);
		}

		public void TestInitializeRelatedEntityCopyTemplateTreeNode_InitializesWorkItemAndWorkProject()
		{
			// Arrange
			var customRelatedEntity = new BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity();
			var entityNode = new EntityCopyTemplateNode();
			var processedProperties = new Dictionary<string, object>();

			var workItemNode = new RelatedEntityCopyTemplateNode();
			var workProjectNode = new RelatedEntityCopyTemplateNode();

			var initializeSubEntityNode = (Type type) =>
			{
				switch (type.Name)
				{
					case "IWorkItem":
						return workItemNode;
					case "IWorkProject":
						return workProjectNode;
					default:
						return null;
				}
			};

			AssertEquals("Precondition", 0, entityNode.Nodes.Count);

			// Act
			customRelatedEntity.InitializeRelatedEntityCopyTemplateTreeNode(
				entityNode, processedProperties, initializeSubEntityNode);

			// Assert
			AssertEquals(2, entityNode.Nodes.Count);

			AssertEquals(workItemNode, ((RelatedEntityCopyTemplateNode)entityNode.Nodes[0]).InnerNode);
			AssertEquals(workProjectNode, ((RelatedEntityCopyTemplateNode)entityNode.Nodes[1]).InnerNode);

			AssertEquals(true, processedProperties.ContainsKey("BNS_RelatedEntityID"));
		}

		public void TestInitializeRelatedEntityCopyTemplateTreeNode_DisablesParentShapeRelatedEntity()
		{
			// Arrange
			var customRelatedEntity = new BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity();
			var entityNode = new EntityCopyTemplateNode();
			var processedProperties = new Dictionary<string, object>();

			var workItemNode = new RelatedEntityCopyTemplateNode();
			var workProjectNode = new RelatedEntityCopyTemplateNode();

			var initializeSubEntityNode = (Type type) =>
			{
				switch (type.Name)
				{
					case "IWorkItem":
						return workItemNode;
					case "IWorkProject":
						return workProjectNode;
					default:
						return null;
				}
			};

			AssertEquals("Precondition", 0, entityNode.Nodes.Count);

			// Act
			customRelatedEntity.InitializeRelatedEntityCopyTemplateTreeNode(
				entityNode, processedProperties, initializeSubEntityNode);

			// Assert
			AssertEquals(true, processedProperties.ContainsKey("BNS_BNS_ParentShape"));
		}

		public void TestInitializeRelatedEntityCopyTemplateTreeNode_DisablesRootShapeRelatedEntity()
		{
			// Arrange
			var customRelatedEntity = new BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity();
			var entityNode = new EntityCopyTemplateNode();
			var processedProperties = new Dictionary<string, object>();

			var workItemNode = new RelatedEntityCopyTemplateNode();
			var workProjectNode = new RelatedEntityCopyTemplateNode();

			var initializeSubEntityNode = (Type type) =>
			{
				switch (type.Name)
				{
					case "IWorkItem":
						return workItemNode;
					case "IWorkProject":
						return workProjectNode;
					default:
						return null;
				}
			};

			AssertEquals("Precondition", 0, entityNode.Nodes.Count);

			// Act
			customRelatedEntity.InitializeRelatedEntityCopyTemplateTreeNode(
				entityNode, processedProperties, initializeSubEntityNode);

			// Assert
			AssertEquals(true, processedProperties.ContainsKey("BNS_BNS_RootShape"));
		}

		public void TestGetRelatedEntityObjectFK_GivenRelatedWorkItemJobHeader_ReturnsWorkItemFK()
		{
			// Arrange
			var customRelatedEntity = new BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity();

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			jobHeader.FH_Category = "JOB";
			jobHeader.FH_ParentTableCode = "WKI";
			jobHeader.FH_ParentId = workItem.PK;
			Factory.Save();

			var relatedEntityCopyTemplateNode = new RelatedEntityCopyTemplateNode();
			relatedEntityCopyTemplateNode.Name = "WorkItem";

			// Act
			var result = customRelatedEntity.GetRelatedEntityObjectFK(
				Factory, relatedEntityCopyTemplateNode, jobHeader.PK);

			// Assert
			AssertEquals(workItem.PK, result);
		}

		public void TestGetRelatedEntityObjectFK_GivenRelatedWorkItemWorkFlow_ReturnsNull()
		{
			// Arrange
			var customRelatedEntity = new BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity();

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			jobHeader.FH_Category = "WKF";
			jobHeader.FH_ParentTableCode = "WKI";
			jobHeader.FH_ParentId = workItem.PK;
			Factory.Save();

			var relatedEntityCopyTemplateNode = new RelatedEntityCopyTemplateNode();
			relatedEntityCopyTemplateNode.Name = "WorkItem";

			// Act
			var result = customRelatedEntity.GetRelatedEntityObjectFK(
				Factory, relatedEntityCopyTemplateNode, jobHeader.PK);

			// Assert
			AssertEquals(null, result);
		}

		public void TestGetRelatedEntityObjectFK_GivenRelatedWorkItemJobHeaderAndWorkProjectRelatedEntityCopyTemplateNode_ReturnsNull()
		{
			// Arrange
			var customRelatedEntity = new BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity();

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			jobHeader.FH_Category = "JOB";
			jobHeader.FH_ParentTableCode = "WKI";
			jobHeader.FH_ParentId = workItem.PK;
			Factory.Save();

			var relatedEntityCopyTemplateNode = new RelatedEntityCopyTemplateNode();
			relatedEntityCopyTemplateNode.Name = "WorkProject";

			// Act
			var result = customRelatedEntity.GetRelatedEntityObjectFK(
				Factory, relatedEntityCopyTemplateNode, jobHeader.PK);

			// Assert
			AssertEquals(null, result);
		}

		public void TestGetRelatedEntityObjectFK_GivenRelatedWorkProjectJobHeader_ReturnsWorkProjectFK()
		{
			// Arrange
			var customRelatedEntity = new BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity();

			var workProject = Factory.NewWithValidTestData<Project>();
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			jobHeader.FH_Category = "JOB";
			jobHeader.FH_ParentTableCode = "WKP";
			jobHeader.FH_ParentId = workProject.PK;
			Factory.Save();

			var relatedEntityCopyTemplateNode = new RelatedEntityCopyTemplateNode();
			relatedEntityCopyTemplateNode.Name = "WorkProject";

			// Act
			var result = customRelatedEntity.GetRelatedEntityObjectFK(
				Factory, relatedEntityCopyTemplateNode, jobHeader.PK);

			// Assert
			AssertEquals(workProject.PK, result);
		}

		public void TestGetRelatedEntityObjectFK_GivenRelatedWorkProjectWorkFlow_ReturnsNull()
		{
			// Arrange
			var customRelatedEntity = new BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity();

			var workProject = Factory.NewWithValidTestData<Project>();
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			jobHeader.FH_Category = "WKF";
			jobHeader.FH_ParentTableCode = "WKP";
			jobHeader.FH_ParentId = workProject.PK;
			Factory.Save();

			var relatedEntityCopyTemplateNode = new RelatedEntityCopyTemplateNode();
			relatedEntityCopyTemplateNode.Name = "WorkProject";

			// Act
			var result = customRelatedEntity.GetRelatedEntityObjectFK(
				Factory, relatedEntityCopyTemplateNode, jobHeader.PK);

			// Assert
			AssertEquals(null, result);
		}

		public void TestGetRelatedEntityObjectFK_GivenRelatedWorkProjectJobHeaderAndWorkItemRelatedEntityCopyTemplateNode_ReturnsNull()
		{
			// Arrange
			var customRelatedEntity = new BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity();

			var workProject = Factory.NewWithValidTestData<Project>();
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			jobHeader.FH_Category = "JOB";
			jobHeader.FH_ParentTableCode = "WKP";
			jobHeader.FH_ParentId = workProject.PK;
			Factory.Save();

			var relatedEntityCopyTemplateNode = new RelatedEntityCopyTemplateNode();
			relatedEntityCopyTemplateNode.Name = "WorkItem";

			// Act
			var result = customRelatedEntity.GetRelatedEntityObjectFK(
				Factory, relatedEntityCopyTemplateNode, jobHeader.PK);

			// Assert
			AssertEquals(null, result);
		}

		public void TestGetOriginalRelatedEntityFromRelatedEntityObject_GivenRelatedWorkItem_ReturnsWorkItemJobHeader()
		{
			// Arrange
			var customRelatedEntity = new BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity();

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			Factory.Save();

			var relatedEntityCopyTemplateNode = new RelatedEntityCopyTemplateNode();
			relatedEntityCopyTemplateNode.Name = "WorkItem";
			var getPropertyValue = (object source, string propertyName) =>
			{
				if (source == workItem && propertyName == "JobWorkflow")
				{
					return jobHeader;
				}

				return null;
			};

			// Act
			var result = customRelatedEntity.GetOriginalRelatedEntityFromRelatedEntityObject(
				relatedEntityCopyTemplateNode, workItem, getPropertyValue);

			// Assert
			AssertEquals(jobHeader, result);
		}

		public void TestGetOriginalRelatedEntityFromRelatedEntityObject_GivenRelatedWorkProject_ReturnsWorkProjectJobHeader()
		{
			// Arrange
			var customRelatedEntity = new BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity();

			var workProject = Factory.NewWithValidTestData<Project>();
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			Factory.Save();

			var relatedEntityCopyTemplateNode = new RelatedEntityCopyTemplateNode();
			relatedEntityCopyTemplateNode.Name = "WorkProject";
			var getPropertyValue = (object source, string propertyName) =>
			{
				if (source == workProject && propertyName == "JobWorkflow")
				{
					return jobHeader;
				}

				return null;
			};

			// Act
			var result = customRelatedEntity.GetOriginalRelatedEntityFromRelatedEntityObject(
				relatedEntityCopyTemplateNode, workProject, getPropertyValue);

			// Assert
			AssertEquals(jobHeader, result);
		}

		public void TestGetOriginalRelatedEntityFromRelatedEntityObject_GivenUnexpectedRelatedEntity_ReturnsRelatedEntity()
		{
			// Arrange
			var customRelatedEntity = new BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity();

			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			Factory.Save();

			var relatedEntityCopyTemplateNode = new RelatedEntityCopyTemplateNode();
			relatedEntityCopyTemplateNode.Name = "Dummy";
			var getPropertyValue = (object source, string propertyName) =>
			{
				if (source == dummy && propertyName == "JobWorkflow")
				{
					return jobHeader;
				}

				return null;
			};

			// Act
			var result = customRelatedEntity.GetOriginalRelatedEntityFromRelatedEntityObject(
				relatedEntityCopyTemplateNode, dummy, getPropertyValue);

			// Assert
			AssertEquals(dummy, result);
		}
	}
}
