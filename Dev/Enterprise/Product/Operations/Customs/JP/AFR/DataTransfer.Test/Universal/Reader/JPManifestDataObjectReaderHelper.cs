using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	class JPManifestDataObjectReaderHelperTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestHandlingOfUnprocessedBills()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				var header1 = Factory.BOFactory.New<JPAFRHeader>();
				var bill1 = header1.Bills.AddNew();
				bill1.JPB_BillNumber = "HB1";
				var bill2 = header1.Bills.AddNew();
				bill2.JPB_BillNumber = "HB2";
				var bill3 = header1.Bills.AddNew();
				bill3.JPB_BillNumber = "HB3";
				var bill4 = header1.Bills.AddNew();
				bill4.JPB_BillNumber = "HB4";

				var header2 = Factory.BOFactory.New<JPAFRHeader>();
				var bill5 = header2.Bills.AddNew();
				bill5.JPB_BillNumber = "HB5";
				var bill6 = header2.Bills.AddNew();
				bill6.JPB_BillNumber = "HB6";

				var header3 = Factory.BOFactory.New<JPAFRHeader>();
				var bill7 = header3.Bills.AddNew();
				bill7.JPB_BillNumber = "HB7";

				var helper = new JPManifestDataObjectReaderHelper(Factory);
				helper.MarkUnprocessedExistingBillsFor(header1);
				helper.MarkUnprocessedExistingBillsFor(header2);
				helper.MarkProcessed(bill2);
				helper.MarkProcessed(bill3);
				helper.MarkProcessed(bill6);
				helper.MarkProcessed(bill7);
				foreach (var bill in new[] { bill1, bill2, bill3, bill4, bill5, bill6, bill7 })
				{
					AssertEquals("IsDeleted", false, bill.IsDeleted);
				}

				var logger = new TestErrorLogger();
				helper.DeleteUnprocessedBillsFor(header3, logger);
				foreach (var bill in new[] { bill1, bill2, bill3, bill4, bill5, bill6, bill7 })
				{
					AssertEquals("IsDeleted", false, bill.IsDeleted);
				}
				AssertEquals("logger.Logs", "", logger.Logs);

				helper.DeleteUnprocessedBillsFor(header1, logger);
				AssertEquals("bill1.IsDeleted", true, bill1.IsDeleted);
				AssertEquals("bill4.IsDeleted", true, bill4.IsDeleted);
				foreach (var bill in new[] { bill2, bill3, bill5, bill6, bill7 })
				{
					AssertEquals("IsDeleted", false, bill.IsDeleted);
				}
				AssertMultilineASCIIEquals("logger.Logs", @"Information - Deleted Bill Of Lading HB1 from UniversalShipment.
Information - Deleted Bill Of Lading HB4 from UniversalShipment.", logger.Logs);

				logger.ClearLogs();
				helper.DeleteUnprocessedBillsFor(header2, logger);
				AssertEquals("bill5.IsDeleted", true, bill5.IsDeleted);
				foreach (var bill in new[] { bill2, bill3, bill6, bill7 })
				{
					AssertEquals("IsDeleted", false, bill.IsDeleted);
				}
				AssertEquals("logger.Logs", "Information - Deleted Bill Of Lading HB5 from UniversalShipment.", logger.Logs);
			}
		}
	}
}
