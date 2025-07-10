using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.CLE.Testing
{
	public class ContainerDatesFlatFileDataRowImporterTest : BaseFreightTest
	{
		public void TestImport()
		{
			Container.Factory.Save();
			NotificationBuffer notify = new NotificationBuffer();
			ContainerDatesFlatFileDataRow row = new ContainerDatesFlatFileDataRow("09-03-2007,,CONTAINER1,,10-03-2007,,,,CLE");
			Importer.Import(row, notify, Exceptions);
			AssertEquals("ArrivalCartageComplete", new ZDateTime(2007, 3, 9), Container.JC_ArrivalCartageComplete);
			AssertEquals("EmptyReturnedOn", new ZDateTime(2007, 3, 10), Container.JC_ContainerYardEmptyReturnGateIn);
			row = new ContainerDatesFlatFileDataRow("YARD,,CONTAINER1,,10-03-2007,,,,CLE");
			Importer.Import(row, notify, Exceptions);
			AssertEquals("FCLHeldInTransitStaging", true, Container.JC_FCLHeldInTransitStaging);
		}

		public void TestNonImportContainerIgnored()
		{
			CommonConsol consol = GetExportConsol(typeof(CommonConsol));
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_ContainerNum = "CONTAINER";
			container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			Factory.Save();
			Assert("ArrivalCartageComplete", container.JC_ArrivalCartageComplete.IsEmpty);
			Assert("EmptyReturnedOn", container.JC_ContainerYardEmptyReturnGateIn.IsEmpty);
			Assert("Export container:", container.IsExport());
			NotificationBuffer notify = new NotificationBuffer();
			ContainerDatesFlatFileDataRow row = new ContainerDatesFlatFileDataRow("09-03-2007,,CONTAINER,,10-03-2007,,,,CLE");
			Importer.Import(row, notify, Exceptions);
			Assert("ArrivalCartageComplete still empty:", container.JC_ArrivalCartageComplete.IsEmpty);
			Assert("EmptyReturnedOn still empty:", container.JC_ContainerYardEmptyReturnGateIn.IsEmpty);
			notify.Clear();
			Exceptions.CreateExceptionReport(notify);
			Assert("Notify:", notify.AsString.Contains(ContainerDatesExceptionBuffer.NoExceptionFound));
		}

		public void TestImportForContainerLinkedToDeclaration()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			jobDec.JE_HouseBill = "HouseBill";
			BaseCusContainer cusContainer = jobDec.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "CONTAINER";
			Factory.Save();
			NotificationBuffer notify = new NotificationBuffer();
			ContainerDatesFlatFileDataRow row = new ContainerDatesFlatFileDataRow("09-03-2007,,CONTAINER,,10-03-2007,,,,CLE");
			Importer.Import(row, notify, Exceptions);
			CommonContainer jobContainer = Factory.Load<CommonContainer>(cusContainer.CO_JC);
			AssertEquals("ArrivalCartageComplete should have been updated:", new ZDateTime(2007, 3, 9), jobContainer.JC_ArrivalCartageComplete);
			AssertEquals("EmptyReturnedOn:", new ZDateTime(2007, 3, 10), jobContainer.JC_ContainerYardEmptyReturnGateIn);
			jobDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			Factory.Save();
			row = new ContainerDatesFlatFileDataRow("15-03-2007,,CONTAINER,,17-03-2007,,,,CLE");
			Importer.Import(row, notify, Exceptions);
			AssertEquals("ArrivalCartageComplete should be the same:", new ZDateTime(2007, 3, 9), jobContainer.JC_ArrivalCartageComplete);
			AssertEquals("EmptyReturnedOn should be the same", new ZDateTime(2007, 3, 10), jobContainer.JC_ContainerYardEmptyReturnGateIn);
		}

		public void TestHeldInTransitUpdatedWhenYARD()
		{
			Container.Factory.Save();
			AssertEquals("Held in transit:", ZBool.False, Container.JC_FCLHeldInTransitStaging);
			NotificationBuffer notify = new NotificationBuffer();
			ContainerDatesFlatFileDataRow row = new ContainerDatesFlatFileDataRow("YARD,,CONTAINER1,,,,,,CLE");
			Importer.Import(row, notify, Exceptions);
			AssertEquals("Held in transit updated:", ZBool.True, Container.JC_FCLHeldInTransitStaging);
		}

		public void TestImportWithExceptions()
		{
			CLEDataRegistry.Instance.ContainerUploadEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.PostMastersGroupPK);
			GlbGroup pmgGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			pmgGroup.Staff[0].GS_EmailAddress = "test@cargowise.com";
			Factory.Save();
			ContainerDatesFlatFileDataRow row = new ContainerDatesFlatFileDataRow("09-03-2007,,CONTAINER1,,10-03-2007,,,,CLE");
			ContainerDatesExceptionBuffer exceptions = new ContainerDatesExceptionBuffer();
			NotificationBuffer notify = new NotificationBuffer();
			Importer.Import(row, notify, exceptions);
			Container.Factory.Save();
			Importer.Import(row, notify, exceptions);
			row = new ContainerDatesFlatFileDataRow("11-03-2007,,CONTAINER1,,12-03-2007,,,,CLE");
			Importer.Import(row, notify, exceptions);
			row = new ContainerDatesFlatFileDataRow(",,CONTAINER1,,12-03-2007,,,,CLE");
			Importer.Import(row, notify, exceptions);
			row = new ContainerDatesFlatFileDataRow("11-03-2007,,CONTAINER1,,10-03-2007,,,,CLE");
			Importer.Import(row, notify, exceptions);
			row = new ContainerDatesFlatFileDataRow("U/BOND,,CONTAINER1,,12-03-2007,,,,CLE");
			Importer.Import(row, notify, exceptions);
			row = new ContainerDatesFlatFileDataRow("YARD,,CONTAINER1,,10-03-2007,,,,CLE");
			Importer.Import(row, notify, exceptions);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			exceptions.CreateExceptionReport(notify);
			AttachmentDef attachment = Env.OutgoingMailManager.EmailsCreated[0].Attachments[0];
			string exceptionReportBody = Encoding.ASCII.GetString(attachment.Data);
			string expectedString = "Container,Job,Delivered,De-Hire" + System.Environment.NewLine + "CONTAINER1,CLE,09-03-2007,10-03-2007," + CLEConstants.ContainerNotFound + System.Environment.NewLine + "CONTAINER1,CLE,11-03-2007,12-03-2007," + CLEConstants.PreviouslyDelivered + " 09-Mar-07" + System.Environment.NewLine + "CONTAINER1,CLE,11-03-2007,12-03-2007," + CLEConstants.PreviouslyDeHired + " 10-Mar-07" + System.Environment.NewLine + "CONTAINER1,CLE,,12-03-2007," + CLEConstants.DeHireButNoDelivery + System.Environment.NewLine + "CONTAINER1,CLE,11-03-2007,10-03-2007," + CLEConstants.DeHirePriorToDelivery + System.Environment.NewLine + "CONTAINER1,CLE,11-03-2007,10-03-2007," + CLEConstants.PreviouslyDeHired + " 12-Mar-07" + System.Environment.NewLine + "CONTAINER1,CLE,YARD,10-03-2007," + CLEConstants.YardAndDeHireDate + " 10-Mar-07" + System.Environment.NewLine;
			AssertEquals(expectedString, exceptionReportBody);
		}

		CommonContainer Container
		{
			get
			{
				if (container == null)
				{
					CommonConsol consol = GetImportConsol(typeof(CommonConsol));
					container = consol.Containers.AddNew();
					container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
					container.JC_ContainerNum = "CONTAINER1";
					container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
				}

				return container;
			}
		}

		CommonContainer container;
		ContainerDatesFlatFileDataRowImporter Importer
		{
			get
			{
				return importer ?? (importer = new ContainerDatesFlatFileDataRowImporter(new BusinessObjectFactoryProvider(Factory)));
			}
		}

		ContainerDatesFlatFileDataRowImporter importer;
		ContainerDatesExceptionBuffer Exceptions
		{
			get
			{
				return (exceptions) ?? (exceptions = new ContainerDatesExceptionBuffer());
			}
		}

		ContainerDatesExceptionBuffer exceptions;
	}
}
