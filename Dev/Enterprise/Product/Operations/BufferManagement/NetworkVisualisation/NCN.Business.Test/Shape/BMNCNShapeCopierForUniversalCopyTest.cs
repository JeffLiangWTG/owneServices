using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Shape;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test.Shape
{
	public class BMNCNShapeCopierForUniversalCopyTest : TestCaseWithFactory
	{
		public void TestCopyBMNCNShapeInfoForUniversalCopy_CopiesAttachementsBetweenCopiedShapes()
		{
			// Arrange
			var copier = new BMNCNShapeCopierForUniversalCopy();

			var fromShapeSource = Factory.NewWithValidTestData<BMNCNShape>();
			var toShapeSource = Factory.NewWithValidTestData<BMNCNShape>();

			var attachmentSource = Factory.NewWithValidTestData<BMNCNAttachment>();
			attachmentSource.BNA_BNS_FromShape = fromShapeSource.PK;
			attachmentSource.BNA_BNS_ToShape = toShapeSource.PK;
			attachmentSource.BNA_BNS_Owner = fromShapeSource.PK;

			var fromShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();
			var toShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();

			var copiedEntities = new Dictionary<object, object>()
			{
				{ fromShapeSource, fromShapeCopied },
				{ toShapeSource, toShapeCopied },
			};

			// Act
			copier.FinishCopyAction(copiedEntities);

			// Assert
			AssertEquals(3, copiedEntities.Count);
			var copiedAttachement = (BMNCNAttachment)copiedEntities.Values.ToArray()[2];

			AssertEquals(fromShapeCopied.PK, copiedAttachement.BNA_BNS_FromShape);
			AssertEquals(toShapeCopied.PK, copiedAttachement.BNA_BNS_ToShape);
			AssertEquals(fromShapeCopied.PK, copiedAttachement.BNA_BNS_Owner);
		}

		public void TestCopyBMNCNShapeInfoForUniversalCopy_DoesNotCopyAttachementsBetweenCopiedShapesAndExternalShapes()
		{
			// Arrange
			var copier = new BMNCNShapeCopierForUniversalCopy();

			var fromShapeSource = Factory.NewWithValidTestData<BMNCNShape>();
			var toShapeExternal = Factory.NewWithValidTestData<BMNCNShape>();

			var attachmentSource = Factory.NewWithValidTestData<BMNCNAttachment>();
			attachmentSource.BNA_BNS_FromShape = fromShapeSource.PK;
			attachmentSource.BNA_BNS_ToShape = toShapeExternal.PK;
			attachmentSource.BNA_BNS_Owner = fromShapeSource.PK;

			var fromShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();

			var copiedEntities = new Dictionary<object, object>()
			{
				{ fromShapeSource, fromShapeCopied }
			};

			// Act
			copier.FinishCopyAction(copiedEntities);

			// Assert
			AssertEquals(1, copiedEntities.Count);
		}

		public void TestCopyBMNCNShapeInfoForUniversalCopy_CopiesAttachementDependencyLinksForCopiedProcessHeaders()
		{
			// Arrange
			var copier = new BMNCNShapeCopierForUniversalCopy();

			var fromJobSource = Factory.NewWithValidTestData<ProcessHeader>();
			var toJobSource = Factory.NewWithValidTestData<ProcessHeader>();

			var linkSource = Factory.NewWithValidTestData<ProcessHeaderLink>();
			linkSource.FP_FH_HeaderFrom = fromJobSource.PK;
			linkSource.FP_FH_HeaderTo = toJobSource.PK;

			var fromShapeSource = Factory.NewWithValidTestData<BMNCNShape>();
			fromShapeSource.BNS_RelatedEntityTableCode = "FH";
			fromShapeSource.BNS_RelatedEntityID = fromJobSource.PK;
			var toShapeSource = Factory.NewWithValidTestData<BMNCNShape>();
			toShapeSource.BNS_RelatedEntityTableCode = "FH";
			toShapeSource.BNS_RelatedEntityID = toJobSource.PK;

			var attachmentSource = Factory.NewWithValidTestData<BMNCNAttachment>();
			attachmentSource.BNA_BNS_FromShape = fromShapeSource.PK;
			attachmentSource.BNA_BNS_ToShape = toShapeSource.PK;
			attachmentSource.BNA_BNS_Owner = fromShapeSource.PK;
			attachmentSource.BNA_FP_ProcessHeaderLink = linkSource.PK;

			var fromJobCopied = Factory.NewWithValidTestData<ProcessHeader>();
			var toJobCopied = Factory.NewWithValidTestData<ProcessHeader>();

			var fromShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();
			fromShapeCopied.BNS_RelatedEntityTableCode = "FH";
			fromShapeCopied.BNS_RelatedEntityID = fromJobCopied.PK;
			var toShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();
			toShapeCopied.BNS_RelatedEntityTableCode = "FH";
			toShapeCopied.BNS_RelatedEntityID = toJobCopied.PK;

			var copiedEntities = new Dictionary<object, object>()
			{
				{ fromJobSource, fromJobCopied },
				{ toJobSource, toJobCopied },
				{ fromShapeSource, fromShapeCopied },
				{ toShapeSource, toShapeCopied },
			};

			// Act
			copier.FinishCopyAction(copiedEntities);

			// Assert
			AssertEquals(6, copiedEntities.Count);
			var copiedDependencyLink = (ProcessHeaderLink)copiedEntities.Values.ToArray()[4];
			var copiedAttachement = (BMNCNAttachment)copiedEntities.Values.ToArray()[5];

			AssertEquals(copiedDependencyLink.PK, copiedAttachement.BNA_FP_ProcessHeaderLink);
			AssertEquals(fromJobCopied.PK, copiedDependencyLink.FP_FH_HeaderFrom);
			AssertEquals(toJobCopied.PK, copiedDependencyLink.FP_FH_HeaderTo);
		}

		public void TestCopyBMNCNShapeInfoForUniversalCopy_DoesNotCopyAttachementDependencyLinksForExternalProcessHeaders()
		{
			// Arrange
			var copier = new BMNCNShapeCopierForUniversalCopy();

			var fromJobSource = Factory.NewWithValidTestData<ProcessHeader>();
			var toJobExternal = Factory.NewWithValidTestData<ProcessHeader>();

			var linkSource = Factory.NewWithValidTestData<ProcessHeaderLink>();
			linkSource.FP_FH_HeaderFrom = fromJobSource.PK;
			linkSource.FP_FH_HeaderTo = toJobExternal.PK;

			var fromShapeSource = Factory.NewWithValidTestData<BMNCNShape>();
			fromShapeSource.BNS_RelatedEntityTableCode = "FH";
			fromShapeSource.BNS_RelatedEntityID = fromJobSource.PK;
			var toShapeSource = Factory.NewWithValidTestData<BMNCNShape>();
			toShapeSource.BNS_RelatedEntityTableCode = "FH";
			toShapeSource.BNS_RelatedEntityID = toJobExternal.PK;

			var attachmentSource = Factory.NewWithValidTestData<BMNCNAttachment>();
			attachmentSource.BNA_BNS_FromShape = fromShapeSource.PK;
			attachmentSource.BNA_BNS_ToShape = toShapeSource.PK;
			attachmentSource.BNA_BNS_Owner = fromShapeSource.PK;
			attachmentSource.BNA_FP_ProcessHeaderLink = linkSource.PK;

			var fromJobCopied = Factory.NewWithValidTestData<ProcessHeader>();

			var fromShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();
			fromShapeCopied.BNS_RelatedEntityTableCode = "FH";
			fromShapeCopied.BNS_RelatedEntityID = fromJobCopied.PK;
			var toShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();

			var copiedEntities = new Dictionary<object, object>()
			{
				{ fromJobSource, fromJobCopied },
				{ fromShapeSource, fromShapeCopied },
				{ toShapeSource, toShapeCopied },
			};

			// Act
			copier.FinishCopyAction(copiedEntities);

			// Assert
			AssertEquals("Assert only the shape attachement was copied", 4, copiedEntities.Count);
		}

		public void TestCopyBMNCNShapeInfoForUniversalCopy_DoesNotCopyAttachementDependencyLinksIfTheyAlreadyExist()
		{
			// Arrange
			var copier = new BMNCNShapeCopierForUniversalCopy();

			var fromJobSource = Factory.NewWithValidTestData<ProcessHeader>();
			var toJobSource = Factory.NewWithValidTestData<ProcessHeader>();

			var linkSource = Factory.NewWithValidTestData<ProcessHeaderLink>();
			linkSource.FP_FH_HeaderFrom = fromJobSource.PK;
			linkSource.FP_FH_HeaderTo = toJobSource.PK;

			var fromShapeSource = Factory.NewWithValidTestData<BMNCNShape>();
			fromShapeSource.BNS_RelatedEntityTableCode = "FH";
			fromShapeSource.BNS_RelatedEntityID = fromJobSource.PK;
			var toShapeSource = Factory.NewWithValidTestData<BMNCNShape>();
			toShapeSource.BNS_RelatedEntityTableCode = "FH";
			toShapeSource.BNS_RelatedEntityID = toJobSource.PK;

			var attachmentSource = Factory.NewWithValidTestData<BMNCNAttachment>();
			attachmentSource.BNA_BNS_FromShape = fromShapeSource.PK;
			attachmentSource.BNA_BNS_ToShape = toShapeSource.PK;
			attachmentSource.BNA_BNS_Owner = fromShapeSource.PK;
			attachmentSource.BNA_FP_ProcessHeaderLink = linkSource.PK;

			var fromJobCopied = Factory.NewWithValidTestData<ProcessHeader>();
			var toJobCopied = Factory.NewWithValidTestData<ProcessHeader>();

			var fromShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();
			fromShapeCopied.BNS_RelatedEntityTableCode = "FH";
			fromShapeCopied.BNS_RelatedEntityID = fromJobCopied.PK;
			var toShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();
			toShapeCopied.BNS_RelatedEntityTableCode = "FH";
			toShapeCopied.BNS_RelatedEntityID = toJobCopied.PK;

			var linkCopied = Factory.NewWithValidTestData<ProcessHeaderLink>();
			linkCopied.FP_FH_HeaderFrom = fromJobCopied.PK;
			linkCopied.FP_FH_HeaderTo = toJobCopied.PK;

			var copiedEntities = new Dictionary<object, object>()
			{
				{ fromJobSource, fromJobCopied },
				{ toJobSource, toJobCopied },
				{ fromShapeSource, fromShapeCopied },
				{ toShapeSource, toShapeCopied },
			};

			// Act
			copier.FinishCopyAction(copiedEntities);

			// Assert
			AssertEquals("Assert only the shape attachement was copied", 5, copiedEntities.Count);
		}

		public void TestCopyBMNCNShapeInfoForUniversalCopy_SetsCopiedShapesParents()
		{
			// Arrange
			var copier = new BMNCNShapeCopierForUniversalCopy();

			var parentShapeSource = Factory.NewWithValidTestData<BMNCNShape>();
			var childShapeSource = Factory.NewWithValidTestData<BMNCNShape>();
			childShapeSource.BNS_BNS_ParentShape = parentShapeSource.PK;

			var parentShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();
			var childShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();

			var copiedEntities = new Dictionary<object, object>()
			{
				{ parentShapeSource, parentShapeCopied },
				{ childShapeSource, childShapeCopied },
			};

			// Act
			copier.FinishCopyAction(copiedEntities);

			// Assert
			AssertEquals(parentShapeCopied.PK, childShapeCopied.BNS_BNS_ParentShape);
		}

		public void TestCopyBMNCNShapeInfoForUniversalCopy_CopiesShapeLayoutData()
		{
			// Arrange
			var copier = new BMNCNShapeCopierForUniversalCopy();

			var shapeSource = Factory.NewWithValidTestData<BMNCNShape>();

			shapeSource.Left = 1.5;
			shapeSource.Top = 2.5;
			shapeSource.ScrollPosition = "Scroll";
			shapeSource.BackColor = "Black";
			shapeSource.ForeColor = "White";
			shapeSource.CornerRadius = 4;
			shapeSource.Width = 5.5;
			shapeSource.Height = 6.5;
			shapeSource.ZIndex = 0.5;

			var shapeCopied = Factory.NewWithValidTestData<BMNCNShape>();

			var copiedEntities = new Dictionary<object, object>()
			{
				{ shapeSource, shapeCopied }
			};

			// Act
			copier.FinishCopyAction(copiedEntities);

			// Assert
			AssertEquals((ZDecimal)1.5, shapeCopied.Left);
			AssertEquals((ZDecimal)2.5, shapeCopied.Top);
			AssertEquals("Scroll", shapeCopied.ScrollPosition);
			AssertEquals("Black", shapeCopied.BackColor);
			AssertEquals("White", shapeCopied.ForeColor);
			AssertEquals(4, shapeCopied.CornerRadius);
			AssertEquals((ZDecimal)5.5, shapeCopied.Width);
			AssertEquals((ZDecimal)6.5, shapeCopied.Height);
			AssertEquals((ZDecimal)0.5, shapeCopied.ZIndex);
		}

		public void TestCopyBMNCNShapeInfoForUniversalCopy_LinksChildShapesToWorkflowsOfParentShapesLinkedJob()
		{
			// Arrange
			var copier = new BMNCNShapeCopierForUniversalCopy();

			var jobSource = Factory.NewWithValidTestData<ProcessHeader>();
			var workflowSource = Factory.NewWithValidTestData<ProcessHeader>();

			var parentShapeSource = Factory.NewWithValidTestData<BMNCNShape>();
			parentShapeSource.BNS_RelatedEntityTableCode = "FH";
			parentShapeSource.BNS_RelatedEntityID = jobSource.PK;
			var childShapeSource = Factory.NewWithValidTestData<BMNCNShape>();
			childShapeSource.BNS_RelatedEntityTableCode = "FH";
			childShapeSource.BNS_RelatedEntityID = workflowSource.PK;
			childShapeSource.BNS_BNS_ParentShape = parentShapeSource.PK;

			var jobCopied = Factory.NewWithValidTestData<ProcessHeader>();
			var workflowCopied = Factory.NewWithValidTestData<ProcessHeader>();

			var parentShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();
			parentShapeCopied.BNS_RelatedEntityTableCode = "FH";
			parentShapeCopied.BNS_RelatedEntityID = jobCopied.PK;
			var childShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();
			childShapeCopied.BNS_RelatedEntityTableCode = "FH";
			childShapeCopied.BNS_RelatedEntityID = workflowCopied.PK;

			var copiedEntities = new Dictionary<object, object>()
			{
				{ jobSource, jobCopied },
				{ workflowSource, workflowCopied },
				{ parentShapeSource, parentShapeCopied },
				{ childShapeSource, childShapeCopied },
			};

			// Act
			copier.FinishCopyAction(copiedEntities);

			// Assert
			AssertEquals(workflowCopied.PK, childShapeCopied.BNS_RelatedEntityID);
		}

		public void TestCopyBMNCNShapeInfoForUniversalCopy_UnsetsRelatedEntityTableCodeOfRelatedObjectsThatCouldNotBeCopied()
		{
			// Arrange
			var copier = new BMNCNShapeCopierForUniversalCopy();

			var jobSource = Factory.NewWithValidTestData<ProcessHeader>();
			var workflowSource = Factory.NewWithValidTestData<ProcessHeader>();

			var parentShapeSource = Factory.NewWithValidTestData<BMNCNShape>();
			parentShapeSource.BNS_RelatedEntityTableCode = "FH";
			parentShapeSource.BNS_RelatedEntityID = jobSource.PK;
			var childShapeSource = Factory.NewWithValidTestData<BMNCNShape>();
			childShapeSource.BNS_RelatedEntityTableCode = "FH";
			childShapeSource.BNS_RelatedEntityID = workflowSource.PK;
			childShapeSource.BNS_BNS_ParentShape = parentShapeSource.PK;

			var jobCopied = Factory.NewWithValidTestData<ProcessHeader>();

			var parentShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();
			parentShapeCopied.BNS_RelatedEntityTableCode = "FH";
			parentShapeCopied.BNS_RelatedEntityID = jobCopied.PK;
			var childShapeCopied = Factory.NewWithValidTestData<BMNCNShape>();
			childShapeCopied.BNS_RelatedEntityTableCode = "FH";

			var copiedEntities = new Dictionary<object, object>()
			{
				{ jobSource, jobCopied },
				{ parentShapeSource, parentShapeCopied },
				{ childShapeSource, childShapeCopied },
			};

			// Act
			copier.FinishCopyAction(copiedEntities);

			// Assert
			AssertEquals(ZString.Empty, childShapeCopied.BNS_RelatedEntityTableCode);
		}
	}
}
