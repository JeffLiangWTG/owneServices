using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class AutoDocumentDeliveryJobWithEDocsTest : TestCaseWithFactory
	{
		public void TestDelivery_ExternalStorageExceptionIsHandled()
		{
			// Arrange
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var businessObjectToDeliver = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			businessObjectToDeliver["ConsigneePK"] = org.PK;
			businessObjectToDeliver[JobShipmentSchema.JS_IsForwardRegistered] = false;

			var parentCommand = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Pre-Alert"));
			var childCommand = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Delay Alert"));

			var pivot = parentCommand.ChildMenus.AddNew();
			pivot.SF_SU_Inward = parentCommand.PK;
			pivot.SF_SU_Outward = childCommand.PK;

			Factory.Save();

			var isReportExceptionForDeveloperCalled = false;
			var externalStorageExceptionMock = new Mock<ExternalStorageException>("", "S3", null);
			externalStorageExceptionMock.SetupGet(e => e.UnableToAccessStorageFriendlyMessage).Returns("Failed to access external storage");
			externalStorageExceptionMock.Setup(e => e.ReportExceptionForDeveloper()).Callback(() => isReportExceptionForDeveloperCalled = true);

			var notifications = new NotificationBuffer();
			var job = new AutoDocumentDeliveryJobForTest((IDocumentSupportable)businessObjectToDeliver, parentCommand.PK)
			{
				ActionOnDeliveredBeforeDocumentPrintSetDisposed = () => throw externalStorageExceptionMock.Object
			};

			// Act & Assert
			AssertNoExceptionThrown(() => job.Deliver(notifications));
			AssertEquals("Exception should be reported if required", true, isReportExceptionForDeveloperCalled);
			AssertEquals("There should be errors", true, notifications.HasErrors);
			AssertContains("Failed to access external storage", notifications.AsString.Trim());
		}

		public void TestResetEDocs()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var businessObjectToDeliver = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			businessObjectToDeliver["ConsigneePK"] = org.PK;
			businessObjectToDeliver[JobShipmentSchema.JS_IsForwardRegistered] = false;

			var parentCommand = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Pre-Alert"));
			var childCommand = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Delay Alert"));

			var pivot = parentCommand.ChildMenus.AddNew();
			pivot.SF_SU_Inward = parentCommand.PK;
			pivot.SF_SU_Outward = childCommand.PK;

			Factory.Save();

			var notifications = new NotificationBuffer();
			var job = new AutoDocumentDeliveryJobForTest((IDocumentSupportable)businessObjectToDeliver, parentCommand.PK);
			job.Deliver(notifications);

			AssertEquals("Should reset the eDoc IncludedInPrint to true", true, job.EDocs[0].IncludedInPrint);
			AssertEquals("Should reset the eDoc ShouldPrintByDefault to false", false, job.EDocs[0].ShouldPrintByDefault);
			AssertEquals("Should reset the eDoc IncludedInPrint to true", true, job.EDocs[1].IncludedInPrint);
			AssertEquals("Should reset the eDoc ShouldPrintByDefault to false", false, job.EDocs[1].ShouldPrintByDefault);
		}

		#region Implementation

		[Serializable]
		class AutoDocumentDeliveryJobForTest : AutoDocumentDeliveryJob
		{
			public AutoDocumentDeliveryJobForTest(IDocumentSupportable businessObject, ZGuid documentCommandPK)
				: base(businessObject, false, documentCommandPK)
			{
			}

#if NETFRAMEWORK
			protected AutoDocumentDeliveryJobForTest(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif

			protected override DeliveryInstructions GetDeliveryInstructions(DocumentPack pack)
			{
				var eDoc = Factory.New<DocumentPrintSetTest.DummyBizoStorageDocs>();
				pack.Add(eDoc);
				EDocs.Add(eDoc);
				var deliveryInstractions = base.GetDeliveryInstructions(pack);
				eDoc.IncludedInPrint = true;
				return deliveryInstractions;
			}

			protected override void OnDeliveredBeforeDocumentPrintSetDisposed(DocumentPrintSet documentPrintSet)
			{
				ActionOnDeliveredBeforeDocumentPrintSetDisposed?.Invoke();
			}

			public Action ActionOnDeliveredBeforeDocumentPrintSetDisposed { get; set; }
 
			public List<DocumentPrintSetTest.DummyBizoStorageDocs> EDocs { get; set; } = new List<DocumentPrintSetTest.DummyBizoStorageDocs>();
		}

		#endregion
	}
}
