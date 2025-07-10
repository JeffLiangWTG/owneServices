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
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ORtfTextUtil = Enterprise.ZArchitecture.Core.ORtfTextUtil;

namespace Enterprise.Client.Wow.Testing
{
	public class OrderCsvRecordTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportOrdersCsv()
		{
			OrderCollection orders = new OrderCollection(Factory, new ZQuery());
			orders.ApplySort(JobOrderHeaderSchema.JD_OrderNumber.Name, System.ComponentModel.ListSortDirection.Ascending);
			orders.DeleteAll();
			SetupOrderTestData(Factory);
			using (StreamReader fileContents = new WowTestUtil().GetStreamReaderForTestFile("DataImportExport\\Orders\\Testing\\TestOrders.csv"))
			{
				NotificationBuffer buffer = new NotificationBuffer(null);
				ITransactionParticipant[] transactionActions;
				fImporter.ImportDataToFactory(fileContents, "", buffer, SourceInfo.EmptySourceInfo, out transactionActions);
				// check the count, all orders (even bodged ones) will be in there
				AssertEquals("Total number of orders now in factory", 9, orders.Count);
				// make sure the data was migrated correctly
				AssertEquals("KOWLING COMPANY LIMITED", orders[0].Supplier.OH_FullName.ToUpper());
				AssertEquals("SUPERMARKET GENERAL MERCHANDISE", orders[0].Buyer.OH_FullName.ToUpper());
				AssertEquals("USD", orders[0].JD_RX_NKOrderCurrency);
				AssertEquals("STD", orders[0].ServiceLevel_NI.RS_Code);
				AssertEquals("SHENZHEN", orders[0].PortOfLoading.RL_PortName.ToUpper());
				AssertEquals(Enterprise.Core.Constants.ContainerModes.FCL, orders[0].JD_ContainerMode);
				AssertContainsNote(orders[0].Notes, WowConstants.OrderIncoTermNoteDescription, "FOB");
				new WowTestUtil().AssertBusinessPropsEquals(orders[0], OrderCsvRecord.OrderPropertyMappings, null, new ZString("P08735"), new ZString("U"), new ZDecimal(88), null, null, null, null, new ZDecimal(72), null, new ZString("FOB"), new ZString(""), new ZString("PETER PROBERT"), new ZString("DONKEY BOY"), new ZString("C&F"));
				new WowTestUtil().AssertBusinessPropsEquals(orders[8], OrderCsvRecord.OrderPropertyMappings, null, new ZString("P08755"), new ZString("D"), new ZDecimal(230), null, null, null, null, new ZDecimal(57), null, new ZString("C&F"), new ZString(""), ZString.Empty, ZString.Empty, new ZString("LOSAN")); // the first 5 letters go in when no port is available
																																																																														 // make sure doing this a second time produces no changes
				new WowTestUtil().CheckNoChangesAfterRunningImportSecondTime(Factory, fImporter, fileContents, buffer);
			}
		}

		/// <summary>
		/// Set up the related data (fks to orgs, currency etc) required to match on the first order in the test data.
		/// </summary>
		public static void SetupOrderTestData(BusinessObjectFactory factory)
		{
			factory.NewWithValidTestData<OrgHeader>().OH_FullName = "KOWLING COMPANY LIMITED";
			factory.NewWithValidTestData<OrgHeader>().OH_FullName = "DENICE & FILICE PACKAGING COMPANY";
			factory.NewWithValidTestData<OrgHeader>().OH_FullName = "SUPERMARKET GENERAL MERCHANDISE";
			factory.NewWithValidTestData<OrgHeader>().OH_FullName = "SUPERMARKET PRODUCE";
			factory.New<OrgContact>().OC_ContactName = "PETER PROBERT";
			factory.New<RefCurrency>().RX_Code = "USD";
			factory.New<RefServiceLevel>().RS_Code = "FOB";
			factory.New<RefServiceLevel>().RS_Code = "C&F";
			var unloco = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_PortName, SQLComparisonOperator.Equal, "SHENZHEN"));
			unloco.RL_Code = "C&F";
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeleteOrders()
		{
			WoolworthsOrder bO = Factory.New<WoolworthsOrder>();
			bO.JD_OrderNumber = "P08735";
			using (StreamReader fileContents = new WowTestUtil().GetStreamReaderForTestFile("DataImportExport\\Orders\\Testing\\DeleteOrder.csv"))
			{
				NotificationBuffer buffer = new NotificationBuffer(null);
				ITransactionParticipant[] transactionActions;
				fImporter.ImportDataToFactory(fileContents, "", buffer, SourceInfo.EmptySourceInfo, out transactionActions);
				AssertEquals("Order should be deleted", true, bO.IsDeleted);
			}
		}

		[ExpectNoExceptions]
		public void TestCanSaveBigNumbers()
		{
			WoolworthsOrder order = Factory.New<WoolworthsOrder>();
			order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.JD_OrderNumber = "P08735";
			OrderLine line = order.OrderLines.AddNew();
			line.JO_LineNo = 2;
			line.JO_Quantity = new ZDecimal(10000000.12345);
			line.JO_QtyInvoiced = new ZDecimal(10000000.12345);
			line.JO_QtyReceived = new ZDecimal(10000000.12345);
			line.JO_ItemPrice = new ZDecimal(10000000.12345);
			OrderLineDelivery delivery = line.Deliveries.AddNew();
			delivery.J4_Allocated = new ZDecimal(10000000.12345);
			Factory.Save();
		}

		public void TestBigNumbersThrowsZSaveException()
		{
			WoolworthsOrder order = Factory.New<WoolworthsOrder>();
			order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.JD_OrderNumber = "P08735";
			OrderLine line = order.OrderLines.AddNew();
			line.JO_LineNo = 2;
			line.JO_Quantity = new ZDecimal(100000000000000.12345);
			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				Assert("Dont throw some weird exception: " + ex.Message, ex.Message.IndexOf("converting data") != -1 || ex.Message.IndexOf("overflow") != -1);
			}
		}

		#region Implementation
		TestWowDataImporter fImporter;
		void AssertContainsNote(Notes notes, ZString description, ZString noteContents)
		{
			bool found = false;
			foreach (StmNote note in notes.GetAllNotes())
			{
				if (note.ST_Description == description)
				{
					found = true;
					AssertEquals("Correct note contents", noteContents, ORtfTextUtil.RtfToText(note.ST_NoteData));
				}
			}

			AssertEquals("Found the note", true, found);
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

		protected override void SetUp()
		{
			base.SetUp();
			fImporter = new TestWowDataImporter(Factory);
		}
		#endregion
	}
}
