using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNAttachmentCollection))]
	class BMNCNAttachmentCollectionTest : ActiveBusinessObjectCollectionTestCase<BMNCNAttachmentCollection>
	{
		public void TestAdd_WhenParentShapeSpecified_ShouldSetOwner()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory));

			var collectionWithoutParent = new BMNCNAttachmentCollection(Factory);
			var collectionWithParent = new BMNCNAttachmentCollection(diagramShape, BMNCNAttachmentSchema.BNA_BNS_Owner);

			var attachmentWithoutParent = collectionWithoutParent.AddNew();
			AssertEquals(ZGuid.Empty, attachmentWithoutParent.BNA_BNS_Owner);

			var attachmentWithParent = collectionWithParent.AddNew();
			AssertEquals(diagramShape.PK, attachmentWithParent.BNA_BNS_Owner);
		}
	}
}
