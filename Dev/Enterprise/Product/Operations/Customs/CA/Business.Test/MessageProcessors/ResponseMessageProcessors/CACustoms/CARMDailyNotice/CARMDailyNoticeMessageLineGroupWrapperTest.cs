using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	public class CARMDailyNoticeMessageLineGroupWrapperTest : TestCaseWithFactory
	{
		public void TestProperties_AH()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "100023258RM0001", Core.Constants.CountryCodes.Canada);
			Factory.Save();

			var wrapper = CARMDailyNoticeMessageWrapperTest.GetCARMDailyNoticeMessageWrapper(Factory, CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter, CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeImporterMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("The count of line group should be 1", 1, wrapper.LineGroups.Count());
				var lineGroup = wrapper.LineGroups.ToList()[0];
				AssertEquals("ZCARMDNOTICEAH.HEADER.PARTY.NAME_ORG1 + NAME_ORG2 + NAME_ORG3 + NAME_ORG4", "Importer 100023258 Org name 2", lineGroup.LegalName);
				AssertEquals("ZCARMDNOTICEAH.HEADER.PARTY.ACCOUNT", "100023258RM0001", lineGroup.ImporterBusinessNumber);
				AssertEquals("Importer", importer, lineGroup.Importer);
				AssertEquals("ZCARMDNOTICEAH.SUMMARY.PAYMENTS", 11m, lineGroup.PaidAmount);
				AssertEquals("ZCARMDNOTICEAH.SUMMARY.DISB", 22m, lineGroup.RefundAmount);
				AssertEquals("LINETOTALS.DUTIES", 252408m, lineGroup.TotalDuties);
				AssertEquals("LINETOTALS.SIMA", 98m, lineGroup.TotalSIMA);
				AssertEquals("LINETOTALS.EXCISE", 413.08m, lineGroup.TotalExciseTax);
				AssertEquals("LINETOTALS.EXCISEDUTIES", 1242.03m, lineGroup.TotalExciseDuties);
				AssertEquals("LINETOTALS.GST + PST + HST", 84380.5m, lineGroup.TotalGSTAndPSTAndHST);
				AssertEquals("LINETOTALS.INTEREST", 95m, lineGroup.TotalInterests);
				AssertEquals("LINETOTALS.OTHERS", 94m, lineGroup.TotalOthers);
				AssertEquals("LINETOTALS.TOTALS", 337837.53m, lineGroup.TotalTotals);
				AssertEquals("Should be 3", 3, lineGroup.LineIteams.Count());
			});
		}

		public void TestProperties_CB()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "178234732RM0001", Core.Constants.CountryCodes.Canada);
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "178234732RM0002", Core.Constants.CountryCodes.Canada);
			Factory.Save();

			var wrapper = CARMDailyNoticeMessageWrapperTest.GetCARMDailyNoticeMessageWrapper(Factory, CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeBroker, CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeBrokerMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("The count of line group should be 2", 2, wrapper.LineGroups.Count());
				var lineGroup = wrapper.LineGroups.ToList()[0];
				AssertEquals("ZCARMDNOTICECBDETAILSIMPORTER.PARTY.NAME_ORG1 + NAME_ORG2 + NAME_ORG3 + NAME_ORG4", "Importer 17823473201", lineGroup.LegalName);
				AssertEquals("ZCARMDNOTICECBDETAILSIMPORTER.PARTY.ACCOUNT", "178234732RM0001", lineGroup.ImporterBusinessNumber);
				AssertEquals("Importer1", importer1, lineGroup.Importer);
				AssertEquals("LINETOTALS.PAYMENTS", 99m, lineGroup.PaidAmount);
				AssertEquals("Should be zero for now", ZDecimal.Zero, lineGroup.RefundAmount);
				AssertEquals("LINETOTALS.DUTIES", 200000m, lineGroup.TotalDuties);
				AssertEquals("LINETOTALS.SIMA", 32m, lineGroup.TotalSIMA);
				AssertEquals("LINETOTALS.EXCISE", 1000m, lineGroup.TotalExciseTax);
				AssertEquals("LINETOTALS.EXCISEDUTIES", 43.16m, lineGroup.TotalExciseDuties);
				AssertEquals("LINETOTALS.GST + PST + HST", 10300m, lineGroup.TotalGSTAndPSTAndHST);
				AssertEquals("LINETOTALS.INTEREST", 33m, lineGroup.TotalInterests);
				AssertEquals("LINETOTALS.OTHERS", 34m, lineGroup.TotalOthers);
				AssertEquals("LINETOTALS.TOTALS", 211300m, lineGroup.TotalTotals);
				AssertEquals("Should be 2", 2, lineGroup.LineIteams.Count());

				var lineGroup1 = wrapper.LineGroups.ToList()[1];
				AssertEquals("ZCARMDNOTICECBDETAILSIMPORTER.PARTY.NAME_ORG1 + NAME_ORG2 + NAME_ORG3 + NAME_ORG4", "Importer 17823473202", lineGroup1.LegalName);
				AssertEquals("ZCARMDNOTICECBDETAILSIMPORTER.PARTY.ACCOUNT", "178234732RM0002", lineGroup1.ImporterBusinessNumber);
				AssertEquals("Importer2", importer2, lineGroup1.Importer);
				AssertEquals("LINETOTALS.PAYMENTS", 88m, lineGroup1.PaidAmount);
				AssertEquals("Should be zero for now", ZDecimal.Zero, lineGroup1.RefundAmount);
				AssertEquals("LINETOTALS.DUTIES", 100000m, lineGroup1.TotalDuties);
				AssertEquals("LINETOTALS.SIMA", 0m, lineGroup1.TotalSIMA);
				AssertEquals("LINETOTALS.EXCISE", 751.13m, lineGroup1.TotalExciseTax);
				AssertEquals("LINETOTALS.EXCISEDUTIES", 41.39m, lineGroup1.TotalExciseDuties);
				AssertEquals("LINETOTALS.GST + PST + HST", 10000m, lineGroup1.TotalGSTAndPSTAndHST);
				AssertEquals("LINETOTALS.INTEREST", 0m, lineGroup1.TotalInterests);
				AssertEquals("LINETOTALS.OTHERS", 0m, lineGroup1.TotalOthers);
				AssertEquals("LINETOTALS.TOTALS", 110000m, lineGroup1.TotalTotals);
				AssertEquals("Should be 1", 1, lineGroup1.LineIteams.Count());
			});
		}

		public void TestITableValues()
		{
			var wrapper = CARMDailyNoticeMessageWrapperTest.GetCARMDailyNoticeMessageWrapper(Factory, CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter, CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeImporterMessageText());
			var lineGroup = wrapper.LineGroups.First();
			var tableValues = (ITableValues)lineGroup;
			var values = tableValues.Values.ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Importer Total", ((CellWithFormatting)values[0]).CellValue);
				AssertEquals("TotalDuties", "252,408.00", values[1]);
				AssertEquals("TotalSIMA", "98.00", values[2]);
				AssertEquals("TotalExciseTax", "413.08", values[3]);
				AssertEquals("TotalExciseDuties", "1,242.03", values[4]);
				AssertEquals("TotalGSTAndPSTAndHST", "84,380.50", values[5]);
				AssertEquals("TotalInterests", "95.00", values[6]);
				AssertEquals("TotalOthers", "94.00", values[7]);
				AssertEquals("TotalTotals", "337,837.53", values[8]);
			});
		}
	}
}
