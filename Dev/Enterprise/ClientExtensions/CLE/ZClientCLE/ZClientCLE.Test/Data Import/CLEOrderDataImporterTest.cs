using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.CLE.OrdersDataImport.Testing
{
	class CLEOrderDataImporterTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			int orderCount = Factory.GetDatabaseCount(typeof(Order));
			AssertEquals("Precondition", 0, orderCount);
			NotificationBuffer buffer = new NotificationBuffer();
			CLEOrderDataImporter importer = new CLEOrderDataImporter();
			SystemDefinedOrganisation value = new UnmatchedOrganisation(Factory);
			value.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string pathToFile = resourceRetriever.SaveResourceToFile("order-4splits.csv");
				importer.ImportData(pathToFile, buffer, SourceInfo.EmptySourceInfo);
			}
			Order[] loadedOrders = Factory.Load<Order>(new ZQuery());
			AssertEquals("5 new order should have been loaded", 5, loadedOrders.Length);
			AssertEquals("Order number should be 352326/46", "352326/46", loadedOrders[0].JD_OrderNumber);
			AssertEquals("Order has 2 order lines", 2, loadedOrders[0].OrderLines.Count);
			AssertEquals("Order number should be 352333/99", "352333/99", loadedOrders[1].JD_OrderNumber);
			AssertEquals("Order has only one order line", 1, loadedOrders[1].OrderLines.Count);
			AssertEquals("Order number solit should be 0", (byte)0, loadedOrders[1].JD_OrderNumberSplit);
			AssertEquals("Order number should be 352333/99", "352333/99", loadedOrders[2].JD_OrderNumber);
			AssertEquals("Order number solit should be 1", (byte)1, loadedOrders[2].JD_OrderNumberSplit);
			AssertEquals("Order has 1 order lines", 1, loadedOrders[2].OrderLines.Count);
			AssertEquals("Order number should be 352333/99", "352333/99", loadedOrders[3].JD_OrderNumber);
			AssertEquals("Order number split should be 2", (byte)2, loadedOrders[3].JD_OrderNumberSplit);
			AssertEquals("Order has only one order line", 1, loadedOrders[3].OrderLines.Count);
			AssertEquals("Order number should be 352333/99", "352333/99", loadedOrders[4].JD_OrderNumber);
			AssertEquals("Order number split should be 2", (byte)3, loadedOrders[4].JD_OrderNumberSplit);
			AssertEquals("Order has only one order line", 1, loadedOrders[4].OrderLines.Count);
			AssertEquals(ZString.Empty, importer.ErrorOrderNumbers);
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string pathToFile = resourceRetriever.SaveResourceToFile("order-2splits.csv");
				importer.ImportData(pathToFile, buffer, SourceInfo.EmptySourceInfo);
			}
			Factory.Save();
			loadedOrders = Factory.Load<Order>(new ZQuery());
			AssertEquals("4 new order should have been loaded", 5, loadedOrders.Length);
			AssertEquals("Order number should be 352326/46", "352326/46", loadedOrders[0].JD_OrderNumber);
			AssertEquals("Order has 1 order line", 1, loadedOrders[0].OrderLines.Count);
			AssertEquals("Order line number", 2342, loadedOrders[0].OrderLines[0].JO_LineNo);
			AssertEquals("Order number should be 352333/99", "352333/99", loadedOrders[1].JD_OrderNumber);
			AssertEquals("Order has only one order line", 1, loadedOrders[1].OrderLines.Count);
			AssertEquals("Order number solit should be 0", (byte)0, loadedOrders[1].JD_OrderNumberSplit);
			AssertEquals("Order number should be 352333/99", "352333/99", loadedOrders[2].JD_OrderNumber);
			AssertEquals("Order number solit should be 1", (byte)1, loadedOrders[2].JD_OrderNumberSplit);
			AssertEquals("Order has 1 order lines", 2, loadedOrders[2].OrderLines.Count);
		}

		public void TestImport_CrapData()
		{
			int orderCount = Factory.GetDatabaseCount(typeof(Order));
			AssertEquals("Precondition", 0, orderCount);
			NotificationBuffer buffer = new NotificationBuffer();
			CLEOrderDataImporter importer = new CLEOrderDataImporter();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string pathToFile = resourceRetriever.SaveResourceToFile("crap.wtf");
				AssertEquals("Precondition: number of emaail created = 0", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				importer.ImportData(pathToFile, buffer, SourceInfo.EmptySourceInfo);
			}
			Order[] loadedOrders = Factory.Load<Order>(new ZQuery());
			AssertEquals(0, loadedOrders.Length);
		}

		public void TestEmailBodyWhenTwoOrderDeliverPointNotFound()
		{
			GlbGroup group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@edi.com.au";
			NotificationDataRegistry.Instance.OrderImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.PostMastersGroupPK);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			SystemDefinedOrganisation value = new UnmatchedOrganisation(Factory);
			value.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			Factory.Save();
			NotificationBuffer buffer = new NotificationBuffer();
			CLEOrderDataImporter importer = new CLEOrderDataImporter();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string pathToFile = resourceRetriever.SaveResourceToFile("TwoOrderBothUnmatchedDeliveryPoints.csv");
				importer.ImportData(pathToFile, buffer, SourceInfo.EmptySourceInfo);
			}
			Assert("One email should have been sent", Env.OutgoingMailManager.EmailsCreated.Count > 0);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			var pattern = new Regex(@"^No matching Deliver Point found on 'Ordered by' organization. Deliver point could therefore not be imported on the following order.*\r\nOrder:.*\r\nOrder:.*");
			Assert("Email body should contain 2 orders with unmatched delivery points.", pattern.IsMatch(email.Body));
		}

		public void TestImportThreshold()
		{
			int orderCount = Factory.GetDatabaseCount(typeof(Order));
			AssertEquals("Precondition", 0, orderCount);
			NotificationBuffer buffer = new NotificationBuffer();
			var importer = new CLEOrderDataImporterForTest();
			SystemDefinedOrganisation value = new UnmatchedOrganisation(Factory);
			value.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			CLEDataRegistry.Instance.MaximumOrdersToDeliver = 3;
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string pathToFile = resourceRetriever.SaveResourceToFile("order-2splits.csv");
				importer.ImportData(pathToFile, buffer, SourceInfo.EmptySourceInfo);
			}
			Order[] loadedOrders = Factory.Load<Order>(new ZQuery());
			AssertEquals(4, loadedOrders.Length);
			AssertEquals(0, importer.testDictionary.Count);
			AssertEquals(ZString.Empty, importer.ErrorOrderNumbers);
		}

		class CLEOrderDataImporterForTest : CLEOrderDataImporter
		{
			protected override Dictionary<ZGuid, ImportedOrder> NewImportedOrdersDictionary()
			{
				return testDictionary;
			}

			internal Dictionary<ZGuid, ImportedOrder> testDictionary = new Dictionary<ZGuid, ImportedOrder>();
		}

		#region Set up
		ZGuid notificationGroupPK;
		protected override void SetUp()
		{
			base.SetUp();
			DataTransferSwitchRegistryBusinessObject b = new DataTransferSwitchRegistryBusinessObject();
			GlbGroup g = Factory.NewWithValidTestData<GlbGroup>();
			notificationGroupPK = g.PK;
			g.GG_Code = "TMP";
			var staff = g.Staff.AddNew();
			staff.GS_EmailAddress = "bbb@ccc.com";
			staff.GS_Code = "ZAC";
			Factory.Save();
			b.GroupPK = g.PK;
			b.NextRunDateTime = ZDateTime.Now.AddMinutes(-20);
			embeddedResourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			b.Directory = embeddedResourceRetriever.SaveAllResourcesToFiles();
			b.Interval = 25;
			b.EnableInterface = true;
			CLEDataRegistry.Instance.SwitchOrderImportItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, b);
			NotificationDataRegistry.Instance.ImportedOrderChangesNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid());
		}
		EmbeddedResourceRetriever embeddedResourceRetriever;
		#endregion

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever.Dispose();
		}
	}
}
