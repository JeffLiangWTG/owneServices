using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Wow.Testing
{
	public class ContainerManifestCsvImportFileTest : TestCaseWithFactory
	{
		public void TestParseDate()
		{
			ZDateTime date = TestContainerManifestCsvImportFile.ParseDate("10/03/2004");
			AssertEquals(new ZDateTime(2004, 3, 10), date);
			ZDateTime date2 = TestContainerManifestCsvImportFile.ParseDate("2004/03/10");
			AssertEquals(new ZDateTime(2004, 3, 10), date2);
			ZDateTime date3 = TestContainerManifestCsvImportFile.ParseDate("Mar-03-2004");
			AssertEquals(new ZDateTime(2004, 3, 3), date3);
		}

		class TestContainerManifestCsvImportFile : ContainerManifestCsvImportFile
		{
			public TestContainerManifestCsvImportFile(StreamReader reader) : base(new BusinessObjectFactoryProvider(), reader)
			{
			}

			public new static ZDateTime ParseDate(string dateAsString)
			{
				return ContainerManifestCsvImportFile.ParseDate(dateAsString);
			}
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestContainerManifestCsvImportFile_()
		{
			new WowTestUtil().DeleteAllOrders(Factory);
			AddOrderWithLineAndDelivery(Factory, "P14373", "075758", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14635", "801074", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14635", "801075", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14953", "203257", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14438", "014367", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14517", "025365", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14360", "015396", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14360", "096942", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14360", "096943", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14469", "074636", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14648", "074636", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14514", "036840", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14514", "020877", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14514", "003166", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14837", "801320", "AUADL");
			AddOrderWithLineAndDelivery(Factory, "P14294", "136813", "AUADL");
			using (StreamReader fileContents = new WowTestUtil().GetStreamReaderForTestFile("DataImportExport\\ContainerManifest\\Testing\\TestContainerManifest.csv"))
			{
				string fileContentsAsString = fileContents.ReadToEnd();
				fileContents.BaseStream.Position = 0;
				string[] lines = fileContentsAsString.Split('\n');
				NotificationBuffer buffer = new NotificationBuffer(null);
				ITransactionParticipant[] transactionActionsUnused;
				fImporter.ImportDataToFactory(fileContents, "", buffer, SourceInfo.EmptySourceInfo, out transactionActionsUnused);
				ZQuery ordersFilter = new ZQuery();
				OrderCollection orders = new OrderCollection(Factory, ordersFilter);
				orders.ApplySort(Order.Schema.JD_OrderNumber, System.ComponentModel.ListSortDirection.Ascending);
				// check the count, all containers should be in there
				int containerCount = CountContainers(orders);
				AssertEquals("Container count", 16, containerCount);
				// make sure the data was migrated correctly
				OrderLineDeliverContainer container1 = orders[0].OrderLines[0].Deliveries[0].Containers[0];
				AssertEquals("P14294", orders[0].JD_OrderNumber);
				AssertEquals("136813", orders[0].OrderLines[0].JO_Partno);
				OrderLineDeliverContainer container2 = orders[1].OrderLines[1].Deliveries[0].Containers[0];
				AssertEquals("P14360", orders[1].JD_OrderNumber);
				AssertEquals("096942", orders[1].OrderLines[1].JO_Partno);
				AssertEquals("GATU0191605", container1.J5_ContainerNum);
				AssertEquals("GATU0191605", container2.J5_ContainerNum);
				AssertEquals("D651774", container1.J5_ContainerSeal);
				AssertEquals("D651774", container2.J5_ContainerSeal);
				AssertEquals("HKHKG", container1.J5_RL_NKLoadPort);
				AssertEquals("HKHKG", container2.J5_RL_NKLoadPort);
				AssertEquals("OOCL MELBOURNE", container1.J5_RV_NKArrivalVessel);
				AssertEquals("OOCL MELBOURNE", container2.J5_RV_NKArrivalVessel);
				AssertEquals("HKHKG", container1.J5_CustomAttribute1);
				AssertEquals("HKHKG", container2.J5_CustomAttribute1);
				AssertEquals("006S", container1.J5_Voyage);
				AssertEquals("006S", container2.J5_Voyage);
				AssertEquals(new ZDateTime(2003, 12, 1), container1.J5_ETD);
				AssertEquals(new ZDateTime(2003, 12, 1), container2.J5_ETD);
				AssertEquals(new ZDateTime(2003, 12, 17), container1.J5_ETA);
				AssertEquals(new ZDateTime(2003, 12, 17), container2.J5_ETA);
				new WowTestUtil().AssertBusinessPropsEquals(container1, ContainerManifestCsvLine.ContainerPropertyMappings, null, null, null, null, null, new ZShort((short)16), new ZDecimal(1.11), new ZDecimal(120), new ZDecimal(384), null, null, new ZString("OOLU86215033"));
				new WowTestUtil().AssertBusinessPropsEquals(container2, ContainerManifestCsvLine.ContainerPropertyMappings, null, null, null, null, null, new ZShort((short)4), new ZDecimal(0.23), new ZDecimal(62), new ZDecimal(2304), null, null, new ZString("OOLU86215033"));
				// make sure doing this a second time produces no changes
				new WowTestUtil().CheckNoChangesAfterRunningImportSecondTime(Factory, fImporter, fileContents, buffer);
			}
		}

		#region Implementation
		protected void AddOrderWithLineAndDelivery(BusinessObjectFactory factory, string orderNo, string partNo, string destinationPort)
		{
			Order order = factory.LoadTop1<Order>(new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.Equal, orderNo));
			if (order == null)
			{
				order = factory.New<Order>();
				order.BuyerPK = factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
				order.SupplierPK = factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
				order.JD_OrderNumber = orderNo;
			}

			OrderLine line = order.OrderLines.AddNew();
			line.JO_LineNo = 98;
			line.JO_Partno = partNo;
			OrderLineDelivery delivery = line.Deliveries.AddNew();
			delivery.J4_RL_NKDestinationPort = destinationPort;
		}

		int CountContainers(OrderCollection orders)
		{
			int result = 0;
			foreach (Order order in orders)
			{
				foreach (OrderLine line in order.OrderLines)
				{
					foreach (OrderLineDelivery delivery in line.Deliveries)
					{
						result += delivery.Containers.Count;
					}
				}
			}

			return result;
		}

		TestWowDataImporter fImporter;
		protected override void SetUp()
		{
			base.SetUp();
			fImporter = new TestWowDataImporter(Factory);
		}

		class TestWowDataImporter : WowDataImporter
		{
			public TestWowDataImporter(BusinessObjectFactory factory) : base(new SingleBusinessObjectFactoryProvider(factory))
			{
			}

			public new bool ImportDataToFactory(TextReader data, string attachmentFileName, INotifications notify, ISourceInfo sourceInfo, out ITransactionParticipant[] transactionActions)
			{
				return base.ImportDataToFactory(data, attachmentFileName, notify, sourceInfo, out transactionActions);
			}
		}
		#endregion
	}
}
