using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirOutturnDataObjectReaderHelperTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestHandlingOfUnprocessedBills()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var underbond1 = Factory.BOFactory.New<CusUnderbond>();
				var outturn1 = Factory.BOFactory.New<CusOutturn>();
				outturn1.C5_HouseBill = "HB1";
				var outturn2 = Factory.BOFactory.New<CusOutturn>();
				outturn2.C5_HouseBill = "HB2";
				var outturn3 = Factory.BOFactory.New<CusOutturn>();
				outturn3.C5_HouseBill = "HB3";
				var outturn4 = Factory.BOFactory.New<CusOutturn>();
				outturn4.C5_HouseBill = "HB4";
				underbond1.Outturns.AddRange(outturn1, outturn2, outturn3, outturn4);

				var underbond2 = Factory.BOFactory.New<CusUnderbond>();
				var outturn5 = Factory.BOFactory.New<CusOutturn>();
				outturn5.C5_HouseBill = "HB5";
				var outturn6 = Factory.BOFactory.New<CusOutturn>();
				outturn6.C5_HouseBill = "HB6";
				underbond2.Outturns.AddRange(outturn5, outturn6);

				var underbond3 = Factory.BOFactory.New<CusUnderbond>();
				var outturn7 = Factory.BOFactory.New<CusOutturn>();
				outturn7.C5_HouseBill = "HB7";
				underbond3.Outturns.Add(outturn7);

				var helper = new AirOutturnDataObjectReaderHelper(Factory);
				helper.MarkUnprocessedExistingBillsFor(underbond1);
				helper.MarkUnprocessedExistingBillsFor(underbond2);
				helper.MarkProcessed(outturn2);
				helper.MarkProcessed(outturn3);
				helper.MarkProcessed(outturn6);
				helper.MarkProcessed(outturn7);
				foreach (var outturn in new[] { outturn1, outturn2, outturn3, outturn4, outturn5, outturn6, outturn7 })
				{
					AssertEquals("IsDeleted", false, outturn.IsDeleted);
				}

				var logger = new TestErrorLogger();
				helper.DeleteUnprocessedBillsFor(underbond3, logger);
				foreach (var outturn in new[] { outturn1, outturn2, outturn3, outturn4, outturn5, outturn6, outturn7 })
				{
					AssertEquals("IsDeleted", false, outturn.IsDeleted);
				}
				AssertEquals("logger.Logs", "", logger.Logs);

				helper.DeleteUnprocessedBillsFor(underbond1, logger);
				AssertEquals("outturn1.IsDeleted", true, outturn1.IsDeleted);
				AssertEquals("outturn4.IsDeleted", true, outturn4.IsDeleted);
				foreach (var outturn in new[] { outturn2, outturn3, outturn5, outturn6, outturn7 })
				{
					AssertEquals("IsDeleted", false, outturn.IsDeleted);
				}
				AssertMultilineASCIIEquals("logger.Logs", @"Information - Deleted Outturn Bill HB1 from UniversalShipment.
Information - Deleted Outturn Bill HB4 from UniversalShipment.", logger.Logs);

				logger.ClearLogs();
				helper.DeleteUnprocessedBillsFor(underbond2, logger);
				AssertEquals("outturn5.IsDeleted", true, outturn5.IsDeleted);
				foreach (var outturn in new[] { outturn2, outturn3, outturn6, outturn7 })
				{
					AssertEquals("IsDeleted", false, outturn.IsDeleted);
				}
				AssertEquals("logger.Logs", "Information - Deleted Outturn Bill HB5 from UniversalShipment.", logger.Logs);
			}
		}
	}
}
