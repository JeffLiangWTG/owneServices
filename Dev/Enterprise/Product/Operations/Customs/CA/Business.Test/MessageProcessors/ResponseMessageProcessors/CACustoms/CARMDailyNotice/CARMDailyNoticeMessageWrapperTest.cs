using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	public class CARMDailyNoticeMessageWrapperTest : TestCaseWithFactory
	{
		public void TestProperties_AH()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "100023258RM0001", Core.Constants.CountryCodes.Canada);
			Factory.Save();

			var wrapper = GetCARMDailyNoticeMessageWrapper(Factory, CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter, CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeImporterMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("File_Name", "DN-100023258RM0001-202208251", wrapper.FileName);
				AssertEquals("File_Split", "E01", wrapper.FileSeq);
				AssertEquals("Should be I", CusStatementHeaderTypes.Codes.Importer, wrapper.StatementType);
				AssertEquals("Should be CARM Daily Notice Importer", CARMDailyNoticeMessageSubTypeList.Descriptions.CARMDNoticeImporter, wrapper.MessageSubTypeDescription);
				AssertEquals("HEADER.PARTY.NAME_ORG1 + NAME_ORG2 + NAME_ORG3 + NAME_ORG4", "Importer 100023258 Org name 2", wrapper.LegalName);
				AssertEquals("HEADER.PARTY.ACCOUNT", "100023258RM0001", wrapper.ImporterBusinessNumber);
				AssertEquals("The last four digits of HEADER.PARTY.ACCOUNT", "0001", wrapper.RMNumber);
				AssertEquals("HEADER.PARTY.BN9", "100023258", wrapper.BN9);
				AssertEquals("HEADER.DN_DATE", new ZDateTime(2022, 02, 02), wrapper.StatementDate);
				AssertEquals("SUMMARY.IMP_DEC", 337837.53m, wrapper.StatementAmount);
				AssertEquals("SUMMARY.PAYMENTS", 11m, wrapper.PaidAmount);
				AssertEquals("SUMMARY.DISB", 22m, wrapper.RefundAmount);
				AssertEquals("SUMMARY.REV_DIST.PAYMENT_DUE_DATE", new ZDateTime(2021, 12, 31), wrapper.DueDate.Value);
				AssertEquals("Note that language is EN", "DNAH - The CARM Client Portal is now live. Check the CBSA Website for more information.", wrapper.MessageEN);
				AssertEquals("Note that language is FR", "DNAH - Le Portail client de la GCRA est maintenant disponible. Verifier le site Web de lASFC pour plus dinformation.", wrapper.MessageFR);
				AssertEquals("The count of line group should be 1", 1, wrapper.LineGroups.Count());
				AssertEquals("SUMMARY.REV_DIST.DUTIES", 252408m, wrapper.TotalDuties);
				AssertEquals("SUMMARY.REV_DIST.SIMA", 0m, wrapper.TotalSIMA);
				AssertEquals("SUMMARY.REV_DIST.EXCISE", 413.08m, wrapper.TotalExciseTax);
				AssertEquals("SUMMARY.REV_DIST.EXCISEDUTIES", 1242.03m, wrapper.TotalExciseDuties);
				AssertEquals("SUMMARY.REV_DIST.GST + PST + HST", 84187.5m, wrapper.TotalGSTAndPSTAndHST);
				AssertEquals("SUMMARY.REV_DIST.INTEREST", 0m, wrapper.TotalInterests);
				AssertEquals("SUMMARY.REV_DIST.OTHERS", 0m, wrapper.TotalOthers);
				AssertEquals("SUMMARY.REV_DIST.TOTALS", 337837.53m, wrapper.TotalTotals);
				AssertEquals("Importer", importer.PK, wrapper.Importer.PK);
				AssertEquals("Should not be Broker", false, wrapper.IsBroker);
			});
		}

		public void TestProperties_CB()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DCA";
			company.GC_Name = "DCA TEST";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			GlbCompany.CurrentCompany.GC_Code = "DCA";

			var broker = Factory.NewWithValidTestData<OrgHeader>();
			company.GC_OH_OrgProxy = broker.PK;
			broker.OH_IsBroker = true;
			broker.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "178234732", Core.Constants.CountryCodes.Canada);
			Factory.Save();

			var wrapper = GetCARMDailyNoticeMessageWrapper(Factory, CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeBroker, CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeBrokerMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("File_Name", "DN-178234732-20220825173010", wrapper.FileName);
				AssertEquals("File_Split", "E01", wrapper.FileSeq);
				AssertEquals("Should be B", CusStatementHeaderTypes.Codes.Broker, wrapper.StatementType);
				AssertEquals("Should be CARM Daily Notice Broker", CARMDailyNoticeMessageSubTypeList.Descriptions.CARMDNoticeBroker, wrapper.MessageSubTypeDescription);
				AssertEquals("HEADER.PARTY.OP_NAME", "Broker 178234732", wrapper.LegalName);
				AssertEquals("HEADER.PARTY.BN9", "178234732", wrapper.ImporterBusinessNumber);
				AssertEquals("RMNumber Should be empty for now", ZString.Empty, wrapper.RMNumber);
				AssertEquals("HEADER.PARTY.BN9", "178234732", wrapper.BN9);
				AssertEquals("HEADER.DN_DATE", new ZDateTime(2022, 02, 03), wrapper.StatementDate);
				AssertEquals("StatementAmount should be zero for now", ZDecimal.Zero, wrapper.StatementAmount);
				AssertEquals("PaidAmount should be zero for now", ZDecimal.Zero, wrapper.PaidAmount);
				AssertEquals("RefundAmount should be zero for now", ZDecimal.Zero, wrapper.RefundAmount);
				AssertEquals("DueDate should has no value", false, wrapper.DueDate.HasValue);
				AssertEquals("Note that language is EN", "DNCB - The CARM Client Portal is now live. Check the CBSA Website for more information.", wrapper.MessageEN);
				AssertEquals("Note that language is FR", "DNCB - Le Portail client de la GCRA est maintenant disponible. Verifier le site Web de lASFC pour plus dinformation.", wrapper.MessageFR);
				AssertEquals("The count of line group should be 2", 2, wrapper.LineGroups.Count());
				AssertEquals("The Sum of Line Group Duties", 300000m, wrapper.TotalDuties);
				AssertEquals("The Sum of Line Group SIMA", 32m, wrapper.TotalSIMA);
				AssertEquals("The Sum of Line Group EXCISE", 1751.13m, wrapper.TotalExciseTax);
				AssertEquals("The Sum of Line Group EXCISEDUTIES", 84.55m, wrapper.TotalExciseDuties);
				AssertEquals("The Sum of Line Group GST + PST + HST", 20300m, wrapper.TotalGSTAndPSTAndHST);
				AssertEquals("The Sum of Line Group INTEREST", 33m, wrapper.TotalInterests);
				AssertEquals("The Sum of Line Group OTHERS", 34m, wrapper.TotalOthers);
				AssertEquals("The Sum of Line Group TOTALS", 321300m, wrapper.TotalTotals);
				AssertEquals("Broker", broker, wrapper.Importer);
				AssertEquals("Should be Broker", true, wrapper.IsBroker);
			});
		}

		public void TestITableInterpretation()
		{
			var wrapper = GetCARMDailyNoticeMessageWrapper(Factory, CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter, CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeImporterMessageText());
			var tableInterpretation = (ITableInterpretation)wrapper;
			AssertEquals("Report Grand Total", tableInterpretation.Caption);

			var titles = tableInterpretation.Titles.ToList();
			AssertEquals("Duties", titles[0]);
			AssertEquals("SIMA", titles[1]);
			AssertEquals("Excise Tax", titles[2]);
			AssertEquals("Excise Duties", titles[3]);
			AssertEquals("GST/PST/HST", titles[4]);
			AssertEquals("Interests", titles[5]);
			AssertEquals("Others", titles[6]);
			AssertEquals("Totals", titles[7]);

			var values = tableInterpretation.Values.ToList();
			AssertEquals("Duties", "252,408.00", values[0]);
			AssertEquals("SIMA", "0.00", values[1]);
			AssertEquals("Excise Tax", "413.08", values[2]);
			AssertEquals("Excise Duties", "1,242.03", values[3]);
			AssertEquals("GST/PST/HST", "84,187.50", values[4]);
			AssertEquals("Interests", "0.00", values[5]);
			AssertEquals("Others", "0.00", values[6]);
			AssertEquals("Totals", "337,837.53", values[7]);
		}

		public static CARMDailyNoticeMessageWrapper GetCARMDailyNoticeMessageWrapper(BusinessObjectFactory factory, ZString messageSubType, ZString messageText)
		{
			var message = factory.New<CARMDailyNoticeMessage>();
			message.EM_MessageText = messageText;
			message.EM_MessageSubType = messageSubType;

			return new CARMDailyNoticeMessageWrapper(message);
		}
	}
}
