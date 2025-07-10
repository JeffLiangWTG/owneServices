using System;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.CLE.Testing
{
	public class ContainerDatesImporterTest : BaseFreightTest
	{
		public void TestImport()
		{
			CommonConsol consol = GetImportConsol(typeof(CommonConsol));
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			Assert("ArrivalCartageComplete", container.JC_ArrivalCartageComplete.IsEmpty);
			Assert("EmptyReturnedOn", container.JC_ContainerYardEmptyReturnGateIn.IsEmpty);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			CLEDataRegistry.Instance.ContainerUploadEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.PostMastersGroupPK);
			GlbGroup pmgGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			pmgGroup.Staff[0].GS_EmailAddress = "test@cargowise.com";
			Factory.Save();
			NotificationBuffer notify = new NotificationBuffer();
			ContainerDatesImporter importer = new ContainerDatesImporter();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string sampleFile = resourceRetriever.SaveResourceToFile("ContainerDeHireUpload.csv");
				importer.ImportData(sampleFile, notify, SourceInfo.EmptySourceInfo);
			}
			AssertEquals("ArrivalCartageComplete after import:", new ZDateTime(2007, 3, 9), container.JC_ArrivalCartageComplete);
			AssertEquals("Notify should not have errors " + notify.AsString + " date: " + container.JC_ContainerYardEmptyReturnGateIn, false, notify.HasErrors);
			AssertEquals("EmptyReturnedOn after import:", new ZDateTime(2007, 3, 10), container.JC_ContainerYardEmptyReturnGateIn);
		}
	}
}
