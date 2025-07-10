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
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	public class DeliveryCsvRecordTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDeliveriesCsv()
		{
			ZQuery filter = new ZQuery();
			OrderCollection orders = new OrderCollection(Factory, filter);
			orders.ApplySort(Order.Schema.JD_OrderNumber, System.ComponentModel.ListSortDirection.Ascending);
			orders.DeleteAll();
			OrderCsvRecordTest.SetupOrderTestData(Factory);
			OrgAddress addr = Factory.New<OrgAddress>();
			addr.OA_Code = "1904";
			addr.OA_RL_NKRelatedPortCode = "USXHP";
			addr.OA_OH = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.Equal, "SUPERMARKET GENERAL MERCHANDISE")).PK;
			using (StreamReader fileContents = new WowTestUtil().GetStreamReaderForTestFile("DataImportExport\\Orders\\Testing\\TestOrders.csv"))
			{
				ITransactionParticipant[] transactionActions;
				fImporter.ImportDataToFactory(fileContents, "", new NotificationBuffer(null), SourceInfo.EmptySourceInfo, out transactionActions);
				// number of Deliveries in first order line of first order
				AssertEquals("Order line count in first order", 2, orders[0].OrderLines.Count);
				AssertEquals("Delivery count in first line of first order", 1, orders[0].OrderLines[0].Deliveries.Count);
				// make sure the data was migrated correctly
				AssertEquals("DeliverPoint", "1904", orders[0].OrderLines[0].Deliveries[0].J4_OA_NKDeliveryPoint);
				new WowTestUtil().AssertBusinessPropsEquals(orders[0].OrderLines[0].Deliveries[0], DeliveryCsvRecord.DeliveryPropertyMappings, null, "P08747", new ZDecimal(10744), new ZString("1904"), new ZString("SYDNE"), new ZDecimal(8928), new ZString(""));
				// make sure doing this a second time produces no changes
				new WowTestUtil().CheckNoChangesAfterRunningImportSecondTime(Factory, fImporter, fileContents, new NotificationBuffer(null));
			}
		}

		class TestWowDataImporter : WowDataImporter
		{
			public TestWowDataImporter(BusinessObjectFactory factory) : base(new SingleBusinessObjectFactoryProvider(factory))
			{
			}

			public new bool ImportDataToFactory(TextReader data, string attachmentFileName, INotifications notify, ISourceInfo info, out ITransactionParticipant[] transactionActions)
			{
				return base.ImportDataToFactory(data, attachmentFileName, notify, info, out transactionActions);
			}
		}

		TestWowDataImporter fImporter;
		protected override void SetUp()
		{
			base.SetUp();
			fImporter = new TestWowDataImporter(Factory);
		}
	}
}
