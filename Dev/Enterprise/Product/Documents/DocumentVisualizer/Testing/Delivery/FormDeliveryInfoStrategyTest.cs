using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class FormDeliveryInfoStrategyTest : TestCaseWithFactory
	{
		#region TestCreateDeliveryInfo_Preview

		public void TestCreateDeliveryInfo_Preview()
		{
			foreach (var deliveryMethod in DeliveryMethods)
			{
				foreach (var attachmentType in AttachmentTypes)
				{
					AssertCreateDeliveryInfo_Preview(deliveryMethod, attachmentType);
				}
			}
		}

		void AssertCreateDeliveryInfo_Preview(PrintCopyType deliveryMethod, string attachmentType)
		{
			var logParent = Factory.New<DummyWithLogs>();
			var eDocsParent = Factory.New<Forwarding.IForwardingShipment>();

			Factory.Save();

			var deliverable = new Mock<IDocumentDeliverable>();

			deliverable
				.Setup(d => d.GetDeliveryInfo(It.IsAny<bool>(), It.IsAny<FileType>()))
				.Returns(new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document));

			deliverable
				.SetupGet(d => d.EDocsParent)
				.Returns((IDocManagerSupport)eDocsParent);

			var deliveryInstructions = new DeliveryInstructions
			{
				Destination = DeliveryInstructionDestination.Preview
			};

			var contact = deliveryInstructions
				.Recipients
				.AddNew();

			contact.DeliveryMethod = deliveryMethod.ToString();
			contact.AttachmentType = attachmentType;
			contact.Email = "unit.test@cargowise.com";

			var strategy = new FormDeliveryInfoStrategy();
			var deliveryInfo = strategy.CreateDeliveryInfo(deliverable.Object, contact, deliveryInstructions);

			AssertNotNull("delivery info has been created", deliveryInfo);
			AssertEquals("delivery group has not been set", ZGuid.Empty, deliveryInfo.DeliveryGroupID);

			AssertEquals("delivery group ParentGuid has been not been set", ZGuid.Empty, deliveryInfo.ParentGuid);
			Assert("delivery group ParentTableName has not been set", string.IsNullOrEmpty(deliveryInfo.ParentTableName));
			Assert("delivery group RelatedBusinessContext has not been set", string.IsNullOrEmpty(deliveryInfo.RelatedBusinessContext));

			deliverable.Verify(d => d.GetDeliveryInfo(false, FileType.XLS),
				Times.Once,
				"Preview looks accurate with file type set to XLS");
		}

		#endregion

		#region TestCreateDeliveryInfo_Email / TestCreateDeliveryInfo_Fax

		public void TestCreateDeliveryInfo_Email()
		{
			foreach (var attachmentType in AttachmentTypes)
			{
				AssertCreateDeliveryInfoWithDocEngineEDocsSupport(PrintCopyType.EML, attachmentType);
			}
		}

		public void TestCreateDeliveryInfo_Fax()
		{
			foreach (var attachmentType in AttachmentTypes)
			{
				AssertCreateDeliveryInfoWithDocEngineEDocsSupport(PrintCopyType.FAX, attachmentType);
			}
		}

		void AssertCreateDeliveryInfoWithDocEngineEDocsSupport(PrintCopyType deliveryMethod, string attachmentType)
		{
			var logParent = Factory.New<DummyWithLogs>();
			var eDocsParent = Factory.New<Forwarding.IForwardingShipment>();

			Factory.Save();

			var deliverable = new Mock<IDocumentDeliverable>();

			deliverable
				.Setup(d => d.GetDeliveryInfo(It.IsAny<bool>(), It.IsAny<FileType>()))
				.Returns(new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document));

			deliverable
				.SetupGet(d => d.EDocsParent)
				.Returns((IDocManagerSupport)eDocsParent);

			var deliveryInstructions = new DeliveryInstructions
			{
				Destination = DeliveryInstructionDestination.TakenFromContact
			};

			var contact = deliveryInstructions
				.Recipients
				.AddNew();

			contact.DeliveryMethod = deliveryMethod.ToString();
			contact.AttachmentType = attachmentType;
			contact.Email = "unit.test@cargowise.com";

			var strategy = new FormDeliveryInfoStrategy();
			var deliveryInfo = strategy.CreateDeliveryInfo(deliverable.Object, contact, deliveryInstructions);

			AssertNotNull("delivery info has been created", deliveryInfo);
			AssertNotEquals("delivery group has been set", ZGuid.Empty, deliveryInfo.DeliveryGroupID);

			// Email and Fax deliveries result in combined DocPack, which then gets delivered to eDocs so at this stage we cannot replace nor create separate StmPrintJob for eDocs
			AssertEquals("delivery group ParentGuid has been set", eDocsParent.PK, deliveryInfo.ParentGuid);
			AssertEquals("delivery group ParentTableName has been set", JobShipmentSchema.Constants.TableName, deliveryInfo.ParentTableName);
			AssertEquals("delivery group RelatedBusinessContext has been set", "SHP", deliveryInfo.RelatedBusinessContext);

			deliverable.Verify(d => d.GetDeliveryInfo(false, FileType.PDF),
				Times.Once,
				"since the documents are combined then we're using the worst case scenario i.e. PDF which is the only format that needs to scale down the text in cells with a lot of text");
		}

		public void TestDoNotCreateAnotherDeliveryInfo()
		{
			var logParent = Factory.New<DummyWithLogs>();
			var eDocsParent = Factory.New<Forwarding.IForwardingShipment>();

			Factory.Save();

			var deliverable = new Mock<IDocumentDeliverable>();

			deliverable
				.Setup(d => d.GetDeliveryInfo(It.IsAny<bool>(), It.IsAny<FileType>()))
				.Returns(new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document));

			deliverable
				.SetupGet(d => d.EDocsParent)
				.Returns((IDocManagerSupport)eDocsParent);

			var deliveryInstructions = new DeliveryInstructions
			{
				Destination = DeliveryInstructionDestination.TakenFromContact
			};

			AssertEquals("prereq: delivery instructions have delivery group", 1, deliveryInstructions.DeliveryGroups.Count);

			var existingDeliveryGroup = deliveryInstructions.DeliveryGroups[0];
			existingDeliveryGroup.SB_EmailSubjectLine = "test email subject";

			var contact = deliveryInstructions
				.Recipients
				.AddNew();

			contact.DeliveryMethod = nameof(PrintCopyType.EML);
			contact.AttachmentType = OrgConstants.AttachmentType.PDF;
			contact.Email = "unit.test@cargowise.com";

			var strategy = new FormDeliveryInfoStrategy();
			var deliveryInfo = strategy.CreateDeliveryInfo(deliverable.Object, contact, deliveryInstructions);

			AssertNotNull("delivery info has been created", deliveryInfo);
			AssertEquals("no new delivery group has been created", 1, deliveryInstructions.DeliveryGroups.Count);
		}

		#endregion

		#region TestCreateDeliveryInfo_Print

		public void TestCreateDeliveryInfo_Print_PrintPDFDeliveryIsOn() => AssertCreateDeliveryInfo_Print(true);

		public void TestCreateDeliveryInfo_Print_PrintPDFDeliveryIsOff() => AssertCreateDeliveryInfo_Print(false);

		void AssertCreateDeliveryInfo_Print(bool deliverDocumentsToPrintersInPdfFormat)
		{
			using (DocumentsDataRegistry.Instance.DeliverDocumentsToPrintersInPdfFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, deliverDocumentsToPrintersInPdfFormat))
			{
				var logParent = Factory.New<DummyWithLogs>();
				var eDocsParent = Factory.New<Forwarding.IForwardingShipment>();

				Factory.Save();

				var deliverable = new Mock<IDocumentDeliverable>();

				deliverable
					.Setup(d => d.GetDeliveryInfo(It.IsAny<bool>(), It.IsAny<FileType>()))
					.Returns(new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document));

				deliverable
					.SetupGet(d => d.EDocsParent)
					.Returns((IDocManagerSupport)eDocsParent);

				var deliveryInstructions = new DeliveryInstructions
				{
					Destination = DeliveryInstructionDestination.TakenFromContact
				};

				var contact = deliveryInstructions
					.Recipients
					.AddNew();

				contact.DeliveryMethod = nameof(PrintCopyType.PRN);
				contact.AttachmentType = ZString.Empty;
				contact.Email = "unit.test@cargowise.com";

				var strategy = new FormDeliveryInfoStrategy();
				var deliveryInfo = strategy.CreateDeliveryInfo(deliverable.Object, contact, deliveryInstructions);

				AssertNotNull("delivery info has been created", deliveryInfo);
				AssertNotEquals("delivery group has been set", ZGuid.Empty, deliveryInfo.DeliveryGroupID);

				AssertEquals("delivery group ParentGuid has been set", eDocsParent.PK, deliveryInfo.ParentGuid);
				AssertEquals("delivery group ParentTableName has been set", JobShipmentSchema.Constants.TableName, deliveryInfo.ParentTableName);
				AssertEquals("delivery group RelatedBusinessContext has been set", "SHP", deliveryInfo.RelatedBusinessContext);

				deliverable.Verify(d => d.GetDeliveryInfo(false, FileType.PDF),
					Times.Once,
					"we always use the worst case scenario i.e. we use PDF to make sure the document is scaled");
			}
		}

		#endregion

		#region TestCreateDeliveryInfo_HandleNull

		public void TestCreateDeliveryInfo_HandleNull()
		{
			var logParent = Factory.New<DummyWithLogs>();
			var eDocsParent = Factory.New<Forwarding.IForwardingShipment>();

			Factory.Save();

			var deliverable = new Mock<IDocumentDeliverable>();

			deliverable
				.Setup(d => d.GetDeliveryInfo(It.IsAny<bool>(), It.IsAny<FileType>()))
				.Returns((DeliveryInfo)null);

			var deliveryInstructions = new DeliveryInstructions
			{
				Destination = DeliveryInstructionDestination.Preview
			};

			var contact = deliveryInstructions
				.Recipients
				.AddNew();

			contact.DeliveryMethod = nameof(PrintCopyType.PRN);
			contact.AttachmentType = ZString.Empty;
			contact.Email = "unit.test@cargowise.com";

			var strategy = new FormDeliveryInfoStrategy();
			AssertNoExceptionThrown(() => strategy.CreateDeliveryInfo(deliverable.Object, contact, deliveryInstructions));
		}

		#endregion

		#region TestAccessibleViaObjectFactory

		public void TestAccessibleViaObjectFactory()
		{
			Assert("FormDeliveryInfoStrategy has been registered in ObjectFactory", ObjectFactory.Get<IFormDeliveryInfoStrategy>() is FormDeliveryInfoStrategy);
		}

		#endregion

		#region Implementation

		IEnumerable<PrintCopyType> DeliveryMethods
		{
			get
			{
				yield return PrintCopyType.PRN;
				yield return PrintCopyType.EML;
				yield return PrintCopyType.FAX;
				yield return PrintCopyType.ALL;
			}
		}

		IEnumerable<string> AttachmentTypes
		{
			get
			{
				yield return OrgConstants.AttachmentType.FIL;
				yield return OrgConstants.AttachmentType.PDF;
				yield return OrgConstants.AttachmentType.PDFA;
				yield return OrgConstants.AttachmentType.PDFC;
				yield return OrgConstants.AttachmentType.TIF;
				yield return OrgConstants.AttachmentType.XLS;
				yield return OrgConstants.AttachmentType.XLSX;
				yield return OrgConstants.AttachmentType.HTML;
				yield return OrgConstants.AttachmentType.HTMF;
			}
		}

		#endregion
	}
}
