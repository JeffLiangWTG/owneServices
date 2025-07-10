using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Moq;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	public class SendNativePortMessageOriginalTest : TestCaseWithFactory
	{
		public void TestID()
		{
			AssertEquals(CommandIds.SendMessage, command.Id);
		}

		public void TestIsEnabled()
		{
			Assert(command.IsEnabled);
		}

		public void TestInvoke()
		{
			var nctsHeader = Factory.New<NctsHeader>();

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = "DOA";
			documentData.JDD_ParentID = nctsHeader.PK;
			documentData.JDD_ParentTableCode = nctsHeader.TablePrefix;

			var documentInfo = new Mock<IDocumentInfo>();
			var document = new Mock<IDocument>();
			var dynamicData = new Mock<IDynamicData>();
			var template = new Mock<ITemplate>();
			var notificationService = new Mock<IUserNotificationService>();

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(broker);
			services.Register<IUserNotificationService>(notificationService.Object);

			documentInfo.SetupGet(di => di.Document).Returns(document.Object);
			documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
			documentInfo.SetupGet(di => di.Template).Returns(template.Object);
			documentInfo.SetupGet(di => di.Services).Returns(services);

			document.SetupGet(di => di.Data).Returns(dynamicData.Object);
			document.SetupGet(di => di.Name).Returns("DOA");

			template.SetupGet(di => di.DataContext).Returns(DataContext.FRPortsRegularizationTransitDOA);

			var docDataObject = new DOADataObject
			{
				SendingPartyID = "SNDID",
				SendingPartySICCode = "SNDSIC",
				RecipientID = "RCPID",
				RecipientSICCode = "RCPSIC",
				DeclarationNumber = "123",
				DeclarationType = "T1",
				JobNumber = "NCT001",
				TotalNumberOfPacks = 12,
				TotalGrossWeightInKilograms = 24
			};
			dynamicData.SetupGet(di => di.Value).Returns(docDataObject);

			command.NotifyDocumentInfoCreated(documentInfo.Object);
			Assert(command.Invoke());
			notificationService.Verify(s => s.ShowMessage("Sent successfully.", "Successful"), Times.Once);

			document.SetupGet(di => di.Notifications).Returns(new[]
			{
				new Notification(new Mock<INotificationSource>().Object, NotificationType.Error, "Error")
			});
			command.Invoke();
			notificationService.Verify(s => s.ShowMessage("This document contains errors. Please fix all errors before sending.", "Sending Message"), Times.Once);

			document.SetupGet(di => di.Notifications).Returns(new[]
			{
				new Notification(new Mock<INotificationSource>().Object, NotificationType.MessageError, "Message Error")
			});
			command.Invoke();
			notificationService.Verify(s => s.ShowMessage("This document contains message errors. Please fix all message errors before sending.", "Sending Message"), Times.Once);

			dynamicData.SetupGet(x => x.HasChanges).Returns(true);
			command.Invoke();
			notificationService.Verify(s => s.ShowMessage("Please save all changes before sending.", "Sending Message"), Times.Once);
		}

		public void TestInvokeCAED()
		{
			var nctsHeader = Factory.New<NctsHeader>();

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = MessageSubTypeList.Codes.CAED;
			documentData.JDD_ParentID = nctsHeader.PK;
			documentData.JDD_ParentTableCode = nctsHeader.TablePrefix;
			nctsHeader.DepartureHeaderContainers.AddNew().FillWithValidTestData();

			var documentInfo = new Mock<IDocumentInfo>();
			var document = new Mock<IDocument>();
			var dynamicData = new Mock<IDynamicData>();
			var template = new Mock<ITemplate>();
			var notificationService = new Mock<IUserNotificationService>();

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(broker);
			services.Register<IUserNotificationService>(notificationService.Object);

			documentInfo.SetupGet(di => di.Document).Returns(document.Object);
			documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
			documentInfo.SetupGet(di => di.Template).Returns(template.Object);
			documentInfo.SetupGet(di => di.Services).Returns(services);

			document.SetupGet(di => di.Data).Returns(dynamicData.Object);
			document.SetupGet(di => di.Name).Returns(MessageSubTypeList.Codes.CAED);

			template.SetupGet(di => di.DataContext).Returns(DataContext.FRPortsCustomsCheckCAED);

			var container = nctsHeader.DepartureHeaderContainers.AddNew();

			var list = new List<ZString>();
			list.Add("container");
			list.Add("container2");

			var docDataObject = new CAEDDataObject
			{
				SendingPartyID = "SNDID",
				SendingPartySICCode = "SNDSIC",
				RecipientID = "RCPID",
				RecipientSICCode = "RCPSIC",
				DeclarationType = "T1",
				JobNumber = "NCT001",
				TotalNumberOfPacks = 12,
				Containers = list,
				DeclarantsSIRETNumber = "FR123456789",
				Port = "test",
				PortDuesAmount = 10,
				CommonAccessRef = "ref",
				CTOPartySICCode = "CTO"
			};

			dynamicData.SetupGet(di => di.Value).Returns(docDataObject);

			command.NotifyDocumentInfoCreated(documentInfo.Object);
			Assert(command.Invoke());
			notificationService.Verify(s => s.ShowMessage("Sent successfully.", "Successful"), Times.Once);

			document.SetupGet(di => di.Notifications).Returns(new[]
			{
				new Notification(new Mock<INotificationSource>().Object, NotificationType.Error, "Error")
			});
			command.Invoke();
			notificationService.Verify(s => s.ShowMessage("This document contains errors. Please fix all errors before sending.", "Sending Message"), Times.Once);

			document.SetupGet(di => di.Notifications).Returns(new[]
			{
				new Notification(new Mock<INotificationSource>().Object, NotificationType.MessageError, "Message Error")
			});
			command.Invoke();
			notificationService.Verify(s => s.ShowMessage("This document contains message errors. Please fix all message errors before sending.", "Sending Message"), Times.Once);

			dynamicData.SetupGet(x => x.HasChanges).Returns(true);
			command.Invoke();
			notificationService.Verify(s => s.ShowMessage("Please save all changes before sending.", "Sending Message"), Times.Once);
		}

		public void TestInvokeCAEDFromEntry()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var container1 = declaration.CusContainers.AddNew();
			container1.FillWithValidTestData();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var pivot = Factory.New<Customs.Business.CusContainerEntryHeaderPivot>();
			pivot.CCE_CH_EntryHeader = entryHeader.PK;
			pivot.CCE_CO_Container = container1.PK;

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = MessageSubTypeList.Codes.CAED;
			documentData.JDD_ParentID = entryHeader.PK;
			documentData.JDD_ParentTableCode = entryHeader.TablePrefix;

			var documentInfo = new Mock<IDocumentInfo>();
			var document = new Mock<IDocument>();
			var dynamicData = new Mock<IDynamicData>();
			var template = new Mock<ITemplate>();
			var notificationService = new Mock<IUserNotificationService>();

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(broker);
			services.Register<IUserNotificationService>(notificationService.Object);

			documentInfo.SetupGet(di => di.Document).Returns(document.Object);
			documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
			documentInfo.SetupGet(di => di.Template).Returns(template.Object);
			documentInfo.SetupGet(di => di.Services).Returns(services);

			document.SetupGet(di => di.Data).Returns(dynamicData.Object);
			document.SetupGet(di => di.Name).Returns(MessageSubTypeList.Codes.CAED);

			template.SetupGet(di => di.DataContext).Returns(DataContext.FRPortsCustomsCheckCAED);
			var list = new List<ZString>();
			list.Add("container");
			list.Add("container2");

			var docDataObject = new CAEDDataObject
			{
				SendingPartyID = "SNDID",
				SendingPartySICCode = "SNDSIC",
				RecipientID = "RCPID",
				RecipientSICCode = "RCPSIC",
				DeclarationType = "T1",
				JobNumber = "NCT001",
				TotalNumberOfPacks = 12,
				Containers = list,
				DeclarantsSIRETNumber = "FR123456789",
				Port = "test",
				PortDuesAmount = 10,
				CommonAccessRef = "ref",
				CTOPartySICCode = "CTO"
			};

			dynamicData.SetupGet(di => di.Value).Returns(docDataObject);

			command.NotifyDocumentInfoCreated(documentInfo.Object);
			Assert(command.Invoke());
			notificationService.Verify(s => s.ShowMessage("Sent successfully.", "Successful"), Times.Once);

			document.SetupGet(di => di.Notifications).Returns(new[]
			{
				new Notification(new Mock<INotificationSource>().Object, NotificationType.Error, "Error")
			});
			command.Invoke();
			notificationService.Verify(s => s.ShowMessage("This document contains errors. Please fix all errors before sending.", "Sending Message"), Times.Once);

			document.SetupGet(di => di.Notifications).Returns(new[]
			{
				new Notification(new Mock<INotificationSource>().Object, NotificationType.MessageError, "Message Error")
			});
			command.Invoke();
			notificationService.Verify(s => s.ShowMessage("This document contains message errors. Please fix all message errors before sending.", "Sending Message"), Times.Once);

			dynamicData.SetupGet(x => x.HasChanges).Returns(true);
			command.Invoke();
			notificationService.Verify(s => s.ShowMessage("Please save all changes before sending.", "Sending Message"), Times.Once);
		}

		protected override void SetUp()
		{
			base.SetUp();
			command = new SendNativePortMessageOriginal();
		}
		SendNativePortMessageOriginal command;
	}
}
